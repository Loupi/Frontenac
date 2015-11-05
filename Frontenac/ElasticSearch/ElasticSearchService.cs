using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Globalization;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Geo;
using Frontenac.Infrastructure;
using Frontenac.Infrastructure.Indexing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nest;
using IGeoShape = Frontenac.Blueprints.Geo.IGeoShape;

namespace Frontenac.ElasticSearch
{
    public class BulkStrategy
    {
        private readonly ElasticClient _client;
        private readonly ElasticSearchService _searchService;
        private BulkDescriptor _bulkDescriptor;
        
        public BulkStrategy(ElasticClient client, ElasticSearchService searchService)
        {
            _client = client;
            _searchService = searchService;
        }

        public void Update(string index, long id, Dictionary<string, object> values, Dictionary<string, object> upsert)
        {
            if (_bulkDescriptor == null)
                _bulkDescriptor = new BulkDescriptor();

            _bulkDescriptor.Update<object, object>(u => u.Index(index).Id(id).Doc(values).Upsert(upsert));
        }

        public void DeleteDocuments(Type indexType, long id)
        {
            if (indexType == typeof (IVertex))
            {
                var indices = _searchService.VertexIndices.GetIndices().ToArray();
                Delete(indices, id, "vertex-");
            }
            else
            {
                var indices = _searchService.EdgeIndices.GetIndices().ToArray();
                Delete(indices, id, "edge-");
            }

            DeleteUserDocuments(indexType, id);
        }

        public void DeleteUserDocuments(Type indexType, long id)
        {
            if (indexType == typeof(IVertex))
            {
                var indices = _searchService.UserVertexIndices.GetIndices().ToArray();
                Delete(indices, id, "vertex_");
            }
            else
            {
                var indices = _searchService.UserEdgeIndices.GetIndices().ToArray();
                Delete(indices, id, "edge_");
            }
        }

        private void Delete(IEnumerable<string> indices, long id, string prefix)
        {
            foreach (var index in indices)
            {
                string indexName = string.Concat(prefix, index);
                var docs = _client.Search<Ref>(s => s.Index(indexName).AllTypes().Query(q => q.Ids(new[] { id.ToString(CultureInfo.InvariantCulture) })));
                if (docs.Total > 0)
                {
                    var ids = docs.Documents.Select(@ref => @ref.Id).ToList();

                    if (ids.Count > 0)
                    {
                        if (_bulkDescriptor == null)
                            _bulkDescriptor = new BulkDescriptor();

                        _bulkDescriptor.DeleteMany<object>(ids, (descriptor, l) => descriptor.Index(indexName));
                    }
                }
            }
        }

        public void Commit()
        {
            if (_bulkDescriptor != null)
            {
                _client.Bulk(_bulkDescriptor);
                _bulkDescriptor = null;
            }
        }

        public Task<IBulkResponse> CommitAsync()
        {
            if (_bulkDescriptor == null) return null;
            var task = _client.BulkAsync(_bulkDescriptor);
            _bulkDescriptor = null;
            return task;
        }

        public void Rollback()
        {
            _bulkDescriptor = null;
        }
    }

    public class ElasticSearchService : IndexingService, IIndexStore
    {
        private readonly ElasticClient _client;
        private readonly FluentDictionary<string, AnalyzerBase> _analyzers;

        private readonly BulkStrategy _transaction;        

        private readonly ConcurrentDictionary<string, List<string>> _indices = new ConcurrentDictionary<string, List<string>>();

        public ElasticSearchService()
        {
            VertexIndicesColumnName = "vertex-";
            EdgeIndicesColumnName = "edge-";
            UserVertexIndicesColumnName = "vertex_";
            UserEdgeIndicesColumnName = "edge_";

            _client = CreateClient();

            _transaction = new BulkStrategy(_client, this);

            var analyzer = new CustomAnalyzer
            {
                Filter = new List<string> { "standard", "asciifolding", "lowercase" },
                Tokenizer = "keyword"
            };

            var autocomplete = new CustomAnalyzer
            {
                Filter = new List<string> { "standard", "asciifolding", "lowercase" },
                Tokenizer = "edgeNGram"
            };

            _analyzers = new FluentDictionary<string, AnalyzerBase> { { "exact", analyzer }, { "autocomplete", autocomplete } };
        }

