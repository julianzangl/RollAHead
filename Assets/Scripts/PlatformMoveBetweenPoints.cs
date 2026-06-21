using UnityEngine;

[DisallowMultipleComponent]
public class PlatformMoveBetweenPoints : MonoBehaviour
{
    [Header("World Coordinates")]
    [SerializeField] private Vector3 pointA;
    [SerializeField] private Vector3 pointB = new Vector3(0f, 0f, 5f);

    [Header("Movement Settings")]
    [SerializeField, Min(0.01f)] private float speed = 2f;
    [SerializeField, Min(0f)] private float pauseAtPoints;
    [SerializeField] private bool startAtPointA = true;

    private Rigidbody platformBody;
    private Vector3 positionA;
    private Vector3 positionB;
    private bool movingToPointB;
    private float pauseTimer;
    private bool initialized;

    void Reset()
    {
        pointA = transform.position;
        pointB = transform.position + Vector3.forward * 5f;
    }

    void Awake()
    {
        platformBody = GetComponent<Rigidbody>();

        if (platformBody != null)
        {
            platformBody.isKinematic = true;
            platformBody.useGravity = false;
            platformBody.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    void Start()
    {
        positionA = pointA;
        positionB = pointB;
        movingToPointB = startAtPointA;

        SetPosition(startAtPointA ? positionA : positionB);
        initialized = true;
    }

    void FixedUpdate()
    {
        if (!initialized) return;

        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.fixedDeltaTime;
            return;
        }

        Vector3 target = movingToPointB ? positionB : positionA;
        Vector3 nextPosition = Vector3.MoveTowards(
            CurrentPosition(),
            target,
            speed * Time.fixedDeltaTime);

        SetPosition(nextPosition);

        if (Vector3.SqrMagnitude(nextPosition - target) > 0.0001f) return;

        movingToPointB = !movingToPointB;
        pauseTimer = pauseAtPoints;
    }

    private Vector3 CurrentPosition()
    {
        return platformBody != null ? platformBody.position : transform.position;
    }

    private void SetPosition(Vector3 position)
    {
        if (platformBody != null)
            platformBody.MovePosition(position);
        else
            transform.position = position;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(pointA, pointB);
        Gizmos.DrawWireSphere(pointA, 0.25f);
        Gizmos.DrawWireSphere(pointB, 0.25f);
    }
}
