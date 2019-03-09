using System;
using System.Diagnostics.Contracts;
using Frontenac.Infrastructure.Indexing.Indexers;

namespace Frontenac.Infrastructure.Indexing
{
    [ContractClassFor(typeof (IIndexerFactory))]
    public abstract class IndexerFactoryContract : IIndexerFactory
    {
        public Indexer Create(Type contentType)
        {
            if (contentType == null)
                throw new ArgumentNullException(nameof(contentType));
            return null;
        }

        public void Destroy(Indexer indexer)
        {
            if (indexer == null)
                throw new ArgumentNullException(nameof(indexer));
        }
    }
}