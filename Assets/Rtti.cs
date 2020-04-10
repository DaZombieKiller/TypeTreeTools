using System;
using System.Runtime.InteropServices;

public unsafe partial struct Rtti
{
    public Rtti* BaseClass;
    IntPtr producer;
    readonly IntPtr name;
    readonly IntPtr nativeNamespace;
#if UNITY_2017_2_OR_NEWER
    readonly IntPtr module;
#endif
    public ClassID PersistentTypeID;
    public int ByteSize;
    public uint RuntimeTypeIndex;
    public uint DescendantCount;
    public bool IsAbstract;
    public bool IsSealed;
#if UNITY_2017_2_OR_NEWER
    public bool IsEditorOnly;
#endif

    public FunctionPointer<ObjectProducerDelegate> Producer
    {
        get => new FunctionPointer<ObjectProducerDelegate> { Pointer = producer };
        set => producer = value.Pointer;
    }

    public string Name => Marshal.PtrToStringAnsi(name);

    public string NativeNamespace => Marshal.PtrToStringAnsi(nativeNamespace);

#if UNITY_2017_2_OR_NEWER
    public string Module => Marshal.PtrToStringAnsi(module);
#endif
}
