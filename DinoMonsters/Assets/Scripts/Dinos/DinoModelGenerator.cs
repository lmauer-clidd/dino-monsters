// ============================================================
// Dino Monsters -- Procedural 3D Dino Model Generator
// ============================================================
//
// Generates low-poly 3D dino models from species data using
// Unity primitives. No external assets required.
// Colors derived from primary type. Type-specific features
// (fins, spikes, crystals) added based on type.
// ============================================================

using UnityEngine;

public static class DinoModelGenerator
{
    // ===============================================================
    // Main Entry Point
    // ===============================================================

    /// <summary>
    /// Create a basic 3D dino model from species data.
    /// </summary>
    /// <param name="speciesId">Species ID from DataLoader</param>
    /// <param name="isEnemy">If true, model faces left (rotated 180)</param>
    public static GameObject CreateDinoModel(int speciesId, bool isEnemy = false)
    {
        var species = DataLoader.Instance.GetSpecies(speciesId);
        if (species == null) return CreateDefaultDino();

        var root = new GameObject($"Dino_{species.name}");

        // Base color from primary type
        Color baseColor = GetTypeColor(species.types[0]);
        Color darkColor = baseColor * 0.7f;
        darkColor.a = 1f;
        Color lightColor = Color.Lerp(baseColor, Color.white, 0.3f);
        Color irisColor = GetTypeIrisColor(species.types[0]);

        // --- Body (elongated sphere) ---
        var body = CreatePart(root.transform, PrimitiveType.Sphere,
            Vector3.zero, new Vector3(0.8f, 0.6f, 1f), baseColor);

        // --- Neck (connects head to body smoothly) ---
        CreatePart(root.transform, PrimitiveType.Sphere,
            new Vector3(0, 0.18f, 0.4f), new Vector3(0.35f, 0.32f, 0.35f), baseColor);

        // --- Head (smaller sphere, forward) ---
        var head = CreatePart(root.transform, PrimitiveType.Sphere,
            new Vector3(0, 0.3f, 0.6f), new Vector3(0.45f, 0.4f, 0.45f), baseColor);

        // --- Snout (small sphere extending forward) ---
        CreatePart(head.transform, PrimitiveType.Sphere,
            new Vector3(0, -0.04f, 0.15f), new Vector3(0.25f, 0.2f, 0.2f), baseColor);

        // --- Eyes (larger white sclera, colored iris, black pupil) ---
        // Right eye
        CreatePart(head.transform, PrimitiveType.Sphere,
            new Vector3(0.15f, 0.05f, 0.14f), Vector3.one * 0.16f, Color.white); // sclera
        CreatePart(head.transform, PrimitiveType.Sphere,
            new Vector3(0.15f, 0.05f, 0.19f), Vector3.one * 0.10f, irisColor);   // iris
        CreatePart(head.transform, PrimitiveType.Sphere,
            new Vector3(0.15f, 0.06f, 0.22f), Vector3.one * 0.05f, Color.black); // pupil
        // Eye shine (tiny white highlight)
        CreatePart(head.transform, PrimitiveType.Sphere,
            new Vector3(0.13f, 0.08f, 0.23f), Vector3.one * 0.025f, Color.white);

        // Left eye
        CreatePart(head.transform, PrimitiveType.Sphere,
            new Vector3(-0.15f, 0.05f, 0.14f), Vector3.one * 0.16f, Color.white);
        CreatePart(head.transform, PrimitiveType.Sphere,
            new Vector3(-0.15f, 0.05f, 0.19f), Vector3.one * 0.10f, irisColor);
        CreatePart(head.transform, PrimitiveType.Sphere,
            new Vector3(-0.15f, 0.06f, 0.22f), Vector3.one * 0.05f, Color.black);
        CreatePart(head.transform, PrimitiveType.Sphere,
            new Vector3(-0.17f, 0.08f, 0.23f), Vector3.one * 0.025f, Color.white);

        // --- Mouth (dark line under head) ---
        CreatePart(head.transform, PrimitiveType.Cube,
            new Vector3(0, -0.12f, 0.18f), new Vector3(0.2f, 0.02f, 0.08f), darkColor);

        // --- Nostrils (tiny dark dots on snout) ---
        CreatePart(head.transform, PrimitiveType.Sphere,
            new Vector3(0.06f, -0.01f, 0.28f), Vector3.one * 0.025f, darkColor);
        CreatePart(head.transform, PrimitiveType.Sphere,
            new Vector3(-0.06f, -0.01f, 0.28f), Vector3.one * 0.025f, darkColor);

        // --- Belly (lighter color) ---
        CreatePart(root.transform, PrimitiveType.Sphere,
            new Vector3(0, -0.1f, 0.1f), new Vector3(0.5f, 0.4f, 0.6f), lightColor);

        // --- Legs (4 capsules with feet) ---
        float legY = -0.35f;
        CreatePart(root.transform, PrimitiveType.Capsule,
            new Vector3(0.25f, legY, 0.2f), new Vector3(0.16f, 0.2f, 0.16f), darkColor);
        CreatePart(root.transform, PrimitiveType.Capsule,
            new Vector3(-0.25f, legY, 0.2f), new Vector3(0.16f, 0.2f, 0.16f), darkColor);
        CreatePart(root.transform, PrimitiveType.Capsule,
            new Vector3(0.25f, legY, -0.3f), new Vector3(0.16f, 0.2f, 0.16f), darkColor);
        CreatePart(root.transform, PrimitiveType.Capsule,
            new Vector3(-0.25f, legY, -0.3f), new Vector3(0.16f, 0.2f, 0.16f), darkColor);

        // --- Feet (small spheres at bottom of legs) ---
        float footY = -0.52f;
        Color footColor = darkColor * 0.85f; footColor.a = 1f;
        CreatePart(root.transform, PrimitiveType.Sphere,
            new Vector3(0.25f, footY, 0.23f), new Vector3(0.14f, 0.06f, 0.16f), footColor);
        CreatePart(root.transform, PrimitiveType.Sphere,
            new Vector3(-0.25f, footY, 0.23f), new Vector3(0.14f, 0.06f, 0.16f), footColor);
        CreatePart(root.transform, PrimitiveType.Sphere,
            new Vector3(0.25f, footY, -0.27f), new Vector3(0.14f, 0.06f, 0.16f), footColor);
        CreatePart(root.transform, PrimitiveType.Sphere,
            new Vector3(-0.25f, footY, -0.27f), new Vector3(0.14f, 0.06f, 0.16f), footColor);

        // --- Tail (thicker base tapering to tip) ---
        // Tail base (thick)
        var tailBase = CreatePart(root.transform, PrimitiveType.Capsule,
            new Vector3(0, 0.02f, -0.55f), new Vector3(0.20f, 0.25f, 0.20f), baseColor);
        tailBase.transform.localRotation = Quaternion.Euler(75, 0, 0);
        // Tail tip (thin)
        var tailTip = CreatePart(root.transform, PrimitiveType.Capsule,
            new Vector3(0, 0.1f, -0.85f), new Vector3(0.10f, 0.2f, 0.10f), baseColor);
        tailTip.transform.localRotation = Quaternion.Euler(85, 0, 0);

        // --- Type-specific features ---
        AddTypeFeatures(root.transform, species.types[0], baseColor);

        // --- Second type accent (if dual type) ---
        if (species.types.Length > 1 && species.types[1] != species.types[0])
        {
            AddSecondTypeAccent(root.transform, species.types[1]);
        }

        // --- Scale based on evolution stage ---
        // Infer stage from species data: id ranges or base stat total
        float scale = GetScaleForSpecies(species);
        root.transform.localScale = Vector3.one * scale;

        // --- Face direction ---
        if (isEnemy)
            root.transform.localRotation = Quaternion.Euler(0, 180, 0);

        return root;
    }

