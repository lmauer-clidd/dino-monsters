using UnityEngine;

/// <summary>
/// Procedural 3D building generator for the overworld.
/// Creates buildings from primitives: cube walls, pyramid/prism roof,
/// dark door rectangle, and optional sign post.
/// Enhanced with porches, chimneys, flower boxes, shadow planes,
/// and visual distinction for Dino Centers and shops.
/// </summary>
public static class BuildingRenderer
{
    /// <summary>
    /// Create a procedural building at the given position.
    /// </summary>
    public static GameObject CreateBuilding(Transform parent, Vector3 position,
        int width, int depth, int stories, Color wallColor, Color roofColor,
        string signText = null)
    {
        float wallHeight = stories * 1.2f;
        float halfWidth = width * 0.5f;
        float halfDepth = depth * 0.5f;
        // Door X in local coords: matches the door tile offset used in OverworldManager
        // Pattern: (width-1)/2 gives 1 for w=3,4 and 2 for w=5,6
        float doorX = (width - 1) / 2;

        var root = new GameObject($"Building_{position.x}_{position.z}");
        root.transform.SetParent(parent);
        root.transform.position = position;

        // ----- Shadow plane under building -----
        CreateShadowPlane(root.transform, halfWidth, halfDepth, width, depth);

        // ----- Walls (main body) -----
        var walls = GameObject.CreatePrimitive(PrimitiveType.Cube);
        walls.transform.SetParent(root.transform);
        walls.transform.localPosition = new Vector3(halfWidth, wallHeight * 0.5f, halfDepth);
        walls.transform.localScale = new Vector3(width, wallHeight, depth);
        walls.name = "Walls";
        var wallR = walls.GetComponent<Renderer>();
        wallR.sharedMaterial = MaterialManager.GetSolidColor(wallColor);
        // Remove collider — collision is handled by tile system, not physics
        Object.Destroy(walls.GetComponent<Collider>());

        // ----- Detect building type from colors -----
        bool isDinoCenter = IsDinoCenterColor(wallColor);
        bool isShop = IsShopColor(wallColor);
        bool isGym = IsGymColor(wallColor);

        // ----- Roof -----
        if (isDinoCenter)
            CreateRoof(root.transform, halfWidth, halfDepth, width, depth, wallHeight, new Color(0.85f, 0.20f, 0.18f));
        else
            CreateRoof(root.transform, halfWidth, halfDepth, width, depth, wallHeight, roofColor);

        // ----- Chimney (on houses, not on Dino Centers, shops, or gyms) -----
        if (!isDinoCenter && !isShop && !isGym)
            CreateChimney(root.transform, halfWidth, halfDepth, wallHeight);

        // ----- Door -----
        CreateDoor(root.transform, doorX, depth, wallHeight);

        // ----- Windows -----
        CreateWindows(root.transform, width, depth, wallHeight, stories);

        // ----- Flower boxes under front windows -----
        if (!isDinoCenter && !isShop && !isGym)
            CreateFlowerBoxes(root.transform, width, wallHeight, stories);

        // ----- Dino Center: cross symbol on front -----
        if (isDinoCenter)
            CreateDinoCenterCross(root.transform, halfWidth, wallHeight);

        // ----- Shop: blue awning over door -----
        if (isShop)
            CreateShopAwning(root.transform, halfWidth, width, wallColor);

        // ----- Gym: banner/flag on roof -----
        if (isGym)
            CreateGymBanner(root.transform, halfWidth, halfDepth, wallHeight, wallColor);

        // ----- Sign post (if provided) -----
        if (!string.IsNullOrEmpty(signText))
        {
            CreateSignPost(root.transform, width, depth);
        }

        return root;
    }

    // ================================================================
    // Shadow Plane
    // ================================================================

