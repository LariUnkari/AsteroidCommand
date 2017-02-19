using UnityEngine;

public class Environment : MonoBehaviour
{
    public static Environment s_instance;

    public Transform m_playerSpawn;

    private Transform m_audioRoot;
    private Transform m_entityRoot;

    public static Transform AudioRoot { get { return s_instance != null ? s_instance.m_audioRoot : null; } }
    public static Transform EntityRoot { get { return s_instance != null ? s_instance.m_entityRoot : null; } }
    public static Transform PlayerSpawn { get { return s_instance != null ? s_instance.m_playerSpawn : null; } }

    private void Awake()
    {
        if (s_instance != null)
        {
            Debug.LogError(DebugUtilities.AddTimestampPrefix("Multiple instances of " + typeof(Environment) + " detected! Destroying additional instance '" + name + "' in favor of the existing one"), s_instance);
            Destroy(gameObject);
            return;
        }

        // Create containers
        m_audioRoot = new GameObject("AudioSources").transform;
        m_entityRoot = new GameObject("Entities").transform;

        s_instance = this;

        Debug.Log(DebugUtilities.AddTimestampPrefix("Environment.Awake()"), this);

        if (!GameManager.IsLoaded)
        {
            GameObject gameManagerPrefab = Resources.Load("GameManager", typeof(GameObject)) as GameObject;
            if (gameManagerPrefab != null)
            {
                Debug.Log(DebugUtilities.AddTimestampPrefix("Loading in GameManager resource..."));
                GameObject go = Instantiate<GameObject>(gameManagerPrefab);
                go.name = gameManagerPrefab.name;
            }
            else
                Debug.LogError(DebugUtilities.AddTimestampPrefix("Unable to load GameManager resource!"));
        }
    }
}
