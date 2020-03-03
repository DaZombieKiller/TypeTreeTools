using System.Runtime.CompilerServices;
using Object = UnityEngine.Object;

public unsafe sealed class RawUnityObject
{
    public NativeObject* CachedPtr;

    public int InstanceID;

    public Object Object => Unsafe.As<Object>(this);

    RawUnityObject()
    {
    }

    public static RawUnityObject FromObject(Object obj)
    {
        return Unsafe.As<RawUnityObject>(obj);
    }
}
