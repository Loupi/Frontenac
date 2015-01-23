using System.Diagnostics.Contracts;

namespace Frontenac.Infrastructure.Indexing
{
    [ContractClassFor(typeof (IDocumentFactory))]
    public abstract class DocumentFactoryContract : IDocumentFactory
    {
        public IDocument Create(object document)
        {
            Contract.Requires(document != null);
            Contract.Ensures(Contract.Result<IDocument>() != null);
            return null;
        }
    }
}