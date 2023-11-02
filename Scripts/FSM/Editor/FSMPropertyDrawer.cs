using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomPropertyDrawer(typeof(FiniteStateMachine<>))]
public class FSMPropertyDrawer : PropertyDrawer
{
    private const string IdPropertyName = "<Id>k__BackingField";

    private Dictionary<string, ReorderableList> statesListCache = new Dictionary<string, ReorderableList>();
    private Dictionary<string, ReorderableList> transitionsListCache = new Dictionary<string, ReorderableList>();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var target = property.serializedObject.targetObject;
        var stateMachine = fieldInfo.GetValue(property.serializedObject.targetObject);

        FieldInfo currentStateField = stateMachine.GetType().GetField("currentState", BindingFlags.NonPublic | BindingFlags.Instance);
        FSMInternalState currentState = currentStateField.GetValue(stateMachine) as FSMInternalState;
        string currentStateName = currentState != null ? currentState.Id : "No State Active";


        EditorGUI.BeginProperty(position, label, property);

        var statesProp = property.FindPropertyRelative("editorStates");
        var transitionsProp = property.FindPropertyRelative("editorTransitions");

        GUILayout.Box($"Current State:\n**{currentStateName}**", GUILayout.Width(172), GUILayout.Height(48));

        EditorGUILayout.LabelField($"Finite State Machine <{stateMachine.GetType().GetGenericArguments()[0]}>");
        EditorGUILayout.Space();

        EditorGUI.BeginProperty(position, label, statesProp);
        ReorderableList list = GetStatesList(property, statesProp);
        list.DoLayoutList();
        EditorGUI.EndProperty();


        EditorGUI.BeginProperty(position, label, transitionsProp);
        ReorderableList transitionList = GetTransitionsList(property, transitionsProp);
        transitionList.DoLayoutList();
        EditorGUI.EndProperty();


        FieldInfo pi = stateMachine.GetType().GetField("editorStates", BindingFlags.NonPublic | BindingFlags.Instance);
        var currentStateList = (List<FSMInternalState>)(pi.GetValue(stateMachine));

        // Add required states
        var requiredStates = target.GetType().GetCustomAttributes<RequireState>()?.Select(x => x.types);
        if (requiredStates != null)
        {
            foreach (var attributesList in requiredStates)
            {
                foreach (var clazz in attributesList)
                {
                    if (!currentStateList.Any(state => state?.GetType() == clazz))
                    {
                        AddState(statesProp, clazz);
                    }
                }

            }
        }

