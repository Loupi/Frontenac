using System;
using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        public static IDictionary<string, object> Map(this IElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            return element.ToDictionary(t => t.Key, t => t.Value);
        }

        public static IEnumerable<IDictionary<string, object>> Map(this IEnumerable<IElement> elements)
        {
            if (elements == null)
                throw new ArgumentNullException(nameof(elements));

            return elements.Select(e => e.Map());
        }
    }
}
