using System.Collections;
using UnityEngine;
using TMPro;

public class StoryTextManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject storyPanel;
    [SerializeField] private TextMeshProUGUI storyText;

    [Header("Settings")]
    [SerializeField] private float displayDuration = 7f;

    private Coroutine currentStoryCoroutine;

    public void ShowStoryText(string text)
    {
        if (currentStoryCoroutine != null)
        {
            StopCoroutine(currentStoryCoroutine);
        }

        currentStoryCoroutine = StartCoroutine(ShowStoryTextRoutine(text));
    }

    private IEnumerator ShowStoryTextRoutine(string text)
    {
        storyPanel.SetActive(true);
        storyText.text = text;

        yield return new WaitForSeconds(displayDuration);

        storyPanel.SetActive(false);
        currentStoryCoroutine = null;
    }
}