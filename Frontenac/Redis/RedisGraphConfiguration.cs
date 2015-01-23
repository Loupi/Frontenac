using Frontenac.Redis.Properties;
using Frontenac.Infrastructure;

namespace Frontenac.Redis
{
    public class RedisGraphConfiguration : IGraphConfiguration
    {
        public string GetPath()
        {
            return Settings.Default.InstanceName;
        }
    }
}