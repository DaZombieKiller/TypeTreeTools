using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public static class TypeTreeUtility
{
    public static unsafe void WriteCommonStrings(BinaryWriter bw)
    {
        var source = CommonString.BufferBegin;
        var length = CommonString.BufferEnd - source - 1;
        var buffer = new byte[length];

        fixed (byte* destination = buffer)
            Buffer.MemoryCopy(source, destination, length, length);

        bw.Write(buffer);
    }

    public static unsafe void WriteDataFile(TransferInstructionFlags flags, BinaryWriter bw)
    {
        foreach (char c in Application.unityVersion)
            bw.Write((byte)c);
        bw.Write((byte)0);

        bw.Write((int)Application.platform);
        bw.Write((byte)1); // hasTypeTrees
        var countPosition = (int)bw.BaseStream.Position;
        var typeCount     = 0;

        foreach (ref var type in Rtti.RuntimeTypes)
        {
            ref var iter = ref type;
            while (iter.IsAbstract)
            {
                if (iter.BaseClass == null)
                    goto NextType;

                iter = ref *iter.BaseClass;
            }

            var obj = NativeObject.GetOrProduce(iter);

            if (obj == null)
                continue;

            if (obj->TryGetTypeTree(flags, out var tree))
            {
                // Shouldn't this write type.PersistentTypeID instead?
                // I'm leaving it as iter.PersistentTypeID for consistency
                // with existing C++ code that generates structs.dat
                bw.Write((int)iter.PersistentTypeID);

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
    
    public static unsafe void WriteDumpFile(TransferInstructionFlags flags, TextWriter tw)
    {
        foreach (ref var type in Rtti.RuntimeTypes)
        {
            ref var iter    = ref type;
            var inheritance = string.Empty;

            while (true)
            {
                inheritance += iter.Name;

                if (iter.BaseClass == null)
                    break;

                inheritance += " <- ";
                iter         = ref *iter.BaseClass;
            }

            tw.WriteLine("\n// classID{{{0}}}: {1}", type.PersistentTypeID, inheritance);

            iter = ref type;
            while (iter.IsAbstract)
            {
                tw.WriteLine("// {0} is abstract", iter.Name);

                if (iter.BaseClass == null)
                    break;

                iter = ref *iter.BaseClass;
            }

            var obj = NativeObject.GetOrProduce(type);

            if (obj == null)
                continue;

            if (obj->TryGetTypeTree(flags, out var tree))
                tree.Dump(tw);

            NativeObject.DestroyTemporary(obj);
        }
    }

    public static void WriteClassesJson(TextWriter tw)
    {
        var dictionary = new Dictionary<ClassID, string>();

        foreach (ref var type in Rtti.RuntimeTypes)
            dictionary.Add(type.PersistentTypeID, type.Name);

        tw.Write(JsonConvert.SerializeObject(dictionary));
    }
}
