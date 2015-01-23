using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using Elasticsearch.Net;
using Frontenac.Blueprints;

namespace Frontenac.Infrastructure.Indexing.ElasticSearch
{
    public class ElasticSearchService : IndexingService
    {
        private readonly ElasticsearchClient _client;

        public ElasticSearchService(ElasticsearchClient client)
        {
            _client = client;
        }

        public override void Initialize(string databasePath)
        {
            
        }

        private static string GetIndexName(Type indexType)
        {
            Contract.Requires(indexType != null);
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

            return indexType == typeof(IVertex) ? "Vertex" : "Edge";
        }

        private static string GetUserIndexName(Type indexType)
        {
            Contract.Requires(indexType != null);
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

            return indexType == typeof (IVertex) ? "UserVertex" : "UserEdge";
        }

        public override long Set(Type indexType, long id, string indexName, string propertyName, object value, bool isUserIndex)
        {
            var indexTypeName = isUserIndex ? GetUserIndexName(indexType) : GetIndexName(indexType);
            var values = new Dictionary<string, object> {{propertyName, value}};
            _client.Index(indexTypeName, indexName, id.ToString(CultureInfo.InvariantCulture), values);
            return 0;
        }

        public override void WaitForGeneration(long generation)
        {
            
        }

        public override IEnumerable<long> Get(Type indexType, string indexName, string key, object value, bool isUserIndex, int hitsLimit = 1000)
        {
            var indexTypeName = isUserIndex ? GetUserIndexName(indexType) : GetIndexName(indexType);
            var values = new Dictionary<string, object> {{key, value}};
            var response = _client.Search(indexTypeName, indexName, values);
            
            throw new NotImplementedException();
        }

        public override long DeleteDocuments(Type indexType, long id)
        {
            var q = string.Concat("_id = ", id);
            var indexTypeName = GetIndexName(indexType);
            _client.DeleteByQuery(indexTypeName, q);

            indexTypeName = GetUserIndexName(indexType);
            _client.DeleteByQuery(indexTypeName, q);

            return 0;
        }

        public override long DeleteUserDocuments(Type indexType, long id, string key, object value)
        {
            var indexTypeName = GetUserIndexName(indexType);
            var values = new Dictionary<string, object> {{"_id", indexType.ToString()}, {key, value}};
            _client.DeleteByQuery(indexTypeName, string.Concat("_id = ", id), values);
            return 0;
        }

        public override IEnumerable<long> Query(Type indexType, IEnumerable<QueryElement> query, int hitsLimit = 1000)
        {
            throw new NotImplementedException();
        }

        public override long DeleteIndex(Type indexType, string indexName, bool isUserIndex)
        {
            var indexTypeName = isUserIndex ? GetUserIndexName(indexType) : GetIndexName(indexType);
            _client.IndicesDeleteMapping(indexTypeName, indexName);
            return 0;
        }

        public override void Commit()
        {

        }

        public override void Prepare()
        {

        }

        public override void Rollback()
        {
            
        }
    }
}
