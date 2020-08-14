using System;
using System.Runtime.InteropServices;

namespace Unity.Core
{
    public unsafe partial struct TypeTree
    {
#if UNITY_2019_1 || UNITY_2019_2
        public TypeTreeShareableData* Data;
        public TypeTreeShareableData m_PrivateData;
#elif UNITY_2019_3_OR_NEWER
        public TypeTreeShareableData* Data;
        public IntPtr ReferencedTypes;
        [MarshalAs(UnmanagedType.U1)]
        public bool PoolOwned;
#else
        public DynamicArray<TypeTreeNode> Nodes;
        public DynamicArray<byte> StringBuffer;
        public DynamicArray<uint> ByteOffsets;
#endif

        public void Init()
        {
            s_TypeTree(ref this, ref *kMemTypeTree);
        }
    }
}
