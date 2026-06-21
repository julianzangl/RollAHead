using UnityEngine;

// Level exit. When the zombie body walks through it, the level-complete UI is shown
// via the LevelCompleteManager (same Canvas flow as JewelLevelEnd).
public class Finish : MonoBehaviour
{
    [SerializeField] private LevelCompleteManager levelCompleteManager;

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

        if (levelCompleteManager == null)
            levelCompleteManager = FindFirstObjectByType<LevelCompleteManager>();

        if (levelCompleteManager != null)
            levelCompleteManager.CompleteLevel();
        else
            Debug.LogWarning("[Finish] No LevelCompleteManager in scene — add one so the win screen shows.");
    }
}
