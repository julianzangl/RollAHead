using System;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class HeadThrow : MonoBehaviour
{

    [SerializeField] private GameObject throwableHead;
    [SerializeField] private GameObject currentHead;
    [SerializeField] private Transform headSocket;
    [SerializeField] private CinemachineCamera camera;
    [SerializeField] private float throwForce = 10f;
    private InputAction throwAction;
    private bool isHeadThrown;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        throwAction = InputSystem.actions.FindAction("Throw");
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

        Vector3 throwDirection = (camera.transform.forward + camera.transform.up * 0.5f).normalized;

        GameObject head = Instantiate(throwableHead, headSocket.position, headSocket.rotation);
        currentHead.SetActive(false);
        Rigidbody rb = head.GetComponent<Rigidbody>();
        rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);

        camera.Follow = head.transform;
        camera.LookAt = head.transform;
    }
}
