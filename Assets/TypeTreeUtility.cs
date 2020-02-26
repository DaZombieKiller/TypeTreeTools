using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public static class TypeTreeUtility
{
    public static unsafe void WriteCommonStrings(BinaryWriter bw)
    {
        var source = (byte*)CommonString.BufferBegin;
        var length = (byte*)CommonString.BufferEnd - source - 1;
        var buffer = new byte[length];

        fixed (byte* destination = buffer)
            Buffer.MemoryCopy(source, destination, length, length);

        bw.Write(buffer);
    }

    public static unsafe void WriteDataFile(PdbService service, TransferInstructionFlags flags, BinaryWriter bw)
    {
        foreach (char c in Application.unityVersion)
            bw.Write((byte)c);
        bw.Write((byte)0);

        bw.Write((int)Application.platform);
        bw.Write((byte)1); // hasTypeTrees
        var countPosition = (int)bw.BaseStream.Position;
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

            var obj = NativeObject.FromType(ref iter, service);
            if (obj != null && obj->TryGetTypeTree(flags, out TypeTree tree))
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

        bw.Seek(countPosition, SeekOrigin.Begin);
        bw.Write(typeCount);
    }
    
    public static unsafe void WriteDumpFile(PdbService service, TransferInstructionFlags flags, TextWriter tw)
    {
        foreach (ref var type in UnityType.RuntimeTypes)
        {
            ref var iter    = ref type;
            var inheritance = string.Empty;

            while (true)
            {
                inheritance += iter.GetName();

                if (iter.BaseClass == null)
                    break;

                inheritance += " <- ";
                iter         = ref *iter.BaseClass;
            }

            tw.WriteLine("\n// classID{{{0}}}: {1}", type.PersistentTypeID, inheritance);

            iter = ref type;
            while (iter.IsAbstract)
            {
                tw.WriteLine("// {0} is abstract", iter.GetName());

                if (iter.BaseClass == null)
                    break;

                iter = ref *iter.BaseClass;
            }

            var obj = NativeObject.FromType(ref type, service);

            if (obj != null && obj->TryGetTypeTree(flags, out var tree))
                tree.Dump(tw);

            NativeObject.DestroyTemporary(obj);
        }
    }

    public static void WriteClassesJson(TextWriter tw)
    {
        var dictionary = new Dictionary<int, string>();

        foreach (ref var type in UnityType.RuntimeTypes)
            dictionary.Add(type.PersistentTypeID, type.GetName());

        tw.Write(JsonConvert.SerializeObject(dictionary));
    }
}
