using UnityEngine;

// One-shot head-switch pickup: when the body walks in, it equips the chosen
// head ability and removes the collected capsule from the scene.
[RequireComponent(typeof(Collider))]
public class HeadSwitchPickupOnce : MonoBehaviour
{
    [SerializeField] private HeadThrow.HeadAbility ability = HeadThrow.HeadAbility.Normal;

    private bool collected;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected || other.GetComponentInParent<Character>() == null) return;

        HeadThrow headThrow = other.GetComponentInParent<HeadThrow>();
        if (headThrow == null)
            headThrow = Object.FindFirstObjectByType<HeadThrow>();
        if (headThrow == null) return;

        collected = true;
        headThrow.EquipAbility(ability);
        Destroy(gameObject);
    }
}
