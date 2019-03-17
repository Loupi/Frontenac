using System;
using System.Text;
using MmGraph.Database.Records;
using MmGraph.Database.Win32;

namespace MmGraph.Database.Repositories
{
    public class StringRepository : BlockRepository<StringBlockRecord>
    {
        private const string StoreName = "string.store";

        public StringRepository()
            : this(StoreName)
        {
        }

        public StringRepository(string storeName)
            : base(storeName)
        {
        }

        protected override unsafe void Delete(int id, byte* pointer)
        {
            var entry = ReadBlock(id, pointer);

            if (entry.PreviousBlockId != -1)
                throw new InvalidOperationException("Strings can only be deleted from their first block.");
            
            while (entry.NextBlockId != -1)
            {
                pointer = GetPointer(entry.NextBlockId);
                if (*pointer == 0)
                    throw new InvalidOperationException(
                        $"Cannot delete next entry '{entry.NextBlockId}' of '{id}' because it is not in use.");

                id = entry.NextBlockId;
                entry = ReadBlock(id, pointer);
                
                *pointer = 0;
                IdGenerator.FreeId(id);
            }
        }

        protected override unsafe void Write(int id, byte* pointer, StringBlockRecord record)
        {
            const int alignment = 2;
            const int modAlignment = MaxBlockSize % alignment;
            const int blockSize = MaxBlockSize - modAlignment;

            var value = Encoding.UTF8.GetBytes(record.Value);
            var length = value.Length;
            var mod = length % blockSize;
            var fullBlocks = length < blockSize 
                ? 0
                : (length - mod) / blockSize;
            var blocksRequired = fullBlocks + (mod > 0 ? 1 : 0);
            
            record.PreviousBlockId = -1;

            fixed (byte* pc = value)
            {
                var c = pc;
                for (var i = 0; i < fullBlocks; i++)
                {
                    record.NextBlockId = i < blocksRequired
                        ? IdGenerator.GenerateId()
                        : -1;

                    record.Length = blockSize;
                    WriteBlock(pointer, record);
                    Memory.Copy(pointer + OffsetData, c + i * blockSize, blockSize);

                    record.PreviousBlockId = id;
                    id = record.NextBlockId;

                    if (record.NextBlockId != -1)
                        pointer = GetOrAllocPointer(id);
                }

                if (mod <= 0) return;
                
                record.Length = mod;
                record.NextBlockId = -1;
                WriteBlock(pointer, record);
                Memory.Copy(pointer + OffsetData, c + fullBlocks * blockSize, (uint)mod);
            }
        }

        protected override unsafe StringBlockRecord Read(int id, byte* pointer)
        {
            var record = ReadBlock(id, pointer);
            if (record.PreviousBlockId != -1)
                throw new InvalidOperationException("Strings can only be read from their first block.");

            if (record.Length > MaxBlockSize)
                throw new InvalidOperationException($"Length of '{id}' exceeds MaxBlockSize.");

            var firstRecord = record;
            var sb = new StringBuilder(); // TODO: Pass length from property value
            var length = record.Length;
            var charData = new byte[length];
            fixed (byte* pChar = &charData[0])
            {
                Memory.Copy(pChar, pointer + OffsetData, (uint)record.Length);
                sb.Append(Encoding.UTF8.GetString(charData));
            }

            while (record.NextBlockId != -1)
            {
                pointer = GetPointer(record.NextBlockId);
                
                if (*pointer == 0)
                    throw new InvalidOperationException($"Cannot read entry '{record.NextBlockId}' of {id} because it is not in use.");

                id = record.NextBlockId;
                record = ReadBlock(id, pointer);

                if (record.Length > MaxBlockSize)
                    throw new InvalidOperationException($"Length of entry '{id}' exceeds MaxBlockSize.");
                
                length = record.Length;
                charData = new byte[length];

                fixed (byte* pChar = &charData[0])
                {
                    Memory.Copy(pChar, pointer + OffsetData, (uint)record.Length);
                    sb.Append(Encoding.UTF8.GetString(charData));
                }
            }

            firstRecord.Value = sb.ToString();
            return firstRecord;
        }
    }
}
