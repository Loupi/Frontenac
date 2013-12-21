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

        public static IEdge<TModel> CreateWrapper<TInModel, TOutModel, TModel>(
            IVertex<TOutModel> outVertex,
            Expression expression,
            IVertex<TInModel> inVertex) where TModel : class
        {
            Contract.Requires(outVertex != null);
            Contract.Requires(expression != null);
            Contract.Requires(inVertex != null);
            Contract.Ensures(Contract.Result<IEdge<TModel>>() != null);

            var edge = outVertex.AddEdge(expression.Resolve(), inVertex);
            var model = edge.Proxy<TModel>();
            return new Edge<TModel>(edge, model);
        }

        public static IEdge<TModel> CreateWrapper<TInModel, TOutModel, TModel>(
            IVertex<TOutModel> outVertex,
            Expression expression,
            IVertex<TInModel> inVertex,
            Action<TModel> assignMembers) where TModel : class
        {
            Contract.Requires(outVertex != null);
            Contract.Requires(expression != null);
            Contract.Requires(inVertex != null);
            Contract.Requires(assignMembers != null);
            Contract.Ensures(Contract.Result<IEdge<TModel>>() != null);

            var wrapper = CreateWrapper<TInModel, TOutModel, TModel>(outVertex, expression, inVertex);
            assignMembers(wrapper.Model);
            return wrapper;
        }

        public static TModel Proxy<TModel>(this IElement element) where TModel : class
        {
            Contract.Requires(element != null);
            Contract.Ensures(Contract.Result<TModel>() != null);

            object typeName;
            var typeToProxy = element.TryGetValue("_type", out typeName)
                                  ? Type.GetType(typeName.ToString())
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

            throw new InvalidOperationException("Given expression is not type MemberAccess.");
        }
    }
}
