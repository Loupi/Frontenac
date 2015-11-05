using System.Diagnostics;
using System.Diagnostics.Contracts;
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
            Contract.Requires(factory != null);
            Contract.Requires(serializer != null);
            Contract.Requires(multiplexer != null);
            Contract.Requires(indexingService != null);
            Contract.Requires(configuration != null);

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