    // ===============================================================
    // Type-Specific Visual Features
    // ===============================================================

    private static void AddTypeFeatures(Transform parent, int type, Color baseColor)
    {
        DinoType dt = (DinoType)type;
        switch (dt)
        {
            case DinoType.Fire:
                // Large flame-like spikes on back (5 spikes, bigger)
                for (int i = 0; i < 5; i++)
                {
                    float h = 0.28f - i * 0.03f;
                    var spike = CreatePart(parent, PrimitiveType.Cube,
                        new Vector3(0, 0.35f + i * 0.08f, -0.3f + i * 0.15f),
                        new Vector3(0.12f, h, 0.12f),
                        Color.Lerp(new Color(1f, 0.8f, 0.1f), new Color(1f, 0.3f, 0.05f), i / 4f));
                    spike.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-20, 20));
                }
                // Side flame wisps
                CreatePart(parent, PrimitiveType.Cube,
                    new Vector3(0.3f, 0.3f, -0.1f), new Vector3(0.06f, 0.18f, 0.06f),
                    new Color(1f, 0.5f, 0.1f));
                CreatePart(parent, PrimitiveType.Cube,
                    new Vector3(-0.3f, 0.3f, -0.1f), new Vector3(0.06f, 0.18f, 0.06f),
                    new Color(1f, 0.5f, 0.1f));
                // Large flame tip on tail
                CreatePart(parent, PrimitiveType.Sphere,
                    new Vector3(0, 0.15f, -1.0f), new Vector3(0.22f, 0.22f, 0.22f),
                    new Color(1f, 0.6f, 0.1f));
                CreatePart(parent, PrimitiveType.Sphere,
                    new Vector3(0, 0.15f, -1.0f), new Vector3(0.14f, 0.14f, 0.14f),
                    new Color(1f, 0.9f, 0.3f));
                break;

