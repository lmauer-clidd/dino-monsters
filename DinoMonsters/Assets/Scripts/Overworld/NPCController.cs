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

    // Articulated parts for animation
    private Transform npcBody, npcHead, npcArmL, npcArmR, npcLegL, npcLegR;

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
        modelRoot = CharacterModelGenerator.CreateSimpleNPC(transform, bodyColor, isTrainer);

        // Direction indicator — create a small dark sphere for eye direction
        var indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        indicator.transform.SetParent(modelRoot);
        indicator.transform.localPosition = new Vector3(0, 0.87f, 0.16f);
        indicator.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        var indR = indicator.GetComponent<Renderer>();
        indR.sharedMaterial = MaterialManager.GetSolidColor(Color.black);
        Object.Destroy(indicator.GetComponent<Collider>());
        directionIndicator = indicator.transform;

        // Trainer exclamation mark
        if (isTrainer)
        {
            var mark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mark.transform.SetParent(modelRoot);
            mark.transform.localPosition = new Vector3(0, 1.15f, 0);
            mark.transform.localScale = new Vector3(0.10f, 0.10f, 0.10f);
            mark.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetEmissive(new Color(0.9f, 0.2f, 0.2f), 0.6f);
            Object.Destroy(mark.GetComponent<Collider>());
            exclamationMark = mark.transform;
        }

        // Resolve articulated joints
        npcBody = modelRoot.Find("Body");
        npcHead = modelRoot.Find("Head");
        npcArmL = modelRoot.Find("ArmL");
        npcArmR = modelRoot.Find("ArmR");
        npcLegL = modelRoot.Find("LegL");
        npcLegR = modelRoot.Find("LegR");

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

        // Animations
        if (isMoving)
            AnimateNPCWalk();
        else
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

    void AnimateNPCWalk()
    {
        idleBobTimer += Time.deltaTime * 8f;
        float sin = Mathf.Sin(idleBobTimer);
        float absSin = Mathf.Abs(sin);

        // Legs swing
        float legSwing = sin * 28f;
        if (npcLegL != null) npcLegL.localRotation = Quaternion.Euler(legSwing, 0, 0);
        if (npcLegR != null) npcLegR.localRotation = Quaternion.Euler(-legSwing, 0, 0);

        // Arms swing opposite
        float armSwing = sin * 22f;
        if (npcArmL != null) npcArmL.localRotation = Quaternion.Euler(-armSwing, 0, 3f);
        if (npcArmR != null) npcArmR.localRotation = Quaternion.Euler(armSwing, 0, -3f);

        // Body bob + tilt
        float bob = absSin * 0.04f;
        modelRoot.localPosition = modelBasePosition + new Vector3(0, bob, 0);
        if (npcBody != null)
            npcBody.localRotation = Quaternion.Euler(3f, 0, Mathf.Cos(idleBobTimer) * 2f);

        // Head counter-movement
        if (npcHead != null)
            npcHead.localRotation = Quaternion.Euler(-1.5f, Mathf.Cos(idleBobTimer) * 1f, 0);
    }

    void UpdateIdleAnimations()
    {
        if (modelRoot == null) return;

        idleBobTimer += Time.deltaTime;
        float t = idleBobTimer;

        // --- Body: gentle breathing bob ---
        float breathBob = Mathf.Sin(t * 4.2f) * 0.012f;
        modelRoot.localPosition = modelBasePosition + new Vector3(0, breathBob, 0);

        // --- Head: look around, nod ---
        if (npcHead != null)
        {
            float headNod = Mathf.Sin(t * 1.4f) * 2.5f;
            float headTurn = Mathf.Sin(t * 0.6f + 0.7f) * 4f;
            float headTilt = Mathf.Sin(t * 0.8f + 2f) * 2f;
            npcHead.localRotation = Quaternion.Euler(headNod, headTurn, headTilt);
        }

        // --- Body: subtle breathing scale + sway ---
        if (npcBody != null)
        {
            float sway = Mathf.Sin(t * 0.9f) * 1f;
            npcBody.localRotation = Quaternion.Euler(0, 0, sway);
        }

        // --- Arms: relaxed idle sway ---
        if (npcArmL != null)
        {
            float armL_swing = Mathf.Sin(t * 1.2f) * 4f;
            npcArmL.localRotation = Quaternion.Euler(armL_swing, 0, 4f + Mathf.Sin(t * 0.7f) * 2f);
        }
        if (npcArmR != null)
        {
            float armR_swing = Mathf.Sin(t * 1.2f + 1.5f) * 4f;
            npcArmR.localRotation = Quaternion.Euler(armR_swing, 0, -4f - Mathf.Sin(t * 0.7f + 1.5f) * 2f);
        }

        // --- Legs: subtle weight shift ---
        if (npcLegL != null)
            npcLegL.localRotation = Quaternion.Euler(Mathf.Sin(t * 0.5f) * 2f, 0, 0);
        if (npcLegR != null)
            npcLegR.localRotation = Quaternion.Euler(Mathf.Sin(t * 0.5f + Mathf.PI) * 2f, 0, 0);

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
        if (inDialogue) return;

        // Face the player
        FaceTowards(player.gridX, player.gridY);

        // Lock player movement during dialogue
        player.LockInput();
        inDialogue = true;

        // Show full dialogue sequence
        if (dialogueLines != null && dialogueLines.Length > 0)
        {
            ShowDialogueSequence(player, 0);
        }
        else
        {
            player.UnlockInput();
            inDialogue = false;
        }
    }

    private void ShowDialogueSequence(PlayerController player, int lineIndex)
    {
        if (lineIndex >= dialogueLines.Length)
        {
            // All lines done — check trainer battle
            if (isTrainer && !GameState.Instance.IsTrainerDefeated(trainerId))
            {
                if (DialogueUI.Instance != null)
                {
                    DialogueUI.Instance.ShowText($"{npcName} veut se battre !", npcName, () =>
                    {
                        inDialogue = false;
                        var setup = new BattleSetupData
                        {
                            isWild = false,
                            trainerId = trainerId,
                            trainerName = npcName,
                            returnScene = GameState.Instance.CurrentMapId,
                            returnPosition = new Vector2(player.gridX, player.gridY)
                        };
                        if (trainerSpeciesIds != null)
                        {
                            setup.trainerParty = new System.Collections.Generic.List<TrainerDinoEntry>();
                            for (int i = 0; i < trainerSpeciesIds.Length; i++)
                            {
                                int lvl = (trainerLevels != null && i < trainerLevels.Length) ? trainerLevels[i] : 5;
                                setup.trainerParty.Add(new TrainerDinoEntry { speciesId = trainerSpeciesIds[i], level = lvl });
                            }
                        }
                        if (GameManager.Instance != null)
                            GameManager.Instance.StartBattle(setup);
                    });
                }
                else
                {
                    inDialogue = false;
                    player.UnlockInput();
                }
            }
            else
            {
                inDialogue = false;
                player.UnlockInput();
            }
            return;
        }

        string line = dialogueLines[lineIndex];

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowText(line, npcName, () =>
            {
                ShowDialogueSequence(player, lineIndex + 1);
            });
        }
        else
        {
            Debug.Log($"[{npcName}] {line}");
            ShowDialogueSequence(player, lineIndex + 1);
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
