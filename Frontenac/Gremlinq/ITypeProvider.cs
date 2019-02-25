using System;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public interface ITypeProvider
    {
        void SetType(IElement element, Type type);
        bool TryGetType(IElement element, out Type type);
    }
}