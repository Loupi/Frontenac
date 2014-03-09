using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        public static void CreateIndex<TModel, TIndexType>(
            this IKeyIndexableGraph graph,
            Expression<Func<TModel, TIndexType>> propertySelector,
            Type indexType)
        {
            Contract.Requires(graph != null);
            Contract.Requires(propertySelector != null);
            Contract.Requires(indexType != null);

            var name = propertySelector.Resolve();
            if (!graph.GetIndexedKeys(indexType).Contains(name))
                graph.CreateKeyIndex(name, indexType);
        }

        public static void CreateVertexIndex<TModel, TResult>(
            this IKeyIndexableGraph graph,
            Expression<Func<TModel, TResult>> propertySelector)
        {
            Contract.Requires(graph != null);
            Contract.Requires(propertySelector != null);

            graph.CreateIndex(propertySelector, typeof(IVertex));
        }

        public static void CreateEdgeIndex<TModel, TResult>(
            this IKeyIndexableGraph graph,
            Expression<Func<TModel, TResult>> propertySelector)
        {
            Contract.Requires(graph != null);
            Contract.Requires(propertySelector != null);

            graph.CreateIndex(propertySelector, typeof(IEdge));
        }
    }
}
