using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum State { Init, Game };

    public static GameManager s_instance;

    public ScenarioPreset m_scenario;

    public AudioMixer m_audioMixer;

    private State m_state;

    private TurretController m_activeTurretController;

    public static bool IsLoaded { get { return s_instance != null; } }

    public static AudioMixer AudioMixer { get { return s_instance != null ? s_instance.m_audioMixer : null; } }
    public static TurretController ActivePlayerController { get { return s_instance != null ? s_instance.m_activeTurretController : null; } }

    private void Awake()
    {
        if (s_instance != null)
        {
            Debug.LogError(DebugUtilities.AddTimestampPrefix("Multiple instances of " + typeof(GameManager) + " detected! Destroying additional instance '" + name + "' in favor of the existing one"), s_instance);
            Destroy(gameObject);
            return;
        }

        m_state = State.Init;
        s_instance = this;

        Debug.Log(DebugUtilities.AddTimestampPrefix("GameManager.Awake()"), this);
    }

    private void Start()
    {
        Debug.Log(DebugUtilities.AddTimestampPrefix("GameManager.Start()"), this);

        SetVolume(-80f);

        if (!UserInterface.IsLoaded)
        {
            Debug.Log(DebugUtilities.AddTimestampPrefix("Loading in UserInterface scene..."));
            SceneManager.LoadScene("UserInterface", LoadSceneMode.Additive);
        }
        else
        {
            Debug.Log(DebugUtilities.AddTimestampPrefix("UserInterface scene was already loaded!"));
            InitGame();
        }
    }

    private void InitGame()
    {
        if (m_scenario != null)
        {
            if (!ScenarioManager.IsLoaded)
            {
                GameObject go = new GameObject("ScenarioManager");
                go.AddComponent<ScenarioManager>();
            }

            ScenarioManager.InitializeScenario(m_scenario);

            Cursor.visible = false;
            m_state = State.Game;
        }
        else
            Debug.LogError(DebugUtilities.AddTimestampPrefix("No Scenario provided!"));
    }

    private void Update()
    {
        if (m_state == State.Init && UserInterface.IsLoaded)
            InitGame();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TurretController newController = null;
            if (ActivePlayerController is PlayerController)
                newController = ActivePlayerController.GetComponent<AIController>();
            else
                newController = ActivePlayerController.GetComponent<PlayerController>();

            if (newController != null)
                SetActiveTurretController(newController);
        }

        // TODO: Create a simple pause menu for this
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    public static void SetActiveTurretController(TurretController turretController)
    {
        if (s_instance == null || s_instance.m_activeTurretController == turretController)
            return;

        if (s_instance.m_activeTurretController != null)
        {
            s_instance.m_activeTurretController.SetActive(false);

            if (turretController != null)
            {
                turretController.SetCanControl(s_instance.m_activeTurretController.CanControl);
                turretController.SetCanFire(s_instance.m_activeTurretController.CanFire);
            }
        }

        s_instance.m_activeTurretController = turretController;

        if (turretController != null)
            turretController.SetActive(true);
    }

    public static void SetVolume(float volume)
    {
        if (s_instance == null)
            return;

        if (s_instance.m_audioMixer != null)
            s_instance.m_audioMixer.SetFloat("VolumeMaster", volume);
    }
}
