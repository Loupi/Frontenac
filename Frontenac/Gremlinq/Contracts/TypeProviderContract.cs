using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq.Contracts
{
    [ContractClassFor(typeof (ITypeProvider))]
    public abstract class TypeProviderContract : ITypeProvider
    {
        public void SetType(IElement element, Type type)
        {
            Contract.Requires(element != null);
            Contract.Requires(type != null);
        }

        public bool TryGetType(IDictionary<string, object> element, IGraph graph, out Type type)
        {
            Contract.Requires(element != null);
            Contract.Requires(graph != null);
            type = null;
            return false;
        }

        public IEnumerable<IVertex> GetVerticesOfType(IGraph graph, Type type)
        {
            Contract.Requires(graph != null);
            Contract.Requires(type != null);
            Contract.Ensures(Contract.Result<IEnumerable<IVertex>>() != null);
            
            return null;
        }

        public IEnumerable<IEdge> GetEdgesOfType(IGraph graph, Type type)
        {
            Contract.Requires(graph != null);
            Contract.Requires(type != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge>>() != null);

            return null;
        }

        public IEnumerable<Type> GetTypes(IGraph graph)
        {
            Contract.Requires(graph != null);
            Contract.Ensures(Contract.Result<IEnumerable<Type>>() != null);

            return null;
        }
    }
}
