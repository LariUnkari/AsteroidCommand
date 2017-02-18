using UnityEngine;
using System.Collections;

public class ProximityTrigger : MonoBehaviour
{
    public SoundEffectPreset m_triggerSFX;
    public float m_minimumTriggerInterval;

    public LayerMask m_triggerMask;

    private float m_timeSinceLastTrigger;

    private void Start()
    {
        m_timeSinceLastTrigger = m_minimumTriggerInterval;
    }

    private void Update()
    {
        m_timeSinceLastTrigger += Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (m_timeSinceLastTrigger < m_minimumTriggerInterval)
            return;

        Debug.Log(other.name + " entered " + typeof(ProximityTrigger) + " " + name, this);

        if ((m_triggerMask.value & (1 << other.gameObject.layer)) > 0)
        {
            Debug.Log(typeof(ProximityTrigger) + " " + name + " was triggered by " + other.name, other);
            m_triggerSFX.PlayAt(Camera.main.transform.position + Camera.main.transform.forward);
            m_timeSinceLastTrigger = 0f;
        }
    }
}
