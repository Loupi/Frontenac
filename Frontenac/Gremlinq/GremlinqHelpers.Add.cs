using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        public static IVertex<TModel> AddVertex<TModel>(this IGraph graph) 
            where TModel : class
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            var vertex = graph.AddVertex(null);
            GremlinqContext.Current.TypeProvider.SetType(vertex, typeof(TModel));
            return vertex.As<TModel>();
        }

        public static IVertex<TModel> AddVertex<TModel>(this IGraph graph, Action<TModel> assignMembers) 
            where TModel : class
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (assignMembers == null)
                throw new ArgumentNullException(nameof(assignMembers));

            var vertex = graph.AddVertex(null);
            GremlinqContext.Current.TypeProvider.SetType(vertex, typeof(TModel));
            return vertex.Wrap(assignMembers);
        }

        public static IVertex<TModel> AddVertex<TModel>(this IGraph graph, object id)
            where TModel : class
        {
            if (id == null)
                throw new ArgumentNullException(nameof(graph));
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var vertex = graph.AddVertex(id);
            GremlinqContext.Current.TypeProvider.SetType(vertex, typeof(TModel));
            return vertex.As<TModel>();
        }

        public static IVertex<TModel> AddVertex<TModel>(this IGraph graph, object id, Action<TModel> assignMembers)
            where TModel : class
        {
            if (id == null)
                throw new ArgumentNullException(nameof(graph));
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (assignMembers == null)
                throw new ArgumentNullException(nameof(assignMembers));

            var vertex = graph.AddVertex(id);
            GremlinqContext.Current.TypeProvider.SetType(vertex, typeof(TModel));
            return vertex.Wrap(assignMembers);
        }

        public static IEdge AddEdge<TOutModel, TInModel>(
            this IVertex<TOutModel> outVertex, object id,
            Expression<Func<TOutModel, TInModel>> edgePropertySelector,
            IVertex<TInModel> inVertex)
        {
            if (outVertex == null)
                throw new ArgumentNullException(nameof(outVertex));
            if (edgePropertySelector == null)
                throw new ArgumentNullException(nameof(edgePropertySelector));
            if (inVertex == null)
                throw new ArgumentNullException(nameof(inVertex));

            return outVertex.AddEdge(id, edgePropertySelector.Resolve(), inVertex);
        }

        public static IEdge AddEdge<TOutModel, TInModel>(
            this IVertex<TOutModel> outVertex, object id,
            Expression<Func<TOutModel, IEnumerable<TInModel>>> edgePropertySelector,
            IVertex<TInModel> inVertex)
        {
            if (outVertex == null)
                throw new ArgumentNullException(nameof(outVertex));
            if (edgePropertySelector == null)
                throw new ArgumentNullException(nameof(edgePropertySelector));
            if (inVertex == null)
                throw new ArgumentNullException(nameof(inVertex));

            return outVertex.AddEdge(id, edgePropertySelector.Resolve(), inVertex);
        }

        public static IEdge<TModel> AddEdge<TOutModel, TInModel, TModel>(
            this IVertex<TOutModel> outVertex, object id,
            Expression<Func<TOutModel, KeyValuePair<TModel, TInModel>>> edgePropertySelector,
            IVertex<TInModel> inVertex) 
            where TModel : class
            where TInModel : class 
        {
            if (outVertex == null)
                throw new ArgumentNullException(nameof(outVertex));
            if (edgePropertySelector == null)
                throw new ArgumentNullException(nameof(edgePropertySelector));
            if (inVertex == null)
                throw new ArgumentNullException(nameof(inVertex));

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
            if (outVertex == null)
                throw new ArgumentNullException(nameof(outVertex));
            if (edgePropertySelector == null)
                throw new ArgumentNullException(nameof(edgePropertySelector));
            if (inVertex == null)
                throw new ArgumentNullException(nameof(inVertex));

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
            if (outVertex == null)
                throw new ArgumentNullException(nameof(outVertex));
            if (edgePropertySelector == null)
                throw new ArgumentNullException(nameof(edgePropertySelector));
            if (inVertex == null)
                throw new ArgumentNullException(nameof(inVertex));
            if (assignMembers == null)
                throw new ArgumentNullException(nameof(assignMembers));

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
            if (outVertex == null)
                throw new ArgumentNullException(nameof(outVertex));
            if (edgePropertySelector == null)
                throw new ArgumentNullException(nameof(edgePropertySelector));
            if (inVertex == null)
                throw new ArgumentNullException(nameof(inVertex));
            if (assignMembers == null)
                throw new ArgumentNullException(nameof(assignMembers));

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
            if (outVertex == null)
                throw new ArgumentNullException(nameof(outVertex));
            if (edgePropertySelector == null)
                throw new ArgumentNullException(nameof(edgePropertySelector));
            if (inVertex == null)
                throw new ArgumentNullException(nameof(inVertex));
            if (assignMembers == null)
                throw new ArgumentNullException(nameof(assignMembers));

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
            if (outVertex == null)
                throw new ArgumentNullException(nameof(outVertex));
            if (edgePropertySelector == null)
                throw new ArgumentNullException(nameof(edgePropertySelector));
            if (inVertex == null)
                throw new ArgumentNullException(nameof(inVertex));

            var edge = outVertex.AddEdge(id, edgePropertySelector.Resolve(), inVertex);
            GremlinqContext.Current.TypeProvider.SetType(edge, typeof(TEdgeModel));
            return edge.As<TEdgeModel>();
        }

        public static void Add<TEdgeModel, TVertexModel>(this ICollection<KeyValuePair<TEdgeModel, TVertexModel>> relation,
                                                         TVertexModel vertex)
            where TEdgeModel : class
            where TVertexModel : class
        {
            if (relation == null)
                throw new ArgumentNullException(nameof(relation));

            relation.Add(new KeyValuePair<TEdgeModel, TVertexModel>(null, vertex));
        }

        public static void Add<TEdgeModel, TVertexModel>(this ICollection<KeyValuePair<TEdgeModel, TVertexModel>> relation, 
                                                         TEdgeModel edge, TVertexModel vertex)
            where TEdgeModel : class
            where TVertexModel : class
        {
            if (relation == null)
                throw new ArgumentNullException(nameof(relation));

            relation.Add(new KeyValuePair<TEdgeModel, TVertexModel>(edge, vertex));
        }

        public static void Add<TEdgeModel, TVertexModel>(this ICollection<KeyValuePair<TEdgeModel, TVertexModel>> relation,
                                                         TVertexModel vertex, Action<TEdgeModel> assignEdge)
            where TEdgeModel : class
            where TVertexModel : class
        {
            if (relation == null)
                throw new ArgumentNullException(nameof(relation));
            if (assignEdge == null)
                throw new ArgumentNullException(nameof(assignEdge));

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
            if (relation == null)
                throw new ArgumentNullException(nameof(relation));
            if (assignEdge == null)
                throw new ArgumentNullException(nameof(assignEdge));

            var edgeModel = Transient<TDerivedEdgeModel>();
            assignEdge(edgeModel);

            relation.Add(new KeyValuePair<TEdgeModel, TVertexModel>(edgeModel, vertex));
        }
    }
}
