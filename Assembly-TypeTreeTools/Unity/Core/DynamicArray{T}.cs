namespace Unity.Core
{
    public unsafe struct DynamicArray<T>
        where T : unmanaged
    {
        public T* Ptr;
        public MemLabelId Label;
        public ulong Size;
        public ulong Capacity;
    }
}
