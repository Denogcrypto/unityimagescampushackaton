using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to each activity Button in the UI.
/// Displays name, risk tier, willing/not-willing state, energy cost preview.
/// </summary>
public class ActivityButton : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private ActivityData activity;

    [Header("UI Elements")]
    [SerializeField] private Text nameLabel;
    [SerializeField] private Text costLabel;        // "⚡ 15"  or  "⚡ 23 (no disp.)"
    [SerializeField] private Text moodDeltaLabel;   // "+6"  or  "+2"
    [SerializeField] private Image riskTierIcon;              // swap sprite based on tier
    [SerializeField] private Sprite lowRiskSprite;
    [SerializeField] private Sprite highRiskSprite;
    [SerializeField] private Image notWillingOverlay;         // semi-transparent tint when fatigued
    [SerializeField] private Button button;

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
            nameLabel.text = activity.ActivityName;

        if (!activity.IsRestActivity)
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
                riskTierIcon.sprite = activity.RiskTier == RiskTier.High ? highRiskSprite : lowRiskSprite;
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
            button.interactable = canAfford || activity.IsRestActivity;
    }

    void OnClick()
    {
        UIManager uiManager = FindObjectOfType<UIManager>();
        uiManager?.RequestActivity(activity);
    }
}
