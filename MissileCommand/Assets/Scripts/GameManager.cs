using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public enum GameState { Init, Demo, Game };

public class GameManager : MonoBehaviour
{
    public static GameManager s_instance;

    public ScenarioPreset m_scenario;

    public AudioMixer m_audioMixer;

    private GameState m_state;

    private TurretController m_activeTurretController;

    public static bool IsLoaded { get { return s_instance != null; } }

    public static AudioMixer AudioMixer { get { return s_instance != null ? s_instance.m_audioMixer : null; } }
    public static TurretController ActivePlayerController { get { return s_instance != null ? s_instance.m_activeTurretController : null; } }

    public static GameState State { get { return s_instance != null ? s_instance.m_state : GameState.Init; } }

    private void Awake()
    {
        if (s_instance != null)
        {
            Debug.LogError(DebugUtilities.AddTimestampPrefix("Multiple instances of " + typeof(GameManager) + " detected! Destroying additional instance '" + name + "' in favor of the existing one"), s_instance);
            Destroy(gameObject);
            return;
        }

        Cursor.visible = false;

        m_state = GameState.Init;
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
            SetState(GameState.Demo);
        }
        else
            Debug.LogError(DebugUtilities.AddTimestampPrefix("No Scenario provided!"));
    }

    private void Update()
    {
        if (m_state == GameState.Init && UserInterface.IsLoaded)
            InitGame();

        if (m_state == GameState.Demo)
        {
            if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
            {
                SetState(GameState.Game);
            }
        }
        else if (m_state == GameState.Game)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TurretController newController = null;
                if (ActivePlayerController is PlayerController)
                    newController = ActivePlayerController.GetComponent<AIController>();
                else
                    newController = ActivePlayerController.GetComponent<PlayerController>();

                if (newController != null)
                    SetActiveTurretController(newController, true);
            }
        }

        // TODO: Create a simple pause menu for this
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    public static void SetState(GameState state)
    {
        if (s_instance != null)
            s_instance.SetStateInternal(state);
    }

    public void SetStateInternal(GameState state)
    {
        if (m_state == state)
            return;

        Debug.Log(DebugUtilities.AddTimestampPrefix("Game state changing from " + m_state + " to " + state));

        m_state = state;

        if (state == GameState.Demo)
        {
            SetVolume(-80f);
            ScenarioManager.StartRound(0);
            UserInterface.StartDemoRoutine();
        }
        if (state == GameState.Game)
        {
            SetVolume(0f);
            ScenarioManager.InitializeScenario(m_scenario);
            ScenarioManager.StartRound(0);
        }
    }

    public static void SetActiveTurretController(TurretController turretController, bool inheritControlStates)
    {
        if (s_instance == null || s_instance.m_activeTurretController == turretController)
            return;
        
        if (s_instance.m_activeTurretController != null)
        {
            if (inheritControlStates && turretController != null)
            {
                turretController.SetCanControl(s_instance.m_activeTurretController.CanControl);
                turretController.SetCanFire(s_instance.m_activeTurretController.CanFire);
            }
            
            foreach (TurretController tc in s_instance.m_activeTurretController.GetComponents<TurretController>())
                tc.SetActive(false);
        }

        s_instance.m_activeTurretController = turretController;

        if (turretController != null)
        {
            foreach (TurretController tc in turretController.gameObject.GetComponents<TurretController>())
                tc.SetActive(tc == turretController);
        }
    }

    public static void SetVolume(float volume)
    {
        if (s_instance == null)
            return;

        if (s_instance.m_audioMixer != null)
        {
            Debug.Log(DebugUtilities.AddTimestampPrefix("Master volume being set to " + volume.ToString("F2") + "dB"));
            s_instance.m_audioMixer.SetFloat("VolumeMaster", volume);
        }
    }
}
