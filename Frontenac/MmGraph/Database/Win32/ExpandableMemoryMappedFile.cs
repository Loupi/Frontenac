using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace MmGraph.Database.Win32
{
    public sealed unsafe class ExpandableMemoryMappedFile : IDisposable
    {
        public const int AllocationGranularity = 64 * 1024;

        private class MemoryMappedArea
        {
            public MemoryMappedFile Mmf;
            public byte* Address;
            public long Size;
        }


        private readonly FileStream _fs;
        private readonly List<MemoryMappedArea> _areas = new List<MemoryMappedArea>();
        private readonly List<long> _offsets = new List<long>();
        private readonly List<Address> _addresses = new List<Address>();

        public long Length
        {
            get
            {
                CheckDisposed();
                return _fs.Length;
            }
        }

        public ExpandableMemoryMappedFile(string filePath, long initialFileSize)
        {
            if (initialFileSize <= 0 || initialFileSize % AllocationGranularity != 0)
                throw new ArgumentException(
                    "The initial file size must be a multiple of 64Kb and grater than zero",
                    nameof(initialFileSize));

            var existingFile = File.Exists(filePath);
            _fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

            if (existingFile)
            {
                if(_fs.Length <= 0 || _fs.Length % AllocationGranularity != 0)
                    throw new InvalidOperationException(
                        "Invalid file. Its length must be a multiple of 64Kb and greater than zero");
            }
            else
            {
                _fs.SetLength(initialFileSize);
            }
            
            CreateFirstArea();
        }

        private void CreateFirstArea()
        {
            var mmf = MemoryMappedFile.CreateFromFile(
                _fs, null, _fs.Length, MemoryMappedFileAccess.ReadWrite, null, 
                HandleInheritability.None, true);

            var address = Win32FileMapping.MapViewOfFileEx(
                mmf.SafeMemoryMappedFileHandle.DangerousGetHandle(),
                Win32FileMapping.FileMapAccess.Read | Win32FileMapping.FileMapAccess.Write,
                0, 0, new UIntPtr((ulong)_fs.Length), null);

            if (address == null) throw new Win32Exception();

            var area = new MemoryMappedArea
            {
                Address = address,
                Mmf = mmf,
                Size = _fs.Length
            };
            _areas.Add(area);

            _addresses.Add(new Address(address));
            _offsets.Add(0);
        }


        public void Expand(long bytesToGrow)
        {
            CheckDisposed();

            if (bytesToGrow <= 0 || bytesToGrow % AllocationGranularity != 0)
                throw new ArgumentException(
                    "The growth must be a multiple of 64Kb and greater than zero",
                    nameof(bytesToGrow));

            var offset = _fs.Length;
            _fs.SetLength(_fs.Length + bytesToGrow);
            var mmf = MemoryMappedFile.CreateFromFile(
                _fs, null, _fs.Length, MemoryMappedFileAccess.ReadWrite, null, 
                HandleInheritability.None, true);

            var offsetPointer = (uint*)&offset;
            var lastArea = _areas[_areas.Count - 1];
            var desiredAddress = lastArea.Address + lastArea.Size;
            var address = Win32FileMapping.MapViewOfFileEx(
                mmf.SafeMemoryMappedFileHandle.DangerousGetHandle(),
                Win32FileMapping.FileMapAccess.Read | Win32FileMapping.FileMapAccess.Write,
                offsetPointer[1], offsetPointer[0], new UIntPtr((ulong)bytesToGrow), desiredAddress);

            if (address == null)
            {
                address = Win32FileMapping.MapViewOfFileEx(
                    mmf.SafeMemoryMappedFileHandle.DangerousGetHandle(),
                   Win32FileMapping.FileMapAccess.Read | Win32FileMapping.FileMapAccess.Write,
                   offsetPointer[1], offsetPointer[0], new UIntPtr((ulong)bytesToGrow), null);
            }

            if (address == null) throw new Win32Exception();

            var area = new MemoryMappedArea
            {
                Address = address,
                Mmf = mmf,
                Size = bytesToGrow
            };

            _areas.Add(area);

            if (desiredAddress == address) return;

            _offsets.Add(offset);
            _addresses.Add(new Address(address));
        }

        public byte* GetPointer(long offset)
        {
            CheckDisposed();
            var i = _offsets.Count;

            if (i <= 128)
            {
                // linear search is more efficient for small arrays.
                // Experiments show 140 as the cutpoint on x64 and 100 on x86.
                while (--i > 0 && _offsets[i] > offset)
                {
                }
            }
            else
            {
                // binary search is more efficient for large arrays
                i = _offsets.BinarySearch(offset);
                if (i < 0) i = ~i - 1;
            }

            return _addresses[i].MemoryLocation + offset - _offsets[i];
        }

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ExpandableMemoryMappedFile()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                foreach (var a in _areas)
                {
                    Win32FileMapping.UnmapViewOfFile(a.Address);
                    a.Mmf.Dispose();
                }

                _fs.Dispose();
                _areas.Clear();
            }

            _disposed = true;
        }

        private void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().Name);
        }

        public void Flush()
        {
            CheckDisposed();

            foreach (var area in _areas)
            {
                if (!Win32FileMapping.FlushViewOfFile(area.Address, new IntPtr(area.Size)))
                    throw new Win32Exception();
            }

            _fs.Flush(true);
        }
    }
}
