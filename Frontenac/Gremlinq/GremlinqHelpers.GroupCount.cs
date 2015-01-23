using System;
using System.Collections.Generic;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        public static IDictionary<T, int> GroupCount<T>(
            this IEnumerable<T> source)
        {
            var dic = new Dictionary<T, int>();
#pragma warning disable 168
            foreach (var source1 in source.GroupCount(dic))
#pragma warning restore 168
            {
                
            }
            return dic;
        }

        public static IEnumerable<T> GroupCount<T>(
            this IEnumerable<T> source,
            IDictionary<T, int> m)
        {

            foreach (var element in source)
            {
                int value;
                if (!m.TryGetValue(element, out value))
                    m.Add(element, 1);
                else
                    m[element] = value + 1;

                yield return element;
            }
        }

        public static IEnumerable<T> GroupCount<T>(
            this IEnumerable<T> source,
            IDictionary<T, double> m)
        {

            foreach (var element in source)
            {
                double value;
                if (!m.TryGetValue(element, out value))
                    m.Add(element, 1);
                else
                    m[element] = value + 1;

                yield return element;
            }
        }

        public static IEnumerable<T> GroupCount<T, TKey, TValue>(
            this IEnumerable<T> source,
            IDictionary<TKey, TValue> m,
            Func<T, TKey> keySelector,
            Func<T, TValue, TValue> valueUpdater)
        {
            foreach (var element in source)
            {
                TValue value;
                var key = keySelector(element);
                if (!m.TryGetValue(key, out value))
                    m.Add(key, valueUpdater(element, default(TValue)));
                else
                    m[key] = valueUpdater(element, value);

                yield return element;
            }
        }
    }
}
