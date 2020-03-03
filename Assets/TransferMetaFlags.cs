using System;

[Flags]
public enum TransferMetaFlags
{
    None                                = 0,
    HideInEditor                        = 1 << 0,
    NotEditable                         = 1 << 4,
    StrongPPtr                          = 1 << 6,
    TreatIntegerValueAsBoolean          = 1 << 8,
    SimpleEditor                        = 1 << 11,
    DebugProperty                       = 1 << 12,
    AlignBytes                          = 1 << 14,
    AnyChildUsesAlignBytes              = 1 << 15,
    IgnoreWithInspectorUndo             = 1 << 16,
    EditorDisplaysCharacterMap          = 1 << 18,
    IgnoreInMetaFiles                   = 1 << 19,
    TransferAsArrayEntryNameInMetaFiles = 1 << 20,
    TransferUsingFlowMappingStyle       = 1 << 21,
    GenerateBitwiseDifferences          = 1 << 22,
    DontAnimate                         = 1 << 23,
    TransferHex64                       = 1 << 24,
    CharProperty                        = 1 << 25,
    DontValidateUTF8                    = 1 << 26,
    Bit28                               = 1 << 28
}
