using UnityEngine;

// Reusable head-switch point: when the body walks in, it equips the chosen head ability.
// Unlike the one-shot pickups it does NOT disappear, so you can switch back and forth
// between several of them (handy for testing all heads in one scene).
[RequireComponent(typeof(Collider))]
public class HeadSwitchPickup : MonoBehaviour
{
    [SerializeField] private HeadThrow.HeadAbility ability = HeadThrow.HeadAbility.Normal;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<Character>() == null) return;

        HeadThrow headThrow = other.GetComponentInParent<HeadThrow>();
        if (headThrow == null)
            headThrow = Object.FindFirstObjectByType<HeadThrow>();
        if (headThrow == null) return;

        headThrow.EquipAbility(ability);
    }
}
