using System.Diagnostics.Contracts;

namespace Frontenac.Grave.Indexing
{
    [ContractClassFor(typeof (IDocument))]
    public abstract class DocumentContract : IDocument
    {
        public bool Write(string key, object value)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
            return default(bool);
        }

        public bool Present(object value)
        {
            return default(bool);
        }
    }
}