using UnityEngine;
using System.Collections;
using System;

public class Projectile : Entity
{
    public LineRenderer m_trailRenderer;

    public float m_speed = 5f;
    public Vector3 m_modelRotateVelocity = Vector3.forward * 360f / 5f;

    public SoundEffectPreset m_hitSFX;
    public GameObject m_detonateEffectPrefab;
    
    protected Vector3 m_startPosition;

    private bool m_isInitialized;
    private Transform m_modelTransform;

    private void Start()
    {
        if (!m_isInitialized)
            Initialize(transform.position, null);
    }

    public virtual void Initialize(Vector3 startPosition, GameObject modelPrefab)
    {
        m_startPosition = startPosition;

        if (modelPrefab != null)
        {
            GameObject go = Instantiate<GameObject>(modelPrefab, transform.position, transform.rotation, transform);
            m_modelTransform = go.transform;

            TransformUtilities.SetLayerToHierarchy(m_modelTransform, gameObject.layer);
        }

        m_isInitialized = true;
    }

    public void ModifySpeed(float multiplier)
    {
        m_speed *= multiplier;
    }

    protected virtual void Update()
    {
        if (m_modelTransform != null)
            m_modelTransform.Rotate(m_modelRotateVelocity * Time.deltaTime, Space.Self);

        transform.position = transform.position + transform.forward * Time.deltaTime * 0.1f * m_speed;

        UpdateTrail();
    }

    protected void UpdateTrail()
    {
        m_trailRenderer.SetPosition(0, m_startPosition);
        m_trailRenderer.SetPosition(1, transform.position);
    } 

    public virtual void Detonate()
    {
        if (m_detonateEffectPrefab != null)
            Instantiate<GameObject>(m_detonateEffectPrefab, transform.position, Quaternion.identity, GameManager.EntityRoot);

        OnDeath(false);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        Component obj = other.attachedRigidbody != null ? (Component)other.attachedRigidbody : (Component)other;

        if (m_team == Team.Enemy)
        {
            Debug.Log(DebugUtilities.AddTimestampPrefix(m_team + " Projectile '" + name + "' hit " + obj.tag + "!"), other);

            if (obj.tag == "Ground")
            {
                if (m_hitSFX != null)
                    m_hitSFX.PlayAt(transform.position);

                // TODO: Make the player lose the game

                Detonate();
            }
            else if (obj.tag == "Player")
            {
                if (m_hitSFX != null)
                    m_hitSFX.PlayAt(transform.position);

                PlayerController pc = other.GetComponentInParent<PlayerController>();
                pc.Disable();

                Detonate();
            }
            else if (obj.tag == "Fire")
            {
                OnDeath(true);
            }
        }
    }
}
