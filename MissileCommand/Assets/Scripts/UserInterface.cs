using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour
{
    public static UserInterface s_instance;

    public Text m_scoreAmount;
    public string m_scoreFormat = "{0}";
    public Text m_shotsAmount;
    public string m_shotsFormat = "{0}";

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

    public Text m_penaltyAmount;
    public string m_penaltyFormat = "-{0}";
    public SoundEffectPreset m_penaltyPointSFX;
    public float m_penaltyPointInterval = 0.3f;
    public float m_penaltySpeedUpTimeLimit = 3f;
    public float m_penaltyPointIntervalMin = 0.1f;

    //public GameObject[] m_ammoIndicators = new GameObject[10];

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

        HideRoundStartWidget();
        HideRoundEndWidget();

        Debug.Log(DebugUtilities.AddTimestampPrefix("UserInterface.Awake()"), this);
    }

    public static void ShowRoundStartWidget(int roundIndex)
    {
        if (s_instance == null)
            return;

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

        return s_instance.StartCoroutine(s_instance.AlertRoutine());
    }

    public System.Collections.IEnumerator AlertRoutine()
    {
        if (UserInterface.RoundStartSFX != null)
        {
            int repeats = m_alertSoundCount;
            while (repeats-- > 0)
            {
                UserInterface.RoundStartSFX.PlayAt(Camera.main.transform.position, Environment.AudioRoot);

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

        return s_instance.StartCoroutine(s_instance.PenaltyRoutine(playerShotsFired));
    }

    public System.Collections.IEnumerator PenaltyRoutine(int playerShotsFired)
    {
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
                m_penaltyPointSFX.PlayAt(Camera.main.transform.position, Environment.AudioRoot);

            yield return new WaitForSecondsRealtime(penaltyCycleInterval);
        }

        yield return new WaitForSecondsRealtime(2f);
    }

    public static void HideRoundEndWidget()
    {
        if (s_instance != null)
            s_instance.m_roundEndWidget.SetActive(false);
    }

    public static void SetScore(int amount)
    {
        if (s_instance != null)
            s_instance.m_scoreAmount.text = string.Format(s_instance.m_scoreFormat, amount);
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
