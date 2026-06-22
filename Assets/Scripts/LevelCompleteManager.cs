using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelCompleteManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject levelCompletePanel;

    [Header("Player")]
    [SerializeField] private GameObject player;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string nextLevelSceneName = "Level2";

    [Header("Timer")]
    [SerializeField] private LevelTimer levelTimer;
    [SerializeField] private TextMeshProUGUI finalTimeText;
    [SerializeField] private GameObject timerTextObject;

    private bool levelCompleted = false;

    public void CompleteLevel()
    {
        if (levelCompleted)
            return;

        Time.timeScale = 0f; // Bitte lieber Gott das ist die letzte Idee
        levelCompleted = true;

        StopAndShowFinalTime();
        ShowLevelCompleteUI();
        UnlockCursor();
    }

    private void StopAndShowFinalTime()
    {
        if (levelTimer != null)
        {
            levelTimer.StopTimer();

            if (finalTimeText != null)
            {
                finalTimeText.text = "Zeit: " + levelTimer.GetFormattedTime();
            }
        }

        if (timerTextObject != null)
        {
            timerTextObject.SetActive(false);
        }
    }

    private void ShowLevelCompleteUI()
    {
        levelCompletePanel.SetActive(true);
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextLevelSceneName);
    }
}
