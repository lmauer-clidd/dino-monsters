// ============================================================
// Dino Monsters -- Procedural Character Model Generator
// ============================================================
// Creates detailed, articulated humanoid character models from
// Unity primitives. Supports multiple archetypes:
// Player, Rival, Villager, Trainer, GymLeader, Scientist, etc.
// All models use named joints for procedural animation.
// ============================================================

using UnityEngine;

public static class CharacterModelGenerator
{
    // ===============================================================
    // Character Archetypes
    // ===============================================================

    public enum CharacterType
    {
        Player,
        Rival,
        Villager,
        VillagerFemale,
        Trainer,
        TrainerFemale,
        GymLeader,
        Scientist,
        Nurse,
        Shopkeeper,
        OldMan,
        Child,
        MeteoreGrunt,
        MeteoreLeader,
    }

    // ===============================================================
    // Color Profiles per Archetype
    // ===============================================================

    private struct CharacterColors
    {
        public Color skin, hair, shirt, pants, shoes, accent, hat;
        public bool hasHat, hasBackpack, hasLabCoat, hasUniform;
    }

    private static CharacterColors GetColors(CharacterType type, Color? customColor = null)
    {
        var c = new CharacterColors();
        c.skin = new Color(0.93f, 0.80f, 0.67f);

        switch (type)
        {
            case CharacterType.Player:
                c.hair = new Color(0.25f, 0.15f, 0.08f);
                c.shirt = new Color(0.20f, 0.45f, 0.85f);
                c.pants = new Color(0.22f, 0.22f, 0.35f);
                c.shoes = new Color(0.30f, 0.18f, 0.12f);
                c.hat = new Color(0.85f, 0.18f, 0.18f);
                c.accent = new Color(0.65f, 0.35f, 0.15f);
                c.hasHat = true;
                c.hasBackpack = true;
                break;

            case CharacterType.Rival:
                c.hair = new Color(0.15f, 0.12f, 0.30f); // dark purple
                c.shirt = new Color(0.55f, 0.20f, 0.55f); // purple
                c.pants = new Color(0.20f, 0.20f, 0.25f);
                c.shoes = new Color(0.25f, 0.15f, 0.15f);
                c.hat = new Color(0.40f, 0.15f, 0.45f);
                c.accent = new Color(0.85f, 0.75f, 0.20f); // gold pendant
                c.hasHat = false;
                break;

            case CharacterType.Villager:
                c.hair = new Color(0.35f, 0.22f, 0.12f);
                c.shirt = customColor ?? new Color(0.60f, 0.40f, 0.30f);
                c.pants = new Color(0.38f, 0.32f, 0.22f);
                c.shoes = new Color(0.32f, 0.22f, 0.14f);
                c.accent = c.shirt * 0.7f; c.accent.a = 1f;
                break;

            case CharacterType.VillagerFemale:
                c.hair = new Color(0.45f, 0.25f, 0.12f);
                c.shirt = customColor ?? new Color(0.85f, 0.45f, 0.50f);
                c.pants = new Color(0.42f, 0.35f, 0.30f);
                c.shoes = new Color(0.50f, 0.30f, 0.25f);
                c.accent = new Color(0.90f, 0.70f, 0.30f);
                break;

            case CharacterType.Trainer:
                c.hair = new Color(0.20f, 0.18f, 0.15f);
                c.shirt = customColor ?? new Color(0.30f, 0.55f, 0.30f);
                c.pants = new Color(0.28f, 0.25f, 0.20f);
                c.shoes = new Color(0.35f, 0.20f, 0.12f);
                c.hat = new Color(0.30f, 0.50f, 0.28f);
                c.accent = new Color(0.85f, 0.35f, 0.20f);
                c.hasHat = true;
                break;

            case CharacterType.TrainerFemale:
                c.hair = new Color(0.55f, 0.30f, 0.15f);
                c.shirt = customColor ?? new Color(0.45f, 0.65f, 0.85f);
                c.pants = new Color(0.30f, 0.28f, 0.38f);
                c.shoes = new Color(0.60f, 0.35f, 0.30f);
                c.accent = new Color(0.90f, 0.55f, 0.35f);
                break;

            case CharacterType.GymLeader:
                c.hair = new Color(0.15f, 0.12f, 0.10f);
                c.shirt = customColor ?? new Color(0.75f, 0.55f, 0.30f);
                c.pants = new Color(0.20f, 0.18f, 0.15f);
                c.shoes = new Color(0.15f, 0.12f, 0.10f);
                c.accent = new Color(0.94f, 0.78f, 0.30f); // gold trim
                c.hasUniform = true;
                break;

            case CharacterType.Scientist:
                c.hair = new Color(0.50f, 0.48f, 0.45f); // grey
                c.shirt = new Color(0.88f, 0.88f, 0.85f); // white coat
                c.pants = new Color(0.35f, 0.32f, 0.30f);
                c.shoes = new Color(0.25f, 0.20f, 0.18f);
                c.accent = new Color(0.40f, 0.60f, 0.80f);
                c.hasLabCoat = true;
                break;

            case CharacterType.Nurse:
                c.hair = new Color(0.60f, 0.30f, 0.35f); // pink-ish
                c.shirt = new Color(0.95f, 0.70f, 0.75f); // pink uniform
                c.pants = new Color(0.90f, 0.85f, 0.86f);
                c.shoes = new Color(0.85f, 0.80f, 0.80f);
                c.accent = new Color(0.90f, 0.25f, 0.30f); // red cross
                break;

            case CharacterType.Shopkeeper:
                c.hair = new Color(0.30f, 0.25f, 0.15f);
                c.shirt = new Color(0.30f, 0.55f, 0.25f); // green apron
                c.pants = new Color(0.35f, 0.30f, 0.22f);
                c.shoes = new Color(0.30f, 0.22f, 0.14f);
                c.accent = new Color(0.45f, 0.70f, 0.30f);
                break;

            case CharacterType.OldMan:
                c.skin = new Color(0.88f, 0.75f, 0.62f);
                c.hair = new Color(0.72f, 0.70f, 0.68f); // white/grey
                c.shirt = new Color(0.55f, 0.48f, 0.38f);
                c.pants = new Color(0.40f, 0.35f, 0.28f);
                c.shoes = new Color(0.30f, 0.22f, 0.15f);
                c.accent = c.shirt;
                break;

            case CharacterType.Child:
                c.skin = new Color(0.95f, 0.82f, 0.70f);
                c.hair = new Color(0.55f, 0.35f, 0.18f);
                c.shirt = customColor ?? new Color(0.90f, 0.55f, 0.20f);
                c.pants = new Color(0.30f, 0.40f, 0.55f);
                c.shoes = new Color(0.85f, 0.30f, 0.25f);
                c.accent = c.shirt;
                break;

            case CharacterType.MeteoreGrunt:
                c.hair = new Color(0.18f, 0.15f, 0.22f);
                c.shirt = new Color(0.22f, 0.20f, 0.30f); // dark uniform
                c.pants = new Color(0.18f, 0.16f, 0.22f);
                c.shoes = new Color(0.12f, 0.10f, 0.14f);
                c.accent = new Color(0.70f, 0.20f, 0.20f); // red meteor emblem
                c.hasUniform = true;
                break;

            case CharacterType.MeteoreLeader:
                c.hair = new Color(0.10f, 0.08f, 0.15f);
                c.shirt = new Color(0.30f, 0.12f, 0.35f); // dark purple
                c.pants = new Color(0.15f, 0.10f, 0.18f);
                c.shoes = new Color(0.10f, 0.08f, 0.12f);
                c.accent = new Color(0.90f, 0.25f, 0.15f); // red
                c.hasUniform = true;
                break;

            default:
                c.hair = new Color(0.30f, 0.22f, 0.14f);
                c.shirt = customColor ?? new Color(0.50f, 0.45f, 0.40f);
                c.pants = new Color(0.35f, 0.30f, 0.25f);
                c.shoes = new Color(0.30f, 0.20f, 0.14f);
                c.accent = c.shirt;
                break;
        }

        return c;
    }

