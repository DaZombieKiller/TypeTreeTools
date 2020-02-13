using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using Dia2Lib;
using Debug = UnityEngine.Debug;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class PdbImportAttribute : Attribute
{
    public string SymbolName { get; }

    public PdbImportAttribute(string symbolName)
    {
        SymbolName = symbolName;
    }

    [InitializeOnLoadMethod]
    [RuntimeInitializeOnLoadMethod]
    static unsafe void Initialize()
    {
        var module     = Process.GetCurrentProcess().MainModule;
        var searchPath = Path.GetDirectoryName(module.FileName);
        var dia        = new DiaSourceClass();
        dia.loadDataForExe(module.FileName, searchPath, null);
        dia.openSession(out IDiaSession session);

        foreach (FieldInfo field in TypeCache.GetFieldsWithAttribute<PdbImportAttribute>())
        {
            if (!field.IsStatic)
            {
                Debug.LogErrorFormat("{0} must be static.", field.Name);
                continue;
            }

            session.globalScope.findChildren(
                SymTagEnum.SymTagPublicSymbol,
                field.GetCustomAttribute<PdbImportAttribute>().SymbolName,
                compareFlags: 0u,
                out IDiaEnumSymbols symbols
            );

            foreach (IDiaSymbol symbol in symbols)
            {
                var rva     = symbol.relativeVirtualAddress;
                var address = new IntPtr(module.BaseAddress.ToInt64() + rva);

                if (field.FieldType == typeof(IntPtr))
                    field.SetValue(null, address);
                else if (field.FieldType == typeof(UIntPtr))
                    field.SetValue(null, new UIntPtr(address.ToPointer()));
                else if (field.FieldType.IsSubclassOf(typeof(Delegate)))
                    field.SetValue(null, Marshal.GetDelegateForFunctionPointer(address, field.FieldType));
                else
                    Debug.LogErrorFormat("{0} must be of IntPtr, UIntPtr or delegate type.", field.Name);

                break;
            }
        }
    }
}
