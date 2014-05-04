using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using Frontenac.Blueprints;
using Frontenac.Grave.Esent;
using Frontenac.Grave.Geo;
using Lucene.Net.Contrib.Management;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Spatial.Prefix;
using Lucene.Net.Spatial.Prefix.Tree;
using Lucene.Net.Spatial.Queries;
using Lucene.Net.Store;
using Spatial4n.Core.Context;
using Spatial4n.Core.Distance;
using Spatial4n.Core.Shapes;
using Directory = System.IO.Directory;

namespace Frontenac.Grave.Indexing.Lucene
{
    public class LuceneIndexingService : IndexingService
    {
        private readonly NrtManager _nrtManager;
        private readonly SearcherManager _searcherManager;
        private readonly IIndexerFactory _indexerFactory;

        public LuceneIndexingService(EsentContext context,
                                     IIndexerFactory indexerFactory,
                                     NrtManager nrtManager,
                                     IIndexCollectionFactory indexCollectionFactory)
            : base(context, indexCollectionFactory)
        {
            Contract.Requires(context != null);
            Contract.Requires(indexerFactory != null);
            Contract.Requires(nrtManager != null);
            Contract.Requires(indexCollectionFactory != null);

            _indexerFactory = indexerFactory;
            _nrtManager = nrtManager;
            _searcherManager = _nrtManager.GetSearcherManager();
        }

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing) return;

            _searcherManager.Dispose();
            _nrtManager.Dispose();
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

        private static string GetIdColumn(Type indexType)
        {
            Contract.Requires(indexType != null);
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

            return indexType == typeof(IVertex) 
                ? LuceneIndexingServiceParameters.Default.VertexIdColumnName 
                : LuceneIndexingServiceParameters.Default.EdgeIdColumnName;
        }

        private static string GetKeyColumn(Type indexType)
        {
            Contract.Requires(indexType != null);
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

            return indexType == typeof(IVertex) 
                ? LuceneIndexingServiceParameters.Default.VertexKeyColumnName 
                : LuceneIndexingServiceParameters.Default.EdgeKeyColumnName;
        }

        private static string GetIndexColumn(Type indexType)
        {
            Contract.Requires(indexType != null);
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

            return indexType == typeof(IVertex) 
                ? LuceneIndexingServiceParameters.Default.VertexIndexColumnName 
                : LuceneIndexingServiceParameters.Default.EdgeIndexColumnName;
        }

