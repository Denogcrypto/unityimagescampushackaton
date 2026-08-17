using UnityEngine;

/// <summary>
/// Energía inicial (día 1 por tabla, días 2-7 por fórmula) y gasto por
/// actividad (4.5, S4). Nunca queda negativa: el costo que exceda lo
/// disponible simplemente la deja en 0 (dispara cierre forzado en GameManager).
/// </summary>
public class EnergySystem
{
    private readonly GameConfig config;

    public float Current { get; private set; }

    public EnergySystem(GameConfig config)
    {
        this.config = config;
    }

    public void InitializeDay1(MoodState moodDay1)
    {
        Vector2 range = config.EnergiaInicial(moodDay1);
        float before = Current;
        Current = Random.Range(range.x, range.y);
        GameEvents.RaiseEnergyChanged(Mathf.RoundToInt(Current), Mathf.RoundToInt(Current - before));
    }

    /// energiaHoy = clamp( (100 - sobranteEfectivo) * multiplicadorAnimo , 15 , 100 )
    public void RecoverForNewDay(MoodState moodToday, float sobranteEfectivo)
    {
        float before = Current;
        float raw = (100f - sobranteEfectivo) * config.MultiplicadorEnergia(moodToday);
        Current = Mathf.Clamp(raw, config.EnergiaMinima, 100f);
        GameEvents.RaiseEnergyChanged(Mathf.RoundToInt(Current), Mathf.RoundToInt(Current - before));
    }

    public void Spend(float cost)
    {
        float before = Current;
        Current = Mathf.Max(0f, Current - cost);
        GameEvents.RaiseEnergyChanged(Mathf.RoundToInt(Current), Mathf.RoundToInt(Current - before));
    }
}
