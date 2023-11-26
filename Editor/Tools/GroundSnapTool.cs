using UnityEditor;
using UnityEngine;

// Author: https://gist.github.com/C-Through/64dd2084a7f4adefef03af1d82dd5566
public static class GroundSnapTool
{
    private const string ActionName = "Snap to Ground";

    [MenuItem("Tools/Scene/Snap To Ground %g")]
    public static void Perform()
    {
        RegisterUndo();

        foreach (Transform transform in Selection.transforms)
            MoveToGround(transform);
    }

    private static void RegisterUndo()
    {
        Undo.RegisterCompleteObjectUndo(Selection.transforms, ActionName);
    }

    private static void MoveToGround(Transform transform)
    {
        bool hasCollider = transform.GetComponent<Collider>() != null;
        bool hitDown = Physics.Raycast(transform.position, Vector3.down, out RaycastHit downHit);
        bool hitUp = Physics.Raycast(downHit.point, Vector3.up, out RaycastHit upHit);

        if (hitDown && !hasCollider)
        {
            Vector3 translation = new Vector3(0, downHit.point.y - transform.position.y, 0);
            transform.Translate(translation, Space.World);
            return;
        }

        if (hitDown && hitUp)
        {
            Vector3 translation = new Vector3(0, downHit.point.y - upHit.point.y, 0);
            transform.Translate(translation, Space.World);
        }
    }
}
