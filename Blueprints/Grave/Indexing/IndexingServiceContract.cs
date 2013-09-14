using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Grave.Esent;

namespace Grave.Indexing
{
    [ContractClassFor(typeof(IndexingService))]
    public abstract class IndexingServiceContract : IndexingService
    {
        protected IndexingServiceContract(EsentConfigContext context) : base(context)
        {
        }

        public override long DeleteDocuments(Type indexType, int id)
        {
            Contract.Requires(IsValidIndexType(indexType));
            return default(long);
        }

        public override long DeleteIndex(Type indexType, string indexName, bool isUserIndex)
        {
            Contract.Requires(IsValidIndexType(indexType));
            Contract.Requires(!string.IsNullOrWhiteSpace(indexName));
            return default(long);
        }

        public override long DeleteUserDocuments(Type indexType, int id, string key, object value)
        {
            Contract.Requires(IsValidIndexType(indexType));
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
            return default(long);
        }

        public override IEnumerable<int> Get(Type indexType, string indexName, string key, object value, bool isUserIndex, int hitsLimit = 1000)
        {
            Contract.Requires(IsValidIndexType(indexType));
            Contract.Requires(!string.IsNullOrWhiteSpace(indexName));
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
            Contract.Requires(hitsLimit >= 0);
            Contract.Ensures(Contract.Result<IEnumerable<int>>() != null);
            return null;
        }

        public override IEnumerable<int> Query(Type indexType, IEnumerable<GraveQueryElement> query, int hitsLimit = 1000)
        {
            Contract.Requires(IsValidIndexType(indexType));
            Contract.Requires(query != null);
            Contract.Requires(hitsLimit >= 0);
            Contract.Ensures(Contract.Result<IEnumerable<int>>() != null);
            return null;
        }

        public override long Set(Type indexType, int id, string indexName, string propertyName, object value, bool isUserIndex)
        {
            Contract.Requires(IsValidIndexType(indexType));
            Contract.Requires(!string.IsNullOrWhiteSpace(indexName));
            Contract.Requires(!string.IsNullOrWhiteSpace(propertyName));
            return default(long);
        }

        public override void WaitForGeneration(long generation)
        {
            
        }
    }
}
