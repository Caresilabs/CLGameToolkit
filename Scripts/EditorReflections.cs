using System;
using System.Reflection;

public static class EditorReflections
{
    public static T GetPrivateVariable<T>(object obj, string name)
    {
        FieldInfo fi = GetFieldInfo(obj, name);

        return fi != null ? (T)fi.GetValue(obj) : default;
    }

    public static bool SetPrivateVariable<T>(object obj, string name, T data)
    {
        FieldInfo fi = GetFieldInfo(obj, name);

        if (fi != null)
        {
            fi.SetValue(obj, data);
            return true;
        }

        return false;
    }

    public static R GetStaticVariable<T, R>(string name)
    {
        Type type = typeof(T);
        FieldInfo info = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Static);
        return (R)info.GetValue(null);
    }

    public static void SetStaticVariable<T>(string name, object value)
    {
        Type type = typeof(T);
        FieldInfo info = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Static);
        info.SetValue(null, value);
    }

    public static FieldInfo GetFieldInfo(object obj, string name)
    {
        System.Reflection.FieldInfo fi = null;
        var t = obj.GetType();

        while (t != null)
        {
            fi = t.GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            if (fi != null) break;

            t = t.BaseType;
        }

        return fi;
    }

#if UNITY_EDITOR
    public static UnityEngine.Transform GetDefaultParent()
    {
        return typeof(UnityEditor.SceneView).GetMethod("GetDefaultParentObjectIfSet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            .Invoke(null, null) as UnityEngine.Transform;
    }


#endif
}
