using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PhysicsExtentions
{
    private static readonly RaycastHit[] results = new RaycastHit[20];

    public static IEnumerable<RaycastHit> ConeCastAll(Vector3 origin, Vector3 direction, float coneAngle, float maxDistance, float distanceThreshold = 1f, int layerMask = -1)
    {
        var angleRad = Mathf.Deg2Rad * coneAngle * 0.5f;
        var maxRadius = Mathf.Sin(angleRad) * maxDistance;

        int count = Physics.SphereCastNonAlloc(origin, maxRadius, direction, results, maxDistance, layerMask);
        if (count == 0)
        {
            return Array.Empty<RaycastHit>();
        }

        // Can we save one Cos/Sin op here?
        var minDotProduct = Mathf.Cos(angleRad);

        var filtered = results.Take(count)
            .Select(hit =>
            {
                if (hit.point == Vector3.zero)  // Overlaps item 
                    hit.point = hit.transform.position;

                return hit;
            })
            .Where((hit, index) =>
        {
            Vector3 directionToHit = hit.point - origin;
            // 1. Or we are too close and override it.
            // 2. Check distance, as we could have a collsion further away due to the spheres.
            // 3. Check the cone angle is allowed.
            float distance = hit.distance > 0 ? hit.distance : directionToHit.magnitude;
            return distance < distanceThreshold || (distance <= maxDistance && Vector3.Dot(directionToHit.normalized, direction) > minDotProduct);
        });


        return filtered;
    }
}
