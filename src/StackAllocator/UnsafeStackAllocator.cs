using System;
using System.Text;

namespace StackAllocator
{
    public static class UnsafeStackAllocator
    {
        public static void NewArrayOf<T>(int length, Action<T[]> action) where T : struct
        {
            ArrayStackAllocator<T>.Allocates(length, action);
        }

        public static void NewString(byte[] bytes, int length, Decoder decoder, Action<string> action)
        {
            StringStackAllocator.Allocates(bytes, length, decoder, action);
        }
    }
}