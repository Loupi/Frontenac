using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        public static IEnumerable<IVertex> V(this IGraph graph)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            return graph.GetVertices();
        }

        public static IEnumerable<IVertex<TModel>> V<TModel>(this IGraph graph)
            where TModel : class 
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            return graph.GetVertices().As<TModel>();
        }

        public static IVertex V(this IGraph graph, object id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(graph));
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            return graph.GetVertex(id);
        }

        public static IVertex<TModel> V<TModel>(this IGraph graph, object id) where TModel : class
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            return graph.V(id).As<TModel>();
        }

        public static IEnumerable<IVertex> V(this IGraph graph, string propertyName, object value)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException(nameof(propertyName));

            return graph.GetVertices(propertyName, value);
        }

        public static IEnumerable<IVertex<TModel>> V<TModel, TValue>(this IGraph graph,
            Expression<Func<TModel, TValue>> propertySelector,
            TValue value) where TModel : class
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return graph.V(propertySelector.Resolve(), value).As<TModel>();
        }
    }
}
