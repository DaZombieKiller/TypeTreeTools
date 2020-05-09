using System;
using System.Reflection;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Unity.Core
{
    public unsafe static class ObjectUtility
    {
        static readonly Func<Object, IntPtr> s_GetCachedPtr;

        static ObjectUtility()
        {
            var info       = typeof(Object).GetMethod("GetCachedPtr", BindingFlags.NonPublic | BindingFlags.Instance);
            s_GetCachedPtr = (Func<Object, IntPtr>)info.CreateDelegate(typeof(Func<Object, IntPtr>));
        }

        public static NativeObject* GetCachedPointer(Object obj)
        {
            return (NativeObject*)s_GetCachedPtr(obj);
        }

        public static Object GetSpriteAtlasDatabase()
        {
            var native = NativeObject.GetSpriteAtlasDatabase();
            return EditorUtility.InstanceIDToObject(native->InstanceID);
        }

        public static Object GetSceneVisibilityState()
        {
            var native = NativeObject.GetSceneVisibilityState();
            return EditorUtility.InstanceIDToObject(native->InstanceID);
        }

        public static Object GetInspectorExpandedState()
        {
            var native = NativeObject.GetInspectorExpandedState();
            return EditorUtility.InstanceIDToObject(native->InstanceID);
        }

        public static Object GetAnnotationManager()
        {
            var native = NativeObject.GetAnnotationManager();
            return EditorUtility.InstanceIDToObject(native->InstanceID);
        }

        public static Object GetMonoManager()
        {
            var native = NativeObject.GetMonoManager();
            return EditorUtility.InstanceIDToObject(native->InstanceID);
        }
    }
}
