using System;
using MmGraph.Database.Records;

namespace MmGraph.Database.Repositories
{
    public class EdgeRepository : MemoryMappedRepository<EdgeRecord>
    {
        private const string StoreName = "edge.store";
        private const int RecordSize = 33;
        private const int OffsetOutNodeId = 1;
        private const int OffsetInNodeId = 5;
        private const int OffsetEdgeTypeId = 9;
        private const int OffsetOutNodePreviousEdgeId = 13;
        private const int OffsetOutNodeNextEdgeId = 17;
        private const int OffsetInNodePreviousEdgeId = 21;
        private const int OffsetInNodeNextEdgeId = 25;
        private const int OffsetNextPropertyId = 29;

        public EdgeRepository() 
            : base(StoreName, RecordSize)
        {
        }

        public unsafe void DeleteList(int id, bool useIn = false)
        {
            var nextEdgeId = id;
            while (nextEdgeId != -1)
            {
                var pointer = GetPointer(nextEdgeId);
                if (*pointer == 0)
                    throw new InvalidOperationException(
                        $"Cannot delete next entry '{nextEdgeId}' of '{id}' because it is not in use.");
                
                var edgeRecord = Read(nextEdgeId, pointer);
                *pointer = 0;
                IdGenerator.FreeId(nextEdgeId);

                nextEdgeId = edgeRecord.OutNodeId == id 
                    ? edgeRecord.OutNodeNextEdgeId 
                    : edgeRecord.InNodeNextEdgeId;
            }
        }

        protected override unsafe EdgeRecord Read(int id, byte* pointer)
        {
            var record = new EdgeRecord
            {
                Id = id,
                IsDirected = (*pointer & 0x2) == 0x2,
                OutNodeId = *(int*)(pointer + OffsetOutNodeId),
                InNodeId = *(int*)(pointer + OffsetInNodeId),
                LabelId = *(int*)(pointer + OffsetEdgeTypeId),
                OutNodePreviousEdgeId = *(int*)(pointer + OffsetOutNodePreviousEdgeId),
                OutNodeNextEdgeId = *(int*)(pointer + OffsetOutNodeNextEdgeId),
                InNodePreviousEdgeId = *(int*)(pointer + OffsetInNodePreviousEdgeId),
                InNodeNextEdgeId = *(int*)(pointer + OffsetInNodeNextEdgeId),
                NextPropertyId = *(int*)(pointer + OffsetNextPropertyId)
            };
            return record;
        }

        protected override unsafe void Write(int id, byte* pointer, EdgeRecord record)
        {
            *pointer = (byte)(1 | (record.IsDirected ? 0x02 : 0));
            *(int*)(pointer + OffsetOutNodeId) = record.OutNodeId;
            *(int*)(pointer + OffsetInNodeId) = record.InNodeId;
            *(int*)(pointer + OffsetEdgeTypeId) = record.LabelId;
            *(int*)(pointer + OffsetOutNodePreviousEdgeId) = record.OutNodePreviousEdgeId;
            *(int*)(pointer + OffsetOutNodeNextEdgeId) = record.OutNodeNextEdgeId;
            *(int*)(pointer + OffsetInNodePreviousEdgeId) = record.InNodePreviousEdgeId;
            *(int*)(pointer + OffsetInNodeNextEdgeId) = record.InNodeNextEdgeId;
            *(int*)(pointer + OffsetNextPropertyId) = record.NextPropertyId;
        }
    }
}