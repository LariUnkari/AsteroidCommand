using UnityEngine;
using System.Collections.Generic;

public class ScenarioManager : MonoBehaviour
{
    public delegate void OnRoundStartedDelegate(int index);
    public static OnRoundStartedDelegate OnRoundStartedCallback;
    public delegate void OnRoundEndedDelegate(bool success);
    public static OnRoundEndedDelegate OnRoundEndedCallback;
    public delegate void OnEntitySpawnedDelegate(Entity entity);
    public static OnEntitySpawnedDelegate OnEntitySpawnedCallback;
    public delegate void OnEntityDeathDelegate(Entity entity);
    public static OnEntityDeathDelegate OnEntityDeathCallback;

    public static ScenarioManager s_instance;

    private ScenarioPreset m_preset;

    private ScenarioRound m_currentRound;
    private int m_nextRoundIndex;

    private Dictionary<int, Entity> m_entities;
    private int m_entitiesSpawned;
    
    private int m_highScore;
    private int m_playerScore;
    private int m_playerShotsFired;

    private Rect m_gameArea;

    private System.Collections.IEnumerator m_roundRoutineIEnumerator;

    public static bool IsLoaded { get { return s_instance != null; } }

    public static ScenarioPreset Scenario { get { return s_instance != null ? s_instance.m_preset : null; } }

    public static int HighScore { get { return s_instance != null ? s_instance.m_highScore : 0; } }
    public static int CurrentScore { get { return s_instance != null ? s_instance.m_playerScore : -1; } }
    public static int ShotsFired { get { return s_instance != null ? s_instance.m_playerShotsFired : -1; } }

    public static float RoundTime { get { return s_instance != null && s_instance.m_currentRound != null ? s_instance.m_currentRound.Time : -1f; } }
    public static int RoundIndex { get { return s_instance != null && s_instance.m_currentRound != null ? s_instance.m_currentRound.Index : -1; } }
    public static int WaveIndex { get { return s_instance != null && s_instance.m_currentRound != null ? s_instance.m_currentRound.WaveIndex : -1; } }

    public static Rect GameArea { get { return s_instance != null ? s_instance.m_gameArea : new Rect(); } }

    private void Awake()
    {
        if (s_instance != null)
        {
            Debug.LogError(DebugUtilities.AddTimestampPrefix("Multiple instances of " + typeof(ScenarioManager) + " detected! Destroying additional instance '" + name + "' in favor of the existing one"), s_instance);
            Destroy(gameObject);
            return;
        }

        s_instance = this;

        Debug.Log(DebugUtilities.AddTimestampPrefix("ScenarioManager.Awake()"), this);
    }

    public static void InitializeScenario(ScenarioPreset preset)
    {
        if (s_instance != null)
            s_instance.InitializeScenarioInternal(preset);
    }

    public void InitializeScenarioInternal(ScenarioPreset preset)
    {
        if (preset == null)
            return;

        if (m_preset != null)
        {
            if (GameManager.ActivePlayerController != null)
                Destroy(GameManager.ActivePlayerController.gameObject);
        }

        m_preset = preset;

        m_currentRound = null;

        SetupGameArea();

        if (m_entities == null)
            m_entities = new Dictionary<int, Entity>();
        else
        {
            // TODO: Cache entities instead of outright destroying them
            foreach (Entity e in m_entities.Values)
                Destroy(e.gameObject);

            m_entities.Clear();
        }

        m_playerScore = 0;
        m_playerShotsFired = 0;

        UserInterface.SetScore(m_playerScore);

        SetHighScore(LoadHighScore());

        if (m_preset.m_playerPrefab != null)
        {
            Vector3 position = Environment.PlayerSpawn != null ? Environment.PlayerSpawn.position : Vector3.zero;
            Quaternion rotation = Environment.PlayerSpawn != null ? Environment.PlayerSpawn.rotation : Quaternion.identity;
            GameObject go = Instantiate<GameObject>(m_preset.m_playerPrefab, position, rotation, Environment.EntityRoot);
            go.name = m_preset.m_playerPrefab.name;

            TurretController tc;
            if (GameManager.State == GameState.Game)
                tc = go.GetComponent<PlayerController>();
            else
                tc = go.GetComponent<AIController>();

            GameManager.SetActiveTurretController(tc, false);
            tc.SetCanControl(false);
        }
        else
            Debug.LogError(DebugUtilities.AddTimestampPrefix("ScenarioPreset doesn't define a player prefab!"), m_preset);
    }

