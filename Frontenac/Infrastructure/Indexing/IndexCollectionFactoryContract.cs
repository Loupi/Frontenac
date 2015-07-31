using System;
using System.Diagnostics.Contracts;

namespace Frontenac.Infrastructure.Indexing
{
    [ContractClassFor(typeof(IIndexCollectionFactory))]
    public abstract class IndexCollectionFactoryContract : IIndexCollectionFactory
    {
        public IIndexCollection Create(string indicesColumnName, Type indexType, bool isUserIndex, IndexingService indexingService)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(indicesColumnName));
            Contract.Requires(indexingService != null);
            return null;
        }
    }
}
