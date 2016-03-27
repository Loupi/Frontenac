using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Frontenac.Blueprints;
using Frontenac.Gremlinq.Contracts;

namespace Frontenac.Gremlinq
{
    [ContractClass(typeof(TypeProviderContract))]
    public interface ITypeProvider
    {
        void SetType(IElement element, Type type);
        bool TryGetType(IDictionary<string, object> element, IGraph graph, out Type type);
        IEnumerable<IVertex> GetVerticesOfType(IGraph graph, Type type);
        IEnumerable<IEdge> GetEdgesOfType(IGraph graph, Type type);
        IEnumerable<Type> GetTypes(IGraph graph);
    }
}