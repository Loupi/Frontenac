using System.Diagnostics.Contracts;

namespace Frontenac.Infrastructure.Indexing.Indexers
{
    [ContractClassFor(typeof (Indexer))]
    public abstract class IndexerContract : Indexer
    {
        protected IndexerContract(IDocument document) : base(document)
        {
        }

        public override void Index(string documentName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(documentName));
        }
    }
}