using System;
using System.Runtime.InteropServices;
using StackAllocator.Unmanaged;

namespace StackAllocator
{
    internal unsafe class ArrayStackAllocator<T> where T : struct
    {
        private static readonly IObjectHeader _arrayHeader = CaptureArrayHeader();

        public static void Allocates(int length, Action<T[]> action)
        {
            var stackAllocatedArrayLength = ComputeStackAllocationSize(length);
            var stackAllocatedArray = stackalloc byte[stackAllocatedArrayLength];
            var stackAllocatedArrayObject = GetArrayObject(stackAllocatedArray, length);

            action(stackAllocatedArrayObject);
        }

        private static int ComputeStackAllocationSize(int arrayLength)
        {
            var headerSize = Environment.Is64BitProcess ? Array64.HeaderSize : Array32.HeaderSize;
            return headerSize + (arrayLength*GetSizeOfElement());
        }

        private static int GetSizeOfElement()
        {
            var elementType = typeof (T);

            if (elementType == typeof (char))
                return 2;

            if (elementType.IsEnum)
                return Marshal.SizeOf(Enum.GetUnderlyingType(elementType));

            return Marshal.SizeOf(elementType);
        }

        private static T[] GetArrayObject(void* stackAllocatedArray, int arrayLength)
        {
            if (Environment.Is64BitProcess)
            {
                var array64Pointer = (Array64*) stackAllocatedArray;
                array64Pointer->SyncBlockIndex = _arrayHeader.SyncBlockIndex;
                array64Pointer->MethodTablePointer = _arrayHeader.MethodTablePointer;
                array64Pointer->Length = arrayLength;
                return PointerHelper<T[]>.Reinterpret(((byte*) array64Pointer) + Array64.SyncBlocSize);
            }

            var array32Pointer = (Array32*) stackAllocatedArray;
            array32Pointer->SyncBlockIndex = _arrayHeader.SyncBlockIndex;
            array32Pointer->MethodTablePointer = _arrayHeader.MethodTablePointer;
            array32Pointer->Length = arrayLength;
            return PointerHelper<T[]>.Reinterpret(((byte*) array32Pointer) + Array32.SyncBlocSize);
        }

        private static IObjectHeader CaptureArrayHeader()
        {
            if (Environment.Is64BitProcess)
                return ArrayHeaderHelper<T>.CaptureHeader();

            return ArrayHeaderHelper<T>.CaptureHeader();
        }
    }
}