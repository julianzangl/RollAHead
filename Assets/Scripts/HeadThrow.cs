using System.Collections;
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

    private InputAction throwAction;
    private bool isHeadThrown;
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
    }

    void Update()
    {
        if (throwAction.WasPressedThisFrame() && !isHeadThrown)
        {
            ThrowHead();
        }

        // Update camTarget position without inheriting head rotation (prevents camera wobble)
        if (spawnedHead != null && camTarget != null)
            camTarget.transform.position = spawnedHead.transform.position + Vector3.up * 1.5f;
    }

    private void ThrowHead()
    {
        isHeadThrown = true;
        animator.SetBool("isWalking", false); // Stop walking animation when throwing head
        Vector3 throwDirection = camera.transform.forward;

        spawnedHead = Instantiate(throwableHead, headSocket.position, headSocket.rotation);
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

        camera.Follow = camTarget.transform;
        camera.LookAt = camTarget.transform;
        if (rotationComposer != null)
            rotationComposer.TargetOffset = new Vector3(0f, -1.5f, 0f);
        if (orbitalFollow != null)
            orbitalFollow.TargetOffset = new Vector3(0f, -2.5f, 0f); // Adjust the radius as needed
        if (characterBody != null)
            characterBody.enabled = false;
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
