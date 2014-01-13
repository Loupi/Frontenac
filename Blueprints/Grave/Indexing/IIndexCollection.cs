using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Grave.Indexing
{
    [ContractClass(typeof(IndexCollectionContract))]
    public interface IIndexCollection
    {
        void CreateIndex(string indexName);
        long DropIndex(string indexName);
        IEnumerable<string> GetIndices();
        bool HasIndex(string indexName);
        long Set(int id, string indexName, string key, object value);
        void WaitForGeneration(long generation);
        IEnumerable<int> Get(string term, object value, int hitsLimit = 1000);
        IEnumerable<int> Get(string indexName, string key, object value, int hitsLimit = 1000);
        long DeleteDocuments(int id);
        long DeleteIndex(string indexName);
        void Commit();
        void Rollback();
    }
}