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
                var indexName = string.Concat(prefix, index);
                var docs = _client.Search<Ref>(s => s
                    .Index(indexName)
                    .AllTypes()
                    .Query(q => q.Ids(descriptor => descriptor.Values(id.ToString(CultureInfo.InvariantCulture)))));
                if (docs.Total <= 0) continue;
                var ids = docs.Documents.Select(@ref => @ref.Id).ToList();

                if (ids.Count <= 0) continue;
                if (_bulkDescriptor == null)
                    _bulkDescriptor = new BulkDescriptor();

                _bulkDescriptor.DeleteMany<object>(ids, (descriptor, l) => descriptor.Index(indexName));
            }
        }

        public void Commit()
        {
            if (_bulkDescriptor == null) return;
            _client.Bulk(_bulkDescriptor);
            _bulkDescriptor = null;
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

        private readonly BulkStrategy _transaction;        

        private readonly ConcurrentDictionary<string, List<string>> _indices 
            = new ConcurrentDictionary<string, List<string>>();

        public ElasticSearchService()
        {
            VertexIndicesColumnName = "vertex-";
            EdgeIndicesColumnName = "edge-";
            UserVertexIndicesColumnName = "vertex_";
            UserEdgeIndicesColumnName = "edge_";

            _client = CreateClient();

            _transaction = new BulkStrategy(_client, this);
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
            var indices = client.GetAlias(descriptor => descriptor.AllIndices()).Indices.Keys.ToList();
            //TODO remove var indices = client.GetAliases(a => a.Indices("*")).Indices.Keys.ToList();
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
            return isUserIndex 
                ? string.Concat(GetUserIndexName(indexType), indexName.ToLowerInvariant()) 
                : string.Concat(GetIndexName(indexType), indexName.ToLowerInvariant());
        }

        public override void Initialize(IGraphConfiguration configuration)
        {
            
        }

        public override long Set(Type indexType, 
                                 long id, 
                                 string indexName, 
                                 string propertyName, 
                                 object value, 
                                 bool isUserIndex)
        {
            var index = GetIndexName(indexType, indexName, isUserIndex);
            Dictionary<string, object> values;
            var point = value as GeoPoint;
            if (point != null)
            {
                var geoPoint = point;
                values = new Dictionary<string, object>
                {
                    {
                        propertyName.ToLowerInvariant(),
                        new GeoLocation(geoPoint.Latitude, geoPoint.Longitude)
                    }
                };
                
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

        public override IEnumerable<long> Get(Type indexType, 
                                              string indexName, 
                                              string key, 
                                              object value, 
                                              bool isUserIndex, int hitsLimit = 1000)
        {
            var index = GetIndexName(indexType, indexName, isUserIndex);
            var response = _client.Search<Ref>(s => s
                .Index(index)
                .AllTypes()
                .FielddataFields(descriptor => descriptor.Field(key))
                .Size(hitsLimit)
                .Query(q => q.Wildcard(key, value.ToString().ToLowerInvariant())));
            return response.Documents.Select(r => r.Id).ToArray();
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
            var queryDescriptor = new QueryContainerDescriptor<Ref>();
            
            foreach (var graveQueryElement in graveQueryElements)
            {
                QueryContainer elasticQuery = null;
                if (graveQueryElement is IntervalQueryElement)
                {
                    var interval = graveQueryElement as IntervalQueryElement;
                    elasticQuery = CreateQuery(queryDescriptor, interval.Key, interval.StartValue, 
                                               interval.StartValue, interval.EndValue, true, true);
                }
                else if (graveQueryElement is ComparableQueryElement)
                {
                    var comparable = graveQueryElement as ComparableQueryElement;
                    var shape = comparable.Value as IGeoShape;
                    elasticQuery = shape != null 
                        ? CreateGeoQuery(queryDescriptor, comparable.Key, shape) 
                        : CreateComparableQuery(comparable);
                }

                elasticQueries.Add(elasticQuery);
            }

            var indices = string.Concat(
                indexType == typeof (IVertex) ? VertexIndicesColumnName : EdgeIndicesColumnName, "*");

            var queryToRun = elasticQueries.Count == 1 
                ? elasticQueries[0] 
                : queryDescriptor.Bool(b => b.MustNot(elasticQueries.ToArray()));

            var response = _client.Search<Ref>(s => s.Index(indices).AllTypes().Query(descriptor => queryToRun).Size(hitsLimit));
            return response.Documents.Select(@ref => @ref.Id);
        }

        public override long DeleteIndex(Type indexType, string indexName, bool isUserIndex)
        {
            var index = GetIndexName(indexType, indexName, isUserIndex);
            _client.DeleteIndex(index);

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
            throw new NotSupportedException();
        }

        public override void Rollback()
        {
            _transaction.Rollback();
        }

        private static QueryContainer CreateGeoQuery(QueryContainerDescriptor<Ref> q, string key, IGeoShape geoShape)
        {
            var geoCircle = geoShape as GeoCircle;
            if (geoCircle != null)
            {
                var circle = geoCircle;
                return q.GeoShapeCircle(
                    c => c.Name(key.ToLowerInvariant())
                     .Coordinates(new[] {circle.Center.Latitude, circle.Center.Longitude})
                     .Radius(string.Concat(circle.Radius, "Km")));
            }

            var geoPoint = geoShape as GeoPoint;
            if (geoPoint != null)
            {
                var point = geoPoint;
                return q.GeoShapePoint(p => p.Name(key.ToLowerInvariant()).Coordinates(new[] {point.Latitude, point.Longitude}));
            }

            var rectangle = geoShape as GeoRectangle;
            if (rectangle == null) throw new InvalidOperationException("Unknown GeoShape type");
            var rect = rectangle;
            return q.GeoShapeEnvelope(e => e
                .Name(key.ToLowerInvariant())
                .Coordinates(new []
                {
                    new GeoCoordinate(rect.TopLeft.Latitude, rect.TopLeft.Longitude),
                    new GeoCoordinate(rect.BottomRight.Latitude, rect.BottomRight.Longitude), 
                }));
        }

        private static QueryContainer CreateComparableQuery(ComparableQueryElement comparable)
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
                        throw new IndexOutOfRangeException();
                }
            }

            var queryDescriptor = new QueryContainerDescriptor<Ref>();
            var query = CreateQuery(queryDescriptor, comparable.Key.ToLowerInvariant(), 
                                    comparable.Value, min, max, minInclusive, maxInclusive);

            if (comparable.Comparison != Compare.NotEqual) return query;
            var query1 = query;
            query = queryDescriptor.Bool(b => b.MustNot(query1));

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

        private static QueryContainer CreateQuery(QueryContainerDescriptor<Ref> q, 
                                                  string field, 
                                                  object value, 
                                                  object min, 
                                                  object max, 
                                                  bool minInclusive, 
                                                  bool maxInclusive)
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
                    var iq = r.Field(field);
                    

                    var iq2 = minInclusive
                                  ? iq.GreaterThanOrEquals(minVal)
                                  : iq.GreaterThan(minVal);

                    if (maxInclusive)
                        iq2.LessThanOrEquals(maxVal);
                    else
                        iq2.LessThan(maxVal);

                    return r;
                });
            }
            
            if (isDate)
            {
                var minVal = min == null ? (DateTime?)null : Convert.ToDateTime(min);
                var maxVal = max == null ? (DateTime?)null : Convert.ToDateTime(max);
                
// ReSharper disable ImplicitlyCapturedClosure
                return q.DateRange(r =>
// ReSharper restore ImplicitlyCapturedClosure
                {
                    var iq = r.Field(field);

                    var iq2 = minInclusive
                                  ? iq.GreaterThanOrEquals(minVal)
                                  : iq.GreaterThan(minVal);

                    if(maxInclusive)
                        iq2.LessThanOrEquals(maxVal);
                    else
                        iq2.LessThan(maxVal);

                    return r;
                });
            }

            if (!isStringRange) return q.Term(t => t.Field(field).Value(value));
            {
                var minVal = Convert.ToString(min);
                var maxVal = Convert.ToString(max);

// ReSharper disable ImplicitlyCapturedClosure
                return q.TermRange(r =>
// ReSharper restore ImplicitlyCapturedClosure
                {
                    var iq = r.Field(field);

                    var iq2 = minInclusive
                        ? iq.GreaterThanOrEquals(minVal)
                        : iq.GreaterThan(minVal);

                    if(maxInclusive)
                        iq2.LessThanOrEquals(maxVal);
                    else
                        iq2.LessThan(maxVal);

                    return r;
                });
            }
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
                    _client.CreateIndex(fullName, descriptor =>
                        descriptor.Mappings(md => md.Map(TypeName.From<object>(),
                            mdd => mdd.Properties(p => p
                                .GeoPoint(
                                    mappingDescriptor => mappingDescriptor.Name(indexName.ToLowerInvariant()).LatLon())))));
