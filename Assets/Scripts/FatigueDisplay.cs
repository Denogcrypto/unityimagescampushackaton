using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Optional: shows a fatigue bar under each activity button.
/// Attach to a child Image of the ActivityButton and assign the activity.
/// </summary>
public class FatigueDisplay : MonoBehaviour
{
    public ActivityData activity;
    public Slider fatigueSlider;
    public Image fillImage;
    public Color willingColor = new Color(0.3f, 0.9f, 0.3f);   // green
    public Color notWillingColor = new Color(0.9f, 0.3f, 0.3f); // red

    void OnEnable() => Refresh();

    public void Refresh()
    {
        if (activity == null || fatigueSlider == null) return;
        var gm = GameManager.Instance;
        if (gm == null) return;

        float fatigue = gm.GetFatigue(activity);
        fatigueSlider.value = fatigue / 100f;

        if (fillImage != null)
            fillImage.color = gm.IsNotWilling(activity) ? notWillingColor : willingColor;
    }
}
