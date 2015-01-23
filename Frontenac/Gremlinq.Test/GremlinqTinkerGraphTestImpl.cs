using Frontenac.Blueprints.Impls.TG;
using IGraph = Frontenac.Blueprints.IGraph;

namespace Frontenac.Gremlinq.Test
{
    public class GremlinqTinkerGraphTestImpl : TinkerGraphTestImpl
    {
        public override IGraph GenerateGraph(string graphDirectoryName)
        {
            var graph = base.GenerateGraph(graphDirectoryName);
            TinkerGraphFactory.CreateTinkerGraph(graph);
            return graph;
        }
    }
}