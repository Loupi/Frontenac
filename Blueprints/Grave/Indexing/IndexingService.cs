using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using Frontenac.Blueprints;
using Frontenac.Grave.Esent;

namespace Frontenac.Grave.Indexing
{
    [ContractClass(typeof (IndexingServiceContract))]
    public abstract class IndexingService : IDisposable
    {
        private const string VertexIndicesColumnName = "VertexIndices";
        private const string EdgeIndicesColumnName = "EdgeIndices";
        private const string UserVertexIndicesColumnName = "UserVertexIndices";
        private const string UserEdgeIndicesColumnName = "UserEdgeIndices";
        public const int ConfigVertexId = 1;

        internal readonly ReaderWriterLockSlim IndicesLock = new ReaderWriterLockSlim();
        private readonly EsentContext _context;
        private readonly IIndexCollectionFactory _indexCollectionFactory;

        protected IndexingService(EsentContext context, IIndexCollectionFactory indexCollectionFactory)
        {
            Contract.Requires(context != null);
            Contract.Requires(indexCollectionFactory != null);

            _context = context;
            _indexCollectionFactory = indexCollectionFactory;
            LoadConfig();
        }

        public IIndexCollection VertexIndices { get; private set; }
        public IIndexCollection EdgeIndices { get; private set; }
        public IIndexCollection UserVertexIndices { get; private set; }
        public IIndexCollection UserEdgeIndices { get; private set; }

        [Pure]
        public static bool IsValidIndexType(Type indexType)
        {
            Contract.Requires(indexType != null);
            return indexType.IsAssignableFrom(typeof (GraveVertex)) || indexType.IsAssignableFrom(typeof (GraveEdge));
        }

        private void LoadConfig()
        {
            if (!_context.ConfigTable.SetCursor(ConfigVertexId))
                _context.ConfigTable.AddRow();

            VertexIndices = _indexCollectionFactory.Create(VertexIndicesColumnName, typeof (IVertex), false, this);
            EdgeIndices = _indexCollectionFactory.Create(EdgeIndicesColumnName, typeof(IEdge), false, this);
            UserVertexIndices = _indexCollectionFactory.Create(UserVertexIndicesColumnName, typeof(IVertex), true, this);
            UserEdgeIndices = _indexCollectionFactory.Create(UserEdgeIndicesColumnName, typeof(IEdge), true, this);
        }

        public void CreateIndexOfType(string indexName, string indexColumn, List<string> indices)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(indexName));
            Contract.Requires(!String.IsNullOrWhiteSpace(indexColumn));
            Contract.Requires(indices != null);

            IndicesLock.EnterWriteLock();
            try
            {
                if (indices.Contains(indexName)) return;
                indices.Add(indexName);
                _context.ConfigTable.WriteCell(ConfigVertexId, indexColumn, indices);
            }
            finally
            {
                IndicesLock.ExitWriteLock();
            }
        }

        public List<string> GetIndicesOfType(string indexType)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(indexType));

            return _context.ConfigTable.ReadCell(ConfigVertexId, indexType) as List<string> ?? new List<string>();
        }

        public long DropIndexOfType(string indexName, string indexColumn, Type indexType, List<string> indices,
                                    bool isUserIndex)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(indexName));
            Contract.Requires(!String.IsNullOrWhiteSpace(indexColumn));
            Contract.Requires(IsValidIndexType(indexType));
            Contract.Requires(indices != null);

            long result;

            IndicesLock.EnterWriteLock();
            try
            {
                if (indices.Contains(indexName))
                {
                    indices.Remove(indexName);
                    _context.ConfigTable.WriteCell(ConfigVertexId, indexColumn, indices);
                    result = DeleteIndex(indexType, indexName, isUserIndex);
                }
                else
                    result = -1;
            }
            finally
            {
                IndicesLock.ExitWriteLock();
            }

            return result;
        }

        public abstract long Set(Type indexType, int id, string indexName, string propertyName, object value,
                                 bool isUserIndex);

        public abstract void WaitForGeneration(long generation);

        public abstract IEnumerable<int> Get(Type indexType, string indexName, string key, object value,
                                             bool isUserIndex, int hitsLimit = 1000);

        public abstract long DeleteDocuments(Type indexType, int id);

        public abstract long DeleteUserDocuments(Type indexType, int id, string key, object value);

        public abstract long DeleteIndex(Type indexType, string indexName, bool isUserIndex);

        public abstract IEnumerable<int> Query(Type indexType, IEnumerable<GraveQueryElement> query,
                                               int hitsLimit = 1000);

        public abstract void Commit();
        public abstract void Prepare();
        public abstract void Rollback();

        #region IDisposable

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

            if (disposing)
                IndicesLock.Dispose();

            _disposed = true;
        }

        #endregion
    }
}