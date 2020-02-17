using System;

public readonly struct MemoryLabel
{
#pragma warning disable IDE0051 // Remove unused private members
# if DEBUG
    readonly IntPtr unk;
# endif
    readonly int id;
#pragma warning restore IDE0051
}
