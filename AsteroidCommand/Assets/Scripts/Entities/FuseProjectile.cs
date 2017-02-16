using UnityEngine;
using System.Collections;

public class FuseProjectile : Projectile
{
    private Vector3 m_targetPosition;
    private Vector3 m_sourcePosition;
    private float m_travelDuration;
    private float m_travelT;

    public void Initialize(Vector3 source, Vector3 target)
    {
        m_sourcePosition = source;
        m_targetPosition = target;

        // TODO: Make a global speed modifier instead of static 0.1 multiplier
        m_travelDuration = Vector3.Distance(m_targetPosition, m_sourcePosition) / (0.1f * m_speed);
        m_travelT = 0f;
    }

    protected override void Update()
    {
        m_travelT += Time.deltaTime / m_travelDuration;
        transform.position = Vector3.Lerp(m_sourcePosition, m_targetPosition, m_travelT);
        
        if (m_travelT >= 1f)
        {
            Debug.Log(DebugUtilities.AddTimestampPrefix("Projectile reached target position, detonating!"));
            OnDetonate();
        }
    }
}