    static void CreateShadowPlane(Transform parent, float halfWidth, float halfDepth, int width, int depth)
    {
        var shadow = GameObject.CreatePrimitive(PrimitiveType.Quad);
        shadow.transform.SetParent(parent);
        shadow.transform.localPosition = new Vector3(halfWidth, 0.01f, halfDepth);
        shadow.transform.localRotation = Quaternion.Euler(90, 0, 0);
        shadow.transform.localScale = new Vector3(width + 0.6f, depth + 0.6f, 1f);
        shadow.name = "Shadow";
        var r = shadow.GetComponent<Renderer>();
        r.sharedMaterial = MaterialManager.GetTransparent(new Color(0.1f, 0.1f, 0.1f), 0.25f);
        Object.Destroy(shadow.GetComponent<Collider>());
    }

    // ================================================================
    // Roof
    // ================================================================

    static void CreateRoof(Transform parent, float halfWidth, float halfDepth,
        int width, int depth, float wallHeight, Color roofColor)
    {
        float roofHeight = 0.8f;

        // Main roof body -- wider than walls, flattened
        var roofBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roofBase.transform.SetParent(parent);
        roofBase.transform.localPosition = new Vector3(halfWidth, wallHeight + 0.1f, halfDepth);
        roofBase.transform.localScale = new Vector3(width + 0.3f, 0.15f, depth + 0.3f);
        roofBase.name = "RoofBase";
        var roofBaseR = roofBase.GetComponent<Renderer>();
        Color roofBaseColor = roofColor * 0.9f; roofBaseColor.a = 1f;
        roofBaseR.sharedMaterial = MaterialManager.GetSolidColor(roofBaseColor);
        Object.Destroy(roofBase.GetComponent<Collider>());

        // Roof peak -- narrower layers stacked to form a pyramid shape
        int layers = 3;
        for (int i = 1; i <= layers; i++)
        {
            float t = (float)i / layers;
            float shrink = t * 0.7f;

            var layer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            layer.transform.SetParent(parent);
            layer.transform.localPosition = new Vector3(
                halfWidth,
                wallHeight + 0.1f + (roofHeight * t),
                halfDepth);
            layer.transform.localScale = new Vector3(
                (width + 0.3f) * (1f - shrink),
                roofHeight / layers,
                (depth + 0.3f) * (1f - shrink));
            layer.name = $"RoofLayer_{i}";

            var layerR = layer.GetComponent<Renderer>();
            Color layerColor = Color.Lerp(roofColor, roofColor * 0.7f, t);
            layerColor.a = 1f;
            layerR.sharedMaterial = MaterialManager.GetSolidColor(layerColor);
            Object.Destroy(layer.GetComponent<Collider>());
        }
    }

    // ================================================================
    // Chimney
    // ================================================================

    static void CreateChimney(Transform parent, float halfWidth, float halfDepth, float wallHeight)
    {
        float chimneyHeight = 0.6f;
        var chimney = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chimney.transform.SetParent(parent);
        chimney.transform.localPosition = new Vector3(
            halfWidth + halfWidth * 0.4f,
            wallHeight + 0.8f + chimneyHeight * 0.5f,
            halfDepth * 0.6f);
        chimney.transform.localScale = new Vector3(0.25f, chimneyHeight, 0.25f);
        chimney.name = "Chimney";
        var cr = chimney.GetComponent<Renderer>();
        cr.sharedMaterial = MaterialManager.GetSolidColor(new Color(0.55f, 0.35f, 0.25f));
        Object.Destroy(chimney.GetComponent<Collider>());

        // Chimney cap
        var cap = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cap.transform.SetParent(parent);
        cap.transform.localPosition = chimney.transform.localPosition + new Vector3(0, chimneyHeight * 0.5f + 0.03f, 0);
        cap.transform.localScale = new Vector3(0.33f, 0.07f, 0.33f);
        cap.name = "ChimneyCap";
        var capR = cap.GetComponent<Renderer>();
        capR.sharedMaterial = MaterialManager.GetSolidColor(new Color(0.4f, 0.28f, 0.20f));
        Object.Destroy(cap.GetComponent<Collider>());

        // Smoke puff (small light-gray sphere above chimney)
        var smoke = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        smoke.transform.SetParent(parent);
        smoke.transform.localPosition = chimney.transform.localPosition + new Vector3(0.05f, chimneyHeight * 0.5f + 0.18f, 0);
        smoke.transform.localScale = new Vector3(0.12f, 0.10f, 0.12f);
        smoke.name = "Smoke";
        var smokeR = smoke.GetComponent<Renderer>();
        smokeR.sharedMaterial = MaterialManager.GetTransparent(new Color(0.8f, 0.8f, 0.8f), 0.4f);
        Object.Destroy(smoke.GetComponent<Collider>());

        // Second smoke puff (offset)
        var smoke2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        smoke2.transform.SetParent(parent);
        smoke2.transform.localPosition = chimney.transform.localPosition + new Vector3(-0.03f, chimneyHeight * 0.5f + 0.30f, 0.04f);
        smoke2.transform.localScale = new Vector3(0.09f, 0.08f, 0.09f);
        smoke2.name = "Smoke2";
        var smoke2R = smoke2.GetComponent<Renderer>();
        smoke2R.sharedMaterial = MaterialManager.GetTransparent(new Color(0.85f, 0.85f, 0.85f), 0.3f);
        Object.Destroy(smoke2.GetComponent<Collider>());
    }

