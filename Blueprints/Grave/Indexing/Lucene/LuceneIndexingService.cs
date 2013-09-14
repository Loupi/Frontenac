using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using Frontenac.Blueprints;
using Grave.Esent;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Contrib.Management;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Directory = System.IO.Directory;
using Version = Lucene.Net.Util.Version;

namespace Grave.Indexing.Lucene
{
    public class LuceneIndexingService : IndexingService
    {
        private readonly StandardAnalyzer _analyzer;
        private readonly FSDirectory _directory;
        private readonly IIndexerFactory _indexerFactory;
        private readonly NrtManager _nrtManager;
        private readonly LuceneIndexingServiceParameters _parameters;
        private readonly NrtManagerReopener _reopener;
        private readonly SearcherManager _searcherManager;
        private readonly IndexWriter _writer;

        public Dictionary<Type, object> ClassMaps = new Dictionary<Type, object>();

        public LuceneIndexingService(EsentConfigContext configContext, LuceneIndexingServiceParameters parameters,
                                     FSDirectory directory, IIndexerFactory indexerFactory)
            : base(configContext)
        {
            Contract.Requires(configContext != null);
            Contract.Requires(parameters != null);
            Contract.Requires(directory != null);
            Contract.Requires(indexerFactory != null);

            _parameters = parameters;
            _directory = directory;
            _indexerFactory = indexerFactory;

            _analyzer = new StandardAnalyzer(Version.LUCENE_30);
            _writer = new IndexWriter(_directory, _analyzer, IndexWriter.MaxFieldLength.UNLIMITED);
            _nrtManager = new NrtManager(_writer);
            _searcherManager = _nrtManager.GetSearcherManager();
            _reopener = new NrtManagerReopener(_nrtManager, TimeSpan.FromSeconds(_parameters.MaxStaleSeconds),
                                               TimeSpan.FromMilliseconds(_parameters.MinStaleMilliseconds),
                                               _parameters.CloseTimeoutSeconds);
        }

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing) return;
            _reopener.Dispose();

