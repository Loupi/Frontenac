using System.Reflection.Emit;

namespace MmGraph.Database.Win32
{
    public static class Memory
    {
        public static unsafe void Copy(void* destination, void* source, uint size)
        {
            // Pick fastest function based on number of bytes to copy
            if (size < 32 || size >= 262144)
                CustomCopy(destination, source, size);
            else
                CpBlk(destination, source, size);
        }

        // ReSharper disable once RedundantUnsafeContext
        private static readonly unsafe CopyBlockDelegate CpBlkDelegate = GenerateCpBlk();

        private unsafe delegate void CopyBlockDelegate(void* des, void* src, uint bytes);

        private static unsafe void CpBlk(void* dest, void* src, uint count)
        {
            var local = CpBlkDelegate;
            local(dest, src, count);
        }

        // ReSharper disable once RedundantUnsafeContext
        private static unsafe CopyBlockDelegate GenerateCpBlk()
        {
            var method = new DynamicMethod("CopyBlockIL", typeof(void), new[] { typeof(void*), typeof(void*), typeof(uint) }, typeof(Memory));
            var emitter = method.GetILGenerator();
            // emit IL
            emitter.Emit(OpCodes.Ldarg_0);
            emitter.Emit(OpCodes.Ldarg_1);
            emitter.Emit(OpCodes.Ldarg_2);
            emitter.Emit(OpCodes.Cpblk);
            emitter.Emit(OpCodes.Ret);
            // compile to delegate
            return (CopyBlockDelegate)method.CreateDelegate(typeof(CopyBlockDelegate));
        }
        
        private static unsafe void CustomCopy(void* destination, void* source, uint size)
        {
            var block = size >> 3;
            var pDest = (long*)destination;
            var pSrc = (long*)source;

            for (var i = 0; i < block; i++)
            {
                *pDest = *pSrc; pDest++; pSrc++;
            }

            destination = (byte*)pDest;
            source = (byte*)pSrc;
            size = size - (block << 3);

            if (size > 0)
            {
                var pDestB = (byte*)destination;
                var pSrcB = (byte*)source;
                for (var i = 0; i < size; i++)
                {
                    *pDestB = *pSrcB; pDestB++; pSrcB++;
                }
            }
        }

        /*
        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr memcpy(IntPtr dest, IntPtr src, UIntPtr count);

        private static unsafe void MemCpy(void* destination, void* source, uint size)
        {
            memcpy(new IntPtr(destination), new IntPtr(source), new UIntPtr(size));
        }
        */
    }
}