    // ================================================================
    // Door
    // ================================================================

    static void CreateDoor(Transform parent, float doorLocalX, int depth, float wallHeight)
    {
        float doorWidth = 0.7f;
        float doorHeight = Mathf.Min(1.1f, wallHeight * 0.75f);

        // Door frame -- thin CUBE (visible from all angles unlike Quad)
        var frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frame.transform.SetParent(parent);
        frame.transform.localPosition = new Vector3(doorLocalX, doorHeight * 0.5f, -0.01f);
        frame.transform.localScale = new Vector3(doorWidth + 0.14f, doorHeight + 0.10f, 0.04f);
        frame.name = "DoorFrame";
        frame.GetComponent<Renderer>().sharedMaterial =
            MaterialManager.GetSolidColor(new Color(0.70f, 0.55f, 0.35f));
        Object.Destroy(frame.GetComponent<Collider>());

        // Door panel -- darker brown, slightly in front of frame
        var door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.transform.SetParent(parent);
        door.transform.localPosition = new Vector3(doorLocalX, doorHeight * 0.5f, -0.03f);
        door.transform.localScale = new Vector3(doorWidth, doorHeight, 0.04f);
        door.name = "Door";
        door.GetComponent<Renderer>().sharedMaterial =
            MaterialManager.GetSolidColor(new Color(0.45f, 0.28f, 0.15f));
        Object.Destroy(door.GetComponent<Collider>());

        // Door knob -- gold sphere
        var knob = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        knob.transform.SetParent(parent);
        knob.transform.localPosition = new Vector3(doorLocalX + doorWidth * 0.3f, doorHeight * 0.45f, -0.06f);
        knob.transform.localScale = Vector3.one * 0.07f;
        knob.name = "DoorKnob";
        knob.GetComponent<Renderer>().sharedMaterial =
            MaterialManager.GetSolidColor(new Color(0.90f, 0.75f, 0.25f));
        Object.Destroy(knob.GetComponent<Collider>());

        // Door light -- yellow sphere above door
        var light = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        light.transform.SetParent(parent);
        light.transform.localPosition = new Vector3(doorLocalX, doorHeight + 0.10f, -0.05f);
        light.transform.localScale = Vector3.one * 0.10f;
        light.name = "DoorLight";
        light.GetComponent<Renderer>().sharedMaterial =
            MaterialManager.GetSolidColor(new Color(1.0f, 0.92f, 0.50f));
        Object.Destroy(light.GetComponent<Collider>());

        // Porch / welcome step -- flat box in front of door
        var porch = GameObject.CreatePrimitive(PrimitiveType.Cube);
        porch.transform.SetParent(parent);
        porch.transform.localPosition = new Vector3(doorLocalX, 0.04f, -0.25f);
        porch.transform.localScale = new Vector3(0.8f, 0.08f, 0.4f);
        porch.name = "Porch";
        porch.GetComponent<Renderer>().sharedMaterial =
            MaterialManager.GetSolidColor(new Color(0.60f, 0.55f, 0.45f));
        Object.Destroy(porch.GetComponent<Collider>());
    }

