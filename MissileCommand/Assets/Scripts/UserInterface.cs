using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UserInterface : MonoBehaviour
{
    public static UserInterface s_instance;

    public Text m_scoreAmount;
    public string m_scoreFormat = "{0}";
    public Text m_highScoreAmount;
    public string m_highScoreFormat = "{0}";
    public Text m_shotsAmount;
    public string m_shotsFormat = "{0}";

    public GameObject m_demoPromptWidget;
    public float m_demoPromptFlashInterval = 1f;

    public GameObject m_roundStartWidget;
    public Text m_roundIndex;
    public string m_roundIndexFormat = "{0}";

    public SoundEffectPreset m_roundStartSFX;
    public float m_roundStartTimeLimit = 2f;
    public float m_alertInterval = 0.3f;
    public int m_alertSoundCount = 4;

    public GameObject m_roundEndWidget;
    public Text m_roundEndMessage;
    public string m_endSuccess = "WELL DONE";
    public string m_endDefeat = "GAME OVER";
    public string m_endHighScore = "NEW HIGHSCORE";

    public Text m_penaltyAmount;
    public string m_penaltyFormat = "-{0}";
    public SoundEffectPreset m_penaltyPointSFX;
    public float m_penaltyPointInterval = 0.3f;
    public float m_penaltySpeedUpTimeLimit = 3f;
    public float m_penaltyPointIntervalMin = 0.1f;

    public SoundEffectPreset m_highScorePointSFX;
    public float m_highScoreChangeTime = 1f;
    public float m_highScoreChangeInterval = 0.3f;

    //public GameObject[] m_ammoIndicators = new GameObject[10];

    private IEnumerator m_roundRoutineIEnumerator;

    private AudioSource m_uiAudio;

    public static bool IsLoaded { get { return s_instance != null; } }

    public static Transform Root { get { return s_instance != null ? s_instance.transform : null; } }

    public static SoundEffectPreset RoundStartSFX { get { return s_instance != null ? s_instance.m_roundStartSFX : null; } }

    private void Awake()
    {
        if (s_instance != null)
        {
            Debug.LogError(DebugUtilities.AddTimestampPrefix("Multiple instances of " + typeof(UserInterface) + " detected! Destroying additional instance '" + name + "' in favor of the existing one"), s_instance);
            Destroy(gameObject);
            return;
        }

        s_instance = this;

        m_demoPromptWidget.SetActive(false);

        HideRoundStartWidget();
        HideRoundEndWidget();

        GameObject go = new GameObject("AudioSource_UserInterface");
        go.transform.position = Camera.main.transform.position;
        go.transform.parent = Environment.AudioRoot;
        m_uiAudio = go.AddComponent<AudioSource>();

        Debug.Log(DebugUtilities.AddTimestampPrefix("UserInterface.Awake()"), this);
    }

    /// <summary>
    /// Only one round routine can be active at any given time, this makes sure that holds true
    /// </summary>
    /// <param name="routine">IEnumerator to run</param>
    /// <returns>Started Coroutine</returns>
    private Coroutine StartRoundRoutine(System.Collections.IEnumerator routine)
    {
        StopRoundRoutine();

        m_roundRoutineIEnumerator = routine;
        return StartCoroutine(routine);
    }

    private void StopRoundRoutine()
    {
        if (m_roundRoutineIEnumerator != null)
        {
            Debug.LogWarning(DebugUtilities.AddTimestampPrefix("UserInterface stopping previous round routine!"));
            StopCoroutine(m_roundRoutineIEnumerator);
        }
    }

    public static void StopRoundRoutines()
    {
        if (s_instance != null)
        {
            s_instance.StopRoundRoutine();
            s_instance.m_uiAudio.Stop();
        }
    }

    public static Coroutine StartDemoRoutine()
    {
        if (s_instance == null)
            return null;
        
        return s_instance.StartCoroutine(s_instance.DemoRoutine());
    }

    public IEnumerator DemoRoutine()
    {
        m_demoPromptWidget.SetActive(false);

        float t = m_demoPromptFlashInterval;
        while (GameManager.State == GameState.Demo)
        {
            if (t < 0f)
            {
                t = m_demoPromptFlashInterval;
                m_demoPromptWidget.SetActive(!m_demoPromptWidget.activeSelf);
            }

            yield return null;

            t -= Time.deltaTime;
        }

        m_demoPromptWidget.SetActive(false);
    }

    public static void ShowRoundStartWidget(int roundIndex)
    {
        if (s_instance == null)
            return;

        UserInterface.HideRoundEndWidget();

        if (s_instance.m_roundStartWidget != null)
        {
            s_instance.m_roundIndex.text = string.Format(s_instance.m_roundIndexFormat, roundIndex + 1);
            s_instance.m_roundStartWidget.SetActive(true);
        }
    }

    public static Coroutine StartAlertRoutine()
    {
        if (s_instance == null)
            return null;
        
        return s_instance.StartRoundRoutine(s_instance.AlertRoutine());
    }

    public IEnumerator AlertRoutine()
    {
        if (m_roundStartSFX != null)
        {
            int repeats = m_alertSoundCount;
            
            while (repeats-- > 0)
            {
                Debug.Log(DebugUtilities.AddTimestampPrefix("Beep!"));
                m_roundStartSFX.PlayOnSource(m_uiAudio);
                yield return new WaitForSecondsRealtime(m_alertInterval);
            }
            
            yield return new WaitForSecondsRealtime(m_roundStartTimeLimit - m_alertSoundCount * m_alertInterval);
        }
        else
            yield return new WaitForSecondsRealtime(m_roundStartTimeLimit);
    }

    public static void HideRoundStartWidget()
    {
        if (s_instance != null)
            s_instance.m_roundStartWidget.SetActive(false);
    }

    public static void ShowRoundEndWidget(bool success)
    {
        if (s_instance == null)
            return;

        UserInterface.HideRoundStartWidget();

        if (s_instance.m_roundEndWidget != null)
        {
            s_instance.m_roundEndMessage.text = success ? s_instance.m_endSuccess : s_instance.m_endDefeat;
            s_instance.m_roundEndWidget.SetActive(true);
        }
    }

    public static Coroutine StartPenaltyRoutine(int playerShotsFired)
    {
        if (s_instance == null)
            return null;

        return s_instance.StartRoundRoutine(s_instance.PenaltyRoutine(playerShotsFired));
    }

    public IEnumerator PenaltyRoutine(int playerShotsFired)
    {
        // It's fine if a few penalty points take a "long" time to process, but it needs to be fast when there's a lot of them
        float penaltyCycleInterval = m_penaltyPointInterval;
        if (playerShotsFired * m_penaltyPointInterval > m_penaltySpeedUpTimeLimit)
            penaltyCycleInterval = Mathf.Clamp(m_penaltySpeedUpTimeLimit / playerShotsFired, m_penaltyPointIntervalMin, m_penaltyPointInterval);
        
        //Debug.Log(DebugUtilities.AddTimestampPrefix("Shots penalty being applied! Cycle interval: " + penaltyCycleInterval));

        int penaltyAmount = 0;
        while (playerShotsFired > 0)
        {
            playerShotsFired--;
            ScenarioManager.SetShotsFired(playerShotsFired);
            ScenarioManager.ModifyScore(-5);

            penaltyAmount += 5;
            SetPenaltyAmount(-penaltyAmount);

            if (m_penaltyPointSFX != null)
            {
                Debug.Log(DebugUtilities.AddTimestampPrefix("Chuck!"));
                m_penaltyPointSFX.PlayOnSource(m_uiAudio);
            }

            yield return new WaitForSecondsRealtime(penaltyCycleInterval);
        }
    }

    public static void HideRoundEndWidget()
    {
        if (s_instance != null)
            s_instance.m_roundEndWidget.SetActive(false);
    }

    public static Coroutine StartHighScoreRoutine(int oldScore, int newScore)
    {
        if (s_instance == null)
            return null;

        return s_instance.StartRoundRoutine(s_instance.HighScoreRoutine(oldScore, newScore));
    }

    public IEnumerator HighScoreRoutine(int oldScore, int newScore)
    {
        m_roundEndMessage.text = m_endHighScore;

        yield return new WaitForSeconds(1f);

        float t = Time.time + m_highScoreChangeTime;
        while (Time.time < t)
        {
            SetHighScore(Mathf.FloorToInt(Mathf.Lerp(oldScore, newScore, (t - Time.time) / m_highScoreChangeTime)));

            if (m_highScorePointSFX != null)
                m_highScorePointSFX.PlayOnSource(m_uiAudio);

            yield return new WaitForSeconds(m_highScoreChangeInterval);
        }
    }

    public static void SetScore(int amount)
    {
        if (s_instance != null)
            s_instance.m_scoreAmount.text = string.Format(s_instance.m_scoreFormat, amount);
    }

    public static void SetHighScore(int amount)
    {
        if (s_instance != null)
            s_instance.m_highScoreAmount.text = string.Format(s_instance.m_highScoreFormat, amount);
    }

    public static void SetShotsFired(int amount)
    {
        if (s_instance != null)
            s_instance.m_shotsAmount.text = string.Format(s_instance.m_shotsFormat, amount);
    }

    public static void SetPenaltyAmount(int amount)
    {
        if (s_instance != null)
            s_instance.m_penaltyAmount.text = string.Format(s_instance.m_penaltyFormat, amount);
    }
}
