using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Tooltip("Имя игровой сцены в Build Settings")]
    public string gameSceneName = "SampleScene";

    [Tooltip("Изображение фона главного меню (автоматически загружается 'BG' из Resources или перетащите вручную)")]
    public Sprite backgroundSprite;

    private GameObject mainPanel;
    private GameObject settingsPanel;
    private TMP_Text   sensLabel;
    private Slider     musicSlider;
    private TMP_Text   musicValueLabel;

    const float SensMin = 20f, SensMax = 300f, SensDefault = 100f;
    const float VolMin = 0f, VolMax = 100f;

    // ── палитра ──────────────────────────────────────────────────
    static readonly Color C_BG       = new Color(212f/255f, 212f/255f, 212f/255f);
    static readonly Color C_Panel    = new Color(0.09f, 0.07f, 0.12f, 0.97f);
    static readonly Color C_Red      = new Color(0.72f, 0.10f, 0.10f);
    static readonly Color C_Title    = new Color(0.92f, 0.82f, 0.65f);
    static readonly Color C_Sub      = new Color(0.48f, 0.43f, 0.38f);
    static readonly Color C_BtnNorm  = new Color(0.14f, 0.11f, 0.18f);
    static readonly Color C_BtnHover = new Color(0.28f, 0.08f, 0.08f);
    static readonly Color C_BtnPress = new Color(0.07f, 0.04f, 0.05f);
    static readonly Color C_BtnText  = new Color(0.90f, 0.88f, 0.85f);

    // ── жизненный цикл ───────────────────────────────────────────
    void Awake()
    {
        // Автоматически загружаем спрайт "BG" из папки Resources если не задан вручную
        if (backgroundSprite == null)
        {
            backgroundSprite = Resources.Load<Sprite>("BG");
        }

        // EventSystem ОБЯЗАТЕЛЕН — без него кнопки не работают
        if (FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        BuildUI();
    }

    // Держим курсор разлокированным каждый кадр —
    // некоторые сцены/скрипты могут сбросить его при переходе
    void Update()
    {
        if (Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }
    }

    // ── построение UI ────────────────────────────────────────────
    void BuildUI()
    {
        // Canvas
        var cGo    = new GameObject("MenuCanvas");
        var canvas = cGo.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        var sc = cGo.AddComponent<CanvasScaler>();
        sc.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920, 1080);
        cGo.AddComponent<GraphicRaycaster>();

        // Фон
        DecoImg(cGo.transform, Color.white, fill: true, backgroundSprite);

        // Тонкая красная полоска слева экрана
        var stripeGo = new GameObject("LeftStripe");
        stripeGo.transform.SetParent(cGo.transform, false);
        var si = stripeGo.AddComponent<Image>();
        si.color         = new Color(0.55f, 0.05f, 0.05f, 0.45f);
        si.raycastTarget = false;
        var sRt = stripeGo.GetComponent<RectTransform>();
        sRt.anchorMin = Vector2.zero;
        sRt.anchorMax = new Vector2(0f, 1f);
        sRt.offsetMin = Vector2.zero;
        sRt.offsetMax = new Vector2(5f, 0f);

        // ── ГЛАВНАЯ ПАНЕЛЬ ──────────────────────────────────────
        mainPanel = MakeCard(cGo.transform, new Vector2(460f, 450f));

        TopBar(mainPanel.transform, C_Red);
        TxtLabel(mainPanel.transform, "THE CAMPUS", 48f, FontStyles.Bold,
                 C_Red,   new Vector2(0f, 162f), new Vector2(430f, 62f));
        TxtLabel(mainPanel.transform, "IS WRONG",   30f, FontStyles.Bold,
                 C_Title, new Vector2(0f, 118f), new Vector2(430f, 44f));
        Divider(mainPanel.transform, new Vector2(0f, 82f), 340f);
        TxtLabel(mainPanel.transform, "найди аномалию. не ошибись.",
                 14f, FontStyles.Italic, C_Sub, new Vector2(0f, 56f), new Vector2(430f, 24f));

        MakeBtn(mainPanel.transform, "▶   ИГРАТЬ",    new Vector2(0f,  -10f), OnPlay,
                C_Red,      new Color(0.90f, 0.15f, 0.15f));
        MakeBtn(mainPanel.transform, "⚙   НАСТРОЙКИ", new Vector2(0f,  -68f), OnSettings,
                C_BtnNorm,  C_BtnHover);
        MakeBtn(mainPanel.transform, "✕   ВЫЙТИ",     new Vector2(0f, -126f), () => Application.Quit(),
                C_BtnNorm,  C_BtnHover);

        // ── ПАНЕЛЬ НАСТРОЕК ─────────────────────────────────────
        settingsPanel = MakeCard(cGo.transform, new Vector2(500f, 500f));
        settingsPanel.SetActive(false);

        TopBar(settingsPanel.transform, C_Red);
        TxtLabel(settingsPanel.transform, "НАСТРОЙКИ", 34f, FontStyles.Bold,
                 C_Title, new Vector2(0f, 190f), new Vector2(430f, 50f));
        Divider(settingsPanel.transform, new Vector2(0f, 154f), 340f);

        TxtLabel(settingsPanel.transform, "Чувствительность мыши",
                 16f, FontStyles.Normal, new Color(0.65f, 0.60f, 0.55f),
                 new Vector2(0f, 120f), new Vector2(430f, 28f));

        float saved = PlayerPrefs.GetFloat("MouseSensitivity", SensDefault);
        var slider  = MakeSlider(settingsPanel.transform, new Vector2(0f, 78f), SensMin, SensMax, saved);
        slider.onValueChanged.AddListener(OnSensChanged);

        // Числовое значение
        var vGo = new GameObject("SensVal");
        vGo.transform.SetParent(settingsPanel.transform, false);
        sensLabel           = vGo.AddComponent<TextMeshProUGUI>();
        sensLabel.text      = Mathf.RoundToInt(saved).ToString();
        sensLabel.fontSize  = 17f;
        sensLabel.alignment = TextAlignmentOptions.Center;
        sensLabel.color     = new Color(0.65f, 0.20f, 0.20f);
        sensLabel.raycastTarget = false;
        var vRt = vGo.GetComponent<RectTransform>();
        vRt.anchorMin = vRt.anchorMax = new Vector2(0.5f, 0.5f);
        vRt.sizeDelta        = new Vector2(80f, 28f);
        vRt.anchoredPosition = new Vector2(0f, 40f);

        Divider(settingsPanel.transform, new Vector2(0f, 20f), 340f);

        // Громкость музыки
        TxtLabel(settingsPanel.transform, "Громкость музыки",
                 16f, FontStyles.Normal, new Color(0.65f, 0.60f, 0.55f),
                 new Vector2(0f, -42f), new Vector2(430f, 28f));

        float savedMusic = PlayerPrefs.GetFloat("MusicVolume", 0.5f) * 100f;
        musicSlider = MakeSlider(settingsPanel.transform, new Vector2(0f, -84f), VolMin, VolMax, savedMusic);
        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        // Числовое значение громкости
        var mvGo = new GameObject("MusicVal");
        mvGo.transform.SetParent(settingsPanel.transform, false);
        musicValueLabel           = mvGo.AddComponent<TextMeshProUGUI>();
        musicValueLabel.text      = Mathf.RoundToInt(savedMusic).ToString();
        musicValueLabel.fontSize  = 17f;
        musicValueLabel.alignment = TextAlignmentOptions.Center;
        musicValueLabel.color     = new Color(0.65f, 0.20f, 0.20f);
        musicValueLabel.raycastTarget = false;
        var mvRt = mvGo.GetComponent<RectTransform>();
        mvRt.anchorMin = mvRt.anchorMax = new Vector2(0.5f, 0.5f);
        mvRt.sizeDelta        = new Vector2(80f, 28f);
        mvRt.anchoredPosition = new Vector2(0f, -124f);

        MakeBtn(settingsPanel.transform, "←   НАЗАД", new Vector2(0f, -200f), OnBack,
                C_BtnNorm, C_BtnHover);
    }

    // ── обработчики ──────────────────────────────────────────────
    void OnPlay()     => SceneManager.LoadScene(gameSceneName);
    void OnSettings() { mainPanel.SetActive(false); settingsPanel.SetActive(true); }
    void OnBack()     { settingsPanel.SetActive(false); mainPanel.SetActive(true); }

    void OnSensChanged(float v)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", v);
        PlayerPrefs.Save();
        if (sensLabel) sensLabel.text = Mathf.RoundToInt(v).ToString();
    }

    void OnMusicVolumeChanged(float v)
    {
        float volume = v / 100f;
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
        if (musicValueLabel) musicValueLabel.text = Mathf.RoundToInt(v).ToString();

        // Применить громкость через AudioManager или напрямую
        var am = FindObjectOfType<AudioManager>();
        if (am != null)
        {
            am.SetMusicVolume(volume);
        }
        else
        {
            // Если AudioManager нет, применить напрямую к камере
            var mainCamera = Camera.main;
            if (mainCamera != null && mainCamera.TryGetComponent<AudioSource>(out var audioSource))
            {
                audioSource.volume = volume;
            }
        }
    }

    // ── строители элементов ──────────────────────────────────────

    static GameObject MakeCard(Transform parent, Vector2 size)
    {
        var go = new GameObject("Card");
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color         = C_Panel;
        img.raycastTarget = false;           // сама карточка не перехватывает клики
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin        = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta        = size;
        rt.anchoredPosition = Vector2.zero;
        return go;
    }

    // Красная линия-акцент поверх карточки
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
        img.color         = new Color(0.60f, 0.10f, 0.10f, 0.50f);
        img.raycastTarget = false;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin        = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta        = new Vector2(width, 1f);
        rt.anchoredPosition = pos;
    }

    static void TxtLabel(Transform parent, string text, float size, FontStyles style,
                          Color color, Vector2 pos, Vector2 sz)
    {
        var go  = new GameObject("Txt");
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text            = text;
        tmp.fontSize        = size;
        tmp.fontStyle       = style;
        tmp.alignment       = TextAlignmentOptions.Center;
        tmp.color           = color;
        tmp.raycastTarget   = false;         // текст не перехватывает клики
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin        = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta        = sz;
        rt.anchoredPosition = pos;
    }

    // Кнопка. img.color = white, colorBlock задаёт нужные цвета напрямую
    static void MakeBtn(Transform parent, string label, Vector2 pos,
                         UnityEngine.Events.UnityAction action, Color normal, Color hover)
    {
        var go = new GameObject("Btn_" + label.Trim());
        go.transform.SetParent(parent, false);

        // Image.color = white, чтобы ColorBlock работал правильно (цвет = colorBlock × 1)
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
        rt.anchorMin        = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta        = new Vector2(310f, 46f);
        rt.anchoredPosition = pos;

        // Левая красная черта на кнопке (декоративная, не перехватывает)
        var bar = new GameObject("Bar");
        bar.transform.SetParent(go.transform, false);
        var bImg = bar.AddComponent<Image>();
        bImg.color         = new Color(0.72f, 0.10f, 0.10f, 0.80f);
        bImg.raycastTarget = false;
        var bRt = bar.GetComponent<RectTransform>();
        bRt.anchorMin = Vector2.zero;
        bRt.anchorMax = new Vector2(0f, 1f);
        bRt.offsetMin = Vector2.zero;
        bRt.offsetMax = new Vector2(4f, 0f);

        // Текст кнопки
        var tGo = new GameObject("Lbl");
        tGo.transform.SetParent(go.transform, false);
        var tmp = tGo.AddComponent<TextMeshProUGUI>();
        tmp.text          = label;
        tmp.fontSize      = 19f;
        tmp.fontStyle     = FontStyles.Bold;
        tmp.alignment     = TextAlignmentOptions.Left;
        tmp.color         = C_BtnText;
        tmp.raycastTarget = false;           // текст не перехватывает клики
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
        rt.anchorMin        = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta        = new Vector2(340f, 22f);
        rt.anchoredPosition = pos;

        // Track
        var track = new GameObject("Track");
        track.transform.SetParent(go.transform, false);
        var tImg = track.AddComponent<Image>();
        tImg.color         = new Color(0.20f, 0.17f, 0.24f);
        tImg.raycastTarget = false;
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

        // Handle Area
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

    // Декоративный Image — raycastTarget = false чтобы не блокировал клики
    static void DecoImg(Transform parent, Color color, bool fill = false, Sprite sprite = null)
    {
        var go = new GameObject("Deco");
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color         = sprite != null ? Color.white : color;
        img.raycastTarget = false;
        if (sprite != null)
        {
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
        }
        if (fill)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
    }
}
