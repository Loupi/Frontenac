using System;
using System.Diagnostics.Contracts;

namespace Frontenac.Infrastructure.Indexing
{
    [ContractClassFor(typeof(IIndexCollectionFactory))]
    public abstract class IndexCollectionFactoryContract : IIndexCollectionFactory
    {
        public IIndexCollection Create(string indicesColumnName, Type indexType, bool isUserIndex, IndexingService indexingService)
        {
            if (string.IsNullOrWhiteSpace(indicesColumnName))
                throw new ArgumentNullException(nameof(indicesColumnName));
            if (indexingService == null)
                throw new ArgumentNullException(nameof(indexingService));
            return null;
        }
    }
}
