using System;

/// <summary>
/// Static event bus (section 5). Lógica y UI nunca se llaman directo — la UI
/// se suscribe acá, los sistemas de Core disparan estos eventos.
/// </summary>
public static class GameEvents
{
    public static event Action<int, int> OnDayStarted;              // (dia, totalDias)
    public static event Action<MoodState, int> OnMoodComputed;      // (estado, valorCrudo)
    public static event Action<StatId> OnCriticalStatChanged;
    public static event Action<StatId, int, int> OnStatChanged;     // (stat, valorNuevo, delta)
    public static event Action<int, int> OnEnergyChanged;           // (valorNuevo, delta)
    public static event Action<ActivityData, ActivityResult> OnActivityResolved;
    public static event Action<DayCloseReason> OnDayClosed;
    public static event Action<WeekReport> OnWeekEnded;
    public static event Action<ActivityCategory> OnBackgroundChangeRequested;

    public static void RaiseDayStarted(int dia, int totalDias) => OnDayStarted?.Invoke(dia, totalDias);
    public static void RaiseMoodComputed(MoodState estado, int valorCrudo) => OnMoodComputed?.Invoke(estado, valorCrudo);
    public static void RaiseCriticalStatChanged(StatId stat) => OnCriticalStatChanged?.Invoke(stat);
    public static void RaiseStatChanged(StatId stat, int valorNuevo, int delta) => OnStatChanged?.Invoke(stat, valorNuevo, delta);
    public static void RaiseEnergyChanged(int valorNuevo, int delta) => OnEnergyChanged?.Invoke(valorNuevo, delta);
    public static void RaiseActivityResolved(ActivityData activity, ActivityResult result) => OnActivityResolved?.Invoke(activity, result);
    public static void RaiseDayClosed(DayCloseReason reason) => OnDayClosed?.Invoke(reason);
    public static void RaiseWeekEnded(WeekReport report) => OnWeekEnded?.Invoke(report);
    public static void RaiseBackgroundChangeRequested(ActivityCategory category) => OnBackgroundChangeRequested?.Invoke(category);
}
