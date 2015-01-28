using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Impls.TG;
using Frontenac.CastleWindsor;
using Frontenac.Grave.Esent;
using Frontenac.Gremlinq;
using Frontenac.Infrastructure;
using Frontenac.Infrastructure.Geo;
using Frontenac.Infrastructure.Indexing;
using Frontenac.Infrastructure.Indexing.Indexers;
using Frontenac.Infrastructure.Serializers;
using Frontenac.Lucene;

namespace Frontenac.Grave
{
    public static class Program
    {
        public static void SetupGrave(this IContainer container)
        {
            Contract.Requires(container != null);

            container.Register(LifeStyle.Singleton, typeof(ObjectIndexer), typeof(Indexer));
            container.Register(LifeStyle.Singleton, typeof(DefaultIndexerFactory), typeof(IIndexerFactory));
            container.Register(LifeStyle.Singleton, typeof(DefaultGraphFactory), typeof(IGraphFactory));

            container.Register(LifeStyle.Singleton, typeof(JsonContentSerializer), typeof(IContentSerializer));

            container.Register(LifeStyle.Transient, typeof(LuceneIndexingService), typeof(IndexingService));

            container.Register(LifeStyle.Transient, typeof(EsentInstance));

            //passer config par factory a la place
            container.Register(LifeStyle.Singleton, typeof(GraveGraphConfiguration), typeof(IGraphConfiguration));

            container.Register(LifeStyle.Transient, typeof(GraveGraph), typeof(IGraph), typeof(IKeyIndexableGraph), typeof(IIndexableGraph));


            container.Register(LifeStyle.Transient, typeof(TinkerGrapĥ), typeof(IGraph));
        }

        private static void Main()
        {
            using (var container = new CastleWindsorContainer())
            {
                container.SetupGrave();

                using (var factory = container.Resolve<IGraphFactory>())
                {
                    var graph = factory.Create<IKeyIndexableGraph>();
                    //TestGraphOfTheGods(graph);  
                    container.Release(factory);
                }
            }

            //GraphFactory.Setup(container);

            /*GremlinqContext.ContextFactory = new StaticGremlinqContextFactory(
                new Dictionary<int, Type> //The types that are allowed to be proxied
                    {
                        {1, typeof (IAgedCharacter)},
                        {2, typeof (IBattle)},
                        {3, typeof (ICharacter)},
                        {4, typeof (IContributor)},
                        {5, typeof (IDemiGod)},
                        {6, typeof (IGod)},
                        {7, typeof (IHuman)},
                        {8, typeof (ILive)},
                        {9, typeof (ILocation)},
                        {10, typeof (IMonster)},
                        {11, typeof (INamedEntity)},
                        {12, typeof (ITitan)},
                        {13, typeof (IWeightedEntity)}
                    });*/
            
            //Test();        
        }
    }
}