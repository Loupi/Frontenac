﻿using System;
using System.Text;
using Grave.Esent.Serializers;
using Microsoft.Isam.Esent.Interop;
using Microsoft.Isam.Esent.Interop.Vista;

namespace Grave.Esent
{
    public class EsentEdgesTable : EsentTable
    {
        const string LabelColumnName = "$label";
        const string InColumnName = "$in";
        const string OutColumnName = "$out";

        public EsentEdgesTable(Session session, IContentSerializer contentSerializer)
            : base(session, "Edges", contentSerializer)
        {

        }

        protected override JET_TABLECREATE GetTableDefinition()
        {
            var idIndexKey = string.Format("+{0}\0\0", IdColumnName);
            var labelIndexKey = string.Format("+{0}\0\0", LabelColumnName);
            var inIndexKey = string.Format("+{0}\0\0", InColumnName);
            var outIndexKey = string.Format("+{0}\0\0", OutColumnName);
            var labelInIndexKey = string.Format("+{0}\0+{1}\0\0", LabelColumnName, InColumnName);
            var labelOutIndexKey = string.Format("+{0}\0+{1}\0\0", LabelColumnName, OutColumnName);

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
                                grbit = ColumndefGrbit.ColumnAutoincrement | 
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

        public int AddEdge(string label, int vertexIn, int vertexOut)
        {
            int? result;
            using (var update = new Update(Session, TableId, JET_prep.Insert))
            {
                result = Api.RetrieveColumnAsInt32(Session, TableId, Columns[IdColumnName], RetrieveColumnGrbit.RetrieveCopy);
                Api.SetColumn(Session, TableId, Columns[LabelColumnName], label, Encoding.Unicode);
                Api.SetColumn(Session, TableId, Columns[InColumnName], vertexIn);
                Api.SetColumn(Session, TableId, Columns[OutColumnName], vertexOut);
                update.Save();
            }
            return result ?? 0;
        }

        public Tuple<string, int, int> TryGetEdge(int id)
        {
            Tuple<string, int, int> result = null;

            if (SetCursor(id))
                result = GetEdgeData();

            return result;
        }

        public Tuple<string, int, int> GetEdgeData()
        {
            Tuple<string, int, int> result = null;
            var label = Api.RetrieveColumnAsString(Session, TableId, Columns[LabelColumnName], Encoding.Unicode);
            var vertexIn = Api.RetrieveColumnAsInt32(Session, TableId, Columns[InColumnName]);
            var vertexOut = Api.RetrieveColumnAsInt32(Session, TableId, Columns[OutColumnName]);
            if (vertexIn.HasValue && vertexOut.HasValue)
                result = new Tuple<string, int, int>(label, vertexIn.Value, vertexOut.Value);
            return result;
        }
    }
}