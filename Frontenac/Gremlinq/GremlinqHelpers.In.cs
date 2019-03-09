using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        public static IEnumerable<IVertex> In(this IVertex vertex, int branchFactor, params string[] labels)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (labels == null)
                throw new ArgumentNullException(nameof(labels));

            var finalLabels = labels.Length == 0 ? vertex.GetPropertyKeys() : labels;
            return finalLabels.SelectMany(t => vertex.In(t).Take(branchFactor)).Take(branchFactor);
        }

        public static IEnumerable<IVertex> In(this IEnumerable<IVertex> vertices, int branchFactor, params string[] labels)
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            if (labels == null)
                throw new ArgumentNullException(nameof(labels));

            return vertices.SelectMany(t => t.In(branchFactor, labels)).Take(branchFactor);
        }

        public static IEnumerable<IVertex<TInModel>> In<TOutModel, TInModel>(
            this IVertex<TOutModel> vertex,
            int branchFactor,
            Expression<Func<TOutModel, TInModel>> propertySelector) 
            where TInModel : class
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertex.In(branchFactor, propertySelector.Resolve()).As<TInModel>();
        }

        public static IEnumerable<IVertex<TInModel>> In<TOutModel, TInModel>(
            this IEnumerable<IVertex<TOutModel>> vertices,
            int branchFactor,
            Expression<Func<TOutModel, TInModel>> propertySelector) 
            where TInModel : class
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertices.SelectMany(t => t.In(branchFactor, propertySelector)).Take(branchFactor);
        }

        public static IEnumerable<IVertex<TInModel>> In<TOutModel, TInModel>(
            this IEnumerable<IVertex<TOutModel>> vertices,
            int branchFactor,
            Expression<Func<IVertex<TOutModel>, IVertex<TInModel>>> propertySelector) 
            where TInModel : class
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertices.In(branchFactor, propertySelector.Resolve()).As<TInModel>();
        }

        public static IEnumerable<IVertex> In(this IVertex vertex, params string[] labels)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (labels == null)
                throw new ArgumentNullException(nameof(labels));

            return vertex.GetVertices(Direction.In, labels);
        }

        public static IEnumerable<IVertex> In(this IEnumerable<IVertex> vertices, params string[] labels)
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            if (labels == null)
                throw new ArgumentNullException(nameof(labels));

            return vertices.SelectMany(t => t.In(labels));
        }

        public static IEnumerable<IVertex<TInModel>> In<TOutModel, TInModel>(
            this IVertex<TOutModel> vertex,
            Expression<Func<TOutModel, TInModel>> propertySelector) 
            where TInModel : class
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertex.In(propertySelector.Resolve()).As<TInModel>();
        }

        public static IEnumerable<IVertex<TInModel>> In<TOutModel, TInModel>(
            this IVertex<TOutModel> vertex,
            Expression<Func<TOutModel, ICollection<TInModel>>> propertySelector) 
            where TInModel : class
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertex.In(propertySelector.Resolve()).As<TInModel>();
        }

        public static IEnumerable<IVertex<TInModel>> In<TInModel, TOutModel, TEdgeModel>(
            this IVertex<TOutModel> vertex,
            Expression<Func<TOutModel, ICollection<KeyValuePair<TEdgeModel, TInModel>>>> propertySelector)
            where TInModel : class
            where TEdgeModel : class 
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertex.In(propertySelector.Resolve()).As<TInModel>();
        }

        public static IEnumerable<IVertex<TInModel>> In<TInModel, TOutModel, TEdgeModel>(
            this IVertex<TOutModel> vertex,
            Expression<Func<KeyValuePair<TEdgeModel, TOutModel>, TInModel>> propertySelector)
            where TInModel : class
            where TEdgeModel : class
            where TOutModel : class  
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertex.In(propertySelector.Resolve()).As<TInModel>();
        }

        public static IEnumerable<IVertex<TInModel>> In<TInModel, TOutModel, TEdgeModel>(
            this IVertex<TOutModel> vertex,
            Expression<Func<IEnumerable<KeyValuePair<TEdgeModel, TOutModel>>, TInModel>> propertySelector)
            where TInModel : class
            where TEdgeModel : class
            where TOutModel : class  
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertex.In(propertySelector.Resolve()).As<TInModel>();
        }

        public static IEnumerable<IVertex<TInModel>> In<TInModel, TOutModel>(
            this IEnumerable<IVertex<TOutModel>> vertices,
            Expression<Func<TOutModel, TInModel>> propertySelector) 
            where TInModel : class
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertices.SelectMany(t => t.In(propertySelector.Resolve())).As<TInModel>();
        }

        public static IEnumerable<IVertex<TInModel>> In<TInModel, TOutModel>(
            this IEnumerable<IVertex<TOutModel>> vertices,
            Expression<Func<IVertex<TOutModel>, IVertex<TInModel>>> propertySelector) 
            where TInModel : class
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertices.SelectMany(t => t.In(propertySelector.Resolve())).As<TInModel>();
        }

        public static IVertex<TInModel> In<TModel, TInModel>(
            this IEdge<TModel> edge,
            Expression<Func<TModel, TInModel>> edgePropertySelector) 
            where TInModel : class
        {
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));
            if (edgePropertySelector == null)
                throw new ArgumentNullException(nameof(edgePropertySelector));

            var vertex = edge.GetVertex(Direction.In);
            return new Vertex<TInModel>(vertex, vertex.Proxy<TInModel>());
        }

        public static IVertex<TInModel> In<TModel, TInModel>(
            this IEdge<TModel> edge,
            Expression<Func<TModel, IEnumerable<TInModel>>> edgePropertySelector)
            where TInModel : class
        {
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));
            if (edgePropertySelector == null)
                throw new ArgumentNullException(nameof(edgePropertySelector));

            var vertex = edge.GetVertex(Direction.In);
            return new Vertex<TInModel>(vertex, vertex.Proxy<TInModel>());
        }

        public static IEnumerable<IVertex<TInModel>> In<TModel, TInModel>(
            this IVertex<TModel> vertex,
            params Expression<Func<TModel, TInModel>>[] edgePropertySelectors) 
            where TInModel : class
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (edgePropertySelectors == null)
                throw new ArgumentNullException(nameof(edgePropertySelectors));

            return vertex.GetVertices(Direction.In, edgePropertySelectors.Select(Resolve).ToArray()).As<TInModel>();
        }

        public static IEnumerable<IVertex<TInModel>> In<TOutModel, TInModel>(
            this IEnumerable<IVertex<TOutModel>> vertices,
            params Expression<Func<TOutModel, TInModel>>[] edgePropertySelectors)
            where TInModel : class
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            if (edgePropertySelectors == null)
                throw new ArgumentNullException(nameof(edgePropertySelectors));

            return vertices.SelectMany(t => t.In(edgePropertySelectors));
        }

        public static IEnumerable<IVertex<TInModel>> In<TInModel, TOutModel>(
                this IEnumerable<IVertex<TOutModel>> vertices,
                Expression<Func<TOutModel, ICollection<TInModel>>> propertySelector)
                where TInModel : class
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));

            return vertices.SelectMany(t => t.In(propertySelector.Resolve())).As<TInModel>();
        }
    }
}
