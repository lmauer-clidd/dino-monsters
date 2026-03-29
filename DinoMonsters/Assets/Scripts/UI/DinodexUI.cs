// ============================================================
// Dino Monsters -- Dinodex UI (3D Model Viewer)
// ============================================================
// Full-screen Dinodex accessible from the pause menu.
// Shows a scrollable species list + 3D rotating model preview
// with stats, type info, and description.
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DinodexUI : MonoBehaviour
{
    public Action OnBack;

    // --- State ---
    private int selectedIndex = 0;
    private List<int> speciesIds = new List<int>();
    private GameObject currentModel;
    private Camera modelCamera;
    private RenderTexture modelRT;
    private int modelLayer = 31; // Use layer 31 for dino preview

    // --- UI refs ---
    private RectTransform rootRect;
    private RectTransform listContent;
    private GameObject[] listEntries;
    private RawImage modelPreview;
    private TextMeshProUGUI nameText;
    private TextMeshProUGUI numberText;
    private TextMeshProUGUI typeText;
    private TextMeshProUGUI descText;
    private TextMeshProUGUI statsText;
    private TextMeshProUGUI heightWeightText;
    private TextMeshProUGUI seenCaughtText;
    private GameObject detailPanel;
    private Image selectedHighlight;

    // --- Colors ---
    private static readonly Color PANEL_BG = Constants.ColorUiBg;
    private static readonly Color PANEL_BORDER = Constants.ColorUiBorder;
    private static readonly Color TEXT_COLOR = Constants.ColorTextPrimary;
    private static readonly Color SEEN_COLOR = new Color(0.55f, 0.55f, 0.55f);
    private static readonly Color CAUGHT_COLOR = new Color(0.94f, 0.78f, 0.30f);
    private static readonly Color UNKNOWN_COLOR = new Color(0.30f, 0.30f, 0.30f);
    private static readonly Color SELECTED_BG = new Color(0.75f, 0.55f, 0.30f, 0.4f);

    // --- Model rotation ---
    private float rotationSpeed = 40f;

    void Awake()
    {
        BuildSpeciesList();
        CreateUI();
        gameObject.SetActive(false);
    }

    void Update()
    {
        // Rotate model
        if (currentModel != null)
            currentModel.transform.Rotate(Vector3.up, rotationSpeed * Time.unscaledDeltaTime, Space.World);

        // Navigation
        if (InputHelper.Up) ChangeSelection(-1);
        else if (InputHelper.Down) ChangeSelection(1);
        else if (InputHelper.Left) ChangeSelection(-10);
        else if (InputHelper.Right) ChangeSelection(10);

        if (InputHelper.Cancel)
        {
            Hide();
            OnBack?.Invoke();
        }
    }

    // ===============================================================
    // Public API
    // ===============================================================

    public void Show()
    {
        gameObject.SetActive(true);
        if (modelCamera != null) modelCamera.enabled = true;
        BuildSpeciesList(); // refresh in case new species seen
        selectedIndex = 0;
        RefreshList();
        SelectEntry(0);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        DestroyModel();
        if (modelCamera != null) modelCamera.enabled = false;
    }

    // ===============================================================
    // Species List
    // ===============================================================

    private void BuildSpeciesList()
    {
        speciesIds.Clear();
        if (DataLoader.Instance == null) return;

        // Collect all species IDs sorted
        var keys = new List<int>(DataLoader.Instance.Dinos.Keys);
        keys.Sort();
        speciesIds = keys;
    }

    // ===============================================================
    // UI Construction
    // ===============================================================

    private void CreateUI()
    {
        rootRect = gameObject.AddComponent<RectTransform>();
        StretchFill(rootRect);

        // Background
        var bg = gameObject.AddComponent<Image>();
        bg.color = new Color(PANEL_BG.r, PANEL_BG.g, PANEL_BG.b, 0.98f);

        // Title bar
        var titleBar = CreatePanel("TitleBar", rootRect,
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1),
            new Vector2(0, -5), new Vector2(0, 50));
        titleBar.color = new Color(0.75f, 0.55f, 0.30f, 0.9f);

        CreateText("Title", titleBar.rectTransform, "DINODEX",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(200, 40), 28, TextAlignmentOptions.Center)
            .fontStyle = FontStyles.Bold;

        // Seen/Caught counter at top right
        seenCaughtText = CreateText("SeenCaught", titleBar.rectTransform, "",
            new Vector2(1, 0.5f), new Vector2(1, 0.5f),
            new Vector2(-15, 0), new Vector2(250, 30), 16, TextAlignmentOptions.Right);

        // ---- LEFT PANEL: Species list ----
        var listPanel = CreatePanel("ListPanel", rootRect,
            new Vector2(0, 0), new Vector2(0.35f, 1), new Vector2(0, 0),
            new Vector2(10, 10), new Vector2(-5, -60));
        listPanel.color = new Color(0, 0, 0, 0.3f);

        // Scroll area
        var scrollArea = new GameObject("ScrollArea");
        scrollArea.transform.SetParent(listPanel.transform, false);
        var scrollRect = scrollArea.AddComponent<RectTransform>();
        StretchFill(scrollRect);
        scrollRect.offsetMin = new Vector2(5, 5);
        scrollRect.offsetMax = new Vector2(-5, -5);
        scrollArea.AddComponent<RectMask2D>();

        // List content
        var contentGO = new GameObject("ListContent");
        contentGO.transform.SetParent(scrollArea.transform, false);
        listContent = contentGO.AddComponent<RectTransform>();
        listContent.anchorMin = new Vector2(0, 1);
        listContent.anchorMax = new Vector2(1, 1);
        listContent.pivot = new Vector2(0.5f, 1);
        listContent.anchoredPosition = Vector2.zero;

        // ---- RIGHT PANEL: Details + 3D preview ----
        detailPanel = new GameObject("DetailPanel");
        detailPanel.transform.SetParent(rootRect, false);
        var detailRect = detailPanel.AddComponent<RectTransform>();
        detailRect.anchorMin = new Vector2(0.35f, 0);
        detailRect.anchorMax = new Vector2(1, 1);
        detailRect.pivot = new Vector2(0, 0);
        detailRect.offsetMin = new Vector2(5, 10);
        detailRect.offsetMax = new Vector2(-10, -60);

        // Model preview area — background + RawImage on separate GameObjects
        var previewBgGO = new GameObject("PreviewBg");
        previewBgGO.transform.SetParent(detailPanel.transform, false);
        var previewBgRect = previewBgGO.AddComponent<RectTransform>();
        previewBgRect.anchorMin = new Vector2(0, 0.40f);
        previewBgRect.anchorMax = new Vector2(1, 1);
        previewBgRect.offsetMin = new Vector2(10, 5);
        previewBgRect.offsetMax = new Vector2(-10, -5);
        var previewBg = previewBgGO.AddComponent<Image>();
        previewBg.color = new Color(0.12f, 0.10f, 0.08f, 0.8f);

        // RawImage on top of background — square aspect ratio preserved
        var previewGO = new GameObject("ModelPreview");
        previewGO.transform.SetParent(previewBgGO.transform, false);
        var previewRect = previewGO.AddComponent<RectTransform>();
        StretchFill(previewRect);
        modelPreview = previewGO.AddComponent<RawImage>();
        modelPreview.color = Color.white;

        // Keep square aspect ratio (RT is 512x512)
        var fitter = previewGO.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fitter.aspectRatio = 1f;

        // Setup render texture + camera for model preview
        SetupModelCamera();

        // Info panel (bottom of detail panel)
        var infoPanel = new GameObject("InfoPanel");
        infoPanel.transform.SetParent(detailPanel.transform, false);
        var infoRect = infoPanel.AddComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0, 0);
        infoRect.anchorMax = new Vector2(1, 0.40f);
        infoRect.offsetMin = new Vector2(10, 10);
        infoRect.offsetMax = new Vector2(-10, -5);

        // Number + Name
        numberText = CreateText("Number", infoRect, "#001",
            new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(0, 0), new Vector2(80, 32), 22, TextAlignmentOptions.Left);
        numberText.fontStyle = FontStyles.Bold;
        numberText.color = CAUGHT_COLOR;

        nameText = CreateText("Name", infoRect, "PYREX",
            new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(80, 0), new Vector2(300, 32), 24, TextAlignmentOptions.Left);
        nameText.fontStyle = FontStyles.Bold;

        // Type
        typeText = CreateText("Type", infoRect, "Feu",
            new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(0, -35), new Vector2(300, 26), 18, TextAlignmentOptions.Left);

        // Height / Weight
        heightWeightText = CreateText("HW", infoRect, "",
            new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(0, -35), new Vector2(200, 26), 16, TextAlignmentOptions.Right);
        heightWeightText.color = new Color(TEXT_COLOR.r, TEXT_COLOR.g, TEXT_COLOR.b, 0.7f);

        // Stats
        statsText = CreateText("Stats", infoRect, "",
            new Vector2(0, 1), new Vector2(1, 1),
            new Vector2(0, -65), new Vector2(0, 60), 14, TextAlignmentOptions.Left);
        statsText.enableWordWrapping = true;

        // Description
        descText = CreateText("Desc", infoRect, "",
            new Vector2(0, 0), new Vector2(1, 0),
            new Vector2(0, 5), new Vector2(0, 50), 14, TextAlignmentOptions.Left);
        descText.fontStyle = FontStyles.Italic;
        descText.enableWordWrapping = true;
        descText.color = new Color(TEXT_COLOR.r, TEXT_COLOR.g, TEXT_COLOR.b, 0.8f);

        // Back hint
        CreateText("BackHint", rootRect, "[Echap] Retour",
            new Vector2(0, 0), new Vector2(0, 0),
            new Vector2(15, 8), new Vector2(200, 25), 14, TextAlignmentOptions.Left)
            .color = new Color(1, 1, 1, 0.4f);
    }

    // ===============================================================
    // Model Camera (renders dino to RenderTexture)
    // ===============================================================

    private void SetupModelCamera()
    {
        modelRT = new RenderTexture(512, 512, 16);
        modelRT.antiAliasing = 4;

        // Camera must NOT be parented to UI — keep it in world space
        var camGO = new GameObject("DinodexModelCamera");
        camGO.transform.position = new Vector3(100, 1.2f, 105);
        camGO.transform.rotation = Quaternion.Euler(10, 180, 0);

        modelCamera = camGO.AddComponent<Camera>();
        modelCamera.targetTexture = modelRT;
        modelCamera.clearFlags = CameraClearFlags.SolidColor;
        modelCamera.backgroundColor = new Color(0.10f, 0.09f, 0.08f, 1f);
        modelCamera.cullingMask = 1 << modelLayer;
        modelCamera.fieldOfView = 40;
        modelCamera.nearClipPlane = 0.1f;
        modelCamera.farClipPlane = 50f;
        modelCamera.depth = -10;

        modelPreview.texture = modelRT;
    }

    // ===============================================================
    // List Population
    // ===============================================================

    private void RefreshList()
    {
        // Clear old entries
        for (int i = listContent.childCount - 1; i >= 0; i--)
            Destroy(listContent.GetChild(i).gameObject);

        float entryH = 32f;
        float spacing = 2f;
        listContent.sizeDelta = new Vector2(0, speciesIds.Count * (entryH + spacing));
        listEntries = new GameObject[speciesIds.Count];

        int seenTotal = 0, caughtTotal = 0;

        for (int i = 0; i < speciesIds.Count; i++)
        {
            int sid = speciesIds[i];
            var species = DataLoader.Instance.GetSpecies(sid);
            var entry = GameState.Instance.GetDinodexEntry(sid);

            bool seen = entry != null && entry.seen;
            bool caught = entry != null && entry.caught;
            if (seen) seenTotal++;
            if (caught) caughtTotal++;

            var rowGO = new GameObject($"Entry_{sid}");
            rowGO.transform.SetParent(listContent, false);
            var rowRect = rowGO.AddComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0, 1);
            rowRect.anchorMax = new Vector2(1, 1);
            rowRect.pivot = new Vector2(0.5f, 1);
            rowRect.anchoredPosition = new Vector2(0, -i * (entryH + spacing));
            rowRect.sizeDelta = new Vector2(0, entryH);

            var rowImg = rowGO.AddComponent<Image>();
            rowImg.color = new Color(0, 0, 0, 0.15f);

            // Number
            string numStr = $"#{sid:D3}";
            var numTxt = CreateText($"Num_{sid}", rowRect, numStr,
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(5, 0), new Vector2(50, 28), 14, TextAlignmentOptions.Left);

            // Status icon
            string icon = caught ? "*" : (seen ? "o" : "-");
            Color iconColor = caught ? CAUGHT_COLOR : (seen ? SEEN_COLOR : UNKNOWN_COLOR);
            var iconTxt = CreateText($"Icon_{sid}", rowRect, icon,
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(52, 0), new Vector2(20, 28), 14, TextAlignmentOptions.Center);
            iconTxt.color = iconColor;

            // Name (show if seen, otherwise "???")
            string displayName = seen ? (species != null ? species.name : "???") : "???";
            Color nameColor = seen ? TEXT_COLOR : UNKNOWN_COLOR;
            var nameTxt = CreateText($"Name_{sid}", rowRect, displayName,
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(72, 0), new Vector2(200, 28), 15, TextAlignmentOptions.Left);
            nameTxt.color = nameColor;

            listEntries[i] = rowGO;
        }

        // Update counter
        seenCaughtText.text = $"Vus: {seenTotal}  Captures: {caughtTotal} / {speciesIds.Count}";
    }

    // ===============================================================
    // Selection
    // ===============================================================

    private void ChangeSelection(int delta)
    {
        int newIndex = selectedIndex + delta;
        newIndex = Mathf.Clamp(newIndex, 0, speciesIds.Count - 1);
        if (newIndex != selectedIndex)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayMenuMove();
            SelectEntry(newIndex);
        }
    }

    private void SelectEntry(int index)
    {
        // Unhighlight old
        if (selectedIndex >= 0 && selectedIndex < listEntries.Length && listEntries[selectedIndex] != null)
        {
            var oldImg = listEntries[selectedIndex].GetComponent<Image>();
            if (oldImg != null) oldImg.color = new Color(0, 0, 0, 0.15f);
        }

        selectedIndex = index;

        // Highlight new
        if (index >= 0 && index < listEntries.Length && listEntries[index] != null)
        {
            var newImg = listEntries[index].GetComponent<Image>();
            if (newImg != null) newImg.color = SELECTED_BG;

            // Scroll to keep selected visible
            ScrollToEntry(index);
        }

        // Update detail panel
        UpdateDetail(speciesIds[index]);
    }

    private void ScrollToEntry(int index)
    {
        float entryH = 32f + 2f;
        float entryY = index * entryH;
        float viewH = listContent.parent.GetComponent<RectTransform>().rect.height;
        float contentH = listContent.sizeDelta.y;

        float scrollY = entryY - viewH * 0.5f + entryH * 0.5f;
        scrollY = Mathf.Clamp(scrollY, 0, Mathf.Max(0, contentH - viewH));
        listContent.anchoredPosition = new Vector2(0, scrollY);
    }

    // ===============================================================
    // Detail Panel Update
    // ===============================================================

    private void UpdateDetail(int speciesId)
    {
        var species = DataLoader.Instance.GetSpecies(speciesId);
        var entry = GameState.Instance.GetDinodexEntry(speciesId);
        bool seen = entry != null && entry.seen;
        bool caught = entry != null && entry.caught;

        numberText.text = $"#{speciesId:D3}";

        if (!seen)
        {
            nameText.text = "???";
            typeText.text = "";
            statsText.text = "";
            descText.text = "Ce dino n'a pas encore ete rencontre...";
            heightWeightText.text = "";
            DestroyModel();
            return;
        }

        // Name
        nameText.text = species != null ? species.name.ToUpper() : "???";

        // Types
        if (species != null)
        {
            string t1 = GetTypeName(species.types[0]);
            string typeStr = t1;
            if (species.types.Length > 1 && species.types[1] != species.types[0])
                typeStr += " / " + GetTypeName(species.types[1]);
            typeText.text = typeStr;
            typeText.color = GetTypeDisplayColor(species.types[0]);
        }

        // Height / Weight
        if (species != null)
            heightWeightText.text = $"{species.height:F1}m  {species.weight:F1}kg";

        // Stats
        if (species != null && species.baseStats != null && caught)
        {
            var bs = species.baseStats;
            statsText.text = $"PV: {bs.hp}  Atk: {bs.atk}  Def: {bs.def}  " +
                             $"AtkSp: {bs.spatk}  DefSp: {bs.spdef}  Vit: {bs.speed}  " +
                             $"(Total: {bs.hp + bs.atk + bs.def + bs.spatk + bs.spdef + bs.speed})";
        }
        else
        {
            statsText.text = caught ? "" : "Attrapez-le pour voir ses stats !";
        }

        // Description
        if (species != null)
            descText.text = species.description ?? "";

        // 3D Model
        SpawnModel(speciesId, seen);
    }

    // ===============================================================
    // 3D Model Spawning
    // ===============================================================

    private void SpawnModel(int speciesId, bool seen)
    {
        DestroyModel();

        if (!seen) return;

        currentModel = DinoModelGenerator.CreateDinoModel(speciesId, false);
        if (currentModel == null) return;

        // Position in front of model camera (camera at z=105 looking -Z, model at z=102)
        currentModel.transform.position = new Vector3(100, 0.5f, 102);
        currentModel.transform.rotation = Quaternion.Euler(0, 0, 0);

        // Set layer for camera culling
        SetLayerRecursive(currentModel, modelLayer);

        // Attach animator for idle
        var animator = currentModel.AddComponent<DinoAnimator>();
        animator.PlayIdle();
    }

    private void DestroyModel()
    {
        if (currentModel != null)
        {
            Destroy(currentModel);
            currentModel = null;
        }
    }

    private void SetLayerRecursive(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
            SetLayerRecursive(child.gameObject, layer);
    }

    // ===============================================================
    // Type Helpers
    // ===============================================================

    private string GetTypeName(int type)
    {
        return (DinoType)type switch
        {
            DinoType.Normal   => "Normal",
            DinoType.Fire     => "Feu",
            DinoType.Water    => "Eau",
            DinoType.Earth    => "Terre",
            DinoType.Air      => "Air",
            DinoType.Electric => "Electrik",
            DinoType.Ice      => "Glace",
            DinoType.Venom    => "Venin",
            DinoType.Flora    => "Flore",
            DinoType.Fossil   => "Fossile",
            DinoType.Shadow   => "Ombre",
            DinoType.Light    => "Lumiere",
            DinoType.Metal    => "Metal",
            DinoType.Primal   => "Primal",
            _                 => "???"
        };
    }

    private Color GetTypeDisplayColor(int type)
    {
        return (DinoType)type switch
        {
            DinoType.Fire     => new Color(0.94f, 0.50f, 0.19f),
            DinoType.Water    => new Color(0.41f, 0.56f, 0.94f),
            DinoType.Flora    => new Color(0.47f, 0.78f, 0.30f),
            DinoType.Electric => new Color(0.97f, 0.82f, 0.17f),
            DinoType.Ice      => new Color(0.59f, 0.85f, 0.85f),
            DinoType.Earth    => new Color(0.88f, 0.75f, 0.40f),
            DinoType.Venom    => new Color(0.63f, 0.25f, 0.63f),
            DinoType.Shadow   => new Color(0.44f, 0.34f, 0.59f),
            DinoType.Light    => new Color(0.97f, 0.97f, 0.47f),
            DinoType.Metal    => new Color(0.72f, 0.72f, 0.82f),
            DinoType.Fossil   => new Color(0.72f, 0.63f, 0.38f),
            DinoType.Air      => new Color(0.66f, 0.56f, 0.94f),
            DinoType.Primal   => new Color(0.91f, 0.75f, 0.31f),
            _                 => TEXT_COLOR
        };
    }

    // ===============================================================
    // UI Helpers
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

    private Image CreatePanel(string name, RectTransform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 offsetMin, Vector2 offsetMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        var img = go.AddComponent<Image>();
        return img;
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
        DestroyModel();
        if (modelCamera != null)
            Destroy(modelCamera.gameObject);
        if (modelRT != null)
        {
            modelRT.Release();
            Destroy(modelRT);
        }
    }
}
