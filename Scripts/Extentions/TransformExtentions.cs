using System.Collections.Generic;
using UnityEngine;

public static class TransformExtentions
{
    public static bool IsNullOrDestroyed(object obj)
    {
        return ReferenceEquals(obj, null) || obj.Equals(null);
    }

    public static Transform FindDeepChild(this Transform parent, string name)
    {
        Queue<Transform> queue = new();
        queue.Enqueue(parent);
        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            if (c.name == name) return c;
            foreach (Transform t in c) queue.Enqueue(t);
        }
        return null;
    }

    public static Transform CreateEmptyChild(this Transform parent, string name)
    {
        var newChild = new GameObject(name);
        newChild.isStatic = true;
        newChild.transform.SetParent(parent, false);
        return newChild.transform;
    }

    public static T GetComponentOrAdd<T>(this Transform transform) where T : Component
    {
        if (transform.TryGetComponent<T>(out var component))
            return component;
        else
            return transform.gameObject.AddComponent<T>();
    }

    public static Transform[] GetChildrens(this Transform parent)
    {
        Transform[] children = new Transform[parent.childCount];
        for (int i = 0; i < children.Length; i++)
        {
            children[i] = parent.GetChild(i);
        }

        return children;
    }

    public static Transform FindFirstInactiveParent(this Transform transform)
    {
        if (transform.gameObject.activeInHierarchy)
            return null;

        Transform parent = transform.parent;
        while (parent != null)
        {
            if (!parent.gameObject.activeSelf)
                return parent;

            parent = parent.parent;
        }

        return null;
    }

    public static RectTransform AsRect(this Transform transform)
    {
        return transform as RectTransform;
    }

    public static List<Transform> FindObjectsWithTag(this Transform parent, string tag)
    {
        List<Transform> found = new();

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            if (child.CompareTag(tag))
                found.Add(child);

            if (child.childCount > 0)
                found.AddRange(FindObjectsWithTag(child, tag));
        }

        return found;
    }

    public static void ScaleAround(Transform target, Vector3 pivot, Vector3 newScale)
    {
        Vector3 A = target.localPosition;
        Vector3 B = pivot;

        Vector3 C = A - B; // diff from object pivot to desired pivot/origin

        float RS = newScale.x / target.localScale.x; // relataive scale factor

        // calc final position post-scale
        Vector3 FP = B + C * RS;

        // finally, actually perform the scale/translation
        target.localScale = newScale;
        target.localPosition = FP;
    }

    public static void DestroyGO(this Transform transform)
    {
        Object.Destroy(transform.gameObject);
    }

    public static void DestroyChildren(this Transform transform, int offset = 0)
    {
        for (int i = offset; i < transform.childCount; i++)
        {
            var obj = transform.GetChild(i).gameObject;
            // obj.SetActive(false);
            Object.Destroy(obj);
        }
    }

    public static bool IsUntagged(this Transform transform)
    {
        return transform.CompareTag("Untagged");
    }

    public static Bounds GetRotatedBounds(this Bounds bounds, Quaternion rotation)
    {
        Vector3[] corners = new Vector3[8];
        corners[0] = bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, bounds.extents.z);
        corners[1] = bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, -bounds.extents.z);
        corners[2] = bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, bounds.extents.z);
        corners[3] = bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, -bounds.extents.z);
        corners[4] = bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, bounds.extents.z);
        corners[5] = bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, -bounds.extents.z);
        corners[6] = bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, bounds.extents.z);
        corners[7] = bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, -bounds.extents.z);

        // Rotate each corner according to the desired rotation
        for (int i = 0; i < corners.Length; i++)
        {
            corners[i] -= bounds.center; // Shift to local space
            corners[i] = rotation * corners[i];
            corners[i] += bounds.center; // Shift back to world space
        }

        // Calculate the minimum and maximum points among the rotated corners
        Vector3 min = corners[0];
        Vector3 max = corners[0];

        for (int i = 1; i < corners.Length; i++)
        {
            min = Vector3.Min(min, corners[i]);
            max = Vector3.Max(max, corners[i]);
        }

        // Create a new Bounds object using these minimum and maximum points
        return new Bounds((min + max) * 0.5f, max - min);
    }
}
