using System;
using System.Diagnostics.Contracts;
using Frontenac.Blueprints;

namespace Frontenac.Infrastructure
{
    [ContractClass(typeof(GraphFactoryContract))]
    public interface IGraphFactory : IDisposable
    {
        TGraph Create<TGraph>() where TGraph : IGraph;
        void Destroy(IGraph graph);
    }
}