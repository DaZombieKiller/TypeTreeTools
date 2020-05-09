using System;
using System.Runtime.InteropServices;

namespace Unity.Core
{
    public unsafe partial struct TypeTreeIterator
    {
        [PdbSymbol("?Name@TypeTreeIterator@@QEBA?AVTypeTreeString@@XZ")]
        static readonly NameDelegate s_Name;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate IntPtr* NameDelegate(ref TypeTreeIterator it);

        [PdbSymbol("?Type@TypeTreeIterator@@QEBA?AVTypeTreeString@@XZ")]
        static readonly TypeDelegate s_Type;
        delegate IntPtr* TypeDelegate(ref TypeTreeIterator it);

        [PdbSymbol("?Children@TypeTreeIterator@@QEBA?AV1@XZ")]
        static readonly ChildrenDelegate s_Children;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate ref TypeTreeIterator ChildrenDelegate(ref TypeTreeIterator it);

        [PdbSymbol("?Father@TypeTreeIterator@@QEBA?AV1@XZ")]
        static readonly FatherDelegate s_Father;
        delegate ref TypeTreeIterator FatherDelegate(ref TypeTreeIterator it);

        [PdbSymbol("?Next@TypeTreeIterator@@QEBA?AV1@XZ")]
        static readonly NextDelegate s_Next;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate ref TypeTreeIterator NextDelegate(ref TypeTreeIterator it);

        [PdbSymbol("?Last@TypeTreeIterator@@QEBA?AV1@XZ")]
        static readonly LastDelegate s_Last;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate ref TypeTreeIterator LastDelegate(ref TypeTreeIterator it);

        [PdbSymbol("?GetNode@TypeTreeIterator@@AEBAPEBUTypeTreeNode@@XZ")]
        static readonly GetNodeDelegate s_GetNode;
        delegate TypeTreeNode* GetNodeDelegate(ref TypeTreeIterator it);

        [PdbSymbol("?HasConstantSize@TypeTreeIterator@@QEBA_NXZ")]
        static readonly HasConstantSizeDelegate s_HasConstantSize;
        [return: MarshalAs(UnmanagedType.U1)]
        delegate bool HasConstantSizeDelegate(ref TypeTreeIterator it);

        [PdbSymbol("?ByteOffset@TypeTreeIterator@@QEBAIXZ")]
        static readonly ByteOffsetDelegate s_ByteOffset;
        delegate uint ByteOffsetDelegate(ref TypeTreeIterator it);
    }
}
