using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using Castle.Components.DictionaryAdapter;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        private static readonly IDictionaryAdapterFactory DictionaryAdapterFactory = new DictionaryAdapterFactory();

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

        public static TModel Proxy<TModel>(this IElement element) 
            where TModel : class
        {
            Contract.Requires(element != null);
            Contract.Ensures(Contract.Result<TModel>() != null);

            object typeName;
            
            var typeToProxy = element.TryGetValue(TypePropertyName, out typeName)
                                  ? System.Type.GetType(typeName.ToString())
                                  : typeof(TModel);

            var propsDesc = new PropertyDescriptor();
            propsDesc.AddBehavior(new DictionaryPropertyConverter());
            var proxy = (TModel)DictionaryAdapterFactory.GetAdapter(typeToProxy, element, propsDesc);
            return proxy;
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

            object typeName;
            if (!element.TryGetValue(TypePropertyName, out typeName))
                throw new NullReferenceException();

            return System.Type.GetType(typeName.ToString());
        }
    }
}
