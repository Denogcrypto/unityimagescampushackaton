using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds the entire Tamagotchi UI at runtime by code.
/// Attach to any GameObject in the scene. Requires GameManager to exist.
/// Paleta: fondo #0D0F0A, panel #1A1F14, verde neon #9BF026, gris oscuro #2A2F22
/// </summary>
[RequireComponent(typeof(Canvas))]
public class UIBuilder : MonoBehaviour
{
    // ── Colores ──────────────────────────────────────────────────
    static readonly Color BG          = Hex("#0D0F0A");
    static readonly Color PanelDark   = Hex("#1A1F14");
    static readonly Color PanelMid    = Hex("#232A1A");
    static readonly Color ScreenBG    = Hex("#0A1205");
    static readonly Color Neon        = Hex("#9BF026");
    static readonly Color NeonDim     = Hex("#4A7C00");
    static readonly Color ButtonDark  = Hex("#1E241A");
    static readonly Color ButtonHL    = Hex("#9BF026");
    static readonly Color TextNeon    = Hex("#9BF026");
    static readonly Color TextDim     = Hex("#4A7C00");
    static readonly Color White       = Color.white;

    // ── Referencias vivas ────────────────────────────────────────
    Text    moodText, energyText, dayText, streakText;
    Slider  moodSlider, energySlider;
    Image   moodFill, energyFill;
    GameObject mainMenu, activitiesPanel, parkPanel, socialPanel;
    GameObject gameOverPanel, victoryPanel, dayTransPanel;
    Text    dayTransLabel, gameOverLabel;
    bool    uiReady = false;

    // ── Actividades cacheadas ─────────────────────────────────────
    ActivityData actLeer, actPasear, actEjercicio, actVisitar, actDescansar;

    void Awake()
    {
        var canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        var cs = GetComponent<CanvasScaler>();
        if (cs == null) cs = gameObject.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(480, 854);
        cs.matchWidthOrHeight = 0.5f;

        var gr = GetComponent<GraphicRaycaster>();
        if (gr == null) gameObject.AddComponent<GraphicRaycaster>();
    }

    void Start()
    {
        CacheActivities();
        BuildUI();
        uiReady = true;

        var gm = GameManager.Instance;
        gm.OnStatsChanged.AddListener(RefreshHUD);
        gm.OnDayEnded.AddListener(ShowDayTransition);
        gm.OnGameOver.AddListener(ShowGameOver);
        gm.OnVictory.AddListener(ShowVictory);

        RefreshHUD();
        ShowMainMenu();
    }

