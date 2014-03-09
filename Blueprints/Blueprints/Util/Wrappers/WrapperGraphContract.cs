using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers
{
    [ContractClassFor(typeof (IWrapperGraph))]
    public abstract class WrapperGraphContract : IWrapperGraph
    {
        public IGraph GetBaseGraph()
        {
            Contract.Ensures(Contract.Result<IGraph>() != null);
            return null;
        }
    }
}