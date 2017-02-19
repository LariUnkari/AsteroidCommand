using UnityEngine;
using System.Collections;

public abstract class TurretController : MonoBehaviour
{
    public GameObject m_cursorTargetPrefab;

    public Turret m_turret;

    protected GameObject m_cursorTargetObject;

    protected bool m_canFire;

    protected Vector3 m_targetPosition;

    void Awake()
    {
        if (m_turret == null)
        {
            enabled = false;
            Debug.LogError(DebugUtilities.AddTimestampPrefix("No Turret for TurretController of type " + GetType() + "!"), this);
        }

        if (m_cursorTargetPrefab != null)
            m_cursorTargetObject = Instantiate<GameObject>(m_cursorTargetPrefab, Vector3.zero, Quaternion.identity);
    }
    
    protected abstract void Update();

    protected void SetTarget(Vector3 targetPosition)
    {
        m_targetPosition = targetPosition;

        m_turret.SetTarget(targetPosition);

        if (m_cursorTargetObject != null)
            m_cursorTargetObject.transform.position = targetPosition;
    }

    public void OnHit()
    {
        // Deduct score if "killed"
        if (enabled)
            ScenarioManager.ModifyScore(-m_turret.m_killScore);

        SetCanControl(false);
    }

    public void SetCanControl(bool canControl)
    {
        // TODO: Check if already disabled
        // TODO: Give player feedback, sounds and such
        enabled = canControl;
    }

    public void SetCanFire(bool canFire)
    {
        m_canFire = canFire;
    }
}
