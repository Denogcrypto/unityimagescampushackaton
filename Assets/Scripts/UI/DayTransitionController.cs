using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// S10 — pantalla de cierre de día. Fondo negro en los dos casos; solo cambia
/// ícono y texto (4.10). ≤2s, salteable con click. Escucha
/// GameEvents.OnDayClosed en vez de acoplarse a GameManager directo.
/// Los nombres de campo (dayLabel/moodStatusLabel/streakLabel) se conservan
/// tal cual estaban cableados en el Inspector de la escena, aunque ahora
/// dayLabel hace de ícono y moodStatusLabel de mensaje — evita tener que
/// re-cablear referencias rotas por un rename.
/// </summary>
public class DayTransitionController : MonoBehaviour, IPointerClickHandler
{
    [Header("Panel")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Text dayLabel;         // ahora: ícono ("ZZZ" / batería)
    [SerializeField] private Text moodStatusLabel;  // ahora: mensaje ("Hora de descansar" / "Sin energía…")
    [SerializeField] private Text streakLabel;       // sin uso — el streak de inestabilidad quedó derogado

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private float holdDuration = 1.5f; // total ≤ 2s sumado al fade

    private Coroutine routine;
    private bool skipRequested;

    void Awake()
    {
        // No desactivamos el GameObject: si lo hiciéramos, Start() nunca
        // correría y esto nunca se suscribiría a OnDayClosed (bug real que
        // tenía este panel — quedaba invisible para siempre). Se oculta
        // solo con CanvasGroup, dejando el objeto activo todo el tiempo.
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        if (streakLabel != null) streakLabel.text = "";
    }

    void Start()
    {
        GameEvents.OnDayClosed += Show;
    }

    void OnDestroy()
    {
        GameEvents.OnDayClosed -= Show;
    }

    public void Show(DayCloseReason reason)
    {
        if (dayLabel != null)
            dayLabel.text = reason == DayCloseReason.Voluntario ? "ZZZ" : "🔋✕";
        if (moodStatusLabel != null)
            moodStatusLabel.text = reason == DayCloseReason.Voluntario ? "Hora de descansar" : "Sin energía…";

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Sequence());
    }

    public void OnPointerClick(PointerEventData eventData) => skipRequested = true;

    IEnumerator Sequence()
    {
        skipRequested = false;
        if (canvasGroup != null) { canvasGroup.blocksRaycasts = true; canvasGroup.interactable = true; }

        yield return Fade(0f, 1f, fadeDuration);

        float t = 0f;
        while (t < holdDuration && !skipRequested)
        {
            t += Time.deltaTime;
            yield return null;
        }

        yield return Fade(1f, 0f, fadeDuration);
        if (canvasGroup != null) { canvasGroup.blocksRaycasts = false; canvasGroup.interactable = false; }
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        if (canvasGroup == null) yield break;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}
