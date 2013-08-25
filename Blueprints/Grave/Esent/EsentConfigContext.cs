using System.IO;
using Grave.Esent.Serializers;
using Microsoft.Isam.Esent.Interop;

namespace Grave.Esent
{
    public class EsentConfigContext : EsentContext
    {
        public EsentConfigTable ConfigTable { get; private set; }

        public EsentConfigContext(Instance instance, Session session, string databaseName, IContentSerializer contentSerializer) : 
            base(instance, session, databaseName, contentSerializer)
        {

        }

        protected override void OpenDatabase()
        {
            ConfigTable = new EsentConfigTable(Session, ContentSerializer);
            CreateDatabase();
            base.OpenDatabase();
        }

        protected override void CloseDatabase()
        {
            base.CloseDatabase();
            ConfigTable.Close();
        }

        protected override JET_DBID OpenDatabase(OpenDatabaseGrbit openFlags)
        {
            var dbid = base.OpenDatabase(openFlags);
            ConfigTable.Open(dbid);
            return dbid;
        }

        void CreateDatabase()
        {
            if (File.Exists(DatabaseName)) return;

            JET_DBID dbid;
            var path = Path.GetDirectoryName(DatabaseName);
            if (path != null)
                Directory.CreateDirectory(path);

            Api.JetCreateDatabase(Session, DatabaseName, null, out dbid, CreateDatabaseGrbit.None);
            VertexTable.Create(dbid);
            EdgesTable.Create(dbid);
            ConfigTable.Create(dbid);
        }
    }
}