        private static ElasticClient CreateClient()
        {
            string connectionString = null;
            try
            {
                connectionString = ConfigurationManager.ConnectionStrings["ElasticSearch"].ConnectionString;
            }
            catch
            {
                //ignored
            }

            var uri = string.IsNullOrWhiteSpace(connectionString) ? "http://localhost:9200" : connectionString;
            var node = new Uri(uri);
            var settings = new ConnectionSettings(node);
            return new ElasticClient(settings);
        }

        public static void DropAll()
        {
            var client = CreateClient();
            var indices = client.GetAliases(a => a.Indices("*")).Indices.Keys.ToList();
            foreach (var index in indices)
            {
                client.DeleteIndex(index);
            }
        }

        private string GetIndexName(Type indexType)
        {
            Contract.Requires(indexType != null);
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

            return indexType == typeof(IVertex) ? VertexIndicesColumnName : EdgeIndicesColumnName;
        }

        private string GetUserIndexName(Type indexType)
        {
            Contract.Requires(indexType != null);
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

            return indexType == typeof (IVertex) ? UserVertexIndicesColumnName : UserEdgeIndicesColumnName;
        }

        private string GetIndexName(Type indexType, string indexName, bool isUserIndex)
        {
            return isUserIndex ? string.Concat(GetUserIndexName(indexType), indexName.ToLowerInvariant()) : string.Concat(GetIndexName(indexType), indexName.ToLowerInvariant());
        }

        public override void Initialize(IGraphConfiguration configuration)
        {
            
        }

        public override long Set(Type indexType, long id, string indexName, string propertyName, object value, bool isUserIndex)
        {
            var index = GetIndexName(indexType, indexName, isUserIndex);
            Dictionary<string, object> values;
            if (value is GeoPoint)
            {
                var geoPoint = value as GeoPoint;
                values = new Dictionary<string, object> { { propertyName.ToLowerInvariant(), new GeoLocation(geoPoint.Latitude, geoPoint.Longitude) } };
                
            }
            else
            {
                values = new Dictionary<string, object> {{propertyName.ToLowerInvariant(), value}};
            }
             
            var upsert = new Dictionary<string, object> {{"id", id}, {propertyName, value}};

            _transaction.Update(index, id, values, upsert);

            return 0;
        }

        public override void WaitForGeneration(long generation)
        {
            
        }

        public override IEnumerable<long> Get(Type indexType, string indexName, string key, object value, bool isUserIndex, int hitsLimit = 1000)
        {
            var index = GetIndexName(indexType, indexName, isUserIndex);

            var response = _client.Search<Ref>(s => s.Index(index).AllTypes().FielddataFields(key).Size(hitsLimit).Query(q => q.Wildcard(key, value.ToString().ToLowerInvariant())));
            return response.Documents.Select(@ref => @ref.Id).ToArray();
        }

        public override long DeleteDocuments(Type indexType, long id)
        {
            _transaction.DeleteDocuments(indexType, id);

            return 0;
        }

        public override long DeleteUserDocuments(Type indexType, long id, string key, object value)
        {
            _transaction.DeleteUserDocuments(indexType, id);

            return 0;
        }

        public override IEnumerable<long> Query(Type indexType, IEnumerable<QueryElement> query, int hitsLimit = 1000)
        {
            var graveQueryElements = query as QueryElement[] ?? query.ToArray();
            if (!graveQueryElements.Any())
                return Enumerable.Empty<long>();

            var elasticQueries = new List<QueryContainer>();
            var queryDescriptor = new QueryDescriptor<Ref>();
            
            foreach (var graveQueryElement in graveQueryElements)
            {
                QueryContainer elasticQuery = null;
                if (graveQueryElement is IntervalQueryElement)
                {
                    var interval = graveQueryElement as IntervalQueryElement;
                    elasticQuery = CreateQuery(queryDescriptor, interval.Key, interval.StartValue, interval.StartValue, interval.EndValue, true, true);
                }
                else if (graveQueryElement is ComparableQueryElement)
                {
                    var comparable = graveQueryElement as ComparableQueryElement;
                    if (comparable.Value is IGeoShape)
                        elasticQuery = CreateGeoQuery(queryDescriptor, comparable.Key, comparable.Value as IGeoShape);
                    else
                        elasticQuery = CreateComparableQuery(comparable);
                }

                elasticQueries.Add(elasticQuery);
            }

            var indices = string.Concat(
                indexType == typeof (IVertex) ? VertexIndicesColumnName : EdgeIndicesColumnName, "*");

            var queryToRun = elasticQueries.Count == 1 ? elasticQueries[0] : queryDescriptor.Bool(b => b.MustNot(elasticQueries.ToArray()));

            var response = _client.Search<Ref>(s => s.Indices(indices).AllTypes().Query(queryToRun).Size(hitsLimit));
            return response.Documents.Select(@ref => @ref.Id);
        }

