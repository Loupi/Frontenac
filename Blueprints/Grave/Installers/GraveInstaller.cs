using Castle.Facilities.Startable;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Frontenac.Blueprints;
using Frontenac.Grave.Esent;
using Frontenac.Grave.Esent.Serializers;
using Frontenac.Grave.Properties;
using Microsoft.Isam.Esent.Interop;

namespace Frontenac.Grave.Installers
{
    public class GraveInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<Instance>()
                         .UsingFactoryMethod(t => EsentContextBase.CreateInstance(Settings.Default.InstanceName,
                                                                                  Settings.Default.LogsPath,
                                                                                  Settings.Default.TempPath,
                                                                                  Settings.Default.SystemPath)),
                Component.For<InstanceStarter>()
                         .StartUsingMethod(t => t.Start)
                         .StopUsingMethod(t => t.Stop),

                Component.For<IContentSerializer>()
                         .ImplementedBy<JsonContentSerializer>(),

                Component.For<Session>()
                         .LifestyleTransient()
                         .DynamicParameters((k, p) => p["instance"] = (JET_INSTANCE) k.Resolve<Instance>()),

                Component.For<EsentContext>()
                         .LifestyleTransient()
                         .Named("EsentContext")
                         .DependsOn(Dependency.OnConfigValue("databaseName", Settings.Default.DatabaseFilePath)),

                Component.For<EsentConfigContext>()
                         .DependsOn(Dependency.OnConfigValue("databaseName", Settings.Default.DatabaseFilePath))
                         .Start(),

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
                         .LifestyleTransient()
                );
        }
    }
}