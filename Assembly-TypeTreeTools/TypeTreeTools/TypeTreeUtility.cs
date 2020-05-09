using System.IO;
using Unity.Core;

namespace TypeTreeTools
{
    public static unsafe class TypeTreeUtility
    {
        public static void CreateBinaryDump(TypeTree tree, BinaryWriter writer)
        {
            writer.Write(tree.Data->Nodes.Size);
            writer.Write(tree.Data->StringBuffer.Size);

            var iterator = new TypeTreeIterator(&tree);
            BinaryDumpNodes(iterator, writer);

            for (ulong i = 0, n = tree.Data->StringBuffer.Size; i < n; i++)
                writer.Write(tree.Data->StringBuffer.Ptr[i]);
        }

        static void BinaryDumpNodes(TypeTreeIterator it, BinaryWriter writer)
        {
            while (!it.IsNull)
            {
                var node = it.GetNode();
                writer.Write(node->Version);
                writer.Write(node->Level);
                writer.Write((byte)node->TypeFlags);
                writer.Write(node->TypeStrOffset);
                writer.Write(node->NameStrOffset);
                writer.Write(node->ByteSize);
                writer.Write(node->Index);
                writer.Write((int)node->MetaFlag);
                BinaryDumpNodes(it.GetChildren(), writer);
                it = it.GetNext();
            }
        }

        public static void CreateTextDump(TypeTree tree, TextWriter writer)
        {
            var iterator = new TypeTreeIterator(&tree);
            TextDumpNodes(iterator, writer);
        }

        static void TextDumpNodes(TypeTreeIterator it, TextWriter writer)
        {
            while (!it.IsNull)
            {
                var node = it.GetNode();

                if (node->Index < 0)
                    goto Next;

                for (int j = 0; j < node->Level; j++)
                    writer.Write("  ");

                writer.WriteLine(string.Format("{0} {1} // ByteSize{{{2}}}, Index{{{3}}}, IsArray{{{4}}}, MetaFlag{{{5}}}",
                    it.Type,
                    it.Name,
                    node->ByteSize.ToString("x"),
                    node->Index.ToString("x"),
                    (byte)node->TypeFlags,
                    ((int)node->MetaFlag).ToString("x")
                ));

            Next:
                TextDumpNodes(it.GetChildren(), writer);
                it = it.GetNext();
            }
        }
    }
}
