using System.Diagnostics.Contracts;

namespace Frontenac.Infrastructure.Indexing
{
    [ContractClass(typeof (DocumentContract))]
    public interface IDocument
    {
        bool Write(string key, object value);
        bool Present(object value);
    }
}