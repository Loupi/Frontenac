using System.Diagnostics.Contracts;
using System.IO;
using Frontenac.Grave.Esent.Serializers;
using Microsoft.Isam.Esent.Interop;

namespace Frontenac.Grave.Esent
{
    public class EsentConfigContext : EsentContextBase
    {
        public EsentConfigContext(Session session, string databaseName, IContentSerializer contentSerializer) :
            base(session, databaseName, contentSerializer)
        {
            Contract.Requires(session != null);
            Contract.Requires(contentSerializer != null);

            ConfigTable = new EsentConfigTable(Session, ContentSerializer);
            CreateDatabase();
            OpenDatabase();
            ConfigTable.Open(Dbid);
        }

        public EsentConfigTable ConfigTable { get; private set; }

        protected override void CloseDatabase()
        {
            base.CloseDatabase();
            ConfigTable.Close();
        }

        private void CreateDatabase()
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