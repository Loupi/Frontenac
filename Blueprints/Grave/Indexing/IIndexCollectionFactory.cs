using System;
using System.Diagnostics.Contracts;

namespace Frontenac.Grave.Indexing
{
    [ContractClass(typeof(IndexCollectionFactoryContract))]
    public interface IIndexCollectionFactory
    {
        IIndexCollection Create(string indicesColumnName, Type indexType, bool isUserIndex,
                               IndexingService indexingService);
    }
}
