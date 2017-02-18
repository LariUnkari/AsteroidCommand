using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager s_instance;

    public ScenarioPreset m_scenario;

    public AudioMixer m_audioMixer;
    public Transform m_audioRoot;

    public Transform m_entityRoot;
    public Transform m_uiRoot;

    private PlayerController m_activePlayerController;

    public static AudioMixer AudioMixer { get { return s_instance != null ? s_instance.m_audioMixer : null; } }
    public static Transform AudioRoot { get { return s_instance != null ? s_instance.m_audioRoot : null; } }

    public static Transform EntityRoot { get { return s_instance != null ? s_instance.m_entityRoot : null; } }
    public static Transform UIRoot { get { return s_instance != null ? s_instance.m_uiRoot : null; } }

    public static PlayerController ActivePlayerController { get { return s_instance != null ? s_instance.m_activePlayerController : null; } }

    private void Awake()
    {
        if (s_instance != null)
        {
            Debug.LogError(DebugUtilities.AddTimestampPrefix("Multiple instances of " + typeof(GameManager) + " detected! Destroying additional instance '" + name + "' in favor of the existing one"), s_instance);
            Destroy(gameObject);
            return;
        }

        s_instance = this;

        if (!UserInterface.IsLoaded)
            SceneManager.LoadScene("UserInterface", LoadSceneMode.Additive);
        else
            Debug.LogWarning("UserInterface scene was already loaded!");
    }

    private void Start()
    {
        Debug.Log(DebugUtilities.AddTimestampPrefix("GameManager.Start()"), this);

        SetVolume(-80f);

        if (m_scenario != null)
        {
            GameObject go = new GameObject("ScenarioManager");
            ScenarioManager sm = go.AddComponent<ScenarioManager>();
            sm.InitializeScenario(m_scenario);
        }
        else
            Debug.LogError(DebugUtilities.AddTimestampPrefix("No Scenario provided!"));
    }

    public static void SetActivePlayerController(PlayerController playerController)
    {
        if (s_instance == null)
            return;

        s_instance.m_activePlayerController = playerController;
    }

    public static void SetVolume(float volume)
    {
        if (s_instance == null)
            return;

        if (s_instance.m_audioMixer != null)
            s_instance.m_audioMixer.SetFloat("VolumeMaster", volume);
    }
}
