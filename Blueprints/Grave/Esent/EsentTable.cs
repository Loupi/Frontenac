using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Grave.Esent.Serializers;
using Microsoft.Isam.Esent.Interop;

namespace Grave.Esent
{
    public abstract class EsentTable
    {
        protected const string IdColumnName = "$id";

        protected JET_TABLEID TableId;
        protected IDictionary<string, JET_COLUMNID> Columns;
        protected readonly Session Session;
        readonly IContentSerializer _contentSerializer;
        private string _tableName;

        protected EsentTable(Session session, string name, IContentSerializer contentSerializer)
        {
            Contract.Requires(session != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(contentSerializer != null);

            Session = session;
            TableName = name;
            _contentSerializer = contentSerializer;
        }

        protected virtual JET_TABLECREATE GetTableDefinition()
        {
            Contract.Ensures(Contract.Result<JET_TABLECREATE>() != null);

            var idIndexKey = string.Format("+{0}\0\0", IdColumnName);

            return new JET_TABLECREATE
            {
                szTableName = TableName,
                cColumns = 1,
                rgcolumncreate = new[]
                    {
                        new JET_COLUMNCREATE
                            {
                                szColumnName = IdColumnName,
                                coltyp = JET_coltyp.Long,
                                grbit = ColumndefGrbit.ColumnAutoincrement | 
                                        ColumndefGrbit.ColumnFixed |
                                        ColumndefGrbit.ColumnNotNULL
                            }
                    },
                cIndexes = 1,
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
                            }
                    }
            };
        }

