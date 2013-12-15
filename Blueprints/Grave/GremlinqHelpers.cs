using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Castle.Components.DictionaryAdapter;
using Frontenac.Blueprints;
using Grave.Geo;
using PropertyDescriptor = Castle.Components.DictionaryAdapter.PropertyDescriptor;

namespace Grave
{
    public static class GremlinqHelpers
    {
        private static readonly DictionaryAdapterFactory DictionaryAdapterFactory = new DictionaryAdapterFactory();

        public static IEnumerable<IElementWrapper<IVertex, TInModel>> Loop<TModel, TInModel>(
                                                this IElementWrapper<IVertex, TModel> element,
                                                Func<TModel, TInModel> startPointSelector,
                                                Func<IElementWrapper<IVertex, TInModel>, IEnumerable<IElementWrapper<IVertex, TInModel>>> loopFunction,
                                                int nbIterations)
        {
            var next = (IEnumerable<IElementWrapper<IVertex, TInModel>>)new[] { element };

            for (var i = 0; i < nbIterations; i++)
                next = next.SelectMany(loopFunction);

            return next;
        }

        public static IEnumerable<IVertex> V(this IGraph g, string propertyName, object value)
        {
            return g.GetVertices(propertyName, value);
        }

        public static IEnumerable<IElementWrapper<IVertex, TModel>> V<TModel, TValue>(this IGraph graph,
                                                Expression<Func<TModel, TValue>> propertySelector, TValue value)
        {
            return graph.GetVertices(ResolvePropertyName(propertySelector), value).As<TModel>();
        }

        public static IDictionary<string, object> Map(this IElement e)
        {
            return e.ToDictionary(t => t.Key, t => t.Value);
        }

        public static IEnumerable<IDictionary<string, object>> Map(this IEnumerable<IElement> elements)
        {
            return elements.Select(e => e.Map());
        }

        public static IEnumerable<IVertex> In(this IVertex vertex, params string[] labels)
        {
            return vertex.GetVertices(Direction.In, labels);
        }

        public static IEnumerable<IVertex> In(this IEnumerable<IVertex> vertices, params string[] labels)
        {
            return vertices.SelectMany(t => t.GetVertices(Direction.In, labels));
        }

        public static IEnumerable<IElementWrapper<IVertex, TInModel>> In<TOutModel, TInModel>(
            this IElementWrapper<IVertex, TOutModel> vertex,
            Expression<Func<TOutModel, TInModel>> propertySelector)
        {
            return vertex.Element
                .In(ResolvePropertyName(propertySelector))
                .As<TInModel>();
        }

        public static IEnumerable<IElementWrapper<IVertex, TInModel>> In<TOutModel, TInModel>(
                                    this IEnumerable<IElementWrapper<IVertex, TOutModel>> vertices,
                                    Expression<Func<TOutModel, TInModel>> propertySelector)
        {
            return vertices
                .SelectMany(t => t.Element.GetVertices(Direction.In, ResolvePropertyName(propertySelector)))
                .As<TInModel>();
        }

        public static IEnumerable<IElementWrapper<IVertex, TInModel>> In<TOutModel, TInModel>(
                                    this IEnumerable<IElementWrapper<IVertex, TOutModel>> vertices,
                                    Expression<Func<IElementWrapper<IVertex, TOutModel>, IElementWrapper<IVertex, TInModel>>> propertySelector)
        {
            return vertices
                .SelectMany(t => t.Element.GetVertices(Direction.In, ResolvePropertyName(propertySelector)))
                .As<TInModel>();
        }

        public static TModel Proxy<TModel>(this IElement element)
        {
            object typeName;
            var typeToProxy = element.TryGetValue("_type", out typeName) ? Type.GetType(typeName.ToString()) : typeof (TModel);
            var propsDesc = new PropertyDescriptor();
            propsDesc.AddBehavior(new DictionaryPropertyConverter());
            var proxy = (TModel)DictionaryAdapterFactory.GetAdapter(typeToProxy, element, propsDesc);
            return proxy;
        }

        public static IElementWrapper<IVertex, TModel> As<TModel>(this IVertex vertex)
        {
            return new ElementWrapper<IVertex, TModel>(vertex, vertex.Proxy<TModel>());
        }

        public static IElementWrapper<IEdge, TModel> As<TModel>(this IEdge edge)
        {
            return new ElementWrapper<IEdge, TModel>(edge, edge.Proxy<TModel>());
        }

        public static IEnumerable<IElementWrapper<IVertex, TModel>> As<TModel>(this IEnumerable<IVertex> vertices)
        {
            return vertices.Select(t => t.As<TModel>());
        }

        public static IEnumerable<IElementWrapper<IEdge, TModel>> As<TModel>(this IEnumerable<IEdge> edges)
        {
            return edges.Select(t => t.As<TModel>());
        }

