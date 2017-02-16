using UnityEngine;
using System.Collections;

public class DebugUtilities
{
    /// <summary>
    /// Add a frame index timestamp in front of your log message
    /// </summary>
    /// <param name="message">The message content</param>
    /// <returns>"[FrameIndex] message"</returns>
    public static string AddTimestampPrefix(string message)
    {
        return "[" + Time.frameCount + "] " + message;
    }

    /// <summary>
    /// Helper to draw a debug arrow using Unity's native Debug.DrawLine() method.
    /// </summary>
    /// <param name="origin">Origin of arrow</param>
    /// <param name="target">Target of arrow head</param>
    /// <param name="normal">Up vector for orientation</param>
    /// <param name="color">Color value</param>
    /// <param name="dashCount">How many dashes to draw (if more than 1)</param>
    /// <param name="duration">How long should the arrow stay drawn for</param>
    /// <param name="arrowHeadLength">Length of the arrow head triangle sides</param>
    /// <param name="arrowHeadAngle">Angle of the arrow head triangle</param>
    public static void DrawArrow(Vector3 origin, Vector3 target, Vector3 normal, Color color, int dashCount = 1, float duration = 0f, float arrowHeadLength = 0.25f, float arrowHeadAngle = 45f)
    {
        // Draw the body

        if (dashCount > 1)
        {
            float t;
            Vector3 a, b;
            int segments = 2 * dashCount - 1;
            for (int i = 0; i < segments; i++)
            {
                if (i % 2 == 0)
                {
                    t = i;
                    a = Vector3.Lerp(origin, target, t / segments);
                    b = Vector3.Lerp(origin, target, (t + 1f) / segments);
                    
                    if (duration > 0f)
                        Debug.DrawLine(a, b, color, duration);
                    else
                        Debug.DrawLine(a, b, color);
                }
            }
        }
        else
        {
            if (duration > 0f)
                Debug.DrawLine(origin, target, color, duration);
            else
                Debug.DrawLine(origin, target, color);
        }

        // Draw the head

        Vector3 direction = target - origin;
        Vector3 right = Quaternion.LookRotation(direction, normal) * Quaternion.Euler(0f, 180f + arrowHeadAngle, 0f) * new Vector3(0f, 0f, 1f);
        Vector3 left = Quaternion.LookRotation(direction, normal) * Quaternion.Euler(0f, 180f - arrowHeadAngle, 0f) * new Vector3(0f, 0f, 1f);

        if (duration > 0f)
        {
            Debug.DrawLine(target, target + right * arrowHeadLength, color, duration);
            Debug.DrawLine(target, target + left * arrowHeadLength, color, duration);
        }
        else
        {
            Debug.DrawLine(target, target + right * arrowHeadLength, color);
            Debug.DrawLine(target, target + left * arrowHeadLength, color);
        }
    }
}
