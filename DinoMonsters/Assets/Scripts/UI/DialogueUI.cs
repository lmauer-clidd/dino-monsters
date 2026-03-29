// ============================================================
// Dino Monsters -- Dialogue UI (Singleton)
// Reusable typewriter dialogue box with choice system.
//
// Usage:
//   DialogueUI.Instance.ShowText("Hello!", "PROF. PALEO", () => { });
//   DialogueUI.Instance.ShowChoices("Which?", new[]{"OUI","NON"}, (idx) => { });
// ============================================================

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    // --- Settings ---
    private const float CHARS_PER_SECOND = 40f;
    private const int MAX_CHOICES = 4;

    // --- UI references ---
    private Canvas canvas;
    private GameObject dialoguePanel;
    private TMP_Text speakerText;
    private TMP_Text bodyText;
    private TMP_Text continueIndicator;
    private GameObject choiceContainer;
    private Button[] choiceButtons = new Button[MAX_CHOICES];
    private TMP_Text[] choiceLabels = new TMP_Text[MAX_CHOICES];

    // --- State ---
    private bool isActive = false;
    private bool isTyping = false;
    private bool skipRequested = false;
    private string fullText = "";
    private Action onComplete;
    private Action<int> onChoiceSelected;
    private Coroutine typewriterCoroutine;

    // --- Choice navigation ---
    private int choiceSelectedIndex = 0;
    private int choiceCount = 0;
    private Color choiceBaseColor = new Color(0.18f, 0.15f, 0.30f, 0.9f);
    private Color choiceHighlightColor = new Color(0.40f, 0.35f, 0.60f, 1f);

    // ---------------------------------------------------------------
    // Singleton
    // ---------------------------------------------------------------

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildUI();
        Hide();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // ---------------------------------------------------------------
    // Public API
    // ---------------------------------------------------------------

    /// <summary>
    /// Show a dialogue line with typewriter effect.
    /// </summary>
    public void ShowText(string text, string speaker = null, Action onDone = null)
    {
        if (isActive) ForceFinishCurrent();

        isActive = true;
        fullText = text;
        onComplete = onDone;
        onChoiceSelected = null;

        speakerText.text = speaker ?? "";
        speakerText.gameObject.SetActive(!string.IsNullOrEmpty(speaker));
        bodyText.text = "";
        continueIndicator.gameObject.SetActive(false);
        choiceContainer.SetActive(false);
        dialoguePanel.SetActive(true);

        typewriterCoroutine = StartCoroutine(TypewriterRoutine(text));
    }

    /// <summary>
    /// Show a choice dialogue. Each option is displayed as a button.
    /// </summary>
    public void ShowChoices(string prompt, string[] choices, Action<int> onSelected)
    {
        if (isActive) ForceFinishCurrent();

        isActive = true;
        fullText = prompt;
        onComplete = null;
        onChoiceSelected = onSelected;

        speakerText.gameObject.SetActive(false);
        bodyText.text = prompt;
        continueIndicator.gameObject.SetActive(false);
        dialoguePanel.SetActive(true);

        // Setup choice buttons
        choiceContainer.SetActive(true);
        choiceCount = choices.Length;
        choiceSelectedIndex = 0;

        for (int i = 0; i < MAX_CHOICES; i++)
        {
            if (i < choices.Length)
            {
                choiceButtons[i].gameObject.SetActive(true);
                choiceLabels[i].text = choices[i];
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }

        UpdateChoiceSelectionVisual();
    }

    /// <summary>
    /// Is the dialogue UI currently visible?
    /// </summary>
    public bool IsActive()
    {
        return isActive;
    }

    /// <summary>
    /// Force close the dialogue.
    /// </summary>
    public void ForceClose()
    {
        ForceFinishCurrent();
        Hide();
    }

    // ---------------------------------------------------------------
    // Update — input handling
    // ---------------------------------------------------------------

    void Update()
    {
        if (!isActive) return;

        // If choices are shown, handle choice navigation
        if (choiceContainer.activeSelf)
        {
            if (InputHelper.Up)
            {
                choiceSelectedIndex--;
                if (choiceSelectedIndex < 0) choiceSelectedIndex = choiceCount - 1;
                UpdateChoiceSelectionVisual();
            }
            else if (InputHelper.Down)
            {
                choiceSelectedIndex++;
                if (choiceSelectedIndex >= choiceCount) choiceSelectedIndex = 0;
                UpdateChoiceSelectionVisual();
            }

            if (InputHelper.Confirm)
            {
                OnChoiceClicked(choiceSelectedIndex);
            }
            return;
        }

        if (InputHelper.Confirm || Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                // Skip typewriter — show full text immediately
                skipRequested = true;
            }
            else
            {
                // Text fully displayed — advance
                Advance();
            }
        }
    }

    void UpdateChoiceSelectionVisual()
    {
        for (int i = 0; i < MAX_CHOICES; i++)
        {
            if (!choiceButtons[i].gameObject.activeSelf) continue;
            var img = choiceButtons[i].GetComponent<Image>();
            if (img == null) continue;

            if (i == choiceSelectedIndex)
            {
                img.color = choiceHighlightColor;
                choiceButtons[i].transform.localScale = new Vector3(1.03f, 1.03f, 1f);
            }
            else
            {
                img.color = choiceBaseColor;
                choiceButtons[i].transform.localScale = Vector3.one;
            }
        }
    }

    // ---------------------------------------------------------------
    // Internal
    // ---------------------------------------------------------------

    void Advance()
    {
        isActive = false;
        dialoguePanel.SetActive(false);

        var callback = onComplete;
        onComplete = null;
        callback?.Invoke();
    }

    void ForceFinishCurrent()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }
        isTyping = false;
        skipRequested = false;
        bodyText.text = fullText;
    }

    void Hide()
    {
        isActive = false;
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    void OnChoiceClicked(int index)
    {
        isActive = false;
        choiceContainer.SetActive(false);
        dialoguePanel.SetActive(false);

        var callback = onChoiceSelected;
        onChoiceSelected = null;
        callback?.Invoke(index);
    }

    // ---------------------------------------------------------------
    // Typewriter coroutine
    // ---------------------------------------------------------------

    IEnumerator TypewriterRoutine(string text)
    {
        isTyping = true;
        skipRequested = false;
        bodyText.text = "";

        float charDelay = 1f / CHARS_PER_SECOND;
        int charIndex = 0;

        while (charIndex < text.Length)
        {
            if (skipRequested)
            {
                bodyText.text = text;
                break;
            }

            charIndex++;
            bodyText.text = text.Substring(0, charIndex);
            yield return new WaitForSeconds(charDelay);
        }

        isTyping = false;
        skipRequested = false;
        typewriterCoroutine = null;

        // Show continue indicator (not for choices)
        if (onChoiceSelected == null)
            continueIndicator.gameObject.SetActive(true);
    }

    // ---------------------------------------------------------------
    // Build UI from code
    // ---------------------------------------------------------------

    void BuildUI()
    {
        // --- Canvas ---
        var canvasGO = new GameObject("DialogueCanvas");
        canvasGO.transform.SetParent(transform);
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // --- Dialogue panel (parchment style — warm cream background) ---
        var panelGO = new GameObject("DialoguePanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        dialoguePanel = panelGO;

        var panelRect = panelGO.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(1, 0);
        panelRect.pivot = new Vector2(0.5f, 0);
        panelRect.anchoredPosition = new Vector2(0, 18);
        panelRect.sizeDelta = new Vector2(-60, 155);

        // Outer border (Terre brûlée)
        var outerBorder = panelGO.AddComponent<Image>();
        outerBorder.color = Constants.ColorTerreBrulee;

        // Inner panel (Calcaire parchment)
        var innerGO = CreatePanel(panelGO.transform, "InnerPanel", Constants.ColorCalcaire);
        var innerRect = innerGO.GetComponent<RectTransform>();
        innerRect.anchorMin = Vector2.zero;
        innerRect.anchorMax = Vector2.one;
        innerRect.offsetMin = new Vector2(4, 4);
        innerRect.offsetMax = new Vector2(-4, -4);

        // Inner accent border (Sédiment)
        var accentGO = CreatePanel(innerGO.transform, "AccentBorder", Constants.ColorSediment);
        var accentRect = accentGO.GetComponent<RectTransform>();
        accentRect.anchorMin = Vector2.zero;
        accentRect.anchorMax = Vector2.one;
        accentRect.offsetMin = Vector2.zero;
        accentRect.offsetMax = Vector2.zero;

        // Content area (slightly lighter cream)
        var contentBg = CreatePanel(accentGO.transform, "ContentBg", Constants.ColorCalcaire);
        var contentRect = contentBg.GetComponent<RectTransform>();
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.offsetMin = new Vector2(2, 2);
        contentRect.offsetMax = new Vector2(-2, -2);

        // Top amber accent line (Résine signature)
        var topLine = CreatePanel(panelGO.transform, "TopAccent", Constants.ColorResine);
        var topRect = topLine.GetComponent<RectTransform>();
        topRect.anchorMin = new Vector2(0, 1);
        topRect.anchorMax = new Vector2(1, 1);
        topRect.pivot = new Vector2(0.5f, 1);
        topRect.anchoredPosition = new Vector2(0, 2);
        topRect.sizeDelta = new Vector2(-8, 3);

        // --- Speaker name tag (amber badge) ---
        var speakerBgGO = new GameObject("SpeakerBg");
        speakerBgGO.transform.SetParent(panelGO.transform, false);
        var speakerBg = speakerBgGO.AddComponent<Image>();
        speakerBg.color = Constants.ColorResine;
        var speakerBgRect = speakerBgGO.GetComponent<RectTransform>();
        speakerBgRect.anchorMin = new Vector2(0, 1);
        speakerBgRect.anchorMax = new Vector2(0, 1);
        speakerBgRect.pivot = new Vector2(0, 0);
        speakerBgRect.anchoredPosition = new Vector2(18, 4);
        speakerBgRect.sizeDelta = new Vector2(200, 30);

        // Speaker border
        var spOutline = speakerBgGO.AddComponent<Outline>();
        spOutline.effectColor = Constants.ColorTerreBrulee;
        spOutline.effectDistance = new Vector2(2, 2);

        speakerText = CreateTMP(speakerBgGO.transform, "SpeakerText", "", 17,
            Constants.ColorObsidienne);
        speakerText.fontStyle = FontStyles.Bold;
        speakerText.alignment = TextAlignmentOptions.Center;
        var spRect = speakerText.GetComponent<RectTransform>();
        spRect.anchorMin = Vector2.zero;
        spRect.anchorMax = Vector2.one;
        spRect.offsetMin = new Vector2(8, 0);
        spRect.offsetMax = new Vector2(-8, 0);

        // --- Body text (dark text on parchment) ---
        bodyText = CreateTMP(contentBg.transform, "BodyText", "", 20,
            Constants.ColorTextDarkPrimary);
        bodyText.alignment = TextAlignmentOptions.TopLeft;
        bodyText.enableWordWrapping = true;
        bodyText.overflowMode = TextOverflowModes.Ellipsis;
        var bodyRect = bodyText.GetComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0, 0);
        bodyRect.anchorMax = new Vector2(1, 1);
        bodyRect.offsetMin = new Vector2(20, 12);
        bodyRect.offsetMax = new Vector2(-20, -12);

        // --- Continue indicator (amber arrow) ---
        continueIndicator = CreateTMP(contentBg.transform, "ContinueArrow",
            "v", 16, Constants.ColorResine);
        continueIndicator.alignment = TextAlignmentOptions.Center;
        var contRect = continueIndicator.GetComponent<RectTransform>();
        contRect.anchorMin = new Vector2(1, 0);
        contRect.anchorMax = new Vector2(1, 0);
        contRect.pivot = new Vector2(1, 0);
        contRect.anchoredPosition = new Vector2(-10, 6);
        contRect.sizeDelta = new Vector2(24, 24);

        // --- Choice container ---
        var choiceGO = new GameObject("ChoiceContainer");
        choiceGO.transform.SetParent(contentBg.transform, false);
        choiceContainer = choiceGO;

        var choiceContRect = choiceGO.AddComponent<RectTransform>();
        choiceContRect.anchorMin = new Vector2(0.5f, 0);
        choiceContRect.anchorMax = new Vector2(1, 1);
        choiceContRect.offsetMin = new Vector2(0, 8);
        choiceContRect.offsetMax = new Vector2(-10, -8);

        // Create choice buttons (warm earthy style)
        for (int i = 0; i < MAX_CHOICES; i++)
        {
            float yPos = -i * 30f;
            int capturedIndex = i;

            var btnGO = new GameObject($"Choice_{i}");
            btnGO.transform.SetParent(choiceGO.transform, false);

            var btnImg = btnGO.AddComponent<Image>();
            Color btnBase = Constants.ColorSediment;
            btnImg.color = new Color(btnBase.r, btnBase.g, btnBase.b, 0.25f);

            var btn = btnGO.AddComponent<Button>();
            btn.navigation = new Navigation { mode = Navigation.Mode.None };
            btn.onClick.AddListener(() => OnChoiceClicked(capturedIndex));

            var btnColors = btn.colors;
            btnColors.normalColor = new Color(btnBase.r, btnBase.g, btnBase.b, 0.25f);
            btnColors.highlightedColor = new Color(Constants.ColorResine.r, Constants.ColorResine.g, Constants.ColorResine.b, 0.5f);
            btnColors.pressedColor = new Color(btnBase.r, btnBase.g, btnBase.b, 0.6f);
            btnColors.selectedColor = btnColors.highlightedColor;
            btn.colors = btnColors;

            var btnRect = btnGO.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0, 1);
            btnRect.anchorMax = new Vector2(1, 1);
            btnRect.pivot = new Vector2(0.5f, 1);
            btnRect.anchoredPosition = new Vector2(0, yPos);
            btnRect.sizeDelta = new Vector2(0, 26);

            var labelTmp = CreateTMP(btnGO.transform, "Label", "", 16,
                Constants.ColorTextDarkPrimary);
            labelTmp.fontStyle = FontStyles.Bold;
            labelTmp.alignment = TextAlignmentOptions.MidlineLeft;
            var labelRect = labelTmp.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(12, 0);
            labelRect.offsetMax = Vector2.zero;

            choiceButtons[i] = btn;
            choiceLabels[i] = labelTmp;
        }

        choiceContainer.SetActive(false);
    }

    // ---------------------------------------------------------------
    // UI helpers
    // ---------------------------------------------------------------

    TMP_Text CreateTMP(Transform parent, string name, string text, int fontSize, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Overflow;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(300, 40);
        return tmp;
    }

    GameObject CreatePanel(Transform parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = color;
        return go;
    }
}
