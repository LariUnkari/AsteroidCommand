using UnityEngine;
using System.Collections;
using System;

public class Projectile : Entity
{
    public LineRenderer m_trailRenderer;

    public Vector3 m_modelRotateVelocity = Vector3.forward * 360f / 5f;

    public SoundEffectPreset m_hitSFX;
    public GameObject m_detonateEffectPrefab;
    
    protected Vector3 m_startPosition;
    protected Vector3 m_targetPosition;
    protected float m_speed;

    protected Vector3 m_velocity;
    protected float m_movementSpeed;

    protected Transform m_modelTransform;

    protected bool m_isInitialized;

    public Vector3 TargetPosition { get { return m_targetPosition; } }
    public float Speed { get { return m_speed; } }
    public Vector3 Velocity { get { return m_velocity; } }

    private void Start()
    {
        if (!m_isInitialized)
            Initialize(-1, transform.position, Vector3.zero, 5f, null);
    }

    public virtual void Initialize(int id, Vector3 startPosition, Vector3 targetPosition, float speed, GameObject modelPrefab)
    {
        base.Initialize(id);

        m_startPosition = startPosition;
        m_targetPosition = targetPosition;

        m_speed = speed;

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

        m_velocity = transform.forward * m_speed;
        m_movementSpeed = m_velocity.magnitude;
        transform.position = transform.position + m_velocity * Time.deltaTime;

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
            ScenarioManager.OnSpawnEntity(m_detonateEffectPrefab, transform.position, Quaternion.identity);

        OnDeath(false);
    }

    public override void OnRoundEnded(bool success)
    {
        base.OnRoundEnded(success);

        m_speed = 0f;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        Component obj = other.attachedRigidbody != null ? (Component)other.attachedRigidbody : (Component)other;

        if (m_team == Team.Enemy)
        {
            //Debug.Log(DebugUtilities.AddTimestampPrefix(m_team + " Projectile '" + name + "' hit " + obj.tag + "!"), other);

            if (obj.tag == "Ground")
            {
                if (m_hitSFX != null)
                    m_hitSFX.PlayAt(transform.position, Environment.AudioRoot);

                Detonate();

                ScenarioManager.EndRound(false);
            }
            else if (obj.tag == "Player")
            {
                if (m_hitSFX != null)
                    m_hitSFX.PlayAt(transform.position, Environment.AudioRoot);

                Detonate();

                ScenarioManager.OnPlayerHit();
            }
            else if (obj.tag == "Fire")
            {
                OnDeath(true);
            }
        }
    }
}
