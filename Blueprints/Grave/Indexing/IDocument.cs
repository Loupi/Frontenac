using System.Diagnostics.Contracts;

namespace Frontenac.Grave.Indexing
{
    [ContractClass(typeof (DocumentContract))]
    public interface IDocument
    {
        bool Write(string key, object value);
        bool Present(object value);
    }
}