using UnityEngine;

public enum Team { Enemy, Player };

public abstract class Entity : MonoBehaviour
{
    public Team m_team;
    public int m_killScore;

    private int m_id;

    public int ID { get { return m_id; } }

    public virtual void Initialize(int id)
    {
        m_id = id;
    }

    protected virtual void OnDeath(bool giveScore)
    {
        ScenarioManager.OnEntityDeath(this, giveScore);
        Destroy(gameObject);
    }

    protected abstract void OnTriggerEnter(Collider other);
}