using System;
using System.Runtime.InteropServices;

namespace MmGraph.MemoryMappedFile
{
    public static unsafe class Win32FileMapping
    {
        [Flags]
        public enum FileMapAccess : uint
        {
            Copy = 0x01,
            Write = 0x02,
            Read = 0x04,
            AllAccess = 0x08,
            Execute = 0x20,
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern byte* MapViewOfFileEx(IntPtr mappingHandle,
                                            FileMapAccess access,
                                            uint offsetHigh,
                                            uint offsetLow,
                                            UIntPtr bytesToMap,
                                            byte* desiredAddress);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool UnmapViewOfFile(byte* address);


        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FlushViewOfFile(byte* address, IntPtr bytesToFlush);
    }
}
