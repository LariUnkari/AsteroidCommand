﻿using UnityEngine;
using System.Collections;

public class FuseProjectile : Projectile
{
    public GameObject m_targetCursorPrefab;

    private GameObject m_targetCursor;

    private Vector3 m_targetPosition;
    private float m_travelDuration;
    private float m_travelT;

    public void Initialize(int id, Vector3 startPosition, Vector3 targetPosition, GameObject modelPrefab)
    {
        base.Initialize(id, startPosition, modelPrefab);
        
        m_targetPosition = targetPosition;

        m_targetCursor = Instantiate<GameObject>(m_targetCursorPrefab, m_targetPosition, Quaternion.LookRotation(Vector3.forward, Vector3.up), GameManager.UIRoot);

        // TODO: Make a global speed modifier instead of static 0.1 multiplier
        m_travelDuration = Vector3.Distance(m_targetPosition, m_startPosition) / (0.1f * m_speed);
        m_travelT = 0f;
    }

    protected override void Update()
    {
        m_travelT += Time.deltaTime / m_travelDuration;
        transform.position = Vector3.Lerp(m_startPosition, m_targetPosition, m_travelT);

        UpdateTrail();
        
        if (m_travelT >= 1f)
        {
            Debug.Log(DebugUtilities.AddTimestampPrefix(m_team + " Projectile " + name + " reached target position, detonating!"));
            Detonate();
        }
    }

    protected override void OnDeath(bool giveScore)
    {
        Destroy(m_targetCursor);

        base.OnDeath(giveScore);
    }
}
