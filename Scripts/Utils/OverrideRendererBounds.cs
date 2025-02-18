using UnityEngine;

public class OverrideRendererBounds : MonoBehaviour
{
    [SerializeField] private Bounds Bounds;

    private void Start()
    {
        if (TryGetComponent<Renderer>(out var renderer))
        {
            Logger.Debug($"Renderer {this} bounds was {renderer.localBounds} -> {Bounds}");
            renderer.localBounds = Bounds;
        }
    }

#if UNITY_EDITOR
    public void OnDrawGizmosSelected()
    {
        if (!TryGetComponent<Renderer>(out var r))
            return;

        var bounds = r.bounds;
        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(bounds.center, bounds.extents * 2);
    }
#endif

}