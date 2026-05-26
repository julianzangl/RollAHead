using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class HeadThrow : MonoBehaviour
{
    [SerializeField] private GameObject throwableHead;
    [SerializeField] private GameObject currentHead;
    [SerializeField] private Transform headSocket;
    [SerializeField] private CinemachineCamera camera;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private Character characterBody; // assign in Inspector, or auto-found

    private InputAction throwAction;
    private bool isHeadThrown;
    private Transform originalFollow;
    private Transform originalLookAt;

    void Start()
    {
        throwAction = InputSystem.actions.FindAction("Throw");
        originalFollow = camera.Follow;
        originalLookAt = camera.LookAt;

        if (characterBody == null)
            characterBody = GetComponent<Character>();
    }

    // Update is called once per frame
    void Update()
    {
        if (throwAction.WasPressedThisFrame() && !isHeadThrown)
        {
            ThrowHead();
        }
    }

    private void ThrowHead()
    {
        isHeadThrown = true;

        Vector3 throwDirection = camera.transform.forward;

        GameObject head = Instantiate(throwableHead, headSocket.position, headSocket.rotation);
        currentHead.SetActive(false);

        // Elevated camera target: keeps camera at character height, not ground level
        GameObject camTarget = new GameObject("_HeadCamTarget");
        camTarget.transform.SetParent(head.transform);
        camTarget.transform.localPosition = new Vector3(0f, 1.5f, 0f);

        Head headScript = head.GetComponent<Head>();
        if (headScript == null)
        {
            Debug.LogError("[HeadThrow] throwableHead prefab is missing the Head script! Add Head.cs to the prefab.");
            currentHead.SetActive(true);
            isHeadThrown = false;
            Destroy(head);
            return;
        }
        headScript.Initialize(throwDirection, throwForce, this);

        camera.Follow = camTarget.transform;
        camera.LookAt = camTarget.transform;

        if (characterBody != null)
            characterBody.enabled = false;
    }

    public void ReturnHead()
    {
        currentHead.SetActive(true);
        isHeadThrown = false;
        camera.Follow = originalFollow;
        camera.LookAt = originalLookAt;
        if (characterBody != null)
            characterBody.enabled = true;
    }
}
