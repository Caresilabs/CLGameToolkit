
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/**
 * TODO Features:
 * Change color during gameplay
 *
 */

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class VolumetricLight : MonoBehaviour
{
    [Range(0f, 180f)]
    [SerializeField] private float Angle = 60f;
    [Min(0.1f)]
    [SerializeField] private float Range = 2f;
    //[Min(0)]
    //[SerializeField] private float SourceRadius = 0;
    [SerializeField] private bool UseCollisionTrigger = false;
    [SerializeField] private LayerMask CollisionMask = -1;

    [HideInInspector]
    [SerializeField] private SphereCollider Collider;

#if UNITY_EDITOR

    [Header("Fog Volume Settings")]
    [SerializeField][ColorUsage(true, true)] private Color VolumeColor = Color.white;
    [Min(0)]
    [SerializeField] private float VolumeOpacity = 0.7f;
    [Min(0)]
    [SerializeField] private float VolumeEdgeFade = 0.2f;

    [Header("Light Settings")]
    [SerializeField] private float DistanceOffset = 0f;
    [Range(0f, 1f)]
    [SerializeField] private float LightSmoothnes = 0.5f;

    [Header("Advanced")]
    [SerializeField] private Shader shader;

    [Min(3f)]
    [SerializeField] private int Resolution = 16; // Number of vertices around the base of the cone
    [SerializeField] private VolumetricLightConfig Config;

    void UpdateValues()
    {
        // Mesh
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.sharedMesh = Config.GetMesh(Resolution);

        // Renderer
        var renderer = GetComponent<MeshRenderer>();
        if (renderer.sharedMaterial == null)
        {
            renderer.sharedMaterial = new Material(shader);
        }

        renderer.staticShadowCaster = false;
        renderer.receiveShadows = false;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.sharedMaterial.SetColor("_Color", VolumeColor);
        renderer.sharedMaterial.SetFloat("_Opacity", VolumeOpacity);
        renderer.sharedMaterial.SetFloat("_EdgeFalloff", VolumeEdgeFade);

        renderer.sharedMaterial.SetFloat("_Range", Range);
        renderer.sharedMaterial.SetFloat("_RadiusNormalized", Mathf.Tan(Mathf.Deg2Rad * Angle * 0.5f));

        var radius = Range * Mathf.Tan(Mathf.Deg2Rad * Angle * 0.5f);
        renderer.localBounds = new Bounds(new Vector3(0, 0, Range * 0.5f), new Vector3(radius * 2, radius * 2, Range));

        // Spot Light
        var light = InitLight();
        light.type = LightType.Spot;
        light.color = VolumeColor.HDRToSDR();
        light.range = Range + DistanceOffset;
        light.spotAngle = Angle;
        light.innerSpotAngle = Angle * (1f - LightSmoothnes);

        // Trigger
        Collider = GetComponent<SphereCollider>();
        if (UseCollisionTrigger)
        {
            if (Collider == null)
            {
                Collider = gameObject.AddComponent<SphereCollider>();
            }
            //  collider.convex = true;
            Collider.isTrigger = true;
            Collider.radius = 0f;
            Collider.enabled = false;
        }
        else
        {
            GameObject.DestroyImmediate(Collider, true);
        }
    }

    private Light InitLight()
    {
        var light = GetComponent<Light>();

        if (light != null) return light;

        light = gameObject.AddComponent<Light>();
        light.shadows = LightShadows.None;
        return light;
    }

    void OnValidate()
    {
        Invoke(nameof(UpdateValues), 0f);
    }


    //private Mesh GenerateConeMesh()
    //{
    //    Mesh mesh = new Mesh();
    //    mesh.name = "LightMesh";

    //    // Vertices
    //    int amountOfTopVertices = Resolution * 2;
    //    int topVerticesStartOffset = Resolution * 2;

    //    Vector3[] vertices = new Vector3[Resolution * 2 + amountOfTopVertices];
    //    float angleIncrement = 360f / Resolution;

    //    float radius = Range * Mathf.Tan(Mathf.Deg2Rad * Angle * 0.5f);


    //    for (int i = 0; i < Resolution; i++)
    //    {
    //        float angle = i * angleIncrement * Mathf.Deg2Rad;
    //        float x = Mathf.Cos(angle);
    //        float z = Mathf.Sin(angle);

    //        vertices[i] = new Vector3(x, z, 1); // new Vector3(radius * x, radius * z, Range);
    //        vertices[Resolution + i] = vertices[i];

    //        // Apex vertex
    //        vertices[topVerticesStartOffset + i] = new Vector3(SourceRadius * x, SourceRadius * z, 0f);
    //        vertices[topVerticesStartOffset + Resolution + i] = vertices[topVerticesStartOffset + i];
    //    }

    //    // Triangles
    //    int[] triangles = new int[Resolution * 6]; // 3 vertices per triangle, 2 triangles per face. Both sides
    //    int triangleIndex = 0;

    //    // Side triangles
    //    for (int i = 0; i < Resolution; i++)
    //    {
    //        triangles[triangleIndex++] = i;
    //        triangles[triangleIndex++] = topVerticesStartOffset + i;
    //        triangles[triangleIndex++] = (i + 1) % Resolution;

    //        triangles[triangleIndex++] = Resolution + i;
    //        triangles[triangleIndex++] = Resolution + ((i + 1) % Resolution);
    //        triangles[triangleIndex++] = topVerticesStartOffset + Resolution + i;
    //    }

    //    // Normals (for simplicity, all normals point outward)
    //    Vector3[] normals = new Vector3[vertices.Length];
    //    for (int i = 0; i < Resolution; i++)
    //    {
    //        float angle = i * angleIncrement * Mathf.Deg2Rad;
    //        float x = Mathf.Cos(angle);
    //        float z = Mathf.Sin(angle);

    //        Vector3 bottom = new Vector3(radius * x, radius * z, Range);

    //        var normal = Vector3.Cross(bottom, Vector3.Cross(bottom, Vector3.forward)).normalized;

    //        normals[i] = normal;
    //        normals[Resolution + i] = -normal;

    //        // Top
    //        normals[topVerticesStartOffset + i] = new Vector3(x, z, 0);
    //        normals[topVerticesStartOffset + Resolution + i] = -normals[topVerticesStartOffset + i];
    //    }

    //    // UV
    //    Vector2[] uv = new Vector2[vertices.Length];
    //    for (int i = 0; i < uv.Length; i++)
    //    {
    //        uv[i] = new Vector2(vertices[i].x / (2f * radius) + 0.5f, 1 - (vertices[i].z / (Range)));
    //    }

    //    mesh.vertices = vertices;
    //    mesh.triangles = triangles;
    //    mesh.normals = normals;
    //    mesh.uv = uv;

    //    mesh.bounds = new Bounds(new Vector3(0, 0, Range * 0.5f), new Vector3(radius * 2, radius * 2, Range));

    //    return mesh;
    //}

    [MenuItem("GameObject/Light/Volumetric Light")]
    public static void CreateNewLight()
    {
        var light = new GameObject("Volumetric Light");
        light.AddComponent<VolumetricLight>();
        light.transform.parent = Selection.activeTransform;
        light.transform.localPosition = Vector3.zero;
        light.transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);

        Selection.activeObject = light;
    }

#endif

    // TODO needs to be in final build
    private HashSet<Transform> currentColliders = new();
    private List<Transform> tempColliders = new();
    private void FixedUpdate()
    {
        if (!UseCollisionTrigger) return;

        var collisions = PhysicsExtentions.ConeCastAll(transform.position, transform.forward, Angle, Range, CollisionMask).Select(hit => hit.transform);
        tempColliders.Clear();

        foreach (var collision in collisions.Except(currentColliders))
        {
            collision.transform.SendMessage("OnTriggerEnter", Collider, SendMessageOptions.DontRequireReceiver);
            currentColliders.Add(collision);
        }

        foreach (var collision in currentColliders.Except(collisions))
        {
            if (collision != null)
                collision.transform.SendMessage("OnTriggerExit", Collider, SendMessageOptions.DontRequireReceiver);
            tempColliders.Add(collision);
        }

        foreach (var collision in tempColliders)
        {
                currentColliders.Remove(collision);
        }
    }

    private void OnDisable()
    {
        foreach (var collision in currentColliders)
        {
            if (collision != null)
                collision.transform.SendMessage("OnTriggerExit", Collider, SendMessageOptions.DontRequireReceiver);
        }

        currentColliders.Clear();
        tempColliders.Clear();
    }

}
