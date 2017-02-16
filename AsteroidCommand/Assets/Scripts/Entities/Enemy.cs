using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public int m_health = 1;
    public float m_speed = 5f;

    private void Update()
    {
        transform.position = transform.position + transform.forward * 0.2f * m_speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Ground")
        {
            Debug.Log(DebugUtilities.AddTimestampPrefix("Enemy '" + name + "' hit Ground!"), this);
            
            // TODO: Make the player lose the game

            Destroy(gameObject);
        }
        else if (other.tag == "Player")
        {
            Debug.Log(DebugUtilities.AddTimestampPrefix("Enemy '" + name + "' hit Player!"), this);

            // TODO: Disable the player turret

            Destroy(gameObject);
        }
        else if (other.tag == "Fire")
        {
            Debug.Log(DebugUtilities.AddTimestampPrefix("Enemy '" + name + "' hit Fire!"), this);

            m_health--;
            if (m_health <= 0)
            {
                // TODO: Give player points for kills

                Destroy(gameObject);
            }
        }
    }
}
