// ============================================================
// Dino Monsters -- Party UI
// Shows up to 6 party dinos in a vertical list with detail
// panel for the selected dino. Programmatic UI.
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PartyUI : MonoBehaviour
{
    // --- Callback ---
    public Action OnBack;

    // --- State ---
    private List<Dino> party;
    private int selectedIndex = 0;

    // --- UI Root ---
    private RectTransform rootRect;
    private bool isBuilt = false;

    // --- Panels ---
    private RectTransform listPanel;
    private RectTransform detailPanel;

    // --- Slot references ---
    private PartySlotUI[] slots = new PartySlotUI[6];

    // --- Detail panel components ---
    private TextMeshProUGUI detailName;
    private TextMeshProUGUI detailSpecies;
    private TextMeshProUGUI detailLevel;
    private TextMeshProUGUI detailType;
    private TextMeshProUGUI detailHpText;
    private Image detailHpBarFill;
    private TextMeshProUGUI detailXpText;
    private Image detailXpBarFill;
    private TextMeshProUGUI detailStatAtk;
    private TextMeshProUGUI detailStatDef;
    private TextMeshProUGUI detailStatSpAtk;
    private TextMeshProUGUI detailStatSpDef;
    private TextMeshProUGUI detailStatSpeed;
    private TextMeshProUGUI detailTemperament;
    private TextMeshProUGUI detailStatus;
    private TextMeshProUGUI[] detailMoveTexts = new TextMeshProUGUI[4];

    // --- Colors ---
    private static readonly Color PANEL_BG = Constants.ColorUiBg;
    private static readonly Color PANEL_BORDER = Constants.ColorUiBorder;
    private static readonly Color TEXT_COLOR = Constants.ColorTextPrimary;
    private static readonly Color HP_GREEN = Constants.ColorHpGreen;
    private static readonly Color HP_YELLOW = Constants.ColorHpYellow;
    private static readonly Color HP_RED = Constants.ColorHpRed;
    private static readonly Color XP_BLUE = Constants.ColorXpBlue;

    // ===============================================================
    // Show / Hide
    // ===============================================================

    public void Show(List<Dino> partyDinos)
    {
        party = partyDinos;
        selectedIndex = 0;

        if (!isBuilt) BuildUI();

        gameObject.SetActive(true);
        RefreshList();
        RefreshDetail();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    // ===============================================================
    // Build UI
    // ===============================================================

    private void BuildUI()
    {
        isBuilt = true;

        rootRect = gameObject.GetComponent<RectTransform>();
        if (rootRect == null) rootRect = gameObject.AddComponent<RectTransform>();
        StretchFill(rootRect);

        // --- Left panel: party list ---
        listPanel = CreatePanel("PartyListPanel", rootRect,
            new Vector2(0f, 0f), new Vector2(0f, 1f),
            new Vector2(30f, 30f), new Vector2(340f, -60f));

        // List title
        CreateText("PartyTitle", listPanel, "EQUIPE",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -8f), new Vector2(300f, 30f), 22, TextAlignmentOptions.Center)
            .fontStyle = FontStyles.Bold;

        // Create 6 slots
        for (int i = 0; i < 6; i++)
        {
            float yPos = -42f - i * 90f;
            slots[i] = CreateSlot(i, listPanel, yPos);
        }

        // Back button
        var backBtn = CreateButton("RETOUR", listPanel,
            new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(10f, 10f), new Vector2(120f, 36f),
            new Color(0.5f, 0.5f, 0.5f));
        backBtn.onClick.AddListener(() => OnBack?.Invoke());

        // --- Right panel: detail ---
        detailPanel = CreatePanel("DetailPanel", rootRect,
            new Vector2(0f, 0f), new Vector2(0f, 1f),
            new Vector2(390f, 30f), new Vector2(540f, -60f));

        BuildDetailPanel();
    }

    // ===============================================================
    // Slot Creation
    // ===============================================================

    private PartySlotUI CreateSlot(int index, RectTransform parent, float yPos)
    {
        var slotGO = new GameObject($"Slot_{index}");
        slotGO.transform.SetParent(parent, false);
        var slotRect = slotGO.AddComponent<RectTransform>();
        slotRect.anchorMin = new Vector2(0f, 1f);
        slotRect.anchorMax = new Vector2(1f, 1f);
        slotRect.pivot = new Vector2(0.5f, 1f);
        slotRect.anchoredPosition = new Vector2(0f, yPos);
        slotRect.sizeDelta = new Vector2(-20f, 84f);

        var slotImg = slotGO.AddComponent<Image>();
        slotImg.color = new Color(PANEL_BG.r + 0.05f, PANEL_BG.g + 0.05f, PANEL_BG.b + 0.08f, 0.9f);

        var btn = slotGO.AddComponent<Button>();
            btn.navigation = new Navigation { mode = Navigation.Mode.None };
        btn.targetGraphic = slotImg;
        int idx = index;
        btn.onClick.AddListener(() => OnSlotClicked(idx));

        var slot = new PartySlotUI();
        slot.root = slotGO;
        slot.background = slotImg;

        // Name
        slot.nameText = CreateText($"SlotName_{index}", slotRect, "",
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(12f, -6f), new Vector2(180f, 26f), 18, TextAlignmentOptions.Left);
        slot.nameText.fontStyle = FontStyles.Bold;

        // Level
        slot.levelText = CreateText($"SlotLevel_{index}", slotRect, "",
            new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-12f, -6f), new Vector2(80f, 26f), 16, TextAlignmentOptions.Right);

        // Type label
        slot.typeText = CreateText($"SlotType_{index}", slotRect, "",
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(12f, -30f), new Vector2(120f, 22f), 14, TextAlignmentOptions.Left);

        // HP bar background
        var hpBg = CreateImage("HpBarBg", slotRect,
            new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(12f, 12f), new Vector2(-24f, 14f),
            new Color(0.15f, 0.15f, 0.2f));

        // HP bar fill
        slot.hpBarFill = CreateImage($"HpBarFill_{index}", hpBg.rectTransform,
            new Vector2(0f, 0f), new Vector2(0f, 1f),
            new Vector2(2f, 2f), new Vector2(0f, -4f),
            HP_GREEN);

        // HP text
        slot.hpText = CreateText($"SlotHp_{index}", slotRect, "",
            new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(-12f, 26f), new Vector2(120f, 20f), 13, TextAlignmentOptions.Right);

        // Status text
        slot.statusText = CreateText($"SlotStatus_{index}", slotRect, "",
            new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-12f, -30f), new Vector2(60f, 22f), 14, TextAlignmentOptions.Right);

        return slot;
    }

    // ===============================================================
    // Detail Panel
    // ===============================================================

    private void BuildDetailPanel()
    {
        // Name
        detailName = CreateText("DetailName", detailPanel, "",
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(15f, -12f), new Vector2(300f, 32f), 26, TextAlignmentOptions.Left);
        detailName.fontStyle = FontStyles.Bold;

        // Species
        detailSpecies = CreateText("DetailSpecies", detailPanel, "",
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(15f, -42f), new Vector2(200f, 24f), 16, TextAlignmentOptions.Left);
        detailSpecies.fontStyle = FontStyles.Italic;

        // Level
        detailLevel = CreateText("DetailLevel", detailPanel, "",
            new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-15f, -12f), new Vector2(120f, 32f), 22, TextAlignmentOptions.Right);

        // Type
        detailType = CreateText("DetailType", detailPanel, "",
            new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-15f, -42f), new Vector2(200f, 24f), 16, TextAlignmentOptions.Right);

        // HP bar
        var hpLabel = CreateText("HpLabel", detailPanel, "PV",
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(15f, -72f), new Vector2(30f, 20f), 14, TextAlignmentOptions.Left);

        var hpBg = CreateImage("DetailHpBg", detailPanel,
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(50f, -72f), new Vector2(-70f, 18f),
            new Color(0.15f, 0.15f, 0.2f));

        detailHpBarFill = CreateImage("DetailHpFill", hpBg.rectTransform,
            new Vector2(0f, 0f), new Vector2(0f, 1f),
            new Vector2(2f, 2f), new Vector2(0f, -4f),
            HP_GREEN);

        detailHpText = CreateText("DetailHpText", detailPanel, "",
            new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-15f, -70f), new Vector2(120f, 22f), 15, TextAlignmentOptions.Right);

        // XP bar
        var xpLabel = CreateText("XpLabel", detailPanel, "EXP",
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(15f, -96f), new Vector2(35f, 16f), 12, TextAlignmentOptions.Left);

        var xpBg = CreateImage("DetailXpBg", detailPanel,
            new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(55f, -96f), new Vector2(-70f, 10f),
            new Color(0.15f, 0.15f, 0.2f));

        detailXpBarFill = CreateImage("DetailXpFill", xpBg.rectTransform,
            new Vector2(0f, 0f), new Vector2(0f, 1f),
            new Vector2(1f, 1f), new Vector2(0f, -2f),
            XP_BLUE);

        detailXpText = CreateText("DetailXpText", detailPanel, "",
            new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-15f, -94f), new Vector2(120f, 16f), 12, TextAlignmentOptions.Right);

        // --- Stats section ---
        float statsY = -125f;

        CreateText("StatsHeader", detailPanel, "STATISTIQUES",
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(15f, statsY), new Vector2(200f, 24f), 16, TextAlignmentOptions.Left)
            .fontStyle = FontStyles.Bold;

        statsY -= 28f;
        detailStatAtk = CreateStatLine("Attaque", detailPanel, statsY); statsY -= 24f;
        detailStatDef = CreateStatLine("Defense", detailPanel, statsY); statsY -= 24f;
        detailStatSpAtk = CreateStatLine("Att. Spe.", detailPanel, statsY); statsY -= 24f;
        detailStatSpDef = CreateStatLine("Def. Spe.", detailPanel, statsY); statsY -= 24f;
        detailStatSpeed = CreateStatLine("Vitesse", detailPanel, statsY); statsY -= 24f;

        // Temperament
        statsY -= 8f;
        detailTemperament = CreateText("DetailTemp", detailPanel, "",
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(15f, statsY), new Vector2(300f, 22f), 14, TextAlignmentOptions.Left);
        detailTemperament.fontStyle = FontStyles.Italic;
        statsY -= 28f;

        // Status
        detailStatus = CreateText("DetailStatus", detailPanel, "",
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(15f, statsY), new Vector2(200f, 22f), 15, TextAlignmentOptions.Left);
        statsY -= 32f;

        // --- Moves section ---
        CreateText("MovesHeader", detailPanel, "CAPACITES",
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(15f, statsY), new Vector2(200f, 24f), 16, TextAlignmentOptions.Left)
            .fontStyle = FontStyles.Bold;

        statsY -= 28f;
        for (int i = 0; i < 4; i++)
        {
            detailMoveTexts[i] = CreateText($"DetailMove_{i}", detailPanel, "",
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(15f, statsY), new Vector2(500f, 22f), 14, TextAlignmentOptions.Left);
            statsY -= 24f;
        }
    }

    private TextMeshProUGUI CreateStatLine(string label, RectTransform parent, float yPos)
    {
        CreateText($"StatLabel_{label}", parent, label,
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(25f, yPos), new Vector2(120f, 22f), 15, TextAlignmentOptions.Left);

        var valueText = CreateText($"StatValue_{label}", parent, "0",
            new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(155f, yPos), new Vector2(80f, 22f), 15, TextAlignmentOptions.Right);
        valueText.fontStyle = FontStyles.Bold;

        return valueText;
    }

    // ===============================================================
    // Refresh
    // ===============================================================

    private void RefreshList()
    {
        for (int i = 0; i < 6; i++)
        {
            if (i < party.Count)
            {
                var dino = party[i];
                slots[i].root.SetActive(true);

                slots[i].nameText.text = dino.nickname;
                slots[i].levelText.text = $"Nv. {dino.level}";

                // Type
                string typeStr = Constants.GetTypeName(dino.Type1);
                Color typeColor = Constants.GetTypeColor(dino.Type1);
                if (dino.Type2.HasValue)
                    typeStr += $" / {Constants.GetTypeName(dino.Type2.Value)}";
                slots[i].typeText.text = typeStr;
                slots[i].typeText.color = typeColor;

                // HP bar
                float hpRatio = dino.maxHp > 0 ? (float)dino.currentHp / dino.maxHp : 0f;
                float barWidth = (slots[i].hpBarFill.rectTransform.parent as RectTransform).rect.width - 4f;
                slots[i].hpBarFill.rectTransform.sizeDelta = new Vector2(
                    Mathf.Max(0f, barWidth * Mathf.Clamp01(hpRatio)), 0f);
                slots[i].hpBarFill.color = GetHPColor(hpRatio);
                slots[i].hpText.text = $"{dino.currentHp}/{dino.maxHp}";

                // Status
                slots[i].statusText.text = GetStatusLabel(dino.status);
                slots[i].statusText.color = GetStatusColor(dino.status);

                // Selection highlight
                bool selected = (i == selectedIndex);
                slots[i].background.color = selected
                    ? new Color(PANEL_BORDER.r, PANEL_BORDER.g, PANEL_BORDER.b, 0.3f)
                    : new Color(PANEL_BG.r + 0.05f, PANEL_BG.g + 0.05f, PANEL_BG.b + 0.08f, 0.9f);

                // Fainted dino dimming
                if (dino.IsFainted())
                {
                    slots[i].nameText.color = new Color(TEXT_COLOR.r, TEXT_COLOR.g, TEXT_COLOR.b, 0.5f);
                }
                else
                {
                    slots[i].nameText.color = TEXT_COLOR;
                }
            }
            else
            {
                slots[i].root.SetActive(false);
            }
        }
    }

    private void RefreshDetail()
    {
        if (party == null || selectedIndex >= party.Count)
        {
            detailPanel.gameObject.SetActive(false);
            return;
        }

        detailPanel.gameObject.SetActive(true);
        var dino = party[selectedIndex];

        detailName.text = dino.nickname;
        detailSpecies.text = dino.SpeciesName;
        detailLevel.text = $"Nv. {dino.level}";

        // Type
        string typeStr = Constants.GetTypeName(dino.Type1);
        if (dino.Type2.HasValue)
            typeStr += $" / {Constants.GetTypeName(dino.Type2.Value)}";
        detailType.text = typeStr;
        detailType.color = Constants.GetTypeColor(dino.Type1);

        // HP
        float hpRatio = dino.maxHp > 0 ? (float)dino.currentHp / dino.maxHp : 0f;
        float hpBarWidth = (detailHpBarFill.rectTransform.parent as RectTransform).rect.width - 4f;
        detailHpBarFill.rectTransform.sizeDelta = new Vector2(
            Mathf.Max(0f, hpBarWidth * Mathf.Clamp01(hpRatio)), 0f);
        detailHpBarFill.color = GetHPColor(hpRatio);
        detailHpText.text = $"{dino.currentHp} / {dino.maxHp}";

        // XP
        float xpRatio = dino.GetXpPercent();
        float xpBarWidth = (detailXpBarFill.rectTransform.parent as RectTransform).rect.width - 2f;
        detailXpBarFill.rectTransform.sizeDelta = new Vector2(
            Mathf.Max(0f, xpBarWidth * Mathf.Clamp01(xpRatio)), 0f);
        int xpToNext = dino.GetXpToNextLevel();
        int xpProgress = dino.GetXpProgress();
        detailXpText.text = xpToNext > 0 ? $"{xpProgress} / {xpToNext}" : "MAX";

        // Stats
        detailStatAtk.text = dino.stats.attack.ToString();
        detailStatDef.text = dino.stats.defense.ToString();
        detailStatSpAtk.text = dino.stats.spAttack.ToString();
        detailStatSpDef.text = dino.stats.spDefense.ToString();
        detailStatSpeed.text = dino.stats.speed.ToString();

        // Color stats affected by temperament
        var temp = dino.TemperamentData;
        ColorStatText(detailStatAtk, "attack", temp);
        ColorStatText(detailStatDef, "defense", temp);
        ColorStatText(detailStatSpAtk, "spAttack", temp);
        ColorStatText(detailStatSpDef, "spDefense", temp);
        ColorStatText(detailStatSpeed, "speed", temp);

        // Temperament
        detailTemperament.text = $"Temperament: {temp.name}";

        // Status
        if (dino.status != StatusEffect.None)
        {
            detailStatus.text = $"Statut: {GetStatusLabel(dino.status)}";
            detailStatus.color = GetStatusColor(dino.status);
        }
        else
        {
            detailStatus.text = "";
        }

        // Moves
        for (int i = 0; i < 4; i++)
        {
            if (i < dino.moves.Count)
            {
                var moveSlot = dino.moves[i];
                var moveData = DataLoader.Instance.GetMove(moveSlot.moveId);
                if (moveData != null)
                {
                    string typeName = Constants.GetTypeName((DinoType)moveData.type);
                    detailMoveTexts[i].text = $"{moveData.name}  [{typeName}]  PP {moveSlot.currentPP}/{moveSlot.maxPP}";

                    Color moveTypeColor = Constants.GetTypeColor((DinoType)moveData.type);
                    detailMoveTexts[i].color = moveTypeColor;
                }
                else
                {
                    detailMoveTexts[i].text = $"Capacite #{moveSlot.moveId}  PP {moveSlot.currentPP}/{moveSlot.maxPP}";
                    detailMoveTexts[i].color = TEXT_COLOR;
                }
            }
            else
            {
                detailMoveTexts[i].text = "---";
                detailMoveTexts[i].color = new Color(TEXT_COLOR.r, TEXT_COLOR.g, TEXT_COLOR.b, 0.3f);
            }
        }
    }

    private void ColorStatText(TextMeshProUGUI text, string statKey, Temperament temp)
    {
        if (temp.plus == statKey)
            text.color = new Color(0.94f, 0.50f, 0.19f); // Orange for boosted
        else if (temp.minus == statKey)
            text.color = new Color(0.41f, 0.56f, 0.94f); // Blue for reduced
        else
            text.color = TEXT_COLOR;
    }

    // ===============================================================
    // Interaction
    // ===============================================================

    private void OnSlotClicked(int index)
    {
        if (index >= party.Count) return;
        selectedIndex = index;
        RefreshList();
        RefreshDetail();
    }

    // ===============================================================
    // Status Helpers
    // ===============================================================

    private static string GetStatusLabel(StatusEffect status)
    {
        switch (status)
        {
            case StatusEffect.Poison:    return "PSN";
            case StatusEffect.Burn:      return "BRL";
            case StatusEffect.Paralysis: return "PAR";
            case StatusEffect.Sleep:     return "SOM";
            case StatusEffect.Freeze:    return "GEL";
            default: return "";
        }
    }

    private static Color GetStatusColor(StatusEffect status)
    {
        switch (status)
        {
            case StatusEffect.Poison:    return new Color(0.63f, 0.25f, 0.63f);
            case StatusEffect.Burn:      return new Color(0.94f, 0.50f, 0.19f);
            case StatusEffect.Paralysis: return new Color(0.97f, 0.82f, 0.17f);
            case StatusEffect.Sleep:     return new Color(0.6f, 0.6f, 0.7f);
            case StatusEffect.Freeze:    return new Color(0.59f, 0.85f, 0.85f);
            default: return Color.white;
        }
    }

    private static Color GetHPColor(float ratio)
    {
        if (ratio > 0.5f) return Constants.ColorHpGreen;
        if (ratio > 0.2f) return Constants.ColorHpYellow;
        return Constants.ColorHpRed;
    }

    // ===============================================================
    // UI Factory Helpers
    // ===============================================================

    private RectTransform CreatePanel(string name, RectTransform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        var img = go.AddComponent<Image>();
        img.color = new Color(PANEL_BG.r, PANEL_BG.g, PANEL_BG.b, 0.95f);

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

    private Image CreateImage(string name, RectTransform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax,
        Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        var img = go.AddComponent<Image>();
        img.color = color;

        return img;
    }

    private Button CreateButton(string label, RectTransform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta,
        Color bgColor)
    {
        var go = new GameObject($"Btn_{label}");
        go.transform.SetParent(parent, false);
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

        var colors = btn.colors;
        colors.normalColor = bgColor;
        colors.highlightedColor = Color.Lerp(bgColor, Color.white, 0.2f);
        colors.pressedColor = bgColor * 0.8f;
        colors.selectedColor = Color.Lerp(bgColor, Color.white, 0.1f);
        btn.colors = colors;

        var outl = go.AddComponent<Outline>();
        outl.effectColor = Color.Lerp(bgColor, Color.black, 0.3f);
        outl.effectDistance = new Vector2(2f, 2f);

        var textGO = new GameObject("Label");
        textGO.transform.SetParent(go.transform, false);
        var textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 16;
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

    // ===============================================================
    // Data class for slot references
    // ===============================================================

    private class PartySlotUI
    {
        public GameObject root;
        public Image background;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI typeText;
        public Image hpBarFill;
        public TextMeshProUGUI hpText;
        public TextMeshProUGUI statusText;
    }
}
