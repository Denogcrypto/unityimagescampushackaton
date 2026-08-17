using System.Collections.Generic;

/// <summary>
/// Acumula diasRegulado por stat (4.11, S7) y arma el WeekReport final con el
/// mensaje general del terapeuta + una línea por stat, usando TherapistLines.
/// </summary>
public class WeekSummary
{
    private static readonly StatId[] AllStats = { StatId.VidaSocial, StatId.Autoestima, StatId.ActividadFisica };

    private readonly GameConfig config;
    private readonly TherapistLines lines;
    private readonly Dictionary<StatId, int> diasRegulado = new Dictionary<StatId, int>();

    public WeekSummary(GameConfig config, TherapistLines lines)
    {
        this.config = config;
        this.lines = lines;
        foreach (var id in AllStats) diasRegulado[id] = 0;
    }

    /// Paso 1 de 4.8 — se llama ANTES del decaimiento/fluctuación del cierre de día.
    public void RegisterDay(StatSystem stats)
    {
        foreach (var id in AllStats)
            if (stats.GetZone(id) == Zone.Regulado)
                diasRegulado[id]++;
    }

    public int DaysRegulated(StatId id) => diasRegulado[id];

    public RegulationCategory Classify(StatId id)
    {
        int dias = diasRegulado[id];
        if (dias >= config.DiasReguladoParaMejorado) return RegulationCategory.Mejorado;
        if (dias >= config.DiasReguladoParaInestable) return RegulationCategory.Inestable;
        return RegulationCategory.Descuidado;
    }

    public WeekReport BuildReport()
    {
        var statLines = new List<WeekReport.StatLine>();
        int mejorado = 0, descuidado = 0;

        foreach (var id in AllStats)
        {
            var category = Classify(id);
            if (category == RegulationCategory.Mejorado) mejorado++;
            if (category == RegulationCategory.Descuidado) descuidado++;
            statLines.Add(new WeekReport.StatLine(id, category, lines.StatLine(id, category)));
        }

        return new WeekReport(lines.GeneralMessage(mejorado, descuidado), statLines);
    }
}
