using System.Collections.Generic;
using UnityEngine;

public class SimpleMovingPlatform : MonoBehaviour
{
    [SerializeField] private Vector3 moveOffset = new Vector3(0f, 0f, 32f);
    [SerializeField] private float speed = 2f;
    [SerializeField] private bool loop = true;
    [SerializeField] private float riderCheckHeight = 1.5f;
    [SerializeField] private float startDelay = 1.5f;

    private Vector3 startPosition;
    private Vector3 endPosition;
    private Vector3 lastPosition;
    private bool movingForward = true;
    private bool moving;
    private bool startDelayRunning;
    private float startTimer;

    void Awake()
    {
        startPosition = transform.position;
        endPosition = startPosition + moveOffset;
        lastPosition = transform.position;
    }

    void FixedUpdate()
    {
        if (!moving)
        {
            UpdateDelayedStart();
            return;
        }

        Vector3 targetPosition = movingForward ? endPosition : startPosition;
        Vector3 nextPosition = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.fixedDeltaTime);

        Vector3 delta = nextPosition - transform.position;
        transform.position = nextPosition;
        MoveRiders(delta);
        lastPosition = transform.position;

        if (Vector3.Distance(transform.position, targetPosition) > 0.01f) return;

        if (loop)
            movingForward = !movingForward;
    }

    public void ResetPlatform()
    {
        moving = false;
        startDelayRunning = false;
        startTimer = 0f;
        movingForward = true;
        transform.position = startPosition;
        lastPosition = startPosition;
    }

    private void UpdateDelayedStart()
    {
        if (!startDelayRunning)
        {
            if (!HasZombieRider()) return;

            startDelayRunning = true;
            startTimer = 0f;
        }

        startTimer += Time.fixedDeltaTime;
        if (startTimer >= startDelay)
            moving = true;
    }

    private bool HasZombieRider()
    {
        Vector3 halfExtents = GetRiderCheckHalfExtents();
        Vector3 center = GetRiderCheckCenter();

        Collider[] hits = Physics.OverlapBox(center, halfExtents, transform.rotation);
        foreach (Collider hit in hits)
        {
            if (hit.GetComponentInParent<Character>() != null)
                return true;
        }

        return false;
    }

    private void MoveRiders(Vector3 delta)
    {
        if (delta.sqrMagnitude <= 0f) return;

        Vector3 halfExtents = GetRiderCheckHalfExtents();
        Vector3 center = GetRiderCheckCenter();

        Collider[] hits = Physics.OverlapBox(center, halfExtents, transform.rotation);
        HashSet<CharacterController> movedControllers = new HashSet<CharacterController>();

        foreach (Collider hit in hits)
        {
            CharacterController controller = hit.GetComponentInParent<CharacterController>();
            if (controller == null || movedControllers.Contains(controller)) continue;

            controller.Move(delta);
            movedControllers.Add(controller);
        }
    }

    private Vector3 GetRiderCheckHalfExtents()
    {
        return new Vector3(
            transform.localScale.x * 0.5f,
            riderCheckHeight * 0.5f,
            transform.localScale.z * 0.5f);
    }

    private Vector3 GetRiderCheckCenter()
    {
        return transform.position + Vector3.up * ((transform.localScale.y * 0.5f) + (riderCheckHeight * 0.5f));
    }
}
