using System;
using System.IO;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Grave.Indexing;
using Grave.Indexing.Indexers;
using Grave.Indexing.Lucene;
using Grave.Properties;
using Lucene.Net.Store;

namespace Grave.Installers
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
                         
                Component.For<Indexer>()
                         .ImplementedBy<TestIndexer>()
                         .LifestyleTransient(),

                Component.For<IIndexerFactory>()
                         .AsFactory(),

                Component.For<FSDirectory>()
                         .UsingFactoryMethod(t =>
                             {
                                 var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.IndexPath);
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
