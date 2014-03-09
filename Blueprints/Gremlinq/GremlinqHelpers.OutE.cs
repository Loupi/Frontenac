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
            return finalLabels.SelectMany(t => vertex.OutE(t).Take(branchFactor));
        }

        public static IEnumerable<IEdge> OutE(this IEnumerable<IVertex> vertices, int branchFactor, params string[] labels)
        {
            Contract.Requires(vertices != null);
            Contract.Requires(labels != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge>>() != null);

            return vertices.SelectMany(t => t.OutE(branchFactor, labels));
        }

        public static IEnumerable<IEdge<TInModel>> OutE<TOutModel, TInModel>(
            this IVertex<TOutModel> vertex,
            int branchFactor,
            Expression<Func<TOutModel, TInModel>> propertySelector) where TInModel : class
        {
            Contract.Requires(vertex != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TInModel>>>() != null);

            return vertex.OutE(branchFactor, propertySelector.Resolve()).As<TInModel>();
        }

        public static IEnumerable<IEdge<TInModel>> OutE<TOutModel, TInModel>(
            this IEnumerable<IVertex<TOutModel>> vertices,
            int branchFactor,
            Expression<Func<TOutModel, TInModel>> propertySelector) where TInModel : class
        {
            Contract.Requires(vertices != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TInModel>>>() != null);

            return vertices.SelectMany(t => t.OutE(branchFactor, propertySelector)).As<TInModel>();
        }

        public static IEnumerable<IEdge<TInModel>> OutE<TOutModel, TInModel>(
            this IEnumerable<IVertex<TOutModel>> vertices,
            int branchFactor,
            Expression<Func<IVertex<TOutModel>, IVertex<TInModel>>> propertySelector) where TInModel : class
        {
            Contract.Requires(vertices != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TInModel>>>() != null);

            return vertices.OutE(branchFactor, propertySelector.Resolve()).As<TInModel>();
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

        public static IEnumerable<IEdge<TInModel>> OutE<TOutModel, TInModel>(
            this IVertex<TOutModel> vertex,
            Expression<Func<TOutModel, TInModel>> propertySelector) where TInModel : class
        {
            Contract.Requires(vertex != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TInModel>>>() != null);

            return vertex.OutE(propertySelector.Resolve()).As<TInModel>();
        }

        public static IEnumerable<IEdge<TInModel>> OutE<TOutModel, TInModel>(
            this IEnumerable<IVertex<TOutModel>> vertices,
            Expression<Func<TOutModel, TInModel>> propertySelector) where TInModel : class
        {
            Contract.Requires(vertices != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TInModel>>>() != null);

            return vertices.SelectMany(t => t.OutE(propertySelector.Resolve())).As<TInModel>();
        }

        public static IEnumerable<IEdge<TInModel>> OutE<TOutModel, TInModel>(
            this IEnumerable<IVertex<TOutModel>> vertices,
            Expression<Func<IVertex<TOutModel>, IVertex<TInModel>>> propertySelector) where TInModel : class
        {
            Contract.Requires(vertices != null);
            Contract.Requires(propertySelector != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TInModel>>>() != null);

            return vertices.SelectMany(t => t.OutE(propertySelector.Resolve())).As<TInModel>();
        }

        public static IEnumerable<IEdge<TInModel>> OutE<TModel, TInModel>(
            this IVertex<TModel> vertex,
            params Expression<Func<TModel, TInModel>>[] edgePropertySelectors) where TInModel : class
        {
            Contract.Requires(vertex != null);
            Contract.Requires(edgePropertySelectors != null);
            Contract.Ensures(Contract.Result<IEnumerable<IEdge<TInModel>>>() != null);

            return vertex
                .GetEdges(Direction.Out, edgePropertySelectors.Select(Resolve).ToArray())
                .As<TInModel>();
        }
    }
}
