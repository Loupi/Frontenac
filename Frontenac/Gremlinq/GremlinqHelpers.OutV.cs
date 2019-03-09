using System;
using System.Collections.Generic;
using Frontenac.Blueprints;
using System.Linq;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        public static IVertex OutV(this IEdge edge)
        {
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));

            return edge.GetVertex(Direction.Out);
        }

        public static IVertex<TModel> OutV<TModel>(this IEdge edge) where TModel : class
        {
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));

            return edge.OutV().As<TModel>();
        }

        public static IEnumerable<IVertex> OutV(this IEnumerable<IEdge> edges)
        {
            if (edges == null)
                throw new ArgumentNullException(nameof(edges));

            return edges.Select(edge => edge.OutV());
        }

        public static IEnumerable<IVertex<TModel>> OutV<TModel>(this IEnumerable<IEdge> edges)
            where TModel: class 
        {
            if (edges == null)
                throw new ArgumentNullException(nameof(edges));

            return edges.Select(edge => edge.OutV<TModel>());
        }
    }
}
