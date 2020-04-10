using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
#if UNITY_EDITOR
using Dia2Lib;
#else
using System.IO;
using System.IO.Pipes;
using UnityEngine;
#endif

public static class ProgramDatabase
{
    static readonly ProcessModule module;
#if UNITY_EDITOR
    static readonly DiaSource dia;
    static readonly IDiaSession session;
#else
    static readonly Process process;
    static readonly StreamWriter writer;
    static readonly StreamReader reader;
    static readonly NamedPipeServerStream server;
#endif

    static ProgramDatabase()
    {
        module = GetUnityModule();
    #if UNITY_EDITOR
        dia = new DiaSource();
        dia.loadDataForExe(module.FileName, null, null);
        dia.openSession(out session);
    #else
        const string PipeName = "Unity.PdbService";
        server                = new NamedPipeServerStream(PipeName, PipeDirection.InOut);
        process               = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName               = Path.GetFullPath("PdbService.exe"),
                Arguments              = PipeName,
                CreateNoWindow         = true,
                UseShellExecute        = false,
                RedirectStandardInput  = true,
                RedirectStandardOutput = true,
            }
        };

        process.Start();
        server.WaitForConnection();

        writer = new StreamWriter(server) { AutoFlush = true };
        reader = new StreamReader(server);

        writer.WriteLine("InitFromExe");
        writer.WriteLine(module.FileName);

        Application.quitting += () =>
        {
            reader.Dispose();
            writer.Dispose();
            server.Dispose();
            process.Kill();
            process.Dispose();
        };
    #endif
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
    #if UNITY_EDITOR
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
    #else
        writer.WriteLine("GetRelativeVirtualAddress");
        writer.WriteLine(symbolName);

        if (bool.Parse(reader.ReadLine()))
        {
            rva = uint.Parse(reader.ReadLine());
            return true;
        }
    #endif

        rva = 0;
        return false;
    }
}
