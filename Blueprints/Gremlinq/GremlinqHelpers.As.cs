using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        public static IVertex<TModel> As<TModel>(this IVertex vertex) 
            where TModel : class
        {
            Contract.Requires(vertex != null);
            Contract.Ensures(Contract.Result<IVertex<TModel>>() != null);

            return new Vertex<TModel>(vertex, vertex.Proxy<TModel>());
        }

        public static IVertex<TCastModel> As<TCastModel>(this IVertex<TCastModel> vertex) 
            where TCastModel : class
        {
            Contract.Requires(vertex != null);
            Contract.Ensures(Contract.Result<IVertex<TCastModel>>() != null);

            return vertex;
        }

        public static IEnumerable<IVertex<TCastModel>> As<TModel, TCastModel>(this IEnumerable<IVertex<TModel>> vertex) 
            where TModel : class 
            where TCastModel : class
        {
            Contract.Requires(vertex != null);
            Contract.Ensures(Contract.Result<IEnumerable<IVertex<TCastModel>>>() != null);

            return vertex.Cast<IVertex<TCastModel>>();
        }

        public static IEnumerable<IVertex<TModel>> As<TModel>(this IEnumerable<IVertex> vertices)
            where TModel : class
        {
            Contract.Requires(vertices != null);
            Contract.Ensures(Contract.Result<IEnumerable<IVertex<TModel>>>() != null);

            return vertices.Select(t => t.As<TModel>());
        }

        public static IEdge<TModel> As<TModel>(this IEdge edge) 
            where TModel : class
        {
            Contract.Requires(edge != null);
            Contract.Ensures(Contract.Result<IEdge>() != null);

            return new Edge<TModel>(edge, edge.Proxy<TModel>());
        }

        public static IEdge<TCastModel> As<TModel, TCastModel>(this IEdge<TModel> edge)
            where TModel : class
        {
            Contract.Requires(edge != null);
            Contract.Ensures(Contract.Result<IEdge<TCastModel>>() != null);

            return (IEdge<TCastModel>)edge;
        }

        public static IEnumerable<IEdge<TCastModel>> As<TModel, TCastModel>(this IEnumerable<IEdge<TModel>> edge)
            where TModel : class
            where TCastModel : class
        {
            Contract.Requires(edge != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TCastModel>>>() != null);

            return edge.Cast<IEdge<TCastModel>>();
        }

        public static IEnumerable<IEdge<TModel>> As<TModel>(this IEnumerable<IEdge> edges) 
            where TModel : class
        {
            Contract.Requires(edges != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TModel>>>() != null);

            return edges.Select(t => t.As<TModel>());
        }
    }
}
