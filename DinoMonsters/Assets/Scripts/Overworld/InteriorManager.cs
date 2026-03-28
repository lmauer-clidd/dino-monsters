using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages building interiors as in-place 3D rooms within the overworld scene.
/// When a player enters a building, the overworld is hidden and a procedural
/// interior room is generated. The player can move around and interact with
/// NPCs inside. Stepping on the exit door tile returns to the overworld.
/// </summary>
public class InteriorManager : MonoBehaviour
{
    public static InteriorManager Instance { get; private set; }

    // Interior root object (holds all interior geometry and NPCs)
    private GameObject interiorRoot;
    private bool isInInterior = false;
    private string currentInteriorType;
    internal Vector3 returnPosition;
    internal string returnMapId;

    // Interior tile map (for walkability checks)
    private int[,] interiorTileMap;
    private int interiorWidth;
    private int interiorHeight;

    // Interior NPCs
    private List<InteriorNPC> interiorNPCs = new List<InteriorNPC>();

    // Exit door position
    private Vector2Int exitDoorPos;

    // Reference to hidden overworld
    private GameObject hiddenOverworldParent;

    // Tile type constants (mirrors OverworldManager)
    private const int FLOOR = 0;
    private const int WALL = 1;
    private const int COUNTER = 2;
    private const int FURNITURE = 3;
    private const int EXIT_DOOR = 4;
    private const int NPC_BLOCK = 5; // NPC position, walkable but interactable

    // =========================================================================
    //  Singleton
    // =========================================================================

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // =========================================================================
    //  Public API
    // =========================================================================

    /// <summary>
    /// Is the player currently inside a building?
    /// </summary>
    public bool IsInInterior => isInInterior;

    /// <summary>
    /// Enter a building interior. Hides the overworld and generates a room.
    /// </summary>
    /// <param name="type">Interior type: "dinocenter", "shop", "house", "gym"</param>
    /// <param name="mapId">The overworld map the player came from</param>
    /// <param name="returnPos">Grid position to return to when exiting</param>
    /// <param name="gymId">Gym ID (only used for gym interiors)</param>
    public void EnterBuilding(string type, string mapId, Vector3 returnPos, int gymId = -1)
    {
        if (isInInterior) return;

        isInInterior = true;
        currentInteriorType = type;
        returnMapId = mapId;
        returnPosition = returnPos;

        // Hide overworld tiles
        var overworldManager = OverworldManager.Instance;
        if (overworldManager != null)
        {
            hiddenOverworldParent = overworldManager.GetTileParent();
            if (hiddenOverworldParent != null)
                hiddenOverworldParent.SetActive(false);
        }

        var player = overworldManager?.player;

        // Create interior
        interiorRoot = new GameObject("InteriorRoot");

        // Generate room based on type
        switch (type.ToLower())
        {
            case "dinocenter":
                GenerateDinoCenter();
                break;
            case "shop":
                GenerateShop();
                break;
            case "house":
                GenerateHouse();
                break;
            case "gym":
                GenerateGym(gymId);
                break;
            default:
                GenerateHouse();
                break;
        }

        // Position player at entrance (exit door position + 1 tile up)
        if (player != null)
        {
            player.SetGridPosition(exitDoorPos.x, exitDoorPos.y + 1);
            // UnlockInput is handled by the calling coroutine (EnterBuildingWithFade)
        }

        // Zoom camera in for interior
        var cam = Object.FindObjectOfType<OverworldCamera>();
        if (cam != null) cam.ZoomIn(0.6f);

        Debug.Log($"[InteriorManager] Entered {type} interior from {mapId}");
    }

    /// <summary>
    /// Exit the current building and return to the overworld.
    /// </summary>
    public void ExitBuilding()
    {
        if (!isInInterior) return;

        isInInterior = false;

        // Cleanup interior
        interiorNPCs.Clear();
        interiorTileMap = null;

        if (interiorRoot != null)
            Destroy(interiorRoot);
        interiorRoot = null;

        // Show overworld again
        if (hiddenOverworldParent != null)
        {
            hiddenOverworldParent.SetActive(true);
            hiddenOverworldParent = null;
        }

        // Reposition player at return position and sync GameState
        // returnPosition is Vector3(doorX, 0, doorY) — grid Y is in .z, not .y
        int retX = (int)returnPosition.x;
        int retY = (int)returnPosition.z;
        var player = OverworldManager.Instance?.player;
        if (player != null)
        {
            player.SetGridPosition(retX, retY);
        }
        GameState.Instance.PlayerX = retX;
        GameState.Instance.PlayerY = retY;
        GameState.Instance.CurrentMapId = returnMapId;

        // Zoom camera back out
        var cam = Object.FindObjectOfType<OverworldCamera>();
        if (cam != null) cam.ZoomOut();

        Debug.Log($"[InteriorManager] Exited to {returnMapId} at {returnPosition}");
    }

    /// <summary>
    /// Check if a position is walkable within the current interior.
    /// </summary>
    public bool IsWalkable(int x, int y)
    {
        if (!isInInterior || interiorTileMap == null) return false;
        if (x < 0 || x >= interiorWidth || y < 0 || y >= interiorHeight) return false;

        int tile = interiorTileMap[x, y];
        return tile == FLOOR || tile == EXIT_DOOR || tile == NPC_BLOCK;
    }

    /// <summary>
    /// Called when the player steps onto a tile inside the interior.
    /// </summary>
    public void OnPlayerStep(int x, int y)
    {
        if (!isInInterior) return;

        // Check if player stepped on exit door
        if (x == exitDoorPos.x && y == exitDoorPos.y)
        {
            ExitBuilding();
        }
    }

