using System.Diagnostics.Contracts;
using Frontenac.Blueprints;

namespace Frontenac.Infrastructure
{
    [ContractClassFor(typeof(IGraphFactory))]
    public abstract class GraphFactoryContract : IGraphFactory
    {
        public TGraph Create<TGraph>() where TGraph : IGraph
        {
            Contract.Ensures(Contract.Result<IGraph>() != null);
            return default(TGraph);
        }

        public void Destroy(IGraph graph)
        {
            Contract.Requires(graph != null);
        }

        public void Dispose()
        {
            
        }
    }
}