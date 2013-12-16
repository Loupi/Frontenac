using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using Frontenac.Blueprints;
using Frontenac.Grave.Geo;
using Frontenac.Gremlinq;

namespace Frontenac.Grave
{
    public static class GremlinqHelpers
    {
        public static IQueryWrapper<TModel> Has<TModel>(this IQueryWrapper<TModel> query, 
                                                        Expression<Func<TModel, GeoPoint>> propertySelector, 
                                                        Compare compare, 
                                                        IGeoShape value)
        {
            Contract.Requires(query != null);
            Contract.Requires(propertySelector != null);
            Contract.Requires(value != null);

            query.InnerQuery.Has(propertySelector.Resolve(), compare, value);
            return query;
        }
    }
}
