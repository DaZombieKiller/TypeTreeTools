using System;

[Flags]
public enum TypeFlags : byte
{
    None          = 0,
    IsArray       = 1 << 0,
    IsRef         = 1 << 1,
    IsRegistry    = 1 << 2,
    IsArrayOfRefs = 1 << 3,
}
