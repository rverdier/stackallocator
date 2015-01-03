using System;
using System.Runtime.InteropServices;

namespace StackAllocator.Unmanaged
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct Array64
    {
        public const int SyncBlocSize = 8;
        public const int HeaderSize = 24;

        [FieldOffset(0)]
        public IntPtr SyncBlockIndex;

        [FieldOffset(8)]
        public IntPtr MethodTablePointer;

        [FieldOffset(16)]
        public long Length;
    }
}