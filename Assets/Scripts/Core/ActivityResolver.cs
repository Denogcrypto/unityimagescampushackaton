using System;
using System.Collections;

/// <summary>
/// Resuelve una actividad en el orden estricto de S5: valida, bloquea input
/// (vía IsBusy), calcula distancia_antes, pide cambio de fondo, corre
/// animación + SFX, aplica el delta, calcula distancia_despues y determina
/// estrella/calavera (4.4, con &lt;=, no &lt;), descuenta el costo, emite el
/// evento de resultado, vuelve el fondo a Casa y desbloquea input.
///
/// Recalcular ánimo/ícono crítico y chequear cierre forzado (pasos 10-11)
/// quedan del lado de GameManager en el callback onComplete: son estado
/// global que no le corresponde a un resolver de una sola actividad.
/// </summary>
public class ActivityResolver
{
    private readonly GameConfig config;
    private readonly StatSystem stats;
    private readonly EnergySystem energy;

    public bool IsBusy { get; private set; }

    public ActivityResolver(GameConfig config, StatSystem stats, EnergySystem energy)
    {
        this.config = config;
        this.stats = stats;
        this.energy = energy;
    }

    public bool CanResolve(ActivityData activity, int actionsUsedToday) =>
        !IsBusy && activity != null && actionsUsedToday < config.TopeAcciones && energy.Current > 0f;

    public IEnumerator Resolve(ActivityData activity, Action onComplete)
    {
        IsBusy = true;

        if (activity.Category != ActivityCategory.Casa)
            GameEvents.RaiseBackgroundChangeRequested(activity.Category);

        float distanciaAntes = stats.Distance(activity.AffectedStat);

        var animator = CharacterAnimator.Instance;
        if (animator != null)
        {
            bool done = false;
            animator.PlayActivity(activity, () => done = true);
            while (!done) yield return null;
        }

        stats.Apply(activity.AffectedStat, config.Delta(activity.Impact));
        float distanciaDespues = stats.Distance(activity.AffectedStat);

        var result = distanciaDespues <= distanciaAntes ? ActivityResult.Estrella : ActivityResult.Calavera;
        float costo = result == ActivityResult.Estrella
            ? config.CostoEstrella(activity.Impact)
            : config.CostoCalavera(activity.Impact);
        energy.Spend(costo);

        GameEvents.RaiseActivityResolved(activity, result);

        if (activity.Category != ActivityCategory.Casa)
            GameEvents.RaiseBackgroundChangeRequested(ActivityCategory.Casa);

        IsBusy = false;
        onComplete?.Invoke();
    }
}
