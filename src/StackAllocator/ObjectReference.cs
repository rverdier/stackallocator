using System.Reflection.Emit;

namespace StackAllocator
{
    internal unsafe struct ObjectReference<T>
    {
        private static readonly PointerReinterpreter _pointerReinterpreter = CreatePointerReinterpreter();

        private readonly T _value;

        private ObjectReference(T value) : this()
        {
            _value = value;
        }

        public static explicit operator ObjectReference<T>(void* pointer)
        {
            return new ObjectReference<T>(_pointerReinterpreter(pointer));
        }

        public static implicit operator T(ObjectReference<T> pointer)
        {
            return pointer._value;
        }

        private static PointerReinterpreter CreatePointerReinterpreter()
        {
            var m = new DynamicMethod("ReinterpretPointerAs" + typeof(T).Name, typeof(T), new[] { typeof(void*) }, typeof(ObjectReference<T>), true);
            var il = m.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ret);
            return (PointerReinterpreter)m.CreateDelegate(typeof(PointerReinterpreter));
        }

        private delegate T PointerReinterpreter(void* nativePointer);
    }
}
