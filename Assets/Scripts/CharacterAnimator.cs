using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Plays activity animations on the character sprite.
/// If no Animator is present, falls back to a simple flash/scale tween.
/// </summary>
public class CharacterAnimator : MonoBehaviour
{
    public static CharacterAnimator Instance { get; private set; }

    [Header("References")]
    public Animator animator;           // optional Unity Animator
    public SpriteRenderer characterSprite;

    [Header("Fallback animation")]
    public float fallbackDuration = 0.8f;   // seconds of fake animation

    // Animator parameter names (must match in the Animator Controller)
    private const string TriggerActivity = "PlayActivity";
    private const string ParamActivityName = "ActivityName";  // not used in basic setup

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (animator == null) animator = GetComponent<Animator>();
        if (characterSprite == null) characterSprite = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Plays the activity animation and calls onComplete when finished.
    /// </summary>
    public void PlayActivity(ActivityData activity, Action onComplete)
    {
        if (animator != null && animator.isActiveAndEnabled)
        {
            StartCoroutine(PlayWithAnimator(activity, onComplete));
        }
        else
        {
            StartCoroutine(FallbackAnimation(activity, onComplete));
        }
    }

    IEnumerator PlayWithAnimator(ActivityData activity, Action onComplete)
    {
        // Set a trigger — extend this with per-activity triggers as needed
        animator.SetTrigger(TriggerActivity);

        // Wait for the next state to start, then for it to finish
        yield return null; // let trigger register
        yield return new WaitForSeconds(GetAnimationLength());

        onComplete?.Invoke();
    }

    IEnumerator FallbackAnimation(ActivityData activity, Action onComplete)
    {
        // Simple scale pulse to signal activity is happening
        if (characterSprite != null)
        {
            float elapsed = 0f;
            Vector3 originalScale = characterSprite.transform.localScale;

            while (elapsed < fallbackDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.PingPong(elapsed * 4f, 1f);
                characterSprite.transform.localScale = originalScale * (1f + 0.08f * t);
                yield return null;
            }

            characterSprite.transform.localScale = originalScale;
        }
        else
        {
            yield return new WaitForSeconds(fallbackDuration);
        }

        onComplete?.Invoke();
    }

    float GetAnimationLength()
    {
        if (animator == null) return fallbackDuration;
        AnimatorClipInfo[] clips = animator.GetCurrentAnimatorClipInfo(0);
        return clips.Length > 0 ? clips[0].clip.length : fallbackDuration;
    }
}
