using System.Collections.Generic;
using System.Linq;

namespace Frontenac.Blueprints
{
    public static class Portability
    {
        public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue ret;
            dictionary.TryGetValue(key, out ret);
            return ret;
        }

        public static TValue Get<TValue>(this IList<TValue> list, int at)
        {
            TValue ret = list.ElementAtOrDefault(at);
            return ret;
        }

        public static TValue JavaRemove<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue ret = default(TValue);
            if (key != null)
            {
                if (dictionary.TryGetValue(key, out ret))
                    dictionary.Remove(key);
            }
            return ret;
        }

        public static TValue Put<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            TValue ret;
            if (dictionary.TryGetValue(key, out ret))
                dictionary[key] = value;
            else
                dictionary.Add(key, value);
            return ret;
        }

        public static bool IsNumber(object expression)
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
