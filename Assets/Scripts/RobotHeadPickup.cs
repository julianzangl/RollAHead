using UnityEngine;

public class RobotHeadPickup : MonoBehaviour
{
    private bool collected;

    void Awake()
    {
        Collider pickupCollider = GetComponent<Collider>();
        if (pickupCollider != null)
            pickupCollider.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected || !IsActivator(other)) return;

        HeadThrow headThrow = other.GetComponentInParent<HeadThrow>();
        if (headThrow == null)
            headThrow = Object.FindFirstObjectByType<HeadThrow>();

        if (headThrow == null) return;

        collected = true;
        headThrow.EnableRobotHead();
        gameObject.SetActive(false);
    }

    private bool IsActivator(Collider other)
    {
        return other.GetComponentInParent<Character>() != null;
    }
}
