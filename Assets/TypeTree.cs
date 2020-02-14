using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

public unsafe struct TypeTreeIterator
{
    IntPtr ptr0;
    IntPtr ptr1;
    public readonly int NodeIndex;
}

public unsafe struct TypeTree
{
    readonly void* dataPointer;

    ref readonly ShareableData Data => ref Unsafe.AsRef<ShareableData>(dataPointer); 

    public ref readonly DynamicArray<TypeTreeNode> Nodes => ref Data.Nodes;

    public ref readonly DynamicArray<byte> Strings => ref Data.Strings;

    public ref readonly DynamicArray<IntPtr> ByteOffsets => ref Data.ByteOffsets;

    public struct ShareableData
    {
        public DynamicArray<TypeTreeNode> Nodes;
        public DynamicArray<byte> Strings;
        public DynamicArray<IntPtr> ByteOffsets;

        public unsafe void Write(BinaryWriter writer)
        {
            writer.Write(Nodes.Length.ToInt32());
            writer.Write(Strings.Length.ToInt32());

            for (int i = 0, n = Nodes.Length.ToInt32(); i < n; i++)
                Nodes.Data[i].Write(writer);

            for (int i = 0, n = Strings.Length.ToInt32(); i < n; i++)
                writer.Write(Strings.Data[i]);
        }

        public void Dump(TextWriter writer)
        {
            for (long i = 0, n = Nodes.Length.ToInt64(); i < n; i++)
            {
                ref TypeTreeNode node = ref Nodes.Data[i];

                var type = GetNodeString(node.TypeOffset);
                var name = GetNodeString(node.NameOffset);

                for (int j = 0; j < node.Depth; j++)
                    writer.Write("  ");

                writer.WriteLine(string.Format("{0} {1} // ByteSize{{{2}}}, Index{{{3}}}, IsArray{{{4}}}, MetaFlag{{{5}}}",
                    type, name, 
                    node.ByteSize.ToString("x"),
                    node.Index.ToString("x"),
                    (byte)node.TypeFlags,
                    ((int)node.MetaFlags).ToString("x")
                ));
            }
        }

        string GetNodeString(int offset)
        {
            var buffer = offset < 0 ? CommonString.BufferBegin : (IntPtr)Strings.Data;
            return Marshal.PtrToStringAnsi(IntPtr.Add(buffer, offset & 0x7fffffff));
        }
    }

    public void Write(BinaryWriter writer)
    {
        Data.Write(writer);
    }

    public void Dump(TextWriter writer)
    {
        Data.Dump(writer);
    }

    [PdbImport("?GenerateTypeTree@@YAXAEBVObject@@AEAVTypeTree@@W4TransferInstructionFlags@@@Z")]
    static readonly GenerateTypeTreeDelegate GenerateTypeTree;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate void GenerateTypeTreeDelegate(in NativeObject obj, out TypeTree tree, TransferInstructionFlags flags);

    [PdbImport("?GetTypeTree@TypeTreeCache@@YA_NPEBVObject@@W4TransferInstructionFlags@@AEAVTypeTree@@@Z")]
    static readonly GetTypeTreeDelegate GetTypeTree;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate bool GetTypeTreeDelegate(in NativeObject obj, TransferInstructionFlags flags, out TypeTree tree);

    public static unsafe bool TryGetTypeTree(NativeObject* obj, out TypeTree tree)
    {
        return TryGetTypeTree(in *obj, out tree);
    }

    public static bool TryGetTypeTree(in NativeObject obj, out TypeTree tree)
    {
        // Unity 2019 and beyond
        if (GetTypeTree != null)
            return GetTypeTree(in obj, 0, out tree);

        // Unity 2018 and older
        GenerateTypeTree(in obj, out tree, 0);
        return true;
    }
}
