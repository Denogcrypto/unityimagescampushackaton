using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// Editor utility: crea las 9 ActivityData de la matriz 3×3 (4.3).
/// Menú: Tamagotchi > Generate Default Activities
/// </summary>
public static class ActivityDataGenerator
{
    private const string OutputPath = "Assets/ScriptableObjects/Activities";

    [MenuItem("Tamagotchi/Generate Default Activities")]
    public static void GenerateAll()
    {
        if (!Directory.Exists(OutputPath))
            Directory.CreateDirectory(OutputPath);

        Create("Escuchar música", ActivityCategory.Casa, ImpactTier.Bajo);
        Create("Cocinar", ActivityCategory.Casa, ImpactTier.Medio);
        Create("Ordenar y limpiar", ActivityCategory.Casa, ImpactTier.Alto);

        Create("Tomar sol", ActivityCategory.Exterior, ImpactTier.Bajo);
        Create("Caminar por el parque", ActivityCategory.Exterior, ImpactTier.Medio);
        Create("Ir al gym", ActivityCategory.Exterior, ImpactTier.Alto);

        Create("Juntarse con la familia", ActivityCategory.Social, ImpactTier.Bajo);
        Create("Salir a comer con amigos", ActivityCategory.Social, ImpactTier.Medio);
        Create("Ir a un recital", ActivityCategory.Social, ImpactTier.Alto);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[ActivityDataGenerator] 9 actividades creadas en {OutputPath}");
    }

    static void Create(string name, ActivityCategory category, ImpactTier impact)
    {
        string path = $"{OutputPath}/{name.Replace(" ", "_")}.asset";
        if (AssetDatabase.LoadAssetAtPath<ActivityData>(path) != null) return; // no pisar si ya existe

        var asset = ScriptableObject.CreateInstance<ActivityData>();
        var so = new SerializedObject(asset);
        so.FindProperty("displayName").stringValue = name;
        so.FindProperty("category").enumValueIndex = (int)category;
        so.FindProperty("impact").enumValueIndex = (int)impact;
        so.ApplyModifiedPropertiesWithoutUndo();

        AssetDatabase.CreateAsset(asset, path);
    }
}
