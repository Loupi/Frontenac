using System;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Contracts
{
    [ContractClassFor(typeof(IIndex))]
    public abstract class IndexContract : IIndex
    {
        public string Name
        {
            get
            {
                Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));
                return null;
            }
        }

        public Type Type
        {
            get
            {
                Contract.Ensures(Contract.Result<Type>() != null);
                return null;
            }
        }

        public void Put(string key, object value, IElement element)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
            Contract.Requires(element != null);
            Contract.Requires(Type.IsInstanceOfType(element));
        }

        public ICloseableIterable<IElement> Get(string key, object value)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
            Contract.Ensures(Contract.Result<ICloseableIterable<IElement>>() != null);
            return null;
        }

        public ICloseableIterable<IElement> Query(string key, object query)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
            Contract.Ensures(Contract.Result<ICloseableIterable<IElement>>() != null);
            return null;
        }

        public long Count(string key, object value)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
            Contract.Ensures(Contract.Result<long>() >= 0);
            return default(long);
        }

        public void Remove(string key, object value, IElement element)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
            Contract.Requires(element != null);
            Contract.Requires(Type.IsInstanceOfType(element));
        }
    }
}
