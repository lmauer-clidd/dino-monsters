using UnityEngine;

/// <summary>
/// Grid-based NPC that can be interacted with.
/// Renders as a colored cylinder with sphere head.
/// Supports dialogue, facing the player, and optional patrol paths.
/// </summary>
public class NPCController : MonoBehaviour
{
    [Header("Identity")]
    public string npcName;
    public string[] dialogueLines;
    public bool isTrainer = false;
    public string trainerId;

    [Header("Trainer Battle Data")]
    public int[] trainerSpeciesIds;
    public int[] trainerLevels;

    [Header("Grid Position")]
    public int gridX;
    public int gridY;

    [Header("Patrol")]
    public Vector2Int[] patrolPath;
    public float patrolWaitTime = 2f;

    [Header("Character Style")]
    public string npcStyle = "casual"; // casual, trainer, gym_leader, team_meteore

    // Visual
    private Transform modelRoot;
    private Color bodyColor = new Color(0.6f, 0.4f, 0.3f);

    // Patrol state
    private int patrolIndex = 0;
    private float patrolTimer = 0f;
    private bool isMoving = false;
    private Vector3 moveStart;
    private Vector3 moveTarget;
    private float moveProgress;
    private float moveSpeed = 3f;

    // Dialogue state
    private int currentDialogueLine = 0;
    private bool inDialogue = false;

    // Facing
    private PlayerController.Direction facing = PlayerController.Direction.Down;
    private Transform directionIndicator;

    // Idle animation state
    private float idleBobTimer = 0f;
    private float idleFacingTimer = 0f;
    private bool isLookingAtPlayer = false;
    private Vector3 modelBasePosition;
    private Transform exclamationMark;
    private float exclamationBounceTimer = 0f;
    private bool isExclamationBouncing = false;

    /// <summary>
    /// Initialize NPC with data. Called by OverworldManager.SpawnNPC.
    /// </summary>
    public void Init(string name, int x, int y, string[] dialogue, Color color)
    {
        npcName = name;
        gridX = x;
        gridY = y;
        dialogueLines = dialogue;
        bodyColor = color;

        transform.position = new Vector3(x, 0, y);
        BuildModel();
    }

    /// <summary>
    /// Initialize as a trainer NPC.
    /// </summary>
    public void InitTrainer(string name, int x, int y, string[] dialogue, Color color,
                            string id)
    {
        Init(name, x, y, dialogue, color);
        isTrainer = true;
        trainerId = id;
    }

    /// <summary>
    /// Set the trainer's battle team (species IDs and levels).
    /// </summary>
    public void SetTrainerBattle(int[] speciesIds, int[] levels)
    {
        trainerSpeciesIds = speciesIds;
        trainerLevels = levels;
    }

    /// <summary>
    /// Get the direction this NPC is currently facing.
    /// </summary>
    public PlayerController.Direction GetFacing()
    {
        return facing;
    }

    /// <summary>
    /// Set a patrol path (list of grid positions the NPC walks between).
    /// </summary>
    public void SetPatrolPath(Vector2Int[] path)
    {
        patrolPath = path;
        patrolIndex = 0;
        patrolTimer = patrolWaitTime;
    }

    void BuildModel()
    {
        BuildProceduralModel();
    }

    // =========================================================================
    //  Procedural Model
    // =========================================================================

