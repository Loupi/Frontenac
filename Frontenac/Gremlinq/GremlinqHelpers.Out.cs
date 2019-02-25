using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        public static IEnumerable<IVertex> Out(this IVertex vertex, int branchFactor, params string[] labels)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (labels == null)
                throw new ArgumentNullException(nameof(labels));

            var finalLabels = labels.Length == 0 ? vertex.GetPropertyKeys() : labels;
            return finalLabels.SelectMany(t => vertex.Out(t).Take(branchFactor));
        }

        public static IEnumerable<IVertex> Out(this IEnumerable<IVertex> vertices, int branchFactor, params string[] labels)
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            if (labels == null)
                throw new ArgumentNullException(nameof(labels));

            return vertices.SelectMany(t => t.Out(branchFactor, labels)).Take(branchFactor);
        }

        public static IEnumerable<IVertex<TOutModel>> Out<TOutModel, TInModel>(
            this IVertex<TInModel> vertex,
            int branchFactor,
            Expression<Func<TInModel, TOutModel>> propertySelector) 
            where TOutModel : class
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertex.Out(branchFactor, propertySelector.Resolve()).As<TOutModel>();
        }

        public static IEnumerable<IVertex<TOutModel>> Out<TOutModel, TInModel>(
            this IEnumerable<IVertex<TInModel>> vertices,
            int branchFactor,
            Expression<Func<TInModel, TOutModel>> propertySelector) 
            where TOutModel : class
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertices.SelectMany(t => t.Out(branchFactor, propertySelector)).Take(branchFactor);
        }

        public static IEnumerable<IVertex<TOutModel>> Out<TOutModel, TInModel>(
            this IEnumerable<IVertex<TInModel>> vertices,
            int branchFactor,
            Expression<Func<IVertex<TInModel>, IVertex<TOutModel>>> propertySelector) 
            where TOutModel : class
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertices.Out(branchFactor, propertySelector.Resolve()).As<TOutModel>();
        }

        public static IEnumerable<IVertex> Out(this IVertex vertex, params string[] labels)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (labels == null)
                throw new ArgumentNullException(nameof(labels));

            return vertex.GetVertices(Direction.Out, labels);
        }

        public static IEnumerable<IVertex> Out(this IEnumerable<IVertex> vertices, params string[] labels)
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            if (labels == null)
                throw new ArgumentNullException(nameof(labels));

            return vertices.SelectMany(t => t.Out(labels));
        }

        public static IEnumerable<IVertex<TOutModel>> Out<TInModel, TOutModel>(
            this IVertex<TInModel> vertex,
            Expression<Func<TInModel, TOutModel>> propertySelector) 
            where TOutModel : class
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertex.Out(propertySelector.Resolve()).As<TOutModel>();
        }

        public static IEnumerable<IVertex<TOutModel>> Out<TInModel, TOutModel>(
            this IVertex<TInModel> vertex,
            Expression<Func<TInModel, IEnumerable<TOutModel>>> propertySelector) 
            where TOutModel : class
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertex.Out(propertySelector.Resolve()).As<TOutModel>();
        }

        public static IEnumerable<IVertex<TOutModel>> Out<TInModel, TOutModel, TEdgeModel>(
            this IVertex<TInModel> vertex,
            Expression<Func<TInModel, IEnumerable<KeyValuePair<TEdgeModel, TOutModel>>>> propertySelector)
            where TOutModel : class
            where TEdgeModel : class 
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertex.Out(propertySelector.Resolve()).As<TOutModel>();
        }

        public static IEnumerable<IVertex<TOutModel>> Out<TOutModel, TInModel, TEdgeModel>(
            this IVertex<TInModel> vertex,
            Expression<Func<KeyValuePair<TEdgeModel, TInModel>, TOutModel>> propertySelector)
            where TOutModel : class
            where TEdgeModel : class
            where TInModel : class  
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertex.Out(propertySelector.Resolve()).As<TOutModel>();
        }

        public static IEnumerable<IVertex<TOutModel>> Out<TOutModel, TInModel, TEdgeModel>(
            this IVertex<TInModel> vertex,
            Expression<Func<IEnumerable<KeyValuePair<TEdgeModel, TInModel>>, TOutModel>> propertySelector)
            where TOutModel : class
            where TEdgeModel : class 
            where TInModel : class 
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertex.Out(propertySelector.Resolve()).As<TOutModel>();
        }

        public static IEnumerable<IVertex<TOutModel>> Out<TOutModel, TInModel>(
            this IEnumerable<IVertex<TInModel>> vertices,
            Expression<Func<TInModel, TOutModel>> propertySelector) 
            where TOutModel : class
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertices.SelectMany(t => t.Out(propertySelector.Resolve())).As<TOutModel>();
        }

        public static IEnumerable<IVertex<TOutModel>> Out<TOutModel, TInModel>(
            this IEnumerable<IVertex<TInModel>> vertices,
            Expression<Func<IVertex<TInModel>, IVertex<TOutModel>>> propertySelector) 
            where TOutModel : class
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            if (propertySelector == null)
                throw new ArgumentNullException(nameof(propertySelector));

            return vertices.SelectMany(t => t.Out(propertySelector.Resolve())).As<TOutModel>();
        }

        public static IVertex<TOutModel> Out<TModel, TOutModel>(
            this IEdge<TModel> edge,
            Expression<Func<TModel, TOutModel>> edgePropertySelector) 
            where TOutModel : class
        {
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));
            if (edgePropertySelector == null)
                throw new ArgumentNullException(nameof(edgePropertySelector));

            var vertex = edge.GetVertex(Direction.Out);
            return new Vertex<TOutModel>(vertex, vertex.Proxy<TOutModel>());
        }

        public static IVertex<TOutModel> Out<TModel, TOutModel>(
            this IEdge<TModel> edge,
            Expression<Func<TModel, IEnumerable<TOutModel>>> edgePropertySelector)
            where TOutModel : class
        {
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));
            if (edgePropertySelector == null)
                throw new ArgumentNullException(nameof(edgePropertySelector));

            var vertex = edge.GetVertex(Direction.Out);
            return new Vertex<TOutModel>(vertex, vertex.Proxy<TOutModel>());
        }

        public static IEnumerable<IVertex<TOutModel>> Out<TInModel, TOutModel>(
            this IVertex<TInModel> vertex,
            params Expression<Func<TInModel, TOutModel>>[] edgePropertySelectors) 
            where TOutModel : class
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (edgePropertySelectors == null)
                throw new ArgumentNullException(nameof(edgePropertySelectors));

            return vertex.GetVertices(Direction.Out, edgePropertySelectors.Select(Resolve).ToArray()).As<TOutModel>();
        }

        public static IEnumerable<IVertex<TOutModel>> Out<TInModel, TOutModel>(
            this IEnumerable<IVertex<TInModel>> vertices,
            params Expression<Func<TInModel, TOutModel>>[] edgePropertySelectors)
            where TOutModel : class
        {
            if (vertices == null)
                throw new ArgumentNullException(nameof(vertices));
            if (edgePropertySelectors == null)
                throw new ArgumentNullException(nameof(edgePropertySelectors));

            return vertices.SelectMany(t => t.Out(edgePropertySelectors));
        }
    }
}
