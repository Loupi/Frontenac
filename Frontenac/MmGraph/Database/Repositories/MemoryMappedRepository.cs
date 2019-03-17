using System;
using System.Collections.Generic;
using MmGraph.Database.Records;
using MmGraph.Database.Win32;

namespace MmGraph.Database.Repositories
{
    public abstract class MemoryMappedRepository<TRecord> : IDisposable
        where TRecord : BaseRecord, new()
    {
        private readonly int _recordsPerBlock;
        private readonly ExpandableMemoryMappedFile _file;

        protected readonly int RawRecordSize;
        protected readonly IdGenerator IdGenerator;

        protected MemoryMappedRepository(string filePath, int rawRecordSize)
        {
            RawRecordSize = rawRecordSize;
            _recordsPerBlock = (ExpandableMemoryMappedFile.AllocationGranularity -
                                   ExpandableMemoryMappedFile.AllocationGranularity % RawRecordSize) / RawRecordSize;
            _file = new ExpandableMemoryMappedFile(filePath, ExpandableMemoryMappedFile.AllocationGranularity);
            IdGenerator = new IdGenerator(filePath + ".ids");
        }

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MemoryMappedRepository()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _file?.Dispose();
                IdGenerator?.Dispose();
            }

            _disposed = true;
        }

        private long Offset(int id)
        {
            if (id <= _recordsPerBlock)
                return (id - 1) * RawRecordSize;

            var indexInBlock = id % _recordsPerBlock;
            return (id - indexInBlock) / _recordsPerBlock * ExpandableMemoryMappedFile.AllocationGranularity +
                   (indexInBlock - 1) * RawRecordSize;
        }

        public IEnumerable<TRecord> Scan()
        {
            var id = 1;
            var offset = Offset(id);
            while (offset < _file.Length)
            {
                if (IsInUse(id))
                    yield return Read(id);
                
                id++;
                offset = Offset(id);
            }
        }
        
        protected unsafe byte* GetPointer(int id)
        {
            var offset = Offset(id);
            if (offset >= _file.Length)
                throw new IndexOutOfRangeException($"Index '{id}' exceeds store length.");

            var pointer = _file.GetPointer(offset);
            return pointer;
        }

        protected unsafe byte* GetOrAllocPointer(int id)
        {
            var offset = Offset(id);
            if (offset >= _file.Length)
                _file.Expand(ExpandableMemoryMappedFile.AllocationGranularity);

            var pointer = _file.GetPointer(offset);
            return pointer;
        }

        public void Flush()
        {
            _file.Flush();
            IdGenerator.Flush();
        }
        
        public unsafe int Create(TRecord record)
        {
            var id = IdGenerator.GenerateId();
            var pointer = GetOrAllocPointer(id);
            if(*pointer != 0)
                throw new InvalidOperationException($"Cannot create entry '{id}' because it is in use.");

            *pointer = 1;
            Write(id, pointer, record);
            return id;
        }

        public unsafe TRecord Read(int id)
        {
            var pointer = GetPointer(id);
            if (*pointer == 0)
                return null;
                //throw new InvalidOperationException($"Cannot read entry '{id}' because it is not in use.");

            return Read(id, pointer);
        }

        public unsafe bool IsInUse(int id)
        {
            var pointer = GetPointer(id);
            return (*pointer & 1) == 1;
        }

        public unsafe int Update(int id, TRecord record)
        {
            var pointer = GetPointer(id);
            if (*pointer == 0)
                throw new InvalidOperationException($"Cannot update entry '{id}' because it is not in use.");
            
            Write(id, pointer, record);
            return id;
        }

        public virtual unsafe void Delete(int id)
        {
            var pointer = GetPointer(id);
            if (*pointer == 0)
                throw new InvalidOperationException($"Cannot delete entry '{id}' because it is not in use.");
            
            *pointer = 0;
            IdGenerator.FreeId(id);
            Delete(id, pointer);
        }

        protected abstract unsafe TRecord Read(int id, byte* pointer);

        protected abstract unsafe void Write(int id, byte* pointer, TRecord record);

        protected virtual unsafe void Delete(int id, byte* pointer) { }
    }
}