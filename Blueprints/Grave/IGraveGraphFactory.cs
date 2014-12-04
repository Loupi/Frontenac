using System;
using System.Diagnostics.Contracts;

namespace Frontenac.Grave
{
    [ContractClass(typeof (GraveGraphFactoryContract))]
    public interface IGraveGraphFactory : IDisposable
    {
        GraveGraph Create();
        GraveTransactionalGraph CreateTransactional();
        void Destroy(GraveGraph graph);
    }
}