    /// <summary>
    /// Try to interact with an interior NPC at the given position.
    /// Returns true if an interaction occurred.
    /// </summary>
    public bool TryInteract(int x, int y)
    {
        if (!isInInterior) return false;

        foreach (var npc in interiorNPCs)
        {
            if (npc.gridX == x && npc.gridY == y)
            {
                npc.Interact();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Check if there is something interactable at a position inside the interior.
    /// </summary>
    public bool HasInteractable(int x, int y)
    {
        if (!isInInterior) return false;

        foreach (var npc in interiorNPCs)
        {
            if (npc.gridX == x && npc.gridY == y)
                return true;
        }
        return false;
    }

    // =========================================================================
    //  DINO CENTER INTERIOR (8x6)
    // =========================================================================

    private void GenerateDinoCenter()
    {
        interiorWidth = 8;
        interiorHeight = 6;
        interiorTileMap = new int[interiorWidth, interiorHeight];

        // Fill floor
        for (int x = 0; x < interiorWidth; x++)
            for (int y = 0; y < interiorHeight; y++)
                interiorTileMap[x, y] = FLOOR;

        // Walls on edges
        for (int x = 0; x < interiorWidth; x++)
        {
            interiorTileMap[x, interiorHeight - 1] = WALL; // top wall
        }
        for (int y = 0; y < interiorHeight; y++)
        {
            interiorTileMap[0, y] = WALL;                    // left wall
            interiorTileMap[interiorWidth - 1, y] = WALL;    // right wall
        }

        // Exit door at bottom center
        exitDoorPos = new Vector2Int(interiorWidth / 2, 0);
        interiorTileMap[exitDoorPos.x, exitDoorPos.y] = EXIT_DOOR;
        // Make adjacent tile also an exit for wider door feel
        interiorTileMap[exitDoorPos.x - 1, exitDoorPos.y] = WALL;
        interiorTileMap[exitDoorPos.x + 1, exitDoorPos.y] = WALL;

        // Reception counter across the back (y=4, x=2 to x=5)
        for (int x = 2; x <= 5; x++)
        {
            interiorTileMap[x, 4] = COUNTER;
        }

        // Healing machine position (on counter)
        interiorTileMap[5, 4] = COUNTER;

        // PC terminal on the side
        interiorTileMap[1, 3] = FURNITURE;

        // Build 3D geometry
        BuildInteriorTiles(
            floorColor: new Color(0.92f, 0.90f, 0.85f),     // White/cream tile
            wallColor: new Color(0.85f, 0.82f, 0.78f),       // Light walls
            accentColor: new Color(0.90f, 0.40f, 0.45f)      // Pink/red accent (dino center)
        );

        // Reception counter (long brown surface)
        CreateFurniture(2, 4, 4f, 0.6f, 0.8f, new Color(0.55f, 0.35f, 0.20f), "Counter");

        // Healing machine on counter (green glowing cube)
        CreateGlowingProp(5, 4, 0.3f, 0.4f, 0.3f, new Color(0.2f, 0.9f, 0.3f), 1.5f, "HealMachine");

        // PC terminal
        CreateFurniture(1, 3, 0.5f, 0.8f, 0.4f, new Color(0.25f, 0.25f, 0.30f), "PCTerminal");
        // Screen glow
        CreateGlowingProp(1, 3, 0.35f, 0.3f, 0.05f, new Color(0.3f, 0.5f, 0.9f), 1.0f, "PCScreen",
            yOffset: 0.65f, zOffset: 0.15f);

        // Exit door mat
        CreateFloorDecor(exitDoorPos.x, exitDoorPos.y, 0.8f, 0.02f, 0.8f,
            new Color(0.75f, 0.25f, 0.25f), "ExitMat");

        // Spawn NPCs

        // Nurse behind counter
        var nurse = SpawnInteriorNPC("Infirmiere", 3, 4, new Color(0.95f, 0.60f, 0.65f));
        nurse.SetInteraction(InteriorNPC.InteractionType.DinoCenterHeal);
        interiorTileMap[3, 4] = COUNTER; // Keep counter tile

        // Decorative NPC 1 - waiting visitor
        var visitor = SpawnInteriorNPC("Visiteur", 2, 2,
            new Color(0.45f, 0.55f, 0.70f));
        visitor.SetDialogue(new[] {
            "J'attends que mes dinos soient soignes...",
            "Le Centre Dino est vraiment pratique !"
        });

        // Decorative NPC 2 - another visitor
        var visitor2 = SpawnInteriorNPC("Dresseur", 5, 2,
            new Color(0.65f, 0.45f, 0.30f));
        visitor2.SetDialogue(new[] {
            "Mon Triceraptor s'est bien battu aujourd'hui !",
            "Tu devrais explorer les grottes au nord."
        });

        // Top wall decorations (dino center sign)
        CreateWallSign(interiorWidth / 2, interiorHeight - 1, new Color(0.90f, 0.40f, 0.45f), "+");
    }

    // =========================================================================
    //  SHOP INTERIOR (8x6)
    // =========================================================================

    private void GenerateShop()
    {
        interiorWidth = 8;
        interiorHeight = 6;
        interiorTileMap = new int[interiorWidth, interiorHeight];

        // Fill floor
        for (int x = 0; x < interiorWidth; x++)
            for (int y = 0; y < interiorHeight; y++)
                interiorTileMap[x, y] = FLOOR;

        // Walls
        for (int x = 0; x < interiorWidth; x++)
            interiorTileMap[x, interiorHeight - 1] = WALL;
        for (int y = 0; y < interiorHeight; y++)
        {
            interiorTileMap[0, y] = WALL;
            interiorTileMap[interiorWidth - 1, y] = WALL;
        }

        // Exit door
        exitDoorPos = new Vector2Int(interiorWidth / 2, 0);
        interiorTileMap[exitDoorPos.x, exitDoorPos.y] = EXIT_DOOR;
        interiorTileMap[exitDoorPos.x - 1, exitDoorPos.y] = WALL;
        interiorTileMap[exitDoorPos.x + 1, exitDoorPos.y] = WALL;

        // Shop counter (y=3, x=2 to x=5)
        for (int x = 2; x <= 5; x++)
            interiorTileMap[x, 3] = COUNTER;

        // Shelves behind counter (back wall)
        for (int x = 1; x <= 6; x++)
            interiorTileMap[x, 4] = FURNITURE;

        // Build 3D
        BuildInteriorTiles(
            floorColor: new Color(0.60f, 0.45f, 0.30f),     // Wooden floor
            wallColor: new Color(0.70f, 0.60f, 0.45f),       // Warm walls
            accentColor: new Color(0.30f, 0.55f, 0.75f)      // Blue accent (shop)
        );

        // Shop counter
        CreateFurniture(2, 3, 4f, 0.6f, 0.8f, new Color(0.50f, 0.35f, 0.20f), "ShopCounter");

        // Back shelves with colored cubes (items)
        for (int x = 1; x <= 6; x++)
        {
            CreateFurniture(x, 4, 0.7f, 1.2f, 0.5f, new Color(0.45f, 0.35f, 0.25f), $"Shelf_{x}");

            // Item cubes on shelves
            Color itemColor = x % 3 == 0 ? new Color(0.85f, 0.30f, 0.30f) :
                              x % 3 == 1 ? new Color(0.30f, 0.55f, 0.85f) :
                                           new Color(0.30f, 0.75f, 0.40f);
            CreateProp(x, 4, 0.2f, 0.2f, 0.2f, itemColor, $"Item_{x}",
                yOffset: 0.8f);
        }

        // Exit door mat
        CreateFloorDecor(exitDoorPos.x, exitDoorPos.y, 0.8f, 0.02f, 0.8f,
            new Color(0.30f, 0.55f, 0.75f), "ExitMat");

        // Shopkeeper NPC
        var shopkeeper = SpawnInteriorNPC("Vendeur", 3, 3, new Color(0.35f, 0.60f, 0.35f));
        shopkeeper.SetInteraction(InteriorNPC.InteractionType.Shop);
        interiorTileMap[3, 3] = COUNTER;

        // Browsing customer
        var customer = SpawnInteriorNPC("Client", 5, 1, new Color(0.50f, 0.40f, 0.60f));
        customer.SetDialogue(new[] {
            "Les Super Balls sont un bon investissement !",
            "N'oublie pas d'acheter des potions avant de partir."
        });
    }

    // =========================================================================
    //  HOUSE INTERIOR (6x5)
    // =========================================================================

    private void GenerateHouse()
    {
        interiorWidth = 6;
        interiorHeight = 5;
        interiorTileMap = new int[interiorWidth, interiorHeight];

        // Fill floor
        for (int x = 0; x < interiorWidth; x++)
            for (int y = 0; y < interiorHeight; y++)
                interiorTileMap[x, y] = FLOOR;

        // Walls
        for (int x = 0; x < interiorWidth; x++)
            interiorTileMap[x, interiorHeight - 1] = WALL;
        for (int y = 0; y < interiorHeight; y++)
        {
            interiorTileMap[0, y] = WALL;
            interiorTileMap[interiorWidth - 1, y] = WALL;
        }

        // Exit door
        exitDoorPos = new Vector2Int(interiorWidth / 2, 0);
        interiorTileMap[exitDoorPos.x, exitDoorPos.y] = EXIT_DOOR;
        interiorTileMap[exitDoorPos.x - 1, exitDoorPos.y] = WALL;
        interiorTileMap[exitDoorPos.x + 1, exitDoorPos.y] = WALL;

        // Furniture positions
        interiorTileMap[1, 3] = FURNITURE; // Bookshelf
        interiorTileMap[4, 3] = FURNITURE; // Bed
        interiorTileMap[2, 2] = FURNITURE; // Table
        interiorTileMap[3, 2] = FURNITURE; // Chair

        // Build 3D
        BuildInteriorTiles(
            floorColor: new Color(0.55f, 0.42f, 0.28f),     // Wooden floor
            wallColor: new Color(0.75f, 0.68f, 0.55f),       // Cozy walls
            accentColor: new Color(0.65f, 0.50f, 0.35f)      // Warm accent
        );

        // Bookshelf
        CreateFurniture(1, 3, 0.7f, 1.3f, 0.5f, new Color(0.40f, 0.28f, 0.18f), "Bookshelf");
        // Books on shelf (colored spines)
        for (int i = 0; i < 3; i++)
        {
            Color bookColor = i == 0 ? new Color(0.8f, 0.2f, 0.2f) :
                              i == 1 ? new Color(0.2f, 0.5f, 0.8f) :
                                       new Color(0.2f, 0.7f, 0.3f);
            CreateProp(1, 3, 0.12f, 0.25f, 0.08f, bookColor, $"Book_{i}",
                xOffset: (i - 1) * 0.18f, yOffset: 0.85f);
        }

        // Bed
        CreateFurniture(4, 3, 0.9f, 0.35f, 1.2f, new Color(0.75f, 0.70f, 0.60f), "BedFrame");
        // Pillow
        CreateProp(4, 3, 0.3f, 0.15f, 0.25f, Color.white, "Pillow",
            yOffset: 0.35f, zOffset: 0.3f);
        // Blanket
        CreateProp(4, 3, 0.7f, 0.08f, 0.8f, new Color(0.30f, 0.45f, 0.70f), "Blanket",
            yOffset: 0.30f);

        // Table
        CreateFurniture(2, 2, 0.8f, 0.5f, 0.8f, new Color(0.50f, 0.38f, 0.25f), "Table");

        // Chair
        CreateFurniture(3, 2, 0.35f, 0.45f, 0.35f, new Color(0.45f, 0.33f, 0.22f), "Chair");
        // Chair back
        CreateProp(3, 2, 0.35f, 0.35f, 0.05f, new Color(0.45f, 0.33f, 0.22f), "ChairBack",
            yOffset: 0.45f, zOffset: -0.15f);

        // Exit mat
        CreateFloorDecor(exitDoorPos.x, exitDoorPos.y, 0.7f, 0.02f, 0.7f,
            new Color(0.65f, 0.50f, 0.35f), "ExitMat");

        // Optional NPC (50% chance)
        if (Random.value > 0.5f)
        {
            var resident = SpawnInteriorNPC("Habitant", 2, 1, new Color(0.60f, 0.50f, 0.40f));
            resident.SetDialogue(new[] {
                "Bienvenue chez moi ! Fais comme chez toi.",
                "As-tu vu les dinos rares sur la route au nord ?",
                "Mon grand-pere etait un champion dino autrefois !"
            });
        }

        // Hidden item on bookshelf (check flag to prevent duplicate pickup)
        string itemFlag = $"house_item_{returnMapId}";
        if (!GameState.Instance.HasFlag(itemFlag))
        {
            var bookshelfNPC = SpawnInteriorNPC("Etagere", 1, 3,
                new Color(0.40f, 0.28f, 0.18f), isObject: true);
            bookshelfNPC.SetInteraction(InteriorNPC.InteractionType.HiddenItem);
            bookshelfNPC.hiddenItemId = 1; // Potion
            bookshelfNPC.hiddenItemFlag = itemFlag;
        }
    }

    // =========================================================================
    //  GYM INTERIOR (10x8)
    // =========================================================================

    private void GenerateGym(int gymId)
    {
        interiorWidth = 10;
        interiorHeight = 8;
        interiorTileMap = new int[interiorWidth, interiorHeight];

        // Fill floor
        for (int x = 0; x < interiorWidth; x++)
            for (int y = 0; y < interiorHeight; y++)
                interiorTileMap[x, y] = FLOOR;

        // Walls
        for (int x = 0; x < interiorWidth; x++)
            interiorTileMap[x, interiorHeight - 1] = WALL;
        for (int y = 0; y < interiorHeight; y++)
        {
            interiorTileMap[0, y] = WALL;
            interiorTileMap[interiorWidth - 1, y] = WALL;
        }

        // Exit door
        exitDoorPos = new Vector2Int(interiorWidth / 2, 0);
        interiorTileMap[exitDoorPos.x, exitDoorPos.y] = EXIT_DOOR;
        interiorTileMap[exitDoorPos.x - 1, exitDoorPos.y] = WALL;
        interiorTileMap[exitDoorPos.x + 1, exitDoorPos.y] = WALL;

        // Arena barriers on sides (decorative walls)
        for (int y = 2; y <= 5; y++)
        {
            interiorTileMap[1, y] = FURNITURE;
            interiorTileMap[interiorWidth - 2, y] = FURNITURE;
        }

        // Gym type color based on gymId
        Color gymColor = GetGymColor(gymId);
        Color gymFloorColor = Color.Lerp(new Color(0.50f, 0.48f, 0.45f), gymColor, 0.3f);

        // Build 3D
        BuildInteriorTiles(
            floorColor: gymFloorColor,
            wallColor: new Color(0.55f, 0.50f, 0.45f),
            accentColor: gymColor
        );

        // Arena floor pattern (colored tiles in center)
        for (int x = 2; x <= interiorWidth - 3; x++)
        {
            for (int y = 2; y <= 5; y++)
            {
                if ((x + y) % 2 == 0)
                {
                    CreateFloorDecor(x, y, 0.9f, 0.015f, 0.9f,
                        Color.Lerp(gymColor, Color.white, 0.5f),
                        $"ArenaPattern_{x}_{y}");
                }
            }
        }

        // Arena barriers (decorative pillars)
        for (int y = 2; y <= 5; y++)
        {
            CreateFurniture(1, y, 0.5f, 1.0f, 0.5f, new Color(0.40f, 0.38f, 0.35f), $"BarrierL_{y}");
            CreateFurniture(interiorWidth - 2, y, 0.5f, 1.0f, 0.5f, new Color(0.40f, 0.38f, 0.35f), $"BarrierR_{y}");
        }

        // Exit mat
        CreateFloorDecor(exitDoorPos.x, exitDoorPos.y, 0.8f, 0.02f, 0.8f,
            gymColor * 0.6f, "ExitMat");

        // Gym leader platform at the back
        CreateFloorDecor(interiorWidth / 2, 6, 2.5f, 0.08f, 1.0f,
            gymColor, "LeaderPlatform");

        // Badge display case near entrance
        CreateFurniture(interiorWidth - 2, 1, 0.5f, 0.6f, 0.3f,
            new Color(0.70f, 0.65f, 0.55f), "BadgeCase");

        // Spawn gym leader at the back
        string leaderName = GetGymLeaderName(gymId);
        var leader = SpawnInteriorNPC(leaderName, interiorWidth / 2, 6,
            gymColor);
        leader.SetInteraction(InteriorNPC.InteractionType.GymLeader);
        leader.gymId = gymId;

        // Spawn 1-2 gym trainers blocking the path
        string trainer1Id = $"GYM_{gymId}_TRAINER_0";
        if (!GameState.Instance.IsTrainerDefeated(trainer1Id))
        {
            var trainer1 = SpawnInteriorNPC("Dresseur", interiorWidth / 2 - 1, 3,
                Color.Lerp(gymColor, new Color(0.5f, 0.5f, 0.5f), 0.4f));
            trainer1.SetInteraction(InteriorNPC.InteractionType.GymTrainer);
            trainer1.trainerId = trainer1Id;
            trainer1.gymId = gymId;
            trainer1.trainerIndex = 0;
        }

        if (gymId >= 2) // Second trainer for gyms 2+
        {
            string trainer2Id = $"GYM_{gymId}_TRAINER_1";
            if (!GameState.Instance.IsTrainerDefeated(trainer2Id))
            {
                var trainer2 = SpawnInteriorNPC("Dresseur", interiorWidth / 2 + 1, 5,
                    Color.Lerp(gymColor, new Color(0.5f, 0.5f, 0.5f), 0.4f));
                trainer2.SetInteraction(InteriorNPC.InteractionType.GymTrainer);
                trainer2.trainerId = trainer2Id;
                trainer2.gymId = gymId;
                trainer2.trainerIndex = 1;
            }
        }

        // Gym sign on back wall
        CreateWallSign(interiorWidth / 2, interiorHeight - 1, gymColor, "GYM");
    }

    // =========================================================================
    //  Gym Helper Methods
    // =========================================================================

    private Color GetGymColor(int gymId)
    {
        switch (gymId)
        {
            case 0: return new Color(0.30f, 0.70f, 0.30f); // Flora - green
            case 1: return new Color(0.30f, 0.50f, 0.85f); // Marin - blue
            case 2: return new Color(0.65f, 0.55f, 0.40f); // Petra - brown/fossil
            case 3: return new Color(0.85f, 0.30f, 0.20f); // Brazier - red/fire
            case 4: return new Color(0.60f, 0.80f, 0.95f); // Givralia - ice blue
            case 5: return new Color(0.90f, 0.80f, 0.20f); // Voltaire - yellow
            case 6: return new Color(0.70f, 0.30f, 0.70f); // Toxica - purple
            case 7: return new Color(0.75f, 0.85f, 0.95f); // Celesta - sky
            default: return new Color(0.50f, 0.50f, 0.50f);
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

    // =========================================================================
    //  Room Geometry Building
    // =========================================================================

    /// <summary>
    /// Build 3D tiles for the interior room.
    /// </summary>
    private void BuildInteriorTiles(Color floorColor, Color wallColor, Color accentColor)
    {
        for (int x = 0; x < interiorWidth; x++)
        {
            for (int y = 0; y < interiorHeight; y++)
            {
                int tileType = interiorTileMap[x, y];
                Vector3 pos = new Vector3(x, 0, y);

                // Always create a floor tile
                CreateTileGeometry(pos, floorColor, "Floor", 1f, 0.05f, 1f, 0f);

                // Additional geometry based on type
                switch (tileType)
                {
                    case WALL:
                        CreateTileGeometry(pos, wallColor, "Wall", 1f, 0.8f, 1f, 0.4f);
                        break;
                    case EXIT_DOOR:
                        // Door frame (darker area)
                        CreateTileGeometry(pos, new Color(0.20f, 0.15f, 0.10f), "Door",
                            0.9f, 0.05f, 0.9f, 0.001f);
                        // Arrow indicator on floor
                        CreateArrowIndicator(x, y, accentColor);
                        break;
                }
            }
        }

        // Accent trim on top of walls
        for (int x = 0; x < interiorWidth; x++)
        {
            if (interiorTileMap[x, interiorHeight - 1] == WALL)
            {
                Vector3 trimPos = new Vector3(x, 0.75f, interiorHeight - 1);
                CreateTileGeometry(trimPos, accentColor, "WallTrim",
                    1.02f, 0.08f, 1.02f, 0f, setY: false);
            }
        }

        // No ceiling — top-down camera needs to see inside the room

        // Ambient light boost for interiors
        var lightGO = new GameObject("InteriorLight");
        lightGO.transform.SetParent(interiorRoot.transform);
        lightGO.transform.position = new Vector3(
            interiorWidth / 2f, 1.8f, interiorHeight / 2f);
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.95f, 0.85f);
        light.intensity = 1.2f;
        light.range = Mathf.Max(interiorWidth, interiorHeight) * 1.5f;
    }

    /// <summary>
    /// Create a single tile piece of geometry.
    /// </summary>
    private void CreateTileGeometry(Vector3 position, Color color, string name,
        float scaleX, float scaleY, float scaleZ, float yOffset, bool setY = true)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(interiorRoot.transform);

        if (setY)
            go.transform.localPosition = new Vector3(position.x, yOffset, position.z);
        else
            go.transform.localPosition = position;

        go.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
        go.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetSolidColor(color);
        Object.Destroy(go.GetComponent<Collider>());
    }

    /// <summary>
    /// Create a directional arrow on the exit door tile.
    /// </summary>
    private void CreateArrowIndicator(int x, int y, Color color)
    {
        // Simple down-arrow made of small cubes
        float baseY = 0.06f;
        Vector3 center = new Vector3(x, baseY, y);

        // Arrow shaft
        var shaft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shaft.name = "ExitArrowShaft";
        shaft.transform.SetParent(interiorRoot.transform);
        shaft.transform.localPosition = center + new Vector3(0, 0, 0.1f);
        shaft.transform.localScale = new Vector3(0.12f, 0.02f, 0.35f);
        shaft.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetEmissive(color, 0.8f);
        Object.Destroy(shaft.GetComponent<Collider>());

        // Arrow head left
        var headL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        headL.name = "ExitArrowL";
        headL.transform.SetParent(interiorRoot.transform);
        headL.transform.localPosition = center + new Vector3(-0.12f, 0, -0.05f);
        headL.transform.localScale = new Vector3(0.12f, 0.02f, 0.12f);
        headL.transform.localRotation = Quaternion.Euler(0, 45, 0);
        headL.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetEmissive(color, 0.8f);
        Object.Destroy(headL.GetComponent<Collider>());

        // Arrow head right
        var headR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        headR.name = "ExitArrowR";
        headR.transform.SetParent(interiorRoot.transform);
        headR.transform.localPosition = center + new Vector3(0.12f, 0, -0.05f);
        headR.transform.localScale = new Vector3(0.12f, 0.02f, 0.12f);
        headR.transform.localRotation = Quaternion.Euler(0, -45, 0);
        headR.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetEmissive(color, 0.8f);
        Object.Destroy(headR.GetComponent<Collider>());
    }

    // =========================================================================
    //  Furniture & Prop Helpers
    // =========================================================================

    private void CreateFurniture(int x, int y, float sizeX, float sizeY, float sizeZ,
        Color color, string name)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(interiorRoot.transform);
        go.transform.localPosition = new Vector3(x, sizeY / 2f, y);
        go.transform.localScale = new Vector3(sizeX, sizeY, sizeZ);
        go.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetSolidColor(color);
        Object.Destroy(go.GetComponent<Collider>());
    }

    private void CreateGlowingProp(int x, int y, float sizeX, float sizeY, float sizeZ,
        Color color, float intensity, string name,
        float yOffset = 0.6f, float zOffset = 0f)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(interiorRoot.transform);
        go.transform.localPosition = new Vector3(x, yOffset, y + zOffset);
        go.transform.localScale = new Vector3(sizeX, sizeY, sizeZ);
        go.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetEmissive(color, intensity);
        Object.Destroy(go.GetComponent<Collider>());

        // Optional point light for glow effect
        var lightGO = new GameObject($"{name}_Light");
        lightGO.transform.SetParent(go.transform);
        lightGO.transform.localPosition = Vector3.zero;
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity * 0.4f;
        light.range = 2f;
    }

    private void CreateProp(int x, int y, float sizeX, float sizeY, float sizeZ,
        Color color, string name,
        float xOffset = 0f, float yOffset = 0f, float zOffset = 0f)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(interiorRoot.transform);
        go.transform.localPosition = new Vector3(x + xOffset, yOffset, y + zOffset);
        go.transform.localScale = new Vector3(sizeX, sizeY, sizeZ);
        go.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetSolidColor(color);
        Object.Destroy(go.GetComponent<Collider>());
    }

