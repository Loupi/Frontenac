using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grave
{
    [ContractClassFor(typeof(IGraveGraphFactory))]
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
