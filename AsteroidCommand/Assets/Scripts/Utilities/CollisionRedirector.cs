using UnityEngine;
using System.Collections;

public class CollisionRedirector : MonoBehaviour
{
    public MonoBehaviour m_target;

    private void OnTriggerEnter(Collider other)
    {
        m_target.BroadcastMessage("OnTriggerEnter", other);
    }

    private void OnTriggerExit(Collider other)
    {
        m_target.BroadcastMessage("OnTriggerExit", other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        m_target.BroadcastMessage("OnCollisionEnter", collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        m_target.BroadcastMessage("OnCollisionExit", collision);
    }
}