    // ===============================================================
    // Main Entry Point
    // ===============================================================

    /// <summary>
    /// Create a detailed, articulated character model.
    /// Returns the root Transform containing named joints for animation.
    /// </summary>
    public static Transform CreateCharacter(Transform parent, CharacterType type, Color? customColor = null)
    {
        var colors = GetColors(type, customColor);
        float heightScale = (type == CharacterType.Child) ? 0.7f : 1.0f;
        float bodyWidth = (type == CharacterType.OldMan) ? 1.1f : 1.0f;

        var root = new GameObject("CharModel").transform;
        root.SetParent(parent);
        root.localPosition = Vector3.zero;

        // ---- LEGS ----
        for (int side = -1; side <= 1; side += 2)
        {
            string legName = side < 0 ? "LegL" : "LegR";
            float lx = side * 0.09f;

            var legJoint = CreateJoint(root, legName, new Vector3(lx, 0.24f * heightScale, 0));
            // Hip
            Part(legJoint, PrimitiveType.Sphere, new Vector3(0, 0.02f, 0),
                new Vector3(0.08f, 0.08f, 0.08f), colors.pants);
            // Upper leg (thigh)
            Part(legJoint, PrimitiveType.Capsule, Vector3.zero,
                new Vector3(0.11f, 0.13f * heightScale, 0.11f), colors.pants);
            // Knee
            Part(legJoint, PrimitiveType.Sphere, new Vector3(0, -0.12f * heightScale, 0.03f),
                new Vector3(0.07f, 0.06f, 0.06f), colors.pants * 0.88f);
            // Lower leg (shin)
            Part(legJoint, PrimitiveType.Capsule, new Vector3(0, -0.18f * heightScale, 0),
                new Vector3(0.09f, 0.12f * heightScale, 0.09f), colors.pants);
            // Ankle
            Part(legJoint, PrimitiveType.Sphere, new Vector3(0, -0.28f * heightScale, 0.01f),
                new Vector3(0.06f, 0.04f, 0.06f), colors.shoes);
            // Shoe
            Part(legJoint, PrimitiveType.Cube, new Vector3(0, -0.30f * heightScale, 0.03f),
                new Vector3(0.11f, 0.06f, 0.16f), colors.shoes);
            // Sole
            Part(legJoint, PrimitiveType.Cube, new Vector3(0, -0.33f * heightScale, 0.03f),
                new Vector3(0.12f, 0.02f, 0.17f), colors.shoes * 0.55f);
            // Shoe tongue/lace detail
            Part(legJoint, PrimitiveType.Cube, new Vector3(0, -0.28f * heightScale, 0.08f),
                new Vector3(0.05f, 0.04f, 0.02f), colors.shoes * 1.2f);
        }

        // ---- TORSO ----
        var bodyJoint = CreateJoint(root, "Body", new Vector3(0, 0.55f * heightScale, 0));
        // Main torso
        Part(bodyJoint, PrimitiveType.Cube, Vector3.zero,
            new Vector3(0.36f * bodyWidth, 0.30f * heightScale, 0.20f), colors.shirt);
        // Chest volume (rounder feel)
        Part(bodyJoint, PrimitiveType.Sphere, new Vector3(0, 0.04f, 0.02f),
            new Vector3(0.32f * bodyWidth, 0.22f * heightScale, 0.18f), colors.shirt);
        // Shoulders
        Part(bodyJoint, PrimitiveType.Sphere, new Vector3(-0.18f * bodyWidth, 0.12f, 0),
            new Vector3(0.10f, 0.08f, 0.10f), colors.shirt);
        Part(bodyJoint, PrimitiveType.Sphere, new Vector3(0.18f * bodyWidth, 0.12f, 0),
            new Vector3(0.10f, 0.08f, 0.10f), colors.shirt);
        // Collar
        Part(bodyJoint, PrimitiveType.Cube, new Vector3(0, 0.15f * heightScale, 0.02f),
            new Vector3(0.22f, 0.04f, 0.08f), colors.shirt * 0.85f);
        // Neck
        Part(bodyJoint, PrimitiveType.Capsule, new Vector3(0, 0.18f * heightScale, 0),
            new Vector3(0.08f, 0.06f, 0.08f), colors.skin);
        // Belt
        Part(bodyJoint, PrimitiveType.Cube, new Vector3(0, -0.14f * heightScale, 0),
            new Vector3(0.37f * bodyWidth, 0.04f, 0.21f), colors.pants * 1.1f);
        // Belt buckle
        Part(bodyJoint, PrimitiveType.Cube, new Vector3(0, -0.14f * heightScale, 0.10f),
            new Vector3(0.04f, 0.03f, 0.02f), new Color(0.75f, 0.70f, 0.45f));

        // Shirt details (buttons or stripes)
        if (colors.hasUniform)
        {
            // Uniform stripe
            Part(bodyJoint, PrimitiveType.Cube, new Vector3(0, 0.04f, 0.10f),
                new Vector3(0.30f, 0.02f, 0.01f), colors.accent);
            Part(bodyJoint, PrimitiveType.Cube, new Vector3(0, -0.04f, 0.10f),
                new Vector3(0.30f, 0.02f, 0.01f), colors.accent);
            // Emblem on chest
            Part(bodyJoint, PrimitiveType.Sphere, new Vector3(0.10f, 0.06f, 0.10f),
                new Vector3(0.05f, 0.05f, 0.02f), colors.accent);
        }
        else
        {
            // Buttons
            for (int i = 0; i < 3; i++)
                Part(bodyJoint, PrimitiveType.Sphere, new Vector3(0, 0.08f - i * 0.08f, 0.10f),
                    Vector3.one * 0.02f, colors.shirt * 0.7f);
        }

        // Lab coat (scientist)
        if (colors.hasLabCoat)
        {
            Part(bodyJoint, PrimitiveType.Cube, new Vector3(0, -0.08f, 0),
                new Vector3(0.38f, 0.20f, 0.22f), colors.shirt);
            // Pockets
            Part(bodyJoint, PrimitiveType.Cube, new Vector3(-0.12f, -0.10f, 0.10f),
                new Vector3(0.08f, 0.06f, 0.01f), colors.shirt * 0.92f);
            Part(bodyJoint, PrimitiveType.Cube, new Vector3(0.12f, -0.10f, 0.10f),
                new Vector3(0.08f, 0.06f, 0.01f), colors.shirt * 0.92f);
            // Pen in pocket
            Part(bodyJoint, PrimitiveType.Cube, new Vector3(0.12f, -0.06f, 0.11f),
                new Vector3(0.01f, 0.05f, 0.01f), colors.accent);
        }

        // Backpack (player only)
        if (colors.hasBackpack)
        {
            var bp = Part(bodyJoint, PrimitiveType.Cube, new Vector3(0, 0.02f, -0.16f),
                new Vector3(0.28f, 0.24f, 0.12f), colors.accent);
            // Straps
            Part(bp.transform, PrimitiveType.Cube, new Vector3(-0.10f, 0.10f, 0.06f),
                new Vector3(0.03f, 0.18f, 0.02f), colors.accent * 0.7f);
            Part(bp.transform, PrimitiveType.Cube, new Vector3(0.10f, 0.10f, 0.06f),
                new Vector3(0.03f, 0.18f, 0.02f), colors.accent * 0.7f);
            // Pokeball decor
            Part(bp.transform, PrimitiveType.Sphere, new Vector3(0, 0, -0.06f),
                new Vector3(0.06f, 0.06f, 0.02f), Color.white);
            Part(bp.transform, PrimitiveType.Sphere, new Vector3(0, -0.01f, -0.065f),
                new Vector3(0.06f, 0.03f, 0.015f), new Color(0.85f, 0.20f, 0.20f));
            // Zipper
            Part(bp.transform, PrimitiveType.Cube, new Vector3(0, 0.06f, -0.06f),
                new Vector3(0.16f, 0.012f, 0.01f), new Color(0.65f, 0.60f, 0.40f));
        }

        // ---- ARMS ----
        for (int side = -1; side <= 1; side += 2)
        {
            string armName = side < 0 ? "ArmL" : "ArmR";
            float ax = side * 0.24f * bodyWidth;

            var armJoint = CreateJoint(root, armName, new Vector3(ax, 0.62f * heightScale, 0));
            // Shoulder sphere
            Part(armJoint, PrimitiveType.Sphere, new Vector3(0, 0, 0),
                new Vector3(0.10f, 0.10f, 0.10f), colors.shirt);
            // Upper arm (shirt color)
            Part(armJoint, PrimitiveType.Capsule, new Vector3(0, -0.08f, 0),
                new Vector3(0.09f, 0.12f * heightScale, 0.09f), colors.shirt);
            // Elbow
            Part(armJoint, PrimitiveType.Sphere, new Vector3(0, -0.16f * heightScale, 0),
                new Vector3(0.06f, 0.06f, 0.06f), colors.skin);
            // Forearm (skin)
            Part(armJoint, PrimitiveType.Capsule, new Vector3(0, -0.22f * heightScale, 0),
                new Vector3(0.07f, 0.10f * heightScale, 0.07f), colors.skin);
            // Wrist
            Part(armJoint, PrimitiveType.Sphere, new Vector3(0, -0.30f * heightScale, 0),
                new Vector3(0.05f, 0.04f, 0.05f), colors.skin);
            // Hand
            Part(armJoint, PrimitiveType.Sphere, new Vector3(0, -0.34f * heightScale, 0.01f),
                new Vector3(0.07f, 0.05f, 0.06f), colors.skin);
            // Thumb
            Part(armJoint, PrimitiveType.Sphere, new Vector3(side * 0.03f, -0.34f * heightScale, 0.03f),
                new Vector3(0.025f, 0.025f, 0.02f), colors.skin);
            // Fingers
            Part(armJoint, PrimitiveType.Sphere, new Vector3(0, -0.37f * heightScale, 0.02f),
                new Vector3(0.04f, 0.02f, 0.03f), colors.skin);
        }

        // ---- HEAD ----
        var headJoint = CreateJoint(root, "Head", new Vector3(0, 0.85f * heightScale, 0));
        // Head sphere — large and visible
        Part(headJoint, PrimitiveType.Sphere, Vector3.zero,
            new Vector3(0.34f, 0.32f, 0.30f), colors.skin);
        // Cheeks (add volume so head is clearly visible from above/behind)
        Part(headJoint, PrimitiveType.Sphere, new Vector3(-0.08f, -0.04f, 0.06f),
            new Vector3(0.10f, 0.08f, 0.08f), colors.skin);
        Part(headJoint, PrimitiveType.Sphere, new Vector3(0.08f, -0.04f, 0.06f),
            new Vector3(0.10f, 0.08f, 0.08f), colors.skin);
        // Ears
        Part(headJoint, PrimitiveType.Sphere, new Vector3(-0.16f, 0, 0),
            new Vector3(0.05f, 0.07f, 0.05f), colors.skin * 0.95f);
        Part(headJoint, PrimitiveType.Sphere, new Vector3(0.16f, 0, 0),
            new Vector3(0.05f, 0.07f, 0.05f), colors.skin * 0.95f);
        // Nose
        Part(headJoint, PrimitiveType.Sphere, new Vector3(0, -0.02f, 0.15f),
            new Vector3(0.05f, 0.04f, 0.04f), colors.skin * 0.92f);

        // Hair — style varies by type
        BuildHair(headJoint, type, colors);

        // Eyes
        for (int side = -1; side <= 1; side += 2)
        {
            float ex = side * 0.09f;
            // Sclera
            Part(headJoint, PrimitiveType.Sphere, new Vector3(ex, 0.02f, 0.14f),
                new Vector3(0.08f, 0.07f, 0.04f), Color.white);
            // Iris
            Color irisCol = (type == CharacterType.Player) ? new Color(0.30f, 0.50f, 0.80f)
                : (type == CharacterType.Rival) ? new Color(0.50f, 0.20f, 0.55f)
                : (type == CharacterType.MeteoreGrunt || type == CharacterType.MeteoreLeader)
                    ? new Color(0.60f, 0.15f, 0.15f)
                : new Color(0.35f, 0.25f, 0.15f);
            Part(headJoint, PrimitiveType.Sphere, new Vector3(ex, 0.02f, 0.16f),
                new Vector3(0.05f, 0.05f, 0.025f), irisCol);
            // Pupil
            Part(headJoint, PrimitiveType.Sphere, new Vector3(ex, 0.02f, 0.17f),
                new Vector3(0.025f, 0.025f, 0.015f), Color.black);
            // Shine
            Part(headJoint, PrimitiveType.Sphere, new Vector3(ex - side * 0.012f, 0.04f, 0.175f),
                Vector3.one * 0.015f, Color.white);
        }
        // Eyebrows
        for (int side = -1; side <= 1; side += 2)
        {
            Part(headJoint, PrimitiveType.Cube, new Vector3(side * 0.09f, 0.07f, 0.13f),
                new Vector3(0.07f, 0.018f, 0.02f), colors.hair * 0.8f);
        }
        // Mouth
        Part(headJoint, PrimitiveType.Cube, new Vector3(0, -0.07f, 0.14f),
            new Vector3(0.07f, 0.014f, 0.01f), new Color(0.75f, 0.45f, 0.40f));

        // ---- HAT ----
        if (colors.hasHat)
        {
            // Brim
            Part(headJoint, PrimitiveType.Cylinder, new Vector3(0, 0.16f, 0.02f),
                new Vector3(0.36f, 0.025f, 0.36f), colors.hat);
            // Crown
            Part(headJoint, PrimitiveType.Cylinder, new Vector3(0, 0.20f, -0.01f),
                new Vector3(0.28f, 0.06f, 0.28f), colors.hat);
            // Emblem
            Part(headJoint, PrimitiveType.Sphere, new Vector3(0, 0.20f, 0.13f),
                new Vector3(0.06f, 0.06f, 0.02f), Color.white);
            // Band
            Part(headJoint, PrimitiveType.Cube, new Vector3(0, 0.165f, 0),
                new Vector3(0.29f, 0.02f, 0.29f), colors.hat * 0.7f);
        }

        // Nurse cross (on hat area)
        if (type == CharacterType.Nurse)
        {
            // Nurse cap
            Part(headJoint, PrimitiveType.Cube, new Vector3(0, 0.16f, 0.06f),
                new Vector3(0.14f, 0.08f, 0.02f), Color.white);
            // Red cross
            Part(headJoint, PrimitiveType.Cube, new Vector3(0, 0.165f, 0.07f),
                new Vector3(0.06f, 0.015f, 0.01f), colors.accent);
            Part(headJoint, PrimitiveType.Cube, new Vector3(0, 0.165f, 0.07f),
                new Vector3(0.015f, 0.06f, 0.01f), colors.accent);
        }

        // ---- SHADOW ----
        var shadow = Part(root, PrimitiveType.Cylinder, new Vector3(0, 0.01f, 0),
            new Vector3(0.4f, 0.001f, 0.4f), Color.black);
        shadow.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetTransparent(Color.black, 0.25f);

        return root;
    }

