namespace Unity.Core
{
    public struct TypeTreeNode
    {
        public short Version;
        public byte Level;
        public TypeFlags TypeFlags;
        public uint TypeStrOffset;
        public uint NameStrOffset;
        public int ByteSize;
        public int Index;
        public TransferMetaFlags MetaFlag;
        public ulong RefTypeHash;
    }
}
