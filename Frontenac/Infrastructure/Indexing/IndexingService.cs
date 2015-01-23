using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Frontenac.Blueprints;

namespace Frontenac.Infrastructure.Indexing
{
    [ContractClass(typeof (IndexingServiceContract))]
    public abstract class IndexingService : IDisposable
    {
        private const string VertexIndicesColumnName = "VertexIndices";
        private const string EdgeIndicesColumnName = "EdgeIndices";
        private const string UserVertexIndicesColumnName = "UserVertexIndices";
        private const string UserEdgeIndicesColumnName = "UserEdgeIndices";

        internal IIndexStore IndexStore;

        public IIndexCollection Create(string indicesColumnName, Type indexType, bool isUserIndex)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(indicesColumnName));
            return new IndexCollection(indicesColumnName, indexType, isUserIndex, this);
        }

        public IIndexCollection VertexIndices { get; private set; }
        public IIndexCollection EdgeIndices { get; private set; }
        public IIndexCollection UserVertexIndices { get; private set; }
        public IIndexCollection UserEdgeIndices { get; private set; }

        public abstract void Initialize(string databasePath);

        public void LoadFromStore(IIndexStore indexStore)
        {
            Contract.Requires(indexStore != null);
            IndexStore = indexStore;
            IndexStore.Load();

            VertexIndices = Create(VertexIndicesColumnName, typeof (IVertex), false);
            EdgeIndices = Create(EdgeIndicesColumnName, typeof(IEdge), false);
            UserVertexIndices = Create(UserVertexIndicesColumnName, typeof(IVertex), true);
            UserEdgeIndices = Create(UserEdgeIndicesColumnName, typeof(IEdge), true);
        }

        public void CreateIndexOfType(string indexName, string indexColumn, List<string> indices)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(indexName));
            Contract.Requires(!String.IsNullOrWhiteSpace(indexColumn));
            Contract.Requires(indices != null);

            IndexStore.Create(indexName, indexColumn, indices);
        }

        public List<string> GetIndicesOfType(string indexType)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(indexType));

            return IndexStore.Get(indexType);
        }

        public long DropIndexOfType(string indexName, string indexColumn, Type indexType, List<string> indices,
                                    bool isUserIndex)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(indexName));
            Contract.Requires(!String.IsNullOrWhiteSpace(indexColumn));
            Contract.Requires(indices != null);

            return IndexStore.Delete(this, indexName, indexColumn, indexType, indices, isUserIndex);
        }

        public abstract long Set(Type indexType, long id, string indexName, string propertyName, object value,
                                 bool isUserIndex);

        public abstract void WaitForGeneration(long generation);

        public abstract IEnumerable<long> Get(Type indexType, string indexName, string key, object value,
                                             bool isUserIndex, int hitsLimit = 1000);

        public abstract long DeleteDocuments(Type indexType, long id);

        public abstract long DeleteUserDocuments(Type indexType, long id, string key, object value);

        public abstract IEnumerable<long> Query(Type indexType, IEnumerable<QueryElement> query,
                                               int hitsLimit = 1000);

        public abstract long DeleteIndex(Type indexType, string indexName, bool isUserIndex);

        public abstract void Commit();
        public abstract void Prepare();
        public abstract void Rollback();
        
        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~IndexingService()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing && IndexStore != null)
                IndexStore.Dispose();

            _disposed = true;
        }
    }
}