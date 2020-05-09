using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Dia;
using Dia2Lib;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace Unity.Core
{
    [InitializeOnLoad]
    internal static class PdbSymbolImporter
    {
        unsafe static PdbSymbolImporter()
        {
            var module = Process.GetCurrentProcess().MainModule;
            var scope  = PdbSession.Current.globalScope;

            foreach (var field in GetFieldsWithAttribute<PdbSymbolAttribute>())
            {
                if (!field.IsStatic)
                {
                    Debug.LogErrorFormat("{0} must be static.", field.Name);
                    continue;
                }

                scope.findChildren(
                    SymTagEnum.SymTagPublicSymbol,
                    GetCustomAttribute<PdbSymbolAttribute>(field).SymbolName,
                    (uint)NameSearchOptions.None,
                    out var symbols
                );

                foreach (IDiaSymbol symbol in symbols)
                {
                    var address = new IntPtr(module.BaseAddress.ToInt64() + symbol.relativeVirtualAddress);

                    if (field.FieldType == typeof(IntPtr))
                        field.SetValue(null, address);
                    else if (field.FieldType == typeof(UIntPtr))
                        field.SetValue(null, new UIntPtr(address.ToPointer()));
                    else if (field.FieldType.IsPointer)
                        CreateStaticSetter<IntPtr>(field).Invoke(address);
                    else if (field.FieldType.IsSubclassOf(typeof(Delegate)))
                        field.SetValue(null, Marshal.GetDelegateForFunctionPointer(address, field.FieldType));
                    else
                        Debug.LogErrorFormat("{0} must be of IntPtr, UIntPtr or delegate type.", field.Name);

                    break;
                }
            }
        }

        static IEnumerable<FieldInfo> GetFieldsWithAttribute<T>()
            where T : Attribute
        {
        #if UNITY_2020_1_OR_NEWER
            return TypeCache.GetFieldsWithAttribute<T>();
        #else
            const BindingFlags AllMemberFlags = 0
                | BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static | BindingFlags.Instance;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            foreach (var type     in assembly.GetTypes())
            foreach (var field    in type.GetFields(AllMemberFlags))
            {
                var attribute = GetCustomAttribute<T>(field);

                if (attribute == null)
                    continue;

                yield return field;
            }
        #endif
        }

        static T GetCustomAttribute<T>(FieldInfo field)
            where T : Attribute
        {
            return (T)Attribute.GetCustomAttribute(field, typeof(T));
        }

        // FieldInfo.SetValue doesn't appear to work for T* fields,
        // so we have to use this absolutely disgusting hack instead.
        static Action<T> CreateStaticSetter<T>(FieldInfo field)
        {
            var method = new DynamicMethod(string.Empty, null, new[] { typeof(T) }, true);
            var il     = method.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Stsfld, field);
            il.Emit(OpCodes.Ret);

            return (Action<T>)method.CreateDelegate(typeof(Action<T>));
        }

        public static void EnsureInitialized()
        {
            // Calls to this will call the static constructor,
            // which will ensure initialization of PDB symbols.
        }
    }
}
