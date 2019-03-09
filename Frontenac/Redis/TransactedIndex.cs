using System;
using Frontenac.Blueprints;
using Frontenac.Infrastructure.Indexing;

namespace Frontenac.Redis
{
    public class TransactedIndex : Index
    {
        private readonly TransactionManager _transactionManager;

        public TransactedIndex(string indexName, Type indexType, IGraph graph, IGenerationBasedIndex genBasedIndex, IndexingService indexingService,
                                TransactionManager transactionManager)
            : base(indexName, indexType, graph, genBasedIndex, indexingService)

        {
            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentNullException(nameof(indexName));

            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            if (genBasedIndex == null)
                throw new ArgumentNullException(nameof(genBasedIndex));

            if (indexingService == null)
                throw new ArgumentNullException(nameof(indexingService));

            if (transactionManager == null)
                throw new ArgumentNullException(nameof(transactionManager));

            _transactionManager = transactionManager;
        }

        public override void Put(string key, object value, IElement element)
        {
            base.Put(key, value, element);

            _transactionManager.End();
        }
    }
}