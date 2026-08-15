using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives all UI: HUD bars, menu navigation, activity buttons, overlays.
/// Attach to a persistent Canvas or UIManager GameObject.
/// </summary>
public class UIManager : MonoBehaviour
{
    // ─── HUD References ──────────────────────────────────────────
    [Header("HUD")]
    [SerializeField] private Slider moodBar;
    [SerializeField] private Slider energyBar;
    [SerializeField] private Text dayLabel;          // "Day X / 30"
    [SerializeField] private Text moodValueLabel;    // optional numeric display
    [SerializeField] private Text energyValueLabel;

    [Header("Mood Bar Safe Zone Markers")]
    [SerializeField] private RectTransform safeZoneMarkerLeft;   // positioned at 30% of bar width
    [SerializeField] private RectTransform safeZoneMarkerRight;  // positioned at 75% of bar width

    [Header("Unstable Day Warning")]
    [SerializeField] private GameObject unstableWarningPanel;    // shown when streak > 0
    [SerializeField] private Text unstableStreakLabel; // "⚠ X/3 días inestables"

    // ─── Menu Panels ─────────────────────────────────────────────
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject activitiesPanel;
    [SerializeField] private GameObject parkPanel;
    [SerializeField] private GameObject socialPanel;

    // ─── Activity Buttons ────────────────────────────────────────
    [Header("Activity Buttons (assign in Inspector)")]
    [SerializeField] private List<ActivityButton> activityButtons = new List<ActivityButton>();

