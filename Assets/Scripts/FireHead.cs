using UnityEngine;
using UnityEngine.InputSystem;

// Fire head: rolls like the normal head but burns away any Burnable object it touches,
// opening up paths blocked by burnable obstacles.
public class FireHead : MonoBehaviour, IThrowableHead
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float groundCheckDistance = 0.4f;
    [SerializeField] private float groundSpinDamping = 30f;
    [SerializeField] private float airControl = 10f;

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
            rb.useGravity = true;
        }

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
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
            isActive = false;
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
        Transform cam = Camera.main.transform;
        Vector3 camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
        Vector3 moveDir = (camForward * input.y + camRight * input.x).normalized;

        if (IsGrounded())
        {
            Vector3 horizontal = moveDir * moveSpeed;
            rb.linearVelocity = new Vector3(horizontal.x, rb.linearVelocity.y, horizontal.z);
            rb.angularVelocity = Vector3.MoveTowards(rb.angularVelocity, Vector3.zero, groundSpinDamping * Time.fixedDeltaTime);
        }
        else if (moveDir.sqrMagnitude > 0.01f)
        {
            rb.AddForce(moveDir * airControl, ForceMode.Acceleration);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isActive) return;

        TryBurn(collision.collider);

        if (collision.collider.TryGetComponent(out PushableBlock pushableBlock))
        {
            Vector3 pushDirection = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            if (pushDirection.sqrMagnitude > 0.01f)
                pushableBlock.Push(pushDirection);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;
        TryBurn(other);
    }

    private void TryBurn(Collider other)
    {
        Burnable burnable = other.GetComponentInParent<Burnable>();
        if (burnable != null)
            burnable.Burn();
    }
}
