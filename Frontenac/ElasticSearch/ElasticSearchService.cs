using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using Frontenac.Blueprints;
using Frontenac.Infrastructure;
using Frontenac.Infrastructure.Geo;
using Frontenac.Infrastructure.Indexing;
using System.Linq;
using Nest;

namespace Frontenac.ElasticSearch
{
    public class ElasticSearchService : IndexingService, IIndexStore
    {
        private readonly ElasticClient _client;
        private readonly FluentDictionary<string, AnalyzerBase> _analyzers;
        private readonly CustomAnalyzer _analyzer;

        public ElasticSearchService(/*ElasticsearchClient client*/)
        {
            VertexIndicesColumnName = "vertex-";
            EdgeIndicesColumnName = "edge-";
            UserVertexIndicesColumnName = "vertex_";
            UserEdgeIndicesColumnName = "edge_";

            _client = new ElasticClient();

            _analyzer = new CustomAnalyzer
                {
                    Filter = new List<string> {"standard", "asciifolding"},
                    Tokenizer = "standard"
                };

            _analyzers = new FluentDictionary<string, AnalyzerBase> {{"exact", _analyzer}};
        }

        public static void DropAll()
        {
            var client = new ElasticClient();
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
            var values = new Dictionary<string, object> {{propertyName, value}};
            var upsert = new Dictionary<string, object> {{"id", id}, {propertyName, value}};


            _client.Update<object, object>(u => u.Index(index).Id(id).Doc(values).Upsert(upsert));
            return 0;
        }

        public override void WaitForGeneration(long generation)
        {
            
        }

        public override IEnumerable<long> Get(Type indexType, string indexName, string key, object value, bool isUserIndex, int hitsLimit = 1000)
        {
            var index = GetIndexName(indexType, indexName, isUserIndex);
            var values = new Dictionary<string, object> {{key, value}};

            var response = _client.Search<Ref>(s => s.Index(index).AllTypes().Size(hitsLimit).Query(q => q.Term(key, value is string ? value.ToString().ToLowerInvariant() : value)));
            return response.Documents.Select(@ref => @ref.Id).ToArray();
        }

        public override long DeleteDocuments(Type indexType, long id)
        {
            var indices =  indexType == typeof (IVertex) ? "vertex*" : "edge*";
            _client.DeleteByQuery<Ref>(d => d.Indices(indices).AllTypes().Query(q => q.Ids(new []{id.ToString(CultureInfo.InvariantCulture)})));
            return 0;
        }

        public override long DeleteUserDocuments(Type indexType, long id, string key, object value)
        {
            var indices = indexType == typeof(IVertex) ? UserVertexIndicesColumnName : UserEdgeIndicesColumnName;
            indices = string.Concat(indices, value);
            _client.DeleteByQuery<Ref>(d => d.Indices(indices).AllTypes().Query(q => q.Ids(new []{id.ToString(CultureInfo.InvariantCulture)})));
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
                    if (comparable.Value is Infrastructure.Geo.IGeoShape)
                        elasticQuery = CreateGeoQuery(queryDescriptor, comparable.Key, comparable.Value as Infrastructure.Geo.IGeoShape);
                    else
                        elasticQuery = CreateComparableQuery(comparable);
                }

                elasticQueries.Add(elasticQuery);
            }

            var indices =  indexType == typeof (IVertex) ? VertexIndicesColumnName : EdgeIndicesColumnName;

            var queryToRun = elasticQueries.Count == 1 ? elasticQueries[0] : queryDescriptor.Bool(b => b.MustNot(elasticQueries.ToArray()));

