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

    static ProcessModule GetUnityModule()
    {
        foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
        {
            if (module.ModuleName == "UnityPlayer.dll")
                return module;
        }

        return Process.GetCurrentProcess().MainModule;
    }

    static unsafe void InitializeField(FieldInfo field, ProcessModule module, uint rva)
    {
        var address = new IntPtr(module.BaseAddress.ToInt64() + rva);

        if (field.FieldType == typeof(IntPtr))
            field.SetValue(null, address);
        else if (field.FieldType == typeof(UIntPtr))
            field.SetValue(null, new UIntPtr(address.ToPointer()));
        else if (field.FieldType.IsSubclassOf(typeof(Delegate)))
            field.SetValue(null, Marshal.GetDelegateForFunctionPointer(address, field.FieldType));
        else
            Debug.LogErrorFormat("{0} must be of IntPtr, UIntPtr or delegate type.", field.Name);
    }

#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    static void InitializeWithDia()
    {
        var module     = GetUnityModule();
        var searchPath = Path.GetDirectoryName(module.FileName);
        var dia        = new DiaSourceClass();
        dia.loadDataForExe(module.FileName, searchPath, null);
        dia.openSession(out IDiaSession session);

        foreach (var field in TypeCache.GetFieldsWithAttribute<PdbImportAttribute>())
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
                InitializeField(field, module, symbol.relativeVirtualAddress);
                break;
            }
        }
    }
#else
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitializeWithService()
    {
        var module = GetUnityModule();

        using (var service = new PdbService(module))
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    foreach (var field in type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static))
                    {
                        var attr = field.GetCustomAttribute<PdbImportAttribute>();

                        if (attr == null)
                            continue;
    
                        if (service.TryGetRelativeVirtualAddress(attr.SymbolName, out uint rva))
                            InitializeField(field, module, rva);
                    }
                }
            }
        }
    }
#endif
}
