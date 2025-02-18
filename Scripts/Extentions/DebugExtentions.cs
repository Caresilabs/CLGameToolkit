using UnityEngine;

public static class DebugExtentions
{
    public static void DrawBox(Vector3 pos, Quaternion rot, Vector3 size, Color color)
    {
        Matrix4x4 m = new();
        m.SetTRS(pos, rot, size);

        var point1 = m.MultiplyPoint(new Vector3(-0.5f, -0.5f, 0.5f));
        var point2 = m.MultiplyPoint(new Vector3(0.5f, -0.5f, 0.5f));
        var point3 = m.MultiplyPoint(new Vector3(0.5f, -0.5f, -0.5f));
        var point4 = m.MultiplyPoint(new Vector3(-0.5f, -0.5f, -0.5f));

        var point5 = m.MultiplyPoint(new Vector3(-0.5f, 0.5f, 0.5f));
        var point6 = m.MultiplyPoint(new Vector3(0.5f, 0.5f, 0.5f));
        var point7 = m.MultiplyPoint(new Vector3(0.5f, 0.5f, -0.5f));
        var point8 = m.MultiplyPoint(new Vector3(-0.5f, 0.5f, -0.5f));

        Debug.DrawLine(point1, point2, color);
        Debug.DrawLine(point2, point3, color);
        Debug.DrawLine(point3, point4, color);
        Debug.DrawLine(point4, point1, color);

        Debug.DrawLine(point5, point6, color);
        Debug.DrawLine(point6, point7, color);
        Debug.DrawLine(point7, point8, color);
        Debug.DrawLine(point8, point5, color);

        Debug.DrawLine(point1, point5, color);
        Debug.DrawLine(point2, point6, color);
        Debug.DrawLine(point3, point7, color);
        Debug.DrawLine(point4, point8, color);
    }
}
