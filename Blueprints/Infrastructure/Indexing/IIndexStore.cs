using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Infrastructure.Indexing
{
    [ContractClass(typeof(IndexStoreContract))]
    public interface IIndexStore : IDisposable
    {
        void Load();
        void Create(string indexName, string indexColumn, List<string> indices);
        List<string> Get(string indexType);
        long Delete(IndexingService indexingService, string indexName, string indexColumn, Type indexType, List<string> indices, bool isUserIndex);
    }
}