using System;

public static unsafe class CommonString
{
    public static ref readonly IntPtr BufferBegin => ref *(IntPtr*)BufferBeginAddr;

    public static ref readonly IntPtr BufferEnd => ref *(IntPtr*)BufferEndAddr;

    [PdbImport("?BufferBegin@CommonString@Unity@@3QEBDEB")]
    static readonly IntPtr BufferBeginAddr;

    [PdbImport("?BufferEnd@CommonString@Unity@@3QEBDEB")]
    static readonly IntPtr BufferEndAddr;
}
