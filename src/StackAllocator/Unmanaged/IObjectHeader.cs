using System;

namespace StackAllocator.Unmanaged
{
    internal interface IObjectHeader
    {
        IntPtr SyncBlockIndex { get; }
        IntPtr MethodTablePointer { get; }
    }
}