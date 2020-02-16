using System;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Collections.Generic;
using Dia2Lib;

class Program
{
    static DiaSourceClass dia;

    static IDiaSession session;

    static Dictionary<string, Action<StreamReader, StreamWriter>> commands;

    static void Main(string[] args)
    {
        if (args.Length < 1)
            return;

        using var client = new NamedPipeClientStream(".", args[0], PipeDirection.InOut);

        InitializeCommands();
        client.Connect();

        using var reader = new StreamReader(client);
        using var writer = new StreamWriter(client) { AutoFlush = true };

        while (client.IsConnected)
        {
            if (commands.TryGetValue(reader.ReadLine(), out var command))
                command(reader, writer);
        }
    }

    [PipeCommand]
    static void InitFromExe(StreamReader reader, StreamWriter writer)
    {
        var path = reader.ReadLine();
        dia      = new DiaSourceClass();
        dia.loadDataForExe(path, Path.GetDirectoryName(path), null);
        dia.openSession(out session);
    }

    [PipeCommand]
    static void InitFromPdb(StreamReader reader, StreamWriter writer)
    {
        dia = new DiaSourceClass();
        dia.loadDataFromPdb(reader.ReadLine());
        dia.openSession(out session);
    }

    [PipeCommand]
    static void GetRelativeVirtualAddress(StreamReader reader, StreamWriter writer)
    {
        var symbolName = reader.ReadLine();

        session.globalScope.findChildren(
            SymTagEnum.SymTagPublicSymbol,
            symbolName,
            compareFlags: 0u,
            out IDiaEnumSymbols symbols
        );

        if (symbols.count <= 0)
            writer.WriteLine(false);
        else
        {
            writer.WriteLine(true);

            foreach (IDiaSymbol symbol in symbols)
            {
                writer.WriteLine(symbol.relativeVirtualAddress);
                break;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    sealed class PipeCommandAttribute : Attribute
    {
        public string Name { get; }

        public PipeCommandAttribute(string name = null)
        {
            Name = name;
        }
    }

    static void InitializeCommands()
    {
        commands = new Dictionary<string, Action<StreamReader, StreamWriter>>();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes())
            {
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                {
                    var attr = method.GetCustomAttribute<PipeCommandAttribute>();

                    if (attr is null)
                        continue;

                    var action = method.CreateDelegate(typeof(Action<StreamReader, StreamWriter>));
                    commands.Add(attr.Name ?? method.Name, (Action<StreamReader, StreamWriter>)action);
                }
            }
        }
    }
}
