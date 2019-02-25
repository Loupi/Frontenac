using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        public static IEnumerable<IEdge> E(this IGraph graph)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            return graph.GetEdges();
        }

        public static IEnumerable<IEdge<TModel>> E<TModel>(this IGraph graph)
            where TModel : class 
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            return graph.GetEdges().As<TModel>();
        }

        public static IEdge E(this IGraph graph, object id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(graph));
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            return graph.GetEdge(id);
        }

        public static IEdge<TModel> E<TModel>(this IGraph graph, object id) where TModel : class
        {
            if (id == null)
                throw new ArgumentNullException(nameof(graph));
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            return graph.E(id).As<TModel>();
        }

        public static IEnumerable<IEdge> E(this IGraph graph, string propertyName, object value)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException(nameof(propertyName));

            return graph.GetEdges(propertyName, value);
        }

        public static IEnumerable<IEdge<TModel>> E<TModel, TValue>(this IGraph graph,
            Expression<Func<TModel, TValue>> propertySelector,
            TValue value) where TModel : class
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return graph.E(propertySelector.Resolve(), value).As<TModel>();
        }
    }
}
