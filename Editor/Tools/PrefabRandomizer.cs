using UnityEditor;
using UnityEngine;

public static class PrefabRandomizer
{
    private const string RandomizeYAction = "Randomize Y";

    [MenuItem("Tools/Scene/" + RandomizeYAction)]
    public static void RandomizeY()
    {
        RegisterUndo(RandomizeYAction);

        foreach (Transform transform in Selection.transforms)
        {
            var newRotation = transform.rotation.eulerAngles;
            newRotation.y = Random.value * 360f;
            transform.eulerAngles = newRotation;
        }
    }

    private static void RegisterUndo(string actionName)
    {
        Undo.RegisterCompleteObjectUndo(Selection.transforms, actionName);
    }

}
