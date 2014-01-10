﻿using System;
using System.Diagnostics.Contracts;
using Frontenac.Grave.Esent.Serializers;
using Microsoft.Isam.Esent.Interop;

namespace Frontenac.Grave.Esent
{
    public abstract class EsentContextBase
    {
        protected readonly IContentSerializer ContentSerializer;
        protected JET_DBID Dbid;

        protected EsentContextBase(Session session, string databaseName, IContentSerializer contentSerializer)
        {
            Contract.Requires(session != null);
            Contract.Requires(contentSerializer != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(databaseName));

            Session = session;
            DatabaseName = CleanDatabaseName(databaseName);
            ContentSerializer = contentSerializer;

            VertexTable = new EsentVertexTable(session, contentSerializer);
            EdgesTable = new EsentEdgesTable(session, contentSerializer);
        }

        #region IDisposable

        private bool _disposed;

        ~EsentContextBase()
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

        public string DatabaseName { get; protected set; }
        public EsentVertexTable VertexTable { get; private set; }
        public EsentEdgesTable EdgesTable { get; private set; }
        public Session Session { get; private set; }

        private static string CleanDatabaseName(string databaseName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(databaseName));
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

            databaseName = databaseName.Trim();

            if (databaseName.EndsWith("."))
                databaseName = databaseName.Remove(databaseName.Length - 1);

            if (!databaseName.ToLower().EndsWith(".db"))
                databaseName = string.Concat(databaseName, ".db");

            return databaseName;
        }

        public static Instance CreateInstance(string instanceName, string logsDirectory, string tempDirectory,
                                              string systemDirectory)
        {
            var instance = new Instance(instanceName);
            instance.Parameters.CircularLog = false;
            instance.Parameters.Recovery = true;
            instance.Parameters.LogBuffers = 8*1024;
            instance.Parameters.LogFileSize = 16*1024;
            instance.Parameters.SystemDirectory = systemDirectory;
            instance.Parameters.TempDirectory = tempDirectory;
            instance.Parameters.LogFileDirectory = logsDirectory;
            instance.Parameters.CreatePathIfNotExist = true;
            instance.Parameters.MaxVerPages = 16 * 1024; //1024 = 64Mb
            instance.Parameters.MaxOpenTables = int.MaxValue;
            instance.Parameters.MaxCursors = int.MaxValue;
            SystemParameters.CacheSizeMin = 16 * 1024;
            instance.Init();
            return instance;
        }

        public EsentVertexTable GetVerticesCursor()
        {
            Contract.Ensures(Contract.Result<EsentVertexTable>() != null);

            var cursor = new EsentVertexTable(Session, ContentSerializer);
            cursor.Open(Dbid);
            return cursor;
        }

        public EsentEdgesTable GetEdgesCursor()
        {
            Contract.Ensures(Contract.Result<EsentEdgesTable>() != null);

            var cursor = new EsentEdgesTable(Session, ContentSerializer);
            cursor.Open(Dbid);
            return cursor;
        }

        protected void OpenDatabase()
        {
            Api.JetAttachDatabase(Session, DatabaseName, AttachDatabaseGrbit.DeleteCorruptIndexes);
            Api.JetOpenDatabase(Session, DatabaseName, null, out Dbid, OpenDatabaseGrbit.None);
            VertexTable.Open(Dbid);
            EdgesTable.Open(Dbid);
        }

        protected virtual void CloseDatabase()
        {
            VertexTable.Close();
            EdgesTable.Close();
            Session.Dispose();
        }
    }
}