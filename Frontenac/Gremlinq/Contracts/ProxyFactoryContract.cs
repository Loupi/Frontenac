using System;
using System.Collections;

namespace Frontenac.Gremlinq.Contracts
{
    public static class ProxyFactoryContract
    {
        public static void ValidateCreate(IDictionary element, Type type)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (type == null)
                throw new ArgumentNullException(nameof(type));
        }
    }
}