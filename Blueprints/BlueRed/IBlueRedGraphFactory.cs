using System;
using System.Diagnostics.Contracts;

namespace Frontenac.BlueRed
{
    [ContractClass(typeof(BlueRedGraphFactoryContract))]
    public interface IBlueRedGraphFactory : IDisposable
    {
        RedisGraph Create();
        void Destroy(RedisGraph graph);
    }
}
