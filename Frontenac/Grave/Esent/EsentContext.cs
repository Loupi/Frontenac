using System;
using Frontenac.Infrastructure.Serializers;
using Microsoft.Isam.Esent.Interop;

namespace Frontenac.Grave.Esent
{
    public class EsentContext : IDisposable
    {
        private readonly EsentInstance _instance;
        private readonly IContentSerializer _contentSerializer;
        private readonly Session _session;
        private readonly JET_DBID _dbid;

        public EsentContext(EsentInstance instance, string databaseName, IContentSerializer contentSerializer)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (contentSerializer == null)
                throw new ArgumentNullException(nameof(contentSerializer));
            if (string.IsNullOrEmpty(databaseName))
                throw new ArgumentNullException(nameof(databaseName));

            _instance = instance;
            _session = _instance.CreateSession();
            _contentSerializer = contentSerializer;

            OpenDatabase(databaseName, out _dbid);
        }

        #region IDisposable

        private bool _disposed;

        ~EsentContext()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                CloseDatabase();
            }

            _disposed = true;
        }

        #endregion

        public EsentVertexTable VertexTable { get; private set; }
        public EsentEdgesTable EdgesTable { get; private set; }
        public EsentConfigTable ConfigTable { get; private set; }

        public EsentVertexTable GetVerticesCursor()
        {
            var cursor = new EsentVertexTable(_session, _contentSerializer);
            cursor.Open(_dbid);
            return cursor;
        }

        public EsentEdgesTable GetEdgesCursor()
        {
            var cursor = new EsentEdgesTable(_session, _contentSerializer);
            cursor.Open(_dbid);
            return cursor;
        }

        private void OpenDatabase(string databaseName, out JET_DBID dbid)
        {
            VertexTable = new EsentVertexTable(_session, _contentSerializer);
            EdgesTable = new EsentEdgesTable(_session, _contentSerializer);
            ConfigTable = new EsentConfigTable(_session, _contentSerializer);

            Api.JetAttachDatabase(_session, databaseName, AttachDatabaseGrbit.DeleteCorruptIndexes);
            Api.JetOpenDatabase(_session, databaseName, null, out dbid, OpenDatabaseGrbit.None);

            VertexTable.Open(_dbid);
            EdgesTable.Open(_dbid);
            ConfigTable.Open(_dbid);
        }

        private void CloseDatabase()
        {
            VertexTable.Close();
            EdgesTable.Close();
            ConfigTable.Close();
            _session.Dispose();
        }

        public EsentTransaction CreateTransaction()
        {
            return _instance.CreateTransaction(_session);
        }
    }
}