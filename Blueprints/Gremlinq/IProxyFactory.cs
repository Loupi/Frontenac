using System;
using System.Diagnostics.Contracts;
using Frontenac.Blueprints;
using Frontenac.Gremlinq.Contracts;

namespace Frontenac.Gremlinq
{
    [ContractClass(typeof(ProxyFactoryContract))]
    public interface  IProxyFactory
    {
        object Create(IElement element, Type proxyType);
    }
}