using System;
using System.Diagnostics.Contracts;
using Frontenac.Blueprints;
using Frontenac.Gremlinq.Contracts;

namespace Frontenac.Gremlinq
{
    [ContractClass(typeof(ElementTypeProviderContract))]
    public interface IElementTypeProvider
    {
        void SetType(IElement element, Type type);
        bool TryGetType(IElement element, out Type type);
        object Proxy(IElement element, Type type);
    }
}