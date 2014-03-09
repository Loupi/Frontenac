using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Grave.Indexing
{
    [ContractClassFor(typeof(IIndexCollection))]
    public abstract class IndexCollectionContract : IIndexCollection
    {
        public void CreateIndex(string indexName)
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

        public long Set(int id, string indexName, string key, object value)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(indexName));
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
            return 0;
        }

        public void WaitForGeneration(long generation)
        {
            
        }

        public IEnumerable<int> Get(string term, object value, int hitsLimit = 1000)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(term));
            Contract.Requires(hitsLimit >= 0);
            Contract.Ensures(Contract.Result<IEnumerable<int>>() != null);
            return null;
        }

        public IEnumerable<int> Get(string indexName, string key, object value, int hitsLimit = 1000)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(indexName));
            Contract.Requires(!string.IsNullOrWhiteSpace(key));
            Contract.Requires(hitsLimit >= 0);
            Contract.Ensures(Contract.Result<IEnumerable<int>>() != null);
            return null;
        }

        public long DeleteDocuments(int id)
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
