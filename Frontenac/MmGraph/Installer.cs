using System;
using Frontenac.Blueprints;
using Frontenac.Infrastructure;
using Frontenac.Infrastructure.Indexing;
using Frontenac.Infrastructure.Indexing.Indexers;
using Frontenac.Infrastructure.Serializers;

namespace MmGraph
{
    public static class Installer
    {
        public static void SetupMmGraph(this IContainer container)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            
            container.Register(LifeStyle.Singleton, typeof(ObjectIndexer), typeof(Indexer));
            container.Register(LifeStyle.Singleton, typeof(DefaultIndexerFactory), typeof(IIndexerFactory));
            container.Register(LifeStyle.Singleton, typeof(DefaultGraphFactory), typeof(IGraphFactory));

            container.Register(LifeStyle.Singleton, typeof(JsonContentSerializer), typeof(IContentSerializer));
            
            container.Register(LifeStyle.Singleton, typeof(GraphConfiguration), typeof(IGraphConfiguration));

            container.Register(LifeStyle.Transient, typeof(Graph),
                                                    typeof(IGraph));
        }


    }
}
