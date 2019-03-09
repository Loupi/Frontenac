using System;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        public static IVertex[] BothV(this IEdge edge)
        {
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));

            return new []{edge.OutV(), edge.InV()};
        }

        public static IVertex<TModel>[] BothV<TModel>(this IEdge edge) where TModel : class
        {
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));

            return new[] { edge.OutV<TModel>(), edge.InV<TModel>() };
        }
    }
}
