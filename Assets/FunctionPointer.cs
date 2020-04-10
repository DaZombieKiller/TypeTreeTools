using System;
using System.Runtime.InteropServices;

public struct FunctionPointer<T>
    where T : Delegate
{
    public IntPtr Pointer;

    public FunctionPointer(IntPtr pointer) => Pointer = pointer;

    public FunctionPointer(T method)
    {
        Pointer  = IntPtr.Zero;
        Delegate = method;
    }

    public T Delegate
    {
        get => Marshal.GetDelegateForFunctionPointer<T>(Pointer);
        set => Pointer = Marshal.GetFunctionPointerForDelegate(value);
    }

    public object DynamicInvoke(params object[] args)
    {
        return Delegate.DynamicInvoke(args);
    }

    public static implicit operator FunctionPointer<T>(IntPtr pointer) => new FunctionPointer<T>(pointer);

    public static implicit operator FunctionPointer<T>(T method) => new FunctionPointer<T>(method);

    public static implicit operator IntPtr(FunctionPointer<T> unmanaged) => unmanaged.Pointer;

    public static implicit operator T(FunctionPointer<T> unmanaged) => unmanaged.Delegate;
}
