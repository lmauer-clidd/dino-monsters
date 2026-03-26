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
        canvas.sortingOrder = 200; // Above other UI

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // --- Dialogue panel (bottom of screen) ---
        var panelGO = new GameObject("DialoguePanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        dialoguePanel = panelGO;

        var panelImg = panelGO.AddComponent<Image>();
        panelImg.color = new Color(0.06f, 0.05f, 0.12f, 0.95f);

        var panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(1, 0);
        panelRect.pivot = new Vector2(0.5f, 0);
        panelRect.anchoredPosition = new Vector2(0, 20);
        panelRect.sizeDelta = new Vector2(-80, 160); // 40px margin each side

        // Border on top
        var borderGO = CreatePanel(panelGO.transform, "Border",
            new Color(0.91f, 0.78f, 0.41f));
        var borderRect = borderGO.GetComponent<RectTransform>();
        borderRect.anchorMin = new Vector2(0, 1);
        borderRect.anchorMax = new Vector2(1, 1);
        borderRect.pivot = new Vector2(0.5f, 1);
        borderRect.anchoredPosition = Vector2.zero;
        borderRect.sizeDelta = new Vector2(0, 3);

        // --- Speaker name tag ---
        var speakerBgGO = CreatePanel(panelGO.transform, "SpeakerBg",
            new Color(0.91f, 0.78f, 0.41f));
        var speakerBgRect = speakerBgGO.GetComponent<RectTransform>();
        speakerBgRect.anchorMin = new Vector2(0, 1);
        speakerBgRect.anchorMax = new Vector2(0, 1);
        speakerBgRect.pivot = new Vector2(0, 0);
        speakerBgRect.anchoredPosition = new Vector2(20, 5);
        speakerBgRect.sizeDelta = new Vector2(200, 32);

        speakerText = CreateTMP(speakerBgGO.transform, "SpeakerText", "", 18,
            new Color(0.08f, 0.06f, 0.15f));
        speakerText.fontStyle = FontStyles.Bold;
        speakerText.alignment = TextAlignmentOptions.Center;
        var spRect = speakerText.GetComponent<RectTransform>();
        spRect.anchorMin = Vector2.zero;
        spRect.anchorMax = Vector2.one;
        spRect.offsetMin = Vector2.zero;
        spRect.offsetMax = Vector2.zero;

        // --- Body text ---
        bodyText = CreateTMP(panelGO.transform, "BodyText", "", 22,
            new Color(0.94f, 0.91f, 0.82f));
        bodyText.alignment = TextAlignmentOptions.TopLeft;
        bodyText.enableWordWrapping = true;
        bodyText.overflowMode = TextOverflowModes.Ellipsis;
        var bodyRect = bodyText.GetComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0, 0);
        bodyRect.anchorMax = new Vector2(1, 1);
        bodyRect.offsetMin = new Vector2(25, 15);
        bodyRect.offsetMax = new Vector2(-25, -20);

        // --- Continue indicator (blinking arrow) ---
        continueIndicator = CreateTMP(panelGO.transform, "ContinueArrow",
            "\u25BC", 16, new Color(0.91f, 0.78f, 0.41f));
        continueIndicator.alignment = TextAlignmentOptions.Center;
        var contRect = continueIndicator.GetComponent<RectTransform>();
        contRect.anchorMin = new Vector2(1, 0);
        contRect.anchorMax = new Vector2(1, 0);
        contRect.pivot = new Vector2(1, 0);
        contRect.anchoredPosition = new Vector2(-15, 10);
        contRect.sizeDelta = new Vector2(30, 30);

        // --- Choice container ---
        var choiceGO = new GameObject("ChoiceContainer");
        choiceGO.transform.SetParent(panelGO.transform, false);
        choiceContainer = choiceGO;

        var choiceRect = choiceGO.AddComponent<RectTransform>();
        choiceRect.anchorMin = new Vector2(0.5f, 0);
        choiceRect.anchorMax = new Vector2(1, 1);
        choiceRect.offsetMin = new Vector2(0, 10);
        choiceRect.offsetMax = new Vector2(-15, -15);

        // Create choice buttons
        for (int i = 0; i < MAX_CHOICES; i++)
        {
            float yPos = -i * 32f;
            int capturedIndex = i;

            var btnGO = new GameObject($"Choice_{i}");
            btnGO.transform.SetParent(choiceGO.transform, false);

            var btnImg = btnGO.AddComponent<Image>();
            btnImg.color = new Color(0.18f, 0.15f, 0.30f, 0.9f);

            var btn = btnGO.AddComponent<Button>();
            btn.navigation = new Navigation { mode = Navigation.Mode.None };
            btn.onClick.AddListener(() => OnChoiceClicked(capturedIndex));

            var colors = btn.colors;
            colors.normalColor = new Color(0.18f, 0.15f, 0.30f, 0.9f);
            colors.highlightedColor = new Color(0.30f, 0.25f, 0.45f, 1f);
            colors.pressedColor = new Color(0.12f, 0.10f, 0.20f, 1f);
            colors.selectedColor = colors.highlightedColor;
            btn.colors = colors;

            var btnRect = btnGO.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0, 1);
            btnRect.anchorMax = new Vector2(1, 1);
            btnRect.pivot = new Vector2(0.5f, 1);
            btnRect.anchoredPosition = new Vector2(0, yPos);
            btnRect.sizeDelta = new Vector2(0, 28);

            var labelTmp = CreateTMP(btnGO.transform, "Label", "", 18, Color.white);
            labelTmp.alignment = TextAlignmentOptions.MidlineLeft;
            var labelRect = labelTmp.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(15, 0);
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
