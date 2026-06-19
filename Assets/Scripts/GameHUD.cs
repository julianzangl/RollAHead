using UnityEngine;
using UnityEngine.SceneManagement;

// Lightweight on-screen help. Auto-spawns once per scene (no manual wiring needed) and draws:
//  - a run timer (top), stopped when the level is finished
//  - a persistent control hint (incl. "hold F to throw the head")
//  - a power bar while charging a throw (Wii-golf swing)
//  - a temporary tip when a new head ability is unlocked (e.g. wall climbing)
//  - a win screen (with the final time) when the Finish block is reached
public class GameHUD : MonoBehaviour
{
    private const float TipDuration = 6f;

    private HeadThrow headThrow;
    private string tipMessage;
    private float tipTimer;

    private float elapsedTime;
    private bool timerRunning = true;
    private bool won;

    private Texture2D panelTex;
    private Texture2D barBgTex;
    private Texture2D barFillTex;
    private Texture2D dimTex;
    private GUIStyle hintStyle;
    private GUIStyle tipStyle;
    private GUIStyle barLabelStyle;
    private GUIStyle timerStyle;
    private GUIStyle winTitleStyle;
    private GUIStyle winTimeStyle;
    private GUIStyle buttonStyle;
    private bool stylesReady;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindFirstObjectByType<GameHUD>() != null) return;

        GameObject go = new GameObject("_GameHUD");
        go.AddComponent<GameHUD>();
    }

    void OnEnable()
    {
        HeadThrow.OnRobotHeadUnlocked += HandleRobotHeadUnlocked;
        Finish.Reached += HandleFinish;
    }

    void OnDisable()
    {
        HeadThrow.OnRobotHeadUnlocked -= HandleRobotHeadUnlocked;
        Finish.Reached -= HandleFinish;
        // Safety: never leave a reloaded/next scene frozen if we paused on the win screen.
        Time.timeScale = 1f;
    }

    private void HandleRobotHeadUnlocked()
    {
        ShowTip("Roboterkopf erhalten!  Halte [Linksklick] an einer Wand, um daran hochzuklettern.");
    }

    private void HandleFinish()
    {
        if (won) return;

        won = true;
        timerRunning = false;
        Time.timeScale = 0f;                  // freeze gameplay behind the win screen
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ShowTip(string message)
    {
        tipMessage = message;
        tipTimer = TipDuration;
    }

    void Update()
    {
        if (headThrow == null)
            headThrow = FindFirstObjectByType<HeadThrow>();

        // Count real gameplay time. Use unscaledDeltaTime so it isn't affected by any pause.
        if (timerRunning && headThrow != null)
            elapsedTime += Time.unscaledDeltaTime;

        if (tipTimer > 0f)
            tipTimer -= Time.unscaledDeltaTime;
    }

    private static string FormatTime(float t)
    {
        int minutes = (int)(t / 60f);
        float seconds = t - minutes * 60f;
        return string.Format("{0:00}:{1:00.00}", minutes, seconds);
    }

    void OnGUI()
    {
        EnsureStyles();

        float w = Screen.width;
        float h = Screen.height;
        float hintPanelH = Mathf.Max(h * 0.06f, 34f);

        // Run timer (top-center)
        if (headThrow != null && !won)
        {
            float timerW = Mathf.Min(w * 0.3f, 260f);
            float timerH = Mathf.Max(h * 0.05f, 30f);
            Rect tr = new Rect((w - timerW) * 0.5f, 10f, timerW, timerH);
            GUI.DrawTexture(tr, panelTex);
            GUI.Label(tr, FormatTime(elapsedTime), timerStyle);
        }

        // Win screen takes over the whole screen — draw it and stop.
        if (won)
        {
            DrawWinScreen(w, h);
            return;
        }

        // Persistent control hint (bottom-center)
        string hint = BuildHint();
        if (!string.IsNullOrEmpty(hint))
        {
            float panelW = Mathf.Min(w * 0.92f, 820f);
            Rect r = new Rect((w - panelW) * 0.5f, h - hintPanelH - 16f, panelW, hintPanelH);
            GUI.DrawTexture(r, panelTex);
            GUI.Label(r, hint, hintStyle);
        }

        // Power bar while charging a throw
        if (headThrow != null && headThrow.IsAiming)
        {
            float barW = Mathf.Min(w * 0.32f, 320f);
            float barH = Mathf.Max(h * 0.022f, 14f);
            float bx = (w - barW) * 0.5f;
            float by = h - hintPanelH - 16f - barH - 22f;

            GUI.DrawTexture(new Rect(bx, by, barW, barH), barBgTex);
            float fill = Mathf.Clamp01(headThrow.ChargeNormalized);
            GUI.DrawTexture(new Rect(bx, by, barW * fill, barH), barFillTex);
            GUI.Label(new Rect(bx, by - barH - 2f, barW, barH), "Wurfkraft", barLabelStyle);
        }

        // Temporary ability tip (top-center)
        if (tipTimer > 0f && !string.IsNullOrEmpty(tipMessage))
        {
            float panelW = Mathf.Min(w * 0.92f, 880f);
            float panelH = Mathf.Max(h * 0.08f, 48f);
            Rect r = new Rect((w - panelW) * 0.5f, h * 0.1f, panelW, panelH);
            GUI.DrawTexture(r, panelTex);
            GUI.Label(r, tipMessage, tipStyle);
        }
    }

    private void DrawWinScreen(float w, float h)
    {
        GUI.DrawTexture(new Rect(0f, 0f, w, h), dimTex);

        float boxW = Mathf.Min(w * 0.6f, 560f);
        float boxH = Mathf.Min(h * 0.5f, 360f);
        Rect box = new Rect((w - boxW) * 0.5f, (h - boxH) * 0.5f, boxW, boxH);
        GUI.DrawTexture(box, panelTex);

        float pad = boxH * 0.12f;
        GUI.Label(new Rect(box.x, box.y + pad, box.width, boxH * 0.25f), "Geschafft!", winTitleStyle);
        GUI.Label(new Rect(box.x, box.y + boxH * 0.42f, box.width, boxH * 0.18f),
            "Zeit: " + FormatTime(elapsedTime), winTimeStyle);

        float btnW = boxW * 0.5f;
        float btnH = boxH * 0.18f;
        Rect btn = new Rect(box.x + (boxW - btnW) * 0.5f, box.y + boxH - btnH - pad, btnW, btnH);
        if (GUI.Button(btn, "Neustart", buttonStyle))
            RestartLevel();
    }

    private string BuildHint()
    {
        // No player in this scene (e.g. a non-gameplay scene) -> show nothing.
        if (headThrow == null)
            return string.Empty;

        if (headThrow.IsHeadThrown)
        {
            string thrown = "[WASD] Bewegen    [F] Kopf zurückholen";
            if (headThrow.RobotHeadUnlocked)
                thrown += "    [Linksklick] an Wänden klettern";
            return thrown;
        }

        return "[WASD] Bewegen    Halte [F] für Kopfwurf (länger gedrückt = weiter)";
    }

    private void EnsureStyles()
    {
        if (stylesReady) return;

        panelTex   = SolidTexture(new Color(0f, 0f, 0f, 0.55f));
        barBgTex   = SolidTexture(new Color(0f, 0f, 0f, 0.6f));
        barFillTex = SolidTexture(new Color(1f, 0.78f, 0.15f, 0.95f));
        dimTex     = SolidTexture(new Color(0f, 0f, 0f, 0.75f));

        int fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.height * 0.025f), 12, 38);

        hintStyle = new GUIStyle
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = fontSize,
            wordWrap = true
        };
        hintStyle.normal.textColor = Color.white;

        tipStyle = new GUIStyle
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.RoundToInt(fontSize * 1.05f),
            fontStyle = FontStyle.Bold,
            wordWrap = true
        };
        tipStyle.normal.textColor = new Color(1f, 0.95f, 0.7f);

        barLabelStyle = new GUIStyle
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.RoundToInt(fontSize * 0.8f)
        };
        barLabelStyle.normal.textColor = Color.white;

        timerStyle = new GUIStyle
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.RoundToInt(fontSize * 1.1f),
            fontStyle = FontStyle.Bold
        };
        timerStyle.normal.textColor = Color.white;

        winTitleStyle = new GUIStyle
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.RoundToInt(fontSize * 2.2f),
            fontStyle = FontStyle.Bold
        };
        winTitleStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);

        winTimeStyle = new GUIStyle
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.RoundToInt(fontSize * 1.3f)
        };
        winTimeStyle.normal.textColor = Color.white;

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.RoundToInt(fontSize * 1.1f),
            fontStyle = FontStyle.Bold
        };

        stylesReady = true;
    }

    private static Texture2D SolidTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}
