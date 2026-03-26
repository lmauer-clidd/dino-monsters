using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Main overworld controller — loads map data, builds 3D tile grid,
/// manages NPC interactions, wild encounters, doors and signs.
/// </summary>
public class OverworldManager : MonoBehaviour
{
    public static OverworldManager Instance { get; private set; }

    [Header("References")]
    public PlayerController player;
    public Camera mainCamera;

    [Header("Settings")]
    public float tileSize = 1f;
    public float encounterRate = 0.15f;

    // Tile type constants
    public const int TILE_GRASS      = 0;
    public const int TILE_PATH       = 1;
    public const int TILE_WALL       = 2;
    public const int TILE_TALL_GRASS = 3;
    public const int TILE_WATER      = 4;
    public const int TILE_SAND       = 5;
    public const int TILE_LEDGE      = 6;

    // Current map data
    private int[,] tileMap;
    private int mapWidth, mapHeight;
    private string currentMapId;
    private List<NPCController> npcs = new List<NPCController>();
    private Dictionary<Vector2Int, DoorData> doors = new Dictionary<Vector2Int, DoorData>();
    private Dictionary<Vector2Int, string> signs = new Dictionary<Vector2Int, string>();
    private Dictionary<Vector2Int, MapTransition> mapTransitions = new Dictionary<Vector2Int, MapTransition>();

    // Tile GameObjects for rendering
    private GameObject[,] tileObjects;
    private Transform tileParent;

    // Colors — warm palette consistent with Constants.cs
    private static readonly Color ColorGrass     = new Color(0.35f, 0.65f, 0.25f);
    private static readonly Color ColorPath      = new Color(0.76f, 0.70f, 0.50f);
    private static readonly Color ColorWall      = new Color(0.45f, 0.40f, 0.35f);
    private static readonly Color ColorTallGrass = new Color(0.25f, 0.55f, 0.20f);
    private static readonly Color ColorWater     = new Color(0.25f, 0.45f, 0.85f);
    private static readonly Color ColorSand      = new Color(0.85f, 0.78f, 0.55f);
    private static readonly Color ColorLedge     = new Color(0.50f, 0.60f, 0.30f);
    private static readonly Color ColorGrassBlade = new Color(0.20f, 0.52f, 0.15f);

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // Check if we are returning from a battle
        if (SceneTransitionData.Instance is BattleSetupData battleReturn)
        {
            BuildMap(battleReturn.returnScene);
            if (player != null)
                player.SetGridPosition((int)battleReturn.returnPosition.x, (int)battleReturn.returnPosition.y);
            SceneTransitionData.Instance = null;
        }
        else
        {
            // Use GameState current map if available, otherwise default
            string mapId = GameState.Instance.CurrentMapId ?? "BOURG_NID";
            BuildMap(mapId);

            // Restore player position from GameState if non-zero
            if (player != null && (GameState.Instance.PlayerX != 0 || GameState.Instance.PlayerY != 0))
                player.SetGridPosition(GameState.Instance.PlayerX, GameState.Instance.PlayerY);
        }

        // Spawn overworld UI components
        SpawnOverworldUI();

        // Play appropriate music based on map type
        PlayMapMusic(currentMapId);

        // Initialize weather system
        if (WeatherSystem.Instance == null)
        {
            var weatherGO = new GameObject("WeatherSystem");
            weatherGO.AddComponent<WeatherSystem>();
        }

        // Set weather and ambient for current map
        if (WeatherSystem.Instance != null)
            WeatherSystem.Instance.SetWeatherForMap(currentMapId);
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayAmbientForMap(currentMapId);

