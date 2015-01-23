using System.IO;
using Frontenac.Grave.Esent;
using Frontenac.Grave.Properties;
using Frontenac.Infrastructure;

namespace Frontenac.Grave
{
    public class GraveGraphConfiguration : IGraphConfiguration
    {
        public string GetPath()
        {
            var databaseName = EsentInstance.CleanDatabaseName(Settings.Default.InstanceName);
            var databasePath = Path.GetDirectoryName(Settings.Default.InstanceName);
            if (string.IsNullOrWhiteSpace(databasePath))
                databasePath = Path.GetFileNameWithoutExtension(databaseName);
            return databasePath;
        }
    }
}