using UnityEngine;

public class Math3D
{
    // A collection of helper functions for 3D vector mathematics
    // Author: Lari Unkari

    /// <summary>
    /// Returns true if the vector intersects the plane, and assigns the out parameter to the intersection point found
    /// </summary>
    public static bool VectorPlaneIntersect(Vector3 vectorPoint, Vector3 vectorLine, Vector3 planePoint, Vector3 planeNormal, out Vector3 intersection)
    {
        float dotN, dotD;
        Vector3 vector;

        dotN = Vector3.Dot((planePoint - vectorPoint), planeNormal);
        dotD = Vector3.Dot(vectorLine, planeNormal);

        if (!Mathf.Approximately(dotD, 0f))
        {
            vector = vectorLine.normalized * dotN / dotD;

            intersection = vectorPoint + vector;

            return true;
        }

        intersection = Vector3.zero;

        return false;
    }

    /// <summary>
    /// Returns the vector projected onto the surface of a plane defined by it's normal
    /// </summary>
    public static Vector3 ProjectVectorOntoPlane(Vector3 vector, Vector3 planeNormal)
    {
        return vector - (planeNormal * Vector3.Dot(vector, planeNormal));
    }

    /// <summary>
    /// Returns positive values if intersection was found: 1 = intersection along line AB, 2 = line point A on plane, 3 = line point B on plane, 4 = line parallel to plane
    /// </summary>
    public static int LinePlaneIntersection(out Vector3 intersection, Vector3 linePointA, Vector3 linePointB, Vector3 planeNormal, Vector3 planePoint)
    {
        float dist, lengthSqr, dotA, dotB, dotDir;
        Vector3 vector = linePointB - linePointA;

        // Calculate the vector magnitude squared, for later comparison, and normalize the vector into a direction
        lengthSqr = vector.sqrMagnitude;
        vector = vector.normalized;

        // Calculate the dot product between the line direction and plane normal
        dotDir = Vector3.Dot(vector, planeNormal);

        // Calculate the dot product between the line from plane point to line point A and plane normal, same with B
        dotA = Vector3.Dot((planePoint - linePointA), planeNormal);
        dotB = Vector3.Dot((planePoint - linePointB), planeNormal);

        // If the vectors are not parallel, we can find an intersection...
        if (dotDir != 0f)
        {
            if (dotA == 0f)
            {
                intersection = linePointA;
                return 2;
            }
            if (dotB == 0f)
            {
                intersection = linePointB;
                return 3;
            }

            dist = dotA / dotDir;

            // Only accept positive distances within the specified limit, meaning we discard hits on the vector which are outside the line
            if (dist >= 0f && dist * dist <= lengthSqr)
            {
                // Calculate the intersection point
                intersection = linePointA + vector.normalized * dist;
                return 1;
            }
        }
        // ...unless the line overlaps the plane surface in which case we find infinite solutions
        else if (dotA == 0f)
        {
            // We can just report the end point
            intersection = linePointB;
            return 4;
        }

        // No intersection could be found
        intersection = Vector3.zero;
        return 0;
    }

    /// <summary>
    /// Calculates the estimated position to intercept a moving target from a moving interceptor
    /// </summary>
    /// <param name="position">Position of the interceptor</param>
    /// <param name="velocity">Velocity of the interceptor</param>
    /// <param name="speed">Speed to move at towards target</param>
    /// <param name="targetPosition">Position of the target</param>
    /// <param name="targetVelocity">Velocity of the target</param>
    /// <param name="threshold">Threshold under which to ignore relative velocity</param>
    /// <returns>True if a solution was found</returns>
    public static bool GetInterceptPosition(Vector3 aInterceptorPos, Vector3 aInterceptorVelocity, float aInterceptorSpeed, Vector3 aTargetPos, Vector3 aTargetSpeed, float threshold, out Vector3 interceptPoint, out float timeToTarget)
    {
        Vector3 targetDir = aTargetPos - aInterceptorPos;
        float iSpeed2 = aInterceptorSpeed * aInterceptorSpeed;
        float tSpeed2 = aTargetSpeed.sqrMagnitude;
        float fDot1 = Vector3.Dot(targetDir, aTargetSpeed);
        float targetDist2 = targetDir.sqrMagnitude;

        float d = (fDot1 * fDot1) - targetDist2 * (tSpeed2 - iSpeed2);
        if (d < threshold)  // negative == no possible course because the interceptor isn't fast enough
        {
            timeToTarget = -1f;
            interceptPoint = Vector3.zero;
            return false;
        }

        float sqrt = Mathf.Sqrt(d);
        float S1 = (-fDot1 - sqrt) / targetDist2;
        float S2 = (-fDot1 + sqrt) / targetDist2;
        if (S1 < threshold)
        {
            if (S2 < threshold)
            {
                timeToTarget = -1f;
                interceptPoint = Vector3.zero;
                return false;
            }

            timeToTarget = S2;
            interceptPoint = S2 * targetDir + aTargetSpeed;
        }
        else
        {
            if (S2 < threshold)
            {
                timeToTarget = S1;
                interceptPoint = S1 * targetDir + aTargetSpeed;
            }
            else if (S1 < S2)
            {
                timeToTarget = S2;
                interceptPoint = S2 * targetDir + aTargetSpeed;
            }
            else
            {
                timeToTarget = S1;
                interceptPoint = S1 * targetDir + aTargetSpeed;
            }
        }

        return true;
    }

