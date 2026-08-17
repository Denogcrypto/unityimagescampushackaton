using UnityEngine;

public enum RiskTier { Low, High }
public enum ActivityCategory { Activities, Park, Social, Special }

[CreateAssetMenu(fileName = "NewActivity", menuName = "Tamagotchi/Activity Data")]
public class ActivityData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string activityName;
    [SerializeField] private ActivityCategory category;
    [SerializeField] private RiskTier riskTier;

    [Header("Energy Cost")]
    [SerializeField] private float energyCostBase;       // when willing
    // energyCostBase * 1.5f when NOT willing (fatigue >= threshold)

    [Header("Mood Delta")]
    [SerializeField] private float moodDeltaBase;         // when willing
    // moodDeltaBase * 0.4f when NOT willing

    [Header("Fatigue")]
    [SerializeField] private float fatiguePerUse = 30f;
    [SerializeField] private float fatigueRecoveryPerDay = 15f;
    [SerializeField] private float fatigueThreshold = 60f; // >= this → not willing

    [Header("Special (Descansar only)")]
    [SerializeField] private bool isRestActivity = false;
    [SerializeField] private float restEnergyRecovery = 20f;         // energy gained
    [SerializeField] private float restMoodPenaltyConsecutive = -3f; // applied if used 2 days in a row

    [Header("Visuals")]
    [SerializeField] private Sprite activityIcon;
    [SerializeField] [TextArea] private string displayDescription;

    // Read-only outside this asset — consumers should never be able to mutate
    // a shared ScriptableObject's config at runtime.
    public string ActivityName => activityName;
    public ActivityCategory Category => category;
    public RiskTier RiskTier => riskTier;
    public float EnergyCostBase => energyCostBase;
    public float MoodDeltaBase => moodDeltaBase;
    public float FatiguePerUse => fatiguePerUse;
    public float FatigueRecoveryPerDay => fatigueRecoveryPerDay;
    public float FatigueThreshold => fatigueThreshold;
    public bool IsRestActivity => isRestActivity;
    public float RestEnergyRecovery => restEnergyRecovery;
    public float RestMoodPenaltyConsecutive => restMoodPenaltyConsecutive;
    public Sprite ActivityIcon => activityIcon;
    public string DisplayDescription => displayDescription;
}
