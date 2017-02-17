using UnityEngine;

[CreateAssetMenuAttribute(fileName = "New ScenarioPreset.asset", menuName = "ScriptableObject/ScenarioPreset")]
public class ScenarioPreset : ScriptableObject
{
    [System.Serializable]
    public class Wave
    {
        public float m_waveTime;
        public Enemy[] m_enemies;
    }

    [System.Serializable]
    public class Enemy
    {
        public float m_spawnInterval = 0.25f;
        public GameObject m_enemyPrefab;
        public GameObject m_modelPrefab;
        public float m_minSpeedModifier = 0.9f;
        public float m_maxSpeedModifier = 1f;
        public int m_enemyCount = 1;
    }
    
    public Wave[] m_waveSettings;
}
