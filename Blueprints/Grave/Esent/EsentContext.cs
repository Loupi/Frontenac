using System;
using System.IO;
using Grave.Esent.Serializers;
using Microsoft.Isam.Esent.Interop;

namespace Grave.Esent
{
    public class EsentContext : EsentCursor
    {
        readonly Instance _instance;
        protected readonly IContentSerializer ContentSerializer;

        public EsentContext(Instance instance, Session session, string databaseName, IContentSerializer contentSerializer)
            : base(session, databaseName, contentSerializer, false)
        {
            if(instance == null)
                throw new ArgumentNullException("instance");

            if(contentSerializer == null)
                throw new ArgumentNullException("contentSerializer");

            _instance = instance;
            ContentSerializer = contentSerializer;

            DatabaseName = CleanDatabaseName(DatabaseName);
            Init();
        }

        static string CleanDatabaseName(string databaseName)
        {
            if (string.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentException("databaseName");

            databaseName = databaseName.Trim();

            if (databaseName.EndsWith("."))
                databaseName = databaseName.Remove(databaseName.Length - 1);

            if (!databaseName.ToLower().EndsWith(".db"))
                databaseName = string.Concat(databaseName, ".db");

            if (string.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentException("databaseName");

            if (databaseName == ".db")
                throw new ArgumentException("databaseName");

            return databaseName;
        }

        public static Instance CreateInstance(string instanceName, string logsDirectory, string tempDirectory, string systemDirectory)
        {
            var instance = new Instance(instanceName);
            instance.Parameters.CircularLog = true;
            instance.Parameters.Recovery = true;
            instance.Parameters.LogBuffers = 8 * 1024;
            instance.Parameters.LogFileSize = 16 * 1024;
            instance.Parameters.SystemDirectory = systemDirectory;
            instance.Parameters.TempDirectory = tempDirectory;
            instance.Parameters.LogFileDirectory = logsDirectory;
            instance.Parameters.CreatePathIfNotExist = true;
            SystemParameters.CacheSizeMin = 16 * 1024;
            instance.Init();
            return instance;
        }

        protected override void OpenDatabase()
        {
            OpenDatabase(OpenDatabaseGrbit.None);
        }

        public EsentCursor GetCursor()
        {
            var cursor = new EsentCursor(new Session(_instance), DatabaseName, ContentSerializer, true);
            cursor.Init();
            return cursor;
        }
    }
}
