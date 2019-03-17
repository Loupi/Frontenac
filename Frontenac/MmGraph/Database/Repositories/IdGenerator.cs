using System;
using System.IO;
using MmGraph.Database.Win32;

namespace MmGraph.Database.Repositories
{
    public class IdGenerator : IDisposable
    {
        private const int GrabSize = 64;
        private const int SizeofId = sizeof(int);
        private const int OffsetNextId = 1;
        private const int OffsetNbFreeIds = 5;
        private const int OffsetFreeIds = 12;

        private readonly ExpandableMemoryMappedFile _file;

        private readonly int[] _freeIds = new int[GrabSize];
        private readonly int[] _deletedIds = new int[GrabSize];
        private int _nbDeletedIds;
        private int _nbFreeIds;

        private bool _disposed;

        public int NextId { get; private set; }

        public int NbFreeIds { get; private set; }
        
        public int NbRecycledIds { get; private set; }

        public bool IsDirty { get; private set; }

        public bool IsCorrupted { get; private set; }

        public IdGenerator(string filePath)
        {
            var fileExists = File.Exists(filePath);
            _file = new ExpandableMemoryMappedFile(filePath, ExpandableMemoryMappedFile.AllocationGranularity);
            Initialize(fileExists);
        }

        private unsafe void Initialize(bool fileExists)
        {
            var pointer = _file.GetPointer(0);
            
            if (!fileExists)
            {
                NextId = 1;
                NbFreeIds = 0;

                *(int*) (pointer + OffsetNextId) = NextId;
                *(int*) (pointer + OffsetNbFreeIds) = NbFreeIds;
            }
            else
            {
                IsCorrupted = *pointer == 1;
                NextId = *(int*) (pointer + OffsetNextId);
                NbFreeIds = *(int*) (pointer + OffsetNbFreeIds);
                Read();
            }

            *pointer = 1;
            IsDirty = false;
            _file.Flush();
        }

        public unsafe void Write()
        {
            var pointer = _file.GetPointer(0);

            var oldNbFreeIds = *(int*)(pointer + OffsetNbFreeIds);
            *(int*) (pointer + OffsetNextId) = NextId;
            *(int*) (pointer + OffsetNbFreeIds) = NbFreeIds;

            if (_nbDeletedIds == 0) return;
            
            var index = oldNbFreeIds - NbRecycledIds;
            var startOffset = Offset(index);
            var endOffset = startOffset + SizeofId * (_nbDeletedIds - 1);
            var modEnd = endOffset - endOffset % ExpandableMemoryMappedFile.AllocationGranularity;

            if (endOffset >= _file.Length)
                _file.Expand(ExpandableMemoryMappedFile.AllocationGranularity);

            pointer = _file.GetPointer(startOffset);

            if (modEnd > startOffset)
            {
                var bytesInFirstPage = modEnd - startOffset;
                var bytesInLastPage = _nbDeletedIds * SizeofId - bytesInFirstPage;
                var pointer2 = _file.GetPointer(modEnd);
                fixed (int* pDeletedIds = &_deletedIds[0])
                {
                    Memory.Copy(pointer, pDeletedIds, (uint)bytesInFirstPage);
                    Memory.Copy(pointer2, (byte*)pDeletedIds + bytesInFirstPage, (uint)bytesInLastPage);
                }
            }
            else
            {
                fixed (int* pDeletedIds = &_deletedIds[0])
                {
                    Memory.Copy(pointer, pDeletedIds, (uint)_nbDeletedIds * SizeofId);
                }
            }

            _nbDeletedIds = 0;
            _nbFreeIds = 0;
            NbRecycledIds = 0;
        }

        public unsafe void Read()
        {
            var greaterThanGrab = NbFreeIds > GrabSize;
            var index = greaterThanGrab ? NbFreeIds - GrabSize : 0;
            _nbFreeIds = greaterThanGrab ? GrabSize : NbFreeIds;

            if (_nbFreeIds == 0) return;
            
            var startOffset = Offset(index);
            var endOffset = startOffset + SizeofId * (_nbFreeIds - 1);
            var modEnd = endOffset - endOffset % ExpandableMemoryMappedFile.AllocationGranularity;

            var pointer = _file.GetPointer(startOffset);

            if (modEnd > startOffset)
            {
                var bytesInFirstPage = modEnd - startOffset;
                var bytesInLastPage = _nbFreeIds * SizeofId - bytesInFirstPage;
                var pointer2 = _file.GetPointer(modEnd);
                fixed (int* pFreeIds = &_freeIds[0])
                {
                    Memory.Copy(pFreeIds, pointer, (uint)bytesInFirstPage);
                    Memory.Copy((byte*)pFreeIds + bytesInFirstPage, pointer2, (uint)bytesInLastPage);
                }
            }
            else
            {
                fixed (int* pFreeIds = &_freeIds[0])
                {
                    Memory.Copy(pFreeIds, pointer, (uint)_nbFreeIds * SizeofId);
                }
            }
        }

        private static int Offset(int index)
        {       
            return index * SizeofId + OffsetFreeIds;
        }

        public int GenerateId()
        {
            int result;

            if (_nbDeletedIds > 0)
            {
                result = _deletedIds[_nbDeletedIds - 1];
                _nbDeletedIds--;
                NbFreeIds--;
                IsDirty = true;
            }
            else if (_nbFreeIds > 0)
            {
                result = _freeIds[_nbFreeIds - 1];
                _nbFreeIds--;
                NbFreeIds--;
                NbRecycledIds++;

                if (NbRecycledIds % GrabSize == 0)
                {
                    Write();
                    Read();
                }
                else
                {
                    IsDirty = true;
                }
            }
            else
            {
                result = NextId;
                NextId++;
                IsDirty = true;
            }
            
            return result;
        }

        public void FreeId(int id)
        {
            _deletedIds[_nbDeletedIds] = id;
            _nbDeletedIds++;
            NbFreeIds++;
            IsDirty = true;
            
            if (_nbDeletedIds != GrabSize) return;

            Write();
            Read();
        }

        public void Flush()
        {
            Write();
            _file.Flush();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~IdGenerator()
        {
            Dispose(false);
        }

        protected virtual unsafe void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (_file != null)
                {
                    Write();
                    *_file.GetPointer(0) = 0;
                }
                _file?.Dispose();
            }

            _disposed = true;
        }        
    }
}
