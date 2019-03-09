using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        public static IEnumerable<IEdge> OutE(this IVertex vertex, int branchFactor, params string[] labels)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (labels == null)
                throw new ArgumentNullException(nameof(labels));

            var finalLabels = labels.Length == 0 ? vertex.GetPropertyKeys() : labels;
            return finalLabels.SelectMany(t => vertex.OutE(t).Take(branchFactor)).Take(branchFactor);
        }

        public static IEnumerable<IEdge> OutE(this IEnumerable<IVertex> vertices, int branchFactor, params string[] labels)
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            if (labels == null)
                throw new ArgumentNullException(nameof(labels));

            return vertices.SelectMany(t => t.OutE(branchFactor, labels)).Take(branchFactor);
        }

        public static IEnumerable<IEdge<TEdgeModel>> OutE<TEdgeModel, TInModel>(
            this IVertex<TInModel> vertex,
            int branchFactor,
            Expression<Func<TInModel, TEdgeModel>> propertySelector) 
            where TEdgeModel : class
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertex.OutE(branchFactor, propertySelector.Resolve()).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> OutE<TEdgeModel, TInModel>(
            this IEnumerable<IVertex<TInModel>> vertices,
            int branchFactor,
            Expression<Func<TInModel, TEdgeModel>> propertySelector) 
            where TEdgeModel : class
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertices.SelectMany(t => t.OutE(branchFactor, propertySelector)).Take(branchFactor);
        }

        public static IEnumerable<IEdge<TEdgeModel>> OutE<TEdgeModel, TInModel>(
            this IEnumerable<IVertex<TInModel>> vertices,
            int branchFactor,
            Expression<Func<IVertex<TInModel>, IEdge<TEdgeModel>>> propertySelector) 
            where TEdgeModel : class
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertices.OutE(branchFactor, propertySelector.Resolve()).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge> OutE(this IVertex vertex, params string[] labels)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (labels == null)
                throw new ArgumentNullException(nameof(labels));

            return vertex.GetEdges(Direction.Out, labels);
        }

        public static IEnumerable<IEdge> OutE(this IEnumerable<IVertex> vertices, params string[] labels)
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            if (labels == null)
                throw new ArgumentNullException(nameof(labels));

            return vertices.SelectMany(t => t.OutE(labels));
        }

        public static IEnumerable<IEdge<TEdgeModel>> OutE<TInModel, TEdgeModel>(
            this IVertex<TInModel> vertex,
            Expression<Func<TInModel, TEdgeModel>> propertySelector)
            where TEdgeModel : class
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertex.OutE(propertySelector.Resolve()).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> OutE<TInModel, TEdgeModel>(
            this IVertex<TInModel> vertex,
            Expression<Func<TInModel, IEnumerable<TEdgeModel>>> propertySelector) 
            where TEdgeModel : class
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertex.OutE(propertySelector.Resolve()).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> OutE<TInModel, TOutModel, TEdgeModel>(
            this IVertex<TInModel> vertex,
            Expression<Func<TInModel, IEnumerable<KeyValuePair<TEdgeModel, TOutModel>>>> propertySelector)
            where TEdgeModel : class
            where TOutModel : class 
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertex.OutE(propertySelector.Resolve()).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> OutE<TInModel, TEdgeModel>(
            this IVertex<TInModel> vertex,
            Expression<Func<KeyValuePair<TEdgeModel, TInModel>, TEdgeModel>> propertySelector)
            where TEdgeModel : class
            where TInModel : class 
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertex.OutE(propertySelector.Resolve()).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> OutE<TEdgeModel, TInModel>(
            this IEnumerable<IVertex<TInModel>> vertices,
            Expression<Func<TInModel, TEdgeModel>> propertySelector) 
            where TEdgeModel : class
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertices.SelectMany(t => t.OutE(propertySelector.Resolve())).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> OutE<TEdgeModel, TInModel>(
            this IEnumerable<IVertex<TInModel>> vertices,
            Expression<Func<IVertex<TInModel>, IEdge<TEdgeModel>>> propertySelector) 
            where TEdgeModel : class
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertices.SelectMany(t => t.OutE(propertySelector.Resolve())).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> OutE<TInModel, TOutModel, TEdgeModel>(
            this IVertex<TInModel> vertex,
            params Expression<Func<TInModel, KeyValuePair<TEdgeModel, TOutModel>>>[] edgePropertySelectors)
            where TEdgeModel : class
            where TOutModel : class 
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (edgePropertySelectors == null)
                throw new ArgumentNullException(nameof(edgePropertySelectors));

            return vertex.GetEdges(Direction.Out, edgePropertySelectors.Select(Resolve).ToArray()).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> OutE<TInModel, TOutModel, TEdgeModel>(
            this IEnumerable<IVertex<TInModel>> vertices,
            params Expression<Func<TInModel, KeyValuePair<TEdgeModel, TOutModel>>>[] edgePropertySelectors)
            where TEdgeModel : class
            where TOutModel : class 
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            if (edgePropertySelectors == null)
                throw new ArgumentNullException(nameof(edgePropertySelectors));

            return vertices.SelectMany(t => t.OutE(edgePropertySelectors));
        }
    }
}
