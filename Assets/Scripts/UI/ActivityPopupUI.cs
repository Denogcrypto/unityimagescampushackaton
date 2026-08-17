using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Popup de feedback por actividad — reemplaza el cambio de fondo por
/// categoría de S12 (sin arte de fondos todavía, pedido explícito del
/// usuario). Muestra el ícono de la actividad (ActivityData.Icon,
/// configurable por asset, sin tocar código).
///
/// El panel (<see cref="panelRoot"/>) solo abre/cierra por escala hacia su
/// propio centro (pivot 0.5/0.5) — no rota ni rebota. El bamboleo
/// (rotación leve izq/der + pulso de escala "bouncy") se aplica únicamente
/// al ícono adentro, durante <see cref="holdDuration"/> segundos.
///
/// ActivityResolver hace <c>yield return</c> sobre <see cref="Play"/>, así
/// que mientras el popup está en pantalla el jugador no puede pedir otra
/// actividad (IsBusy sigue en true) — recién al cerrarse se aplica el
/// delta al stat, se ve el resultado y se dispara el feedback de ánimo/
/// stat crítico sobre la cabeza del personaje (misma idea de "ver la causa
/// antes del efecto" que ya tenía S5, solo que ahora el popup es la causa).
/// </summary>
public class ActivityPopupUI : MonoBehaviour
{
    public static ActivityPopupUI Instance { get; private set; }

    [Header("Panel (solo abre/cierra por escala, no rota)")]
    [SerializeField] private RectTransform panelRoot;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image icon;

    [Header("Timing")]
    [SerializeField] private float openDuration = 0.2f;
    [SerializeField] private float holdDuration = 3f;
    [SerializeField] private float closeDuration = 0.25f;

    [Header("Bamboleo del ícono (rotación leve + pulso de escala)")]
    [SerializeField] private float wobbleAngle = 12f;
    [SerializeField] private float wobbleSpeed = 6f;
    [SerializeField] private float bounceScaleAmount = 0.12f;
    [SerializeField] private float bounceSpeed = 4f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (canvasGroup != null) { canvasGroup.alpha = 0f; canvasGroup.blocksRaycasts = false; canvasGroup.interactable = false; }
        if (panelRoot != null) panelRoot.localScale = Vector3.zero;
        if (icon != null) icon.rectTransform.localRotation = Quaternion.identity;
    }

    public IEnumerator Play(ActivityData activity)
    {
        if (icon != null) icon.sprite = activity.Icon;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        yield return ScaleTo(0f, 1f, openDuration);

        RectTransform iconRT = icon != null ? icon.rectTransform : null;
        float t = 0f;
        while (t < holdDuration)
        {
            if (iconRT != null)
            {
                float angle = Mathf.Sin(t * wobbleSpeed) * wobbleAngle;
                float scale = 1f + Mathf.Abs(Mathf.Sin(t * bounceSpeed)) * bounceScaleAmount;
                iconRT.localRotation = Quaternion.Euler(0f, 0f, angle);
                iconRT.localScale = Vector3.one * scale;
            }
            t += Time.deltaTime;
            yield return null;
        }

        if (iconRT != null)
        {
            iconRT.localRotation = Quaternion.identity;
            iconRT.localScale = Vector3.one;
        }

        yield return ScaleTo(1f, 0f, closeDuration);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }

    IEnumerator ScaleTo(float from, float to, float duration)
    {
        if (panelRoot == null) yield break;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            panelRoot.localScale = Vector3.one * Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        panelRoot.localScale = Vector3.one * to;
    }
}
