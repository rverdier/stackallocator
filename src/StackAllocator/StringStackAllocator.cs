using System;
using System.Text;
using StackAllocator.Unmanaged;

namespace StackAllocator
{
    internal static unsafe class StringStackAllocator
    {
        private static readonly IntPtr _stringTypeHandle = (typeof (string).TypeHandle.Value);

        public static void Allocates(byte[] bytes, int length, Decoder decoder, Action<string> action)
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
                    var string64 = (String64*) stackAllocatedArray;
                    string64->SyncBlockIndex = IntPtr.Zero;
                    string64->MethodTablePointer = _stringTypeHandle;
                    string64->Length = charsCount;
                    return PointerHelper<string>.Reinterpret(((byte*) string64) + String64.SyncBlocSize);
                }

                var string32 = (String32*) stackAllocatedArray;
                string32->SyncBlockIndex = IntPtr.Zero;
                string32->MethodTablePointer = _stringTypeHandle;
                string32->Length = charsCount;
                return PointerHelper<string>.Reinterpret(((byte*) string32) + String32.SyncBlocSize);
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