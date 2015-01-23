using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        public static IEnumerable<IEdge> E(this IGraph graph)
        {
            Contract.Requires(graph != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge>>() != null);

            return graph.GetEdges();
        }

        public static IEnumerable<IEdge<TModel>> E<TModel>(this IGraph graph)
            where TModel : class 
        {
            Contract.Requires(graph != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TModel>>>() != null);

            return graph.GetEdges().As<TModel>();
        }

        public static IEdge E(this IGraph graph, object id)
        {
            Contract.Requires(graph != null);
            Contract.Requires(id != null);
            Contract.Ensures(Contract.Result<IEdge>() != null);

            return graph.GetEdge(id);
        }

        public static IEdge<TModel> E<TModel>(this IGraph graph, object id) where TModel : class
        {
            Contract.Requires(graph != null);
            Contract.Requires(id != null);
            Contract.Ensures(Contract.Result<IEdge<TModel>>() != null);

            return graph.E(id).As<TModel>();
        }

        public static IEnumerable<IEdge> E(this IGraph graph, string propertyName, object value)
        {
            Contract.Requires(graph != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(propertyName));
            Contract.Ensures(Contract.Result<IEnumerable<IEdge>>() != null);

            return graph.GetEdges(propertyName, value);
        }

        public static IEnumerable<IEdge<TModel>> E<TModel, TValue>(this IGraph graph,
            Expression<Func<TModel, TValue>> propertySelector,
            TValue value) where TModel : class
        {
            Contract.Requires(graph != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TModel>>>() != null);

            return graph.E(propertySelector.Resolve(), value).As<TModel>();
        }
    }
}
