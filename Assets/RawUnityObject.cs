using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Object = UnityEngine.Object;

[StructLayout(LayoutKind.Sequential)]
public unsafe sealed class RawUnityObject
{
    public NativeObject* CachedPtr;

    public int InstanceID;

    public string UnityRuntimeErrorString;

    public Object Object => Unsafe.As<Object>(this);

    RawUnityObject()
    {
    }

    public static RawUnityObject FromObject(Object obj)
    {
        return Unsafe.As<RawUnityObject>(obj);
    }
}
