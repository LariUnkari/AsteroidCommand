using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public GameObject m_cursorTargetPrefab;
    public GameObject m_projectilePrefab;
    public GameObject m_projectileModelPrefab;

    public SoundEffectPreset m_fireSFX;

    public float m_projectileSpeed = 5f;

    public Transform m_turretTransform;
    public Transform m_muzzleTransform;

    private Vector3 m_targetPosition;
    private GameObject m_cursorTargetObject;

    private void Awake()
    {
        GameManager.SetActivePlayerController(this);

        if (m_cursorTargetPrefab != null)
            m_cursorTargetObject = Instantiate<GameObject>(m_cursorTargetPrefab, Vector3.zero, Quaternion.identity);
    }

    private void Update()
    {
        Ray screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Math3D.VectorPlaneIntersect(screenRay.origin, screenRay.direction, Vector3.zero, Vector3.back, out m_targetPosition);

        if (m_turretTransform != null)
            m_turretTransform.LookAt(m_targetPosition, m_turretTransform.parent.up);

        if (m_cursorTargetObject != null)
            m_cursorTargetObject.transform.position = m_targetPosition;

        if (Input.GetMouseButtonDown(0))
        {
            if (m_projectilePrefab != null)
            {
                Vector3 position = m_muzzleTransform.position;
                Quaternion rotation = Quaternion.LookRotation(m_targetPosition - position, Vector3.back);

                Entity pe = ScenarioManager.OnSpawnEntity(m_projectilePrefab, position, rotation);
                if (pe != null)
                {
                    pe.name = m_projectilePrefab.name + "_" + ScenarioManager.ShotsFired;

                    FuseProjectile fp = pe as FuseProjectile;
                    if (fp != null)
                        fp.Initialize(fp.ID, m_muzzleTransform.position, m_targetPosition, m_projectileSpeed, m_projectileModelPrefab);
                    else
                        Debug.LogError(DebugUtilities.AddTimestampPrefix("Couldn't find FuseProjectile component in player missile prefab instance!"), pe);
                }

                if (m_fireSFX != null)
                    m_fireSFX.PlayAt(m_muzzleTransform.position, GameManager.AudioRoot);
                
                ScenarioManager.ModifyShotsFired(1);
            }
        }
    }

    public void SetActive(bool active)
    {
        // TODO: Check if already disabled
        // TODO: Give player feedback, sounds and such
        enabled = active;
    }
}
