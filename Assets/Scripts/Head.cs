using UnityEngine;
using UnityEngine.InputSystem;

public class Head : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float groundCheckDistance = 0.4f;
    [SerializeField] private float groundSpinDamping = 30f;   // how fast the head stops spinning when grounded
    [SerializeField] private float airControl = 6f;           // gentle steering while airborne (does NOT cancel momentum)

    private Rigidbody rb;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction recallAction;
    private HeadThrow headThrow;

    private bool isActive;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity  = true;
        }

        // CharacterController and Rigidbody conflict — disable CC
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        moveAction   = InputSystem.actions.FindAction("Move");
        jumpAction   = InputSystem.actions.FindAction("Jump");
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
            return;
        }

        if (jumpAction.WasPressedThisFrame() && IsGrounded())
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void FixedUpdate()
    {
        if (!isActive) return;

        Vector2 input = moveAction.ReadValue<Vector2>();
        Transform cam = Camera.main.transform;
        Vector3 camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 camRight   = Vector3.ProjectOnPlane(cam.right,   Vector3.up).normalized;
        Vector3 moveDir    = (camForward * input.y + camRight * input.x).normalized;

        if (IsGrounded())
        {
            // On the ground: direct velocity control for precise puzzle movement.
            Vector3 horizontal = moveDir * moveSpeed;
            rb.linearVelocity = new Vector3(horizontal.x, rb.linearVelocity.y, horizontal.z);

            // Kill the thrown tumble once grounded so the head settles instead of spinning forever.
            rb.angularVelocity = Vector3.MoveTowards(rb.angularVelocity, Vector3.zero, groundSpinDamping * Time.fixedDeltaTime);
        }
        else if (moveDir.sqrMagnitude > 0.01f)
        {
            // Airborne: keep the throw/jump momentum, only allow gentle steering. Never zero it out.
            rb.AddForce(moveDir * airControl, ForceMode.Acceleration);
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
}

