using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        public static IVertex<TModel> AddVertex<TModel>(this IGraph graph) 
            where TModel : class
        {
            Contract.Requires(graph != null);
            Contract.Ensures(Contract.Result<IVertex<TModel>>() != null);

            var vertex = graph.AddVertex(null);
            GremlinqContext.Current.TypeProvider.SetType(vertex, typeof(TModel));
            return vertex.As<TModel>();
        }

        public static IVertex<TModel> AddVertex<TModel>(this IGraph graph, Action<TModel> assignMembers) 
            where TModel : class
        {
            Contract.Requires(graph != null);
            Contract.Requires(assignMembers != null);
            Contract.Ensures(Contract.Result<IVertex<TModel>>() != null);

            var vertex = graph.AddVertex(null);
            GremlinqContext.Current.TypeProvider.SetType(vertex, typeof(TModel));
            return vertex.Wrap(assignMembers);
        }

        public static IVertex<TModel> AddVertex<TModel>(this IGraph graph, object id)
            where TModel : class
        {
            Contract.Requires(graph != null);
            Contract.Requires(id != null);
            Contract.Ensures(Contract.Result<IVertex<TModel>>() != null);

            var vertex = graph.AddVertex(id);
            GremlinqContext.Current.TypeProvider.SetType(vertex, typeof(TModel));
            return vertex.As<TModel>();
        }

        public static IVertex<TModel> AddVertex<TModel>(this IGraph graph, object id, Action<TModel> assignMembers)
            where TModel : class
        {
            Contract.Requires(graph != null);
            Contract.Requires(id != null);
            Contract.Requires(assignMembers != null);
            Contract.Ensures(Contract.Result<IVertex<TModel>>() != null);

            var vertex = graph.AddVertex(id);
            GremlinqContext.Current.TypeProvider.SetType(vertex, typeof(TModel));
            return vertex.Wrap(assignMembers);
        }

        public static IEdge AddEdge<TOutModel, TInModel>(
            this IVertex<TOutModel> outVertex, object id,
            Expression<Func<TOutModel, TInModel>> edgePropertySelector,
            IVertex<TInModel> inVertex)
        {
            Contract.Requires(outVertex != null);
            Contract.Requires(edgePropertySelector != null);
            Contract.Requires(inVertex != null);
            Contract.Ensures(Contract.Result<IEdge>() != null);

            return outVertex.AddEdge(id, edgePropertySelector.Resolve(), inVertex);
        }

        public static IEdge AddEdge<TOutModel, TInModel>(
            this IVertex<TOutModel> outVertex, object id,
            Expression<Func<TOutModel, IEnumerable<TInModel>>> edgePropertySelector,
            IVertex<TInModel> inVertex)
        {
            Contract.Requires(outVertex != null);
            Contract.Requires(edgePropertySelector != null);
            Contract.Requires(inVertex != null);
            Contract.Ensures(Contract.Result<IEdge>() != null);

            return outVertex.AddEdge(id, edgePropertySelector.Resolve(), inVertex);
        }

        public static IEdge<TModel> AddEdge<TOutModel, TInModel, TModel>(
            this IVertex<TOutModel> outVertex, object id,
            Expression<Func<TOutModel, KeyValuePair<TModel, TInModel>>> edgePropertySelector,
            IVertex<TInModel> inVertex) 
            where TModel : class
            where TInModel : class 
        {
            Contract.Requires(outVertex != null);
            Contract.Requires(edgePropertySelector != null);
            Contract.Requires(inVertex != null);
            Contract.Ensures(Contract.Result<IEdge<TModel>>() != null);

            var edge = outVertex.AddEdge(id, edgePropertySelector.Resolve(), inVertex);
            GremlinqContext.Current.TypeProvider.SetType(edge, typeof(TModel));
            return edge.As<TModel>();
        }

        public static IEdge<TModel> AddEdge<TOutModel, TInModel, TModel>(
            this IVertex<TOutModel> outVertex, object id,
            Expression<Func<TOutModel, IEnumerable<KeyValuePair<TModel, TInModel>>>> edgePropertySelector,
            IVertex<TInModel> inVertex) 
            where TModel : class
            where TInModel : class 
        {
            Contract.Requires(outVertex != null);
            Contract.Requires(edgePropertySelector != null);
            Contract.Requires(inVertex != null);
            Contract.Ensures(Contract.Result<IEdge<TModel>>() != null);

            var edge = outVertex.AddEdge(id, edgePropertySelector.Resolve(), inVertex);
            GremlinqContext.Current.TypeProvider.SetType(edge, typeof(TModel));
            return edge.As<TModel>();
        }

        public static IEdge<TModel> AddEdge<TOutModel, TInModel, TModel>(
            this IVertex<TOutModel> outVertex, object id,
            Expression<Func<TOutModel, KeyValuePair<TModel, TInModel>>> edgePropertySelector,
            IVertex<TInModel> inVertex,
            Action<TModel> assignMembers) 
            where TModel : class
            where TInModel : class 
        {
            Contract.Requires(outVertex != null);
            Contract.Requires(edgePropertySelector != null);
            Contract.Requires(inVertex != null);
            Contract.Requires(assignMembers != null);
            Contract.Ensures(Contract.Result<IEdge<TModel>>() != null);

            var edge = outVertex.AddEdge(id, edgePropertySelector.Resolve(), inVertex);
            GremlinqContext.Current.TypeProvider.SetType(edge, typeof(TModel));
            return Wrap(edge, assignMembers);
        }

        public static IEdge<TEdgeModel> AddEdge<TOutModel, TInModel, TModel, TEdgeModel>(
            this IVertex<TOutModel> outVertex, object id,
            Expression<Func<TOutModel, KeyValuePair<TModel, TInModel>>> edgePropertySelector,
            IVertex<TInModel> inVertex,
            Action<TEdgeModel> assignMembers)
            where TModel : class
            where TInModel : class 
            where TEdgeModel : class, TModel
        {
            Contract.Requires(outVertex != null);
            Contract.Requires(edgePropertySelector != null);
            Contract.Requires(inVertex != null);
            Contract.Requires(assignMembers != null);
            Contract.Ensures(Contract.Result<IEdge<TModel>>() != null);

            var edge = outVertex.AddEdge(id, edgePropertySelector.Resolve(), inVertex);
            GremlinqContext.Current.TypeProvider.SetType(edge, typeof(TEdgeModel));
            return Wrap(edge, assignMembers);
        }

        public static IEdge<TModel> AddEdge<TOutModel, TInModel, TModel>(
            this IVertex<TOutModel> outVertex, object id,
            Expression<Func<TOutModel, IEnumerable<KeyValuePair<TModel, TInModel>>>> edgePropertySelector,
            IVertex<TInModel> inVertex,
            Action<TModel> assignMembers)
            where TModel : class
            where TInModel : class 
        {
            Contract.Requires(outVertex != null);
            Contract.Requires(edgePropertySelector != null);
            Contract.Requires(inVertex != null);
            Contract.Requires(assignMembers != null);
            Contract.Ensures(Contract.Result<IEdge<TModel>>() != null);

            var edge = outVertex.AddEdge(id, edgePropertySelector.Resolve(), inVertex);
            GremlinqContext.Current.TypeProvider.SetType(edge, typeof(TModel));
            return Wrap(edge, assignMembers);
        }

        public static IEdge<TEdgeModel> AddEdge<TOutModel, TInModel, TModel, TEdgeModel>(
            this IVertex<TOutModel> outVertex, object id,
            Expression<Func<TOutModel, IEnumerable<KeyValuePair<TModel, TInModel>>>> edgePropertySelector,
            IVertex<TInModel> inVertex,
            Action<TEdgeModel> assignMembers)
            where TModel : class
            where TInModel : class 
            where TEdgeModel : class, TModel
        {
            Contract.Requires(outVertex != null);
            Contract.Requires(edgePropertySelector != null);
            Contract.Requires(inVertex != null);
            Contract.Ensures(Contract.Result<IEdge<TModel>>() != null);

            var edge = outVertex.AddEdge(id, edgePropertySelector.Resolve(), inVertex);
            GremlinqContext.Current.TypeProvider.SetType(edge, typeof(TEdgeModel));
            return edge.As<TEdgeModel>();
        }

        public static void Add<TEdgeModel, TVertexModel>(this ICollection<KeyValuePair<TEdgeModel, TVertexModel>> relation,
                                                         TVertexModel vertex)
            where TEdgeModel : class
            where TVertexModel : class
        {
            Contract.Requires(relation != null);

            relation.Add(new KeyValuePair<TEdgeModel, TVertexModel>(null, vertex));
        }

        public static void Add<TEdgeModel, TVertexModel>(this ICollection<KeyValuePair<TEdgeModel, TVertexModel>> relation, 
                                                         TEdgeModel edge, TVertexModel vertex)
            where TEdgeModel : class
            where TVertexModel : class
        {
            Contract.Requires(relation != null);

            relation.Add(new KeyValuePair<TEdgeModel, TVertexModel>(edge, vertex));
        }

        public static void Add<TEdgeModel, TVertexModel>(this ICollection<KeyValuePair<TEdgeModel, TVertexModel>> relation,
                                                         TVertexModel vertex, Action<TEdgeModel> assignEdge)
            where TEdgeModel : class
            where TVertexModel : class
        {
            Contract.Requires(relation != null);
            Contract.Requires(assignEdge != null);

            var edgeModel = Transient<TEdgeModel>();
            assignEdge(edgeModel);

            relation.Add(new KeyValuePair<TEdgeModel, TVertexModel>(edgeModel, vertex));
        }

        public static void Add<TEdgeModel, TDerivedEdgeModel, TVertexModel>(this ICollection<KeyValuePair<TEdgeModel, TVertexModel>> relation,
                                                                            TVertexModel vertex, Action<TDerivedEdgeModel> assignEdge)
            where TEdgeModel : class
            where TVertexModel : class
            where TDerivedEdgeModel: class, TEdgeModel
        {
            Contract.Requires(relation != null);
            Contract.Requires(assignEdge != null);

            var edgeModel = Transient<TDerivedEdgeModel>();
            assignEdge(edgeModel);

            relation.Add(new KeyValuePair<TEdgeModel, TVertexModel>(edgeModel, vertex));
        }
    }
}