    private void SetupGameArea()
    {
        // Limit the game area into a 4:3 shape
        float width = 4f * Camera.main.orthographicSize * 2f / 3f;
        Vector3 topRight = Camera.main.transform.position + new Vector3(width / 2f, Camera.main.orthographicSize, -Camera.main.transform.position.z);
        Vector3 bottomLeft = Camera.main.transform.position - new Vector3(width / 2f, Camera.main.orthographicSize, Camera.main.transform.position.z);

        // Lift up the game area bottom to floor level
        m_gameArea = new Rect(bottomLeft.x, 0f, topRight.x - bottomLeft.x, topRight.y - 0f);
        DebugUtilities.DrawArrow(bottomLeft, topRight, Vector3.back, Color.blue, 0, 0.5f);
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

    /// <summary>
    /// Only one round routine can be active at any given time, this makes sure that holds true
    /// </summary>
    /// <param name="routine">IEnumerator to run</param>
    /// <returns>Started Coroutine</returns>
    private Coroutine StartRoundRoutine(System.Collections.IEnumerator routine)
    {
        if (m_roundRoutineIEnumerator != null)
        {
            Debug.LogWarning(DebugUtilities.AddTimestampPrefix("ScenarioManager stopping previous round routine!"));
            StopCoroutine(m_roundRoutineIEnumerator);
        }

        m_roundRoutineIEnumerator = routine;
        return StartCoroutine(routine);
    }

    public void StartRoundInternal(int index)
    {
        Time.timeScale = 1f;

        SetShotsFired(0);

        if (GameManager.ActivePlayerController != null)
            GameManager.ActivePlayerController.SetCanFire(false);

        m_currentRound = ScenarioGenerator.GenerateRound(index, m_preset);
        m_nextRoundIndex = index + 1;

        StartRoundRoutine(RoundStartRoutine());
    }

    private System.Collections.IEnumerator RoundStartRoutine()
    {
        UserInterface.StopRoundRoutines();
        UserInterface.HideRoundEndWidget();

        yield return new WaitForSecondsRealtime(1f);

        if (GameManager.State == GameState.Game)
        {
            UserInterface.ShowRoundStartWidget(m_currentRound.Index);

            yield return UserInterface.StartAlertRoutine();
        }

        UserInterface.HideRoundStartWidget();
        m_currentRound.Start();

        if (OnRoundStartedCallback != null)
            OnRoundStartedCallback.Invoke(m_currentRound.Index);
    }

    public void EndRoundInternal(bool success)
    {
        Debug.Log(DebugUtilities.AddTimestampPrefix("Round is ending..."));

        m_currentRound.End();

        if (OnRoundEndedCallback != null)
            OnRoundEndedCallback.Invoke(success);

        StartRoundRoutine(RoundEndRoutine(success));
    }

    private System.Collections.IEnumerator RoundEndRoutine(bool success)
    {
        UserInterface.StopRoundRoutines();

        if (GameManager.State == GameState.Game)
        {
            UserInterface.ShowRoundEndWidget(success);
            UserInterface.SetPenaltyAmount(0);

            yield return new WaitForSecondsRealtime(1f);

            //Debug.Log(DebugUtilities.AddTimestampPrefix("Shots fired this round: " + m_playerShotsFired));

            if (m_playerShotsFired > 0)
                yield return UserInterface.StartPenaltyRoutine(m_playerShotsFired);
            else
                yield return new WaitForSeconds(1f);

            if (!success && GameManager.State == GameState.Game && m_playerScore > m_highScore)
            {
                SaveHighScore(m_playerScore);

                yield return UserInterface.StartHighScoreRoutine(m_playerScore, m_highScore);

                SetHighScore(m_playerScore);
            }

            yield return new WaitForSeconds(2f);

            UserInterface.HideRoundEndWidget();
        }

        // TODO: Cache entities instead of outright destroying them
        foreach (Entity e in m_entities.Values)
            Destroy(e.gameObject);

        m_entities.Clear();
        
        if (success)
            StartRoundInternal(m_nextRoundIndex);
        else
            GameManager.SetState(GameState.Demo);
    }

    /// <summary>
    /// Loads the HighScore from PlayerPrefs
    /// </summary>
    /// <returns>a high score, hopefully</returns>
    public static int LoadHighScore()
    {
        return PlayerPrefs.GetInt("HighScore");
    }

    /// <summary>
    /// Saves the HighScore to PlayerPrefs
    /// </summary>
    public static void SaveHighScore(int highScore)
    {
        PlayerPrefs.SetInt("HighScore", highScore);
    }

    public static void SetHighScore(int highScore)
    {
        if (s_instance != null)
            s_instance.m_highScore = highScore;

        UserInterface.SetHighScore(highScore);
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
                float speed = enemy.m_speed * (ScenarioManager.Scenario != null ? ScenarioManager.Scenario.m_globalSpeedMultiplier : 1f);
                p.Initialize(p.ID, startPosition, targetPosition, speed, enemy.m_modelPrefab);
            }
        }

        //DebugUtilities.DrawArrow(source, target, Vector3.back, Color.red, 0, 0.5f);
    }

    public static Entity OnSpawnEntity(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        return s_instance == null ? null : s_instance.OnSpawnEntityInternal(prefab, position, rotation);
    }

    public Entity OnSpawnEntityInternal(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject go = Instantiate<GameObject>(prefab, position, rotation, Environment.EntityRoot);

        Entity entity = go.GetComponent<Entity>();
        if (entity != null)
        {
            entity.Initialize(m_entitiesSpawned);
            m_entities.Add(m_entitiesSpawned, entity);
            m_entitiesSpawned++;

            if (OnEntitySpawnedCallback != null)
                OnEntitySpawnedCallback.Invoke(entity);

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
        if (OnEntityDeathCallback != null)
            OnEntityDeathCallback.Invoke(entity);

        if (giveScore && entity.m_team == Team.Enemy)
            ModifyScore(entity.m_killScore);

        m_entities.Remove(entity.ID);

        if (m_entities.Count == 0 && m_currentRound != null && m_currentRound.IsLastWave)
        {
            // All waves cleared, player won the round
            EndRoundInternal(true);
        }
    }

    public static void OnPlayerHit()
    {
        //if (GameManager.ActivePlayerController != null)
            //GameManager.ActivePlayerController.OnHit();

        EndRound(false);
    }

    public static void StartRound(int index)
    {
        if (s_instance != null)
            s_instance.StartRoundInternal(index);
    }

    public static void EndRound(bool success)
    {
        if (s_instance != null)
            s_instance.EndRoundInternal(success);
    }
}
