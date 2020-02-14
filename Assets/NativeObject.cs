using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Object = UnityEngine.Object;
using UnityEngine;

public unsafe readonly struct NativeObject
{
    static readonly Func<int, Object> FindObjectFromInstanceID;
    static readonly Func<Object, IntPtr> GetCachedPtr;

    static NativeObject()
    {
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
    public readonly uint CachedTypeIndex;

    public uint RuntimeTypeIndex => CachedTypeIndex >> 0x15;

    public ClassID ClassID => (ClassID)UnityType.RuntimeTypes[RuntimeTypeIndex].PersistentTypeID;

    [PdbImport("?Produce@Object@@CAPEAV1@PEBVType@Unity@@0HUMemLabelId@@W4ObjectCreationMode@@@Z")]
    static readonly ProduceDelegate Produce;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate NativeObject* ProduceDelegate(in UnityType a, in UnityType b, int instanceID, MemoryLabel label, int creationMode);

    [PdbImport("?GetSpriteAtlasDatabase@@YAAEAVSpriteAtlasDatabase@@XZ")]
    static readonly GetSpriteAtlasDatabaseDelegate GetSpriteAtlasDatabase;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate NativeObject* GetSpriteAtlasDatabaseDelegate();

    [PdbImport("?GetSceneVisibilityState@@YAAEAVSceneVisibilityState@@XZ")]
    static readonly GetSceneVisibilityStateDelegate GetSceneVisibilityState;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate NativeObject* GetSceneVisibilityStateDelegate();

    [PdbImport("?GetInspectorExpandedState@@YAAEAVInspectorExpandedState@@XZ")]
    static readonly GetInspectorExpandedStateDelegate GetInspectorExpandedState;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate NativeObject* GetInspectorExpandedStateDelegate();

    [PdbImport("?GetAnnotationManager@@YAAEAVAnnotationManager@@XZ")]
    static readonly GetAnnotationManagerDelegate GetAnnotationManager;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate NativeObject* GetAnnotationManagerDelegate();

    [PdbImport("?GetMonoManager@@YAAEAVMonoManager@@XZ")]
    static readonly GetMonoManagerDelegate GetMonoManager;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate NativeObject* GetMonoManagerDelegate();

    public static Object ToObject(NativeObject* obj)
    {
        return FindObjectFromInstanceID(obj->InstanceID);
    }

    public static Object ToObject(in NativeObject obj)
    {
        return FindObjectFromInstanceID(obj.InstanceID);
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
        case ClassID.SpriteAtlasDatabase:
            return GetSpriteAtlasDatabase();
        case ClassID.SceneVisibilityState:
            return GetSceneVisibilityState();
        case ClassID.InspectorExpandedState:
            return GetInspectorExpandedState();
        case ClassID.AnnotationManager:
            return GetAnnotationManager();
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
