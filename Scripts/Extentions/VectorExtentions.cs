using System.Globalization;
using UnityEngine;

public static class VectorExtentions
{
    public static Vector3 XZ(this Vector3 vector, float overrideY = 0f)
    {
        return new Vector3(vector.x, overrideY, vector.z);
    }

    public static Vector3 Parse(string vector)
    {
        if (vector == null || vector == "")
        {
            return Vector3.zero;
        }

        string stripped = vector
            .Replace("(", "")
               .Replace(")", "")
                  .Replace(" ", "");

        string[] array = stripped.Split(',');
        return new Vector3(
            float.Parse(array[0], CultureInfo.InvariantCulture),
            float.Parse(array[1], CultureInfo.InvariantCulture),
            float.Parse(array[2], CultureInfo.InvariantCulture));
    }

    public static bool IsZero(this Vector3 vector)
    {
        return vector.x == 0 && vector.y == 0 && vector.z == 0;
    }

    public static Vector3 RoundDirectionToNearestAngle(this Vector3 direction, float angleStep)
    {
        // Convert the direction vector to spherical coordinates
        float radius = direction.magnitude;
        float theta = Mathf.Atan2(direction.z, direction.x); // Azimuthal angle
        float phi = Mathf.Acos(direction.y / radius); // Polar angle
        float angleStepRad = angleStep * Mathf.Deg2Rad;

        // Round the angles to the nearest step
        theta = Mathf.Round(theta / angleStepRad) * angleStepRad;
        phi = Mathf.Round(phi / angleStepRad) * angleStepRad;

        // Convert back to Cartesian coordinates
        Vector3 roundedDirection = new(
            radius * Mathf.Sin(phi) * Mathf.Cos(theta),
            radius * Mathf.Cos(phi),
            radius * Mathf.Sin(phi) * Mathf.Sin(theta)
        );

        return roundedDirection;
    }

    public static Vector3 RandomPoint(this Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }

    /// <summary>
    /// Computes a random point in an annulus (a ring-shaped area) based on minimum and 
    /// maximum radius values around a central Vector3 point (origin).
    /// Source: https://github.com/adammyhre/Unity-Utils/blob/master/UnityUtils/Scripts/Extensions/Vector3Extensions.cs
    /// </summary>
    /// <param name="origin">The center Vector3 point of the annulus.</param>
    /// <param name="minRadius">Minimum radius of the annulus.</param>
    /// <param name="maxRadius">Maximum radius of the annulus.</param>
    /// <returns>A random Vector3 point within the specified annulus.</returns>
    public static Vector3 RandomPointInAnnulus(this Vector3 origin, float minRadius, float maxRadius)
    {
        float angle = Random.value * Mathf.PI * 2f;
        Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));

        // Squaring and then square-rooting radii to ensure uniform distribution within the annulus
        float minRadiusSquared = minRadius * minRadius;
        float maxRadiusSquared = maxRadius * maxRadius;
        float distance = Mathf.Sqrt(Random.value * (maxRadiusSquared - minRadiusSquared) + minRadiusSquared);

        // Converting the 2D direction vector to a 3D position vector
        Vector3 position = new Vector3(direction.x, 0, direction.y) * distance;
        return origin + position;
    }

    public static Vector3 ClosestPointOnEdge(Bounds bounds, Vector3 point, Vector3 axis)
    {
        Vector3 boundsMin = bounds.min;
        Vector3 boundsMax = bounds.max;

        float closestDistance = float.MaxValue;
        float closestValue = 0f;
        int closestIndex = -1;

        for (int i = 0; i < 3; i++)
        {
            if (axis[i] == 0)
                continue;

            float minEdge = boundsMin[i];
            float maxEdge = boundsMax[i];
            float pointValue = point[i];

            float distanceToMinEdge = pointValue - minEdge;
            float distanceToMaxEdge = maxEdge - pointValue;

            float absDistanceToMinEdge = Mathf.Abs(distanceToMinEdge);
            float absDistanceToMaxEdge = Mathf.Abs(distanceToMaxEdge);

            // Determine the closest edge and update values if closer
            if (absDistanceToMinEdge < closestDistance || absDistanceToMaxEdge < closestDistance)
            {
                if (absDistanceToMinEdge < absDistanceToMaxEdge)
                {
                    closestDistance = absDistanceToMinEdge;
                    closestValue = minEdge;
                }
                else
                {
                    closestDistance = absDistanceToMaxEdge;
                    closestValue = maxEdge;
                }
                closestIndex = i;
            }
        }

        Vector3 closestPoint = point;
        if (closestIndex >= 0)
            closestPoint[closestIndex] = closestValue;

        return closestPoint;
    }

    public static Quaternion OnlyY(this Quaternion q)
    {
        return Quaternion.Euler(0, q.eulerAngles.y, 0);
    }

}
