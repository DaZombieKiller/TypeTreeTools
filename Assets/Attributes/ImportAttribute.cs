using System;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
#if UNITY_EDITOR
using UnityEditor;
#else
using UnityEngine;
#endif

public abstract class ImportAttribute : Attribute
{
    protected const BindingFlags AllMemberFlags = 0
        | BindingFlags.Public | BindingFlags.NonPublic
        | BindingFlags.Static | BindingFlags.Instance;

    protected static IEnumerable<FieldInfo> GetFieldsWithAttribute<T>()
        where T : Attribute
    {
    #if !UNITY_EDITOR || !UNITY_2020_1_OR_NEWER
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        foreach (var type in assembly.GetTypes())
        foreach (var field in type.GetFields(AllMemberFlags))
        {
            var attribute = field.GetCustomAttribute<T>();

            if (attribute == null)
                continue;

            yield return field;
        }
    #else
        return TypeCache.GetFieldsWithAttribute<T>();
    #endif
    }

    protected static IEnumerable<Type> GetTypesDerivedFrom<T>()
    {
    #if !UNITY_EDITOR || !UNITY_2019_3_OR_NEWER
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        foreach (var type in assembly.GetTypes())
        {
            if (type.IsSubclassOf(typeof(T)))
                yield return type;
        }
    #else
        return TypeCache.GetTypesDerivedFrom<T>();
    #endif
    }

#if UNITY_EDITOR
    [InitializeOnLoadMethod]
#else
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
    static void Initialize()
    {
        foreach (Type type in GetTypesDerivedFrom<ImportAttribute>())
        {
            RuntimeHelpers.RunClassConstructor(type.TypeHandle);
        }
    }
}
