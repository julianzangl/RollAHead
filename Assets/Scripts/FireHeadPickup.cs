using UnityEngine;

// Walk the body into this to equip the fire head (burns Burnable objects on contact).
public class FireHeadPickup : MonoBehaviour
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
        if (collected || other.GetComponentInParent<Character>() == null) return;

        HeadThrow headThrow = other.GetComponentInParent<HeadThrow>();
        if (headThrow == null)
            headThrow = Object.FindFirstObjectByType<HeadThrow>();
        if (headThrow == null) return;

        collected = true;
        headThrow.EnableFireHead();
        gameObject.SetActive(false);
    }
}
