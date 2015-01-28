using System;
using System.Text;
using StackAllocator.Unmanaged;

namespace StackAllocator
{
    internal static unsafe class StringStackAllocator
    {
        private static readonly IntPtr _stringTypeHandle = (typeof (string).TypeHandle.Value);

        public static void Allocate(byte[] bytes, int length, Decoder decoder, Action<string> action)
        {
            var stackAllocatedArrayLength = ComputeStackAllocationSize(bytes, length, decoder);
            var stackAllocatedArray = stackalloc byte[stackAllocatedArrayLength];
            var stackAllocatedStringObject = GetStringObject(bytes, length, decoder, stackAllocatedArray);

            action(stackAllocatedStringObject);
        }

        private static string GetStringObject(byte[] bytes, int length, Decoder decoder, byte* stackAllocatedArray)
        {
            fixed (byte* p = bytes)
            {
                var headerSize = Environment.Is64BitProcess ? String64.HeaderSize : String32.HeaderSize;

                var stringBodyPointer = (char*) (stackAllocatedArray + headerSize);

                var charsCount = decoder.GetChars(p, length, stringBodyPointer, length, true);

                *(stringBodyPointer + charsCount) = '\0';

                if (Environment.Is64BitProcess)
                {
                    var string64Pointer = (String64*) stackAllocatedArray;
                    string64Pointer->SyncBlockIndex = IntPtr.Zero;
                    string64Pointer->MethodTablePointer = _stringTypeHandle;
                    string64Pointer->Length = charsCount;
                    var string64ObjectPointer = ((byte*) string64Pointer) + String64.SyncBlocSize;
                    return (ObjectReference<string>)string64ObjectPointer;
                }

                var string32Pointer = (String32*) stackAllocatedArray;
                string32Pointer->SyncBlockIndex = IntPtr.Zero;
                string32Pointer->MethodTablePointer = _stringTypeHandle;
                string32Pointer->Length = charsCount;
                var string32ObjectPointer = ((byte*) string32Pointer) + String32.SyncBlocSize;
                return (ObjectReference<string>)string32ObjectPointer;
            }
        }

        private static int ComputeStackAllocationSize(byte[] bytes, int length, Decoder decoder)
        {
            var headerSize = Environment.Is64BitProcess ? String64.HeaderSize : String32.HeaderSize;
            var charCount = decoder.GetCharCount(bytes, 0, length) + 1; // +1 for nul character at the end
            return headerSize + (charCount*sizeof (char));
        }
    }
}