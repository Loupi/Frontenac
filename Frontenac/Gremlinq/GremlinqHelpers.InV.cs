using System;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        public static IVertex InV(this IEdge edge)
        {
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));

            return edge.GetVertex(Direction.In);
        }

        public static IVertex<TModel> InV<TModel>(this IEdge edge) where TModel : class
        {
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));

            return edge.InV().As<TModel>();
        }
    }
}
