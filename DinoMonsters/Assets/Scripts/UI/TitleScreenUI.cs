// ============================================================
// Dino Monsters -- Title Screen UI
// Programmatic UI: dark background, gold title, blink prompt,
// then menu buttons (New Game / Continue).
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TitleScreenUI : MonoBehaviour
{
    private Canvas canvas;
    private TMP_Text titleText;
    private TMP_Text subtitleText;
    private TMP_Text versionText;
    private Button newGameBtn;
    private Button continueBtn;
    private TMP_Text pressStartText;

    private bool showingMenu = false;
    private float blinkTimer = 0f;

    // --- Navigation ---
    private int selectedIndex = 0;
    private Button[] menuButtons;
    private Color[] menuButtonBaseColors;

    void Start()
    {
        CreateUI();

        // Don't show continue here — it will be shown in ShowMenu() if a save exists

        // Play title music
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayTitleMusic();
    }

    void Update()
    {
        if (!showingMenu)
        {
            // Blink "Press Enter" text
            blinkTimer += Time.deltaTime;
            if (pressStartText != null)
            {
                float alpha = Mathf.Abs(Mathf.Sin(blinkTimer * 2f));
                pressStartText.alpha = alpha;
            }

            // Any key or click shows the menu
            if (InputHelper.Confirm || Input.GetMouseButtonDown(0))
            {
                ShowMenu();
            }
        }
        else
        {
            // Navigate menu with Up/Down
            if (InputHelper.Up)
            {
                ChangeSelection(-1);
            }
            else if (InputHelper.Down)
            {
                ChangeSelection(1);
            }

            // Confirm selection
            if (InputHelper.Confirm)
            {
                ConfirmSelection();
            }
        }
    }

    void ChangeSelection(int delta)
    {
        if (menuButtons == null) return;

        // Build list of active button indices
        var activeIndices = new System.Collections.Generic.List<int>();
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] != null && menuButtons[i].gameObject.activeSelf)
                activeIndices.Add(i);
        }
        if (activeIndices.Count == 0) return;

        int currentPos = activeIndices.IndexOf(selectedIndex);
        if (currentPos < 0) currentPos = 0;

        currentPos += delta;
        if (currentPos < 0) currentPos = activeIndices.Count - 1;
        if (currentPos >= activeIndices.Count) currentPos = 0;

        selectedIndex = activeIndices[currentPos];

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMenuMove();

        UpdateSelectionVisual();
    }

    void ConfirmSelection()
    {
        if (menuButtons == null) return;
        if (selectedIndex == 0) OnNewGame();
        else if (selectedIndex == 1) OnContinue();
    }

    void UpdateSelectionVisual()
    {
        if (menuButtons == null) return;
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] == null) continue;
            var img = menuButtons[i].GetComponent<Image>();
            if (img == null) continue;

            if (i == selectedIndex)
            {
                // Highlighted: brighter color + slight scale
                img.color = menuButtonBaseColors[i] * 1.4f;
                menuButtons[i].transform.localScale = Vector3.one * 1.02f;
            }
            else
            {
                // Normal
                img.color = menuButtonBaseColors[i];
                menuButtons[i].transform.localScale = Vector3.one;
            }
        }
    }

    // ---------------------------------------------------------------
    // Build the entire UI hierarchy from code
    // ---------------------------------------------------------------

    void CreateUI()
    {
        // --- EventSystem (REQUIRED for UI clicks to work) ---
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // --- Canvas ---
        var canvasGO = new GameObject("TitleCanvas");
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
            new Color(0.08f, 0.06f, 0.15f)); // Dark purple
        StretchFill(bgGO.GetComponent<RectTransform>());

        // --- Title: DINO MONSTERS ---
        titleText = CreateText(canvasGO.transform, "TitleText", "DINO MONSTERS", 72,
            new Color(0.91f, 0.78f, 0.41f), // Gold
            new Vector2(0, 120));
        titleText.fontStyle = FontStyles.Bold;

        // --- Subtitle ---
        subtitleText = CreateText(canvasGO.transform, "SubtitleText",
            "L'Aventure Jurassique", 28,
            new Color(0.94f, 0.91f, 0.82f), // Cream
            new Vector2(0, 60));
        subtitleText.fontStyle = FontStyles.Italic;

        // --- Press Enter ---
        pressStartText = CreateText(canvasGO.transform, "PressStart",
            "APPUYEZ SUR START", 20,
            new Color(0.94f, 0.91f, 0.82f),
            new Vector2(0, -80));

        // --- Buttons (hidden until menu is shown) ---
        newGameBtn = CreateButton(canvasGO.transform, "NewGameBtn",
            "NOUVELLE PARTIE", new Vector2(0, -30), OnNewGame);
        newGameBtn.gameObject.SetActive(false);

        continueBtn = CreateButton(canvasGO.transform, "ContinueBtn",
            "CONTINUER", new Vector2(0, -90), OnContinue);
        continueBtn.gameObject.SetActive(false);

        // --- Version tag ---
        versionText = CreateText(canvasGO.transform, "Version",
            "v0.1.0", 14,
            new Color(1f, 1f, 1f, 0.3f),
            new Vector2(0, -330));
    }

    // ---------------------------------------------------------------
    // Menu transitions
    // ---------------------------------------------------------------

    void ShowMenu()
    {
        showingMenu = true;
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMenuSelect();
        pressStartText.gameObject.SetActive(false);
        newGameBtn.gameObject.SetActive(true);

        // continueBtn visibility was already set in Start based on save
        if (SaveSystem.HasSave())
            continueBtn.gameObject.SetActive(true);

        // Setup navigation arrays
        menuButtons = new Button[] { newGameBtn, continueBtn };
        menuButtonBaseColors = new Color[] {
            new Color(0.53f, 0.60f, 0.25f, 0.9f),
            new Color(0.53f, 0.60f, 0.25f, 0.9f)
        };
        selectedIndex = 0;
        UpdateSelectionVisual();
    }

    void OnNewGame()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMenuSelect();
            AudioManager.Instance.StopMusic();
        }
        SceneManager.LoadScene("StarterSelect");
    }

    void OnContinue()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMenuSelect();
            AudioManager.Instance.StopMusic();
        }
        SaveSystem.Load();
        SceneManager.LoadScene("Overworld");
    }

    // ---------------------------------------------------------------
    // UI factory helpers
    // ---------------------------------------------------------------

    TMP_Text CreateText(Transform parent, string name, string text, int fontSize,
        Color color, Vector2 anchoredPos)
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
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = new Vector2(800, 100);

        return tmp;
    }

    Button CreateButton(Transform parent, string name, string label,
        Vector2 anchoredPos, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        // Button background
        var img = go.AddComponent<Image>();
        img.color = new Color(0.53f, 0.60f, 0.25f, 0.9f); // Olive green

        var btn = go.AddComponent<Button>();
        btn.onClick.AddListener(onClick);

        // Disable automatic UI navigation (we handle it manually via InputHelper)
        btn.navigation = new Navigation { mode = Navigation.Mode.None };

        // Hover/press color block
        var colors = btn.colors;
        colors.normalColor = new Color(0.53f, 0.60f, 0.25f, 0.9f);
        colors.highlightedColor = new Color(0.63f, 0.70f, 0.35f, 1f);
        colors.pressedColor = new Color(0.43f, 0.50f, 0.15f, 1f);
        colors.selectedColor = colors.highlightedColor;
        btn.colors = colors;

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = new Vector2(300, 50);

        // Label
        var txtGO = new GameObject("Label");
        txtGO.transform.SetParent(go.transform, false);

        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 22;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;

        var txtRect = txtGO.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.sizeDelta = Vector2.zero;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        return btn;
    }

    GameObject CreatePanel(Transform parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
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
