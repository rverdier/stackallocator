using System.Reflection.Emit;
using InlineIL;

namespace StackAllocator
{
    internal unsafe struct ObjectReference<T>
    {
        private readonly T _value;

        private ObjectReference(T value) : this()
        {
            _value = value;
        }

        public static explicit operator ObjectReference<T>(void* pointer)
        {
            return new ObjectReference<T>(ReinterpretPointerAs<T>(pointer));
        }

        public static implicit operator T(ObjectReference<T> pointer)
        {
            return pointer._value;
        }

        private static T ReinterpretPointerAs<T>(void* pointer)
        {
            IL.Emit.Ldarg_0();
            IL.Emit.Ret();
            throw IL.Unreachable();
        }
    }
}
