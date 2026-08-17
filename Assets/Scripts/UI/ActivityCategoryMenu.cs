using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Navegación de 2 niveles del menú de actividades — pedido explícito del
/// usuario (2026-08-17), reemplaza el menú plano de 9 botones de S9 del
/// brief: 3 botones de categoría -> lista de las 3 actividades de esa
/// categoría + botón Volver. Al resolverse una actividad, o al empezar un
/// día nuevo, vuelve sola al menú de categorías.
///
/// No requiere wiring manual por Inspector de los 9 ActivityButton ni de
/// las 3 categorías: los descubre por GetComponentsInChildren bajo los
/// paneles que sí se asignan, así que reordenar/agregar botones en la
/// escena no rompe esto.
/// </summary>
public class ActivityCategoryMenu : MonoBehaviour
{
    [SerializeField] private GameObject categoryMenuPanel;
    [SerializeField] private GameObject activityListPanel;
    [SerializeField] private Button backButton;

    private CategoryButton[] categoryButtons;
    private ActivityButton[] activityButtons;

    void Awake()
    {
        categoryButtons = categoryMenuPanel.GetComponentsInChildren<CategoryButton>(true);
        activityButtons = activityListPanel.GetComponentsInChildren<ActivityButton>(true);

        foreach (var cb in categoryButtons)
        {
            var category = cb.Category;
            var button = cb.GetComponent<Button>();
            button.onClick.AddListener(() => ShowCategory(category));
        }

        backButton?.onClick.AddListener(ShowCategoryMenu);
    }

    void Start()
    {
        GameEvents.OnActivityResolved += OnActivityResolved;
        GameEvents.OnDayStarted += OnDayStarted;
        ShowCategoryMenu();
    }

    void OnDestroy()
    {
        GameEvents.OnActivityResolved -= OnActivityResolved;
        GameEvents.OnDayStarted -= OnDayStarted;
    }

    void OnActivityResolved(ActivityData activity, ActivityResult result) => ShowCategoryMenu();
    void OnDayStarted(int dia, int totalDias) => ShowCategoryMenu();

    void ShowCategory(ActivityCategory category)
    {
        categoryMenuPanel.SetActive(false);
        activityListPanel.SetActive(true);
        foreach (var ab in activityButtons)
            ab.gameObject.SetActive(ab.Category == category);
    }

    void ShowCategoryMenu()
    {
        activityListPanel.SetActive(false);
        categoryMenuPanel.SetActive(true);
    }
}
