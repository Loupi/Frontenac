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
        public static IEnumerable<IEdge> OutE(this IVertex vertex, int branchFactor, params string[] labels)
        {
            Contract.Requires(vertex != null);
            Contract.Requires(labels != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge>>() != null);

            var finalLabels = labels.Length == 0 ? vertex.GetPropertyKeys() : labels;
            return finalLabels.SelectMany(t => vertex.OutE(t).Take(branchFactor)).Take(branchFactor);
        }

        public static IEnumerable<IEdge> OutE(this IEnumerable<IVertex> vertices, int branchFactor, params string[] labels)
        {
            Contract.Requires(vertices != null);
            Contract.Requires(labels != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge>>() != null);

            return vertices.SelectMany(t => t.OutE(branchFactor, labels)).Take(branchFactor);
        }

        public static IEnumerable<IEdge<TEdgeModel>> OutE<TEdgeModel, TInModel>(
            this IVertex<TInModel> vertex,
            int branchFactor,
            Expression<Func<TInModel, TEdgeModel>> propertySelector) 
            where TEdgeModel : class
        {
            Contract.Requires(vertex != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TEdgeModel>>>() != null);

            return vertex.OutE(branchFactor, propertySelector.Resolve()).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> OutE<TEdgeModel, TInModel>(
            this IEnumerable<IVertex<TInModel>> vertices,
            int branchFactor,
            Expression<Func<TInModel, TEdgeModel>> propertySelector) 
            where TEdgeModel : class
        {
            Contract.Requires(vertices != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TEdgeModel>>>() != null);

            return vertices.SelectMany(t => t.OutE(branchFactor, propertySelector)).Take(branchFactor);
        }

        public static IEnumerable<IEdge<TEdgeModel>> OutE<TEdgeModel, TInModel>(
            this IEnumerable<IVertex<TInModel>> vertices,
            int branchFactor,
            Expression<Func<IVertex<TInModel>, IEdge<TEdgeModel>>> propertySelector) 
            where TEdgeModel : class
        {
            Contract.Requires(vertices != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TEdgeModel>>>() != null);

            return vertices.OutE(branchFactor, propertySelector.Resolve()).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge> OutE(this IVertex vertex, params string[] labels)
        {
            Contract.Requires(vertex != null);
            Contract.Requires(labels != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge>>() != null);

            return vertex.GetEdges(Direction.Out, labels);
        }

        public static IEnumerable<IEdge> OutE(this IEnumerable<IVertex> vertices, params string[] labels)
        {
            Contract.Requires(vertices != null);
            Contract.Requires(labels != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge>>() != null);

            return vertices.SelectMany(t => t.OutE(labels));
        }

        public static IEnumerable<IEdge<TEdgeModel>> OutE<TInModel, TEdgeModel>(
            this IVertex<TInModel> vertex,
            Expression<Func<TInModel, TEdgeModel>> propertySelector)
            where TEdgeModel : class
        {
            Contract.Requires(vertex != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TEdgeModel>>>() != null);

            return vertex.OutE(propertySelector.Resolve()).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> OutE<TInModel, TEdgeModel>(
            this IVertex<TInModel> vertex,
            Expression<Func<TInModel, IEnumerable<TEdgeModel>>> propertySelector) 
            where TEdgeModel : class
        {
            Contract.Requires(vertex != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TEdgeModel>>>() != null);

            return vertex.OutE(propertySelector.Resolve()).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> OutE<TInModel, TOutModel, TEdgeModel>(
            this IVertex<TInModel> vertex,
            Expression<Func<TInModel, IEnumerable<KeyValuePair<TEdgeModel, TOutModel>>>> propertySelector)
            where TEdgeModel : class
            where TOutModel : class 
        {
            Contract.Requires(vertex != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TEdgeModel>>>() != null);

            return vertex.OutE(propertySelector.Resolve()).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> OutE<TInModel, TEdgeModel>(
            this IVertex<TInModel> vertex,
            Expression<Func<KeyValuePair<TEdgeModel, TInModel>, TEdgeModel>> propertySelector)
            where TEdgeModel : class
            where TInModel : class 
        {
            Contract.Requires(vertex != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TEdgeModel>>>() != null);

            return vertex.OutE(propertySelector.Resolve()).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> OutE<TEdgeModel, TInModel>(
            this IEnumerable<IVertex<TInModel>> vertices,
            Expression<Func<TInModel, TEdgeModel>> propertySelector) 
            where TEdgeModel : class
        {
            Contract.Requires(vertices != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TEdgeModel>>>() != null);

            return vertices.SelectMany(t => t.OutE(propertySelector.Resolve())).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> OutE<TEdgeModel, TInModel>(
            this IEnumerable<IVertex<TInModel>> vertices,
            Expression<Func<IVertex<TInModel>, IEdge<TEdgeModel>>> propertySelector) 
            where TEdgeModel : class
        {
            Contract.Requires(vertices != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TEdgeModel>>>() != null);

            return vertices.SelectMany(t => t.OutE(propertySelector.Resolve())).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> OutE<TInModel, TOutModel, TEdgeModel>(
            this IVertex<TInModel> vertex,
            params Expression<Func<TInModel, KeyValuePair<TEdgeModel, TOutModel>>>[] edgePropertySelectors)
            where TEdgeModel : class
            where TOutModel : class 
        {
            Contract.Requires(vertex != null);
            Contract.Requires(edgePropertySelectors != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TEdgeModel>>>() != null);

            return vertex.GetEdges(Direction.Out, edgePropertySelectors.Select(Resolve).ToArray()).As<TEdgeModel>();
        }

        public static IEnumerable<IEdge<TEdgeModel>> OutE<TInModel, TOutModel, TEdgeModel>(
            this IEnumerable<IVertex<TInModel>> vertices,
            params Expression<Func<TInModel, KeyValuePair<TEdgeModel, TOutModel>>>[] edgePropertySelectors)
            where TEdgeModel : class
            where TOutModel : class 
        {
            Contract.Requires(vertices != null);
            Contract.Requires(edgePropertySelectors != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TEdgeModel>>>() != null);

            return vertices.SelectMany(t => t.OutE(edgePropertySelectors));
        }
    }
}
