using System;

namespace Frontenac.Infrastructure.Indexing
{
    public class IndexCollectionFactory : IIndexCollectionFactory
    {
        public IIndexCollection Create(string indicesColumnName, Type indexType, bool isUserIndex, IndexingService indexingService)
        {
            return new IndexCollection(indicesColumnName, indexType, isUserIndex, indexingService);
        }
    }
}
