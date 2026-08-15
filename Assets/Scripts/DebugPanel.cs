using UnityEngine;

/// <summary>
/// On-screen debug panel (only visible in Editor / Development Builds).
/// Validates GameManager victory/defeat logic without real UI.
/// Disable or destroy this GameObject in release builds.
/// </summary>
public class DebugPanel : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private GameManager gm;
    private GUIStyle boxStyle;
    private GUIStyle buttonStyle;
    private bool initialized = false;

    void Start()
    {
        gm = GameManager.Instance;
    }

    void InitStyles()
    {
        if (initialized) return;
        boxStyle = new GUIStyle(GUI.skin.box)
        {
            fontSize = 22,
            normal = { textColor = new Color(0.6f, 1f, 0.2f) }
        };
        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 20,
            fixedHeight = 48,
            fixedWidth = 200
        };
        initialized = true;
    }

    void OnGUI()
    {
        if (gm == null) return;
        InitStyles();

        float panelX = 10f;
        float panelY = 10f;
        float w = 220f;
        float h = 500f;

        GUI.Box(new Rect(panelX, panelY, w, h), "", boxStyle);

        float x = panelX + 10f;
        float y = panelY + 10f;
        float bh = 50f;
        float gap = 8f;

        // Status
        GUI.Label(new Rect(x, y, w - 20, 30), $"Día: {gm.CurrentDay}/{gm.totalDays}", boxStyle);
        y += 30;
        GUI.Label(new Rect(x, y, w - 20, 30), $"Ánimo: {gm.Mood:F1}", boxStyle);
        y += 30;
        GUI.Label(new Rect(x, y, w - 20, 30), $"Energía: {gm.Energy:F1}", boxStyle);
        y += 30;
        GUI.Label(new Rect(x, y, w - 20, 30), $"Inestable: {gm.UnstableDaysStreak}/3", boxStyle);
        y += 35;

        if (GUI.Button(new Rect(x, y, w - 20, bh), "+10 Ánimo", buttonStyle)) gm.Debug_AddMood();
        y += bh + gap;
        if (GUI.Button(new Rect(x, y, w - 20, bh), "-10 Ánimo", buttonStyle)) gm.Debug_SubMood();
        y += bh + gap;
        if (GUI.Button(new Rect(x, y, w - 20, bh), "+20 Energía", buttonStyle)) gm.Debug_AddEnergy();
        y += bh + gap;
        if (GUI.Button(new Rect(x, y, w - 20, bh), "-20 Energía", buttonStyle)) gm.Debug_SubEnergy();
        y += bh + gap;
        if (GUI.Button(new Rect(x, y, w - 20, bh), "Avanzar Día", buttonStyle)) gm.Debug_AdvanceDay();
        y += bh + gap;
        if (GUI.Button(new Rect(x, y, w - 20, bh), "Ánimo Peligro (20)", buttonStyle)) gm.Debug_MoodDanger();
        y += bh + gap;
        if (GUI.Button(new Rect(x, y, w - 20, bh), "Ánimo Exceso (90)", buttonStyle)) gm.Debug_MoodOverhappy();
    }
#endif
}
