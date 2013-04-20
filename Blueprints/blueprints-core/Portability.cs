using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints
{
    public static class Portability
    {
        public static TValue get<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue ret;
            dictionary.TryGetValue(key, out ret);
            return ret;
        }

        public static TValue get<TValue>(this IList<TValue> list, int at)
        {
            TValue ret;
            ret = list.ElementAtOrDefault(at);
            return ret;
        }

        public static TValue javaRemove<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue ret = default(TValue);
            if (key != null)
            {
                if (dictionary.TryGetValue(key, out ret))
                    dictionary.Remove(key);
            }
            return ret;
        }

        public static TValue put<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            TValue ret;
            if (dictionary.TryGetValue(key, out ret))
                dictionary[key] = value;
            else
                dictionary.Add(key, value);
            return ret;
        }

        public static bool isNumber(object expression)
        {
            return expression is byte ||
                   expression is sbyte ||
                   expression is ushort ||
                   expression is short ||
                   expression is uint ||
                   expression is int ||
                   expression is ulong ||
                   expression is long ||
                   expression is float ||
                   expression is double ||
                   expression is decimal;
        }
    }
}
