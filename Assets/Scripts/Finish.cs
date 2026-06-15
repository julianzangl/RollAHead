using UnityEngine;

// Level exit. When the zombie body walks through it, the run is finished:
// the HUD stops its timer and shows the win screen.
public class Finish : MonoBehaviour
{
    public static event System.Action Reached;

    private bool triggered;

    void Awake()
    {
        Collider finishCollider = GetComponent<Collider>();
        if (finishCollider != null)
            finishCollider.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        // Only the body finishes the level, not a thrown head.
        if (other.GetComponentInParent<Character>() == null) return;

        triggered = true;
        Reached?.Invoke();
    }
}
