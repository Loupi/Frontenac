using System.Diagnostics.Contracts;

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

        public void Destroy(GraveGraph graph)
        {
            Contract.Requires(graph != null);
        }

        public abstract void Dispose();
    }
}