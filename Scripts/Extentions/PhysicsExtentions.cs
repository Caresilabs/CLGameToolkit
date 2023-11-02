using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PhysicsExtentions
{
    private static readonly RaycastHit[] results = new RaycastHit[20];

    public static IEnumerable<RaycastHit> ConeCastAll(Vector3 origin, Vector3 direction, float coneAngle, float maxDistance, float distanceThreshold = 1f, int layerMask = -1)
    {
        var angleRad = Mathf.Deg2Rad * coneAngle;
        var maxRadius = Mathf.Sin(angleRad * 0.5f) * maxDistance;

        int count = Physics.SphereCastNonAlloc(origin, maxRadius, direction, results, maxDistance, layerMask);
        if (count == 0)
        {
            return Array.Empty<RaycastHit>();
        }


        var minDotProduct = Mathf.Cos(angleRad);

        var filtered = results.Take(count)
            .Select(hit =>
            {
                if (hit.point == Vector3.zero) // Overlaps item 
                    hit.point = hit.transform.position;

                return hit;
            })
            .Where((hit, index) =>
        {
            Vector3 directionToHit = hit.point - origin;
            return Vector3.Dot(directionToHit.normalized, direction) > minDotProduct || directionToHit.magnitude < distanceThreshold;
        });


        return filtered;    //.ToArray();
    }
}
