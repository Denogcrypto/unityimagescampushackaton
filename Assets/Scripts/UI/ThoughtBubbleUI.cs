using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Nube de pensamiento (S8): 2 íconos sobre el personaje — ánimo (5 estados,
/// 4.2) y stat crítico (3 stats + flecha de dirección, 4.9). Se actualiza al
/// inicio del día y después de cada actividad vía GameEvents; nunca lee
/// GameManager por polling. El ícono de stat crítico arranca oculto y solo
/// se muestra cuando llega OnCriticalStatChanged (el día 1 no lo emite).
///
/// Los sprites (moodSprites/statSprites/arrowSprite) quedan sin asignar hasta
/// S12 (arte). Mientras tanto, moodLabel/criticalStatLabel muestran el
/// nombre en texto para poder verificar la lógica en Play Mode.
/// </summary>
public class ThoughtBubbleUI : MonoBehaviour
{
    [Header("Sigue al personaje (Screen Space Overlay)")]
    [SerializeField] private Transform followTarget;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private Camera worldCamera;

    [Header("Ánimo — 5 estados (orden: Depresivo,Triste,Neutral,Feliz,Alterado)")]
    [SerializeField] private Image moodIcon;
    [SerializeField] private Text moodLabel; // placeholder hasta S12
    [SerializeField] private Sprite[] moodSprites = new Sprite[5];

    [Header("Stat crítico — 3 stats (orden: VidaSocial,Autoestima,ActividadFisica) + flecha")]
    [SerializeField] private GameObject criticalStatRoot;
    [SerializeField] private Image criticalStatIcon;
    [SerializeField] private Text criticalStatLabel; // placeholder hasta S12
    [SerializeField] private Image arrowIcon;
    [SerializeField] private Sprite[] statSprites = new Sprite[3];
    [SerializeField] private Sprite arrowSprite; // una sola flecha; se espeja para "abajo" (4.9)

    void OnEnable()
    {
        GameEvents.OnMoodComputed += HandleMoodComputed;
        GameEvents.OnCriticalStatChanged += HandleCriticalStatChanged;
        GameEvents.OnDayStarted += HandleDayStarted;

        if (criticalStatRoot != null) criticalStatRoot.SetActive(false);
        if (worldCamera == null) worldCamera = Camera.main;
    }

    void OnDisable()
    {
        GameEvents.OnMoodComputed -= HandleMoodComputed;
        GameEvents.OnCriticalStatChanged -= HandleCriticalStatChanged;
        GameEvents.OnDayStarted -= HandleDayStarted;
    }

    void LateUpdate()
    {
        if (followTarget == null || worldCamera == null) return;
        Vector3 screenPoint = worldCamera.WorldToScreenPoint(followTarget.position + worldOffset);
        ((RectTransform)transform).position = screenPoint;
    }

    // Día 1 nunca dispara OnCriticalStatChanged (4.9) — nos aseguramos de que
    // el ícono arranque oculto por si quedó activo de una sesión anterior.
    void HandleDayStarted(int dia, int totalDias)
    {
        if (dia == 1 && criticalStatRoot != null) criticalStatRoot.SetActive(false);
    }

    void HandleMoodComputed(MoodState state, int rawValue)
    {
        int index = (int)state;
        if (moodIcon != null && index >= 0 && index < moodSprites.Length && moodSprites[index] != null)
            moodIcon.sprite = moodSprites[index];
        if (moodLabel != null) moodLabel.text = state.ToString();
    }

    void HandleCriticalStatChanged(StatId stat)
    {
        if (criticalStatRoot != null) criticalStatRoot.SetActive(true);

        int index = (int)stat;
        if (criticalStatIcon != null && index >= 0 && index < statSprites.Length && statSprites[index] != null)
            criticalStatIcon.sprite = statSprites[index];
        if (criticalStatLabel != null) criticalStatLabel.text = TherapistLines.StatDisplayName(stat);

        bool aboveZone = GameManager.Instance != null && GameManager.Instance.IsCriticalStatAboveZone(stat);
        if (arrowIcon != null)
        {
            if (arrowSprite != null) arrowIcon.sprite = arrowSprite;
            var scale = arrowIcon.rectTransform.localScale;
            scale.y = aboveZone ? Mathf.Abs(scale.y) : -Mathf.Abs(scale.y);
            arrowIcon.rectTransform.localScale = scale;
        }
    }
}
