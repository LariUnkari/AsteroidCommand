using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour
{
    public static UserInterface s_instance;

    public Text m_scoreAmount;
    public string m_scoreFormat = "{0}";
    public Text m_shotsAmount;
    public string m_shotsFormat = "{0}";

    public GameObject m_roundEndWidget;
    public Text m_penaltyAmount;
    public string m_penaltyFormat = "-{0}";
    public SoundEffectPreset m_penaltyPointSFX;

    //public GameObject[] m_ammoIndicators = new GameObject[10];

    public static bool IsLoaded { get { return s_instance != null; } }

    public static SoundEffectPreset PenaltyPointSFX { get { return s_instance != null ? s_instance.m_penaltyPointSFX : null; } }

    private void Awake()
    {
        if (s_instance != null)
        {
            Debug.LogError(DebugUtilities.AddTimestampPrefix("Multiple instances of " + typeof(UserInterface) + " detected! Destroying additional instance '" + name + "' in favor of the existing one"), s_instance);
            Destroy(gameObject);
            return;
        }

        s_instance = this;

        ShowRoundEndWidget(false);

        Debug.Log(DebugUtilities.AddTimestampPrefix("UserInterface initialized!"), this);
    }

    public static void ShowRoundEndWidget(bool show)
    {
        if (s_instance == null)
            return;

        if (s_instance.m_roundEndWidget != null)
            s_instance.m_roundEndWidget.SetActive(show);
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