        public override long DeleteIndex(Type indexType, string indexName, bool isUserIndex)
        {
            var index = GetIndexName(indexType, indexName, isUserIndex);
            _client.DeleteIndex(d => d.Index(index));

            string indexColumn;
            if (isUserIndex)
            {
                indexColumn = indexType == typeof (IVertex) ? UserVertexIndicesColumnName : UserEdgeIndicesColumnName;
            }
            else
            {
                indexColumn = indexType == typeof(IVertex) ? VertexIndicesColumnName : EdgeIndicesColumnName;
            }

            List<string> indices;
            _indices.TryRemove(indexColumn, out indices);

            return 0;
        }

        public override void Commit()
        {
            _transaction.Commit();
        }

        public override Task CommitAsync()
        {
            return _transaction.CommitAsync();
        }

        public override void Prepare()
        {
            throw new NotImplementedException();
        }

        public override void Rollback()
        {
            _transaction.Rollback();
        }

        static QueryContainer CreateGeoQuery(QueryDescriptor<Ref> q, string key, IGeoShape geoShape)
        {
            if (geoShape is GeoCircle)
            {
                var circle = geoShape as GeoCircle;
                return q.GeoShapeCircle(
                    c => c.Name(key.ToLowerInvariant())
                     .Coordinates(new[] {circle.Center.Latitude, circle.Center.Longitude})
                     .Radius(string.Concat(circle.Radius, "Km")));
            }

            if (geoShape is GeoPoint)
            {
                var point = geoShape as GeoPoint;
                return q.GeoShapePoint(p => p.Name(key.ToLowerInvariant()).Coordinates(new[] {point.Latitude, point.Longitude}));
            }

            if (geoShape is GeoRectangle)
            {
                var rect = geoShape as GeoRectangle;
                return q.GeoShapeEnvelope(e => e.Name(key.ToLowerInvariant()).Coordinates(new[]{new []{rect.TopLeft.Latitude, rect.TopLeft.Longitude},
                    new []{rect.BottomRight.Latitude,rect.BottomRight.Longitude}}));
            }

            throw new InvalidOperationException("Unknown GeoShape type");
        }