    // ─────────────────────────────────────────────────────────────
    // CACHE ACTIVITIES
    // ─────────────────────────────────────────────────────────────
    void CacheActivities()
    {
        var gm = GameManager.Instance;
        foreach (var a in gm.AllActivities)
        {
            if (a == null) continue;
            switch (a.ActivityName)
            {
                case "Leer":             actLeer      = a; break;
                case "Pasear":           actPasear    = a; break;
                case "Ejercicio Intenso":actEjercicio = a; break;
                case "Visitar Amigos":   actVisitar   = a; break;
                case "Descansar":        actDescansar = a; break;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────
    // BUILD UI
    // ─────────────────────────────────────────────────────────────
    void BuildUI()
    {
        Transform root = transform;

        // ── Fondo total ──────────────────────────────────────────
        var bg = MakeImage("Background", root, BG);
        bg.rectTransform.anchorMin = Vector2.zero;
        bg.rectTransform.anchorMax = Vector2.one;
        bg.rectTransform.offsetMin = Vector2.zero;
        bg.rectTransform.offsetMax = Vector2.zero;

        // ── Panel principal — ocupa casi toda la pantalla ────────
        var device = MakeImage("DevicePanel", root, PanelDark);
        device.rectTransform.anchorMin = Vector2.zero;
        device.rectTransform.anchorMax = Vector2.one;
        device.rectTransform.offsetMin = new Vector2(16, 16);
        device.rectTransform.offsetMax = new Vector2(-16, -16);

        Transform dev = device.transform;

        // ── Header ───────────────────────────────────────────────
        BuildHeader(dev);

        // ── Pantalla ─────────────────────────────────────────────
        BuildScreen(dev);

        // ── Menu principal (botones 2x2) ─────────────────────────
        mainMenu = BuildMainMenu(dev);

        // ── Subpaneles de actividades ────────────────────────────
        activitiesPanel = BuildSubPanel(dev, "ActivitiesPanel",
            ("Leer",          actLeer,      false),
            ("Ejercicio",     actEjercicio, true));

        parkPanel = BuildSubPanel(dev, "ParkPanel",
            ("Pasear",        actPasear,    false),
            (null,            null,         false));

        socialPanel = BuildSubPanel(dev, "SocialPanel",
            ("Visitar Amigos",actVisitar,   true),
            (null,            null,         false));

        // ── Barra inferior de navegación ─────────────────────────
        BuildNavBar(dev);

        // ── Overlays ─────────────────────────────────────────────
        dayTransPanel = BuildDayTransition(root);
        gameOverPanel = BuildGameOverPanel(root);
        victoryPanel  = BuildVictoryPanel(root);
    }

    // ─────────────────────────────────────────────────────────────
    void BuildHeader(Transform parent)
    {
        var header = MakeImage("Header", parent, PanelMid);
        header.rectTransform.anchorMin = new Vector2(0, 1);
        header.rectTransform.anchorMax = new Vector2(1, 1);
        header.rectTransform.sizeDelta = new Vector2(0, 50);
        header.rectTransform.anchoredPosition = new Vector2(0, -25);

        // Título
        var title = MakeText("Title", header.rectTransform, "TAMAGOTCHI", 18, TextNeon, FontStyle.Bold);
        title.rectTransform.anchorMin = Vector2.zero;
        title.rectTransform.anchorMax = Vector2.one;
        title.rectTransform.offsetMin = Vector2.zero;
        title.rectTransform.offsetMax = Vector2.zero;
        title.alignment = TextAnchor.MiddleCenter;

        // Día — esquina derecha
        dayText = MakeText("DayLabel", header.rectTransform, "DÍA 1/30", 11, NeonDim, FontStyle.Normal);
        dayText.rectTransform.anchorMin = new Vector2(1, 0);
        dayText.rectTransform.anchorMax = new Vector2(1, 1);
        dayText.rectTransform.sizeDelta = new Vector2(90, 0);
        dayText.rectTransform.anchoredPosition = new Vector2(-48, 0);
        dayText.alignment = TextAnchor.MiddleCenter;
    }

    // ─────────────────────────────────────────────────────────────
    void BuildScreen(Transform parent)
    {
        // Marco de pantalla: desde debajo del header hasta el 45% de la pantalla
        var frame = MakeImage("ScreenFrame", parent, ScreenBG);
        frame.rectTransform.anchorMin = new Vector2(0, 0.45f);
        frame.rectTransform.anchorMax = new Vector2(1, 1);
        frame.rectTransform.offsetMin = new Vector2(8, 0);
        frame.rectTransform.offsetMax = new Vector2(-8, -52);  // -52 = deja espacio al header

        Transform fr = frame.rectTransform;

        // Barras de stats — arriba del frame
        BuildStatBars(fr);

        // Streak warning — esquina inferior izquierda
        streakText = MakeText("StreakLabel", fr, "", 10, Hex("#FF4444"), FontStyle.Bold);
        streakText.rectTransform.anchorMin = new Vector2(0, 0);
        streakText.rectTransform.anchorMax = new Vector2(1, 0);
        streakText.rectTransform.sizeDelta = new Vector2(0, 22);
        streakText.rectTransform.anchoredPosition = new Vector2(8, 12);
        streakText.alignment = TextAnchor.LowerLeft;
    }

    void BuildStatBars(Transform parent)
    {
        var barsRow = MakeImage("StatsRow", parent, PanelMid);
        barsRow.rectTransform.anchorMin = new Vector2(0, 1);
        barsRow.rectTransform.anchorMax = new Vector2(1, 1);
        barsRow.rectTransform.sizeDelta = new Vector2(0, 28);
        barsRow.rectTransform.anchoredPosition = new Vector2(0, -14);

        Transform br = barsRow.rectTransform;

        // ENG label + slider
        var engLabel = MakeText("LblENG", br, "ENG", 10, TextDim, FontStyle.Normal);
        engLabel.rectTransform.anchorMin = new Vector2(0, 0);
        engLabel.rectTransform.anchorMax = new Vector2(0, 1);
        engLabel.rectTransform.sizeDelta = new Vector2(36, 0);
        engLabel.rectTransform.anchoredPosition = new Vector2(22, 0);
        engLabel.alignment = TextAnchor.MiddleCenter;

        energySlider = MakeStatSlider("EnergySlider", br, new Vector2(0.22f, 0.5f), 0.24f, NeonDim, out energyFill);

        // JOY label + slider
        var joyLabel = MakeText("LblJOY", br, "JOY", 10, TextDim, FontStyle.Normal);
        joyLabel.rectTransform.anchorMin = new Vector2(0.5f, 0);
        joyLabel.rectTransform.anchorMax = new Vector2(0.5f, 1);
        joyLabel.rectTransform.sizeDelta = new Vector2(36, 0);
        joyLabel.rectTransform.anchoredPosition = new Vector2(4, 0);
        joyLabel.alignment = TextAnchor.MiddleCenter;

        moodSlider = MakeStatSlider("MoodSlider", br, new Vector2(0.72f, 0.5f), 0.24f, Neon, out moodFill);
    }

    Slider MakeStatSlider(string name, Transform parent, Vector2 anchorPos, float widthPct,
        Color fillColor, out Image fillImg)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var slider = go.AddComponent<Slider>();
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(anchorPos.x - widthPct / 2f, 0.2f);
        rt.anchorMax = new Vector2(anchorPos.x + widthPct / 2f, 0.8f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        slider.minValue = 0; slider.maxValue = 1; slider.value = 1;
        slider.interactable = false;
        slider.direction = Slider.Direction.LeftToRight;

        var bgGo = new GameObject("Background");
        bgGo.transform.SetParent(go.transform, false);
        var bgImg = bgGo.AddComponent<Image>();
        bgImg.color = PanelDark;
        var bgRt = bgGo.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;

        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(go.transform, false);
        var faRt = fillArea.AddComponent<RectTransform>();
        faRt.anchorMin = Vector2.zero; faRt.anchorMax = Vector2.one;
        faRt.offsetMin = Vector2.zero; faRt.offsetMax = Vector2.zero;

        var fillGo = new GameObject("Fill");
        fillGo.transform.SetParent(fillArea.transform, false);
        fillImg = fillGo.AddComponent<Image>();
        fillImg.color = fillColor;
        var fillRt = fillGo.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = new Vector2(1, 1);
        fillRt.offsetMin = Vector2.zero; fillRt.offsetMax = Vector2.zero;

        slider.fillRect = fillRt;
        return slider;
    }

    // ─────────────────────────────────────────────────────────────
    // MAIN MENU — grilla 2x2 de categorías + Descansar
    // ─────────────────────────────────────────────────────────────
    GameObject BuildMainMenu(Transform parent)
    {
        var panel = new GameObject("MainMenu");
        panel.transform.SetParent(parent, false);
        var rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0.45f);
        rt.offsetMin = new Vector2(8, 68);
        rt.offsetMax = new Vector2(-8, 0);

        Transform p = panel.transform;

        // Contenedor de las 4 celdas en grilla 2x2
        // Fila superior
        MakeGridButton("ACTIVIDADES", "arcade-machine", false, p,
            new Vector2(0, 0.5f), new Vector2(0.5f, 1f), 4,
            () => ShowPanel(activitiesPanel));

        MakeGridButton("PARQUE", "tree", false, p,
            new Vector2(0.5f, 0.5f), new Vector2(1f, 1f), 4,
            () => ShowPanel(parkPanel));

        // Fila inferior
        MakeGridButton("SOCIAL", "feedback-emoji", false, p,
            new Vector2(0, 0), new Vector2(0.5f, 0.5f), 4,
            () => ShowPanel(socialPanel));

        MakeGridButton("DESCANSAR", "youtube", true, p,
            new Vector2(0.5f, 0), new Vector2(1f, 0.5f), 4,
            () => OnActivityClicked(actDescansar));

        return panel;
    }

    void MakeGridButton(string label, string iconName, bool highlighted,
        Transform parent, Vector2 anchorMin, Vector2 anchorMax, float padding,
        System.Action onClick)
    {
        Color bg  = highlighted ? ButtonHL : ButtonDark;
        Color txt = highlighted ? PanelDark : TextNeon;

        var go = new GameObject("Btn_" + label);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = bg;
        var btn = go.AddComponent<Button>();
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = new Vector2(padding, padding);
        rt.offsetMax = new Vector2(-padding, -padding);

        var colors = btn.colors;
        colors.normalColor = bg;
        colors.highlightedColor = Color.Lerp(bg, Neon, 0.35f);
        colors.pressedColor = Neon;
        btn.colors = colors;

        if (highlighted)
        {
            var outline = go.AddComponent<Outline>();
            outline.effectColor = Neon;
            outline.effectDistance = new Vector2(1.5f, -1.5f);
        }

        // Ícono como sprite (mitad superior)
        var iconGo = new GameObject("Icon");
        iconGo.transform.SetParent(go.transform, false);
        var iconRt = iconGo.AddComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.15f, 0.4f);
        iconRt.anchorMax = new Vector2(0.85f, 0.95f);
        iconRt.offsetMin = Vector2.zero;
        iconRt.offsetMax = Vector2.zero;

        // Cargar sprite desde Resources/Icons/
        var sprite = Resources.Load<Sprite>($"Icons/{iconName}");
        if (sprite != null)
        {
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.sprite = sprite;
            iconImg.color = Color.white;
            iconImg.preserveAspect = true;
        }
        else
        {
            // Fallback: texto centrado
            var fallback = iconGo.AddComponent<Text>();
            fallback.text = iconName.Length >= 3
                ? iconName.Substring(0, 3).ToUpper()
                : iconName.ToUpper();
            fallback.fontSize = 20;
            fallback.color = txt;
            fallback.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            fallback.alignment = TextAnchor.MiddleCenter;
            fallback.horizontalOverflow = HorizontalWrapMode.Overflow;
            fallback.verticalOverflow = VerticalWrapMode.Overflow;
        }

        // Label (parte inferior)
        var labelT = MakeText("Label", go.transform, label, 11, txt, FontStyle.Bold);
        labelT.rectTransform.anchorMin = new Vector2(0, 0);
        labelT.rectTransform.anchorMax = new Vector2(1, 0.4f);
        labelT.rectTransform.offsetMin = Vector2.zero;
        labelT.rectTransform.offsetMax = Vector2.zero;
        labelT.alignment = TextAnchor.MiddleCenter;

        btn.onClick.AddListener(() => onClick?.Invoke());
    }

    // ─────────────────────────────────────────────────────────────
    // SUBPANEL — lista de actividades con botón Volver
    // ─────────────────────────────────────────────────────────────
    GameObject BuildSubPanel(Transform parent, string panelName,
        (string label, ActivityData data, bool isHigh) a1,
        (string label, ActivityData data, bool isHigh) a2)
    {
        var panel = new GameObject(panelName);
        panel.transform.SetParent(parent, false);
        var rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0.45f);
        rt.offsetMin = new Vector2(8, 68);
        rt.offsetMax = new Vector2(-8, 0);
        panel.SetActive(false);

        Transform p = panel.transform;

        // Botón volver
        var backGo = new GameObject("Btn_Back");
        backGo.transform.SetParent(p, false);
        var backImg = backGo.AddComponent<Image>();
        backImg.color = ButtonDark;
        var backBtn = backGo.AddComponent<Button>();
        var backRt = backGo.GetComponent<RectTransform>();
        backRt.anchorMin = new Vector2(0, 1);
        backRt.anchorMax = new Vector2(0, 1);
        backRt.sizeDelta = new Vector2(100, 32);
        backRt.anchoredPosition = new Vector2(58, -18);
        var backTxt = MakeText("Lbl", backGo.transform, "← Volver", 11, TextDim, FontStyle.Normal);
        backTxt.rectTransform.anchorMin = Vector2.zero;
        backTxt.rectTransform.anchorMax = Vector2.one;
        backTxt.alignment = TextAnchor.MiddleCenter;
        backBtn.onClick.AddListener(ShowMainMenu);

        // Actividad 1
        if (a1.data != null)
        {
            var slot1 = new GameObject("Slot1");
            slot1.transform.SetParent(p, false);
            var s1rt = slot1.AddComponent<RectTransform>();
            s1rt.anchorMin = new Vector2(0, 0.5f);
            s1rt.anchorMax = new Vector2(1, 1);
            s1rt.offsetMin = new Vector2(0, 4);
            s1rt.offsetMax = new Vector2(0, -40);
            var b1 = MakeActivityButtonAnchored(a1.label, a1.data, a1.isHigh, s1rt);
            var cap1 = a1.data;
            b1.onClick.AddListener(() => OnActivityClicked(cap1));
        }

        // Actividad 2
        if (a2.data != null)
        {
            var slot2 = new GameObject("Slot2");
            slot2.transform.SetParent(p, false);
            var s2rt = slot2.AddComponent<RectTransform>();
            s2rt.anchorMin = new Vector2(0, 0);
            s2rt.anchorMax = new Vector2(1, 0.5f);
            s2rt.offsetMin = new Vector2(0, 4);
            s2rt.offsetMax = new Vector2(0, -4);
            var b2 = MakeActivityButtonAnchored(a2.label, a2.data, a2.isHigh, s2rt);
            var cap2 = a2.data;
            b2.onClick.AddListener(() => OnActivityClicked(cap2));
        }

        return panel;
    }

