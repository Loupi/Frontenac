using System;
using Grave.Esent.Serializers;
using Microsoft.Isam.Esent.Interop;

namespace Grave.Esent
{
    public class  EsentCursor : IDisposable
    {
        public string DatabaseName { get; protected set; }
        public EsentVertexTable VertexTable { get; private set; }
        public EsentEdgesTable EdgesTable { get; private set; }
        public Session Session { get; private set; }

        readonly bool _shouldReleaseSession;

        public EsentCursor(Session session, string databaseName, IContentSerializer contentSerializer, bool shouldReleaseSession)
        {
            if (string.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentException("databaseName");

            if (session == null)
                throw new ArgumentNullException("session");

            Session = session;
            DatabaseName = databaseName;
            _shouldReleaseSession = shouldReleaseSession;
            VertexTable = new EsentVertexTable(session, contentSerializer);
            EdgesTable = new EsentEdgesTable(session, contentSerializer);
        }

        #region IDisposable
        bool _disposed;

        ~EsentCursor()
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
                if(_shouldReleaseSession)
                    Session.Dispose();
            }

            _disposed = true;
        }

        #endregion

        protected virtual JET_DBID OpenDatabase(OpenDatabaseGrbit openFlags)
        {
            JET_DBID dbid;
            Api.JetAttachDatabase(Session, DatabaseName, AttachDatabaseGrbit.DeleteCorruptIndexes);
            Api.JetOpenDatabase(Session, DatabaseName, null, out dbid, OpenDatabaseGrbit.None);
            VertexTable.Open(dbid);
            EdgesTable.Open(dbid);
            return dbid;
        }

        internal protected void Init()
        {
            OpenDatabase();
        }

        protected virtual void OpenDatabase()
        {
            OpenDatabase(OpenDatabaseGrbit.ReadOnly);
        }

        protected virtual void CloseDatabase()
        {
            VertexTable.Close();
            EdgesTable.Close();
        }
    }
}
