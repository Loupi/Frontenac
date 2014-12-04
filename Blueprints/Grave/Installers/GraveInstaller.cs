using System.IO;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Frontenac.Blueprints;
using Frontenac.Grave.Esent;
using Frontenac.Grave.Properties;
using Frontenac.Infrastructure.Installers;
using Frontenac.Infrastructure.Serializers;

namespace Frontenac.Grave.Installers
{
    public class GraveInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<EsentInstance>()
                         .DependsOn(Dependency.OnConfigValue("instanceName", Settings.Default.InstanceName)),

                Component.For<IContentSerializer>()
                         .ImplementedBy<JsonContentSerializer>(),

                Component.For<IGraveGraphFactory>()
                         .AsFactory(),

                Component.For<IGraph>()
                         .Forward<IKeyIndexableGraph, IIndexableGraph, GraveGraph>()
                         .ImplementedBy<GraveGraph>()
                         .LifestyleTransient(),

                Component.For<IGraph>()
                         .Forward<IKeyIndexableGraph, IIndexableGraph, ITransactionalGraph, GraveTransactionalGraph>()
                         .ImplementedBy<GraveTransactionalGraph>()
                         .DependsOn(Dependency.OnComponent("indexCollectionFactory", "TransactionalIndexCollectionFactory"))
                         .DependsOn(Dependency.OnComponent("indexingServiceFactory", "TransactionalIndexingServiceFactory"))
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
            var databaseName = EsentInstance.CleanDatabaseName(Settings.Default.InstanceName);
            var databasePath = Path.GetDirectoryName(Settings.Default.InstanceName);
            if (string.IsNullOrWhiteSpace(databasePath))
                databasePath = Path.GetFileNameWithoutExtension(databaseName);
            return databasePath;
        }
    }
}