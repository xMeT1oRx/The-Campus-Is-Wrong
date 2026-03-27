using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Singleton HUD. GameManager создаёт его автоматически в Awake —
/// вручную добавлять в сцену не нужно.
/// </summary>
public class HUD : MonoBehaviour
{
    public static HUD Instance { get; private set; }

    private GameObject promptGo;
    private TMP_Text   promptText;
    private GameObject winPanel;
    private GameObject losePanel;
    private TMP_Text   timerText;

    private float elapsed = 0f;
    private bool  running = true;

    // ── палитра ──────────────────────────────────────────────────
    static readonly Color C_Win  = new Color(0.15f, 0.80f, 0.35f);
    static readonly Color C_Lose = new Color(0.85f, 0.15f, 0.15f);
    static readonly Color C_Dark = new Color(0.06f, 0.05f, 0.09f, 0.95f);

    // ── жизненный цикл ───────────────────────────────────────────
    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        Build();
    }

    void Update()
    {
        if (!running) return;
        elapsed += Time.deltaTime;
        if (timerText != null)
            timerText.text = FormatTime(elapsed);
    }

    static string FormatTime(float t)
    {
        int m = (int)t / 60;
        int s = (int)t % 60;
        return $"{m:00}:{s:00}";
    }

    // ── публичный API ─────────────────────────────────────────────
    public void ShowPrompt(bool show) => promptGo.SetActive(show);

    public void ShowWin()
    {
        running = false;
        promptGo.SetActive(false);
        winPanel.SetActive(true);
    }

    public void ShowLose()
    {
        running = false;
        promptGo.SetActive(false);
        losePanel.SetActive(true);
    }

    // ── построение ───────────────────────────────────────────────
    void Build()
    {
        // Canvas
        var cGo = new GameObject("HUD_Canvas");
        var canvas = cGo.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30;
        var sc = cGo.AddComponent<CanvasScaler>();
        sc.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920, 1080);
        cGo.AddComponent<GraphicRaycaster>();

        BuildCrosshair(canvas.transform);
        BuildPrompt(canvas.transform);
        BuildTimer(canvas.transform);
        winPanel  = BuildResultPanel(canvas.transform, "АНОМАЛИЯ НАЙДЕНА", C_Win);
        losePanel = BuildResultPanel(canvas.transform, "ЭТО НЕ АНОМАЛИЯ",  C_Lose);

        winPanel.SetActive(false);
        losePanel.SetActive(false);
    }

    // Крестик-прицел
    void BuildCrosshair(Transform parent)
    {
        // Горизонтальная черта
        MakeDecoImg(parent, "CH_H", Color.white * 0.9f,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(14f, 2f), Vector2.zero);

        // Вертикальная черта
        MakeDecoImg(parent, "CH_V", Color.white * 0.9f,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(2f, 14f), Vector2.zero);
    }

    // Подсказка снизу экрана
    void BuildPrompt(Transform parent)
    {
        promptGo = new GameObject("Prompt");
        promptGo.transform.SetParent(parent, false);

        var bg = promptGo.AddComponent<Image>();
        bg.color         = new Color(0f, 0f, 0f, 0.60f);
        bg.raycastTarget = false;

        var rt = promptGo.GetComponent<RectTransform>();
        rt.anchorMin        = rt.anchorMax = new Vector2(0.5f, 0.08f);
        rt.sizeDelta        = new Vector2(560f, 52f);
        rt.anchoredPosition = Vector2.zero;

        var tGo = new GameObject("Txt");
        tGo.transform.SetParent(promptGo.transform, false);
        promptText           = tGo.AddComponent<TextMeshProUGUI>();
        promptText.text      = "[E] — Отметить аномалию";
        promptText.fontSize  = 22f;
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.color     = new Color(1f, 0.92f, 0.75f);
        promptText.raycastTarget = false;
        Fill(tGo.GetComponent<RectTransform>());

        promptGo.SetActive(false);
    }

    // Таймер в углу
    void BuildTimer(Transform parent)
    {
        var go = new GameObject("Timer");
        go.transform.SetParent(parent, false);
        timerText           = go.AddComponent<TextMeshProUGUI>();
        timerText.text      = "00:00";
        timerText.fontSize  = 22f;
        timerText.alignment = TextAlignmentOptions.Right;
        timerText.color     = new Color(0.85f, 0.82f, 0.78f, 0.70f);
        timerText.raycastTarget = false;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin        = new Vector2(1f, 1f);
        rt.anchorMax        = new Vector2(1f, 1f);
        rt.sizeDelta        = new Vector2(120f, 36f);
        rt.anchoredPosition = new Vector2(-20f, -20f);
    }

    // Оверлей победы / поражения
    GameObject BuildResultPanel(Transform parent, string message, Color accent)
    {
        // Затемнение
        var overlay = new GameObject("ResultOverlay");
        overlay.transform.SetParent(parent, false);
        var oImg = overlay.AddComponent<Image>();
        oImg.color         = new Color(0f, 0f, 0f, 0.75f);
        oImg.raycastTarget = false;
        Fill(overlay.GetComponent<RectTransform>());

        // Карточка
        var card = new GameObject("Card");
        card.transform.SetParent(overlay.transform, false);
        var cImg = card.AddComponent<Image>();
        cImg.color         = C_Dark;
        cImg.raycastTarget = false;
        var cRt = card.GetComponent<RectTransform>();
        cRt.anchorMin = cRt.anchorMax = new Vector2(0.5f, 0.5f);
        cRt.sizeDelta        = new Vector2(560f, 300f);
        cRt.anchoredPosition = Vector2.zero;

        // Акцентная полоска сверху
        MakeDecoImg(card.transform, "TopBar", accent,
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, 5f), Vector2.zero);

        // Сообщение
        TxtLabel(card.transform, message,
                 36f, FontStyles.Bold, accent,
                 new Vector2(0f, 68f), new Vector2(520f, 56f));

        // Время прохождения
        TxtLabel(card.transform, $"Время: {FormatTime(elapsed)}",
                 18f, FontStyles.Normal, new Color(0.55f, 0.52f, 0.48f),
                 new Vector2(0f, 16f), new Vector2(520f, 30f));

        // Кнопки
        ResultBtn(card.transform, "Заново",      new Vector2(-120f, -72f),
            () => GameManager.Instance?.RestartGame());
        ResultBtn(card.transform, "Главное меню", new Vector2(120f, -72f),
            () => GameManager.Instance?.LoadMainMenu());

        return overlay;
    }

    // ── утилиты ─────────────────────────────────────────────────
    static void ResultBtn(Transform parent, string label, Vector2 pos,
                           UnityEngine.Events.UnityAction action)
    {
        var go = new GameObject("Btn_" + label);
        go.transform.SetParent(parent, false);

        var img = go.AddComponent<Image>();
        img.color = Color.white;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.colors = new ColorBlock
        {
            normalColor      = new Color(0.16f, 0.14f, 0.20f),
            highlightedColor = new Color(0.28f, 0.10f, 0.10f),
            pressedColor     = new Color(0.08f, 0.04f, 0.06f),
            selectedColor    = new Color(0.16f, 0.14f, 0.20f),
            disabledColor    = new Color(0.3f, 0.3f, 0.3f, 0.5f),
            colorMultiplier  = 1f,
            fadeDuration     = 0.08f
        };
        btn.onClick.AddListener(action);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta        = new Vector2(200f, 46f);
        rt.anchoredPosition = pos;

        var tGo = new GameObject("Lbl");
        tGo.transform.SetParent(go.transform, false);
        var tmp = tGo.AddComponent<TextMeshProUGUI>();
        tmp.text          = label;
        tmp.fontSize      = 19f;
        tmp.fontStyle     = FontStyles.Bold;
        tmp.alignment     = TextAlignmentOptions.Center;
        tmp.color         = new Color(0.90f, 0.88f, 0.85f);
        tmp.raycastTarget = false;
        Fill(tGo.GetComponent<RectTransform>());
    }

    static void TxtLabel(Transform parent, string text, float size, FontStyles style,
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

    static void MakeDecoImg(Transform parent, string name, Color color,
                             Vector2 anchorMin, Vector2 anchorMax,
                             Vector2 sizeDelta, Vector2 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color         = color;
        img.raycastTarget = false;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin        = anchorMin;
        rt.anchorMax        = anchorMax;
        rt.sizeDelta        = sizeDelta;
        rt.anchoredPosition = pos;
    }

    static void Fill(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}
