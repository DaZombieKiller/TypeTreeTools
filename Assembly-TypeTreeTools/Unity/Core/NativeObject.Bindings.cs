using System.Runtime.InteropServices;

namespace Unity.Core
{
    public unsafe partial struct NativeObject
    {
        [PdbSymbol("?GenerateTypeTree@@YAXAEBVObject@@AEAVTypeTree@@W4TransferInstructionFlags@@@Z")]
        static readonly GenerateTypeTreeDelegate s_GenerateTypeTree;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void GenerateTypeTreeDelegate(in NativeObject obj, ref TypeTree tree, TransferInstructionFlags flags);

        [PdbSymbol("?GetTypeTree@TypeTreeCache@@YA_NPEBVObject@@W4TransferInstructionFlags@@AEAVTypeTree@@@Z")]
        static readonly GetTypeTreeDelegate s_GetTypeTree;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate bool GetTypeTreeDelegate(in NativeObject obj, TransferInstructionFlags flags, ref TypeTree tree);

        [PdbSymbol("?GetSpriteAtlasDatabase@@YAAEAVSpriteAtlasDatabase@@XZ")]
        static readonly GetSpriteAtlasDatabaseDelegate s_GetSpriteAtlasDatabase;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate NativeObject* GetSpriteAtlasDatabaseDelegate();

        [PdbSymbol("?GetSceneVisibilityState@@YAAEAVSceneVisibilityState@@XZ")]
        static readonly GetSceneVisibilityStateDelegate s_GetSceneVisibilityState;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate NativeObject* GetSceneVisibilityStateDelegate();

        [PdbSymbol("?GetInspectorExpandedState@@YAAEAVInspectorExpandedState@@XZ")]
        static readonly GetInspectorExpandedStateDelegate s_GetInspectorExpandedState;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate NativeObject* GetInspectorExpandedStateDelegate();

        [PdbSymbol("?GetAnnotationManager@@YAAEAVAnnotationManager@@XZ")]
        static readonly GetAnnotationManagerDelegate s_GetAnnotationManager;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate NativeObject* GetAnnotationManagerDelegate();

        [PdbSymbol("?GetMonoManager@@YAAEAVMonoManager@@XZ")]
        static readonly GetMonoManagerDelegate s_GetMonoManager;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate NativeObject* GetMonoManagerDelegate();

        [PdbSymbol("?Produce@Object@@CAPEAV1@PEBVType@Unity@@0HUMemLabelId@@W4ObjectCreationMode@@@Z")]
        static readonly ProduceDelegate s_Produce;
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate NativeObject* ProduceDelegate(in RuntimeTypeInfo a, in RuntimeTypeInfo b, int instanceID, MemLabelId label, ObjectCreationMode creationMode);
    }
}
