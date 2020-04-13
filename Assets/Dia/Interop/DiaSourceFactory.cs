using System;
using System.Runtime.InteropServices;
using Dia2Lib;

namespace Dia.Interop
{
    public static class DiaSourceFactory
    {
        [DllImport("msdia140", ExactSpelling = true, BestFitMapping = false)]
        static extern int DllGetClassObject(
            in Guid classId,
            in Guid interfaceId,
            [MarshalAs(UnmanagedType.IUnknown)] out object @object
        );

        static int DllGetClassObject<T>(in Guid classId, out T @object)
            where T : class
        {
            var r   = DllGetClassObject(classId, typeof(T).GUID, out var box);
            @object = (T)box;
            return r;
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
