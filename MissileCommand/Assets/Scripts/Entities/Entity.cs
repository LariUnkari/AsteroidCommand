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

        ScenarioManager.OnRoundStartedCallback += OnRoundStarted;
        ScenarioManager.OnRoundEndedCallback += OnRoundEnded;
    }

    protected abstract void OnTriggerEnter(Collider other);

    public virtual void OnRoundStarted(int index)
    {
        // Do nothing
    }

    public virtual void OnRoundEnded(bool success)
    {
        // Do nothing
    }

    protected virtual void OnDeath(bool giveScore)
    {
        ScenarioManager.OnEntityDeath(this, giveScore);
        Destroy(gameObject);
    }

    protected virtual void OnDestroy()
    {
        ScenarioManager.OnRoundStartedCallback -= OnRoundStarted;
        ScenarioManager.OnRoundEndedCallback -= OnRoundEnded;
    }
}