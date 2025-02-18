
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
    [Header("Collision")]
    [SerializeField] private bool UseCollisionTrigger = false;
    [SerializeField] private LayerMask CollisionMask = -1;

    [HideInInspector]
    [SerializeField] private SphereCollider Collider;

    private void Start()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        MeshFilter meshFilter = GetComponent<MeshFilter>();

        if (meshFilter.sharedMesh == null)
            meshFilter.sharedMesh = Config.GetMesh(Resolution);

        UpdateRenderBounds(renderer);

        if (UseCollisionTrigger)
        {
            tempColliders = new();
            currentColliders = new();
        }
    }

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
        var material = renderer.sharedMaterial;

        renderer.staticShadowCaster = false;
        renderer.receiveShadows = false;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        if (GameObjectUtility.AreStaticEditorFlagsSet(gameObject, StaticEditorFlags.ContributeGI))
        {
            GameObjectUtility.SetStaticEditorFlags(gameObject, GameObjectUtility.GetStaticEditorFlags(gameObject) & ~(StaticEditorFlags.ContributeGI | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic));
        }

        material.SetColor("_Color", VolumeColor);
        material.SetFloat("_Opacity", VolumeOpacity);
        material.SetFloat("_EdgeFalloff", VolumeEdgeFade);

        material.SetFloat("_Range", Range);
        material.SetFloat("_RadiusNormalized", Mathf.Tan(Mathf.Deg2Rad * Angle * 0.5f));

        UpdateRenderBounds(renderer);

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
            Object.DestroyImmediate(Collider, true);
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
        if (Application.isPlaying) return;

        Invoke(nameof(UpdateValues), 0f);
    }

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

    private void UpdateRenderBounds(MeshRenderer renderer)
    {
        var radius = Range * Mathf.Tan(Mathf.Deg2Rad * Angle * 0.5f);
        renderer.localBounds = new Bounds(new Vector3(0, 0, Range * 0.5f), new Vector3(radius * 2, radius * 2, Range));
    }

    private HashSet<Transform> currentColliders;
    private HashSet<Transform> tempColliders;
    private void FixedUpdate()
    {
        if (!UseCollisionTrigger) return;

        IEnumerable<RaycastHit> collisions = PhysicsExtentions.ConeCastAll(transform.position, transform.forward, Angle, Range, .25f, CollisionMask); //.Select(hit => hit.transform);
        tempColliders.Clear();

        // Populate newColliders list with the transforms from hits
        foreach (RaycastHit collision in collisions)
        {
            Transform collider = collision.transform;
            tempColliders.Add(collider);

            //// Handle newly entered colliders
            if (!currentColliders.Contains(collider))
            {
                collider.SendMessage("OnTriggerEnter", Collider, SendMessageOptions.DontRequireReceiver);
                currentColliders.Add(collider);
            }
        }

        // Remove exited colliders
        foreach (Transform collider in currentColliders)
        {
            if (tempColliders.Contains(collider))
            {
                tempColliders.Remove(collider);
            }
            else if (collider != null)
            {
                collider.SendMessage("OnTriggerExit", Collider, SendMessageOptions.DontRequireReceiver);
                tempColliders.Add(collider);
            }
        }

        foreach (Transform collision in tempColliders)
        {
            currentColliders.Remove(collision);
        }

        //foreach (var collision in collisions.Except(currentColliders))
        //{
        //    collision.transform.SendMessage("OnTriggerEnter", Collider, SendMessageOptions.DontRequireReceiver);
        //    currentColliders.Add(collision);
        //}

        //foreach (var collision in currentColliders.Except(collisions))
        //{
        //    if (collision != null)
        //        collision.transform.SendMessage("OnTriggerExit", Collider, SendMessageOptions.DontRequireReceiver);
        //    tempColliders.Add(collision);
        //}

        //foreach (var collision in tempColliders)
        //{
        //    currentColliders.Remove(collision);
        //}
    }

    private void OnDisable()
    {
        if (!UseCollisionTrigger) return;

        foreach (var collision in currentColliders)
        {
            if (collision != null)
                collision.transform.SendMessage("OnTriggerExit", Collider, SendMessageOptions.DontRequireReceiver);
        }

        currentColliders.Clear();
        tempColliders.Clear();
    }


    [Min(3f)]
    [SerializeField] private int Resolution = 16; // Number of vertices around the base of the cone
    [SerializeField] private VolumetricLightConfig Config;

}
