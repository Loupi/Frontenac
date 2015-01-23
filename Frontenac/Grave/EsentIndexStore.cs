using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using Frontenac.Grave.Esent;
using Frontenac.Infrastructure.Indexing;

namespace Frontenac.Grave
{
    public class EsentIndexStore : IIndexStore
    {
        public const int ConfigVertexId = 1;
        private readonly ReaderWriterLockSlim _indicesLock = new ReaderWriterLockSlim();
        private readonly EsentContext _context;

        public EsentIndexStore(EsentContext context)
        {
            Contract.Requires(context != null);
            _context = context;
        }

        public void Load()
        {
            if (!_context.ConfigTable.SetCursor(ConfigVertexId))
                _context.ConfigTable.AddRow();
        }

        public void Create(string indexName, string indexColumn, List<string> indices)
        {
            _indicesLock.EnterWriteLock();
            try
            {
                if (indices.Contains(indexName)) return;
                indices.Add(indexName);
                _context.ConfigTable.WriteCell(ConfigVertexId, indexColumn, indices);
            }
            finally
            {
                _indicesLock.ExitWriteLock();
            }
        }

        public List<string> Get(string indexType)
        {
            return _context.ConfigTable.ReadCell(ConfigVertexId, indexType) as List<string> ?? new List<string>();
        }

        public long Delete(IndexingService indexingService, string indexName, string indexColumn, Type indexType, List<string> indices, bool isUserIndex)
        {
            long result;

            _indicesLock.EnterWriteLock();
            try
            {
                if (indices.Contains(indexName))
                {
                    indices.Remove(indexName);
                    _context.ConfigTable.WriteCell(ConfigVertexId, indexColumn, indices);
                    result = indexingService.DeleteIndex(indexType, indexName, isUserIndex);
                }
                else
                    result = -1;
            }
            finally
            {
                _indicesLock.ExitWriteLock();
            }

            return result;
        }

        #region IDisposable

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~EsentIndexStore()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                _indicesLock.Dispose();

            _disposed = true;
        }

        #endregion
    }
}