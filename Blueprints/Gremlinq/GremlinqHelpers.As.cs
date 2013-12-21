using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        public static IVertex<TModel> As<TModel>(this IVertex vertex) where TModel : class
        {
            Contract.Requires(vertex != null);
            Contract.Ensures(Contract.Result<IVertex<TModel>>() != null);

            return new Vertex<TModel>(vertex, vertex.Proxy<TModel>());
        }

        public static IEdge<TModel> As<TModel>(this IEdge edge) where TModel : class
        {
            Contract.Requires(edge != null);
            Contract.Ensures(Contract.Result<IEdge>() != null);

            return new Edge<TModel>(edge, edge.Proxy<TModel>());
        }

        public static IEnumerable<IVertex<TModel>> As<TModel>(this IEnumerable<IVertex> vertices) where TModel : class
        {
            Contract.Requires(vertices != null);
            Contract.Ensures(Contract.Result<IEnumerable<IVertex<TModel>>>() != null);

            return vertices.Select(t => t.As<TModel>());
        }

        public static IEnumerable<IEdge<TModel>> As<TModel>(this IEnumerable<IEdge> edges) where TModel : class
        {
            Contract.Requires(edges != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TModel>>>() != null);

            return edges.Select(t => t.As<TModel>());
        }
    }
}
