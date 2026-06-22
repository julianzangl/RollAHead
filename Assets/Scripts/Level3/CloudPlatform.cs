using System.Collections.Generic;
using UnityEngine;

public class CloudPlatform : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<SlimeHead>() == null)
        {
            // Make platform pass-through for non-SlimeHead objects
            Physics.IgnoreCollision(GetComponent<Collider>(), collision.collider, true);
        }
    }
}