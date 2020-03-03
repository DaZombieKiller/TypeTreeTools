using System;
using System.Runtime.InteropServices;

public struct UnmanagedDelegate<T>
    where T : Delegate
{
    public IntPtr Pointer;

    public UnmanagedDelegate(IntPtr pointer) => Pointer = pointer;

    public UnmanagedDelegate(T method)
    {
        Pointer  = IntPtr.Zero;
        Delegate = method;
    }

    public T Delegate
    {
        get => Marshal.GetDelegateForFunctionPointer<T>(Pointer);
        set => Pointer = Marshal.GetFunctionPointerForDelegate(value);
    }

    public object Invoke(params object[] args)
    {
        return Delegate.DynamicInvoke(args);
    }

    public static implicit operator UnmanagedDelegate<T>(IntPtr pointer) => new UnmanagedDelegate<T>(pointer);

    public static implicit operator UnmanagedDelegate<T>(T method) => new UnmanagedDelegate<T>(method);

    public static implicit operator IntPtr(UnmanagedDelegate<T> unmanaged) => unmanaged.Pointer;

    public static implicit operator T(UnmanagedDelegate<T> unmanaged) => unmanaged.Delegate;
}
