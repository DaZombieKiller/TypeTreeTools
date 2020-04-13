using System;
using System.Runtime.InteropServices;

namespace Dia.Interop
{
    [ComImport]
    [Guid("00000001-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IClassFactory
    {
        [PreserveSig]
        int CreateInstance(
            [MarshalAs(UnmanagedType.IUnknown)] object outer,
            in Guid interfaceId,
            [MarshalAs(UnmanagedType.IUnknown)] out object instance
        );

        [PreserveSig]
        int LockServer(bool fLock);
    }

    public static class IClassFactoryExtensions
    {
        public static int CreateInstance<T>(this IClassFactory factory, object outer, out T instance)
            where T : class
        {
            var r    = factory.CreateInstance(outer, typeof(T).GUID, out var @object);
            instance = (T)@object;
            return r;
        }
    }
}
