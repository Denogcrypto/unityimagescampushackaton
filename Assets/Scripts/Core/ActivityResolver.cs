using System;
using System.Collections;

/// <summary>
/// Resuelve una actividad en el orden estricto de S5: valida, bloquea input
/// (vía IsBusy), calcula distancia_antes, muestra el popup de feedback con
/// el ícono de la actividad (reemplaza el cambio de fondo por categoría de
/// S12 — sin arte de fondos todavía, pedido explícito del usuario), aplica
/// el delta, calcula distancia_despues y determina estrella/calavera (4.4,
/// con &lt;=, no &lt;), descuenta el costo, emite el evento de resultado y
/// desbloquea input.
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

        float distanciaAntes = stats.Distance(activity.AffectedStat);

        var popup = ActivityPopupUI.Instance;
        if (popup != null)
            yield return popup.Play(activity);

        stats.Apply(activity.AffectedStat, config.Delta(activity.Impact));
        float distanciaDespues = stats.Distance(activity.AffectedStat);

        var result = distanciaDespues <= distanciaAntes ? ActivityResult.Estrella : ActivityResult.Calavera;
        float costo = result == ActivityResult.Estrella
            ? config.CostoEstrella(activity.Impact)
            : config.CostoCalavera(activity.Impact);
        energy.Spend(costo);

        // IsBusy=false y onComplete (que cuenta la acción, recalcula ánimo/
        // crítico y puede cerrar el día — pasos 10-11) van ANTES de emitir
        // OnActivityResolved. Bug real que tenía esto al revés: UIManager
        // escucha OnActivityResolved para refrescar los botones, y si ese
        // evento se dispara con IsBusy todavía en true, CanRequestActivity
        // da false para las 9 actividades y el refresh que las reactiva
        // nunca vuelve a correr — quedan bloqueadas hasta que se fuerza un
        // cierre de día. Descansar no se ve afectado porque su Button no
        // pasa por este mismo refresh.
        IsBusy = false;
        onComplete?.Invoke();
        GameEvents.RaiseActivityResolved(activity, result);
    }
}
