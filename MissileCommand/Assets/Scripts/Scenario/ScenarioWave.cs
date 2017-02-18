using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ScenarioWave
{
    public struct Enemy
    {
        public GameObject m_entityPrefab;
        public GameObject m_modelPrefab;
        public float m_speed;

        public Enemy(GameObject entityPrefab, GameObject modelPrefab, float speed)
        {
            m_entityPrefab = entityPrefab;
            m_modelPrefab = modelPrefab;
            m_speed = speed;
        }
    }

    public int m_index;

    private float m_waveTime;
    
    private List<Enemy[]> m_enemies;
    
    public int Index { get { return m_index; } }
    public float WaveTime { get { return m_waveTime; } }

    public ScenarioWave(int index, float waveTime)
    {
        m_index = index;
        m_waveTime = waveTime;
        m_enemies = new List<Enemy[]>();
    }

    public void AddEnemies(Enemy[] enemies)
    {
        m_enemies.Add(enemies);
    }
    
    public void SpawnEnemies()
    {
        for (int i = 0; i < m_enemies.Count; i++)
        {
            for (int e = 0; e < m_enemies[i].Length; e++)
                ScenarioManager.OnSpawnEnemy(m_enemies[i][e], m_index, e);
        }
    }
}