            var response = _client.Search<Ref>(s => s.Indices(indices).AllTypes().Fields("id").Query(queryToRun).Size(hitsLimit));
            return response.Documents.Select(@ref => @ref.Id);
        }

        public override long DeleteIndex(Type indexType, string indexName, bool isUserIndex)
        {
            var index = GetIndexName(indexType, indexName, isUserIndex);
            _client.DeleteIndex(d => d.Index(index));
            return 0;
        }

        public override void Commit()
        {
            throw new NotImplementedException();
        }

        public override void Prepare()
        {
            throw new NotImplementedException();
        }

        public override void Rollback()
        {
            throw new NotImplementedException();
        }

        static QueryContainer CreateGeoQuery(QueryDescriptor<Ref> q, string key, Infrastructure.Geo.IGeoShape geoShape)
        {
            if (geoShape is GeoCircle)
            {
                var circle = geoShape as GeoCircle;
                return q.GeoShapeCircle(
                    c => c.Name(key)
                     .Coordinates(new[] {circle.Center.Latitude, circle.Center.Longitude})
                     .Radius(string.Concat(circle.Radius, "Km")));
            }

            if (geoShape is GeoPoint)
            {
                var point = geoShape as GeoPoint;
                return q.GeoShapePoint(p => p.Name(key).Coordinates(new[] {point.Latitude, point.Longitude}));
            }

            if (geoShape is GeoRectangle)
            {
                var rect = geoShape as GeoRectangle;
                return q.GeoShapeEnvelope(e => e.Name(key).Coordinates(new[]{new []{rect.TopLeft.Latitude, rect.TopLeft.Longitude},
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
            var query = CreateQuery(queryDescriptor, comparable.Key, comparable.Value, min, max, minInclusive, maxInclusive);

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

                return q.Range(r =>
                {
                    var iq = r.Name(field);

                    var iq2 = minInclusive
                                  ? iq.GreaterOrEquals(minVal)
                                  : iq.Greater(minVal);

                    var iq3 = maxInclusive
                                  ? iq2.LowerOrEquals(maxVal)
                                  : iq2.Lower(maxVal);
                });
            }
            
            if (isDate)
            {
                var minVal = min == null ? (DateTime?)null : Convert.ToDateTime(min);
                var maxVal = max == null ? (DateTime?)null : Convert.ToDateTime(max);

                return q.Range(r =>
                {
                    var iq = r.Name(field);

                    var iq2 = minInclusive
                                  ? iq.GreaterOrEquals(minVal)
                                  : iq.Greater(minVal);

                    var iq3 = maxInclusive
                                  ? iq2.LowerOrEquals(maxVal)
                                  : iq2.Lower(maxVal);
                });
            }
            
            if (isStringRange)
            {
                var minVal = Convert.ToString(min);
                var maxVal = Convert.ToString(max);

                return q.Range(r =>
                {
                    var iq = r.Name(field);

                    var iq2 = minInclusive
                                  ? iq.GreaterOrEquals(minVal)
                                  : iq.Greater(minVal);

                    var iq3 = maxInclusive
                                  ? iq2.LowerOrEquals(maxVal)
                                  : iq2.Lower(maxVal);
                });
            }
            
            return q.Term(t => t.Name(field).Value(value));
        }

        public void LoadIndices()
        {

        }

        public void CreateIndex(string indexName, string indexColumn)
        {
            //var isUserIndex = indexColumn.EndsWith("_");
            var fullName = /*isUserIndex ? */ string.Concat(indexColumn, indexName.ToLowerInvariant());// : indexColumn;

            if (_client.IndexExists(fullName).Exists)
                return;

            _client.CreateIndex(d => d.Index(fullName).Analysis(a => a.Analyzers(b => _analyzers)));
        }

        public List<string> GetIndices(string indexType)
        {
            var result = _client.GetAliases(s => s.Indices(string.Concat(indexType, "*"))).Indices.Where(pair => pair.Key.StartsWith(indexType)).Select(pair => pair.Key.Substring(indexType.Length)).ToList();
            return result;
            //return _client.IndicesStats(s => s.Indices(string.Concat(indexType, "*"))).Indices.Where(pair => pair.Key.StartsWith(indexType)).Select(pair => pair.Key).ToList();
        }

        public long DeleteIndex(IndexingService indexingService, string indexName, string indexColumn, Type indexType, bool isUserIndex)
        {
            var fullName = string.Concat(indexColumn, indexName.ToLowerInvariant());
            _client.DeleteIndex(fullName);
            return 0;
        }

        public void DropIndex(string indexName)
        {
            var realName = string.Concat(UserVertexIndicesColumnName, indexName.ToLowerInvariant());
            if (_client.IndexExists(realName).Exists)
                _client.DeleteIndex(realName);
            else
            {
                realName = string.Concat(UserEdgeIndicesColumnName, indexName.ToLowerInvariant());
                if (_client.IndexExists(realName).Exists)
                    _client.DeleteIndex(realName);
            }
        }
    }

    public class Ref
    {
        public long Id { get; set; }
    }
}
