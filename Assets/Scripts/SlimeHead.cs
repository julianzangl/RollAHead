using UnityEngine;
using UnityEngine.InputSystem;

public class SlimeHead : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f;  // Increased from 6f
    [SerializeField] private float groundCheckDistance = 0.4f;
    [SerializeField] private float bounceMultiplier = 1.5f;  // How much bounce force is retained
    [SerializeField] private float maxBounceVelocity = 20f;   // Limit max bounce speed
    [SerializeField] private LayerMask bounceLayers = -1;     // What layers can bounce on

    private Rigidbody rb;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction recallAction;
    private HeadThrow headThrow;

    private bool isActive;
    private bool wasGroundedLastFrame;
    private Vector3 lastVelocityBeforeImpact;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.maxDepenetrationVelocity = 10f;  // Helps with bounce stability
        }

        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        recallAction = new InputAction("Throw", InputActionType.Button, "<Keyboard>/f");
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
            isActive = false;          // stop input immediately
            headThrow.ReturnHead();    // HeadThrow drives fly-back and destroys us
            Debug.Log(recallAction.enabled);
            return;
        }

        bool isGroundedNow = IsGrounded();

        // Jump with coyote time effect (small buffer for jump input)
        if (jumpAction.WasPressedThisFrame() && isGroundedNow)
        {
            // Add upward force for jump
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            // Small boost to ensure jump feels responsive
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, 0f), rb.linearVelocity.z);
        }

        wasGroundedLastFrame = isGroundedNow;
    }

    void FixedUpdate()
    {
        if (!isActive) return;

        Vector2 input = moveAction.ReadValue<Vector2>();
        Transform cam = Camera.main.transform;
        Vector3 camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
        Vector3 moveDir = (camForward * input.y + camRight * input.x).normalized;

        // Set horizontal velocity, preserve vertical (gravity handled by Rigidbody)
        Vector3 horizontal = moveDir * moveSpeed;
        rb.linearVelocity = new Vector3(horizontal.x, rb.linearVelocity.y, horizontal.z);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isActive) return;

        // Check if we should bounce on this object
        if (((1 << collision.gameObject.layer) & bounceLayers) != 0)
        {
            // Get the collision normal (direction of the surface)
            Vector3 collisionNormal = collision.contacts[0].normal;

            // Calculate bounce velocity based on incoming velocity
            Vector3 incomingVelocity = rb.linearVelocity;
            Vector3 reflectedVelocity = Vector3.Reflect(incomingVelocity, collisionNormal);

            // Apply bounce multiplier (reduce energy loss)
            reflectedVelocity *= bounceMultiplier;

            // Limit maximum bounce velocity
            if (reflectedVelocity.magnitude > maxBounceVelocity)
            {
                reflectedVelocity = reflectedVelocity.normalized * maxBounceVelocity;
            }

            // Apply the bounce
            rb.linearVelocity = reflectedVelocity;

            // Add small extra upward boost when bouncing on ground
            if (collisionNormal.y > 0.5f && incomingVelocity.y < -5f)
            {
                rb.AddForce(Vector3.up * jumpForce * 0.5f, ForceMode.Impulse);
            }
        }

        // Handle pushable blocks
        if (collision.collider.TryGetComponent(out PushableBlock pushableBlock))
        {
            Vector3 pushDirection = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            if (pushDirection.sqrMagnitude < 0.01f)
                pushDirection = collision.transform.position - transform.position;

            pushableBlock.Push(pushDirection);
        }
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

    // Optional: Add trail effect for better feedback
    void OnCollisionExit(Collision collision)
    {
        // Could add particles or sound effects here for bounce feedback
    }
}