// ReSharper restore ImplicitlyCapturedClosure
                else
                {
                    var complete = parameters[0] as AutoCompleteParameter;
                    if (complete == null) return;
                    var autoComplete = complete;
                    

                    _client.CreateIndex(fullName, id => id
                        .Settings(settingsDescriptor => settingsDescriptor.Analysis(b =>
                        {
                            b.Analyzers(aa => aa.Custom("default",
                                        cd => cd.Filters("standard", "asciifolding", "lowercase")
                                                .Tokenizer("edgeNGram")));
                            b.Tokenizers(td => td.EdgeNGram("edgeNGram", descriptor =>
                                descriptor.MinGram(autoComplete.NGram.Min)
                                          .MaxGram(autoComplete.NGram.Max)));
                            return b;
                        })));
                }
            }
            else
                _client.CreateIndex(fullName, descriptor => descriptor
                    .Settings(settingsDescriptor => settingsDescriptor.Analysis(b =>
                    {
                        b.Analyzers(aa => aa.Custom("default",
                                    cd => cd.Filters("standard", "asciifolding", "lowercase")
                                            .Tokenizer("keyword")));
                        return b;
                    })));
        }

        public List<string> GetIndices(string indexType)
        {
            List<string> indices;
            if (_indices.TryGetValue(indexType, out indices))
                return indices;
            indices = _client.GetAlias(descriptor => descriptor.Index(string.Concat(indexType, "*"))).Indices
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
                if (!_client.IndexExists(realName).Exists) return;
                _indices.TryRemove(UserEdgeIndicesColumnName, out indices);
                _client.DeleteIndex(realName);
            }
        }
    }

    public class Ref
    {
        public long Id { get; set; }
    }
}
