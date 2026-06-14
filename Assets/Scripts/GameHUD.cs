using UnityEngine;

// Lightweight on-screen help. Auto-spawns once per scene (no manual wiring needed) and draws:
//  - a persistent control hint (incl. "hold F to throw the head")
//  - a power bar while charging a throw (Wii-golf swing)
//  - a temporary tip when a new head ability is unlocked (e.g. wall climbing)
public class GameHUD : MonoBehaviour
{
    private const float TipDuration = 6f;

    private HeadThrow headThrow;
    private string tipMessage;
    private float tipTimer;

    private Texture2D panelTex;
    private Texture2D barBgTex;
    private Texture2D barFillTex;
    private GUIStyle hintStyle;
    private GUIStyle tipStyle;
    private GUIStyle barLabelStyle;
    private bool stylesReady;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindFirstObjectByType<GameHUD>() != null) return;

        GameObject go = new GameObject("_GameHUD");
        go.AddComponent<GameHUD>();
    }

    void OnEnable()  { HeadThrow.OnRobotHeadUnlocked += HandleRobotHeadUnlocked; }
    void OnDisable() { HeadThrow.OnRobotHeadUnlocked -= HandleRobotHeadUnlocked; }

    private void HandleRobotHeadUnlocked()
    {
        ShowTip("Roboterkopf erhalten!  Halte [Linksklick] an einer Wand, um daran hochzuklettern.");
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

        if (tipTimer > 0f)
            tipTimer -= Time.deltaTime;
    }

    void OnGUI()
    {
        EnsureStyles();

        float w = Screen.width;
        float h = Screen.height;
        float hintPanelH = Mathf.Max(h * 0.06f, 34f);

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
            Rect r = new Rect((w - panelW) * 0.5f, h * 0.06f, panelW, panelH);
            GUI.DrawTexture(r, panelTex);
            GUI.Label(r, tipMessage, tipStyle);
        }
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

        return "[WASD] Bewegen    [Leertaste] Springen    Halte [F] für Kopfwurf (länger gedrückt = weiter)";
    }

    private void EnsureStyles()
    {
        if (stylesReady) return;

        panelTex   = SolidTexture(new Color(0f, 0f, 0f, 0.55f));
        barBgTex   = SolidTexture(new Color(0f, 0f, 0f, 0.6f));
        barFillTex = SolidTexture(new Color(1f, 0.78f, 0.15f, 0.95f));

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
