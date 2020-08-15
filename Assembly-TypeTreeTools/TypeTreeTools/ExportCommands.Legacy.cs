using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using Unity.Core;
using Newtonsoft.Json;

namespace TypeTreeTools
{
    unsafe static partial class ExportCommands
    {
        const string OutputDirectory = "Output";

        [MenuItem("Tools/Type Tree/Legacy/Export String Data")]
        static unsafe void ExportStringData()
        {
            var source = *(byte**)CommonString.BufferBegin;
            var length = *(byte**)CommonString.BufferEnd - source - 1;
            var buffer = new byte[length];

            fixed (byte* destination = buffer)
                Buffer.MemoryCopy(source, destination, length, length);

            Directory.CreateDirectory(OutputDirectory);
            File.WriteAllBytes(Path.Combine(OutputDirectory, "strings.dat"), buffer);
        }

        [MenuItem("Tools/Type Tree/Legacy/Export Classes JSON")]
        static void ExportClassesJson()
        {
            var dictionary = new Dictionary<int, string>();

            for (int i = 0; i < RuntimeTypes.Count; i++)
            {
                var type = RuntimeTypes.Types[i];
                var name = Marshal.PtrToStringAnsi(type->ClassName);
                dictionary.Add((int)type->PersistentTypeID, name);
            }

            Directory.CreateDirectory(OutputDirectory);
            File.WriteAllText(Path.Combine(OutputDirectory, "classes.json"), JsonConvert.SerializeObject(dictionary));
        }

        [MenuItem("Tools/Type Tree/Legacy/Export Struct Data")]
        static unsafe void ExportStructData()
        {
            Directory.CreateDirectory(OutputDirectory);
            var flags    = TransferInstructionFlags.SerializeGameRelease;
            using var bw = new BinaryWriter(File.OpenWrite(Path.Combine(OutputDirectory, "structs.dat")));   

            foreach (char c in Application.unityVersion)
                bw.Write((byte)c);
            bw.Write((byte)0);

            bw.Write((int)Application.platform);
            bw.Write((byte)1); // hasTypeTrees
            var countPosition = (int)bw.BaseStream.Position;
            var typeCount     = 0;

            for (int i = 0; i < RuntimeTypes.Count; i++)
            {
                var type = RuntimeTypes.Types[i];
                var iter = type;

                while (iter->IsAbstract)
                {
                    if (iter->Base == null)
                        goto NextType;

                    iter = iter->Base;
                }

                var obj = NativeObject.GetOrProduce(*iter);

                if (obj == null)
                    continue;

                var tree = new TypeTree();
                tree.Init();
                if (obj->GetTypeTree(flags, ref tree))
                {
                    // Shouldn't this write type.PersistentTypeID instead?
                    // I'm leaving it as iter.PersistentTypeID for consistency
                    // with existing C++ code that generates structs.dat
                    bw.Write((int)iter->PersistentTypeID);

                    // GUID
                    for (int j = 0, n = (int)iter->PersistentTypeID < 0 ? 0x20 : 0x10; j < n; ++j)
                        bw.Write((byte)0);

                    TypeTreeUtility.CreateBinaryDump(tree, bw);
                    typeCount++;
                }

                NativeObject.DestroyIfNotSingletonOrPersistent(obj);

            NextType:
                continue;
            }

            bw.Seek(countPosition, SeekOrigin.Begin);
            bw.Write(typeCount);
        }

        [MenuItem("Tools/Type Tree/Legacy/Export Struct Dump")]
        static unsafe void ExportStructDump()
        {
            Directory.CreateDirectory(OutputDirectory);
            var flags    = TransferInstructionFlags.SerializeGameRelease;
            using var tw = new StreamWriter(Path.Combine(OutputDirectory, "structs.dump"));

            for (int i = 0; i < RuntimeTypes.Count; i++)
            {
                var type        = RuntimeTypes.Types[i];
                var iter        = type;
                var inheritance = string.Empty;

                while (true)
                {
                    inheritance += Marshal.PtrToStringAnsi(iter->ClassName);

                    if (iter->Base == null)
                        break;

                    inheritance += " <- ";
                    iter         = iter->Base;
                }

                tw.WriteLine("\n// classID{{{0}}}: {1}", (int)type->PersistentTypeID, inheritance);

                iter = type;
                while (iter->IsAbstract)
                {
                    tw.WriteLine("// {0} is abstract", Marshal.PtrToStringAnsi(iter->ClassName));

                    if (iter->Base == null)
                        break;

                    iter = iter->Base;
                }

                var obj = NativeObject.GetOrProduce(*iter);

                if (obj == null)
                    continue;

                var tree = new TypeTree();
                tree.Init();
                if (obj->GetTypeTree(flags, ref tree))
                    TypeTreeUtility.CreateTextDump(tree, tw);

                NativeObject.DestroyIfNotSingletonOrPersistent(obj);
            }
        }
    }
}
