using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to each activity Button in the UI.
/// Displays name, risk tier, willing/not-willing state, energy cost preview.
/// </summary>
public class ActivityButton : MonoBehaviour
{
    [Header("Data")]
    public ActivityData activity;

    [Header("UI Elements")]
    public Text nameLabel;
    public Text costLabel;        // "⚡ 15"  or  "⚡ 23 (no disp.)"
    public Text moodDeltaLabel;   // "+6"  or  "+2"
    public Image riskTierIcon;              // swap sprite based on tier
    public Sprite lowRiskSprite;
    public Sprite highRiskSprite;
    public Image notWillingOverlay;         // semi-transparent tint when fatigued
    public Button button;

    void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        button?.onClick.AddListener(OnClick);
    }

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (activity == null) return;
        var gm = GameManager.Instance;
        if (gm == null) return;

        bool notWilling = gm.IsNotWilling(activity);
        bool canAfford = gm.CanAfford(activity);

        // Name
        if (nameLabel != null)
            nameLabel.text = activity.activityName;

        if (!activity.isRestActivity)
        {
            float cost = gm.GetEffectiveEnergyCost(activity);
            float delta = gm.GetEffectiveMoodDelta(activity);

            // Cost label
            if (costLabel != null)
            {
                costLabel.text = notWilling
                    ? $"⚡ {cost:F0}  (no disp.)"
                    : $"⚡ {cost:F0}";
            }

            // Mood delta label
            if (moodDeltaLabel != null)
                moodDeltaLabel.text = delta >= 0 ? $"+{delta:F0} 😊" : $"{delta:F0} 😞";

            // Risk icon
            if (riskTierIcon != null)
            {
                riskTierIcon.sprite = activity.riskTier == RiskTier.High ? highRiskSprite : lowRiskSprite;
                riskTierIcon.gameObject.SetActive(true);
            }
        }
        else
        {
            // Descansar
            if (costLabel != null)      costLabel.text = "Termina el día";
            if (moodDeltaLabel != null) moodDeltaLabel.text = "+20 ⚡";
            if (riskTierIcon != null)   riskTierIcon.gameObject.SetActive(false);
        }

        // Not-willing overlay
        if (notWillingOverlay != null)
            notWillingOverlay.gameObject.SetActive(notWilling);

        // Disable button if can't afford
        if (button != null)
            button.interactable = canAfford || activity.isRestActivity;
    }

    void OnClick()
    {
        UIManager uiManager = FindObjectOfType<UIManager>();
        uiManager?.RequestActivity(activity);
    }
}
