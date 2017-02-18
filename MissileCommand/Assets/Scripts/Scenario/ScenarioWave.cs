using UnityEngine;
using System.Collections;

[System.Serializable]
public class ScenarioWave
{
    public delegate void OnSpawnEnemy(ScenarioPreset.Enemy enemy, int waveIndex, int enemyIndex);
    public OnSpawnEnemy OnSpawnEnemyCallback;

    public int m_index;
    public ScenarioPreset.Wave m_preset;

    [SerializeField] private bool m_isStarted;
    [SerializeField] private bool m_isFinished;

    [SerializeField] private float m_waveT;

    [SerializeField] private int m_currentWaveEnemyIndex;
    [SerializeField] private ScenarioPreset.Enemy m_nextEnemy;

    public bool IsStarted { get { return m_isStarted; } }
    public bool IsFinished { get { return m_isFinished; } }

    public int WaveEnemyIndex { get { return m_currentWaveEnemyIndex; } }

    public ScenarioWave(ScenarioPreset.Wave preset, int index)
    {
        m_index = index;
        m_preset = preset;
        m_isStarted = false;
        m_isFinished = false;
    }

    public void Begin(float scenarioTime)
    {
        if (m_preset.m_enemies.Length == 0)
        {
            Finish();
            return;
        }

        m_isStarted = true;
        m_currentWaveEnemyIndex = 0;
        m_nextEnemy = m_preset.m_enemies[m_currentWaveEnemyIndex];
        m_waveT = scenarioTime - m_preset.m_waveTime;

        Debug.Log(DebugUtilities.AddTimestampPrefix("Begin Wave " + m_index + " at time " + scenarioTime));
    }

    public void Update(float deltaTime)
    {
        if (m_waveT >= m_nextEnemy.m_spawnInterval)
        {
            m_waveT -= m_nextEnemy.m_spawnInterval;
        }
        else
            m_waveT += deltaTime;

        if (m_waveT >= m_nextEnemy.m_spawnInterval)
            SpawnEnemy(m_nextEnemy);

        if (m_currentWaveEnemyIndex >= m_nextEnemy.m_enemyCount)
            Finish();
    }

    public void Finish()
    {
        m_isFinished = true;
    }

    public bool ShouldBegin(float scenarioTime)
    {
        return !m_isStarted && scenarioTime >= m_preset.m_waveTime;
    }

    private void SpawnEnemy(ScenarioPreset.Enemy enemy)
    {
        ScenarioManager.OnSpawnEnemy(enemy, m_index, m_currentWaveEnemyIndex);

        m_currentWaveEnemyIndex++;
    }
}