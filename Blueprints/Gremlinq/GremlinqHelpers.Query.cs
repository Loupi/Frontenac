using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        public static IQuery<TModel> Query<TModel>(this IGraph graph)
        {
            Contract.Requires(graph != null);
            Contract.Ensures(Contract.Result<IQuery<TModel>>() != null);

            return new Query<TModel>(graph.Query());
        }

        public static IQuery<TModel> Has<TModel, TResult>(
            this IQuery<TModel> query,
            Expression<Func<TModel, TResult>> propertySelector,
            Compare compare, TResult value)
        {
            Contract.Requires(query != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IQuery<TModel>>() != null);

            query.InnerQuery.Has(propertySelector.Resolve(), compare, value);
            return query;
        }

        public static IEnumerable<IEdge<TModel>> Edges<TModel>(this IQuery<TModel> query) where TModel : class
        {
            Contract.Requires(query != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TModel>>>() != null);

            return query.InnerQuery.Edges().As<TModel>();
        }
    }
}
