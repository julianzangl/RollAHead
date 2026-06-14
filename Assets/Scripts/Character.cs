using UnityEngine;
using UnityEngine.InputSystem;

public class Character : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpForce = 5f;


    private Animator animator;
    private CharacterController controller;
    private InputAction moveAction;
    private InputAction jumpAction;
    private float verticalVelocity;

    private bool isWalking;

    // When aiming a throw, HeadThrow owns the character's facing, so we skip self-rotation
    // to avoid the two fighting over transform.rotation in the same frame.
    public bool SuppressRotation { get; set; }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        animator = GetComponent<Animator>();

        // Rigidbody + CharacterController on the same object fight each other.
        // Set Rigidbody to kinematic so CharacterController owns the movement.
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    void Update()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();

        // Camera-relative horizontal movement
        Transform cam = Camera.main.transform;
        Vector3 camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 camRight   = Vector3.ProjectOnPlane(cam.right,   Vector3.up).normalized;
        Vector3 moveDir = (camForward * input.y + camRight * input.x).normalized;
        isWalking = moveDir.sqrMagnitude > 0.01f;
        animator.SetBool("isWalking", isWalking);
        // Gravity + jump
        if (controller.isGrounded)
        {
            verticalVelocity = -1f;
            if (jumpAction != null && jumpAction.WasPressedThisFrame())
                verticalVelocity = jumpForce;
        }
        else
            verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = moveDir * moveSpeed + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);

        // Rotate character towards movement direction (unless an aim is driving the facing)
        if (!SuppressRotation && moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.moveDirection.y < -0.3f) return;

        if (hit.collider.TryGetComponent(out PushableBlock pushableBlock))
            pushableBlock.Push(hit.moveDirection);
    }
}

