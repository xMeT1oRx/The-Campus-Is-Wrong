using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Пауза. GameManager создаёт её автоматически.
/// Escape — открыть/закрыть. Во время паузы Time.timeScale = 0.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance { get; private set; }

    public bool IsPaused { get; private set; }

    [Tooltip("Имя главной сцены меню")]
    public string mainMenuScene = "MainMenu";

    private GameObject root;          // корень всего UI паузы
    private GameObject pausePanel;
    private GameObject settingsPanel;
    private TMP_Text   sensValueLabel;

    const float SensMin = 20f, SensMax = 300f, SensDefault = 100f;

    // ── палитра ──────────────────────────────────────────────────
    static readonly Color C_Overlay  = new Color(0f,    0f,    0f,    0.65f);
    static readonly Color C_Panel    = new Color(0.07f, 0.06f, 0.10f, 0.97f);
    static readonly Color C_Red      = new Color(0.68f, 0.10f, 0.10f);
    static readonly Color C_Title    = new Color(0.90f, 0.82f, 0.65f);
    static readonly Color C_Sub      = new Color(0.48f, 0.43f, 0.38f);
    static readonly Color C_BtnNorm  = new Color(0.13f, 0.10f, 0.17f);
    static readonly Color C_BtnHover = new Color(0.26f, 0.08f, 0.08f);
    static readonly Color C_BtnPress = new Color(0.06f, 0.03f, 0.05f);
    static readonly Color C_BtnText  = new Color(0.90f, 0.88f, 0.85f);

    // ── жизненный цикл ───────────────────────────────────────────
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        BuildUI();
        SetPaused(false);
    }

    void Update()
    {
        // Escape: пауза только если игра идёт или уже на паузе
        // (не реагируем если показан win/lose экран)
        var gm = GameManager.Instance;
        if (gm != null && gm.State != GameManager.GameState.Playing) return;

        if (Input.GetKeyDown(KeyCode.Escape))
            SetPaused(!IsPaused);
    }

    // ── публичное управление ─────────────────────────────────────
    public void SetPaused(bool pause)
    {
        IsPaused = pause;
        root.SetActive(pause);

        if (pause)
        {
            Time.timeScale       = 0f;
            Cursor.lockState     = CursorLockMode.None;
            Cursor.visible       = true;
            pausePanel.SetActive(true);
            settingsPanel.SetActive(false);
        }
        else
        {
            Time.timeScale       = 1f;
            Cursor.lockState     = CursorLockMode.Locked;
            Cursor.visible       = false;
        }
    }

    // ── построение UI ────────────────────────────────────────────
    void BuildUI()
    {
        // Canvas поверх всего
        var cGo    = new GameObject("PauseCanvas");
        var canvas = cGo.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var sc = cGo.AddComponent<CanvasScaler>();
        sc.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920, 1080);
        cGo.AddComponent<GraphicRaycaster>();

        // Корень — полноэкранный overlay
        root = new GameObject("PauseRoot");
        root.transform.SetParent(cGo.transform, false);
        var rootImg = root.AddComponent<Image>();
        rootImg.color         = C_Overlay;
        rootImg.raycastTarget = true;   // блокирует клики на игру
        Fill(root.GetComponent<RectTransform>());

        // ── ПАНЕЛЬ ПАУЗЫ ──────────────────────────────────────
        pausePanel = MakeCard(root.transform, new Vector2(420f, 460f));

        TopBar(pausePanel.transform, C_Red);
        TxtLbl(pausePanel.transform, "ПАУЗА",
               36f, FontStyles.Bold, C_Title, new Vector2(0f, 170f), new Vector2(390f, 52f));
        Divider(pausePanel.transform, new Vector2(0f, 135f), 320f);

        MakeBtn(pausePanel.transform, "▶   ПРОДОЛЖИТЬ",
                new Vector2(0f,  72f), () => SetPaused(false),
                C_Red, new Color(0.88f, 0.15f, 0.15f));

        MakeBtn(pausePanel.transform, "↺   ПЕРЕЗАПУСТИТЬ",
                new Vector2(0f,  12f), OnRestart,     C_BtnNorm, C_BtnHover);

        MakeBtn(pausePanel.transform, "⚙   НАСТРОЙКИ",
                new Vector2(0f, -48f), OnOpenSettings, C_BtnNorm, C_BtnHover);

        MakeBtn(pausePanel.transform, "⌂   ГЛАВНОЕ МЕНЮ",
                new Vector2(0f,-108f), OnMainMenu,    C_BtnNorm, C_BtnHover);

        MakeBtn(pausePanel.transform, "✕   ВЫЙТИ",
                new Vector2(0f,-168f), () => Application.Quit(), C_BtnNorm, C_BtnHover);

        // ── ПАНЕЛЬ НАСТРОЕК ───────────────────────────────────
        settingsPanel = MakeCard(root.transform, new Vector2(420f, 320f));
        settingsPanel.SetActive(false);

        TopBar(settingsPanel.transform, C_Red);
        TxtLbl(settingsPanel.transform, "НАСТРОЙКИ",
               30f, FontStyles.Bold, C_Title, new Vector2(0f, 106f), new Vector2(390f, 46f));
        Divider(settingsPanel.transform, new Vector2(0f, 72f), 320f);

        TxtLbl(settingsPanel.transform, "Чувствительность мыши",
               16f, FontStyles.Normal, C_Sub, new Vector2(0f, 38f), new Vector2(390f, 26f));

        float saved = PlayerPrefs.GetFloat("MouseSensitivity", SensDefault);
        var slider  = MakeSlider(settingsPanel.transform, new Vector2(0f, 0f), SensMin, SensMax, saved);
        slider.onValueChanged.AddListener(OnSensChanged);

        // числовое значение
        var vGo = new GameObject("SensVal");
        vGo.transform.SetParent(settingsPanel.transform, false);
        sensValueLabel           = vGo.AddComponent<TextMeshProUGUI>();
        sensValueLabel.text      = Mathf.RoundToInt(saved).ToString();
        sensValueLabel.fontSize  = 16f;
        sensValueLabel.alignment = TextAlignmentOptions.Center;
        sensValueLabel.color     = new Color(0.65f, 0.20f, 0.20f);
        sensValueLabel.raycastTarget = false;
        var svRt = vGo.GetComponent<RectTransform>();
        svRt.anchorMin = svRt.anchorMax = new Vector2(0.5f, 0.5f);
        svRt.sizeDelta        = new Vector2(80f, 26f);
        svRt.anchoredPosition = new Vector2(0f, -30f);

        MakeBtn(settingsPanel.transform, "←   НАЗАД",
                new Vector2(0f, -96f), OnCloseSettings, C_BtnNorm, C_BtnHover);
    }

    // ── обработчики ──────────────────────────────────────────────
    void OnRestart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    void OnOpenSettings()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    void OnCloseSettings()
    {
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    void OnSensChanged(float v)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", v);
        PlayerPrefs.Save();
        if (sensValueLabel) sensValueLabel.text = Mathf.RoundToInt(v).ToString();

        // Применяем сразу, без перезапуска
        var cc = FindObjectOfType<CameraController>();
        if (cc != null) cc.mouseSensitivity = v;
    }

    // ── строители ────────────────────────────────────────────────
    static GameObject MakeCard(Transform parent, Vector2 size)
    {
        var go = new GameObject("Card");
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color         = C_Panel;
        img.raycastTarget = false;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta        = size;
        rt.anchoredPosition = Vector2.zero;
        return go;
    }

    static void TopBar(Transform parent, Color color)
    {
        var go = new GameObject("TopBar");
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color         = color;
        img.raycastTarget = false;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.offsetMin = new Vector2(0f, -4f);
        rt.offsetMax = Vector2.zero;
    }

    static void Divider(Transform parent, Vector2 pos, float width)
    {
        var go = new GameObject("Div");
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color         = new Color(0.55f, 0.08f, 0.08f, 0.45f);
        img.raycastTarget = false;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta        = new Vector2(width, 1f);
        rt.anchoredPosition = pos;
    }

    static void TxtLbl(Transform parent, string text, float size, FontStyles style,
                        Color color, Vector2 pos, Vector2 sz)
    {
        var go = new GameObject("Lbl");
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text          = text;
        tmp.fontSize      = size;
        tmp.fontStyle     = style;
        tmp.alignment     = TextAlignmentOptions.Center;
        tmp.color         = color;
        tmp.raycastTarget = false;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta        = sz;
        rt.anchoredPosition = pos;
    }

    static void MakeBtn(Transform parent, string label, Vector2 pos,
                         UnityEngine.Events.UnityAction action, Color normal, Color hover)
    {
        var go = new GameObject("Btn_" + label.Trim());
        go.transform.SetParent(parent, false);

        var img = go.AddComponent<Image>();
        img.color = Color.white;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.colors = new ColorBlock
        {
            normalColor      = normal,
            highlightedColor = hover,
            pressedColor     = C_BtnPress,
            selectedColor    = normal,
            disabledColor    = new Color(0.3f, 0.3f, 0.3f, 0.5f),
            colorMultiplier  = 1f,
            fadeDuration     = 0.08f
        };
        btn.onClick.AddListener(action);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta        = new Vector2(310f, 44f);
        rt.anchoredPosition = pos;

        // Красная левая черта
        var bar = new GameObject("Bar");
        bar.transform.SetParent(go.transform, false);
        var bImg = bar.AddComponent<Image>();
        bImg.color         = new Color(0.68f, 0.10f, 0.10f, 0.75f);
        bImg.raycastTarget = false;
        var bRt = bar.GetComponent<RectTransform>();
        bRt.anchorMin = Vector2.zero;
        bRt.anchorMax = new Vector2(0f, 1f);
        bRt.offsetMin = Vector2.zero;
        bRt.offsetMax = new Vector2(4f, 0f);

        // Текст
        var tGo = new GameObject("Lbl");
        tGo.transform.SetParent(go.transform, false);
        var tmp = tGo.AddComponent<TextMeshProUGUI>();
        tmp.text          = label;
        tmp.fontSize      = 18f;
        tmp.fontStyle     = FontStyles.Bold;
        tmp.alignment     = TextAlignmentOptions.Left;
        tmp.color         = C_BtnText;
        tmp.raycastTarget = false;
        var tRt = tGo.GetComponent<RectTransform>();
        tRt.anchorMin = Vector2.zero;
        tRt.anchorMax = Vector2.one;
        tRt.offsetMin = new Vector2(16f, 0f);
        tRt.offsetMax = Vector2.zero;
    }

    static Slider MakeSlider(Transform parent, Vector2 pos, float min, float max, float val)
    {
        var go = new GameObject("Slider");
        go.transform.SetParent(parent, false);
        var slider = go.AddComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.value    = val;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta        = new Vector2(330f, 22f);
        rt.anchoredPosition = pos;

        // Track
        var track = new GameObject("Track");
        track.transform.SetParent(go.transform, false);
        track.AddComponent<Image>().color = new Color(0.20f, 0.17f, 0.24f);
        track.GetComponent<Image>().raycastTarget = false;
        var tRt = track.GetComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0f, 0.3f);
        tRt.anchorMax = new Vector2(1f, 0.7f);
        tRt.offsetMin = tRt.offsetMax = Vector2.zero;

        // Fill Area
        var fa = new GameObject("FillArea");
        fa.transform.SetParent(go.transform, false);
        var faRt = fa.AddComponent<RectTransform>();
        faRt.anchorMin = new Vector2(0f, 0.3f);
        faRt.anchorMax = new Vector2(1f, 0.7f);
        faRt.offsetMin = new Vector2(4f, 0f);
        faRt.offsetMax = new Vector2(-12f, 0f);
        var fill = new GameObject("Fill");
        fill.transform.SetParent(fa.transform, false);
        var fImg = fill.AddComponent<Image>();
        fImg.color         = C_Red;
        fImg.raycastTarget = false;
        var fRt = fill.GetComponent<RectTransform>();
        fRt.anchorMin = Vector2.zero;
        fRt.anchorMax = Vector2.one;
        fRt.offsetMin = fRt.offsetMax = Vector2.zero;
        slider.fillRect = fRt;

        // Handle
        var ha = new GameObject("HandleArea");
        ha.transform.SetParent(go.transform, false);
        var haRt = ha.AddComponent<RectTransform>();
        haRt.anchorMin = Vector2.zero;
        haRt.anchorMax = Vector2.one;
        haRt.offsetMin = new Vector2(8f, 0f);
        haRt.offsetMax = new Vector2(-8f, 0f);
        var handle = new GameObject("Handle");
        handle.transform.SetParent(ha.transform, false);
        var hImg = handle.AddComponent<Image>();
        hImg.color = Color.white;
        var hRt = handle.GetComponent<RectTransform>();
        hRt.sizeDelta = new Vector2(14f, 14f);
        slider.handleRect    = hRt;
        slider.targetGraphic = hImg;

        return slider;
    }

    static void Fill(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}
