using System;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq.Contracts
{
    public static class TypeProviderContract
    {
        public static void ValidateSetType(IElement element, Type type)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (type == null)
                throw new ArgumentNullException(nameof(type));
        }

        public static void ValidateTryGetType(IElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));
        }
    }
}