        public override long DeleteIndex(Type indexType, string indexName, bool isUserIndex)
        {
            return _nrtManager.DeleteDocuments(new TermQuery(new Term(isUserIndex ? GetIndexColumn(indexType)
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

        public override IEnumerable<int> Query(Type indexType, IEnumerable<GraveQueryElement> query, int hitsLimit = 1000)
        {
            var graveQueryElements = query as GraveQueryElement[] ?? query.ToArray();
            if (!graveQueryElements.Any())
                return Enumerable.Empty<int>();

            var luceneQueries = new List<Query>();
            foreach (var graveQueryElement in graveQueryElements)
            {
                Query luceneQuery = null;
                if (graveQueryElement is GraveIntervalQueryElement)
                {
                    var interval = graveQueryElement as GraveIntervalQueryElement;
                    luceneQuery = WrapQuery(indexType,
                                            CreateQuery(interval.Key, interval.StartValue, interval.StartValue,
                                                        interval.EndValue, true, true), interval.Key, false);
                }
                else if (graveQueryElement is GraveComparableQueryElement)
                {
                    var comparable = graveQueryElement as GraveComparableQueryElement;

                    if (comparable.Value is IGeoShape)
                        luceneQuery = CreateGeoQuery(indexType, comparable.Key, comparable.Value as IGeoShape);
                    else
                        luceneQuery = CreateComparableQuery(indexType, comparable);
                }

                luceneQueries.Add(luceneQuery);
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

        public override void Prepare()
        {
            VertexIndices.Commit();
            EdgeIndices.Commit();
            UserVertexIndices.Commit();
            UserEdgeIndices.Commit();
            _nrtManager.Prepare();
        }

        public override void Commit()
        {
            _nrtManager.Commit();
        }

        public override void Rollback()
        {
            VertexIndices.Rollback();
            EdgeIndices.Rollback();
            UserVertexIndices.Rollback();
            UserEdgeIndices.Rollback();
            _nrtManager.Rollback();
        }

        static Query CreateGeoQuery(Type indexType, string key, IGeoShape geoShape)
        {
            Shape shape;
            if (geoShape is GeoCircle)
            {
                var circle = geoShape as GeoCircle;
                shape = SpatialContext.GEO.MakeCircle(circle.Center.Latitude, circle.Center.Longitude,
                                                      DistanceUtils.Dist2Degrees(circle.Radius, DistanceUtils.EARTH_MEAN_RADIUS_KM));
            }
            else if (geoShape is GeoPoint)
            {
                var point = geoShape as GeoPoint;
                shape = SpatialContext.GEO.MakePoint(point.Latitude, point.Longitude);
            }
            else if (geoShape is GeoRectangle)
            {
                var rect = geoShape as GeoRectangle;
                shape = SpatialContext.GEO.MakeRectangle(rect.TopLeft.Latitude, rect.TopLeft.Longitude,
                                                         rect.BottomRight.Latitude,
                                                         rect.BottomRight.Longitude);
            }
            else
            {
                throw new InvalidOperationException("Unknown GeoShape type");
            }

            var grid = new GeohashPrefixTree(SpatialContext.GEO, 11);
            var strategy = new RecursivePrefixTreeStrategy(grid, key);
            Query query = strategy.MakeQuery(new SpatialArgs(SpatialOperation.Intersects, shape));
            query = WrapQuery(indexType, query, key, false);

            return query;
        }

        static Query CreateComparableQuery(Type indexType, GraveComparableQueryElement comparable)
        {
            Contract.Requires(comparable != null);

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

            var query = CreateQuery(comparable.Key, comparable.Value, min, max, minInclusive, maxInclusive);
            query = WrapQuery(indexType, query, comparable.Key, false);

            if (comparable.Comparison == Compare.NotEqual)
            {
                var booleanQuery = new BooleanQuery { new BooleanClause(query, Occur.MUST_NOT) };
                query = booleanQuery;
            }

            return query;
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
                    new BooleanClause(WrapQuery(indexType, CreateQuery(key, value, value, value, true, true), 
                        GetIndexColumn(indexType), true), Occur.MUST)
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

        private static Query CreateQuery(string key, object value, object minValue, object maxValue, bool minInclusive, bool maxInclusive)
        {
            Contract.Requires(((((!(Portability.IsNumber(value)) || !(string.IsNullOrWhiteSpace(key))) || key == null) || key.Length != 0) || value != null));
            Contract.Ensures(Contract.Result<Query>() != null);

            var query = Portability.IsNumber(value)
                            ? CreateRangeQuery(key, value, minValue, maxValue, minInclusive, maxInclusive)
                            : new TermQuery(new Term(key, value as string));

            return query;
        }

        private static Query WrapQuery(Type indexType, Query query, string indexName, bool isUserIndex)
        {
            Contract.Requires(query != null);
            Contract.Requires(IsValidIndexType(indexType));
            Contract.Ensures(Contract.Result<Query>() != null);

            if (!string.IsNullOrWhiteSpace(indexName))
            {
                query = new BooleanQuery
                    {
                        new BooleanClause(new TermQuery(new Term(isUserIndex ? GetIndexColumn(indexType)
                                                                             : GetKeyColumn(indexType), indexName)), Occur.MUST),
                        new BooleanClause(query, Occur.MUST)
                    };
            }

            return query;
        }

        public override IEnumerable<int> Get(Type indexType, string indexName, string key, object value, bool isUserIndex, int hitsLimit = 1000)
        {
            var query = WrapQuery(indexType, CreateQuery(key, value, value, value, true, true), indexName, isUserIndex);
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

        private static Query CreateRangeQuery(string term, object value, object min, object max, bool minInclusive, bool maxInclusive)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(term));
            Contract.Requires(value != null);
            Contract.Ensures(Contract.Result<Query>() != null);

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
