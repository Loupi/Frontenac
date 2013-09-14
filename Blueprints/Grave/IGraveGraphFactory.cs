using System;
using System.Diagnostics.Contracts;

namespace Grave
{
    [ContractClass(typeof (GraveGraphFactoryContract))]
    public interface IGraveGraphFactory : IDisposable
    {
        GraveGraph Create();
        void Destroy(GraveGraph graph);
    }
}