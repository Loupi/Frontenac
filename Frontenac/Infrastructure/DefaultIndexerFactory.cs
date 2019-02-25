using System;
using Frontenac.Infrastructure.Indexing;
using Frontenac.Infrastructure.Indexing.Indexers;

namespace Frontenac.Infrastructure
{
    public class DefaultIndexerFactory : IIndexerFactory
    {
        private readonly IContainer _container;

        public DefaultIndexerFactory(IContainer container)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

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