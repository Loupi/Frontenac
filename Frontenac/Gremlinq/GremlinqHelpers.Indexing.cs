using System;
using System.Linq;
using System.Linq.Expressions;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Geo;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        public static void CreateIndex<TModel, TIndex>(
            this IKeyIndexableGraph graph,
            Expression<Func<TModel, TIndex>> propertySelector,
            Type indexType)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));
            if (indexType == null)
                throw new ArgumentNullException(nameof(indexType));

            var name = propertySelector.Resolve();
            Parameter[] parameters = null;
            if (typeof(TIndex) == typeof(GeoPoint))
                parameters = new Parameter[]{new Parameter<string,GeoPoint>("GeoPoint", null)};
            
            if (!graph.GetIndexedKeys(indexType).Contains(name))
                graph.CreateKeyIndex(name, indexType, parameters);
        }

        public static void CreateVertexIndex<TModel, TResult>(
            this IKeyIndexableGraph graph,
            Expression<Func<TModel, TResult>> propertySelector)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            graph.CreateIndex(propertySelector, typeof(IVertex));
        }

        public static void CreateEdgeIndex<TModel, TResult>(
            this IKeyIndexableGraph graph,
            Expression<Func<TModel, TResult>> propertySelector)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            graph.CreateIndex(propertySelector, typeof(IEdge));
        }
    }
}
