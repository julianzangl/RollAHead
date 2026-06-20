using UnityEngine;

public class JewelLevelEnd : MonoBehaviour
{
    [SerializeField] private LevelCompleteManager levelCompleteManager;

    private bool wasCollected = false;

    private void OnTriggerEnter(Collider other)
    {
        if (wasCollected)
            return;

        if (other.CompareTag("Player"))
        {
            wasCollected = true;

            levelCompleteManager.CompleteLevel();

            gameObject.SetActive(false);
        }
    }
}