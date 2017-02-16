using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    public float m_speed = 5f;

    public GameObject m_detonateEffectPrefab;

    protected virtual void Update()
    {
        transform.position = transform.position + transform.forward * Time.deltaTime * 0.1f * m_speed;
    }

    protected virtual void OnDetonate()
    {
        if (m_detonateEffectPrefab != null)
            Instantiate<GameObject>(m_detonateEffectPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