            _searcherManager.Dispose();
            _nrtManager.Dispose();
            _writer.Dispose();
            _analyzer.Dispose();
        }

        #endregion

        public static FSDirectory CreateMMapDirectory(string path)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(path));
            Contract.Ensures(Contract.Result<FSDirectory>() != null);

            Directory.CreateDirectory(path);
            var directory = new MMapDirectory(new DirectoryInfo(path), new SingleInstanceLockFactory());
            if (IndexWriter.IsLocked(directory)) IndexWriter.Unlock(directory);
            var lockFilePath = Path.Combine(path, "write.lock");
            if (File.Exists(lockFilePath)) File.Delete(lockFilePath);
            return directory;
        }

        private static Query ParseQuery(string searchQuery, QueryParser parser)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(searchQuery));
            Contract.Requires(parser != null);
            Contract.Ensures(Contract.Result<Query>() != null);

            Query query;
            try
            {
                query = parser.Parse(searchQuery.Trim());
            }
            catch (ParseException)
            {
                query = parser.Parse(QueryParser.Escape(searchQuery.Trim()));
            }
            return query;
        }

        private static Document CreateDocument(string idColumnName, string keyColumnName, int id, string propertyName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(idColumnName));
            Contract.Requires(!string.IsNullOrWhiteSpace(keyColumnName));
            Contract.Requires(!string.IsNullOrWhiteSpace(propertyName));
            Contract.Ensures(Contract.Result<Document>() != null);

            var document = new Document();
            document.Add(new NumericField(idColumnName, Field.Store.YES, true).SetIntValue(id));
            document.Add(new Field(keyColumnName, propertyName, Field.Store.YES, Field.Index.NOT_ANALYZED));
            return document;
        }

        private static Query CreateKeyQuery(string idColumnName, string keyColumnName, int id, string propertyName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(idColumnName));
            Contract.Requires(!string.IsNullOrWhiteSpace(keyColumnName));
            Contract.Requires(!string.IsNullOrWhiteSpace(propertyName));
            Contract.Ensures(Contract.Result<Query>() != null);

            return new BooleanQuery
                {
                    new BooleanClause(NumericRangeQuery.NewIntRange(idColumnName, id, id, true, true), Occur.MUST),
                    new BooleanClause(new TermQuery(new Term(keyColumnName, propertyName)), Occur.MUST)
                };
        }

        private string GetIdColumn(Type indexType)
        {
            Contract.Requires(indexType != null);
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

            return indexType == typeof (IVertex) ? _parameters.VertexIdColumnName : _parameters.EdgeIdColumnName;
        }

        private string GetKeyColumn(Type indexType)
        {
            Contract.Requires(indexType != null);
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

            return indexType == typeof (IVertex) ? _parameters.VertexKeyColumnName : _parameters.EdgeKeyColumnName;
        }

        private string GetIndexColumn(Type indexType)
        {
            Contract.Requires(indexType != null);
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

            return indexType == typeof (IVertex) ? _parameters.VertexIndexColumnName : _parameters.EdgeIndexColumnName;
        }

        public override long DeleteIndex(Type indexType, string indexName, bool isUserIndex)
        {
            return _nrtManager.DeleteDocuments(new TermQuery(new Term(isUserIndex
                                                                          ? GetIndexColumn(indexType)
                                                                          : GetKeyColumn(indexType), indexName)));
        }

        private static object GetMinValue(object value)
        {
            Contract.Requires(value != null);
            Contract.Ensures(Contract.Result<object>() != null);

            if (value is double || value is float)
                return double.MinValue;

            return long.MinValue;
        }

        private static object GetMaxValue(object value)
        {
            Contract.Requires(value != null);
            Contract.Ensures(Contract.Result<object>() != null);

            if (value is double || value is float)
                return double.MaxValue;

            return long.MaxValue;
        }

        public override IEnumerable<int> Query(Type indexType, IEnumerable<GraveQueryElement> query,
                                               int hitsLimit = 1000)
        {
            var graveQueryElements = query as GraveQueryElement[] ?? query.ToArray();
            if (!graveQueryElements.Any())
                return Enumerable.Empty<int>();

            var luceneQueries = new List<Query>();
            foreach (var graveQueryElement in graveQueryElements)
            {
                if (graveQueryElement is GraveIntervalQueryElement)
                {
                    var interval = graveQueryElement as GraveIntervalQueryElement;
                    luceneQueries.Add(CreateQuery(indexType, interval.Key, interval.StartValue, interval.StartValue,
                                                  interval.EndValue, true, true, interval.Key, false));
                }
                else if (graveQueryElement is GraveComparableQueryElement)
                {
                    var comparable = graveQueryElement as GraveComparableQueryElement;
                    object min;
                    object max;
                    bool minInclusive;
                    bool maxInclusive;

                    if (comparable.Value == null || !Portability.IsNumber(comparable.Value))
                    {
                        min = comparable.Value;
                        max = comparable.Value;
                        minInclusive = true;
                        maxInclusive = true;
                    }
                    else
                    {
                        switch (comparable.Comparison)
                        {
                            case Compare.Equal:
                            case Compare.NotEqual:
                                min = comparable.Value;
                                max = comparable.Value;
                                minInclusive = true;
                                maxInclusive = true;
                                break;

                            case Compare.GreaterThan:
                                min = comparable.Value;
                                max = GetMaxValue(comparable.Value);
                                minInclusive = false;
                                maxInclusive = true;
                                break;

                            case Compare.GreaterThanEqual:
                                min = comparable.Value;
                                max = GetMaxValue(comparable.Value);
                                minInclusive = true;
                                maxInclusive = true;
                                break;

                            case Compare.LessThan:
                                min = GetMinValue(comparable.Value);
                                max = comparable.Value;
                                minInclusive = true;
                                maxInclusive = false;
                                break;

                            case Compare.LessThanEqual:
                                min = GetMinValue(comparable.Value);
                                max = comparable.Value;
                                minInclusive = true;
                                maxInclusive = true;
                                break;

                            default:
                                throw new NotSupportedException();
                        }
                    }

                    var luceneQuery = CreateQuery(indexType, comparable.Key, comparable.Value, min, max,
                                                  minInclusive, maxInclusive, comparable.Key, false);
                    if (comparable.Comparison == Compare.NotEqual)
                    {
                        var booleanQuery = new BooleanQuery
                            {
                                new BooleanClause(luceneQuery, Occur.MUST_NOT)
                            };
                        luceneQuery = booleanQuery;
                    }

                    luceneQueries.Add(luceneQuery);
                }
            }

            Query queryToRun;
            if (luceneQueries.Count == 1)
                queryToRun = luceneQueries[0];
            else
            {
                var booleanQuery = new BooleanQuery();
                foreach (var q in luceneQueries)
                {
                    booleanQuery.Add(q, Occur.MUST);
                }
                queryToRun = booleanQuery;
            }

            return Fetch(indexType, queryToRun, hitsLimit);
        }

        public override long DeleteDocuments(Type indexType, int id)
        {
            return _nrtManager.DeleteDocuments(NumericRangeQuery.NewIntRange(GetIdColumn(indexType), id, id, true, true));
        }

        public override long DeleteUserDocuments(Type indexType, int id, string key, object value)
        {
            var query = new BooleanQuery
                {
                    new BooleanClause(NumericRangeQuery.NewIntRange(GetIdColumn(indexType), id, id, true, true),
                                      Occur.MUST),
                    new BooleanClause(
                        CreateQuery(indexType, key, value, value, value, true, true, GetIndexColumn(indexType), true),
                        Occur.MUST)
                };
            return _nrtManager.DeleteDocuments(query);
        }

        public override long Set(Type indexType, int id, string indexName, string propertyName, object value,
                                 bool isUserIndex)
        {
            var idColumn = GetIdColumn(indexType);
            var keyColumn = isUserIndex ? GetIndexColumn(indexType) : GetKeyColumn(indexType);
            var generation = _nrtManager.DeleteDocuments(CreateKeyQuery(idColumn, keyColumn, id, indexName));
            if (value == null) return generation;

            var rawDocument = CreateDocument(idColumn, keyColumn, id, indexName);
            var document = new LuceneDocument(rawDocument);
            var indexer = _indexerFactory.Create(value, document);
            indexer.Index(propertyName);
            generation = _nrtManager.AddDocument(rawDocument);

            return generation;
        }

        public override void WaitForGeneration(long generation)
        {
            _nrtManager.WaitForGeneration(generation);
        }

        private Query CreateQuery(Type indexType, string key, object value, object minValue, object maxValue,
                                  bool minInclusive, bool maxInclusive, string indexName, bool isUserIndex)
        {
            Contract.Requires(IsValidIndexType(indexType));
            Contract.Ensures(Contract.Result<Query>() != null);

            var query = Portability.IsNumber(value)
                            ? CreateRangeQuery(key, value, minValue, maxValue, minInclusive, maxInclusive)
                            : new TermQuery(new Term(key, value as string));

            if (!string.IsNullOrWhiteSpace(indexName))
            {
                query = new BooleanQuery
                    {
                        new BooleanClause(new TermQuery(new Term(isUserIndex
                                                                     ? GetIndexColumn(indexType)
                                                                     : GetKeyColumn(indexType), indexName)), Occur.MUST),
                        new BooleanClause(query, Occur.MUST)
                    };
            }

            return query;
        }

        public override IEnumerable<int> Get(Type indexType, string indexName, string key, object value,
                                             bool isUserIndex, int hitsLimit = 1000)
        {
            var query = CreateQuery(indexType, key, value, value, value, true, true, indexName, isUserIndex);
            return Fetch(indexType, query, hitsLimit);
        }

        private IEnumerable<int> Fetch(Type indexType, Query query, int hitsLimit)
        {
            Contract.Requires(IsValidIndexType(indexType));
            Contract.Ensures(Contract.Result<IEnumerable<int>>() != null);

            using (var searcherToken = _searcherManager.Acquire())
            {
                var hits = searcherToken.Searcher.Search(query, hitsLimit).ScoreDocs;
                var idColumn = GetIdColumn(indexType);
                foreach (var hit in hits)
                {
                    yield return int.Parse(searcherToken.Searcher.Doc(hit.Doc).Get(idColumn));
                }
            }
        }

        private static Query CreateRangeQuery(string term, object value, object min, object max, bool minInclusive,
                                              bool maxInclusive)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(term));
            Contract.Requires(value != null);
            Contract.Ensures(Contract.Result<Query>() != null);

            Query query;

            if (value is sbyte || value is byte || value is short || value is ushort ||
                value is int || value is uint || value is long || value is ulong)
            {
                var minVal = min == null ? (long?) null : Convert.ToInt64(min);
                var maxVal = max == null ? (long?) null : Convert.ToInt64(max);
                query = NumericRangeQuery.NewLongRange(term, minVal, maxVal, minInclusive, maxInclusive);
            }
            else
            {
                var minVal = min == null ? (double?) null : Convert.ToDouble(min);
                var maxVal = max == null ? (double?) null : Convert.ToDouble(max);
                query = NumericRangeQuery.NewDoubleRange(term, minVal, maxVal, minInclusive, maxInclusive);
            }

            return query;
        }
    }
}