    // ===============================================================
    // Hair Styles
    // ===============================================================

    private static void BuildHair(Transform head, CharacterType type, CharacterColors colors)
    {
        switch (type)
        {
            case CharacterType.Player:
                // Spiky messy hair under hat
                Part(head, PrimitiveType.Sphere, new Vector3(0, 0.06f, -0.06f),
                    new Vector3(0.28f, 0.18f, 0.16f), colors.hair);
                // Side tufts sticking out from hat
                Part(head, PrimitiveType.Sphere, new Vector3(-0.14f, 0.08f, 0.02f),
                    new Vector3(0.06f, 0.04f, 0.06f), colors.hair);
                Part(head, PrimitiveType.Sphere, new Vector3(0.14f, 0.08f, 0.02f),
                    new Vector3(0.06f, 0.04f, 0.06f), colors.hair);
                break;

            case CharacterType.Rival:
                // Spiky upward hair
                Part(head, PrimitiveType.Sphere, new Vector3(0, 0.10f, -0.02f),
                    new Vector3(0.26f, 0.16f, 0.22f), colors.hair);
                // Spikes
                for (int i = 0; i < 4; i++)
                {
                    float angle = -30 + i * 20;
                    var spike = Part(head, PrimitiveType.Cube,
                        new Vector3((i - 1.5f) * 0.06f, 0.18f, -0.04f + i * 0.02f),
                        new Vector3(0.04f, 0.10f, 0.04f), colors.hair);
                    spike.transform.localRotation = Quaternion.Euler(angle * 0.3f, 0, angle);
                }
                break;

            case CharacterType.VillagerFemale:
            case CharacterType.TrainerFemale:
            case CharacterType.Nurse:
                // Long hair
                Part(head, PrimitiveType.Sphere, new Vector3(0, 0.06f, -0.04f),
                    new Vector3(0.30f, 0.22f, 0.20f), colors.hair);
                // Hair flowing down back
                Part(head, PrimitiveType.Capsule, new Vector3(0, -0.06f, -0.10f),
                    new Vector3(0.22f, 0.16f, 0.12f), colors.hair);
                // Bangs
                Part(head, PrimitiveType.Sphere, new Vector3(0, 0.08f, 0.08f),
                    new Vector3(0.24f, 0.08f, 0.06f), colors.hair);
                break;

            case CharacterType.OldMan:
            case CharacterType.Scientist:
                // Balding with side hair
                Part(head, PrimitiveType.Sphere, new Vector3(-0.10f, 0.02f, -0.04f),
                    new Vector3(0.10f, 0.12f, 0.12f), colors.hair);
                Part(head, PrimitiveType.Sphere, new Vector3(0.10f, 0.02f, -0.04f),
                    new Vector3(0.10f, 0.12f, 0.12f), colors.hair);
                break;

            case CharacterType.Child:
                // Round puffy hair
                Part(head, PrimitiveType.Sphere, new Vector3(0, 0.08f, -0.02f),
                    new Vector3(0.30f, 0.18f, 0.24f), colors.hair);
                break;

            case CharacterType.MeteoreGrunt:
            case CharacterType.MeteoreLeader:
                // Slicked back
                Part(head, PrimitiveType.Sphere, new Vector3(0, 0.06f, -0.06f),
                    new Vector3(0.28f, 0.16f, 0.18f), colors.hair);
                // Helmet/visor
                Part(head, PrimitiveType.Cube, new Vector3(0, 0.10f, 0.06f),
                    new Vector3(0.26f, 0.04f, 0.08f), new Color(0.20f, 0.18f, 0.25f));
                break;

            default:
                // Generic short hair
                Part(head, PrimitiveType.Sphere, new Vector3(0, 0.06f, -0.04f),
                    new Vector3(0.28f, 0.18f, 0.18f), colors.hair);
                break;
        }
    }

    // ===============================================================
    // Simplified NPC Model (for overworld — less detail)
    // ===============================================================

    /// <summary>
    /// Create a simpler but still articulated NPC model (fewer primitives).
    /// Use for overworld NPCs where detail is less important.
    /// </summary>
    public static Transform CreateSimpleNPC(Transform parent, Color bodyColor, bool isTrainer)
    {
        // Determine archetype from color heuristic
        CharacterType type = CharacterType.Villager;
        if (isTrainer) type = CharacterType.Trainer;

        return CreateCharacter(parent, type, bodyColor);
    }

    // ===============================================================
    // Helpers
    // ===============================================================

    private static Transform CreateJoint(Transform parent, string name, Vector3 localPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.identity;
        return go.transform;
    }

    private static GameObject Part(Transform parent, PrimitiveType type,
        Vector3 pos, Vector3 scale, Color color)
    {
        var go = GameObject.CreatePrimitive(type);
        go.transform.SetParent(parent);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        go.transform.localRotation = Quaternion.identity;

        var r = go.GetComponent<Renderer>();
        r.sharedMaterial = MaterialManager.GetSolidColor(color);

        var col = go.GetComponent<Collider>();
        if (col != null) Object.Destroy(col);

        return go;
    }
}
