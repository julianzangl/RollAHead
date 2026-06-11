using UnityEngine;

public class RespawnTrigger : MonoBehaviour
{
    void Awake()
    {
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
            triggerCollider.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        CharacterController controller = other.GetComponentInParent<CharacterController>();
        if (controller != null && other.GetComponentInParent<Character>() != null)
        {
            RespawnCharacter(controller);
            return;
        }

        Rigidbody body = other.attachedRigidbody;
        if (body != null && (body.GetComponent<Head>() != null || body.GetComponent<RobotHead>() != null))
            RespawnRigidbody(body);
    }

    private void RespawnCharacter(CharacterController controller)
    {
        controller.enabled = false;
        controller.transform.position = RespawnPoint.CurrentPosition;
        controller.enabled = true;
    }

    private void RespawnRigidbody(Rigidbody body)
    {
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.position = RespawnPoint.CurrentPosition + Vector3.up * 1.5f;
    }
}
