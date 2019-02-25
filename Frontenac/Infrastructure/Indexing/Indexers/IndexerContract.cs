using System;
using System.Diagnostics.Contracts;

namespace Frontenac.Infrastructure.Indexing.Indexers
{
    [ContractClassFor(typeof (Indexer))]
    public abstract class IndexerContract : Indexer
    {
        public override void Index(IDocument document, string documentName, object content)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            if (string.IsNullOrWhiteSpace(documentName))
                throw new ArgumentNullException(nameof(documentName));
            if (content == null)
                throw new ArgumentNullException(nameof(content));
        }
    }
}