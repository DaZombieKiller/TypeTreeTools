using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Debug = UnityEngine.Debug;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class PdbImportAttribute : ImportAttribute
{
    public string SymbolName { get; }

    public PdbImportAttribute(string symbolName)
    {
        SymbolName = symbolName;
    }

    unsafe static PdbImportAttribute()
    {
        foreach (var field in GetFieldsWithAttribute<PdbImportAttribute>())
        {
            if (!field.IsStatic)
            {
                Debug.LogErrorFormat("{0} must be static.", field.Name);
                continue;
            }

            var attr = field.GetCustomAttribute<PdbImportAttribute>();
            if (ProgramDatabase.TryGetAddressForSymbol(attr.SymbolName, out IntPtr address))
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
        }
    }
}
