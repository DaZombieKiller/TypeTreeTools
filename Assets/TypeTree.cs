using System.IO;
using System.Runtime.CompilerServices;
#if UNITY_64
using nint = System.Int64;
#else
using nint = System.Int32;
#endif

public unsafe partial struct TypeTree
{
    readonly void* dataPointer;

    ref readonly ShareableData Data => ref Unsafe.AsRef<ShareableData>(dataPointer); 

    public ref readonly DynamicArray<TypeTreeNode> Nodes => ref Data.Nodes;

    public ref readonly DynamicArray<byte> StringBuffer => ref Data.StringBuffer;

    public TypeTreeIterator GetIterator()
    {
        return new TypeTreeIterator(dataPointer);
    }

    public struct ShareableData
    {
        public DynamicArray<TypeTreeNode> Nodes;
        public DynamicArray<byte> StringBuffer;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(Nodes.Length);
        writer.Write(StringBuffer.Length);

        WriteNodes(GetIterator(), writer);

        for (nint i = 0, n = StringBuffer.Length; i < n; i++)
            writer.Write(StringBuffer[i]);
    }

    static void WriteNodes(TypeTreeIterator it, BinaryWriter writer)
    {
        while (!it.IsNull)
        {
            it.Node.Write(writer);
            WriteNodes(it.GetChildren(), writer);
            it = it.GetNext();
        }
    }

    public void Dump(TextWriter writer)
    {
        DumpNodes(GetIterator(), writer);
    }

    static void DumpNodes(TypeTreeIterator it, TextWriter writer)
    {
        while (!it.IsNull)
        {
            var node = it.Node;

            if (node.Index < 0)
                goto Next;

            for (int j = 0; j < node.Depth; j++)
                writer.Write("  ");

            writer.WriteLine(string.Format("{0} {1} // ByteSize{{{2}}}, Index{{{3}}}, IsArray{{{4}}}, MetaFlags{{{5}}}",
                it.Type,
                it.Name,
                node.ByteSize.ToString("x"),
                node.Index.ToString("x"),
                (byte)node.TypeFlags,
                ((int)node.MetaFlags).ToString("x")
            ));

        Next:
            DumpNodes(it.GetChildren(), writer);
            it = it.GetNext();
        }
    }
}
