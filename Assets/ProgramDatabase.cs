using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Dia2Lib;
using Dia.Interop;

public static class ProgramDatabase
{
    static readonly ProcessModule module;
    static readonly IDiaDataSource dia;
    static readonly IDiaSession session;

    static ProgramDatabase()
    {
        module = GetUnityModule();
        dia    = DiaSourceFactory.CreateInstance();
        dia.loadDataForExe(module.FileName, null, null);
        dia.openSession(out session);
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

    public static bool TryGetAddressForSymbol(string symbolName, out IntPtr address)
    {
        if (TryGetRelativeVirtualAddress(symbolName, out uint rva))
        {
            address = new IntPtr(module.BaseAddress.ToInt64() + rva);
            return true;
        }

        address = IntPtr.Zero;
        return false;
    }

    public static bool TryGetSymbolForAddress(IntPtr address, out IDiaSymbol symbol)
    {
        session.globalScope.findChildrenExByRVA(
            SymTagEnum.SymTagPublicSymbol,
            name: null,
            compareFlags: 0,
            rva: (uint)(address.ToInt64() - module.BaseAddress.ToInt64()),
            out IDiaEnumSymbols symbols
        );

        foreach (IDiaSymbol result in symbols)
        {
            symbol = result;
            return true;
        }

        symbol = null;
        return false;
    }

    public static bool TryGetDelegateForSymbol(string symbolName, Type delegateType, out Delegate method)
    {
        if (TryGetAddressForSymbol(symbolName, out IntPtr address))
        {
            method = Marshal.GetDelegateForFunctionPointer(address, delegateType);
            return true;
        }

        method = null;
        return false;
    }

    public static bool TryGetDelegateForSymbol<T>(string symbolName, out T method)
        where T : Delegate
    {
        if (TryGetAddressForSymbol(symbolName, out IntPtr address))
        {
            method = Marshal.GetDelegateForFunctionPointer<T>(address);
            return true;
        }

        method = null;
        return false;
    }

    public static bool TryGetRelativeVirtualAddress(string symbolName, out uint rva)
    {
        session.globalScope.findChildren(
            SymTagEnum.SymTagPublicSymbol,
            symbolName,
            compareFlags: 0u,
            out IDiaEnumSymbols symbols
        );

        foreach (IDiaSymbol symbol in symbols)
        {
            rva = symbol.relativeVirtualAddress;
            return true;
        }

        rva = 0;
        return false;
    }
}
