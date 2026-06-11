using UnityEngine;

public class PushableBlock : MonoBehaviour
{
    [SerializeField] private float pushDistance = 0.08f;
    [SerializeField] private float pushCooldown = 0.03f;
    [SerializeField] private float snapDistance = 0.9f;

    private Rigidbody rb;
    private float nextPushTime;
    private bool snapped;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    public void Push(Vector3 worldDirection)
    {
        if (snapped || Time.time < nextPushTime) return;

        Vector3 horizontalDirection = new Vector3(worldDirection.x, 0f, worldDirection.z);
        if (horizontalDirection.sqrMagnitude < 0.01f) return;

        rb.MovePosition(rb.position + horizontalDirection.normalized * pushDistance);
        nextPushTime = Time.time + pushCooldown;

        TrySnap();
    }

    private void TrySnap()
    {
        foreach (BlockSnapPoint snapPoint in BlockSnapPoint.ActivePoints)
        {
            if (!snapPoint.CanSnap(this)) continue;

            Vector3 blockPosition = transform.position;
            Vector3 snapPosition = GetSnapPosition(snapPoint);
            if (Vector3.Distance(blockPosition, snapPosition) > snapDistance) continue;

            snapped = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = snapPosition;
            rb.isKinematic = true;
            snapPoint.Occupy(this);
            return;
        }
    }

    private Vector3 GetSnapPosition(BlockSnapPoint snapPoint)
    {
        if (snapPoint.name.Contains("01"))
            return new Vector3(0f, -0.5f, 28f);

        if (snapPoint.name.Contains("02"))
            return new Vector3(0f, -0.5f, 30f);

        if (snapPoint.name.Contains("03"))
            return new Vector3(0f, -0.5f, 32f);

        return snapPoint.transform.position;
    }
}
