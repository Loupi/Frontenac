using System.Diagnostics.Contracts;

namespace Grave.Indexing
{
    [ContractClass(typeof(DocumentFactoryContract))]
    public interface IDocumentFactory
    {
        IDocument Create(object document);
    }
}
