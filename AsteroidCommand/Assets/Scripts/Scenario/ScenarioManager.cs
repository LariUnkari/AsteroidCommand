using UnityEngine;
using System.Collections.Generic;

public class ScenarioManager : MonoBehaviour
{
    public static ScenarioManager s_instance;

    private ScenarioPreset m_preset;

    private float m_scenarioTime;
    private int m_currentWaveIndex;
    private int m_nextWaveIndex;
    private int m_firstActiveWaveIndex;

    private List<ScenarioWave> m_waves;
    private ScenarioWave m_cachedWave;

    private int m_playerScore;

    private Rect m_gameArea;

    public float ScenarioTime { get { return m_scenarioTime; } }
    public int WaveIndex { get { return m_currentWaveIndex; } }
    public int PlayerScore { get { return m_playerScore; } }

    private void Awake()
    {
        if (s_instance != null)
        {
            Debug.LogError(DebugUtilities.AddTimestampPrefix("Multiple instances of " + typeof(ScenarioManager) + " detected! Destroying additional instance '" + name + "' in favor of the existing one"), s_instance);
            Destroy(gameObject);
            return;
        }

        s_instance = this;
    }

    public void InitializeScenario(ScenarioPreset preset)
    {
        m_preset = preset;

        m_scenarioTime = 0f;
        m_currentWaveIndex = -1;
        m_nextWaveIndex = -1;
        m_firstActiveWaveIndex = 0;

        m_waves = new List<ScenarioWave>();

        ScenarioWave wave;
        for (int i = 0; i < m_preset.m_waveSettings.Length; i++)
        {
            wave = new ScenarioWave(m_preset.m_waveSettings[i], i);
            m_waves.Add(wave);
        }

        SetupGameArea();

        m_playerScore = 0;
    }

    private void Update()
    {
        // Don't update scenario time on the first frame
        if (m_nextWaveIndex == -1)
            m_nextWaveIndex = 0;
        else
            m_scenarioTime += Time.deltaTime;

        // Begin a new wave if it's time for it
        if (m_nextWaveIndex < m_waves.Count)
        {
            m_cachedWave = m_waves[m_nextWaveIndex];
            if (m_cachedWave.ShouldBegin(m_scenarioTime))
            {
                m_cachedWave.Begin(m_scenarioTime);

                m_currentWaveIndex = m_nextWaveIndex;
                m_nextWaveIndex++;
            }
        }
        
        // Update all active waves
        for (int i = m_firstActiveWaveIndex; i <= m_currentWaveIndex; i++)
        {
            m_cachedWave = m_waves[i];

            if (m_cachedWave.IsFinished && m_firstActiveWaveIndex == i)
            {
                m_firstActiveWaveIndex++;
                continue;
            }

            m_cachedWave.Update(Time.deltaTime);
        }
    }

    private void SetupGameArea()
    {
        Ray cameraRay;
        Vector3 bottomLeft, topRight;

        cameraRay = Camera.main.ViewportPointToRay(Vector3.zero);
        Math3D.VectorPlaneIntersect(cameraRay.origin, cameraRay.direction, Vector3.zero, Vector3.back, out bottomLeft);

        cameraRay = Camera.main.ViewportPointToRay(Vector2.one);
        Math3D.VectorPlaneIntersect(cameraRay.origin, cameraRay.direction, Vector3.zero, Vector3.back, out topRight);

        m_gameArea = new Rect(bottomLeft.x, bottomLeft.y, topRight.x - bottomLeft.x, topRight.y - bottomLeft.y);
        
        //DebugUtilities.DrawArrow(bottomLeft, topRight, Vector3.back, Color.blue, 0, 0.5f);
    }

    public static void OnSpawnEnemy(ScenarioPreset.Enemy enemy, int waveIndex, int enemyIndex)
    {
        if (s_instance != null)
            s_instance.OnSpawnEnemyInternal(enemy, waveIndex, enemyIndex);
    }

    public void OnSpawnEnemyInternal(ScenarioPreset.Enemy enemy, int waveIndex, int enemyIndex)
    {
        Debug.Log(DebugUtilities.AddTimestampPrefix("Wave " + waveIndex + " spawning enemy " + enemyIndex + " at time " + m_scenarioTime));
        
        Vector3 startPosition = Vector3.Lerp(new Vector3(m_gameArea.xMin, m_gameArea.yMax, 0f), new Vector3(m_gameArea.xMax, m_gameArea.yMax, 0f), Random.value);
        Vector3 targetPosition = Vector3.Lerp(new Vector3(m_gameArea.xMin, 0f, 0f), new Vector3(m_gameArea.xMax, 0f, 0f), Random.value);

        //DebugUtilities.DrawArrow(source, target, Vector3.back, Color.red, 0, 0.5f);

        Quaternion rotation = Quaternion.LookRotation(targetPosition - startPosition, Vector3.back);

        GameObject go = Instantiate<GameObject>(enemy.m_enemyPrefab, startPosition, rotation, GameManager.EntityRoot);
        go.name = enemy.m_enemyPrefab.name + "_" + waveIndex + "-" + enemyIndex;

        Projectile p = go.GetComponent<Projectile>();
        if (p != null)
        {
            p.Initialize(startPosition, enemy.m_modelPrefab);
            p.ModifySpeed(Random.Range(enemy.m_minSpeedModifier, enemy.m_maxSpeedModifier));
        }
    }

    public static void OnEntityDeath(Entity entity, bool giveScore)
    {
        if (s_instance != null)
            s_instance.OnEntityDeathInternal(entity, giveScore);
    }

    public void OnEntityDeathInternal(Entity entity, bool giveScore)
    {
        if (giveScore && entity.m_team == Team.Enemy)
            AddScore(entity.m_killScore);
    }

    public void AddScore(int amount)
    {
        m_playerScore += amount;
        Debug.Log(DebugUtilities.AddTimestampPrefix("Player score increased by " + amount + ". Current score: " + m_playerScore));
    }
}
