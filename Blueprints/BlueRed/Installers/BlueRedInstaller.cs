using System.IO;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Frontenac.BlueRed.Properties;
using Frontenac.Blueprints;
using Frontenac.Infrastructure.Installers;
using Frontenac.Infrastructure.Serializers;
using StackExchange.Redis;

namespace Frontenac.BlueRed.Installers
{
    public class BlueRedInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<ConnectionMultiplexer>()
                         .Instance(ConnectionMultiplexer.Connect("localhost:6379")),

                Component.For<IContentSerializer>()
                         .ImplementedBy<JsonContentSerializer>(),

                Component.For<IBlueRedGraphFactory>()
                         .AsFactory(),

                Component.For<IGraph>()
                         .Forward<IKeyIndexableGraph, IIndexableGraph, RedisGraph>()
                         .ImplementedBy<RedisGraph>()
                         .LifestyleTransient(),

                Component.For<IDatabasePathProvider>()
                         .ImplementedBy<DatabasePathProvider>()
                );
        }
    }

    public class DatabasePathProvider : IDatabasePathProvider
    {
        public string GetPath()
        {
            return Settings.Default.InstanceName;
        }
    }
}
