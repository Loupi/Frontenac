using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Frontenac.Blueprints;

namespace Frontenac.Infrastructure.Indexing
{
    [ContractClassFor(typeof(IIndexCollection))]
    public abstract class IndexCollectionContract : IIndexCollection
    {
        public void CreateIndex(string indexName, Parameter[] parameters)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(indexName));
        }

        public long DropIndex(string indexName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(indexName));
            return 0;
        }

        public IEnumerable<string> GetIndices()
        {
            Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);
            return null;
        }

        public bool HasIndex(string indexName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(indexName));
            return true;
        }

        public long Set(long id, string indexName, string key, object value)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(indexName));
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
            return 0;
        }

        public void WaitForGeneration(long generation)
        {
            
        }

        public IEnumerable<long> Get(string term, object value, int hitsLimit = 1000)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(term));
            Contract.Requires(hitsLimit >= 0);
            Contract.Ensures(Contract.Result<IEnumerable<long>>() != null);
            return null;
        }

        public IEnumerable<long> Get(string indexName, string key, object value, int hitsLimit = 1000)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(indexName));
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
            Contract.Requires(hitsLimit >= 0);
            Contract.Ensures(Contract.Result<IEnumerable<long>>() != null);
            return null;
        }

        public long DeleteDocuments(long id)
        {
            return 0;
        }

        public long DeleteIndex(string indexName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(indexName));
            return 0;
        }

        public void Commit()
        {

        }

        public void Rollback()
        {
            
        }
    }
}
