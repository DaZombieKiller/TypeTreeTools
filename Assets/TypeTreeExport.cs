using System;
using System.IO;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

public unsafe static class TypeTreeExport
{
    [MenuItem("Tools/Type Tree/Generate structs.dump")]
    static unsafe void GenerateStructsDump()
    {
        using (var fs = File.OpenWrite("structs.dump"))
        using (var sw = new StreamWriter(fs))
        {
            fs.SetLength(0);

            foreach (ref var type in UnityType.RuntimeTypes)
            {
                ref var iter    = ref type;
                var inheritance = string.Empty;

                while (true)
                {
                    inheritance += Marshal.PtrToStringAnsi(iter.Name);

                    if (iter.BaseClass == null)
                        break;

                    inheritance += " <- ";
                    iter         = ref *iter.BaseClass;
                }

                sw.WriteLine("\n// classID{{{0}}}: {1}", type.PersistentTypeID, inheritance);

                iter = ref type;
                while (iter.IsAbstract)
                {
                    sw.WriteLine("// {0} is abstract", Marshal.PtrToStringAnsi(iter.Name));

                    if (iter.BaseClass == null)
                        goto NextType;

                    iter = ref *iter.BaseClass;
                }

                var obj = NativeObject.FromType(in iter);

                if (obj != null && obj->TryGetTypeTree(out TypeTree tree))
                    tree.Dump(sw);

                NativeObject.DestroyTemporary(obj);

            NextType:
                continue;
            }
        }
    }

    [MenuItem("Tools/Type Tree/Generate structs.dat")]
    static unsafe void GenerateStructsDat()
    {
        using (var fs = File.OpenWrite("structs.dat"))
        using (var bw = new BinaryWriter(fs))
        {
            fs.SetLength(0);

            foreach (char c in Application.unityVersion)
                bw.Write((byte)c);
            bw.Write((byte)0);

            bw.Write((int)Application.platform);
            bw.Write((byte)1); // hasTypeTrees
            var countPosition = fs.Position;
            var typeCount     = 0;

            foreach (ref var type in UnityType.RuntimeTypes)
            {
                ref var iter = ref type;
                while (iter.IsAbstract)
                {
                    if (iter.BaseClass == null)
                        goto NextType;

                    iter = ref *iter.BaseClass;
                }

                var obj = NativeObject.FromType(in iter);
                if (obj != null && obj->TryGetTypeTree(out TypeTree tree))
                {
                    // Shouldn't this write type.PersistentTypeID instead?
                    // I'm leaving it as iter.PersistentTypeID for consistency
                    // with existing C++ code that generates structs.dat
                    bw.Write(iter.PersistentTypeID);

                    // GUID
                    for (int i = 0, n = iter.PersistentTypeID < 0 ? 0x20 : 0x10; i < n; i++)
                        bw.Write((byte)0);

                    tree.Write(bw);
                    typeCount++;
                }

                NativeObject.DestroyTemporary(obj);

            NextType:
                continue;
            }

            fs.Position = countPosition;
            bw.Write(typeCount);
        }
    }

    [MenuItem("Tools/Type Tree/Generate strings.dat")]
    static unsafe void GenerateStringsDat()
    {
        var source = (byte*)CommonString.BufferBegin;
        var length = (byte*)CommonString.BufferEnd - source - 1;
        var buffer = new byte[length];

        fixed (byte* destination = buffer)
            Buffer.MemoryCopy(source, destination, length, length);

        File.WriteAllBytes("strings.dat", buffer);
    }

    [MenuItem("Tools/Type Tree/Generate classes.json")]
    static unsafe void GenerateClassesJson()
    {
        var dictionary = new Dictionary<int, string>();
        foreach (ref var type in UnityType.RuntimeTypes)
        {
            var name = Marshal.PtrToStringAnsi(type.Name);
            dictionary.Add(type.PersistentTypeID, name);
        }

        File.WriteAllText("classes.json", JsonConvert.SerializeObject(dictionary));
    }

    [MenuItem("Tools/Type Tree/Generate ClassID Enum")]
    static void GenerateClassIDEnum()
    {
        using (var cp = CodeDomProvider.CreateProvider("C#"))
        using (var fs = File.OpenWrite("Assets/ClassID.cs"))
        using (var sw = new StreamWriter(fs))
        {
            fs.SetLength(0);
            sw.WriteLine($"// This file was generated with Unity {Application.unityVersion}.");
            sw.WriteLine("public enum ClassID");
            sw.WriteLine("{");

            foreach (ref var type in UnityType.RuntimeTypes)
            {
                var name = Marshal.PtrToStringAnsi(type.Name);

                // Prepend keywords with an '@' character.
                if (!cp.IsValidIdentifier(name))
                    name = '@' + name;

                sw.WriteLine("    {0} = {1},", name, type.PersistentTypeID);
            }

            sw.WriteLine("}");
        }

        AssetDatabase.Refresh();
    }
}
