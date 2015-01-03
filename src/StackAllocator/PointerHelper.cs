using System.Reflection.Emit;

namespace StackAllocator
{
    internal static unsafe class PointerHelper<T>
    {
        private static readonly PointerReinterpreter _pointerReinterpreter = CreatePointerReinterpreter();

        public static T Reinterpret(void* pointer)
        {
            return _pointerReinterpreter.Invoke(pointer);
        }

        private static PointerReinterpreter CreatePointerReinterpreter()
        {
            var m = new DynamicMethod("ReinterpretPointerAs" + typeof (T).Name, typeof (T), new[] {typeof (void*)}, typeof (PointerHelper<T>), true);
            var il = m.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ret);
            return (PointerReinterpreter) m.CreateDelegate(typeof (PointerReinterpreter));
        }

        private delegate T PointerReinterpreter(void* nativePointer);
    }
}