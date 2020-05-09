using System;
using System.Runtime.InteropServices;

namespace Unity.Core
{
    public unsafe struct RuntimeTypeInfo
    {
        public RuntimeTypeInfo* Base;
        public IntPtr Factory;
        public IntPtr ClassName;
        public IntPtr ClassNamespace;
        public IntPtr Module;
        public PersistentTypeID PersistentTypeID;
        public int Size;
        public DerivedFromInfo DerivedFromInfo;
        [MarshalAs(UnmanagedType.U1)]
        public bool IsAbstract;
        [MarshalAs(UnmanagedType.U1)]
        public bool IsSealed;
        [MarshalAs(UnmanagedType.U1)]
        public bool IsEditorOnly;
        [MarshalAs(UnmanagedType.U1)]
        public bool IsStripped;
        public IntPtr Attributes;
        public ulong AttributeCount;
    }

    public struct DerivedFromInfo
    {
        public uint TypeIndex;
        public uint DescendantCount;
    }
}
