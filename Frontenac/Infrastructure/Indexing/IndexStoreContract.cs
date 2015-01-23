using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Infrastructure.Indexing
{
    [ContractClassFor(typeof(IIndexStore))]
    public abstract class IndexStoreContract : IIndexStore
    {
        public void Load()
        {
            
        }

        public void Create(string indexName, string indexColumn, List<string> indices)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(indexName));
            Contract.Requires(!String.IsNullOrWhiteSpace(indexColumn));
            Contract.Requires(indices != null);
        }

        public List<string> Get(string indexType)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(indexType));
            return null;
        }

        public long Delete(IndexingService indexingService, string indexName, string indexColumn, Type indexType, List<string> indices,
                                    bool isUserIndex)
        {
            Contract.Requires(indexingService != null);
            Contract.Requires(!String.IsNullOrWhiteSpace(indexName));
            Contract.Requires(!String.IsNullOrWhiteSpace(indexColumn));
            Contract.Requires(indices != null);
            return 0;
        }

        public long DeleteIndex(Type indexType, string indexName, bool isUserIndex)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(indexName));
            return default(long);
        }

        public void Dispose()
        {
            
        }
    }
}