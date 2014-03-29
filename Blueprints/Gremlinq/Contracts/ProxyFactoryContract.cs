using System;
using System.Collections;
using System.Diagnostics.Contracts;

namespace Frontenac.Gremlinq.Contracts
{
    [ContractClassFor(typeof(IProxyFactory))]
    public abstract class ProxyFactoryContract : IProxyFactory
    {
        public object Create(IDictionary element, Type type)
        {
            Contract.Requires(element != null);
            Contract.Requires(type != null);
            Contract.Ensures(Contract.Result<object>() != null);
            return null;
        }
    }
}