    void BuildProceduralModel()
    {
        modelRoot = new GameObject("NPCModel").transform;
        modelRoot.SetParent(transform);
        modelRoot.localPosition = Vector3.zero;

        // Body — cylinder
        var body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.transform.SetParent(modelRoot);
        body.transform.localPosition = new Vector3(0, 0.4f, 0);
        body.transform.localScale = new Vector3(0.45f, 0.4f, 0.45f);
        var bodyR = body.GetComponent<Renderer>();
        bodyR.material = new Material(Shader.Find("Standard"));
        bodyR.material.color = bodyColor;
        Object.Destroy(body.GetComponent<Collider>());

        // Head — sphere
        var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.transform.SetParent(modelRoot);
        head.transform.localPosition = new Vector3(0, 0.85f, 0);
        head.transform.localScale = new Vector3(0.32f, 0.32f, 0.32f);
        var headR = head.GetComponent<Renderer>();
        headR.material = new Material(Shader.Find("Standard"));
        headR.material.color = new Color(0.92f, 0.78f, 0.65f); // Skin
        Object.Destroy(head.GetComponent<Collider>());

        // Direction indicator (eyes)
        var indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        indicator.transform.SetParent(modelRoot);
        indicator.transform.localPosition = new Vector3(0, 0.85f, 0.18f);
        indicator.transform.localScale = new Vector3(0.07f, 0.07f, 0.07f);
        var indR = indicator.GetComponent<Renderer>();
        indR.material = new Material(Shader.Find("Standard"));
        indR.material.color = Constants.ColorBlack;
        Object.Destroy(indicator.GetComponent<Collider>());
        directionIndicator = indicator.transform;

        // Trainer exclamation mark (small red sphere above head)
        if (isTrainer)
        {
            var mark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mark.transform.SetParent(modelRoot);
            mark.transform.localPosition = new Vector3(0, 1.15f, 0);
            mark.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            var markR = mark.GetComponent<Renderer>();
            markR.material = new Material(Shader.Find("Standard"));
            markR.material.color = new Color(0.9f, 0.2f, 0.2f);
            Object.Destroy(mark.GetComponent<Collider>());
            exclamationMark = mark.transform;
        }

        // Store base position for idle bob
        modelBasePosition = modelRoot.localPosition;
    }

    void Update()
    {
        if (inDialogue) return;

        // Patrol movement
        if (patrolPath != null && patrolPath.Length > 0)
        {
            UpdatePatrol();
        }

        // Idle animations
        UpdateIdleAnimations();
    }

    // =========================================================================
    //  Patrol
    // =========================================================================

    void UpdatePatrol()
    {
        if (isMoving)
        {
            moveProgress += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(moveStart, moveTarget, moveProgress);

            if (moveProgress >= 1f)
            {
                transform.position = moveTarget;
                isMoving = false;
                patrolTimer = patrolWaitTime;
            }
            return;
        }

        patrolTimer -= Time.deltaTime;
        if (patrolTimer <= 0f)
        {
            // Move to next patrol point
            patrolIndex = (patrolIndex + 1) % patrolPath.Length;
            Vector2Int next = patrolPath[patrolIndex];

            // Update facing
            int dx = next.x - gridX;
            int dy = next.y - gridY;
            if (dy > 0) facing = PlayerController.Direction.Up;
            else if (dy < 0) facing = PlayerController.Direction.Down;
            else if (dx < 0) facing = PlayerController.Direction.Left;
            else if (dx > 0) facing = PlayerController.Direction.Right;
            UpdateFacingVisual();

            gridX = next.x;
            gridY = next.y;
            moveStart = transform.position;
            moveTarget = new Vector3(next.x, 0, next.y);
            moveProgress = 0f;
            isMoving = true;
        }
    }

    // =========================================================================
    //  Idle Animations
    // =========================================================================

    void UpdateIdleAnimations()
    {
        if (modelRoot == null) return;

        // --- Gentle up-down bob (0.02 amplitude, 1.5s period) ---
        idleBobTimer += Time.deltaTime;
        float bobOffset = Mathf.Sin(idleBobTimer * (2f * Mathf.PI / 1.5f)) * 0.02f;
        modelRoot.localPosition = modelBasePosition + new Vector3(0, bobOffset, 0);

        // --- Look toward player when within 3 tiles ---
        var player = FindObjectOfType<PlayerController>();
        if (player != null && !isMoving)
        {
            int dx = player.gridX - gridX;
            int dy = player.gridY - gridY;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);

            if (dist <= 3f)
            {
                if (!isLookingAtPlayer)
                {
                    isLookingAtPlayer = true;
                    FaceTowards(player.gridX, player.gridY);
                }

                // Trainer exclamation bounce when spotting player within range
                if (isTrainer && exclamationMark != null && !isExclamationBouncing && dist <= 3f)
                {
                    string tid = trainerId ?? "";
                    bool defeated = !string.IsNullOrEmpty(tid) && GameState.Instance != null
                                    && GameState.Instance.IsTrainerDefeated(tid);
                    if (!defeated)
                    {
                        isExclamationBouncing = true;
                        exclamationBounceTimer = 0f;
                    }
                }
            }
            else
            {
                isLookingAtPlayer = false;
            }
        }