    // ─── Overlays ────────────────────────────────────────────────
    // Day transition panel is owned entirely by DayTransitionController (shows and
    // hides itself via its own coroutine) — UIManager does not touch it.
    [Header("Overlays")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text gameOverLabel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private Text victoryLabel;

    // ─── Animation lock ──────────────────────────────────────────
    private bool waitingForAnimation = false;

    // ─────────────────────────────────────────────────────────────
    void Start()
    {
        // Hook into GameManager events
        var gm = GameManager.Instance;
        gm.OnStatsChanged.AddListener(RefreshHUD);
        gm.OnDayStarted.AddListener(OnDayStarted);
        gm.OnGameOver.AddListener(ShowGameOver);
        gm.OnVictory.AddListener(ShowVictory);

        // Initial state
        ShowMainMenu();
        RefreshHUD();
        SetSafeZoneMarkers();
    }

    // ─── HUD ─────────────────────────────────────────────────────

    void RefreshHUD()
    {
        var gm = GameManager.Instance;

        if (moodBar != null)    moodBar.value = gm.Mood / 100f;
        if (energyBar != null)  energyBar.value = gm.Energy / 100f;
        if (dayLabel != null)   dayLabel.text = $"Día {gm.CurrentDay} / {gm.TotalDays}";
        if (moodValueLabel != null)   moodValueLabel.text = $"{gm.Mood:F0}";
        if (energyValueLabel != null) energyValueLabel.text = $"{gm.Energy:F0}";

        // Unstable streak indicator
        if (unstableWarningPanel != null)
            unstableWarningPanel.SetActive(gm.UnstableDaysStreak > 0);
        if (unstableStreakLabel != null)
            unstableStreakLabel.text = $"⚠ {gm.UnstableDaysStreak}/{gm.MaxUnstableDays} días inestables";

        // Refresh button states
        RefreshActivityButtons();
    }

    void SetSafeZoneMarkers()
    {
        if (moodBar == null) return;
        float barWidth = moodBar.GetComponent<RectTransform>().rect.width;
        float safeMinPct = GameManager.Instance.MoodSafeMin / 100f;
        float safeMaxPct = GameManager.Instance.MoodSafeMax / 100f;

        if (safeZoneMarkerLeft != null)
            safeZoneMarkerLeft.anchoredPosition = new Vector2(barWidth * safeMinPct, 0);
        if (safeZoneMarkerRight != null)
            safeZoneMarkerRight.anchoredPosition = new Vector2(barWidth * safeMaxPct, 0);
    }

    void RefreshActivityButtons()
    {
        foreach (var btn in activityButtons)
            btn?.Refresh();
    }

    // ─── Menu Navigation ─────────────────────────────────────────

    public void ShowMainMenu()
    {
        SetPanel(mainMenuPanel, true);
        SetPanel(activitiesPanel, false);
        SetPanel(parkPanel, false);
        SetPanel(socialPanel, false);
    }

    public void ShowActivitiesPanel()
    {
        SetPanel(mainMenuPanel, false);
        SetPanel(activitiesPanel, true);
        SetPanel(parkPanel, false);
        SetPanel(socialPanel, false);
    }

    public void ShowParkPanel()
    {
        SetPanel(mainMenuPanel, false);
        SetPanel(activitiesPanel, false);
        SetPanel(parkPanel, true);
        SetPanel(socialPanel, false);
    }

    public void ShowSocialPanel()
    {
        SetPanel(mainMenuPanel, false);
        SetPanel(activitiesPanel, false);
        SetPanel(parkPanel, false);
        SetPanel(socialPanel, true);
    }

    void SetPanel(GameObject panel, bool active)
    {
        if (panel != null) panel.SetActive(active);
    }

    // ─── Activity Trigger ────────────────────────────────────────

    /// Called by ActivityButton when the player taps an activity.
    public void RequestActivity(ActivityData activity)
    {
        if (waitingForAnimation) return;
        if (activity == null) return;
        var gm = GameManager.Instance;
        if (gm.IsGameOver || gm.IsVictory) return;

        if (!activity.IsRestActivity && !gm.CanAfford(activity))
        {
            // Could flash an error here
            Debug.Log($"[UI] No energy for {activity.ActivityName}");
            return;
        }

        // Trigger animation via CharacterAnimator, then apply
        var animator = CharacterAnimator.Instance;
        if (animator != null)
        {
            waitingForAnimation = true;
            animator.PlayActivity(activity, () =>
            {
                waitingForAnimation = false;
                gm.ApplyActivity(activity);
                ShowMainMenu();
            });
        }
        else
        {
            // No animator: apply directly
            gm.ApplyActivity(activity);
            ShowMainMenu();
        }
    }

    // ─── Day Events ──────────────────────────────────────────────

    void OnDayStarted()
    {
        ShowMainMenu();
    }

    // ─── End Screens ─────────────────────────────────────────────

    void ShowGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        var gm = GameManager.Instance;

        string reasonText = gm.LastGameOverReason == GameManager.GameOverReason.TooSad
            ? "Tu personaje quedó demasiado triste."
            : "Tu personaje quedó demasiado feliz.";

        if (gameOverLabel != null)
            gameOverLabel.text = $"{reasonText}\n\n{ArchetypeText(gm.GetArchetype())}";
    }

    void ShowVictory()
    {
        if (victoryPanel != null) victoryPanel.SetActive(true);
        var gm = GameManager.Instance;

        if (victoryLabel != null)
            victoryLabel.text = $"¡Lo lograste!\n\n{ArchetypeText(gm.GetArchetype())}";
    }

    /// S11: describes the play archetype and mentions the other two as equally valid paths.
    string ArchetypeText(GameManager.PlayArchetype archetype)
    {
        switch (archetype)
        {
            case GameManager.PlayArchetype.Stabilizer:
                return "Tu estilo: Estabilizador — pasos cortos y seguros.\n"
                     + "También se puede ganar arriesgando con golpes grandes (Especialista de choque) o alternando según el día (Adaptativo).";
            case GameManager.PlayArchetype.ShockSpecialist:
                return "Tu estilo: Especialista de choque — golpes grandes, riesgo alto.\n"
                     + "También se puede ganar con pasos cortos y seguros (Estabilizador) o alternando según el día (Adaptativo).";
            default:
                return "Tu estilo: Adaptativo — leíste el estado cada día.\n"
                     + "También se puede ganar con pasos cortos y seguros (Estabilizador) o golpes grandes (Especialista de choque).";
        }
    }

    // ─── Public restart helper (wire to button) ──────────────────
    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}
