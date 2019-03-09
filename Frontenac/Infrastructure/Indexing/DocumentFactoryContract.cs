using System;
using System.Diagnostics.Contracts;

namespace Frontenac.Infrastructure.Indexing
{
    [ContractClassFor(typeof (IDocumentFactory))]
    public abstract class DocumentFactoryContract : IDocumentFactory
    {
        public IDocument Create(object document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            return null;
        }
    }
}