﻿using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Frontenac.Blueprints
{
    public static class Portability
    {
        public const string ContractExceptionName = "System.Diagnostics.Contracts.__ContractsRuntime+ContractException";

        public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            Contract.Requires(dictionary != null);

            TValue ret;
            dictionary.TryGetValue(key, out ret);
            return ret;
        }

        public static TValue Get<TValue>(this IList<TValue> list, int at)
        {
            Contract.Requires(list != null);

            var ret = list.ElementAtOrDefault(at);
            return ret;
        }

        public static TValue JavaRemove<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            var ret = default(TValue);
            if (!Equals(key, default(TKey)))
            {
                if (dictionary.TryGetValue(key, out ret))
                    dictionary.Remove(key);
            }
            return ret;
        }

        public static TValue Put<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            Contract.Requires(dictionary != null);

            TValue ret;
            if (dictionary.TryGetValue(key, out ret))
                dictionary[key] = value;
            else
                dictionary.Add(key, value);
            return ret;
        }

        [Pure]
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