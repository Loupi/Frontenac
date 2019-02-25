using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {   
        public static IVertex<TModel> Wrap<TModel>(
            this IVertex vertex,
            Action<TModel> assignMembers)
            where TModel : class
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (assignMembers == null)
                throw new ArgumentNullException(nameof(assignMembers));

            var wrapper = vertex.As<TModel>();
            assignMembers(wrapper.Model);
            return wrapper;
        }

        public static IEdge<TModel> Wrap<TModel>(
            this IEdge edge,
            Action<TModel> assignMembers)
            where TModel : class
        {
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));
            if (assignMembers == null)
                throw new ArgumentNullException(nameof(assignMembers));

            var wrapper = edge.As<TModel>();
            assignMembers(wrapper.Model);
            return wrapper;
        }

        public static object Proxy(this IElement element, Type type)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var proxy = GremlinqContext.Current.ProxyFactory.Create(element, type);
            return proxy;
        }

        public static IEnumerable<object> Proxy(this IEnumerable<IElement> elements, Type type)
        {
            if (elements == null)
                throw new ArgumentNullException(nameof(elements));

            return elements.Select(element => element.Proxy(type));
        }

        public static TModel Proxy<TModel>(this IElement element) 
            where TModel : class
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            var proxy = (TModel)element.ProxyBestFit(typeof(TModel));
            return proxy;
        }

        public static TModel Transient<TModel>()
            where TModel : class
        {
            var dictionary = new Dictionary<string, object>();
            var proxy = (TModel)GremlinqContext.Current.ProxyFactory.Create(dictionary, typeof(TModel));
            return proxy;
        }

        public static TModel Transient<TModel>(this IGraph graph)
            where TModel : class
        {
            return Transient<TModel>();
        }

        public static TModel Transient<TModel>(this IGraph graph, Action<TModel> assignMembers)
            where TModel : class
        {
            if (assignMembers == null)
                throw new ArgumentNullException(nameof(assignMembers));

            var proxy = Transient<TModel>(graph);
            assignMembers(proxy);
            return proxy;
        }

        public static object ProxyBestFit(this IElement element, Type baseType)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (baseType == null)
                throw new ArgumentNullException(nameof(baseType));

            Type type;
            var proxyType = GremlinqContext.Current.TypeProvider.TryGetType(element, out type) ? type : baseType;

            if (proxyType == null)
                throw new NullReferenceException("No type present on element");

            if (!baseType.IsAssignableFrom(proxyType))
                throw new InvalidOperationException(string.Format("Element type {0} does not match TModel {1}.", type, baseType));

            var proxy = GremlinqContext.Current.ProxyFactory.Create(element, proxyType);
            return proxy;
        }

        public static IEnumerable<TModel> Proxy<TModel>(this IEnumerable<IElement> elements)
            where TModel : class
        {
            if (elements == null)
                throw new ArgumentNullException(nameof(elements));

            return elements.Select(element => element.Proxy<TModel>());
        }

        public static string Resolve(this Expression e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (e.NodeType == ExpressionType.Lambda)
                return ((LambdaExpression)e).Body.Resolve();

            if (e.NodeType == ExpressionType.MemberAccess)
                return ((MemberExpression)e).Member.Name;

            throw new InvalidOperationException("Given expression is not of type MemberAccess.");
        }

        public static Type Type(this IElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            Type type;
            if (!GremlinqContext.Current.TypeProvider.TryGetType(element, out type))
                throw new InvalidOperationException(string.Format("No type found for {0}", element));

            return type;
        }

        public static IEnumerable<TResult> OfType<TResult>(this IEnumerable<IElement> elements)
            where TResult : class
        {
            if (elements == null)
                throw new ArgumentNullException(nameof(elements));

            Type type;
            var context = GremlinqContext.Current;
            return elements.Where(t => context.TypeProvider.TryGetType(t, out type) && typeof(TResult).IsAssignableFrom(t.Type()))
                .Select(t => t.Proxy<TResult>());
        }
    }
}
