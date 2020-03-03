using System;

public static unsafe class CommonString
{
    [PdbImport("?BufferBegin@CommonString@Unity@@3QEBDEB")]
    static readonly IntPtr BufferBeginAddr;
    public static ref readonly byte* BufferBegin => ref *(byte**)BufferBeginAddr;

    [PdbImport("?BufferEnd@CommonString@Unity@@3QEBDEB")]
    static readonly IntPtr BufferEndAddr;
    public static ref readonly byte* BufferEnd => ref *(byte**)BufferEndAddr;
}
