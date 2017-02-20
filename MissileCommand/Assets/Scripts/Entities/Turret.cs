using UnityEngine;
using System.Collections;
using System;

public class Turret : Entity
{
    public GameObject m_projectilePrefab;
    public GameObject m_projectileModelPrefab;

    public SoundEffectPreset m_fireSFX;

    public float m_projectileSpeed = 30f;

    public Transform m_baseTransform;
    public Transform m_turretTransform;
    public Transform m_muzzleTransform;

    protected Vector3 m_targetPosition;
    protected Vector3 m_targetVector;
    protected Vector3 m_turretBaseForward;

    public void SetTarget(Vector3 targetPosition)
    {
        m_targetPosition = targetPosition;
        //m_targetVector = targetPosition - m_baseTransform.position;
        DebugUtilities.DrawArrow(m_baseTransform.position, targetPosition, Vector3.back, Color.green);

        if (m_baseTransform != null)
        {
            m_targetVector = m_baseTransform.InverseTransformPoint(m_targetPosition);
            m_targetVector.y = 0f;
            //m_turretBaseForward.x = m_targetVector.x;
            //m_turretBaseForward.z = m_targetVector.y;
            //m_turretBaseForward.y = 0f;

            m_baseTransform.LookAt(m_baseTransform.TransformPoint(m_targetVector), transform.up);
            //m_baseTransform.LookAt(m_baseTransform.position + m_turretBaseForward, transform.up);
        }

        // TODO: This is a bit off in some angles, fix it
        if (m_turretTransform != null)
            m_turretTransform.LookAt(targetPosition + m_turretBaseForward, m_turretTransform.parent.up);
    }

    public void Fire()
    {
        if (m_projectilePrefab != null)
        {
            Vector3 position = m_muzzleTransform.position;
            position.z = 0f;

            Quaternion rotation = Quaternion.LookRotation(m_targetPosition - position, Vector3.back);

            Entity entity = ScenarioManager.OnSpawnEntity(m_projectilePrefab, position, rotation);
            if (entity != null)
            {
                entity.name = m_projectilePrefab.name + "_" + ScenarioManager.ShotsFired;

                FuseProjectile fp = entity as FuseProjectile;
                if (fp != null)
                {
                    float speed = m_projectileSpeed * (ScenarioManager.Scenario != null ? ScenarioManager.Scenario.m_globalSpeedMultiplier : 1f);
                    fp.Initialize(fp.ID, m_muzzleTransform.position, m_targetPosition, speed, m_projectileModelPrefab);
                }
                else
                    Debug.LogError(DebugUtilities.AddTimestampPrefix("Couldn't find FuseProjectile component in player missile prefab instance!"), entity);
            }

            if (m_fireSFX != null)
                m_fireSFX.PlayAt(m_muzzleTransform.position, Environment.AudioRoot);

            ScenarioManager.ModifyShotsFired(1);

            //Debug.Log(DebugUtilities.AddTimestampPrefix("Turret fired shot " + ScenarioManager.ShotsFired + "!"), entity);
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        // Do nothing
    }
}
