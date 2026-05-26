using UnityEngine;
using UnityEngine.InputSystem;

public class Head : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float groundCheckDistance = 0.4f;

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

        // Set horizontal velocity, preserve vertical (gravity handled by Rigidbody)
        Vector3 horizontal = moveDir * moveSpeed;
        rb.linearVelocity = new Vector3(horizontal.x, rb.linearVelocity.y, horizontal.z);
    }
}

