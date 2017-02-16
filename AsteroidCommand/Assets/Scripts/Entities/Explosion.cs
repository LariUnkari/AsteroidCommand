using UnityEngine;
using System.Collections;

public class Explosion : MonoBehaviour
{
    public GameObject m_effectPrefab;

    public float m_radius = 0.5f;
    public float m_duration = 2f;

    public SphereCollider m_collider;

    private void Awake()
    {
        m_collider.radius = m_radius;
        Instantiate<GameObject>(m_effectPrefab, transform.position, transform.rotation, transform);
    }
}
