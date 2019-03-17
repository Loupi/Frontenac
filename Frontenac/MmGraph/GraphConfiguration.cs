using Frontenac.Infrastructure;
using MmGraph.Properties;

namespace MmGraph
{
    public class GraphConfiguration : IGraphConfiguration
    {
        public string GetPath()
        {
            return Settings.Default.InstanceName;
        }
    }
}