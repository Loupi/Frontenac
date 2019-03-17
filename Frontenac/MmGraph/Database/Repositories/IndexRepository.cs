using MmGraph.Database.Records;

namespace MmGraph.Database.Repositories
{
    public class IndexRepository : MemoryMappedRepository<IndexRecord>
    {
        private const int RecordSize = 9;
        private const int OffsetPropertyCount = 1;
        private const int OffsetKeyBlockId = 5;

        public IndexRepository(string storeName)
            : base(storeName, RecordSize)
        {
        }

        protected override unsafe IndexRecord Read(int id, byte* pointer)
        {
            var record = new IndexRecord
            {
                Id = id,
                Count = *(int*)(pointer + OffsetPropertyCount),
                KeyBlockId = *(int*)(pointer + OffsetKeyBlockId)
            };
            return record;
        }

        protected override unsafe void Write(int id, byte* pointer, IndexRecord record)
        {
            *(int*)(pointer + OffsetPropertyCount) = record.Count;
            *(int*)(pointer + OffsetKeyBlockId) = record.KeyBlockId;
        }
    }
}