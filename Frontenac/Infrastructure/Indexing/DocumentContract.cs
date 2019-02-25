using System;
using System.Diagnostics.Contracts;

namespace Frontenac.Infrastructure.Indexing
{
    [ContractClassFor(typeof (IDocument))]
    public abstract class DocumentContract : IDocument
    {
        public bool Write(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            return default(bool);
        }

        public bool Present(object value)
        {
            return default(bool);
        }
    }
}