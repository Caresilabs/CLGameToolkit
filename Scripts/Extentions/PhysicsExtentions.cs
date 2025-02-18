using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PhysicsExtentions
{
    private static readonly RaycastHit[] results = new RaycastHit[20];
    private static readonly Collider[] tmpColliders = new Collider[30];

    /// <summary>
    /// ConeCast along a direction.
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="direction"></param>
    /// <param name="coneAngle"></param>
    /// <param name="maxDistance"></param>
    /// <param name="distanceThreshold">All objects within this threshold are marked as hit. This is due to the imprecision of the first sphere in the internal cast.</param>
    /// <param name="layerMask"></param>
    /// <returns>Resulting hit objects</returns>
    public static IEnumerable<RaycastHit> ConeCastAll(Vector3 origin, Vector3 direction, float coneAngle, float maxDistance, float distanceThreshold = .25f, int layerMask = -1)
    {
        var angleRad = Mathf.Deg2Rad * coneAngle * 0.5f;
        var maxRadius = Mathf.Sin(angleRad) * maxDistance;

        var originWithOffset = origin + direction * maxRadius; // The origin is set by sphere's center.
        int count = Physics.SphereCastNonAlloc(originWithOffset, maxRadius, direction, results, maxDistance, layerMask);

        if (count == 0)
        {
            return Array.Empty<RaycastHit>();
        }

        // Can we save one Cos/Sin op here?
        var minDotProduct = Mathf.Cos(angleRad);

        var filtered = results.Take(count)
            .Select(hit =>
            {
                if (hit.distance == 0)
                {
                    hit.point = hit.collider.ClosestPointOnBounds(origin + direction * distanceThreshold); // Guestimate because object is too close
                    hit.distance = hit.point.IsZero() ? 0 : Vector3.Distance(origin, hit.point); // Update distance of close object
                    return hit;
                }

                Vector3 pointVector = hit.point - origin;
                Vector3 pointDirection = pointVector.normalized;

                // Check the cone angle with the hit.
                if (Vector3.Dot(pointDirection, direction) < minDotProduct)
                {
                    // Our  hit is outisde of the cone, however some parts might still be inside.
                    Vector3 rayDirection = Vector3.RotateTowards(direction, pointDirection, angleRad - .01f, 0f);
                    bool raycast = hit.collider.bounds.IntersectRay(new Ray(origin, rayDirection), out var hitDis); // For higher precision: hit.collider.Raycast(new Ray(origin, rayDirection), out var hitInfo, maxDistance);
                    hit.point = raycast ? origin + rayDirection * hitDis : Vector3.positiveInfinity;
                }

                hit.distance = (hit.point - origin).magnitude + float.Epsilon; // Update distance.;

                return hit;
            })
            .Where((hit, index) =>
        {
            // Are we too close, override it.
            // Check distance, as we could have a collsion further away due to the spheres.
            float distance = hit.distance;

            if (distance <= distanceThreshold)
                return true;

            return distance <= maxDistance;
        });

        return filtered;
    }

    public static void RotateAround(this Rigidbody rb, Vector3 pivot, Vector3 axis, float angle)
    {
        // Calculate the direction from the pivot to the object
        Vector3 direction = rb.position - pivot;

        // Calculate the rotation using Quaternion
        Quaternion rotation = Quaternion.AngleAxis(angle, axis);

        // Update the position of the Rigidbody
        Vector3 newPosition = rotation * direction + pivot;

        rb.Move(newPosition, rotation * rb.rotation);
    }

    public static int OverlapSphere(Vector3 position, float radius, out Collider[] colliders, int layer = -1)
    {
        int hitCount = Physics.OverlapSphereNonAlloc(position, radius, tmpColliders, layer, QueryTriggerInteraction.Collide);
        colliders = tmpColliders;
        return hitCount;
    }

    public static void AddForceSphere(Vector3 position, float radius, float force, ForceMode forceMode = ForceMode.Force, int layer = -1)
    {
        int count = Physics.OverlapSphereNonAlloc(position, radius, tmpColliders, layer);
        for (int i = 0; i < count; i++)
        {
            if (tmpColliders[i].TryGetComponent(out Rigidbody nearbyObject))
            {
                nearbyObject.AddExplosionForce(force, position, radius, 0f, forceMode);
            }
        }
    }
}
