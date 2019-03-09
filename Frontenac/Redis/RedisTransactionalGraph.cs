using System;
using System.Diagnostics;
using Frontenac.Blueprints;
using Frontenac.Infrastructure;
using Frontenac.Infrastructure.Indexing;
using Frontenac.Infrastructure.Serializers;
using StackExchange.Redis;

namespace Frontenac.Redis
{
    [DebuggerDisplay("")]
    public class RedisTransactionalGraph : RedisGraph, IThreadedTransactionalGraph
    {
        private readonly IGraphFactory _factory;

        public RedisTransactionalGraph(IGraphFactory factory, 
            IContentSerializer serializer, 
            ConnectionMultiplexer multiplexer, 
            IndexingService indexingService, 
            IGraphConfiguration configuration) 
            : base(factory, serializer, multiplexer, indexingService, configuration)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));

            if (multiplexer == null)
                throw new ArgumentNullException(nameof(multiplexer));

            if (indexingService == null)
                throw new ArgumentNullException(nameof(indexingService));

            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            _factory = factory;
        }

        public void Commit()
        {
            TransactionManager.Commit();
        }

        public void Rollback()
        {
            TransactionManager.Rollback();
        }

        public ITransactionalGraph NewTransaction()
        {
            return _factory.Create<ITransactionalGraph>();
        }
    }
}