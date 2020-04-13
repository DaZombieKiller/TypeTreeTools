using System;
using System.Runtime.InteropServices;
using Dia2Lib;

namespace Dia.Interop
{
    public static class DiaSourceFactory
    {
        [DllImport("msdia140", ExactSpelling = true, BestFitMapping = false, PreserveSig = false)]
        static extern void DllGetClassObject(
            in Guid classId,
            in Guid interfaceId,
            [MarshalAs(UnmanagedType.IUnknown)] out object @object
        );

        static void DllGetClassObject<T>(in Guid classId, out T @object)
            where T : class
        {
            DllGetClassObject(classId, typeof(T).GUID, out var box);
            @object = box as T;
        }

        public static IDiaDataSource CreateInstance()
        {
            DllGetClassObject(
                typeof(DiaSourceClass).GUID,
                out IClassFactory factory
            );
            
            try
            {
                factory.CreateInstance(null, out IDiaDataSource source);
                return source;
            }
            finally
            {
                Marshal.ReleaseComObject(factory);
            }
        }
    }
}
