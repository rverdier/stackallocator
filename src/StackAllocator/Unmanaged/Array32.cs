using System;
using System.Runtime.InteropServices;

namespace StackAllocator.Unmanaged
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct Array32
    {
        public const int SyncBlocSize = 4;
        public const int HeaderSize = 12;

        [FieldOffset(0)]
        public IntPtr SyncBlockIndex;

        [FieldOffset(4)]
        public IntPtr MethodTablePointer;

        [FieldOffset(8)]
        public int Length;
    }
}