            case DinoType.Water:
                // Large dorsal fin
                CreatePart(parent, PrimitiveType.Cube,
                    new Vector3(0, 0.45f, 0), new Vector3(0.05f, 0.35f, 0.6f),
                    new Color(0.3f, 0.5f, 0.9f));
                // Side fins (like pectoral fins)
                var finL = CreatePart(parent, PrimitiveType.Cube,
                    new Vector3(0.4f, 0.05f, 0.1f), new Vector3(0.3f, 0.04f, 0.2f),
                    new Color(0.35f, 0.55f, 0.92f));
                finL.transform.localRotation = Quaternion.Euler(0, 0, -20);
                var finR = CreatePart(parent, PrimitiveType.Cube,
                    new Vector3(-0.4f, 0.05f, 0.1f), new Vector3(0.3f, 0.04f, 0.2f),
                    new Color(0.35f, 0.55f, 0.92f));
                finR.transform.localRotation = Quaternion.Euler(0, 0, 20);
                // Large tail fin (horizontal spread)
                CreatePart(parent, PrimitiveType.Cube,
                    new Vector3(0, 0, -0.95f), new Vector3(0.45f, 0.05f, 0.2f),
                    new Color(0.3f, 0.5f, 0.9f));
                // Tail fin vertical accent
                CreatePart(parent, PrimitiveType.Cube,
                    new Vector3(0, 0.1f, -0.95f), new Vector3(0.05f, 0.2f, 0.15f),
                    new Color(0.25f, 0.45f, 0.85f));
                break;

