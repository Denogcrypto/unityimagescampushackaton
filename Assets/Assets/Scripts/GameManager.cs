using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    // ─── Singleton ───────────────────────────────────────────────
    public static GameManager Instance { get; private set; }

    // ─── Config ──────────────────────────────────────────────────
    [Header("Day Settings")]
    public int totalDays = 30;

    [Header("Mood Settings")]
    [Range(0, 100)] public float moodInitial = 50f;
    [Range(0, 100)] public float moodSafeMin = 30f;
    [Range(0, 100)] public float moodSafeMax = 75f;
    public float moodFluctuationMin = -8f;
    public float moodFluctuationMax = 8f;

    [Header("Energy Settings")]
    public float energyRecoveryMin = 40f;
    public float energyRecoveryMax = 100f;

    [Header("Defeat Condition")]
    public int maxUnstableDays = 3;

    [Header("Activities (assign all ActivityData assets)")]
    public List<ActivityData> allActivities = new List<ActivityData>();

    // ─── Runtime State ───────────────────────────────────────────
    public int CurrentDay { get; private set; } = 1;
    public float Mood { get; private set; }
    public float Energy { get; private set; }
    public int UnstableDaysStreak { get; private set; } = 0;
    public bool IsGameOver { get; private set; } = false;
    public bool IsVictory { get; private set; } = false;

    // Fatigue per activity: key = activity name
    private Dictionary<string, float> fatigueMap = new Dictionary<string, float>();

    // Track rest used yesterday (for consecutive penalty)
    private bool restUsedYesterday = false;
    private bool restUsedToday = false;

    // Activities used this day (for fatigue accumulation)
    private HashSet<string> activitiesUsedToday = new HashSet<string>();

    // ─── Events ──────────────────────────────────────────────────
    public UnityEvent OnDayStarted = new UnityEvent();
    public UnityEvent OnDayEnded = new UnityEvent();
    public UnityEvent OnActivityApplied = new UnityEvent();
    public UnityEvent OnGameOver = new UnityEvent();
    public UnityEvent OnVictory = new UnityEvent();
    public UnityEvent OnStatsChanged = new UnityEvent();

    // ─── Init ────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Initialize fatigue for all activities
        foreach (var act in allActivities)
            if (act != null)
                fatigueMap[act.activityName] = 0f;
    }

    void Start()
    {
        Mood = moodInitial;
        Energy = Random.Range(energyRecoveryMin, energyRecoveryMax);
        Energy = Mathf.Clamp(Energy, 0f, 100f);
        OnDayStarted.Invoke();
        OnStatsChanged.Invoke();
    }

    // ─── Public Queries ──────────────────────────────────────────

    /// Returns current fatigue for an activity (0 if unknown).
    public float GetFatigue(ActivityData activity)
    {
        if (activity == null) return 0f;
        return fatigueMap.TryGetValue(activity.activityName, out float f) ? f : 0f;
    }

    /// Returns true if the character is NOT willing to do the activity.
    public bool IsNotWilling(ActivityData activity)
    {
        if (activity == null || activity.isRestActivity) return false;
        return GetFatigue(activity) >= activity.fatigueThreshold;
    }

    /// Effective energy cost considering willingness.
    public float GetEffectiveEnergyCost(ActivityData activity)
    {
        if (activity == null) return 0f;
        if (activity.isRestActivity) return 0f;
        float cost = activity.energyCostBase;
        if (IsNotWilling(activity)) cost *= 1.5f;
        return cost;
    }

    /// Effective mood delta considering willingness.
    public float GetEffectiveMoodDelta(ActivityData activity)
    {
        if (activity == null) return 0f;
        if (activity.isRestActivity) return 0f;
        float delta = activity.moodDeltaBase;
        if (IsNotWilling(activity)) delta *= 0.4f;
        return delta;
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

        if (activity.isRestActivity)
        {
            // Descansar: recover energy, end the day
            float moodChange = 0f;
            if (restUsedYesterday) moodChange = activity.restMoodPenaltyConsecutive;

            Energy = Mathf.Clamp(Energy + activity.restEnergyRecovery, 0f, 100f);
            Mood = Mathf.Clamp(Mood + moodChange, 0f, 100f);

            restUsedToday = true;
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
        if (!fatigueMap.ContainsKey(activity.activityName))
            fatigueMap[activity.activityName] = 0f;
        fatigueMap[activity.activityName] = Mathf.Min(
            fatigueMap[activity.activityName] + activity.fatiguePerUse, 100f);

        activitiesUsedToday.Add(activity.activityName);

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
            if (act == null || act.isRestActivity) continue;
            if (!activitiesUsedToday.Contains(act.activityName))
            {
                float key = fatigueMap.TryGetValue(act.activityName, out float f) ? f : 0f;
                fatigueMap[act.activityName] = Mathf.Max(0f, key - act.fatigueRecoveryPerDay);
            }
        }

        activitiesUsedToday.Clear();
        restUsedYesterday = restUsedToday;
        restUsedToday = false;

        // Energy recovery
        float recoveredEnergy = Random.Range(energyRecoveryMin, energyRecoveryMax);
        Energy = Mathf.Clamp(recoveredEnergy, 0f, 100f);

        // Mood fluctuation
        float fluctuation = Random.Range(moodFluctuationMin, moodFluctuationMax);
        Mood = Mathf.Clamp(Mood + fluctuation, 0f, 100f);

        OnDayStarted.Invoke();
        OnStatsChanged.Invoke();
    }

    // ─── Debug Helpers ───────────────────────────────────────────
#if UNITY_EDITOR
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
#endif
}
