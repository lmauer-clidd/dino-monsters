// ============================================================
// Dino Monsters -- Battle UI (Full HUD via Unity UI + TextMeshPro)
// ============================================================
//
// All UI is created programmatically. No prefabs required.
// Layout follows Pokemon-style battle HUD:
//   - Top-left:    Enemy info (name, level, HP bar, status)
//   - Bottom-right: Player info (name, level, HP bar, XP bar, status)
//   - Bottom:       Action menu (ATTAQUE / SAC / DINOS / FUITE)
//   - Bottom:       Move selection (4 buttons with type colors + PP)
//   - Center:       Message box for battle text
//   - Floating:     Damage numbers
// ============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class BattleUI : MonoBehaviour
{
    // --------------- Callbacks ---------------

    public Action<int> OnAttackSelected;
    public Action OnBagSelected;
    public Action OnDinoSelected;
    public Action OnRunSelected;

    // --------------- UI Root ---------------

    private Canvas canvas;
    private CanvasScaler scaler;
    private RectTransform canvasRect;

    // --------------- Panels ---------------

    // Enemy info (top-left)
    private RectTransform enemyPanel;
    private TextMeshProUGUI enemyNameText;
    private TextMeshProUGUI enemyLevelText;
    private Image enemyHpBarFill;
    private TextMeshProUGUI enemyStatusText;

    // Player info (bottom-right)
    private RectTransform playerPanel;
    private TextMeshProUGUI playerNameText;
    private TextMeshProUGUI playerLevelText;
    private Image playerHpBarFill;
    private TextMeshProUGUI playerHpText;
    private Image playerXpBarFill;
    private TextMeshProUGUI playerStatusText;

    // Action menu
    private RectTransform actionMenuPanel;
    private Button attackButton;
    private Button bagButton;
    private Button dinoButton;
    private Button runButton;

    // Move menu
    private RectTransform moveMenuPanel;
    private Button[] moveButtons = new Button[4];
    private TextMeshProUGUI[] moveNameTexts = new TextMeshProUGUI[4];
    private TextMeshProUGUI[] movePPTexts = new TextMeshProUGUI[4];
    private Image[] moveButtonImages = new Image[4];
    private Button moveBackButton;

    // Message box
    private RectTransform messagePanel;
    private TextMeshProUGUI messageText;
    public bool IsTypingMessage { get; private set; }
    private Coroutine typewriterCoroutine;

    // Damage numbers pool
    private List<GameObject> activeDamageNumbers = new List<GameObject>();

    // --- Navigation ---
    private int actionSelectedIndex = 0; // 0=ATTAQUE, 1=SAC, 2=DINOS, 3=FUITE (2x2 grid)
    private int moveSelectedIndex = 0;   // 0-3 for moves (2x2 grid)
    private Button[] actionButtons;
    private Color[] actionBaseColors;
    private Color[] moveBaseColors;

    // --- Cached colors ---
    private static readonly Color HP_GREEN = Constants.ColorHpGreen;
    private static readonly Color HP_YELLOW = Constants.ColorHpYellow;
    private static readonly Color HP_RED = Constants.ColorHpRed;
    private static readonly Color XP_BLUE = Constants.ColorXpBlue;
    private static readonly Color PANEL_BG = Constants.ColorUiBg;
    private static readonly Color PANEL_BORDER = Constants.ColorUiBorder;
    private static readonly Color TEXT_COLOR = Constants.ColorTextPrimary;
    private static readonly Color TEXT_DARK = Constants.ColorTextDark;
    private static readonly Color MENU_BG = Constants.ColorMenuBg;

    // --- HP animation ---
    private Coroutine playerHpAnim;
    private Coroutine enemyHpAnim;

    // ===============================================================
    // Initialization
    // ===============================================================

    void Awake()
    {
        CreateCanvas();
        CreateEnemyPanel();
        CreatePlayerPanel();
        CreateActionMenu();
        CreateMoveMenu();
        CreateMessageBox();

        // Start with menus hidden
        actionMenuPanel.gameObject.SetActive(false);
        moveMenuPanel.gameObject.SetActive(false);
        messagePanel.gameObject.SetActive(false);
    }

    // ===============================================================
    // Update -- Gamepad/Keyboard Navigation
    // ===============================================================

    void Update()
    {
        if (actionMenuPanel.gameObject.activeSelf)
        {
            HandleGridNavigation(ref actionSelectedIndex, 2, 2, () => UpdateActionSelectionVisual());

            if (InputHelper.Confirm)
            {
                OnActionButton(actionSelectedIndex);
            }
        }
        else if (moveMenuPanel.gameObject.activeSelf)
        {
            // Count active move buttons
            int activeCount = 0;
            for (int i = 0; i < 4; i++)
                if (moveButtons[i].gameObject.activeSelf && moveButtons[i].interactable)
                    activeCount++;

            if (activeCount > 0)
            {
                int cols = 2;
                int rows = (activeCount + 1) / 2;
                HandleGridNavigation(ref moveSelectedIndex, cols, rows, () => UpdateMoveSelectionVisual());

                // Clamp to active range
                if (moveSelectedIndex >= activeCount)
                    moveSelectedIndex = activeCount - 1;
            }

            if (InputHelper.Confirm && moveSelectedIndex >= 0 && moveSelectedIndex < 4
                && moveButtons[moveSelectedIndex].gameObject.activeSelf
                && moveButtons[moveSelectedIndex].interactable)
            {
                OnAttackSelected?.Invoke(moveSelectedIndex);
            }

            if (InputHelper.Cancel)
            {
                HideMoveMenu();
                ShowActionMenu();
            }
        }
    }

    private void HandleGridNavigation(ref int index, int cols, int rows, System.Action onChanged)
    {
        int col = index % cols;
        int row = index / cols;
        bool changed = false;

        if (InputHelper.Up)    { row--; changed = true; }
        if (InputHelper.Down)  { row++; changed = true; }
        if (InputHelper.Left)  { col--; changed = true; }
        if (InputHelper.Right) { col++; changed = true; }

        if (!changed) return;

        // Wrap
        if (row < 0) row = rows - 1;
        if (row >= rows) row = 0;
        if (col < 0) col = cols - 1;
        if (col >= cols) col = 0;

        index = row * cols + col;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMenuMove();

        onChanged?.Invoke();
    }

    private void UpdateActionSelectionVisual()
    {
        if (actionButtons == null) return;
        for (int i = 0; i < actionButtons.Length; i++)
        {
            if (actionButtons[i] == null) continue;
            var img = actionButtons[i].GetComponent<Image>();
            var outline = actionButtons[i].GetComponent<Outline>();
            if (i == actionSelectedIndex)
            {
                img.color = actionBaseColors[i] * 1.3f;
                actionButtons[i].transform.localScale = Vector3.one * 1.02f;
                if (outline != null)
                    outline.effectColor = Color.white;
            }
            else
            {
                img.color = actionBaseColors[i];
                actionButtons[i].transform.localScale = Vector3.one;
                if (outline != null)
                    outline.effectColor = Color.Lerp(actionBaseColors[i], Color.black, 0.3f);
            }
        }
    }

    private void UpdateMoveSelectionVisual()
    {
        if (moveBaseColors == null) return;
        for (int i = 0; i < moveButtons.Length; i++)
        {
            if (!moveButtons[i].gameObject.activeSelf) continue;
            var outline = moveButtons[i].GetComponent<Outline>();
            if (outline == null)
                outline = moveButtons[i].gameObject.AddComponent<Outline>();

            if (i == moveSelectedIndex)
            {
                moveButtons[i].transform.localScale = Vector3.one * 1.02f;
                outline.effectColor = Color.white;
                outline.effectDistance = new Vector2(3f, 3f);
            }
            else
            {
                moveButtons[i].transform.localScale = Vector3.one;
                outline.effectColor = Color.clear;
                outline.effectDistance = new Vector2(0f, 0f);
            }
        }
    }

    // ===============================================================
    // Canvas Setup
    // ===============================================================

    private void CreateCanvas()
    {
        var canvasGO = new GameObject("BattleCanvas");
        canvasGO.transform.SetParent(transform);
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();
        canvasRect = canvasGO.GetComponent<RectTransform>();

        // --- EventSystem (REQUIRED for UI clicks to work) ---
        if (FindObjectOfType<EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();
        }
    }

    // ===============================================================
    // Enemy Info Panel (top-left)
    // ===============================================================

    private void CreateEnemyPanel()
    {
        enemyPanel = CreatePanel("EnemyPanel", canvasRect,
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(20f, -20f), new Vector2(360f, 90f));

        // Name
        enemyNameText = CreateText("EnemyName", enemyPanel, "???",
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(15f, -10f), new Vector2(200f, 30f), 22, TextAlignmentOptions.Left);

        // Level
        enemyLevelText = CreateText("EnemyLevel", enemyPanel, "Nv. 0",
            new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-15f, -10f), new Vector2(100f, 30f), 20, TextAlignmentOptions.Right);

        // HP bar background
        var hpBarBg = CreateImage("EnemyHpBarBg", enemyPanel,
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(15f, -48f), new Vector2(330f, 16f),
            new Color(0.15f, 0.15f, 0.2f));

        // HP bar label
        CreateText("HpLabel", hpBarBg.rectTransform, "PV",
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(-22f, 0f), new Vector2(25f, 16f), 12, TextAlignmentOptions.Center);

        // HP bar fill
        var hpBarFillGO = CreateImage("EnemyHpBarFill", hpBarBg.rectTransform,
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(2f, 0f), new Vector2(326f, 12f),
            HP_GREEN);
        enemyHpBarFill = hpBarFillGO;

        // Status
        enemyStatusText = CreateText("EnemyStatus", enemyPanel, "",
            new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(15f, 5f), new Vector2(100f, 22f), 14, TextAlignmentOptions.Left);
    }

    // ===============================================================
    // Player Info Panel (bottom-right)
    // ===============================================================

    private void CreatePlayerPanel()
    {
        playerPanel = CreatePanel("PlayerPanel", canvasRect,
            new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(-20f, 180f), new Vector2(380f, 110f));

        // Name
        playerNameText = CreateText("PlayerName", playerPanel, "???",
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(15f, -10f), new Vector2(200f, 30f), 22, TextAlignmentOptions.Left);

        // Level
        playerLevelText = CreateText("PlayerLevel", playerPanel, "Nv. 0",
            new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-15f, -10f), new Vector2(100f, 30f), 20, TextAlignmentOptions.Right);

        // HP bar background
        var hpBarBg = CreateImage("PlayerHpBarBg", playerPanel,
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(15f, -48f), new Vector2(350f, 16f),
            new Color(0.15f, 0.15f, 0.2f));

        // HP bar label
        CreateText("HpLabel", hpBarBg.rectTransform, "PV",
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(-22f, 0f), new Vector2(25f, 16f), 12, TextAlignmentOptions.Center);

        // HP bar fill
        var hpBarFillGO = CreateImage("PlayerHpBarFill", hpBarBg.rectTransform,
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(2f, 0f), new Vector2(346f, 12f),
            HP_GREEN);
        playerHpBarFill = hpBarFillGO;

        // HP text (numeric)
        playerHpText = CreateText("PlayerHpText", playerPanel, "0 / 0",
            new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-15f, -62f), new Vector2(150f, 22f), 16, TextAlignmentOptions.Right);

        // XP bar background
        var xpBarBg = CreateImage("PlayerXpBarBg", playerPanel,
            new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(15f, 8f), new Vector2(350f, 8f),
            new Color(0.15f, 0.15f, 0.2f));

        // XP bar fill
        var xpBarFillGO = CreateImage("PlayerXpBarFill", xpBarBg.rectTransform,
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(0f, 0f), new Vector2(0f, 6f),
            XP_BLUE);
        playerXpBarFill = xpBarFillGO;

        // Status
        playerStatusText = CreateText("PlayerStatus", playerPanel, "",
            new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(15f, 26f), new Vector2(100f, 22f), 14, TextAlignmentOptions.Left);
    }

    // ===============================================================
    // Action Menu (bottom area — 4 buttons)
    // ===============================================================

    private void CreateActionMenu()
    {
        actionMenuPanel = CreatePanel("ActionMenu", canvasRect,
            new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(-20f, 20f), new Vector2(320f, 150f));

        string[] labels = { "ATTAQUE", "SAC", "DINOS", "FUITE" };
        Color[] btnColors = {
            new Color(0.94f, 0.50f, 0.19f), // orange
            new Color(0.47f, 0.78f, 0.30f), // green
            new Color(0.41f, 0.56f, 0.94f), // blue
            new Color(0.75f, 0.75f, 0.68f), // gray
        };

        actionButtons = new Button[4];
        actionBaseColors = new Color[4];

        for (int i = 0; i < 4; i++)
        {
            int row = i / 2;
            int col = i % 2;
            float x = 10f + col * 155f;
            float y = -10f - row * 70f;

            var btn = CreateButton(labels[i], actionMenuPanel,
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(x, y), new Vector2(145f, 60f),
                btnColors[i], 20);

            int index = i;
            btn.onClick.AddListener(() => OnActionButton(index));

            actionButtons[i] = btn;
            actionBaseColors[i] = btnColors[i];

            switch (i)
            {
                case 0: attackButton = btn; break;
                case 1: bagButton = btn; break;
                case 2: dinoButton = btn; break;
                case 3: runButton = btn; break;
            }
        }
    }

    private void OnActionButton(int index)
    {
        switch (index)
        {
            case 0: // ATTAQUE
                HideActionMenu();
                ShowMoveMenuFromBattle();
                break;
            case 1: // SAC
                OnBagSelected?.Invoke();
                break;
            case 2: // DINOS
                OnDinoSelected?.Invoke();
                break;
            case 3: // FUITE
                OnRunSelected?.Invoke();
                break;
        }
    }

    // ===============================================================
    // Move Menu (4 move buttons + back)
    // ===============================================================

    private void CreateMoveMenu()
    {
        moveMenuPanel = CreatePanel("MoveMenu", canvasRect,
            new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(20f, 20f), new Vector2(700f, 150f));

        for (int i = 0; i < 4; i++)
        {
            int row = i / 2;
            int col = i % 2;
            float x = 10f + col * 340f;
            float y = -10f - row * 65f;

            var btnRect = CreatePanel($"MoveBtn_{i}", moveMenuPanel,
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(x, y), new Vector2(330f, 55f));

            // CreatePanel already adds an Image; reuse it instead of adding a duplicate
            var btnImg = btnRect.gameObject.GetComponent<Image>();
            btnImg.color = Color.gray;
            moveButtonImages[i] = btnImg;

            var btn = btnRect.gameObject.AddComponent<Button>();
            btn.navigation = new Navigation { mode = Navigation.Mode.None };
            btn.targetGraphic = btnImg;
            moveButtons[i] = btn;

            // Move name (with category/power on second line)
            moveNameTexts[i] = CreateText($"MoveName_{i}", btnRect, "---",
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(12f, 0f), new Vector2(220f, 50f), 16, TextAlignmentOptions.Left);
            moveNameTexts[i].richText = true;
            moveNameTexts[i].enableWordWrapping = true;

            // PP text
            movePPTexts[i] = CreateText($"MovePP_{i}", btnRect, "",
                new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-12f, 0f), new Vector2(100f, 22f), 14, TextAlignmentOptions.Right);

            int moveIdx = i;
            btn.onClick.AddListener(() =>
            {
                OnAttackSelected?.Invoke(moveIdx);
            });
        }

        // Back button
        moveBackButton = CreateButton("RETOUR", moveMenuPanel,
            new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(-10f, 10f), new Vector2(120f, 40f),
            new Color(0.5f, 0.5f, 0.5f), 16);
        moveBackButton.onClick.AddListener(() =>
        {
            HideMoveMenu();
            ShowActionMenu();
        });
    }

    public void ShowMoveMenu(MoveSlot[] moves)
    {
        moveMenuPanel.gameObject.SetActive(true);
        moveSelectedIndex = 0;
        moveBaseColors = new Color[4];

        for (int i = 0; i < 4; i++)
        {
            if (i < moves.Length && moves[i] != null)
            {
                var moveSlot = moves[i];
                var moveData = DataLoader.Instance.GetMove(moveSlot.moveId);

                if (moveData != null)
                {
                    // Type color for button background
                    DinoType moveType = (DinoType)moveData.type;
                    Color typeColor = Constants.GetTypeColor(moveType);
                    moveButtonImages[i].color = typeColor;

                    // Category label
                    string catLabel = moveData.category == "physical" ? "PHYSIQUE" :
                                      moveData.category == "special" ? "SPECIAL" : "STATUT";

                    // Power display
                    string powerStr = moveData.power > 0 ? $"Pw:{moveData.power}" : "";

                    // Move name + category
                    moveNameTexts[i].text = $"{moveData.name.ToUpper()}\n<size=12>{catLabel} {powerStr}</size>";

                    // PP remaining
                    movePPTexts[i].text = $"PP {moveSlot.currentPP}/{moveSlot.maxPP}";

                    // Dim if no PP (greyed out and unselectable)
                    if (moveSlot.currentPP <= 0)
                    {
                        moveButtonImages[i].color = typeColor * 0.4f;
                        movePPTexts[i].color = HP_RED;
                        moveNameTexts[i].alpha = 0.5f;
                    }
                    else
                    {
                        movePPTexts[i].color = TEXT_COLOR;
                        moveNameTexts[i].alpha = 1f;
                    }

                    moveButtons[i].interactable = moveSlot.currentPP > 0;
                    moveButtons[i].gameObject.SetActive(true);
                }
                else
                {
                    moveNameTexts[i].text = "???";
                    movePPTexts[i].text = "";
                    moveButtons[i].gameObject.SetActive(true);
                    moveButtons[i].interactable = false;
                }
            }
            else
            {
                moveButtons[i].gameObject.SetActive(false);
            }

            // Store base color for navigation highlight
            moveBaseColors[i] = moveButtonImages[i].color;
        }

        UpdateMoveSelectionVisual();
    }

    private void ShowMoveMenuFromBattle()
    {
        // Get player dino moves from BattleSceneManager context
        var manager = FindObjectOfType<BattleSceneManager>();
        if (manager == null) return;

        var battleSystem = GetBattleSystem();
        if (battleSystem == null) return;

        var playerDino = battleSystem.PlayerDino;
        if (playerDino == null || playerDino.moves == null) return;

        ShowMoveMenu(playerDino.moves.ToArray());
    }

    private BattleSystem GetBattleSystem()
    {
        // Access battle system via reflection or direct reference
        var manager = FindObjectOfType<BattleSceneManager>();
        if (manager == null) return null;

        // Use reflection to get the private battleSystem field
        var field = typeof(BattleSceneManager).GetField("battleSystem",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field?.GetValue(manager) as BattleSystem;
    }

    // ===============================================================
    // Message Box
    // ===============================================================

    private void CreateMessageBox()
    {
        messagePanel = CreatePanel("MessageBox", canvasRect,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 20f), new Vector2(900f, 80f));

        // CreatePanel already adds an Image and Outline; update their properties
        var msgBg = messagePanel.gameObject.GetComponent<Image>();
        msgBg.color = new Color(PANEL_BG.r, PANEL_BG.g, PANEL_BG.b, 0.95f);

        var outline = messagePanel.gameObject.GetComponent<Outline>();
        outline.effectColor = PANEL_BORDER;
        outline.effectDistance = new Vector2(2f, 2f);

        messageText = CreateText("MessageText", messagePanel, "",
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
            new Vector2(20f, 0f), new Vector2(860f, 60f), 22, TextAlignmentOptions.Left);
    }

    public void ShowMessage(string text, Action onDone = null)
    {
        messagePanel.gameObject.SetActive(true);

        // Stop any running typewriter
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        typewriterCoroutine = StartCoroutine(TypewriterEffect(text, onDone));
    }

    private IEnumerator TypewriterEffect(string fullText, Action onDone)
    {
        IsTypingMessage = true;
        messageText.text = "";

        float charDelay = Constants.DialogueSpeed;
        bool fastForward = false;

        for (int i = 0; i < fullText.Length; i++)
        {
            messageText.text += fullText[i];

            // Allow fast-forward on any input
            if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
                fastForward = true;

            if (fastForward)
                charDelay = Constants.DialogueFastSpeed;

            yield return new WaitForSeconds(charDelay);
        }

        messageText.text = fullText;
        IsTypingMessage = false;

        // Wait for player input to dismiss
        yield return new WaitForSeconds(0.2f);
        yield return new WaitUntil(() => Input.anyKeyDown || Input.GetMouseButtonDown(0));

        onDone?.Invoke();
    }

    public void HideMessage()
    {
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);
        messagePanel.gameObject.SetActive(false);
    }

    // ===============================================================
    // Public API — Info Updates
    // ===============================================================

    public void SetPlayerInfo(string name, int level, int currentHp, int maxHp)
    {
        playerNameText.text = name;
        playerLevelText.text = $"Nv. {level}";
        playerHpText.text = $"{currentHp} / {maxHp}";
        UpdateHPBarImmediate(playerHpBarFill, currentHp, maxHp);
    }

    public void SetEnemyInfo(string name, int level, int currentHp, int maxHp)
    {
        enemyNameText.text = name;
        enemyLevelText.text = $"Nv. {level}";
        UpdateHPBarImmediate(enemyHpBarFill, currentHp, maxHp);
    }

    public void UpdateHP(bool isPlayer, int current, int max, bool animate = true)
    {
        Image hpBar = isPlayer ? playerHpBarFill : enemyHpBarFill;

        if (isPlayer)
            playerHpText.text = $"{current} / {max}";

        if (animate)
        {
            if (isPlayer)
            {
                if (playerHpAnim != null) StopCoroutine(playerHpAnim);
                playerHpAnim = StartCoroutine(AnimateHPBar(hpBar, current, max, isPlayer));
            }
            else
            {
                if (enemyHpAnim != null) StopCoroutine(enemyHpAnim);
                enemyHpAnim = StartCoroutine(AnimateHPBar(hpBar, current, max, isPlayer));
            }
        }
        else
        {
            UpdateHPBarImmediate(hpBar, current, max);
        }
    }

    private void UpdateHPBarImmediate(Image hpBar, int current, int max)
    {
        float ratio = max > 0 ? (float)current / max : 0f;
        hpBar.rectTransform.localScale = new Vector3(Mathf.Clamp01(ratio), 1f, 1f);
        hpBar.color = GetHPColor(ratio);
    }

    private IEnumerator AnimateHPBar(Image hpBar, int targetHp, int maxHp, bool isPlayer)
    {
        float targetRatio = maxHp > 0 ? (float)targetHp / maxHp : 0f;
        float currentRatio = hpBar.rectTransform.localScale.x;
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float ratio = Mathf.Lerp(currentRatio, targetRatio, t);
            hpBar.rectTransform.localScale = new Vector3(Mathf.Clamp01(ratio), 1f, 1f);
            hpBar.color = GetHPColor(ratio);

            // Update numeric text during animation (player only)
            if (isPlayer)
            {
                int displayHp = Mathf.RoundToInt(Mathf.Lerp(currentRatio * maxHp, targetHp, t));
                playerHpText.text = $"{displayHp} / {maxHp}";
            }

            yield return null;
        }

        hpBar.rectTransform.localScale = new Vector3(Mathf.Clamp01(targetRatio), 1f, 1f);
        hpBar.color = GetHPColor(targetRatio);

        if (isPlayer)
            playerHpText.text = $"{targetHp} / {maxHp}";
    }

    public void UpdateXP(float xpPercent)
    {
        float width = 350f * Mathf.Clamp01(xpPercent);
        playerXpBarFill.rectTransform.sizeDelta = new Vector2(width, 6f);
    }

    // --- XP animation ---
    private Coroutine xpAnim;

    /// <summary>
    /// Animate the XP bar from current fill to target percent.
    /// If levelUp is true, fills to 100% first, pauses, then resets to newPercent.
    /// Returns immediately; use the onDone callback to know when finished.
    /// </summary>
    public void AnimateXP(float fromPercent, float toPercent, bool levelUp, Action onDone = null)
    {
        if (xpAnim != null) StopCoroutine(xpAnim);
        xpAnim = StartCoroutine(AnimateXPBar(fromPercent, toPercent, levelUp, onDone));
    }

    private IEnumerator AnimateXPBar(float fromPercent, float toPercent, bool levelUp, Action onDone)
    {
        float maxWidth = 350f;

        if (levelUp)
        {
            // Phase 1: fill from current to 100%
            float duration1 = 0.5f * (1f - fromPercent);
            if (duration1 < 0.15f) duration1 = 0.15f;
            float elapsed = 0f;

            while (elapsed < duration1)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration1;
                float pct = Mathf.Lerp(fromPercent, 1f, t);
                playerXpBarFill.rectTransform.sizeDelta = new Vector2(maxWidth * pct, 6f);
                yield return null;
            }
            playerXpBarFill.rectTransform.sizeDelta = new Vector2(maxWidth, 6f);

            // Brief pause at full
            yield return new WaitForSeconds(0.25f);

            // Reset to 0
            playerXpBarFill.rectTransform.sizeDelta = new Vector2(0f, 6f);
            yield return new WaitForSeconds(0.1f);

            // Phase 2: fill from 0 to new percent
            float duration2 = 0.4f * toPercent;
            if (duration2 < 0.1f && toPercent > 0f) duration2 = 0.1f;
            elapsed = 0f;

            while (elapsed < duration2)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration2;
                float pct = Mathf.Lerp(0f, toPercent, t);
                playerXpBarFill.rectTransform.sizeDelta = new Vector2(maxWidth * pct, 6f);
                yield return null;
            }
            playerXpBarFill.rectTransform.sizeDelta = new Vector2(maxWidth * Mathf.Clamp01(toPercent), 6f);
        }
        else
        {
            // Simple fill animation
            float duration = 0.6f * Mathf.Abs(toPercent - fromPercent);
            if (duration < 0.2f) duration = 0.2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float pct = Mathf.Lerp(fromPercent, toPercent, t);
                playerXpBarFill.rectTransform.sizeDelta = new Vector2(maxWidth * Mathf.Clamp01(pct), 6f);
                yield return null;
            }
            playerXpBarFill.rectTransform.sizeDelta = new Vector2(maxWidth * Mathf.Clamp01(toPercent), 6f);
        }

        onDone?.Invoke();
    }

    /// <summary>
    /// Update the player panel level text (used during level-up animation).
    /// </summary>
    public void UpdatePlayerLevel(int level)
    {
        playerLevelText.text = $"Nv. {level}";
    }

    /// <summary>
    /// Update the player panel name (used after evolution).
    /// </summary>
    public void UpdatePlayerName(string name)
    {
        playerNameText.text = name;
    }

    // ===============================================================
    // Status Display
    // ===============================================================

    public void SetStatus(bool isPlayer, StatusEffect status)
    {
        TextMeshProUGUI statusText = isPlayer ? playerStatusText : enemyStatusText;

        if (status == StatusEffect.None)
        {
            statusText.text = "";
            return;
        }

        string label;
        Color statusColor;

        switch (status)
        {
            case StatusEffect.Poison:
                label = "PSN";
                statusColor = new Color(0.63f, 0.25f, 0.63f);
                break;
            case StatusEffect.Burn:
                label = "BRL";
                statusColor = new Color(0.94f, 0.50f, 0.19f);
                break;
            case StatusEffect.Paralysis:
                label = "PAR";
                statusColor = new Color(0.97f, 0.82f, 0.17f);
                break;
            case StatusEffect.Sleep:
                label = "SOM";
                statusColor = new Color(0.6f, 0.6f, 0.7f);
                break;
            case StatusEffect.Freeze:
                label = "GEL";
                statusColor = new Color(0.59f, 0.85f, 0.85f);
                break;
            default:
                label = "";
                statusColor = Color.white;
                break;
        }

        statusText.text = label;
        statusText.color = statusColor;
    }

    // ===============================================================
    // Menu Visibility
    // ===============================================================

    public void ShowActionMenu()
    {
        HideMessage();
        HideMoveMenu();
        actionMenuPanel.gameObject.SetActive(true);
        actionSelectedIndex = 0;
        UpdateActionSelectionVisual();
    }

    public void HideActionMenu()
    {
        actionMenuPanel.gameObject.SetActive(false);
    }

    public void HideMoveMenu()
    {
        moveMenuPanel.gameObject.SetActive(false);
    }

    // ===============================================================
    // Damage Numbers
    // ===============================================================

    public void ShowDamageNumber(Vector3 worldPos, int damage, float effectiveness)
    {
        StartCoroutine(DamageNumberCoroutine(worldPos, damage, effectiveness));
    }

    private IEnumerator DamageNumberCoroutine(Vector3 worldPos, int damage, float effectiveness)
    {
        // Create a temporary world-space canvas for the damage number
        var dmgGO = new GameObject("DamageNumber");
        var dmgCanvas = dmgGO.AddComponent<Canvas>();
        dmgCanvas.renderMode = RenderMode.WorldSpace;
        dmgCanvas.sortingOrder = 200;

        var dmgRect = dmgGO.GetComponent<RectTransform>();
        dmgRect.position = worldPos;
        dmgRect.sizeDelta = new Vector2(2f, 0.5f);
        dmgRect.localScale = Vector3.one * 0.02f;

        // Always face camera
        if (Camera.main != null)
            dmgGO.transform.rotation = Camera.main.transform.rotation;

        // Text
        var textGO = new GameObject("Text");
        textGO.transform.SetParent(dmgGO.transform);
        var text = textGO.AddComponent<TextMeshProUGUI>();
        text.text = damage.ToString();
        text.fontSize = 36;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;

        var textRect = textGO.GetComponent<RectTransform>();
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(200f, 60f);

        // Color based on effectiveness
        if (effectiveness > 1f)
            text.color = new Color(1f, 0.3f, 0.1f); // red-orange for super effective
        else if (effectiveness < 1f)
            text.color = new Color(0.6f, 0.6f, 0.7f); // gray for not very effective
        else
            text.color = Color.white;

        activeDamageNumbers.Add(dmgGO);

        // Animate: float up and fade
        float duration = 1.2f;
        float elapsed = 0f;
        Vector3 startPos = worldPos;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            dmgRect.position = startPos + Vector3.up * t * 1.5f;

            // Fade out in the second half
            if (t > 0.5f)
            {
                float alpha = 1f - (t - 0.5f) * 2f;
                text.alpha = alpha;
            }

            // Face camera
            if (Camera.main != null)
                dmgGO.transform.rotation = Camera.main.transform.rotation;

            yield return null;
        }

        activeDamageNumbers.Remove(dmgGO);
        Destroy(dmgGO);
    }

    // ===============================================================
    // UI Helper Methods
    // ===============================================================

    private static Color GetHPColor(float ratio)
    {
        if (ratio > 0.5f) return HP_GREEN;
        if (ratio > 0.2f) return HP_YELLOW;
        return HP_RED;
    }

    private RectTransform CreatePanel(string name, RectTransform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = anchorMin;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;

        // Background
        var img = go.AddComponent<Image>();
        img.color = new Color(PANEL_BG.r, PANEL_BG.g, PANEL_BG.b, 0.9f);

        // Border
        var outline = go.AddComponent<Outline>();
        outline.effectColor = PANEL_BORDER;
        outline.effectDistance = new Vector2(2f, 2f);

        return rect;
    }

    private TextMeshProUGUI CreateText(string name, RectTransform parent, string content,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta,
        float fontSize, TextAlignmentOptions alignment)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
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

    private Image CreateImage(string name, RectTransform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta,
        Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = anchorMin;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;

        var img = go.AddComponent<Image>();
        img.color = color;

        return img;
    }

    private Button CreateButton(string label, RectTransform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta,
        Color bgColor, float fontSize)
    {
        var go = new GameObject($"Btn_{label}");
        go.transform.SetParent(parent);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = anchorMin;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;

        var img = go.AddComponent<Image>();
        img.color = bgColor;

        var btn = go.AddComponent<Button>();
            btn.navigation = new Navigation { mode = Navigation.Mode.None };
        btn.targetGraphic = img;

        // Button colors
        var colors = btn.colors;
        colors.normalColor = bgColor;
        colors.highlightedColor = Color.Lerp(bgColor, Color.white, 0.2f);
        colors.pressedColor = bgColor * 0.8f;
        colors.selectedColor = Color.Lerp(bgColor, Color.white, 0.1f);
        btn.colors = colors;

        // Outline
        var outline = go.AddComponent<Outline>();
        outline.effectColor = Color.Lerp(bgColor, Color.black, 0.3f);
        outline.effectDistance = new Vector2(2f, 2f);

        // Label text
        var textGO = new GameObject("Label");
        textGO.transform.SetParent(go.transform);
        var textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = TEXT_COLOR;
        tmp.fontStyle = FontStyles.Bold;

        return btn;
    }

    // ===============================================================
    // Cleanup
    // ===============================================================

    void OnDestroy()
    {
        foreach (var dmg in activeDamageNumbers)
        {
            if (dmg != null) Destroy(dmg);
        }
        activeDamageNumbers.Clear();
    }
}
