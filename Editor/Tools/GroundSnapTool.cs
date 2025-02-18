using UnityEditor;
using UnityEngine;

public static class GroundSnapTool
{
    private const string ActionName = "Snap to Ground";

    [MenuItem("Tools/Scene/Snap To Ground %g")]
    public static void Perform()
    {
        RegisterUndo();

        foreach (Transform transform in Selection.transforms)
            MoveToGround(transform, false);
    }

    [MenuItem("Tools/Scene/Snap To Ground & Normal &%g")]
    public static void PerformWithNormal()
    {
        RegisterUndo();

        foreach (Transform transform in Selection.transforms)
            MoveToGround(transform, true);
    }

    private static void RegisterUndo()
    {
        Undo.RegisterCompleteObjectUndo(Selection.transforms, ActionName);
    }

    public static void MoveToGround(Transform transform, bool useNormal)
    {
        var collider = transform.GetComponent<Collider>();
        bool hasCollider = collider != null;
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
            var upHitPosition = upHit.transform == transform ? upHit.point : transform.position;
            Vector3 translation = new Vector3(0, downHit.point.y - upHitPosition.y, 0);
            transform.Translate(translation, Space.World);
        }

        if (useNormal && hitDown)
        {
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, downHit.normal) * transform.rotation;
            transform.rotation = targetRotation;
        }
    }
}
