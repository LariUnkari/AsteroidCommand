using UnityEngine;
using System.Collections;

public class FuseProjectile : Projectile
{
    public GameObject m_targetCursorPrefab;

    private GameObject m_targetCursor;

    private float m_travelDuration;
    private float m_travelT;

    public override void Initialize(int id, Vector3 startPosition, Vector3 targetPosition, float speed, GameObject modelPrefab)
    {
        base.Initialize(id, startPosition, targetPosition, speed, modelPrefab);

        m_targetCursor = Instantiate<GameObject>(m_targetCursorPrefab, m_targetPosition, Quaternion.LookRotation(Vector3.forward, Vector3.up), UserInterface.Root);
        
        m_travelDuration = Vector3.Distance(m_targetPosition, m_startPosition) / m_speed;
        m_travelT = 0f;
    }

    protected override void Update()
    {
        m_travelT += Time.deltaTime / m_travelDuration;
        transform.position = Vector3.Lerp(m_startPosition, m_targetPosition, m_travelT);

        UpdateTrail();
        
        if (m_travelT >= 1f)
        {
            //Debug.Log(DebugUtilities.AddTimestampPrefix(m_team + " Projectile " + name + " reached target position, detonating!"));
            Detonate();
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (m_targetCursor != null)
            Destroy(m_targetCursor);
    }
}
