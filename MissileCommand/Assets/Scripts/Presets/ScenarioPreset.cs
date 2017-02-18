using UnityEngine;

[CreateAssetMenuAttribute(fileName = "New ScenarioPreset.asset", menuName = "ScriptableObject/ScenarioPreset")]
public class ScenarioPreset : ScriptableObject
{
    [System.Serializable]
    public class Enemy
    {
        public GameObject m_enemyPrefab;
        public GameObject m_modelPrefab;

        public int m_firstRoundToSpawn = 1;                 // Round number (starts from 1) to start spawning the enemy on

        public float m_speedMin = 5f;                       // Minimum default speed
        public float m_speedMax = 6f;                       // Maximum default speed
        public AnimationCurve m_speedCurve =                // Curve to define probability of speeds between min and max
            new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0f), new Keyframe(1f, 1f) });

        public float m_speedMultiplierGain = 1.05f;         // Multiplier to increase the speed with each round
        public float m_speedMultiplierMax = 2f;             // Maximum speed multiplier. No cap whatsoever if <= 1.0

        public int m_spawnCountMin = 10;                    // Minimum number of enemies to spawn in a round
        public int m_spawnCountMax = 30;                    // Maximum number of enemies to spawn in a round
        public int m_spawnCountRoundMax = 20;               // Round number (starts from 1) to reach maximum spawn count on
        public AnimationCurve m_spawnCountCurve =           // Curve to define minimum and maximum spawn count during the rounds
            new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0f), new Keyframe(1f, 1f) });

        public float m_waveCountMultiplier = 0.4f;          // Multiply total wave count with this to find how many waves to spawn
        public AnimationCurve m_waveProbabilityCurve =      // Curve to define probability of a wave to spawn at a specific time during the round
            new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0f), new Keyframe(1f, 1f) });
    }

    public Enemy[] m_enemyPresets = new Enemy[] { new Enemy() };

    public int m_waveCountMin = 10;                 // Minimum number of waves to consider
    public int m_waveCountMax = 20;                 // Maximum number of waves to consider
    public int m_waveCountRoundMax = 10;            // Round number (starts from 1) to reach maximum wave count on
    public AnimationCurve m_waveCountCurve =        // Curve to define the increase of wave count from min to max
        new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0f), new Keyframe(1f, 1f) });

    public float m_waveIntervalMin = 0.5f;          // Minimum interval between waves (fastest rate)
    public float m_waveIntervalMax = 1f;            // Maximum interval between waves (slowest rate)
    public int m_waveIntervalRoundMax = 20;         // Round number (starts from 1) to reach minimum wave interval on
    public AnimationCurve m_waveIntervalCurve =     // Curve to define wave interval change from max to min
        new AnimationCurve(new Keyframe[] { new Keyframe(0f, 1f), new Keyframe(1f, 0f) });
}
