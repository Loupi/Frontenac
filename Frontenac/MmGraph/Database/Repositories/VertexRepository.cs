using MmGraph.Database.Records;

namespace MmGraph.Database.Repositories
{
    public class VertexRepository : MemoryMappedRepository<VertexRecord>
    {
        private const string StoreName = "vertex.store";
        private const int RecordSize = 9;
        private const int OffsetNextEdgeId = 1;
        private const int OffsetNextPropertyId = 5;

        public VertexRepository()
            : base(StoreName, RecordSize)
        {

        }

        protected override unsafe VertexRecord Read(int id, byte* pointer)
        {
            var record = new VertexRecord
            {
                Id = id,
                NextEdgeId = *(int*)(pointer + OffsetNextEdgeId),
                NextPropertyId = *(int*)(pointer + OffsetNextPropertyId)
            };
            return record;
        }

        protected override unsafe void Write(int id, byte* pointer, VertexRecord record)
        {
            *(int*)(pointer + OffsetNextEdgeId) = record.NextEdgeId;
            *(int*)(pointer + OffsetNextPropertyId) = record.NextPropertyId;
        }
    }
}