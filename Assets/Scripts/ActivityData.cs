using UnityEngine;

public enum RiskTier { Low, High }
public enum ActivityCategory { Activities, Park, Social, Special }

[CreateAssetMenu(fileName = "NewActivity", menuName = "Tamagotchi/Activity Data")]
public class ActivityData : ScriptableObject
{
    [Header("Identity")]
    public string activityName;
    public ActivityCategory category;
    public RiskTier riskTier;

    [Header("Energy Cost")]
    public float energyCostBase;       // when willing
    // energyCostBase * 1.5f when NOT willing (fatigue >= threshold)

    [Header("Mood Delta")]
    public float moodDeltaBase;        // when willing
    // moodDeltaBase * 0.4f when NOT willing

    [Header("Fatigue")]
    public float fatiguePerUse = 30f;
    public float fatigueRecoveryPerDay = 15f;
    public float fatigueThreshold = 60f;   // >= this → not willing

    [Header("Special (Descansar only)")]
    public bool isRestActivity = false;
    public float restEnergyRecovery = 20f;     // energy gained
    public float restMoodPenaltyConsecutive = -3f; // applied if used 2 days in a row

    [Header("Visuals")]
    public Sprite activityIcon;
    [TextArea] public string displayDescription;
}
