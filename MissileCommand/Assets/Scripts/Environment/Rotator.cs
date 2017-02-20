using UnityEngine;
using System.Collections;

public class Rotator : MonoBehaviour
{
    public Vector3 m_rotation;

    private void Update()
    {
        transform.Rotate(m_rotation * Time.deltaTime, Space.Self);
    }
}
