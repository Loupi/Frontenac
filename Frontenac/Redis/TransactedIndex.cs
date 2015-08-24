using System;
using System.Diagnostics.Contracts;
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
            Contract.Requires(!string.IsNullOrWhiteSpace(indexName));
            Contract.Requires(graph != null);
            Contract.Requires(genBasedIndex != null);
            Contract.Requires(indexingService != null);
            Contract.Requires(transactionManager != null);

            _transactionManager = transactionManager;
        }

        public override void Put(string key, object value, IElement element)
        {
            base.Put(key, value, element);

            _transactionManager.End();
        }
    }
}