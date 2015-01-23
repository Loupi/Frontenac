using System;
using System.Diagnostics.Contracts;
using Frontenac.Infrastructure.Indexing.Indexers;

namespace Frontenac.Infrastructure.Indexing
{
    [ContractClass(typeof (IndexerFactoryContract))]
    public interface IIndexerFactory
    {
        Indexer Create(Type contentType);
        void Destroy(Indexer indexer);
    }
}