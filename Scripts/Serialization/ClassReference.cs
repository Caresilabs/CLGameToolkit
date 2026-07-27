using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CLGameToolkit.Serialization
{
    [Serializable]
    public class ClassReference<T>
    {
        [SerializeField] private string ClassTypeName;

        public Type ClassType
        {
            get
            {
                if (string.IsNullOrEmpty(ClassTypeName)) return null;
                return Type.GetType(ClassTypeName);
            }
        }

        public void SetClassType(Type type)
        {
            if (type == null || !typeof(T).IsAssignableFrom(type))
            {
                Debug.LogError($"Invalid type assignment. {type?.Name} is not assignable to {typeof(T).Name}");
                return;
            }
            ClassTypeName = type.AssemblyQualifiedName;
        }

        public T CreateInstance()
        {
            if (ClassType == null)
                return default;

            return (T)Activator.CreateInstance(ClassType);
        }
    }


#if UNITY_EDITOR

    [UnityEditor.CustomPropertyDrawer(typeof(ClassReference<>), true)]
    public class ClassReferenceDrawer : UnityEditor.PropertyDrawer
    {
        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {
            UnityEditor.SerializedProperty classTypeNameProp = property.FindPropertyRelative("ClassTypeName");

            Type fieldType = fieldInfo.FieldType;
            Type baseType = fieldType.GetGenericArguments()[0];

            List<Type> derivedTypes = GetAllDerivedTypes(baseType);
            string[] displayNames = derivedTypes.Select(t => t?.FullName ?? "Null").ToArray();

            int currentIndex = 0;
            if (!string.IsNullOrEmpty(classTypeNameProp.stringValue))
            {
                var currentType = Type.GetType(classTypeNameProp.stringValue);
                currentIndex = derivedTypes.IndexOf(currentType);
                if (currentIndex < 0) currentIndex = 0;
            }

            UnityEditor.EditorGUI.BeginProperty(position, label, property);
            int newIndex = UnityEditor.EditorGUI.Popup(position, property.displayName, currentIndex, displayNames);
            if (newIndex != currentIndex && newIndex >= 0 && newIndex < derivedTypes.Count)
            {
                classTypeNameProp.stringValue = newIndex == 0 ? string.Empty : derivedTypes[newIndex].AssemblyQualifiedName;
            }

            UnityEditor.EditorGUI.EndProperty();
        }

        private static List<Type> GetAllDerivedTypes(Type baseType)
        {
            var result = UnityEditor.TypeCache
                .GetTypesDerivedFrom(baseType)
                .Where(t => !t.IsAbstract)
                .OrderBy(t => t.Name)
                .ToList();

            if (!baseType.IsAbstract && !baseType.IsInterface)
                result.Insert(0, baseType);

            result.Insert(0, null);

            return result;
        }

        private static List<Type> GetAllDerivedTypesOld(Type baseType)
        {
            var result = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var asm in assemblies)
            {
                Type[] types;
                try { types = asm.GetTypes(); }
                catch (ReflectionTypeLoadException e) { types = e.Types.Where(t => t != null).ToArray(); }

                foreach (var t in types)
                {
                    if (t == null || t.IsAbstract) continue;

                    if (!t.IsAbstract && baseType.IsAssignableFrom(t))
                        result.Add(t);
                }
            }

            var list = result.OrderBy(t => t.Name).ToList();
            list.Insert(0, null);

            return list;
        }
    }
#endif
}

