using UnityEngine;
using TMPro;

public class LevelTimer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;

    private float elapsedTime = 0f;
    private bool timerRunning = true;

    private void Update()
    {
        if (!timerRunning)
            return;

        elapsedTime += Time.deltaTime;
        UpdateTimerText();
    }

    private void UpdateTimerText()
    {
        timerText.text = FormatTime(elapsedTime);
    }

    public void StopTimer()
    {
        timerRunning = false;
        UpdateTimerText();
    }

    public float GetElapsedTime()
    {
        return elapsedTime;
    }

    public string GetFormattedTime()
    {
        return FormatTime(elapsedTime);
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int milliseconds = Mathf.FloorToInt((time * 100f) % 100f);

        return $"{minutes:00}:{seconds:00}.{milliseconds:00}";
    }
}