using System;
using System.Diagnostics.Contracts;

namespace Frontenac.Infrastructure.Indexing
{
    [ContractClass(typeof(IndexCollectionFactoryContract))]
    public interface IIndexCollectionFactory
    {
        IIndexCollection Create(string indicesColumnName, Type indexType, bool isUserIndex, IndexingService indexingService);
    }
}
