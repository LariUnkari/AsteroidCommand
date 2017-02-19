using UnityEngine;
using System.Collections;

public abstract class TurretController : MonoBehaviour
{
    public GameObject m_cursorTargetPrefab;

    public Turret m_turret;

    protected GameObject m_cursorTargetObject;

    protected bool m_canControl;
    protected bool m_canFire;

    protected Vector3 m_targetPosition;

    public bool CanControl { get { return m_canControl; } }
    public bool CanFire { get { return m_canFire; } }

    protected virtual void Awake()
    {
        if (m_turret == null)
        {
            enabled = false;
            Debug.LogError(DebugUtilities.AddTimestampPrefix("No Turret for TurretController of type " + GetType() + "!"), this);
        }

        if (m_cursorTargetPrefab != null)
            m_cursorTargetObject = Instantiate<GameObject>(m_cursorTargetPrefab, Vector3.zero, Quaternion.identity);

        ScenarioManager.OnRoundStartedCallback += OnRoundStarted;
        ScenarioManager.OnRoundEndedCallback += OnRoundEnded;
    }
    
    protected abstract void Update();

    protected void SetTarget(Vector3 targetPosition)
    {
        m_targetPosition = targetPosition;

        m_turret.SetTarget(targetPosition);

        if (m_cursorTargetObject != null)
            m_cursorTargetObject.transform.position = targetPosition;
    }

    public virtual void OnRoundStarted(int index)
    {
        // Do nothing
    }

    public virtual void OnRoundEnded(bool success)
    {
        //SetActive(false);
    }

    public virtual void SetActive(bool isActive)
    {
        Debug.Log(DebugUtilities.AddTimestampPrefix(GetType() + ".SetActive(" + isActive + ")"), this);
        enabled = isActive;

        if (m_cursorTargetObject != null)
            m_cursorTargetObject.SetActive(isActive);
    }

    public void SetCanControl(bool canControl)
    {
        // TODO: Check if already disabled
        // TODO: Give player feedback, sounds and such
        m_canControl = canControl;
    }

    public void SetCanFire(bool canFire)
    {
        m_canFire = canFire;
    }

    public void OnHit()
    {
        // Deduct score if "killed"
        if (m_canControl)
            ScenarioManager.ModifyScore(-m_turret.m_killScore);

        SetCanControl(false);
    }

    private void OnDestroy()
    {
        ScenarioManager.OnRoundStartedCallback -= OnRoundStarted;
        ScenarioManager.OnRoundEndedCallback -= OnRoundEnded;
    }
}
