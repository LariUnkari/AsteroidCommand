using UnityEngine;
using System.Collections.Generic;

public class AIController : TurretController
{
    [System.Serializable]
    public class EnemyContact
    {
        public Entity m_entity;
        public float m_speed;
        public float m_timeToTarget;
        public float m_threat;
        public bool m_wasShotAt;

        public Vector3 Position { get { return m_entity != null ? m_entity.transform.position : Vector3.up * 3; } }
        public Vector3 Direction { get { return m_entity != null ? m_entity.transform.forward : Vector3.up; } }
        public Vector3 Velocity { get { return Direction * m_speed; } }

        public EnemyContact(Entity entity, float speed, float timeToTarget)
        {
            m_entity = entity;
            m_speed = speed;
            m_timeToTarget = timeToTarget;
            m_threat = 0f;
            m_wasShotAt = false;
        }
    }

    public float m_defaultScanInterval = 0.75f;
    public float m_defaultTargetSeekInterval = 0.5f;
    public float m_perRoundSpeedUp = 1.05f;

    private float m_projectileSpeed;
    private float m_timeToTarget;

    private List<Entity> m_pendingContacts;
    private Dictionary<int, EnemyContact> m_enemies;

    private EnemyContact m_currentTarget;

    private float m_scanInterval;
    private float m_scanT;

    private float m_targetSeekInterval;
    private float m_targetSeekT;

    protected override void Awake()
    {
        base.Awake();

        m_projectileSpeed = (m_turret != null ? m_turret.m_projectileSpeed : 1f) * (ScenarioManager.Scenario != null ? ScenarioManager.Scenario.m_globalSpeedMultiplier : 1f);
        m_pendingContacts = new List<Entity>();
        m_enemies = new Dictionary<int, EnemyContact>();

        ScenarioManager.OnEntitySpawnedCallback += OnEntitySpawned;
        ScenarioManager.OnEntityDeathCallback += OnEntityDeath;
    }

    protected override void Update()
    {
        if (!m_canControl)
            return;

        if (m_pendingContacts.Count > 0)
        {
            foreach (Entity entity in m_pendingContacts)
            {
                float speed, timeToTarget;
                if (entity is Projectile)
                {
                    Projectile p = entity as Projectile;
                    speed = p.Speed;
                    timeToTarget = Vector3.Distance(entity.transform.position, p.TargetPosition) / p.Speed;
                }
                else
                {
                    speed = 0f;
                    timeToTarget = -1f;
                }

                EnemyContact newEnemy = new EnemyContact(entity, speed, timeToTarget);
                m_enemies.Add(entity.ID, newEnemy);

                if (m_currentTarget == null || m_currentTarget.m_entity == null)
                    SetTargetEnemy(newEnemy);
            }

            m_pendingContacts.Clear();
        }

        foreach (EnemyContact ec in m_enemies.Values)
        {
            ec.m_timeToTarget -= Time.deltaTime;
            ec.m_threat = ec.m_timeToTarget > 0f ? 1f / (ec.m_timeToTarget) : 0f;
        }
        
        if (m_scanT <= 0f)
        {
            if (m_currentTarget == null || m_currentTarget.m_entity == null)
            {
                m_scanT = m_scanInterval;
                SetTargetEnemy(FindHighestThreat());
            }
        }
        else
            m_scanT -= Time.deltaTime;
        
        if (m_currentTarget != null && m_currentTarget.m_entity != null)
        {
            Vector3 targetPosition;
            bool solutionFound = false;
            if (Math3D.TryGetInterceptPoint(transform.position, Vector3.zero, m_projectileSpeed, m_currentTarget.Position, m_currentTarget.Velocity, out targetPosition))
                solutionFound = targetPosition.y > ScenarioManager.GameArea.yMin && targetPosition.y < ScenarioManager.GameArea.yMax;
            
            if (solutionFound)
                SetTarget(targetPosition);

            if (m_targetSeekT <= 0f)
            {
                if (solutionFound && m_canFire)
                {
                    m_turret.Fire();
                    m_currentTarget.m_wasShotAt = true;
                    m_currentTarget = null;
                    m_targetSeekT = m_targetSeekInterval;
                }
            }
            else
                m_targetSeekT -= Time.deltaTime;
        }
    }

    private EnemyContact FindHighestThreat()
    {
        EnemyContact currentEnemyContact = null;
        
        foreach (EnemyContact ec in m_enemies.Values)
        {
            if (ec.m_wasShotAt)
                continue;

            if (currentEnemyContact == null || ec.m_threat > currentEnemyContact.m_threat)
                currentEnemyContact = ec;
        }
        
        return currentEnemyContact;
    }

    private void SetTargetEnemy(EnemyContact target)
    {
        if (m_currentTarget == target)
            return;

        m_currentTarget = target;
        m_targetSeekT = m_targetSeekInterval;

        if (target != null && target.m_entity != null)
        {
            //Debug.LogWarning(DebugUtilities.AddTimestampPrefix(GetType() + ": New target set: " + target.m_entity.name + ", threat=" + target.m_threat), target.m_entity);
            DebugUtilities.DrawArrow(transform.position, target.Position, Vector3.back, Color.red, 0, 1f);
        }
        //else
            //Debug.LogWarning(DebugUtilities.AddTimestampPrefix(GetType() + ": Target cleared"));
    }

    private void SetActionIntervals(int roundIndex)
    {
        m_scanInterval = m_defaultScanInterval / Mathf.Pow(m_perRoundSpeedUp, roundIndex);
        m_targetSeekInterval = m_defaultTargetSeekInterval / Mathf.Pow(m_perRoundSpeedUp, roundIndex);
    }

    public override void SetActive(bool isActive)
    {
        base.SetActive(isActive);

        if (isActive)
            SetActionIntervals(ScenarioManager.RoundIndex);
    }

    public override void OnRoundStarted(int index)
    {
        base.OnRoundStarted(index);

        SetActionIntervals(index);
    }

    private void OnEntitySpawned(Entity entity)
    {
        if (entity.m_team == m_turret.m_team)
            return;

        if (!m_enemies.ContainsKey(entity.ID))
        {
            //Debug.Log(DebugUtilities.AddTimestampPrefix(GetType() + ": New contact[" + entity.ID + "]: " + entity.name + ")"), entity);

            // Add to pending contacts since the entity might not have been properly initialized yet
            m_pendingContacts.Add(entity);
        }
        else
            Debug.LogWarning(DebugUtilities.AddTimestampPrefix("AIController: Entity spawned with an ID(" + entity.ID + ") that was already registered!"), entity);
    }

    private void OnEntityDeath(Entity entity)
    {
        m_enemies.Remove(entity.ID);
    }

    private void OnDestroy()
    {
        ScenarioManager.OnEntitySpawnedCallback -= OnEntitySpawned;
        ScenarioManager.OnEntityDeathCallback -= OnEntityDeath;
    }
}
