using System;
using System.IO;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Frontenac.Grave.Indexing;
using Frontenac.Grave.Indexing.Indexers;
using Frontenac.Grave.Indexing.Lucene;
using Frontenac.Grave.Properties;
using Lucene.Net.Store;

namespace Frontenac.Grave.Installers
{
    public class LuceneInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<Indexer>()
                         .ImplementedBy<ObjectIndexer>()
                         .LifestyleTransient()
                         .DependsOn(Dependency.OnValue("maxDepth", Settings.Default.ObjectIndexerMaxDepth)),

                Component.For<IIndexerFactory>()
                         .AsFactory(),

                Component.For<FSDirectory>()
                         .UsingFactoryMethod(t =>
                             {
                                 var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                                         Settings.Default.IndexPath);
                                 return LuceneIndexingService.CreateMMapDirectory(path);
                             }),

                Component.For<LuceneIndexingServiceParameters>()
                         .Instance(LuceneIndexingServiceParameters.Default),

                Component.For<IndexingService>()
                         .ImplementedBy<LuceneIndexingService>()
                );
        }
    }
}