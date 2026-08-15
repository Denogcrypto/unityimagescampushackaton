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
    [SerializeField] private float moodDeltaBase;        // when willing
    // moodDeltaBase * 0.4f when NOT willing

    [Header("Fatigue")]
    [SerializeField] private float fatiguePerUse = 30f;
    [SerializeField] private float fatigueRecoveryPerDay = 15f;
    [SerializeField] private float fatigueThreshold = 60f;   // >= this → not willing

    [Header("Special (Descansar only)")]
    [SerializeField] private bool isRestActivity = false;
    [SerializeField] private float restEnergyRecovery = 20f;     // energy gained
    [SerializeField] private float restMoodPenaltyConsecutive = -3f; // applied if used 2 days in a row

    [Header("Visuals")]
    [SerializeField] private Sprite activityIcon;
    [SerializeField] [TextArea] private string displayDescription;

    public string ActivityName { get => activityName; set => activityName = value; }
    public ActivityCategory Category { get => category; set => category = value; }
    public RiskTier RiskTier { get => riskTier; set => riskTier = value; }
    public float EnergyCostBase { get => energyCostBase; set => energyCostBase = value; }
    public float MoodDeltaBase { get => moodDeltaBase; set => moodDeltaBase = value; }
    public float FatiguePerUse { get => fatiguePerUse; set => fatiguePerUse = value; }
    public float FatigueRecoveryPerDay { get => fatigueRecoveryPerDay; set => fatigueRecoveryPerDay = value; }
    public float FatigueThreshold { get => fatigueThreshold; set => fatigueThreshold = value; }
    public bool IsRestActivity { get => isRestActivity; set => isRestActivity = value; }
    public float RestEnergyRecovery { get => restEnergyRecovery; set => restEnergyRecovery = value; }
    public float RestMoodPenaltyConsecutive { get => restMoodPenaltyConsecutive; set => restMoodPenaltyConsecutive = value; }
    public Sprite ActivityIcon { get => activityIcon; set => activityIcon = value; }
    public string DisplayDescription { get => displayDescription; set => displayDescription = value; }
}
