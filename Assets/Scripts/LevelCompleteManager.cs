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

    [Header("Star Rating")]
    [SerializeField] private TextMeshProUGUI starRatingText;
    [SerializeField] private float threeStarTime = 180f;
    [SerializeField] private float twoStarTime = 360f;

    private bool levelCompleted = false;

    public void CompleteLevel()
    {
        if (levelCompleted)
            return;

        Time.timeScale = 0f; // Bitte lieber Gott das ist die letzte Idee
        levelCompleted = true;

        StopAndShowFinalTime();
        ShowStarRating();
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

    private void ShowStarRating()
    {
        if (levelTimer == null || starRatingText == null)
            return;

        float finalTime = levelTimer.GetElapsedTime();
        int stars = CalculateStars(finalTime);

        starRatingText.text = GetStarText(stars);
    }

    private int CalculateStars(float time)
    {
        if (time <= threeStarTime)
        {
            return 3;
        }

        if (time <= twoStarTime)
        {
            return 2;
        }

        return 1;
    }

    private string GetStarText(int stars)
    {
        if (stars == 3)
        {
            //return "★★★"; //Unity erkennt den Stern nicht 
            return "3/3 Sterne";
        }

        if (stars == 2)
        {
            //return "★★☆";
            return "2/3 Sterne";
        }

        //return "★☆☆";
        return "1/3 Sterne";
    }
}
