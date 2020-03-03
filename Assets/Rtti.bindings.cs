using System;

public unsafe partial struct Rtti
{
    [PdbImport("?ms_runtimeTypes@RTTI@@0URuntimeTypeArray@1@A")]
    static readonly IntPtr runtimeTypes;
    public static ref readonly RuntimeTypeArray RuntimeTypes => ref *(RuntimeTypeArray*)runtimeTypes;
}
