using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Geo;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        public static IQuery<TModel> Query<TModel>(this IGraph graph)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            return new Query<TModel>(graph.Query());
        }

        public static IQuery<TModel> Has<TModel>(
            this IQuery<TModel> query,
            Expression<Func<TModel, GeoPoint>> propertySelector,
            IGeoShape value)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            query.InnerQuery.Has(propertySelector.Resolve(), Compare.Equal, value);
            return query;
        }

        public static IQuery<TModel> Has<TModel, TResult>(
            this IQuery<TModel> query,
            Expression<Func<TModel, TResult>> propertySelector,
            Compare compare, TResult value)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            query.InnerQuery.Has(propertySelector.Resolve(), compare, value);
            return query;
        }

        public static IEnumerable<IEdge<TModel>> Edges<TModel>(this IQuery<TModel> query) where TModel : class
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return query.InnerQuery.Edges().As<TModel>();
        }

        public static IEnumerable<IVertex<TModel>> Vertices<TModel>(this IQuery<TModel> query) where TModel : class
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return query.InnerQuery.Vertices().As<TModel>();
        }
    }
}
