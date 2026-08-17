using System.Collections.Generic;
using UnityEngine;

public enum Zone { Bajo, Regulado, Alto }

/// <summary>
/// Mantiene los 3 stats (4.1), calcula zonas/distancias, aplica deltas con
/// clamp [0,100], decaimiento (4.6) y fluctuación nocturna (4.7). No dispara
/// eventos de ánimo/crítico por su cuenta — eso lo recalcula quien la use
/// (DayCycle/GameManager), así S2 se mantiene enfocado solo en los stats.
/// </summary>
public class StatSystem
{
    private static readonly StatId[] AllStats = { StatId.VidaSocial, StatId.Autoestima, StatId.ActividadFisica };

    private readonly GameConfig config;
    private readonly Dictionary<StatId, float> values = new Dictionary<StatId, float>();

    public StatSystem(GameConfig config)
    {
        this.config = config;
        foreach (var id in AllStats) values[id] = config.StatInicial(id);
    }

    public float Get(StatId id) => values[id];

    public void Apply(StatId id, float delta)
    {
        float before = values[id];
        float after = Mathf.Clamp(before + delta, 0f, 100f);
        values[id] = after;
        GameEvents.RaiseStatChanged(id, Mathf.RoundToInt(after), Mathf.RoundToInt(after - before));
    }

    public Zone GetZone(StatId id)
    {
        float v = values[id];
        if (v < config.ZonaBajoMax) return Zone.Bajo;
        if (v > config.ZonaAltoMin) return Zone.Alto;
        return Zone.Regulado;
    }

    /// distancia(stat) — 4.4: 0 dentro de la zona, si no la distancia al borde más cercano.
    public float Distance(StatId id)
    {
        float v = values[id];
        if (v < config.ZonaBajoMax) return config.ZonaBajoMax - v;
        if (v > config.ZonaAltoMin) return v - config.ZonaAltoMin;
        return 0f;
    }

    /// margen(stat) — 4.9, solo tiene sentido dentro de la zona Regulado.
    public float Margin(StatId id)
    {
        float v = values[id];
        return Mathf.Min(v - config.ZonaBajoMax, config.ZonaAltoMin - v);
    }

    /// True si el stat está más cerca del borde superior (para la flecha de 4.9).
    public bool IsCloserToUpperBorder(StatId id)
    {
        float v = values[id];
        if (v > config.ZonaAltoMin) return true;
        if (v < config.ZonaBajoMax) return false;
        return (config.ZonaAltoMin - v) <= (v - config.ZonaBajoMax);
    }

    public void ApplyDecay()
    {
        foreach (var id in AllStats) Apply(id, -config.DecaimientoDiario);
    }

    /// 4.7 — solo entre los que quedaron Regulado, un único stat, dirección 50/50.
    /// Devuelve el stat afectado, o null si ninguno estaba Regulado.
    public StatId? ApplyNightlyFluctuation()
    {
        var regulated = new List<StatId>();
        foreach (var id in AllStats)
            if (GetZone(id) == Zone.Regulado) regulated.Add(id);

        if (regulated.Count == 0) return null;

        var chosen = regulated[Random.Range(0, regulated.Count)];
        float magnitude = Random.Range(config.FluctuacionMin, config.FluctuacionMax);
        float direction = Random.value < 0.5f ? -1f : 1f;
        Apply(chosen, magnitude * direction);
        return chosen;
    }

    /// 4.9 — mayor distancia gana; si los tres están en 0 (todos regulados), gana
    /// el de menor margen. Empates: orden fijo VidaSocial→Autoestima→ActividadFisica
    /// (ya es el orden de AllStats, así que el primer máximo/mínimo encontrado gana).
    public StatId GetCriticalStat()
    {
        StatId bestByDistance = AllStats[0];
        float bestDistance = -1f;
        foreach (var id in AllStats)
        {
            float d = Distance(id);
            if (d > bestDistance) { bestDistance = d; bestByDistance = id; }
        }
        if (bestDistance > 0f) return bestByDistance;

        StatId bestByMargin = AllStats[0];
        float smallestMargin = float.MaxValue;
        foreach (var id in AllStats)
        {
            float m = Margin(id);
            if (m < smallestMargin) { smallestMargin = m; bestByMargin = id; }
        }
        return bestByMargin;
    }
}
