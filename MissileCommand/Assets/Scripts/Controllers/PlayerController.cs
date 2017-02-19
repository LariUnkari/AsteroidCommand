using UnityEngine;
using System.Collections;

public class PlayerController : TurretController
{
    protected override void Update()
    {
        Ray screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Math3D.VectorPlaneIntersect(screenRay.origin, screenRay.direction, Vector3.zero, Vector3.back, out m_targetPosition);

        SetTarget(m_targetPosition);

        if (m_canFire && Input.GetMouseButtonDown(0))
            m_turret.Fire();
    }
}
