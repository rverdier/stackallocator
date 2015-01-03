using System;
using System.Text;

namespace StackAllocator
{
    public static class StackAllocator
    {
        static void AllocatesString(byte[] bytes, int length, Decoder decoder, Action<string> action)
        {
        }

        static void AllocatesArray<T>(int length, Action<T[]> action) where T : struct
        {
        }
    }
}