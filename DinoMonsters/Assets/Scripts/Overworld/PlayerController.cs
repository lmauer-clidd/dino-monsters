using UnityEngine;

/// <summary>
/// Player controller with animated character model and smooth joystick movement.
/// Hybrid: smooth analog movement with grid-snapping for tile interactions.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4.5f;
    public float runSpeedMultiplier = 1.7f;
    public float gridSnapThreshold = 0.08f;

    // Grid position (integer, for tile lookups)
    public int gridX { get; private set; }
    public int gridY { get; private set; }

    // Facing direction
    public enum Direction { Up, Down, Left, Right }
    public Direction facing = Direction.Down;

    // Movement state
    private bool isMoving = false;
    private bool isRunning = false;
    private bool inputLocked = false;
    private Vector3 velocity;
    private float idleTimer;

    // Animation state
    private float walkCycle;
    private float breathCycle;

    // Character parts (procedural animated model)
    private Transform modelRoot;
    private Transform bodyT, headT, hatT;
    private Transform armL, armR, legL, legR;
    private Transform eyeL, eyeR;
    private Transform backpack;
    private Renderer bodyRenderer;

    // Smooth rotation
    private float targetYaw;
    private float currentYaw;

    void Start()
    {
        BuildCharacterModel();
        currentYaw = 180f; // face down
        targetYaw = 180f;
    }

    // =========================================================================
    //  Procedural Character Model
    // =========================================================================

    void BuildCharacterModel()
    {
        modelRoot = CharacterModelGenerator.CreateCharacter(transform, CharacterModelGenerator.CharacterType.Player);

        // Resolve named joints for animation
        bodyT = modelRoot.Find("Body");
        headT = modelRoot.Find("Head");
        armL = modelRoot.Find("ArmL");
        armR = modelRoot.Find("ArmR");
        legL = modelRoot.Find("LegL");
        legR = modelRoot.Find("LegR");

        // Eye references for blink animation — only set if actually found (never fallback to headT!)
        if (headT != null)
        {
            eyeL = FindDeep(headT, "EyeL");
            eyeR = FindDeep(headT, "EyeR");
        }

        hatT = headT; // hat is built into head joint
        backpack = bodyT; // backpack is built into body joint
        bodyRenderer = bodyT != null ? bodyT.GetComponentInChildren<Renderer>() : null;

        // Blink animation disabled — new model has no named EyeL/EyeR primitives
    }

    private Transform FindDeep(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var found = FindDeep(child, name);
            if (found != null) return found;
        }
        return null;
    }

    GameObject CreatePart(string name, Transform parent, Vector3 localPos, Vector3 localScale, Color color, PrimitiveType prim)
    {
        var go = GameObject.CreatePrimitive(prim);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.localPosition = localPos;
        go.transform.localScale = localScale;
        go.transform.localRotation = Quaternion.identity;

        var r = go.GetComponent<Renderer>();
        r.sharedMaterial = MaterialManager.GetSolidColor(color);

        var col = go.GetComponent<Collider>();
        if (col != null) Destroy(col);

        return go;
    }

    // =========================================================================
    //  Update
    // =========================================================================

    void Update()
    {
        if (inputLocked)
        {
            AnimateIdle();
            return;
        }

        HandleMovement();
        HandleInteractionInput();
        AnimateCharacter();
    }

    // =========================================================================
    //  Smooth movement with grid snapping
    // =========================================================================

    void HandleMovement()
    {
        // Read analog input
        float inputH = 0f, inputV = 0f;

        // Keyboard (digital)
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) inputV += 1f;
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) inputV -= 1f;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) inputH -= 1f;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) inputH += 1f;

        // Gamepad analog (combine stick + dpad)
        float stickH = SafeAxis("Horizontal");
        float stickV = SafeAxis("Vertical");
        float dpadH = SafeAxis("DPadH");
        float dpadV = SafeAxis("DPadV");
        float gamepadH = Mathf.Abs(dpadH) > Mathf.Abs(stickH) ? dpadH : stickH;
        float gamepadV = Mathf.Abs(dpadV) > Mathf.Abs(stickV) ? dpadV : stickV;

        if (Mathf.Abs(gamepadH) > 0.2f) inputH += gamepadH;
        if (Mathf.Abs(gamepadV) > 0.2f) inputV += gamepadV;

        // Clamp to max 1
        inputH = Mathf.Clamp(inputH, -1f, 1f);
        inputV = Mathf.Clamp(inputV, -1f, 1f);

        bool hasInput = Mathf.Abs(inputH) > 0.1f || Mathf.Abs(inputV) > 0.1f;
        isRunning = InputHelper.RunHeld;
        float speed = moveSpeed * (isRunning ? runSpeedMultiplier : 1f);

        if (hasInput)
        {
            isMoving = true;
            idleTimer = 0f;

            // Determine primary direction for facing (strongest axis wins)
            if (Mathf.Abs(inputH) > Mathf.Abs(inputV))
                facing = inputH > 0 ? Direction.Right : Direction.Left;
            else
                facing = inputV > 0 ? Direction.Up : Direction.Down;

            // Smooth rotation toward movement direction
            targetYaw = facing switch
            {
                Direction.Up => 0f,
                Direction.Down => 180f,
                Direction.Left => 270f,
                Direction.Right => 90f,
                _ => targetYaw
            };

            // Move along the dominant axis (one axis at a time for grid alignment)
            Vector3 moveDir;
            if (Mathf.Abs(inputH) > Mathf.Abs(inputV))
                moveDir = new Vector3(Mathf.Sign(inputH), 0, 0);
            else
                moveDir = new Vector3(0, 0, Mathf.Sign(inputV));

            // Speed is proportional to stick magnitude for analog feel
            float magnitude = Mathf.Clamp01(new Vector2(inputH, inputV).magnitude);
            Vector3 movement = moveDir * (speed * magnitude * Time.deltaTime);
            Vector3 newPos = transform.position + movement;

            // Check walkability of target tile
            int checkX = Mathf.RoundToInt(newPos.x);
            int checkZ = Mathf.RoundToInt(newPos.z);

            bool canMove = true;
            if (OverworldManager.Instance != null)
            {
                // Check if we're crossing into a new tile
                if (checkX != gridX || checkZ != gridY)
                {
                    if (!OverworldManager.Instance.IsWalkable(checkX, checkZ))
                        canMove = false;
                }
            }

            if (canMove)
            {
                transform.position = newPos;

                // Update grid position when close enough to center of new tile
                int newGridX = Mathf.RoundToInt(transform.position.x);
                int newGridZ = Mathf.RoundToInt(transform.position.z);

                if (newGridX != gridX || newGridZ != gridY)
                {
                    float distToCenter = Vector2.Distance(
                        new Vector2(transform.position.x, transform.position.z),
                        new Vector2(newGridX, newGridZ));

                    if (distToCenter < gridSnapThreshold + 0.4f)
                    {
                        gridX = newGridX;
                        gridY = newGridZ;

                        // Notify overworld of step
                        if (OverworldManager.Instance != null)
                            OverworldManager.Instance.OnPlayerStep(gridX, gridY);
                    }
                }
            }
        }
        else
        {
            isMoving = false;
            idleTimer += Time.deltaTime;

            // Snap to nearest grid center when stopped
            Vector3 gridCenter = new Vector3(gridX, 0, gridY);
            if (Vector3.Distance(transform.position, gridCenter) > 0.01f)
            {
                transform.position = Vector3.Lerp(transform.position, gridCenter, Time.deltaTime * 12f);
                if (Vector3.Distance(transform.position, gridCenter) < 0.02f)
                    transform.position = gridCenter;
            }
        }

        // Smooth rotation
        currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, Time.deltaTime * 12f);
        if (modelRoot != null)
            modelRoot.localRotation = Quaternion.Euler(0, currentYaw, 0);
    }

    float SafeAxis(string name)
    {
        try { return Input.GetAxisRaw(name); }
        catch { return 0f; }
    }

    // =========================================================================
    //  Procedural Animation
    // =========================================================================

    void AnimateCharacter()
    {
        if (isMoving)
        {
            AnimateWalk();
        }
        else
        {
            AnimateIdle();
        }
    }

    void AnimateWalk()
    {
        float animSpeed = isRunning ? 14f : 10f;
        walkCycle += Time.deltaTime * animSpeed;
        float sin = Mathf.Sin(walkCycle);
        float cos = Mathf.Cos(walkCycle);
        float absSin = Mathf.Abs(sin);
        float runMult = isRunning ? 1.4f : 1.0f;

        // ---- Legs: full stride with knee bend ----
        float legSwing = sin * 35f * runMult;
        float kneeOffset = absSin * 5f; // slight extra bend at extremes
        if (legL != null)
            legL.localRotation = Quaternion.Euler(legSwing + kneeOffset, 0, 0);
        if (legR != null)
            legR.localRotation = Quaternion.Euler(-legSwing + kneeOffset, 0, 0);

        // ---- Arms: natural swing, opposite to legs, with elbow bend ----
        float armSwing = sin * 30f * runMult;
        float elbowBend = absSin * 8f;
        if (armL != null)
            armL.localRotation = Quaternion.Euler(-armSwing, 0, 3f + elbowBend);
        if (armR != null)
            armR.localRotation = Quaternion.Euler(armSwing, 0, -3f - elbowBend);

        // ---- Body: bob + tilt + sway ----
        float bob = absSin * 0.05f * runMult;
        float tiltFwd = isRunning ? 10f : 4f;
        float sway = cos * 2.5f * runMult; // lateral sway
        if (bodyT != null)
        {
            bodyT.localPosition = new Vector3(cos * 0.01f * runMult, 0.55f + bob, 0);
            bodyT.localRotation = Quaternion.Euler(tiltFwd, 0, sway);
        }

        // ---- Head: counter-tilt + slight bounce ----
        if (headT != null)
        {
            float headBob = absSin * 0.03f * runMult;
            headT.localPosition = new Vector3(0, 0.85f + headBob, 0);
            // Head tilts opposite to body for natural feel
            headT.localRotation = Quaternion.Euler(-tiltFwd * 0.4f, cos * 1.5f, -sway * 0.3f);
        }
    }

    void AnimateIdle()
    {
        breathCycle += Time.deltaTime * 2f;
        idleTimer += Time.deltaTime;
        float breath = Mathf.Sin(breathCycle) * 0.012f;
        float breathSlow = Mathf.Sin(breathCycle * 0.5f);

        // ---- Body: gentle breathing swell ----
        if (bodyT != null)
        {
            bodyT.localPosition = new Vector3(0, 0.55f + breath, 0);
            // Subtle breathing sway (no scale — would crush the joint)
            bodyT.localRotation = Quaternion.Euler(0, 0, breathSlow * 0.5f);
        }

        // ---- Head: gentle look-around + nod ----
        if (headT != null)
        {
            float headNod = Mathf.Sin(breathCycle * 0.7f) * 2f;
            float headTurn = Mathf.Sin(breathCycle * 0.3f + 1f) * 3f;
            float headTilt = Mathf.Sin(breathCycle * 0.4f + 2f) * 1.5f;
            headT.localPosition = new Vector3(0, 0.85f + breath * 0.8f, 0);
            headT.localRotation = Quaternion.Euler(headNod, headTurn, headTilt);
        }

        // ---- Arms: relaxed sway ----
        if (armL != null)
        {
            float armSway = Mathf.Sin(breathCycle * 0.6f) * 3f;
            armL.localRotation = Quaternion.Euler(armSway, 0, 4f + Mathf.Sin(breathCycle * 0.4f) * 1.5f);
        }
        if (armR != null)
        {
            float armSway = Mathf.Sin(breathCycle * 0.6f + 1f) * 3f;
            armR.localRotation = Quaternion.Euler(armSway, 0, -4f - Mathf.Sin(breathCycle * 0.4f + 1f) * 1.5f);
        }

        // ---- Legs: subtle weight shift ----
        if (legL != null)
            legL.localRotation = Quaternion.Euler(Mathf.Sin(breathCycle * 0.25f) * 1.5f, 0, 0);
        if (legR != null)
            legR.localRotation = Quaternion.Euler(Mathf.Sin(breathCycle * 0.25f + Mathf.PI) * 1.5f, 0, 0);
    }

    // =========================================================================
    //  Interaction
    // =========================================================================

    void HandleInteractionInput()
    {
        if (InputHelper.Interact)
        {
            int fx = gridX, fy = gridY;
            switch (facing)
            {
                case Direction.Up:    fy++; break;
                case Direction.Down:  fy--; break;
                case Direction.Left:  fx--; break;
                case Direction.Right: fx++; break;
            }

            if (OverworldManager.Instance != null)
                OverworldManager.Instance.TryInteract(fx, fy);
        }
    }

    // =========================================================================
    //  Public API
    // =========================================================================

    public void SetGridPosition(int x, int y)
    {
        gridX = x;
        gridY = y;
        transform.position = new Vector3(x, 0, y);
        isMoving = false;
    }

    public void LockInput() { inputLocked = true; }
    public void UnlockInput() { inputLocked = false; }

    public bool IsMoving() { return isMoving; }

    public Vector2Int GetFacingTile()
    {
        int fx = gridX, fy = gridY;
        switch (facing)
        {
            case Direction.Up:    fy++; break;
            case Direction.Down:  fy--; break;
            case Direction.Left:  fx--; break;
            case Direction.Right: fx++; break;
        }
        return new Vector2Int(fx, fy);
    }
}
