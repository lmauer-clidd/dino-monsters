// ============================================================
// Dino Monsters -- Pause Menu UI
// Accessible with ESC during overworld. Programmatic UI.
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenuUI : MonoBehaviour
{
    public static PauseMenuUI Instance { get; private set; }

    // --- State ---
    private bool isPaused = false;
    private Canvas canvas;
    private RectTransform canvasRect;
    private GameObject rootPanel;

    // --- Sub-panels ---
    private GameObject menuPanel;
    private GameObject partyPanel;
    private GameObject bagPanel;
    private GameObject saveConfirmPanel;

    // --- World Map ---
    private WorldMapUI worldMapUI;

    // --- Navigation ---
    private int menuSelectedIndex = 0;
    private Button[] menuButtons;
    private Color[] menuBaseColors;
    private bool isInSubPanel = false;

    // --- Party UI sub-components ---
    private PartyUI partyUI;

    // --- Bag UI sub-components ---
    private RectTransform bagListContent;
    private TextMeshProUGUI bagEmptyText;

    // --- Colors (consistent with BattleUI / Constants) ---
    private static readonly Color PANEL_BG = Constants.ColorUiBg;
    private static readonly Color PANEL_BORDER = Constants.ColorUiBorder;
    private static readonly Color TEXT_COLOR = Constants.ColorTextPrimary;
    private static readonly Color OVERLAY_COLOR = new Color(0f, 0f, 0f, 0.6f);

    // Menu button colors
    private static readonly Color BTN_DINOS    = new Color(0.41f, 0.56f, 0.94f);
    private static readonly Color BTN_SAC      = new Color(0.47f, 0.78f, 0.30f);
    private static readonly Color BTN_DINODEX  = new Color(0.75f, 0.55f, 0.30f);
    private static readonly Color BTN_CARTE    = new Color(0.55f, 0.75f, 0.75f);
    private static readonly Color BTN_SAVE     = new Color(0.94f, 0.78f, 0.30f);
    private static readonly Color BTN_OPTIONS  = new Color(0.60f, 0.60f, 0.65f);
    private static readonly Color BTN_QUIT     = new Color(0.75f, 0.35f, 0.30f);

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        CreateCanvas();
        rootPanel.SetActive(false);
    }

    void Update()
    {
        if (InputHelper.Pause)
        {
            if (isPaused)
            {
                if (isInSubPanel)
                {
                    // Return to main menu from sub-panel
                    ShowMainMenu();
                    isInSubPanel = false;
                }
                else
                {
                    ClosePauseMenu();
                }
            }
            else
            {
                OpenPauseMenu();
            }
            return;
        }

        if (!isPaused) return;

        // Sub-panel: B/Escape goes back to main menu
        if (isInSubPanel)
        {
            if (InputHelper.Cancel)
            {
                ShowMainMenu();
                isInSubPanel = false;
            }
            return;
        }

        // Main menu navigation
        if (menuPanel != null && menuPanel.activeSelf && menuButtons != null)
        {
            if (InputHelper.Up)
            {
                ChangeMenuSelection(-1);
            }
            else if (InputHelper.Down)
            {
                ChangeMenuSelection(1);
            }

            if (InputHelper.Confirm)
            {
                ConfirmMenuSelection();
            }

            if (InputHelper.Cancel)
            {
                ClosePauseMenu();
            }
        }
    }

    void ChangeMenuSelection(int delta)
    {
        if (menuButtons == null) return;

        // Build list of interactable indices
        var activeIndices = new System.Collections.Generic.List<int>();
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] != null && menuButtons[i].interactable)
                activeIndices.Add(i);
        }
        if (activeIndices.Count == 0) return;

        int currentPos = activeIndices.IndexOf(menuSelectedIndex);
        if (currentPos < 0) currentPos = 0;

        currentPos += delta;
        if (currentPos < 0) currentPos = activeIndices.Count - 1;
        if (currentPos >= activeIndices.Count) currentPos = 0;

        menuSelectedIndex = activeIndices[currentPos];

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMenuMove();

        UpdateMenuSelectionVisual();
    }

    void ConfirmMenuSelection()
    {
        if (menuButtons == null || menuSelectedIndex < 0 || menuSelectedIndex >= menuButtons.Length) return;
        if (!menuButtons[menuSelectedIndex].interactable) return;

        // Trigger the button's onClick
        menuButtons[menuSelectedIndex].onClick.Invoke();
    }

    void UpdateMenuSelectionVisual()
    {
        if (menuButtons == null) return;
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] == null) continue;
            var img = menuButtons[i].GetComponent<Image>();
            var outl = menuButtons[i].GetComponent<Outline>();
            if (img == null) continue;

            if (i == menuSelectedIndex && menuButtons[i].interactable)
            {
                img.color = Color.Lerp(menuBaseColors[i], Color.white, 0.35f);
                menuButtons[i].transform.localScale = Vector3.one * 1.02f;
                if (outl != null)
                    outl.effectColor = Color.white;
            }
            else
            {
                img.color = menuButtons[i].interactable ? menuBaseColors[i]
                    : new Color(menuBaseColors[i].r, menuBaseColors[i].g, menuBaseColors[i].b, 0.4f);
                menuButtons[i].transform.localScale = Vector3.one;
                if (outl != null)
                    outl.effectColor = Color.Lerp(menuBaseColors[i], Color.black, 0.3f);
            }
        }
    }

    // ===============================================================
    // Open / Close
    // ===============================================================

    public void OpenPauseMenu()
    {
        if (isPaused) return;
        isPaused = true;
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMenuSelect();
        Time.timeScale = 0f;
        rootPanel.SetActive(true);
        isInSubPanel = false;
        menuSelectedIndex = 0;
        ShowMainMenu();

        // Lock player input
        var player = FindObjectOfType<PlayerController>();
        if (player != null) player.LockInput();

        // Hide overworld HUD
        if (OverworldHUD.Instance != null)
            OverworldHUD.Instance.SetVisible(false);
    }

    public void ClosePauseMenu()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMenuMove();
        isPaused = false;
        Time.timeScale = 1f;
        rootPanel.SetActive(false);

        // Unlock player input
        var player = FindObjectOfType<PlayerController>();
        if (player != null) player.UnlockInput();

        // Show overworld HUD
        if (OverworldHUD.Instance != null)
            OverworldHUD.Instance.SetVisible(true);
    }

    public bool IsPaused => isPaused;

    // ===============================================================
    // Canvas Setup
    // ===============================================================

    private void CreateCanvas()
    {
        // --- EventSystem ---
        if (FindObjectOfType<EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();
        }

        // --- Canvas ---
        var canvasGO = new GameObject("PauseCanvas");
        canvasGO.transform.SetParent(transform);
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200; // Above battle UI

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();
        canvasRect = canvasGO.GetComponent<RectTransform>();

        // --- Root panel (overlay) ---
        rootPanel = new GameObject("PauseRoot");
        rootPanel.transform.SetParent(canvasGO.transform, false);
        var rootRect = rootPanel.AddComponent<RectTransform>();
        StretchFill(rootRect);
        var rootImg = rootPanel.AddComponent<Image>();
        rootImg.color = OVERLAY_COLOR;

        // --- Build sub-panels ---
        CreateMainMenu();
        CreateBagPanel();
        CreateSaveConfirmPanel();
    }

    // ===============================================================
    // Main Menu Panel
    // ===============================================================

    private void CreateMainMenu()
    {
        menuPanel = new GameObject("MenuPanel");
        menuPanel.transform.SetParent(rootPanel.transform, false);
        var rect = menuPanel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(1f, 0.5f);
        rect.anchoredPosition = new Vector2(-30f, 0f);
        rect.sizeDelta = new Vector2(260f, 460f);

        var img = menuPanel.AddComponent<Image>();
        img.color = new Color(PANEL_BG.r, PANEL_BG.g, PANEL_BG.b, 0.95f);
        var outline = menuPanel.AddComponent<Outline>();
        outline.effectColor = PANEL_BORDER;
        outline.effectDistance = new Vector2(2f, 2f);

        // Title
        CreateText("MenuTitle", rect, "MENU",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -10f), new Vector2(240f, 35f), 24, TextAlignmentOptions.Center)
            .fontStyle = FontStyles.Bold;

        // Buttons
        string[] labels = { "DINOS", "SAC", "DINODEX", "CARTE", "SAUVEGARDER", "OPTIONS", "QUITTER" };
        Color[] colors = { BTN_DINOS, BTN_SAC, BTN_DINODEX, BTN_CARTE, BTN_SAVE, BTN_OPTIONS, BTN_QUIT };
        System.Action[] actions = {
            OnDinos, OnBag, OnDinodex, OnCarte, OnSave, OnOptions, OnQuit
        };

        menuButtons = new Button[labels.Length];
        menuBaseColors = new Color[labels.Length];

        for (int i = 0; i < labels.Length; i++)
        {
            float yPos = -50f - i * 56f;
            var btn = CreateMenuButton(labels[i], rect, new Vector2(10f, yPos),
                new Vector2(240f, 48f), colors[i], actions[i]);

            menuButtons[i] = btn;
            menuBaseColors[i] = colors[i];

            // Disable unimplemented buttons
            if (labels[i] == "DINODEX" || labels[i] == "OPTIONS")
            {
                btn.interactable = false;
                var btnImg = btn.GetComponent<Image>();
                btnImg.color = new Color(colors[i].r, colors[i].g, colors[i].b, 0.4f);
            }
        }
    }

    // ===============================================================
    // Bag Panel
    // ===============================================================

    private void CreateBagPanel()
    {
        bagPanel = new GameObject("BagPanel");
        bagPanel.transform.SetParent(rootPanel.transform, false);
        var rect = bagPanel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(500f, 500f);

        var img = bagPanel.AddComponent<Image>();
        img.color = new Color(PANEL_BG.r, PANEL_BG.g, PANEL_BG.b, 0.95f);
        var outline = bagPanel.AddComponent<Outline>();
        outline.effectColor = PANEL_BORDER;
        outline.effectDistance = new Vector2(2f, 2f);

        // Title
        CreateText("BagTitle", rect, "SAC",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -10f), new Vector2(460f, 35f), 26, TextAlignmentOptions.Center)
            .fontStyle = FontStyles.Bold;

        // Scrollable list area
        var listArea = new GameObject("BagListArea");
        listArea.transform.SetParent(bagPanel.transform, false);
        var listRect = listArea.AddComponent<RectTransform>();
        listRect.anchorMin = new Vector2(0f, 0f);
        listRect.anchorMax = new Vector2(1f, 1f);
        listRect.offsetMin = new Vector2(15f, 60f);
        listRect.offsetMax = new Vector2(-15f, -50f);

        // Mask for scrolling
        var mask = listArea.AddComponent<RectMask2D>();

        // Content container
        var contentGO = new GameObject("BagContent");
        contentGO.transform.SetParent(listArea.transform, false);
        bagListContent = contentGO.AddComponent<RectTransform>();
        bagListContent.anchorMin = new Vector2(0f, 1f);
        bagListContent.anchorMax = new Vector2(1f, 1f);
        bagListContent.pivot = new Vector2(0.5f, 1f);
        bagListContent.anchoredPosition = Vector2.zero;
        bagListContent.sizeDelta = new Vector2(0f, 0f);

        // Empty text
        bagEmptyText = CreateText("BagEmpty", rect, "Le sac est vide...",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 0f), new Vector2(400f, 40f), 20, TextAlignmentOptions.Center);
        bagEmptyText.color = new Color(TEXT_COLOR.r, TEXT_COLOR.g, TEXT_COLOR.b, 0.5f);
        bagEmptyText.fontStyle = FontStyles.Italic;

        // Back button
        CreateMenuButton("RETOUR", rect, new Vector2(10f, -455f),
            new Vector2(120f, 36f), new Color(0.5f, 0.5f, 0.5f), OnBagBack);

        bagPanel.SetActive(false);
    }

    // ===============================================================
    // Save Confirm Panel
    // ===============================================================

    private void CreateSaveConfirmPanel()
    {
        saveConfirmPanel = new GameObject("SaveConfirm");
        saveConfirmPanel.transform.SetParent(rootPanel.transform, false);
        var rect = saveConfirmPanel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(400f, 160f);

        var img = saveConfirmPanel.AddComponent<Image>();
        img.color = new Color(PANEL_BG.r, PANEL_BG.g, PANEL_BG.b, 0.98f);
        var outline = saveConfirmPanel.AddComponent<Outline>();
        outline.effectColor = PANEL_BORDER;
        outline.effectDistance = new Vector2(2f, 2f);

        CreateText("SaveMsg", rect, "Partie sauvegardee !",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, 20f), new Vector2(360f, 40f), 24, TextAlignmentOptions.Center)
            .fontStyle = FontStyles.Bold;

        CreateMenuButton("OK", rect, new Vector2(140f, -110f),
            new Vector2(120f, 40f), BTN_SAVE, OnSaveConfirmOk);

        saveConfirmPanel.SetActive(false);
    }

    // ===============================================================
    // Panel Navigation
    // ===============================================================

    private void ShowMainMenu()
    {
        menuPanel.SetActive(true);
        bagPanel.SetActive(false);
        saveConfirmPanel.SetActive(false);
        isInSubPanel = false;

        if (partyUI != null)
            partyUI.Hide();

        if (worldMapUI != null)
            worldMapUI.Hide();

        UpdateMenuSelectionVisual();
    }

    // ===============================================================
    // Button Callbacks
    // ===============================================================

    private void OnDinos()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuSelect();
        isInSubPanel = true;
        menuPanel.SetActive(false);
        bagPanel.SetActive(false);

        // Create PartyUI if needed
        if (partyUI == null)
        {
            var partyGO = new GameObject("PartyUI");
            partyGO.transform.SetParent(rootPanel.transform, false);
            partyUI = partyGO.AddComponent<PartyUI>();
            partyUI.OnBack = () => ShowMainMenu();
        }

        partyUI.Show(GameState.Instance.Party);
    }

    private void OnBag()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuSelect();
        isInSubPanel = true;
        menuPanel.SetActive(false);
        bagPanel.SetActive(true);
        PopulateBagList();
    }

    private void OnDinodex()
    {
        // Not implemented yet
        Debug.Log("[PauseMenu] Dinodex not implemented yet");
    }

    private void OnCarte()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuSelect();
        isInSubPanel = true;
        menuPanel.SetActive(false);

        // Create WorldMapUI if needed
        if (worldMapUI == null)
        {
            var mapGO = new GameObject("WorldMapUI");
            mapGO.transform.SetParent(rootPanel.transform, false);
            worldMapUI = mapGO.AddComponent<WorldMapUI>();
            worldMapUI.OnClose = () => ShowMainMenu();
        }

        worldMapUI.Show();
    }

    private void OnSave()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuSelect();
        isInSubPanel = true;
        SaveSystem.Save(GameState.Instance);
        menuPanel.SetActive(false);
        saveConfirmPanel.SetActive(true);
    }

    private void OnOptions()
    {
        // Not implemented yet
        Debug.Log("[PauseMenu] Options not implemented yet");
    }

    private void OnQuit()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuSelect();
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene("Title");
    }

    private void OnBagBack()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuMove();
        ShowMainMenu();
    }

    private void OnSaveConfirmOk()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuSelect();
        ShowMainMenu();
    }

    // ===============================================================
    // Bag Population
    // ===============================================================

    private void PopulateBagList()
    {
        // Clear existing items
        for (int i = bagListContent.childCount - 1; i >= 0; i--)
            Destroy(bagListContent.GetChild(i).gameObject);

        var items = GameState.Instance.Inventory.GetAllItems();

        if (items.Count == 0)
        {
            bagEmptyText.gameObject.SetActive(true);
            bagListContent.sizeDelta = new Vector2(0f, 0f);
            return;
        }

        bagEmptyText.gameObject.SetActive(false);
        float itemHeight = 44f;
        float spacing = 4f;
        float totalHeight = items.Count * (itemHeight + spacing);
        bagListContent.sizeDelta = new Vector2(0f, totalHeight);

        for (int i = 0; i < items.Count; i++)
        {
            var kvp = items[i];
            var itemData = DataLoader.Instance.GetItem(kvp.Key);
            string itemName = itemData != null ? itemData.name : $"Objet #{kvp.Key}";
            int count = kvp.Value;

            float yPos = -i * (itemHeight + spacing);

            // Item row
            var rowGO = new GameObject($"Item_{kvp.Key}");
            rowGO.transform.SetParent(bagListContent, false);
            var rowRect = rowGO.AddComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0f, 1f);
            rowRect.anchorMax = new Vector2(1f, 1f);
            rowRect.pivot = new Vector2(0.5f, 1f);
            rowRect.anchoredPosition = new Vector2(0f, yPos);
            rowRect.sizeDelta = new Vector2(0f, itemHeight);

            var rowImg = rowGO.AddComponent<Image>();
            rowImg.color = new Color(PANEL_BG.r + 0.05f, PANEL_BG.g + 0.05f, PANEL_BG.b + 0.1f, 0.8f);

            // Item name
            CreateText($"ItemName_{kvp.Key}", rowRect, itemName,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(15f, 0f), new Vector2(300f, 30f), 18, TextAlignmentOptions.Left);

            // Quantity
            CreateText($"ItemQty_{kvp.Key}", rowRect, $"x{count}",
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-15f, 0f), new Vector2(80f, 30f), 18, TextAlignmentOptions.Right);
        }
    }

    // ===============================================================
    // UI Factory Helpers (same pattern as BattleUI)
    // ===============================================================

    private TextMeshProUGUI CreateText(string name, RectTransform parent, string content,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta,
        float fontSize, TextAlignmentOptions alignment)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = anchorMin;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = TEXT_COLOR;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Overflow;

        return tmp;
    }

    private Button CreateMenuButton(string label, RectTransform parent,
        Vector2 anchoredPos, Vector2 sizeDelta, Color bgColor, System.Action onClick)
    {
        var go = new GameObject($"Btn_{label}");
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;

        var img = go.AddComponent<Image>();
        img.color = bgColor;

        var btn = go.AddComponent<Button>();
            btn.navigation = new Navigation { mode = Navigation.Mode.None };
        btn.targetGraphic = img;

        var colors = btn.colors;
        colors.normalColor = bgColor;
        colors.highlightedColor = Color.Lerp(bgColor, Color.white, 0.2f);
        colors.pressedColor = bgColor * 0.8f;
        colors.selectedColor = Color.Lerp(bgColor, Color.white, 0.1f);
        btn.colors = colors;

        btn.onClick.AddListener(() => onClick());

        var outl = go.AddComponent<Outline>();
        outl.effectColor = Color.Lerp(bgColor, Color.black, 0.3f);
        outl.effectDistance = new Vector2(2f, 2f);

        // Label text
        var textGO = new GameObject("Label");
        textGO.transform.SetParent(go.transform, false);
        var textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 20;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = TEXT_COLOR;
        tmp.fontStyle = FontStyles.Bold;

        return btn;
    }

    private void StretchFill(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        // Ensure time scale is restored if destroyed while paused
        if (isPaused) Time.timeScale = 1f;
    }
}
