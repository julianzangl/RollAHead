using UnityEngine;
using UnityEngine.InputSystem;

// Slime head: bounces off the ground on every landing so it keeps hopping forever.
// Built for vertical jump-'n'-run sections — the player just steers, the bounce is automatic.
public class SlimeHead : MonoBehaviour, IThrowableHead
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float bounceForce = 9f;          // upward speed applied on each landing
    [SerializeField] private float groundCheckDistance = 0.4f;
    [SerializeField] private float bounceCooldown = 0.08f;    // guards against multi-bounce on one contact
    [SerializeField] private Color headColor = new Color(0.35f, 0.85f, 0.3f, 1f); // slime tint

    private Rigidbody rb;
    private InputAction moveAction;
    private InputAction recallAction;
    private HeadThrow headThrow;

    private bool isActive;
    private float nextBounceTime;

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
        recallAction = new InputAction("Recall", InputActionType.Button, "<Keyboard>/f");
        recallAction.Enable();

        ApplyHeadColor();
    }

    public void Initialize(Vector3 throwDirection, float throwForce, HeadThrow headThrow)
    {
        this.headThrow = headThrow;
        isActive = true;
        rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
    }

    // Tint the thrown head so it keeps the ability colour after being thrown,
    // matching the colour shown while it's attached to the body.
    private void ApplyHeadColor()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        foreach (Renderer headRenderer in renderers)
        {
            Material material = headRenderer.material;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", headColor);
            if (material.HasProperty("_Color")) material.SetColor("_Color", headColor);
        }
    }

    void OnDestroy()
    {
        recallAction.Disable();
        recallAction.Dispose();
    }

    void Update()
    {
        if (!isActive) return;

        if (recallAction.WasPressedThisFrame())
        {
            isActive = false;
            headThrow.ReturnHead();
        }
    }

    void FixedUpdate()
    {
        if (!isActive) return;

        // Horizontal steering only — vertical is owned by gravity + the bounce.
        Vector2 input = moveAction.ReadValue<Vector2>();
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        Transform cam = mainCamera.transform;
        Vector3 camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
        Vector3 moveDir = (camForward * input.y + camRight * input.x).normalized;

        Vector3 horizontal = moveDir * moveSpeed;
        rb.linearVelocity = new Vector3(horizontal.x, rb.linearVelocity.y, horizontal.z);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isActive) return;
        if (Time.time < nextBounceTime) return;

        // Bounce whenever we land on top of a surface.
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y <= 0.5f) continue;

            Vector3 velocity = rb.linearVelocity;
            velocity.y = bounceForce;
            rb.linearVelocity = velocity;
            nextBounceTime = Time.time + bounceCooldown;
            break;
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
