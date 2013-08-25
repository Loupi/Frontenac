using System;
using System.Collections.Generic;
using Frontenac.Blueprints;
using Grave.Esent.Serializers;
using Microsoft.Isam.Esent.Interop;

namespace Grave.Esent
{
    public class EsentVertexTable : EsentTable
    {
        public EsentVertexTable(Session session, IContentSerializer contentSerializer) : base(session, "Vertices", contentSerializer)
        {

        }

        public void AddEdge(int vertexId, Direction direction, string label, int edgeId)
        {
            if(direction == Direction.Both)
                throw new ArgumentException("direction");

            if(string.IsNullOrWhiteSpace(label))
                throw new ArgumentException("label");

            if (!SetCursor(vertexId)) return;

            var labelColumn = GetEdgeColumnName(direction, label);
            CreateEdgeColumn(labelColumn);
            WriteEdgeContent(labelColumn, edgeId, 0);
        }

        void WriteEdgeContent(string labelColumn, int? edgeId, int iTag)
        {
            using (var transaction = new Transaction(Session))
            {
                using (var update = new Update(Session, TableId, JET_prep.Replace))
                {
                    var setInfo = new JET_SETINFO {itagSequence = iTag};
                    byte[] data = null;
                    if(edgeId.HasValue)
                        data = BitConverter.GetBytes(edgeId.Value);
                    Api.JetSetColumn(Session, TableId, Columns[labelColumn], 
                                     data, data == null ? 0 : data.Length, 0, SetColumnGrbit.UniqueMultiValues, setInfo);
                    update.Save();
                }
                transaction.Commit(CommitTransactionGrbit.None);
            }
        }

        public void DeleteEdge(int vertexId, Direction direction, string label, int edgeId)
        {
            if (direction == Direction.Both)
                throw new ArgumentException("direction");

            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentException("label");

            if (!SetCursor(vertexId)) return;

            var labelColumn = GetEdgeColumnName(direction, label);

            if (!SetEdgeCursor(labelColumn, edgeId)) return;

            var retrievecolumn = new JET_RETRIEVECOLUMN { columnid = Columns[labelColumn], grbit = RetrieveColumnGrbit.RetrieveTag };
            Api.JetRetrieveColumns(Session, TableId, new[] { retrievecolumn }, 1);

            WriteEdgeContent(labelColumn, null, retrievecolumn.itagSequence);
        }

        public bool SetEdgeCursor(int vertexId, string label, Direction direction, int edgeId)
        {
            if (direction == Direction.Both)
                throw new ArgumentException("direction");

            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentException("label");

            if (!SetCursor(vertexId)) return false;

            var labelColumn = GetEdgeColumnName(direction, label);

            return SetEdgeCursor(labelColumn, edgeId);
        }

        bool SetEdgeCursor(string edgeLabel, int edgeId)
        {
            Api.JetSetCurrentIndex(Session, TableId, string.Concat(edgeLabel, "Index"));
            Api.MakeKey(Session, TableId, edgeId, MakeKeyGrbit.NewKey);
            return Api.TrySeek(Session, TableId, SeekGrbit.SeekEQ);
        }

        static string GetEdgeColumnName(Direction direction, string label)
        {
            return string.Format("$e_{0}_{1}", direction == Direction.In ? "i" : "o", label);
        }

        void CreateEdgeColumn(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
                throw new ArgumentException("labelName");

            JET_COLUMNID columnId;
            if (Columns.TryGetValue(columnName, out columnId)) return;
            var bookmark = new byte[5];
            int sz;
            Api.JetGetBookmark(Session, TableId, bookmark, bookmark.Length, out sz);

            Api.JetAddColumn(Session, TableId, columnName, new JET_COLUMNDEF
            {
                coltyp = JET_coltyp.Long,
                grbit = ColumndefGrbit.ColumnMultiValued | ColumndefGrbit.ColumnTagged
            }, null, 0, out columnId);
            Columns.Add(columnName, columnId);

            var description = string.Format("+{0}\0\0", columnName);
            Api.JetCreateIndex(Session, TableId, string.Concat(columnName, "Index"), CreateIndexGrbit.None, description, description.Length, 50);

            Api.JetGotoBookmark(Session, TableId, bookmark, bookmark.Length);
        }

        public int CountEdges(int vertexId, string labelName)
        {
            if (!SetCursor(vertexId)) return 0;
            var retrievecolumn = new JET_RETRIEVECOLUMN {columnid = Columns[labelName], itagSequence = 0};
            Api.JetRetrieveColumns(Session, TableId, new[] { retrievecolumn }, 1);
            return retrievecolumn.itagSequence;
        }

        public IEnumerable<int> GetEdges(int vertexId, string labelName)
        {
            var nbEdges = CountEdges(vertexId, labelName);
            var columnId = Columns[labelName];
            for (var itag = 1; itag <= nbEdges; itag++)
            {
                var retinfo = new JET_RETINFO { itagSequence = itag };
                var data = Api.RetrieveColumn(Session, TableId, columnId, RetrieveColumnGrbit.None, retinfo);
                var edgeId = BitConverter.ToInt32(data, 0);
                yield return edgeId;
            }
        }
    }
}
