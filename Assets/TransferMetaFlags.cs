using System;

[Flags]
public enum TransferMetaFlags
{
    None                       = 0,
    StrongPPtrMask             = 1 << 6,
    AlignBytesFlag             = 1 << 14,
    AnyChildUsesAlignBytesFlag = 1 << 15,
}