    private void CreateFloorDecor(int x, int y, float sizeX, float sizeY, float sizeZ,
        Color color, string name)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(interiorRoot.transform);
        go.transform.localPosition = new Vector3(x, 0.026f, y);
        go.transform.localScale = new Vector3(sizeX, sizeY, sizeZ);
        go.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetSolidColor(color);
        Object.Destroy(go.GetComponent<Collider>());
    }

    private void CreateWallSign(int x, int y, Color color, string label)
    {
        // Sign background
        var sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sign.name = "WallSign";
        sign.transform.SetParent(interiorRoot.transform);
        sign.transform.localPosition = new Vector3(x, 1.1f, y - 0.45f);
        sign.transform.localScale = new Vector3(1.2f, 0.5f, 0.05f);
        sign.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetSolidColor(color);
        Object.Destroy(sign.GetComponent<Collider>());

        // Sign emblem (white cross for dino center, etc.)
        var emblem = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        emblem.name = "SignEmblem";
        emblem.transform.SetParent(sign.transform);
        emblem.transform.localPosition = new Vector3(0, 0, -0.6f);
        emblem.transform.localScale = new Vector3(0.4f, 0.6f, 0.5f);
        emblem.GetComponent<Renderer>().sharedMaterial =
            MaterialManager.GetEmissive(Color.white, 0.5f);
        Object.Destroy(emblem.GetComponent<Collider>());
    }

    // =========================================================================
    //  Interior NPC Spawning
    // =========================================================================

    private InteriorNPC SpawnInteriorNPC(string npcName, int x, int y, Color color,
        bool isObject = false)
    {
        var go = new GameObject($"InteriorNPC_{npcName}");
        go.transform.SetParent(interiorRoot.transform);
        var npc = go.AddComponent<InteriorNPC>();
        npc.Init(npcName, x, y, color, isObject);
        interiorNPCs.Add(npc);
        return npc;
    }
}

