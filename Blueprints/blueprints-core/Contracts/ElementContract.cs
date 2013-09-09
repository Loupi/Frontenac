using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Contracts
{
    [ContractClassFor(typeof(IElement))]
    public abstract class ElementContract : IElement
    {
        public object GetProperty(string key)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
            return null;
        }

        public IEnumerable<string> GetPropertyKeys()
        {
            Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);
            return null;
        }

        public void SetProperty(string key, object value)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
        }

        public object RemoveProperty(string key)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
            return null;
        }

        public void Remove()
        {
            
        }

        public object Id 
        {
            get
            {
                Contract.Ensures(Contract.Result<object>() != null);
                return default(object);
            }
        }
    }
}
