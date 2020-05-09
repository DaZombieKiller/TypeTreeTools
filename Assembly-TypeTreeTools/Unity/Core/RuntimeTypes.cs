namespace Unity.Core
{
    public unsafe static class RuntimeTypes
    {
        public static uint Count => ms_runtimeTypes->Count;

        public static RuntimeTypeInfo** Types => &ms_runtimeTypes->First;

        [PdbSymbol("?ms_runtimeTypes@RTTI@@0URuntimeTypeArray@1@A")]
        static readonly RuntimeTypeArray* ms_runtimeTypes;

        unsafe struct RuntimeTypeArray
        {
            public readonly uint Count;
            public RuntimeTypeInfo* First;
        }
    }
}
