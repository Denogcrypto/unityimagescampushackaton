using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Minimal SFX/music singleton. Background music loops from Start; every
/// Button in the scene (active or not) gets the click SFX wired automatically,
/// so new buttons don't need manual hookup. PlaySfx is also available for
/// other callers (e.g. CharacterAnimator.PlayActivity) once activity clips exist.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Music")]
    [SerializeField] private AudioClip musicLoop;

    [Header("UI")]
    [SerializeField] private AudioClip buttonClickClip;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
        }
    }

    void Start()
    {
        if (musicLoop != null)
        {
            musicSource.clip = musicLoop;
            musicSource.Play();
        }

        HookButtonClickSfx();
    }

    void HookButtonClickSfx()
    {
        var buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var button in buttons)
            button.onClick.AddListener(PlayButtonClick);
    }

    public void PlayButtonClick() => PlaySfx(buttonClickClip);

    public void PlaySfx(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }
}
