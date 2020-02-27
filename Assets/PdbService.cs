using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
#if UNITY_EDITOR
using Dia2Lib;
#else
using System.IO.Pipes;
#endif

public class PdbService : IDisposable
{
    public ProcessModule Module { get; }
#if UNITY_EDITOR
    readonly DiaSource dia;
    IDiaSession session;
#else
    readonly Process process;
    readonly StreamWriter writer;
    readonly StreamReader reader;
    readonly NamedPipeServerStream server;

    const string PipeName = "Unity.PdbService";
#endif

    public PdbService()
        : this(GetUnityModule())
    {
    }

    public PdbService(ProcessModule module)
    {
        Module = module;
    #if UNITY_EDITOR
        dia = new DiaSource();
    #else
        server  = new NamedPipeServerStream(PipeName, PipeDirection.InOut);
        process = new Process
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
    #endif
        InitFromExe(module.FileName);
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

    void InitFromExe(string path)
    {
    #if UNITY_EDITOR
        dia.loadDataForExe(path, Path.GetDirectoryName(path), null);
        dia.openSession(out session);
    #else
        writer.WriteLine("InitFromExe");
        writer.WriteLine(path);
    #endif
    }

    public bool TryGetAddressForSymbol(string symbolName, out IntPtr address)
    {
        if (TryGetRelativeVirtualAddress(symbolName, out uint rva))
        {
            address = new IntPtr(Module.BaseAddress.ToInt64() + rva);
            return true;
        }

        address = IntPtr.Zero;
        return false;
    }

    public bool TryGetDelegateForSymbol(string symbolName, Type delegateType, out Delegate method)
    {
        if (TryGetAddressForSymbol(symbolName, out IntPtr address))
        {
            method = Marshal.GetDelegateForFunctionPointer(address, delegateType);
            return true;
        }

        method = null;
        return false;
    }

    public bool TryGetDelegateForSymbol<T>(string symbolName, out T method)
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

    public bool TryGetRelativeVirtualAddress(string symbolName, out uint rva)
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

    public void Dispose()
    {
    #if !UNITY_EDITOR
        reader.Dispose();
        writer.Dispose();
        server.Dispose();
        process.Kill();
        process.Dispose();
    #endif
    }
}
