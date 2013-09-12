using System.IO;
using Grave.Esent.Serializers;
using Microsoft.Isam.Esent.Interop;

namespace Grave.Esent
{
    public class EsentConfigContext : EsentContextBase
    {
        public EsentConfigTable ConfigTable { get; private set; }

        public EsentConfigContext(Session session, string databaseName, IContentSerializer contentSerializer) : 
            base(session, databaseName, contentSerializer)
        {
            ConfigTable = new EsentConfigTable(Session, ContentSerializer);
            CreateDatabase();
            OpenDatabase();
            ConfigTable.Open(Dbid);
        }

        protected override void CloseDatabase()
        {
            base.CloseDatabase();
            ConfigTable.Close();
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
