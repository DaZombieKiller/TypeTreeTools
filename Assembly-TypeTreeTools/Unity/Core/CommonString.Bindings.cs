using System;

namespace Unity.Core
{
    public unsafe static class CommonString
    {
        static CommonString() => PdbSymbolImporter.EnsureInitialized();

        [PdbSymbol("?BufferBegin@CommonString@Unity@@3QEBDEB")]
        public static readonly IntPtr* BufferBegin;

        [PdbSymbol("?BufferEnd@CommonString@Unity@@3QEBDEB")]
        public static readonly IntPtr* BufferEnd;
    }
}
