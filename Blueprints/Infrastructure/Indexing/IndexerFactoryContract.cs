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
            Contract.Requires(contentType != null);
            Contract.Ensures(Contract.Result<Indexer>() != null);
            return null;
        }

        public void Destroy(Indexer indexer)
        {
            Contract.Requires(indexer != null);
        }
    }
}