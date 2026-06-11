using UnityEngine;
using UnityEngine.InputSystem;

public class RobotHead : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float groundCheckDistance = 0.4f;
    [SerializeField] private float climbSpeed = 3f;
    [SerializeField] private float wallCheckDistance = 0.5f;
    [SerializeField] private float wallCheckRadius = 0.12f;
    [SerializeField] private float wallStickSpeed = 1.5f;
    [SerializeField] private LayerMask climbableLayers = ~0;

    private Rigidbody rb;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction climbAction;
    private InputAction recallAction;
    private HeadThrow headThrow;

    private bool ownsClimbAction;
    private bool isActive;
    private bool isClimbing;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        climbAction = InputSystem.actions.FindAction("Attack");

        if (climbAction == null)
        {
            climbAction = new InputAction("RobotClimb", InputActionType.Button, "<Mouse>/leftButton");
            climbAction.Enable();
            ownsClimbAction = true;
        }

        recallAction = new InputAction("Recall", InputActionType.Button, "<Keyboard>/f");
        recallAction.Enable();
    }

    public void Initialize(Vector3 throwDirection, float throwForce, HeadThrow headThrow)
    {
        this.headThrow = headThrow;
        isActive = true;
        rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
    }

    void OnDestroy()
    {
        if (ownsClimbAction)
        {
            climbAction.Disable();
            climbAction.Dispose();
        }

        recallAction.Disable();
        recallAction.Dispose();
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);
    }

    void Update()
    {
        if (!isActive) return;

        if (recallAction.WasPressedThisFrame())
        {
            isActive = false;
            StopClimbing();
            headThrow.ReturnHead();
            return;
        }

        if (jumpAction.WasPressedThisFrame() && IsGrounded())
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void FixedUpdate()
    {
        if (!isActive) return;

        Vector2 input = moveAction.ReadValue<Vector2>();
        bool wantsToClimb = climbAction != null && climbAction.IsPressed();

        if (wantsToClimb && TryFindWall(out RaycastHit wallHit))
        {
            StartClimbing();
            rb.linearVelocity = GetClimbVelocity(input, wallHit.normal);
            return;
        }

        StopClimbing();

        Vector3 moveDir = GetCameraRelativeMove(input);
        Vector3 horizontal = moveDir * moveSpeed;
        rb.linearVelocity = new Vector3(horizontal.x, rb.linearVelocity.y, horizontal.z);
    }

    void StartClimbing()
    {
        isClimbing = true;
        rb.useGravity = false;
    }

    void StopClimbing()
    {
        if (!isClimbing) return;

        isClimbing = false;
        if (rb != null)
            rb.useGravity = true;
    }

    Vector3 GetCameraRelativeMove(Vector2 input)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
            return Vector3.zero;

        Transform cam = mainCamera.transform;
        Vector3 camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
        return (camForward * input.y + camRight * input.x).normalized;
    }

    Vector3 GetClimbVelocity(Vector2 input, Vector3 wallNormal)
    {
        Vector3 wallRight = Vector3.Cross(Vector3.up, wallNormal).normalized;

        Camera mainCamera = Camera.main;
        if (mainCamera != null && Vector3.Dot(wallRight, mainCamera.transform.right) < 0f)
            wallRight = -wallRight;

        Vector3 climbDirection = Vector3.up * input.y + wallRight * input.x;
        if (climbDirection.sqrMagnitude > 1f)
            climbDirection.Normalize();

        return climbDirection * climbSpeed - wallNormal * wallStickSpeed;
    }

    bool TryFindWall(out RaycastHit bestHit)
    {
        bestHit = default;
        float bestDistance = float.MaxValue;

        Vector3[] directions =
        {
            Vector3.forward,
            Vector3.back,
            Vector3.right,
            Vector3.left,
            new Vector3(1f, 0f, 1f).normalized,
            new Vector3(-1f, 0f, 1f).normalized,
            new Vector3(1f, 0f, -1f).normalized,
            new Vector3(-1f, 0f, -1f).normalized
        };

        for (int i = 0; i < directions.Length; i++)
        {
            if (!Physics.SphereCast(
                    transform.position,
                    wallCheckRadius,
                    directions[i],
                    out RaycastHit hit,
                    wallCheckDistance,
                    climbableLayers,
                    QueryTriggerInteraction.Ignore))
                continue;

            if (Mathf.Abs(hit.normal.y) > 0.2f)
                continue;

            if (hit.distance >= bestDistance)
                continue;

            bestDistance = hit.distance;
            bestHit = hit;
        }

        return bestDistance < float.MaxValue;
    }

    void OnCollisionStay(Collision collision)
    {
        if (!isActive) return;

        if (!collision.collider.TryGetComponent(out PushableBlock pushableBlock)) return;

        Vector3 pushDirection = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (pushDirection.sqrMagnitude < 0.01f)
            pushDirection = collision.transform.position - transform.position;

        pushableBlock.Push(pushDirection);
    }
}
