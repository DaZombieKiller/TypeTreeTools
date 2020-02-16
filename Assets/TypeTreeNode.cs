using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

public struct TypeTreeNode
{
    public static int Size => size.Value;

    // The size of TypeTreeNode has changed between Unity versions,
    // due to the addition of new fields. This is a really gross hack
    // where TypeTreeIterator is used to figure out the size at runtime.

    // The alternative would be to conditionally add/remove fields from
    // TypeTreeNode based on the engine version using #if, but this would
    // require far more manual work.

    static unsafe readonly Lazy<int> size = new Lazy<int>(() =>
    {
        var gameObject = new GameObject();
        var native     = NativeObject.FromObject(gameObject);
        var size       = sizeof(TypeTreeNode);

        if (native->TryGetTypeTree(out TypeTree tree))
        {
            var it     = tree.GetIterator().GetChildren();
            ref var n0 = ref Unsafe.AsRef(in it.Node);
            ref var n1 = ref Unsafe.AsRef(in it.GetNext().Node);
            size       = (int)Unsafe.ByteOffset(ref n0, ref n1);
        }

        Object.DestroyImmediate(gameObject);
        return size;
    });

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
