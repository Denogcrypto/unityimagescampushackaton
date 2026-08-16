using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    // ─── Singleton ───────────────────────────────────────────────
    public static GameManager Instance { get; private set; }

    // ─── Config ──────────────────────────────────────────────────
    [Header("Day Settings")]
    [SerializeField] private int totalDays = 15;

    [Header("Mood Settings")]
    [Range(0, 100)] [SerializeField] private float moodInitial = 50f;
    [Range(0, 100)] [SerializeField] private float moodSafeMin = 30f;
    [Range(0, 100)] [SerializeField] private float moodSafeMax = 75f;
    [SerializeField] private float moodFluctuationMin = -8f;
    [SerializeField] private float moodFluctuationMax = 8f;
    [SerializeField] private float moodFluctuationAmplitudeEnd = 15f; // amplitude on the last day, interpolated from moodFluctuationMax

    [Header("Energy Settings")]
    [SerializeField] private float energyRecoveryMin = 40f;
    [SerializeField] private float energyRecoveryMax = 100f;

    [Header("Rest Settings")]
    [SerializeField] private float restPenaltyPerEnergyUnit = 0.1f; // -1 mood per 10 leftover energy when resting

    [Header("Defeat Condition")]
    [SerializeField] private int maxUnstableDays = 3;

    [Header("Activities (assign all ActivityData assets)")]
    [SerializeField] private List<ActivityData> allActivities = new List<ActivityData>();

    public int TotalDays => totalDays;
    public float MoodSafeMin => moodSafeMin;
    public float MoodSafeMax => moodSafeMax;
    public int MaxUnstableDays => maxUnstableDays;
    public bool DisableRandomFluctuation => disableRandomFluctuation;
    public int LowImpactCount => lowImpactCount;
    public int HighImpactCount => highImpactCount;
    public GameOverReason LastGameOverReason { get; private set; }

    public enum GameOverReason { TooSad, TooHappy }
    public enum PlayArchetype { Stabilizer, Adaptive, ShockSpecialist }

    // ─── Runtime State ───────────────────────────────────────────
    public int CurrentDay { get; private set; } = 1;
    public float Mood { get; private set; }
    public float Energy { get; private set; }
    public int UnstableDaysStreak { get; private set; } = 0;
    public bool IsGameOver { get; private set; } = false;
    public bool IsVictory { get; private set; } = false;

    // Fatigue per activity: key = activity name
    private Dictionary<string, float> fatigueMap = new Dictionary<string, float>();

    // Activities used this day (for fatigue accumulation)
    private HashSet<string> activitiesUsedToday = new HashSet<string>();

    // Archetype tracking (S11): counts of low/high impact activities used across the run
    private int lowImpactCount = 0;
    private int highImpactCount = 0;

    // Debug toggle: disables random mood fluctuation at the start of each day
    private bool disableRandomFluctuation = false;

    // ─── Events ──────────────────────────────────────────────────
    [SerializeField] private UnityEvent onDayStarted = new UnityEvent();
    [SerializeField] private UnityEvent onDayEnded = new UnityEvent();
    [SerializeField] private UnityEvent onActivityApplied = new UnityEvent();
    [SerializeField] private UnityEvent onGameOver = new UnityEvent();
    [SerializeField] private UnityEvent onVictory = new UnityEvent();
    [SerializeField] private UnityEvent onStatsChanged = new UnityEvent();

    public UnityEvent OnDayStarted => onDayStarted;
    public UnityEvent OnDayEnded => onDayEnded;
    public UnityEvent OnActivityApplied => onActivityApplied;
    public UnityEvent OnGameOver => onGameOver;
    public UnityEvent OnVictory => onVictory;
    public UnityEvent OnStatsChanged => onStatsChanged;

    // ─── Init ────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Initialize fatigue for all activities
        foreach (var act in allActivities)
            if (act != null)
                fatigueMap[act.ActivityName] = 0f;
    }

    void Start()
    {
        Mood = moodInitial;
        BeginDay();
    }

    /// Rolls energy and mood fluctuation for the current day and emits the day-started events.
    /// Shared by Start() (day 1) and AdvanceDay()/Debug_JumpToDay() (subsequent days).
    void BeginDay()
    {
        Energy = Mathf.Clamp(Random.Range(energyRecoveryMin, energyRecoveryMax), 0f, 100f);

        if (!disableRandomFluctuation)
        {
            float progress = totalDays > 1 ? (float)(CurrentDay - 1) / (totalDays - 1) : 0f;
            float amplitude = Mathf.Lerp(moodFluctuationMax, moodFluctuationAmplitudeEnd, Mathf.Clamp01(progress));
            float fluctuation = Random.Range(-amplitude, amplitude);
            Mood = Mathf.Clamp(Mood + fluctuation, 0f, 100f);
        }

        OnDayStarted.Invoke();
        OnStatsChanged.Invoke();
    }

    // ─── Public Queries ──────────────────────────────────────────

    /// Returns current fatigue for an activity (0 if unknown).
    public float GetFatigue(ActivityData activity)
    {
        if (activity == null) return 0f;
        return fatigueMap.TryGetValue(activity.ActivityName, out float f) ? f : 0f;
    }

    /// Returns true if the character is NOT willing to do the activity.
    public bool IsNotWilling(ActivityData activity)
    {
        if (activity == null || activity.IsRestActivity) return false;
        return GetFatigue(activity) >= activity.FatigueThreshold;
    }

    /// Effective energy cost considering willingness.
    public float GetEffectiveEnergyCost(ActivityData activity)
    {
        if (activity == null) return 0f;
        if (activity.IsRestActivity) return 0f;
        return IsNotWilling(activity) ? activity.EnergyCostUnwilling : activity.EnergyCostBase;
    }

    /// Effective mood delta considering willingness. Negative when the activity is done
    /// without willingness — this is the game's only source of controlled mood decay.
    public float GetEffectiveMoodDelta(ActivityData activity)
    {
        if (activity == null) return 0f;
        if (activity.IsRestActivity) return 0f;
        return IsNotWilling(activity) ? activity.MoodDeltaUnwilling : activity.MoodDeltaBase;
    }

    /// True if the player can afford the energy cost.
    public bool CanAfford(ActivityData activity)
    {
        if (activity == null) return false;
        return Energy >= GetEffectiveEnergyCost(activity);
    }

    // ─── Core Actions ────────────────────────────────────────────

    /// Called by UIManager / CharacterAnimator after animation finishes.
    public void ApplyActivity(ActivityData activity)
    {
        if (activity == null || IsGameOver || IsVictory) return;

        if (activity.IsRestActivity)
        {
            // Descansar: penalize leftover unspent energy, end the day
            float penalty = Mathf.Floor(Energy * restPenaltyPerEnergyUnit);
            Mood = Mathf.Clamp(Mood - penalty, 0f, 100f);

            OnActivityApplied.Invoke();
            OnStatsChanged.Invoke();
            AdvanceDay();
            return;
        }

        float energyCost = GetEffectiveEnergyCost(activity);
        if (!CanAfford(activity)) return;

        float moodDelta = GetEffectiveMoodDelta(activity);

        Energy = Mathf.Clamp(Energy - energyCost, 0f, 100f);
        Mood = Mathf.Clamp(Mood + moodDelta, 0f, 100f);

        // Apply fatigue
        if (!fatigueMap.ContainsKey(activity.ActivityName))
            fatigueMap[activity.ActivityName] = 0f;
        fatigueMap[activity.ActivityName] = Mathf.Min(
            fatigueMap[activity.ActivityName] + activity.FatiguePerUse, 100f);

        activitiesUsedToday.Add(activity.ActivityName);

        // Archetype tracking (S11)
        if (activity.RiskTier == RiskTier.High) highImpactCount++;
        else lowImpactCount++;

        OnActivityApplied.Invoke();
        OnStatsChanged.Invoke();

        // Auto advance day when energy depleted
        if (Energy <= 0f)
            AdvanceDay();
    }

    /// Explicitly end the current day (called by Descansar or when energy hits 0).
    public void AdvanceDay()
    {
        if (IsGameOver || IsVictory) return;

        // ── End-of-day: check mood zone ──
        bool moodInSafeZone = Mood >= moodSafeMin && Mood <= moodSafeMax;
        if (!moodInSafeZone)
            UnstableDaysStreak++;
        else
            UnstableDaysStreak = 0;

        OnDayEnded.Invoke();
        OnStatsChanged.Invoke();

        // ── Check defeat ──
        if (UnstableDaysStreak >= maxUnstableDays)
        {
            IsGameOver = true;
            LastGameOverReason = Mood < moodSafeMin ? GameOverReason.TooSad : GameOverReason.TooHappy;
            OnGameOver.Invoke();
            return;
        }

        // ── Check victory ──
        if (CurrentDay >= totalDays)
        {
            IsVictory = true;
            OnVictory.Invoke();
            return;
        }

        // ── Prepare next day ──
        CurrentDay++;

        // Fatigue recovery for unused activities
        foreach (var act in allActivities)
        {
            if (act == null || act.IsRestActivity) continue;
            if (!activitiesUsedToday.Contains(act.ActivityName))
            {
                float key = fatigueMap.TryGetValue(act.ActivityName, out float f) ? f : 0f;
                fatigueMap[act.ActivityName] = Mathf.Max(0f, key - act.FatigueRecoveryPerDay);
            }
        }

        activitiesUsedToday.Clear();
        BeginDay();
    }

    /// Returns the play archetype based on the low/high impact activity ratio (S11).
    public PlayArchetype GetArchetype()
    {
        int total = lowImpactCount + highImpactCount;
        if (total == 0) return PlayArchetype.Adaptive;

        float ratio = (float)highImpactCount / total;
        if (ratio < 0.25f) return PlayArchetype.Stabilizer;
        if (ratio > 0.55f) return PlayArchetype.ShockSpecialist;
        return PlayArchetype.Adaptive;
    }

    // ─── Debug Helpers ───────────────────────────────────────────
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [ContextMenu("Debug: +10 Mood")]
    public void Debug_AddMood() { Mood = Mathf.Clamp(Mood + 10f, 0f, 100f); OnStatsChanged.Invoke(); }

    [ContextMenu("Debug: -10 Mood")]
    public void Debug_SubMood() { Mood = Mathf.Clamp(Mood - 10f, 0f, 100f); OnStatsChanged.Invoke(); }

    [ContextMenu("Debug: +20 Energy")]
    public void Debug_AddEnergy() { Energy = Mathf.Clamp(Energy + 20f, 0f, 100f); OnStatsChanged.Invoke(); }

    [ContextMenu("Debug: -20 Energy")]
    public void Debug_SubEnergy() { Energy = Mathf.Clamp(Energy - 20f, 0f, 100f); OnStatsChanged.Invoke(); }

    [ContextMenu("Debug: Force Advance Day")]
    public void Debug_AdvanceDay() { AdvanceDay(); }

    [ContextMenu("Debug: Force Mood to 20 (below safe zone)")]
    public void Debug_MoodDanger() { Mood = 20f; OnStatsChanged.Invoke(); }

    [ContextMenu("Debug: Force Mood to 90 (above safe zone)")]
    public void Debug_MoodOverhappy() { Mood = 90f; OnStatsChanged.Invoke(); }

    /// Jumps directly to a given day: rerolls energy and mood fluctuation without
    /// stepping through every day in between. Does not reset fatigue.
    public void Debug_JumpToDay(int day)
    {
        CurrentDay = Mathf.Clamp(day, 1, totalDays);
        activitiesUsedToday.Clear();
        BeginDay();
    }

    public void Debug_ResetFatigue()
    {
        var keys = new List<string>(fatigueMap.Keys);
        foreach (var key in keys) fatigueMap[key] = 0f;
        OnStatsChanged.Invoke();
    }

    public void Debug_ToggleFluctuation() { disableRandomFluctuation = !disableRandomFluctuation; }
#endif
}
