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
    private int debugTargetDay = 1;

    void Start()
    {
        gm = GameManager.Instance;
        if (gm != null) debugTargetDay = gm.CurrentDay;
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
        float w = 240f;
        float h = 780f;

        GUI.Box(new Rect(panelX, panelY, w, h), "", boxStyle);

        float x = panelX + 10f;
        float y = panelY + 10f;
        float bh = 46f;
        float gap = 6f;

        // Status
        GUI.Label(new Rect(x, y, w - 20, 28), $"Día: {gm.CurrentDay}/{gm.TotalDays}", boxStyle);
        y += 28;
        GUI.Label(new Rect(x, y, w - 20, 28), $"Ánimo: {gm.Mood:F1}", boxStyle);
        y += 28;
        GUI.Label(new Rect(x, y, w - 20, 28), $"Energía: {gm.Energy:F1}", boxStyle);
        y += 28;
        GUI.Label(new Rect(x, y, w - 20, 28), $"Inestable: {gm.UnstableDaysStreak}/{gm.MaxUnstableDays}", boxStyle);
        y += 32;

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
        y += bh + gap;
        if (GUI.Button(new Rect(x, y, w - 20, bh), "Reset Fatiga", buttonStyle)) gm.Debug_ResetFatigue();
        y += bh + gap;

        string fluctuationLabel = gm.DisableRandomFluctuation ? "Fluctuación: OFF" : "Fluctuación: ON";
        if (GUI.Button(new Rect(x, y, w - 20, bh), fluctuationLabel, buttonStyle)) gm.Debug_ToggleFluctuation();
        y += bh + gap + 6f;

        GUI.Label(new Rect(x, y, w - 20, 28), $"Saltar a día: {debugTargetDay}", boxStyle);
        y += 32;
        float halfW = (w - 20 - gap) / 2f;
        if (GUI.Button(new Rect(x, y, halfW, bh), "-1", buttonStyle)) debugTargetDay = Mathf.Max(1, debugTargetDay - 1);
        if (GUI.Button(new Rect(x + halfW + gap, y, halfW, bh), "+1", buttonStyle)) debugTargetDay = Mathf.Min(gm.TotalDays, debugTargetDay + 1);
        y += bh + gap;
        if (GUI.Button(new Rect(x, y, w - 20, bh), "Saltar", buttonStyle)) gm.Debug_JumpToDay(debugTargetDay);
    }
#endif
}