            case DinoType.Flora:
                // Large leaf canopy on back
                CreatePart(parent, PrimitiveType.Sphere,
                    new Vector3(0, 0.45f, 0), new Vector3(0.45f, 0.18f, 0.45f),
                    new Color(0.2f, 0.7f, 0.2f));
                // Flower bud (bigger, more colorful)
                CreatePart(parent, PrimitiveType.Sphere,
                    new Vector3(0, 0.58f, 0.05f), new Vector3(0.22f, 0.15f, 0.22f),
                    new Color(0.95f, 0.35f, 0.55f));
                // Flower center
                CreatePart(parent, PrimitiveType.Sphere,
                    new Vector3(0, 0.65f, 0.05f), new Vector3(0.10f, 0.08f, 0.10f),
                    new Color(1f, 0.85f, 0.2f));
                // Larger leaves on sides
                var leafL = CreatePart(parent, PrimitiveType.Cube,
                    new Vector3(0.4f, 0.15f, 0), new Vector3(0.3f, 0.05f, 0.2f),
                    new Color(0.25f, 0.65f, 0.25f));
                leafL.transform.localRotation = Quaternion.Euler(0, 10, -15);
                var leafR = CreatePart(parent, PrimitiveType.Cube,
                    new Vector3(-0.4f, 0.15f, 0), new Vector3(0.3f, 0.05f, 0.2f),
                    new Color(0.25f, 0.65f, 0.25f));
                leafR.transform.localRotation = Quaternion.Euler(0, -10, 15);
                // Vine on tail
                CreatePart(parent, PrimitiveType.Capsule,
                    new Vector3(0, 0.05f, -0.8f), new Vector3(0.06f, 0.15f, 0.06f),
                    new Color(0.3f, 0.55f, 0.2f));
                break;

            case DinoType.Electric:
                // Lightning bolt shapes on sides
                CreatePart(parent, PrimitiveType.Cube,
                    new Vector3(0.35f, 0.3f, 0), new Vector3(0.06f, 0.3f, 0.06f),
                    new Color(1f, 0.85f, 0.1f));
                CreatePart(parent, PrimitiveType.Cube,
                    new Vector3(-0.35f, 0.3f, 0), new Vector3(0.06f, 0.3f, 0.06f),
                    new Color(1f, 0.85f, 0.1f));
                // Zigzag tail tip
                CreatePart(parent, PrimitiveType.Cube,
                    new Vector3(0, 0.1f, -0.9f), new Vector3(0.15f, 0.15f, 0.04f),
                    new Color(1f, 0.85f, 0.1f));
                break;

            case DinoType.Ice:
                // Crystal on head
                var crystal = CreatePart(parent, PrimitiveType.Cube,
                    new Vector3(0, 0.55f, 0.4f), new Vector3(0.12f, 0.2f, 0.12f),
                    new Color(0.7f, 0.9f, 1f));
                crystal.transform.localRotation = Quaternion.Euler(0, 45, 0);
                // Ice shards on back
                CreatePart(parent, PrimitiveType.Cube,
                    new Vector3(0.15f, 0.4f, -0.1f), new Vector3(0.06f, 0.15f, 0.06f),
                    new Color(0.8f, 0.95f, 1f));
                CreatePart(parent, PrimitiveType.Cube,
                    new Vector3(-0.15f, 0.4f, -0.1f), new Vector3(0.06f, 0.15f, 0.06f),
                    new Color(0.8f, 0.95f, 1f));
                break;

            case DinoType.Earth:
                // Rocky plates on back
                CreatePart(parent, PrimitiveType.Cube,
                    new Vector3(0, 0.35f, 0.1f), new Vector3(0.5f, 0.08f, 0.3f),
                    new Color(0.6f, 0.5f, 0.35f));
                CreatePart(parent, PrimitiveType.Cube,
                    new Vector3(0, 0.38f, -0.2f), new Vector3(0.4f, 0.08f, 0.25f),
                    new Color(0.55f, 0.45f, 0.3f));
                break;

            case DinoType.Venom:
                // Poison sacs / bumps
                CreatePart(parent, PrimitiveType.Sphere,
                    new Vector3(0.3f, 0.15f, 0.2f), new Vector3(0.15f, 0.12f, 0.15f),
                    new Color(0.7f, 0.2f, 0.7f));
                CreatePart(parent, PrimitiveType.Sphere,
                    new Vector3(-0.3f, 0.15f, 0.2f), new Vector3(0.15f, 0.12f, 0.15f),
                    new Color(0.7f, 0.2f, 0.7f));
                // Stinger on tail
                CreatePart(parent, PrimitiveType.Cube,
                    new Vector3(0, 0.15f, -0.95f), new Vector3(0.04f, 0.12f, 0.04f),
                    new Color(0.5f, 0.1f, 0.5f));
                break;

