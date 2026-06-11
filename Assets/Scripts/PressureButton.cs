using UnityEngine;

public class PressureButton : MonoBehaviour
{
    [SerializeField] private string[] targetGateNames;
    [SerializeField] private bool oneShot = true;

    private bool pressed;

    void Awake()
    {
        Collider buttonCollider = GetComponent<Collider>();
        if (buttonCollider != null)
            buttonCollider.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        TryPress(other);
    }

    void OnTriggerStay(Collider other)
    {
        TryPress(other);
    }

    private void TryPress(Collider other)
    {
        if (oneShot && pressed) return;
        if (!IsActivator(other)) return;

        pressed = true;
        OpenTargetGates();
    }

    private void OpenTargetGates()
    {
        Gate[] gates = Object.FindObjectsByType<Gate>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (string targetName in targetGateNames)
        {
            foreach (Gate gate in gates)
            {
                if (gate.name != targetName) continue;

                gate.Open();
                break;
            }
        }
    }

    private bool IsActivator(Collider other)
    {
        return other.GetComponentInParent<Character>() != null
            || other.GetComponentInParent<Head>() != null
            || other.GetComponentInParent<RobotHead>() != null;
    }
}