        public static string ResolvePropertyName(Expression e)
        {
            if (e.NodeType == ExpressionType.Lambda)
                return ResolvePropertyName(((LambdaExpression)e).Body);

            if (e.NodeType == ExpressionType.MemberAccess)
                return ((MemberExpression)e).Member.Name;

            throw new InvalidOperationException("Given expression is not type MemberAccess.");
        }

        public static IElementWrapper<IVertex, TModel> AddVertex<TModel>(this IGraph graph, Action<TModel> assignMembers)
        {
            var vertex = graph.AddVertex(null);
            var typeName = typeof (TModel).FullName;
            vertex.Add("_type", typeName);
            var proxy = vertex.Proxy<TModel>();
            assignMembers(proxy);
            return new ElementWrapper<IVertex, TModel>(vertex, proxy);
        }

        public static IEdge AddEdge<TOutModel, TInModel>(this IElementWrapper<IVertex, TOutModel> outVertex,
                                                         Expression<Func<TOutModel, TInModel>> edgePropertySelector,
                                                         IElementWrapper<IVertex, TInModel> inVertex)
        {
            return outVertex.Element.AddEdge(ResolvePropertyName(edgePropertySelector), inVertex.Element);
        }

        public static IEdge AddEdge<TOutModel, TInModel>(this IElementWrapper<IVertex, TOutModel> outVertex,
                                                         Expression<Func<TOutModel, IEnumerable<TInModel>>>
                                                             edgePropertySelector,
                                                         IElementWrapper<IVertex, TInModel> inVertex)
        {
            return outVertex.Element.AddEdge(ResolvePropertyName(edgePropertySelector), inVertex.Element);
        }

        public static IElementWrapper<IEdge, TEdgeModel> CreateWrapper<TInModel, TOutModel, TEdgeModel>(
                                                        IElementWrapper<IVertex, TOutModel> outVertex,
                                                        Expression expression,
                                                        IElementWrapper<IVertex, TInModel> inVertex)
        {
            var edge = outVertex.Element.AddEdge(ResolvePropertyName(expression), inVertex.Element);
            var model = edge.Proxy<TEdgeModel>();
            return new ElementWrapper<IEdge, TEdgeModel>(edge, model);
        }

        public static IElementWrapper<IEdge, TEdgeModel> CreateWrapper<TInModel, TOutModel, TEdgeModel>(
                                                        IElementWrapper<IVertex, TOutModel> outVertex,
                                                        Expression expression,
                                                        IElementWrapper<IVertex, TInModel> inVertex,
                                                        Action<TEdgeModel> assignMembers)
        {
            var wrapper = CreateWrapper<TInModel, TOutModel, TEdgeModel>(outVertex, expression, inVertex);
            assignMembers(wrapper.Model);
            return wrapper;
        }

        public static IElementWrapper<IEdge, TEdgeModel> AddEdge<TOutModel, TInModel, TEdgeModel>(
                                                        this IElementWrapper<IVertex, TOutModel> outVertex,
                                                        Expression<Func<TOutModel, KeyValuePair<TEdgeModel, TInModel>>> edgePropertySelector,
                                                        IElementWrapper<IVertex, TInModel> inVertex)
        {
            return CreateWrapper<TInModel, TOutModel, TEdgeModel>(outVertex, edgePropertySelector, inVertex);
        }

        public static IElementWrapper<IEdge, TEdgeModel> AddEdge<TOutModel, TInModel, TEdgeModel>(
                                                        this IElementWrapper<IVertex, TOutModel> outVertex,
                                                        Expression<Func<TOutModel, KeyValuePair<TEdgeModel, TInModel>>> edgePropertySelector,
                                                        IElementWrapper<IVertex, TInModel> inVertex,
                                                        Action<TEdgeModel> assignMembers)
        {
            return CreateWrapper(outVertex, edgePropertySelector, inVertex, assignMembers);
        }

        public static IElementWrapper<IEdge, TEdgeModel> AddEdge<TOutModel, TInModel, TEdgeModel>(
                                                        this IElementWrapper<IVertex, TOutModel> outVertex,
                                                        Expression<Func<TOutModel, IEnumerable<KeyValuePair<TEdgeModel, TInModel>>>> edgePropertySelector,
                                                        IElementWrapper<IVertex, TInModel> inVertex,
                                                        Action<TEdgeModel> assignMembers)
        {
            return CreateWrapper(outVertex, edgePropertySelector, inVertex, assignMembers);
        }

        public static void CreateIndex<TModel, TIndexType>(this IKeyIndexableGraph graph,
                                                           Expression<Func<TModel, TIndexType>> propertySelector,
                                                           Type indexType)
        {
            var name = ResolvePropertyName(propertySelector);
            if (!graph.GetIndexedKeys(indexType).Contains(name))
                graph.CreateKeyIndex(name, indexType);
        }

        public static void CreateVertexIndex<TModel, TResult>(this IKeyIndexableGraph graph,
                                                              Expression<Func<TModel, TResult>> propertySelector)
        {
            graph.CreateIndex(propertySelector, typeof(IVertex));
        }

