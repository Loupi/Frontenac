using System.Diagnostics.Contracts;
using Frontenac.Blueprints;
using Frontenac.ElasticSearch;
using Frontenac.Infrastructure;
using Frontenac.Infrastructure.Indexing;
using Frontenac.Infrastructure.Indexing.Indexers;
using Frontenac.Infrastructure.Serializers;
using StackExchange.Redis;

namespace Frontenac.Redis
{
    public static class Installer
    {
        public static void SetupRedis(this IContainer container)
        {
            Contract.Requires(container != null);

            var config = new ConfigurationOptions
            {
                ConnectTimeout = 100000, 
                ResponseTimeout = 100000, 
                ConnectRetry = 3, 
                SyncTimeout = 100000,
                AbortOnConnectFail = false
            };

            var endpoints = RedisGraph.GetConnectionString().Split(';');
            foreach (var endpoint in endpoints)
                config.EndPoints.Add(endpoint);

            container.Register(LifeStyle.Singleton, typeof(ObjectIndexer), typeof(Indexer));
            container.Register(LifeStyle.Singleton, typeof(DefaultIndexerFactory), typeof(IIndexerFactory));
            container.Register(LifeStyle.Singleton, typeof(DefaultGraphFactory), typeof(IGraphFactory));

            container.Register(LifeStyle.Singleton, typeof(JsonContentSerializer), typeof(IContentSerializer));

            container.Register(LifeStyle.Transient, typeof(ElasticSearchService), typeof(IndexingService));

            container.Register(ConnectionMultiplexer.Connect(config), typeof(ConnectionMultiplexer));

            container.Register(LifeStyle.Singleton, typeof(RedisGraphConfiguration), typeof(IGraphConfiguration));

            container.Register(LifeStyle.Transient, typeof(RedisGraph), 
                                                    typeof(IGraph), 
                                                    typeof(IKeyIndexableGraph), 
                                                    typeof(IIndexableGraph));

            container.Register(LifeStyle.Transient, typeof(RedisTransactionalGraph), 
                                                    typeof(IGraph), 
                                                    typeof(IKeyIndexableGraph), 
                                                    typeof(IIndexableGraph), 
                                                    typeof(ITransactionalGraph), 
                                                    typeof(IThreadedTransactionalGraph));
        }

        
    }
}
