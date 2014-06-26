using System.Diagnostics.Contracts;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        public static IVertex InV(this IEdge edge)
        {
            Contract.Requires(edge != null);
            Contract.Ensures(Contract.Result<IVertex>() != null);

            return edge.GetVertex(Direction.In);
        }

        public static IVertex<TModel> InV<TModel>(this IEdge edge) where TModel : class
        {
            Contract.Requires(edge != null);
            Contract.Ensures(Contract.Result<IVertex<TModel>>() != null);

            return edge.InV().As<TModel>();
        }
    }
}
