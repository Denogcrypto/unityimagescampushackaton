using UnityEngine;

/// <summary>
/// Every balance number in the game lives here — nothing hardcoded in the
/// systems that consume it. See Brief_Implementacion_v2_Tamagotchi.pdf
/// sections 4 and 6 (S1) for the source of each value.
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "Tamagotchi/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("Días")]
    [SerializeField] private int totalDias = 7;

    [Header("Stats iniciales (día 1)")]
    [SerializeField] private float statInicialVidaSocial = 50f;
    [SerializeField] private float statInicialAutoestima = 50f;
    [SerializeField] private float statInicialActividadFisica = 50f;

    [Header("Zonas de stat (4.1)")]
    [SerializeField] private float zonaBajoMax = 35f;   // Bajo: stat < zonaBajoMax
    [SerializeField] private float zonaAltoMin = 70f;    // Alto: stat > zonaAltoMin — entre ambos, Regulado

    [Header("Ánimo (4.2)")]
    [SerializeField] private float dispersionFactor = 0.3f;
    [SerializeField] private float animoDepresivoMax = 20f;
    [SerializeField] private float animoTristeMax = 40f;
    [SerializeField] private float animoNeutralMax = 60f;
    [SerializeField] private float animoFelizMax = 80f;  // Alterado: por encima de esto

    [Header("Delta por impacto (4.3)")]
    [SerializeField] private float deltaBajo = 5f;
    [SerializeField] private float deltaMedio = 10f;
    [SerializeField] private float deltaAlto = 15f;

    [Header("Costo de energía si Estrella (4.3)")]
    [SerializeField] private float costoEstrellaBajo = 0f;
    [SerializeField] private float costoEstrellaMedio = 5f;
    [SerializeField] private float costoEstrellaAlto = 10f;

    [Header("Costo de energía si Calavera (4.3)")]
    [SerializeField] private float costoCalaveraBajo = 10f;
    [SerializeField] private float costoCalaveraMedio = 20f;
    [SerializeField] private float costoCalaveraAlto = 30f;

    [Header("Tope de acciones por día (4.3)")]
    [SerializeField] private int topeAcciones = 4;

    [Header("Energía inicial — día 1 según ánimo (4.5)")]
    [SerializeField] private Vector2 energiaInicialDepresivo = new Vector2(15f, 25f);
    [SerializeField] private Vector2 energiaInicialTriste = new Vector2(25f, 45f);
    [SerializeField] private Vector2 energiaInicialNeutral = new Vector2(45f, 65f);
    [SerializeField] private Vector2 energiaInicialFeliz = new Vector2(65f, 85f);
    [SerializeField] private Vector2 energiaInicialAlterado = new Vector2(85f, 100f);

    [Header("Multiplicador de energía — días 2-7 según ánimo (4.5)")]
    [SerializeField] private float multEnergiaDepresivo = 0.4f;
    [SerializeField] private float multEnergiaTriste = 0.6f;
    [SerializeField] private float multEnergiaNeutral = 0.8f;
    [SerializeField] private float multEnergiaFeliz = 1.0f;
    [SerializeField] private float multEnergiaAlterado = 1.1f;
    [SerializeField] private float energiaMinima = 15f;

    [Header("Decaimiento y fluctuación (4.6, 4.7)")]
    [SerializeField] private float decaimientoDiario = 10f;
    [SerializeField] private float fluctuacionMin = 5f;
    [SerializeField] private float fluctuacionMax = 8f;

    [Header("Cierre de semana (4.11)")]
    [SerializeField] private int diasReguladoParaMejorado = 5;   // 5,6,7 → Mejorado
    [SerializeField] private int diasReguladoParaInestable = 3;  // 3,4 → Inestable; menos → Descuidado

    // ─── Accessors ──────────────────────────────────────────────
    public int TotalDias => totalDias;

    public float StatInicial(StatId stat) => stat switch
    {
        StatId.VidaSocial => statInicialVidaSocial,
        StatId.Autoestima => statInicialAutoestima,
        StatId.ActividadFisica => statInicialActividadFisica,
        _ => 50f,
    };

    public float ZonaBajoMax => zonaBajoMax;
    public float ZonaAltoMin => zonaAltoMin;

    public float DispersionFactor => dispersionFactor;
    public float AnimoDepresivoMax => animoDepresivoMax;
    public float AnimoTristeMax => animoTristeMax;
    public float AnimoNeutralMax => animoNeutralMax;
    public float AnimoFelizMax => animoFelizMax;

    public float Delta(ImpactTier impact) => impact switch
    {
        ImpactTier.Bajo => deltaBajo,
        ImpactTier.Medio => deltaMedio,
        ImpactTier.Alto => deltaAlto,
        _ => 0f,
    };

    public float CostoEstrella(ImpactTier impact) => impact switch
    {
        ImpactTier.Bajo => costoEstrellaBajo,
        ImpactTier.Medio => costoEstrellaMedio,
        ImpactTier.Alto => costoEstrellaAlto,
        _ => 0f,
    };

    public float CostoCalavera(ImpactTier impact) => impact switch
    {
        ImpactTier.Bajo => costoCalaveraBajo,
        ImpactTier.Medio => costoCalaveraMedio,
        ImpactTier.Alto => costoCalaveraAlto,
        _ => 0f,
    };

    public int TopeAcciones => topeAcciones;

    public Vector2 EnergiaInicial(MoodState mood) => mood switch
    {
        MoodState.Depresivo => energiaInicialDepresivo,
        MoodState.Triste => energiaInicialTriste,
        MoodState.Neutral => energiaInicialNeutral,
        MoodState.Feliz => energiaInicialFeliz,
        MoodState.Alterado => energiaInicialAlterado,
        _ => energiaInicialNeutral,
    };

    public float MultiplicadorEnergia(MoodState mood) => mood switch
    {
        MoodState.Depresivo => multEnergiaDepresivo,
        MoodState.Triste => multEnergiaTriste,
        MoodState.Neutral => multEnergiaNeutral,
        MoodState.Feliz => multEnergiaFeliz,
        MoodState.Alterado => multEnergiaAlterado,
        _ => multEnergiaNeutral,
    };

    public float EnergiaMinima => energiaMinima;

    public float DecaimientoDiario => decaimientoDiario;
    public float FluctuacionMin => fluctuacionMin;
    public float FluctuacionMax => fluctuacionMax;

    public int DiasReguladoParaMejorado => diasReguladoParaMejorado;
    public int DiasReguladoParaInestable => diasReguladoParaInestable;
}
