using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the animated "end of day" transition panel.
/// Wire OnDayEnded event from GameManager, or call ShowTransition() directly.
/// </summary>
public class DayTransitionController : MonoBehaviour
{
    [Header("Panel")]
    public CanvasGroup canvasGroup;
    public Text dayLabel;
    public Text moodStatusLabel;   // "Ánimo estable / inestable"
    public Text streakLabel;       // "Días inestables: X/3"

    [Header("Timing")]
    public float fadeInDuration = 0.3f;
    public float holdDuration = 1.8f;
    public float fadeOutDuration = 0.3f;

    void Start()
    {
        var gm = GameManager.Instance;
        gm.OnDayEnded.AddListener(ShowTransition);
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    public void ShowTransition()
    {
        StartCoroutine(TransitionRoutine());
    }

    IEnumerator TransitionRoutine()
    {
        var gm = GameManager.Instance;
        gameObject.SetActive(true);

        // Populate labels
        if (dayLabel != null)
            dayLabel.text = $"Fin del día {gm.CurrentDay}";

        bool stable = gm.Mood >= gm.moodSafeMin && gm.Mood <= gm.moodSafeMax;
        if (moodStatusLabel != null)
            moodStatusLabel.text = stable ? "✓ Ánimo estable" : "✗ Ánimo inestable";

        if (streakLabel != null)
            streakLabel.text = gm.UnstableDaysStreak > 0
                ? $"Días inestables: {gm.UnstableDaysStreak}/{gm.maxUnstableDays}"
                : "";

        // Fade in
        if (canvasGroup != null)
        {
            float t = 0f;
            while (t < fadeInDuration)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = t / fadeInDuration;
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        yield return new WaitForSeconds(holdDuration);

        // Fade out
        if (canvasGroup != null)
        {
            float t = 0f;
            while (t < fadeOutDuration)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = 1f - (t / fadeOutDuration);
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }

        gameObject.SetActive(false);
    }
}
