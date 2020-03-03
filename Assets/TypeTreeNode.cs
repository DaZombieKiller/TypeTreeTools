using System.IO;

public struct TypeTreeNode
{
    public short Version;
    public byte Depth;
    public TypeFlags TypeFlags;
    public int TypeOffset;
    public int NameOffset;
    public int ByteSize;
    public int Index;
    public TransferMetaFlags MetaFlags;
#pragma warning disable IDE0051 // Remove unused private members
    // Only ever seems to be set to 0. Could also be a pointer.
    // Need to disassemble the 32-bit UnityPlayer to be sure.
    readonly int unknown0;
    readonly int unknown1;
#pragma warning restore IDE0051

    public void Write(BinaryWriter writer)
    {
        writer.Write(Version);
        writer.Write(Depth);
        writer.Write((byte)TypeFlags);
        writer.Write(TypeOffset);
        writer.Write(NameOffset);
        writer.Write(ByteSize);
        writer.Write(Index);
        writer.Write((int)MetaFlags);
    }
}
