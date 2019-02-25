using System;
using System.Linq.Expressions;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Geo;

namespace Frontenac.Gremlinq.Test
{
    public static class GremlinqHelpers
    {
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
    }
}