        EditorGUI.EndProperty();
    }

    private ReorderableList GetStatesList(SerializedProperty property, SerializedProperty listProperty)
    {

        if (statesListCache.ContainsKey(property.propertyPath))
        {
            return statesListCache[property.propertyPath];
        }
        else
        {
            ReorderableList list = new ReorderableList(listProperty.serializedObject, listProperty, true, true, true, true);


            list.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "States");
            };

            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = listProperty.GetArrayElementAtIndex(index);

                if (element.managedReferenceValue == null)
                    return;

                var idProp = element.FindPropertyRelative(IdPropertyName);

                bool isEntryState = index == 0;
                string stateName = element.managedReferenceValue.GetType().Name;
                string overrideStateName = idProp.stringValue;


                if (isActive)
                {
                    idProp.stringValue = EditorGUI.TextField(new Rect(rect.x + rect.width * 0.7f, rect.y, rect.width * 0.3f, EditorGUIUtility.singleLineHeight), overrideStateName);

                    if (idProp.stringValue != overrideStateName)
                        EnsureUniqueStateNames(listProperty);
                }
                else if (stateName != idProp.stringValue)
                {
                    EditorGUI.LabelField(new Rect(rect.x + rect.width * 0.7f, rect.y, rect.width * 0.3f, EditorGUIUtility.singleLineHeight), overrideStateName);
                }

                EditorGUI.indentLevel = 1;

                EditorGUI.PropertyField(rect, element, includeChildren: true, label: new GUIContent(stateName + (isEntryState ? " (Entry State)" : "")));   //, new GUIContent(element.managedReferenceValue.GetType().Name));
            };

            list.elementHeightCallback += idx =>
            {
                SerializedProperty elementProp = listProperty.GetArrayElementAtIndex(idx);
                return EditorGUI.GetPropertyHeight(elementProp);
            };

            var stateMachineRef = fieldInfo.GetValue(property.serializedObject.targetObject);
            var genericArg = stateMachineRef.GetType().GetGenericArguments()[0];
            Type genericType = typeof(FSMState<>).MakeGenericType(new Type[] { genericArg });

            var allStateClasses = GetAllStateClasses(genericType);

            list.onAddDropdownCallback = (Rect buttonRect, ReorderableList l) =>
            {
                var menu = new GenericMenu();
                foreach (var clazz in allStateClasses)
                {
                    string prefix = clazz.GetCustomAttribute<StateGroupAttribute>()?.groupName;
                    menu.AddItem(new GUIContent((prefix != null ? $"{prefix}/" : "") + clazz.Name),
                    false, (data) =>
                    {
                        AddState(listProperty, clazz);
                    }, null);
                }

                menu.ShowAsContext();
            };

            statesListCache.Add(property.propertyPath, list);

            return statesListCache[property.propertyPath];
        }

    }

    private void AddState(SerializedProperty listProperty, Type clazz)
    {
        listProperty.serializedObject.Update();

        var index = listProperty.arraySize;
        listProperty.arraySize++;

        var element = listProperty.GetArrayElementAtIndex(index);
        var state = Activator.CreateInstance(clazz) as FSMInternalState;

        PropertyInfo idField = typeof(FSMInternalState).GetProperty("Id");
        idField.SetValue(state, clazz.Name);

        element.managedReferenceValue = state;

        EnsureUniqueStateNames(listProperty);

        listProperty.serializedObject.ApplyModifiedProperties();
    }

    private void EnsureUniqueStateNames(SerializedProperty listProperty)
    {
        //FieldInfo idField = typeof(FSMInternalState).GetField("Id", BindingFlags.NonPublic | BindingFlags.Instance);
        var ids = new HashSet<string>();

        for (int i = 0; i < listProperty.arraySize; i++)
        {
            var element = listProperty.GetArrayElementAtIndex(i);
            if (element?.managedReferenceValue != null)
            {
                var idProp = element.FindPropertyRelative(IdPropertyName);
                string id = idProp.stringValue;

                while (ids.Contains(id))
                {
                    id += " (1)";
                    idProp.stringValue = id;
                }

                ids.Add(id);

            }
        }
    }

    private IEnumerable<Type> GetAllStateClasses(Type baseClass)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSubclassOf(baseClass))
            .OrderBy(type => type.Name);
    }

    private ReorderableList GetTransitionsList(SerializedProperty property, SerializedProperty listProperty)
    {
        if (transitionsListCache.ContainsKey(property.propertyPath))
        {
            return transitionsListCache[property.propertyPath];
        }
        else
        {
            var statesProp = property.FindPropertyRelative("editorStates");

            var obj = fieldInfo.GetValue(property.serializedObject.targetObject);

            FieldInfo pi = obj.GetType().GetField("editorStates", BindingFlags.NonPublic | BindingFlags.Instance);
            PropertyInfo idField = typeof(FSMInternalState).GetProperty("Id");

            var allStates = (List<FSMInternalState>)(pi.GetValue(obj));

            ReorderableList list = new ReorderableList(listProperty.serializedObject, listProperty, true, true, true, true);

            list.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Transitions");
            };

            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                if (index >= listProperty.arraySize)
                {
                    return;
                }

                var element = listProperty.GetArrayElementAtIndex(index);

                var fromProperty = element.FindPropertyRelative("From");
                var toProperty = element.FindPropertyRelative("To");

                var fromOptions = allStates.Select(x => idField.GetValue(x) as string).Prepend("Any").ToArray();// new string[] { "Test", "Test 2" };
                var toOptions = allStates.Select(x => idField.GetValue(x) as string).Where(x => x != fromProperty.stringValue).ToArray();

                if (fromOptions.Length == 0 || toOptions.Length == 0)
                {
                    listProperty.DeleteArrayElementAtIndex(index);
                    return;
                }

                var fromNewValue = EditorGUI.Popup(new Rect(rect.x, rect.y, rect.width * 0.4f, EditorGUIUtility.singleLineHeight), Math.Max(0, Array.IndexOf(fromOptions, fromProperty.stringValue)), fromOptions);
                fromProperty.stringValue = fromOptions[Math.Min(fromNewValue, fromOptions.Length - 1)];

                EditorGUI.LabelField(new Rect(rect.x + rect.width * 0.43f, rect.y, rect.width * 0.4f, EditorGUIUtility.singleLineHeight), "→");

                var toNewValue = EditorGUI.Popup(new Rect(rect.x + rect.width * 0.5f, rect.y, rect.width * 0.4f, EditorGUIUtility.singleLineHeight), Math.Max(0, Array.IndexOf(toOptions, toProperty.stringValue)), toOptions);
                toProperty.stringValue = toOptions[Math.Min(toNewValue, toOptions.Length - 1)];
            };



            list.elementHeightCallback += idx =>
            {
                return EditorGUIUtility.singleLineHeight * 1;
            };


            transitionsListCache.Add(property.propertyPath, list);

            return transitionsListCache[property.propertyPath];
        }

    }

}
