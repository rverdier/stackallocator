using System;
using System.Runtime.InteropServices;

namespace StackAllocator.Unmanaged
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct String64
    {
        public const int SyncBlocSize = 8;
        public const int HeaderSize = 20;

        [FieldOffset(0)]
        public IntPtr SyncBlockIndex;

        [FieldOffset(8)]
        public IntPtr MethodTablePointer;

        [FieldOffset(16)]
        public int Length;
    }
}