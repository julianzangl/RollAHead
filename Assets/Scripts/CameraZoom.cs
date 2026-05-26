using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraZoom : MonoBehaviour
{
    [SerializeField] private float zoomSpeed   = 0.05f;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 20f;

    // Supports both common Cinemachine 3.x body types
    private CinemachineThirdPersonFollow thirdPerson;
    private CinemachineOrbitalFollow     orbital;

    void Start()
    {
        thirdPerson = GetComponent<CinemachineThirdPersonFollow>();
        orbital     = GetComponent<CinemachineOrbitalFollow>();

        if (thirdPerson == null && orbital == null)
            Debug.LogWarning("[CameraZoom] No CinemachineThirdPersonFollow or CinemachineOrbitalFollow found on " + gameObject.name);
    }

    void Update()
    {
        float scroll = Mouse.current?.scroll.ReadValue().y ?? 0f;
        if (scroll == 0f) return;

        float delta = -scroll * zoomSpeed;

        if (thirdPerson != null)
        {
            thirdPerson.CameraDistance = Mathf.Clamp(
                thirdPerson.CameraDistance + delta,
                minDistance, maxDistance);
        }
        else if (orbital != null)
        {
            orbital.Radius = Mathf.Clamp(
                orbital.Radius + delta,
                minDistance, maxDistance);
        }
    }
}
