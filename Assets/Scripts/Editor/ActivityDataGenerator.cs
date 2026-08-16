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
            energyCost: 15f, energyCostUnwilling: 23f,
            moodDelta: 6f, moodDeltaUnwilling: -4f,
            fatiguePerUse: 30f, fatigueRecovery: 15f, threshold: 60f,
            description: "Bajo impacto · Lento pero seguro");

        CreateActivity("Pasear",
            category: ActivityCategory.Park,
            risk: RiskTier.Low,
            energyCost: 13f, energyCostUnwilling: 19f,
            moodDelta: 5f, moodDeltaUnwilling: -3f,
            fatiguePerUse: 30f, fatigueRecovery: 15f, threshold: 60f,
            description: "Bajo impacto · Lento pero seguro");

        CreateActivity("Ejercicio Intenso",
            category: ActivityCategory.Activities,
            risk: RiskTier.High,
            energyCost: 38f, energyCostUnwilling: 53f,
            moodDelta: 18f, moodDeltaUnwilling: -10f,
            fatiguePerUse: 30f, fatigueRecovery: 15f, threshold: 60f,
            description: "Alto impacto · Corrección rápida, riesgo de sobrepaso");

        CreateActivity("Visitar Amigos",
            category: ActivityCategory.Social,
            risk: RiskTier.High,
            energyCost: 32f, energyCostUnwilling: 44f,
            moodDelta: 20f, moodDeltaUnwilling: -12f,
            fatiguePerUse: 30f, fatigueRecovery: 15f, threshold: 60f,
            description: "Alto impacto · Corrección rápida, riesgo de sobrepaso");

        // Descansar (special)
        var rest = ScriptableObject.CreateInstance<ActivityData>();
        rest.ActivityName = "Descansar";
        rest.Category = ActivityCategory.Special;
        rest.RiskTier = RiskTier.Low;
        rest.IsRestActivity = true;
        rest.DisplayDescription = "Especial · Termina el día. Penaliza -1 de ánimo cada 10 de energía sin gastar.";
        SaveAsset(rest, "Descansar");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[ActivityDataGenerator] 5 activities created in {OutputPath}");
        EditorUtility.DisplayDialog("Tamagotchi", "5 actividades generadas en " + OutputPath, "OK");
    }

    static void CreateActivity(string name, ActivityCategory category, RiskTier risk,
        float energyCost, float energyCostUnwilling,
        float moodDelta, float moodDeltaUnwilling,
        float fatiguePerUse, float fatigueRecovery, float threshold,
        string description)
    {
        var asset = ScriptableObject.CreateInstance<ActivityData>();
        asset.ActivityName = name;
        asset.Category = category;
        asset.RiskTier = risk;
        asset.EnergyCostBase = energyCost;
        asset.EnergyCostUnwilling = energyCostUnwilling;
        asset.MoodDeltaBase = moodDelta;
        asset.MoodDeltaUnwilling = moodDeltaUnwilling;
        asset.FatiguePerUse = fatiguePerUse;
        asset.FatigueRecoveryPerDay = fatigueRecovery;
        asset.FatigueThreshold = threshold;
        asset.DisplayDescription = description;
        SaveAsset(asset, name.Replace(" ", "_"));
    }

    static void SaveAsset(ActivityData asset, string fileName)
    {
        string path = $"{OutputPath}/{fileName}.asset";
        AssetDatabase.CreateAsset(asset, path);
    }
}
