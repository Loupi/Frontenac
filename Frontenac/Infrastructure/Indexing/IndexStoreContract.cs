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
            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentNullException(nameof(indexName));
            if (string.IsNullOrWhiteSpace(indexColumn))
                throw new ArgumentNullException(nameof(indexColumn));
        }

        public List<string> GetIndices(string indexType)
        {
            if (string.IsNullOrWhiteSpace(indexType))
                throw new ArgumentNullException(nameof(indexType));
            return null;
        }

        public long DeleteIndex(IndexingService indexingService, string indexName, string indexColumn, Type indexType, bool isUserIndex)
        {
            if (indexingService == null)
                throw new ArgumentNullException(nameof(indexingService));
            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentNullException(nameof(indexName));
            if (string.IsNullOrWhiteSpace(indexColumn))
                throw new ArgumentNullException(nameof(indexColumn));
            return 0;
        }

        public long DeleteIndex(Type indexType, string indexName, bool isUserIndex)
        {
            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentNullException(nameof(indexName));
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