        static QueryContainer CreateComparableQuery(ComparableQueryElement comparable)
        {
            Contract.Requires(comparable != null);

            object min;
            object max;
            bool minInclusive;
            bool maxInclusive;

            if (comparable.Value == null || !GraphHelpers.IsNumber(comparable.Value))
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

            var queryDescriptor = new QueryDescriptor<Ref>();
            var query = CreateQuery(queryDescriptor, comparable.Key.ToLowerInvariant(), comparable.Value, min, max, minInclusive, maxInclusive);

            if (comparable.Comparison == Compare.NotEqual)
            {
                var query1 = query;
                query = queryDescriptor.Bool(b => b.MustNot(query1));
            }

            return query;
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

        static QueryContainer CreateQuery(QueryDescriptor<Ref> q, string field, object value, object min, object max, bool minInclusive, bool maxInclusive)
        {
            var isNumeric = GraphHelpers.IsNumber(value);
            var isDate = value is DateTime;
            var isStringRange = value is string && min is string && max is string;

            if (isNumeric)
            {
                var minVal = min == null ? (double?)null : Convert.ToDouble(min);
                var maxVal = max == null ? (double?)null : Convert.ToDouble(max);

// ReSharper disable ImplicitlyCapturedClosure
                return q.Range(r =>
// ReSharper restore ImplicitlyCapturedClosure
                {
                    var iq = r.OnField(field);
                    

                    var iq2 = minInclusive
                                  ? iq.GreaterOrEquals(minVal)
                                  : iq.Greater(minVal);

                    if (maxInclusive)
                        iq2.LowerOrEquals(maxVal);
                    else
                        iq2.Lower(maxVal);
                });
            }
            
            if (isDate)
            {
                var minVal = min == null ? (DateTime?)null : Convert.ToDateTime(min);
                var maxVal = max == null ? (DateTime?)null : Convert.ToDateTime(max);

// ReSharper disable ImplicitlyCapturedClosure
                return q.Range(r =>
// ReSharper restore ImplicitlyCapturedClosure
                {
                    var iq = r.OnField(field);

                    var iq2 = minInclusive
                                  ? iq.GreaterOrEquals(minVal)
                                  : iq.Greater(minVal);

                    if(maxInclusive)
                        iq2.LowerOrEquals(maxVal);
                    else
                        iq2.Lower(maxVal);
                });
            }
            
            if (isStringRange)
            {
                var minVal = Convert.ToString(min);
                var maxVal = Convert.ToString(max);

// ReSharper disable ImplicitlyCapturedClosure
                return q.Range(r =>
// ReSharper restore ImplicitlyCapturedClosure
                {
                    var iq = r.OnField(field);

                    var iq2 = minInclusive
                                  ? iq.GreaterOrEquals(minVal)
                                  : iq.Greater(minVal);

                    if(maxInclusive)
                        iq2.LowerOrEquals(maxVal);
                    else
                        iq2.Lower(maxVal);
                });
            }

            return q.Term(t => t.OnField(field).Value(value));
        }

        public void LoadIndices()
        {

        }

        public void CreateIndex(string indexName, string indexColumn, Parameter[] parameters)
        {
            var fullName = string.Concat(indexColumn, indexName.ToLowerInvariant());

            if (_client.IndexExists(fullName).Exists)
                return;

            List<string> indices;
            _indices.TryRemove(indexColumn, out indices);

            if (parameters != null && parameters.Length > 0)
            {
                if (parameters[0] is Parameter<string, GeoPoint>)
// ReSharper disable ImplicitlyCapturedClosure
                    _client.CreateIndex(
                        descriptor => descriptor.Index(fullName).AddMapping<object>(m => m.Properties(p => p
// ReSharper restore ImplicitlyCapturedClosure
                            .GeoPoint(
                                mappingDescriptor => mappingDescriptor.Name(indexName.ToLowerInvariant()).IndexLatLon()))));
                else if (parameters[0] is AutoCompleteParameter)
                {
                    var autoComplete = parameters[0] as AutoCompleteParameter;

                    _client.CreateIndex(d => d.Index(fullName).Analysis(a => a.Analyzers(b =>
                    {
                        b.Clear();
                        b.Add("default", _analyzers["autocomplete"]);
                        return b;
                    }).Tokenizers(tokenizers => tokenizers.Add("edgeNGram", new EdgeNGramTokenizer()
                    {
                        MinGram = autoComplete.NGram.Min,
                        MaxGram = autoComplete.NGram.Max
                    }))));
                }
            }
            else
// ReSharper disable ImplicitlyCapturedClosure
                _client.CreateIndex(d => d.Index(fullName).Analysis(a => a.Analyzers(b =>
                {
                    b.Clear();
                    b.Add("default", _analyzers["exact"]);
                    return b;
                })));
// ReSharper restore ImplicitlyCapturedClosure
        }

        public List<string> GetIndices(string indexType)
        {
            List<string> indices;
            if (_indices.TryGetValue(indexType, out indices))
                return indices;
            indices = _client.GetAliases(s => s.Indices(string.Concat(indexType, "*"))).Indices
                .Where(pair => pair.Key.StartsWith(indexType))
                .Select(pair => pair.Key.Substring(indexType.Length)).ToList();
            _indices.TryAdd(indexType, indices);
            return indices;
        }

        public long DeleteIndex(IndexingService indexingService, string indexName, string indexColumn, Type indexType, bool isUserIndex)
        {
            List<string> indices;
            _indices.TryRemove(indexColumn, out indices);

            var fullName = string.Concat(indexColumn, indexName.ToLowerInvariant());
            _client.DeleteIndex(fullName);
            return 0;
        }

        public void DropIndex(string indexName)
        {
            List<string> indices;
            
            var realName = string.Concat(UserVertexIndicesColumnName, indexName.ToLowerInvariant());
            if (_client.IndexExists(realName).Exists)
            {
                _client.DeleteIndex(realName);
                _indices.TryRemove(UserVertexIndicesColumnName, out indices);
            }
            else
            {
                realName = string.Concat(UserEdgeIndicesColumnName, indexName.ToLowerInvariant());
                if (_client.IndexExists(realName).Exists)
                {
                    _indices.TryRemove(UserEdgeIndicesColumnName, out indices);
                    _client.DeleteIndex(realName);
                }
            }
        }
    }

    public class Ref
    {
        public long Id { get; set; }
    }
}
