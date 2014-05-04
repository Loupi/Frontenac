using System;
using System.Diagnostics.Contracts;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq.Contracts
{
    [ContractClassFor(typeof (ITypeProvider))]
    public abstract class TypeProviderContract : ITypeProvider
    {
        public void SetType(IElement element, Type type)
        {
            Contract.Requires(element != null);
            Contract.Requires(type != null);
        }

        public bool TryGetType(IElement element, out Type type)
        {
            Contract.Requires(element != null);
            type = null;
            return false;
        }
    }
}
