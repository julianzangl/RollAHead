using System.Collections.Generic;
using UnityEngine;

public class PermanentMovingPlatform : MonoBehaviour
{
    [SerializeField] private Vector3 moveOffset = new Vector3(0f, 0f, 32f);
    [SerializeField] private float speed = 2f;
    [SerializeField] private bool loop = true;
    [SerializeField] private float riderCheckHeight = 1.5f;

    private Vector3 startPosition;
    private Vector3 endPosition;
    private Vector3 lastPosition;
    private bool movingForward = true;

    void Awake()
    {
        startPosition = transform.position;
        endPosition = startPosition + moveOffset;
        lastPosition = transform.position;
    }

    void FixedUpdate()
    {
        Vector3 targetPosition = movingForward ? endPosition : startPosition;
        Vector3 nextPosition = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.fixedDeltaTime);

        Vector3 delta = nextPosition - transform.position;

        // Move the platform
        transform.position = nextPosition;

        // Move any riders (characters standing on the platform)
        MoveRiders(delta);

        lastPosition = transform.position;

        if (Vector3.Distance(transform.position, targetPosition) > 0.01f) return;

        if (loop)
            movingForward = !movingForward;
    }

    public void ResetPlatform()
    {
        movingForward = true;
        transform.position = startPosition;
        lastPosition = startPosition;
    }

    private void MoveRiders(Vector3 delta)
    {
        if (delta.sqrMagnitude <= 0f) return;

        // Check for riders above the platform
        Vector3 halfExtents = GetRiderCheckHalfExtents();
        Vector3 center = GetRiderCheckCenter();

        Collider[] hits = Physics.OverlapBox(center, halfExtents, transform.rotation);
        HashSet<Transform> movedObjects = new HashSet<Transform>();

        foreach (Collider hit in hits)
        {
            Transform root = hit.transform.root;
            if (movedObjects.Contains(root)) continue;

            // Try CharacterController first
            CharacterController controller = hit.GetComponent<CharacterController>();
            if (controller == null)
                controller = hit.GetComponentInParent<CharacterController>();

            if (controller != null)
            {
                controller.Move(delta);
                movedObjects.Add(root);
                continue;
            }

            // Try Rigidbody (for SlimeHead)
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb == null)
                rb = hit.GetComponentInParent<Rigidbody>();

            if (rb != null && !rb.isKinematic)
            {
                // Move Rigidbody with platform
                rb.MovePosition(rb.position + delta);
                movedObjects.Add(root);
            }
        }
    }

    private Vector3 GetRiderCheckHalfExtents()
    {
        // Make the check area slightly smaller than the platform
        return new Vector3(
            transform.localScale.x * 0.45f,
            riderCheckHeight * 0.5f,
            transform.localScale.z * 0.45f);
    }

    private Vector3 GetRiderCheckCenter()
    {
        // Check above the platform surface
        return transform.position + Vector3.up * ((transform.localScale.y * 0.5f) + (riderCheckHeight * 0.5f));
    }

    public Vector3 GetVelocity()
    {
        return (transform.position - lastPosition) / Time.fixedDeltaTime;
    }

    // Debug visualization (optional)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 center = GetRiderCheckCenter();
        Vector3 halfExtents = GetRiderCheckHalfExtents();
        Gizmos.DrawWireCube(center, halfExtents * 2);
    }
}