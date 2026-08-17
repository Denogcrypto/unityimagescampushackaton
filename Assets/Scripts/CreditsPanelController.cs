using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Wires the "Créditos" button to a pause overlay with a fade + scale
/// open/close effect. Pauses the game (Time.timeScale = 0) while open;
/// the animation itself runs on unscaled time so it isn't frozen too.
/// </summary>
public class CreditsPanelController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button backButton;

    [Header("Panel")]
    [SerializeField] private CanvasGroup panelGroup;   // on the full-screen overlay root
    [SerializeField] private RectTransform panelBox;   // the inner card that scales in/out

    [Header("Animation")]
    [SerializeField] private float animDuration = 0.25f;
    [SerializeField] private float openScaleFrom = 0.85f;

    private float previousTimeScale = 1f;
    private Coroutine animRoutine;

    void Awake()
    {
        panelGroup.alpha = 0f;
        panelGroup.interactable = false;
        panelGroup.blocksRaycasts = false;
        panelBox.localScale = Vector3.one * openScaleFrom;
        panelGroup.gameObject.SetActive(false);
    }

    void Start()
    {
        creditsButton.onClick.AddListener(Open);
        backButton.onClick.AddListener(Close);
    }

    public void Open()
    {
        if (panelGroup.gameObject.activeSelf && panelGroup.interactable) return;

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        panelGroup.gameObject.SetActive(true);
        Restart(Animate(true));
    }

    public void Close()
    {
        Restart(Animate(false));
    }

    void Restart(IEnumerator routine)
    {
        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(routine);
    }

    IEnumerator Animate(bool opening)
    {
        panelGroup.interactable = opening;
        panelGroup.blocksRaycasts = opening;

        float from = opening ? 0f : 1f;
        float to = opening ? 1f : 0f;
        float fromScale = opening ? openScaleFrom : 1f;
        float toScale = opening ? 1f : openScaleFrom;

        float t = 0f;
        while (t < animDuration)
        {
            t += Time.unscaledDeltaTime;
            float eased = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / animDuration));
            panelGroup.alpha = Mathf.Lerp(from, to, eased);
            panelBox.localScale = Vector3.one * Mathf.Lerp(fromScale, toScale, eased);
            yield return null;
        }

        panelGroup.alpha = to;
        panelBox.localScale = Vector3.one * toScale;

        if (!opening)
        {
            panelGroup.gameObject.SetActive(false);
            Time.timeScale = previousTimeScale;
        }

        animRoutine = null;
    }
}
