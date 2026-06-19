using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCompleteManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject levelCompletePanel;

    [Header("Player")]
    [SerializeField] private GameObject player;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string nextLevelSceneName = "Level2";

    private bool levelCompleted = false;

    public void CompleteLevel()
    {
        if (levelCompleted)
            return;

        levelCompleted = true;

        ShowLevelCompleteUI();
        UnlockCursor();
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
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void LoadNextLevel()
    {
        SceneManager.LoadScene(nextLevelSceneName);
    }
}