            case DinoType.Fossil:
                // Bone-like spines
                for (int i = 0; i < 4; i++)
                {
                    CreatePart(parent, PrimitiveType.Cube,
                        new Vector3(0, 0.35f, 0.3f - i * 0.2f),
                        new Vector3(0.06f, 0.12f + i * 0.02f, 0.06f),
                        new Color(0.85f, 0.8f, 0.65f));
                }
                // Skull-like horn on head
                CreatePart(parent, PrimitiveType.Cube,
                    new Vector3(0, 0.45f, 0.7f), new Vector3(0.06f, 0.15f, 0.06f),
                    new Color(0.85f, 0.8f, 0.65f));
                break;

            case DinoType.Shadow:
                // Dark wisps / shadow tendrils
                CreatePart(parent, PrimitiveType.Capsule,
                    new Vector3(0.2f, 0.35f, -0.1f), new Vector3(0.04f, 0.2f, 0.04f),
                    new Color(0.2f, 0.1f, 0.3f));
                CreatePart(parent, PrimitiveType.Capsule,
                    new Vector3(-0.2f, 0.35f, -0.1f), new Vector3(0.04f, 0.2f, 0.04f),
                    new Color(0.2f, 0.1f, 0.3f));
                // Dark aura sphere
                var aura = CreatePart(parent, PrimitiveType.Sphere,
                    Vector3.zero, new Vector3(1.1f, 0.8f, 1.2f),
                    new Color(0.15f, 0.05f, 0.2f, 0.3f));
                SetTransparent(aura);
                break;

            case DinoType.Light:
                // Glowing halo above head
                var halo = CreatePart(parent, PrimitiveType.Cylinder,
                    new Vector3(0, 0.65f, 0.4f), new Vector3(0.3f, 0.01f, 0.3f),
                    new Color(1f, 1f, 0.6f));
                // Light aura
                var lightAura = CreatePart(parent, PrimitiveType.Sphere,
                    Vector3.zero, new Vector3(1.05f, 0.75f, 1.15f),
                    new Color(1f, 1f, 0.7f, 0.2f));
                SetTransparent(lightAura);
                break;

            case DinoType.Metal:
                // Metallic plates
                CreatePart(parent, PrimitiveType.Cube,
                    new Vector3(0, 0.35f, 0.15f), new Vector3(0.6f, 0.06f, 0.4f),
                    new Color(0.75f, 0.75f, 0.8f));
                // Metal claws
                CreatePart(parent, PrimitiveType.Cube,
                    new Vector3(0.3f, -0.45f, 0.25f), new Vector3(0.04f, 0.08f, 0.04f),
                    new Color(0.8f, 0.8f, 0.85f));
                CreatePart(parent, PrimitiveType.Cube,
                    new Vector3(-0.3f, -0.45f, 0.25f), new Vector3(0.04f, 0.08f, 0.04f),
                    new Color(0.8f, 0.8f, 0.85f));
                break;

            case DinoType.Primal:
                // Ancient markings (glowing streaks)
                CreatePart(parent, PrimitiveType.Cube,
                    new Vector3(0.2f, 0.2f, 0.3f), new Vector3(0.03f, 0.15f, 0.03f),
                    new Color(0.95f, 0.8f, 0.3f));
                CreatePart(parent, PrimitiveType.Cube,
                    new Vector3(-0.2f, 0.2f, 0.3f), new Vector3(0.03f, 0.15f, 0.03f),
                    new Color(0.95f, 0.8f, 0.3f));
                // Crown-like horns
                CreatePart(parent, PrimitiveType.Cube,
                    new Vector3(0.1f, 0.5f, 0.5f), new Vector3(0.05f, 0.18f, 0.05f),
                    new Color(0.9f, 0.7f, 0.2f));
                CreatePart(parent, PrimitiveType.Cube,
                    new Vector3(-0.1f, 0.5f, 0.5f), new Vector3(0.05f, 0.18f, 0.05f),
                    new Color(0.9f, 0.7f, 0.2f));
                break;

