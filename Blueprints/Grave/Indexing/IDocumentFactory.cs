using System.Diagnostics.Contracts;

namespace Frontenac.Grave.Indexing
{
    [ContractClass(typeof (DocumentFactoryContract))]
    public interface IDocumentFactory
    {
        IDocument Create(object document);
    }
}