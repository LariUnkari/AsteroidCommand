using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public GameObject m_cursorTargetPrefab;
    public GameObject m_projectilePrefab;
    public GameObject m_projectileModelPrefab;

    public SoundEffectPreset m_fireSFX;

    public Transform m_turretTransform;
    public Transform m_muzzleTransform;

    private Vector3 m_targetPosition;
    private GameObject m_cursorTargetObject;

    private int m_shotsFired;

    private void Awake()
    {
        if (m_cursorTargetPrefab != null)
            m_cursorTargetObject = Instantiate<GameObject>(m_cursorTargetPrefab, Vector3.zero, Quaternion.identity);

        m_shotsFired = 0;
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

                GameObject go = Instantiate<GameObject>(m_projectilePrefab, position, rotation, GameManager.EntityRoot);
                go.name = m_projectilePrefab.name + "_" + m_shotsFired;

                FuseProjectile fp = go.GetComponent<FuseProjectile>();
                if (fp != null)
                    fp.Initialize(m_muzzleTransform.position, m_targetPosition, m_projectileModelPrefab);
                else
                    Debug.LogError(DebugUtilities.AddTimestampPrefix("Couldn't find FuseProjectile component in player missile prefab instance!"), go);

                if (m_fireSFX != null)
                    m_fireSFX.PlayAt(m_muzzleTransform.position);

                m_shotsFired++;
            }
        }
    }

    public void Disable()
    {
        // TODO: Give player feedback, sounds and such
        enabled = false;
    }
}
