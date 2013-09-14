using System.Diagnostics.Contracts;
using Grave.Indexing.Indexers;

namespace Grave.Indexing
{
    [ContractClass(typeof (IndexerFactoryContract))]
    public interface IIndexerFactory
    {
        Indexer Create(object content, IDocument document);
    }
}