    Button MakeActivityButtonAnchored(string label, ActivityData data, bool isHigh, RectTransform slot)
    {
        Color btnColor = isHigh ? Hex("#1A2410") : ButtonDark;
        Color accent   = isHigh ? Neon : NeonDim;

        var go = new GameObject("ActivityBtn");
        go.transform.SetParent(slot, false);
        var img = go.AddComponent<Image>();
        img.color = btnColor;
        var btn = go.AddComponent<Button>();
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(4, 4);
        rt.offsetMax = new Vector2(-4, -4);

        var colors = btn.colors;
        colors.normalColor = btnColor;
        colors.highlightedColor = Color.Lerp(btnColor, Neon, 0.3f);
        colors.pressedColor = Neon;
        btn.colors = colors;

        Transform bt = go.transform;

        // Nombre
        var nameT = MakeText("Name", bt, label.ToUpper(), 15, accent, FontStyle.Bold);
        nameT.rectTransform.anchorMin = new Vector2(0, 0.55f);
        nameT.rectTransform.anchorMax = new Vector2(0.8f, 1);
        nameT.rectTransform.offsetMin = new Vector2(12, 0);
        nameT.rectTransform.offsetMax = Vector2.zero;
        nameT.alignment = TextAnchor.MiddleLeft;

        // Tag BAJO/ALTO
        string tierLabel = isHigh ? "[ ALTO ]" : "[ BAJO ]";
        var tierT = MakeText("Tier", bt, tierLabel, 10, accent, FontStyle.Normal);
        tierT.rectTransform.anchorMin = new Vector2(0.75f, 0.55f);
        tierT.rectTransform.anchorMax = new Vector2(1, 1);
        tierT.rectTransform.offsetMin = Vector2.zero;
        tierT.rectTransform.offsetMax = new Vector2(-8, 0);
        tierT.alignment = TextAnchor.MiddleRight;

        // Costo
        float cost = data != null ? data.EnergyCostBase : 0;
        float delta = data != null ? data.MoodDeltaBase : 0;
        var costT = MakeText("Cost", bt, $"⚡ {cost:F0}   +{delta:F0} 😊", 11, TextDim, FontStyle.Normal);
        costT.rectTransform.anchorMin = new Vector2(0, 0);
        costT.rectTransform.anchorMax = new Vector2(1, 0.5f);
        costT.rectTransform.offsetMin = Vector2.zero;
        costT.rectTransform.offsetMax = Vector2.zero;
        costT.alignment = TextAnchor.MiddleCenter;

        return btn;
    }

