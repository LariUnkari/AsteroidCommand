using UnityEngine;
using System.Collections.Generic;

public class ScenarioManager : MonoBehaviour
{
    public static ScenarioManager s_instance;

    private ScenarioPreset m_preset;

    private ScenarioRound m_currentRound;
    private int m_nextRoundIndex;

    private Dictionary<int, Entity> m_entities;
    private int m_entitiesSpawned;

    private int m_playerScore;
    private int m_playerShotsFired;

    private Rect m_gameArea;

    public static int Score { get { return s_instance != null ? s_instance.m_playerScore : -1; } }
    public static int ShotsFired { get { return s_instance != null ? s_instance.m_playerShotsFired : -1; } }

    public static float RoundTime { get { return s_instance != null && s_instance.m_currentRound != null ? s_instance.m_currentRound.Time : -1f; } }
    public static int WaveIndex { get { return s_instance != null && s_instance.m_currentRound != null ? s_instance.m_currentRound.WaveIndex : -1; } }

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

        m_currentRound = null;
        
        m_entities = new Dictionary<int, Entity>();

        SetupGameArea();

        m_playerScore = 0;
        m_playerShotsFired = 0;

        UserInterface.SetScore(m_playerScore);

        Time.timeScale = 1f;
        StartRound(0);
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
        if (m_currentRound != null && m_currentRound.HasStarted)
            m_currentRound.Update(Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (Input.GetKey(KeyCode.LeftShift))
                m_nextRoundIndex += 10;
            else
                m_nextRoundIndex++;

            Debug.Log(DebugUtilities.AddTimestampPrefix("Debug increase next round index to " + m_nextRoundIndex));
        }
    }

    public void StartRound(int index)
    {
        m_playerShotsFired = 0;
        UserInterface.SetShotsFired(m_playerShotsFired);
        GameManager.SetVolume(0f);

        m_currentRound = ScenarioGenerator.GenerateRound(index, m_preset);
        m_nextRoundIndex = index + 1;

        StartCoroutine(RoundStartRoutine());
    }

    private System.Collections.IEnumerator RoundStartRoutine()
    {
        yield return new WaitForSecondsRealtime(1f);

        UserInterface.ShowRoundStartWidget(m_currentRound.Index);

        if (UserInterface.RoundStartSFX != null)
        {
            float repeatInterval = 0.3f;
            int repeatCount = 4;

            int repeats = repeatCount;
            while (repeats-- > 0)
            {
                UserInterface.RoundStartSFX.PlayAt(Camera.main.transform.position, GameManager.AudioRoot);

                yield return new WaitForSecondsRealtime(repeatInterval);
            }

            yield return new WaitForSecondsRealtime(2f - repeatCount * repeatInterval);
        }
        else
            yield return new WaitForSecondsRealtime(2f);

        UserInterface.HideRoundStartWidget();
        m_currentRound.Start();
    }

    public void EndRound(bool success)
    {
        Debug.Log(DebugUtilities.AddTimestampPrefix("Round is ending..."));
        m_currentRound.End();

        StartCoroutine(RoundEndRoutine(success));
    }

    private System.Collections.IEnumerator RoundEndRoutine(bool success)
    {
        // Pause time on defeat
        if (!success)
            Time.timeScale = 0f;

        UserInterface.ShowRoundEndWidget(success);
        UserInterface.SetPenaltyAmount(0);

        yield return new WaitForSecondsRealtime(1f);

        //Debug.Log(DebugUtilities.AddTimestampPrefix("Shots fired this round: " + m_playerShotsFired));

        yield return UserInterface.StartPenaltyRoutine(m_playerShotsFired);

        UserInterface.HideRoundEndWidget();

        // TODO: Cache entities instead of outright destroying them
        foreach (Entity e in m_entities.Values)
            Destroy(e.gameObject);

        m_entities.Clear();
        
        // TODO: Go to demo mode
        if (success)
            StartRound(m_nextRoundIndex);
        else
            InitializeScenario(m_preset);
    }

    public static void ModifyScore(int amount)
    {
        if (s_instance == null)
            return;

        //Debug.Log(DebugUtilities.AddTimestampPrefix("Player score modified by " + amount));
        SetScore(s_instance.m_playerScore + amount);
    }

    public static void SetScore(int amount)
    {
        if (s_instance == null)
            return;

        s_instance.m_playerScore = amount;
        //Debug.Log(DebugUtilities.AddTimestampPrefix("Player score set to " + s_instance.m_playerScore));

        UserInterface.SetScore(s_instance.m_playerScore);
    }

    public static void ModifyShotsFired(int amount)
    {
        if (s_instance == null)
            return;

        //Debug.Log(DebugUtilities.AddTimestampPrefix("Player shots fired modified by " + amount));
        SetShotsFired(s_instance.m_playerShotsFired + amount);
    }

    public static void SetShotsFired(int amount)
    {
        if (s_instance == null)
            return;

        s_instance.m_playerShotsFired = amount;
        //Debug.Log(DebugUtilities.AddTimestampPrefix("Player shots fired set to " + s_instance.m_playerShotsFired));

        UserInterface.SetShotsFired(s_instance.m_playerShotsFired);
    }

    public static void OnSpawnEnemy(ScenarioWave.Enemy enemy, int waveIndex, int enemyIndex)
    {
        if (s_instance != null)
            s_instance.OnSpawnEnemyInternal(enemy, waveIndex, enemyIndex);
    }

    public void OnSpawnEnemyInternal(ScenarioWave.Enemy enemy, int waveIndex, int enemyIndex)
    {
        //Debug.Log(DebugUtilities.AddTimestampPrefix("Wave " + waveIndex + " spawning enemy " + enemyIndex + " at time " + RoundTime));
        
        Vector3 startPosition = Vector3.Lerp(new Vector3(m_gameArea.xMin, m_gameArea.yMax, 0f), new Vector3(m_gameArea.xMax, m_gameArea.yMax, 0f), Random.value);
        Vector3 targetPosition = Vector3.Lerp(new Vector3(m_gameArea.xMin, 0f, 0f), new Vector3(m_gameArea.xMax, 0f, 0f), Random.value);
        Quaternion rotation = Quaternion.LookRotation(targetPosition - startPosition, Vector3.back);
        
        Entity entity = OnSpawnEntityInternal(enemy.m_entityPrefab, startPosition, rotation);
        if (entity != null)
        {
            entity.name = enemy.m_entityPrefab.name + "_" + waveIndex + "-" + enemyIndex;

            if (entity is Projectile)
            {
                Projectile p = entity as Projectile;
                p.Initialize(p.ID, startPosition, enemy.m_speed, enemy.m_modelPrefab);
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
        if (s_instance != null)
            s_instance.OnEntityDeathInternal(entity, giveScore);
    }

    private void OnEntityDeathInternal(Entity entity, bool giveScore)
    {
        if (giveScore && entity.m_team == Team.Enemy)
            ModifyScore(entity.m_killScore);

        m_entities.Remove(entity.ID);

        if (m_entities.Count == 0 && m_currentRound != null && m_currentRound.IsLastWave)
        {
            // All waves cleared, player won the round
            OnRoundEnd(true);
        }
    }

    public static void OnPlayerHit()
    {
        if (GameManager.ActivePlayerController != null)
            GameManager.ActivePlayerController.SetActive(false);
    }

    public static void OnRoundEnd(bool success)
    {
        if (s_instance == null)
            return;

        s_instance.EndRound(success);
    }
}