// =============================================================================
//  Interior NPC — Lightweight NPC for building interiors
// =============================================================================

public class InteriorNPC : MonoBehaviour
{
    public enum InteractionType
    {
        Dialogue,
        DinoCenterHeal,
        Shop,
        GymLeader,
        GymTrainer,
        HiddenItem
    }

    public string npcName;
    public int gridX;
    public int gridY;
    public InteractionType interactionType = InteractionType.Dialogue;
    public int gymId = -1;
    public string trainerId;
    public int trainerIndex;
    public int hiddenItemId;
    public string hiddenItemFlag;
    public bool isObject = false; // If true, this is a furniture piece, not a person

    private string[] dialogueLines;
    private int dialogueIndex = 0;
    private Color bodyColor;

    public void Init(string name, int x, int y, Color color, bool objectMode = false)
    {
        npcName = name;
        gridX = x;
        gridY = y;
        bodyColor = color;
        isObject = objectMode;
        transform.position = new Vector3(x, 0, y);

        if (!isObject)
            BuildNPCModel();
    }

    public void SetDialogue(string[] lines)
    {
        dialogueLines = lines;
        interactionType = InteractionType.Dialogue;
    }

    public void SetInteraction(InteractionType type)
    {
        interactionType = type;
    }

    private void BuildNPCModel()
    {
        // Body cylinder
        var body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = "Body";
        body.transform.SetParent(transform);
        body.transform.localPosition = new Vector3(0, 0.4f, 0);
        body.transform.localScale = new Vector3(0.40f, 0.35f, 0.40f);
        body.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetSolidColor(bodyColor);
        Object.Destroy(body.GetComponent<Collider>());

        // Head sphere
        var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(transform);
        head.transform.localPosition = new Vector3(0, 0.82f, 0);
        head.transform.localScale = new Vector3(0.30f, 0.30f, 0.30f);
        head.GetComponent<Renderer>().sharedMaterial =
            MaterialManager.GetSolidColor(new Color(0.92f, 0.78f, 0.65f));
        Object.Destroy(head.GetComponent<Collider>());

        // Eyes
        var eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        eye.name = "Eyes";
        eye.transform.SetParent(transform);
        eye.transform.localPosition = new Vector3(0, 0.84f, 0.16f);
        eye.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
        eye.GetComponent<Renderer>().sharedMaterial =
            MaterialManager.GetSolidColor(new Color(0.15f, 0.10f, 0.05f));
        Object.Destroy(eye.GetComponent<Collider>());

        // Special markers based on type
        if (interactionType == InteractionType.GymLeader ||
            interactionType == InteractionType.GymTrainer)
        {
            // Exclamation mark above head
            var mark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mark.name = "TrainerMark";
            mark.transform.SetParent(transform);
            mark.transform.localPosition = new Vector3(0, 1.1f, 0);
            mark.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
            mark.GetComponent<Renderer>().sharedMaterial =
                MaterialManager.GetEmissive(new Color(0.9f, 0.2f, 0.2f), 1.0f);
            Object.Destroy(mark.GetComponent<Collider>());
        }
    }

