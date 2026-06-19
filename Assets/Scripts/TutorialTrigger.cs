using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [SerializeField] private TutorialHint tutorialHint;

    private bool wasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (wasTriggered)
            return;

        if (other.CompareTag("Player"))
        {
            wasTriggered = true;
            tutorialHint.ShowHeadThrowTutorial();
        }
    }
}