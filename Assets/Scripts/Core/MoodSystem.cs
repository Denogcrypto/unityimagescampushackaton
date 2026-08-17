using UnityEngine;

/// <summary>
/// Función pura (4.2, S3): recibe los 3 stats, devuelve el ánimo crudo y su
/// MoodState. No guarda estado propio — es la única fuente de ánimo del juego.
/// La tabla de "clasificación por combinación de stats" del GDD NO se usa acá
/// a propósito (ver 4.2): manda siempre la fórmula.
/// </summary>
public static class MoodSystem
{
    public static (float raw, MoodState state) Compute(float vidaSocial, float autoestima, float actividadFisica, GameConfig config)
    {
        float promedio = (vidaSocial + autoestima + actividadFisica) / 3f;
        float max = Mathf.Max(vidaSocial, Mathf.Max(autoestima, actividadFisica));
        float min = Mathf.Min(vidaSocial, Mathf.Min(autoestima, actividadFisica));
        float dispersion = max - min;

        float raw = Mathf.Clamp(promedio - dispersion * config.DispersionFactor, 0f, 100f);
        return (raw, ClassifyState(raw, config));
    }

    // Bordes [mínimo, máximo) — el 40 es Neutral, no Triste.
    static MoodState ClassifyState(float raw, GameConfig config)
    {
        if (raw < config.AnimoDepresivoMax) return MoodState.Depresivo;
        if (raw < config.AnimoTristeMax) return MoodState.Triste;
        if (raw < config.AnimoNeutralMax) return MoodState.Neutral;
        if (raw < config.AnimoFelizMax) return MoodState.Feliz;
        return MoodState.Alterado;
    }
}
