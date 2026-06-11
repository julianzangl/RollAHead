using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    [SerializeField] private string targetGateName;

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

        collected = true;
        OpenTargetGate();
        gameObject.SetActive(false);
    }

    private void OpenTargetGate()
    {
        Gate[] gates = Object.FindObjectsByType<Gate>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (Gate gate in gates)
        {
            if (gate.name != targetGateName) continue;

            gate.Open();
            return;
        }
    }

    private bool IsActivator(Collider other)
    {
        return other.GetComponentInParent<Character>() != null
            || other.GetComponentInParent<Head>() != null
            || other.GetComponentInParent<RobotHead>() != null;
    }
}
