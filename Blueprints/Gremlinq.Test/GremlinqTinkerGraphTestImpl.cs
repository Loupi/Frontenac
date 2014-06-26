using Frontenac.Blueprints;
using Frontenac.Blueprints.Impls.TG;

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