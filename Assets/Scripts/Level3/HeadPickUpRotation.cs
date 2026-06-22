using UnityEngine;

public class HeadPickUpRotation : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 90f;

    private void FixedUpdate()
    {
        gameObject.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }
}
