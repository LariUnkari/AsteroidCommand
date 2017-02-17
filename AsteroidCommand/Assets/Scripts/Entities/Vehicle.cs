using System;
using UnityEngine;

public class Vehicle : Entity
{
    public int m_health = 1;
    public int m_speed = 5;

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

    protected override void OnDeath(bool giveScore)
    {
        // TODO: Give Player score for the kill
        Destroy(gameObject);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        throw new NotImplementedException();
    }
}
