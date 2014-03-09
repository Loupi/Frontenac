using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        public static IEnumerable<IVertex> V(this IGraph graph, string propertyName, object value)
        {
            Contract.Requires(graph != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(propertyName));
            Contract.Ensures(Contract.Result<IEnumerable<IVertex>>() != null);

            return graph.GetVertices(propertyName, value);
        }

        public static IEnumerable<IVertex<TModel>> V<TModel, TValue>(this IGraph graph,
            Expression<Func<TModel, TValue>> propertySelector,
            TValue value) where TModel : class
        {
            Contract.Requires(graph != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IVertex<TModel>>>() != null);

            return graph.V(propertySelector.Resolve(), value).As<TModel>();
        }
    }
}
