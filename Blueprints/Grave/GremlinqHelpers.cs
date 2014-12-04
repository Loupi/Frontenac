using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using Frontenac.Blueprints;
using Frontenac.Gremlinq;
using Frontenac.Infrastructure.Geo;

namespace Frontenac.Grave
{
    public static class GremlinqHelpers
    {
        public static IQuery<TModel> Has<TModel>(
            this IQuery<TModel> query, 
            Expression<Func<TModel, GeoPoint>> propertySelector,
            IGeoShape value)
        {
            Contract.Requires(query != null);
            Contract.Requires(propertySelector != null);
            Contract.Requires(value != null);

            query.InnerQuery.Has(propertySelector.Resolve(), Compare.Equal, value);
            return query;
        }
    }
}
