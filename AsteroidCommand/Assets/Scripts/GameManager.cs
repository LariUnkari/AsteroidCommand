using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager s_instance;

    public ScenarioPreset m_scenario;

    private void Awake()
    {
        if (s_instance != null)
        {
            Debug.LogError(DebugUtilities.AddTimestampPrefix("Multiple instances of GameManager detected! Destroying additional instance '" + name + "' in favor of the existing one"), s_instance);
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
            sm.Initialize(m_scenario);
        }
        else
            Debug.LogError(DebugUtilities.AddTimestampPrefix("No Scenario provided!"));
    }
}
