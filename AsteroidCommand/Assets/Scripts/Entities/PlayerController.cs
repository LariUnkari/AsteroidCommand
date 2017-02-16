using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public GameObject m_cursorTargetPrefab;
    public GameObject m_projectilePrefab;

    public Transform m_muzzleTransform;

    private Vector3 m_targetPosition;
    private GameObject m_cursorTargetObject;

    private void Awake()
    {
        if (m_cursorTargetPrefab != null)
            m_cursorTargetObject = Instantiate<GameObject>(m_cursorTargetPrefab, Vector3.zero, Quaternion.identity);
    }

    private void Update()
    {
        Ray screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Math3D.VectorPlaneIntersect(screenRay.origin, screenRay.direction, Vector3.zero, Vector3.back, out m_targetPosition);

        if (m_cursorTargetObject != null)
            m_cursorTargetObject.transform.position = m_targetPosition;

        if (Input.GetMouseButtonDown(0))
        {
            if (m_projectilePrefab != null)
            {
                Vector3 position = m_muzzleTransform.position;
                Quaternion rotation = Quaternion.LookRotation(m_targetPosition - position, Vector3.back);

                GameObject go = Instantiate<GameObject>(m_projectilePrefab, position, rotation, GameManager.EntityRoot);

                FuseProjectile fp = go.GetComponent<FuseProjectile>();
                if (fp != null)
                    fp.Initialize(m_muzzleTransform.position, m_targetPosition);
                else
                    Debug.LogError(DebugUtilities.AddTimestampPrefix("Couldn't find FuseProjectile component in player missile prefab instance!"), go);
            }
        }
    }
}
