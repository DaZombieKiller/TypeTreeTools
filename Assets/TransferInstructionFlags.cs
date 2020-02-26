using System;

[Flags]
public enum TransferInstructionFlags
{
    None                                    = 0,
    ReadWriteFromSerializedFile             = 1 << 0,
    AllowDontSaveObjectsToBePersistent      = 1 << 1,
    EnableMessageOptimization               = 1 << 3,
    SerializeDebugProperties                = 1 << 4,
    BuildPlayerOnlySerializeBuildProperties = 1 << 6,
    SerializeGameRelease                    = 1 << 8,
    SerializeForPrefabSystem                = 1 << 14,
    PerformUnloadDependencyTracking         = 1 << 25,
    AllowTextSerialization                  = 1 << 31,
}
