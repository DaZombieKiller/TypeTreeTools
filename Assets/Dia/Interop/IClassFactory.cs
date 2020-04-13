using System;
using System.Runtime.InteropServices;

namespace Dia.Interop
{
    [ComImport]
    [Guid("00000001-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IClassFactory
    {
        void CreateInstance(
            [MarshalAs(UnmanagedType.IUnknown)] object outer,
            in Guid interfaceId,
            [MarshalAs(UnmanagedType.IUnknown)] out object instance
        );

        void LockServer(bool @lock);
    }

    public static class IClassFactoryExtensions
    {
        public static void CreateInstance<T>(this IClassFactory factory, object outer, out T instance)
            where T : class
        {
            factory.CreateInstance(outer, typeof(T).GUID, out var @object);
            instance = @object as T;
        }
    }
}
