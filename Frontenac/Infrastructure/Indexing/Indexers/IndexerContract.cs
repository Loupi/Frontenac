using System.Diagnostics.Contracts;

namespace Frontenac.Infrastructure.Indexing.Indexers
{
    [ContractClassFor(typeof (Indexer))]
    public abstract class IndexerContract : Indexer
    {
        public override void Index(IDocument document, string documentName, object content)
        {
            Contract.Requires(document != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(documentName));
            Contract.Requires(content != null);
        }
    }
}