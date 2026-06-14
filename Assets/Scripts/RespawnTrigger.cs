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
            ResetMovingPlatforms();
            return;
        }

        Rigidbody body = other.attachedRigidbody;
        if (body != null && (body.GetComponent<Head>() != null || body.GetComponent<RobotHead>() != null))
        {
            RespawnRigidbody(body);
            ResetMovingPlatforms();
        }
    }

    private void RespawnCharacter(CharacterController controller)
    {
        Character character = controller.GetComponent<Character>();
        if (character != null)
            character.ResetVerticalVelocity();

        controller.enabled = false;
        controller.transform.position = RespawnPoint.CurrentPosition;
        controller.enabled = true;

        if (character != null)
            character.ResetVerticalVelocity();
    }

    private void RespawnRigidbody(Rigidbody body)
    {
        body.useGravity = true;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.position = RespawnPoint.CurrentPosition + Vector3.up * 1.5f;
    }

    private void ResetMovingPlatforms()
    {
        SimpleMovingPlatform[] platforms = Object.FindObjectsByType<SimpleMovingPlatform>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (SimpleMovingPlatform platform in platforms)
            platform.ResetPlatform();
    }
}
