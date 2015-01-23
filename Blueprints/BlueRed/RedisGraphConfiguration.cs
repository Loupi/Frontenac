using Frontenac.BlueRed.Properties;
using Frontenac.Infrastructure;

namespace Frontenac.BlueRed
{
    public class RedisGraphConfiguration : IGraphConfiguration
    {
        public string GetPath()
        {
            return Settings.Default.InstanceName;
        }
    }
}