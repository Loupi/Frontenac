using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using Frontenac.Infrastructure.Serializers;
using Microsoft.Isam.Esent.Interop;

namespace Frontenac.Grave.Esent
{
    public class EsentInstance : IDisposable
    {
        private readonly IContentSerializer _contentSerializer;
        private readonly Instance _instance;
        private readonly string _databasePath;
        private readonly string _databaseName;
        private int _transactionNumber = 1;

        public EsentInstance(string instanceName, IContentSerializer contentSerializer)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(instanceName));
            Contract.Ensures(!string.IsNullOrWhiteSpace(_databaseName));

            _databaseName = CleanDatabaseName(instanceName);
            _databasePath = Path.GetDirectoryName(instanceName);
            if (string.IsNullOrWhiteSpace(_databasePath))
                _databasePath = Path.GetFileNameWithoutExtension(_databaseName);

            _contentSerializer = contentSerializer;
            _instance = CreateInstance(Path.GetFileNameWithoutExtension(_databaseName), _databasePath);
            CreateDatabase();
        }

        #region IDisposable

        private bool _disposed;

        ~EsentInstance()
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

            _instance.Close();
            
            _disposed = true;
        }

        #endregion

        static Instance CreateInstance(string databaseName, string databasePath)
        {
            var instance = new Instance(databaseName);
            instance.Parameters.CircularLog = false;
            instance.Parameters.Recovery = true;
            instance.Parameters.LogBuffers = 8 * 1024;
            instance.Parameters.LogFileSize = 16 * 1024;
            instance.Parameters.SystemDirectory = databasePath;
            instance.Parameters.TempDirectory = databasePath;
            instance.Parameters.LogFileDirectory = databasePath;
            instance.Parameters.CreatePathIfNotExist = true;
            instance.Parameters.MaxVerPages = 16 * 1024; //1024 = 64Mb
            //instance.Parameters.MaxOpenTables = int.MaxValue;
            //instance.Parameters.MaxCursors = int.MaxValue;
            SystemParameters.CacheSizeMin = 16 * 1024;
            instance.Init();
            return instance;
        }

        public static string CleanDatabaseName(string databaseName)
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

        private void CreateDatabase()
        {
            var databaseFile = Path.Combine(_databasePath, _databaseName);

            if (File.Exists(databaseFile)) return;

            Directory.CreateDirectory(_databasePath);

            using (var session = CreateSession())
            {
                var vertexTable = new EsentVertexTable(session, _contentSerializer);
                var edgesTable = new EsentEdgesTable(session, _contentSerializer);
                var configTable = new EsentConfigTable(session, _contentSerializer);

                JET_DBID dbid;
                Api.JetCreateDatabase(session, databaseFile, null, out dbid, CreateDatabaseGrbit.None);
                vertexTable.Create(dbid);
                edgesTable.Create(dbid);
                configTable.Create(dbid);
            }
        }

        public EsentContext CreateContext()
        {
            var databaseFile = Path.Combine(_databasePath, _databaseName);
            return new EsentContext(this, databaseFile, _contentSerializer);
        }

        public Session CreateSession()
        {
            return new Session(_instance);
        }

        public EsentTransaction CreateTransaction(Session session)
        {
            var transactionNumber = Interlocked.Increment(ref _transactionNumber);
            return new EsentTransaction(session, transactionNumber);
        }
    }
}
