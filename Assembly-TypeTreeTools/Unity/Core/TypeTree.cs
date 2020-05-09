using System;
using System.Runtime.InteropServices;

namespace Unity.Core
{
    public unsafe struct TypeTree
    {
        public TypeTreeShareableData* Data;
        public IntPtr ReferencedTypes;
        [MarshalAs(UnmanagedType.U1)]
        public bool PoolOwned;
    }
}
