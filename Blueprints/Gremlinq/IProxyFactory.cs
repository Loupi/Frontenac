using System;
using System.Collections;
using System.Diagnostics.Contracts;
using Frontenac.Gremlinq.Contracts;

namespace Frontenac.Gremlinq
{
    [ContractClass(typeof(ProxyFactoryContract))]
    public interface  IProxyFactory
    {
        object Create(IDictionary element, Type proxyType);
    }
}