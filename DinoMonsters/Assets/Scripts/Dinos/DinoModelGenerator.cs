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

        // ---- Species-specific models (starters + evolutions) ----
        switch (species.id)
        {
            case 1: return CreatePyrex(species, isEnemy);
            case 2: return CreatePyrovore(species, isEnemy);
            case 3: return CreateVolcanorex(species, isEnemy);
            case 4: return CreateAquadon(species, isEnemy);
            case 5: return CreateMarexis(species, isEnemy);
            case 6: return CreateAbyssaure(species, isEnemy);
            case 7: return CreateFlorasaur(species, isEnemy);
            case 8: return CreateSylvacolle(species, isEnemy);
            case 9: return CreateTitanarbore(species, isEnemy);
        }

        // ---- Generic articulated model (all other species) ----
        return CreateGenericDino(species, isEnemy);
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

    // ===============================================================
    // Shared Helpers for Species-Specific Models
    // ===============================================================

    /// <summary>
    /// Create a named joint (empty GameObject) for articulation.
    /// </summary>
    private static Transform CreateJoint(Transform parent, string name, Vector3 localPos, Quaternion? localRot = null)
    {
        var joint = new GameObject(name);
        joint.transform.SetParent(parent);
        joint.transform.localPosition = localPos;
        joint.transform.localRotation = localRot ?? Quaternion.identity;
        return joint.transform;
    }

    /// <summary>
    /// Create a pair of eyes on a head transform. Supports slit pupils (cube) or round pupils (sphere).
    /// </summary>
    private static void CreateEyes(Transform head, float xOffset, float yOffset, float zOffset,
        float scleraSize, float irisSize, float pupilSize, Color irisColor, bool slitPupils = false)
    {
        for (int side = -1; side <= 1; side += 2)
        {
            float x = side * xOffset;
            // Sclera
            CreatePart(head, PrimitiveType.Sphere,
                new Vector3(x, yOffset, zOffset), Vector3.one * scleraSize, Color.white);
            // Iris
            CreatePart(head, PrimitiveType.Sphere,
                new Vector3(x, yOffset, zOffset + 0.04f), Vector3.one * irisSize, irisColor);
            // Pupil — slit (cube) or round (sphere)
            if (slitPupils)
                CreatePart(head, PrimitiveType.Cube,
                    new Vector3(x, yOffset + 0.01f, zOffset + 0.06f),
                    new Vector3(pupilSize * 0.35f, pupilSize * 1.4f, pupilSize * 0.35f), Color.black);
            else
                CreatePart(head, PrimitiveType.Sphere,
                    new Vector3(x, yOffset + 0.01f, zOffset + 0.06f),
                    Vector3.one * pupilSize, Color.black);
            // Shine highlight
            CreatePart(head, PrimitiveType.Sphere,
                new Vector3(x - side * 0.015f, yOffset + 0.025f, zOffset + 0.07f),
                Vector3.one * (scleraSize * 0.18f), Color.white);
            // Second smaller shine
            CreatePart(head, PrimitiveType.Sphere,
                new Vector3(x + side * 0.01f, yOffset - 0.01f, zOffset + 0.065f),
                Vector3.one * (scleraSize * 0.10f), Color.white);
            // Eyelid (thin curved top)
            var headRenderer = head.GetComponentInChildren<Renderer>();
            Color eyelidColor = (headRenderer != null && headRenderer.sharedMaterial != null)
                ? headRenderer.sharedMaterial.color * 0.85f : Color.gray;
            eyelidColor.a = 1f;
            CreatePart(head, PrimitiveType.Sphere,
                new Vector3(x, yOffset + scleraSize * 0.35f, zOffset + 0.01f),
                new Vector3(scleraSize * 1.1f, scleraSize * 0.25f, scleraSize * 0.5f),
                eyelidColor);
        }
    }

    /// <summary>
    /// Apply final scale (from BST) and direction rotation to a finished model.
    /// </summary>
    private static void FinalizeModel(GameObject root, DinoSpeciesData species, bool isEnemy)
    {
        float scale = GetScaleForSpecies(species);
        root.transform.localScale = Vector3.one * scale;
        if (isEnemy)
            root.transform.localRotation = Quaternion.Euler(0, 180, 0);
    }

    // ===============================================================
    // PYREX — Articulated Bipedal Fire Raptor (Compsognathus)
    // Species ID: 1 — ~90 primitives
    // ===============================================================

    private static GameObject CreatePyrex(DinoSpeciesData species, bool isEnemy)
    {
        var root = new GameObject($"Dino_{species.name}");

        // Color palette
        Color baseOrange   = new Color(0.94f, 0.50f, 0.19f);
        Color darkOrange   = baseOrange * 0.65f; darkOrange.a = 1f;
        Color deepOrange   = baseOrange * 0.50f; deepOrange.a = 1f;
        Color bellyColor   = Color.Lerp(baseOrange, Color.white, 0.38f);
        Color flameYellow  = new Color(1f, 0.80f, 0.10f);
        Color flameDeep    = new Color(1f, 0.30f, 0.05f);
        Color flameMid     = Color.Lerp(flameYellow, flameDeep, 0.4f);
        Color clawBlack    = new Color(0.12f, 0.08f, 0.06f);
        Color irisColor    = new Color(0.95f, 0.35f, 0.10f);
        Color scaleColor   = baseOrange * 0.82f; scaleColor.a = 1f;
        Color toothWhite   = new Color(0.95f, 0.92f, 0.85f);
        Color tongueRed    = new Color(0.85f, 0.25f, 0.25f);
        Color smokeColor   = new Color(0.3f, 0.2f, 0.15f);

        // ==== JOINT HIERARCHY ====
        // Body (main pivot, tilted for raptor stance)
        var body = CreateJoint(root.transform, "Body", Vector3.zero, Quaternion.Euler(15, 0, 0));

        // -- Torso geometry --
        CreatePart(body, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.52f, 0.46f, 0.68f), baseOrange);
        // Chest (front volume)
        CreatePart(body, PrimitiveType.Sphere, new Vector3(0, 0.02f, 0.15f), new Vector3(0.40f, 0.38f, 0.35f), baseOrange);
        // Belly
        CreatePart(body, PrimitiveType.Sphere, new Vector3(0, -0.10f, 0.05f), new Vector3(0.36f, 0.28f, 0.48f), bellyColor);

        // Scale texture on flanks (~8 small cubes)
        for (int i = 0; i < 4; i++)
        {
            float z = -0.15f + i * 0.10f;
            for (int side = -1; side <= 1; side += 2)
            {
                CreatePart(body, PrimitiveType.Cube,
                    new Vector3(side * 0.24f, 0.05f - i * 0.02f, z),
                    new Vector3(0.04f, 0.03f, 0.06f), scaleColor);
            }
        }

        // ---- NECK joint ----
        var neck = CreateJoint(body, "Neck", new Vector3(0, 0.16f, 0.32f));
        CreatePart(neck, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.24f, 0.24f, 0.24f), baseOrange);

        // ---- HEAD joint ----
        var head = CreateJoint(neck, "Head", new Vector3(0, 0.14f, 0.14f));
        CreatePart(head, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.40f, 0.36f, 0.42f), baseOrange);

        // Snout (angular)
        CreatePart(head, PrimitiveType.Sphere, new Vector3(0, -0.02f, 0.17f), new Vector3(0.24f, 0.17f, 0.24f), baseOrange);
        // Brow ridges (2 angular cubes above eyes)
        CreatePart(head, PrimitiveType.Cube, new Vector3(0.10f, 0.12f, 0.12f), new Vector3(0.08f, 0.025f, 0.06f), darkOrange);
        CreatePart(head, PrimitiveType.Cube, new Vector3(-0.10f, 0.12f, 0.12f), new Vector3(0.08f, 0.025f, 0.06f), darkOrange);

        // Nostrils
        CreatePart(head, PrimitiveType.Sphere, new Vector3(0.05f, -0.01f, 0.30f), Vector3.one * 0.022f, darkOrange);
        CreatePart(head, PrimitiveType.Sphere, new Vector3(-0.05f, -0.01f, 0.30f), Vector3.one * 0.022f, darkOrange);
        // Nostril smoke (semi-transparent)
        var smoke1 = CreatePart(head, PrimitiveType.Sphere, new Vector3(0.05f, 0.02f, 0.33f), Vector3.one * 0.035f, smokeColor);
        SetTransparent(smoke1); smoke1.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetTransparent(smokeColor, 0.25f);
        var smoke2 = CreatePart(head, PrimitiveType.Sphere, new Vector3(-0.05f, 0.02f, 0.33f), Vector3.one * 0.035f, smokeColor);
        SetTransparent(smoke2); smoke2.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetTransparent(smokeColor, 0.25f);

        // Eyes (slit pupils)
        CreateEyes(head, 0.13f, 0.05f, 0.14f, 0.14f, 0.09f, 0.05f, irisColor, slitPupils: true);

        // ---- JAW joint (opens/closes for attack) ----
        var jaw = CreateJoint(head, "Jaw", new Vector3(0, -0.12f, 0.10f));
        // Lower jaw
        CreatePart(jaw, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.18f, 0.06f, 0.16f), darkOrange);
        // Mouth line
        CreatePart(jaw, PrimitiveType.Cube, new Vector3(0, 0.03f, 0.06f), new Vector3(0.16f, 0.012f, 0.10f), deepOrange);
        // Teeth (upper — on head, lower — on jaw)
        for (int i = 0; i < 3; i++)
        {
            float tx = -0.05f + i * 0.05f;
            // Upper teeth
            CreatePart(head, PrimitiveType.Cube, new Vector3(tx, -0.11f, 0.20f + i * 0.02f),
                new Vector3(0.018f, 0.03f, 0.012f), toothWhite);
            // Lower teeth
            CreatePart(jaw, PrimitiveType.Cube, new Vector3(tx, 0.04f, 0.04f + i * 0.02f),
                new Vector3(0.016f, 0.025f, 0.010f), toothWhite);
        }
        // Tongue
        CreatePart(jaw, PrimitiveType.Sphere, new Vector3(0, 0.02f, 0.03f),
            new Vector3(0.08f, 0.015f, 0.06f), tongueRed);

        // ---- CREST joint (flame spikes, flicker animation) ----
        var crest = CreateJoint(head, "Crest", new Vector3(0, 0.18f, 0f));
        // 5 flame spikes of varying sizes
        float[] crestHeights = { 0.24f, 0.18f, 0.20f, 0.15f, 0.12f };
        float[] crestX =       { 0f,    -0.06f, 0.06f, -0.10f, 0.10f };
        float[] crestPhase =   { 0f,    0.3f,   -0.2f,  0.5f,   -0.4f };
        for (int i = 0; i < 5; i++)
        {
            Color c = Color.Lerp(flameYellow, flameDeep, i * 0.2f);
            var spike = CreatePart(crest, PrimitiveType.Cube,
                new Vector3(crestX[i], crestHeights[i] * 0.5f, -0.02f * i),
                new Vector3(0.05f + 0.01f * (2 - i), crestHeights[i], 0.045f), c);
            spike.transform.localRotation = Quaternion.Euler(-8 + crestPhase[i] * 5, 0, crestPhase[i] * 12);
            spike.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetEmissive(c, 0.5f);
        }

        // ---- ARMS (joints Arm_L, Arm_R) ----
        for (int side = -1; side <= 1; side += 2)
        {
            string armName = side < 0 ? "Arm_L" : "Arm_R";
            var arm = CreateJoint(body, armName, new Vector3(side * 0.26f, 0.06f, 0.18f),
                Quaternion.Euler(10, 0, side * -25));
            // Upper arm
            CreatePart(arm, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.06f, 0.08f, 0.06f), darkOrange);
            // Forearm
            CreatePart(arm, PrimitiveType.Capsule, new Vector3(0, -0.12f, 0.02f),
                new Vector3(0.045f, 0.06f, 0.045f), darkOrange);
            // 3 claws
            for (int c = -1; c <= 1; c++)
            {
                CreatePart(arm, PrimitiveType.Cube, new Vector3(c * 0.02f, -0.18f, 0.04f),
                    new Vector3(0.015f, 0.035f, 0.015f), clawBlack);
            }
        }

        // ---- LEGS (joints Leg_L, Leg_R) — bipedal digitigrade ----
        for (int side = -1; side <= 1; side += 2)
        {
            string legName = side < 0 ? "Leg_L" : "Leg_R";
            var leg = CreateJoint(body, legName, new Vector3(side * 0.18f, -0.20f, -0.05f));
            // Muscular thigh
            var thigh = CreatePart(leg, PrimitiveType.Capsule, Vector3.zero,
                new Vector3(0.15f, 0.17f, 0.15f), darkOrange);
            thigh.transform.localRotation = Quaternion.Euler(15, 0, 0);
            // Thigh muscle detail
            CreatePart(leg, PrimitiveType.Sphere, new Vector3(side * 0.04f, 0.02f, 0.02f),
                new Vector3(0.08f, 0.10f, 0.08f), baseOrange);

            // Shin (digitigrade)
            var shin = CreatePart(leg, PrimitiveType.Capsule, new Vector3(0, -0.22f, 0.08f),
                new Vector3(0.09f, 0.15f, 0.09f), darkOrange);
            shin.transform.localRotation = Quaternion.Euler(-25, 0, 0);

            // Ankle detail
            CreatePart(leg, PrimitiveType.Sphere, new Vector3(0, -0.30f, 0.14f),
                new Vector3(0.07f, 0.05f, 0.07f), darkOrange);

            // Foot
            CreatePart(leg, PrimitiveType.Sphere, new Vector3(0, -0.35f, 0.18f),
                new Vector3(0.14f, 0.04f, 0.17f), darkOrange);

            // 3 toes with claws
            for (int t = -1; t <= 1; t++)
            {
                CreatePart(leg, PrimitiveType.Cube, new Vector3(t * 0.035f, -0.36f, 0.28f),
                    new Vector3(0.022f, 0.025f, 0.055f), clawBlack);
            }

            // Scales on thigh
            CreatePart(leg, PrimitiveType.Cube, new Vector3(side * 0.06f, 0.06f, 0f),
                new Vector3(0.035f, 0.025f, 0.04f), scaleColor);
        }

        // ---- TAIL (chained joints: Tail_1 → Tail_2 → Tail_3 → Flame) ----
        var tail1 = CreateJoint(body, "Tail_1", new Vector3(0, 0.02f, -0.35f), Quaternion.Euler(65, 0, 0));
        CreatePart(tail1, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.15f, 0.20f, 0.15f), baseOrange);
        // Tail stripe
        CreatePart(tail1, PrimitiveType.Cube, new Vector3(0, 0.0f, 0), new Vector3(0.16f, 0.015f, 0.04f), darkOrange);

        var tail2 = CreateJoint(tail1, "Tail_2", new Vector3(0, 0.28f, 0));
        tail2.localRotation = Quaternion.Euler(10, 0, 0);
        CreatePart(tail2, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.10f, 0.18f, 0.10f), baseOrange);
        CreatePart(tail2, PrimitiveType.Cube, new Vector3(0, 0, 0), new Vector3(0.11f, 0.012f, 0.035f), darkOrange);

        var tail3 = CreateJoint(tail2, "Tail_3", new Vector3(0, 0.25f, 0));
        tail3.localRotation = Quaternion.Euler(8, 0, 0);
        CreatePart(tail3, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.06f, 0.14f, 0.06f), baseOrange);
        CreatePart(tail3, PrimitiveType.Cube, new Vector3(0, 0, 0), new Vector3(0.07f, 0.010f, 0.03f), scaleColor);

        // Flame joint at tail tip
        var flame = CreateJoint(tail3, "Flame", new Vector3(0, 0.20f, 0));
        // 3-layer flame (outer → mid → inner)
        var flOuter = CreatePart(flame, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.22f, 0.22f, 0.22f), flameDeep);
        flOuter.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetEmissive(new Color(1f, 0.45f, 0.08f), 0.7f);
        var flMid = CreatePart(flame, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.14f, 0.16f, 0.14f), flameMid);
        flMid.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetEmissive(new Color(1f, 0.70f, 0.15f), 0.9f);
        var flInner = CreatePart(flame, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.08f, 0.10f, 0.08f), flameYellow);
        flInner.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetEmissive(new Color(1f, 0.92f, 0.40f), 1.2f);

        FinalizeModel(root, species, isEnemy);
        return root;
    }

    // ===============================================================
    // AQUADON — Articulated Baby Plesiosaur (Plesiosaurus juvenile)
    // Species ID: 4 — ~85 primitives
    // ===============================================================

    private static GameObject CreateAquadon(DinoSpeciesData species, bool isEnemy)
    {
        var root = new GameObject($"Dino_{species.name}");

        // Color palette
        Color baseBlue     = new Color(0.41f, 0.56f, 0.94f);
        Color darkBlue     = baseBlue * 0.72f; darkBlue.a = 1f;
        Color deepBlue     = baseBlue * 0.55f; deepBlue.a = 1f;
        Color bellyColor   = Color.Lerp(baseBlue, Color.white, 0.58f);
        Color flipperColor = baseBlue * 0.78f; flipperColor.a = 1f;
        Color flipperLight = Color.Lerp(baseBlue, Color.white, 0.35f);
        Color irisColor    = new Color(0.20f, 0.40f, 0.90f);
        Color foamWhite    = Color.white;
        Color crestColor   = new Color(0.35f, 0.50f, 0.88f);
        Color stripeColor  = Color.Lerp(baseBlue, Color.white, 0.20f);

        // ==== JOINT HIERARCHY ====
        var body = CreateJoint(root.transform, "Body", Vector3.zero);

        // -- Body geometry (round, turtle-like) --
        CreatePart(body, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.72f, 0.50f, 0.82f), baseBlue);
        // Back volume
        CreatePart(body, PrimitiveType.Sphere, new Vector3(0, 0.06f, -0.05f), new Vector3(0.60f, 0.35f, 0.65f), baseBlue);
        // Belly
        CreatePart(body, PrimitiveType.Sphere, new Vector3(0, -0.12f, 0.02f), new Vector3(0.56f, 0.34f, 0.64f), bellyColor);

        // Flank stripes (soft markings)
        for (int i = 0; i < 3; i++)
        {
            float z = 0.10f - i * 0.15f;
            for (int side = -1; side <= 1; side += 2)
            {
                CreatePart(body, PrimitiveType.Cube,
                    new Vector3(side * 0.30f, 0.02f - i * 0.02f, z),
                    new Vector3(0.04f, 0.015f, 0.08f), stripeColor);
            }
        }

        // ---- NECK chain (4 articulated joints — signature plesiosaur) ----
        var neck1 = CreateJoint(body, "Neck_1", new Vector3(0, 0.14f, 0.36f));
        CreatePart(neck1, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.19f, 0.14f, 0.19f), baseBlue);
        // Dorsal ridge
        CreatePart(neck1, PrimitiveType.Cube, new Vector3(0, 0.09f, 0), new Vector3(0.04f, 0.04f, 0.06f), crestColor);

        var neck2 = CreateJoint(neck1, "Neck_2", new Vector3(0, 0.18f, 0.10f));
        CreatePart(neck2, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.16f, 0.13f, 0.16f), baseBlue);
        CreatePart(neck2, PrimitiveType.Cube, new Vector3(0, 0.08f, 0), new Vector3(0.035f, 0.035f, 0.05f), crestColor);

        var neck3 = CreateJoint(neck2, "Neck_3", new Vector3(0, 0.16f, 0.06f));
        CreatePart(neck3, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.14f, 0.12f, 0.14f), baseBlue);
        CreatePart(neck3, PrimitiveType.Cube, new Vector3(0, 0.07f, 0), new Vector3(0.03f, 0.03f, 0.04f), crestColor);

        var neck4 = CreateJoint(neck3, "Neck_4", new Vector3(0, 0.14f, 0.04f));
        CreatePart(neck4, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.13f, 0.11f, 0.13f), baseBlue);

        // ---- HEAD joint ----
        var head = CreateJoint(neck4, "Head", new Vector3(0, 0.12f, 0.05f));
        CreatePart(head, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.30f, 0.28f, 0.32f), baseBlue);
        // Cheeks (rounder face)
        CreatePart(head, PrimitiveType.Sphere, new Vector3(0.10f, -0.02f, 0.04f), new Vector3(0.08f, 0.07f, 0.07f), baseBlue);
        CreatePart(head, PrimitiveType.Sphere, new Vector3(-0.10f, -0.02f, 0.04f), new Vector3(0.08f, 0.07f, 0.07f), baseBlue);

        // Snout
        CreatePart(head, PrimitiveType.Sphere, new Vector3(0, -0.02f, 0.13f), new Vector3(0.17f, 0.13f, 0.15f), baseBlue);
        // Mouth (gentle smile)
        CreatePart(head, PrimitiveType.Cube, new Vector3(0, -0.08f, 0.14f), new Vector3(0.12f, 0.012f, 0.06f), darkBlue);
        // Small ear flaps
        CreatePart(head, PrimitiveType.Cube, new Vector3(0.12f, 0.06f, -0.04f), new Vector3(0.06f, 0.04f, 0.03f), darkBlue);
        CreatePart(head, PrimitiveType.Cube, new Vector3(-0.12f, 0.06f, -0.04f), new Vector3(0.06f, 0.04f, 0.03f), darkBlue);
        // Head crest
        CreatePart(head, PrimitiveType.Cube, new Vector3(0, 0.14f, -0.02f), new Vector3(0.04f, 0.05f, 0.08f), crestColor);

        // Nostrils
        CreatePart(head, PrimitiveType.Sphere, new Vector3(0.04f, 0.01f, 0.24f), Vector3.one * 0.018f, darkBlue);
        CreatePart(head, PrimitiveType.Sphere, new Vector3(-0.04f, 0.01f, 0.24f), Vector3.one * 0.018f, darkBlue);

        // Eyes (large — adorable, round pupils)
        CreateEyes(head, 0.11f, 0.04f, 0.10f, 0.16f, 0.11f, 0.05f, irisColor, slitPupils: false);

        // ---- FLIPPERS (articulated joints) ----
        // Front flippers (larger, double-layer)
        for (int side = -1; side <= 1; side += 2)
        {
            string fn = side < 0 ? "Flipper_FL" : "Flipper_FR";
            var flipper = CreateJoint(body, fn, new Vector3(side * 0.36f, -0.05f, 0.14f),
                Quaternion.Euler(0, side * 15, side * -18));
            // Main flipper
            CreatePart(flipper, PrimitiveType.Sphere, Vector3.zero,
                new Vector3(0.26f, 0.05f, 0.17f), flipperColor);
            // Lighter membrane
            CreatePart(flipper, PrimitiveType.Sphere, new Vector3(side * 0.04f, 0.005f, 0.02f),
                new Vector3(0.18f, 0.02f, 0.12f), flipperLight);
            // Nervures (2 lines)
            CreatePart(flipper, PrimitiveType.Cube, new Vector3(0, 0.025f, 0.02f),
                new Vector3(0.20f, 0.008f, 0.012f), darkBlue);
            CreatePart(flipper, PrimitiveType.Cube, new Vector3(side * 0.03f, 0.025f, -0.03f),
                new Vector3(0.15f, 0.006f, 0.010f), darkBlue);
        }
        // Rear flippers (smaller)
        for (int side = -1; side <= 1; side += 2)
        {
            string fn = side < 0 ? "Flipper_RL" : "Flipper_RR";
            var flipper = CreateJoint(body, fn, new Vector3(side * 0.30f, -0.10f, -0.26f),
                Quaternion.Euler(0, side * 10, side * -12));
            CreatePart(flipper, PrimitiveType.Sphere, Vector3.zero,
                new Vector3(0.20f, 0.04f, 0.13f), flipperColor);
            CreatePart(flipper, PrimitiveType.Sphere, new Vector3(side * 0.03f, 0.004f, 0.01f),
                new Vector3(0.14f, 0.018f, 0.09f), flipperLight);
        }

        // ---- TAIL + PADDLE joint ----
        var tail1 = CreateJoint(body, "Tail_1", new Vector3(0, 0, -0.42f), Quaternion.Euler(80, 0, 0));
        CreatePart(tail1, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.15f, 0.15f, 0.15f), baseBlue);

        var tailPaddle = CreateJoint(tail1, "TailPaddle", new Vector3(0, 0.20f, 0));
        CreatePart(tailPaddle, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.22f, 0.035f, 0.15f), flipperColor);
        // Paddle nervures
        CreatePart(tailPaddle, PrimitiveType.Cube, new Vector3(0, 0.018f, 0),
            new Vector3(0.18f, 0.006f, 0.01f), darkBlue);

        // ---- FOAM PATCHES (~6 spots) ----
        Vector3[] foamPositions = {
            new Vector3(0.20f, 0.20f, 0.10f), new Vector3(-0.16f, 0.18f, -0.10f),
            new Vector3(0.06f, 0.22f, -0.22f), new Vector3(-0.22f, 0.15f, 0.20f),
            new Vector3(0.12f, 0.10f, 0.28f), new Vector3(-0.08f, 0.20f, 0.02f)
        };
        float[] foamAlphas = { 0.50f, 0.45f, 0.40f, 0.48f, 0.35f, 0.42f };
        for (int i = 0; i < foamPositions.Length; i++)
        {
            float sz = 0.06f + (i % 3) * 0.025f;
            var foam = CreatePart(body, PrimitiveType.Sphere, foamPositions[i],
                new Vector3(sz * 2, sz * 0.4f, sz * 1.5f), foamWhite);
            foam.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetTransparent(foamWhite, foamAlphas[i]);
        }

        FinalizeModel(root, species, isEnemy);
        return root;
    }

    // ===============================================================
    // FLORASAUR — Articulated Baby Sauropod with Flowers (Apatosaurus)
    // Species ID: 7 — ~95 primitives
    // ===============================================================

    private static GameObject CreateFlorasaur(DinoSpeciesData species, bool isEnemy)
    {
        var root = new GameObject($"Dino_{species.name}");

        // Color palette
        Color baseGreen      = new Color(0.47f, 0.78f, 0.30f);
        Color darkGreen      = baseGreen * 0.70f; darkGreen.a = 1f;
        Color deepGreen      = baseGreen * 0.55f; deepGreen.a = 1f;
        Color bellyColor     = Color.Lerp(baseGreen, Color.white, 0.32f);
        Color leafBright     = new Color(0.30f, 0.85f, 0.20f);
        Color leafDark       = new Color(0.20f, 0.55f, 0.12f);
        Color hazelIris      = new Color(0.55f, 0.40f, 0.20f);
        Color barkBrown      = new Color(0.45f, 0.32f, 0.18f);
        Color barkLight      = new Color(0.55f, 0.42f, 0.25f);
        Color flowerPink     = new Color(0.95f, 0.45f, 0.60f);
        Color flowerLavender = new Color(0.75f, 0.55f, 0.90f);
        Color flowerWhite    = new Color(1f, 0.80f, 0.85f);
        Color flowerPeach    = new Color(1f, 0.70f, 0.55f);
        Color flowerBlue     = new Color(0.60f, 0.65f, 0.95f);
        Color flowerCenter   = new Color(1f, 0.85f, 0.20f);
        Color mossColor      = new Color(0.35f, 0.65f, 0.25f);
        Color freckleColor   = new Color(0.40f, 0.30f, 0.18f);
        Color vineColor      = new Color(0.25f, 0.58f, 0.18f);
        Color dewColor       = new Color(0.70f, 0.90f, 1.0f);
        Color butterflyBody  = new Color(0.20f, 0.15f, 0.10f);
        Color butterflyWing  = new Color(1f, 0.65f, 0.20f);

        // ==== JOINT HIERARCHY ====
        var body = CreateJoint(root.transform, "Body", Vector3.zero);

        // -- Body geometry (barrel sauropod) --
        CreatePart(body, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.74f, 0.60f, 0.90f), baseGreen);
        // Back ridge
        CreatePart(body, PrimitiveType.Sphere, new Vector3(0, 0.10f, -0.05f), new Vector3(0.55f, 0.30f, 0.70f), baseGreen);
        // Belly
        CreatePart(body, PrimitiveType.Sphere, new Vector3(0, -0.12f, 0.04f), new Vector3(0.54f, 0.40f, 0.64f), bellyColor);

        // Bark texture patches on flanks
        for (int i = 0; i < 3; i++)
        {
            float z = 0.15f - i * 0.15f;
            for (int side = -1; side <= 1; side += 2)
            {
                var bark = CreatePart(body, PrimitiveType.Cube,
                    new Vector3(side * 0.32f, 0.04f - i * 0.04f, z),
                    new Vector3(0.06f, 0.04f, 0.08f), barkBrown);
                bark.transform.localRotation = Quaternion.Euler(0, i * 10, side * 5);
            }
        }

        // Moss patches (semi-transparent)
        var moss1 = CreatePart(body, PrimitiveType.Sphere, new Vector3(0.15f, 0.28f, 0.05f),
            new Vector3(0.12f, 0.03f, 0.10f), mossColor);
        moss1.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetTransparent(mossColor, 0.50f);
        var moss2 = CreatePart(body, PrimitiveType.Sphere, new Vector3(-0.10f, 0.26f, -0.15f),
            new Vector3(0.10f, 0.025f, 0.08f), mossColor);
        moss2.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetTransparent(mossColor, 0.45f);

        // ---- NECK chain (2 joints, thick sauropod) ----
        var neck1 = CreateJoint(body, "Neck_1", new Vector3(0, 0.14f, 0.40f));
        CreatePart(neck1, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.21f, 0.15f, 0.21f), baseGreen);
        // Vine wrapping around neck
        var vine1 = CreatePart(neck1, PrimitiveType.Cube, new Vector3(0.08f, 0.02f, 0.04f),
            new Vector3(0.03f, 0.10f, 0.03f), vineColor);
        vine1.transform.localRotation = Quaternion.Euler(15, 30, 10);

        var neck2 = CreateJoint(neck1, "Neck_2", new Vector3(0, 0.18f, 0.12f));
        CreatePart(neck2, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.17f, 0.13f, 0.17f), baseGreen);
        var vine2 = CreatePart(neck2, PrimitiveType.Cube, new Vector3(-0.06f, 0.01f, 0.03f),
            new Vector3(0.025f, 0.08f, 0.025f), vineColor);
        vine2.transform.localRotation = Quaternion.Euler(-10, -25, -8);

        // ---- HEAD joint ----
        var head = CreateJoint(neck2, "Head", new Vector3(0, 0.14f, 0.08f));
        CreatePart(head, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.30f, 0.28f, 0.32f), baseGreen);
        // Cheeks (round, friendly)
        CreatePart(head, PrimitiveType.Sphere, new Vector3(0.10f, -0.02f, 0.04f), new Vector3(0.07f, 0.06f, 0.06f), bellyColor);
        CreatePart(head, PrimitiveType.Sphere, new Vector3(-0.10f, -0.02f, 0.04f), new Vector3(0.07f, 0.06f, 0.06f), bellyColor);
        // Freckles
        CreatePart(head, PrimitiveType.Sphere, new Vector3(0.08f, -0.01f, 0.12f), Vector3.one * 0.018f, freckleColor);
        CreatePart(head, PrimitiveType.Sphere, new Vector3(0.11f, -0.03f, 0.10f), Vector3.one * 0.014f, freckleColor);
        CreatePart(head, PrimitiveType.Sphere, new Vector3(-0.08f, -0.01f, 0.12f), Vector3.one * 0.018f, freckleColor);
        CreatePart(head, PrimitiveType.Sphere, new Vector3(-0.11f, -0.03f, 0.10f), Vector3.one * 0.014f, freckleColor);

        // Snout (broad, flat)
        CreatePart(head, PrimitiveType.Sphere, new Vector3(0, -0.02f, 0.13f), new Vector3(0.19f, 0.13f, 0.16f), baseGreen);
        // Smile
        var smileL = CreatePart(head, PrimitiveType.Cube, new Vector3(-0.04f, -0.09f, 0.16f),
            new Vector3(0.06f, 0.012f, 0.03f), darkGreen);
        smileL.transform.localRotation = Quaternion.Euler(0, 0, -12);
        var smileR = CreatePart(head, PrimitiveType.Cube, new Vector3(0.04f, -0.09f, 0.16f),
            new Vector3(0.06f, 0.012f, 0.03f), darkGreen);
        smileR.transform.localRotation = Quaternion.Euler(0, 0, 12);

        // Eyes (hazel — unique)
        CreateEyes(head, 0.10f, 0.04f, 0.10f, 0.14f, 0.09f, 0.04f, hazelIris, slitPupils: false);

        // Sprouts on head (3 with buds)
        float[] sproutX = { -0.06f, 0.06f, 0f };
        float[] sproutH = { 0.08f, 0.07f, 0.10f };
        for (int i = 0; i < 3; i++)
        {
            CreatePart(head, PrimitiveType.Capsule,
                new Vector3(sproutX[i], 0.15f, -0.03f + i * 0.02f),
                new Vector3(0.022f, sproutH[i], 0.022f), leafBright);
            // Leaf/bud at tip
            CreatePart(head, PrimitiveType.Cube,
                new Vector3(sproutX[i], 0.15f + sproutH[i], -0.01f + i * 0.02f),
                new Vector3(0.045f, 0.015f, 0.035f), i == 2 ? flowerPink : leafBright);
        }

        // ---- LEGS (4 columnar joints) ----
        string[] legNames = { "Leg_FR", "Leg_FL", "Leg_RR", "Leg_RL" };
        float[][] legPos = {
            new float[] { 0.24f, -0.32f, 0.20f },
            new float[] { -0.24f, -0.32f, 0.20f },
            new float[] { 0.24f, -0.32f, -0.22f },
            new float[] { -0.24f, -0.32f, -0.22f },
        };
        for (int i = 0; i < 4; i++)
        {
            var leg = CreateJoint(body, legNames[i],
                new Vector3(legPos[i][0], legPos[i][1], legPos[i][2]));
            // Trunk-like leg
            CreatePart(leg, PrimitiveType.Capsule, Vector3.zero,
                new Vector3(0.17f, 0.21f, 0.17f), darkGreen);
            // Bark rings on leg
            CreatePart(leg, PrimitiveType.Cube, new Vector3(0, 0.05f, 0),
                new Vector3(0.18f, 0.015f, 0.18f), barkLight);
            CreatePart(leg, PrimitiveType.Cube, new Vector3(0, -0.06f, 0),
                new Vector3(0.17f, 0.012f, 0.17f), barkBrown);
            // Wide flat foot
            CreatePart(leg, PrimitiveType.Sphere, new Vector3(0, -0.20f, 0.02f),
                new Vector3(0.16f, 0.05f, 0.18f), darkGreen);
            // Root details at base of feet
            CreatePart(leg, PrimitiveType.Cube, new Vector3(0.05f, -0.22f, 0.08f),
                new Vector3(0.02f, 0.02f, 0.06f), barkBrown);
            CreatePart(leg, PrimitiveType.Cube, new Vector3(-0.04f, -0.22f, 0.06f),
                new Vector3(0.018f, 0.018f, 0.05f), barkBrown);
        }

        // ---- TAIL (chained: Tail_1 → Tail_2 → TailLeaf) ----
        var tail1j = CreateJoint(body, "Tail_1", new Vector3(0, 0.02f, -0.46f), Quaternion.Euler(70, 0, 0));
        CreatePart(tail1j, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.16f, 0.20f, 0.16f), baseGreen);
        // Vine on tail
        var tailVine = CreatePart(tail1j, PrimitiveType.Cube, new Vector3(0.05f, 0.08f, 0),
            new Vector3(0.02f, 0.12f, 0.02f), vineColor);
        tailVine.transform.localRotation = Quaternion.Euler(0, 20, 10);

        var tail2j = CreateJoint(tail1j, "Tail_2", new Vector3(0, 0.28f, 0), Quaternion.Euler(10, 0, 0));
        CreatePart(tail2j, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.10f, 0.16f, 0.10f), baseGreen);

        var tailLeaf = CreateJoint(tail2j, "TailLeaf", new Vector3(0, 0.22f, 0));
        // Large leaf at tip
        CreatePart(tailLeaf, PrimitiveType.Cube, Vector3.zero, new Vector3(0.14f, 0.025f, 0.10f), leafBright);
        // Leaf veins
        CreatePart(tailLeaf, PrimitiveType.Cube, new Vector3(0, 0.013f, 0), new Vector3(0.01f, 0.008f, 0.08f), leafDark);

        // ---- FLOWERS ON BACK (5 flowers on joints for sway animation) ----
        Color[] flColors = { flowerPink, flowerLavender, flowerWhite, flowerPeach, flowerBlue };
        Vector3[] flPos = {
            new Vector3(0, 0.38f, 0.14f),
            new Vector3(0.08f, 0.36f, -0.02f),
            new Vector3(-0.06f, 0.34f, -0.16f),
            new Vector3(0.10f, 0.32f, -0.28f),
            new Vector3(-0.08f, 0.30f, 0.04f),
        };
        int[] flPetals = { 5, 5, 4, 5, 4 };
        float[] flSizes = { 0.065f, 0.055f, 0.045f, 0.050f, 0.048f };
        for (int i = 0; i < 5; i++)
        {
            var flowerJoint = CreateJoint(body, $"Flower_{i + 1}", flPos[i]);
            // Stem
            CreatePart(flowerJoint, PrimitiveType.Capsule, new Vector3(0, -0.03f, 0),
                new Vector3(0.015f, 0.04f, 0.015f), vineColor);
            // Flower
            CreateFlower(flowerJoint, Vector3.zero, flSizes[i], flPetals[i], flColors[i], flowerCenter);
            // Dewdrop on one petal (every other flower)
            if (i % 2 == 0)
            {
                var dew = CreatePart(flowerJoint, PrimitiveType.Sphere,
                    new Vector3(flSizes[i] * 0.6f, 0.01f, 0),
                    Vector3.one * 0.018f, dewColor);
                dew.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetTransparent(dewColor, 0.6f);
            }
        }

        // Leaves between flowers
        for (int i = 0; i < 3; i++)
        {
            float lx = (i % 2 == 0) ? 0.10f : -0.08f;
            float lz = 0.08f - i * 0.12f;
            var leaf = CreatePart(body, PrimitiveType.Cube,
                new Vector3(lx, 0.33f - i * 0.02f, lz),
                new Vector3(0.07f, 0.016f, 0.05f), leafBright);
            leaf.transform.localRotation = Quaternion.Euler(0, 20 * (i % 2 == 0 ? 1 : -1), -10 + i * 5);
        }

        // Leaf motifs on flanks
        for (int side = -1; side <= 1; side += 2)
        {
            var motif = CreatePart(body, PrimitiveType.Cube,
                new Vector3(side * 0.33f, 0.08f, 0.02f),
                new Vector3(0.08f, 0.016f, 0.06f), leafDark);
            motif.transform.localRotation = Quaternion.Euler(0, side * 15, side * -18);
        }

        // ---- BUTTERFLY (3 primitives near main flower) ----
        var butterfly = CreateJoint(body, "Butterfly", new Vector3(0.15f, 0.50f, 0.16f));
        // Body
        CreatePart(butterfly, PrimitiveType.Capsule, Vector3.zero,
            new Vector3(0.012f, 0.02f, 0.012f), butterflyBody);
        // Wings
        var wingL = CreatePart(butterfly, PrimitiveType.Cube, new Vector3(-0.025f, 0.005f, 0),
            new Vector3(0.04f, 0.003f, 0.03f), butterflyWing);
        wingL.transform.localRotation = Quaternion.Euler(0, 0, -15);
        var wingR = CreatePart(butterfly, PrimitiveType.Cube, new Vector3(0.025f, 0.005f, 0),
            new Vector3(0.04f, 0.003f, 0.03f), butterflyWing);
        wingR.transform.localRotation = Quaternion.Euler(0, 0, 15);

        FinalizeModel(root, species, isEnemy);
        return root;
    }

    // ===============================================================
    // PYROVORE — Sleek Teen Fire Theropod (Allosaurus)
    // Species ID: 2
    // ===============================================================

    private static GameObject CreatePyrovore(DinoSpeciesData species, bool isEnemy)
    {
        var root = new GameObject($"Dino_{species.name}");
        Color base1 = new Color(0.65f, 0.18f, 0.12f); // dark red
        Color dark1 = base1 * 0.6f; dark1.a = 1f;
        Color belly = Color.Lerp(base1, new Color(1f, 0.5f, 0.2f), 0.25f);
        Color veinGlow = new Color(1f, 0.4f, 0.08f);
        Color clawBlack = new Color(0.10f, 0.06f, 0.04f);
        Color flameYellow = new Color(1f, 0.75f, 0.10f);
        Color flameDeep = new Color(1f, 0.25f, 0.05f);

        var body = CreateJoint(root.transform, "Body", Vector3.zero, Quaternion.Euler(12, 0, 0));
        CreatePart(body, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.58f, 0.52f, 0.78f), base1);
        CreatePart(body, PrimitiveType.Sphere, new Vector3(0, 0.03f, 0.18f), new Vector3(0.44f, 0.42f, 0.40f), base1);
        CreatePart(body, PrimitiveType.Sphere, new Vector3(0, -0.12f, 0.05f), new Vector3(0.40f, 0.32f, 0.52f), belly);
        // Incandescent veins on flanks
        for (int side = -1; side <= 1; side += 2)
        {
            for (int i = 0; i < 3; i++)
            {
                var vein = CreatePart(body, PrimitiveType.Cube,
                    new Vector3(side * 0.27f, 0.02f - i * 0.05f, 0.20f - i * 0.15f),
                    new Vector3(0.035f, 0.012f, 0.12f), veinGlow);
                vein.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetEmissive(veinGlow, 0.4f);
            }
        }

        var neck = CreateJoint(body, "Neck", new Vector3(0, 0.18f, 0.38f));
        CreatePart(neck, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.26f, 0.26f, 0.26f), base1);

        var head = CreateJoint(neck, "Head", new Vector3(0, 0.15f, 0.14f));
        CreatePart(head, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.38f, 0.34f, 0.44f), base1);
        CreatePart(head, PrimitiveType.Sphere, new Vector3(0, -0.03f, 0.18f), new Vector3(0.26f, 0.18f, 0.26f), base1);
        CreateEyes(head, 0.13f, 0.05f, 0.14f, 0.12f, 0.08f, 0.04f, new Color(0.95f, 0.30f, 0.08f), slitPupils: true);

        var jaw = CreateJoint(head, "Jaw", new Vector3(0, -0.12f, 0.10f));
        CreatePart(jaw, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.20f, 0.06f, 0.18f), dark1);
        for (int i = 0; i < 4; i++)
        {
            CreatePart(head, PrimitiveType.Cube, new Vector3(-0.06f + i * 0.04f, -0.11f, 0.22f + i * 0.015f),
                new Vector3(0.015f, 0.035f, 0.010f), Color.white);
        }

        // Fire mane (5 large flames along head/neck)
        var crest = CreateJoint(head, "Crest", new Vector3(0, 0.16f, -0.05f));
        for (int i = 0; i < 5; i++)
        {
            Color c = Color.Lerp(flameYellow, flameDeep, i * 0.2f);
            var spike = CreatePart(crest, PrimitiveType.Cube,
                new Vector3(0, 0.02f, -i * 0.08f), new Vector3(0.06f, 0.18f + i * 0.02f, 0.05f), c);
            spike.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetEmissive(c, 0.6f);
        }

        // Arms (short with smoking claws)
        for (int side = -1; side <= 1; side += 2)
        {
            var arm = CreateJoint(body, side < 0 ? "Arm_L" : "Arm_R",
                new Vector3(side * 0.28f, 0.06f, 0.22f), Quaternion.Euler(10, 0, side * -20));
            CreatePart(arm, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.07f, 0.10f, 0.07f), dark1);
            CreatePart(arm, PrimitiveType.Capsule, new Vector3(0, -0.14f, 0.02f), new Vector3(0.05f, 0.07f, 0.05f), dark1);
            for (int c = -1; c <= 1; c++)
                CreatePart(arm, PrimitiveType.Cube, new Vector3(c * 0.02f, -0.20f, 0.04f),
                    new Vector3(0.014f, 0.04f, 0.014f), clawBlack);
        }

        // Legs (muscular biped)
        for (int side = -1; side <= 1; side += 2)
        {
            var leg = CreateJoint(body, side < 0 ? "Leg_L" : "Leg_R",
                new Vector3(side * 0.20f, -0.24f, -0.05f));
            CreatePart(leg, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.16f, 0.20f, 0.16f), dark1);
            CreatePart(leg, PrimitiveType.Capsule, new Vector3(0, -0.26f, 0.10f),
                new Vector3(0.10f, 0.16f, 0.10f), dark1);
            CreatePart(leg, PrimitiveType.Sphere, new Vector3(0, -0.40f, 0.16f),
                new Vector3(0.15f, 0.05f, 0.18f), dark1);
            for (int t = -1; t <= 1; t++)
                CreatePart(leg, PrimitiveType.Cube, new Vector3(t * 0.04f, -0.42f, 0.28f),
                    new Vector3(0.018f, 0.03f, 0.05f), clawBlack);
        }

        // Tail
        var tail1 = CreateJoint(body, "Tail_1", new Vector3(0, 0.02f, -0.42f), Quaternion.Euler(60, 0, 0));
        CreatePart(tail1, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.16f, 0.22f, 0.16f), base1);
        var tail2 = CreateJoint(tail1, "Tail_2", new Vector3(0, 0.30f, 0), Quaternion.Euler(10, 0, 0));
        CreatePart(tail2, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.10f, 0.18f, 0.10f), base1);
        var tail3 = CreateJoint(tail2, "Tail_3", new Vector3(0, 0.26f, 0));
        CreatePart(tail3, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.06f, 0.14f, 0.06f), base1);

        FinalizeModel(root, species, isEnemy);
        return root;
    }

    // ===============================================================
    // VOLCANOREX — Colossal Fire/Rock T-Rex
    // Species ID: 3
    // ===============================================================

    private static GameObject CreateVolcanorex(DinoSpeciesData species, bool isEnemy)
    {
        var root = new GameObject($"Dino_{species.name}");
        Color base1 = new Color(0.50f, 0.15f, 0.08f);
        Color dark1 = base1 * 0.55f; dark1.a = 1f;
        Color belly = new Color(0.70f, 0.35f, 0.15f);
        Color lavaOrange = new Color(1f, 0.50f, 0.05f);
        Color rockGray = new Color(0.40f, 0.35f, 0.30f);
        Color emberGlow = new Color(1f, 0.35f, 0.05f);
        Color smokeColor = new Color(0.25f, 0.20f, 0.15f);

        var body = CreateJoint(root.transform, "Body", Vector3.zero, Quaternion.Euler(8, 0, 0));
        CreatePart(body, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.85f, 0.75f, 1.10f), base1);
        CreatePart(body, PrimitiveType.Sphere, new Vector3(0, -0.15f, 0.08f), new Vector3(0.60f, 0.50f, 0.75f), belly);
        // Volcanic rock plates on back
        for (int i = 0; i < 4; i++)
        {
            var plate = CreatePart(body, PrimitiveType.Cube,
                new Vector3(0, 0.40f + i * 0.04f, 0.20f - i * 0.20f),
                new Vector3(0.55f - i * 0.06f, 0.10f, 0.22f), rockGray);
            // Lava between plates
            var lava = CreatePart(body, PrimitiveType.Cube,
                new Vector3(0, 0.37f + i * 0.04f, 0.22f - i * 0.20f),
                new Vector3(0.50f - i * 0.06f, 0.025f, 0.18f), lavaOrange);
            lava.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetEmissive(lavaOrange, 0.7f);
        }

        var neck = CreateJoint(body, "Neck", new Vector3(0, 0.22f, 0.50f));
        CreatePart(neck, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.38f, 0.36f, 0.36f), base1);

        var head = CreateJoint(neck, "Head", new Vector3(0, 0.18f, 0.16f));
        CreatePart(head, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.52f, 0.46f, 0.58f), base1);
        CreatePart(head, PrimitiveType.Sphere, new Vector3(0, -0.04f, 0.24f), new Vector3(0.36f, 0.28f, 0.32f), base1);
        // Ember eyes
        CreateEyes(head, 0.18f, 0.06f, 0.16f, 0.14f, 0.09f, 0.05f, emberGlow, slitPupils: true);
        // Smoke from nostrils
        var sm1 = CreatePart(head, PrimitiveType.Sphere, new Vector3(0.07f, 0.02f, 0.40f), Vector3.one * 0.06f, smokeColor);
        sm1.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetTransparent(smokeColor, 0.30f);
        var sm2 = CreatePart(head, PrimitiveType.Sphere, new Vector3(-0.07f, 0.02f, 0.40f), Vector3.one * 0.06f, smokeColor);
        sm2.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetTransparent(smokeColor, 0.30f);

        var jaw = CreateJoint(head, "Jaw", new Vector3(0, -0.16f, 0.12f));
        CreatePart(jaw, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.30f, 0.10f, 0.26f), dark1);
        for (int i = 0; i < 5; i++)
            CreatePart(head, PrimitiveType.Cube, new Vector3(-0.08f + i * 0.04f, -0.15f, 0.28f + i * 0.01f),
                new Vector3(0.018f, 0.05f, 0.012f), Color.white);

        // Tiny arms
        for (int side = -1; side <= 1; side += 2)
        {
            var arm = CreateJoint(body, side < 0 ? "Arm_L" : "Arm_R",
                new Vector3(side * 0.38f, 0.08f, 0.30f), Quaternion.Euler(15, 0, side * -20));
            CreatePart(arm, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.08f, 0.10f, 0.08f), dark1);
            CreatePart(arm, PrimitiveType.Cube, new Vector3(0, -0.12f, 0.03f), new Vector3(0.02f, 0.05f, 0.02f), new Color(0.1f, 0.08f, 0.05f));
        }

        // Massive legs
        for (int side = -1; side <= 1; side += 2)
        {
            var leg = CreateJoint(body, side < 0 ? "Leg_L" : "Leg_R",
                new Vector3(side * 0.28f, -0.32f, -0.10f));
            CreatePart(leg, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.22f, 0.28f, 0.22f), dark1);
            CreatePart(leg, PrimitiveType.Capsule, new Vector3(0, -0.36f, 0.12f),
                new Vector3(0.14f, 0.20f, 0.14f), dark1);
            CreatePart(leg, PrimitiveType.Sphere, new Vector3(0, -0.52f, 0.18f),
                new Vector3(0.20f, 0.06f, 0.24f), dark1);
        }

        // Tail with molten rock mass
        var t1 = CreateJoint(body, "Tail_1", new Vector3(0, 0.02f, -0.58f), Quaternion.Euler(55, 0, 0));
        CreatePart(t1, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.22f, 0.28f, 0.22f), base1);
        var t2 = CreateJoint(t1, "Tail_2", new Vector3(0, 0.38f, 0), Quaternion.Euler(10, 0, 0));
        CreatePart(t2, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.16f, 0.24f, 0.16f), base1);
        var t3 = CreateJoint(t2, "Tail_3", new Vector3(0, 0.32f, 0));
        CreatePart(t3, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.28f, 0.25f, 0.28f), rockGray);
        var molten = CreatePart(t3, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.20f, 0.18f, 0.20f), lavaOrange);
        molten.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetEmissive(lavaOrange, 0.8f);

        FinalizeModel(root, species, isEnemy);
        return root;
    }

    // ===============================================================
    // MAREXIS — Sleek Teen Marine Reptile (Mosasaurus juvenile)
    // Species ID: 5
    // ===============================================================

    private static GameObject CreateMarexis(DinoSpeciesData species, bool isEnemy)
    {
        var root = new GameObject($"Dino_{species.name}");
        Color base1 = new Color(0.15f, 0.22f, 0.55f); // midnight blue
        Color dark1 = base1 * 0.65f; dark1.a = 1f;
        Color belly = Color.Lerp(base1, Color.white, 0.45f);
        Color finColor = new Color(0.20f, 0.30f, 0.65f);
        Color bioGlow = new Color(0.30f, 0.70f, 1.0f);

        var body = CreateJoint(root.transform, "Body", Vector3.zero);
        CreatePart(body, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.55f, 0.42f, 0.90f), base1);
        CreatePart(body, PrimitiveType.Sphere, new Vector3(0, -0.10f, 0.02f), new Vector3(0.42f, 0.28f, 0.70f), belly);
        // Bioluminescent patterns
        for (int side = -1; side <= 1; side += 2)
        {
            for (int i = 0; i < 4; i++)
            {
                var dot = CreatePart(body, PrimitiveType.Sphere,
                    new Vector3(side * 0.25f, -0.02f + i * 0.02f, 0.25f - i * 0.14f),
                    Vector3.one * (0.03f - i * 0.004f), bioGlow);
                dot.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetEmissive(bioGlow, 0.5f);
            }
        }
        // Dorsal fin
        CreatePart(body, PrimitiveType.Cube, new Vector3(0, 0.28f, -0.05f),
            new Vector3(0.04f, 0.22f, 0.40f), finColor);

        // Elongated head (no long neck — mosasaur)
        var neck = CreateJoint(body, "Neck", new Vector3(0, 0.06f, 0.42f));
        CreatePart(neck, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.28f, 0.24f, 0.28f), base1);

        var head = CreateJoint(neck, "Head", new Vector3(0, 0.06f, 0.16f));
        CreatePart(head, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.30f, 0.26f, 0.42f), base1);
        CreatePart(head, PrimitiveType.Sphere, new Vector3(0, -0.02f, 0.20f), new Vector3(0.18f, 0.14f, 0.20f), base1);
        CreateEyes(head, 0.12f, 0.04f, 0.08f, 0.11f, 0.07f, 0.04f, new Color(0.20f, 0.50f, 0.95f), slitPupils: false);

        var jaw = CreateJoint(head, "Jaw", new Vector3(0, -0.10f, 0.12f));
        CreatePart(jaw, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.16f, 0.05f, 0.22f), dark1);
        for (int i = 0; i < 5; i++)
            CreatePart(jaw, PrimitiveType.Cube, new Vector3(-0.04f + i * 0.02f, 0.03f, 0.02f + i * 0.03f),
                new Vector3(0.008f, 0.02f, 0.006f), Color.white);

        // Side fins
        for (int side = -1; side <= 1; side += 2)
        {
            string fn = side < 0 ? "Flipper_FL" : "Flipper_FR";
            var fin = CreateJoint(body, fn, new Vector3(side * 0.30f, -0.04f, 0.15f),
                Quaternion.Euler(0, side * 15, side * -15));
            CreatePart(fin, PrimitiveType.Cube, Vector3.zero, new Vector3(0.28f, 0.03f, 0.14f), finColor);
        }
        for (int side = -1; side <= 1; side += 2)
        {
            string fn = side < 0 ? "Flipper_RL" : "Flipper_RR";
            var fin = CreateJoint(body, fn, new Vector3(side * 0.24f, -0.06f, -0.28f),
                Quaternion.Euler(0, side * 10, side * -10));
            CreatePart(fin, PrimitiveType.Cube, Vector3.zero, new Vector3(0.20f, 0.025f, 0.10f), finColor);
        }

        // Tail with sharp fin
        var tail1 = CreateJoint(body, "Tail_1", new Vector3(0, 0, -0.48f), Quaternion.Euler(80, 0, 0));
        CreatePart(tail1, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.14f, 0.20f, 0.14f), base1);
        CreatePart(tail1, PrimitiveType.Cube, new Vector3(0, 0.10f, 0), new Vector3(0.03f, 0.14f, 0.08f), finColor);
        var tp = CreateJoint(tail1, "TailPaddle", new Vector3(0, 0.28f, 0));
        CreatePart(tp, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.22f, 0.04f, 0.16f), finColor);

        FinalizeModel(root, species, isEnemy);
        return root;
    }

    // ===============================================================
    // ABYSSAURE — Titanic Abyssal Leviathan (Water/Ice)
    // Species ID: 6
    // ===============================================================

    private static GameObject CreateAbyssaure(DinoSpeciesData species, bool isEnemy)
    {
        var root = new GameObject($"Dino_{species.name}");
        Color base1 = new Color(0.08f, 0.10f, 0.28f); // near-black blue
        Color dark1 = base1 * 0.55f; dark1.a = 1f;
        Color belly = new Color(0.15f, 0.18f, 0.35f);
        Color iceBlue = new Color(0.55f, 0.80f, 1.0f);
        Color glacialGlow = new Color(0.40f, 0.75f, 1.0f);
        Color finColor = new Color(0.12f, 0.15f, 0.40f);

        var body = CreateJoint(root.transform, "Body", Vector3.zero);
        CreatePart(body, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.80f, 0.60f, 1.15f), base1);
        CreatePart(body, PrimitiveType.Sphere, new Vector3(0, -0.14f, 0.05f), new Vector3(0.58f, 0.38f, 0.85f), belly);

        var neck = CreateJoint(body, "Neck", new Vector3(0, 0.10f, 0.54f));
        CreatePart(neck, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.36f, 0.34f, 0.36f), base1);

        var head = CreateJoint(neck, "Head", new Vector3(0, 0.12f, 0.18f));
        CreatePart(head, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.48f, 0.40f, 0.56f), base1);
        CreatePart(head, PrimitiveType.Sphere, new Vector3(0, -0.04f, 0.24f), new Vector3(0.30f, 0.22f, 0.28f), base1);
        // Glacial eyes
        CreateEyes(head, 0.16f, 0.05f, 0.14f, 0.14f, 0.10f, 0.05f, glacialGlow, slitPupils: true);
        // Ice crystal crown
        var crest = CreateJoint(head, "Crest", new Vector3(0, 0.22f, 0f));
        float[] crX = { 0, -0.10f, 0.10f, -0.06f, 0.06f };
        float[] crH = { 0.20f, 0.14f, 0.14f, 0.10f, 0.10f };
        for (int i = 0; i < 5; i++)
        {
            var crystal = CreatePart(crest, PrimitiveType.Cube, new Vector3(crX[i], crH[i] * 0.5f, -0.02f * i),
                new Vector3(0.05f, crH[i], 0.04f), iceBlue);
            crystal.transform.localRotation = Quaternion.Euler(0, i * 20, (i % 2 == 0 ? 5 : -5));
            crystal.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetEmissive(iceBlue, 0.4f);
        }

        var jaw = CreateJoint(head, "Jaw", new Vector3(0, -0.14f, 0.14f));
        CreatePart(jaw, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.26f, 0.08f, 0.24f), dark1);

        // Immense wing-like fins
        for (int side = -1; side <= 1; side += 2)
        {
            string fn = side < 0 ? "Flipper_FL" : "Flipper_FR";
            var fin = CreateJoint(body, fn, new Vector3(side * 0.42f, -0.02f, 0.10f),
                Quaternion.Euler(0, side * 20, side * -12));
            CreatePart(fin, PrimitiveType.Cube, Vector3.zero, new Vector3(0.45f, 0.04f, 0.22f), finColor);
            CreatePart(fin, PrimitiveType.Cube, new Vector3(side * 0.08f, 0.005f, 0),
                new Vector3(0.30f, 0.015f, 0.15f), Color.Lerp(finColor, iceBlue, 0.2f));
        }
        for (int side = -1; side <= 1; side += 2)
        {
            string fn = side < 0 ? "Flipper_RL" : "Flipper_RR";
            var fin = CreateJoint(body, fn, new Vector3(side * 0.35f, -0.08f, -0.35f),
                Quaternion.Euler(0, side * 12, side * -8));
            CreatePart(fin, PrimitiveType.Cube, Vector3.zero, new Vector3(0.30f, 0.03f, 0.14f), finColor);
        }

        // Tail with frost-covered fin
        var t1 = CreateJoint(body, "Tail_1", new Vector3(0, 0, -0.60f), Quaternion.Euler(75, 0, 0));
        CreatePart(t1, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.20f, 0.28f, 0.20f), base1);
        var t2 = CreateJoint(t1, "Tail_2", new Vector3(0, 0.38f, 0));
        CreatePart(t2, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.14f, 0.22f, 0.14f), base1);
        var tp = CreateJoint(t2, "TailPaddle", new Vector3(0, 0.30f, 0));
        CreatePart(tp, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.32f, 0.05f, 0.20f), finColor);
        // Frost on tail
        var frost = CreatePart(tp, PrimitiveType.Sphere, new Vector3(0, 0.03f, 0), new Vector3(0.26f, 0.03f, 0.16f), iceBlue);
        frost.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetEmissive(iceBlue, 0.3f);

        FinalizeModel(root, species, isEnemy);
        return root;
    }

    // ===============================================================
    // SYLVACOLLE — Medium Sauropod with Trees (Brachiosaurus juvenile)
    // Species ID: 8
    // ===============================================================

    private static GameObject CreateSylvacolle(DinoSpeciesData species, bool isEnemy)
    {
        var root = new GameObject($"Dino_{species.name}");
        Color base1 = new Color(0.25f, 0.50f, 0.18f); // forest green
        Color dark1 = base1 * 0.65f; dark1.a = 1f;
        Color belly = Color.Lerp(base1, Color.white, 0.28f);
        Color barkBrown = new Color(0.42f, 0.30f, 0.16f);
        Color ivyGreen = new Color(0.22f, 0.55f, 0.15f);
        Color mossBrown = new Color(0.38f, 0.32f, 0.20f);
        Color mushGlow = new Color(0.50f, 0.80f, 0.40f);

        var body = CreateJoint(root.transform, "Body", Vector3.zero);
        CreatePart(body, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.70f, 0.58f, 0.92f), base1);
        CreatePart(body, PrimitiveType.Sphere, new Vector3(0, -0.12f, 0.04f), new Vector3(0.52f, 0.38f, 0.64f), belly);
        // Moss-brown patches
        CreatePart(body, PrimitiveType.Sphere, new Vector3(0.22f, 0.12f, 0.10f), new Vector3(0.14f, 0.06f, 0.12f), mossBrown);
        CreatePart(body, PrimitiveType.Sphere, new Vector3(-0.18f, 0.14f, -0.15f), new Vector3(0.12f, 0.05f, 0.10f), mossBrown);

        // Longer neck (brachiosaur) with ivy
        var n1 = CreateJoint(body, "Neck_1", new Vector3(0, 0.16f, 0.42f));
        CreatePart(n1, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.20f, 0.16f, 0.20f), base1);
        var ivy1 = CreatePart(n1, PrimitiveType.Cube, new Vector3(0.07f, 0, 0.04f),
            new Vector3(0.025f, 0.12f, 0.025f), ivyGreen);
        ivy1.transform.localRotation = Quaternion.Euler(10, 25, 8);

        var n2 = CreateJoint(n1, "Neck_2", new Vector3(0, 0.20f, 0.10f));
        CreatePart(n2, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.17f, 0.14f, 0.17f), base1);
        var ivy2 = CreatePart(n2, PrimitiveType.Cube, new Vector3(-0.06f, 0, 0.03f),
            new Vector3(0.02f, 0.10f, 0.02f), ivyGreen);
        ivy2.transform.localRotation = Quaternion.Euler(-8, -20, -6);

        var head = CreateJoint(n2, "Head", new Vector3(0, 0.16f, 0.08f));
        CreatePart(head, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.28f, 0.26f, 0.30f), base1);
        CreatePart(head, PrimitiveType.Sphere, new Vector3(0, -0.02f, 0.12f), new Vector3(0.18f, 0.13f, 0.15f), base1);
        CreateEyes(head, 0.10f, 0.04f, 0.10f, 0.13f, 0.08f, 0.04f, new Color(0.50f, 0.38f, 0.18f), slitPupils: false);

        // Trees and bushes on back
        for (int i = 0; i < 3; i++)
        {
            string fn = $"Flower_{i + 1}";
            var tree = CreateJoint(body, fn, new Vector3(0.06f - i * 0.05f, 0.36f, 0.15f - i * 0.16f));
            // Trunk
            CreatePart(tree, PrimitiveType.Cylinder, new Vector3(0, 0.04f, 0),
                new Vector3(0.03f, 0.08f, 0.03f), barkBrown);
            // Canopy
            CreatePart(tree, PrimitiveType.Sphere, new Vector3(0, 0.12f, 0),
                new Vector3(0.12f - i * 0.02f, 0.10f, 0.12f - i * 0.02f), ivyGreen);
        }
        // Bushes
        CreatePart(body, PrimitiveType.Sphere, new Vector3(0.14f, 0.32f, -0.05f),
            new Vector3(0.08f, 0.06f, 0.08f), ivyGreen);
        CreatePart(body, PrimitiveType.Sphere, new Vector3(-0.12f, 0.30f, 0.08f),
            new Vector3(0.07f, 0.05f, 0.07f), base1);

        // 4 legs
        string[] legNames = { "Leg_FR", "Leg_FL", "Leg_RR", "Leg_RL" };
        float[][] lp = {
            new float[] { 0.24f, -0.32f, 0.22f }, new float[] { -0.24f, -0.32f, 0.22f },
            new float[] { 0.24f, -0.32f, -0.24f }, new float[] { -0.24f, -0.32f, -0.24f }
        };
        for (int i = 0; i < 4; i++)
        {
            var leg = CreateJoint(body, legNames[i], new Vector3(lp[i][0], lp[i][1], lp[i][2]));
            CreatePart(leg, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.16f, 0.21f, 0.16f), dark1);
            CreatePart(leg, PrimitiveType.Sphere, new Vector3(0, -0.21f, 0.02f),
                new Vector3(0.15f, 0.05f, 0.17f), dark1);
        }

        // Tail with glowing mushrooms
        var tail1 = CreateJoint(body, "Tail_1", new Vector3(0, 0.02f, -0.48f), Quaternion.Euler(68, 0, 0));
        CreatePart(tail1, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.15f, 0.20f, 0.15f), base1);
        // Luminous mushrooms at tail base
        for (int i = 0; i < 3; i++)
        {
            float a = i * 120f * Mathf.Deg2Rad;
            var mush = CreatePart(tail1, PrimitiveType.Sphere,
                new Vector3(Mathf.Sin(a) * 0.10f, -0.05f, Mathf.Cos(a) * 0.06f),
                new Vector3(0.05f, 0.04f, 0.05f), mushGlow);
            mush.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetEmissive(mushGlow, 0.4f);
            CreatePart(tail1, PrimitiveType.Cylinder,
                new Vector3(Mathf.Sin(a) * 0.10f, -0.08f, Mathf.Cos(a) * 0.06f),
                new Vector3(0.015f, 0.025f, 0.015f), barkBrown);
        }
        var tail2 = CreateJoint(tail1, "Tail_2", new Vector3(0, 0.28f, 0));
        CreatePart(tail2, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.09f, 0.16f, 0.09f), base1);

        FinalizeModel(root, species, isEnemy);
        return root;
    }

    // ===============================================================
    // TITANARBORE — Titanic Living Ecosystem Sauropod (Plant/Earth)
    // Species ID: 9
    // ===============================================================

    private static GameObject CreateTitanarbore(DinoSpeciesData species, bool isEnemy)
    {
        var root = new GameObject($"Dino_{species.name}");
        Color base1 = new Color(0.30f, 0.48f, 0.20f); // deep moss green
        Color dark1 = base1 * 0.60f; dark1.a = 1f;
        Color belly = Color.Lerp(base1, new Color(0.55f, 0.45f, 0.25f), 0.3f);
        Color barkBrown = new Color(0.40f, 0.28f, 0.14f);
        Color barkDark = new Color(0.30f, 0.20f, 0.10f);
        Color leafDark = new Color(0.18f, 0.42f, 0.12f);
        Color leafBright = new Color(0.28f, 0.70f, 0.18f);
        Color mossColor = new Color(0.32f, 0.55f, 0.22f);
        Color vineColor = new Color(0.22f, 0.50f, 0.15f);
        Color earthBrown = new Color(0.55f, 0.42f, 0.22f);

        var body = CreateJoint(root.transform, "Body", Vector3.zero);
        CreatePart(body, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.90f, 0.75f, 1.15f), base1);
        CreatePart(body, PrimitiveType.Sphere, new Vector3(0, -0.18f, 0.06f), new Vector3(0.65f, 0.48f, 0.80f), belly);
        // Earth patches
        CreatePart(body, PrimitiveType.Cube, new Vector3(0.35f, -0.05f, 0.10f), new Vector3(0.10f, 0.06f, 0.12f), earthBrown);
        CreatePart(body, PrimitiveType.Cube, new Vector3(-0.30f, -0.02f, -0.15f), new Vector3(0.08f, 0.05f, 0.10f), earthBrown);

        // Forest on back — 4 trees
        for (int i = 0; i < 4; i++)
        {
            string fn = $"Flower_{i + 1}";
            float tx = (i % 2 == 0 ? 0.08f : -0.06f) + (i > 1 ? -0.04f : 0.04f);
            var tree = CreateJoint(body, fn, new Vector3(tx, 0.44f, 0.20f - i * 0.14f));
            // Trunk
            CreatePart(tree, PrimitiveType.Cylinder, new Vector3(0, 0.06f, 0),
                new Vector3(0.04f, 0.12f - i * 0.01f, 0.04f), barkBrown);
            // Canopy
            CreatePart(tree, PrimitiveType.Sphere, new Vector3(0, 0.18f, 0),
                new Vector3(0.16f - i * 0.02f, 0.14f, 0.16f - i * 0.02f), leafDark);
            CreatePart(tree, PrimitiveType.Sphere, new Vector3(0.03f, 0.20f, 0.02f),
                new Vector3(0.10f, 0.08f, 0.10f), leafBright);
        }
        // 5th tree for Flower_5
        var tree5 = CreateJoint(body, "Flower_5", new Vector3(0.12f, 0.40f, -0.30f));
        CreatePart(tree5, PrimitiveType.Cylinder, new Vector3(0, 0.04f, 0), new Vector3(0.03f, 0.08f, 0.03f), barkDark);
        CreatePart(tree5, PrimitiveType.Sphere, new Vector3(0, 0.12f, 0), new Vector3(0.10f, 0.08f, 0.10f), leafDark);

        // Thick moss-covered neck
        var n1 = CreateJoint(body, "Neck_1", new Vector3(0, 0.20f, 0.54f));
        CreatePart(n1, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.26f, 0.20f, 0.26f), base1);
        // Thick moss
        CreatePart(n1, PrimitiveType.Sphere, new Vector3(0, 0.10f, 0), new Vector3(0.22f, 0.06f, 0.18f), mossColor);
        // Vines
        var v1 = CreatePart(n1, PrimitiveType.Cube, new Vector3(0.10f, 0, 0.05f),
            new Vector3(0.02f, 0.16f, 0.02f), vineColor);
        v1.transform.localRotation = Quaternion.Euler(10, 30, 8);

        var n2 = CreateJoint(n1, "Neck_2", new Vector3(0, 0.26f, 0.12f));
        CreatePart(n2, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.22f, 0.18f, 0.22f), base1);
        CreatePart(n2, PrimitiveType.Sphere, new Vector3(0, 0.09f, 0), new Vector3(0.18f, 0.05f, 0.14f), mossColor);

        var head = CreateJoint(n2, "Head", new Vector3(0, 0.20f, 0.10f));
        CreatePart(head, PrimitiveType.Sphere, Vector3.zero, new Vector3(0.30f, 0.28f, 0.34f), base1);
        CreatePart(head, PrimitiveType.Sphere, new Vector3(0, -0.02f, 0.14f), new Vector3(0.20f, 0.14f, 0.18f), base1);
        CreateEyes(head, 0.11f, 0.04f, 0.10f, 0.13f, 0.08f, 0.04f, new Color(0.50f, 0.38f, 0.18f), slitPupils: false);

        // Massive trunk-like legs
        string[] legNames = { "Leg_FR", "Leg_FL", "Leg_RR", "Leg_RL" };
        float[][] lp = {
            new float[] { 0.30f, -0.38f, 0.28f }, new float[] { -0.30f, -0.38f, 0.28f },
            new float[] { 0.30f, -0.38f, -0.28f }, new float[] { -0.30f, -0.38f, -0.28f }
        };
        for (int i = 0; i < 4; i++)
        {
            var leg = CreateJoint(body, legNames[i], new Vector3(lp[i][0], lp[i][1], lp[i][2]));
            CreatePart(leg, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.22f, 0.28f, 0.22f), barkBrown);
            CreatePart(leg, PrimitiveType.Cube, new Vector3(0, 0.08f, 0), new Vector3(0.23f, 0.015f, 0.23f), barkDark);
            CreatePart(leg, PrimitiveType.Cube, new Vector3(0, -0.06f, 0), new Vector3(0.22f, 0.012f, 0.22f), barkDark);
            CreatePart(leg, PrimitiveType.Sphere, new Vector3(0, -0.28f, 0.02f),
                new Vector3(0.20f, 0.06f, 0.22f), dark1);
            // Roots at feet
            CreatePart(leg, PrimitiveType.Cube, new Vector3(0.06f, -0.30f, 0.08f),
                new Vector3(0.02f, 0.02f, 0.07f), barkDark);
        }

        // Tail
        var tail1 = CreateJoint(body, "Tail_1", new Vector3(0, 0.04f, -0.60f), Quaternion.Euler(65, 0, 0));
        CreatePart(tail1, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.20f, 0.25f, 0.20f), base1);
        var tail2 = CreateJoint(tail1, "Tail_2", new Vector3(0, 0.35f, 0));
        CreatePart(tail2, PrimitiveType.Capsule, Vector3.zero, new Vector3(0.12f, 0.20f, 0.12f), base1);
        // Grass grows at tail tip
        CreatePart(tail2, PrimitiveType.Cube, new Vector3(0, 0.25f, 0), new Vector3(0.14f, 0.03f, 0.10f), leafBright);

        FinalizeModel(root, species, isEnemy);
        return root;
    }

    // ===============================================================
    // GENERIC ARTICULATED DINO — Type-based body archetype
    // Used for all species without a specific model (IDs 10-150)
    // ===============================================================

    private static GameObject CreateGenericDino(DinoSpeciesData species, bool isEnemy)
    {
        var root = new GameObject($"Dino_{species.name}");

        Color baseColor = GetTypeColor(species.types[0]);
        Color darkColor = baseColor * 0.68f; darkColor.a = 1f;
        Color lightColor = Color.Lerp(baseColor, Color.white, 0.30f);
        Color irisColor = GetTypeIrisColor(species.types[0]);

        DinoType primaryType = (DinoType)species.types[0];

        // Determine body archetype from primary type
        bool isBiped = primaryType == DinoType.Fire || primaryType == DinoType.Electric ||
                       primaryType == DinoType.Shadow || primaryType == DinoType.Light ||
                       primaryType == DinoType.Primal;
        bool isAquatic = primaryType == DinoType.Water || primaryType == DinoType.Ice;
        bool isFlying = primaryType == DinoType.Air;
        // Everything else: quadruped (Normal, Earth, Flora, Fossil, Venom, Metal)

        // Scale factors by evolution stage
        float bst = species.baseStats != null
            ? species.baseStats.hp + species.baseStats.atk + species.baseStats.def +
              species.baseStats.spatk + species.baseStats.spdef + species.baseStats.speed : 300;
        float bodyScale = bst < 300 ? 0.85f : (bst < 420 ? 1.0f : 1.15f);
        float headScale = bst < 300 ? 1.2f : (bst < 420 ? 1.0f : 0.85f); // babies have big heads

        // ==== BUILD ARTICULATED MODEL ====
        var body = CreateJoint(root.transform, "Body", Vector3.zero,
            isBiped ? Quaternion.Euler(12, 0, 0) : Quaternion.identity);

        // -- Torso --
        float bodyW = (isAquatic ? 0.65f : 0.60f) * bodyScale;
        float bodyH = (isAquatic ? 0.42f : 0.50f) * bodyScale;
        float bodyD = (isAquatic ? 0.85f : 0.72f) * bodyScale;
        CreatePart(body, PrimitiveType.Sphere, Vector3.zero, new Vector3(bodyW, bodyH, bodyD), baseColor);
        CreatePart(body, PrimitiveType.Sphere,
            new Vector3(0, -bodyH * 0.18f, 0.03f),
            new Vector3(bodyW * 0.72f, bodyH * 0.65f, bodyD * 0.72f), lightColor);

        // -- Neck --
        var neck = CreateJoint(body, "Neck", new Vector3(0, bodyH * 0.30f, bodyD * 0.42f));
        CreatePart(neck, PrimitiveType.Sphere, Vector3.zero,
            new Vector3(0.22f * bodyScale, 0.22f * bodyScale, 0.22f * bodyScale), baseColor);

        // -- Head --
        float hs = 0.36f * headScale;
        var head = CreateJoint(neck, "Head", new Vector3(0, 0.14f * bodyScale, 0.12f * bodyScale));
        CreatePart(head, PrimitiveType.Sphere, Vector3.zero, new Vector3(hs, hs * 0.9f, hs * 1.05f), baseColor);
        // Snout
        CreatePart(head, PrimitiveType.Sphere, new Vector3(0, -0.02f, hs * 0.42f),
            new Vector3(hs * 0.58f, hs * 0.42f, hs * 0.52f), baseColor);
        // Mouth
        CreatePart(head, PrimitiveType.Cube, new Vector3(0, -hs * 0.28f, hs * 0.42f),
            new Vector3(hs * 0.5f, 0.012f, hs * 0.3f), darkColor);
        // Nostrils
        CreatePart(head, PrimitiveType.Sphere, new Vector3(0.04f, 0, hs * 0.50f), Vector3.one * 0.02f, darkColor);
        CreatePart(head, PrimitiveType.Sphere, new Vector3(-0.04f, 0, hs * 0.50f), Vector3.one * 0.02f, darkColor);
        // Eyes
        bool slitEyes = isBiped || primaryType == DinoType.Venom;
        CreateEyes(head, hs * 0.35f, 0.04f, hs * 0.32f,
            0.12f * headScale, 0.08f * headScale, 0.04f * headScale, irisColor, slitPupils: slitEyes);

        // -- Limbs based on archetype --
        if (isAquatic)
        {
            // 4 flippers
            for (int side = -1; side <= 1; side += 2)
            {
                var ff = CreateJoint(body, side < 0 ? "Flipper_FL" : "Flipper_FR",
                    new Vector3(side * bodyW * 0.55f, -bodyH * 0.10f, bodyD * 0.15f),
                    Quaternion.Euler(0, side * 15, side * -15));
                CreatePart(ff, PrimitiveType.Sphere, Vector3.zero,
                    new Vector3(0.22f * bodyScale, 0.04f, 0.14f * bodyScale), darkColor);
            }
            for (int side = -1; side <= 1; side += 2)
            {
                var rf = CreateJoint(body, side < 0 ? "Flipper_RL" : "Flipper_RR",
                    new Vector3(side * bodyW * 0.45f, -bodyH * 0.18f, -bodyD * 0.30f),
                    Quaternion.Euler(0, side * 10, side * -10));
                CreatePart(rf, PrimitiveType.Sphere, Vector3.zero,
                    new Vector3(0.16f * bodyScale, 0.03f, 0.10f * bodyScale), darkColor);
            }
        }
        else if (isBiped)
        {
            // 2 arms + 2 legs
            for (int side = -1; side <= 1; side += 2)
            {
                var arm = CreateJoint(body, side < 0 ? "Arm_L" : "Arm_R",
                    new Vector3(side * bodyW * 0.48f, bodyH * 0.05f, bodyD * 0.20f),
                    Quaternion.Euler(10, 0, side * -22));
                CreatePart(arm, PrimitiveType.Capsule, Vector3.zero,
                    new Vector3(0.06f * bodyScale, 0.10f * bodyScale, 0.06f * bodyScale), darkColor);
                CreatePart(arm, PrimitiveType.Cube, new Vector3(0, -0.13f * bodyScale, 0.02f),
                    new Vector3(0.015f, 0.04f, 0.015f), darkColor * 0.7f);
            }
            for (int side = -1; side <= 1; side += 2)
            {
                var leg = CreateJoint(body, side < 0 ? "Leg_L" : "Leg_R",
                    new Vector3(side * bodyW * 0.32f, -bodyH * 0.50f, -bodyD * 0.05f));
                CreatePart(leg, PrimitiveType.Capsule, Vector3.zero,
                    new Vector3(0.14f * bodyScale, 0.20f * bodyScale, 0.14f * bodyScale), darkColor);
                CreatePart(leg, PrimitiveType.Capsule, new Vector3(0, -0.24f * bodyScale, 0.08f * bodyScale),
                    new Vector3(0.09f * bodyScale, 0.15f * bodyScale, 0.09f * bodyScale), darkColor);
                CreatePart(leg, PrimitiveType.Sphere, new Vector3(0, -0.38f * bodyScale, 0.12f * bodyScale),
                    new Vector3(0.13f * bodyScale, 0.04f, 0.16f * bodyScale), darkColor);
            }
        }
        else if (isFlying)
        {
            // Wings + 2 small legs
            for (int side = -1; side <= 1; side += 2)
            {
                var wing = CreateJoint(body, side < 0 ? "Arm_L" : "Arm_R",
                    new Vector3(side * bodyW * 0.50f, bodyH * 0.15f, 0),
                    Quaternion.Euler(0, 0, side * -12));
                CreatePart(wing, PrimitiveType.Cube, Vector3.zero,
                    new Vector3(0.35f * bodyScale, 0.03f, 0.22f * bodyScale), darkColor);
                CreatePart(wing, PrimitiveType.Cube, new Vector3(side * 0.10f, 0.005f, 0),
                    new Vector3(0.20f * bodyScale, 0.015f, 0.16f * bodyScale),
                    Color.Lerp(baseColor, Color.white, 0.15f));
            }
            for (int side = -1; side <= 1; side += 2)
            {
                var leg = CreateJoint(body, side < 0 ? "Leg_L" : "Leg_R",
                    new Vector3(side * bodyW * 0.28f, -bodyH * 0.45f, -bodyD * 0.10f));
                CreatePart(leg, PrimitiveType.Capsule, Vector3.zero,
                    new Vector3(0.08f * bodyScale, 0.14f * bodyScale, 0.08f * bodyScale), darkColor);
                CreatePart(leg, PrimitiveType.Sphere, new Vector3(0, -0.16f * bodyScale, 0.04f * bodyScale),
                    new Vector3(0.10f * bodyScale, 0.03f, 0.12f * bodyScale), darkColor);
            }
        }
        else
        {
            // Quadruped — 4 legs
            string[] legN = { "Leg_FR", "Leg_FL", "Leg_RR", "Leg_RL" };
            float[][] legP = {
                new float[] { bodyW * 0.40f, -bodyH * 0.55f, bodyD * 0.24f },
                new float[] { -bodyW * 0.40f, -bodyH * 0.55f, bodyD * 0.24f },
                new float[] { bodyW * 0.40f, -bodyH * 0.55f, -bodyD * 0.28f },
                new float[] { -bodyW * 0.40f, -bodyH * 0.55f, -bodyD * 0.28f },
            };
            for (int i = 0; i < 4; i++)
            {
                var leg = CreateJoint(body, legN[i], new Vector3(legP[i][0], legP[i][1], legP[i][2]));
                CreatePart(leg, PrimitiveType.Capsule, Vector3.zero,
                    new Vector3(0.14f * bodyScale, 0.20f * bodyScale, 0.14f * bodyScale), darkColor);
                CreatePart(leg, PrimitiveType.Sphere, new Vector3(0, -0.20f * bodyScale, 0.02f),
                    new Vector3(0.13f * bodyScale, 0.04f, 0.15f * bodyScale), darkColor);
            }
        }

        // -- Tail (chained, 2 segments) --
        float tailZ = isAquatic ? -bodyD * 0.52f : -bodyD * 0.48f;
        var tail1 = CreateJoint(body, "Tail_1", new Vector3(0, 0.02f, tailZ),
            Quaternion.Euler(isAquatic ? 80 : 68, 0, 0));
        CreatePart(tail1, PrimitiveType.Capsule, Vector3.zero,
            new Vector3(0.14f * bodyScale, 0.20f * bodyScale, 0.14f * bodyScale), baseColor);

        var tail2 = CreateJoint(tail1, "Tail_2", new Vector3(0, 0.26f * bodyScale, 0));
        CreatePart(tail2, PrimitiveType.Capsule, Vector3.zero,
            new Vector3(0.08f * bodyScale, 0.16f * bodyScale, 0.08f * bodyScale), baseColor);

        // Tail paddle for aquatic
        if (isAquatic)
        {
            var tp = CreateJoint(tail2, "TailPaddle", new Vector3(0, 0.22f * bodyScale, 0));
            CreatePart(tp, PrimitiveType.Sphere, Vector3.zero,
                new Vector3(0.18f * bodyScale, 0.03f, 0.12f * bodyScale), darkColor);
        }

        // -- Type-specific decorations --
        AddTypeFeatures(body, species.types[0], baseColor);

        // -- Dual-type accent stripes --
        if (species.types.Length > 1 && species.types[1] != species.types[0])
        {
            Color accentColor = GetTypeColor(species.types[1]);
            CreatePart(body, PrimitiveType.Cube,
                new Vector3(0, bodyH * 0.22f, bodyD * 0.25f),
                new Vector3(bodyW * 1.02f, 0.02f, 0.06f), accentColor);
            CreatePart(body, PrimitiveType.Cube,
                new Vector3(0, bodyH * 0.08f, -bodyD * 0.05f),
                new Vector3(bodyW * 1.02f, 0.02f, 0.06f), accentColor);
        }

        FinalizeModel(root, species, isEnemy);
        return root;
    }

    /// <summary>
    /// Create a small flower from petals arranged in a ring + a center sphere.
    /// </summary>
    private static void CreateFlower(Transform parent, Vector3 position,
        float petalSize, int petalCount, Color petalColor, Color centerColor)
    {
        // Petals arranged in a ring
        for (int i = 0; i < petalCount; i++)
        {
            float angle = i * (360f / petalCount);
            float rad = angle * Mathf.Deg2Rad;
            float px = Mathf.Sin(rad) * petalSize * 0.8f;
            float pz = Mathf.Cos(rad) * petalSize * 0.8f;

            var petal = CreatePart(parent, PrimitiveType.Cube,
                position + new Vector3(px, 0.02f, pz),
                new Vector3(petalSize, 0.015f, petalSize), petalColor);
            petal.transform.localRotation = Quaternion.Euler(0, angle, 0);
        }

        // Center
        CreatePart(parent, PrimitiveType.Sphere,
            position + new Vector3(0, 0.03f, 0),
            new Vector3(petalSize * 0.7f, petalSize * 0.5f, petalSize * 0.7f), centerColor);
    }
}
