namespace Unity.Core
{
    public unsafe struct TypeTreeShareableData
    {
        public DynamicArray<TypeTreeNode> Nodes;
        public DynamicArray<byte> StringBuffer;
        public DynamicArray<uint> ByteOffsets;
        public TransferInstructionFlags FlagsAtGeneration;
        public int RefCount;
        public MemLabelId* MemLabel;
    }
}
