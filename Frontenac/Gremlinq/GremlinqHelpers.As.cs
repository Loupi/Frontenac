using System;
using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        public static IVertex<TModel> As<TModel>(this IVertex vertex) 
            where TModel : class
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));

            return new Vertex<TModel>(vertex, vertex.Proxy<TModel>());
        }

        public static IVertex<TCastModel> As<TCastModel>(this IVertex<TCastModel> vertex) 
            where TCastModel : class
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));

            return vertex;
        }

        public static IEnumerable<IVertex<TCastModel>> As<TModel, TCastModel>(this IEnumerable<IVertex<TModel>> vertex) 
            where TModel : class 
            where TCastModel : class
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));

            return vertex.Cast<IVertex<TCastModel>>();
        }

        public static IEnumerable<IVertex<TModel>> As<TModel>(this IEnumerable<IVertex> vertices)
            where TModel : class
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));

            return vertices.Select(t => t.As<TModel>());
        }

        public static IEdge<TModel> As<TModel>(this IEdge edge) 
            where TModel : class
        {
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));

            return new Edge<TModel>(edge, edge.Proxy<TModel>());
        }

        public static IEdge<TCastModel> As<TModel, TCastModel>(this IEdge<TModel> edge)
            where TModel : class
        {
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));

            return (IEdge<TCastModel>)edge;
        }

        public static IEnumerable<IEdge<TCastModel>> As<TModel, TCastModel>(this IEnumerable<IEdge<TModel>> edge)
            where TModel : class
            where TCastModel : class
        {
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));

            return edge.Cast<IEdge<TCastModel>>();
        }

        public static IEnumerable<IEdge<TModel>> As<TModel>(this IEnumerable<IEdge> edges) 
            where TModel : class
        {
            if (edges == null)
                throw new ArgumentNullException(nameof(edges));

            return edges.Select(t => t.As<TModel>());
        }
    }
}