    /// <summary>
    /// Handle interaction when player talks to this NPC.
    /// </summary>
    public void Interact()
    {
        // Face towards the player
        var player = OverworldManager.Instance?.player;

        switch (interactionType)
        {
            case InteractionType.Dialogue:
                HandleDialogue();
                break;
            case InteractionType.DinoCenterHeal:
                HandleDinoCenterHeal();
                break;
            case InteractionType.Shop:
                HandleShopInteraction();
                break;
            case InteractionType.GymLeader:
                HandleGymLeader();
                break;
            case InteractionType.GymTrainer:
                HandleGymTrainer();
                break;
            case InteractionType.HiddenItem:
                HandleHiddenItem();
                break;
        }
    }

    // =========================================================================
    //  Interaction Handlers
    // =========================================================================

    private void HandleDialogue()
    {
        if (dialogueLines == null || dialogueLines.Length == 0) return;

        string line = dialogueLines[dialogueIndex];
        dialogueIndex = (dialogueIndex + 1) % dialogueLines.Length;

        var player = OverworldManager.Instance?.player;
        if (player != null) player.LockInput();

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowText(line, npcName, () =>
            {
                if (player != null) player.UnlockInput();
            });
        }
        else
        {
            Debug.Log($"[{npcName}] {line}");
            if (player != null) player.UnlockInput();
        }
    }

    private void HandleDinoCenterHeal()
    {
        var player = OverworldManager.Instance?.player;
        if (player != null) player.LockInput();

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowText(
                "Bienvenue au Centre Dino !\nJe vais soigner vos compagnons.",
                "INFIRMIERE",
                () =>
                {
                    // Play heal SFX
                    if (AudioManager.Instance != null)
                        AudioManager.Instance.PlayHeal();

                    // Heal all party dinos
                    foreach (var dino in GameState.Instance.Party)
                        dino.FullHeal();

                    // Set heal point
                    GameState.Instance.SetHealPoint(
                        InteriorManager.Instance.returnMapId,
                        (int)InteriorManager.Instance.returnPosition.x,
                        (int)InteriorManager.Instance.returnPosition.y);

                    // Show confirmation
                    DialogueUI.Instance.ShowText(
                        "Vos dinos sont en pleine forme !",
                        "INFIRMIERE",
                        () =>
                        {
                            if (player != null) player.UnlockInput();
                        }
                    );
                }
            );
        }
        else
        {
            foreach (var dino in GameState.Instance.Party)
                dino.FullHeal();
            Debug.Log("[Interior] All dinos healed!");
            if (player != null) player.UnlockInput();
        }
    }

    private void HandleShopInteraction()
    {
        var player = OverworldManager.Instance?.player;
        if (player != null) player.LockInput();

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowChoices(
                "Bienvenue a la boutique !\nQue puis-je faire pour vous ?",
                new[] { "Acheter", "Vendre", "Quitter" },
                (idx) =>
                {
                    if (idx == 0)
                        ShowBuyMenu();
                    else if (idx == 1)
                        ShowSellMenu();
                    else
                        CloseShopDialogue();
                }
            );
        }
        else
        {
            Debug.Log("[Interior] Shop opened");
            if (player != null) player.UnlockInput();
        }
    }

    // Shop item definitions
    private static readonly int[] shopItemIds = { 1, 2, 5, 12, 16, 17 };
    private static readonly string[] shopItemNames =
        { "Potion", "Super Potion", "Antidote", "Rappel", "Jurassic Ball", "Super Ball" };
    private static readonly int[] shopItemPrices = { 300, 700, 100, 1500, 200, 600 };

    private void ShowBuyMenu()
    {
        int count = shopItemIds.Length;
        int money = GameState.Instance.Money;
        string[] choices = new string[count + 1];
        for (int i = 0; i < count; i++)
            choices[i] = $"{shopItemNames[i]} - {shopItemPrices[i]}$";
        choices[count] = "Retour";

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowChoices(
                $"Vous avez {money}$.\nQue voulez-vous acheter ?",
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
                                $"Vous avez achete {itemName} !\nIl vous reste {GameState.Instance.Money}$.",
                                "VENDEUR",
                                () => ShowBuyMenu()
                            );
                        }
                        else
                        {
                            DialogueUI.Instance.ShowText(
                                "Vous n'avez pas assez d'argent !",
                                "VENDEUR",
                                () => ShowBuyMenu()
                            );
                        }
                    }
                    else
                    {
                        HandleShopInteraction(); // Go back to buy/sell/quit
                    }
                }
            );
        }
    }

    private void ShowSellMenu()
    {
        var allItems = GameState.Instance.Inventory.GetAllItems();
        if (allItems.Count == 0)
        {
            if (DialogueUI.Instance != null)
            {
                DialogueUI.Instance.ShowText(
                    "Vous n'avez rien a vendre !",
                    "VENDEUR",
                    () => HandleShopInteraction()
                );
            }
            return;
        }

        // Build sell choices (max 4 items due to DialogueUI MAX_CHOICES)
        int displayCount = Mathf.Min(allItems.Count, 3);
        string[] choices = new string[displayCount + 1];
        for (int i = 0; i < displayCount; i++)
        {
            var item = allItems[i];
            var itemData = DataLoader.Instance?.GetItem(item.Key);
            string name = itemData != null ? itemData.name : $"Objet #{item.Key}";
            int sellPrice = itemData != null ? itemData.price / 2 : 50;
            choices[i] = $"{name} x{item.Value} - {sellPrice}$";
        }
        choices[displayCount] = "Retour";

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowChoices(
                "Que voulez-vous vendre ?",
                choices,
                (idx) =>
                {
                    if (idx >= 0 && idx < displayCount)
                    {
                        var item = allItems[idx];
                        var itemData = DataLoader.Instance?.GetItem(item.Key);
                        int sellPrice = itemData != null ? itemData.price / 2 : 50;
                        string name = itemData != null ? itemData.name : $"Objet #{item.Key}";

                        GameState.Instance.Inventory.RemoveItem(item.Key);
                        GameState.Instance.AddMoney(sellPrice);

                        if (AudioManager.Instance != null)
                            AudioManager.Instance.PlayMenuSelect();

                        DialogueUI.Instance.ShowText(
                            $"Vous avez vendu {name} pour {sellPrice}$ !\nVous avez {GameState.Instance.Money}$.",
                            "VENDEUR",
                            () => ShowSellMenu()
                        );
                    }
                    else
                    {
                        HandleShopInteraction();
                    }
                }
            );
        }
    }

    private void CloseShopDialogue()
    {
        var player = OverworldManager.Instance?.player;

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowText(
                "Merci et a bientot !",
                "VENDEUR",
                () =>
                {
                    if (player != null) player.UnlockInput();
                }
            );
        }
        else
        {
            if (player != null) player.UnlockInput();
        }
    }

    private void HandleGymLeader()
    {
        var player = OverworldManager.Instance?.player;
        if (player != null) player.LockInput();

        // Check if badge already earned
        if (GameState.Instance.HasBadge(gymId))
        {
            if (DialogueUI.Instance != null)
            {
                DialogueUI.Instance.ShowText(
                    "Tu as deja mon badge ! Continue ton voyage !",
                    npcName,
                    () => { if (player != null) player.UnlockInput(); }
                );
            }
            return;
        }

        // Check if gym trainers are still undefeated (block leader access)
        bool trainersDefeated = true;
        for (int i = 0; i < 2; i++)
        {
            string tid = $"GYM_{gymId}_TRAINER_{i}";
            // Only check trainers that exist for this gym
            if (gymId >= 2 || i == 0)
            {
                if (!GameState.Instance.IsTrainerDefeated(tid))
                {
                    trainersDefeated = false;
                    break;
                }
            }
        }

        if (!trainersDefeated)
        {
            if (DialogueUI.Instance != null)
            {
                DialogueUI.Instance.ShowText(
                    "Tu dois d'abord vaincre mes dresseurs avant de m'affronter !",
                    npcName,
                    () => { if (player != null) player.UnlockInput(); }
                );
            }
            return;
        }

        // Start gym battle
        string introDialogue = GetGymLeaderIntro(gymId);

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowText(introDialogue, npcName, () =>
            {
                // Exit interior first, then start battle
                InteriorManager.Instance.ExitBuilding();
                GameManager.Instance.StartGymBattle(gymId);
            });
        }
        else
        {
            InteriorManager.Instance.ExitBuilding();
            GameManager.Instance.StartGymBattle(gymId);
        }
    }

    private void HandleGymTrainer()
    {
        var player = OverworldManager.Instance?.player;

        // Check if already defeated
        if (GameState.Instance.IsTrainerDefeated(trainerId))
        {
            if (player != null) player.LockInput();
            if (DialogueUI.Instance != null)
            {
                DialogueUI.Instance.ShowText(
                    "Tu m'as deja battu ! Le champion t'attend.",
                    npcName,
                    () => { if (player != null) player.UnlockInput(); }
                );
            }
            return;
        }

        if (player != null) player.LockInput();

        // Gym trainer battle setup
        int[] speciesIds = GetGymTrainerSpecies(gymId, trainerIndex);
        int[] levels = GetGymTrainerLevels(gymId, trainerIndex);

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowText(
                "Tu veux defier le champion ? Il faudra d'abord me vaincre !",
                npcName,
                () =>
                {
                    // Exit interior and start trainer battle
                    InteriorManager.Instance.ExitBuilding();

                    var setup = new BattleSetupData
                    {
                        isWild = false,
                        isGymLeader = false,
                        trainerId = trainerId,
                        trainerName = npcName,
                        trainerParty = new List<TrainerDinoEntry>(),
                        returnScene = GameState.Instance.CurrentMapId,
                        returnPosition = new Vector2(
                            GameState.Instance.PlayerX,
                            GameState.Instance.PlayerY)
                    };

                    for (int i = 0; i < speciesIds.Length; i++)
                    {
                        setup.trainerParty.Add(new TrainerDinoEntry
                        {
                            speciesId = speciesIds[i],
                            level = levels[i]
                        });
                    }

                    GameManager.Instance.StartBattle(setup);
                }
            );
        }
    }

    private void HandleHiddenItem()
    {
        var player = OverworldManager.Instance?.player;
        if (player != null) player.LockInput();

        if (!string.IsNullOrEmpty(hiddenItemFlag) && GameState.Instance.HasFlag(hiddenItemFlag))
        {
            // Already picked up
            if (DialogueUI.Instance != null)
            {
                DialogueUI.Instance.ShowText(
                    "Il n'y a rien d'autre ici.",
                    null,
                    () => { if (player != null) player.UnlockInput(); }
                );
            }
            return;
        }

        // Give item
        GameState.Instance.Inventory.AddItem(hiddenItemId);
        if (!string.IsNullOrEmpty(hiddenItemFlag))
            GameState.Instance.SetFlag(hiddenItemFlag);

        var itemData = DataLoader.Instance?.GetItem(hiddenItemId);
        string itemName = itemData != null ? itemData.name : "un objet";

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMenuSelect();

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowText(
                $"Vous avez trouve {itemName} !",
                null,
                () => { if (player != null) player.UnlockInput(); }
            );
        }
        else
        {
            Debug.Log($"[Interior] Found hidden item: {itemName}");
            if (player != null) player.UnlockInput();
        }
    }

    // =========================================================================
    //  Gym Trainer Data
    // =========================================================================

    private int[] GetGymTrainerSpecies(int gymId, int trainerIdx)
    {
        // Simplified trainer teams based on gym type
        switch (gymId)
        {
            case 0: return new[] { 19 };                // Grass
            case 1: return trainerIdx == 0 ? new[] { 4 } : new[] { 12 };     // Water
            case 2: return trainerIdx == 0 ? new[] { 15 } : new[] { 16 };    // Fossil
            case 3: return trainerIdx == 0 ? new[] { 7 } : new[] { 8 };      // Fire
            case 4: return trainerIdx == 0 ? new[] { 10 } : new[] { 11 };    // Ice
            case 5: return trainerIdx == 0 ? new[] { 22 } : new[] { 23 };    // Electric
            case 6: return trainerIdx == 0 ? new[] { 25 } : new[] { 26 };    // Poison
            case 7: return trainerIdx == 0 ? new[] { 28 } : new[] { 29 };    // Flying
            default: return new[] { 1 };
        }
    }

    private int[] GetGymTrainerLevels(int gymId, int trainerIdx)
    {
        int baseLevel = 10 + gymId * 4;
        return new[] { baseLevel + trainerIdx };
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
}
