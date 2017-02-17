using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager s_instance;

    public ScenarioPreset m_scenario;
    
    public Transform m_entityRoot;
    public Transform m_uiRoot;

    public static Transform EntityRoot { get { return s_instance != null ? s_instance.m_entityRoot : null; } }
    public static Transform UIRoot { get { return s_instance != null ? s_instance.m_uiRoot : null; } }

    private void Awake()
    {
        if (s_instance != null)
        {
            Debug.LogError(DebugUtilities.AddTimestampPrefix("Multiple instances of " + typeof(GameManager) + " detected! Destroying additional instance '" + name + "' in favor of the existing one"), s_instance);
            Destroy(gameObject);
            return;
        }

        s_instance = this;
    }

    private void Start()
    {
        Debug.Log(DebugUtilities.AddTimestampPrefix("GameManager.Start()"), this);

        if (m_scenario != null)
        {
            GameObject go = new GameObject("ScenarioManager");
            ScenarioManager sm = go.AddComponent<ScenarioManager>();
            sm.InitializeScenario(m_scenario);
        }
        else
            Debug.LogError(DebugUtilities.AddTimestampPrefix("No Scenario provided!"));
    }
}
