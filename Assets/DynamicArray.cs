using System;

public readonly struct DynamicArray<T>
    where T : unmanaged
{
    public readonly unsafe T* Data;
    public readonly MemoryLabel Label;
    public readonly IntPtr Length;
    public readonly IntPtr Capacity;
}
