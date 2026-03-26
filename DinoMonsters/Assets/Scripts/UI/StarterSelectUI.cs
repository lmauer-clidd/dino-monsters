// ============================================================
// Dino Monsters -- Starter Selection Screen
// Shows 3 dino cards (Pyrex #1, Aquadon #4, Florasaur #7).
// Player clicks a card, confirms, then the game begins.
// ============================================================

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class StarterSelectUI : MonoBehaviour
{
    // Starter species IDs
    private static readonly int[] STARTER_IDS = { 1, 4, 7 };

    // Type colors for card accents
    private static readonly Color COLOR_FIRE  = new Color(0.91f, 0.30f, 0.24f);
    private static readonly Color COLOR_WATER = new Color(0.25f, 0.57f, 0.87f);
    private static readonly Color COLOR_FLORA = new Color(0.30f, 0.75f, 0.35f);

    private Canvas canvas;
    private RectTransform cardsContainer;
    private GameObject confirmPanel;
    private TMP_Text confirmNameText;
    private TMP_Text confirmDescText;
    private int selectedStarterId = -1;

    // Player name input
    private TMP_InputField nameInput;
    private string playerName = "RED";

    // --- Navigation ---
    private int cardSelectedIndex = 0;
    private Button[] cardButtons;
    private Image[] cardImages;
    private Color[] cardBaseColors;
    private bool confirmShowing = false;
    private int confirmSelectedIndex = 0; // 0=OUI, 1=NON
    private Button confirmYesBtn;
    private Button confirmNoBtn;
    private Color confirmYesBaseColor;
    private Color confirmNoBaseColor;

    void Start()
    {
        CreateUI();

        // Initialize navigation arrays after UI is built
        cardButtons = new Button[3];
        cardImages = new Image[3];
        cardBaseColors = new Color[3];
        for (int i = 0; i < 3; i++)
        {
            var cardObj = cardsContainer.GetChild(i);
            cardButtons[i] = cardObj.GetComponent<Button>();
            cardImages[i] = cardObj.GetComponent<Image>();
            cardBaseColors[i] = cardImages[i].color;
        }
        cardSelectedIndex = 0;
        UpdateCardSelectionVisual();
    }

    void Update()
    {
        // Skip navigation if name input is focused
        if (nameInput != null && nameInput.isFocused) return;

        if (confirmShowing)
        {
            // Navigate confirm dialog
            if (InputHelper.Left || InputHelper.Right || InputHelper.Up || InputHelper.Down)
            {
                confirmSelectedIndex = (confirmSelectedIndex == 0) ? 1 : 0;
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayMenuMove();
                UpdateConfirmSelectionVisual();
            }

            if (InputHelper.Confirm)
            {
                if (confirmSelectedIndex == 0) OnConfirmYes();
                else OnConfirmNo();
            }

            if (InputHelper.Cancel)
            {
                OnConfirmNo();
            }
        }
        else
        {
            // Navigate cards
            if (InputHelper.Left)
            {
                cardSelectedIndex--;
                if (cardSelectedIndex < 0) cardSelectedIndex = 2;
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayMenuMove();
                UpdateCardSelectionVisual();
            }
            else if (InputHelper.Right)
            {
                cardSelectedIndex++;
                if (cardSelectedIndex > 2) cardSelectedIndex = 0;
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayMenuMove();
                UpdateCardSelectionVisual();
            }

            if (InputHelper.Confirm)
            {
                OnCardClicked(STARTER_IDS[cardSelectedIndex]);
            }
        }
    }

    void UpdateCardSelectionVisual()
    {
        if (cardImages == null) return;
        for (int i = 0; i < cardImages.Length; i++)
        {
            if (cardImages[i] == null) continue;
            if (i == cardSelectedIndex)
            {
                // Gold highlight border + scale up
                cardImages[i].color = new Color(0.91f, 0.78f, 0.41f, 0.35f); // Gold tint
                cardImages[i].transform.localScale = Vector3.one * 1.02f;
            }
            else
            {
                cardImages[i].color = cardBaseColors[i];
                cardImages[i].transform.localScale = Vector3.one;
            }
        }
    }

    void UpdateConfirmSelectionVisual()
    {
        if (confirmYesBtn == null || confirmNoBtn == null) return;

        var yesImg = confirmYesBtn.GetComponent<Image>();
        var noImg = confirmNoBtn.GetComponent<Image>();

        if (confirmSelectedIndex == 0)
        {
            yesImg.color = confirmYesBaseColor * 1.4f;
            confirmYesBtn.transform.localScale = Vector3.one * 1.08f;
            noImg.color = confirmNoBaseColor;
            confirmNoBtn.transform.localScale = Vector3.one;
        }
        else
        {
            yesImg.color = confirmYesBaseColor;
            confirmYesBtn.transform.localScale = Vector3.one;
            noImg.color = confirmNoBaseColor * 1.4f;
            confirmNoBtn.transform.localScale = Vector3.one * 1.08f;
        }
    }

    // ---------------------------------------------------------------
    // Build the entire UI
    // ---------------------------------------------------------------

    void CreateUI()
    {
        // --- EventSystem (REQUIRED for UI clicks) ---
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // --- Canvas ---
        var canvasGO = new GameObject("StarterCanvas");
        canvasGO.transform.SetParent(transform);
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // --- Background ---
        var bgGO = CreatePanel(canvasGO.transform, "Background",
            new Color(0.10f, 0.08f, 0.18f));
        StretchFill(bgGO.GetComponent<RectTransform>());

        // --- Header ---
        CreateText(canvasGO.transform, "Header",
            "CHOISIS TON PARTENAIRE", 36,
            new Color(0.91f, 0.78f, 0.41f),
            new Vector2(0, 280));

        // --- Player name input ---
        CreateNameInput(canvasGO.transform);

        // --- Cards container ---
        var containerGO = new GameObject("CardsContainer");
        containerGO.transform.SetParent(canvasGO.transform, false);
        cardsContainer = containerGO.AddComponent<RectTransform>();
        cardsContainer.anchorMin = new Vector2(0.5f, 0.5f);
        cardsContainer.anchorMax = new Vector2(0.5f, 0.5f);
        cardsContainer.anchoredPosition = new Vector2(0, -20);
        cardsContainer.sizeDelta = new Vector2(900, 320);

        // --- Create 3 starter cards ---
        float cardSpacing = 300f;
        float startX = -cardSpacing;

        for (int i = 0; i < STARTER_IDS.Length; i++)
        {
            int speciesId = STARTER_IDS[i];
            Color accentColor = GetTypeColor(i);
            float xPos = startX + i * cardSpacing;
            CreateStarterCard(cardsContainer, speciesId, accentColor, new Vector2(xPos, 0));
        }

        // --- Confirm panel (hidden) ---
        CreateConfirmPanel(canvasGO.transform);
    }

    void CreateNameInput(Transform parent)
    {
        // Label
        CreateText(parent, "NameLabel", "TON NOM:", 18,
            new Color(0.94f, 0.91f, 0.82f),
            new Vector2(-100, 210));

        // Input field container
        var inputGO = new GameObject("NameInput");
        inputGO.transform.SetParent(parent, false);

        var inputRect = inputGO.AddComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0.5f, 0.5f);
        inputRect.anchorMax = new Vector2(0.5f, 0.5f);
        inputRect.anchoredPosition = new Vector2(60, 210);
        inputRect.sizeDelta = new Vector2(200, 40);

        var inputBg = inputGO.AddComponent<Image>();
        inputBg.color = new Color(0.15f, 0.12f, 0.25f);

        // Text area child
        var textAreaGO = new GameObject("Text Area");
        textAreaGO.transform.SetParent(inputGO.transform, false);
        var textAreaRect = textAreaGO.AddComponent<RectTransform>();
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        textAreaRect.offsetMin = new Vector2(10, 5);
        textAreaRect.offsetMax = new Vector2(-10, -5);

        // Input text
        var textGO = new GameObject("Text");
        textGO.transform.SetParent(textAreaGO.transform, false);
        var inputText = textGO.AddComponent<TextMeshProUGUI>();
        inputText.fontSize = 20;
        inputText.color = Color.white;
        inputText.alignment = TextAlignmentOptions.MidlineLeft;
        var textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // Placeholder
        var placeholderGO = new GameObject("Placeholder");
        placeholderGO.transform.SetParent(textAreaGO.transform, false);
        var placeholder = placeholderGO.AddComponent<TextMeshProUGUI>();
        placeholder.text = "RED";
        placeholder.fontSize = 20;
        placeholder.color = new Color(1f, 1f, 1f, 0.3f);
        placeholder.alignment = TextAlignmentOptions.MidlineLeft;
        placeholder.fontStyle = FontStyles.Italic;
        var phRect = placeholderGO.GetComponent<RectTransform>();
        phRect.anchorMin = Vector2.zero;
        phRect.anchorMax = Vector2.one;
        phRect.offsetMin = Vector2.zero;
        phRect.offsetMax = Vector2.zero;

        // TMP_InputField component
        nameInput = inputGO.AddComponent<TMP_InputField>();
        nameInput.textViewport = textAreaRect;
        nameInput.textComponent = inputText;
        nameInput.placeholder = placeholder;
        nameInput.text = "RED";
        nameInput.characterLimit = 12;
        nameInput.onValueChanged.AddListener((val) =>
        {
            playerName = string.IsNullOrWhiteSpace(val) ? "RED" : val.Trim();
        });
    }

    // ---------------------------------------------------------------
    // Starter card
    // ---------------------------------------------------------------

    void CreateStarterCard(RectTransform parent, int speciesId, Color accentColor, Vector2 pos)
    {
        var species = DataLoader.Instance.GetSpecies(speciesId);
        if (species == null) return;

        // Card root
        var cardGO = new GameObject($"Card_{species.name}");
        cardGO.transform.SetParent(parent, false);

        var cardImg = cardGO.AddComponent<Image>();
        cardImg.color = new Color(0.15f, 0.12f, 0.25f, 0.95f);

        var cardRect = cardGO.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.anchoredPosition = pos;
        cardRect.sizeDelta = new Vector2(250, 300);

        // Accent bar at top
        var accentGO = CreatePanel(cardGO.transform, "Accent", accentColor);
        var accentRect = accentGO.GetComponent<RectTransform>();
        accentRect.anchorMin = new Vector2(0, 1);
        accentRect.anchorMax = new Vector2(1, 1);
        accentRect.pivot = new Vector2(0.5f, 1);
        accentRect.anchoredPosition = Vector2.zero;
        accentRect.sizeDelta = new Vector2(0, 6);

        // Dino icon placeholder (colored circle)
        var iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(cardGO.transform, false);
        var iconImg = iconGO.AddComponent<Image>();
        iconImg.color = accentColor * 0.6f;
        var iconRect = iconGO.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = new Vector2(0, 55);
        iconRect.sizeDelta = new Vector2(90, 90);

        // Icon letter
        var letterTmp = CreateTextOnTransform(iconGO.transform, "Letter",
            species.name.Substring(0, 1).ToUpper(), 40, Color.white, Vector2.zero);
        letterTmp.fontStyle = FontStyles.Bold;
        var letterRect = letterTmp.GetComponent<RectTransform>();
        letterRect.anchorMin = Vector2.zero;
        letterRect.anchorMax = Vector2.one;
        letterRect.offsetMin = Vector2.zero;
        letterRect.offsetMax = Vector2.zero;

        // Species name
        CreateTextOnTransform(cardGO.transform, "Name", species.name.ToUpper(), 24,
            Color.white, new Vector2(0, -15));

        // Type name
        string typeName = GetTypeName(species.types[0]);
        CreateTextOnTransform(cardGO.transform, "Type", typeName, 16,
            accentColor, new Vector2(0, -42));

        // Description (truncated)
        string desc = species.description ?? "";
        if (desc.Length > 80) desc = desc.Substring(0, 77) + "...";
        var descTmp = CreateTextOnTransform(cardGO.transform, "Desc", desc, 13,
            new Color(0.7f, 0.7f, 0.7f), new Vector2(0, -80));
        descTmp.enableWordWrapping = true;
        descTmp.GetComponent<RectTransform>().sizeDelta = new Vector2(220, 60);

        // Click handler
        var btn = cardGO.AddComponent<Button>();
            btn.navigation = new Navigation { mode = Navigation.Mode.None };
        var btnColors = btn.colors;
        btnColors.normalColor = new Color(0.15f, 0.12f, 0.25f, 0.95f);
        btnColors.highlightedColor = new Color(0.22f, 0.18f, 0.35f, 1f);
        btnColors.pressedColor = new Color(0.10f, 0.08f, 0.18f, 1f);
        btnColors.selectedColor = btnColors.highlightedColor;
        btn.colors = btnColors;
        btn.targetGraphic = cardImg;

        int capturedId = speciesId;
        btn.onClick.AddListener(() => OnCardClicked(capturedId));
    }

    // ---------------------------------------------------------------
    // Confirm panel
    // ---------------------------------------------------------------

    void CreateConfirmPanel(Transform parent)
    {
        // Overlay
        confirmPanel = new GameObject("ConfirmPanel");
        confirmPanel.transform.SetParent(parent, false);

        var overlayImg = confirmPanel.AddComponent<Image>();
        overlayImg.color = new Color(0, 0, 0, 0.7f);
        StretchFill(confirmPanel.GetComponent<RectTransform>());

        // Dialog box
        var dialogGO = CreatePanel(confirmPanel.transform, "Dialog",
            new Color(0.12f, 0.10f, 0.22f));
        var dialogRect = dialogGO.GetComponent<RectTransform>();
        dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
        dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
        dialogRect.anchoredPosition = Vector2.zero;
        dialogRect.sizeDelta = new Vector2(450, 250);

        // Confirm name text
        confirmNameText = CreateTextOnTransform(dialogGO.transform, "ConfirmName",
            "", 28, new Color(0.91f, 0.78f, 0.41f), new Vector2(0, 70));
        confirmNameText.fontStyle = FontStyles.Bold;

        // Confirm description
        confirmDescText = CreateTextOnTransform(dialogGO.transform, "ConfirmDesc",
            "", 16, new Color(0.8f, 0.8f, 0.8f), new Vector2(0, 20));
        confirmDescText.enableWordWrapping = true;
        confirmDescText.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 50);

        // Confirm question
        CreateTextOnTransform(dialogGO.transform, "ConfirmQ",
            "Tu es sur de ton choix ?", 18,
            new Color(0.94f, 0.91f, 0.82f), new Vector2(0, -25));

        // Yes button
        confirmYesBtn = CreateButtonOnTransform(dialogGO.transform, "YesBtn",
            "OUI !", new Vector2(-80, -80), OnConfirmYes);
        confirmYesBaseColor = new Color(0.30f, 0.65f, 0.30f);
        SetButtonColor(confirmYesBtn, confirmYesBaseColor);

        // No button
        confirmNoBtn = CreateButtonOnTransform(dialogGO.transform, "NoBtn",
            "NON", new Vector2(80, -80), OnConfirmNo);
        confirmNoBaseColor = new Color(0.65f, 0.30f, 0.30f);
        SetButtonColor(confirmNoBtn, confirmNoBaseColor);

        confirmPanel.SetActive(false);
    }

    // ---------------------------------------------------------------
    // Actions
    // ---------------------------------------------------------------

    void OnCardClicked(int speciesId)
    {
        selectedStarterId = speciesId;
        var species = DataLoader.Instance.GetSpecies(speciesId);
        if (species == null) return;

        confirmNameText.text = species.name.ToUpper();
        confirmDescText.text = species.description ?? "";
        confirmPanel.SetActive(true);
        confirmShowing = true;
        confirmSelectedIndex = 0;
        UpdateConfirmSelectionVisual();
    }

    void OnConfirmYes()
    {
        if (selectedStarterId < 0) return;

        // Initialize new game via GameManager
        GameManager.Instance.StartNewGame(playerName, selectedStarterId);

        // Set story flags for starter choice
        GameState.Instance.SetFlag("has_starter", true);
        if (selectedStarterId == 1) GameState.Instance.SetFlag("starter_fire", true);
        else if (selectedStarterId == 4) GameState.Instance.SetFlag("starter_water", true);
        else if (selectedStarterId == 7) GameState.Instance.SetFlag("starter_grass", true);
    }

    void OnConfirmNo()
    {
        selectedStarterId = -1;
        confirmPanel.SetActive(false);
        confirmShowing = false;
    }

    // ---------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------

    Color GetTypeColor(int starterIndex)
    {
        switch (starterIndex)
        {
            case 0: return COLOR_FIRE;
            case 1: return COLOR_WATER;
            case 2: return COLOR_FLORA;
            default: return Color.gray;
        }
    }

    string GetTypeName(int typeId)
    {
        if (System.Enum.IsDefined(typeof(DinoType), typeId))
            return ((DinoType)typeId).ToString();
        return "???";
    }

    TMP_Text CreateText(Transform parent, string name, string text, int fontSize,
        Color color, Vector2 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Overflow;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(800, 50);
        return tmp;
    }

    TMP_Text CreateTextOnTransform(Transform parent, string name, string text,
        int fontSize, Color color, Vector2 pos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(300, 40);
        return tmp;
    }

    Button CreateButtonOnTransform(Transform parent, string name, string label,
        Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var img = go.AddComponent<Image>();
        img.color = new Color(0.53f, 0.60f, 0.25f);

        var btn = go.AddComponent<Button>();
            btn.navigation = new Navigation { mode = Navigation.Mode.None };
        btn.onClick.AddListener(onClick);

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(140, 45);

        var txtGO = new GameObject("Label");
        txtGO.transform.SetParent(go.transform, false);
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 20;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        var txtRect = txtGO.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        return btn;
    }

    void SetButtonColor(Button btn, Color color)
    {
        var img = btn.GetComponent<Image>();
        if (img != null) img.color = color;

        var colors = btn.colors;
        colors.normalColor = color;
        colors.highlightedColor = color * 1.15f;
        colors.pressedColor = color * 0.8f;
        colors.selectedColor = colors.highlightedColor;
        btn.colors = colors;
    }

    GameObject CreatePanel(Transform parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = color;
        return go;
    }

    void StretchFill(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
