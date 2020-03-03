using System;
using System.Runtime.InteropServices;

public unsafe partial struct TypeTreeIterator
{
#if UNITY_EDITOR
    [PdbImport("?HasConstantSize@TypeTreeIterator@@QEBA_NXZ")]
    static readonly HasConstantSizeDelegate HasConstantSizeGetter;
    [return: MarshalAs(UnmanagedType.U1)]
    delegate bool HasConstantSizeDelegate(in TypeTreeIterator it);

    [PdbImport("?ByteOffset@TypeTreeIterator@@QEBAIXZ")]
    static readonly ByteOffsetDelegate ByteOffsetGetter;
    delegate uint ByteOffsetDelegate(in TypeTreeIterator it);
#endif

    [PdbImport("?Name@TypeTreeIterator@@QEBA?AVTypeTreeString@@XZ")]
    static readonly NameDelegate NameGetter;
    delegate ref readonly IntPtr NameDelegate(in TypeTreeIterator it);

    [PdbImport("?Type@TypeTreeIterator@@QEBA?AVTypeTreeString@@XZ")]
    static readonly TypeDelegate TypeGetter;
    delegate ref readonly IntPtr TypeDelegate(in TypeTreeIterator it);

    [PdbImport("?GetNode@TypeTreeIterator@@AEBAPEBUTypeTreeNode@@XZ")]
    public static readonly GetNodeDelegate GetNode;
    public delegate TypeTreeNode* GetNodeDelegate(in TypeTreeIterator it);

    [PdbImport("?Children@TypeTreeIterator@@QEBA?AV1@XZ")]
    public static readonly ChildrenDelegate Children;
    public delegate TypeTreeIterator* ChildrenDelegate(ref TypeTreeIterator it);

    [PdbImport("?Father@TypeTreeIterator@@QEBA?AV1@XZ")]
    static readonly FatherDelegate Father;
    delegate TypeTreeIterator* FatherDelegate(ref TypeTreeIterator it);

    [PdbImport("?Last@TypeTreeIterator@@QEBA?AV1@XZ")]
    static readonly LastDelegate Last;
    delegate TypeTreeIterator* LastDelegate(ref TypeTreeIterator it);

    [PdbImport("?Next@TypeTreeIterator@@QEBA?AV1@XZ")]
    static readonly NextDelegate Next;
    delegate TypeTreeIterator* NextDelegate(ref TypeTreeIterator it);
}