    // ─────────────────────────────────────────────────────────────
    // NAV BAR inferior
    // ─────────────────────────────────────────────────────────────
    void BuildNavBar(Transform parent)
    {
        var bar = MakeImage("NavBar", parent, PanelMid);
        bar.rectTransform.anchorMin = new Vector2(0, 0);
        bar.rectTransform.anchorMax = new Vector2(1, 0);
        bar.rectTransform.sizeDelta = new Vector2(0, 60);
        bar.rectTransform.anchoredPosition = new Vector2(0, 30);

        energyText = MakeText("EnergyNum", bar.rectTransform, "⚡ 42", 13, NeonDim, FontStyle.Normal);
        energyText.rectTransform.anchorMin = new Vector2(0, 0);
        energyText.rectTransform.anchorMax = new Vector2(0.5f, 1);
        energyText.rectTransform.offsetMin = Vector2.zero;
        energyText.rectTransform.offsetMax = Vector2.zero;
        energyText.alignment = TextAnchor.MiddleCenter;

        moodText = MakeText("MoodNum", bar.rectTransform, "😊 50", 13, Neon, FontStyle.Normal);
        moodText.rectTransform.anchorMin = new Vector2(0.5f, 0);
        moodText.rectTransform.anchorMax = new Vector2(1, 1);
        moodText.rectTransform.offsetMin = Vector2.zero;
        moodText.rectTransform.offsetMax = Vector2.zero;
        moodText.alignment = TextAnchor.MiddleCenter;
    }

