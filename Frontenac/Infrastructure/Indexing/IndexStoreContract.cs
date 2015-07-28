using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Frontenac.Blueprints;

namespace Frontenac.Infrastructure.Indexing
{
    [ContractClassFor(typeof(IIndexStore))]
    public abstract class IndexStoreContract : IIndexStore
    {
        public void LoadIndices()
        {
            
        }

        public void CreateIndex(string indexName, string indexColumn, Parameter[] parameters)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(indexName));
            Contract.Requires(!String.IsNullOrWhiteSpace(indexColumn));
        }

        public List<string> GetIndices(string indexType)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(indexType));
            return null;
        }

        public long DeleteIndex(IndexingService indexingService, string indexName, string indexColumn, Type indexType, bool isUserIndex)
        {
            Contract.Requires(indexingService != null);
            Contract.Requires(!String.IsNullOrWhiteSpace(indexName));
            Contract.Requires(!String.IsNullOrWhiteSpace(indexColumn));
            return 0;
        }

        public long DeleteIndex(Type indexType, string indexName, bool isUserIndex)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(indexName));
            return default(long);
        }

        public void DropIndex(string indexName)
        {
            //Contract.Requires(!string.IsNullOrWhiteSpace(indexName));
        }

        public void Dispose()
        {
            
        }
    }
}