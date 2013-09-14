using System.Diagnostics.Contracts;
using Grave.Indexing.Indexers;

namespace Grave.Indexing
{
    [ContractClassFor(typeof (IIndexerFactory))]
    public abstract class IndexerFactoryContract : IIndexerFactory
    {
        public Indexer Create(object content, IDocument document)
        {
            Contract.Requires(content != null);
            Contract.Requires(document != null);
            Contract.Ensures(Contract.Result<Indexer>() != null);
            return null;
        }
    }
}