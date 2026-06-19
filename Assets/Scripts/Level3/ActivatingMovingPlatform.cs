using Unity.VisualScripting;
using UnityEngine;

public class ActivatingMovingPlatform : MonoBehaviour
{
    [SerializeField] GameObject movingPlatform;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) return;

        movingPlatform.GetComponent<MovingPlatform>().SetShouldMoving(true);

        this.gameObject.SetActive(false);
    }
}
