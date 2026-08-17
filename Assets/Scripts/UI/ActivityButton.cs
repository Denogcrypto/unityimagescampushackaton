using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Un botón del menú plano de 9 actividades (S9). No muestra delta ni costo
/// exacto a propósito — el jugador tiene que estimar, eso es el juego.
/// Deshabilitado si no queda energía o ya se usaron las acciones del día.
/// </summary>
public class ActivityButton : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private ActivityData activity;

    [Header("UI Elements")]
    [SerializeField] private Text nameLabel;
    [SerializeField] private Image icon;
    [SerializeField] private Button button;

    void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        button?.onClick.AddListener(OnClick);
    }

    void OnEnable() => Refresh();

    public void Refresh()
    {
        if (activity == null || GameManager.Instance == null) return;

        if (nameLabel != null) nameLabel.text = activity.DisplayName;
        if (icon != null && activity.Icon != null) icon.sprite = activity.Icon;

        if (button != null)
            button.interactable = GameManager.Instance.CanRequestActivity(activity);
    }

    void OnClick() => GameManager.Instance?.RequestActivity(activity);
}
