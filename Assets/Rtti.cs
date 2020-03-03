using System;
using System.Runtime.InteropServices;

public unsafe partial struct Rtti
{
    public Rtti* BaseClass;
    IntPtr producer;
    readonly IntPtr namePtr;
    readonly IntPtr nativeNamespacePtr;
#if UNITY_2017_2_OR_NEWER
    readonly IntPtr modulePtr;
#endif
    public int PersistentTypeID;
    public int ByteSize;
    public uint RuntimeTypeIndex;
    public uint DescendantCount;
    public bool IsAbstract;
    public bool IsSealed;
#if UNITY_2017_2_OR_NEWER
    public bool IsEditorOnly;
#endif

    public UnmanagedDelegate<ObjectProducerDelegate> Producer
    {
        get => new UnmanagedDelegate<ObjectProducerDelegate> { Pointer = producer };
        set => producer = value.Pointer;
    }

    public string Name => Marshal.PtrToStringAnsi(namePtr);

    public string NativeNamespace => Marshal.PtrToStringAnsi(nativeNamespacePtr);

#if UNITY_2017_2_OR_NEWER
    public string Module => Marshal.PtrToStringAnsi(modulePtr);
#endif
}