    /// <summary>
    /// Try to find a position to intercept a moving target while not accounting for changes trajectories
    /// </summary>
    /// <param name="interceptorPosition">Interceptor position</param>
    /// <param name="interceptorVelocity">Interceptor velocity</param>
    /// <param name="interceptSpeed">Speed to intercept with, like a projectile</param>
    /// <param name="targetPosition">Target position</param>
    /// <param name="targetVelocity">Target velocity</param>
    /// <param name="interceptPoint">Point of interception if found</param>
    /// <returns>True if a solution was found</returns>
    public static bool TryGetInterceptPoint(Vector3 interceptorPosition, Vector3 interceptorVelocity, float interceptSpeed, Vector3 targetPosition, Vector3 targetVelocity, out Vector3 interceptPoint)
    {
        Vector3 targetRelativePosition = targetPosition - interceptorPosition;
        Vector3 targetRelativeVelocity = targetVelocity - interceptorVelocity;

        float timeToTarget;
        if (TryGetInterceptTime(interceptSpeed, targetRelativePosition, targetRelativeVelocity, out timeToTarget))
        {
            interceptPoint = targetPosition + timeToTarget * (targetRelativeVelocity);
            return true;
        }

        interceptPoint = Vector3.zero;
        return false;
    }

    /// <summary>
    /// Try to calculate the time to intercept a target relative to the interceptor
    /// </summary>
    /// <param name="interceptSpeed">Speed to intercept with, like a projectile</param>
    /// <param name="targetRelativePosition">Target position</param>
    /// <param name="targetRelativeVelocity">Target velocity</param>
    /// <param name="timeToTarget"></param>
    /// <returns>True if a solution was found</returns>
    public static bool TryGetInterceptTime(float interceptSpeed, Vector3 targetRelativePosition, Vector3 targetRelativeVelocity, out float timeToTarget)
    {
        float velocitySquared = targetRelativeVelocity.sqrMagnitude;
        if (velocitySquared < 0.001f)
        {
            timeToTarget = 0f;
            return true;
        }

        float a = velocitySquared - interceptSpeed * interceptSpeed;

        // Handle similar velocities
        if (Mathf.Abs(a) < 0.001f)
        {
            timeToTarget = Mathf.Max(-targetRelativePosition.sqrMagnitude / (2f * Vector3.Dot(targetRelativeVelocity, targetRelativePosition)), 0f);
            return true;
        }

        float b = 2f * Vector3.Dot(targetRelativeVelocity, targetRelativePosition);
        float c = targetRelativePosition.sqrMagnitude;
        float determinant = b * b - 4f * a * c;

        if (determinant > 0f)
        {
            float t1 = (-b + Mathf.Sqrt(determinant)) / (2f * a);
            float t2 = (-b - Mathf.Sqrt(determinant)) / (2f * a);
            if (t1 > 0f)
                timeToTarget = t2 > 0f ? Mathf.Min(t1, t2) : t1;
            else
                timeToTarget = Mathf.Max(t2, 0f);

            return true;
        }
        else if (determinant < 0f)
        {
            timeToTarget = -1f;
            return false;
        }

        timeToTarget = Mathf.Max(-b / (2f * a), 0f);
        return true;
    }

    /// <summary>
    /// Returns an array of Vector3[4], the vertices of a quad defined by the given parameters
    /// </summary>
    public static Vector3[] GetQuadVertices(Vector3 position, Vector3 normal, Vector3 up, float width, float height)
    {
        Vector3[] vertices = new Vector3[4];
        Quaternion rot = Quaternion.LookRotation(up, normal);

        vertices[0] = position + rot * new Vector3(-0.5f, 0f, 0.5f) * width;
        vertices[1] = position + rot * new Vector3(0.5f, 0f, 0.5f) * width;
        vertices[2] = position + rot * new Vector3(0.5f, 0f, -0.5f) * width;
        vertices[3] = position + rot * new Vector3(-0.5f, 0f, -0.5f) * width;

        return vertices;
    }

    /// <summary>
    /// Returns an array of Vector3[segments], the vertices of a circle defined by the given parameters
    /// </summary>
    public static Vector3[] GetCircleVertices(Vector3 position, Vector3 normal, Vector3 up, float radius, int segments)
    {
        Vector3[] vertices = new Vector3[segments];
        Quaternion rot = Quaternion.LookRotation(normal, up);
        Vector3 newVertex;
        float a, s1, c1;

        for (int i = 0; i < segments; i++)
        {
            a = (float)i / (float)segments * 2f * Mathf.PI;
            s1 = Mathf.Sin(a);
            c1 = Mathf.Cos(a);

            newVertex.x = s1 * radius;
            newVertex.y = c1 * radius;
            newVertex.z = 0;
            newVertex = position + rot * newVertex;

            vertices[i] = newVertex;
        }

        return vertices;
    }
}
