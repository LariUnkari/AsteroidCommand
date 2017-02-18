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
