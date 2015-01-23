using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Frontenac.Blueprints;
using System.Linq;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        public static IVertex OutV(this IEdge edge)
        {
            Contract.Requires(edge != null);
            Contract.Ensures(Contract.Result<IVertex>() != null);

            return edge.GetVertex(Direction.Out);
        }

        public static IVertex<TModel> OutV<TModel>(this IEdge edge) where TModel : class
        {
            Contract.Requires(edge != null);
            Contract.Ensures(Contract.Result<IVertex<TModel>>() != null);

            return edge.OutV().As<TModel>();
        }

        public static IEnumerable<IVertex> OutV(this IEnumerable<IEdge> edges)
        {
            Contract.Requires(edges != null);
            Contract.Ensures(Contract.Result<IEnumerable<IVertex>>() != null);

            return edges.Select(edge => edge.OutV());
        }

        public static IEnumerable<IVertex<TModel>> OutV<TModel>(this IEnumerable<IEdge> edges)
            where TModel: class 
        {
            Contract.Requires(edges != null);
            Contract.Ensures(Contract.Result<IEnumerable<IVertex>>() != null);

            return edges.Select(edge => edge.OutV<TModel>());
        }
    }
}
