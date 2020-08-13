using System;
using System.Collections.Specialized;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Unity.Core
{
    public unsafe partial struct NativeObject
    {
        static NativeObject() => PdbSymbolImporter.EnsureInitialized();

        static readonly BitVector32.Section MemLabelIdentifierSection = BitVector32.CreateSection(1 << 11);

        static readonly BitVector32.Section TemporaryFlagsSection = BitVector32.CreateSection(1 << 0, MemLabelIdentifierSection);

        static readonly BitVector32.Section HideFlagsSection = BitVector32.CreateSection(1 << 6, TemporaryFlagsSection);

        static readonly BitVector32.Section IsPersistentSection = BitVector32.CreateSection(1 << 0, HideFlagsSection);

        static readonly BitVector32.Section CachedTypeIndexSection = BitVector32.CreateSection(1 << 10, IsPersistentSection);

        public IntPtr* VirtualFunctionTable;

        public int InstanceID;

        BitVector32 bits;

        // There are more fields but they aren't needed.

        public byte TemporaryFlags
        {
            get { return (byte)bits[TemporaryFlagsSection]; }
            set { bits[TemporaryFlagsSection] = value; }
        }

        public HideFlags HideFlags
        {
            get { return (HideFlags)bits[HideFlagsSection]; }
            set { bits[HideFlagsSection] = (int)value; }
        }

        public bool IsPersistent
        {
            get { return bits[IsPersistentSection] != 0; }
            set { bits[IsPersistentSection] = value ? 1 : 0; }
        }

        public uint CachedTypeIndex
        {
            get { return (uint)bits[CachedTypeIndexSection]; }
            set { bits[CachedTypeIndexSection] = (int)value; }
        }

        public bool GetTypeTree(TransferInstructionFlags flags, ref TypeTree tree)
        {
            if (s_GetTypeTree != null)
                return s_GetTypeTree(this, flags, ref tree);

            if (s_GenerateTypeTree != null)
            {
                s_GenerateTypeTree(this, ref tree, flags);
                return true;
            }

            throw new NotImplementedException();
        }

        public static NativeObject* GetSpriteAtlasDatabase()
        {
            return s_GetSpriteAtlasDatabase();
        }

        public static NativeObject* GetSceneVisibilityState()
        {
            return s_GetSceneVisibilityState();
        }

        public static NativeObject* GetInspectorExpandedState()
        {
            return s_GetInspectorExpandedState();
        }

        public static NativeObject* GetAnnotationManager()
        {
            return s_GetAnnotationManager();
        }

        public static NativeObject* GetMonoManager()
        {
            return s_GetMonoManager();
        }

        public static NativeObject* Produce(in RuntimeTypeInfo type, int instanceID, ObjectCreationMode creationMode)
        {
            // TODO: Support producing abstract types. To do this, the following steps are necessary:
            //       1. Replace T::VirtualRedirectTransfer with T::Transfer. This can be done by either
            //          hooking the method via EasyHook, or modifying the virtual function table.
            //          This works because both methods have compatible signatures.
            //       2. Create a new Factory method for the type, by locating its constructor function
            //          and using that to create a new delegate.
            //       3. Create a new RuntimeTypeInfo based on the original, with the new Factory method.
            //          It also needs to have the IsAbstract field set to false.
            //       4. Hook T::GetTypeVirtualInternal to return the appropriate RuntimeTypeInfo.
            if (type.IsAbstract)
                return null;

            // TODO: Why does this take two types?
            return s_Produce(type, type, instanceID, new MemLabelId(), creationMode);
        }

        public static NativeObject* GetOrProduce(in RuntimeTypeInfo type) => type.PersistentTypeID switch
        {
            PersistentTypeID.SpriteAtlasDatabase    => GetSpriteAtlasDatabase(),
            PersistentTypeID.SceneVisibilityState   => GetSceneVisibilityState(),
            PersistentTypeID.InspectorExpandedState => GetInspectorExpandedState(),
            PersistentTypeID.AnnotationManager      => GetAnnotationManager(),
            PersistentTypeID.MonoManager            => GetMonoManager(),
            _                                       => Produce(type, 0, ObjectCreationMode.Default),
        };

        public static void DestroyIfNotSingletonOrPersistent(NativeObject* obj)
        {
            if (obj->IsPersistent)
                return;

            switch (RuntimeTypes.Types[obj->CachedTypeIndex]->PersistentTypeID)
            {
            case PersistentTypeID.SpriteAtlasDatabase:
            case PersistentTypeID.SceneVisibilityState:
            case PersistentTypeID.InspectorExpandedState:
            case PersistentTypeID.AnnotationManager:
            case PersistentTypeID.MonoManager:
            case PersistentTypeID.AssetBundle:
                return;
            }

            var managed = EditorUtility.InstanceIDToObject(obj->InstanceID);
            Object.DestroyImmediate(managed);
        }
    }
}