        public static void CreateEdgeIndex<TModel, TResult>(this IKeyIndexableGraph graph,
                                                            Expression<Func<TModel, TResult>> propertySelector)
        {
            graph.CreateIndex(propertySelector, typeof(IEdge));
        }

        public static IQueryWrapper<TModel> Query<TModel>(this IGraph graph)
        {
            return new QueryWrapper<TModel>(graph.Query());
        }

        public static IQueryWrapper<TModel> Has<TModel, TResult>(this IQueryWrapper<TModel> query, 
                                                                 Expression<Func<TModel, TResult>> propertySelector, 
                                                                 Compare compare, TResult value)
        {
            query.InnerQuery.Has(ResolvePropertyName(propertySelector), compare, value);
            return query;
        }

        public static IQueryWrapper<TModel> Has<TModel>(this IQueryWrapper<TModel> query, 
                                                        Expression<Func<TModel, GeoPoint>> propertySelector, 
                                                        Compare compare, 
                                                        IGeoShape value)
        {
            query.InnerQuery.Has(ResolvePropertyName(propertySelector), compare, value);
            return query;
        }

        public static IEnumerable<IElementWrapper<IEdge, TModel>> Edges<TModel>(this IQueryWrapper<TModel> query)
        {
            return query.InnerQuery.Edges().As<TModel>();
        }
        
        public static IElementWrapper<IVertex, TInModel> In<TModel, TInModel>(this IElementWrapper<IEdge, TModel> edge,
                                                                              Expression<Func<TModel, TInModel>> edgePropertySelector)
        {
            var vertex = edge.Element.GetVertex(Direction.In);
            return new ElementWrapper<IVertex, TInModel>(vertex, vertex.Proxy<TInModel>());
        }

        public static IElementWrapper<IVertex, TOutModel> Out<TModel, TOutModel>(this IElementWrapper<IEdge, TModel> edge,
                                                                              Expression<Func<TModel, TOutModel>> edgePropertySelector)
        {
            var vertex = edge.Element.GetVertex(Direction.Out);
            return new ElementWrapper<IVertex, TOutModel>(vertex, vertex.Proxy<TOutModel>());
        }

        public static IEnumerable<IElementWrapper<IVertex, TOutModel>> Out<TModel, TOutModel>(this IElementWrapper<IVertex, TModel> vertex,
                                                                              params Expression<Func<TModel, TOutModel>>[] edgePropertySelectors)
        {
            return vertex.Element.GetVertices(Direction.Out, edgePropertySelectors.Select(ResolvePropertyName).ToArray()).As<TOutModel>();
        }

        public static IEnumerable<IElementWrapper<IEdge, TEdgeModel>> Out<TModel, TOutModel, TEdgeModel>(this IElementWrapper<IVertex, TModel> vertex,
                                                                              params Expression<Func<TModel, KeyValuePair<TEdgeModel, TOutModel>>>[] edgePropertySelectors)
        {
            return vertex.Element.GetEdges(Direction.Out, edgePropertySelectors.Select(ResolvePropertyName).ToArray()).As<TEdgeModel>();
        }

        public static IEnumerable<IElementWrapper<IEdge, TEdgeModel>> Out<TModel, TOutModel, TEdgeModel>(this IElementWrapper<IVertex, TModel> vertex,
                                                                              params Expression<Func<TModel, IEnumerable<KeyValuePair<TEdgeModel, TOutModel>>>>[] edgePropertySelectors)
        {
            return vertex.Element.GetEdges(Direction.Out, edgePropertySelectors.Select(ResolvePropertyName).ToArray()).As<TEdgeModel>();
        }

        public static IEnumerable<IElementWrapper<IVertex, TInModel>> In<TModel, TInModel>(this IElementWrapper<IVertex, TModel> vertex,
                                                                              params Expression<Func<TModel, TInModel>>[] edgePropertySelectors)
        {
            return vertex.Element.GetVertices(Direction.In, edgePropertySelectors.Select(ResolvePropertyName).ToArray()).As<TInModel>();
        }

        public class ElementWrapper<TElement, TModel> : IElementWrapper<TElement, TModel> where TElement : IElement
        {
            internal ElementWrapper(TElement element, TModel model)
            {
                Element = element;
                Model = model;
            }

            public TElement Element { get; private set; }
            public TModel Model { get; private set; }
        }

        public interface IElementWrapper<out TElement, out TModel>
        {
            TElement Element { get; }
            TModel Model { get; }
        }

        public class QueryWrapper<TModel> : IQueryWrapper<TModel>
        {
            internal QueryWrapper(IQuery query)
            {
                InnerQuery = query;
            }

            public IQuery InnerQuery { get; set; }
        }

        public interface IQueryWrapper<out TModel>
        {
            IQuery InnerQuery { get; set; }
        }
    }
}