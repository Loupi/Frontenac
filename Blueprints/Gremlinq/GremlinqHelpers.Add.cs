﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        public static IVertex<TModel> AddVertex<TModel>(this IGraph graph) where TModel : class
        {
            Contract.Requires(graph != null);
            Contract.Ensures(Contract.Result<IVertex<TModel>>() != null);

            var vertex = graph.AddVertex(null);
            GremlinqContext.Current.TypeProvider.SetType(vertex, typeof(TModel));
            return vertex.As<TModel>();
        }

        public static IVertex<TModel> AddVertex<TModel>(this IGraph graph, Action<TModel> assignMembers) where TModel : class
        {
            Contract.Requires(graph != null);
            Contract.Requires(assignMembers != null);
            Contract.Ensures(Contract.Result<IVertex<TModel>>() != null);

            var vertex = graph.AddVertex(null);
            GremlinqContext.Current.TypeProvider.SetType(vertex, typeof(TModel));
            return vertex.Wrap(assignMembers);
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
            GremlinqContext.Current.TypeProvider.SetType(edge, typeof(TModel));
            return edge.As<TModel>();
        }

        public static IEdge<TModel> AddEdge<TOutModel, TInModel, TModel>(
            this IVertex<TOutModel> outVertex,
            Expression<Func<TOutModel, IEnumerable<KeyValuePair<TModel, TInModel>>>> edgePropertySelector,
            IVertex<TInModel> inVertex) where TModel : class
        {
            Contract.Requires(outVertex != null);
            Contract.Requires(edgePropertySelector != null);
            Contract.Requires(inVertex != null);
            Contract.Ensures(Contract.Result<IEdge<TModel>>() != null);

            var edge = outVertex.AddEdge(edgePropertySelector.Resolve(), inVertex);
            GremlinqContext.Current.TypeProvider.SetType(edge, typeof(TModel));
            return edge.As<TModel>();
        }

        public static IEdge<TModel> AddEdge<TOutModel, TInModel, TModel>(
            this IVertex<TOutModel> outVertex,
            Expression<Func<TOutModel, KeyValuePair<TModel, TInModel>>> edgePropertySelector,
            IVertex<TInModel> inVertex,
            Action<TModel> assignMembers) 
            where TModel : class
        {
            Contract.Requires(outVertex != null);
            Contract.Requires(edgePropertySelector != null);
            Contract.Requires(inVertex != null);
            Contract.Requires(assignMembers != null);
            Contract.Ensures(Contract.Result<IEdge<TModel>>() != null);

            var edge = outVertex.AddEdge(edgePropertySelector.Resolve(), inVertex);
            GremlinqContext.Current.TypeProvider.SetType(edge, typeof(TModel));
            return Wrap(edge, assignMembers);
        }

        public static IEdge<TEdgeModel> AddEdge<TOutModel, TInModel, TModel, TEdgeModel>(
            this IVertex<TOutModel> outVertex,
            Expression<Func<TOutModel, KeyValuePair<TModel, TInModel>>> edgePropertySelector,
            IVertex<TInModel> inVertex,
            Action<TEdgeModel> assignMembers)
            where TModel : class
            where TEdgeModel : class, TModel
        {
            Contract.Requires(outVertex != null);
            Contract.Requires(edgePropertySelector != null);
            Contract.Requires(inVertex != null);
            Contract.Requires(assignMembers != null);
            Contract.Ensures(Contract.Result<IEdge<TModel>>() != null);

            var edge = outVertex.AddEdge(edgePropertySelector.Resolve(), inVertex);
            GremlinqContext.Current.TypeProvider.SetType(edge, typeof(TEdgeModel));
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
            GremlinqContext.Current.TypeProvider.SetType(edge, typeof(TModel));
            return Wrap(edge, assignMembers);
        }

        public static IEdge<TEdgeModel> AddEdge<TOutModel, TInModel, TModel, TEdgeModel>(
            this IVertex<TOutModel> outVertex,
            Expression<Func<TOutModel, IEnumerable<KeyValuePair<TModel, TInModel>>>> edgePropertySelector,
            IVertex<TInModel> inVertex,
            Action<TEdgeModel> assignMembers) 
            where TModel : class
            where TEdgeModel : class, TModel
        {
            Contract.Requires(outVertex != null);
            Contract.Requires(edgePropertySelector != null);
            Contract.Requires(inVertex != null);
            Contract.Ensures(Contract.Result<IEdge<TModel>>() != null);

            var edge = outVertex.AddEdge(edgePropertySelector.Resolve(), inVertex);
            GremlinqContext.Current.TypeProvider.SetType(edge, typeof(TEdgeModel));
            return edge.As<TEdgeModel>();
        }
    }
}
