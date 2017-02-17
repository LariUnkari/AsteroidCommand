using UnityEngine;
using System.Collections;

public class TransformUtilities
{
    /// <summary>
    /// Simply sets the specified layer on all objects in hierachy starting from the parent
    /// </summary>
    public static void SetLayerToHierarchy(Transform parent, int layer)
    {
        parent.gameObject.layer = layer;

        foreach (Transform child in parent)
            SetLayerToHierarchy(child, layer);
    }
}
