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
    [SerializeField] private Animator animator;           // optional Unity Animator
    [SerializeField] private SpriteRenderer characterSprite;

    [Header("Fallback animation")]
    [SerializeField] private float fallbackDuration = 0.8f;   // seconds of fake animation

    [Header("Mood Visual States (placeholder — swap for real art, S12)")]
    [SerializeField] private Color lowMoodColor = new Color(0.25f, 0.30f, 0.35f);   // apagado (below safe zone)
    [SerializeField] private Color calmMoodColor = new Color(0.55f, 0.68f, 0.06f);  // estable (safe zone)
    [SerializeField] private Color highMoodColor = new Color(0.75f, 0.15f, 0.15f);  // acelerado — debe leerse como malo, no feliz
    [SerializeField] private float accelJitterAmount = 0.06f;
    [SerializeField] private float accelJitterSpeed = 22f;

    private Vector3 basePosition;
    private bool isAccelerated;

    // Animator parameter names (must match in the Animator Controller)
    private const string TriggerActivity = "PlayActivity";
    private const string ParamActivityName = "ActivityName";  // not used in basic setup

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (animator == null) animator = GetComponent<Animator>();
        if (characterSprite == null) characterSprite = GetComponent<SpriteRenderer>();
        if (characterSprite != null) basePosition = characterSprite.transform.localPosition;
    }

    void Start()
    {
        // Subscribing in Start() (not OnEnable/Awake) guarantees GameManager.Instance
        // is already set, regardless of scene object initialization order.
        if (GameManager.Instance != null) GameManager.Instance.OnStatsChanged.AddListener(RefreshMoodVisual);
        RefreshMoodVisual();
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null) GameManager.Instance.OnStatsChanged.RemoveListener(RefreshMoodVisual);
    }

    /// Placeholder mood readout until real art (S12) lands: tints the sprite and,
    /// when "acelerado", adds a nervous jitter — the accelerated state must read
    /// as bad, never as happy.
    void RefreshMoodVisual()
    {
        var gm = GameManager.Instance;
        if (gm == null || characterSprite == null) return;

        if (gm.Mood < gm.MoodSafeMin) { characterSprite.color = lowMoodColor; isAccelerated = false; }
        else if (gm.Mood > gm.MoodSafeMax) { characterSprite.color = highMoodColor; isAccelerated = true; }
        else { characterSprite.color = calmMoodColor; isAccelerated = false; }

        if (!isAccelerated) characterSprite.transform.localPosition = basePosition;
    }

    void Update()
    {
        if (!isAccelerated || characterSprite == null) return;
        float jx = (Mathf.PerlinNoise(Time.time * accelJitterSpeed, 0f) - 0.5f) * 2f * accelJitterAmount;
        float jy = (Mathf.PerlinNoise(0f, Time.time * accelJitterSpeed) - 0.5f) * 2f * accelJitterAmount;
        characterSprite.transform.localPosition = basePosition + new Vector3(jx, jy, 0f);
    }

    /// <summary>
    /// Plays the activity animation and calls onComplete when finished.
    /// </summary>
    public void PlayActivity(ActivityData activity, Action onComplete)
    {
        if (activity != null) AudioManager.Instance?.PlaySfx(activity.Sfx);

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
