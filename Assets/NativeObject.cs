using System;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using Object = UnityEngine.Object;
using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using EasyHook;
#if UNITY_64
using nint = System.Int64;
#else
using nint = System.Int32;
#endif

public unsafe partial struct NativeObject
{
    static readonly BitVector32.Section MemLabelSection = BitVector32.CreateSection(1 << 10);

    static readonly BitVector32.Section IsRootOwnerSection = BitVector32.CreateSection(1 << 0, MemLabelSection);

    static readonly BitVector32.Section TemporaryFlagsSection = BitVector32.CreateSection(1 << 0, IsRootOwnerSection);

    static readonly BitVector32.Section HideFlagsSection = BitVector32.CreateSection(1 << 6, TemporaryFlagsSection);

    static readonly BitVector32.Section IsPersistentSection = BitVector32.CreateSection(1 << 0, HideFlagsSection);

    static readonly BitVector32.Section CachedTypeIndexSection = BitVector32.CreateSection(1 << 10, IsPersistentSection);

    public VirtualMethodTable* MethodTable;

    public int InstanceID;

    BitVector32 bitVector;

    public bool IsRootOwner
    {
        get => bitVector[IsRootOwnerSection] != 0;
        set => bitVector[IsRootOwnerSection] = value ? 1 : 0;
    }

    public bool TemporaryFlags
    {
        get => bitVector[TemporaryFlagsSection] != 0;
        set => bitVector[TemporaryFlagsSection] = value ? 1 : 0;
    }

    public HideFlags HideFlags
    {
        get => (HideFlags)bitVector[HideFlagsSection];
        set => bitVector[HideFlagsSection] = (int)value;
    }

    public bool IsPersistent
    {
        get => bitVector[IsPersistentSection] != 0;
        set => bitVector[IsPersistentSection] = value ? 1 : 0;
    }

    public uint CachedTypeIndex
    {
        get => (uint)bitVector[CachedTypeIndexSection];
        set => bitVector[CachedTypeIndexSection] = (int)value;
    }

    public ClassID ClassID => (ClassID)Rtti.RuntimeTypes[CachedTypeIndex].PersistentTypeID;

    static readonly Action EmptyAction = () => { };

    public bool TryGetTypeTree(TransferInstructionFlags flags, out TypeTree tree)
    {
        LocalHook.Release();

        // TypeTree::Pool::Release seems to cause crashes occasionally. True cause is currently unknown.
        // For now, it gets disabled whenever the type tree is retrieved. This will lead to memory leaks,
        // but the editor is only intended to be used to dump the tree and then closed.
        using (var hook = LocalHook.Create(TypeTree.Pool.Release, EmptyAction, null))
        {
            hook.ThreadACL.SetInclusiveACL(new[] { 0 });
            if (Rtti.RuntimeTypes[CachedTypeIndex].IsAbstract)
            {
                var type = Rtti.RuntimeTypes[CachedTypeIndex];
                using (new ProduceAbstractScope(&type))
                    return TryGetTypeTreeImpl(flags, out tree);
            }

            return TryGetTypeTreeImpl(flags, out tree);
        }
    }

    bool TryGetTypeTreeImpl(TransferInstructionFlags flags, out TypeTree tree)
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

    public static Object ToObject(NativeObject* ptr)
    {
        if (ptr == null)
            return null;

        var raw        = RawUnityObject.FromObject(new Object());
        raw.CachedPtr  = ptr;
        raw.InstanceID = ptr->InstanceID;
        return raw.Object;
    }

    public static NativeObject* FromObject(Object obj)
    {
        if (obj == null)
            return null;

        return RawUnityObject.FromObject(obj).CachedPtr;
    }

    public static NativeObject* GetOrProduce(in Rtti type)
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
                var producible = type;

                using (new ProduceAbstractScope(&producible))
                    return Produce(producible, producible, 0, default, ObjectCreationMode.Default);
            }

            return Produce(type, type, 0, default, ObjectCreationMode.Default);
        }
    }

    static NativeObject* ProduceAbstract(in Rtti type)
    {
        return null;
    }

    public static void DestroyTemporary(NativeObject* obj)
    {
        if (obj == null)
            return;

        // The object may have been forcefully produced from an abstract type.
        // We'll need to free the custom method table as well as use a special
        // method for destroying the object.
        if (Rtti.RuntimeTypes[obj->CachedTypeIndex].IsAbstract)
        {
            var methodTable = obj->MethodTable;
            DestroySingleObject(ref *obj);
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

    // I don't know the actual size of the vtable, but 60 pointers is big enough
    // to prevent a hard crash when allocating a custom one for use with internal
    // type tree APIs (at least on Unity 2020) so it's good enough.
    [StructLayout(LayoutKind.Explicit, Size = sizeof(nint) * 60)]
    public struct VirtualMethodTable
    {
        [FieldOffset(sizeof(nint) * 8)]
        IntPtr getTypeVirtualInternal;

        public delegate ref readonly Rtti GetTypeVirtualInternalDelegate(in NativeObject obj);

        public UnmanagedDelegate<GetTypeVirtualInternalDelegate> GetTypeVirtualInternal
        {
            get => getTypeVirtualInternal;
            set => getTypeVirtualInternal = value;
        }
    }
}
