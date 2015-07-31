using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Frontenac.Blueprints;

namespace Frontenac.Infrastructure.Indexing
{
    [ContractClass(typeof(IndexStoreContract))]
    public interface IIndexStore
    {
        void LoadIndices();
        void CreateIndex(string indexName, string indexColumn, Parameter[] parameters);
        List<string> GetIndices(string indexType);
        long DeleteIndex(IndexingService indexingService, string indexName, string indexColumn, Type indexType, bool isUserIndex);
        void DropIndex(string indexName);
    }
}