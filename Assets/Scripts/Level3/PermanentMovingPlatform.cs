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

        // Move only SlimeHead riders
        MoveSlimeHeadRiders(delta);

        lastPosition = transform.position;

        if (Vector3.Distance(transform.position, targetPosition) > 0.01f) return;

        if (loop)
            movingForward = !movingForward;
    }

    private void MoveSlimeHeadRiders(Vector3 delta)
    {
        if (delta.sqrMagnitude <= 0f) return;

        // Check for riders above the platform
        Vector3 halfExtents = GetRiderCheckHalfExtents();
        Vector3 center = GetRiderCheckCenter();

        Collider[] hits = Physics.OverlapBox(center, halfExtents, transform.rotation);
        HashSet<Rigidbody> movedRigidbodies = new HashSet<Rigidbody>();

        foreach (Collider hit in hits)
        {
            // Nur SlimeHead erkennen (nach Tag oder Komponente)
            if (!IsSlimeHead(hit)) continue;

            // Rigidbody vom SlimeHead holen
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb == null)
                rb = hit.GetComponentInParent<Rigidbody>();

            if (rb == null || movedRigidbodies.Contains(rb)) continue;

            // SlimeHead mit der Plattform bewegen
            rb.MovePosition(rb.position + delta);
            movedRigidbodies.Add(rb);
        }
    }

    private bool IsSlimeHead(Collider hit)
    {
        // Methode 1: Nach Tag erkennen
        if (hit.CompareTag("SlimeHead")) return true;

        // Methode 2: Nach Komponente erkennen
        if (hit.GetComponent<SlimeHead>() != null) return true;
        if (hit.GetComponentInParent<SlimeHead>() != null) return true;

        // Methode 3: Nach Layer erkennen (falls du einen speziellen Layer verwendest)
        if (hit.gameObject.layer == LayerMask.NameToLayer("SlimeHead")) return true;

        return false;
    }

    private Vector3 GetRiderCheckHalfExtents()
    {
        return new Vector3(
            transform.localScale.x * 0.45f,
            riderCheckHeight * 0.5f,
            transform.localScale.z * 0.45f);
    }

    private Vector3 GetRiderCheckCenter()
    {
        return transform.position + Vector3.up * ((transform.localScale.y * 0.5f) + (riderCheckHeight * 0.5f));
    }

    public Vector3 GetVelocity()
    {
        return (transform.position - lastPosition) / Time.fixedDeltaTime;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 center = GetRiderCheckCenter();
        Vector3 halfExtents = GetRiderCheckHalfExtents();
        Gizmos.DrawWireCube(center, halfExtents * 2);
    }

    public void ResetPlatform()
    {
        movingForward = true;
        transform.position = startPosition;
        lastPosition = startPosition;
    }
}