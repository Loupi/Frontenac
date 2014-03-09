using System.Diagnostics.Contracts;
using Frontenac.Grave.Esent;

namespace Frontenac.Grave
{
    [ContractClassFor(typeof (IGraveGraphFactory))]
    public abstract class GraveGraphFactoryContract : IGraveGraphFactory
    {
        public GraveGraph Create()
        {
            Contract.Ensures(Contract.Result<GraveGraph>() != null);
            return null;
        }

        public GraveTransactionalGraph CreateTransactional()
        {
            Contract.Ensures(Contract.Result<GraveTransactionalGraph>() != null);
            return null;
        }

        public void Destroy(GraveGraph graph)
        {
            Contract.Requires(graph != null);
        }

        public EsentContext GetEsentContext()
        {
            Contract.Ensures(Contract.Result<EsentContext>() != null);
            return null;
        }

        public void Destroy(EsentContext context)
        {
            Contract.Requires(context != null);
        }

        public abstract void Dispose();
    }
}