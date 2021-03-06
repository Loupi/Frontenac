﻿using System;
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
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            container.Register(LifeStyle.Singleton, typeof(ObjectIndexer), typeof(Indexer));
            container.Register(LifeStyle.Singleton, typeof(DefaultIndexerFactory), typeof(IIndexerFactory));
            container.Register(LifeStyle.Singleton, typeof(DefaultGraphFactory), typeof(IGraphFactory));

            container.Register(LifeStyle.Singleton, typeof(JsonContentSerializer), typeof(IContentSerializer));

            container.Register(LifeStyle.Transient, typeof(ElasticSearchService), typeof(IndexingService));

            container.Register(ConnectionMultiplexer.Connect("localhost:6379"), typeof(ConnectionMultiplexer));

            container.Register(LifeStyle.Singleton, typeof(RedisGraphConfiguration), typeof(IGraphConfiguration));

            container.Register(LifeStyle.Transient, typeof(RedisGraph), typeof(IGraph), typeof(IKeyIndexableGraph), typeof(IIndexableGraph));
        }
    }
}
