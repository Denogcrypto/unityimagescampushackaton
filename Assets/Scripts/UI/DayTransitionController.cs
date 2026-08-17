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
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        if (streakLabel != null) streakLabel.text = "";
        gameObject.SetActive(false);
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
        gameObject.SetActive(true);

        yield return Fade(0f, 1f, fadeDuration);

        float t = 0f;
        while (t < holdDuration && !skipRequested)
        {
            t += Time.deltaTime;
            yield return null;
        }

        yield return Fade(1f, 0f, fadeDuration);
        gameObject.SetActive(false);
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
