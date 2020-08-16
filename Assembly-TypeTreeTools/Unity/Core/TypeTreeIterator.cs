using System;
using System.Runtime.InteropServices;

namespace Unity.Core
{
    public unsafe partial struct TypeTreeIterator
    {
        public TypeTree* LinkedTypeTree;
        public TypeTreeShareableData* TypeTreeData;
        public ulong NodeIndex;

        static TypeTreeIterator() => PdbSymbolImporter.EnsureInitialized();

        public TypeTreeIterator(TypeTree* tree)
        {
            LinkedTypeTree = tree;
#if UNITY_2019_1_OR_NEWER
            TypeTreeData   = tree->Data;
#else
            TypeTreeData = null;
#endif
            NodeIndex = 0;
        }

        public string Name
        {
            get
            {
                ThrowIfIteratorIsNull();
                return PtrPtrToStringAnsi(s_Name(ref this));
            }
        }

        public string Type
        {
            get
            {
                ThrowIfIteratorIsNull();
                return PtrPtrToStringAnsi(s_Type(ref this));
            }
        }

        public bool HasConstantSize
        {
            get
            {
                ThrowIfIteratorIsNull();
                return s_HasConstantSize(ref this);
            }
        }

        public uint ByteOffset
        {
            get
            {
                ThrowIfIteratorIsNull();
                return s_ByteOffset(ref this);
            }
        }

        public bool IsNull
        {
            get
            {
                return LinkedTypeTree == null || TypeTreeData == null;
            }
        }

        void ThrowIfIteratorIsNull()
        {
            if (IsNull)
                throw new NullReferenceException("Iterator is null.");
        }

        // These methods appear to mutate the TypeTreeIterator,
        // despite the native methods being marked const, thus
        // a copy of the iterator is created before calling them.
        public TypeTreeIterator GetNext()
        {
            ThrowIfIteratorIsNull();
            var copy = this;
            return s_Next(ref copy);
        }

        public TypeTreeIterator GetLast()
        {
            ThrowIfIteratorIsNull();
            var copy = this;
            return s_Last(ref copy);
        }

        public TypeTreeIterator GetFather()
        {
            ThrowIfIteratorIsNull();
            var copy = this;
            return s_Father(ref copy);
        }

        public TypeTreeIterator GetChildren()
        {
            ThrowIfIteratorIsNull();
            var copy = this;
            return s_Children(ref copy);
        }

        public TypeTreeNode* GetNode()
        {
            ThrowIfIteratorIsNull();
            return s_GetNode(ref this);
        }

        static string PtrPtrToStringAnsi(IntPtr* p)
        {
            if (p == null)
                return string.Empty;

            return Marshal.PtrToStringAnsi(*p);
        }
    }
}
