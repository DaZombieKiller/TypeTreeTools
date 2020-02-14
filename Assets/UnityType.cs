using System;

public unsafe struct UnityType
{
    public UnityType* BaseClass;
    public IntPtr ProduceHelper;
    public IntPtr Name;
    public IntPtr NativeNamespace;
#if UNITY_2017_1_OR_NEWER
    public IntPtr Module;
#endif
    public int PersistentTypeID;
    public int ByteSize;
    public uint RuntimeTypeIndex;
    public uint DescendantCount;
    public bool IsAbstract;
    public bool IsSealed;
#if UNITY_2017_1_OR_NEWER
    public bool IsEditorOnly;
#endif

    [PdbImport("?ms_runtimeTypes@RTTI@@0URuntimeTypeArray@1@A")]
    static readonly IntPtr runtimeTypes;

    public static ref readonly RuntimeTypeArray RuntimeTypes
    {
        get => ref *(RuntimeTypeArray*)runtimeTypes;
    }

    public static ref readonly UnityType GetByClassID(ClassID id)
    {
        foreach (ref var type in RuntimeTypes)
        {
            if (type.PersistentTypeID == (int)id)
                return ref type;
        }

        throw new ArgumentException(null, nameof(id));
    }

    public bool HasTypeTree
    {
        get
        {
            if (IsAbstract)
                return false;

            // Investigate
            switch ((ClassID)PersistentTypeID)
            {
            case ClassID.MonoBehaviour:
            case ClassID.ScriptedImporter:
            case ClassID.SerializableManagedHost:
            case ClassID.SerializableManagedRefTestClass:
            case ClassID.Vector3f:
            case ClassID.AudioMixerLiveUpdateBool:
            case ClassID.@bool:
            case ClassID.@void:
            case ClassID.RootMotionData:
            case ClassID.AudioMixerLiveUpdateFloat:
            case ClassID.MonoObject:
            case ClassID.Collision2D:
            case ClassID.Polygon2D:
            case ClassID.Collision:
            case ClassID.@float:
            case ClassID.@int:
                return false;
            }

            return true;
        }
    }
}
