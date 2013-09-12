using System.Diagnostics.Contracts;
using Castle.Core;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Frontenac.Blueprints;
using Grave.Esent;
using Grave.Esent.Serializers;
using Grave.Properties;
using Microsoft.Isam.Esent.Interop;
using Castle.Facilities.Startable;

namespace Grave.Installers
{
    public class InstanceStarter : IStartable
    {
        readonly Instance _instance;

        public InstanceStarter(Instance instance)
        {
            Contract.Requires(instance != null);

            _instance = instance;
        }

        public void Start()
        {
            
        }

        public void Stop()
        {
            _instance.Close();
        }
    }

    public class GraveInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<Instance>()
                         .UsingFactoryMethod(t => EsentContext.CreateInstance(Settings.Default.InstanceName, 
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
                         .DynamicParameters((k, p) => p["instance"] = (JET_INSTANCE)k.Resolve<Instance>()),

                Component.For<EsentContext>()
                         .LifestyleTransient()
                         .DependsOn(Dependency.OnConfigValue("databaseName", Settings.Default.DatabaseFilePath)),

                Component.For<EsentConfigContext>()
                         .DependsOn(Dependency.OnConfigValue("databaseName", Settings.Default.DatabaseFilePath))
                         .Start(),

                Component.For<IGraveGraphFactory>()
                         .AsFactory(),

                Component.For<IGraph>()
                         .Forward<IKeyIndexableGraph,IIndexableGraph,GraveGraph>()
                         .ImplementedBy<GraveGraph>()
                         .LifestyleTransient()
                );
        }
    }
}
