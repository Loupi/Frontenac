using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Frontenac.Blueprints;
using Grave.Esent;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Contrib.Management;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Version = Lucene.Net.Util.Version;

namespace Grave.Indexing.Lucene
{
    public class LuceneIndexingService : IndexingService
    {
        readonly LuceneIndexingServiceParameters _parameters;
        readonly FSDirectory _directory;
        readonly IIndexerFactory _indexerFactory;
        readonly IndexWriter _writer;
        readonly NrtManager _nrtManager;
        readonly SearcherManager _searcherManager;
        readonly StandardAnalyzer _analyzer;
        readonly NrtManagerReopener _reopener;
        readonly Task _reopenerTask;

        public Dictionary<Type, object> ClassMaps = new Dictionary<Type, object>();

        public LuceneIndexingService(EsentConfigContext configContext, LuceneIndexingServiceParameters parameters, FSDirectory directory, IIndexerFactory indexerFactory)
            : base(configContext)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            if (directory == null)
                throw new ArgumentNullException("directory");

            _parameters = parameters;
            _directory = directory;
            _indexerFactory = indexerFactory;

            _analyzer = new StandardAnalyzer(Version.LUCENE_30);
            _writer = new IndexWriter(_directory, _analyzer, IndexWriter.MaxFieldLength.UNLIMITED);
            _nrtManager = new NrtManager(_writer);
            _searcherManager = _nrtManager.GetSearcherManager();
            _reopener = new NrtManagerReopener(_nrtManager, TimeSpan.FromSeconds(_parameters.MaxStaleSeconds),
                                                            TimeSpan.FromMilliseconds(_parameters.MinStaleMilliseconds));
            _reopenerTask = Task.Factory.StartNew(() => _reopener.Start());
        }

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (!disposing) return;
            _reopener.Dispose();
            _reopenerTask.Wait(TimeSpan.FromSeconds(_parameters.CloseTimeoutSeconds));
            _searcherManager.Dispose();
            _nrtManager.Dispose();
            _writer.Dispose();
            _analyzer.Dispose();
            //_directory.Dispose();
        }

        #endregion

        public static FSDirectory CreateMMapDirectory(string path)
        {
            System.IO.Directory.CreateDirectory(path);
            var directory = new MMapDirectory(new DirectoryInfo(path), new SingleInstanceLockFactory());
            if (IndexWriter.IsLocked(directory)) IndexWriter.Unlock(directory);
            var lockFilePath = Path.Combine(path, "write.lock");
            if (File.Exists(lockFilePath)) File.Delete(lockFilePath);
            return directory;
        }

        static Query ParseQuery(string searchQuery, QueryParser parser)
        {
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

        static Document CreateDocument(string idColumnName, string keyColumnName, int id, string propertyName)
        {
            var document = new Document();
            document.Add(new NumericField(idColumnName, Field.Store.YES, true).SetIntValue(id));
            document.Add(new Field(keyColumnName, propertyName, Field.Store.YES, Field.Index.NOT_ANALYZED));
            return document;
        }

        static Query CreateKeyQuery(string idColumnName, string keyColumnName, int id, string propertyName)
        {
            return new BooleanQuery
                {
                    new BooleanClause(NumericRangeQuery.NewIntRange(idColumnName, id, id, true, true), Occur.MUST),
                    new BooleanClause(new TermQuery(new Term(keyColumnName, propertyName)), Occur.MUST)
                };
        }

        string GetIdColumn(Type indexType)
        {
            return indexType == typeof(IVertex) ? _parameters.VertexIdColumnName : _parameters.EdgeIdColumnName;
        }

        string GetKeyColumn(Type indexType)
        {
            return indexType == typeof(IVertex) ? _parameters.VertexKeyColumnName : _parameters.EdgeKeyColumnName;
        }

        string GetIndexColumn(Type indexType)
        {
            return indexType == typeof(IVertex) ? _parameters.VertexIndexColumnName : _parameters.EdgeIndexColumnName;
        }

        public override long DeleteIndex(Type indexType, string indexName, bool isUserIndex)
        {
            return _nrtManager.DeleteDocuments(new TermQuery(new Term(isUserIndex ? GetIndexColumn(indexType) : GetKeyColumn(indexType), indexName)));
        }

        public override long DeleteDocuments(Type indexType, int id)
        {
            return _nrtManager.DeleteDocuments(NumericRangeQuery.NewIntRange(GetIdColumn(indexType), id, id, true, true));
        }

        public override long DeleteUserDocuments(Type indexType, int id, string key, object value)
        {
            var query = new BooleanQuery
                {
                    new BooleanClause(NumericRangeQuery.NewIntRange(GetIdColumn(indexType), id, id, true, true), Occur.MUST),
                    new BooleanClause(CreateQuery(indexType, key, value, GetIndexColumn(indexType), true), Occur.MUST)
                };
            return _nrtManager.DeleteDocuments(query);
        }

        public override long Set(Type indexType, int id, string indexName, string propertyName, object value, bool isUserIndex)
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

        Query CreateQuery(Type indexType, string key, object value, string indexName, bool isUserIndex)
        {
            Query query;

            if (Portability.IsNumber(value))
                query = CreateRangeQuery(key, value, value, value, true, true);
            else
                query = new TermQuery(new Term(key, value as string));
                //query = ParseQuery(value as string, new QueryParser(Version.LUCENE_30, key, _analyzer));

            if (!string.IsNullOrWhiteSpace(indexName))
            {
                query = new BooleanQuery
                {
                    new BooleanClause(new TermQuery(new Term(isUserIndex ? GetIndexColumn(indexType) : GetKeyColumn(indexType), indexName)), Occur.MUST),
                    new BooleanClause(query, Occur.MUST)
                };
            }

            return query;
        }

        public override IEnumerable<int> Get(Type indexType, string indexName, string key, object value, bool isUserIndex, int hitsLimit = 1000)
        {
            var query = CreateQuery(indexType, key, value, indexName, isUserIndex);

            using (var searcherToken = _searcherManager.Acquire())
            {
                var hits = searcherToken.Searcher.Search(query, hitsLimit).ScoreDocs;
                foreach (var hit in hits)
                {
                    yield return int.Parse(searcherToken.Searcher.Doc(hit.Doc).Get(GetIdColumn(indexType)));
                }
            }
        }

        static Query CreateRangeQuery(string term, object value, object min, object max, bool minInclusive, bool maxInclusive)
        {
            Query query;

            if (value is sbyte || value is byte || value is short || value is ushort || 
                value is int || value is uint || value is long || value is ulong)
            {
                var minVal = min == null ? (long?)null : Convert.ToInt64(min);
                var maxVal = max == null ? (long?)null : Convert.ToInt64(max);
                query = NumericRangeQuery.NewLongRange(term, minVal, maxVal, minInclusive, maxInclusive);
            }
            else
            {
                var minVal = min == null ? (double?)null : Convert.ToDouble(min);
                var maxVal = max == null ? (double?)null : Convert.ToDouble(max);
                query = NumericRangeQuery.NewDoubleRange(term, minVal, maxVal, minInclusive, maxInclusive);
            }

            return query;
        }
    }
}
