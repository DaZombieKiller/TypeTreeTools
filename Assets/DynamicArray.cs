using System;
using System.Runtime.CompilerServices;
#if UNITY_64
using nint = System.Int64;
#else
using nint = System.Int32;
#endif

public readonly unsafe struct DynamicArray<T>
    where T : struct
{
    readonly Pointer<T> data;
    public readonly MemoryLabel Label;
    public readonly nint Length;
    public readonly nint Capacity;

    public ref T this[nint index]
    {
        get
        {
            if (index < 0 || index >= Length)
                throw new IndexOutOfRangeException();

            return ref Unsafe.Add(ref data.Value, new IntPtr(index));
        }
    }

    public ref T GetPinnableReference()
    {
        return ref data.Value;
    }
}
