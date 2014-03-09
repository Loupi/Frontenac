using System.Diagnostics.Contracts;
using Frontenac.Grave.Indexing.Indexers;

namespace Frontenac.Grave.Indexing
{
    [ContractClass(typeof (IndexerFactoryContract))]
    public interface IIndexerFactory
    {
        Indexer Create(object content, IDocument document);
    }
}