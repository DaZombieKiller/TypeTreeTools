using System;
using System.Runtime.CompilerServices;

public readonly struct DynamicArray<T>
    where T : unmanaged
{
    public readonly unsafe T* Data;
    public readonly MemoryLabel Label;
    public readonly IntPtr Length;
    public readonly IntPtr Capacity;
}

public readonly struct TypeTreeNodeArray
{
    public readonly unsafe TypeTreeNode* Data;
    public readonly MemoryLabel Label;
    public readonly IntPtr Length;
    public readonly IntPtr Capacity;

    public unsafe ref TypeTreeNode GetAt(long index)
    {
        var offset = new IntPtr(index * TypeTreeNode.Size);
        return ref Unsafe.AddByteOffset(ref *Data, offset);
    }
}
