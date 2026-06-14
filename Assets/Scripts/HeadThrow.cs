using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class HeadThrow : MonoBehaviour
{
    [SerializeField] private GameObject throwableHead;
    [SerializeField] private GameObject currentHead;
    [SerializeField] private Transform headSocket;
    [SerializeField] private new CinemachineCamera camera;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private Character characterBody; // assign in Inspector, or auto-found

    [Header("Throw Aiming")]
    [SerializeField] private float throwTorque = 8f;          // spin so the head tumbles instead of flying lifeless
    [SerializeField] private float launchAngle = 40f;         // fixed upward angle so it always arcs (Angry-Birds style)
    [SerializeField] private float aimTurnSpeed = 12f;         // how fast the character turns to face the aim while holding
    [SerializeField] private float spawnForwardOffset = 0.6f;  // push the thrown head out of the player before launch
    [SerializeField] private LayerMask aimCollisionLayers = ~0;

    [Header("Aim Dots")]
    [SerializeField] private int maxDots = 25;                // max dots in the preview
    [SerializeField] private float dotSpacing = 0.55f;        // world-space gap between dots along the arc
    [SerializeField] private float dotSize = 0.08f;           // diameter of the first dot
    [SerializeField] private float dotEndScale = 0.2f;       // last dot shrinks to this fraction of dotSize
    [SerializeField] private Color dotColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private float aimSimStep = 0.02f;        // fine integration step for the arc

    private InputAction throwAction;
    private bool isHeadThrown;
    private readonly List<Transform> aimDots = new List<Transform>();
    private Material dotMaterial;
    private float throwableMass = 1f;
    private Collider[] ownColliders;
    private Transform originalFollow;
    private Transform originalLookAt;
    private Vector3 originalTargetOffset;
    private CinemachineRotationComposer rotationComposer;
    private Vector3 originalOrbitalOffset;
    private CinemachineOrbitalFollow orbitalFollow;
    private GameObject spawnedHead;
    private GameObject camTarget;
    private bool robotHeadUnlocked;

    private Animator animator;
    private static readonly Color RobotHeadColor = new Color(0.72f, 0.76f, 0.78f, 1f);

    void Start()
    {
        throwAction = InputSystem.actions.FindAction("Throw");
        originalFollow = camera.Follow;
        originalLookAt = camera.LookAt;
        animator = GetComponent<Animator>();

        rotationComposer = camera.GetComponent<CinemachineRotationComposer>();
        if (rotationComposer == null)
            Debug.LogWarning("[HeadThrow] No CinemachineRotationComposer found on the camera. TargetOffset will not be adjusted.");
        else
            originalTargetOffset = rotationComposer.TargetOffset;
        
        orbitalFollow = camera.GetComponent<CinemachineOrbitalFollow>();
        if (orbitalFollow == null)
            Debug.LogWarning("[HeadThrow] No CinemachineOrbitalFollow found on the camera. Radius will not be adjusted.");
        else
            originalOrbitalOffset = orbitalFollow.TargetOffset;

        if (characterBody == null)
            characterBody = GetComponent<Character>();

        // The aim ray starts at the head socket, which sits inside the player's own colliders;
        // cache them so the preview ray doesn't immediately hit ourselves and collapse to nothing.
        ownColliders = GetComponentsInChildren<Collider>(true);

        if (throwableHead != null)
        {
            Rigidbody prefabRb = throwableHead.GetComponent<Rigidbody>();
            if (prefabRb != null && prefabRb.mass > 0f)
                throwableMass = prefabRb.mass;
        }

        SetupDotMaterial();
    }

    private void SetupDotMaterial()
    {
        // Pick a shader that actually renders under URP ("Sprites/Default" shows up magenta there).
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        if (shader == null) shader = Shader.Find("Sprites/Default");

        dotMaterial = new Material(shader);
        dotMaterial.color = dotColor;
        if (dotMaterial.HasProperty("_BaseColor")) dotMaterial.SetColor("_BaseColor", dotColor);
    }

    private Transform GetDot(int index)
    {
        // Lazily create dots (small unlit spheres, no collider) and reuse them across frames.
        while (aimDots.Count <= index)
        {
            GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dot.name = "_AimDot";
            Collider dotCollider = dot.GetComponent<Collider>();
            if (dotCollider != null) Destroy(dotCollider);

            Renderer dotRenderer = dot.GetComponent<Renderer>();
            dotRenderer.sharedMaterial = dotMaterial;
            dotRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            dotRenderer.receiveShadows = false;

            dot.SetActive(false);
            aimDots.Add(dot.transform);
        }
        return aimDots[index];
    }

    private void HideAimDots()
    {
        for (int i = 0; i < aimDots.Count; i++)
            if (aimDots[i] != null && aimDots[i].gameObject.activeSelf)
                aimDots[i].gameObject.SetActive(false);
    }

    void Update()
    {
        // Hold to aim (live trajectory preview), release to throw.
        bool aiming = !isHeadThrown && throwAction.IsPressed();
        if (aiming)
        {
            FaceAimDirection();
            UpdateAimPreview();
        }
        else
        {
            HideAimDots();
        }

        // While aiming, HeadThrow owns the facing — stop Character from fighting it.
        if (characterBody != null)
            characterBody.SuppressRotation = aiming;

        if (throwAction.WasReleasedThisFrame() && !isHeadThrown)
        {
            HideAimDots();
            ThrowHead();
        }

        // Update camTarget position without inheriting head rotation (prevents camera wobble)
        if (spawnedHead != null && camTarget != null)
            camTarget.transform.position = spawnedHead.transform.position + Vector3.up * 1.5f;
    }

    // Camera look direction flattened onto the ground plane.
    private Vector3 CameraFlatForward()
    {
        Vector3 flat = Vector3.ProjectOnPlane(camera.transform.forward, Vector3.up);
        if (flat.sqrMagnitude < 0.001f)
            flat = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
        return flat.normalized;
    }

    // While aiming, turn the character to face where the camera looks, so the camera "drives"
    // the character's facing and you end up seeing its back instead of its front.
    private void FaceAimDirection()
    {
        Vector3 flat = CameraFlatForward();
        if (flat.sqrMagnitude < 0.001f) return;

        Quaternion target = Quaternion.LookRotation(flat, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, aimTurnSpeed * Time.deltaTime);
    }

    // Aim direction = where the CHARACTER faces (horizontal), pitched up by a fixed launch angle
    // so the head always flies a clean arc in the direction the character is looking.
    private Vector3 GetThrowDirection()
    {
        Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        if (flatForward.sqrMagnitude < 0.001f)
            flatForward = CameraFlatForward();

        Vector3 right = Vector3.Cross(Vector3.up, flatForward);
        // Negative angle around the right axis tilts the forward vector upward.
        return Quaternion.AngleAxis(-launchAngle, right) * flatForward;
    }

    private Vector3 GetThrowOrigin()
    {
        return headSocket.position;
    }

    // Walk the projectile arc (same launch the head gets: impulse / mass = velocity, under gravity)
    // and drop evenly-spaced dots along it, stopping where it hits a surface.
    private void UpdateAimPreview()
    {
        Vector3 origin = GetThrowOrigin();
        Vector3 velocity = GetThrowDirection() * (throwForce / throwableMass);
        Vector3 gravity = Physics.gravity;

        Vector3 previous = origin;
        float distanceSinceDot = dotSpacing;   // place a dot almost immediately
        int dotIndex = 0;
        float maxTime = 6f;

        for (float t = aimSimStep; t <= maxTime && dotIndex < maxDots; t += aimSimStep)
        {
            Vector3 point = origin + velocity * t + 0.5f * gravity * t * t;
            Vector3 segment = point - previous;
            float segmentLength = segment.magnitude;

            // Stop the arc at the first surface (ignoring the player's own colliders).
            if (segmentLength > 0.0001f &&
                AimRaycast(previous, segment.normalized, segmentLength, out RaycastHit hit))
            {
                PlaceDot(dotIndex, hit.point);
                dotIndex++;
                break;
            }

            distanceSinceDot += segmentLength;
            if (distanceSinceDot >= dotSpacing)
            {
                distanceSinceDot -= dotSpacing;
                PlaceDot(dotIndex, point);
                dotIndex++;
            }

            previous = point;
        }

        // Hide any dots left over from a longer arc last frame.
        for (int i = dotIndex; i < aimDots.Count; i++)
            if (aimDots[i].gameObject.activeSelf)
                aimDots[i].gameObject.SetActive(false);
    }

    private void PlaceDot(int index, Vector3 position)
    {
        Transform dot = GetDot(index);
        dot.position = position;

        // Taper the dots so they shrink toward the landing point.
        float fraction = maxDots > 1 ? (float)index / (maxDots - 1) : 0f;
        float scale = dotSize * Mathf.Lerp(1f, dotEndScale, fraction);
        dot.localScale = new Vector3(scale, scale, scale);

        if (!dot.gameObject.activeSelf)
            dot.gameObject.SetActive(true);
    }

    // Raycast that skips the thrower's own colliders, so the arc isn't cut off at the player.
    private bool AimRaycast(Vector3 from, Vector3 direction, float distance, out RaycastHit nearest)
    {
        nearest = default;
        RaycastHit[] hits = Physics.RaycastAll(from, direction, distance, aimCollisionLayers, QueryTriggerInteraction.Ignore);
        float bestDistance = float.MaxValue;
        bool found = false;

        foreach (RaycastHit hit in hits)
        {
            if (IsOwnCollider(hit.collider)) continue;
            if (hit.distance >= bestDistance) continue;

            bestDistance = hit.distance;
            nearest = hit;
            found = true;
        }

        return found;
    }

    private bool IsOwnCollider(Collider candidate)
    {
        if (ownColliders == null) return false;
        for (int i = 0; i < ownColliders.Length; i++)
            if (ownColliders[i] == candidate) return true;
        return false;
    }

    // Stop the player's body from blocking the freshly thrown head (it spawns right next to us).
    private void IgnoreCollisionsWithPlayer(GameObject head)
    {
        if (ownColliders == null) return;

        Collider[] headColliders = head.GetComponentsInChildren<Collider>(true);
        foreach (Collider headCollider in headColliders)
        {
            if (headCollider.isTrigger) continue;
            foreach (Collider ownCollider in ownColliders)
            {
                if (ownCollider == null || ownCollider.isTrigger) continue;
                Physics.IgnoreCollision(headCollider, ownCollider, true);
            }
        }
    }

    private void ThrowHead()
    {
        isHeadThrown = true;
        animator.SetBool("isWalking", false); // Stop walking animation when throwing head
        Vector3 throwDirection = GetThrowDirection();

        // Spawn the head pushed out along the throw direction so it clears the player's body,
        // otherwise it spawns inside the player's collider and just pops straight up.
        Vector3 spawnPosition = headSocket.position + throwDirection * spawnForwardOffset;
        spawnedHead = Instantiate(throwableHead, spawnPosition, headSocket.rotation);
        IgnoreCollisionsWithPlayer(spawnedHead);
        RobotHead upgradedRobotHead = robotHeadUnlocked ? ApplyRobotHeadUpgrade(spawnedHead) : null;

        currentHead.SetActive(false);

        // Camera target is NOT parented to head — manually tracked in Update() to avoid rotation
        camTarget = new GameObject("_HeadCamTarget");
        camTarget.transform.position = spawnedHead.transform.position + Vector3.up * 1.5f;

        Head headScript = robotHeadUnlocked ? null : spawnedHead.GetComponent<Head>();
        RobotHead robotHeadScript = upgradedRobotHead != null ? upgradedRobotHead : spawnedHead.GetComponent<RobotHead>();
        if (headScript == null && robotHeadScript == null)
        {
            Debug.LogError("[HeadThrow] throwableHead prefab is missing the Head script! Add Head.cs or RobotHead.cs to the prefab.");
            currentHead.SetActive(true);
            isHeadThrown = false;
            Destroy(spawnedHead);
            Destroy(camTarget);
            return;
        }
        if (robotHeadScript != null)
            robotHeadScript.Initialize(throwDirection, throwForce, this);
        else
            headScript.Initialize(throwDirection, throwForce, this);

        // Add a tumble so the head feels alive in flight instead of sliding stiffly.
        Rigidbody spawnedRb = spawnedHead.GetComponent<Rigidbody>();
        if (spawnedRb != null && throwTorque > 0f)
        {
            Vector3 spinAxis = Vector3.Cross(throwDirection, Vector3.up).normalized;
            if (spinAxis.sqrMagnitude < 0.01f) spinAxis = Vector3.right;
            spawnedRb.AddTorque(spinAxis * throwTorque, ForceMode.Impulse);
        }

        camera.Follow = camTarget.transform;
        camera.LookAt = camTarget.transform;
        if (rotationComposer != null)
            rotationComposer.TargetOffset = new Vector3(0f, -1.5f, 0f);
        if (orbitalFollow != null)
            orbitalFollow.TargetOffset = new Vector3(0f, -2.5f, 0f); // Adjust the radius as needed
        if (characterBody != null)
            characterBody.enabled = false;
    }

    void OnDestroy()
    {
        for (int i = 0; i < aimDots.Count; i++)
            if (aimDots[i] != null)
                Destroy(aimDots[i].gameObject);
        aimDots.Clear();

        if (dotMaterial != null)
            Destroy(dotMaterial);
    }

    public void EnableRobotHead()
    {
        robotHeadUnlocked = true;
        ApplySilverColor(currentHead);

        if (spawnedHead != null)
        {
            RobotHead robotHead = ApplyRobotHeadUpgrade(spawnedHead);
            robotHead.Initialize(Vector3.zero, 0f, this);
        }
    }

    private RobotHead ApplyRobotHeadUpgrade(GameObject headObject)
    {
        Head normalHead = headObject.GetComponent<Head>();
        if (normalHead != null)
        {
            normalHead.enabled = false;
            Destroy(normalHead);
        }

        RobotHead robotHead = headObject.GetComponent<RobotHead>();
        if (robotHead == null)
            robotHead = headObject.AddComponent<RobotHead>();

        ApplySilverColor(headObject);
        return robotHead;
    }

    private void ApplySilverColor(GameObject target)
    {
        if (target == null) return;

        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer headRenderer in renderers)
        {
            Material material = headRenderer.material;
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", RobotHeadColor);
            if (material.HasProperty("_Color"))
                material.SetColor("_Color", RobotHeadColor);
        }
    }

    public void ReturnHead()
    {
        StartCoroutine(FlyBackRoutine());
    }

    private IEnumerator FlyBackRoutine()
    {
        // Freeze physics so we can drive the position manually
        Rigidbody rb = spawnedHead.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        Vector3 startPos = spawnedHead.transform.position;
        float duration = 0.45f;
        float elapsed  = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            spawnedHead.transform.position = Vector3.Lerp(startPos, headSocket.position, t);
            yield return null;
        }

        // Restore state
        currentHead.SetActive(true);
        isHeadThrown = false;
        camera.Follow = originalFollow;
        camera.LookAt = originalLookAt;
        if (rotationComposer != null)
            rotationComposer.TargetOffset = originalTargetOffset;
        if (orbitalFollow != null)
            orbitalFollow.TargetOffset = originalOrbitalOffset;
        if (characterBody != null) characterBody.enabled = true;
        Destroy(camTarget);
        Destroy(spawnedHead);
        camTarget    = null;
        spawnedHead  = null;
    }
}
