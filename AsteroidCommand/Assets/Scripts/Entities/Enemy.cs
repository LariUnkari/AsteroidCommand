using UnityEngine;
using System.Collections;

public class Enemy : Projectile
{
    public int m_health = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Ground")
        {
            Debug.Log(DebugUtilities.AddTimestampPrefix("Enemy '" + name + "' hit Ground!"), this);

            // TODO: Make the player lose the game

            OnDetonate();
        }
        else if (other.tag == "Player")
        {
            Debug.Log(DebugUtilities.AddTimestampPrefix("Enemy '" + name + "' hit Player!"), this);

            // TODO: Disable the player turret

            OnDetonate();
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
