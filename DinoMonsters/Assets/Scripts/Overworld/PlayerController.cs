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
        modelRoot = new GameObject("PlayerModel").transform;
        modelRoot.SetParent(transform);
        modelRoot.localPosition = Vector3.zero;

        Color skinColor = new Color(0.93f, 0.80f, 0.67f);
        Color shirtColor = new Color(0.20f, 0.45f, 0.85f);
        Color pantsColor = new Color(0.22f, 0.22f, 0.35f);
        Color hatColor = new Color(0.85f, 0.18f, 0.18f);
        Color shoeColor = new Color(0.30f, 0.18f, 0.12f);
        Color backpackColor = new Color(0.65f, 0.35f, 0.15f);
        Color hairColor = new Color(0.25f, 0.15f, 0.08f);

        // --- Torso (body) ---
        var body = CreatePart("Body", modelRoot, new Vector3(0, 0.55f, 0),
            new Vector3(0.38f, 0.30f, 0.22f), shirtColor, PrimitiveType.Cube);
        bodyT = body.transform;
        bodyRenderer = body.GetComponent<Renderer>();

        // --- Waist / belt ---
        CreatePart("Belt", bodyT, new Vector3(0, -0.14f, 0),
            new Vector3(0.36f, 0.05f, 0.21f), new Color(0.30f, 0.28f, 0.20f), PrimitiveType.Cube);

        // --- Head ---
        var head = CreatePart("Head", modelRoot, new Vector3(0, 0.87f, 0),
            new Vector3(0.28f, 0.28f, 0.26f), skinColor, PrimitiveType.Sphere);
        headT = head.transform;

        // --- Hair (back of head) ---
        CreatePart("Hair", headT, new Vector3(0, 0.04f, -0.06f),
            new Vector3(0.27f, 0.20f, 0.14f), hairColor, PrimitiveType.Sphere);

        // --- Eyes ---
        var eL = CreatePart("EyeL", headT, new Vector3(-0.07f, 0.02f, 0.12f),
            new Vector3(0.06f, 0.06f, 0.03f), Color.white, PrimitiveType.Sphere);
        eyeL = eL.transform;
        CreatePart("PupilL", eL.transform, new Vector3(0, 0, 0.01f),
            new Vector3(0.55f, 0.55f, 0.5f), new Color(0.15f, 0.10f, 0.05f), PrimitiveType.Sphere);

        var eR = CreatePart("EyeR", headT, new Vector3(0.07f, 0.02f, 0.12f),
            new Vector3(0.06f, 0.06f, 0.03f), Color.white, PrimitiveType.Sphere);
        eyeR = eR.transform;
        CreatePart("PupilR", eR.transform, new Vector3(0, 0, 0.01f),
            new Vector3(0.55f, 0.55f, 0.5f), new Color(0.15f, 0.10f, 0.05f), PrimitiveType.Sphere);

        // --- Mouth (small line) ---
        CreatePart("Mouth", headT, new Vector3(0, -0.05f, 0.13f),
            new Vector3(0.08f, 0.015f, 0.01f), new Color(0.75f, 0.45f, 0.40f), PrimitiveType.Cube);

        // --- Hat ---
        var hat = CreatePart("HatBrim", modelRoot, new Vector3(0, 1.04f, 0.02f),
            new Vector3(0.36f, 0.05f, 0.36f), hatColor, PrimitiveType.Cylinder);
        hatT = hat.transform;
        CreatePart("HatTop", hatT, new Vector3(0, 0.06f, -0.02f),
            new Vector3(0.28f, 0.10f, 0.28f), hatColor, PrimitiveType.Cylinder);
        // Hat emblem (white circle on front)
        CreatePart("HatEmblem", hatT, new Vector3(0, 0.06f, 0.12f),
            new Vector3(0.08f, 0.08f, 0.02f), Color.white, PrimitiveType.Sphere);

        // --- Arms ---
        var aL = CreatePart("ArmL", modelRoot, new Vector3(-0.24f, 0.52f, 0),
            new Vector3(0.10f, 0.28f, 0.10f), shirtColor, PrimitiveType.Capsule);
        armL = aL.transform;
        CreatePart("HandL", armL, new Vector3(0, -0.16f, 0),
            new Vector3(0.07f, 0.07f, 0.07f), skinColor, PrimitiveType.Sphere);

        var aR = CreatePart("ArmR", modelRoot, new Vector3(0.24f, 0.52f, 0),
            new Vector3(0.10f, 0.28f, 0.10f), shirtColor, PrimitiveType.Capsule);
        armR = aR.transform;
        CreatePart("HandR", armR, new Vector3(0, -0.16f, 0),
            new Vector3(0.07f, 0.07f, 0.07f), skinColor, PrimitiveType.Sphere);

        // --- Legs ---
        var lL = CreatePart("LegL", modelRoot, new Vector3(-0.09f, 0.22f, 0),
            new Vector3(0.12f, 0.26f, 0.12f), pantsColor, PrimitiveType.Capsule);
        legL = lL.transform;
        CreatePart("ShoeL", legL, new Vector3(0, -0.15f, 0.02f),
            new Vector3(0.11f, 0.06f, 0.15f), shoeColor, PrimitiveType.Cube);

        var lR = CreatePart("LegR", modelRoot, new Vector3(0.09f, 0.22f, 0),
            new Vector3(0.12f, 0.26f, 0.12f), pantsColor, PrimitiveType.Capsule);
        legR = lR.transform;
        CreatePart("ShoeR", legR, new Vector3(0, -0.15f, 0.02f),
            new Vector3(0.11f, 0.06f, 0.15f), shoeColor, PrimitiveType.Cube);

        // --- Backpack ---
        var bp = CreatePart("Backpack", bodyT, new Vector3(0, 0.02f, -0.16f),
            new Vector3(0.28f, 0.24f, 0.12f), backpackColor, PrimitiveType.Cube);
        backpack = bp.transform;
        // Backpack strap
        CreatePart("StrapL", bp.transform, new Vector3(-0.10f, 0.10f, 0.06f),
            new Vector3(0.03f, 0.18f, 0.02f), backpackColor * 0.7f, PrimitiveType.Cube);
        CreatePart("StrapR", bp.transform, new Vector3(0.10f, 0.10f, 0.06f),
            new Vector3(0.03f, 0.18f, 0.02f), backpackColor * 0.7f, PrimitiveType.Cube);
        // Pokeball decoration on backpack
        CreatePart("BPDecor", bp.transform, new Vector3(0, 0, -0.06f),
            new Vector3(0.06f, 0.06f, 0.02f), Color.white, PrimitiveType.Sphere);

        // Shadow on ground
        var shadow = CreatePart("Shadow", transform, new Vector3(0, 0.01f, 0),
            new Vector3(0.5f, 0.001f, 0.5f), new Color(0, 0, 0, 0.3f), PrimitiveType.Cylinder);
        var shadowR = shadow.GetComponent<Renderer>();
        shadowR.material = MaterialManager.GetTransparent(new Color(0, 0, 0, 0.3f), 0.3f);
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
            float animSpeed = isRunning ? 14f : 10f;
            walkCycle += Time.deltaTime * animSpeed;
            float sin = Mathf.Sin(walkCycle);
            float cos = Mathf.Cos(walkCycle);

            // Leg swing (opposite phase)
            float legAngle = sin * 30f;
            if (legL != null) legL.localRotation = Quaternion.Euler(legAngle, 0, 0);
            if (legR != null) legR.localRotation = Quaternion.Euler(-legAngle, 0, 0);

            // Arm swing (opposite to legs)
            float armAngle = sin * 25f;
            if (armL != null) armL.localRotation = Quaternion.Euler(-armAngle, 0, 0);
            if (armR != null) armR.localRotation = Quaternion.Euler(armAngle, 0, 0);

            // Body bob (up/down)
            float bob = Mathf.Abs(sin) * 0.04f;
            if (bodyT != null) bodyT.localPosition = new Vector3(0, 0.55f + bob, 0);
            if (headT != null) headT.localPosition = new Vector3(0, 0.87f + bob, 0);
            if (hatT != null) hatT.localPosition = new Vector3(0, 1.04f + bob, 0.02f);

            // Slight body tilt forward when running
            float tilt = isRunning ? 8f : 3f;
            if (bodyT != null) bodyT.localRotation = Quaternion.Euler(tilt, 0, cos * 2f);
            if (headT != null) headT.localRotation = Quaternion.Euler(-tilt * 0.5f, 0, 0);
        }
        else
        {
            AnimateIdle();
        }
    }

    void AnimateIdle()
    {
        breathCycle += Time.deltaTime * 2f;
        float breath = Mathf.Sin(breathCycle) * 0.01f;

        // Subtle breathing
        if (bodyT != null)
        {
            bodyT.localPosition = new Vector3(0, 0.55f + breath, 0);
            bodyT.localRotation = Quaternion.identity;
        }
        if (headT != null)
        {
            headT.localPosition = new Vector3(0, 0.87f + breath, 0);
            headT.localRotation = Quaternion.identity;
        }
        if (hatT != null)
            hatT.localPosition = new Vector3(0, 1.04f + breath, 0.02f);

        // Arms hang naturally
        if (armL != null) armL.localRotation = Quaternion.Euler(0, 0, 3f);
        if (armR != null) armR.localRotation = Quaternion.Euler(0, 0, -3f);
        if (legL != null) legL.localRotation = Quaternion.identity;
        if (legR != null) legR.localRotation = Quaternion.identity;

        // Idle blink
        if (idleTimer > 3f && Mathf.Sin(idleTimer * 5f) > 0.95f)
        {
            if (eyeL != null) eyeL.localScale = new Vector3(0.06f, 0.01f, 0.03f);
            if (eyeR != null) eyeR.localScale = new Vector3(0.06f, 0.01f, 0.03f);
        }
        else
        {
            if (eyeL != null) eyeL.localScale = new Vector3(0.06f, 0.06f, 0.03f);
            if (eyeR != null) eyeR.localScale = new Vector3(0.06f, 0.06f, 0.03f);
        }
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
