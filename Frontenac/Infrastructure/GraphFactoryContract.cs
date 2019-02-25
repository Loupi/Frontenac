using System;
using System.Diagnostics.Contracts;
using Frontenac.Blueprints;

namespace Frontenac.Infrastructure
{
    [ContractClassFor(typeof(IGraphFactory))]
    public abstract class GraphFactoryContract : IGraphFactory
    {
        public TGraph Create<TGraph>() where TGraph : IGraph
        {
            return default(TGraph);
        }

        public void Destroy(IGraph graph)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
        }

        public void Dispose()
        {

        }
    }
}