        public string TableName
        {
            get
            {
                Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));
                return _tableName;
            }
            protected set
            {
                Contract.Requires(!string.IsNullOrWhiteSpace(value));
                _tableName = value;
            }
        }

        public void Create(JET_DBID dbid)
        {
            var tableDefinition = GetTableDefinition();
            Api.JetCreateTableColumnIndex3(Session, dbid, tableDefinition);
            Api.JetCloseTable(Session, tableDefinition.tableid);
        }

        public void Open(JET_DBID dbid)
        {
            Api.JetOpenTable(Session, dbid, TableName, null, 0, OpenTableGrbit.Updatable, out TableId);
            Columns = Api.GetColumnDictionary(Session, TableId);
        }

        public void Close()
        {
            Api.JetCloseTable(Session, TableId);
        }

        public int AddRow()
        {
            int? result;
            using (var update = new Update(Session, TableId, JET_prep.Insert))
            {
                result = Api.RetrieveColumnAsInt32(Session, TableId, Columns[IdColumnName], RetrieveColumnGrbit.RetrieveCopy);
                update.Save();
            }
            return result ?? 0;
        }

        public IEnumerable<string> GetColumnsForRow(int id)
        {
            Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);

            if (!SetCursor(id)) return Enumerable.Empty<string>();
            int nbColumns;
            JET_ENUMCOLUMN[] columnIds;
            JET_PFNREALLOC allocator = (context, pv, cb) => IntPtr.Zero == pv ? Marshal.AllocHGlobal(new IntPtr(cb)) : Marshal.ReAllocHGlobal(pv, new IntPtr(cb));
            Api.JetEnumerateColumns(Session, TableId, 0, null, out nbColumns, out columnIds, allocator, IntPtr.Zero, 0, EnumerateColumnsGrbit.EnumeratePresenceOnly);
            var result = columnIds.Where(t => t.err == JET_wrn.ColumnPresent).Join(Columns, t => t.columnid, t => t.Value, (t, u) => u.Key).ToArray();
            allocator(IntPtr.Zero, columnIds[0].pvData, 0);
            return result;
        }

        public IEnumerable<string> GetColumns()
        {
            Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);

            return Columns.Keys.ToArray();
        }

        public bool SetCursor(int id)
        {
            Api.JetSetCurrentIndex(Session, TableId, IdColumnName);
            Api.MakeKey(Session, TableId, id, MakeKeyGrbit.NewKey);
            return Api.TrySeek(Session, TableId, SeekGrbit.SeekEQ);
        }

        public void DeleteRow(int id)
        {
            if (SetCursor(id))
                Api.JetDelete(Session, TableId);
        }

        public int MoveFirst()
        {
            Api.JetSetCurrentIndex(Session, TableId, IdColumnName);
            Api.MoveBeforeFirst(Session, TableId);
            return MoveNext();
        }

        public int MoveNext()
        {
            var result = 0;
            if (Api.TryMoveNext(Session, TableId))
                result = Api.RetrieveColumnAsInt32(Session, TableId, Columns[IdColumnName]) ?? 0;
            return result;
        }

        public object DeleteCell(int id, string columnName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(columnName));

            object result = null;
            JET_COLUMNID columnId;
            if (!Columns.TryGetValue(columnName, out columnId)) return null;

            if (SetCursor(id))
            {
                result = ReadCellContent(columnId);
                using (var transaction = new Transaction(Session))
                {
                    using (var update = new Update(Session, TableId, JET_prep.Replace))
                    {
                        Api.SetColumn(Session, TableId, columnId, null, Encoding.Unicode);
                        update.Save();
                    }
                    transaction.Commit(CommitTransactionGrbit.None);
                }
            }
            return result;
        }

        object ReadCellContent(JET_COLUMNID columnId)
        {
            object result = null;
            var raw = Api.RetrieveColumn(Session, TableId, columnId);
            if (raw != null && raw.Length > 0)
                result = _contentSerializer.Deserialize(raw);

            return result;
        }

        public object ReadCell(int id, string columnName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(columnName));

            object result = null;
            JET_COLUMNID columnId;
            if (!Columns.TryGetValue(columnName, out columnId))
                return null;
            if (SetCursor(id))
                result = ReadCellContent(columnId);
            return result;
        }

        public void WriteCell(int id, string columnName, object value)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(columnName));

            if (!SetCursor(id)) return;
            CreateColumn(columnName);
            WriteCellContent(Columns[columnName], value);
        }

        protected void WriteCellContent(JET_COLUMNID columnId, object value)
        {
            using (var transaction = new Transaction(Session))
            {
                using (var update = new Update(Session, TableId, JET_prep.Replace))
                {
                    if (value != null)
                    {
                        var data = _contentSerializer.Serialize(value);
                        Api.JetSetColumn(Session, TableId, columnId, data, data.Length, SetColumnGrbit.None, null);
                    }
                    else
                        Api.SetColumn(Session, TableId, columnId, null, Encoding.Unicode, SetColumnGrbit.ZeroLength);
                    update.Save();
                }
                transaction.Commit(CommitTransactionGrbit.None);
            }
        }

        public void CreateColumn(string columnName)
        {
            CreateColumn(columnName, _contentSerializer.IsBinary ? JET_coltyp.LongBinary : JET_coltyp.LongText,
                         ColumndefGrbit.ColumnMaybeNull);
        }

        public bool CreateColumn(string columnName, JET_coltyp colType, ColumndefGrbit grBit)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(columnName));

            JET_COLUMNID columnId;
            if (Columns.TryGetValue(columnName, out columnId)) return false;
            var bookmark = new byte[5];
            int sz;
            Api.JetGetBookmark(Session, TableId, bookmark, bookmark.Length, out sz);

            Api.JetAddColumn(Session, TableId, columnName, new JET_COLUMNDEF
                {
                    coltyp = _contentSerializer.IsBinary ? JET_coltyp.LongBinary : JET_coltyp.LongText,
                    grbit = ColumndefGrbit.ColumnMaybeNull
                }, null, 0, out columnId);
            Columns.Add(columnName, columnId);

            Api.JetGotoBookmark(Session, TableId, bookmark, bookmark.Length);
            return true;
        }

        public long GetApproximateRecordCount(int samples)
        {
            var total = 0L;

            try
            {
                for (var i = 0; i < samples; ++i)
                {
                    var recpos = new JET_RECPOS { centriesTotal = samples, centriesLT = i };
                    Api.JetGotoPosition(Session, TableId, recpos);
                    Api.JetGetRecordPosition(Session, TableId, out recpos);
                    total += recpos.centriesTotal;
                }
            }
            catch (EsentRecordNotFoundException) { }

            var result = total / samples;
            return result;
        }
    }
}
