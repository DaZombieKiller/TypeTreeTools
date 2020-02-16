using System;
using System.Reflection;
using System.Collections.Specialized;
using Object = UnityEngine.Object;
using UnityEngine;

public unsafe readonly struct NativeObject
{
    static readonly Func<int, Object> ForceLoadFromInstanceID;
    static readonly Func<int, Object> FindObjectFromInstanceID;
    static readonly Func<Object, IntPtr> GetCachedPtr;

    static NativeObject()
    {
        ForceLoadFromInstanceID = (Func<int, Object>)typeof(Object)
            .GetMethod("ForceLoadFromInstanceID", BindingFlags.NonPublic | BindingFlags.Static)
            .CreateDelegate(typeof(Func<int, Object>));

        FindObjectFromInstanceID = (Func<int, Object>)typeof(Object)
            .GetMethod("FindObjectFromInstanceID", BindingFlags.NonPublic | BindingFlags.Static)
            .CreateDelegate(typeof(Func<int, Object>));

        GetCachedPtr = (Func<Object, IntPtr>)typeof(Object)
            .GetMethod("GetCachedPtr", BindingFlags.NonPublic | BindingFlags.Instance)
            .CreateDelegate(typeof(Func<Object, IntPtr>));
    }

#pragma warning disable IDE0051 // Remove unused private members
    readonly IntPtr methodTable;
#pragma warning restore IDE0051
    public readonly int InstanceID;
    readonly BitVector32 objectBits;

    static readonly BitVector32.Section UnknownSection = BitVector32.CreateSection(1 << 12);

    static readonly BitVector32.Section HideFlagsSection = BitVector32.CreateSection(1 << 7, UnknownSection);

    static readonly BitVector32.Section RuntimeTypeIndexSection = BitVector32.CreateSection(1 << 13, HideFlagsSection);

    public HideFlags HideFlags => (HideFlags)objectBits[HideFlagsSection];

    public uint RuntimeTypeIndex => (uint)objectBits[RuntimeTypeIndexSection];

    public ClassID ClassID => (ClassID)UnityType.RuntimeTypes[RuntimeTypeIndex].PersistentTypeID;

    [PdbImport("?Produce@Object@@CAPEAV1@PEBVType@Unity@@0HUMemLabelId@@W4ObjectCreationMode@@@Z")]
    static readonly ProduceDelegate Produce;
    delegate NativeObject* ProduceDelegate(in UnityType a, in UnityType b, int instanceID, MemoryLabel label, ObjectCreationMode creationMode);

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

    [PdbImport("?GenerateStrippedTypeTree@@YAXAEBVObject@@AEAVTypeTree@@AEBUBuildUsageTag@@W4TransferInstructionFlags@@@Z")]
    static readonly GenerateStrippedTypeTreeDelegate GenerateStrippedTypeTreeMethod;
    delegate void GenerateStrippedTypeTreeDelegate(in NativeObject obj, out TypeTree tree, BuildUsageTag* tag, TransferInstructionFlags flags);

    public bool TryGetTypeTree(out TypeTree tree)
    {
        // Unity 2019 and beyond
        if (GetTypeTree != null)
            return GetTypeTree(in this, 0, out tree);

        // Unity 2018 and older
        GenerateTypeTree(in this, out tree, 0);
        return true;
    }

    public void GenerateStrippedTypeTree(out TypeTree tree)
    {
    #if UNITY_EDITOR
        GenerateStrippedTypeTreeMethod(in this, out tree, null, 0);
    #else
        TryGetTypeTree(out tree);
    #endif
    }

    public static Object ToObject(NativeObject* obj)
    {
        if (obj == null)
            return null;

        return ToObject(in *obj);
    }

    public static Object ToObject(in NativeObject obj)
    {
        var managedObject = FindObjectFromInstanceID(obj.InstanceID);

        if (managedObject)
            return managedObject;

        return ForceLoadFromInstanceID(obj.InstanceID);
    }

    public static NativeObject* FromObject(Object obj)
    {
        return (NativeObject*)GetCachedPtr(obj);
    }

    public static NativeObject* FromType(in UnityType type)
    {
        switch ((ClassID)type.PersistentTypeID)
        {
        // These types are singletons, and creating new instances of them
        // via Produce will cause an assertion failure and crash Unity.
    #if UNITY_EDITOR
        case ClassID.SpriteAtlasDatabase:
            return GetSpriteAtlasDatabase();
        case ClassID.SceneVisibilityState:
            return GetSceneVisibilityState();
        case ClassID.InspectorExpandedState:
            return GetInspectorExpandedState();
        case ClassID.AnnotationManager:
            return GetAnnotationManager();
    #endif
        case ClassID.MonoManager:
            return GetMonoManager();
        case ClassID.GameObject:
            // Unity will crash if Produce is used to create a GameObject.
            // The crash is not immediate, but seemingly on the next frame.
            return FromObject(new GameObject());
        default:
            return Produce(in type, in type, 0, default, 0);
        }
    }

    public static void DestroyTemporary(NativeObject* obj)
    {
        if (obj == null)
            return;

        // AssetBundle should not be destroyed via Destroy or DestroyImmediate,
        // but rather through AssetBundle.Unload. However, because the temporary
        // object we create is not exactly a "real" AssetBundle, doing so will
        // spit out an error. Further down the line, the project should include
        // a "dummy" asset bundle which will be loaded instead of using Produce.
        if (obj->ClassID == ClassID.AssetBundle || obj->ClassID.IsSingletonType())
            return;

        Object.DestroyImmediate(ToObject(obj));
    }
}
