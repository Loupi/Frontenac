using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Infrastructure.Indexing
{
    [ContractClassFor(typeof (IndexingService))]
    public abstract class IndexingServiceContract : IndexingService
    {
        public override long DeleteDocuments(Type indexType, long id)
        {
            return default(long);
        }

        public override long DeleteUserDocuments(Type indexType, long id, string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            return default(long);
        }

        public override IEnumerable<long> Get(Type indexType, string indexName, string key, object value,
                                             bool isUserIndex, int hitsLimit = 1000)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            if (hitsLimit < 0)
                throw new ArgumentException("hitsLimit must be greater than or equal to zero");
            return null;
        }

        public override IEnumerable<long> Query(Type indexType, IEnumerable<QueryElement> query,
                                               int hitsLimit = 1000)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));
            if (hitsLimit < 0)
                throw new ArgumentException("hitsLimit must be greater than or equal to zero");
            return null;
        }

        public override long Set(Type indexType, long id, string indexName, string propertyName, object value,
                                 bool isUserIndex)
        {
            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentNullException(nameof(indexName));
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException(nameof(propertyName));
            return default(long);
        }

        public override void WaitForGeneration(long generation)
        {
        }
    }
}