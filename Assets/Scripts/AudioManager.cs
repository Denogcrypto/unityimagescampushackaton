using UnityEngine;

/// <summary>
/// Minimal SFX/music wrapper (S13 placeholder). PlaySfx is already wired from
/// CharacterAnimator.PlayActivity — assign AudioClips on ActivityData once the
/// designer delivers them and sound starts playing with no further code changes.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Music")]
    [SerializeField] private AudioClip musicLoop;

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
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }
}
