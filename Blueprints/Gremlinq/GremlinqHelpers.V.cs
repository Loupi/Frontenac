using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        public static IEnumerable<IVertex> V(this IGraph graph)
        {
            Contract.Requires(graph != null);
            Contract.Ensures(Contract.Result<IEnumerable<IVertex>>() != null);

            return graph.GetVertices();
        }

        public static IEnumerable<IVertex<TModel>> V<TModel>(this IGraph graph)
            where TModel : class 
        {
            Contract.Requires(graph != null);
            Contract.Ensures(Contract.Result<IEnumerable<IVertex>>() != null);

            return graph.GetVertices().As<TModel>();
        }

        public static IVertex V(this IGraph graph, object id)
        {
            Contract.Requires(graph != null);
            Contract.Requires(id != null);
            Contract.Ensures(Contract.Result<IVertex>() != null);

            return graph.GetVertex(id);
        }

        public static IVertex<TModel> V<TModel>(this IGraph graph, object id) where TModel : class
        {
            Contract.Requires(graph != null);
            Contract.Requires(id != null);
            Contract.Ensures(Contract.Result<IVertex<TModel>>() != null);

            return graph.V(id).As<TModel>();
        }

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
