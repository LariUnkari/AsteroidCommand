using UnityEngine;
using System.Collections;

public class DelayedSpawn : MonoBehaviour
{
    public GameObject m_prefab;
    public float m_delay = 0.5f;

    public Vector3 m_positionOffset;
    public bool m_attachToParent = false;

    private float m_time;

    private void Awake()
    {
        m_time = Time.time + m_delay;
    }

    private void Update()
    {
        if (m_time >= Time.time)
        {
            enabled = false;
            Instantiate(m_prefab, transform.position + m_positionOffset, m_prefab.transform.rotation, m_attachToParent ? transform.parent : null);
        }
    }
}
