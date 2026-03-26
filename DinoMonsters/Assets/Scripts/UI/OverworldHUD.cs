// ============================================================
// Dino Monsters -- Overworld HUD
// Always-visible HUD during overworld: location name, money,
// interaction hint. Programmatic UI.
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class OverworldHUD : MonoBehaviour
{
    public static OverworldHUD Instance { get; private set; }

    // --- UI ---
    private Canvas canvas;
    private RectTransform canvasRect;
    private TextMeshProUGUI locationText;
    private TextMeshProUGUI moneyText;
    private GameObject hintPanel;
    private TextMeshProUGUI hintText;

    // --- State ---
    private string currentLocation = "";
    private bool hintVisible = false;
    private float hintTimer = 0f;

    // --- Colors ---
    private static readonly Color TEXT_COLOR = Constants.ColorTextPrimary;
    private static readonly Color PANEL_BG = Constants.ColorUiBg;
    private static readonly Color PANEL_BORDER = Constants.ColorUiBorder;
    private static readonly Color MONEY_COLOR = new Color(0.94f, 0.82f, 0.30f); // Gold

    // --- Map display names ---
    private static string GetMapDisplayName(string mapId)
    {
        switch (mapId)
        {
            case "BOURG_NID":      return "Bourg-Nid";
            case "ROUTE_1":        return "Route 1";
            case "LAB_INTERIOR":   return "Laboratoire Prof. Saule";
            case "HOUSE_PLAYER":   return "Maison";
            case "HOUSE_RIVAL":    return "Maison du Rival";
            case "VILLE_CRETE":    return "Ville-Crete";
            default:               return mapId;
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        CreateUI();
    }

    void Update()
    {
        // Update money display
        if (moneyText != null)
        {
            int money = GameState.Instance.Money;
            moneyText.text = $"$ {money:N0}";
        }

        // Update location
        if (OverworldManager.Instance != null)
        {
            string mapId = GameState.Instance.CurrentMapId;
            if (mapId != currentLocation)
            {
                currentLocation = mapId;
                locationText.text = GetMapDisplayName(mapId);
            }
        }

        // Check interaction hint
        UpdateInteractionHint();

        // Hide hint after timer
        if (hintVisible && hintTimer > 0f)
        {
            hintTimer -= Time.deltaTime;
            if (hintTimer <= 0f)
                HideHint();
        }
    }

    // ===============================================================
    // Create UI
    // ===============================================================

    private void CreateUI()
    {
        // --- EventSystem ---
        if (FindObjectOfType<EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();
        }

        // --- Canvas ---
        var canvasGO = new GameObject("OverworldHUDCanvas");
        canvasGO.transform.SetParent(transform);
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50; // Below pause menu

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();
        canvasRect = canvasGO.GetComponent<RectTransform>();

        // --- Location name (top-left) ---
        var locPanel = CreateHUDPanel("LocationPanel", canvasRect,
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(12f, -12f), new Vector2(260f, 36f));

        locationText = CreateText("LocationText", locPanel, "",
            new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
            Vector2.zero, Vector2.zero,
            18, TextAlignmentOptions.Center);
        var locTextRect = locationText.GetComponent<RectTransform>();
        locTextRect.anchorMin = Vector2.zero;
        locTextRect.anchorMax = Vector2.one;
        locTextRect.offsetMin = new Vector2(10f, 0f);
        locTextRect.offsetMax = new Vector2(-10f, 0f);

        // --- Money (top-right) ---
        var moneyPanel = CreateHUDPanel("MoneyPanel", canvasRect,
            new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-12f, -12f), new Vector2(180f, 36f));
        // Shift pivot to top-right
        moneyPanel.pivot = new Vector2(1f, 1f);

        moneyText = CreateText("MoneyText", moneyPanel, "$ 0",
            new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
            Vector2.zero, Vector2.zero,
            18, TextAlignmentOptions.Center);
        moneyText.color = MONEY_COLOR;
        moneyText.fontStyle = FontStyles.Bold;
        var moneyTextRect = moneyText.GetComponent<RectTransform>();
        moneyTextRect.anchorMin = Vector2.zero;
        moneyTextRect.anchorMax = Vector2.one;
        moneyTextRect.offsetMin = new Vector2(10f, 0f);
        moneyTextRect.offsetMax = new Vector2(-10f, 0f);

        // --- Interaction hint (bottom-center) ---
        hintPanel = new GameObject("HintPanel");
        hintPanel.transform.SetParent(canvasGO.transform, false);
        var hintRect = hintPanel.AddComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0.5f, 0f);
        hintRect.anchorMax = new Vector2(0.5f, 0f);
        hintRect.pivot = new Vector2(0.5f, 0f);
        hintRect.anchoredPosition = new Vector2(0f, 20f);
        hintRect.sizeDelta = new Vector2(360f, 36f);

        var hintImg = hintPanel.AddComponent<Image>();
        hintImg.color = new Color(PANEL_BG.r, PANEL_BG.g, PANEL_BG.b, 0.85f);
        var hintOutline = hintPanel.AddComponent<Outline>();
        hintOutline.effectColor = PANEL_BORDER;
        hintOutline.effectDistance = new Vector2(1f, 1f);

        hintText = CreateText("HintText", hintRect, "Appuyez sur E pour interagir",
            new Vector2(0f, 0f), new Vector2(1f, 1f),
            Vector2.zero, Vector2.zero,
            16, TextAlignmentOptions.Center);
        var hintTextRect = hintText.GetComponent<RectTransform>();
        hintTextRect.anchorMin = Vector2.zero;
        hintTextRect.anchorMax = Vector2.one;
        hintTextRect.offsetMin = Vector2.zero;
        hintTextRect.offsetMax = Vector2.zero;

        hintPanel.SetActive(false);
    }

    // ===============================================================
    // Interaction Hint
    // ===============================================================

    private void UpdateInteractionHint()
    {
        var player = FindObjectOfType<PlayerController>();
        if (player == null || OverworldManager.Instance == null)
        {
            if (hintVisible) HideHint();
            return;
        }

        // Check if there's something interactable in front of the player
        Vector2Int facingTile = player.GetFacingTile();
        bool hasInteractable = HasInteractable(facingTile.x, facingTile.y);

        if (hasInteractable && !hintVisible)
        {
            ShowHint("Appuyez sur E pour interagir");
        }
        else if (!hasInteractable && hintVisible && hintTimer <= 0f)
        {
            HideHint();
        }
    }

    private bool HasInteractable(int x, int y)
    {
        var om = OverworldManager.Instance;
        if (om == null) return false;
        return om.HasInteractable(x, y);
    }

    public void ShowHint(string text)
    {
        hintText.text = text;
        hintPanel.SetActive(true);
        hintVisible = true;
        hintTimer = 0f; // No auto-hide when persistent
    }

    public void ShowHintTimed(string text, float duration = 3f)
    {
        hintText.text = text;
        hintPanel.SetActive(true);
        hintVisible = true;
        hintTimer = duration;
    }

    public void HideHint()
    {
        hintPanel.SetActive(false);
        hintVisible = false;
        hintTimer = 0f;
    }

    // ===============================================================
    // Public API
    // ===============================================================

    public void SetVisible(bool visible)
    {
        canvas.gameObject.SetActive(visible);
    }

    // ===============================================================
    // UI Factory Helpers
    // ===============================================================

    private RectTransform CreateHUDPanel(string name, RectTransform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = anchorMin;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;

        var img = go.AddComponent<Image>();
        img.color = new Color(PANEL_BG.r, PANEL_BG.g, PANEL_BG.b, 0.80f);

        var outline = go.AddComponent<Outline>();
        outline.effectColor = PANEL_BORDER;
        outline.effectDistance = new Vector2(1f, 1f);

        return rect;
    }

    private TextMeshProUGUI CreateText(string name, RectTransform parent, string content,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta,
        float fontSize, TextAlignmentOptions alignment)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
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

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
