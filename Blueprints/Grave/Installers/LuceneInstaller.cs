using System;
using System.IO;
using Castle.Facilities.TypedFactory;
using Castle.Facilities.Startable;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Frontenac.Grave.Indexing;
using Frontenac.Grave.Indexing.Indexers;
using Frontenac.Grave.Indexing.Lucene;
using Frontenac.Grave.Properties;
using Lucene.Net.Analysis;
using Lucene.Net.Contrib.Management;
using Lucene.Net.Index;

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

                Component.For<Lucene.Net.Store.Directory>()
                         .UsingFactoryMethod(t =>
                             {
                                 var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                                         Settings.Default.IndexPath);
                                 return LuceneIndexingService.CreateMMapDirectory(path);
                             }),

                Component.For<Analyzer>()
                         .ImplementedBy<KeywordAnalyzer>(),

                Component.For<IndexWriter>()
                         .DependsOn(Dependency.OnValue("mfl", IndexWriter.MaxFieldLength.UNLIMITED)),

                Component.For<IIndexCollectionFactory>()
                         .ImplementedBy<IndexCollectionFactory>(),

                Component.For<IIndexCollectionFactory>()
                         .ImplementedBy<TransactionalIndexCollectionFactory>()
                         .Named("TransactionalIndexCollectionFactory"),

                Component.For<IndexingService>()
                         .Forward<LuceneIndexingService>()
                         .ImplementedBy<LuceneIndexingService>(),

                Component.For<IndexingService>()
                         .Forward<LuceneIndexingService>()
                         .Named("TransactionalIndexingService")
                         .DependsOn(Dependency.OnComponent("indexCollectionFactory", "TransactionalIndexCollectionFactory"))
                         .LifestyleTransient(),

                Component.For<NrtManagerReopener>()
                         .UsingFactoryMethod(input =>
                             {
                                 var indexingService = input.Resolve<LuceneIndexingService>();
                                 if (indexingService == null)
                                     throw new NullReferenceException();

                                 return new NrtManagerReopener(indexingService.NrtManager, 
                                     TimeSpan.FromSeconds(LuceneIndexingServiceParameters.Default.MaxStaleSeconds),
                                     TimeSpan.FromMilliseconds(LuceneIndexingServiceParameters.Default.MinStaleMilliseconds),
                                     LuceneIndexingServiceParameters.Default.CloseTimeoutSeconds);
                             })
                         .Start()
                );
        }
    }
}