using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Infrastructure.Indexing
{
    [ContractClassFor(typeof (IndexingService))]
    public abstract class IndexingServiceContract : IndexingService
    {
        protected IndexingServiceContract(IIndexCollectionFactory indexCollectionFactory) 
            : base(indexCollectionFactory)
        {
        }

        public override long DeleteDocuments(Type indexType, long id)
        {
            return default(long);
        }

        public override long DeleteUserDocuments(Type indexType, long id, string key, object value)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
            return default(long);
        }

        public override IEnumerable<long> Get(Type indexType, string indexName, string key, object value,
                                             bool isUserIndex, int hitsLimit = 1000)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
            Contract.Requires(hitsLimit >= 0);
            Contract.Ensures(Contract.Result<IEnumerable<long>>() != null);
            return null;
        }

        public override IEnumerable<long> Query(Type indexType, IEnumerable<QueryElement> query,
                                               int hitsLimit = 1000)
        {
            Contract.Requires(query != null);
            Contract.Requires(hitsLimit >= 0);
            Contract.Ensures(Contract.Result<IEnumerable<long>>() != null);
            return null;
        }

        public override long Set(Type indexType, long id, string indexName, string propertyName, object value,
                                 bool isUserIndex)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(indexName));
            Contract.Requires(!string.IsNullOrWhiteSpace(propertyName));
            return default(long);
        }

        public override void WaitForGeneration(long generation)
        {
        }
    }
}