        // --- Exclamation mark bounce animation ---
        if (isExclamationBouncing && exclamationMark != null)
        {
            exclamationBounceTimer += Time.deltaTime;
            float bounce = Mathf.Abs(Mathf.Sin(exclamationBounceTimer * 8f)) * 0.15f;
            exclamationMark.localPosition = new Vector3(0, 1.15f + bounce, 0);

            // Stop bouncing after ~1.5 seconds
            if (exclamationBounceTimer > 1.5f)
            {
                isExclamationBouncing = false;
                exclamationMark.localPosition = new Vector3(0, 1.15f, 0);
            }
        }

        // --- Random facing change for non-trainer, non-patrolling NPCs ---
        if (!isTrainer && (patrolPath == null || patrolPath.Length == 0) && !isLookingAtPlayer)
        {
            idleFacingTimer -= Time.deltaTime;
            if (idleFacingTimer <= 0f)
            {
                // Pick a random direction
                int dir = Random.Range(0, 4);
                switch (dir)
                {
                    case 0: facing = PlayerController.Direction.Up; break;
                    case 1: facing = PlayerController.Direction.Down; break;
                    case 2: facing = PlayerController.Direction.Left; break;
                    case 3: facing = PlayerController.Direction.Right; break;
                }
                UpdateFacingVisual();

                // Next change in 5-10 seconds
                idleFacingTimer = Random.Range(5f, 10f);
            }
        }
    }

    // =========================================================================
    //  Interaction
    // =========================================================================

    /// <summary>
    /// Called when the player interacts with this NPC.
    /// </summary>
    public void Interact(PlayerController player)
    {
        // Face the player
        FaceTowards(player.gridX, player.gridY);

        // Show dialogue
        if (dialogueLines != null && dialogueLines.Length > 0)
        {
            string line = dialogueLines[currentDialogueLine];
            Debug.Log($"[{npcName}] {line}");

            // Advance dialogue for next interaction
            currentDialogueLine = (currentDialogueLine + 1) % dialogueLines.Length;

            // TODO: Show in dialogue UI instead of Debug.Log
            // DialogueUI.Instance.ShowDialogue(npcName, line);
        }

        // If trainer, trigger battle after dialogue
        if (isTrainer)
        {
            Debug.Log($"[{npcName}] veut se battre !");
            // TODO: Trigger trainer battle
        }
    }

    /// <summary>
    /// Turn the NPC to face towards a grid position.
    /// </summary>
    public void FaceTowards(int targetX, int targetY)
    {
        int dx = targetX - gridX;
        int dy = targetY - gridY;

        if (Mathf.Abs(dx) > Mathf.Abs(dy))
        {
            facing = dx > 0 ? PlayerController.Direction.Right : PlayerController.Direction.Left;
        }
        else
        {
            facing = dy > 0 ? PlayerController.Direction.Up : PlayerController.Direction.Down;
        }

        UpdateFacingVisual();
    }

    void UpdateFacingVisual()
    {
        if (directionIndicator == null) return;
        switch (facing)
        {
            case PlayerController.Direction.Up:
                directionIndicator.localPosition = new Vector3(0, 0.85f, 0.18f);
                break;
            case PlayerController.Direction.Down:
                directionIndicator.localPosition = new Vector3(0, 0.85f, -0.18f);
                break;
            case PlayerController.Direction.Left:
                directionIndicator.localPosition = new Vector3(-0.18f, 0.85f, 0);
                break;
            case PlayerController.Direction.Right:
                directionIndicator.localPosition = new Vector3(0.18f, 0.85f, 0);
                break;
        }
    }
}
