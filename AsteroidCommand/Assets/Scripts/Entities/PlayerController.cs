using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public Vector3 m_targetPosition;
    public GameObject m_cursorTargetPrefab;

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
    }
}
