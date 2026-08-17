using UnityEngine;

/// <summary>
/// S14 — panel de debug (OnGUI, editor/dev build only). Sliders de los 3
/// stats, set de energía, forzar cierre de día, saltar al día N, forzar
/// estado de ánimo, toggle de fluctuación, valores crudos ocultos a la vista.
/// </summary>
public class DebugPanel : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private GameManager gm;
    private string jumpDayInput = "1";

    void Start() => gm = GameManager.Instance;

    void OnGUI()
    {
        if (gm == null) return;

        float w = 300f, x = 10f, y = 10f;
        GUI.Box(new Rect(x, y, w, 430), "DEBUG");
        y += 25;

        GUI.Label(new Rect(x + 10, y, w - 20, 20),
            $"Día {gm.CurrentDay}/{gm.TotalDias} · Energía {gm.Energy:F0} · Acciones {gm.ActionsUsedToday}");
        y += 24;

        y = StatSlider(x, y, w, StatId.VidaSocial, "Vida social");
        y = StatSlider(x, y, w, StatId.Autoestima, "Autoestima");
        y = StatSlider(x, y, w, StatId.ActividadFisica, "Actividad física");

        y += 6;
        GUI.Label(new Rect(x + 10, y, w - 20, 20), "Energía");
        y += 18;
        float newEnergy = GUI.HorizontalSlider(new Rect(x + 10, y, w - 20, 20), gm.Energy, 0f, 100f);
        if (!Mathf.Approximately(newEnergy, gm.Energy)) gm.Debug_SetEnergy(newEnergy);
        y += 26;

        if (GUI.Button(new Rect(x + 10, y, w - 20, 30), "Forzar cierre de día (Descansar)"))
            gm.Debug_ForceCloseDay();
        y += 34;

        GUI.Label(new Rect(x + 10, y, 70, 24), "Ir al día");
        jumpDayInput = GUI.TextField(new Rect(x + 90, y, 50, 24), jumpDayInput);
        if (GUI.Button(new Rect(x + 150, y, 140, 24), "Saltar"))
            if (int.TryParse(jumpDayInput, out int day)) gm.Debug_JumpToDay(day);
        y += 30;

        GUI.Label(new Rect(x + 10, y, w - 20, 20), "Forzar ánimo:");
        y += 20;
        DrawMoodButtons(x, y, w);
        y += 30;

        bool fluct = gm.Debug_FluctuationEnabled;
        bool newFluct = GUI.Toggle(new Rect(x + 10, y, w - 20, 24), fluct, " Fluctuación nocturna activa");
        if (newFluct != fluct) gm.Debug_FluctuationEnabled = newFluct;
    }

    float StatSlider(float x, float y, float w, StatId id, string label)
    {
        float value = gm.GetStat(id);
        GUI.Label(new Rect(x + 10, y, w - 20, 20), $"{label}: {value:F0} ({gm.GetStatZone(id)})");
        y += 18;
        float newValue = GUI.HorizontalSlider(new Rect(x + 10, y, w - 20, 20), value, 0f, 100f);
        if (!Mathf.Approximately(newValue, value)) gm.Debug_SetStat(id, newValue);
        return y + 26;
    }

    void DrawMoodButtons(float x, float y, float w)
    {
        string[] labels = { "Depresivo", "Triste", "Neutral", "Feliz", "Alterado" };
        float bw = (w - 20) / labels.Length;
        for (int i = 0; i < labels.Length; i++)
        {
            if (GUI.Button(new Rect(x + 10 + i * bw, y, bw - 2, 24), labels[i]))
                gm.Debug_ForceMood((MoodState)i);
        }
    }
#endif
}
