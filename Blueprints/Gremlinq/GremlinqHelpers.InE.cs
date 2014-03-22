using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        public static IEnumerable<IEdge> InE(this IVertex vertex, int branchFactor, params string[] labels)
        {
            Contract.Requires(vertex != null);
            Contract.Requires(labels != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge>>() != null);

            var finalLabels = labels.Length == 0 ? vertex.GetPropertyKeys() : labels;
            return finalLabels.SelectMany(t => vertex.InE(t).Take(branchFactor));
        }

        public static IEnumerable<IEdge> InE(this IEnumerable<IVertex> vertices, int branchFactor, params string[] labels)
        {
            Contract.Requires(vertices != null);
            Contract.Requires(labels != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge>>() != null);

            return vertices.SelectMany(t => t.InE(branchFactor, labels));
        }

        public static IEnumerable<IEdge<TEdgeModel>> InE<TEdgeModel, TOutModel>(
            this IVertex<TOutModel> vertex,
            int branchFactor,
            Expression<Func<TOutModel, TEdgeModel>> propertySelector)
            where TEdgeModel : class
        {
            Contract.Requires(vertex != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TEdgeModel>>>() != null);

            return vertex.InE(branchFactor, propertySelector.Resolve()).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> InE<TEdgeModel, TOutModel>(
            this IEnumerable<IVertex<TOutModel>> vertices,
            int branchFactor,
            Expression<Func<TOutModel, TEdgeModel>> propertySelector)
            where TEdgeModel : class
        {
            Contract.Requires(vertices != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TEdgeModel>>>() != null);

            return vertices.SelectMany(t => t.InE(branchFactor, propertySelector));
        }

        public static IEnumerable<IEdge<TEdgeModel>> InE<TEdgeModel, TOutModel>(
            this IEnumerable<IVertex<TOutModel>> vertices,
            int branchFactor,
            Expression<Func<IVertex<TOutModel>, IEdge<TEdgeModel>>> propertySelector)
            where TEdgeModel : class
        {
            Contract.Requires(vertices != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TEdgeModel>>>() != null);

            return vertices.InE(branchFactor, propertySelector.Resolve()).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge> InE(this IVertex vertex, params string[] labels)
        {
            Contract.Requires(vertex != null);
            Contract.Requires(labels != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge>>() != null);

            return vertex.GetEdges(Direction.In, labels);
        }

        public static IEnumerable<IEdge> InE(this IEnumerable<IVertex> vertices, params string[] labels)
        {
            Contract.Requires(vertices != null);
            Contract.Requires(labels != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge>>() != null);

            return vertices.SelectMany(t => t.InE(labels));
        }

        public static IEnumerable<IEdge<TEdgeModel>> InE<TOutModel, TEdgeModel>(
            this IVertex<TOutModel> vertex,
            Expression<Func<TOutModel, TEdgeModel>> propertySelector)
            where TEdgeModel : class
        {
            Contract.Requires(vertex != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TEdgeModel>>>() != null);

            return vertex.InE(propertySelector.Resolve()).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> InE<TOutModel, TEdgeModel>(
            this IVertex<TOutModel> vertex,
            Expression<Func<TOutModel, IEnumerable<TEdgeModel>>> propertySelector)
            where TEdgeModel : class
        {
            Contract.Requires(vertex != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TEdgeModel>>>() != null);

            return vertex.InE(propertySelector.Resolve()).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> InE<TInModel, TOutModel, TEdgeModel>(
            this IVertex<TOutModel> vertex,
            Expression<Func<TOutModel, IEnumerable<KeyValuePair<TEdgeModel, TInModel>>>> propertySelector)
            where TEdgeModel : class
        {
            Contract.Requires(vertex != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TEdgeModel>>>() != null);

            return vertex.InE(propertySelector.Resolve()).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> InE<TOutModel, TEdgeModel>(
            this IVertex<TOutModel> vertex,
            Expression<Func<KeyValuePair<TEdgeModel, TOutModel>, TEdgeModel>> propertySelector)
            where TEdgeModel : class
        {
            Contract.Requires(vertex != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TEdgeModel>>>() != null);

            return vertex.InE(propertySelector.Resolve()).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> InE<TEdgeModel, TOutModel>(
            this IEnumerable<IVertex<TOutModel>> vertices,
            Expression<Func<TOutModel, TEdgeModel>> propertySelector)
            where TEdgeModel : class
        {
            Contract.Requires(vertices != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TEdgeModel>>>() != null);

            return vertices.SelectMany(t => t.InE(propertySelector.Resolve())).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> InE<TEdgeModel, TOutModel>(
            this IEnumerable<IVertex<TOutModel>> vertices,
            Expression<Func<IVertex<TOutModel>, IEdge<TEdgeModel>>> propertySelector)
            where TEdgeModel : class
        {
            Contract.Requires(vertices != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TEdgeModel>>>() != null);

            return vertices.SelectMany(t => t.InE(propertySelector.Resolve())).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> InE<TOutModel, TInModel, TEdgeModel>(
            this IVertex<TOutModel> vertex,
            params Expression<Func<TOutModel, KeyValuePair<TEdgeModel, TInModel>>>[] edgePropertySelectors)
            where TEdgeModel : class
        {
            Contract.Requires(vertex != null);
            Contract.Requires(edgePropertySelectors != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TEdgeModel>>>() != null);

            return vertex.GetEdges(Direction.In, edgePropertySelectors.Select(Resolve).ToArray()).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> InE<TOutModel, TInModel, TEdgeModel>(
            this IEnumerable<IVertex<TOutModel>> vertices,
            params Expression<Func<TOutModel, KeyValuePair<TEdgeModel, TInModel>>>[] edgePropertySelectors)
            where TEdgeModel : class
        {
            Contract.Requires(vertices != null);
            Contract.Requires(edgePropertySelectors != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TEdgeModel>>>() != null);

            return vertices.SelectMany(t => t.InE(edgePropertySelectors));
        }
    }
}
