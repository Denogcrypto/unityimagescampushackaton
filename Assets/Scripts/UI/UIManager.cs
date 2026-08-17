using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Conecta GameEvents (bus estático) con la UI de la escena. No llama a
/// GameManager directo salvo para pedir acciones (RequestActivity/Descansar);
/// toda lectura de estado llega por evento, nunca por polling.
/// Las barras de HUD (Mood/Energy) se ocultan siempre — 4.12: "no se dibujan
/// nunca". Se dejan los campos serializados para no romper el cableado del
/// Inspector, pero no reciben valores reales.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("HUD (oculto siempre, ver 4.12)")]
    [SerializeField] private Slider moodBar;
    [SerializeField] private Slider energyBar;
    [SerializeField] private Text dayLabel;

    [Header("Menú de actividades (S9 — plano)")]
    [SerializeField] private GameObject activityMenuPanel;
    [SerializeField] private List<ActivityButton> activityButtons = new List<ActivityButton>();
    [SerializeField] private Button descansarButton;

    void Start()
    {
        if (moodBar != null) moodBar.gameObject.SetActive(false);
        if (energyBar != null) energyBar.gameObject.SetActive(false);

        descansarButton?.onClick.AddListener(() => GameManager.Instance.RequestDescansar());

        GameEvents.OnDayStarted += OnDayStarted;
        GameEvents.OnActivityResolved += (_, __) => RefreshActivityButtons();

        if (dayLabel != null && GameManager.Instance != null)
            dayLabel.text = $"Día {GameManager.Instance.CurrentDay} / {GameManager.Instance.TotalDias}";

        RefreshActivityButtons();
    }

    void OnDestroy()
    {
        GameEvents.OnDayStarted -= OnDayStarted;
    }

    void OnDayStarted(int dia, int totalDias)
    {
        if (dayLabel != null) dayLabel.text = $"Día {dia} / {totalDias}";
        RefreshActivityButtons();
    }

    void RefreshActivityButtons()
    {
        foreach (var btn in activityButtons)
            btn?.Refresh();
    }
}
