using UnityEngine;
using System.Collections;

[System.Serializable]
public class ScenarioRound
{
    private int m_index;

    private bool m_hasStarted;
    private bool m_hasEnded;

    private float m_time;
    
    private ScenarioWave[] m_waves;

    private ScenarioWave m_cachedWave;
    
    private int m_currentWaveIndex;
    private int m_nextWaveIndex;

    public int Index { get { return m_index; } }
    public float Time { get { return m_time; } }
    public bool HasStarted { get { return m_hasStarted; } }
    public bool HasEnded { get { return m_hasEnded; } }
    public int WaveIndex { get { return m_currentWaveIndex; } }
    public bool IsLastWave { get { return m_currentWaveIndex == m_waves.Length - 1; } }

    public ScenarioRound(int index, ScenarioWave[] waves)
    {
        m_index = index;
        m_waves = waves;
    }

    public void Init()
    {
        m_time = 0f;
        m_hasStarted = false;

        m_currentWaveIndex = -1;
        m_nextWaveIndex = 0;
    }

    public void Start()
    {
        m_hasStarted = true;

        if (GameManager.ActivePlayerController != null)
        {
            GameManager.ActivePlayerController.SetCanFire(true);
            GameManager.ActivePlayerController.SetCanControl(true);
        }
    }

    public void End()
    {
        if (GameManager.ActivePlayerController != null)
        {
            GameManager.ActivePlayerController.SetCanFire(false);
            GameManager.ActivePlayerController.SetCanControl(false);
        }
    }

    public void Update(float deltaTime)
    {
        m_time += deltaTime;

        // Begin a new wave if it's time for it
        if (m_nextWaveIndex < m_waves.Length)
        {
            m_cachedWave = m_waves[m_nextWaveIndex];
            if (m_cachedWave == null)
            {
                m_currentWaveIndex = m_nextWaveIndex;
                m_nextWaveIndex++;
            }
            else if (m_time >= m_cachedWave.WaveTime)
            {
                m_cachedWave.SpawnEnemies();

                m_currentWaveIndex = m_nextWaveIndex;
                m_nextWaveIndex++;
            }
        }
    }
}
