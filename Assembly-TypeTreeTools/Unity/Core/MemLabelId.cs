namespace Unity.Core
{
    public struct MemLabelId
    {
        public AllocationRootWithSalt RootReference;
        public MemoryLabelIdentifier Identifier;
    }

    public struct AllocationRootWithSalt
    {
        public uint Salt;
        public uint RootReferenceIndex;
    }

    public enum MemoryLabelIdentifier
    {
        // Not worth filling out
    }
}
