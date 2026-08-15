using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// Editor utility: creates the 5 default ActivityData ScriptableObjects
/// from the design document. Run via menu: Tamagotchi > Generate Default Activities
/// </summary>
public static class ActivityDataGenerator
{
    private const string OutputPath = "Assets/ScriptableObjects/Activities";

    [MenuItem("Tamagotchi/Generate Default Activities")]
    public static void GenerateAll()
    {
        if (!Directory.Exists(OutputPath))
            Directory.CreateDirectory(OutputPath);

        CreateActivity("Leer",
            category: ActivityCategory.Activities,
            risk: RiskTier.Low,
            energyCost: 15f, moodDelta: 6f,
            fatiguePerUse: 30f, fatigueRecovery: 15f, threshold: 60f,
            description: "Bajo impacto · Lento pero seguro");

        CreateActivity("Pasear",
            category: ActivityCategory.Park,
            risk: RiskTier.Low,
            energyCost: 13f, moodDelta: 5f,
            fatiguePerUse: 30f, fatigueRecovery: 15f, threshold: 60f,
            description: "Bajo impacto · Lento pero seguro");

        CreateActivity("Ejercicio Intenso",
            category: ActivityCategory.Activities,
            risk: RiskTier.High,
            energyCost: 38f, moodDelta: 18f,
            fatiguePerUse: 30f, fatigueRecovery: 15f, threshold: 60f,
            description: "Alto impacto · Corrección rápida, riesgo de sobrepaso");

        CreateActivity("Visitar Amigos",
            category: ActivityCategory.Social,
            risk: RiskTier.High,
            energyCost: 32f, moodDelta: 20f,
            fatiguePerUse: 30f, fatigueRecovery: 15f, threshold: 60f,
            description: "Alto impacto · Corrección rápida, riesgo de sobrepaso");

        // Descansar (special)
        var rest = ScriptableObject.CreateInstance<ActivityData>();
        rest.activityName = "Descansar";
        rest.category = ActivityCategory.Special;
        rest.riskTier = RiskTier.Low;
        rest.isRestActivity = true;
        rest.restEnergyRecovery = 20f;
        rest.restMoodPenaltyConsecutive = -3f;
        rest.displayDescription = "Especial · Recupera +20 energía y termina el día. Penalización si se usa 2 días seguidos.";
        SaveAsset(rest, "Descansar");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[ActivityDataGenerator] 5 activities created in {OutputPath}");
        EditorUtility.DisplayDialog("Tamagotchi", "5 actividades generadas en " + OutputPath, "OK");
    }

    static void CreateActivity(string name, ActivityCategory category, RiskTier risk,
        float energyCost, float moodDelta,
        float fatiguePerUse, float fatigueRecovery, float threshold,
        string description)
    {
        var asset = ScriptableObject.CreateInstance<ActivityData>();
        asset.activityName = name;
        asset.category = category;
        asset.riskTier = risk;
        asset.energyCostBase = energyCost;
        asset.moodDeltaBase = moodDelta;
        asset.fatiguePerUse = fatiguePerUse;
        asset.fatigueRecoveryPerDay = fatigueRecovery;
        asset.fatigueThreshold = threshold;
        asset.displayDescription = description;
        SaveAsset(asset, name.Replace(" ", "_"));
    }

    static void SaveAsset(ActivityData asset, string fileName)
    {
        string path = $"{OutputPath}/{fileName}.asset";
        AssetDatabase.CreateAsset(asset, path);
    }
}