    // ─────────────────────────────────────────────────────────────
    // OVERLAYS
    // ─────────────────────────────────────────────────────────────
    GameObject BuildDayTransition(Transform root)
    {
        var panel = MakeImage("DayTransition", root, new Color(0, 0, 0, 0.85f));
        panel.rectTransform.anchorMin = Vector2.zero;
        panel.rectTransform.anchorMax = Vector2.one;
        panel.rectTransform.offsetMin = Vector2.zero;
        panel.rectTransform.offsetMax = Vector2.zero;

        dayTransLabel = MakeText("Label", panel.transform, "Fin del día 1", 28, Neon, FontStyle.Bold);
        dayTransLabel.rectTransform.anchoredPosition = Vector2.zero;
        dayTransLabel.rectTransform.sizeDelta = new Vector2(400, 60);
        dayTransLabel.alignment = TextAnchor.MiddleCenter;

        panel.gameObject.SetActive(false);
        return panel.gameObject;
    }

    GameObject BuildGameOverPanel(Transform root)
    {
        var panel = MakeImage("GameOverPanel", root, new Color(0.05f, 0f, 0f, 0.95f));
        panel.rectTransform.anchorMin = Vector2.zero;
        panel.rectTransform.anchorMax = Vector2.one;
        panel.rectTransform.offsetMin = Vector2.zero;
        panel.rectTransform.offsetMax = Vector2.zero;

        gameOverLabel = MakeText("Label", panel.transform,
            "INESTABLE\nGame Over", 26, Hex("#FF4444"), FontStyle.Bold);
        gameOverLabel.rectTransform.anchoredPosition = new Vector2(0, 60);
        gameOverLabel.rectTransform.sizeDelta = new Vector2(400, 100);
        gameOverLabel.alignment = TextAnchor.MiddleCenter;

        var btn = MakeButton("REINTENTAR", panel.transform, ButtonDark, Neon, false,
            new Vector2(0, -60), 200, 54);
        btn.onClick.AddListener(() =>
            UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex));

