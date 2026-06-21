using UnityEngine;

// Minimal HUD: shows ONLY the throw-power bar while charging a throw
// (how far the head will be thrown). Everything else (control hints, timer,
// ability tips, win screen) is handled by the Canvas-based UI now.
// Auto-spawns once per scene, no manual wiring needed.
public class GameHUD : MonoBehaviour
{
    private HeadThrow headThrow;

    private Texture2D barBgTex;
    private Texture2D barFillTex;
    private GUIStyle barLabelStyle;
    private bool stylesReady;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindFirstObjectByType<GameHUD>() != null) return;

        GameObject go = new GameObject("_GameHUD");
        go.AddComponent<GameHUD>();
    }

    void Update()
    {
        if (headThrow == null)
            headThrow = FindFirstObjectByType<HeadThrow>();
    }

    void OnGUI()
    {
        if (headThrow == null || !headThrow.IsAiming) return;

        EnsureStyles();

        float w = Screen.width;
        float h = Screen.height;
        float barW = Mathf.Min(w * 0.32f, 320f);
        float barH = Mathf.Max(h * 0.022f, 14f);
        float bx = (w - barW) * 0.5f;
        float by = h - barH - 40f;

        GUI.DrawTexture(new Rect(bx, by, barW, barH), barBgTex);
        float fill = Mathf.Clamp01(headThrow.ChargeNormalized);
        GUI.DrawTexture(new Rect(bx, by, barW * fill, barH), barFillTex);
        GUI.Label(new Rect(bx, by - barH - 2f, barW, barH), "Wurfkraft", barLabelStyle);
    }

    private void EnsureStyles()
    {
        if (stylesReady) return;

        barBgTex   = SolidTexture(new Color(0f, 0f, 0f, 0.6f));
        barFillTex = SolidTexture(new Color(1f, 0.78f, 0.15f, 0.95f));

        int fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.height * 0.025f), 12, 38);
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
