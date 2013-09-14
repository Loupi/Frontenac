using System.Diagnostics.Contracts;

namespace Grave.Indexing.Indexers
{
    [ContractClass(typeof (IndexerContract))]
    public abstract class Indexer
    {
        protected readonly IDocument Document;

        protected Indexer(IDocument document)
        {
            Contract.Requires(document != null);

            Document = document;
        }

        public abstract void Index(string documentName);
    }
}