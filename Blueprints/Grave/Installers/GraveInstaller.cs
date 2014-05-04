using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Frontenac.Blueprints;
using Frontenac.Grave.Esent;
using Frontenac.Grave.Esent.Serializers;
using Frontenac.Grave.Properties;

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
                         .LifestyleTransient()
                );
        }
    }
}