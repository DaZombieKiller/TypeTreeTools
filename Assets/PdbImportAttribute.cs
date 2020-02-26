using System;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEditor;
using Debug = UnityEngine.Debug;
#if !UNITY_EDITOR
using UnityEngine;
#endif

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class PdbImportAttribute : Attribute
{
    public string SymbolName { get; }

    public PdbImportAttribute(string symbolName)
    {
        SymbolName = symbolName;
    }

    static unsafe void InitializeField(FieldInfo field, IntPtr address)
    {
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
#else
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
    static void InitializeWithService()
    {
        using (var service = new PdbService())
        {

#if !UNITY_EDITOR || !UNITY_2020_1_OR_NEWER
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            foreach (var type in assembly.GetTypes())
            foreach (var field in type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static))
#else
            foreach (var field in TypeCache.GetFieldsWithAttribute<PdbImportAttribute>())
#endif
            {
                if (!field.IsStatic)
                {
                    Debug.LogErrorFormat("{0} must be static.", field.Name);
                    continue;
                }

                var attr = field.GetCustomAttribute<PdbImportAttribute>();

                if (attr == null)
                    continue;
    
                if (service.TryGetAddressForSymbol(attr.SymbolName, out IntPtr address))
                    InitializeField(field, address);
            }
        }
    }
}
