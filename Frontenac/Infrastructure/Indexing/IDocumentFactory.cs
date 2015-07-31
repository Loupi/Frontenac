using System.Diagnostics.Contracts;

namespace Frontenac.Infrastructure.Indexing
{
    [ContractClass(typeof (DocumentFactoryContract))]
    public interface IDocumentFactory
    {
        IDocument Create(object document);
    }
}