        // Fade in from black (handles return from battle or initial load)
        if (ScreenFade.Instance != null)
            ScreenFade.Instance.FadeIn(0.3f);
    }

    /// <summary>
    /// Play the appropriate music track for the given map.
    /// Routes get route music, towns/cities get town music.
    /// </summary>
    private void PlayMapMusic(string mapId)
    {
        if (AudioManager.Instance == null || string.IsNullOrEmpty(mapId)) return;

        if (mapId.StartsWith("ROUTE"))
            AudioManager.Instance.PlayRouteMusic();
        else
            AudioManager.Instance.PlayTownMusic();
    }

    void SpawnOverworldUI()
    {
        // Pause menu
        if (FindObjectOfType<PauseMenuUI>() == null)
        {
            var pauseGO = new GameObject("PauseMenuUI");
            pauseGO.AddComponent<PauseMenuUI>();
        }

        // Overworld HUD
        if (FindObjectOfType<OverworldHUD>() == null)
        {
            var hudGO = new GameObject("OverworldHUD");
            hudGO.AddComponent<OverworldHUD>();
        }

        // Interior Manager (for building interiors)
        if (FindObjectOfType<InteriorManager>() == null)
        {
            var interiorGO = new GameObject("InteriorManager");
            interiorGO.AddComponent<InteriorManager>();
        }
    }

    // =========================================================================
    //  Map Building
    // =========================================================================

    public void BuildMap(string mapId)
    {
        ClearMap();
        currentMapId = mapId;
        GameState.Instance.CurrentMapId = mapId;

        // Create a parent object to hold all tiles
        tileParent = new GameObject("MapTiles").transform;

        // Generate map data based on mapId
        GenerateMapData(mapId);

        // Build 3D tiles
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                CreateTile(x, y, tileMap[x, y]);
            }
        }

        // Position player at default spawn if not already set
        if (player != null && player.gridX == 0 && player.gridY == 0)
            player.SetGridPosition(10, 10);
    }

    void GenerateMapData(string mapId)
    {
        switch (mapId)
        {
            case "BOURG_NID":
                GenerateBourgNid();
                break;
            case "ROUTE_1":
                GenerateRoute1();
                break;
            case "VILLE_FOUGERE":
                GenerateVilleFougere();
                break;
            case "ROUTE_2":
                GenerateRoute2();
                break;
            case "ROUTE_3":
                GenerateRoute3();
                break;
            case "PORT_COQUILLE":
                GeneratePortCoquille();
                break;
            case "ROUTE_4":
                GenerateRoute4();
                break;
            case "ROCHE_HAUTE":
                GenerateRocheHaute();
                break;
            case "ROUTE_5":
                GenerateRoute5();
                break;
            case "VOLCANVILLE":
                GenerateVolcanville();
                break;
            case "ROUTE_6":
                GenerateRoute6();
                break;
            case "CRYO_CITE":
                GenerateCryoCite();
                break;
            case "ROUTE_7":
                GenerateRoute7();
                break;
            case "ELECTROPOLIS":
                GenerateElectropolis();
                break;
            case "ROUTE_8":
                GenerateRoute8();
                break;
            case "MARAIS_NOIR":
                GenerateMaraisNoir();
                break;
            case "ROUTE_9":
                GenerateRoute9();
                break;
            case "CIEL_HAUT":
                GenerateCielHaut();
                break;
            case "VICTORY_ROAD":
                GenerateVictoryRoad();
                break;
            case "PALEO_CAPITAL":
                GeneratePaleoCapital();
                break;
            default:
                GenerateBourgNid();
                break;
        }
    }

    void GenerateBourgNid()
    {
        mapWidth = 24;
        mapHeight = 24;
        tileMap = new int[mapWidth, mapHeight];
        tileObjects = new GameObject[mapWidth, mapHeight];

        // Fill with grass
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                tileMap[x, y] = TILE_GRASS;

        // Border walls
        for (int x = 0; x < mapWidth; x++)
        {
            tileMap[x, 0] = TILE_WALL;
            tileMap[x, mapHeight - 1] = TILE_WALL;
        }
        for (int y = 0; y < mapHeight; y++)
        {
            tileMap[0, y] = TILE_WALL;
            tileMap[mapWidth - 1, y] = TILE_WALL;
        }

        // Main village path (cross shape)
        for (int x = 1; x < mapWidth - 1; x++) { tileMap[x, 12] = TILE_PATH; tileMap[x, 13] = TILE_PATH; }
        for (int y = 1; y < mapHeight - 1; y++) { tileMap[11, y] = TILE_PATH; tileMap[12, y] = TILE_PATH; }

        // Small plaza in center
        for (int x = 9; x <= 14; x++)
            for (int y = 10; y <= 15; y++)
                tileMap[x, y] = TILE_PATH;

        // Pond area (bottom-right)
        for (int x = 16; x <= 20; x++)
            for (int y = 2; y <= 5; y++)
                tileMap[x, y] = TILE_WATER;
        // Sand around pond
        for (int x = 15; x <= 21; x++)
            for (int y = 1; y <= 6; y++)
                if (tileMap[x, y] != TILE_WATER && tileMap[x, y] != TILE_WALL)
                    tileMap[x, y] = TILE_SAND;

        // Tall grass patches
        for (int x = 2; x <= 7; x++)
            for (int y = 2; y <= 6; y++)
                tileMap[x, y] = TILE_TALL_GRASS;

        for (int x = 2; x <= 7; x++)
            for (int y = 17; y <= 21; y++)
                tileMap[x, y] = TILE_TALL_GRASS;

        // Exit to Route 1 (north, gap in wall)
        tileMap[11, mapHeight - 1] = TILE_PATH;
        tileMap[12, mapHeight - 1] = TILE_PATH;

        // --- Buildings ---
        // Professor's Lab (top-right area)
        Vector3 labPos = new Vector3(17, 0, 17);
        BuildingRenderer.CreateBuilding(tileParent, labPos, 4, 3, 2,
            new Color(0.85f, 0.82f, 0.75f), new Color(0.55f, 0.25f, 0.20f));
        // Block tiles under building
        for (int bx = 17; bx <= 20; bx++)
            for (int by = 17; by <= 19; by++)
                tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(18, 17)] = new DoorData { targetMap = "LAB_INTERIOR", spawnX = 3, spawnY = 1 };
        tileMap[18, 17] = TILE_PATH; // door tile walkable
        if (17 > 0 && tileMap[18, 16] != TILE_PATH) tileMap[18, 16] = TILE_PATH; // porch
        signs[new Vector2Int(17, 17)] = "Laboratoire du Prof. Saule\nRecherche sur les Dinomonstres";

        // Player's house (left of center)
        Vector3 housePos = new Vector3(3, 0, 10);
        BuildingRenderer.CreateBuilding(tileParent, housePos, 3, 3, 1,
            new Color(0.80f, 0.75f, 0.65f), new Color(0.40f, 0.30f, 0.55f));
        for (int bx = 3; bx <= 5; bx++)
            for (int by = 10; by <= 12; by++)
                if (tileMap[bx, by] != TILE_PATH) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(4, 10)] = new DoorData { targetMap = "HOUSE_PLAYER", spawnX = 3, spawnY = 1 };
        tileMap[4, 10] = TILE_PATH; // door tile walkable
        if (tileMap[4, 9] != TILE_PATH) tileMap[4, 9] = TILE_PATH; // porch

        // Rival's house (right of center)
        Vector3 rivalPos = new Vector3(17, 0, 10);
        BuildingRenderer.CreateBuilding(tileParent, rivalPos, 3, 3, 1,
            new Color(0.75f, 0.78f, 0.80f), new Color(0.25f, 0.40f, 0.30f));
        for (int bx = 17; bx <= 19; bx++)
            for (int by = 10; by <= 12; by++)
                if (tileMap[bx, by] != TILE_PATH) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(18, 10)] = new DoorData { targetMap = "HOUSE_RIVAL", spawnX = 3, spawnY = 1 };
        tileMap[18, 10] = TILE_PATH; // door tile walkable
        if (tileMap[18, 9] != TILE_PATH) tileMap[18, 9] = TILE_PATH; // porch

        // Dino Center (bottom-right of plaza)
        Vector3 centerPos = new Vector3(14, 0, 7);
        BuildingRenderer.CreateBuilding(tileParent, centerPos, 4, 3, 2,
            new Color(0.90f, 0.35f, 0.35f), new Color(0.85f, 0.82f, 0.78f));
        for (int bx = 14; bx <= 17; bx++)
            for (int by = 7; by <= 9; by++)
                tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(15, 7)] = new DoorData { targetMap = "DINO_CENTER", spawnX = 3, spawnY = 1, isDinoCenter = true };
        tileMap[15, 7] = TILE_PATH; // door tile walkable
        if (tileMap[15, 6] != TILE_PATH) tileMap[15, 6] = TILE_PATH; // porch
        signs[new Vector2Int(14, 7)] = "Centre Dino\nSoins gratuits pour vos Dinomonstres";

        // --- NPCs ---
        SpawnNPC("Villageois", 13, 8, new string[] {
            "Bienvenue a Bourg-Nid !",
            "C'est un village paisible ou vivent des Dinomonstres."
        }, new Color(0.7f, 0.4f, 0.3f));

        SpawnNPC("Fillette", 6, 13, new string[] {
            "Mon papa dit que les Dinomonstres de la Route 1 sont gentils !",
            "Mais il ne faut pas aller dans les hautes herbes sans Dinomonstre..."
        }, new Color(0.9f, 0.6f, 0.7f));

        // --- Map transition edges ---
        // North exit -> Route 1
        mapTransitions[new Vector2Int(11, mapHeight - 1)] = new MapTransition { targetMap = "ROUTE_1", spawnX = 7, spawnY = 1 };
        mapTransitions[new Vector2Int(12, mapHeight - 1)] = new MapTransition { targetMap = "ROUTE_1", spawnX = 8, spawnY = 1 };

        // --- Additional NPCs with helpful hints ---
        SpawnNPC("Vieux Sage", 10, 15, new string[] {
            "Va voir le Prof. Saule au labo si tu veux un dino !",
            "Son laboratoire est au nord-est du village."
        }, new Color(0.5f, 0.5f, 0.5f));

        // Sign at village entrance
        signs[new Vector2Int(11, 21)] = "BOURG-NID\n\"Le berceau des Dinomonstres\"";

        // --- Story NPCs ---
        SpawnNPC("Mere de famille", 8, 10, new string[] {
            "Quand j'etais petite, il n'y avait que des Pyrex dans les environs.",
            "Maintenant, on voit de nouvelles especes chaque annee !",
            "Fais attention dans les hautes herbes si tes dinos ne sont pas en forme."
        }, new Color(0.8f, 0.5f, 0.6f));

        SpawnNPC("Pecheur retraite", 17, 5, new string[] {
            "La Team Meteore... J'en ai entendu parler recemment.",
            "Ils veulent capturer tous les dinos rares. Sois prudent !",
            "Si tu croises des types en uniforme noir, mefie-toi."
        }, new Color(0.5f, 0.6f, 0.5f));

        SpawnNPC("Assistante du Prof.", 5, 17, new string[] {
            "Le Prof. Saule etudie les Dinomonstres depuis 30 ans !",
            "Savais-tu que les types elementaires ont des avantages et faiblesses ?",
            "Le Feu bat la Plante, la Plante bat l'Eau, et l'Eau bat le Feu !"
        }, new Color(0.6f, 0.7f, 0.8f));
    }

    void GenerateRoute1()
    {
        mapWidth = 16;
        mapHeight = 40;
        tileMap = new int[mapWidth, mapHeight];
        tileObjects = new GameObject[mapWidth, mapHeight];

        // Fill with grass
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                tileMap[x, y] = TILE_GRASS;

        // Border walls
        for (int x = 0; x < mapWidth; x++) { tileMap[x, 0] = TILE_WALL; tileMap[x, mapHeight - 1] = TILE_WALL; }
        for (int y = 0; y < mapHeight; y++) { tileMap[0, y] = TILE_WALL; tileMap[mapWidth - 1, y] = TILE_WALL; }

        // Main path winding north
        for (int y = 1; y < mapHeight - 1; y++)
        {
            int pathX = 7 + Mathf.RoundToInt(Mathf.Sin(y * 0.3f) * 2);
            tileMap[pathX, y] = TILE_PATH;
            tileMap[pathX + 1, y] = TILE_PATH;
        }

        // Tall grass patches along the route
        for (int x = 2; x <= 5; x++)
            for (int y = 5; y <= 12; y++)
                if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_TALL_GRASS;

        for (int x = 10; x <= 13; x++)
            for (int y = 15; y <= 22; y++)
                if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_TALL_GRASS;

        for (int x = 2; x <= 6; x++)
            for (int y = 25; y <= 32; y++)
                if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_TALL_GRASS;

        // Ledges (one-way jumps going south)
        for (int x = 3; x <= 6; x++) tileMap[x, 14] = TILE_LEDGE;
        for (int x = 10; x <= 13; x++) tileMap[x, 24] = TILE_LEDGE;

        // Entry from Bourg-Nid (south)
        tileMap[7, 0] = TILE_PATH;
        tileMap[8, 0] = TILE_PATH;

        // Exit to Ville-Fougere (north)
        tileMap[7, mapHeight - 1] = TILE_PATH;
        tileMap[8, mapHeight - 1] = TILE_PATH;

        // --- Map transition edges ---
        mapTransitions[new Vector2Int(7, 0)] = new MapTransition { targetMap = "BOURG_NID", spawnX = 11, spawnY = 22 };
        mapTransitions[new Vector2Int(8, 0)] = new MapTransition { targetMap = "BOURG_NID", spawnX = 12, spawnY = 22 };
        mapTransitions[new Vector2Int(7, mapHeight - 1)] = new MapTransition { targetMap = "VILLE_FOUGERE", spawnX = 12, spawnY = 1 };
        mapTransitions[new Vector2Int(8, mapHeight - 1)] = new MapTransition { targetMap = "VILLE_FOUGERE", spawnX = 13, spawnY = 1 };

        // --- Signs ---
        signs[new Vector2Int(7, 2)] = "ROUTE 1\nBourg-Nid <-- --> Ville-Fougere";
        signs[new Vector2Int(7, 36)] = "VILLE-FOUGERE\nTout droit !";

        // --- Trainer NPCs ---
        // Trainer 1: Bug catcher near first tall grass patch (faces left toward grass)
        SpawnTrainer("Gamin Leo", 8, 10, new string[] {
            "He ! Tu as des Dinomonstres ?",
            "Montre-moi ce qu'ils savent faire !"
        }, new Color(0.5f, 0.7f, 0.3f), "TRAINER_ROUTE1_LEO",
           new int[] { 37 }, new int[] { 4 });

        // Trainer 2: Lass near middle section (faces left)
        SpawnTrainer("Fillette Mia", 6, 20, new string[] {
            "Les Dinomonstres de cette route sont trop mignons !",
            "Mais les miens sont plus forts !"
        }, new Color(0.9f, 0.5f, 0.6f), "TRAINER_ROUTE1_MIA",
           new int[] { 38, 39 }, new int[] { 3, 4 });

        // Trainer 3: Youngster near the north end (faces left toward grass)
        SpawnTrainer("Campeur Axel", 10, 30, new string[] {
            "J'ai capture mon premier Dinomonstre ici !",
            "Il est deja super fort !"
        }, new Color(0.4f, 0.5f, 0.7f), "TRAINER_ROUTE1_AXEL",
           new int[] { 40, 37 }, new int[] { 4, 5 });

        // --- Helpful NPC ---
        SpawnNPC("Randonneur", 9, 3, new string[] {
            "Les herbes hautes cachent des dinos sauvages !",
            "Affaiblis-les avant de lancer une Jurassic Ball."
        }, new Color(0.6f, 0.5f, 0.3f));

        // --- Procedural trees along the route ---
        AddRouteTrees(0.10f);
    }

    // =========================================================================
    //  Ville-Fougere — First town with gym, dino center, shop
    // =========================================================================

    void GenerateVilleFougere()
    {
        mapWidth = 25;
        mapHeight = 25;
        tileMap = new int[mapWidth, mapHeight];
        tileObjects = new GameObject[mapWidth, mapHeight];

        // Fill with grass
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                tileMap[x, y] = TILE_GRASS;

        // Border walls
        for (int x = 0; x < mapWidth; x++) { tileMap[x, 0] = TILE_WALL; tileMap[x, mapHeight - 1] = TILE_WALL; }
        for (int y = 0; y < mapHeight; y++) { tileMap[0, y] = TILE_WALL; tileMap[mapWidth - 1, y] = TILE_WALL; }

        // Main road: horizontal through center
        for (int x = 1; x < mapWidth - 1; x++)
        {
            tileMap[x, 12] = TILE_PATH;
            tileMap[x, 13] = TILE_PATH;
        }
        // Vertical road crossing
        for (int y = 1; y < mapHeight - 1; y++)
        {
            tileMap[12, y] = TILE_PATH;
            tileMap[13, y] = TILE_PATH;
        }

        // Town plaza (around the intersection)
        for (int x = 10; x <= 15; x++)
            for (int y = 10; y <= 15; y++)
                tileMap[x, y] = TILE_PATH;

        // Decorative fountain in center (water tile)
        tileMap[12, 12] = TILE_WATER;
        tileMap[13, 12] = TILE_WATER;
        tileMap[12, 13] = TILE_WATER;
        tileMap[13, 13] = TILE_WATER;

        // Sand around fountain
        for (int x = 11; x <= 14; x++)
            for (int y = 11; y <= 14; y++)
                if (tileMap[x, y] != TILE_WATER) tileMap[x, y] = TILE_SAND;

        // Small garden patches
        for (int x = 2; x <= 5; x++)
            for (int y = 17; y <= 20; y++)
                tileMap[x, y] = TILE_TALL_GRASS;

        for (int x = 19; x <= 22; x++)
            for (int y = 17; y <= 20; y++)
                tileMap[x, y] = TILE_TALL_GRASS;

        // --- Entry from Route 1 (south) ---
        tileMap[12, 0] = TILE_PATH;
        tileMap[13, 0] = TILE_PATH;

        // --- Exit to Route 2 (north) ---
        tileMap[12, mapHeight - 1] = TILE_PATH;
        tileMap[13, mapHeight - 1] = TILE_PATH;

        // --- Exit to Route 3 (east) ---
        tileMap[mapWidth - 1, 12] = TILE_PATH;
        tileMap[mapWidth - 1, 13] = TILE_PATH;

        // --- Map transitions ---
        mapTransitions[new Vector2Int(12, 0)] = new MapTransition { targetMap = "ROUTE_1", spawnX = 7, spawnY = 38 };
        mapTransitions[new Vector2Int(13, 0)] = new MapTransition { targetMap = "ROUTE_1", spawnX = 8, spawnY = 38 };
        mapTransitions[new Vector2Int(12, mapHeight - 1)] = new MapTransition { targetMap = "ROUTE_2", spawnX = 8, spawnY = 1 };
        mapTransitions[new Vector2Int(13, mapHeight - 1)] = new MapTransition { targetMap = "ROUTE_2", spawnX = 9, spawnY = 1 };
        // East -> Route 3
        mapTransitions[new Vector2Int(mapWidth - 1, 12)] = new MapTransition { targetMap = "ROUTE_3", spawnX = 1, spawnY = 14 };
        mapTransitions[new Vector2Int(mapWidth - 1, 13)] = new MapTransition { targetMap = "ROUTE_3", spawnX = 1, spawnY = 15 };

        // ============ Buildings ============

        // --- Dino Center (left side of plaza) ---
        Vector3 dinoCenterPos = new Vector3(2, 0, 10);
        BuildingRenderer.CreateBuilding(tileParent, dinoCenterPos, 5, 3, 2,
            new Color(0.90f, 0.35f, 0.35f), new Color(0.85f, 0.82f, 0.78f));
        for (int bx = 2; bx <= 6; bx++)
            for (int by = 10; by <= 12; by++)
                tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(4, 10)] = new DoorData { targetMap = "DINO_CENTER_VF", spawnX = 3, spawnY = 1, isDinoCenter = true };
        tileMap[4, 10] = TILE_PATH;
        if (tileMap[4, 9] != TILE_PATH) tileMap[4, 9] = TILE_PATH;
        signs[new Vector2Int(2, 10)] = "CENTRE DINO\nSoins gratuits pour vos dinos";

        // --- Shop (right side of plaza) ---
        Vector3 shopPos = new Vector3(18, 0, 10);
        BuildingRenderer.CreateBuilding(tileParent, shopPos, 4, 3, 1,
            new Color(0.40f, 0.55f, 0.85f), new Color(0.30f, 0.40f, 0.60f));
        for (int bx = 18; bx <= 21; bx++)
            for (int by = 10; by <= 12; by++)
                tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(19, 10)] = new DoorData { targetMap = "SHOP_VF", spawnX = 3, spawnY = 1, isShop = true };
        tileMap[19, 10] = TILE_PATH;
        if (tileMap[19, 9] != TILE_PATH) tileMap[19, 9] = TILE_PATH;
        signs[new Vector2Int(18, 10)] = "BOUTIQUE\nPotions et Jurassic Balls";

        // --- Gym (top-center, prominent building) ---
        Vector3 gymPos = new Vector3(9, 0, 18);
        BuildingRenderer.CreateBuilding(tileParent, gymPos, 6, 4, 2,
            new Color(0.40f, 0.70f, 0.35f), new Color(0.25f, 0.50f, 0.20f));
        for (int bx = 9; bx <= 14; bx++)
            for (int by = 18; by <= 21; by++)
                tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(11, 18)] = new DoorData { targetMap = "GYM_VF", spawnX = 4, spawnY = 1, isGym = true, gymId = 0 };
        tileMap[11, 18] = TILE_PATH;
        if (tileMap[11, 17] != TILE_PATH) tileMap[11, 17] = TILE_PATH;
        signs[new Vector2Int(9, 18)] = "ARENE DE VILLE-FOUGERE\nChampion: FLORA\nType: Plante";

        // --- House 1 (bottom-left) ---
        Vector3 house1Pos = new Vector3(2, 0, 3);
        BuildingRenderer.CreateBuilding(tileParent, house1Pos, 3, 3, 1,
            new Color(0.80f, 0.75f, 0.65f), new Color(0.50f, 0.35f, 0.25f));
        for (int bx = 2; bx <= 4; bx++)
            for (int by = 3; by <= 5; by++)
                tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(3, 3)] = new DoorData { targetMap = "HOUSE_VF_1", spawnX = 3, spawnY = 1 };
        tileMap[3, 3] = TILE_PATH;
        if (tileMap[3, 2] != TILE_PATH) tileMap[3, 2] = TILE_PATH;

        // --- House 2 (bottom-right) ---
        Vector3 house2Pos = new Vector3(19, 0, 3);
        BuildingRenderer.CreateBuilding(tileParent, house2Pos, 3, 3, 1,
            new Color(0.80f, 0.75f, 0.65f), new Color(0.40f, 0.25f, 0.35f));
        for (int bx = 19; bx <= 21; bx++)
            for (int by = 3; by <= 5; by++)
                tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(20, 3)] = new DoorData { targetMap = "HOUSE_VF_2", spawnX = 3, spawnY = 1 };
        tileMap[20, 3] = TILE_PATH;
        if (tileMap[20, 2] != TILE_PATH) tileMap[20, 2] = TILE_PATH;

        // ============ Signs ============
        signs[new Vector2Int(12, 2)] = "VILLE-FOUGERE\n\"Ou la nature s'epanouit\"";
        signs[new Vector2Int(12, 22)] = "ROUTE 2 -->\nForet de Fougere";

        // ============ NPCs ============
        SpawnNPC("Citoyen", 16, 12, new string[] {
            "Bienvenue a Ville-Fougere !",
            "Notre champion d'arene, Flora, est specialiste du type Plante.",
            "Prepare-toi bien avant de la defier !"
        }, new Color(0.6f, 0.5f, 0.4f));

        SpawnNPC("Garcon", 8, 8, new string[] {
            "Le Centre Dino soigne tes Dinomonstres gratuitement !",
            "N'hesite pas a y aller avant l'arene."
        }, new Color(0.4f, 0.6f, 0.8f));

        SpawnNPC("Marchande", 17, 13, new string[] {
            "La boutique vend des Potions et des Jurassic Balls.",
            "Tu en auras besoin pour la Route 2 !"
        }, new Color(0.8f, 0.5f, 0.3f));

        SpawnNPC("Vieille Dame", 5, 15, new string[] {
            "Flora utilise des dinos de type Plante.",
            "Un dino de type Feu serait tres efficace contre elle !"
        }, new Color(0.7f, 0.6f, 0.7f));

        SpawnNPC("Dresseur", 15, 7, new string[] {
            "J'ai deja battu Flora ! Son Fougeraptor est coriace.",
            "Assure-toi que tes dinos sont au moins niveau 10."
        }, new Color(0.3f, 0.5f, 0.4f));

        // --- Story NPCs ---
        SpawnNPC("Herboriste", 4, 10, new string[] {
            "Les plantes de cette region sont uniques !",
            "Le type Plante est faible face au Feu, a la Glace et au Vol.",
            "Si tu n'as pas de dino Feu, essaie le type Vol contre Flora !"
        }, new Color(0.4f, 0.7f, 0.4f));

        SpawnNPC("Voyageur", 20, 8, new string[] {
            "Je viens de la Route 3. J'ai vu des types louches en uniforme noir...",
            "Ils se font appeler la Team Meteore. Ils capturent des dinos sauvages en masse !",
            "Fais attention si tu vas vers le sud."
        }, new Color(0.6f, 0.5f, 0.3f));

        SpawnNPC("Collectionneur", 10, 18, new string[] {
            "Tu remplis le Dinodex ? C'est genial !",
            "Chaque route a des dinos differents. Explore bien !",
            "Apres l'arene, la Route 2 mene vers de nouvelles decouvertes."
        }, new Color(0.5f, 0.4f, 0.6f));
    }

    // =========================================================================
    //  Route 2 — Connects Ville-Fougere northward
    // =========================================================================

    void GenerateRoute2()
    {
        mapWidth = 20;
        mapHeight = 35;
        tileMap = new int[mapWidth, mapHeight];
        tileObjects = new GameObject[mapWidth, mapHeight];

        // Fill with grass
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                tileMap[x, y] = TILE_GRASS;

        // Border walls
        for (int x = 0; x < mapWidth; x++) { tileMap[x, 0] = TILE_WALL; tileMap[x, mapHeight - 1] = TILE_WALL; }
        for (int y = 0; y < mapHeight; y++) { tileMap[0, y] = TILE_WALL; tileMap[mapWidth - 1, y] = TILE_WALL; }

        // Main path — wider with a curve
        for (int y = 1; y < mapHeight - 1; y++)
        {
            int pathX = 9 + Mathf.RoundToInt(Mathf.Sin(y * 0.2f) * 3);
            pathX = Mathf.Clamp(pathX, 1, mapWidth - 3);
            tileMap[pathX, y] = TILE_PATH;
            tileMap[pathX + 1, y] = TILE_PATH;
        }

        // Dense tall grass patches (forest route feel)
        for (int x = 1; x <= 5; x++)
            for (int y = 3; y <= 10; y++)
                if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_TALL_GRASS;

        for (int x = 13; x <= 18; x++)
            for (int y = 8; y <= 16; y++)
                if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_TALL_GRASS;

        for (int x = 2; x <= 7; x++)
            for (int y = 18; y <= 25; y++)
                if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_TALL_GRASS;

        for (int x = 12; x <= 17; x++)
            for (int y = 26; y <= 32; y++)
                if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_TALL_GRASS;

        // Small pond (midway rest area)
        for (int x = 3; x <= 5; x++)
            for (int y = 14; y <= 16; y++)
                tileMap[x, y] = TILE_WATER;
        for (int x = 2; x <= 6; x++)
            for (int y = 13; y <= 17; y++)
                if (tileMap[x, y] != TILE_WATER && tileMap[x, y] != TILE_WALL)
                    tileMap[x, y] = TILE_SAND;

        // Ledges
        for (int x = 2; x <= 5; x++) tileMap[x, 11] = TILE_LEDGE;
        for (int x = 14; x <= 17; x++) tileMap[x, 23] = TILE_LEDGE;

        // --- Entry from Ville-Fougere (south) ---
        tileMap[8, 0] = TILE_PATH;
        tileMap[9, 0] = TILE_PATH;

        // --- North exit (future area, currently wall with sign) ---
        tileMap[9, mapHeight - 1] = TILE_WALL;
        tileMap[10, mapHeight - 1] = TILE_WALL;

        // --- Map transitions ---
        mapTransitions[new Vector2Int(8, 0)] = new MapTransition { targetMap = "VILLE_FOUGERE", spawnX = 12, spawnY = 23 };
        mapTransitions[new Vector2Int(9, 0)] = new MapTransition { targetMap = "VILLE_FOUGERE", spawnX = 13, spawnY = 23 };

        // --- Signs ---
        signs[new Vector2Int(8, 2)] = "ROUTE 2 — Foret de Fougere\nVille-Fougere <-- --> ???";
        signs[new Vector2Int(9, 32)] = "Travaux en cours.\nPassage ferme pour le moment.";

        // --- Trainer NPCs ---
        SpawnTrainer("Insectomane Luc", 6, 7, new string[] {
            "Les dinos insectes de cette foret sont incroyables !",
            "Voyons si tu peux battre les miens !"
        }, new Color(0.3f, 0.6f, 0.2f), "TRAINER_ROUTE2_LUC",
           new int[] { 39, 41 }, new int[] { 6, 7 });

        SpawnTrainer("Randonneuse Eva", 14, 20, new string[] {
            "J'adore me promener dans cette foret.",
            "Mais mes dinos adorent aussi se battre !"
        }, new Color(0.7f, 0.4f, 0.5f), "TRAINER_ROUTE2_EVA",
           new int[] { 37, 38, 40 }, new int[] { 5, 6, 5 });

        // --- Helpful NPCs ---
        SpawnNPC("Chercheur", 4, 18, new string[] {
            "Les dinos de cette route sont plus forts que sur la Route 1.",
            "Assure-toi d'avoir assez de Potions !"
        }, new Color(0.5f, 0.5f, 0.6f));

        SpawnNPC("Garde-forestier", 10, 30, new string[] {
            "La route au nord est bloquee pour le moment.",
            "Reviens plus tard quand les travaux seront finis !"
        }, new Color(0.4f, 0.55f, 0.3f));

        // --- Dense procedural trees (forest route) ---
        AddRouteTrees(0.18f);
    }

    // =========================================================================
    //  Route 3 — Rocky path connecting Ville-Fougere east to Port-Coquille
    // =========================================================================

    void GenerateRoute3()
    {
        mapWidth = 20;
        mapHeight = 30;
        tileMap = new int[mapWidth, mapHeight];
        tileObjects = new GameObject[mapWidth, mapHeight];

        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                tileMap[x, y] = TILE_GRASS;

        for (int x = 0; x < mapWidth; x++) { tileMap[x, 0] = TILE_WALL; tileMap[x, mapHeight - 1] = TILE_WALL; }
        for (int y = 0; y < mapHeight; y++) { tileMap[0, y] = TILE_WALL; tileMap[mapWidth - 1, y] = TILE_WALL; }

        // Main winding path
        for (int y = 1; y < mapHeight - 1; y++)
        {
            int pathX = 9 + Mathf.RoundToInt(Mathf.Sin(y * 0.25f) * 3);
            pathX = Mathf.Clamp(pathX, 1, mapWidth - 3);
            tileMap[pathX, y] = TILE_PATH;
            tileMap[pathX + 1, y] = TILE_PATH;
        }

        // Boulder clusters
        int[] boulderX = { 3, 5, 14, 16, 4, 15, 7, 12 };
        int[] boulderY = { 5, 8, 6, 10, 18, 20, 24, 25 };
        for (int i = 0; i < boulderX.Length; i++)
            if (tileMap[boulderX[i], boulderY[i]] == TILE_GRASS)
                tileMap[boulderX[i], boulderY[i]] = TILE_WALL;

        // Rocky sand patches
        for (int x = 2; x <= 6; x++)
            for (int y = 4; y <= 9; y++)
                if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_SAND;
        for (int x = 13; x <= 17; x++)
            for (int y = 5; y <= 11; y++)
                if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_SAND;

        // Tall grass patches for encounters
        for (int x = 2; x <= 6; x++)
            for (int y = 12; y <= 17; y++)
                if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_TALL_GRASS;
        for (int x = 12; x <= 17; x++)
            for (int y = 18; y <= 24; y++)
                if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_TALL_GRASS;
        for (int x = 3; x <= 8; x++)
            for (int y = 24; y <= 27; y++)
                if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_TALL_GRASS;

        // Stream crossing
        for (int x = 1; x <= 5; x++)
            tileMap[x, 22] = TILE_WATER;

        // Ledges
        for (int x = 3; x <= 7; x++) tileMap[x, 10] = TILE_LEDGE;

        // West entry from Ville-Fougere
        tileMap[0, 14] = TILE_PATH;
        tileMap[0, 15] = TILE_PATH;
        // East exit to Port-Coquille
        tileMap[mapWidth - 1, 14] = TILE_PATH;
        tileMap[mapWidth - 1, 15] = TILE_PATH;

        // Map transitions
        mapTransitions[new Vector2Int(0, 14)] = new MapTransition { targetMap = "VILLE_FOUGERE", spawnX = 23, spawnY = 12 };
        mapTransitions[new Vector2Int(0, 15)] = new MapTransition { targetMap = "VILLE_FOUGERE", spawnX = 23, spawnY = 13 };
        mapTransitions[new Vector2Int(mapWidth - 1, 14)] = new MapTransition { targetMap = "PORT_COQUILLE", spawnX = 1, spawnY = 12 };
        mapTransitions[new Vector2Int(mapWidth - 1, 15)] = new MapTransition { targetMap = "PORT_COQUILLE", spawnX = 1, spawnY = 13 };

        // Relay Dino Center (halfway)
        Vector3 relayCenterPos = new Vector3(8, 0, 14);
        BuildingRenderer.CreateBuilding(tileParent, relayCenterPos, 3, 2, 1,
            new Color(0.90f, 0.35f, 0.35f), new Color(0.85f, 0.82f, 0.78f));
        for (int bx = 8; bx <= 10; bx++)
            for (int by = 14; by <= 15; by++)
                tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(9, 14)] = new DoorData { targetMap = "DINO_CENTER_R3", spawnX = 3, spawnY = 1, isDinoCenter = true };
        tileMap[9, 14] = TILE_PATH;
        if (tileMap[9, 13] != TILE_PATH) tileMap[9, 13] = TILE_PATH;
        signs[new Vector2Int(8, 14)] = "RELAIS DINO\nSoins en chemin";

        signs[new Vector2Int(2, 14)] = "ROUTE 3 — Chemin Rocheux\nVille-Fougere <-- --> Port-Coquille";
        signs[new Vector2Int(17, 14)] = "PORT-COQUILLE\nTout droit !";

        SpawnTrainer("Randonneur Marc", 6, 15, new string[] {
            "Ces rochers cachent des dinos costauds !",
            "Voyons si tu es a la hauteur !"
        }, new Color(0.5f, 0.45f, 0.35f), "TRAINER_ROUTE3_MARC",
           new int[] { 49, 70 }, new int[] { 9, 10 });

        SpawnTrainer("Exploratrice Nina", 14, 22, new string[] {
            "J'adore explorer ces chemins rocheux.",
            "Mes dinos aquatiques sont imbattables !"
        }, new Color(0.4f, 0.6f, 0.8f), "TRAINER_ROUTE3_NINA",
           new int[] { 70, 49 }, new int[] { 10, 11 });

        SpawnNPC("Geologue", 4, 7, new string[] {
            "Ces rochers sont d'origine volcanique !",
            "On trouve des dinos Terre et Eau dans le coin."
        }, new Color(0.6f, 0.5f, 0.4f));
    }

    // =========================================================================
    //  Port-Coquille — Coastal town, Gym 2 (Water)
    // =========================================================================

    void GeneratePortCoquille()
    {
        mapWidth = 25;
        mapHeight = 25;
        tileMap = new int[mapWidth, mapHeight];
        tileObjects = new GameObject[mapWidth, mapHeight];

        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                tileMap[x, y] = TILE_GRASS;

        for (int x = 0; x < mapWidth; x++) { tileMap[x, 0] = TILE_WALL; tileMap[x, mapHeight - 1] = TILE_WALL; }
        for (int y = 0; y < mapHeight; y++) { tileMap[0, y] = TILE_WALL; tileMap[mapWidth - 1, y] = TILE_WALL; }

        // Waterfront (south edge — docks)
        for (int x = 1; x < mapWidth - 1; x++)
            for (int y = 1; y <= 4; y++)
                tileMap[x, y] = TILE_WATER;
        for (int x = 1; x < mapWidth - 1; x++)
            for (int y = 5; y <= 6; y++)
                tileMap[x, y] = TILE_SAND;

        // Dock piers
        for (int y = 1; y <= 6; y++) { tileMap[6, y] = TILE_PATH; tileMap[12, y] = TILE_PATH; tileMap[18, y] = TILE_PATH; }

        // Main roads
        for (int x = 1; x < mapWidth - 1; x++) { tileMap[x, 12] = TILE_PATH; tileMap[x, 13] = TILE_PATH; }
        for (int y = 6; y < mapHeight - 1; y++) { tileMap[12, y] = TILE_PATH; tileMap[13, y] = TILE_PATH; }

        // Town plaza
        for (int x = 10; x <= 15; x++)
            for (int y = 10; y <= 15; y++)
                tileMap[x, y] = TILE_PATH;

        // West entry from Route 3
        tileMap[0, 12] = TILE_PATH; tileMap[0, 13] = TILE_PATH;
        // North exit to Route 4
        tileMap[12, mapHeight - 1] = TILE_PATH; tileMap[13, mapHeight - 1] = TILE_PATH;

        mapTransitions[new Vector2Int(0, 12)] = new MapTransition { targetMap = "ROUTE_3", spawnX = 18, spawnY = 14 };
        mapTransitions[new Vector2Int(0, 13)] = new MapTransition { targetMap = "ROUTE_3", spawnX = 18, spawnY = 15 };
        mapTransitions[new Vector2Int(12, mapHeight - 1)] = new MapTransition { targetMap = "ROUTE_4", spawnX = 10, spawnY = 1 };
        mapTransitions[new Vector2Int(13, mapHeight - 1)] = new MapTransition { targetMap = "ROUTE_4", spawnX = 11, spawnY = 1 };

        // Dino Center
        Vector3 dinoCenterPos = new Vector3(2, 0, 10);
        BuildingRenderer.CreateBuilding(tileParent, dinoCenterPos, 5, 3, 2,
            new Color(0.90f, 0.35f, 0.35f), new Color(0.85f, 0.82f, 0.78f));
        for (int bx = 2; bx <= 6; bx++) for (int by = 10; by <= 12; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(4, 10)] = new DoorData { targetMap = "DINO_CENTER_PC", spawnX = 3, spawnY = 1, isDinoCenter = true };
        tileMap[4, 10] = TILE_PATH;
        if (tileMap[4, 9] != TILE_PATH) tileMap[4, 9] = TILE_PATH;
        signs[new Vector2Int(2, 10)] = "CENTRE DINO\nSoins gratuits pour vos dinos";

        // Shop
        Vector3 shopPos = new Vector3(18, 0, 10);
        BuildingRenderer.CreateBuilding(tileParent, shopPos, 4, 3, 1,
            new Color(0.40f, 0.55f, 0.85f), new Color(0.30f, 0.40f, 0.60f));
        for (int bx = 18; bx <= 21; bx++) for (int by = 10; by <= 12; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(19, 10)] = new DoorData { targetMap = "SHOP_PC", spawnX = 3, spawnY = 1, isShop = true };
        tileMap[19, 10] = TILE_PATH;
        if (tileMap[19, 9] != TILE_PATH) tileMap[19, 9] = TILE_PATH;
        signs[new Vector2Int(18, 10)] = "BOUTIQUE MARINE\nArticles pour dresseurs";

        // Gym (water themed — blue)
        Vector3 gymPos = new Vector3(9, 0, 18);
        BuildingRenderer.CreateBuilding(tileParent, gymPos, 6, 4, 2,
            new Color(0.30f, 0.50f, 0.85f), new Color(0.20f, 0.35f, 0.70f));
        for (int bx = 9; bx <= 14; bx++) for (int by = 18; by <= 21; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(11, 18)] = new DoorData { targetMap = "GYM_PC", spawnX = 4, spawnY = 1, isGym = true, gymId = 1 };
        tileMap[11, 18] = TILE_PATH;
        if (tileMap[11, 17] != TILE_PATH) tileMap[11, 17] = TILE_PATH;
        signs[new Vector2Int(9, 18)] = "ARENE DE PORT-COQUILLE\nChampion: MARIN\nType: Eau";

        // House 1 (top-left)
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(2, 0, 17), 3, 3, 1,
            new Color(0.80f, 0.75f, 0.65f), new Color(0.40f, 0.50f, 0.60f));
        for (int bx = 2; bx <= 4; bx++) for (int by = 17; by <= 19; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(3, 17)] = new DoorData { targetMap = "HOUSE_PC_1", spawnX = 3, spawnY = 1 };
        tileMap[3, 17] = TILE_PATH;
        if (tileMap[3, 16] != TILE_PATH) tileMap[3, 16] = TILE_PATH;

        // House 2 (top-right)
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(20, 0, 17), 3, 3, 1,
            new Color(0.80f, 0.75f, 0.65f), new Color(0.35f, 0.45f, 0.55f));
        for (int bx = 20; bx <= 22; bx++) for (int by = 17; by <= 19; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(21, 17)] = new DoorData { targetMap = "HOUSE_PC_2", spawnX = 3, spawnY = 1 };
        tileMap[21, 17] = TILE_PATH;
        if (tileMap[21, 16] != TILE_PATH) tileMap[21, 16] = TILE_PATH;

        signs[new Vector2Int(12, 7)] = "PORT-COQUILLE\n\"La perle de la cote\"";
        signs[new Vector2Int(12, 22)] = "ROUTE 4 -->\nVers Roche-Haute";

        SpawnNPC("Pecheur", 7, 6, new string[] {
            "Les dinos aquatiques sont les plus beaux !",
            "On en trouve plein dans les eaux du port."
        }, new Color(0.4f, 0.5f, 0.7f));
        SpawnNPC("Marin", 18, 6, new string[] {
            "Le champion Marin est un specialiste de l'eau.",
            "Ses dinos sont redoutables quand il pleut !"
        }, new Color(0.3f, 0.4f, 0.6f));
        SpawnNPC("Fillette", 16, 13, new string[] {
            "J'adore jouer sur la plage !",
            "Parfois on voit des dinos sortir de l'eau."
        }, new Color(0.9f, 0.6f, 0.7f));
        SpawnNPC("Vieux Loup de Mer", 5, 13, new string[] {
            "Marin utilise des dinos Eau puissants.",
            "Un dino de type Plante ou Electrique serait ideal contre lui !"
        }, new Color(0.5f, 0.5f, 0.5f));

        // --- Story NPCs ---
        SpawnNPC("Scientifique marine", 12, 10, new string[] {
            "Nos recherches sur les dinos marins avancent bien...",
            "Enfin, elles avancaient. La Team Meteore a vole certains de nos specimens !",
            "Si tu vas a Roche-Haute, peut-etre que tu pourras les retrouver dans la mine."
        }, new Color(0.4f, 0.6f, 0.7f));

        SpawnNPC("Capitaine", 22, 9, new string[] {
            "J'ai navigue sur toutes les mers de la region.",
            "On raconte qu'un dino legendaire dort sous les vagues...",
            "Mais la Team Meteore s'interesse plutot a celui des profondeurs terrestres."
        }, new Color(0.3f, 0.3f, 0.5f));

        SpawnNPC("Vendeur d'appats", 10, 4, new string[] {
            "Marin, le champion, a trois dinos Eau. Ils sont costauds !",
            "Le type Electrique est super efficace contre l'Eau.",
            "Tu peux trouver des dinos Electrique sur la Route 3 si tu es chanceux."
        }, new Color(0.7f, 0.6f, 0.4f));
    }

    // =========================================================================
    //  Route 4 — Mountain path, Port-Coquille north to Roche-Haute
    // =========================================================================

    void GenerateRoute4()
    {
        mapWidth = 20;
        mapHeight = 35;
        tileMap = new int[mapWidth, mapHeight];
        tileObjects = new GameObject[mapWidth, mapHeight];

        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                tileMap[x, y] = TILE_GRASS;

        for (int x = 0; x < mapWidth; x++) { tileMap[x, 0] = TILE_WALL; tileMap[x, mapHeight - 1] = TILE_WALL; }
        for (int y = 0; y < mapHeight; y++) { tileMap[0, y] = TILE_WALL; tileMap[mapWidth - 1, y] = TILE_WALL; }

        // Steep winding mountain path
        for (int y = 1; y < mapHeight - 1; y++)
        {
            int pathX = 9 + Mathf.RoundToInt(Mathf.Sin(y * 0.35f) * 3);
            pathX = Mathf.Clamp(pathX, 1, mapWidth - 3);
            tileMap[pathX, y] = TILE_PATH;
            tileMap[pathX + 1, y] = TILE_PATH;
        }

        // Rocky terrain
        for (int x = 1; x <= 4; x++) for (int y = 3; y <= 10; y++) if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_SAND;
        for (int x = 14; x <= 18; x++) for (int y = 8; y <= 16; y++) if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_SAND;
        for (int x = 1; x <= 6; x++) for (int y = 22; y <= 28; y++) if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_SAND;

        // Boulder walls
        for (int x = 1; x <= 3; x++) tileMap[x, 12] = TILE_WALL;
        for (int x = 15; x <= 18; x++) tileMap[x, 20] = TILE_WALL;
        for (int x = 1; x <= 4; x++) tileMap[x, 30] = TILE_WALL;

        // Tall grass
        for (int x = 2; x <= 6; x++) for (int y = 14; y <= 19; y++) if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_TALL_GRASS;
        for (int x = 12; x <= 17; x++) for (int y = 22; y <= 28; y++) if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_TALL_GRASS;
        for (int x = 5; x <= 10; x++) for (int y = 30; y <= 33; y++) if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_TALL_GRASS;

        // Ledges
        for (int x = 4; x <= 8; x++) tileMap[x, 11] = TILE_LEDGE;
        for (int x = 12; x <= 16; x++) tileMap[x, 21] = TILE_LEDGE;

        // Cave entrance marker
        for (int x = 2; x <= 4; x++) for (int by = 20; by <= 21; by++) tileMap[x, by] = TILE_WALL;
        signs[new Vector2Int(2, 20)] = "GROTTE DE LA MONTAGNE\n(Entree bloquee par des rochers)";

        // South entry from Port-Coquille
        tileMap[10, 0] = TILE_PATH; tileMap[11, 0] = TILE_PATH;
        // North exit to Roche-Haute
        tileMap[9, mapHeight - 1] = TILE_PATH; tileMap[10, mapHeight - 1] = TILE_PATH;

        mapTransitions[new Vector2Int(10, 0)] = new MapTransition { targetMap = "PORT_COQUILLE", spawnX = 12, spawnY = 23 };
        mapTransitions[new Vector2Int(11, 0)] = new MapTransition { targetMap = "PORT_COQUILLE", spawnX = 13, spawnY = 23 };
        mapTransitions[new Vector2Int(9, mapHeight - 1)] = new MapTransition { targetMap = "ROCHE_HAUTE", spawnX = 12, spawnY = 1 };
        mapTransitions[new Vector2Int(10, mapHeight - 1)] = new MapTransition { targetMap = "ROCHE_HAUTE", spawnX = 13, spawnY = 1 };

        signs[new Vector2Int(10, 2)] = "ROUTE 4 — Sentier Montagneux\nPort-Coquille <-- --> Roche-Haute";
        signs[new Vector2Int(9, 32)] = "ROCHE-HAUTE\nTout droit !";

        SpawnTrainer("Mineur Jean", 7, 16, new string[] {
            "J'ai trouve des fossiles incroyables ici !",
            "Mes dinos sont durs comme la roche !"
        }, new Color(0.5f, 0.4f, 0.3f), "TRAINER_ROUTE4_JEAN",
           new int[] { 10, 49 }, new int[] { 12, 13 });
        SpawnTrainer("Alpiniste Chloe", 14, 25, new string[] {
            "L'air de la montagne rend mes dinos plus forts !",
            "Tu ne passeras pas facilement !"
        }, new Color(0.6f, 0.5f, 0.7f), "TRAINER_ROUTE4_CHLOE",
           new int[] { 11, 55, 49 }, new int[] { 12, 13, 12 });
        SpawnTrainer("Speleologue Theo", 5, 8, new string[] {
            "J'explore les grottes a la recherche de dinos fossiles.",
            "Ceux que j'ai trouves sont redoutables !"
        }, new Color(0.4f, 0.4f, 0.5f), "TRAINER_ROUTE4_THEO",
           new int[] { 64, 10 }, new int[] { 13, 14 });

        SpawnNPC("Guide de montagne", 11, 15, new string[] {
            "Attention aux ledges, on ne peut sauter que vers le bas !",
            "La grotte est bloquee pour l'instant, reviens plus tard."
        }, new Color(0.5f, 0.55f, 0.4f));
    }

    // =========================================================================
    //  Roche-Haute — Mining town, Gym 3 (Rock/Fossil)
    // =========================================================================

    void GenerateRocheHaute()
    {
        mapWidth = 25;
        mapHeight = 25;
        tileMap = new int[mapWidth, mapHeight];
        tileObjects = new GameObject[mapWidth, mapHeight];

        // Fill with sand (rocky ground)
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                tileMap[x, y] = TILE_SAND;

        for (int x = 0; x < mapWidth; x++) { tileMap[x, 0] = TILE_WALL; tileMap[x, mapHeight - 1] = TILE_WALL; }
        for (int y = 0; y < mapHeight; y++) { tileMap[0, y] = TILE_WALL; tileMap[mapWidth - 1, y] = TILE_WALL; }

        // Main roads
        for (int x = 1; x < mapWidth - 1; x++) { tileMap[x, 12] = TILE_PATH; tileMap[x, 13] = TILE_PATH; }
        for (int y = 1; y < mapHeight - 1; y++) { tileMap[12, y] = TILE_PATH; tileMap[13, y] = TILE_PATH; }

        // Town plaza
        for (int x = 10; x <= 15; x++) for (int y = 10; y <= 15; y++) tileMap[x, y] = TILE_PATH;

        // Decorative boulders
        int[] decBX = { 3, 5, 20, 22, 4, 21, 8, 17 };
        int[] decBY = { 3, 5, 4, 6, 20, 21, 7, 8 };
        for (int i = 0; i < decBX.Length; i++) tileMap[decBX[i], decBY[i]] = TILE_WALL;

        // Small grass patches
        for (int x = 2; x <= 5; x++) for (int y = 7; y <= 9; y++) if (tileMap[x, y] == TILE_SAND) tileMap[x, y] = TILE_GRASS;
        for (int x = 19; x <= 22; x++) for (int y = 7; y <= 9; y++) if (tileMap[x, y] == TILE_SAND) tileMap[x, y] = TILE_GRASS;

        // South entry from Route 4
        tileMap[12, 0] = TILE_PATH; tileMap[13, 0] = TILE_PATH;
        // North exit to Route 5
        tileMap[12, mapHeight - 1] = TILE_PATH; tileMap[13, mapHeight - 1] = TILE_PATH;

        mapTransitions[new Vector2Int(12, 0)] = new MapTransition { targetMap = "ROUTE_4", spawnX = 9, spawnY = 33 };
        mapTransitions[new Vector2Int(13, 0)] = new MapTransition { targetMap = "ROUTE_4", spawnX = 10, spawnY = 33 };
        mapTransitions[new Vector2Int(12, mapHeight - 1)] = new MapTransition { targetMap = "ROUTE_5", spawnX = 10, spawnY = 1 };
        mapTransitions[new Vector2Int(13, mapHeight - 1)] = new MapTransition { targetMap = "ROUTE_5", spawnX = 11, spawnY = 1 };

        // Dino Center
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(2, 0, 10), 5, 3, 2,
            new Color(0.90f, 0.35f, 0.35f), new Color(0.85f, 0.82f, 0.78f));
        for (int bx = 2; bx <= 6; bx++) for (int by = 10; by <= 12; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(4, 10)] = new DoorData { targetMap = "DINO_CENTER_RH", spawnX = 3, spawnY = 1, isDinoCenter = true };
        tileMap[4, 10] = TILE_PATH;
        if (tileMap[4, 9] != TILE_PATH) tileMap[4, 9] = TILE_PATH;
        signs[new Vector2Int(2, 10)] = "CENTRE DINO\nSoins gratuits pour vos dinos";

        // Shop
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(18, 0, 10), 4, 3, 1,
            new Color(0.40f, 0.55f, 0.85f), new Color(0.45f, 0.35f, 0.25f));
        for (int bx = 18; bx <= 21; bx++) for (int by = 10; by <= 12; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(19, 10)] = new DoorData { targetMap = "SHOP_RH", spawnX = 3, spawnY = 1, isShop = true };
        tileMap[19, 10] = TILE_PATH;
        if (tileMap[19, 9] != TILE_PATH) tileMap[19, 9] = TILE_PATH;
        signs[new Vector2Int(18, 10)] = "BOUTIQUE DU MINEUR\nOutils et potions";

        // Gym (rock/fossil themed — green for gym detection)
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(9, 0, 18), 6, 4, 2,
            new Color(0.40f, 0.70f, 0.35f), new Color(0.40f, 0.35f, 0.30f));
        for (int bx = 9; bx <= 14; bx++) for (int by = 18; by <= 21; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(11, 18)] = new DoorData { targetMap = "GYM_RH", spawnX = 4, spawnY = 1, isGym = true, gymId = 2 };
        tileMap[11, 18] = TILE_PATH;
        if (tileMap[11, 17] != TILE_PATH) tileMap[11, 17] = TILE_PATH;
        signs[new Vector2Int(9, 18)] = "ARENE DE ROCHE-HAUTE\nChampion: PETRA\nType: Fossile";

        // Mine entrance
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(2, 0, 18), 4, 3, 2,
            new Color(0.40f, 0.35f, 0.30f), new Color(0.30f, 0.25f, 0.20f));
        for (int bx = 2; bx <= 5; bx++) for (int by = 18; by <= 20; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(3, 18)] = new DoorData { targetMap = "MINE_RH", spawnX = 3, spawnY = 1 };
        tileMap[3, 18] = TILE_PATH;
        if (tileMap[3, 17] != TILE_PATH) tileMap[3, 17] = TILE_PATH;
        signs[new Vector2Int(2, 18)] = "MINE DE ROCHE-HAUTE\n(Acces restreint)";

        // House 1 (bottom-left)
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(2, 0, 3), 3, 3, 1,
            new Color(0.80f, 0.75f, 0.65f), new Color(0.45f, 0.35f, 0.25f));
        for (int bx = 2; bx <= 4; bx++) for (int by = 3; by <= 5; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(3, 3)] = new DoorData { targetMap = "HOUSE_RH_1", spawnX = 3, spawnY = 1 };
        tileMap[3, 3] = TILE_PATH;
        if (tileMap[3, 2] != TILE_PATH) tileMap[3, 2] = TILE_PATH;

        // House 2 (bottom-right)
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(19, 0, 3), 3, 3, 1,
            new Color(0.80f, 0.75f, 0.65f), new Color(0.40f, 0.30f, 0.25f));
        for (int bx = 19; bx <= 21; bx++) for (int by = 3; by <= 5; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(20, 3)] = new DoorData { targetMap = "HOUSE_RH_2", spawnX = 3, spawnY = 1 };
        tileMap[20, 3] = TILE_PATH;
        if (tileMap[20, 2] != TILE_PATH) tileMap[20, 2] = TILE_PATH;

        // House 3 (right side)
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(20, 0, 15), 3, 3, 1,
            new Color(0.80f, 0.75f, 0.65f), new Color(0.42f, 0.32f, 0.28f));
        for (int bx = 20; bx <= 22; bx++) for (int by = 15; by <= 17; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(21, 15)] = new DoorData { targetMap = "HOUSE_RH_3", spawnX = 3, spawnY = 1 };
        tileMap[21, 15] = TILE_PATH;
        if (tileMap[21, 14] != TILE_PATH) tileMap[21, 14] = TILE_PATH;

        signs[new Vector2Int(12, 2)] = "ROCHE-HAUTE\n\"Gardienne des fossiles\"";
        signs[new Vector2Int(12, 22)] = "ROUTE 5 -->\nVers Volcanville";

        SpawnNPC("Mineur", 16, 12, new string[] {
            "Bienvenue a Roche-Haute !",
            "On extrait des fossiles de dinos dans la mine.",
            "Certains peuvent etre ressuscites !"
        }, new Color(0.5f, 0.45f, 0.35f));
        SpawnNPC("Geologue", 8, 8, new string[] {
            "Petra, la championne, utilise des dinos Fossile.",
            "Ils sont tres resistants mais lents.",
            "Un dino Eau ou Plante pourrait les surprendre !"
        }, new Color(0.6f, 0.5f, 0.4f));
        SpawnNPC("Apprenti Mineur", 4, 15, new string[] {
            "La mine est pleine de dinos sauvages de type Terre.",
            "Il faut etre bien prepare avant d'y entrer !"
        }, new Color(0.55f, 0.45f, 0.35f));
        SpawnNPC("Ancienne", 17, 7, new string[] {
            "Les fossiles de Roche-Haute sont vieux de millions d'annees.",
            "Petra les comprend comme personne d'autre."
        }, new Color(0.6f, 0.55f, 0.5f));

        // --- Story NPCs ---
        SpawnNPC("Garde-mine", 10, 10, new string[] {
            "La mine... Des types bizarres y ont ete vus recemment.",
            "Ils portent un uniforme noir avec un symbole de meteore.",
            "La Team Meteore utiliserait la mine pour capturer des dinos rares !"
        }, new Color(0.4f, 0.4f, 0.35f));

        SpawnNPC("Paleontologue", 6, 5, new string[] {
            "Petra est une championne redoutable avec ses dinos Fossile et Terre.",
            "Le type Eau est super efficace contre la Terre et la Roche.",
            "Si tu as un dino Eau, tu auras l'avantage a l'arene !"
        }, new Color(0.5f, 0.5f, 0.6f));

        SpawnNPC("Marchande ambulante", 14, 18, new string[] {
            "Je voyage de ville en ville pour vendre mes trouvailles.",
            "A Volcanville, plus au sud, il y a une arene de type Feu. Intense !",
            "Si tu vas la-bas, emmene des dinos Eau ou Terre avec toi."
        }, new Color(0.8f, 0.6f, 0.3f));
    }

    // =========================================================================
    //  Route 5 — Volcanic path, Roche-Haute to Volcanville
    // =========================================================================

    void GenerateRoute5()
    {
        mapWidth = 20;
        mapHeight = 30;
        tileMap = new int[mapWidth, mapHeight];
        tileObjects = new GameObject[mapWidth, mapHeight];

        // Fill with sand (volcanic terrain)
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                tileMap[x, y] = TILE_SAND;

        for (int x = 0; x < mapWidth; x++) { tileMap[x, 0] = TILE_WALL; tileMap[x, mapHeight - 1] = TILE_WALL; }
        for (int y = 0; y < mapHeight; y++) { tileMap[0, y] = TILE_WALL; tileMap[mapWidth - 1, y] = TILE_WALL; }

        // Main winding path
        for (int y = 1; y < mapHeight - 1; y++)
        {
            int pathX = 10 + Mathf.RoundToInt(Mathf.Sin(y * 0.3f) * 3);
            pathX = Mathf.Clamp(pathX, 1, mapWidth - 3);
            tileMap[pathX, y] = TILE_PATH;
            tileMap[pathX + 1, y] = TILE_PATH;
        }

        // Lava rivers (water tiles — impassable)
        for (int y = 5; y <= 12; y++) tileMap[3, y] = TILE_WATER;
        for (int y = 8; y <= 15; y++) tileMap[16, y] = TILE_WATER;
        for (int x = 3; x <= 7; x++) tileMap[x, 12] = TILE_WATER;
        for (int x = 12; x <= 16; x++) tileMap[x, 8] = TILE_WATER;

        // Tall grass
        for (int x = 5; x <= 9; x++) for (int y = 3; y <= 7; y++) if (tileMap[x, y] == TILE_SAND) tileMap[x, y] = TILE_TALL_GRASS;
        for (int x = 2; x <= 7; x++) for (int y = 16; y <= 21; y++) if (tileMap[x, y] == TILE_SAND) tileMap[x, y] = TILE_TALL_GRASS;
        for (int x = 12; x <= 17; x++) for (int y = 22; y <= 27; y++) if (tileMap[x, y] == TILE_SAND) tileMap[x, y] = TILE_TALL_GRASS;

        // Rock formations
        for (int x = 1; x <= 2; x++) for (int y = 14; y <= 16; y++) tileMap[x, y] = TILE_WALL;
        for (int x = 17; x <= 18; x++) for (int y = 18; y <= 20; y++) tileMap[x, y] = TILE_WALL;

        // Ledges
        for (int x = 5; x <= 8; x++) tileMap[x, 14] = TILE_LEDGE;
        for (int x = 11; x <= 15; x++) tileMap[x, 20] = TILE_LEDGE;

        // South entry from Roche-Haute
        tileMap[10, 0] = TILE_PATH; tileMap[11, 0] = TILE_PATH;
        // North exit to Volcanville
        tileMap[9, mapHeight - 1] = TILE_PATH; tileMap[10, mapHeight - 1] = TILE_PATH;

        mapTransitions[new Vector2Int(10, 0)] = new MapTransition { targetMap = "ROCHE_HAUTE", spawnX = 12, spawnY = 23 };
        mapTransitions[new Vector2Int(11, 0)] = new MapTransition { targetMap = "ROCHE_HAUTE", spawnX = 13, spawnY = 23 };
        mapTransitions[new Vector2Int(9, mapHeight - 1)] = new MapTransition { targetMap = "VOLCANVILLE", spawnX = 12, spawnY = 1 };
        mapTransitions[new Vector2Int(10, mapHeight - 1)] = new MapTransition { targetMap = "VOLCANVILLE", spawnX = 13, spawnY = 1 };

        // Ranger cabin rest stop (halfway)
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(8, 0, 14), 3, 2, 1,
            new Color(0.90f, 0.35f, 0.35f), new Color(0.50f, 0.30f, 0.20f));
        for (int bx = 8; bx <= 10; bx++) for (int by = 14; by <= 15; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(9, 14)] = new DoorData { targetMap = "DINO_CENTER_R5", spawnX = 3, spawnY = 1, isDinoCenter = true };
        tileMap[9, 14] = TILE_PATH;
        if (tileMap[9, 13] != TILE_PATH) tileMap[9, 13] = TILE_PATH;
        signs[new Vector2Int(8, 14)] = "CABANE DU RANGER\nRepos et soins";

        signs[new Vector2Int(10, 2)] = "ROUTE 5 — Sentier Volcanique\nRoche-Haute <-- --> Volcanville";
        signs[new Vector2Int(9, 27)] = "VOLCANVILLE\nAttention, terrain brulant !";

        SpawnTrainer("Pyromane Lucas", 7, 6, new string[] {
            "Le feu ne me fait pas peur !",
            "Mes dinos adorent la chaleur !"
        }, new Color(0.8f, 0.4f, 0.2f), "TRAINER_ROUTE5_LUCAS",
           new int[] { 75, 49 }, new int[] { 15, 16 });
        SpawnTrainer("Randonneuse Lea", 14, 24, new string[] {
            "Ce chemin volcanique est dangereux...",
            "Mais mes dinos sont prets a tout !"
        }, new Color(0.7f, 0.5f, 0.4f), "TRAINER_ROUTE5_LEA",
           new int[] { 10, 75, 49 }, new int[] { 15, 16, 17 });

        SpawnNPC("Ranger", 10, 16, new string[] {
            "Attention aux rivieres de lave !",
            "Les dinos de type Feu et Terre vivent dans ce climat."
        }, new Color(0.4f, 0.55f, 0.3f));
    }

    // =========================================================================
    //  Volcanville — Volcanic city, Gym 4 (Fire)
    // =========================================================================

    void GenerateVolcanville()
    {
        mapWidth = 25;
        mapHeight = 25;
        tileMap = new int[mapWidth, mapHeight];
        tileObjects = new GameObject[mapWidth, mapHeight];

        // Fill with sand (volcanic ground)
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                tileMap[x, y] = TILE_SAND;

        for (int x = 0; x < mapWidth; x++) { tileMap[x, 0] = TILE_WALL; tileMap[x, mapHeight - 1] = TILE_WALL; }
        for (int y = 0; y < mapHeight; y++) { tileMap[0, y] = TILE_WALL; tileMap[mapWidth - 1, y] = TILE_WALL; }

        // Main roads
        for (int x = 1; x < mapWidth - 1; x++) { tileMap[x, 12] = TILE_PATH; tileMap[x, 13] = TILE_PATH; }
        for (int y = 1; y < mapHeight - 1; y++) { tileMap[12, y] = TILE_PATH; tileMap[13, y] = TILE_PATH; }

        // Town plaza
        for (int x = 10; x <= 15; x++) for (int y = 10; y <= 15; y++) tileMap[x, y] = TILE_PATH;

        // Hot springs (bottom-right)
        for (int x = 17; x <= 21; x++) for (int y = 2; y <= 5; y++) tileMap[x, y] = TILE_WATER;
        for (int x = 16; x <= 22; x++)
            for (int y = 1; y <= 6; y++)
                if (tileMap[x, y] != TILE_WATER && tileMap[x, y] != TILE_WALL)
                    tileMap[x, y] = TILE_SAND;

        // Lava pool (top-left decorative)
        for (int x = 2; x <= 4; x++) for (int y = 20; y <= 22; y++) tileMap[x, y] = TILE_WATER;

        // Green patches near hot springs
        for (int x = 2; x <= 6; x++) for (int y = 2; y <= 5; y++) if (tileMap[x, y] == TILE_SAND) tileMap[x, y] = TILE_GRASS;

        // Decorative boulders
        tileMap[3, 8] = TILE_WALL; tileMap[5, 7] = TILE_WALL;
        tileMap[20, 8] = TILE_WALL; tileMap[22, 9] = TILE_WALL;

        // South entry from Route 5
        tileMap[12, 0] = TILE_PATH; tileMap[13, 0] = TILE_PATH;
        // North exit to Route 6
        tileMap[12, mapHeight - 1] = TILE_PATH; tileMap[13, mapHeight - 1] = TILE_PATH;

        mapTransitions[new Vector2Int(12, 0)] = new MapTransition { targetMap = "ROUTE_5", spawnX = 9, spawnY = 28 };
        mapTransitions[new Vector2Int(13, 0)] = new MapTransition { targetMap = "ROUTE_5", spawnX = 10, spawnY = 28 };
        mapTransitions[new Vector2Int(12, mapHeight - 1)] = new MapTransition { targetMap = "ROUTE_6", spawnX = 10, spawnY = 1 };
        mapTransitions[new Vector2Int(13, mapHeight - 1)] = new MapTransition { targetMap = "ROUTE_6", spawnX = 11, spawnY = 1 };

        // Dino Center
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(2, 0, 10), 5, 3, 2,
            new Color(0.90f, 0.35f, 0.35f), new Color(0.85f, 0.82f, 0.78f));
        for (int bx = 2; bx <= 6; bx++) for (int by = 10; by <= 12; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(4, 10)] = new DoorData { targetMap = "DINO_CENTER_VC", spawnX = 3, spawnY = 1, isDinoCenter = true };
        tileMap[4, 10] = TILE_PATH;
        if (tileMap[4, 9] != TILE_PATH) tileMap[4, 9] = TILE_PATH;
        signs[new Vector2Int(2, 10)] = "CENTRE DINO\nSoins gratuits pour vos dinos";

        // Shop
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(18, 0, 10), 4, 3, 1,
            new Color(0.40f, 0.55f, 0.85f), new Color(0.55f, 0.35f, 0.20f));
        for (int bx = 18; bx <= 21; bx++) for (int by = 10; by <= 12; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(19, 10)] = new DoorData { targetMap = "SHOP_VC", spawnX = 3, spawnY = 1, isShop = true };
        tileMap[19, 10] = TILE_PATH;
        if (tileMap[19, 9] != TILE_PATH) tileMap[19, 9] = TILE_PATH;
        signs[new Vector2Int(18, 10)] = "BOUTIQUE VOLCANIQUE\nPotions anti-brulure en stock !";

        // Gym (fire themed — green base for gym detection)
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(9, 0, 18), 6, 4, 2,
            new Color(0.40f, 0.70f, 0.35f), new Color(0.70f, 0.25f, 0.15f));
        for (int bx = 9; bx <= 14; bx++) for (int by = 18; by <= 21; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(11, 18)] = new DoorData { targetMap = "GYM_VC", spawnX = 4, spawnY = 1, isGym = true, gymId = 3 };
        tileMap[11, 18] = TILE_PATH;
        if (tileMap[11, 17] != TILE_PATH) tileMap[11, 17] = TILE_PATH;
        signs[new Vector2Int(9, 18)] = "ARENE DE VOLCANVILLE\nChampion: BRAZIER\nType: Feu";

        // Forge (unique building)
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(19, 0, 17), 4, 3, 2,
            new Color(0.55f, 0.35f, 0.25f), new Color(0.80f, 0.45f, 0.20f));
        for (int bx = 19; bx <= 22; bx++) for (int by = 17; by <= 19; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(20, 17)] = new DoorData { targetMap = "FORGE_VC", spawnX = 3, spawnY = 1 };
        tileMap[20, 17] = TILE_PATH;
        if (tileMap[20, 16] != TILE_PATH) tileMap[20, 16] = TILE_PATH;
        signs[new Vector2Int(19, 17)] = "LA FORGE\nEquipements forges dans le feu volcanique";

        signs[new Vector2Int(12, 2)] = "VOLCANVILLE\n\"La cite de feu\"";
        signs[new Vector2Int(12, 22)] = "ROUTE 6 -->\nVers Cryo-Cite";

        SpawnNPC("Forgeron", 17, 13, new string[] {
            "Bienvenue a Volcanville !",
            "La chaleur du volcan nous donne une energie incroyable.",
            "Et nos dinos de type Feu adorent ca !"
        }, new Color(0.7f, 0.4f, 0.2f));
        SpawnNPC("Thermaliste", 19, 4, new string[] {
            "Les sources chaudes de Volcanville sont connues dans tout le monde.",
            "Meme les dinos viennent s'y baigner !"
        }, new Color(0.5f, 0.6f, 0.7f));
        SpawnNPC("Dresseuse", 8, 8, new string[] {
            "Brazier, le champion, est un maitre du type Feu.",
            "Ses dinos brulent d'une puissance incroyable !",
            "Un dino de type Eau ou Terre serait ton meilleur atout."
        }, new Color(0.8f, 0.5f, 0.6f));
        SpawnNPC("Enfant", 15, 7, new string[] {
            "J'ai vu un dino de lave sortir du volcan hier !",
            "Il etait enorme et tout rouge !"
        }, new Color(0.9f, 0.6f, 0.4f));

        // --- Story NPCs ---
        SpawnNPC("Ancien du village", 4, 12, new string[] {
            "Il existe une legende ici a Volcanville...",
            "On dit qu'un Dinomonstre ancestral, Primordius, dort sous le volcan.",
            "La Team Meteore serait a sa recherche. Ca m'inquiete beaucoup."
        }, new Color(0.6f, 0.5f, 0.4f));

        SpawnNPC("Vulcanologue", 22, 10, new string[] {
            "Les emissions du volcan ont augmente recemment.",
            "Brazier utilise 3 dinos Feu a l'arene. Le type Eau les ecrase !",
            "Un dino Terre resiste aussi bien aux attaques Feu."
        }, new Color(0.5f, 0.4f, 0.5f));

        SpawnNPC("Randonneuse", 12, 20, new string[] {
            "Tu as combien de badges ? La suite du voyage est longue !",
            "Apres Volcanville, la temperature chute : Cryo-Cite est gelante.",
            "Prepare des dinos de type Feu ou Combat pour la championne de Glace."
        }, new Color(0.7f, 0.5f, 0.5f));
    }

    // =========================================================================
    //  Route 6 — Icy path, Volcanville to Cryo-Cite
    // =========================================================================

    void GenerateRoute6()
    {
        mapWidth = 20;
        mapHeight = 30;
        tileMap = new int[mapWidth, mapHeight];
        tileObjects = new GameObject[mapWidth, mapHeight];

        // Fill with grass (will overlay with white/light blue)
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                tileMap[x, y] = TILE_GRASS;

        for (int x = 0; x < mapWidth; x++) { tileMap[x, 0] = TILE_WALL; tileMap[x, mapHeight - 1] = TILE_WALL; }
        for (int y = 0; y < mapHeight; y++) { tileMap[0, y] = TILE_WALL; tileMap[mapWidth - 1, y] = TILE_WALL; }

        // Main winding path
        for (int y = 1; y < mapHeight - 1; y++)
        {
            int pathX = 9 + Mathf.RoundToInt(Mathf.Sin(y * 0.25f) * 3);
            pathX = Mathf.Clamp(pathX, 1, mapWidth - 3);
            tileMap[pathX, y] = TILE_PATH;
            tileMap[pathX + 1, y] = TILE_PATH;
        }

        // Frozen ponds (water tiles)
        for (int x = 2; x <= 5; x++) for (int y = 6; y <= 9; y++) tileMap[x, y] = TILE_WATER;
        for (int x = 13; x <= 17; x++) for (int y = 18; y <= 21; y++) tileMap[x, y] = TILE_WATER;

        // Snow banks (sand tiles representing snow)
        for (int x = 1; x <= 6; x++) for (int y = 3; y <= 5; y++) if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_SAND;
        for (int x = 12; x <= 18; x++) for (int y = 14; y <= 17; y++) if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_SAND;
        for (int x = 2; x <= 8; x++) for (int y = 22; y <= 26; y++) if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_SAND;

        // Tall grass (icy encounters)
        for (int x = 2; x <= 6; x++) for (int y = 11; y <= 16; y++) if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_TALL_GRASS;
        for (int x = 12; x <= 17; x++) for (int y = 5; y <= 11; y++) if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_TALL_GRASS;
        for (int x = 10; x <= 16; x++) for (int y = 23; y <= 27; y++) if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_TALL_GRASS;

        // Ice boulder walls
        for (int x = 1; x <= 3; x++) tileMap[x, 18] = TILE_WALL;
        for (int x = 15; x <= 18; x++) tileMap[x, 12] = TILE_WALL;

        // Ledges
        for (int x = 4; x <= 8; x++) tileMap[x, 10] = TILE_LEDGE;
        for (int x = 11; x <= 15; x++) tileMap[x, 22] = TILE_LEDGE;

        // South entry from Volcanville
        tileMap[10, 0] = TILE_PATH; tileMap[11, 0] = TILE_PATH;
        // North exit to Cryo-Cite
        tileMap[9, mapHeight - 1] = TILE_PATH; tileMap[10, mapHeight - 1] = TILE_PATH;

        mapTransitions[new Vector2Int(10, 0)] = new MapTransition { targetMap = "VOLCANVILLE", spawnX = 12, spawnY = 23 };
        mapTransitions[new Vector2Int(11, 0)] = new MapTransition { targetMap = "VOLCANVILLE", spawnX = 13, spawnY = 23 };
        mapTransitions[new Vector2Int(9, mapHeight - 1)] = new MapTransition { targetMap = "CRYO_CITE", spawnX = 12, spawnY = 1 };
        mapTransitions[new Vector2Int(10, mapHeight - 1)] = new MapTransition { targetMap = "CRYO_CITE", spawnX = 13, spawnY = 1 };

        signs[new Vector2Int(10, 2)] = "ROUTE 6 — Sentier Glace\nVolcanville <-- --> Cryo-Cite";
        signs[new Vector2Int(9, 27)] = "CRYO-CITE\nAttention, terrain glissant !";

        SpawnTrainer("Skieuse Nora", 6, 13, new string[] {
            "Le froid ne me fait pas peur !",
            "Mes dinos de glace sont imbattables !"
        }, new Color(0.7f, 0.8f, 0.9f), "TRAINER_ROUTE6_NORA",
           new int[] { 63, 65 }, new int[] { 19, 20 });

        SpawnTrainer("Patineur Yuri", 14, 24, new string[] {
            "Je glisse sur la glace avec mes dinos !",
            "Essaie de nous suivre !"
        }, new Color(0.5f, 0.6f, 0.8f), "TRAINER_ROUTE6_YURI",
           new int[] { 64, 66, 70 }, new int[] { 19, 20, 21 });

        SpawnNPC("Alpiniste", 8, 20, new string[] {
            "Le blizzard peut se lever a tout moment ici.",
            "Les dinos de type Glace adorent ce climat !"
        }, new Color(0.6f, 0.65f, 0.7f));

        AddRouteTrees(0.06f);
    }

    // =========================================================================
    //  Cryo-Cite — Frozen city, Gym 5 (Ice)
    // =========================================================================

    void GenerateCryoCite()
    {
        mapWidth = 25;
        mapHeight = 25;
        tileMap = new int[mapWidth, mapHeight];
        tileObjects = new GameObject[mapWidth, mapHeight];

        // Fill with sand (snow ground)
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                tileMap[x, y] = TILE_SAND;

        for (int x = 0; x < mapWidth; x++) { tileMap[x, 0] = TILE_WALL; tileMap[x, mapHeight - 1] = TILE_WALL; }
        for (int y = 0; y < mapHeight; y++) { tileMap[0, y] = TILE_WALL; tileMap[mapWidth - 1, y] = TILE_WALL; }

        // Main roads
        for (int x = 1; x < mapWidth - 1; x++) { tileMap[x, 12] = TILE_PATH; tileMap[x, 13] = TILE_PATH; }
        for (int y = 1; y < mapHeight - 1; y++) { tileMap[12, y] = TILE_PATH; tileMap[13, y] = TILE_PATH; }

        // Town plaza
        for (int x = 10; x <= 15; x++) for (int y = 10; y <= 15; y++) tileMap[x, y] = TILE_PATH;

        // Frozen pond (center-left)
        for (int x = 3; x <= 7; x++) for (int y = 3; y <= 6; y++) tileMap[x, y] = TILE_WATER;
        // Ice border around pond
        for (int x = 2; x <= 8; x++)
            for (int y = 2; y <= 7; y++)
                if (tileMap[x, y] != TILE_WATER && tileMap[x, y] != TILE_WALL)
                    tileMap[x, y] = TILE_SAND;

        // Small grass patches (hardy plants)
        for (int x = 19; x <= 22; x++) for (int y = 2; y <= 4; y++) if (tileMap[x, y] == TILE_SAND) tileMap[x, y] = TILE_GRASS;

        // South entry from Route 6
        tileMap[12, 0] = TILE_PATH; tileMap[13, 0] = TILE_PATH;
        // North exit to Route 7
        tileMap[12, mapHeight - 1] = TILE_PATH; tileMap[13, mapHeight - 1] = TILE_PATH;

        mapTransitions[new Vector2Int(12, 0)] = new MapTransition { targetMap = "ROUTE_6", spawnX = 9, spawnY = 28 };
        mapTransitions[new Vector2Int(13, 0)] = new MapTransition { targetMap = "ROUTE_6", spawnX = 10, spawnY = 28 };
        mapTransitions[new Vector2Int(12, mapHeight - 1)] = new MapTransition { targetMap = "ROUTE_7", spawnX = 10, spawnY = 1 };
        mapTransitions[new Vector2Int(13, mapHeight - 1)] = new MapTransition { targetMap = "ROUTE_7", spawnX = 11, spawnY = 1 };

        // Dino Center
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(2, 0, 10), 5, 3, 2,
            new Color(0.90f, 0.35f, 0.35f), new Color(0.85f, 0.82f, 0.78f));
        for (int bx = 2; bx <= 6; bx++) for (int by = 10; by <= 12; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(4, 10)] = new DoorData { targetMap = "DINO_CENTER_CC", spawnX = 3, spawnY = 1, isDinoCenter = true };
        tileMap[4, 10] = TILE_PATH;
        if (tileMap[4, 9] != TILE_PATH) tileMap[4, 9] = TILE_PATH;
        signs[new Vector2Int(2, 10)] = "CENTRE DINO\nSoins gratuits pour vos dinos";

        // Shop
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(18, 0, 10), 4, 3, 1,
            new Color(0.40f, 0.55f, 0.85f), new Color(0.50f, 0.60f, 0.75f));
        for (int bx = 18; bx <= 21; bx++) for (int by = 10; by <= 12; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(19, 10)] = new DoorData { targetMap = "SHOP_CC", spawnX = 3, spawnY = 1, isShop = true };
        tileMap[19, 10] = TILE_PATH;
        if (tileMap[19, 9] != TILE_PATH) tileMap[19, 9] = TILE_PATH;
        signs[new Vector2Int(18, 10)] = "BOUTIQUE GIVRE\nAnti-gel et potions chaudes";

        // Gym (ice themed — green base for gym detection)
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(9, 0, 18), 6, 4, 2,
            new Color(0.40f, 0.70f, 0.35f), new Color(0.55f, 0.72f, 0.85f));
        for (int bx = 9; bx <= 14; bx++) for (int by = 18; by <= 21; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(11, 18)] = new DoorData { targetMap = "GYM_CC", spawnX = 4, spawnY = 1, isGym = true, gymId = 4 };
        tileMap[11, 18] = TILE_PATH;
        if (tileMap[11, 17] != TILE_PATH) tileMap[11, 17] = TILE_PATH;
        signs[new Vector2Int(9, 18)] = "ARENE DE CRYO-CITE\nChampion: GIVRALIA\nType: Glace";

        // Research Lab
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(19, 0, 17), 4, 3, 2,
            new Color(0.80f, 0.85f, 0.90f), new Color(0.60f, 0.65f, 0.75f));
        for (int bx = 19; bx <= 22; bx++) for (int by = 17; by <= 19; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(20, 17)] = new DoorData { targetMap = "LAB_CC", spawnX = 3, spawnY = 1 };
        tileMap[20, 17] = TILE_PATH;
        if (tileMap[20, 16] != TILE_PATH) tileMap[20, 16] = TILE_PATH;
        signs[new Vector2Int(19, 17)] = "LABORATOIRE CRYO\nRecherche sur les dinos de glace";

        signs[new Vector2Int(12, 2)] = "CRYO-CITE\n\"La cite des neiges eternelles\"";
        signs[new Vector2Int(12, 22)] = "ROUTE 7 -->\nVers Electropolis";

        SpawnNPC("Habitant", 16, 12, new string[] {
            "Bienvenue a Cryo-Cite !",
            "N'oublie pas ton manteau, il fait toujours froid ici."
        }, new Color(0.6f, 0.7f, 0.8f));
        SpawnNPC("Chercheuse", 20, 15, new string[] {
            "Givralia est la championne la plus glaciale de la region !",
            "Ses dinos de type Glace sont redoutables.",
            "Un dino de type Feu ou Combat serait ton meilleur atout."
        }, new Color(0.5f, 0.6f, 0.7f));
        SpawnNPC("Enfant", 8, 5, new string[] {
            "Le lac est completement gele !",
            "Parfois on voit des dinos glisser dessus."
        }, new Color(0.8f, 0.7f, 0.9f));
        SpawnNPC("Vieux Sage", 5, 13, new string[] {
            "Les dinos de glace sont beaux mais dangereux.",
            "Givralia les maitrise comme personne."
        }, new Color(0.5f, 0.5f, 0.5f));

        // --- Story NPCs ---
        SpawnNPC("Explorateur polaire", 15, 10, new string[] {
            "Givralia a 3 dinos Glace. Le Feu les fait fondre !",
            "Le type Combat est aussi super efficace contre la Glace.",
            "Fais attention aux attaques Blizzard, elles touchent toute l'equipe !"
        }, new Color(0.4f, 0.5f, 0.7f));

        SpawnNPC("Bergere", 18, 8, new string[] {
            "On dit que la Team Meteore a une base secrete dans les marais, au sud-est.",
            "Ils capturent des dinos pour un projet secret...",
            "Si tu as assez de badges, tu devrais aller enqueter."
        }, new Color(0.6f, 0.6f, 0.7f));

        SpawnNPC("Cristallographe", 10, 18, new string[] {
            "Les cristaux de glace de cette region sont uniques.",
            "Certains dinos Glace peuvent creer des tempetes de neige !",
            "Avec assez de badges, tu auras acces a des routes plus dangereuses."
        }, new Color(0.5f, 0.6f, 0.8f));
    }

    // =========================================================================
    //  Route 7 — Electric plains, Cryo-Cite to Electropolis
    // =========================================================================

    void GenerateRoute7()
    {
        mapWidth = 20;
        mapHeight = 30;
        tileMap = new int[mapWidth, mapHeight];
        tileObjects = new GameObject[mapWidth, mapHeight];

        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                tileMap[x, y] = TILE_GRASS;

        for (int x = 0; x < mapWidth; x++) { tileMap[x, 0] = TILE_WALL; tileMap[x, mapHeight - 1] = TILE_WALL; }
        for (int y = 0; y < mapHeight; y++) { tileMap[0, y] = TILE_WALL; tileMap[mapWidth - 1, y] = TILE_WALL; }

        // Main winding path
        for (int y = 1; y < mapHeight - 1; y++)
        {
            int pathX = 9 + Mathf.RoundToInt(Mathf.Sin(y * 0.2f) * 3);
            pathX = Mathf.Clamp(pathX, 1, mapWidth - 3);
            tileMap[pathX, y] = TILE_PATH;
            tileMap[pathX + 1, y] = TILE_PATH;
        }

        // Sandy plains areas
        for (int x = 1; x <= 6; x++) for (int y = 3; y <= 8; y++) if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_SAND;
        for (int x = 13; x <= 18; x++) for (int y = 15; y <= 20; y++) if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_SAND;

        // Tall grass patches
        for (int x = 2; x <= 7; x++) for (int y = 10; y <= 16; y++) if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_TALL_GRASS;
        for (int x = 11; x <= 17; x++) for (int y = 4; y <= 10; y++) if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_TALL_GRASS;
        for (int x = 5; x <= 12; x++) for (int y = 22; y <= 27; y++) if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_TALL_GRASS;

        // Metal rock formations (walls)
        for (int x = 1; x <= 3; x++) tileMap[x, 20] = TILE_WALL;
        for (int x = 16; x <= 18; x++) tileMap[x, 10] = TILE_WALL;
        tileMap[5, 18] = TILE_WALL; tileMap[14, 22] = TILE_WALL;

        // Ledges
        for (int x = 3; x <= 7; x++) tileMap[x, 9] = TILE_LEDGE;
        for (int x = 12; x <= 16; x++) tileMap[x, 21] = TILE_LEDGE;

        // Mountain refuge rest stop (Dino Center)
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(8, 0, 14), 3, 2, 1,
            new Color(0.90f, 0.35f, 0.35f), new Color(0.85f, 0.82f, 0.78f));
        for (int bx = 8; bx <= 10; bx++) for (int by = 14; by <= 15; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(9, 14)] = new DoorData { targetMap = "DINO_CENTER_R7", spawnX = 3, spawnY = 1, isDinoCenter = true };
        tileMap[9, 14] = TILE_PATH;
        if (tileMap[9, 13] != TILE_PATH) tileMap[9, 13] = TILE_PATH;
        signs[new Vector2Int(8, 14)] = "REFUGE DE MONTAGNE\nRepos et soins";

        // South entry from Cryo-Cite
        tileMap[10, 0] = TILE_PATH; tileMap[11, 0] = TILE_PATH;
        // North exit to Electropolis
        tileMap[9, mapHeight - 1] = TILE_PATH; tileMap[10, mapHeight - 1] = TILE_PATH;

        mapTransitions[new Vector2Int(10, 0)] = new MapTransition { targetMap = "CRYO_CITE", spawnX = 12, spawnY = 23 };
        mapTransitions[new Vector2Int(11, 0)] = new MapTransition { targetMap = "CRYO_CITE", spawnX = 13, spawnY = 23 };
        mapTransitions[new Vector2Int(9, mapHeight - 1)] = new MapTransition { targetMap = "ELECTROPOLIS", spawnX = 12, spawnY = 1 };
        mapTransitions[new Vector2Int(10, mapHeight - 1)] = new MapTransition { targetMap = "ELECTROPOLIS", spawnX = 13, spawnY = 1 };

        signs[new Vector2Int(10, 2)] = "ROUTE 7 — Plaines Electriques\nCryo-Cite <-- --> Electropolis";
        signs[new Vector2Int(9, 27)] = "ELECTROPOLIS\nTout droit !";

        SpawnTrainer("Technicien Max", 6, 12, new string[] {
            "L'electricite est ma passion !",
            "Mes dinos vont te paralyser !"
        }, new Color(0.8f, 0.8f, 0.3f), "TRAINER_ROUTE7_MAX",
           new int[] { 71, 73 }, new int[] { 23, 24 });

        SpawnTrainer("Ingenieure Zoe", 14, 7, new string[] {
            "Les dinos Metal sont les plus solides !",
            "Tu ne pourras pas les percer !"
        }, new Color(0.6f, 0.6f, 0.7f), "TRAINER_ROUTE7_ZOE",
           new int[] { 74, 76, 72 }, new int[] { 23, 24, 25 });

        SpawnTrainer("Randonneur Eli", 8, 25, new string[] {
            "Cette route est pleine de surprises !",
            "Mes dinos sont charges a bloc !"
        }, new Color(0.5f, 0.6f, 0.4f), "TRAINER_ROUTE7_ELI",
           new int[] { 75, 71 }, new int[] { 24, 25 });

        SpawnNPC("Berger", 12, 18, new string[] {
            "L'air est electrique par ici...",
            "Les dinos de type Electrique et Metal rodent dans les plaines."
        }, new Color(0.5f, 0.55f, 0.4f));

        AddRouteTrees(0.08f);
    }

    // =========================================================================
    //  Electropolis — High-tech city, Gym 6 (Electric)
    // =========================================================================

    void GenerateElectropolis()
    {
        mapWidth = 25;
        mapHeight = 25;
        tileMap = new int[mapWidth, mapHeight];
        tileObjects = new GameObject[mapWidth, mapHeight];

        // Fill with grass
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                tileMap[x, y] = TILE_GRASS;

        for (int x = 0; x < mapWidth; x++) { tileMap[x, 0] = TILE_WALL; tileMap[x, mapHeight - 1] = TILE_WALL; }
        for (int y = 0; y < mapHeight; y++) { tileMap[0, y] = TILE_WALL; tileMap[mapWidth - 1, y] = TILE_WALL; }

        // Main roads (wide urban grid)
        for (int x = 1; x < mapWidth - 1; x++) { tileMap[x, 12] = TILE_PATH; tileMap[x, 13] = TILE_PATH; }
        for (int y = 1; y < mapHeight - 1; y++) { tileMap[12, y] = TILE_PATH; tileMap[13, y] = TILE_PATH; }
        // Secondary horizontal road
        for (int x = 1; x < mapWidth - 1; x++) { tileMap[x, 7] = TILE_PATH; }
        for (int x = 1; x < mapWidth - 1; x++) { tileMap[x, 18] = TILE_PATH; }

        // Town plaza (larger for urban feel)
        for (int x = 9; x <= 16; x++) for (int y = 10; y <= 15; y++) tileMap[x, y] = TILE_PATH;

        // Decorative fountain
        tileMap[12, 12] = TILE_WATER; tileMap[13, 12] = TILE_WATER;
        tileMap[12, 13] = TILE_WATER; tileMap[13, 13] = TILE_WATER;

        // Sand around fountain
        for (int x = 11; x <= 14; x++)
            for (int y = 11; y <= 14; y++)
                if (tileMap[x, y] != TILE_WATER) tileMap[x, y] = TILE_SAND;

        // Small park areas
        for (int x = 2; x <= 5; x++) for (int y = 2; y <= 5; y++) if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_TALL_GRASS;
        for (int x = 19; x <= 22; x++) for (int y = 20; y <= 22; y++) if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_TALL_GRASS;

        // South entry from Route 7
        tileMap[12, 0] = TILE_PATH; tileMap[13, 0] = TILE_PATH;
        // North exit to Route 8
        tileMap[12, mapHeight - 1] = TILE_PATH; tileMap[13, mapHeight - 1] = TILE_PATH;

        mapTransitions[new Vector2Int(12, 0)] = new MapTransition { targetMap = "ROUTE_7", spawnX = 9, spawnY = 28 };
        mapTransitions[new Vector2Int(13, 0)] = new MapTransition { targetMap = "ROUTE_7", spawnX = 10, spawnY = 28 };
        mapTransitions[new Vector2Int(12, mapHeight - 1)] = new MapTransition { targetMap = "ROUTE_8", spawnX = 10, spawnY = 1 };
        mapTransitions[new Vector2Int(13, mapHeight - 1)] = new MapTransition { targetMap = "ROUTE_8", spawnX = 11, spawnY = 1 };

        // Dino Center
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(2, 0, 10), 5, 3, 2,
            new Color(0.90f, 0.35f, 0.35f), new Color(0.85f, 0.82f, 0.78f));
        for (int bx = 2; bx <= 6; bx++) for (int by = 10; by <= 12; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(4, 10)] = new DoorData { targetMap = "DINO_CENTER_EP", spawnX = 3, spawnY = 1, isDinoCenter = true };
        tileMap[4, 10] = TILE_PATH;
        if (tileMap[4, 9] != TILE_PATH) tileMap[4, 9] = TILE_PATH;
        signs[new Vector2Int(2, 10)] = "CENTRE DINO\nSoins gratuits pour vos dinos";

        // Shop
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(18, 0, 10), 4, 3, 1,
            new Color(0.40f, 0.55f, 0.85f), new Color(0.60f, 0.60f, 0.20f));
        for (int bx = 18; bx <= 21; bx++) for (int by = 10; by <= 12; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(19, 10)] = new DoorData { targetMap = "SHOP_EP", spawnX = 3, spawnY = 1, isShop = true };
        tileMap[19, 10] = TILE_PATH;
        if (tileMap[19, 9] != TILE_PATH) tileMap[19, 9] = TILE_PATH;
        signs[new Vector2Int(18, 10)] = "MEGA BOUTIQUE\nTechnologie et potions";

        // Gym (electric themed — green base for gym detection)
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(9, 0, 18), 6, 4, 2,
            new Color(0.40f, 0.70f, 0.35f), new Color(0.75f, 0.70f, 0.20f));
        for (int bx = 9; bx <= 14; bx++) for (int by = 18; by <= 21; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(11, 18)] = new DoorData { targetMap = "GYM_EP", spawnX = 4, spawnY = 1, isGym = true, gymId = 5 };
        tileMap[11, 18] = TILE_PATH;
        if (tileMap[11, 17] != TILE_PATH) tileMap[11, 17] = TILE_PATH;
        signs[new Vector2Int(9, 18)] = "ARENE D'ELECTROPOLIS\nChampion: VOLTAIRE\nType: Electrique";

        // Game Corner
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(19, 0, 17), 4, 3, 2,
            new Color(0.85f, 0.30f, 0.60f), new Color(0.70f, 0.20f, 0.50f));
        for (int bx = 19; bx <= 22; bx++) for (int by = 17; by <= 19; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(20, 17)] = new DoorData { targetMap = "GAME_CORNER_EP", spawnX = 3, spawnY = 1 };
        tileMap[20, 17] = TILE_PATH;
        if (tileMap[20, 16] != TILE_PATH) tileMap[20, 16] = TILE_PATH;
        signs[new Vector2Int(19, 17)] = "COIN DES JEUX\nTentez votre chance !";

        // House 1
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(2, 0, 3), 3, 3, 1,
            new Color(0.80f, 0.75f, 0.65f), new Color(0.55f, 0.55f, 0.50f));
        for (int bx = 2; bx <= 4; bx++) for (int by = 3; by <= 5; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(3, 3)] = new DoorData { targetMap = "HOUSE_EP_1", spawnX = 3, spawnY = 1 };
        tileMap[3, 3] = TILE_PATH;
        if (tileMap[3, 2] != TILE_PATH) tileMap[3, 2] = TILE_PATH;

        // House 2
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(8, 0, 3), 3, 3, 1,
            new Color(0.80f, 0.75f, 0.65f), new Color(0.50f, 0.52f, 0.55f));
        for (int bx = 8; bx <= 10; bx++) for (int by = 3; by <= 5; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(9, 3)] = new DoorData { targetMap = "HOUSE_EP_2", spawnX = 3, spawnY = 1 };
        tileMap[9, 3] = TILE_PATH;
        if (tileMap[9, 2] != TILE_PATH) tileMap[9, 2] = TILE_PATH;

        // House 3
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(19, 0, 3), 3, 3, 1,
            new Color(0.80f, 0.75f, 0.65f), new Color(0.52f, 0.50f, 0.55f));
        for (int bx = 19; bx <= 21; bx++) for (int by = 3; by <= 5; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(20, 3)] = new DoorData { targetMap = "HOUSE_EP_3", spawnX = 3, spawnY = 1 };
        tileMap[20, 3] = TILE_PATH;
        if (tileMap[20, 2] != TILE_PATH) tileMap[20, 2] = TILE_PATH;

        signs[new Vector2Int(12, 2)] = "ELECTROPOLIS\n\"La cite de la foudre\"";
        signs[new Vector2Int(12, 22)] = "ROUTE 8 -->\nVers Marais-Noir";

        SpawnNPC("Technicien", 16, 12, new string[] {
            "Bienvenue a Electropolis !",
            "La ville la plus moderne de la region.",
            "Tout fonctionne a l'energie electrique ici !"
        }, new Color(0.7f, 0.7f, 0.3f));
        SpawnNPC("Joueuse", 21, 15, new string[] {
            "Le Coin des Jeux est trop cool !",
            "On peut y gagner des dinos rares !"
        }, new Color(0.9f, 0.5f, 0.7f));
        SpawnNPC("Scientifique", 8, 8, new string[] {
            "Voltaire, le champion, maitrise l'electricite.",
            "Ses dinos paralysent en un eclair !",
            "Un dino de type Terre resisterait a ses attaques."
        }, new Color(0.5f, 0.5f, 0.6f));
        SpawnNPC("Garcon", 15, 7, new string[] {
            "Electropolis ne dort jamais !",
            "Les lumieres brillent meme la nuit."
        }, new Color(0.6f, 0.7f, 0.5f));

        // --- Story NPCs ---
        SpawnNPC("Ingenieure", 4, 10, new string[] {
            "Voltaire est redoutable ! Ses dinos Electrique paralysent souvent.",
            "Le type Terre est immunise contre les attaques Electrique.",
            "Si tu as un dino Terre, l'arene sera beaucoup plus facile !"
        }, new Color(0.6f, 0.6f, 0.4f));

        SpawnNPC("Informateur", 20, 10, new string[] {
            "Tu as entendu parler de la Team Meteore ?",
            "Leur base serait cachee dans les Marais-Noir, plus au sud.",
            "Ils planifient quelque chose d'enorme... Sois prudent si tu y vas."
        }, new Color(0.4f, 0.4f, 0.5f));

        SpawnNPC("Fan de technologie", 12, 18, new string[] {
            "La technologie d'Electropolis est la plus avancee de la region !",
            "Nos machines peuvent meme ressusciter des fossiles de dinos !",
            "Si tu trouves un fossile, apporte-le au laboratoire central."
        }, new Color(0.5f, 0.6f, 0.3f));
    }

    // =========================================================================
    //  Route 8 — Toxic swamp, Electropolis to Marais-Noir
    // =========================================================================

    void GenerateRoute8()
    {
        mapWidth = 20;
        mapHeight = 30;
        tileMap = new int[mapWidth, mapHeight];
        tileObjects = new GameObject[mapWidth, mapHeight];

        // Fill with grass
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                tileMap[x, y] = TILE_GRASS;

        for (int x = 0; x < mapWidth; x++) { tileMap[x, 0] = TILE_WALL; tileMap[x, mapHeight - 1] = TILE_WALL; }
        for (int y = 0; y < mapHeight; y++) { tileMap[0, y] = TILE_WALL; tileMap[mapWidth - 1, y] = TILE_WALL; }

        // Main narrow path winding through swamp
        for (int y = 1; y < mapHeight - 1; y++)
        {
            int pathX = 10 + Mathf.RoundToInt(Mathf.Sin(y * 0.35f) * 3);
            pathX = Mathf.Clamp(pathX, 1, mapWidth - 3);
            tileMap[pathX, y] = TILE_PATH;
            tileMap[pathX + 1, y] = TILE_PATH;
        }

        // Swamp water (lots of water tiles)
        for (int x = 1; x <= 6; x++) for (int y = 3; y <= 8; y++) tileMap[x, y] = TILE_WATER;
        for (int x = 13; x <= 18; x++) for (int y = 5; y <= 10; y++) tileMap[x, y] = TILE_WATER;
        for (int x = 1; x <= 5; x++) for (int y = 14; y <= 18; y++) tileMap[x, y] = TILE_WATER;
        for (int x = 14; x <= 18; x++) for (int y = 17; y <= 22; y++) tileMap[x, y] = TILE_WATER;
        for (int x = 2; x <= 7; x++) for (int y = 24; y <= 27; y++) tileMap[x, y] = TILE_WATER;

        // Narrow land bridges through swamp
        for (int y = 3; y <= 8; y++) tileMap[4, y] = TILE_PATH;
        for (int y = 14; y <= 18; y++) tileMap[3, y] = TILE_PATH;

        // Tall grass on remaining dry areas
        for (int x = 7; x <= 11; x++) for (int y = 4; y <= 9; y++) if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_TALL_GRASS;
        for (int x = 6; x <= 12; x++) for (int y = 14; y <= 20; y++) if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_TALL_GRASS;
        for (int x = 8; x <= 14; x++) for (int y = 23; y <= 27; y++) if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_TALL_GRASS;

        // Dead tree walls
        tileMap[3, 10] = TILE_WALL; tileMap[7, 12] = TILE_WALL;
        tileMap[16, 14] = TILE_WALL; tileMap[12, 22] = TILE_WALL;

        // South entry from Electropolis
        tileMap[10, 0] = TILE_PATH; tileMap[11, 0] = TILE_PATH;
        // North exit to Marais-Noir
        tileMap[9, mapHeight - 1] = TILE_PATH; tileMap[10, mapHeight - 1] = TILE_PATH;

        mapTransitions[new Vector2Int(10, 0)] = new MapTransition { targetMap = "ELECTROPOLIS", spawnX = 12, spawnY = 23 };
        mapTransitions[new Vector2Int(11, 0)] = new MapTransition { targetMap = "ELECTROPOLIS", spawnX = 13, spawnY = 23 };
        mapTransitions[new Vector2Int(9, mapHeight - 1)] = new MapTransition { targetMap = "MARAIS_NOIR", spawnX = 12, spawnY = 1 };
        mapTransitions[new Vector2Int(10, mapHeight - 1)] = new MapTransition { targetMap = "MARAIS_NOIR", spawnX = 13, spawnY = 1 };

        signs[new Vector2Int(10, 2)] = "ROUTE 8 — Marecage Toxique\nElectropolis <-- --> Marais-Noir";
        signs[new Vector2Int(9, 27)] = "MARAIS-NOIR\nAttention, terrain toxique !";

        SpawnTrainer("Sorciere Morgane", 8, 7, new string[] {
            "Le marais cache des secrets sombres...",
            "Mes dinos venimeux vont te surprendre !"
        }, new Color(0.5f, 0.3f, 0.6f), "TRAINER_ROUTE8_MORGANE",
           new int[] { 79, 81 }, new int[] { 27, 28 });

        SpawnTrainer("Toxicologue Remy", 13, 19, new string[] {
            "J'etudie les poisons naturels du marais.",
            "Mes dinos ont absorbe toute cette toxicite !"
        }, new Color(0.4f, 0.5f, 0.3f), "TRAINER_ROUTE8_REMY",
           new int[] { 80, 82, 83 }, new int[] { 27, 28, 29 });

        SpawnNPC("Ermite", 6, 16, new string[] {
            "Ce marais est dangereux, ne quitte pas le chemin !",
            "Les dinos Venin et Ombre rodent dans les eaux sombres."
        }, new Color(0.4f, 0.4f, 0.35f));

        AddRouteTrees(0.05f);
    }

    // =========================================================================
    //  Marais-Noir — Dark swamp town, Gym 7 (Venom)
    // =========================================================================

    void GenerateMaraisNoir()
    {
        mapWidth = 25;
        mapHeight = 25;
        tileMap = new int[mapWidth, mapHeight];
        tileObjects = new GameObject[mapWidth, mapHeight];

        // Fill with grass (dark/swampy ground)
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                tileMap[x, y] = TILE_GRASS;

        for (int x = 0; x < mapWidth; x++) { tileMap[x, 0] = TILE_WALL; tileMap[x, mapHeight - 1] = TILE_WALL; }
        for (int y = 0; y < mapHeight; y++) { tileMap[0, y] = TILE_WALL; tileMap[mapWidth - 1, y] = TILE_WALL; }

        // Main roads
        for (int x = 1; x < mapWidth - 1; x++) { tileMap[x, 12] = TILE_PATH; tileMap[x, 13] = TILE_PATH; }
        for (int y = 1; y < mapHeight - 1; y++) { tileMap[12, y] = TILE_PATH; tileMap[13, y] = TILE_PATH; }

        // Town plaza
        for (int x = 10; x <= 15; x++) for (int y = 10; y <= 15; y++) tileMap[x, y] = TILE_PATH;

        // Toxic pools (water tiles)
        for (int x = 2; x <= 5; x++) for (int y = 2; y <= 4; y++) tileMap[x, y] = TILE_WATER;
        for (int x = 19; x <= 22; x++) for (int y = 2; y <= 4; y++) tileMap[x, y] = TILE_WATER;
        for (int x = 3; x <= 6; x++) for (int y = 20; y <= 22; y++) tileMap[x, y] = TILE_WATER;

        // Swamp border sand
        for (int x = 1; x <= 6; x++)
            for (int y = 1; y <= 5; y++)
                if (tileMap[x, y] != TILE_WATER && tileMap[x, y] != TILE_WALL)
                    tileMap[x, y] = TILE_SAND;
        for (int x = 18; x <= 23; x++)
            for (int y = 1; y <= 5; y++)
                if (tileMap[x, y] != TILE_WATER && tileMap[x, y] != TILE_WALL)
                    tileMap[x, y] = TILE_SAND;

        // Tall grass patches (toxic plants)
        for (int x = 2; x <= 6; x++) for (int y = 7; y <= 9; y++) if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_TALL_GRASS;
        for (int x = 18; x <= 22; x++) for (int y = 7; y <= 9; y++) if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_TALL_GRASS;

        // Dead tree walls
        tileMap[7, 5] = TILE_WALL; tileMap[17, 6] = TILE_WALL;
        tileMap[4, 16] = TILE_WALL; tileMap[20, 16] = TILE_WALL;

        // South entry from Route 8
        tileMap[12, 0] = TILE_PATH; tileMap[13, 0] = TILE_PATH;
        // North exit to Route 9
        tileMap[12, mapHeight - 1] = TILE_PATH; tileMap[13, mapHeight - 1] = TILE_PATH;

        mapTransitions[new Vector2Int(12, 0)] = new MapTransition { targetMap = "ROUTE_8", spawnX = 9, spawnY = 28 };
        mapTransitions[new Vector2Int(13, 0)] = new MapTransition { targetMap = "ROUTE_8", spawnX = 10, spawnY = 28 };
        mapTransitions[new Vector2Int(12, mapHeight - 1)] = new MapTransition { targetMap = "ROUTE_9", spawnX = 10, spawnY = 1 };
        mapTransitions[new Vector2Int(13, mapHeight - 1)] = new MapTransition { targetMap = "ROUTE_9", spawnX = 11, spawnY = 1 };

        // Dino Center
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(2, 0, 10), 5, 3, 2,
            new Color(0.90f, 0.35f, 0.35f), new Color(0.85f, 0.82f, 0.78f));
        for (int bx = 2; bx <= 6; bx++) for (int by = 10; by <= 12; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(4, 10)] = new DoorData { targetMap = "DINO_CENTER_MN", spawnX = 3, spawnY = 1, isDinoCenter = true };
        tileMap[4, 10] = TILE_PATH;
        if (tileMap[4, 9] != TILE_PATH) tileMap[4, 9] = TILE_PATH;
        signs[new Vector2Int(2, 10)] = "CENTRE DINO\nSoins gratuits pour vos dinos";

        // Shop
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(18, 0, 10), 4, 3, 1,
            new Color(0.40f, 0.55f, 0.85f), new Color(0.35f, 0.25f, 0.40f));
        for (int bx = 18; bx <= 21; bx++) for (int by = 10; by <= 12; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(19, 10)] = new DoorData { targetMap = "SHOP_MN", spawnX = 3, spawnY = 1, isShop = true };
        tileMap[19, 10] = TILE_PATH;
        if (tileMap[19, 9] != TILE_PATH) tileMap[19, 9] = TILE_PATH;
        signs[new Vector2Int(18, 10)] = "BOUTIQUE DU MARAIS\nAntidotes et remedes";

        // Gym (venom themed — green base for gym detection)
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(9, 0, 18), 6, 4, 2,
            new Color(0.40f, 0.70f, 0.35f), new Color(0.35f, 0.15f, 0.40f));
        for (int bx = 9; bx <= 14; bx++) for (int by = 18; by <= 21; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(11, 18)] = new DoorData { targetMap = "GYM_MN", spawnX = 4, spawnY = 1, isGym = true, gymId = 6 };
        tileMap[11, 18] = TILE_PATH;
        if (tileMap[11, 17] != TILE_PATH) tileMap[11, 17] = TILE_PATH;
        signs[new Vector2Int(9, 18)] = "ARENE DE MARAIS-NOIR\nChampion: TOXICA\nType: Venin";

        // Herbalist house
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(19, 0, 17), 4, 3, 1,
            new Color(0.45f, 0.55f, 0.35f), new Color(0.30f, 0.40f, 0.25f));
        for (int bx = 19; bx <= 22; bx++) for (int by = 17; by <= 19; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(20, 17)] = new DoorData { targetMap = "HERBALIST_MN", spawnX = 3, spawnY = 1 };
        tileMap[20, 17] = TILE_PATH;
        if (tileMap[20, 16] != TILE_PATH) tileMap[20, 16] = TILE_PATH;
        signs[new Vector2Int(19, 17)] = "L'HERBORISTE\nRemedes naturels et antidotes";

        signs[new Vector2Int(12, 2)] = "MARAIS-NOIR\n\"L'ombre qui guerit\"";
        signs[new Vector2Int(12, 22)] = "ROUTE 9 -->\nVers Ciel-Haut";

        SpawnNPC("Herboriste", 17, 13, new string[] {
            "Bienvenue a Marais-Noir.",
            "Le marais est sombre, mais il recele des plantes medicinales rares."
        }, new Color(0.4f, 0.5f, 0.35f));
        SpawnNPC("Chasseur", 8, 8, new string[] {
            "Toxica, la championne, est une maitresse du poison.",
            "Ses dinos empoisonnent au moindre contact !",
            "Un dino de type Terre ou Psychique pourrait resister."
        }, new Color(0.5f, 0.4f, 0.5f));
        SpawnNPC("Fillette", 15, 7, new string[] {
            "J'ai peur des dinos Ombre...",
            "Mais Toxica dit qu'ils ne sont pas mechants."
        }, new Color(0.7f, 0.5f, 0.7f));
        SpawnNPC("Ancien", 5, 15, new string[] {
            "Le marais a toujours ete un lieu de mystere.",
            "Les dinos Venin protegent cet endroit depuis des siecles."
        }, new Color(0.45f, 0.42f, 0.40f));

        // --- Story NPCs ---
        SpawnNPC("Espion repenti", 12, 10, new string[] {
            "J'etais un sbire de la Team Meteore, mais j'ai quitte...",
            "Leur base secrete est quelque part dans ce marais.",
            "Le Commandant Nova veut eveiller le dino legendaire Primordius !"
        }, new Color(0.4f, 0.4f, 0.4f));

        SpawnNPC("Guerisseuse", 20, 8, new string[] {
            "Toxica utilise des dinos Venin. Le type Terre les neutralise !",
            "Attention : les dinos Venin empoisonnent souvent. Apporte des Antidotes !",
            "Le type Psychique est aussi tres efficace si tu en as un."
        }, new Color(0.5f, 0.6f, 0.4f));

        SpawnNPC("Mysterieux", 8, 18, new string[] {
            "Tu cherches la Team Meteore ? Leur symbole est partout ici...",
            "Ils sont dangereux. Assure-toi que tes dinos sont bien entraines.",
            "Le Commandant a des dinos Ombre et Venin tres puissants."
        }, new Color(0.35f, 0.35f, 0.4f));
    }

    // =========================================================================
    //  Route 9 — Sky path, Marais-Noir to Ciel-Haut
    // =========================================================================

    void GenerateRoute9()
    {
        mapWidth = 20;
        mapHeight = 35;
        tileMap = new int[mapWidth, mapHeight];
        tileObjects = new GameObject[mapWidth, mapHeight];

        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                tileMap[x, y] = TILE_GRASS;

        for (int x = 0; x < mapWidth; x++) { tileMap[x, 0] = TILE_WALL; tileMap[x, mapHeight - 1] = TILE_WALL; }
        for (int y = 0; y < mapHeight; y++) { tileMap[0, y] = TILE_WALL; tileMap[mapWidth - 1, y] = TILE_WALL; }

        // Main winding mountain summit path
        for (int y = 1; y < mapHeight - 1; y++)
        {
            int pathX = 9 + Mathf.RoundToInt(Mathf.Sin(y * 0.3f) * 3);
            pathX = Mathf.Clamp(pathX, 1, mapWidth - 3);
            tileMap[pathX, y] = TILE_PATH;
            tileMap[pathX + 1, y] = TILE_PATH;
        }

        // Rocky/sandy terrain (mountain summit)
        for (int x = 1; x <= 5; x++) for (int y = 2; y <= 7; y++) if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_SAND;
        for (int x = 14; x <= 18; x++) for (int y = 10; y <= 16; y++) if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_SAND;
        for (int x = 2; x <= 7; x++) for (int y = 25; y <= 30; y++) if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_SAND;

        // Tall grass patches
        for (int x = 2; x <= 7; x++) for (int y = 8; y <= 14; y++) if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_TALL_GRASS;
        for (int x = 12; x <= 17; x++) for (int y = 18; y <= 24; y++) if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_TALL_GRASS;
        for (int x = 5; x <= 11; x++) for (int y = 28; y <= 33; y++) if (tileMap[x, y] == TILE_GRASS) tileMap[x, y] = TILE_TALL_GRASS;

        // Cliff edges (walls)
        for (int x = 1; x <= 3; x++) tileMap[x, 16] = TILE_WALL;
        for (int x = 16; x <= 18; x++) tileMap[x, 8] = TILE_WALL;
        for (int x = 1; x <= 2; x++) tileMap[x, 26] = TILE_WALL;
        for (int x = 17; x <= 18; x++) tileMap[x, 28] = TILE_WALL;

        // Mountain ledges
        for (int x = 3; x <= 7; x++) tileMap[x, 15] = TILE_LEDGE;
        for (int x = 12; x <= 16; x++) tileMap[x, 25] = TILE_LEDGE;

        // Cloud pools (sky lakes — water tiles)
        for (int x = 14; x <= 17; x++) for (int y = 3; y <= 5; y++) tileMap[x, y] = TILE_WATER;

        // South entry from Marais-Noir
        tileMap[10, 0] = TILE_PATH; tileMap[11, 0] = TILE_PATH;
        // North exit to Ciel-Haut
        tileMap[9, mapHeight - 1] = TILE_PATH; tileMap[10, mapHeight - 1] = TILE_PATH;

        mapTransitions[new Vector2Int(10, 0)] = new MapTransition { targetMap = "MARAIS_NOIR", spawnX = 12, spawnY = 23 };
        mapTransitions[new Vector2Int(11, 0)] = new MapTransition { targetMap = "MARAIS_NOIR", spawnX = 13, spawnY = 23 };
        mapTransitions[new Vector2Int(9, mapHeight - 1)] = new MapTransition { targetMap = "CIEL_HAUT", spawnX = 12, spawnY = 1 };
        mapTransitions[new Vector2Int(10, mapHeight - 1)] = new MapTransition { targetMap = "CIEL_HAUT", spawnX = 13, spawnY = 1 };

        signs[new Vector2Int(10, 2)] = "ROUTE 9 — Chemin du Ciel\nMarais-Noir <-- --> Ciel-Haut";
        signs[new Vector2Int(9, 32)] = "CIEL-HAUT\nLe sommet vous attend !";

        SpawnTrainer("Pilote Ace", 6, 10, new string[] {
            "Les dinos volants regnent sur ces sommets !",
            "Voyons si tu peux t'elever a leur niveau !"
        }, new Color(0.5f, 0.7f, 0.9f), "TRAINER_ROUTE9_ACE",
           new int[] { 87, 89 }, new int[] { 31, 32 });

        SpawnTrainer("Mystique Iris", 14, 20, new string[] {
            "La lumiere du sommet revele la verite...",
            "Mes dinos de lumiere vont t'aveugler !"
        }, new Color(0.9f, 0.85f, 0.6f), "TRAINER_ROUTE9_IRIS",
           new int[] { 90, 92, 88 }, new int[] { 32, 33, 34 });

        SpawnTrainer("Aventurier Marco", 8, 30, new string[] {
            "Je suis presque au sommet !",
            "Mes dinos et moi sommes prets pour le defi final !"
        }, new Color(0.6f, 0.5f, 0.4f), "TRAINER_ROUTE9_MARCO",
           new int[] { 91, 93, 87 }, new int[] { 33, 34, 35 });

        SpawnNPC("Moine", 15, 5, new string[] {
            "Le ciel est la demeure des dinos les plus nobles.",
            "Les types Air et Lumiere vivent en harmonie ici."
        }, new Color(0.7f, 0.65f, 0.5f));

        AddRouteTrees(0.04f);
    }

    // =========================================================================
    //  Ciel-Haut — Sky city, Gym 8 (Air)
    // =========================================================================

    void GenerateCielHaut()
    {
        mapWidth = 25;
        mapHeight = 25;
        tileMap = new int[mapWidth, mapHeight];
        tileObjects = new GameObject[mapWidth, mapHeight];

        // Fill with grass (elevated meadow)
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                tileMap[x, y] = TILE_GRASS;

        for (int x = 0; x < mapWidth; x++) { tileMap[x, 0] = TILE_WALL; tileMap[x, mapHeight - 1] = TILE_WALL; }
        for (int y = 0; y < mapHeight; y++) { tileMap[0, y] = TILE_WALL; tileMap[mapWidth - 1, y] = TILE_WALL; }

        // Main roads
        for (int x = 1; x < mapWidth - 1; x++) { tileMap[x, 12] = TILE_PATH; tileMap[x, 13] = TILE_PATH; }
        for (int y = 1; y < mapHeight - 1; y++) { tileMap[12, y] = TILE_PATH; tileMap[13, y] = TILE_PATH; }

        // Town plaza (circular feel)
        for (int x = 9; x <= 16; x++) for (int y = 9; y <= 16; y++) tileMap[x, y] = TILE_PATH;

        // Sky pool (center water feature)
        for (int x = 11; x <= 14; x++) for (int y = 11; y <= 14; y++) tileMap[x, y] = TILE_WATER;

        // Sand border around pool
        for (int x = 10; x <= 15; x++)
            for (int y = 10; y <= 15; y++)
                if (tileMap[x, y] != TILE_WATER) tileMap[x, y] = TILE_SAND;

        // Elevated meadow areas
        for (int x = 2; x <= 6; x++) for (int y = 2; y <= 5; y++) tileMap[x, y] = TILE_TALL_GRASS;
        for (int x = 19; x <= 22; x++) for (int y = 19; y <= 22; y++) tileMap[x, y] = TILE_TALL_GRASS;

        // Cloud viewing platforms (ledges)
        for (int x = 2; x <= 5; x++) tileMap[x, 8] = TILE_LEDGE;
        for (int x = 19; x <= 22; x++) tileMap[x, 8] = TILE_LEDGE;

        // South entry from Route 9
        tileMap[12, 0] = TILE_PATH; tileMap[13, 0] = TILE_PATH;
        // North exit to Victory Road
        tileMap[12, mapHeight - 1] = TILE_PATH; tileMap[13, mapHeight - 1] = TILE_PATH;

        mapTransitions[new Vector2Int(12, 0)] = new MapTransition { targetMap = "ROUTE_9", spawnX = 9, spawnY = 33 };
        mapTransitions[new Vector2Int(13, 0)] = new MapTransition { targetMap = "ROUTE_9", spawnX = 10, spawnY = 33 };
        mapTransitions[new Vector2Int(12, mapHeight - 1)] = new MapTransition { targetMap = "VICTORY_ROAD", spawnX = 10, spawnY = 1 };
        mapTransitions[new Vector2Int(13, mapHeight - 1)] = new MapTransition { targetMap = "VICTORY_ROAD", spawnX = 11, spawnY = 1 };

        // Dino Center
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(2, 0, 10), 5, 3, 2,
            new Color(0.90f, 0.35f, 0.35f), new Color(0.85f, 0.82f, 0.78f));
        for (int bx = 2; bx <= 6; bx++) for (int by = 10; by <= 12; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(4, 10)] = new DoorData { targetMap = "DINO_CENTER_CH", spawnX = 3, spawnY = 1, isDinoCenter = true };
        tileMap[4, 10] = TILE_PATH;
        if (tileMap[4, 9] != TILE_PATH) tileMap[4, 9] = TILE_PATH;
        signs[new Vector2Int(2, 10)] = "CENTRE DINO\nSoins gratuits pour vos dinos";

        // Shop
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(18, 0, 10), 4, 3, 1,
            new Color(0.40f, 0.55f, 0.85f), new Color(0.65f, 0.70f, 0.80f));
        for (int bx = 18; bx <= 21; bx++) for (int by = 10; by <= 12; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(19, 10)] = new DoorData { targetMap = "SHOP_CH", spawnX = 3, spawnY = 1, isShop = true };
        tileMap[19, 10] = TILE_PATH;
        if (tileMap[19, 9] != TILE_PATH) tileMap[19, 9] = TILE_PATH;
        signs[new Vector2Int(18, 10)] = "BOUTIQUE CELESTE\nArticles pour les hauteurs";

        // Gym (air themed — green base for gym detection)
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(9, 0, 18), 6, 4, 2,
            new Color(0.40f, 0.70f, 0.35f), new Color(0.55f, 0.70f, 0.85f));
        for (int bx = 9; bx <= 14; bx++) for (int by = 18; by <= 21; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(11, 18)] = new DoorData { targetMap = "GYM_CH", spawnX = 4, spawnY = 1, isGym = true, gymId = 7 };
        tileMap[11, 18] = TILE_PATH;
        if (tileMap[11, 17] != TILE_PATH) tileMap[11, 17] = TILE_PATH;
        signs[new Vector2Int(9, 18)] = "ARENE DE CIEL-HAUT\nChampion: CELESTA\nType: Air";

        // Temple
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(2, 0, 18), 5, 4, 2,
            new Color(0.90f, 0.88f, 0.80f), new Color(0.75f, 0.70f, 0.60f));
        for (int bx = 2; bx <= 6; bx++) for (int by = 18; by <= 21; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(4, 18)] = new DoorData { targetMap = "TEMPLE_CH", spawnX = 3, spawnY = 1 };
        tileMap[4, 18] = TILE_PATH;
        if (tileMap[4, 17] != TILE_PATH) tileMap[4, 17] = TILE_PATH;
        signs[new Vector2Int(2, 18)] = "TEMPLE DU CIEL\nLieu sacre des anciens";

        // House 1
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(2, 0, 3), 3, 3, 1,
            new Color(0.80f, 0.75f, 0.65f), new Color(0.60f, 0.65f, 0.70f));
        for (int bx = 2; bx <= 4; bx++) for (int by = 3; by <= 5; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(3, 3)] = new DoorData { targetMap = "HOUSE_CH_1", spawnX = 3, spawnY = 1 };
        tileMap[3, 3] = TILE_PATH;
        if (tileMap[3, 2] != TILE_PATH) tileMap[3, 2] = TILE_PATH;

        // House 2
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(19, 0, 3), 3, 3, 1,
            new Color(0.80f, 0.75f, 0.65f), new Color(0.58f, 0.62f, 0.68f));
        for (int bx = 19; bx <= 21; bx++) for (int by = 3; by <= 5; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(20, 3)] = new DoorData { targetMap = "HOUSE_CH_2", spawnX = 3, spawnY = 1 };
        tileMap[20, 3] = TILE_PATH;
        if (tileMap[20, 2] != TILE_PATH) tileMap[20, 2] = TILE_PATH;

        signs[new Vector2Int(12, 2)] = "CIEL-HAUT\n\"Ou le ciel touche la terre\"";
        signs[new Vector2Int(12, 22)] = "VICTOIRE ROAD -->\nLe chemin de la gloire";

        SpawnNPC("Pretresse", 5, 16, new string[] {
            "Bienvenue a Ciel-Haut, la cite des nuages.",
            "Le Temple du Ciel garde les secrets des anciens dinos."
        }, new Color(0.8f, 0.8f, 0.7f));
        SpawnNPC("Gardien", 16, 12, new string[] {
            "Celesta est la derniere championne avant la Ligue !",
            "Ses dinos de type Air sont aussi rapides que le vent.",
            "Un dino Electrique ou Glace pourrait les ralentir."
        }, new Color(0.6f, 0.65f, 0.7f));
        SpawnNPC("Moine", 8, 8, new string[] {
            "Au-dela de Ciel-Haut se trouve la Route de la Victoire.",
            "Seuls les plus forts peuvent y survivre."
        }, new Color(0.7f, 0.65f, 0.5f));
        SpawnNPC("Enfant", 20, 15, new string[] {
            "De la-haut, on peut voir toute la region !",
            "J'ai meme vu Bourg-Nid au loin !"
        }, new Color(0.9f, 0.7f, 0.6f));

        // --- Story NPCs ---
        SpawnNPC("Maitre du vent", 10, 10, new string[] {
            "Celesta est la derniere championne. Ses dinos Air sont tres rapides !",
            "Le type Electrique ou Glace est super efficace contre le type Air.",
            "Apres cette arene, la Route de la Victoire t'attend. Sois pret !"
        }, new Color(0.6f, 0.7f, 0.7f));

        SpawnNPC("Pelerin", 15, 18, new string[] {
            "J'ai voyage a travers toute la region...",
            "La Team Meteore a ete vaincue ! Grace a un jeune dresseur courageux.",
            "Maintenant, le chemin vers la Ligue est ouvert. Vas-y !"
        }, new Color(0.5f, 0.5f, 0.5f));

        SpawnNPC("Ancienne pretresse", 3, 8, new string[] {
            "Le Temple du Ciel abrite des reliques des premiers Dinomonstres.",
            "On dit que Primordius a cree les continents en marchant.",
            "Seul un Champion peut esperer le rencontrer un jour."
        }, new Color(0.7f, 0.7f, 0.6f));
    }

    // =========================================================================
    //  Victory Road — Final route, Ciel-Haut to Paleo Capital
    // =========================================================================

    void GenerateVictoryRoad()
    {
        mapWidth = 20;
        mapHeight = 40;
        tileMap = new int[mapWidth, mapHeight];
        tileObjects = new GameObject[mapWidth, mapHeight];

        // Fill with sand (harsh terrain)
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                tileMap[x, y] = TILE_SAND;

        for (int x = 0; x < mapWidth; x++) { tileMap[x, 0] = TILE_WALL; tileMap[x, mapHeight - 1] = TILE_WALL; }
        for (int y = 0; y < mapHeight; y++) { tileMap[0, y] = TILE_WALL; tileMap[mapWidth - 1, y] = TILE_WALL; }

        // Winding path through harsh terrain
        for (int y = 1; y < mapHeight - 1; y++)
        {
            int pathX = 9 + Mathf.RoundToInt(Mathf.Sin(y * 0.4f) * 3);
            pathX = Mathf.Clamp(pathX, 1, mapWidth - 3);
            tileMap[pathX, y] = TILE_PATH;
            tileMap[pathX + 1, y] = TILE_PATH;
        }

        // Rock walls (maze-like obstacles)
        for (int x = 1; x <= 5; x++) tileMap[x, 8] = TILE_WALL;
        for (int x = 12; x <= 18; x++) tileMap[x, 12] = TILE_WALL;
        for (int x = 1; x <= 7; x++) tileMap[x, 18] = TILE_WALL;
        for (int x = 14; x <= 18; x++) tileMap[x, 22] = TILE_WALL;
        for (int x = 1; x <= 4; x++) tileMap[x, 28] = TILE_WALL;
        for (int x = 15; x <= 18; x++) tileMap[x, 32] = TILE_WALL;

        // Lava/water hazards
        for (int x = 2; x <= 5; x++) for (int y = 14; y <= 16; y++) tileMap[x, y] = TILE_WATER;
        for (int x = 14; x <= 17; x++) for (int y = 26; y <= 28; y++) tileMap[x, y] = TILE_WATER;

        // Tall grass (mixed types)
        for (int x = 2; x <= 7; x++) for (int y = 3; y <= 7; y++) if (tileMap[x, y] == TILE_SAND) tileMap[x, y] = TILE_TALL_GRASS;
        for (int x = 12; x <= 17; x++) for (int y = 14; y <= 20; y++) if (tileMap[x, y] == TILE_SAND) tileMap[x, y] = TILE_TALL_GRASS;
        for (int x = 2; x <= 8; x++) for (int y = 20; y <= 26; y++) if (tileMap[x, y] == TILE_SAND) tileMap[x, y] = TILE_TALL_GRASS;
        for (int x = 10; x <= 16; x++) for (int y = 33; y <= 38; y++) if (tileMap[x, y] == TILE_SAND) tileMap[x, y] = TILE_TALL_GRASS;

        // Ledges
        for (int x = 3; x <= 8; x++) tileMap[x, 10] = TILE_LEDGE;
        for (int x = 10; x <= 15; x++) tileMap[x, 20] = TILE_LEDGE;
        for (int x = 5; x <= 10; x++) tileMap[x, 30] = TILE_LEDGE;

        // South entry from Ciel-Haut
        tileMap[10, 0] = TILE_PATH; tileMap[11, 0] = TILE_PATH;
        // North exit to Paleo Capital
        tileMap[9, mapHeight - 1] = TILE_PATH; tileMap[10, mapHeight - 1] = TILE_PATH;

        mapTransitions[new Vector2Int(10, 0)] = new MapTransition { targetMap = "CIEL_HAUT", spawnX = 12, spawnY = 23 };
        mapTransitions[new Vector2Int(11, 0)] = new MapTransition { targetMap = "CIEL_HAUT", spawnX = 13, spawnY = 23 };
        mapTransitions[new Vector2Int(9, mapHeight - 1)] = new MapTransition { targetMap = "PALEO_CAPITAL", spawnX = 14, spawnY = 1 };
        mapTransitions[new Vector2Int(10, mapHeight - 1)] = new MapTransition { targetMap = "PALEO_CAPITAL", spawnX = 15, spawnY = 1 };

        signs[new Vector2Int(10, 2)] = "ROUTE DE LA VICTOIRE\nSeuls les meilleurs dresseurs survivent ici.";
        signs[new Vector2Int(9, 37)] = "PALEO CAPITAL\nLa Ligue Dino vous attend !";

        SpawnTrainer("Veteran Hector", 6, 5, new string[] {
            "Tu es arrive jusqu'ici ? Impressionnant !",
            "Mais tu ne passeras pas sans te battre !"
        }, new Color(0.5f, 0.4f, 0.3f), "TRAINER_VR_HECTOR",
           new int[] { 96, 100, 105 }, new int[] { 37, 38, 39 });

        SpawnTrainer("Championne Rivale", 14, 16, new string[] {
            "On se retrouve enfin !",
            "Je suis devenue bien plus forte depuis notre derniere rencontre !"
        }, new Color(0.8f, 0.4f, 0.5f), "TRAINER_VR_RIVALE",
           new int[] { 97, 101, 106, 110 }, new int[] { 38, 39, 40, 41 });

        SpawnTrainer("Maitre Karate", 5, 23, new string[] {
            "La Route de la Victoire est l'ultime epreuve !",
            "Seuls les plus forts arrivent a la Ligue !"
        }, new Color(0.6f, 0.5f, 0.4f), "TRAINER_VR_KARATE",
           new int[] { 98, 102, 107 }, new int[] { 39, 40, 41 });

        SpawnTrainer("Scientifique Dr. Nova", 12, 35, new string[] {
            "J'ai etudie tous les types de dinos...",
            "Mon equipe est parfaitement equilibree !"
        }, new Color(0.5f, 0.5f, 0.6f), "TRAINER_VR_NOVA",
           new int[] { 99, 103, 108, 109 }, new int[] { 39, 40, 41, 42 });

        SpawnNPC("Garde", 10, 38, new string[] {
            "La Paleo Capital est juste devant !",
            "La Ligue Dino t'attend. Es-tu pret ?"
        }, new Color(0.4f, 0.4f, 0.5f));
    }

    // =========================================================================
    //  Paleo Capital — Elite 4 city, endgame
    // =========================================================================

    void GeneratePaleoCapital()
    {
        mapWidth = 30;
        mapHeight = 30;
        tileMap = new int[mapWidth, mapHeight];
        tileObjects = new GameObject[mapWidth, mapHeight];

        // Fill with grass (grand city)
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                tileMap[x, y] = TILE_GRASS;

        for (int x = 0; x < mapWidth; x++) { tileMap[x, 0] = TILE_WALL; tileMap[x, mapHeight - 1] = TILE_WALL; }
        for (int y = 0; y < mapHeight; y++) { tileMap[0, y] = TILE_WALL; tileMap[mapWidth - 1, y] = TILE_WALL; }

        // Grand boulevards
        for (int x = 1; x < mapWidth - 1; x++) { tileMap[x, 14] = TILE_PATH; tileMap[x, 15] = TILE_PATH; tileMap[x, 16] = TILE_PATH; }
        for (int y = 1; y < mapHeight - 1; y++) { tileMap[14, y] = TILE_PATH; tileMap[15, y] = TILE_PATH; tileMap[16, y] = TILE_PATH; }

        // Grand plaza (large central area)
        for (int x = 11; x <= 19; x++) for (int y = 11; y <= 19; y++) tileMap[x, y] = TILE_PATH;

        // Monumental fountain
        for (int x = 13; x <= 17; x++) for (int y = 13; y <= 17; y++) tileMap[x, y] = TILE_WATER;
        for (int x = 12; x <= 18; x++)
            for (int y = 12; y <= 18; y++)
                if (tileMap[x, y] != TILE_WATER) tileMap[x, y] = TILE_SAND;

        // Gardens
        for (int x = 2; x <= 6; x++) for (int y = 2; y <= 5; y++) tileMap[x, y] = TILE_TALL_GRASS;
        for (int x = 23; x <= 27; x++) for (int y = 2; y <= 5; y++) tileMap[x, y] = TILE_TALL_GRASS;
        for (int x = 2; x <= 6; x++) for (int y = 24; y <= 27; y++) tileMap[x, y] = TILE_TALL_GRASS;
        for (int x = 23; x <= 27; x++) for (int y = 24; y <= 27; y++) tileMap[x, y] = TILE_TALL_GRASS;

        // South entry from Victory Road
        tileMap[14, 0] = TILE_PATH; tileMap[15, 0] = TILE_PATH; tileMap[16, 0] = TILE_PATH;

        mapTransitions[new Vector2Int(14, 0)] = new MapTransition { targetMap = "VICTORY_ROAD", spawnX = 9, spawnY = 38 };
        mapTransitions[new Vector2Int(15, 0)] = new MapTransition { targetMap = "VICTORY_ROAD", spawnX = 9, spawnY = 38 };
        mapTransitions[new Vector2Int(16, 0)] = new MapTransition { targetMap = "VICTORY_ROAD", spawnX = 10, spawnY = 38 };

        // Dino Center
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(2, 0, 12), 5, 3, 2,
            new Color(0.90f, 0.35f, 0.35f), new Color(0.85f, 0.82f, 0.78f));
        for (int bx = 2; bx <= 6; bx++) for (int by = 12; by <= 14; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(4, 12)] = new DoorData { targetMap = "DINO_CENTER_PC2", spawnX = 3, spawnY = 1, isDinoCenter = true };
        tileMap[4, 12] = TILE_PATH;
        if (tileMap[4, 11] != TILE_PATH) tileMap[4, 11] = TILE_PATH;
        signs[new Vector2Int(2, 12)] = "CENTRE DINO\nSoins gratuits pour vos dinos";

        // Shop
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(23, 0, 12), 4, 3, 1,
            new Color(0.40f, 0.55f, 0.85f), new Color(0.50f, 0.45f, 0.65f));
        for (int bx = 23; bx <= 26; bx++) for (int by = 12; by <= 14; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(24, 12)] = new DoorData { targetMap = "SHOP_PC2", spawnX = 3, spawnY = 1, isShop = true };
        tileMap[24, 12] = TILE_PATH;
        if (tileMap[24, 11] != TILE_PATH) tileMap[24, 11] = TILE_PATH;
        signs[new Vector2Int(23, 12)] = "BOUTIQUE DE LA LIGUE\nLe meilleur equipement pour les champions";

        // League Building (grand, top-center)
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(10, 0, 22), 10, 5, 3,
            new Color(0.85f, 0.80f, 0.65f), new Color(0.70f, 0.60f, 0.40f));
        for (int bx = 10; bx <= 19; bx++) for (int by = 22; by <= 26; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(14, 22)] = new DoorData { targetMap = "LEAGUE_BUILDING", spawnX = 5, spawnY = 1 };
        tileMap[14, 22] = TILE_PATH;
        if (tileMap[14, 21] != TILE_PATH) tileMap[14, 21] = TILE_PATH;
        signs[new Vector2Int(10, 22)] = "LIGUE DINO\nL'ultime defi vous attend ici.\nQuatre membres de l'Elite et le Maitre !";

        // Museum
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(2, 0, 20), 5, 4, 2,
            new Color(0.80f, 0.78f, 0.72f), new Color(0.60f, 0.55f, 0.48f));
        for (int bx = 2; bx <= 6; bx++) for (int by = 20; by <= 23; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(4, 20)] = new DoorData { targetMap = "MUSEUM_PC2", spawnX = 3, spawnY = 1 };
        tileMap[4, 20] = TILE_PATH;
        if (tileMap[4, 19] != TILE_PATH) tileMap[4, 19] = TILE_PATH;
        signs[new Vector2Int(2, 20)] = "MUSEE PALEO\nHistoire des Dinomonstres";

        // Hall of Fame
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(23, 0, 20), 5, 4, 2,
            new Color(0.90f, 0.85f, 0.55f), new Color(0.75f, 0.65f, 0.35f));
        for (int bx = 23; bx <= 27; bx++) for (int by = 20; by <= 23; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(25, 20)] = new DoorData { targetMap = "HALL_OF_FAME", spawnX = 3, spawnY = 1 };
        tileMap[25, 20] = TILE_PATH;
        if (tileMap[25, 19] != TILE_PATH) tileMap[25, 19] = TILE_PATH;
        signs[new Vector2Int(23, 20)] = "TEMPLE DE LA GLOIRE\nLes plus grands dresseurs de l'histoire";

        // Houses
        BuildingRenderer.CreateBuilding(tileParent, new Vector3(2, 0, 7), 3, 3, 1,
            new Color(0.80f, 0.75f, 0.65f), new Color(0.55f, 0.50f, 0.45f));
        for (int bx = 2; bx <= 4; bx++) for (int by = 7; by <= 9; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(3, 7)] = new DoorData { targetMap = "HOUSE_PC2_1", spawnX = 3, spawnY = 1 };
        tileMap[3, 7] = TILE_PATH;
        if (tileMap[3, 6] != TILE_PATH) tileMap[3, 6] = TILE_PATH;

        BuildingRenderer.CreateBuilding(tileParent, new Vector3(8, 0, 7), 3, 3, 1,
            new Color(0.80f, 0.75f, 0.65f), new Color(0.52f, 0.55f, 0.50f));
        for (int bx = 8; bx <= 10; bx++) for (int by = 7; by <= 9; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(9, 7)] = new DoorData { targetMap = "HOUSE_PC2_2", spawnX = 3, spawnY = 1 };
        tileMap[9, 7] = TILE_PATH;
        if (tileMap[9, 6] != TILE_PATH) tileMap[9, 6] = TILE_PATH;

        BuildingRenderer.CreateBuilding(tileParent, new Vector3(19, 0, 7), 3, 3, 1,
            new Color(0.80f, 0.75f, 0.65f), new Color(0.53f, 0.50f, 0.55f));
        for (int bx = 19; bx <= 21; bx++) for (int by = 7; by <= 9; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(20, 7)] = new DoorData { targetMap = "HOUSE_PC2_3", spawnX = 3, spawnY = 1 };
        tileMap[20, 7] = TILE_PATH;
        if (tileMap[20, 6] != TILE_PATH) tileMap[20, 6] = TILE_PATH;

        BuildingRenderer.CreateBuilding(tileParent, new Vector3(25, 0, 7), 3, 3, 1,
            new Color(0.80f, 0.75f, 0.65f), new Color(0.50f, 0.55f, 0.52f));
        for (int bx = 25; bx <= 27; bx++) for (int by = 7; by <= 9; by++) tileMap[bx, by] = TILE_WALL;
        doors[new Vector2Int(26, 7)] = new DoorData { targetMap = "HOUSE_PC2_4", spawnX = 3, spawnY = 1 };
        tileMap[26, 7] = TILE_PATH;
        if (tileMap[26, 6] != TILE_PATH) tileMap[26, 6] = TILE_PATH;

        signs[new Vector2Int(15, 2)] = "PALEO CAPITAL\n\"La cite des champions\"";
        signs[new Vector2Int(15, 20)] = "LIGUE DINO -->\nPreparez-vous a l'ultime defi !";

        SpawnNPC("Garde Royal", 12, 20, new string[] {
            "Bienvenue a Paleo Capital, la cite des champions !",
            "La Ligue Dino se trouve dans le grand batiment au nord.",
            "Assurez-vous que vos dinos sont au maximum avant d'entrer !"
        }, new Color(0.5f, 0.45f, 0.55f));
        SpawnNPC("Historien", 4, 18, new string[] {
            "Le musee retrace l'histoire des Dinomonstres.",
            "Des creatures qui regnaient sur le monde il y a des millions d'annees !"
        }, new Color(0.6f, 0.55f, 0.5f));
        SpawnNPC("Champion Retire", 20, 16, new string[] {
            "J'ai ete champion de la Ligue il y a 20 ans.",
            "L'Elite 4 est compose de quatre dresseurs d'elite.",
            "Et le Maitre est le plus puissant de tous !"
        }, new Color(0.5f, 0.5f, 0.4f));
        SpawnNPC("Journaliste", 8, 15, new string[] {
            "Encore un nouveau challenger !",
            "Combien de badges as-tu ? Il t'en faut 8 pour entrer a la Ligue."
        }, new Color(0.7f, 0.5f, 0.6f));
        SpawnNPC("Fillette", 22, 10, new string[] {
            "Mon papa travaille au Temple de la Gloire !",
            "Un jour, mon nom sera grave la-bas aussi !"
        }, new Color(0.9f, 0.7f, 0.8f));

        // --- Story NPCs ---
        SpawnNPC("Veteran de la Ligue", 15, 10, new string[] {
            "L'Elite 4 est compose de quatre specialistes : Fossile, Glace, Ombre et Dragon.",
            "Prepare une equipe variee pour couvrir toutes les faiblesses !",
            "Le Champion utilise des dinos de tous les types. C'est un vrai defi."
        }, new Color(0.5f, 0.4f, 0.4f));

        SpawnNPC("Strategiste", 6, 12, new string[] {
            "Un conseil : soigne tes dinos entre chaque combat de la Ligue !",
            "Tu ne pourras pas sortir une fois entre. Apporte beaucoup de Potions.",
            "Les dinos de l'Elite 4 sont niveau 44 et plus. Sois pret !"
        }, new Color(0.4f, 0.5f, 0.5f));

        SpawnNPC("Prof. Saule", 18, 18, new string[] {
            "Te voila a Paleo Capital ! Quel chemin parcouru depuis Bourg-Nid !",
            "Je suis fier de toi. Tes dinos et toi avez tant grandi ensemble.",
            "Va, et deviens le Champion de la region ! Je crois en toi !"
        }, new Color(0.6f, 0.6f, 0.5f));
    }

    // =========================================================================
    //  Tile Rendering
    // =========================================================================

    void CreateTile(int x, int y, int type)
    {
        // Deterministic seed for per-tile variation
        Random.State savedState = Random.state;
        Random.InitState(x * 7919 + y * 6271);

        GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tile.transform.SetParent(tileParent);
        tile.transform.position = new Vector3(x * tileSize, -0.5f, y * tileSize);
        tile.transform.localScale = new Vector3(tileSize, 1f, tileSize);
        tile.name = $"Tile_{x}_{y}";

        var renderer = tile.GetComponent<Renderer>();

        switch (type)
        {
            case TILE_GRASS:
                // Slight random Y variation for organic feel
                float grassY = -0.5f + Random.Range(0f, 0.02f);
                tile.transform.position = new Vector3(x * tileSize, grassY, y * tileSize);
                // Subtle color variation per tile
                Color grassVar = new Color(
                    ColorGrass.r + Random.Range(-0.02f, 0.02f),
                    ColorGrass.g + Random.Range(-0.03f, 0.03f),
                    ColorGrass.b + Random.Range(-0.02f, 0.02f));
                renderer.sharedMaterial = MaterialManager.GetSolidColor(grassVar);
                // Occasionally add small flowers on grass
                if (Random.value < 0.08f)
                    AddFlowerDecoration(tile.transform);
                break;

            case TILE_PATH:
                // Detect if this is an intersection (path neighbors on 3+ sides)
                bool isIntersection = IsPathIntersection(x, y);
                Color pathColor = isIntersection
                    ? new Color(ColorPath.r - 0.05f, ColorPath.g - 0.04f, ColorPath.b - 0.03f)
                    : ColorPath;
                renderer.sharedMaterial = MaterialManager.GetSolidColor(pathColor);
                break;

            case TILE_WALL:
                tile.transform.position = new Vector3(x * tileSize, 0.5f, y * tileSize);
                tile.transform.localScale = new Vector3(tileSize, 2f, tileSize);
                renderer.sharedMaterial = MaterialManager.GetSolidColor(ColorWall);
                // Add scattered rocks near walls
                if (Random.value < 0.2f)
                    AddRockDecoration(x, y);
                break;

            case TILE_TALL_GRASS:
                renderer.sharedMaterial = MaterialManager.GetSolidColor(ColorTallGrass);
                AddGrassBlades(tile.transform, x, y);
                break;

            case TILE_WATER:
                tile.transform.position = new Vector3(x * tileSize, -0.7f, y * tileSize);
                // Semi-transparent water with subtle glow
                renderer.sharedMaterial = MaterialManager.GetTransparent(ColorWater, 0.75f);
                AddWaterGlow(tile.transform);
                break;

            case TILE_SAND:
                renderer.sharedMaterial = MaterialManager.GetSolidColor(ColorSand);
                break;

            case TILE_LEDGE:
                renderer.sharedMaterial = MaterialManager.GetSolidColor(ColorLedge);
                tile.transform.position = new Vector3(x * tileSize, -0.25f, y * tileSize);
                tile.transform.localScale = new Vector3(tileSize, 0.5f, tileSize);
                break;
        }

        // Only walls keep colliders (for raycasting etc.)
        if (type != TILE_WALL)
            Object.Destroy(tile.GetComponent<Collider>());

        tileObjects[x, y] = tile;
        Random.state = savedState;
    }

    /// <summary>Check if a path tile has path neighbors on 3+ sides (intersection)</summary>
    bool IsPathIntersection(int x, int y)
    {
        int count = 0;
        if (x > 0 && tileMap[x - 1, y] == TILE_PATH) count++;
        if (x < mapWidth - 1 && tileMap[x + 1, y] == TILE_PATH) count++;
        if (y > 0 && tileMap[x, y - 1] == TILE_PATH) count++;
        if (y < mapHeight - 1 && tileMap[x, y + 1] == TILE_PATH) count++;
        return count >= 3;
    }

    /// <summary>Add a subtle blue glow plane above water tiles</summary>
    void AddWaterGlow(Transform parent)
    {
        var glow = GameObject.CreatePrimitive(PrimitiveType.Quad);
        glow.transform.SetParent(parent);
        glow.transform.localPosition = new Vector3(0, 0.52f, 0);
        glow.transform.localRotation = Quaternion.Euler(90, 0, 0);
        glow.transform.localScale = new Vector3(0.95f, 0.95f, 1f);
        glow.name = "WaterGlow";
        var r = glow.GetComponent<Renderer>();
        r.sharedMaterial = MaterialManager.GetTransparent(new Color(0.4f, 0.6f, 1f), 0.18f);
        Object.Destroy(glow.GetComponent<Collider>());
    }

    /// <summary>Add small rock decoration near wall tiles (on adjacent open ground)</summary>
    void AddRockDecoration(int x, int y)
    {
        // Only place rocks where there's adjacent open space
        bool nearOpen = false;
        if (x > 0 && tileMap[x - 1, y] != TILE_WALL) nearOpen = true;
        if (x < mapWidth - 1 && tileMap[x + 1, y] != TILE_WALL) nearOpen = true;
        if (y > 0 && tileMap[x, y - 1] != TILE_WALL) nearOpen = true;
        if (y < mapHeight - 1 && tileMap[x, y + 1] != TILE_WALL) nearOpen = true;
        if (!nearOpen) return;

        var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rock.transform.SetParent(tileParent);
        float rockScale = Random.Range(0.08f, 0.15f);
        rock.transform.position = new Vector3(
            x * tileSize + Random.Range(-0.3f, 0.3f),
            -0.45f + rockScale * 0.5f,
            y * tileSize + Random.Range(-0.3f, 0.3f));
        rock.transform.localScale = new Vector3(rockScale, rockScale * 0.7f, rockScale);
        rock.name = "Rock";
        var r = rock.GetComponent<Renderer>();
        Color rockColor = new Color(
            0.5f + Random.Range(-0.05f, 0.05f),
            0.48f + Random.Range(-0.05f, 0.05f),
            0.42f + Random.Range(-0.05f, 0.05f));
        r.sharedMaterial = MaterialManager.GetSolidColor(rockColor);
        Object.Destroy(rock.GetComponent<Collider>());
    }

    /// <summary>Add tiny colored flower cubes on grass tiles</summary>
    void AddFlowerDecoration(Transform parent)
    {
        Color[] flowerColors = new Color[] {
            new Color(0.95f, 0.3f, 0.35f),  // red
            new Color(0.95f, 0.85f, 0.2f),  // yellow
            new Color(0.9f, 0.5f, 0.8f),    // pink
            new Color(0.4f, 0.5f, 0.95f),   // blue
            new Color(1f, 0.6f, 0.2f),      // orange
        };
        Color chosen = flowerColors[Random.Range(0, flowerColors.Length)];

        var flower = GameObject.CreatePrimitive(PrimitiveType.Cube);
        flower.transform.SetParent(parent);
        flower.transform.localPosition = new Vector3(
            Random.Range(-0.35f, 0.35f), 0.52f, Random.Range(-0.35f, 0.35f));
        flower.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
        flower.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 45), 0);
        flower.name = "Flower";
        var r = flower.GetComponent<Renderer>();
        r.sharedMaterial = MaterialManager.GetSolidColor(chosen);
        Object.Destroy(flower.GetComponent<Collider>());

        // Tiny green stem
        var stem = GameObject.CreatePrimitive(PrimitiveType.Cube);
        stem.transform.SetParent(parent);
        stem.transform.localPosition = flower.transform.localPosition + new Vector3(0, -0.03f, 0);
        stem.transform.localScale = new Vector3(0.015f, 0.04f, 0.015f);
        stem.name = "Stem";
        var sr = stem.GetComponent<Renderer>();
        sr.sharedMaterial = MaterialManager.GetSolidColor(new Color(0.2f, 0.5f, 0.15f));
        Object.Destroy(stem.GetComponent<Collider>());
    }

    void AddGrassBlades(Transform parent, int x, int y)
    {
        // Deterministic seed so grass looks consistent
        Random.State saved = Random.state;
        Random.InitState(x * 1000 + y);

        for (int i = 0; i < 4; i++)
        {
            var blade = GameObject.CreatePrimitive(PrimitiveType.Quad);
            blade.transform.SetParent(parent);
            blade.transform.localPosition = new Vector3(
                Random.Range(-0.35f, 0.35f), 0.7f, Random.Range(-0.35f, 0.35f));
            blade.transform.localScale = new Vector3(0.12f, 0.35f, 1f);
            blade.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            var r = blade.GetComponent<Renderer>();
            Color bladeColor = new Color(
                ColorGrassBlade.r + Random.Range(-0.03f, 0.03f),
                ColorGrassBlade.g + Random.Range(-0.05f, 0.08f),
                ColorGrassBlade.b + Random.Range(-0.02f, 0.02f));
            r.sharedMaterial = MaterialManager.GetSolidColor(bladeColor);
            Object.Destroy(blade.GetComponent<Collider>());
        }

        Random.state = saved;
    }

    /// <summary>
    /// Create a procedural tree at the given grid position.
    /// Green sphere canopy on brown cylinder trunk, with size variation.
    /// </summary>
    void CreateTree(int x, int y)
    {
        Random.State saved = Random.state;
        Random.InitState(x * 3571 + y * 2909);

        float trunkHeight = Random.Range(0.6f, 1.2f);
        float trunkRadius = Random.Range(0.06f, 0.10f);
        float canopyRadius = Random.Range(0.25f, 0.45f);

        // Trunk
        var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.transform.SetParent(tileParent);
        trunk.transform.position = new Vector3(
            x * tileSize + Random.Range(-0.15f, 0.15f),
            trunkHeight * 0.5f,
            y * tileSize + Random.Range(-0.15f, 0.15f));
        trunk.transform.localScale = new Vector3(trunkRadius * 2, trunkHeight * 0.5f, trunkRadius * 2);
        trunk.name = "TreeTrunk";
        var trunkR = trunk.GetComponent<Renderer>();
        Color trunkColor = new Color(
            0.45f + Random.Range(-0.05f, 0.05f),
            0.30f + Random.Range(-0.03f, 0.03f),
            0.15f + Random.Range(-0.03f, 0.03f));
        trunkR.sharedMaterial = MaterialManager.GetSolidColor(trunkColor);
        Object.Destroy(trunk.GetComponent<Collider>());

        // Canopy (main sphere)
        var canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        canopy.transform.SetParent(tileParent);
        canopy.transform.position = trunk.transform.position + new Vector3(0, trunkHeight * 0.5f + canopyRadius * 0.5f, 0);
        canopy.transform.localScale = new Vector3(canopyRadius * 2, canopyRadius * 1.6f, canopyRadius * 2);
        canopy.name = "TreeCanopy";
        var canopyR = canopy.GetComponent<Renderer>();
        Color canopyColor = new Color(
            0.18f + Random.Range(-0.04f, 0.04f),
            0.55f + Random.Range(-0.08f, 0.08f),
            0.15f + Random.Range(-0.04f, 0.04f));
        canopyR.sharedMaterial = MaterialManager.GetSolidColor(canopyColor);
        Object.Destroy(canopy.GetComponent<Collider>());

        // Secondary canopy blob for fuller look
        var canopy2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        canopy2.transform.SetParent(tileParent);
        canopy2.transform.position = canopy.transform.position +
            new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.05f, 0.1f), Random.Range(-0.1f, 0.1f));
        canopy2.transform.localScale = new Vector3(canopyRadius * 1.5f, canopyRadius * 1.2f, canopyRadius * 1.5f);
        canopy2.name = "TreeCanopy2";
        var canopy2R = canopy2.GetComponent<Renderer>();
        Color canopy2Color = new Color(canopyColor.r + 0.03f, canopyColor.g + 0.05f, canopyColor.b + 0.02f);
        canopy2R.sharedMaterial = MaterialManager.GetSolidColor(canopy2Color);
        Object.Destroy(canopy2.GetComponent<Collider>());

        Random.state = saved;
    }

    /// <summary>
    /// Add trees to grass tiles on routes. Call after setting up tileMap.
    /// Places trees on TILE_GRASS tiles that are not next to paths/doors.
    /// </summary>
    void AddRouteTrees(float density = 0.12f)
    {
        Random.State saved = Random.state;
        Random.InitState(currentMapId.GetHashCode());

        for (int x = 1; x < mapWidth - 1; x++)
        {
            for (int y = 1; y < mapHeight - 1; y++)
            {
                if (tileMap[x, y] != TILE_GRASS) continue;

                // Don't place trees adjacent to paths or buildings
                bool nearPath = false;
                for (int dx = -1; dx <= 1 && !nearPath; dx++)
                    for (int dy = -1; dy <= 1 && !nearPath; dy++)
                    {
                        int nx = x + dx, ny = y + dy;
                        if (nx < 0 || nx >= mapWidth || ny < 0 || ny >= mapHeight) continue;
                        if (tileMap[nx, ny] == TILE_PATH || tileMap[nx, ny] == TILE_WALL) nearPath = true;
                    }
                if (nearPath) continue;

                if (Random.value < density)
                    CreateTree(x, y);
            }
        }

        Random.state = saved;
    }

    // =========================================================================
    //  Queries
    // =========================================================================

    public bool IsWalkable(int x, int y)
    {
        // Delegate to InteriorManager when inside a building
        if (InteriorManager.Instance != null && InteriorManager.Instance.IsInInterior)
            return InteriorManager.Instance.IsWalkable(x, y);

        if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight) return false;

        // Door tiles are always walkable (player enters by stepping on them)
        Vector2Int pos = new Vector2Int(x, y);
        if (doors.ContainsKey(pos)) return true;

        // Map transition tiles are always walkable
        if (mapTransitions.ContainsKey(pos)) return true;

        int type = tileMap[x, y];
        return type != TILE_WALL && type != TILE_WATER;
    }

    public bool IsTallGrass(int x, int y)
    {
        // No tall grass inside buildings
        if (InteriorManager.Instance != null && InteriorManager.Instance.IsInInterior) return false;

        if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight) return false;
        return tileMap[x, y] == TILE_TALL_GRASS;
    }

    public bool IsLedge(int x, int y)
    {
        if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight) return false;
        return tileMap[x, y] == TILE_LEDGE;
    }

    public int GetTileType(int x, int y)
    {
        if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight) return -1;
        return tileMap[x, y];
    }

    // =========================================================================
    //  Events
    // =========================================================================

    public void OnPlayerStep(int x, int y)
    {
        // Delegate to InteriorManager when inside a building
        if (InteriorManager.Instance != null && InteriorManager.Instance.IsInInterior)
        {
            InteriorManager.Instance.OnPlayerStep(x, y);
            return;
        }

        // Sync player position to GameState for saving
        GameState.Instance.PlayerX = x;
        GameState.Instance.PlayerY = y;

        // Map edge transition
        Vector2Int pos = new Vector2Int(x, y);
        if (mapTransitions.ContainsKey(pos))
        {
            var transition = mapTransitions[pos];
            Debug.Log($"[Overworld] Map transition -> {transition.targetMap} at ({transition.spawnX}, {transition.spawnY})");
            TransitionToMap(transition.targetMap, transition.spawnX, transition.spawnY);
            return;
        }

        // Wild encounter in tall grass
        if (IsTallGrass(x, y) && Random.value < encounterRate)
        {
            TriggerWildEncounter();
            return;
        }

        // Trainer sight check — trainers challenge the player when stepped into their line of sight
        CheckTrainerSight(x, y);

        // Story event check — triggers cutscenes, rival battles, etc.
        if (StoryEventSystem.Instance != null && !StoryEventSystem.Instance.IsEventRunning())
        {
            StoryEventSystem.Instance.CheckEvents(currentMapId, x, y);
        }

        // Door transition
        if (doors.ContainsKey(pos))
        {
            var door = doors[pos];
            Debug.Log($"[Overworld] Door -> {door.targetMap} at ({door.spawnX}, {door.spawnY})");

            if (door.isDinoCenter)
            {
                HandleDinoCenter(x, y);
                return;
            }

            if (door.isGym && door.gymId >= 0)
            {
                HandleGymDoor(door.gymId);
                return;
            }

            if (door.isShop)
            {
                HandleShop();
                return;
            }

            // Generic house / other building entry
            HandleHouseEntry();
        }
    }

    // =========================================================================
    //  Gym Door Handling
    // =========================================================================

    private void HandleGymDoor(int gymId)
    {
        if (InteriorManager.Instance != null)
        {
            int dx = player != null ? player.gridX : 0;
            int dy = player != null ? player.gridY : 0;
            StartCoroutine(EnterBuildingWithFade("gym", currentMapId, dx, dy, gymId));
            return;
        }

        // Fallback: legacy in-place dialogue if InteriorManager is not available
        if (GameState.Instance.HasBadge(gymId))
        {
            string leaderName = GetGymLeaderName(gymId);
            if (DialogueUI.Instance != null)
            {
                DialogueUI.Instance.ShowText(
                    "Tu as deja mon badge ! Continue ton voyage !",
                    leaderName, null);
            }
            return;
        }

        string leader = GetGymLeaderName(gymId);
        string introDialogue = GetGymLeaderIntro(gymId);

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowText(introDialogue, leader, () =>
            {
                GameManager.Instance.StartGymBattle(gymId);
            });
        }
        else
        {
            GameManager.Instance.StartGymBattle(gymId);
        }
    }

    private string GetGymLeaderName(int gymId)
    {
        switch (gymId)
        {
            case 0: return "FLORA";
            case 1: return "MARIN";
            case 2: return "PETRA";
            case 3: return "BRAZIER";
            case 4: return "GIVRALIA";
            case 5: return "VOLTAIRE";
            case 6: return "TOXICA";
            case 7: return "CELESTA";
            default: return "CHAMPION";
        }
    }

    private string GetGymLeaderIntro(int gymId)
    {
        switch (gymId)
        {
            case 0: return "Bienvenue dans mon arene ! Je suis Flora, maitresse des dinos plantes. Montrons la puissance de la nature !";
            case 1: return "Ahoy ! Je suis Marin, et mes dinos aquatiques vont te submerger ! Pret pour la vague ?";
            case 2: return "Je suis Petra. Mes fossiles ont traverse les ages... Voyons si tu es digne de les affronter !";
            case 3: return "BRAZIER enflamme l'arene ! Mes dinos de feu vont te bruler ! En garde !";
            case 4: return "Bienvenue dans le froid eternel. Je suis Givralia. Mes dinos de glace vont te geler sur place !";
            case 5: return "Je suis Voltaire ! L'electricite parcourt mes veines ! Mes dinos vont t'electriser !";
            case 6: return "Hehehe... Je suis Toxica. Attention au poison... Mes dinos sont mortellement beaux !";
            case 7: return "Je suis Celesta, gardienne des cieux. Mes dinos aeriens dominent le monde d'en haut !";
            default: return "Prepare-toi au combat !";
        }
    }

    /// <summary>
    /// Transition to a different map, placing the player at the specified position.
    /// </summary>
    public void TransitionToMap(string targetMapId, int spawnX, int spawnY)
    {
        StartCoroutine(TransitionToMapWithFade(targetMapId, spawnX, spawnY));
    }

    private IEnumerator TransitionToMapWithFade(string targetMapId, int spawnX, int spawnY)
    {
        // Lock player input during transition
        if (player != null) player.LockInput();

        // Fade to black
        if (ScreenFade.Instance != null)
            yield return ScreenFade.Instance.FadeOutCoroutine(0.3f);

        // Update GameState
        GameState.Instance.CurrentMapId = targetMapId;
        GameState.Instance.PlayerX = spawnX;
        GameState.Instance.PlayerY = spawnY;

        // Build the new map
        BuildMap(targetMapId);

        // Place the player at the entry point
        if (player != null)
            player.SetGridPosition(spawnX, spawnY);

        // Snap camera to player immediately (no lerp delay after map change)
        var cam = FindObjectOfType<OverworldCamera>();
        if (cam != null) cam.SnapToTarget();

        // Update music for the new map
        PlayMapMusic(targetMapId);

        // Update weather and ambient for the new map
        if (WeatherSystem.Instance != null)
            WeatherSystem.Instance.SetWeatherForMap(targetMapId);
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayAmbientForMap(targetMapId);

        Debug.Log($"[Overworld] Entered {targetMapId} at ({spawnX}, {spawnY})");

        // Fade back in
        if (ScreenFade.Instance != null)
            yield return ScreenFade.Instance.FadeInCoroutine(0.3f);

        // Unlock player input
        if (player != null) player.UnlockInput();

        // Check for map entry story events (rival encounters, cutscenes, etc.)
        if (StoryEventSystem.Instance != null)
        {
            StoryEventSystem.Instance.CheckMapEntryEvents(targetMapId);
        }
    }

    /// <summary>
    /// Check if the player stepped into any trainer's line of sight.
    /// </summary>
    void CheckTrainerSight(int playerX, int playerY)
    {
        foreach (var npc in npcs)
        {
            if (!npc.isTrainer) continue;
            if (string.IsNullOrEmpty(npc.trainerId)) continue;
            if (GameState.Instance.IsTrainerDefeated(npc.trainerId)) continue;

            // Check if player is in the trainer's facing direction, within 5 tiles
            int dx = playerX - npc.gridX;
            int dy = playerY - npc.gridY;

            bool inSight = false;
            switch (npc.GetFacing())
            {
                case PlayerController.Direction.Up:
                    inSight = dx == 0 && dy > 0 && dy <= 5;
                    break;
                case PlayerController.Direction.Down:
                    inSight = dx == 0 && dy < 0 && dy >= -5;
                    break;
                case PlayerController.Direction.Left:
                    inSight = dy == 0 && dx < 0 && dx >= -5;
                    break;
                case PlayerController.Direction.Right:
                    inSight = dy == 0 && dx > 0 && dx <= 5;
                    break;
            }

            if (inSight)
            {
                // Face the player and initiate battle
                npc.FaceTowards(playerX, playerY);
                npc.Interact(player);

                // Start trainer battle
                if (npc.trainerSpeciesIds != null && npc.trainerSpeciesIds.Length > 0)
                {
                    TriggerTrainerBattle(npc);
                }
                break; // Only one trainer at a time
            }
        }
    }

    /// <summary>
    /// Start a battle against a trainer NPC.
    /// </summary>
    void TriggerTrainerBattle(NPCController trainer)
    {
        Debug.Log($"[Overworld] Trainer battle: {trainer.npcName} !");

        // Use the first dino from their team for now
        int speciesId = trainer.trainerSpeciesIds[0];
        int level = trainer.trainerLevels[0];

        var setup = new BattleSetupData
        {
            isWild = false,
            enemySpeciesId = speciesId,
            enemyLevel = level,
            trainerName = trainer.npcName,
            trainerId = trainer.trainerId,
            returnScene = currentMapId,
            returnPosition = new Vector2(player.gridX, player.gridY)
        };

        GameManager.Instance.StartBattle(setup);
    }

    /// <summary>
    /// Heal all party dinos and set heal point when entering a Dino Center.
    /// </summary>
    private void HandleDinoCenter(int doorX, int doorY)
    {
        if (InteriorManager.Instance != null)
        {
            StartCoroutine(EnterBuildingWithFade("dinocenter", currentMapId, doorX, doorY));
            return;
        }

        // Fallback: legacy in-place dialogue if InteriorManager is not available
        var cam = FindObjectOfType<OverworldCamera>();
        if (cam != null) cam.ZoomIn();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayHeal();

        foreach (var dino in GameState.Instance.Party)
            dino.FullHeal();

        GameState.Instance.SetHealPoint(currentMapId, doorX, doorY);

        if (player != null) player.LockInput();

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowText(
                "Bienvenue au Centre Dino !\nVos dinos sont en pleine forme !",
                "INFIRMIERE",
                () =>
                {
                    if (player != null) player.UnlockInput();
                    var camRef = FindObjectOfType<OverworldCamera>();
                    if (camRef != null) camRef.ZoomOut();
                }
            );
        }
        else
        {
            Debug.Log("[Overworld] Dino Center: All party dinos healed!");
            if (player != null) player.UnlockInput();
            var camRef = FindObjectOfType<OverworldCamera>();
            if (camRef != null) camRef.ZoomOut();
        }
    }

    /// <summary>
    /// Fade-based interior entry for buildings using InteriorManager.
    /// </summary>
    private bool isEnteringBuilding = false;

    private IEnumerator EnterBuildingWithFade(string buildingType, string returnMapId, int doorX, int doorY, int gymId = -1)
    {
        if (isEnteringBuilding) yield break;
        isEnteringBuilding = true;

        if (player != null) player.LockInput();

        // Fade to black (skip if no ScreenFade)
        if (ScreenFade.Instance != null)
            yield return ScreenFade.Instance.FadeOutCoroutine(0.3f);

        // Enter the building
        if (InteriorManager.Instance != null)
        {
            InteriorManager.Instance.EnterBuilding(buildingType, returnMapId,
                new Vector3(doorX, 0, doorY), gymId);
        }
        else
        {
            Debug.LogError("[Overworld] InteriorManager.Instance is null — cannot enter building!");
            if (player != null) player.UnlockInput();
            isEnteringBuilding = false;
            if (ScreenFade.Instance != null) ScreenFade.Instance.SetClear();
            yield break;
        }

        yield return new WaitForSeconds(0.15f);

        // Fade back in
        if (ScreenFade.Instance != null)
            yield return ScreenFade.Instance.FadeInCoroutine(0.3f);

        // ALWAYS unlock
        if (player != null) player.UnlockInput();
        isEnteringBuilding = false;
        Debug.Log("[Overworld] Building entered, input unlocked");
    }

    // =========================================================================
    //  Shop Handling
    // =========================================================================

    private void HandleShop()
    {
        if (InteriorManager.Instance != null)
        {
            int dx = player != null ? player.gridX : 0;
            int dy = player != null ? player.gridY : 0;
            StartCoroutine(EnterBuildingWithFade("shop", currentMapId, dx, dy));
            return;
        }

        // Fallback: legacy in-place dialogue if InteriorManager is not available
        if (player != null) player.LockInput();

        var cam = FindObjectOfType<OverworldCamera>();
        if (cam != null) cam.ZoomIn();

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowChoices(
                "Bienvenue a la boutique !\nQue puis-je faire pour vous ?",
                new[] { "Acheter", "Quitter" },
                (idx) =>
                {
                    if (idx == 0)
                        ShowShopBuyMenu();
                    else
                        CloseShop();
                }
            );
        }
        else
        {
            Debug.Log("[Overworld] Shop entered but no DialogueUI!");
            if (player != null) player.UnlockInput();
            var camRef = FindObjectOfType<OverworldCamera>();
            if (camRef != null) camRef.ZoomOut();
        }
    }

    // Shop item definitions
    private static readonly int[] shopItemIds    = { 1, 2, 5, 12, 16, 17 };
    private static readonly string[] shopItemNames = { "Potion", "Super Potion", "Antidote", "Rappel", "Jurassic Ball", "Super Ball" };
    private static readonly int[] shopItemPrices = { 200, 500, 200, 1500, 200, 600 };

    private void ShowShopBuyMenu()
    {
        int count = shopItemIds.Length;
        int money = GameState.Instance.Money;
        string[] choices = new string[count + 1];
        for (int i = 0; i < count; i++)
            choices[i] = shopItemNames[i] + " — " + shopItemPrices[i] + "$";
        choices[count] = "Quitter";

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowChoices(
                "Vous avez " + money + "$.\nQue voulez-vous acheter ?",
                choices,
                (idx) =>
                {
                    if (idx >= 0 && idx < count)
                    {
                        int itemId = shopItemIds[idx];
                        string itemName = shopItemNames[idx];
                        int itemPrice = shopItemPrices[idx];

                        if (GameState.Instance.Money >= itemPrice)
                        {
                            GameState.Instance.RemoveMoney(itemPrice);
                            GameState.Instance.Inventory.AddItem(itemId);

                            if (AudioManager.Instance != null)
                                AudioManager.Instance.PlayMenuSelect();

                            DialogueUI.Instance.ShowText(
                                "Vous avez achete " + itemName + " !\nIl vous reste " + GameState.Instance.Money + "$.",
                                "VENDEUR",
                                () => ShowShopBuyMenu()
                            );
                        }
                        else
                        {
                            DialogueUI.Instance.ShowText(
                                "Vous n'avez pas assez d'argent !",
                                "VENDEUR",
                                () => ShowShopBuyMenu()
                            );
                        }
                    }
                    else
                    {
                        CloseShop();
                    }
                }
            );
        }
    }

    private void CloseShop()
    {
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowText(
                "Merci et a bientot !",
                "VENDEUR",
                () =>
                {
                    if (player != null) player.UnlockInput();
                    var camRef = FindObjectOfType<OverworldCamera>();
                    if (camRef != null) camRef.ZoomOut();
                }
            );
        }
        else
        {
            if (player != null) player.UnlockInput();
            var camRef = FindObjectOfType<OverworldCamera>();
            if (camRef != null) camRef.ZoomOut();
        }
    }

    // =========================================================================
    //  House Entry Handling
    // =========================================================================

    private void HandleHouseEntry()
    {
        if (InteriorManager.Instance != null)
        {
            int dx = player != null ? player.gridX : 0;
            int dy = player != null ? player.gridY : 0;
            StartCoroutine(EnterBuildingWithFade("house", currentMapId, dx, dy));
            return;
        }

        // Fallback: legacy in-place dialogue if InteriorManager is not available
        if (player != null) player.LockInput();

        string[] houseDialogues = new[]
        {
            "Il n'y a personne a la maison...",
            "Un villageois vous salue depuis l'interieur !",
            "La porte est fermee a cle.",
            "Vous entendez du bruit a l'interieur mais personne n'ouvre.",
            "Un dino domestique vous regarde par la fenetre !"
        };

        string dialogue = houseDialogues[Random.Range(0, houseDialogues.Length)];

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowText(dialogue, null, () =>
            {
                if (player != null) player.UnlockInput();
            });
        }
        else
        {
            Debug.Log($"[Overworld] House: {dialogue}");
            if (player != null) player.UnlockInput();
        }
    }

    /// <summary>
    /// Try to interact with the tile at (x, y). Returns true if something was interacted with.
    /// </summary>
    public bool TryInteract(int x, int y)
    {
        // Delegate to InteriorManager when inside a building
        if (InteriorManager.Instance != null && InteriorManager.Instance.IsInInterior)
            return InteriorManager.Instance.TryInteract(x, y);

        Vector2Int pos = new Vector2Int(x, y);

        // Check signs
        if (signs.ContainsKey(pos))
        {
            Debug.Log($"[Sign] {signs[pos]}");
            // TODO: Show dialogue UI
            return true;
        }

        // Check NPCs
        foreach (var npc in npcs)
        {
            if (npc.gridX == x && npc.gridY == y)
            {
                npc.Interact(player);
                return true;
            }
        }

        // Check doors
        if (doors.ContainsKey(pos))
        {
            var door = doors[pos];
            Debug.Log($"[Door] Entering {door.targetMap}");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Check if there is an interactable object at (x, y) without triggering it.
    /// Used by OverworldHUD to show interaction hints.
    /// </summary>
    public bool HasInteractable(int x, int y)
    {
        // Delegate to InteriorManager when inside a building
        if (InteriorManager.Instance != null && InteriorManager.Instance.IsInInterior)
            return InteriorManager.Instance.HasInteractable(x, y);

        Vector2Int pos = new Vector2Int(x, y);
        if (signs.ContainsKey(pos)) return true;
        if (doors.ContainsKey(pos)) return true;
        foreach (var npc in npcs)
        {
            if (npc.gridX == x && npc.gridY == y) return true;
        }
        return false;
    }

    /// <summary>
    /// Get the current map ID.
    /// </summary>
    public string GetCurrentMapId()
    {
        return currentMapId;
    }

    // =========================================================================
    //  Wild Encounters
    // =========================================================================

    void TriggerWildEncounter()
    {
        StartCoroutine(TriggerWildEncounterWithFlash());
    }

    private IEnumerator TriggerWildEncounterWithFlash()
    {
        Debug.Log("[Overworld] Wild encounter triggered!");

        // Lock player input immediately
        if (player != null) player.LockInput();

        // Wild encounter flash: white flashes then fade to black
        if (ScreenFade.Instance != null)
            yield return ScreenFade.Instance.WildEncounterFlash();

        // Pick a random wild dino based on current map
        int[] wildPool;
        int minLevel, maxLevel;

        switch (currentMapId)
        {
            case "ROUTE_1":
                wildPool = new int[] { 37, 38, 39, 40, 41 };
                minLevel = 3;
                maxLevel = 6;
                break;
            case "VILLE_FOUGERE":
                wildPool = new int[] { 37, 38 };
                minLevel = 4;
                maxLevel = 6;
                break;
            case "ROUTE_2":
                wildPool = new int[] { 38, 39, 40, 41, 42 };
                minLevel = 6;
                maxLevel = 10;
                break;
            case "ROUTE_3":
                // Water/Earth types — rocky path
                wildPool = new int[] { 49, 70, 46, 47, 10, 43 };
                minLevel = 8;
                maxLevel = 12;
                break;
            case "PORT_COQUILLE":
                wildPool = new int[] { 70, 49 };
                minLevel = 9;
                maxLevel = 11;
                break;
            case "ROUTE_4":
                // Earth/Fossil types — mountain path
                wildPool = new int[] { 49, 50, 10, 11, 64, 55, 46 };
                minLevel = 11;
                maxLevel = 15;
                break;
            case "ROCHE_HAUTE":
                wildPool = new int[] { 10, 49 };
                minLevel = 12;
                maxLevel = 14;
                break;
            case "ROUTE_5":
                // Fire/Earth types — volcanic path
                wildPool = new int[] { 75, 49, 10, 46, 47, 1, 67 };
                minLevel = 14;
                maxLevel = 18;
                break;
            case "VOLCANVILLE":
                wildPool = new int[] { 75, 1 };
                minLevel = 15;
                maxLevel = 17;
                break;
            case "ROUTE_6":
                // Ice/Water types — icy path
                wildPool = new int[] { 63, 64, 65, 66, 67, 68, 69, 70 };
                minLevel = 18;
                maxLevel = 22;
                break;
            case "CRYO_CITE":
                wildPool = new int[] { 63, 64 };
                minLevel = 19;
                maxLevel = 21;
                break;
            case "ROUTE_7":
                // Electric/Metal types — electric plains
                wildPool = new int[] { 71, 72, 73, 74, 75, 76, 77, 78 };
                minLevel = 22;
                maxLevel = 26;
                break;
            case "ELECTROPOLIS":
                wildPool = new int[] { 71, 72 };
                minLevel = 23;
                maxLevel = 25;
                break;
            case "ROUTE_8":
                // Venom/Shadow types — toxic swamp
                wildPool = new int[] { 79, 80, 81, 82, 83, 84, 85, 86 };
                minLevel = 26;
                maxLevel = 30;
                break;
            case "MARAIS_NOIR":
                wildPool = new int[] { 79, 80 };
                minLevel = 27;
                maxLevel = 29;
                break;
            case "ROUTE_9":
                // Air/Light types — sky path
                wildPool = new int[] { 87, 88, 89, 90, 91, 92, 93, 94, 95 };
                minLevel = 30;
                maxLevel = 36;
                break;
            case "CIEL_HAUT":
                wildPool = new int[] { 87, 88 };
                minLevel = 31;
                maxLevel = 34;
                break;
            case "VICTORY_ROAD":
                // Mixed types — final challenge
                wildPool = new int[] { 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110 };
                minLevel = 36;
                maxLevel = 42;
                break;
            case "BOURG_NID":
            default:
                wildPool = new int[] { 37, 38 };
                minLevel = 2;
                maxLevel = 4;
                break;
        }

        int speciesId = wildPool[Random.Range(0, wildPool.Length)];
        int level = Random.Range(minLevel, maxLevel + 1);

        var setup = new BattleSetupData
        {
            isWild = true,
            enemySpeciesId = speciesId,
            enemyLevel = level,
            returnScene = currentMapId,
            returnPosition = new Vector2(player.gridX, player.gridY)
        };

        // skipFade: true because WildEncounterFlash already faded to black
        GameManager.Instance.StartBattle(setup, skipFade: true);
    }

    // =========================================================================
    //  NPC Spawning
    // =========================================================================

    NPCController SpawnNPC(string npcName, int x, int y, string[] dialogue, Color color)
    {
        var go = new GameObject($"NPC_{npcName}");
        go.transform.SetParent(tileParent);
        var npc = go.AddComponent<NPCController>();
        npc.Init(npcName, x, y, dialogue, color);
        npcs.Add(npc);
        return npc;
    }

    NPCController SpawnTrainer(string npcName, int x, int y, string[] dialogue, Color color,
                                string trainerId, int[] speciesIds, int[] levels)
    {
        var go = new GameObject($"NPC_{npcName}");
        go.transform.SetParent(tileParent);
        var npc = go.AddComponent<NPCController>();
        npc.InitTrainer(npcName, x, y, dialogue, color, trainerId);
        npc.SetTrainerBattle(speciesIds, levels);
        npcs.Add(npc);
        return npc;
    }

    // =========================================================================
    //  Cleanup
    // =========================================================================

    /// <summary>
    /// Get the tile parent GameObject for hiding/showing the overworld.
    /// Used by InteriorManager when entering buildings.
    /// </summary>
    public GameObject GetTileParent()
    {
        return tileParent != null ? tileParent.gameObject : null;
    }

    void ClearMap()
    {
        if (tileParent != null)
            Destroy(tileParent.gameObject);

        tileObjects = null;
        tileMap = null;
        npcs.Clear();
        doors.Clear();
        signs.Clear();
        mapTransitions.Clear();
    }
}

// =========================================================================
//  Supporting Data
// =========================================================================

[System.Serializable]
public class DoorData
{
    public string targetMap;
    public int spawnX;
    public int spawnY;
    public bool isDinoCenter;
    public bool isShop;
    public bool isGym;
    public int gymId = -1;
}

[System.Serializable]
public class MapTransition
{
    public string targetMap;
    public int spawnX;
    public int spawnY;
}
