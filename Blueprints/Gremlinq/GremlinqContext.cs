using System;
using System.Collections.Generic;

namespace Frontenac.Gremlinq
{
    public static class GremlinqContext
    {
        public static IElementTypeProvider ElementTypeProvider { get; set; }

        static GremlinqContext()
        {
            ElementTypeProvider = new DictionaryElementTypeProvider(DictionaryElementTypeProvider.DefaulTypePropertyName, new Dictionary<int, Type>());
        }
    }
}