using System;
using System.Runtime.CompilerServices;

public unsafe struct Pointer<T>
    where T : struct
{
    public Pointer(void* address)
    {
        Address = address;
    }

    public void* Address;

    public bool IsNull => Address == null;

    public ref T Value
    {
        get
        {
            if (IsNull)
                throw new NullReferenceException();

            return ref Unsafe.AsRef<T>(Address);
        }
    }

    public override int GetHashCode()
    {
        return new IntPtr(Address).GetHashCode();
    }

    public override string ToString()
    {
        return new IntPtr(Address).ToString();
    }
}
