// ============================================================
// Dino Monsters -- World Map UI
// Shows discovered locations as colored dots connected by lines.
// Opened from PauseMenu "CARTE" button.
// ============================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class WorldMapUI : MonoBehaviour
{
    public static WorldMapUI Instance { get; private set; }

    private Canvas canvas;
    private GameObject rootPanel;
    private bool isOpen = false;

    // Callbacks
    public System.Action OnClose;

    // Colors
    private static readonly Color BG_COLOR = new Color(0.08f, 0.08f, 0.15f, 0.95f);
    private static readonly Color TOWN_VISITED = new Color(0.45f, 0.75f, 0.95f);
    private static readonly Color TOWN_UNVISITED = new Color(0.4f, 0.4f, 0.45f);
    private static readonly Color ROUTE_COLOR = new Color(0.6f, 0.6f, 0.5f, 0.6f);
    private static readonly Color PLAYER_COLOR = new Color(1f, 0.85f, 0.2f);
    private static readonly Color TEXT_COLOR = Constants.ColorTextPrimary;
    private static readonly Color BADGE_COLOR = new Color(0.95f, 0.7f, 0.2f);

    // Map layout data
    private struct MapNode
    {
        public string mapId;
        public string displayName;
        public Vector2 position; // normalized position on the map panel
        public bool isTown;
        public int gymIndex; // -1 if no gym, 0-7 for badge index
    }

    private struct MapConnection
    {
        public int fromIndex;
        public int toIndex;
    }

    private MapNode[] nodes;
    private MapConnection[] connections;
    private GameObject playerMarker;
    private float blinkTimer;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Show()
    {
        if (isOpen) return;
        isOpen = true;

        if (rootPanel == null)
            CreateUI();

        rootPanel.SetActive(true);
        UpdateMapState();
    }

    public void Hide()
    {
        if (!isOpen) return;
        isOpen = false;
        if (rootPanel != null)
            rootPanel.SetActive(false);
    }

    public bool IsOpen => isOpen;

    void Update()
    {
        if (!isOpen) return;

        // Blink player marker
        if (playerMarker != null)
        {
            blinkTimer += Time.unscaledDeltaTime * 4f;
            var img = playerMarker.GetComponent<Image>();
            if (img != null)
            {
                float alpha = 0.5f + 0.5f * Mathf.Sin(blinkTimer);
                img.color = new Color(PLAYER_COLOR.r, PLAYER_COLOR.g, PLAYER_COLOR.b, alpha);
            }
        }

        // Close on Cancel/Escape
        if (InputHelper.Cancel || InputHelper.Pause)
        {
            Hide();
            OnClose?.Invoke();
        }
    }

    // ===============================================================
    // UI Creation
    // ===============================================================

    private void CreateUI()
    {
        InitMapData();

        // Canvas
        var canvasGO = new GameObject("WorldMapCanvas");
        canvasGO.transform.SetParent(transform, false);
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 210;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // Root panel (dark background)
        rootPanel = new GameObject("MapRoot");
        rootPanel.transform.SetParent(canvasGO.transform, false);
        var rootRect = rootPanel.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;
        var rootImg = rootPanel.AddComponent<Image>();
        rootImg.color = BG_COLOR;

        // Title
        CreateText("CARTE DU MONDE", rootRect,
            new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(400f, 40f),
            28, TextAlignmentOptions.Center, FontStyles.Bold);

        // Map area
        var mapArea = new GameObject("MapArea");
        mapArea.transform.SetParent(rootPanel.transform, false);
        var mapRect = mapArea.AddComponent<RectTransform>();
        mapRect.anchorMin = new Vector2(0.05f, 0.08f);
        mapRect.anchorMax = new Vector2(0.95f, 0.88f);
        mapRect.offsetMin = Vector2.zero;
        mapRect.offsetMax = Vector2.zero;

        // Draw connections first (behind nodes)
        DrawConnections(mapRect);

        // Draw nodes
        DrawNodes(mapRect);

        // RETOUR button
        var btnGO = new GameObject("Btn_Retour");
        btnGO.transform.SetParent(rootPanel.transform, false);
        var btnRect = btnGO.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0f);
        btnRect.anchorMax = new Vector2(0.5f, 0f);
        btnRect.pivot = new Vector2(0.5f, 0f);
        btnRect.anchoredPosition = new Vector2(0f, 10f);
        btnRect.sizeDelta = new Vector2(160f, 40f);

        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.5f, 0.5f, 0.5f);
        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.onClick.AddListener(() =>
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuMove();
            Hide();
            OnClose?.Invoke();
        });

        var btnOutline = btnGO.AddComponent<Outline>();
        btnOutline.effectColor = Color.Lerp(new Color(0.5f, 0.5f, 0.5f), Color.black, 0.3f);
        btnOutline.effectDistance = new Vector2(2f, 2f);

        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(btnGO.transform, false);
        var labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        var labelTmp = labelGO.AddComponent<TextMeshProUGUI>();
        labelTmp.text = "RETOUR";
        labelTmp.fontSize = 20;
        labelTmp.alignment = TextAlignmentOptions.Center;
        labelTmp.color = TEXT_COLOR;
        labelTmp.fontStyle = FontStyles.Bold;

        rootPanel.SetActive(false);
    }

    // ===============================================================
    // Map Data
    // ===============================================================

    private void InitMapData()
    {
        // Layout positions (normalized 0-1 within map area)
        // Following the game world topology
        nodes = new MapNode[]
        {
            // 0: Bourg-Nid (start town)
            new MapNode { mapId = "BOURG_NID", displayName = "Bourg-Nid", position = new Vector2(0.08f, 0.85f), isTown = true, gymIndex = -1 },
            // 1: Route 1
            new MapNode { mapId = "ROUTE_1", displayName = "Route 1", position = new Vector2(0.20f, 0.85f), isTown = false, gymIndex = -1 },
            // 2: Ville-Fougere (Gym 1)
            new MapNode { mapId = "VILLE_FOUGERE", displayName = "Ville-Fougere", position = new Vector2(0.33f, 0.85f), isTown = true, gymIndex = 0 },
            // 3: Route 2
            new MapNode { mapId = "ROUTE_2", displayName = "Route 2", position = new Vector2(0.33f, 0.70f), isTown = false, gymIndex = -1 },
            // 4: Route 3
            new MapNode { mapId = "ROUTE_3", displayName = "Route 3", position = new Vector2(0.48f, 0.85f), isTown = false, gymIndex = -1 },
            // 5: Port-Coquille (Gym 2)
            new MapNode { mapId = "PORT_COQUILLE", displayName = "Port-Coquille", position = new Vector2(0.62f, 0.85f), isTown = true, gymIndex = 1 },
            // 6: Route 4
            new MapNode { mapId = "ROUTE_4", displayName = "Route 4", position = new Vector2(0.62f, 0.70f), isTown = false, gymIndex = -1 },
            // 7: Roche-Haute (Gym 3)
            new MapNode { mapId = "ROCHE_HAUTE", displayName = "Roche-Haute", position = new Vector2(0.62f, 0.55f), isTown = true, gymIndex = 2 },
            // 8: Route 5
            new MapNode { mapId = "ROUTE_5", displayName = "Route 5", position = new Vector2(0.62f, 0.42f), isTown = false, gymIndex = -1 },
            // 9: Volcanville (Gym 4)
            new MapNode { mapId = "VOLCANVILLE", displayName = "Volcanville", position = new Vector2(0.62f, 0.28f), isTown = true, gymIndex = 3 },
            // 10: Route 6
            new MapNode { mapId = "ROUTE_6", displayName = "Route 6", position = new Vector2(0.62f, 0.15f), isTown = false, gymIndex = -1 },
            // 11: Cryo-Cite (Gym 5)
            new MapNode { mapId = "CRYO_CITE", displayName = "Cryo-Cite", position = new Vector2(0.55f, 0.05f), isTown = true, gymIndex = 4 },
            // 12: Route 7
            new MapNode { mapId = "ROUTE_7", displayName = "Route 7", position = new Vector2(0.72f, 0.05f), isTown = false, gymIndex = -1 },
            // 13: Electropolis (Gym 6)
            new MapNode { mapId = "ELECTROPOLIS", displayName = "Electropolis", position = new Vector2(0.85f, 0.05f), isTown = true, gymIndex = 5 },
            // 14: Route 8
            new MapNode { mapId = "ROUTE_8", displayName = "Route 8", position = new Vector2(0.85f, 0.20f), isTown = false, gymIndex = -1 },
            // 15: Marais-Noir (Gym 7)
            new MapNode { mapId = "MARAIS_NOIR", displayName = "Marais-Noir", position = new Vector2(0.85f, 0.35f), isTown = true, gymIndex = 6 },
            // 16: Route 9
            new MapNode { mapId = "ROUTE_9", displayName = "Route 9", position = new Vector2(0.85f, 0.50f), isTown = false, gymIndex = -1 },
            // 17: Ciel-Haut (Gym 8)
            new MapNode { mapId = "CIEL_HAUT", displayName = "Ciel-Haut", position = new Vector2(0.85f, 0.65f), isTown = true, gymIndex = 7 },
            // 18: Victory Road
            new MapNode { mapId = "VICTORY_ROAD", displayName = "Victory Road", position = new Vector2(0.92f, 0.78f), isTown = false, gymIndex = -1 },
            // 19: Paleo Capital
            new MapNode { mapId = "PALEO_CAPITAL", displayName = "Paleo Capital", position = new Vector2(0.92f, 0.92f), isTown = true, gymIndex = -1 },
        };

        connections = new MapConnection[]
        {
            new MapConnection { fromIndex = 0, toIndex = 1 },   // Bourg-Nid -> Route 1
            new MapConnection { fromIndex = 1, toIndex = 2 },   // Route 1 -> Ville-Fougere
            new MapConnection { fromIndex = 2, toIndex = 3 },   // Ville-Fougere -> Route 2
            new MapConnection { fromIndex = 2, toIndex = 4 },   // Ville-Fougere -> Route 3
            new MapConnection { fromIndex = 4, toIndex = 5 },   // Route 3 -> Port-Coquille
            new MapConnection { fromIndex = 5, toIndex = 6 },   // Port-Coquille -> Route 4
            new MapConnection { fromIndex = 6, toIndex = 7 },   // Route 4 -> Roche-Haute
            new MapConnection { fromIndex = 7, toIndex = 8 },   // Roche-Haute -> Route 5
            new MapConnection { fromIndex = 8, toIndex = 9 },   // Route 5 -> Volcanville
            new MapConnection { fromIndex = 9, toIndex = 10 },  // Volcanville -> Route 6
            new MapConnection { fromIndex = 10, toIndex = 11 }, // Route 6 -> Cryo-Cite
            new MapConnection { fromIndex = 11, toIndex = 12 }, // Cryo-Cite -> Route 7
            new MapConnection { fromIndex = 12, toIndex = 13 }, // Route 7 -> Electropolis
            new MapConnection { fromIndex = 13, toIndex = 14 }, // Electropolis -> Route 8
            new MapConnection { fromIndex = 14, toIndex = 15 }, // Route 8 -> Marais-Noir
            new MapConnection { fromIndex = 15, toIndex = 16 }, // Marais-Noir -> Route 9
            new MapConnection { fromIndex = 16, toIndex = 17 }, // Route 9 -> Ciel-Haut
            new MapConnection { fromIndex = 17, toIndex = 18 }, // Ciel-Haut -> Victory Road
            new MapConnection { fromIndex = 18, toIndex = 19 }, // Victory Road -> Paleo Capital
        };
    }

    // ===============================================================
    // Drawing
    // ===============================================================

    private void DrawConnections(RectTransform parent)
    {
        foreach (var conn in connections)
        {
            var from = nodes[conn.fromIndex];
            var to = nodes[conn.toIndex];

            var lineGO = new GameObject($"Line_{conn.fromIndex}_{conn.toIndex}");
            lineGO.transform.SetParent(parent, false);
            var lineRect = lineGO.AddComponent<RectTransform>();
            var lineImg = lineGO.AddComponent<Image>();
            lineImg.color = ROUTE_COLOR;

            // Calculate line between two anchored positions
            lineRect.anchorMin = new Vector2(0f, 0f);
            lineRect.anchorMax = new Vector2(0f, 0f);
            lineRect.pivot = new Vector2(0f, 0.5f);

            // We need to position using anchors relative to parent
            // Convert normalized positions to pixel offsets relative to parent
            // We'll do this by anchoring to bottom-left and computing from parent size
            // Use a trick: anchor to (0,0) and set position based on parent
            lineRect.anchorMin = from.position;
            lineRect.anchorMax = from.position;
            lineRect.pivot = new Vector2(0f, 0.5f);

            Vector2 dir = to.position - from.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // Length in parent-relative space -- approximate as pixels
            // Parent rect will scale, so we express length in anchor units * reference size
            float length = dir.magnitude;

            lineRect.anchoredPosition = Vector2.zero;
            lineRect.sizeDelta = new Vector2(length * 1100f, 2f); // approximate pixel width of map area
            lineRect.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void DrawNodes(RectTransform parent)
    {
        string currentMap = GameState.Instance != null ? GameState.Instance.CurrentMapId : "";

        for (int i = 0; i < nodes.Length; i++)
        {
            var node = nodes[i];

            // Node dot
            float dotSize = node.isTown ? 18f : 10f;
            var dotGO = new GameObject($"Node_{node.mapId}");
            dotGO.transform.SetParent(parent, false);
            var dotRect = dotGO.AddComponent<RectTransform>();
            dotRect.anchorMin = node.position;
            dotRect.anchorMax = node.position;
            dotRect.pivot = new Vector2(0.5f, 0.5f);
            dotRect.anchoredPosition = Vector2.zero;
            dotRect.sizeDelta = new Vector2(dotSize, dotSize);

            var dotImg = dotGO.AddComponent<Image>();

            // Determine if visited (simple heuristic: current or earlier in sequence)
            bool visited = IsMapVisited(node.mapId, currentMap);
            dotImg.color = visited ? TOWN_VISITED : TOWN_UNVISITED;

            // Label for towns
            if (node.isTown)
            {
                var labelGO = new GameObject($"Label_{node.mapId}");
                labelGO.transform.SetParent(dotGO.transform, false);
                var labelRect = labelGO.AddComponent<RectTransform>();
                labelRect.anchorMin = new Vector2(0.5f, 0f);
                labelRect.anchorMax = new Vector2(0.5f, 0f);
                labelRect.pivot = new Vector2(0.5f, 1f);
                labelRect.anchoredPosition = new Vector2(0f, -4f);
                labelRect.sizeDelta = new Vector2(150f, 20f);

                var labelTmp = labelGO.AddComponent<TextMeshProUGUI>();
                labelTmp.text = node.displayName;
                labelTmp.fontSize = 12;
                labelTmp.alignment = TextAlignmentOptions.Center;
                labelTmp.color = visited ? TEXT_COLOR : new Color(TEXT_COLOR.r, TEXT_COLOR.g, TEXT_COLOR.b, 0.4f);
                labelTmp.enableWordWrapping = false;
            }

            // Badge indicator for gym towns
            if (node.gymIndex >= 0 && GameState.Instance != null && GameState.Instance.Badges[node.gymIndex])
            {
                var badgeGO = new GameObject($"Badge_{node.gymIndex}");
                badgeGO.transform.SetParent(dotGO.transform, false);
                var badgeRect = badgeGO.AddComponent<RectTransform>();
                badgeRect.anchorMin = new Vector2(1f, 1f);
                badgeRect.anchorMax = new Vector2(1f, 1f);
                badgeRect.pivot = new Vector2(0f, 0f);
                badgeRect.anchoredPosition = new Vector2(2f, 2f);
                badgeRect.sizeDelta = new Vector2(10f, 10f);

                var badgeImg = badgeGO.AddComponent<Image>();
                badgeImg.color = BADGE_COLOR;
            }

            // Player marker
            if (node.mapId == currentMap)
            {
                playerMarker = new GameObject("PlayerMarker");
                playerMarker.transform.SetParent(dotGO.transform, false);
                var markerRect = playerMarker.AddComponent<RectTransform>();
                markerRect.anchorMin = new Vector2(0.5f, 1f);
                markerRect.anchorMax = new Vector2(0.5f, 1f);
                markerRect.pivot = new Vector2(0.5f, 0f);
                markerRect.anchoredPosition = new Vector2(0f, 4f);
                markerRect.sizeDelta = new Vector2(12f, 12f);

                var markerImg = playerMarker.AddComponent<Image>();
                markerImg.color = PLAYER_COLOR;
            }
        }
    }

    // ===============================================================
    // State
    // ===============================================================

    private void UpdateMapState()
    {
        blinkTimer = 0f;
        // Rebuild would be needed for dynamic updates, but for now
        // the map is rebuilt each time Show() is called via CreateUI check
    }

    private bool IsMapVisited(string mapId, string currentMap)
    {
        if (string.IsNullOrEmpty(currentMap)) return mapId == "BOURG_NID";

        // Consider a map visited if it's the current map or comes before it in progression
        int currentIdx = GetNodeIndex(currentMap);
        int checkIdx = GetNodeIndex(mapId);

        if (currentIdx < 0 || checkIdx < 0) return false;
        return checkIdx <= currentIdx;
    }

    private int GetNodeIndex(string mapId)
    {
        if (nodes == null) return -1;
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i].mapId == mapId) return i;
        }
        return -1;
    }

    // ===============================================================
    // Helpers
    // ===============================================================

    private TextMeshProUGUI CreateText(string content, RectTransform parent,
        Vector2 anchor, Vector2 anchoredPos, Vector2 sizeDelta,
        float fontSize, TextAlignmentOptions alignment, FontStyles style = FontStyles.Normal)
    {
        var go = new GameObject("Text_" + content);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = TEXT_COLOR;
        tmp.fontStyle = style;
        tmp.enableWordWrapping = false;

        return tmp;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
