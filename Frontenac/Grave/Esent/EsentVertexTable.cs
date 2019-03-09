using System;
using System.Collections.Generic;
using Frontenac.Blueprints;
using Frontenac.Infrastructure.Serializers;
using Microsoft.Isam.Esent.Interop;
using Microsoft.Isam.Esent.Interop.Vista;

namespace Frontenac.Grave.Esent
{
    public class EsentVertexTable : EsentTable
    {
        public EsentVertexTable(Session session, IContentSerializer contentSerializer)
            : base(session, "Vertices", contentSerializer)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            if (contentSerializer == null)
                throw new ArgumentNullException(nameof(contentSerializer));
        }

        public void AddEdge(int vertexId, Direction direction, string label, int edgeId, int targetId)
        {
            if (direction == Direction.Both)
                throw new ArgumentException("direction cannot equal Direction.Both");
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentNullException(nameof(label));

            if (!SetCursor(vertexId)) return;

            var labelColumn = GetEdgeColumnName(direction, label);
            CreateEdgeColumn(labelColumn);
            WriteEdgeContent(labelColumn, edgeId, targetId, 0);
        }

        private void WriteEdgeContent(string labelColumn, int? edgeId, int? targetId, int iTag)
        {
            if (string.IsNullOrWhiteSpace(labelColumn))
                throw new ArgumentNullException(nameof(labelColumn));

            using (var transaction = new Transaction(Session))
            {
                using (var update = new Update(Session, TableId, JET_prep.Replace))
                {
                    var setInfo = new JET_SETINFO {itagSequence = iTag};

                    if (edgeId.HasValue && targetId.HasValue)
                    {
// ReSharper disable RedundantCast
                        var key = (ulong) edgeId.Value << 32 | (ulong) (long) targetId.Value;
// ReSharper restore RedundantCast
                        var data = BitConverter.GetBytes(key);
                        Api.JetSetColumn(Session, TableId, Columns[labelColumn], data, data.Length,
                                         SetColumnGrbit.UniqueMultiValues, setInfo);
                    }
                    else
                        Api.JetSetColumn(Session, TableId, Columns[labelColumn], null, 0,
                                         SetColumnGrbit.UniqueMultiValues, setInfo);

                    update.Save();
                }
                transaction.Commit(CommitTransactionGrbit.None);
            }
        }

        public void DeleteEdge(int vertexId, Direction direction, string label, int edgeId, int targetId)
        {
            if (direction == Direction.Both)
                throw new ArgumentException("direction cannot equal Direction.Both");
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentNullException(nameof(label));

            if (!SetCursor(vertexId)) return;

            var labelColumn = GetEdgeColumnName(direction, label);

            if (!SetEdgeCursor(labelColumn, edgeId, targetId)) return;

            var retrievecolumn = new JET_RETRIEVECOLUMN
                {
                    columnid = Columns[labelColumn],
                    grbit = RetrieveColumnGrbit.RetrieveTag |
                            RetrieveColumnGrbit.RetrieveFromIndex
                };

            Api.JetRetrieveColumns(Session, TableId, new[] {retrievecolumn}, 1);

            WriteEdgeContent(labelColumn, null, null, retrievecolumn.itagSequence);
        }

        public bool SetEdgeCursor(int vertexId, string label, Direction direction, int edgeId, int targetId)
        {
            if (direction == Direction.Both)
                throw new ArgumentException("direction cannot equal Direction.Both");
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentNullException(nameof(label));

            if (!SetCursor(vertexId)) return false;

            var labelColumn = GetEdgeColumnName(direction, label);

            return SetEdgeCursor(labelColumn, edgeId, targetId);
        }

        private bool SetEdgeCursor(string edgeLabel, int edgeId, int targetId)
        {
            if (string.IsNullOrWhiteSpace(edgeLabel))
                throw new ArgumentNullException(nameof(edgeLabel));

// ReSharper disable RedundantCast
            var key = (ulong) edgeId << 32 | (ulong) (long) targetId;
// ReSharper restore RedundantCast
            Api.JetSetCurrentIndex(Session, TableId, string.Concat(edgeLabel, "Index"));
            Api.MakeKey(Session, TableId, key, MakeKeyGrbit.NewKey);
            return Api.TrySeek(Session, TableId, SeekGrbit.SeekEQ);
        }

        private static string GetEdgeColumnName(Direction direction, string label)
        {
            return $"$e_{(direction == Direction.In ? "i" : "o")}_{label}";
        }

        private void CreateEdgeColumn(string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName))
                throw new ArgumentNullException(nameof(columnName));

            if (!CreateColumn(columnName, VistaColtyp.LongLong,
                ColumndefGrbit.ColumnMultiValued | ColumndefGrbit.ColumnTagged)) return;
            var description = $"+{columnName}\0\0";
            Api.JetCreateIndex(Session, TableId, string.Concat(columnName, "Index"), CreateIndexGrbit.None,
                description, description.Length, 50);
        }

        public int CountEdges(int vertexId, string labelName)
        {
            if (string.IsNullOrWhiteSpace(labelName))
                throw new ArgumentNullException(nameof(labelName));

            if (!SetCursor(vertexId)) return 0;
            var retrievecolumn = new JET_RETRIEVECOLUMN {columnid = Columns[labelName], itagSequence = 0};
            Api.JetRetrieveColumns(Session, TableId, new[] {retrievecolumn}, 1);
            return retrievecolumn.itagSequence;
        }

        public IEnumerable<long> GetEdges(int vertexId, string labelName)
        {
            if (string.IsNullOrWhiteSpace(labelName))
                throw new ArgumentNullException(nameof(labelName));

            var nbEdges = CountEdges(vertexId, labelName);
            var columnId = Columns[labelName];
            for (var itag = 1; itag <= nbEdges; itag++)
            {
                var retinfo = new JET_RETINFO {itagSequence = itag};
                var data = Api.RetrieveColumn(Session, TableId, columnId, RetrieveColumnGrbit.None, retinfo);
                var key = BitConverter.ToInt64(data, 0);
                yield return key;
            }
        }
    }
}