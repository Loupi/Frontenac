using System;

namespace Frontenac.Grave.Indexing
{
    public class TransactionalIndexCollectionFactory : IIndexCollectionFactory
    {
        public IIndexCollection Create(string indicesColumnName, Type indexType, bool isUserIndex, IndexingService indexingService)
        {
            return new TransactionalIndexCollection(new IndexCollection(indicesColumnName, indexType, isUserIndex, indexingService));
        }
    }
}
