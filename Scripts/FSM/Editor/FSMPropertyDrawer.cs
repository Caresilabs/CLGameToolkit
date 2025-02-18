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

    private readonly Dictionary<UnityEngine.Object, ReorderableList> statesListCache = new();
    private readonly Dictionary<UnityEngine.Object, ReorderableList> transitionsListCache = new();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var target = property.serializedObject.targetObject;
        var stateMachine = fieldInfo.GetValue(target);

        FieldInfo currentStateField = stateMachine.GetType().GetField("currentState", BindingFlags.NonPublic | BindingFlags.Instance);
        FSMInternalState currentState = currentStateField.GetValue(stateMachine) as FSMInternalState;
        string currentStateName = currentState != null ? currentState.Id : "No State Active";

        EditorGUI.BeginProperty(position, label, property);

        var statesProp = property.FindPropertyRelative("editorStates");
        var transitionsProp = property.FindPropertyRelative("editorTransitions");
        var entityProperty = property.FindPropertyRelative("entity");

        GUILayout.Box($"Current State:\n**{currentStateName}**", GUILayout.Width(172), GUILayout.Height(48));

        /// Title
        var entityType = stateMachine.GetType().GetGenericArguments()[0];
        EditorGUILayout.LabelField($"Finite State Machine <{entityType}>", EditorStyles.largeLabel);

        /// Entity label
        if (entityProperty != null)
            EditorGUILayout.PropertyField(entityProperty, new GUIContent($"Entity ({entityType})"));
        else
            EditorGUILayout.LabelField($"Custom Entity:\t<{entityType}> (Be sure to call FSM.Init())");
        EditorGUILayout.Space();

        /// States editor
        EditorGUI.BeginProperty(position, label, statesProp);
        ReorderableList list = GetStatesList(property, statesProp);
        list.DoLayoutList();
        EditorGUI.EndProperty();

        /// Transition editor
        EditorGUI.BeginProperty(position, label, transitionsProp);
        ReorderableList transitionList = GetTransitionsList(property, transitionsProp);
        transitionList.DoLayoutList();
        EditorGUI.EndProperty();

        AddRequiredStates(target, statesProp, stateMachine);

        // SerializationUtility.ClearAllManagedReferencesWithMissingTypes(property.serializedObject.targetObject);
        EditorGUI.EndProperty();
    }

    private void AddRequiredStates(UnityEngine.Object target, SerializedProperty statesProp, object stateMachine)
    {
        if (Application.isPlaying)
            return;

        FieldInfo pi = stateMachine.GetType().GetField("editorStates", BindingFlags.NonPublic | BindingFlags.Instance);
        var currentStateList = (List<FSMInternalState>)(pi.GetValue(stateMachine));

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
    }

    private ReorderableList GetStatesList(SerializedProperty property, SerializedProperty listProperty)
    {
        if (statesListCache.ContainsKey(property.serializedObject.targetObject))
        {
            return statesListCache[property.serializedObject.targetObject];
        }

        ReorderableList list = new ReorderableList(listProperty.serializedObject, listProperty, true, true, true, true);

        list.drawHeaderCallback = rect =>
        {
            EditorGUI.LabelField(rect, "States");
        };

        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            SerializedProperty element = listProperty.GetArrayElementAtIndex(index);

            if (element.managedReferenceValue == null)
            {
                EditorGUI.LabelField(rect, $"Missing Class", EditorStyles.boldLabel);
                return;
            }


            SerializedProperty idProp = element.FindPropertyRelative(IdPropertyName);

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

            //EditorGUI.indentLevel = 1;
            rect.x += 16;
            EditorGUI.PropertyField(rect, element, new GUIContent(stateName + (isEntryState ? " (Entry State)" : "")), true);
        };

        list.elementHeightCallback += idx =>
        {
            SerializedProperty elementProp = listProperty.GetArrayElementAtIndex(idx);
            return EditorGUI.GetPropertyHeight(elementProp);
        };

        object stateMachineRef = fieldInfo.GetValue(property.serializedObject.targetObject);
        Type genericArg = stateMachineRef.GetType().GetGenericArguments()[0];
        Type genericType = typeof(FSMState<>).MakeGenericType(new Type[] { genericArg });

        var allStateClasses = GetAllStateClasses(genericType);

        list.onAddDropdownCallback = (Rect buttonRect, ReorderableList l) =>
        {
            var menu = new GenericMenu();
            foreach (Type clazz in allStateClasses)
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

        statesListCache.Add(property.serializedObject.targetObject, list);

        return list;
    }

    private void AddState(SerializedProperty listProperty, Type clazz)
    {
        listProperty.serializedObject.Update();

        var index = listProperty.arraySize;
        listProperty.arraySize++;

        SerializedProperty element = listProperty.GetArrayElementAtIndex(index);
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
            SerializedProperty element = listProperty.GetArrayElementAtIndex(i);
            if (element?.managedReferenceValue != null)
            {
                SerializedProperty idProp = element.FindPropertyRelative(IdPropertyName);
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
        if (transitionsListCache.ContainsKey(property.serializedObject.targetObject))
        {
            return transitionsListCache[property.serializedObject.targetObject];
        }

        SerializedProperty statesProp = property.FindPropertyRelative("editorStates");

        var obj = fieldInfo.GetValue(property.serializedObject.targetObject);

        FieldInfo pi = obj.GetType().GetField("editorStates", BindingFlags.NonPublic | BindingFlags.Instance);
        PropertyInfo idField = typeof(FSMInternalState).GetProperty("Id");

        IEnumerable<FSMInternalState> allStates = ((List<FSMInternalState>)(pi.GetValue(obj))).Where(x => x != null);

        ReorderableList list = new ReorderableList(listProperty.serializedObject, listProperty, true, true, true, true);

        list.drawHeaderCallback = rect =>
        {
            EditorGUI.LabelField(rect, "Transitions");
        };

        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            if (index >= listProperty.arraySize)
                return;

            SerializedProperty element = listProperty.GetArrayElementAtIndex(index);
            SerializedProperty fromProperty = element.FindPropertyRelative("From");
            SerializedProperty toProperty = element.FindPropertyRelative("To");

            var fromOptions = allStates.Select(x => idField.GetValue(x) as string).Prepend("Any").ToArray();
            var toOptions = allStates.Select(x => idField.GetValue(x) as string).Where(x => x != fromProperty.stringValue).ToArray();

            if (fromOptions.Length == 0 || toOptions.Length == 0)
            {
                listProperty.DeleteArrayElementAtIndex(index);
                return;
            }

            Rect expandableRect = new Rect(rect.x + 12, rect.y, rect.width * 0.4f, EditorGUIUtility.singleLineHeight);
            element.isExpanded = EditorGUI.Foldout(expandableRect, element.isExpanded, "");

            int fromNewValue = EditorGUI.Popup(new Rect(expandableRect.x + 4, rect.y, rect.width * 0.4f, EditorGUIUtility.singleLineHeight), Math.Max(0, Array.IndexOf(fromOptions, fromProperty.stringValue)), fromOptions);
            fromProperty.stringValue = fromOptions[Math.Min(fromNewValue, fromOptions.Length - 1)];

            EditorGUI.LabelField(new Rect(expandableRect.x + rect.width * 0.43f, rect.y, rect.width * 0.4f, EditorGUIUtility.singleLineHeight), "→");

            int toNewValue = EditorGUI.Popup(new Rect(rect.x + rect.width * 0.5f, rect.y, rect.width * 0.4f, EditorGUIUtility.singleLineHeight), Math.Max(0, Array.IndexOf(toOptions, toProperty.stringValue)), toOptions);
            toProperty.stringValue = toOptions[Math.Min(toNewValue, toOptions.Length - 1)];

            if (element.isExpanded)
            {
                SerializedProperty probabilityProperty = element.FindPropertyRelative("Probability");
                float chanceValue = EditorGUI.Slider(new Rect(expandableRect.x + 4, rect.y + expandableRect.height, rect.width * 0.7f, EditorGUIUtility.singleLineHeight), "Probability", probabilityProperty.floatValue, 0, 1);
                probabilityProperty.floatValue = chanceValue;
            }
        };

        list.elementHeightCallback += idx =>
        {
            SerializedProperty element = listProperty.GetArrayElementAtIndex(idx);
            return EditorGUIUtility.singleLineHeight * (element.isExpanded ? 2 : 1);
        };

        list.onAddCallback += list =>
        {
            list.serializedProperty.arraySize++;
            SerializedProperty addedElement = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            addedElement.FindPropertyRelative("Probability").floatValue = 1f;
        };


        transitionsListCache.Add(property.serializedObject.targetObject, list);

        return list;
    }

}
