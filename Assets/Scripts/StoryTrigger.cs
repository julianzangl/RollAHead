using UnityEngine;

public class StoryTrigger : MonoBehaviour
{
    [SerializeField] private StoryTextManager storyTextManager;

    [TextArea(3, 6)]
    [SerializeField] private string storyText;

    [SerializeField] private bool triggerOnlyOnce = true;

    private bool wasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnlyOnce && wasTriggered)
            return;

        if (other.CompareTag("Player"))
        {
            wasTriggered = true;
            storyTextManager.ShowStoryText(storyText);
        }
    }
}