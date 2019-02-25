using System;
using System.Collections;

namespace Frontenac.Gremlinq
{
    public interface  IProxyFactory
    {
        object Create(IDictionary element, Type proxyType);
    }
}