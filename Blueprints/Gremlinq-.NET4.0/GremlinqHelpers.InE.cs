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

        public static IEnumerable<IEdge<TInModel>> InE<TOutModel, TInModel>(
            this IVertex<TOutModel> vertex,
            int branchFactor,
            Expression<Func<TOutModel, TInModel>> propertySelector) where TInModel : class
        {
            Contract.Requires(vertex != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TInModel>>>() != null);

            return vertex.InE(branchFactor, propertySelector.Resolve()).As<TInModel>();
        }

        public static IEnumerable<IEdge<TInModel>> InE<TOutModel, TInModel>(
            this IEnumerable<IVertex<TOutModel>> vertices,
            int branchFactor,
            Expression<Func<TOutModel, TInModel>> propertySelector) where TInModel : class
        {
            Contract.Requires(vertices != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TInModel>>>() != null);

            return vertices.SelectMany(t => t.InE(branchFactor, propertySelector)).As<TInModel>();
        }

        public static IEnumerable<IEdge<TInModel>> InE<TOutModel, TInModel>(
            this IEnumerable<IVertex<TOutModel>> vertices,
            int branchFactor,
            Expression<Func<IVertex<TOutModel>, IVertex<TInModel>>> propertySelector) where TInModel : class
        {
            Contract.Requires(vertices != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TInModel>>>() != null);

            return vertices.InE(branchFactor, propertySelector.Resolve()).As<TInModel>();
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

        public static IEnumerable<IEdge<TInModel>> InE<TOutModel, TInModel>(
            this IVertex<TOutModel> vertex,
            Expression<Func<TOutModel, TInModel>> propertySelector) where TInModel : class
        {
            Contract.Requires(vertex != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TInModel>>>() != null);

            return vertex.InE(propertySelector.Resolve()).As<TInModel>();
        }

        public static IEnumerable<IEdge<TInModel>> InE<TOutModel, TInModel>(
            this IEnumerable<IVertex<TOutModel>> vertices,
            Expression<Func<TOutModel, TInModel>> propertySelector) where TInModel : class
        {
            Contract.Requires(vertices != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TInModel>>>() != null);

            return vertices.SelectMany(t => t.InE(propertySelector.Resolve())).As<TInModel>();
        }

        public static IEnumerable<IEdge<TInModel>> InE<TOutModel, TInModel>(
            this IEnumerable<IVertex<TOutModel>> vertices,
            Expression<Func<IVertex<TOutModel>, IVertex<TInModel>>> propertySelector) where TInModel : class
        {
            Contract.Requires(vertices != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TInModel>>>() != null);

            return vertices.SelectMany(t => t.InE(propertySelector.Resolve())).As<TInModel>();
        }

        public static IEnumerable<IEdge<TInModel>> InE<TModel, TInModel>(
            this IVertex<TModel> vertex,
            params Expression<Func<TModel, TInModel>>[] edgePropertySelectors) where TInModel : class
        {
            Contract.Requires(vertex != null);
            Contract.Requires(edgePropertySelectors != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TInModel>>>() != null);

            return vertex
                .GetEdges(Direction.In, edgePropertySelectors.Select(Resolve).ToArray())
                .As<TInModel>();
        }
    }
}
