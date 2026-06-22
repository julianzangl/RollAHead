using Unity.VisualScripting;
using UnityEngine;

public class ActivatingMovingPlatform : MonoBehaviour
{
    [SerializeField] GameObject movingPlatform;
    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 90f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) return;

        movingPlatform.GetComponent<MovingPlatform>().SetShouldMoving(true);

        this.gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        gameObject.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }
}
