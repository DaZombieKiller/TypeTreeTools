using System;
using System.Runtime.InteropServices;

public unsafe struct TypeTreeIterator
{
    readonly IntPtr unknown;
    readonly IntPtr typeTreeData;
    readonly IntPtr nodeIndex;

    public TypeTreeIterator(IntPtr treeData)
    {
        unknown      = treeData;
        typeTreeData = treeData;
        nodeIndex    = IntPtr.Zero;
    }

    public string Name => Marshal.PtrToStringAnsi(NameGetter(in this));

    public string Type => Marshal.PtrToStringAnsi(TypeGetter(in this));

    public uint ByteOffset => ByteOffsetGetter(in this);

#if UNITY_EDITOR
    public bool HasConstantSize => HasConstantSizeGetter(in this);
#endif

    public bool IsNull => typeTreeData == IntPtr.Zero;

    public ref readonly TypeTreeNode Node => ref *GetNode(in this);

    public ref readonly TypeTreeIterator GetNext() => ref *Next(Copy());

    public ref readonly TypeTreeIterator GetLast() => ref *Last(Copy());

    public ref readonly TypeTreeIterator GetFather() => ref *Father(Copy());

    public ref readonly TypeTreeIterator GetChildren() => ref *Children(Copy());

    TypeTreeIterator Copy() => this;

#if UNITY_EDITOR
    [PdbImport("?HasConstantSize@TypeTreeIterator@@QEBA_NXZ")]
    static readonly HasConstantSizeDelegate HasConstantSizeGetter;

    [return: MarshalAs(UnmanagedType.U1)]
    delegate bool HasConstantSizeDelegate(in TypeTreeIterator it);
#endif

    [PdbImport("?ByteOffset@TypeTreeIterator@@QEBAIXZ")]
    static readonly ByteOffsetDelegate ByteOffsetGetter;
    delegate uint ByteOffsetDelegate(in TypeTreeIterator it);

    [PdbImport("?Children@TypeTreeIterator@@QEBA?AV1@XZ")]
    public static readonly ChildrenDelegate Children;
    public delegate TypeTreeIterator* ChildrenDelegate(in TypeTreeIterator it);

    [PdbImport("?Father@TypeTreeIterator@@QEBA?AV1@XZ")]
    static readonly FatherDelegate Father;
    delegate TypeTreeIterator* FatherDelegate(in TypeTreeIterator it);

    [PdbImport("?Last@TypeTreeIterator@@QEBA?AV1@XZ")]
    static readonly LastDelegate Last;
    delegate TypeTreeIterator* LastDelegate(in TypeTreeIterator it);

    [PdbImport("?Next@TypeTreeIterator@@QEBA?AV1@XZ")]
    static readonly NextDelegate Next;
    delegate TypeTreeIterator* NextDelegate(in TypeTreeIterator it);

    [PdbImport("?Name@TypeTreeIterator@@QEBA?AVTypeTreeString@@XZ")]
    static readonly NameDelegate NameGetter;
    delegate ref readonly IntPtr NameDelegate(in TypeTreeIterator it);

    [PdbImport("?Type@TypeTreeIterator@@QEBA?AVTypeTreeString@@XZ")]
    static readonly TypeDelegate TypeGetter;
    delegate ref readonly IntPtr TypeDelegate(in TypeTreeIterator it);

    [PdbImport("?GetNode@TypeTreeIterator@@AEBAPEBUTypeTreeNode@@XZ")]
    public static readonly GetNodeDelegate GetNode;
    public delegate TypeTreeNode* GetNodeDelegate(in TypeTreeIterator it);
}
