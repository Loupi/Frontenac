using System;
using System.Diagnostics.Contracts;
using Frontenac.Grave.Esent;

namespace Frontenac.Grave
{
    [ContractClass(typeof (GraveGraphFactoryContract))]
    public interface IGraveGraphFactory : IDisposable
    {
        GraveGraph Create();
        GraveTransactionalGraph CreateTransactional();
        void Destroy(GraveGraph graph);
        EsentContext GetEsentContext();
        void Destroy(EsentContext context);
    }
}