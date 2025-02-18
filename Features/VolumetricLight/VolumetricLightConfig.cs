using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VolumetricLightConfig", menuName = "Light/VolumetricLightConfig")]
public class VolumetricLightConfig : ScriptableObject
{
    private static Dictionary<int, Mesh> sharedMeshes = new();

    public Mesh GetMesh(int resolution)
    {
        var mesh = sharedMeshes.GetValueOrDefault(resolution);
        if (mesh == null) mesh = CreateMesh(resolution);
       
        return mesh;
    }

    private Mesh CreateMesh(int resolution)
    {
        // FIXME
        var SourceRadius = 0f;
        var Range = 1f;


        Mesh mesh = new Mesh
        {
            name = $"LightMesh_R{resolution}"
        };

        // Vertices
        int amountOfTopVertices = resolution * 2;
        int topVerticesStartOffset = resolution * 2;

        Vector3[] vertices = new Vector3[resolution * 2 + amountOfTopVertices];
        float angleIncrement = 360f / resolution;

        float radius = 1f; // Range * Mathf.Tan(Mathf.Deg2Rad * Angle * 0.5f);


        for (int i = 0; i < resolution; i++)
        {
            float angle = i * angleIncrement * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle);
            float z = Mathf.Sin(angle);

            vertices[i] = new Vector3(x, z, 1); // new Vector3(radius * x, radius * z, Range);
            vertices[resolution + i] = vertices[i];

            // Apex vertex
            vertices[topVerticesStartOffset + i] = new Vector3(SourceRadius * x, SourceRadius * z, 0f);
            vertices[topVerticesStartOffset + resolution + i] = vertices[topVerticesStartOffset + i];
        }

        // Triangles
        int[] triangles = new int[resolution * 6]; // 3 vertices per triangle, 2 triangles per face. Both sides
        int triangleIndex = 0;

        // Side triangles
        for (int i = 0; i < resolution; i++)
        {
            triangles[triangleIndex++] = i;
            triangles[triangleIndex++] = topVerticesStartOffset + i;
            triangles[triangleIndex++] = (i + 1) % resolution;

            triangles[triangleIndex++] = resolution + i;
            triangles[triangleIndex++] = resolution + ((i + 1) % resolution);
            triangles[triangleIndex++] = topVerticesStartOffset + resolution + i;
        }

        // Normals (for simplicity, all normals point outward)
        //Vector3[] normals = new Vector3[vertices.Length];
        //for (int i = 0; i < resolution; i++)
        //{
        //    float angle = i * angleIncrement * Mathf.Deg2Rad;
        //    float x = Mathf.Cos(angle);
        //    float z = Mathf.Sin(angle);

        //    Vector3 bottom = new Vector3(radius * x, radius * z, Range);

        //    var normal = Vector3.Cross(bottom, Vector3.Cross(bottom, Vector3.forward)).normalized;

        //    normals[i] = normal;
        //    normals[resolution + i] = -normal;

        //    // Top
        //    normals[topVerticesStartOffset + i] = new Vector3(x, z, 0);
        //    normals[topVerticesStartOffset + resolution + i] = -normals[topVerticesStartOffset + i];
        //}

        // UV
        Vector2[] uv = new Vector2[vertices.Length];
        for (int i = 0; i < uv.Length; i++)
        {
            uv[i] = new Vector2(vertices[i].x / (2f * radius) + 0.5f, 1 - (vertices[i].z / (Range)));
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        // mesh.normals = normals;
        mesh.uv = uv;

        sharedMeshes[resolution] = mesh;
        return mesh;
    }
}
