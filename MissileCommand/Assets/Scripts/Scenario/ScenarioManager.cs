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

    private Dictionary<int, Entity> m_entities;
    private int m_entitiesSpawned;

    private int m_playerScore;
    private int m_playerShotsFired;

    private Rect m_gameArea;

    public static float ScenarioTime { get { return s_instance != null ? s_instance.m_scenarioTime : -1f; } }
    public static int WaveIndex { get { return s_instance != null ? s_instance.m_currentWaveIndex : -1; } }
    public static int Score { get { return s_instance != null ? s_instance.m_playerScore : -1; } }
    public static int ShotsFired { get { return s_instance != null ? s_instance.m_playerShotsFired : -1; } }

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
        m_entities = new Dictionary<int, Entity>();

        ScenarioWave wave;
        for (int i = 0; i < m_preset.m_waveSettings.Length; i++)
        {
            wave = new ScenarioWave(m_preset.m_waveSettings[i], i);
            m_waves.Add(wave);
        }

        SetupGameArea();

        m_playerScore = 0;
        m_playerShotsFired = 0;

        UserInterface.SetScore(m_playerScore);
        UserInterface.SetShotsFired(m_playerShotsFired);
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

    public void StartRoundEnd()
    {
        Debug.Log(DebugUtilities.AddTimestampPrefix("Round is ending..."));

        if (GameManager.ActivePlayerController != null)
            GameManager.ActivePlayerController.Disable();

        StartCoroutine(RoundEndRoutine());
    }

    private System.Collections.IEnumerator RoundEndRoutine()
    {
        Time.timeScale = 0f;

        UserInterface.ShowRoundEndWidget(true);
        UserInterface.SetPenaltyAmount(0);

        yield return new WaitForSecondsRealtime(1f);

        Debug.Log(DebugUtilities.AddTimestampPrefix("Shots fired: " + m_playerShotsFired));

        SoundEffectPreset penaltySFX = UserInterface.PenaltyPointSFX;

        float optimalPenaltyCycleDuration = 3f;
        float minimumPenaltyCycleInterval = 0.15f;
        float maximumPenaltyCycleInterval = 0.4f;

        float penaltyCycleInterval = maximumPenaltyCycleInterval;
        if (m_playerShotsFired * maximumPenaltyCycleInterval > optimalPenaltyCycleDuration)
            penaltyCycleInterval = Mathf.Clamp(optimalPenaltyCycleDuration / m_playerShotsFired, minimumPenaltyCycleInterval, maximumPenaltyCycleInterval);

        Debug.Log(DebugUtilities.AddTimestampPrefix("Shots penalty being applied! Cycle interval: " + penaltyCycleInterval));

        int penaltyAmount = 0;
        while (m_playerShotsFired > 0)
        {
            ModifyShotsFired(-1);
            ModifyScore(-5);

            penaltyAmount += 5;
            UserInterface.SetPenaltyAmount(penaltyAmount);

            if (penaltySFX != null)
                penaltySFX.PlayAt(Camera.main.transform.position);

            yield return new WaitForSecondsRealtime(penaltyCycleInterval);
        }

        yield return new WaitForSecondsRealtime(2f);

        UserInterface.ShowRoundEndWidget(false);

        foreach (Entity e in m_entities.Values)
            Destroy(e.gameObject);

        m_entities.Clear();

        // TODO: Return to initial game state
    }

    public static void ModifyScore(int amount)
    {
        if (s_instance == null)
            return;

        s_instance.m_playerScore += amount;
        //Debug.Log(DebugUtilities.AddTimestampPrefix("Player score modified by " + amount + ". Current score: " + s_instance.m_playerScore));

        UserInterface.SetScore(s_instance.m_playerScore);
    }

    public static void ModifyShotsFired(int amount)
    {
        if (s_instance == null)
            return;

        s_instance.m_playerShotsFired += amount;
        //Debug.Log(DebugUtilities.AddTimestampPrefix("Player shots fired modified by " + amount + ". Current amount: " + s_instance.m_playerShotsFired));

        UserInterface.SetShotsFired(s_instance.m_playerShotsFired);
    }

    public static void OnSpawnEnemy(ScenarioPreset.Enemy enemy, int waveIndex, int enemyIndex)
    {
        if (s_instance != null)
            s_instance.OnSpawnEnemyInternal(enemy, waveIndex, enemyIndex);
    }

    public void OnSpawnEnemyInternal(ScenarioPreset.Enemy enemy, int waveIndex, int enemyIndex)
    {
        //Debug.Log(DebugUtilities.AddTimestampPrefix("Wave " + waveIndex + " spawning enemy " + enemyIndex + " at time " + m_scenarioTime));
        
        Vector3 startPosition = Vector3.Lerp(new Vector3(m_gameArea.xMin, m_gameArea.yMax, 0f), new Vector3(m_gameArea.xMax, m_gameArea.yMax, 0f), Random.value);
        Vector3 targetPosition = Vector3.Lerp(new Vector3(m_gameArea.xMin, 0f, 0f), new Vector3(m_gameArea.xMax, 0f, 0f), Random.value);
        Quaternion rotation = Quaternion.LookRotation(targetPosition - startPosition, Vector3.back);
        
        Entity entity = OnSpawnEntityInternal(enemy.m_enemyPrefab, startPosition, rotation);
        if (entity != null)
        {
            entity.name = enemy.m_enemyPrefab.name + "_" + waveIndex + "-" + enemyIndex;

            if (entity is Projectile)
            {
                Projectile p = entity as Projectile;
                p.Initialize(p.ID, startPosition, enemy.m_modelPrefab);
                p.ModifySpeed(Random.Range(enemy.m_minSpeedModifier, enemy.m_maxSpeedModifier));
            }
        }

        //DebugUtilities.DrawArrow(source, target, Vector3.back, Color.red, 0, 0.5f);
    }

    public static Entity OnSpawnEntity(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (s_instance != null)
            return s_instance.OnSpawnEntityInternal(prefab, position, rotation);

        return null;
    }

    public Entity OnSpawnEntityInternal(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject go = Instantiate<GameObject>(prefab, position, rotation, GameManager.EntityRoot);

        Entity entity = go.GetComponent<Entity>();
        if (entity != null)
        {
            entity.Initialize(m_entitiesSpawned);
            m_entities.Add(m_entitiesSpawned, entity);
            m_entitiesSpawned++;
            return entity;
        }

        return null;
    }

    public static void OnEntityDeath(Entity entity, bool giveScore)
    {
        if (giveScore && entity.m_team == Team.Enemy)
            ModifyScore(entity.m_killScore);

        if (s_instance != null)
            s_instance.m_entities.Remove(entity.ID);
    }

    public static void OnPlayerHit()
    {
        if (GameManager.ActivePlayerController != null)
            GameManager.ActivePlayerController.Disable();
    }

    public static void OnRoundEnd()
    {
        if (s_instance == null)
            return;

        s_instance.StartRoundEnd();
    }
}
