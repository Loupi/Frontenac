using System;
using System.Diagnostics.Contracts;
using Frontenac.Infrastructure.Indexing;
using Frontenac.Infrastructure.Indexing.Indexers;

namespace Frontenac.Infrastructure
{
    public class DefaultIndexerFactory : IIndexerFactory
    {
        private readonly IContainer _container;

        public DefaultIndexerFactory(IContainer container)
        {
            Contract.Requires(container != null);

            _container = container;
        }

        public Indexer Create(Type contentType)
        {
            return _container.Resolve<Indexer>();
        }

        public void Destroy(Indexer indexer)
        {
            _container.Release(indexer);
        }
    }
}