using System;
using System.Reflection;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class ReflectionImportAttribute : ImportAttribute
{
    public Type Type { get; }

    public string MethodName { get; }

    public ReflectionImportAttribute(string typeName, string methodName)
    {
        Type       = Type.GetType(typeName, throwOnError: true);
        MethodName = methodName;
    }

    public ReflectionImportAttribute(Type type, string methodName)
    {
        Type       = type;
        MethodName = methodName;
    }

    static ReflectionImportAttribute()
    {
        foreach (var field in GetFieldsWithAttribute<ReflectionImportAttribute>())
        {
            if (!field.IsStatic)
            {
                Debug.LogErrorFormat("{0} must be static.", field.Name);
                continue;
            }

            if (!field.FieldType.IsSubclassOf(typeof(Delegate)))
            {
                Debug.LogErrorFormat("{0} must be of delegate type.", field.Name);
                continue;
            }

            var attribute = field.GetCustomAttribute<ReflectionImportAttribute>();
            var method    = attribute.Type.GetMethod(attribute.MethodName, AllMemberFlags);
            field.SetValue(null, method.CreateDelegate(field.FieldType));
        }
    }
}
