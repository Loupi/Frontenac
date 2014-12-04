using System.Diagnostics.Contracts;

namespace Frontenac.BlueRed
{
    [ContractClassFor(typeof(IBlueRedGraphFactory))]
    public abstract class BlueRedGraphFactoryContract : IBlueRedGraphFactory
    {
        public RedisGraph Create()
        {
            Contract.Ensures(Contract.Result<RedisGraph>() != null);
            return null;
        }

        public void Destroy(RedisGraph graph)
        {
            Contract.Requires(graph != null);
        }

        public abstract void Dispose();
    }
}