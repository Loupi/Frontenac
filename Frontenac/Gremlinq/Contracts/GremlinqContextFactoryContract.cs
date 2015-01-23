using System.Diagnostics.Contracts;

namespace Frontenac.Gremlinq.Contracts
{
    [ContractClassFor(typeof(IGremlinqContextFactory))]
    public abstract class GremlinqContextFactoryContract : IGremlinqContextFactory
    {
        public GremlinqContext Create()
        {
            Contract.Ensures(Contract.Result<GremlinqContext>() != null);
            return null;
        }
    }
}