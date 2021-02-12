using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;

namespace QuickRestClient.ILGeneration
{
    public static class ILGeneratorExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EmitString(this ILGenerator il, string value)
        {
            il.Emit(OpCodes.Ldstr, value);
        }

        public static void EmitEmptyArray<TElement>(this ILGenerator il, int size)
        {
            il.Emit(OpCodes.Ldc_I4_S, size);
            il.Emit(OpCodes.Newarr, typeof(TElement));
        }

        public static void SetArrayReferenceElement(this ILGenerator il, int arrayIndex, Action<ILGenerator> value)
        {
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4_S, arrayIndex);

            value.Invoke(il);

            il.Emit(OpCodes.Stelem_Ref);
        }

        public static void EmitLoadArg(this ILGenerator il, int argumentIndex)
        {
            switch (argumentIndex)
            {
                case 0:
                    il.Emit(OpCodes.Ldarg_0);
                    return;
                case 1:
                    il.Emit(OpCodes.Ldarg_1);
                    return;
                case 2:
                    il.Emit(OpCodes.Ldarg_2);
                    return;
                case 3:
                    il.Emit(OpCodes.Ldarg_3);
                    return;
                default:
                    il.Emit(OpCodes.Ldarg_S, argumentIndex);
                    return;
            }
        }

        public static void EmitLoadArgAddress(this ILGenerator il, int argumentIndex)
        {
            il.Emit(OpCodes.Ldarga_S, argumentIndex);
        }

        public static void EmitToString(this ILGenerator il, Type topOfStack)
        {
            var toStringMethod = topOfStack.GetMethod(
                nameof(object.ToString),
                new Type[0]);
            if (topOfStack.IsPrimitive)
            {
                il.Emit(OpCodes.Call, toStringMethod);
            }
            else
            {
                il.Emit(OpCodes.Callvirt, toStringMethod);
            }
        }
    }
}
