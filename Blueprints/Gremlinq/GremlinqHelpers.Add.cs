using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        private const string TypePropertyName = "__type__";

        public static IVertex<TModel> AddVertex<TModel>(this IGraph graph, Action<TModel> assignMembers) where TModel : class
        {
            Contract.Requires(graph != null);
            Contract.Requires(assignMembers != null);
            Contract.Ensures(Contract.Result<IVertex<TModel>>() != null);

            var vertex = graph.AddVertex(null);
            var typeName = typeof(TModel).AssemblyQualifiedName;
            vertex.Add(TypePropertyName, typeName);
            var wrapper = vertex.Wrap(assignMembers);
            return wrapper;
        }

        public static IEdge AddEdge<TOutModel, TInModel>(
            this IVertex<TOutModel> outVertex,
            Expression<Func<TOutModel, TInModel>> edgePropertySelector,
            IVertex<TInModel> inVertex)
        {
            Contract.Requires(outVertex != null);
            Contract.Requires(edgePropertySelector != null);
            Contract.Requires(inVertex != null);
            Contract.Ensures(Contract.Result<IEdge>() != null);

            return outVertex.AddEdge(edgePropertySelector.Resolve(), inVertex);
        }

        public static IEdge AddEdge<TOutModel, TInModel>(
            this IVertex<TOutModel> outVertex,
            Expression<Func<TOutModel, IEnumerable<TInModel>>> edgePropertySelector,
            IVertex<TInModel> inVertex)
        {
            Contract.Requires(outVertex != null);
            Contract.Requires(edgePropertySelector != null);
            Contract.Requires(inVertex != null);
            Contract.Ensures(Contract.Result<IEdge>() != null);

            return outVertex.AddEdge(edgePropertySelector.Resolve(), inVertex);
        }

        public static IEdge<TModel> AddEdge<TOutModel, TInModel, TModel>(
            this IVertex<TOutModel> outVertex,
            Expression<Func<TOutModel, KeyValuePair<TModel, TInModel>>> edgePropertySelector,
            IVertex<TInModel> inVertex) where TModel : class
        {
            Contract.Requires(outVertex != null);
            Contract.Requires(edgePropertySelector != null);
            Contract.Requires(inVertex != null);
            Contract.Ensures(Contract.Result<IEdge<TModel>>() != null);

            var edge = outVertex.AddEdge(edgePropertySelector.Resolve(), inVertex);
            return edge.As<TModel>();
        }

        public static IEdge<TModel> AddEdge<TOutModel, TInModel, TModel>(
            this IVertex<TOutModel> outVertex,
            Expression<Func<TOutModel, KeyValuePair<TModel, TInModel>>> edgePropertySelector,
            IVertex<TInModel> inVertex,
            Action<TModel> assignMembers) where TModel : class
        {
            Contract.Requires(outVertex != null);
            Contract.Requires(edgePropertySelector != null);
            Contract.Requires(inVertex != null);
            Contract.Requires(assignMembers != null);
            Contract.Ensures(Contract.Result<IEdge<TModel>>() != null);

            var edge = outVertex.AddEdge(edgePropertySelector.Resolve(), inVertex);
            return Wrap(edge, assignMembers);
        }

        public static IEdge<TModel> AddEdge<TOutModel, TInModel, TModel>(
            this IVertex<TOutModel> outVertex,
            Expression<Func<TOutModel, IEnumerable<KeyValuePair<TModel, TInModel>>>> edgePropertySelector,
            IVertex<TInModel> inVertex,
            Action<TModel> assignMembers) where TModel : class
        {
            Contract.Requires(outVertex != null);
            Contract.Requires(edgePropertySelector != null);
            Contract.Requires(inVertex != null);
            Contract.Requires(assignMembers != null);
            Contract.Ensures(Contract.Result<IEdge<TModel>>() != null);

            var edge = outVertex.AddEdge(edgePropertySelector.Resolve(), inVertex);
            return Wrap(edge, assignMembers);
        }
    }
}
