using System.Diagnostics.Contracts;

namespace Frontenac.Infrastructure.Indexing.Indexers
{
    [ContractClass(typeof (IndexerContract))]
    public abstract class Indexer
    {
        public abstract void Index(IDocument document, string documentName, object content);
    }
}