    // ================================================================
    // Porch / Step
    // ================================================================

    static void CreatePorch(Transform parent, float halfWidth, int depth)
    {
        var porch = GameObject.CreatePrimitive(PrimitiveType.Cube);
        porch.transform.SetParent(parent);
        porch.transform.localPosition = new Vector3(halfWidth, 0.04f, -0.2f);
        porch.transform.localScale = new Vector3(0.7f, 0.08f, 0.35f);
        porch.name = "Porch";
        var r = porch.GetComponent<Renderer>();
        r.sharedMaterial = MaterialManager.GetSolidColor(new Color(0.6f, 0.55f, 0.45f));
        Object.Destroy(porch.GetComponent<Collider>());
    }

    // ================================================================
    // Windows
    // ================================================================

    static void CreateWindows(Transform parent, int width, int depth, float wallHeight, int stories)
    {
        float windowSize = 0.3f;
        Color windowColor = new Color(0.6f, 0.75f, 0.9f, 1f);
        Color frameColor = new Color(0.85f, 0.82f, 0.75f);

        for (int story = 0; story < stories; story++)
        {
            float windowY = 0.5f + story * 1.2f;
            if (story == 0) windowY = 0.55f;

            float halfW = width * 0.5f;
            for (int wx = 0; wx < width; wx++)
            {
                float xPos = wx + 0.5f;
                if (story == 0 && Mathf.Abs(xPos - halfW) < 0.6f) continue;

                CreateWindow(parent, new Vector3(xPos, windowY, -0.01f),
                    windowSize, Quaternion.Euler(0, 180, 0), windowColor, frameColor);
            }

            for (int wz = 0; wz < depth; wz++)
            {
                float zPos = wz + 0.5f;
                CreateWindow(parent, new Vector3(-0.01f, windowY, zPos),
                    windowSize, Quaternion.Euler(0, 90, 0), windowColor, frameColor);
            }

            for (int wz = 0; wz < depth; wz++)
            {
                float zPos = wz + 0.5f;
                CreateWindow(parent, new Vector3(width + 0.01f, windowY, zPos),
                    windowSize, Quaternion.Euler(0, -90, 0), windowColor, frameColor);
            }
        }
    }

    static void CreateWindow(Transform parent, Vector3 localPos, float size,
        Quaternion rotation, Color glassColor, Color frameColor)
    {
        var frame = GameObject.CreatePrimitive(PrimitiveType.Quad);
        frame.transform.SetParent(parent);
        frame.transform.localPosition = localPos;
        frame.transform.localScale = new Vector3(size + 0.06f, size + 0.06f, 1f);
        frame.transform.localRotation = rotation;
        frame.name = "WindowFrame";
        var fR = frame.GetComponent<Renderer>();
        fR.sharedMaterial = MaterialManager.GetSolidColor(frameColor);
        Object.Destroy(frame.GetComponent<Collider>());

        var glass = GameObject.CreatePrimitive(PrimitiveType.Quad);
        glass.transform.SetParent(parent);
        Vector3 glassPos = localPos + rotation * new Vector3(0, 0, -0.005f);
        glass.transform.localPosition = glassPos;
        glass.transform.localScale = new Vector3(size, size, 1f);
        glass.transform.localRotation = rotation;
        glass.name = "WindowGlass";
        var gR = glass.GetComponent<Renderer>();
        gR.sharedMaterial = MaterialManager.GetSolidColor(glassColor);
        Object.Destroy(glass.GetComponent<Collider>());
    }

    // ================================================================
    // Flower Boxes (under front ground-floor windows)
    // ================================================================

