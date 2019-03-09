using System;
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
            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentNullException(nameof(indexName));
        }

        public long DropIndex(string indexName)
        {
            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentNullException(nameof(indexName));
            return 0;
        }

        public IEnumerable<string> GetIndices()
        {
            return null;
        }

        public bool HasIndex(string indexName)
        {
            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentNullException(nameof(indexName));
            return true;
        }

        public long Set(long id, string indexName, string key, object value)
        {
            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentNullException(nameof(indexName));
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            return 0;
        }

        public void WaitForGeneration(long generation)
        {
            
        }

        public IEnumerable<long> Get(string term, object value, int hitsLimit = 1000)
        {
            if (string.IsNullOrWhiteSpace(term))
                throw new ArgumentNullException(nameof(term));
            if (hitsLimit < 0)
                throw new ArgumentException("hitsLimit must be greater than or equal to zero");
            return null;
        }

        public IEnumerable<long> Get(string indexName, string key, object value, int hitsLimit = 1000)
        {
            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentNullException(nameof(indexName));

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            if (hitsLimit < 0)
                throw new ArgumentException("hitsLimit must be greater than or equal to zero");
            return null;
        }

        public long DeleteDocuments(long id)
        {
            return 0;
        }

        public long DeleteIndex(string indexName)
        {
            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentNullException(nameof(indexName));
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
