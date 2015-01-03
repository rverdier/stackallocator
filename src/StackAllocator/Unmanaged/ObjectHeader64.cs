using System;
using System.Runtime.InteropServices;

namespace StackAllocator.Unmanaged
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct ObjectHeader64 : IObjectHeader
    {
        [FieldOffset(0)]
        public IntPtr SyncBlockIndex;

        [FieldOffset(8)]
        public IntPtr MethodTablePointer;

        IntPtr IObjectHeader.SyncBlockIndex
        {
            get { return SyncBlockIndex; }
        }

        IntPtr IObjectHeader.MethodTablePointer
        {
            get { return MethodTablePointer; }
        }
    }
}