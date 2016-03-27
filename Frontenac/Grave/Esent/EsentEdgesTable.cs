using System;
using System.Diagnostics.Contracts;
using System.Text;
using Frontenac.Infrastructure.Serializers;
using Microsoft.Isam.Esent.Interop;
using Microsoft.Isam.Esent.Interop.Vista;

namespace Frontenac.Grave.Esent
{
    public class EsentEdgesTable : EsentTable
    {
        private const string LabelColumnName = "$label";
        private const string InColumnName = "$in";
        private const string OutColumnName = "$out";

        public EsentEdgesTable(Session session, IContentSerializer contentSerializer)
            : base(session, "Edges", contentSerializer)
        {
            Contract.Requires(session != null);
            Contract.Requires(contentSerializer != null);
        }

        protected override JET_TABLECREATE GetTableDefinition()
        {
            var idIndexKey = $"+{IdColumnName}\0\0";
            var labelIndexKey = $"+{LabelColumnName}\0\0";
            var inIndexKey = $"+{InColumnName}\0\0";
            var outIndexKey = $"+{OutColumnName}\0\0";
            var labelInIndexKey = $"+{LabelColumnName}\0+{InColumnName}\0\0";
            var labelOutIndexKey = $"+{LabelColumnName}\0+{OutColumnName}\0\0";

            return new JET_TABLECREATE
                {
                    szTableName = TableName,
                    cColumns = 4,
                    rgcolumncreate = new[]
                        {
                            new JET_COLUMNCREATE
                                {
                                    szColumnName = IdColumnName,
                                    coltyp = JET_coltyp.Long,
                                    grbit = //ColumndefGrbit.ColumnAutoincrement |
                                            ColumndefGrbit.ColumnFixed |
                                            ColumndefGrbit.ColumnNotNULL
                                },
                            new JET_COLUMNCREATE
                                {
                                    szColumnName = LabelColumnName,
                                    coltyp = JET_coltyp.Text,
                                    grbit = ColumndefGrbit.ColumnNotNULL
                                },
                            new JET_COLUMNCREATE
                                {
                                    szColumnName = InColumnName,
                                    coltyp = VistaColtyp.UnsignedLong,
                                    grbit = ColumndefGrbit.ColumnFixed |
                                            ColumndefGrbit.ColumnNotNULL
                                },
                            new JET_COLUMNCREATE
                                {
                                    szColumnName = OutColumnName,
                                    coltyp = VistaColtyp.UnsignedLong,
                                    grbit = ColumndefGrbit.ColumnFixed |
                                            ColumndefGrbit.ColumnNotNULL
                                }
                        },
                    cIndexes = 6,
                    rgindexcreate = new[]
                        {
                            new JET_INDEXCREATE
                                {
                                    szIndexName = IdColumnName,
                                    szKey = idIndexKey,
                                    cbKey = idIndexKey.Length,
                                    grbit = CreateIndexGrbit.IndexDisallowNull |
                                            CreateIndexGrbit.IndexPrimary |
                                            CreateIndexGrbit.IndexUnique
                                },
                            new JET_INDEXCREATE
                                {
                                    szIndexName = LabelColumnName,
                                    szKey = labelIndexKey,
                                    cbKey = labelIndexKey.Length,
                                    grbit = CreateIndexGrbit.IndexDisallowNull
                                },
                            new JET_INDEXCREATE
                                {
                                    szIndexName = InColumnName,
                                    szKey = inIndexKey,
                                    cbKey = inIndexKey.Length,
                                    grbit = CreateIndexGrbit.IndexDisallowNull
                                },
                            new JET_INDEXCREATE
                                {
                                    szIndexName = OutColumnName,
                                    szKey = outIndexKey,
                                    cbKey = outIndexKey.Length,
                                    grbit = CreateIndexGrbit.IndexDisallowNull
                                },
                            new JET_INDEXCREATE
                                {
                                    szIndexName = string.Concat(LabelColumnName, InColumnName),
                                    szKey = labelInIndexKey,
                                    cbKey = labelInIndexKey.Length,
                                    grbit = CreateIndexGrbit.IndexDisallowNull
                                },
                            new JET_INDEXCREATE
                                {
                                    szIndexName = string.Concat(LabelColumnName, OutColumnName),
                                    szKey = labelOutIndexKey,
                                    cbKey = labelOutIndexKey.Length,
                                    grbit = CreateIndexGrbit.IndexDisallowNull
                                }
                        }
                };
        }

        public int AddEdge(int id, string label, int vertexIn, int vertexOut)
        {
            using (var update = new Update(Session, TableId, JET_prep.Insert))
            {
                Api.SetColumn(Session, TableId, Columns[IdColumnName], id);
                Api.SetColumn(Session, TableId, Columns[LabelColumnName], label, Encoding.Unicode);
                Api.SetColumn(Session, TableId, Columns[InColumnName], vertexIn);
                Api.SetColumn(Session, TableId, Columns[OutColumnName], vertexOut);
                update.Save();
            }
            return id;
        }

        public bool TryGetEdge(int id, out Tuple<string, int, int> edge)
        {
            var result = SetCursor(id);
            edge = result ? GetEdgeData() : null;
            return result;
        }

        public Tuple<string, int, int> GetEdgeData()
        {
            var label = Api.RetrieveColumnAsString(Session, TableId, Columns[LabelColumnName], Encoding.Unicode);
            var vertexIn = Api.RetrieveColumnAsInt32(Session, TableId, Columns[InColumnName]);
            var vertexOut = Api.RetrieveColumnAsInt32(Session, TableId, Columns[OutColumnName]);
            return vertexIn.HasValue && vertexOut.HasValue
                       ? new Tuple<string, int, int>(label, vertexIn.Value, vertexOut.Value)
                       : null;
        }
    }
}