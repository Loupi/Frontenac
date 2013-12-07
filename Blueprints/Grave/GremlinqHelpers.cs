using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using Castle.Components.DictionaryAdapter;
using Frontenac.Blueprints;

namespace Grave
{
    public static class GremlinqHelpers
    {
        public static IEnumerable<TSource> Loop<TSource>(this TSource element, Func<TSource, IEnumerable<TSource>> func,
                                                         int nbIterations)
        {
            return Loop(new[] {element}, func, nbIterations);
        }

        public static IEnumerable<TSource> Loop<TSource>(this IEnumerable<TSource> sources,
                                                         Func<TSource, IEnumerable<TSource>> func, int iterations)
        {
            var next = sources;

            for (var i = 0; i < iterations; i++)
            {
                next = next.SelectMany(func);
            }

            return next;
        }

        public static IEnumerable<IVertex> V(this IGraph g, string propertyName, object value)
        {
            return g.GetVertices(propertyName, value);
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

        public static IEnumerable<T> P<T>(this IEnumerable<IElement> elements, string propertyName)
        {
            return elements.Where(e => e.ContainsKey(propertyName)).Select(e => e[propertyName]).OfType<T>();
        }

        public static TResult P<TModel, TResult>(this IElement element, Expression<Func<TModel, TResult>> propertySelector)
        {
            return P<TResult>(element, ResolvePropertyName(propertySelector));
        }

        public static bool P<TModel>(this IElement element, Func<TModel, IElement, bool> predicate)
        {
            return predicate.Invoke(default(TModel), element);
        }

        public static TModel As<TModel>(this IElement element) where TModel:class 
        {
            return new DictionaryAdapterFactory().GetAdapter(typeof (TModel), element) as TModel;
        }

        public static TResult P<TResult>(this IElement element, string propertyName)
        {
            var result = default(TResult);
            object value;
            if (element.TryGetValue(propertyName, out value) && value != null)
            {
                if (value is TResult)
                    result = (TResult)value;
                else
                {
                    try
                    {
                        var converted = Convert.ChangeType(value, typeof(TResult));
                        if (converted != null)
                            result = (TResult)converted;
                    }
                    catch
                    {
                        result = default(TResult);
                    }
                }
            }
            return result;
        }

        public static string ResolvePropertyName(Expression e)
        {
            if (e.NodeType == ExpressionType.Lambda)
            {
                return ResolvePropertyName(((LambdaExpression)e).Body);
            }
            if (e.NodeType == ExpressionType.MemberAccess)
            {
                return ((MemberExpression)e).Member.Name;
            }
            throw new InvalidOperationException("Given expression is not type MemberAccess.");
        }

        public static IEnumerable<T> DynamicSelect<T>(this IEnumerable<IVertex> vertices, Func<dynamic, T> dFunc)
        {
            return vertices.Select(t => dFunc(t));
        }

        public static IEnumerable<IVertex> DynamicWhere(this IEnumerable<IVertex> vertices, Predicate<dynamic> dPredicate)
        {
            return vertices.Where(t => dPredicate(t));
        }
    }
}