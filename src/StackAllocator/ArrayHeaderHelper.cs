using System;
using System.Reflection;
using System.Reflection.Emit;
using StackAllocator.Unmanaged;

namespace StackAllocator
{
    internal static class ArrayHeaderHelper<TElement>
    {
        private static readonly Func<IObjectHeader> _headerExtractor = GenerateHeaderExtractor();

        public static IObjectHeader CaptureHeader()
        {
            return _headerExtractor.Invoke();
        }

        private static Func<IObjectHeader> GenerateHeaderExtractor()
        {
            var headerType = Environment.Is64BitProcess ? typeof (ObjectHeader64) : typeof (ObjectHeader32);
            var arrayHeaderSize = Environment.Is64BitProcess ? Array64.HeaderSize : Array32.HeaderSize;

            var dynamicMethod = CreateDynamicMethod();
            var ilGenerator = dynamicMethod.GetILGenerator();

            var pinnedElement = ilGenerator.DeclareLocal(typeof (byte*), true);

            ilGenerator.Emit(OpCodes.Ldc_I4_1);
            ilGenerator.Emit(OpCodes.Newarr, typeof (TElement));
            ilGenerator.Emit(OpCodes.Ldc_I4_0);
            ilGenerator.Emit(OpCodes.Ldelema, typeof (TElement));
            ilGenerator.Emit(OpCodes.Stloc, pinnedElement);

            ilGenerator.Emit(OpCodes.Ldloc, pinnedElement);
            ilGenerator.Emit(OpCodes.Ldc_I4, arrayHeaderSize);
            ilGenerator.Emit(OpCodes.Sub);

            ilGenerator.Emit(OpCodes.Ldobj, headerType);
            ilGenerator.Emit(OpCodes.Box, headerType);
            ilGenerator.Emit(OpCodes.Ret);

            return (Func<IObjectHeader>)dynamicMethod.CreateDelegate(typeof(Func<IObjectHeader>));
        }

        private static DynamicMethod CreateDynamicMethod()
        {
            var returnType = typeof (IObjectHeader);
            var parameterTypes = Type.EmptyTypes;
            var ownerType = typeof (ArrayHeaderHelper<TElement>);

            return new DynamicMethod("CaptureArrayHeaderOf" + typeof (TElement[]).Name, MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, returnType, parameterTypes, ownerType, true);
        }
    }
}