using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Botón del nuevo menú de categorías (2 niveles, pedido explícito del
/// usuario — reemplaza el menú plano de 9 botones de S9). Solo identifica
/// a qué ActivityCategory corresponde; toda la lógica de mostrar/ocultar
/// vive en ActivityCategoryMenu, que descubre estos botones por
/// GetComponentsInChildren en vez de que cada uno se cablee a mano.
/// </summary>
public class CategoryButton : MonoBehaviour
{
    [SerializeField] private ActivityCategory category;
    [SerializeField] private Button button;

    public ActivityCategory Category => category;
    public Button Button => button;

    void Awake()
    {
        if (button == null) button = GetComponent<Button>();
    }
}
