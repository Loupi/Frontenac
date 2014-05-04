using Frontenac.Grave.Esent;

namespace Frontenac.Grave.Indexing
{
    public interface IIndexingServiceFactory
    {
        IndexingService Create(EsentContext context);
    }
}