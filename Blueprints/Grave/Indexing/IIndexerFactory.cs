using Grave.Indexing.Indexers;

namespace Grave.Indexing
{
    public interface IIndexerFactory
    {
        Indexer Create(object content, IDocument document);
    }
}
