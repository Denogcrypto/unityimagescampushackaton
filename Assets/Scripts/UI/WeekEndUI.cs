using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// S11 — pantalla de terapeuta al cierre de semana. Escucha
/// GameEvents.OnWeekEnded y muestra el WeekReport que ya arma WeekSummary
/// (mensaje general + una línea por stat, todo el texto desde
/// TherapistLines). Es la última pantalla de la partida: a diferencia de
/// S10 (DayTransitionController) no hace fade-out ni se salta — no hay
/// nada después. Sin lenguaje de victoria/derrota en ningún lado (4.11).
/// </summary>
public class WeekEndUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Text generalMessageLabel;
    [SerializeField] private Text statLinesLabel;
    [SerializeField] private Button resetButton;

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 0.5f;

    void Awake()
    {
        resetButton?.onClick.AddListener(OnResetClicked);

        // No desactivar el GameObject: si el objeto arranca inactivo, Awake/
        // Start nunca corren y esto nunca se suscribe a OnWeekEnded (mismo
        // bug que tenía DayTransitionController). Ocultar solo con
        // CanvasGroup, dejando el objeto activo todo el tiempo.
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }

    void Start()
    {
        GameEvents.OnWeekEnded += Show;
    }

    void OnDestroy()
    {
        GameEvents.OnWeekEnded -= Show;
    }

    public void Show(WeekReport report)
    {
        if (generalMessageLabel != null) generalMessageLabel.text = report.GeneralMessage;

        if (statLinesLabel != null)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < report.Lines.Count; i++)
            {
                if (i > 0) sb.Append('\n');
                sb.Append(report.Lines[i].Text);
            }
            statLinesLabel.text = sb.ToString();
        }

        if (canvasGroup != null) { canvasGroup.blocksRaycasts = true; canvasGroup.interactable = true; }
        StopAllCoroutines();
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    // Pedido explícito del usuario (el brief marca reinicio de partida
    // fuera de scope, pero se habilita a propósito desde acá).
    void OnResetClicked()
    {
        StopAllCoroutines();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        GameManager.Instance?.ResetGame();
    }
}
