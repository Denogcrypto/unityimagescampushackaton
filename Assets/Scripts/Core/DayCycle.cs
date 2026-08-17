using UnityEngine;

/// <summary>
/// Implementa literalmente el orden de 4.8. GameManager decide CUÁNDO cerrar
/// el día (tope de acciones, energía en 0, o Descansar) y le pasa acá el
/// motivo — esta clase solo garantiza que los pasos pasen en el orden correcto.
/// </summary>
public class DayCycle
{
    private readonly GameConfig config;
    private readonly StatSystem stats;
    private readonly EnergySystem energy;
    private readonly WeekSummary weekSummary;

    public int CurrentDay { get; private set; } = 1;
    public bool WeekEnded { get; private set; }

    /// Debug-only (S14): permite apagar la fluctuación nocturna para probar balance sin ruido.
    public bool FluctuationEnabled { get; set; } = true;

    public DayCycle(GameConfig config, StatSystem stats, EnergySystem energy, WeekSummary weekSummary)
    {
        this.config = config;
        this.stats = stats;
        this.energy = energy;
        this.weekSummary = weekSummary;
    }

    public void StartFirstDay()
    {
        var (raw, state) = MoodSystem.Compute(
            stats.Get(StatId.VidaSocial), stats.Get(StatId.Autoestima), stats.Get(StatId.ActividadFisica), config);
        GameEvents.RaiseMoodComputed(state, Mathf.RoundToInt(raw));
        energy.InitializeDay1(state);
        // Día 1: sin ícono de stat crítico (4.9) — no se emite OnCriticalStatChanged.
        GameEvents.RaiseDayStarted(CurrentDay, config.TotalDias);
    }

    /// <param name="alcanzoTope">Llegó a las acciones máximas del día.</param>
    /// <param name="accionesDisponibles">Le quedaban acciones sin usar al cerrar.</param>
    public void CloseDay(DayCloseReason reason, bool alcanzoTope, bool accionesDisponibles)
    {
        // 1. Registrar zona ANTES del decaimiento.
        weekSummary.RegisterDay(stats);

        // 2. sobranteEfectivo (4.5).
        float sobranteEfectivo = 0f;
        if (reason == DayCloseReason.Voluntario && !alcanzoTope && accionesDisponibles)
            sobranteEfectivo = energy.Current;

        // 3. Decaimiento −10 a los tres stats.
        stats.ApplyDecay();

        // 4. Fluctuación nocturna (solo entre los Regulado).
        if (FluctuationEnabled) stats.ApplyNightlyFluctuation();

        // 5. Cierre de día (S10 lo escucha).
        GameEvents.RaiseDayClosed(reason);

        if (CurrentDay >= config.TotalDias)
        {
            WeekEnded = true;
            GameEvents.RaiseWeekEnded(weekSummary.BuildReport());
            return;
        }

        CurrentDay++;

        // 6. Ánimo del nuevo día.
        var (raw, state) = MoodSystem.Compute(
            stats.Get(StatId.VidaSocial), stats.Get(StatId.Autoestima), stats.Get(StatId.ActividadFisica), config);
        GameEvents.RaiseMoodComputed(state, Mathf.RoundToInt(raw));

        // 7. Energía del nuevo día.
        energy.RecoverForNewDay(state, sobranteEfectivo);

        // 8. Stat crítico → nube de pensamiento.
        GameEvents.RaiseCriticalStatChanged(stats.GetCriticalStat());

        GameEvents.RaiseDayStarted(CurrentDay, config.TotalDias);
    }

    /// Debug-only (S14): saltar directo al día N sin correr el cierre de los días intermedios.
    public void JumpToDay(int day)
    {
        CurrentDay = Mathf.Clamp(day, 1, config.TotalDias);
        GameEvents.RaiseDayStarted(CurrentDay, config.TotalDias);
    }
}
