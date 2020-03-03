using System;
using Object = UnityEngine.Object;

public unsafe partial struct NativeObject
{
    [ReflectionImport(typeof(Object), nameof(ForceLoadFromInstanceID))]
    static readonly Func<int, Object> ForceLoadFromInstanceID;

    [ReflectionImport(typeof(Object), nameof(FindObjectFromInstanceID))]
    static readonly Func<int, Object> FindObjectFromInstanceID;

    [ReflectionImport(typeof(Object), nameof(GetCachedPtr))]
    static readonly Func<Object, IntPtr> GetCachedPtr;

    [PdbImport("?Produce@Object@@CAPEAV1@PEBVType@Unity@@0HUMemLabelId@@W4ObjectCreationMode@@@Z")]
    static readonly ProduceDelegate Produce;
    delegate NativeObject* ProduceDelegate(in Rtti a, in Rtti b, int instanceID, MemoryLabel label, ObjectCreationMode creationMode);

    [PdbImport("?GetSpriteAtlasDatabase@@YAAEAVSpriteAtlasDatabase@@XZ")]
    static readonly GetSpriteAtlasDatabaseDelegate GetSpriteAtlasDatabase;
    delegate NativeObject* GetSpriteAtlasDatabaseDelegate();

    [PdbImport("?GetSceneVisibilityState@@YAAEAVSceneVisibilityState@@XZ")]
    static readonly GetSceneVisibilityStateDelegate GetSceneVisibilityState;
    delegate NativeObject* GetSceneVisibilityStateDelegate();

    [PdbImport("?GetInspectorExpandedState@@YAAEAVInspectorExpandedState@@XZ")]
    static readonly GetInspectorExpandedStateDelegate GetInspectorExpandedState;
    delegate NativeObject* GetInspectorExpandedStateDelegate();

    [PdbImport("?GetAnnotationManager@@YAAEAVAnnotationManager@@XZ")]
    static readonly GetAnnotationManagerDelegate GetAnnotationManager;
    delegate NativeObject* GetAnnotationManagerDelegate();

    [PdbImport("?GetMonoManager@@YAAEAVMonoManager@@XZ")]
    static readonly GetMonoManagerDelegate GetMonoManager;
    delegate NativeObject* GetMonoManagerDelegate();

    [PdbImport("?GenerateTypeTree@@YAXAEBVObject@@AEAVTypeTree@@W4TransferInstructionFlags@@@Z")]
    static readonly GenerateTypeTreeDelegate GenerateTypeTree;
    delegate void GenerateTypeTreeDelegate(in NativeObject obj, out TypeTree tree, TransferInstructionFlags flags);

    [PdbImport("?GetTypeTree@TypeTreeCache@@YA_NPEBVObject@@W4TransferInstructionFlags@@AEAVTypeTree@@@Z")]
    static readonly GetTypeTreeDelegate GetTypeTree;
    delegate bool GetTypeTreeDelegate(in NativeObject obj, TransferInstructionFlags flags, out TypeTree tree);

    [PdbImport("?DestroySingleObject@@YAXPEAVObject@@@Z")]
    static readonly DestroySingleObjectDelegate DestroySingleObject;
    unsafe delegate void DestroySingleObjectDelegate(ref NativeObject obj);
}