    static void CreateFlowerBoxes(Transform parent, int width, float wallHeight, int stories)
    {
        float halfW = width * 0.5f;

        for (int wx = 0; wx < width; wx++)
        {
            float xPos = wx + 0.5f;
            // Skip center (door) on ground floor
            if (Mathf.Abs(xPos - halfW) < 0.6f) continue;

            // Box
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.transform.SetParent(parent);
            box.transform.localPosition = new Vector3(xPos, 0.32f, -0.08f);
            box.transform.localScale = new Vector3(0.28f, 0.06f, 0.08f);
            box.name = "FlowerBox";
            var br = box.GetComponent<Renderer>();
            br.sharedMaterial = MaterialManager.GetSolidColor(new Color(0.5f, 0.35f, 0.20f));
            Object.Destroy(box.GetComponent<Collider>());

            // Tiny colored flowers in the box
            Color[] flowerColors = { new Color(0.9f, 0.3f, 0.3f), new Color(1f, 0.8f, 0.2f), new Color(0.9f, 0.5f, 0.7f) };
            for (int f = 0; f < 3; f++)
            {
                var flower = GameObject.CreatePrimitive(PrimitiveType.Cube);
                flower.transform.SetParent(parent);
                flower.transform.localPosition = new Vector3(
                    xPos - 0.08f + f * 0.08f,
                    0.38f,
                    -0.08f);
                flower.transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);
                flower.name = "BoxFlower";
                var fr = flower.GetComponent<Renderer>();
                fr.sharedMaterial = MaterialManager.GetSolidColor(flowerColors[f % flowerColors.Length]);
                Object.Destroy(flower.GetComponent<Collider>());
            }
        }
    }

    // ================================================================
    // Dino Center Cross Symbol
    // ================================================================

    static void CreateDinoCenterCross(Transform parent, float halfWidth, float wallHeight)
    {
        float crossY = wallHeight * 0.7f;
        Color crossColor = Color.white;
        Color glowColor = new Color(1f, 0.85f, 0.85f);

        // Background circle (glow effect behind cross)
        var glow = GameObject.CreatePrimitive(PrimitiveType.Quad);
        glow.transform.SetParent(parent);
        glow.transform.localPosition = new Vector3(halfWidth, crossY, -0.012f);
        glow.transform.localScale = new Vector3(0.70f, 0.70f, 1f);
        glow.transform.localRotation = Quaternion.Euler(0, 180, 0);
        glow.name = "CrossGlow";
        var glowR = glow.GetComponent<Renderer>();
        glowR.sharedMaterial = MaterialManager.GetSolidColor(glowColor);
        Object.Destroy(glow.GetComponent<Collider>());

        // Horizontal bar (wider, more visible)
        var hBar = GameObject.CreatePrimitive(PrimitiveType.Quad);
        hBar.transform.SetParent(parent);
        hBar.transform.localPosition = new Vector3(halfWidth, crossY, -0.015f);
        hBar.transform.localScale = new Vector3(0.55f, 0.18f, 1f);
        hBar.transform.localRotation = Quaternion.Euler(0, 180, 0);
        hBar.name = "CrossH";
        var hR = hBar.GetComponent<Renderer>();
        hR.sharedMaterial = MaterialManager.GetSolidColor(crossColor);
        Object.Destroy(hBar.GetComponent<Collider>());

        // Vertical bar (wider, more visible)
        var vBar = GameObject.CreatePrimitive(PrimitiveType.Quad);
        vBar.transform.SetParent(parent);
        vBar.transform.localPosition = new Vector3(halfWidth, crossY, -0.016f);
        vBar.transform.localScale = new Vector3(0.18f, 0.55f, 1f);
        vBar.transform.localRotation = Quaternion.Euler(0, 180, 0);
        vBar.name = "CrossV";
        var vR = vBar.GetComponent<Renderer>();
        vR.sharedMaterial = MaterialManager.GetSolidColor(crossColor);
        Object.Destroy(vBar.GetComponent<Collider>());

        // Red "DINO" text plate above door
        var plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plate.transform.SetParent(parent);
        plate.transform.localPosition = new Vector3(halfWidth, wallHeight * 0.35f, -0.02f);
        plate.transform.localScale = new Vector3(0.9f, 0.18f, 0.02f);
        plate.name = "CenterPlate";
        var plateR = plate.GetComponent<Renderer>();
        plateR.sharedMaterial = MaterialManager.GetSolidColor(new Color(0.85f, 0.15f, 0.15f));
        Object.Destroy(plate.GetComponent<Collider>());

        // Red beacon light on the roof (emissive red sphere)
        var beacon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        beacon.transform.SetParent(parent);
        beacon.transform.localPosition = new Vector3(halfWidth, wallHeight + 1.0f, halfWidth * 0.5f);
        beacon.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
        beacon.name = "RoofBeacon";
        var beaconR = beacon.GetComponent<Renderer>();
        beaconR.sharedMaterial = MaterialManager.GetSolidColor(new Color(1.0f, 0.20f, 0.15f));
        Object.Destroy(beacon.GetComponent<Collider>());

        // Beacon pole
        var pole = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pole.transform.SetParent(parent);
        pole.transform.localPosition = new Vector3(halfWidth, wallHeight + 0.85f, halfWidth * 0.5f);
        pole.transform.localScale = new Vector3(0.04f, 0.25f, 0.04f);
        pole.name = "BeaconPole";
        var poleR = pole.GetComponent<Renderer>();
        poleR.sharedMaterial = MaterialManager.GetSolidColor(new Color(0.5f, 0.5f, 0.5f));
        Object.Destroy(pole.GetComponent<Collider>());
    }

    // ================================================================
    // Shop Awning
    // ================================================================

    static void CreateShopAwning(Transform parent, float halfWidth, int width, Color wallColor)
    {
        // Wider, more prominent awning across the full front
        float awningWidth = Mathf.Min(width, 4) * 1.1f;
        var awning = GameObject.CreatePrimitive(PrimitiveType.Cube);
        awning.transform.SetParent(parent);
        awning.transform.localPosition = new Vector3(halfWidth, 0.95f, -0.55f);
        awning.transform.localScale = new Vector3(awningWidth + 0.5f, 0.08f, 0.9f);
        awning.transform.localRotation = Quaternion.Euler(10, 0, 0); // slight downward tilt
        awning.name = "Awning";
        var ar = awning.GetComponent<Renderer>();
        ar.sharedMaterial = MaterialManager.GetSolidColor(new Color(0.20f, 0.35f, 0.75f));
        Object.Destroy(awning.GetComponent<Collider>());

        // Awning stripes (alternating blue/white for visibility)
        Color[] stripeColors = { new Color(0.90f, 0.90f, 0.95f), new Color(0.20f, 0.35f, 0.75f) };
        for (int i = 0; i < 5; i++)
        {
            var stripe = GameObject.CreatePrimitive(PrimitiveType.Quad);
            stripe.transform.SetParent(parent);
            stripe.transform.localPosition = new Vector3(
                halfWidth - awningWidth * 0.4f + i * awningWidth * 0.2f,
                0.96f, -0.45f);
            stripe.transform.localScale = new Vector3(awningWidth * 0.12f, 0.05f, 1f);
            stripe.transform.localRotation = Quaternion.Euler(80, 0, 0);
            stripe.name = "AwningStripe";
            var sr = stripe.GetComponent<Renderer>();
            sr.sharedMaterial = MaterialManager.GetSolidColor(stripeColors[i % 2]);
            Object.Destroy(stripe.GetComponent<Collider>());
        }

        // Shop sign plate (yellow/gold above awning)
        var sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sign.transform.SetParent(parent);
        sign.transform.localPosition = new Vector3(halfWidth, 1.15f, -0.02f);
        sign.transform.localScale = new Vector3(0.9f, 0.18f, 0.04f);
        sign.name = "ShopSign";
        var signR = sign.GetComponent<Renderer>();
        signR.sharedMaterial = MaterialManager.GetSolidColor(new Color(0.95f, 0.85f, 0.25f));
        Object.Destroy(sign.GetComponent<Collider>());

        // Coin symbol on sign (small gold sphere)
        var coin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        coin.transform.SetParent(parent);
        coin.transform.localPosition = new Vector3(halfWidth, 1.15f, -0.06f);
        coin.transform.localScale = new Vector3(0.10f, 0.10f, 0.04f);
        coin.name = "CoinSymbol";
        var coinR = coin.GetComponent<Renderer>();
        coinR.sharedMaterial = MaterialManager.GetSolidColor(new Color(0.85f, 0.70f, 0.10f));
        Object.Destroy(coin.GetComponent<Collider>());
    }

    // ================================================================
    // Sign Post
    // ================================================================

    static void CreateSignPost(Transform parent, int width, int depth)
    {
        float halfWidth = width * 0.5f;

        var post = GameObject.CreatePrimitive(PrimitiveType.Cube);
        post.transform.SetParent(parent);
        post.transform.localPosition = new Vector3(halfWidth - 0.8f, 0.4f, -0.5f);
        post.transform.localScale = new Vector3(0.08f, 0.8f, 0.08f);
        post.name = "SignPost";
        var postR = post.GetComponent<Renderer>();
        postR.sharedMaterial = MaterialManager.GetSolidColor(new Color(0.45f, 0.35f, 0.20f));
        Object.Destroy(post.GetComponent<Collider>());

        var board = GameObject.CreatePrimitive(PrimitiveType.Cube);
        board.transform.SetParent(parent);
        board.transform.localPosition = new Vector3(halfWidth - 0.8f, 0.75f, -0.5f);
        board.transform.localScale = new Vector3(0.5f, 0.3f, 0.06f);
        board.name = "SignBoard";
        var boardR = board.GetComponent<Renderer>();
        boardR.sharedMaterial = MaterialManager.GetSolidColor(new Color(0.80f, 0.72f, 0.50f));
        Object.Destroy(board.GetComponent<Collider>());
    }

    // ================================================================
    // Helpers: Detect building type from wall color
    // ================================================================

    /// <summary>Detect Dino Center by its characteristic red-pink wall color.</summary>
    static bool IsDinoCenterColor(Color c)
    {
        return c.r > 0.80f && c.g < 0.45f && c.b < 0.45f;
    }

    /// <summary>Detect Shop by its characteristic blue wall color.</summary>
    static bool IsShopColor(Color c)
    {
        return c.b > 0.7f && c.b > c.r && c.b > c.g;
    }

    /// <summary>Detect Gym by its characteristic green wall color.</summary>
    static bool IsGymColor(Color c)
    {
        return c.g > 0.55f && c.g > c.r && c.g > c.b && c.r < 0.55f;
    }

    // ================================================================
    // Gym Banner/Flag
    // ================================================================

    static void CreateGymBanner(Transform parent, float halfWidth, float halfDepth, float wallHeight, Color wallColor)
    {
        // Flag pole on roof
        var pole = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pole.transform.SetParent(parent);
        pole.transform.localPosition = new Vector3(halfWidth, wallHeight + 1.0f, halfDepth);
        pole.transform.localScale = new Vector3(0.04f, 0.8f, 0.04f);
        pole.name = "GymFlagPole";
        var poleR = pole.GetComponent<Renderer>();
        poleR.sharedMaterial = MaterialManager.GetSolidColor(new Color(0.6f, 0.6f, 0.6f));
        Object.Destroy(pole.GetComponent<Collider>());

        // Flag/banner matching the gym color
        var flag = GameObject.CreatePrimitive(PrimitiveType.Quad);
        flag.transform.SetParent(parent);
        flag.transform.localPosition = new Vector3(halfWidth + 0.2f, wallHeight + 1.2f, halfDepth);
        flag.transform.localScale = new Vector3(0.35f, 0.25f, 1f);
        flag.transform.localRotation = Quaternion.Euler(0, 90, 0);
        flag.name = "GymFlag";
        var flagR = flag.GetComponent<Renderer>();
        flagR.sharedMaterial = MaterialManager.GetSolidColor(wallColor);
        Object.Destroy(flag.GetComponent<Collider>());

        // Gym sign plate on front wall
        var signPlate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        signPlate.transform.SetParent(parent);
        signPlate.transform.localPosition = new Vector3(halfWidth, wallHeight * 0.75f, -0.02f);
        signPlate.transform.localScale = new Vector3(1.0f, 0.20f, 0.03f);
        signPlate.name = "GymSign";
        var signR = signPlate.GetComponent<Renderer>();
        signR.sharedMaterial = MaterialManager.GetSolidColor(new Color(0.95f, 0.90f, 0.30f));
        Object.Destroy(signPlate.GetComponent<Collider>());
    }
}
