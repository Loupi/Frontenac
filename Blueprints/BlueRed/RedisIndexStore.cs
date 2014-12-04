using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Linq;
using Frontenac.Infrastructure.Indexing;
using StackExchange.Redis;

namespace Frontenac.BlueRed
{
    public class RedisIndexStore : IIndexStore
    {
        private readonly ReaderWriterLockSlim _indicesLock = new ReaderWriterLockSlim();
        readonly ConnectionMultiplexer _multiplexer;

        public RedisIndexStore(ConnectionMultiplexer multiplexer)
        {
            Contract.Requires(multiplexer != null);

            _multiplexer = multiplexer;
        }

        public void Load()
        {
            
        }

        public void Create(string indexName, string indexColumn, List<string> indices)
        {
            _indicesLock.EnterWriteLock();
            try
            {
                if (indices.Contains(indexName)) return;
                indices.Add(indexName);
                var db = _multiplexer.GetDatabase();
                db.SetAdd(indexColumn, indexName);
            }
            finally
            {
                _indicesLock.ExitWriteLock();
            }
        }

        public List<string> Get(string indexType)
        {
            var db = _multiplexer.GetDatabase();
            return db.SetScan(indexType).Select(value => (string) value).ToList();
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
                    var db = _multiplexer.GetDatabase();
                    db.SetRemove(indexColumn, indexName);
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

        ~RedisIndexStore()
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