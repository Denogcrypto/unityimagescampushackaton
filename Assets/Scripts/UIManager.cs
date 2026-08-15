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
    [Header("Overlays")]
    [SerializeField] private GameObject dayTransitionPanel;
    [SerializeField] private Text dayTransitionLabel;  // "Día X"
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text gameOverLabel;
    [SerializeField] private GameObject victoryPanel;

    // ─── Animation lock ──────────────────────────────────────────
    private bool waitingForAnimation = false;

    // ─────────────────────────────────────────────────────────────
    void Start()
    {
        // Hook into GameManager events
        var gm = GameManager.Instance;
        gm.OnStatsChanged.AddListener(RefreshHUD);
        gm.OnDayStarted.AddListener(OnDayStarted);
        gm.OnDayEnded.AddListener(OnDayEnded);
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
        if (dayTransitionPanel != null) dayTransitionPanel.SetActive(false);
        ShowMainMenu();
    }

    void OnDayEnded()
    {
        if (dayTransitionPanel != null)
        {
            dayTransitionPanel.SetActive(true);
            if (dayTransitionLabel != null)
                dayTransitionLabel.text = $"Fin del día {GameManager.Instance.CurrentDay}";
        }
    }

    // ─── End Screens ─────────────────────────────────────────────

    void ShowGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (gameOverLabel != null)
            gameOverLabel.text = "Tu personaje está inestable.\n¡Inténtalo de nuevo!";
    }

    void ShowVictory()
    {
        if (victoryPanel != null) victoryPanel.SetActive(true);
    }

    // ─── Public restart helper (wire to button) ──────────────────
    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}
