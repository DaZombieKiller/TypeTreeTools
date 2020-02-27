using System;
using System.Reflection;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Object = UnityEngine.Object;
using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

// I don't know the actual size of the vtable, but 60 pointers is big enough
// to prevent a hard crash when allocating a custom one for use with internal
// type tree APIs (at least on Unity 2020) so it's good enough.
[StructLayout(LayoutKind.Sequential, Size = 8 * 60)]
public struct ObjectMethodTable
{
    readonly IntPtr unknown0;
    readonly IntPtr unknown1;
    readonly IntPtr unknown2;
    readonly IntPtr unknown3;
    readonly IntPtr unknown4;
    readonly IntPtr unknown5;
    readonly IntPtr unknown6;
    readonly IntPtr unknown7;
    public IntPtr GetTypeVirtualInternal;
}

public unsafe struct NativeObject
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

    public ObjectMethodTable* MethodTable;
    public int InstanceID;
    BitVector32 objectBits;

    static readonly BitVector32.Section UnknownSection = BitVector32.CreateSection(1 << 12);

    static readonly BitVector32.Section HideFlagsSection = BitVector32.CreateSection(1 << 7, UnknownSection);

    static readonly BitVector32.Section RuntimeTypeIndexSection = BitVector32.CreateSection(1 << 13, HideFlagsSection);

    public HideFlags HideFlags
    {
        get => (HideFlags)objectBits[HideFlagsSection];
        set => objectBits[HideFlagsSection] = (int)value;
    }

    public uint RuntimeTypeIndex
    {
        get => (uint)objectBits[RuntimeTypeIndexSection];
        set => objectBits[RuntimeTypeIndexSection] = (int)value;
    }

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

    [PdbImport("?DestroySingleObject@@YAXPEAVObject@@@Z")]
    static readonly DestroySingleObjectDelegate DestroySingleObject;
    unsafe delegate void DestroySingleObjectDelegate(NativeObject* obj);

    public bool TryGetTypeTree(TransferInstructionFlags flags, out TypeTree tree)
    {
        if (GetTypeTree != null)
        {
            // Unity 2019 and beyond
            GetTypeTree(in this, flags, out tree);
            return true;
        }
        else if (GenerateTypeTree != null)
        {
            // Unity 2018 and older
            GenerateTypeTree(in this, out tree, flags);
            return true;
        }

        tree = default;
        return false;
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
        if (obj == null)
            return null;

        return (NativeObject*)GetCachedPtr(obj);
    }

    public static NativeObject* FromType(ref UnityType type, PdbService service)
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
            // If the type is abstract, we need to perform some voodoo to make it producible.
            if (type.IsAbstract)
            {
                using (new ProduceAbstractScope(service, (UnityType*)Unsafe.AsPointer(ref type)))
                    return Produce(in type, in type, 0, default, ObjectCreationMode.Default);
            }

            return Produce(in type, in type, 0, default, ObjectCreationMode.Default);
        }
    }

    public static void DestroyTemporary(NativeObject* obj)
    {
        if (obj == null)
            return;

        // The object may have been forcefully produced from an abstract type.
        // We'll need to free the custom method table as well as use a special
        // method for destroying the object.
        if (UnityType.RuntimeTypes[obj->RuntimeTypeIndex].IsAbstract)
        {
            var methodTable = obj->MethodTable;
            DestroySingleObject(obj);
            UnsafeUtility.Free(methodTable, Allocator.Persistent);
            return;
        }

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
