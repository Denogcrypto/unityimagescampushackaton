using UnityEngine;

/// <summary>
/// One activity out of the 3×3 matrix (categoría × impacto). Delta y costos
/// de energía NO se guardan acá — salen de GameConfig según Impact, así que
/// balancear el juego es tocar un solo asset (4.3, S1).
/// </summary>
[CreateAssetMenu(fileName = "NewActivity", menuName = "Tamagotchi/Activity Data")]
public class ActivityData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string displayName;
    [SerializeField] private ActivityCategory category; // determina el stat afectado
    [SerializeField] private ImpactTier impact;          // determina delta y costos (ver GameConfig)

    [Header("Visuals")]
    [SerializeField] private Sprite icon;
    [SerializeField] private string animationTrigger;
    [SerializeField] private AudioClip sfx;

    public string DisplayName => displayName;
    public ActivityCategory Category => category;
    public ImpactTier Impact => impact;
    public Sprite Icon => icon;
    public string AnimationTrigger => animationTrigger;
    public AudioClip Sfx => sfx;

    /// Ligado 1 a 1 según 4.1: Casa→Autoestima, Exterior→ActividadFisica, Social→VidaSocial.
    public StatId AffectedStat => category switch
    {
        ActivityCategory.Casa => StatId.Autoestima,
        ActivityCategory.Exterior => StatId.ActividadFisica,
        ActivityCategory.Social => StatId.VidaSocial,
        _ => StatId.Autoestima,
    };
}
