using System;

[Flags]
public enum TransferInstructionFlags
{
    None                        = 0,
    ReadWriteFromSerializedFile = 1 << 0,
    SerializeDebugProperties    = 1 << 4,
    SerializeGameRelease        = 1 << 8,
    AllowTextSerialization      = 1 << 31,
}
