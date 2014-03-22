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
        public static IVertex<TModel> Wrap<TModel>(
            this IVertex vertex,
            Action<TModel> assignMembers)
            where TModel : class
        {
            Contract.Requires(vertex != null);
            Contract.Requires(assignMembers != null);
            Contract.Ensures(Contract.Result<IVertex<TModel>>() != null);

            var wrapper = vertex.As<TModel>();
            assignMembers(wrapper.Model);
            return wrapper;
        }

        public static IEdge<TModel> Wrap<TModel>(
            this IEdge edge,
            Action<TModel> assignMembers)
            where TModel : class
        {
            Contract.Requires(edge != null);
            Contract.Requires(assignMembers != null);
            Contract.Ensures(Contract.Result<IEdge<TModel>>() != null);

            var wrapper = edge.As<TModel>();
            assignMembers(wrapper.Model);
            return wrapper;
        }

        public static object Proxy(this IElement element, Type type)
        {
            Contract.Requires(element != null);
            Contract.Requires(type != null);
            Contract.Ensures(Contract.Result<object>() != null);

            var proxy = GremlinqContext.Current.ProxyFactory.Create(element, type);
            return proxy;
        }

        public static IEnumerable<object> Proxy(this IEnumerable<IElement> elements, Type type)
        {
            Contract.Requires(elements != null);
            Contract.Ensures(Contract.Result<IEnumerable<object>>() != null);

            return elements.Select(element => element.Proxy(type));
        }

        public static TModel Proxy<TModel>(this IElement element) 
            where TModel : class
        {
            Contract.Requires(element != null);
            Contract.Ensures(Contract.Result<TModel>() != null);

            Type type;
            var proxyType = GremlinqContext.Current.TypeProvider.TryGetType(element, out type) ? type : typeof(TModel);

            if(proxyType == null)
                throw new NullReferenceException("No type present on element");

            if(!typeof(TModel).IsAssignableFrom(proxyType)) 
                throw  new InvalidOperationException(string.Format("Element type {0} does not match TModel {1}.", type, typeof(TModel)));

            var proxy = (TModel)GremlinqContext.Current.ProxyFactory.Create(element, proxyType);
            return proxy;
        }

        public static IEnumerable<TModel> Proxy<TModel>(this IEnumerable<IElement> elements)
            where TModel : class
        {
            Contract.Requires(elements != null);
            Contract.Ensures(Contract.Result<IEnumerable<object>>() != null);

            return elements.Select(element => element.Proxy<TModel>());
        }

        public static string Resolve(this Expression e)
        {
            Contract.Requires(e != null);
            Contract.Ensures(Contract.Result<string>() != null);

            if (e.NodeType == ExpressionType.Lambda)
                return ((LambdaExpression)e).Body.Resolve();

            if (e.NodeType == ExpressionType.MemberAccess)
                return ((MemberExpression)e).Member.Name;

            throw new InvalidOperationException("Given expression is not of type MemberAccess.");
        }

        public static Type Type(this IElement element)
        {
            Contract.Requires(element != null);
            Contract.Ensures(Contract.Result<Type>() != null);

            Type type;
            if (!GremlinqContext.Current.TypeProvider.TryGetType(element, out type))
                throw new InvalidOperationException(string.Format("No type found for {0}", element));

            return type;
        }

        public static IEnumerable<TResult> OfType<TResult>(this IEnumerable<IElement> elements)
            where TResult : class
        {
            Contract.Requires(elements != null);
            Contract.Ensures(Contract.Result<IEnumerable<TResult>>() != null);

            Type type;
            var context = GremlinqContext.Current;
            return elements.Where(t => context.TypeProvider.TryGetType(t, out type) && typeof(TResult).IsAssignableFrom(t.Type()))
                .Select(t => t.Proxy<TResult>());
        }
    }
}