            case DinoType.Air:
                // Wing-like fins
                var wingL = CreatePart(parent, PrimitiveType.Cube,
                    new Vector3(0.45f, 0.2f, 0), new Vector3(0.35f, 0.04f, 0.25f),
                    new Color(0.7f, 0.6f, 0.95f));
                wingL.transform.localRotation = Quaternion.Euler(0, 0, -15);
                var wingR = CreatePart(parent, PrimitiveType.Cube,
                    new Vector3(-0.45f, 0.2f, 0), new Vector3(0.35f, 0.04f, 0.25f),
                    new Color(0.7f, 0.6f, 0.95f));
                wingR.transform.localRotation = Quaternion.Euler(0, 0, 15);
                // Feather crest
                CreatePart(parent, PrimitiveType.Cube,
                    new Vector3(0, 0.45f, 0.5f), new Vector3(0.04f, 0.15f, 0.12f),
                    new Color(0.7f, 0.6f, 0.95f));
                break;

            case DinoType.Normal:
                // Simple ear-like bumps
                CreatePart(parent, PrimitiveType.Sphere,
                    new Vector3(0.15f, 0.45f, 0.5f), new Vector3(0.08f, 0.12f, 0.08f),
                    baseColor * 0.85f);
                CreatePart(parent, PrimitiveType.Sphere,
                    new Vector3(-0.15f, 0.45f, 0.5f), new Vector3(0.08f, 0.12f, 0.08f),
                    baseColor * 0.85f);
                break;
        }
    }

    // ===============================================================
    // Second Type Accent
    // ===============================================================

    private static void AddSecondTypeAccent(Transform parent, int secondType)
    {
        Color accentColor = GetTypeColor(secondType);

        // Add accent stripes on the body
        CreatePart(parent, PrimitiveType.Cube,
            new Vector3(0, 0.15f, 0.3f), new Vector3(0.82f, 0.03f, 0.08f),
            accentColor);
        CreatePart(parent, PrimitiveType.Cube,
            new Vector3(0, 0.05f, 0f), new Vector3(0.82f, 0.03f, 0.08f),
            accentColor);
    }

    // ===============================================================
    // Scale Inference
    // ===============================================================

    /// <summary>
    /// Infer a scale for the dino model based on species data.
    /// Uses base stat total as a proxy for evolution stage.
    /// </summary>
    private static float GetScaleForSpecies(DinoSpeciesData species)
    {
        if (species.baseStats == null) return 0.85f;

        int bst = species.baseStats.hp + species.baseStats.atk + species.baseStats.def
                + species.baseStats.spatk + species.baseStats.spdef + species.baseStats.speed;

        // More dramatic scale differences: baby is small, adult is imposing
        if (bst < 300) return 0.55f;   // baby — noticeably small
        if (bst < 420) return 0.82f;   // teen — medium
        return 1.15f;                   // adult — large and imposing
    }

    // ===============================================================
    // Part Creation Helper
    // ===============================================================

    private static GameObject CreatePart(Transform parent, PrimitiveType type,
        Vector3 pos, Vector3 scale, Color color)
    {
        var go = GameObject.CreatePrimitive(type);
        go.transform.SetParent(parent);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;

        var r = go.GetComponent<Renderer>();
        r.sharedMaterial = MaterialManager.GetSolidColor(color);

        // Remove collider — not needed for visual model
        var collider = go.GetComponent<Collider>();
        if (collider != null) Object.Destroy(collider);

        return go;
    }

    /// <summary>
    /// Set a part's material to transparent rendering mode.
    /// </summary>
    private static void SetTransparent(GameObject go)
    {
        var r = go.GetComponent<Renderer>();
        if (r == null) return;

        Color c = r.sharedMaterial != null ? r.sharedMaterial.color : Color.white;
        r.sharedMaterial = MaterialManager.GetTransparent(c, c.a);
    }

    // ===============================================================
    // Type Iris Color — distinct eye color per type
    // ===============================================================

    private static Color GetTypeIrisColor(int type)
    {
        return (DinoType)type switch
        {
            DinoType.Normal   => new Color(0.45f, 0.35f, 0.25f),  // brown
            DinoType.Fire     => new Color(0.95f, 0.35f, 0.10f),  // orange-red
            DinoType.Water    => new Color(0.20f, 0.40f, 0.90f),  // deep blue
            DinoType.Earth    => new Color(0.70f, 0.55f, 0.20f),  // amber
            DinoType.Air      => new Color(0.55f, 0.45f, 0.85f),  // lavender
            DinoType.Electric => new Color(0.90f, 0.75f, 0.10f),  // golden
            DinoType.Ice      => new Color(0.40f, 0.75f, 0.90f),  // icy blue
            DinoType.Venom    => new Color(0.70f, 0.15f, 0.55f),  // magenta
            DinoType.Flora    => new Color(0.30f, 0.70f, 0.20f),  // green
            DinoType.Fossil   => new Color(0.60f, 0.50f, 0.25f),  // ochre
            DinoType.Shadow   => new Color(0.55f, 0.15f, 0.20f),  // crimson
            DinoType.Light    => new Color(0.90f, 0.85f, 0.40f),  // bright gold
            DinoType.Metal    => new Color(0.50f, 0.55f, 0.65f),  // steel
            DinoType.Primal   => new Color(0.85f, 0.60f, 0.15f),  // rich gold
            _                 => new Color(0.40f, 0.30f, 0.20f)   // default brown
        };
    }

    // ===============================================================
    // Type Color Lookup (matches Constants.cs palette)
    // ===============================================================

    private static Color GetTypeColor(int type)
    {
        return (DinoType)type switch
        {
            DinoType.Normal   => new Color(0.75f, 0.75f, 0.68f),
            DinoType.Fire     => new Color(0.94f, 0.50f, 0.19f),
            DinoType.Water    => new Color(0.41f, 0.56f, 0.94f),
            DinoType.Earth    => new Color(0.88f, 0.75f, 0.40f),
            DinoType.Air      => new Color(0.66f, 0.56f, 0.94f),
            DinoType.Electric => new Color(0.97f, 0.82f, 0.17f),
            DinoType.Ice      => new Color(0.59f, 0.85f, 0.85f),
            DinoType.Venom    => new Color(0.63f, 0.25f, 0.63f),
            DinoType.Flora    => new Color(0.47f, 0.78f, 0.30f),
            DinoType.Fossil   => new Color(0.72f, 0.63f, 0.38f),
            DinoType.Shadow   => new Color(0.44f, 0.34f, 0.59f),
            DinoType.Light    => new Color(0.97f, 0.97f, 0.47f),
            DinoType.Metal    => new Color(0.72f, 0.72f, 0.82f),
            DinoType.Primal   => new Color(0.91f, 0.75f, 0.31f),
            _                 => Color.gray
        };
    }

    // ===============================================================
    // Default / Fallback Dino
    // ===============================================================

    private static GameObject CreateDefaultDino()
    {
        var root = new GameObject("Dino_Unknown");

        // Simple gray sphere body
        CreatePart(root.transform, PrimitiveType.Sphere,
            Vector3.zero, new Vector3(0.6f, 0.5f, 0.7f), Color.gray);

        // Eyes
        CreatePart(root.transform, PrimitiveType.Sphere,
            new Vector3(0.12f, 0.15f, 0.3f), Vector3.one * 0.08f, Color.white);
        CreatePart(root.transform, PrimitiveType.Sphere,
            new Vector3(-0.12f, 0.15f, 0.3f), Vector3.one * 0.08f, Color.white);
        CreatePart(root.transform, PrimitiveType.Sphere,
            new Vector3(0.12f, 0.15f, 0.33f), Vector3.one * 0.04f, Color.black);
        CreatePart(root.transform, PrimitiveType.Sphere,
            new Vector3(-0.12f, 0.15f, 0.33f), Vector3.one * 0.04f, Color.black);

        return root;
    }
}