        panel.gameObject.SetActive(false);
        return panel.gameObject;
    }

    GameObject BuildVictoryPanel(Transform root)
    {
        var panel = MakeImage("VictoryPanel", root, new Color(0f, 0.05f, 0f, 0.95f));
        panel.rectTransform.anchorMin = Vector2.zero;
        panel.rectTransform.anchorMax = Vector2.one;
        panel.rectTransform.offsetMin = Vector2.zero;
        panel.rectTransform.offsetMax = Vector2.zero;

        var label = MakeText("Label", panel.transform,
            "¡30 DÍAS!\n¡VICTORIA!", 28, Neon, FontStyle.Bold);
        label.rectTransform.anchoredPosition = new Vector2(0, 60);
        label.rectTransform.sizeDelta = new Vector2(400, 100);
        label.alignment = TextAnchor.MiddleCenter;

        var btn = MakeButton("JUGAR DE NUEVO", panel.transform, ButtonDark, Neon, false,
            new Vector2(0, -60), 240, 54);
        btn.onClick.AddListener(() =>
            UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex));

        panel.gameObject.SetActive(false);
        return panel.gameObject;
    }

    // ─────────────────────────────────────────────────────────────
    // HUD REFRESH
    // ─────────────────────────────────────────────────────────────
    void RefreshHUD()
    {
        if (!uiReady) return;
        var gm = GameManager.Instance;

        if (moodSlider)   moodSlider.value   = gm.Mood   / 100f;
        if (energySlider) energySlider.value = gm.Energy / 100f;
        if (moodText)     moodText.text   = $"😊 {gm.Mood:F0}";
        if (energyText)   energyText.text = $"⚡ {gm.Energy:F0}";
        if (dayText)      dayText.text    = $"DÍA {gm.CurrentDay}/{gm.TotalDays}";

        if (streakText)
        {
            streakText.text = gm.UnstableDaysStreak > 0
                ? $"⚠ {gm.UnstableDaysStreak}/3 días inestables"
                : "";
        }

        // Color de barra de ánimo: verde si safe, rojo si fuera
        if (moodFill)
        {
            bool safe = gm.Mood >= gm.MoodSafeMin && gm.Mood <= gm.MoodSafeMax;
            moodFill.color = safe ? Neon : Hex("#FF4444");
        }
    }

    // ─────────────────────────────────────────────────────────────
    // NAVIGATION
    // ─────────────────────────────────────────────────────────────
    void ShowMainMenu()
    {
        mainMenu?.SetActive(true);
        activitiesPanel?.SetActive(false);
        parkPanel?.SetActive(false);
        socialPanel?.SetActive(false);
    }

    void ShowPanel(GameObject panel)
    {
        mainMenu?.SetActive(false);
        activitiesPanel?.SetActive(false);
        parkPanel?.SetActive(false);
        socialPanel?.SetActive(false);
        panel?.SetActive(true);
    }

    void OnActivityClicked(ActivityData activity)
    {
        if (activity == null) return;
        var gm = GameManager.Instance;
        if (!activity.IsRestActivity && !gm.CanAfford(activity)) return;

        var anim = CharacterAnimator.Instance;
        if (anim != null)
        {
            anim.PlayActivity(activity, () =>
            {
                gm.ApplyActivity(activity);
                ShowMainMenu();
            });
        }
        else
        {
            gm.ApplyActivity(activity);
            ShowMainMenu();
        }
    }

    void ShowDayTransition()
    {
        if (dayTransPanel == null) return;
        dayTransPanel.SetActive(true);
        if (dayTransLabel != null)
            dayTransLabel.text = $"FIN DEL DÍA {GameManager.Instance.CurrentDay}";
        Invoke(nameof(HideDayTransition), 2f);
    }

    void HideDayTransition() => dayTransPanel?.SetActive(false);
    void ShowGameOver()      => gameOverPanel?.SetActive(true);
    void ShowVictory()       => victoryPanel?.SetActive(true);

    // ─────────────────────────────────────────────────────────────
    // HELPERS DE CONSTRUCCIÓN UI
    // ─────────────────────────────────────────────────────────────
    Image MakeImage(string name, Transform parent, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        return img;
    }

    Image MakePanel(string name, Transform parent, Color color, float w, float h)
    {
        var img = MakeImage(name, parent, color);
        img.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        img.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        img.rectTransform.sizeDelta = new Vector2(w, h);
        img.rectTransform.anchoredPosition = Vector2.zero;
        return img;
    }

    RectTransform MakeRect(string name, Transform parent, float w, float h)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = Vector2.zero;
        return rt;
    }

    Text MakeText(string name, Transform parent, string content,
        int size, Color color, FontStyle style)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.text = content;
        t.fontSize = size;
        t.color = color;
        t.fontStyle = style;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.supportRichText = false;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }

    void MakeCategoryButton(string label, string icon, bool highlighted,
        RectTransform parentRt, Vector2 anchorCenter, System.Action onClick)
    {
        Color bg  = highlighted ? ButtonHL : ButtonDark;
        Color txt = highlighted ? PanelDark : TextNeon;

        var go = new GameObject("Btn_" + label);
        go.transform.SetParent(parentRt, false);
        var img = go.AddComponent<Image>();
        img.color = bg;
        var btn = go.AddComponent<Button>();
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(anchorCenter.x - 0.24f, 0.04f);
        rt.anchorMax = new Vector2(anchorCenter.x + 0.24f, 0.96f);
        rt.offsetMin = new Vector2(4, 0);
        rt.offsetMax = new Vector2(-4, 0);

        var colors = btn.colors;
        colors.normalColor = bg;
        colors.highlightedColor = Color.Lerp(bg, Neon, 0.3f);
        colors.pressedColor = Neon;
        btn.colors = colors;

        if (highlighted)
        {
            var outline = go.AddComponent<Outline>();
            outline.effectColor = Neon;
            outline.effectDistance = new Vector2(1.5f, -1.5f);
        }

        // Ícono
        var iconT = MakeText("Icon", go.transform, icon, 24, txt, FontStyle.Normal);
        iconT.rectTransform.anchorMin = new Vector2(0, 0.5f);
        iconT.rectTransform.anchorMax = new Vector2(1, 1);
        iconT.rectTransform.offsetMin = Vector2.zero;
        iconT.rectTransform.offsetMax = Vector2.zero;
        iconT.alignment = TextAnchor.MiddleCenter;

        // Label
        var labelT = MakeText("Label", go.transform, label, 11, txt, FontStyle.Bold);
        labelT.rectTransform.anchorMin = new Vector2(0, 0);
        labelT.rectTransform.anchorMax = new Vector2(1, 0.5f);
        labelT.rectTransform.offsetMin = Vector2.zero;
        labelT.rectTransform.offsetMax = Vector2.zero;
        labelT.alignment = TextAnchor.MiddleCenter;

        btn.onClick.AddListener(() => onClick?.Invoke());
    }

    Button MakeButton(string label, Transform parent, Color bgColor, Color textColor,
        bool border, Vector2 pos, float w, float h)
    {
        var go = new GameObject("Btn_" + label);
        go.transform.SetParent(parent, false);

        var img = go.AddComponent<Image>();
        img.color = bgColor;

        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor      = bgColor;
        colors.highlightedColor = Color.Lerp(bgColor, Neon, 0.3f);
        colors.pressedColor     = Neon;
        colors.selectedColor    = bgColor;
        btn.colors = colors;

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = pos;

        if (!string.IsNullOrEmpty(label))
        {
            var t = MakeText("Label", go.transform, label, 13, textColor, FontStyle.Bold);
            t.rectTransform.anchorMin = Vector2.zero;
            t.rectTransform.anchorMax = Vector2.one;
            t.rectTransform.offsetMin = Vector2.zero;
            t.rectTransform.offsetMax = Vector2.zero;
            t.alignment = TextAnchor.MiddleCenter;
        }

        // Borde neón si es categoría activa
        if (border)
        {
            var outline = go.AddComponent<Outline>();
            outline.effectColor = Neon;
            outline.effectDistance = new Vector2(1.5f, -1.5f);
        }

        return btn;
    }

    void PlaceAnchored(RectTransform rt, Vector2 anchor, Vector2 pos)
    {
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.anchoredPosition = pos;
    }

    void RoundedCorners(Image img)
    {
        // Unity built-in no tiene corner radius — se simula con sprite o dejamos sharp
        // Para jam está bien, se puede mejorar con UI Toolkit o un sprite 9-slice luego
    }

    static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}
