using System;
using System.Runtime.InteropServices;

namespace Unity.Core
{
    public unsafe partial struct TypeTree
    {
#if UNITY_2019_1 || UNITY_2019_2
        [PdbSymbol("??0TypeTree@@QEAA@AEBUMemLabelId@@_N@Z")]
        static readonly TypeTreeDelegate s_TypeTree;
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate IntPtr* TypeTreeDelegate(ref TypeTree typeTree, ref MemLabelId memLabel, bool allocatePrivateData = false);
#else
        [PdbSymbol("??0TypeTree@@QEAA@AEBUMemLabelId@@@Z")]
        static readonly TypeTreeDelegate s_TypeTree;
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate IntPtr* TypeTreeDelegate(ref TypeTree typeTree, ref MemLabelId memLabel);
#endif
        [PdbSymbol("?kMemTypeTree@@3UMemLabelId@@A")]
        public static MemLabelId* kMemTypeTree;
    }
}
