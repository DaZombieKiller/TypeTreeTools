using System;
using System.IO;
using System.IO.Pipes;
using System.Diagnostics;

public class PdbService : IDisposable
{
    readonly Process process;
    readonly StreamWriter writer;
    readonly StreamReader reader;
    readonly NamedPipeServerStream server;

    const string PipeName = "Unity.PdbService";

    public PdbService(ProcessModule module)
    {
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
        InitFromExe(module.FileName);
    }

    void InitFromExe(string path)
    {
        writer.WriteLine("InitFromExe");
        writer.WriteLine(path);
    }

    public bool TryGetRelativeVirtualAddress(string symbol, out uint rva)
    {
        writer.WriteLine("GetRelativeVirtualAddress");
        writer.WriteLine(symbol);

        if (bool.Parse(reader.ReadLine()))
        {
            rva = uint.Parse(reader.ReadLine());
            return true;
        }

        rva = 0;
        return false;
    }

    public void Dispose()
    {
        reader.Dispose();
        writer.Dispose();
        server.Dispose();
        process.Kill();
        process.Dispose();
    }
}
