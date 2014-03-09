using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq
{
    public static partial class GremlinqHelpers
    {
        public static IDictionary<string, object> Map(this IElement element)
        {
            Contract.Requires(element != null);
            Contract.Ensures(Contract.Result<IDictionary<string, object>>() != null);

            return element.ToDictionary(t => t.Key, t => t.Value);
        }

        public static IEnumerable<IDictionary<string, object>> Map(this IEnumerable<IElement> elements)
        {
            Contract.Requires(elements != null);
            Contract.Ensures(Contract.Result<IEnumerable<IDictionary<string, object>>>() != null);

            return elements.Select(e => e.Map());
        }
    }
}
