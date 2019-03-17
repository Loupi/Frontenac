using MmGraph.Database.Records;

namespace MmGraph.Database.Repositories
{
    public abstract class BlockRepository<TRecord> : MemoryMappedRepository<TRecord>
        where TRecord : BlockRecord, new()
    {
        private const int RecordSize = 64;
        protected const int MaxBlockSize = 51;

        protected const int OffsetPreviousBlockId = 1;
        protected const int OffsetNextBlockId = 5;
        protected const int OffsetLength = 9;
        protected const int OffsetData = 13;

        protected BlockRepository(string filePath) 
            : base(filePath, RecordSize)
        {

        }
        protected unsafe TRecord ReadBlock(int id, byte* pointer)
        {
            var record = new TRecord
            {
                Id = id,
                InUse = *pointer,
                PreviousBlockId = *(int*)(pointer + OffsetPreviousBlockId),
                NextBlockId = *(int*)(pointer + OffsetNextBlockId),
                Length = *(int*)(pointer + OffsetLength)
            };
            return record;
        }

        protected unsafe void WriteBlock(byte* pointer, TRecord record)
        {
            *pointer = 1;
            *(int*) (pointer + OffsetPreviousBlockId) = record.PreviousBlockId;
            *(int*) (pointer + OffsetNextBlockId) = record.NextBlockId;
            *(int*) (pointer + OffsetLength) = record.Length;
        }
    }
}
