using System;
using System.Diagnostics.Contracts;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq.Contracts
{
    [ContractClassFor(typeof(IProxyFactory))]
    public abstract class ProxyFactoryContract : IProxyFactory
    {
        public object Create(IElement element, Type type)
        {
            Contract.Requires(element != null);
            Contract.Requires(type != null);
            Contract.Ensures(Contract.Result<object>() != null);
            return null;
        }
    }
}