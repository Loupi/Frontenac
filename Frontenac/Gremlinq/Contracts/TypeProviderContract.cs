using System;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq.Contracts
{
    public static class TypeProviderContract
    {
        public static void ValidateSetType(IElement element, Type type)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (type == null)
                throw new ArgumentNullException(nameof(type));
        }

        public static void ValidateTryGetType(IElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
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
