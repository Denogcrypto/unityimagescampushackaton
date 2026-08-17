using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Orquestador delgado: arma los sistemas (Stat/Energy/Week/DayCycle/Resolver)
/// en Awake y expone la API que la UI necesita. La lógica de reglas vive en
/// cada sistema — acá solo se decide CUÁNDO llamar a cada uno y se corre la
/// corrutina de ActivityResolver (necesita un MonoBehaviour).
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Config (asignar los ScriptableObject)")]
    [SerializeField] private GameConfig config;
    [SerializeField] private TherapistLines therapistLines;

    [Header("Actividades (las 9 del brief)")]
    [SerializeField] private List<ActivityData> allActivities = new List<ActivityData>();

    public GameConfig Config => config;
    public IReadOnlyList<ActivityData> AllActivities => allActivities;

    private StatSystem stats;
    private EnergySystem energy;
    private WeekSummary weekSummary;
    private DayCycle dayCycle;
    private ActivityResolver resolver;

    private int actionsUsedToday;

    public int CurrentDay => dayCycle.CurrentDay;
    public int TotalDias => config.TotalDias;
    public float Energy => energy.Current;
    public int ActionsUsedToday => actionsUsedToday;
    public bool IsBusy => resolver.IsBusy;
    public bool IsWeekEnded => dayCycle.WeekEnded;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        stats = new StatSystem(config);
        energy = new EnergySystem(config);
        weekSummary = new WeekSummary(config, therapistLines);
        dayCycle = new DayCycle(config, stats, energy, weekSummary);
        resolver = new ActivityResolver(config, stats, energy);
    }

    void Start()
    {
        dayCycle.StartFirstDay();
    }

    /// Reinicia la partida entera al día 1 (pedido explícito del usuario —
    /// el brief marca "reinicio de partida" fuera de scope, pero acá se
    /// habilita desde la pantalla de terapeuta). Recrea los sistemas desde
    /// cero, igual que Awake, así no queda estado viejo (fluctuación
    /// deshabilitada por debug, diasRegulado acumulado, etc.).
    public void ResetGame()
    {
        stats = new StatSystem(config);
        energy = new EnergySystem(config);
        weekSummary = new WeekSummary(config, therapistLines);
        dayCycle = new DayCycle(config, stats, energy, weekSummary);
        resolver = new ActivityResolver(config, stats, energy);
        actionsUsedToday = 0;

        dayCycle.StartFirstDay();
    }

    public float GetStat(StatId id) => stats.Get(id);
    public Zone GetStatZone(StatId id) => stats.GetZone(id);
    public float GetStatDistance(StatId id) => stats.Distance(id);
    public bool IsCriticalStatAboveZone(StatId id) => stats.IsCloserToUpperBorder(id);
    public StatId GetCriticalStat() => stats.GetCriticalStat();

    /// S5 paso 1: solo valida partida activa, acciones < tope, energía > 0.
    /// El costo real (estrella/calavera) no se conoce hasta resolver la
    /// actividad, así que no se puede — ni se debe, ver S9 — precalcular acá.
    public bool CanRequestActivity(ActivityData activity) =>
        !dayCycle.WeekEnded && resolver.CanResolve(activity, actionsUsedToday);

    public void RequestActivity(ActivityData activity)
    {
        if (!CanRequestActivity(activity)) return;
        StartCoroutine(resolver.Resolve(activity, OnActivityResolved));
    }

    /// Botón "Descansar": cierre voluntario del día, en cualquier momento.
    public void RequestDescansar()
    {
        if (resolver.IsBusy || dayCycle.WeekEnded) return;
        CloseDay(DayCloseReason.Voluntario);
    }

    void OnActivityResolved()
    {
        actionsUsedToday++;
        RecomputeMoodAndCritical();

        if (energy.Current <= 0f)
            CloseDay(DayCloseReason.SinEnergia);
        else if (actionsUsedToday >= config.TopeAcciones)
            // Pregunta abierta (no cubierta por el brief): llegar al tope sin
            // haber tocado "Descansar" no es ni un cierre voluntario clásico
            // ni "sin energía" literal. Uso Voluntario como default más chico
            // porque el jugador sí llegó a jugar sus 4 acciones — señalado en
            // el resumen de esta sesión para que diseño lo confirme.
            CloseDay(DayCloseReason.Voluntario);
    }

    void CloseDay(DayCloseReason reason)
    {
        bool alcanzoTope = actionsUsedToday >= config.TopeAcciones;
        bool accionesDisponibles = actionsUsedToday < config.TopeAcciones;
        dayCycle.CloseDay(reason, alcanzoTope, accionesDisponibles);
        actionsUsedToday = 0;
    }

    void RecomputeMoodAndCritical()
    {
        var (raw, state) = MoodSystem.Compute(
            stats.Get(StatId.VidaSocial), stats.Get(StatId.Autoestima), stats.Get(StatId.ActividadFisica), config);
        GameEvents.RaiseMoodComputed(state, Mathf.RoundToInt(raw));

        // 4.9: el ícono de stat crítico no se muestra el día 1. DayCycle.
        // StartFirstDay() ya respeta esto al arrancar el día, pero este
        // método también corre después de CADA actividad resuelta (no solo
        // al inicio de día) — sin este chequeo, la nube de pensamiento
        // mostraba el ícono crítico apenas se resolvía la primera acción
        // del día 1, aunque nadie lo hubiera disparado todavía.
        if (dayCycle.CurrentDay > 1)
            GameEvents.RaiseCriticalStatChanged(stats.GetCriticalStat());
    }

    // ─── Debug helpers (usados por DebugPanel, S14) ────────────────
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public void Debug_SetStat(StatId id, float value) => stats.Apply(id, value - stats.Get(id));
    public void Debug_SetEnergy(float value) => energy.Spend(energy.Current - value);
    public void Debug_ForceCloseDay() => RequestDescansar();
    public void Debug_JumpToDay(int day) => dayCycle.JumpToDay(day);

    public bool Debug_FluctuationEnabled
    {
        get => dayCycle.FluctuationEnabled;
        set => dayCycle.FluctuationEnabled = value;
    }

    /// Fuerza los 3 stats al mismo valor representativo del MoodState pedido
    /// (con dispersión 0, ánimo = ese valor exacto).
    public void Debug_ForceMood(MoodState state)
    {
        float value = state switch
        {
            MoodState.Depresivo => 10f,
            MoodState.Triste => 30f,
            MoodState.Neutral => 50f,
            MoodState.Feliz => 70f,
            MoodState.Alterado => 90f,
            _ => 50f,
        };
        Debug_SetStat(StatId.VidaSocial, value);
        Debug_SetStat(StatId.Autoestima, value);
        Debug_SetStat(StatId.ActividadFisica, value);
        RecomputeMoodAndCritical();
    }
#endif
}
