// ============================================================
// Dino Monsters -- Shop UI (Buy/Sell overlay)
// ============================================================
// Dedicated full-screen shop panel with scrollable item list,
// quantity selection, and persistent cursor position.
// Uses the Fossil/Amber art direction palette.
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance { get; private set; }

    // --- Config ---
    private int[] itemIds;
    private string[] itemNames;
    private int[] itemPrices;
    private Action onClose;

    // --- State ---
    private bool isOpen = false;
    private bool isBuyMode = true;
    private int cursorIndex = 0;
    private int scrollOffset = 0;
    private const int VISIBLE_ROWS = 6;

    // Quantity selection
    private bool selectingQty = false;
    private int quantity = 1;

    // --- UI refs ---
    private Canvas canvas;
    private GameObject rootPanel;
    private GameObject listPanel;
    private GameObject[] rowObjects;
    private TextMeshProUGUI[] rowTexts;
    private Image[] rowBgs;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI moneyText;
    private TextMeshProUGUI infoText;
    private TextMeshProUGUI hintText;
    private GameObject qtyPanel;
    private TextMeshProUGUI qtyText;
    private TextMeshProUGUI qtyPriceText;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildUI();
        rootPanel.SetActive(false);
    }

    void Update()
    {
        if (!isOpen) return;

        if (selectingQty)
            HandleQtyInput();
        else
            HandleListInput();
    }

    // ===============================================================
    // Public API
    // ===============================================================

    public void OpenBuy(int[] ids, string[] names, int[] prices, Action onCloseCallback)
    {
        isBuyMode = true;
        itemIds = ids;
        itemNames = names;
        itemPrices = prices;
        onClose = onCloseCallback;
        cursorIndex = 0;
        scrollOffset = 0;
        selectingQty = false;
        quantity = 1;

        titleText.text = "ACHETER";
        isOpen = true;
        rootPanel.SetActive(true);
        LockPlayer();
        RefreshList();
    }

    public void OpenSell(Action onCloseCallback)
    {
        isBuyMode = false;
        onClose = onCloseCallback;
        cursorIndex = 0;
        scrollOffset = 0;
        selectingQty = false;
        quantity = 1;

        titleText.text = "VENDRE";
        BuildSellData();
        isOpen = true;
        rootPanel.SetActive(true);
        LockPlayer();
        RefreshList();
    }

    public void Close()
    {
        isOpen = false;
        rootPanel.SetActive(false);
        var cb = onClose;
        onClose = null;
        cb?.Invoke();
    }

    private void LockPlayer()
    {
        var player = UnityEngine.Object.FindObjectOfType<PlayerController>();
        if (player != null) player.LockInput();
    }

    public bool IsOpen => isOpen;

    // ===============================================================
    // Input — List Navigation
    // ===============================================================

    private void HandleListInput()
    {
        int totalItems = (itemIds != null ? itemIds.Length : 0) + 1; // +1 for "Retour"

        if (InputHelper.Up)
        {
            cursorIndex--;
            if (cursorIndex < 0) cursorIndex = totalItems - 1;
            EnsureVisible();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuMove();
            RefreshList();
        }
        else if (InputHelper.Down)
        {
            cursorIndex++;
            if (cursorIndex >= totalItems) cursorIndex = 0;
            EnsureVisible();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuMove();
            RefreshList();
        }
        else if (InputHelper.Confirm)
        {
            if (itemIds != null && cursorIndex < itemIds.Length)
            {
                // Enter quantity mode
                selectingQty = true;
                quantity = 1;
                if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuSelect();
                RefreshQtyPanel();
                qtyPanel.SetActive(true);
            }
            else
            {
                // "Retour"
                Close();
            }
        }
        else if (InputHelper.Cancel)
        {
            Close();
        }
    }

    // ===============================================================
    // Input — Quantity Selection
    // ===============================================================

    private void HandleQtyInput()
    {
        int maxQty = 99;
        if (isBuyMode && cursorIndex < itemPrices.Length)
        {
            int price = itemPrices[cursorIndex];
            maxQty = price > 0 ? GameState.Instance.Money / price : 99;
        }
        else if (!isBuyMode && cursorIndex < itemIds.Length)
        {
            maxQty = GameState.Instance.Inventory.GetCount(itemIds[cursorIndex]);
        }
        if (maxQty < 1) maxQty = 1;

        if (InputHelper.Right)
        {
            quantity = Mathf.Min(maxQty, quantity + 1);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuMove();
            RefreshQtyPanel();
        }
        else if (InputHelper.Left)
        {
            quantity = Mathf.Max(1, quantity - 1);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuMove();
            RefreshQtyPanel();
        }
        else if (InputHelper.Up)
        {
            quantity = Mathf.Min(maxQty, quantity + 5);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuMove();
            RefreshQtyPanel();
        }
        else if (InputHelper.Down)
        {
            quantity = Mathf.Max(1, quantity - 5);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuMove();
            RefreshQtyPanel();
        }
        else if (InputHelper.Confirm)
        {
            ExecuteTransaction();
            selectingQty = false;
            qtyPanel.SetActive(false);
            // Rebuild sell data if selling (item counts changed)
            if (!isBuyMode) BuildSellData();
            RefreshList();
        }
        else if (InputHelper.Cancel)
        {
            selectingQty = false;
            quantity = 1;
            qtyPanel.SetActive(false);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuMove();
        }
    }

    // ===============================================================
    // Transaction
    // ===============================================================

    private void ExecuteTransaction()
    {
        if (cursorIndex >= itemIds.Length) return;

        int itemId = itemIds[cursorIndex];
        string itemName = itemNames[cursorIndex];
        int unitPrice = itemPrices[cursorIndex];

        if (isBuyMode)
        {
            int totalCost = unitPrice * quantity;
            if (GameState.Instance.Money >= totalCost)
            {
                GameState.Instance.RemoveMoney(totalCost);
                GameState.Instance.Inventory.AddItem(itemId, quantity);
                if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuSelect();
                ShowInfo($"Achete {quantity}x {itemName} !");
            }
            else
            {
                ShowInfo("Pas assez d'argent !");
            }
        }
        else
        {
            int count = GameState.Instance.Inventory.GetCount(itemId);
            int sellQty = Mathf.Min(quantity, count);
            int totalGain = unitPrice * sellQty;
            for (int i = 0; i < sellQty; i++)
                GameState.Instance.Inventory.RemoveItem(itemId);
            GameState.Instance.AddMoney(totalGain);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuSelect();
            ShowInfo($"Vendu {sellQty}x {itemName} pour {totalGain}$ !");
        }
    }

    // ===============================================================
    // Sell Data Builder
    // ===============================================================

    private void BuildSellData()
    {
        var allItems = GameState.Instance.Inventory.GetAllItems();
        var ids = new List<int>();
        var names = new List<string>();
        var prices = new List<int>();

        foreach (var kvp in allItems)
        {
            if (kvp.Value <= 0) continue;
            var data = DataLoader.Instance?.GetItem(kvp.Key);
            ids.Add(kvp.Key);
            names.Add(data != null ? data.name : $"Objet #{kvp.Key}");
            prices.Add(data != null ? data.price / 2 : 50);
        }

        itemIds = ids.ToArray();
        itemNames = names.ToArray();
        itemPrices = prices.ToArray();

        // Clamp cursor if items were removed
        int total = itemIds.Length + 1;
        if (cursorIndex >= total) cursorIndex = total - 1;
        if (cursorIndex < 0) cursorIndex = 0;
    }

    // ===============================================================
    // UI Refresh
    // ===============================================================

    private void RefreshList()
    {
        moneyText.text = $"$ {GameState.Instance.Money}";
        int totalItems = (itemIds != null ? itemIds.Length : 0) + 1;

        for (int i = 0; i < VISIBLE_ROWS; i++)
        {
            int dataIdx = scrollOffset + i;
            if (dataIdx >= totalItems)
            {
                rowObjects[i].SetActive(false);
                continue;
            }

            rowObjects[i].SetActive(true);
            bool isSelected = (dataIdx == cursorIndex);
            bool isRetour = (dataIdx == totalItems - 1);

            if (isRetour)
            {
                rowTexts[i].text = "    Retour";
                rowBgs[i].color = isSelected
                    ? new Color(Constants.ColorResine.r, Constants.ColorResine.g, Constants.ColorResine.b, 0.35f)
                    : new Color(0, 0, 0, 0.08f);
            }
            else
            {
                string arrow = isSelected ? ">" : "    ";
                int owned = GameState.Instance.Inventory.GetCount(itemIds[dataIdx]);
                string ownedStr = owned > 0 ? $"  (x{owned})" : "";
                rowTexts[i].text = $"{arrow}{itemNames[dataIdx]}    {itemPrices[dataIdx]}${ownedStr}";
                rowBgs[i].color = isSelected
                    ? new Color(Constants.ColorResine.r, Constants.ColorResine.g, Constants.ColorResine.b, 0.35f)
                    : new Color(0, 0, 0, (dataIdx % 2 == 0) ? 0.05f : 0.12f);
            }

            rowTexts[i].color = isSelected ? Constants.ColorResine : Constants.ColorTextDarkPrimary;
            if (isSelected) rowTexts[i].fontStyle = FontStyles.Bold;
            else rowTexts[i].fontStyle = FontStyles.Normal;
        }

        // Show item description if an item is selected
        if (cursorIndex < (itemIds != null ? itemIds.Length : 0))
        {
            var data = DataLoader.Instance?.GetItem(itemIds[cursorIndex]);
            infoText.text = data != null && data.description != null ? data.description : "";
        }
        else
        {
            infoText.text = "";
        }

        // Hint text
        hintText.text = selectingQty ? "<> Quantite   Haut/Bas x5   [Entree] Confirmer   [Echap] Annuler"
            : "[Entree] Choisir   [Echap] Quitter";
    }

    private void RefreshQtyPanel()
    {
        if (cursorIndex >= itemIds.Length) return;
        int price = itemPrices[cursorIndex];
        qtyText.text = $"<  {quantity}  >";
        qtyPriceText.text = $"Total: {quantity * price}$";
    }

    private void ShowInfo(string msg)
    {
        infoText.text = msg;
    }

    private void EnsureVisible()
    {
        if (cursorIndex < scrollOffset)
            scrollOffset = cursorIndex;
        else if (cursorIndex >= scrollOffset + VISIBLE_ROWS)
            scrollOffset = cursorIndex - VISIBLE_ROWS + 1;
        if (scrollOffset < 0) scrollOffset = 0;
    }

    // ===============================================================
    // UI Construction — Fossil/Amber parchment style
    // ===============================================================

    private void BuildUI()
    {
        // Canvas
        var canvasGO = new GameObject("ShopCanvas");
        canvasGO.transform.SetParent(transform);
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 180;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // Root overlay
        rootPanel = new GameObject("ShopRoot");
        rootPanel.transform.SetParent(canvasGO.transform, false);
        var rootRect = rootPanel.AddComponent<RectTransform>();
        Stretch(rootRect);
        rootPanel.AddComponent<Image>().color = Constants.ColorMenuOverlay;

        // Main panel (parchment)
        var mainGO = new GameObject("MainPanel");
        mainGO.transform.SetParent(rootPanel.transform, false);
        var mainRect = mainGO.AddComponent<RectTransform>();
        mainRect.anchorMin = new Vector2(0.5f, 0.5f);
        mainRect.anchorMax = new Vector2(0.5f, 0.5f);
        mainRect.sizeDelta = new Vector2(540, 420);

        // Outer border
        mainGO.AddComponent<Image>().color = Constants.ColorTerreBrulee;

        // Inner fill
        var innerGO = new GameObject("Inner");
        innerGO.transform.SetParent(mainGO.transform, false);
        var innerRect = innerGO.AddComponent<RectTransform>();
        Stretch(innerRect);
        innerRect.offsetMin = new Vector2(3, 3);
        innerRect.offsetMax = new Vector2(-3, -3);
        innerGO.AddComponent<Image>().color = Constants.ColorCalcaire;

        // Amber accent line top
        var accentGO = new GameObject("Accent");
        accentGO.transform.SetParent(mainGO.transform, false);
        var accentRect = accentGO.AddComponent<RectTransform>();
        accentRect.anchorMin = new Vector2(0, 1);
        accentRect.anchorMax = new Vector2(1, 1);
        accentRect.pivot = new Vector2(0.5f, 1);
        accentRect.anchoredPosition = Vector2.zero;
        accentRect.sizeDelta = new Vector2(0, 3);
        accentGO.AddComponent<Image>().color = Constants.ColorResine;

        // Title bar
        var titleBg = new GameObject("TitleBg");
        titleBg.transform.SetParent(innerGO.transform, false);
        var tbRect = titleBg.AddComponent<RectTransform>();
        tbRect.anchorMin = new Vector2(0, 1);
        tbRect.anchorMax = new Vector2(1, 1);
        tbRect.pivot = new Vector2(0.5f, 1);
        tbRect.anchoredPosition = new Vector2(0, -2);
        tbRect.sizeDelta = new Vector2(-10, 36);
        titleBg.AddComponent<Image>().color = new Color(Constants.ColorSediment.r, Constants.ColorSediment.g, Constants.ColorSediment.b, 0.3f);

        titleText = MakeText(titleBg.transform, "Title", "ACHETER", 20, FontStyles.Bold, Constants.ColorTerreBrulee);
        Stretch(titleText.rectTransform);
        titleText.alignment = TextAlignmentOptions.Center;

        // Money display (top right)
        moneyText = MakeText(innerGO.transform, "Money", "$ 0", 18, FontStyles.Bold, Constants.ColorResine);
        var mRect = moneyText.rectTransform;
        mRect.anchorMin = new Vector2(1, 1);
        mRect.anchorMax = new Vector2(1, 1);
        mRect.pivot = new Vector2(1, 1);
        mRect.anchoredPosition = new Vector2(-15, -8);
        mRect.sizeDelta = new Vector2(150, 30);
        moneyText.alignment = TextAlignmentOptions.Right;

        // Item list area
        listPanel = new GameObject("ListPanel");
        listPanel.transform.SetParent(innerGO.transform, false);
        var lpRect = listPanel.AddComponent<RectTransform>();
        lpRect.anchorMin = new Vector2(0, 0.22f);
        lpRect.anchorMax = new Vector2(1, 0.88f);
        lpRect.offsetMin = new Vector2(8, 0);
        lpRect.offsetMax = new Vector2(-8, 0);

        // Create row slots
        rowObjects = new GameObject[VISIBLE_ROWS];
        rowTexts = new TextMeshProUGUI[VISIBLE_ROWS];
        rowBgs = new Image[VISIBLE_ROWS];

        for (int i = 0; i < VISIBLE_ROWS; i++)
        {
            var rowGO = new GameObject($"Row_{i}");
            rowGO.transform.SetParent(listPanel.transform, false);
            var rr = rowGO.AddComponent<RectTransform>();
            rr.anchorMin = new Vector2(0, 1);
            rr.anchorMax = new Vector2(1, 1);
            rr.pivot = new Vector2(0.5f, 1);
            float rowH = 38f;
            rr.anchoredPosition = new Vector2(0, -i * (rowH + 2));
            rr.sizeDelta = new Vector2(0, rowH);

            var bg = rowGO.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.05f);
            rowBgs[i] = bg;

            var txt = MakeText(rowGO.transform, "Text", "", 17, FontStyles.Normal, Constants.ColorTextDarkPrimary);
            Stretch(txt.rectTransform);
            txt.rectTransform.offsetMin = new Vector2(10, 0);
            txt.rectTransform.offsetMax = new Vector2(-10, 0);
            txt.alignment = TextAlignmentOptions.MidlineLeft;
            rowTexts[i] = txt;

            rowObjects[i] = rowGO;
        }

        // Info / description area (bottom)
        infoText = MakeText(innerGO.transform, "Info", "", 14, FontStyles.Italic, Constants.ColorTextSecondary);
        var iRect = infoText.rectTransform;
        iRect.anchorMin = new Vector2(0, 0.08f);
        iRect.anchorMax = new Vector2(1, 0.22f);
        iRect.offsetMin = new Vector2(15, 0);
        iRect.offsetMax = new Vector2(-15, 0);
        infoText.alignment = TextAlignmentOptions.TopLeft;
        infoText.enableWordWrapping = true;

        // Hint bar (bottom)
        hintText = MakeText(innerGO.transform, "Hint", "", 12, FontStyles.Normal, Constants.ColorTextSecondary);
        var hRect = hintText.rectTransform;
        hRect.anchorMin = new Vector2(0, 0);
        hRect.anchorMax = new Vector2(1, 0.08f);
        hRect.offsetMin = new Vector2(10, 0);
        hRect.offsetMax = new Vector2(-10, 0);
        hintText.alignment = TextAlignmentOptions.Center;

        // Quantity selection panel (overlay on top of list)
        qtyPanel = new GameObject("QtyPanel");
        qtyPanel.transform.SetParent(innerGO.transform, false);
        var qpRect = qtyPanel.AddComponent<RectTransform>();
        qpRect.anchorMin = new Vector2(0.5f, 0.5f);
        qpRect.anchorMax = new Vector2(0.5f, 0.5f);
        qpRect.sizeDelta = new Vector2(220, 90);
        qtyPanel.AddComponent<Image>().color = Constants.ColorTerreBrulee;

        var qpInner = new GameObject("QtyInner");
        qpInner.transform.SetParent(qtyPanel.transform, false);
        var qpIR = qpInner.AddComponent<RectTransform>();
        Stretch(qpIR);
        qpIR.offsetMin = new Vector2(2, 2);
        qpIR.offsetMax = new Vector2(-2, -2);
        qpInner.AddComponent<Image>().color = Constants.ColorCalcaire;

        MakeText(qpInner.transform, "QtyLabel", "Quantite", 14, FontStyles.Bold, Constants.ColorTerreBrulee)
            .rectTransform.anchoredPosition = new Vector2(0, 25);

        qtyText = MakeText(qpInner.transform, "QtyValue", "<  1  >", 22, FontStyles.Bold, Constants.ColorResine);
        qtyText.rectTransform.anchoredPosition = new Vector2(0, 0);
        qtyText.alignment = TextAlignmentOptions.Center;

        qtyPriceText = MakeText(qpInner.transform, "QtyPrice", "Total: 300$", 14, FontStyles.Normal, Constants.ColorTextDarkPrimary);
        qtyPriceText.rectTransform.anchoredPosition = new Vector2(0, -22);
        qtyPriceText.alignment = TextAlignmentOptions.Center;

        qtyPanel.SetActive(false);
    }

    // ===============================================================
    // Helpers
    // ===============================================================

    private TextMeshProUGUI MakeText(Transform parent, string name, string text,
        float size, FontStyles style, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.alignment = TextAlignmentOptions.Center;
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(500, 30);
        return tmp;
    }

    private void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
