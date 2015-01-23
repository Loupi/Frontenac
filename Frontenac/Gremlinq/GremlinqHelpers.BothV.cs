using System.Diagnostics.Contracts;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        public static IVertex[] BothV(this IEdge edge)
        {
            Contract.Requires(edge != null);
            Contract.Ensures(Contract.Result<IVertex[]>() != null);

            return new []{edge.OutV(), edge.InV()};
        }

        public static IVertex<TModel>[] BothV<TModel>(this IEdge edge) where TModel : class
        {
            Contract.Requires(edge != null);
            Contract.Ensures(Contract.Result<IVertex<TModel>[]>() != null);

            return new[] { edge.OutV<TModel>(), edge.InV<TModel>() };
        }
    }
}
