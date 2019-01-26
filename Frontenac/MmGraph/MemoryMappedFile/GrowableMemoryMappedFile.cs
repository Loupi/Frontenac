using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace MmGraph.MemoryMappedFile
{
    public unsafe sealed class GrowableMemoryMappedFile : IDisposable
    {
        private const int AllocationGranularity = 64 * 1024;

        private class MemoryMappedArea
        {
            public System.IO.MemoryMappedFiles.MemoryMappedFile Mmf;
            public byte* Address;
            public long Size;
        }


        private FileStream _fs;

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

        public GrowableMemoryMappedFile(string filePath, long initialFileSize)
        {
            if (initialFileSize <= 0 || initialFileSize % AllocationGranularity != 0)
            {
                throw new ArgumentException(
                    "The initial file size must be a multiple of 64Kb and grater than zero",
                    nameof(initialFileSize));
            }

            bool existingFile = File.Exists(filePath);
            _fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

            if (existingFile)
            {
                if (_fs.Length <= 0 || _fs.Length % AllocationGranularity != 0)
                {
                    throw new InvalidOperationException(
                        "Invalid file. Its lenght must be a multiple of 64Kb and greater than zero");
                }
            }
            else
            {
                _fs.SetLength(initialFileSize);
            }

            CreateFirstArea();
        }

        private void CreateFirstArea()
        {
            var mmf = System.IO.MemoryMappedFiles.MemoryMappedFile.CreateFromFile(
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


        public void Grow(long bytesToGrow)
        {
            CheckDisposed();

            if (bytesToGrow <= 0 || bytesToGrow % AllocationGranularity != 0)
            {
                throw new ArgumentException(
                    "The growth must be a multiple of 64Kb and greater than zero",
                    nameof(bytesToGrow));
            }

            long offset = _fs.Length;
            _fs.SetLength(_fs.Length + bytesToGrow);
            var mmf = System.IO.MemoryMappedFiles.MemoryMappedFile.CreateFromFile(
                _fs, null, _fs.Length, MemoryMappedFileAccess.ReadWrite, null, 
                HandleInheritability.None, true);

            uint* offsetPointer = (uint*)&offset;
            var lastArea = _areas[_areas.Count - 1];
            byte* desiredAddress = lastArea.Address + lastArea.Size;
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

            if (desiredAddress != address)
            {
                _offsets.Add(offset);
                _addresses.Add(new Address(address));
            }
        }

        public byte* GetPointer(long offset)
        {
            CheckDisposed();
            int i = _offsets.Count;

            if (i <= 128)
            {
                // linear search is more efficient for small arrays.
                // Experiments show 140 as the cutpoint on x64 and 100 on x86.
                while (--i > 0 && _offsets[i] > offset) ;
            }
            else
            {
                // binary search is more efficient for large arrays
                i = _offsets.BinarySearch(offset);
                if (i < 0) i = ~i - 1;
            }

            return _addresses[i].MemoryLocation + offset - _offsets[i];
        }

        private bool isDisposed;

        public void Dispose()
        {
            if (isDisposed) return;

            isDisposed = true;

            foreach (var a in this._areas)
            {
                Win32FileMapping.UnmapViewOfFile(a.Address);
                a.Mmf.Dispose();
            }

            _fs.Dispose();
            _areas.Clear();
        }

        private void CheckDisposed()
        {
            if (isDisposed) throw new ObjectDisposedException(this.GetType().Name);
        }

        public void Flush()
        {
            CheckDisposed();

            foreach (var area in _areas)
            {
                if (!Win32FileMapping.FlushViewOfFile(area.Address, new IntPtr(area.Size)))
                {
                    throw new Win32Exception();
                }
            }

            _fs.Flush(true);
        }
    }
}
