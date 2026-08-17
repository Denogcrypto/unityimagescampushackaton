using UnityEngine;

/// <summary>
/// All week-end therapist text (4.11) lives here — nothing hardcoded in
/// WeekSummary. Default values below are the exact wording from the brief for
/// the 4 general messages; the per-stat line templates are a first pass for
/// design to edit from the Inspector.
/// </summary>
[CreateAssetMenu(fileName = "TherapistLines", menuName = "Tamagotchi/Therapist Lines")]
public class TherapistLines : ScriptableObject
{
    [Header("Mensaje general según combinación (4.11)")]
    [TextArea]
    [SerializeField] private string mensajeTodosMejorado =
        "Esta semana estuviste muy bien acompañado de vos mismo. Sigamos así.";
    [TextArea]
    [SerializeField] private string mensajeMejoradoInestableSinDescuidado =
        "Hubo avances esta semana, aunque no en todo. Es un buen punto de partida.";
    [TextArea]
    [SerializeField] private string mensajeUnDescuidado =
        "Fue una semana difícil. No pasa nada, para eso seguimos viniendo acá.";
    [TextArea]
    [SerializeField] private string mensajeDosOTresDescuidado =
        "Esta semana costó sostener el equilibrio. Vamos a seguir trabajando en esto juntos.";

    [Header("Línea por stat — usar {stat} para insertar el nombre")]
    [TextArea]
    [SerializeField] private string lineaMejorado = "{stat} mejoró esta semana.";
    [TextArea]
    [SerializeField] private string lineaInestable = "{stat} estuvo inestable, subiendo y bajando de zona.";
    [TextArea]
    [SerializeField] private string lineaDescuidado = "{stat} quedó descuidado casi toda la semana.";

    public string GeneralMessage(int mejorado, int descuidado)
    {
        if (mejorado == 3) return mensajeTodosMejorado;
        if (descuidado == 0) return mensajeMejoradoInestableSinDescuidado;
        if (descuidado == 1) return mensajeUnDescuidado;
        return mensajeDosOTresDescuidado;
    }

    public string StatLine(StatId stat, RegulationCategory category)
    {
        string template = category switch
        {
            RegulationCategory.Mejorado => lineaMejorado,
            RegulationCategory.Inestable => lineaInestable,
            _ => lineaDescuidado,
        };
        return template.Replace("{stat}", StatDisplayName(stat));
    }

    public static string StatDisplayName(StatId stat) => stat switch
    {
        StatId.VidaSocial => "Vida social",
        StatId.Autoestima => "Autoestima",
        StatId.ActividadFisica => "Actividad física",
        _ => stat.ToString(),
    };
}
