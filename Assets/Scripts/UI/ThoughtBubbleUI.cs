using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Nube de pensamiento — pedido explícito del usuario (2026-08-17),
/// reemplaza el S8 original (dos íconos simultáneos y persistentes) por una
/// secuencia de globos temporales, cada uno visible 3 segundos:
///
/// 1. Al inicio de cada día: ánimo del día (siempre) y, desde el día 2,
///    el stat que el personaje quiere mejorar ese día (el crítico, 4.9) —
///    uno después del otro, no simultáneos.
/// 2. Al resolver cada actividad: estrella si la actividad tocó el stat
///    "objetivo" anunciado al inicio del día, calavera si tocó otro.
///    Esto es un indicador puramente visual, independiente del estrella/
///    calavera de distancia-a-zona que ya existía (S5, 4.4) — ese sigue
///    definiendo el costo de energía exactamente igual que antes, sin
///    tocarlo. "Buena decisión" acá significa "atendiste el stat que
///    hacía falta", sin importar si quedó regulado o no.
///
/// El objetivo del día se calcula una sola vez (en OnDayStarted, leyendo
/// GameManager directo) y NO se vuelve a tocar hasta el día siguiente —
/// GameEvents.OnMoodComputed/OnCriticalStatChanged se re-disparan después
/// de cada actividad con el estado YA actualizado, así que no sirven para
/// guardar "el objetivo con el que arrancó el día"; por eso este script no
/// se suscribe a esos dos eventos.
///
/// Sprites sin asignar a propósito (S12 — el usuario los agrega después
/// desde el Inspector, sin tocar código).
/// </summary>
public class ThoughtBubbleUI : MonoBehaviour
{
    [Header("Sigue al personaje (Screen Space Overlay)")]
    [SerializeField] private Transform followTarget;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private Camera worldCamera;

    [Header("Globo (un solo ícono a la vez)")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image icon;

    [Header("Ánimo — orden Depresivo,Triste,Neutral,Feliz,Alterado")]
    [SerializeField] private Sprite[] moodSprites = new Sprite[5];

    [Header("Stat a mejorar — orden VidaSocial,Autoestima,ActividadFisica")]
    [SerializeField] private Sprite[] statSprites = new Sprite[3];

    [Header("Resultado de actividad")]
    [SerializeField] private Sprite starSprite;
    [SerializeField] private Sprite skullSprite;

    [Header("Timing")]
    [SerializeField] private float showDuration = 3f;

    private StatId? todayTargetStat;
    private Coroutine routine;

    void OnEnable()
    {
        GameEvents.OnDayStarted += HandleDayStarted;
        GameEvents.OnActivityResolved += HandleActivityResolved;
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        if (worldCamera == null) worldCamera = Camera.main;
    }

    void OnDisable()
    {
        GameEvents.OnDayStarted -= HandleDayStarted;
        GameEvents.OnActivityResolved -= HandleActivityResolved;
    }

    void LateUpdate()
    {
        if (followTarget == null || worldCamera == null) return;
        Vector3 screenPoint = worldCamera.WorldToScreenPoint(followTarget.position + worldOffset);
        ((RectTransform)transform).position = screenPoint;
    }

    void HandleDayStarted(int dia, int totalDias)
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        var (raw, mood) = MoodSystem.Compute(
            gm.GetStat(StatId.VidaSocial), gm.GetStat(StatId.Autoestima), gm.GetStat(StatId.ActividadFisica), gm.Config);

        // El objetivo se calcula siempre (lo necesita el feedback de
        // actividades de más abajo), pero el globo que lo anuncia solo se
        // muestra desde el día 2 (4.9: el día 1 no tiene stat crítico real
        // que señalar con 50/50/50).
        todayTargetStat = gm.GetCriticalStat();

        if (dia > 1)
            PlaySequence(MoodSprite(mood), StatSprite(todayTargetStat.Value));
        else
            PlaySequence(MoodSprite(mood));
    }

    void HandleActivityResolved(ActivityData activity, ActivityResult result)
    {
        if (!todayTargetStat.HasValue || activity == null) return;
        bool goodDecision = activity.AffectedStat == todayTargetStat.Value;
        PlaySequence(goodDecision ? starSprite : skullSprite);
    }

    void PlaySequence(params Sprite[] sprites)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(SequenceRoutine(sprites));
    }

    IEnumerator SequenceRoutine(Sprite[] sprites)
    {
        foreach (var sprite in sprites)
        {
            if (icon != null) icon.sprite = sprite;
            if (canvasGroup != null) canvasGroup.alpha = 1f;
            yield return new WaitForSeconds(showDuration);
            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }
    }

    Sprite MoodSprite(MoodState state)
    {
        int index = (int)state;
        return index >= 0 && index < moodSprites.Length ? moodSprites[index] : null;
    }

    Sprite StatSprite(StatId stat)
    {
        int index = (int)stat;
        return index >= 0 && index < statSprites.Length ? statSprites[index] : null;
    }
}
