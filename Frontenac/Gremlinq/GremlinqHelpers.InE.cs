using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        public static IEnumerable<IEdge> InE(this IVertex vertex, int branchFactor, params string[] labels)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (labels == null)
                throw new ArgumentNullException(nameof(labels));

            var finalLabels = labels.Length == 0 ? vertex.GetPropertyKeys() : labels;
            return finalLabels.SelectMany(t => vertex.InE(t).Take(branchFactor));
        }

        public static IEnumerable<IEdge> InE(this IEnumerable<IVertex> vertices, int branchFactor, params string[] labels)
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            if (labels == null)
                throw new ArgumentNullException(nameof(labels));

            return vertices.SelectMany(t => t.InE(branchFactor, labels)).Take(branchFactor);
        }

        public static IEnumerable<IEdge<TEdgeModel>> InE<TEdgeModel, TOutModel>(
            this IVertex<TOutModel> vertex,
            int branchFactor,
            Expression<Func<TOutModel, TEdgeModel>> propertySelector)
            where TEdgeModel : class
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertex.InE(branchFactor, propertySelector.Resolve()).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> InE<TEdgeModel, TOutModel>(
            this IEnumerable<IVertex<TOutModel>> vertices,
            int branchFactor,
            Expression<Func<TOutModel, TEdgeModel>> propertySelector)
            where TEdgeModel : class
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertices.SelectMany(t => t.InE(branchFactor, propertySelector)).Take(branchFactor);
        }

        public static IEnumerable<IEdge<TEdgeModel>> InE<TEdgeModel, TOutModel>(
            this IEnumerable<IVertex<TOutModel>> vertices,
            int branchFactor,
            Expression<Func<IVertex<TOutModel>, IEdge<TEdgeModel>>> propertySelector)
            where TEdgeModel : class
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertices.InE(branchFactor, propertySelector.Resolve()).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge> InE(this IVertex vertex, params string[] labels)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (labels == null)
                throw new ArgumentNullException(nameof(labels));

            return vertex.GetEdges(Direction.In, labels);
        }

        public static IEnumerable<IEdge> InE(this IEnumerable<IVertex> vertices, params string[] labels)
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            if (labels == null)
                throw new ArgumentNullException(nameof(labels));

            return vertices.SelectMany(t => t.InE(labels));
        }

        public static IEnumerable<IEdge<TEdgeModel>> InE<TOutModel, TEdgeModel>(
            this IVertex<TOutModel> vertex,
            Expression<Func<TOutModel, TEdgeModel>> propertySelector)
            where TEdgeModel : class
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertex.InE(propertySelector.Resolve()).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> InE<TOutModel, TEdgeModel>(
            this IVertex<TOutModel> vertex,
            Expression<Func<TOutModel, IEnumerable<TEdgeModel>>> propertySelector)
            where TEdgeModel : class
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertex.InE(propertySelector.Resolve()).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> InE<TInModel, TOutModel, TEdgeModel>(
            this IVertex<TOutModel> vertex,
            Expression<Func<TOutModel, IEnumerable<KeyValuePair<TEdgeModel, TInModel>>>> propertySelector)
            where TEdgeModel : class
            where TInModel : class 
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertex.InE(propertySelector.Resolve()).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> InE<TOutModel, TEdgeModel>(
            this IVertex<TOutModel> vertex,
            Expression<Func<KeyValuePair<TEdgeModel, TOutModel>, TEdgeModel>> propertySelector)
            where TEdgeModel : class
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertex.InE(propertySelector.Resolve()).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> InE<TEdgeModel, TOutModel>(
            this IEnumerable<IVertex<TOutModel>> vertices,
            Expression<Func<TOutModel, TEdgeModel>> propertySelector)
            where TEdgeModel : class
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertices.SelectMany(t => t.InE(propertySelector.Resolve())).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> InE<TEdgeModel, TOutModel>(
            this IEnumerable<IVertex<TOutModel>> vertices,
            Expression<Func<IVertex<TOutModel>, IEdge<TEdgeModel>>> propertySelector)
            where TEdgeModel : class
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertices.SelectMany(t => t.InE(propertySelector.Resolve())).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> InE<TOutModel, TInModel, TEdgeModel>(
            this IVertex<TOutModel> vertex,
            params Expression<Func<TOutModel, KeyValuePair<TEdgeModel, TInModel>>>[] edgePropertySelectors)
            where TEdgeModel : class
            where TInModel : class 
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (edgePropertySelectors == null)
                throw new ArgumentNullException(nameof(edgePropertySelectors));

            return vertex.GetEdges(Direction.In, edgePropertySelectors.Select(Resolve).ToArray()).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> InE<TOutModel, TInModel, TEdgeModel>(
            this IEnumerable<IVertex<TOutModel>> vertices,
            params Expression<Func<TOutModel, KeyValuePair<TEdgeModel, TInModel>>>[] edgePropertySelectors)
            where TEdgeModel : class
            where TInModel : class 
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            if (edgePropertySelectors == null)
                throw new ArgumentNullException(nameof(edgePropertySelectors));

            return vertices.SelectMany(t => t.InE(edgePropertySelectors));
        }
    }
}
