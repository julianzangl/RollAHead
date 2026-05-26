using UnityEngine;
using UnityEngine.InputSystem;

public class Head : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float gravity = -14f;
    [SerializeField] private float throwDrag = 4f;

    private CharacterController controller;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction recallAction;
    private HeadThrow headThrow;

    private Vector3 throwVelocity;
    private float verticalVelocity;
    private bool isActive;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        moveAction   = InputSystem.actions.FindAction("Move");
        jumpAction   = InputSystem.actions.FindAction("Jump");
        recallAction = new InputAction("Recall", InputActionType.Button, "<Keyboard>/f");
        recallAction.Enable();

        // Disable any Rigidbody that might still be on the prefab
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
    }

    public void Initialize(Vector3 throwDirection, float throwForce, HeadThrow headThrow)
    {
        this.headThrow = headThrow;
        throwVelocity = throwDirection * throwForce;
        isActive = true;
    }

    void OnDestroy()
    {
        recallAction.Disable();
        recallAction.Dispose();
    }

    void Update()
    {
        if (!isActive) return;

        // F key: recall head back to body
        if (recallAction.WasPressedThisFrame())
        {
            headThrow.ReturnHead();
            Destroy(gameObject);
            return;
        }

        // Decay initial throw velocity (prevents infinite rolling)
        throwVelocity = Vector3.MoveTowards(throwVelocity, Vector3.zero, throwDrag * Time.deltaTime);

        // Gravity & jump
        if (controller.isGrounded)
        {
            verticalVelocity = -1f;
            if (jumpAction.WasPressedThisFrame())
                verticalVelocity = jumpForce;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        // Camera-relative WASD input
        Vector2 input = moveAction.ReadValue<Vector2>();
        Transform cam = Camera.main.transform;
        Vector3 camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 camRight   = Vector3.ProjectOnPlane(cam.right,   Vector3.up).normalized;
        Vector3 moveDir    = (camForward * input.y + camRight * input.x).normalized;

        // Combine decaying throw velocity + player input + gravity
        Vector3 horizontal = new Vector3(throwVelocity.x, 0f, throwVelocity.z)
                             + moveDir * moveSpeed;
        Vector3 finalVelocity = horizontal + Vector3.up * verticalVelocity;

        controller.Move(finalVelocity * Time.deltaTime);
    }
}

