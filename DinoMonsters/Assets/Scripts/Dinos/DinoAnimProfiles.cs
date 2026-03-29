// ============================================================
// Dino Monsters -- Animation Profiles for Procedural Dinos
// ============================================================
// Defines per-species idle animation parameters as arrays of
// IdleChannel structs. Each channel drives a sine-wave
// oscillation on a named joint transform.
// ============================================================

using UnityEngine;

public static class DinoAnimProfiles
{
    // ===============================================================
    // Idle Channel Definition
    // ===============================================================

    public struct IdleChannel
    {
        public string partName;       // joint transform name
        public Vector3 rotAmplitude;  // rotation amplitude in degrees per axis
        public float frequency;       // oscillation frequency in Hz
        public float phaseOffset;     // phase offset in radians

        // Optional: scale oscillation (for breathing, flame flicker)
        public Vector3 scaleAmplitude; // scale delta per axis (0 = no scale anim)
        public float scaleFrequency;   // scale oscillation frequency

        // Optional: position oscillation
        public Vector3 posAmplitude;   // position delta per axis
        public float posFrequency;

        public IdleChannel(string part, Vector3 rotAmp, float freq, float phase = 0f)
        {
            partName = part;
            rotAmplitude = rotAmp;
            frequency = freq;
            phaseOffset = phase;
            scaleAmplitude = Vector3.zero;
            scaleFrequency = 0f;
            posAmplitude = Vector3.zero;
            posFrequency = 0f;
        }

        public IdleChannel WithScale(Vector3 amp, float freq)
        {
            scaleAmplitude = amp;
            scaleFrequency = freq;
            return this;
        }

        public IdleChannel WithPosition(Vector3 amp, float freq)
        {
            posAmplitude = amp;
            posFrequency = freq;
            return this;
        }
    }

    // ===============================================================
    // Profile Lookup
    // ===============================================================

    public static IdleChannel[] GetIdleProfile(int speciesId)
    {
        switch (speciesId)
        {
            case 1: return PyrexIdle();
            case 2: return PyrexIdle(); // Pyrovore — same archetype as Pyrex
            case 3: return PyrexIdle(); // Volcanorex — same archetype
            case 4: return AquadonIdle();
            case 5: return AquadonIdle(); // Marexis — aquatic archetype
            case 6: return AquadonIdle(); // Abyssaure — aquatic archetype
            case 7: return FlorasaurIdle();
            case 8: return FlorasaurIdle(); // Sylvacolle — same archetype
            case 9: return FlorasaurIdle(); // Titanarbore — same archetype
            default: return GenericIdle();
        }
    }

    // ===============================================================
    // PYREX — Fire Raptor
    // ===============================================================

    private static IdleChannel[] PyrexIdle()
    {
        return new IdleChannel[]
        {
            // Breathing — body scale pulse
            new IdleChannel("Body", new Vector3(1f, 0, 0), 1.5f)
                .WithScale(new Vector3(0.01f, 0.025f, 0.01f), 1.5f),

            // Head bob — subtle nod
            new IdleChannel("Head", new Vector3(5f, 0, 2f), 1.2f, 0.3f),

            // Jaw — very subtle open/close (breathing)
            new IdleChannel("Jaw", new Vector3(1.5f, 0, 0), 1.5f, 1.0f),

            // Crest — flame flicker (fast, chaotic feel)
            new IdleChannel("Crest", new Vector3(0, 0, 8f), 3.5f, 0.7f)
                .WithScale(new Vector3(0.08f, 0.12f, 0.08f), 4.0f),

            // Tail sway — cascading Y-axis wave
            new IdleChannel("Tail_1", new Vector3(0, 12f, 0), 0.8f, 0f),
            new IdleChannel("Tail_2", new Vector3(0, 15f, 0), 0.8f, 0.8f),
            new IdleChannel("Tail_3", new Vector3(0, 18f, 0), 0.8f, 1.6f),

            // Flame — scale pulse (independent of crest)
            new IdleChannel("Flame", new Vector3(0, 0, 0), 2.5f)
                .WithScale(new Vector3(0.15f, 0.15f, 0.15f), 3.0f),

            // Arms — gentle sway
            new IdleChannel("Arm_L", new Vector3(3f, 0, 2f), 0.6f, 0.5f),
            new IdleChannel("Arm_R", new Vector3(3f, 0, 2f), 0.6f, 2.0f),

            // Legs — very subtle shift
            new IdleChannel("Leg_L", new Vector3(2f, 0, 0), 0.4f, 0f),
            new IdleChannel("Leg_R", new Vector3(2f, 0, 0), 0.4f, Mathf.PI),
        };
    }

    // ===============================================================
    // AQUADON — Baby Plesiosaur
    // ===============================================================

    private static IdleChannel[] AquadonIdle()
    {
        return new IdleChannel[]
        {
            // Breathing — body scale pulse
            new IdleChannel("Body", new Vector3(0, 0, 0), 1.2f)
                .WithScale(new Vector3(0.01f, 0.02f, 0.01f), 1.2f),

            // Neck undulation — S-wave propagating through 4 segments
            new IdleChannel("Neck_1", new Vector3(6f, 3f, 0), 0.6f, 0f),
            new IdleChannel("Neck_2", new Vector3(7f, 4f, 0), 0.6f, 0.9f),
            new IdleChannel("Neck_3", new Vector3(8f, 5f, 0), 0.6f, 1.8f),
            new IdleChannel("Neck_4", new Vector3(6f, 3f, 0), 0.6f, 2.7f),

            // Head tilt — gentle curious look
            new IdleChannel("Head", new Vector3(4f, 3f, 5f), 0.8f, 0.5f),

            // Flippers — slow paddling
            new IdleChannel("Flipper_FL", new Vector3(0, 0, 12f), 0.4f, 0f),
            new IdleChannel("Flipper_FR", new Vector3(0, 0, 12f), 0.4f, Mathf.PI),
            new IdleChannel("Flipper_RL", new Vector3(0, 0, 8f), 0.35f, 0.5f),
            new IdleChannel("Flipper_RR", new Vector3(0, 0, 8f), 0.35f, Mathf.PI + 0.5f),

            // Tail paddle — gentle sway
            new IdleChannel("TailPaddle", new Vector3(0, 15f, 0), 0.5f, 1.0f),
        };
    }

    // ===============================================================
    // FLORASAUR — Baby Sauropod with Flowers
    // ===============================================================

    private static IdleChannel[] FlorasaurIdle()
    {
        return new IdleChannel[]
        {
            // Breathing — body scale pulse
            new IdleChannel("Body", new Vector3(1f, 0, 0), 1.0f)
                .WithScale(new Vector3(0.01f, 0.02f, 0.01f), 1.0f),

            // Neck — gentle nod and sway
            new IdleChannel("Neck_1", new Vector3(3f, 2f, 0), 0.5f, 0f),
            new IdleChannel("Neck_2", new Vector3(4f, 3f, 0), 0.5f, 0.6f),

            // Head — curious tilt
            new IdleChannel("Head", new Vector3(5f, 2f, 3f), 0.7f, 0.8f),

            // Flowers sway — each at different phase for organic feel
            new IdleChannel("Flower_1", new Vector3(0, 0, 8f), 0.5f, 0f)
                .WithPosition(new Vector3(0, 0.005f, 0), 0.5f),
            new IdleChannel("Flower_2", new Vector3(0, 0, 7f), 0.45f, 1.2f)
                .WithPosition(new Vector3(0, 0.004f, 0), 0.45f),
            new IdleChannel("Flower_3", new Vector3(0, 0, 9f), 0.55f, 2.4f)
                .WithPosition(new Vector3(0, 0.006f, 0), 0.55f),
            new IdleChannel("Flower_4", new Vector3(0, 0, 6f), 0.48f, 3.5f),
            new IdleChannel("Flower_5", new Vector3(0, 0, 7f), 0.52f, 4.2f),

            // Tail sway
            new IdleChannel("Tail_1", new Vector3(0, 10f, 0), 0.6f, 0f),
            new IdleChannel("Tail_2", new Vector3(0, 14f, 0), 0.6f, 0.8f),

            // Legs — very subtle weight shift
            new IdleChannel("Leg_FL", new Vector3(1.5f, 0, 0), 0.3f, 0f),
            new IdleChannel("Leg_FR", new Vector3(1.5f, 0, 0), 0.3f, Mathf.PI),
            new IdleChannel("Leg_RL", new Vector3(1f, 0, 0), 0.25f, 0.5f),
            new IdleChannel("Leg_RR", new Vector3(1f, 0, 0), 0.25f, Mathf.PI + 0.5f),

            // Butterfly wings (if present)
            new IdleChannel("Butterfly", new Vector3(0, 20f, 0), 3.0f, 0f)
                .WithPosition(new Vector3(0.02f, 0.015f, 0.01f), 0.3f),
        };
    }

    // ===============================================================
    // Generic — For non-starter dinos
    // ===============================================================

    private static IdleChannel[] GenericIdle()
    {
        return new IdleChannel[]
        {
            // Breathing
            new IdleChannel("Body", new Vector3(1f, 0, 0), 1.3f)
                .WithScale(new Vector3(0.008f, 0.018f, 0.008f), 1.3f),

            // Neck
            new IdleChannel("Neck", new Vector3(2f, 1f, 0), 0.6f, 0.3f),

            // Head bob
            new IdleChannel("Head", new Vector3(4f, 2f, 2f), 0.8f, 0.5f),

            // Tail sway
            new IdleChannel("Tail_1", new Vector3(0, 10f, 0), 0.7f, 0f),
            new IdleChannel("Tail_2", new Vector3(0, 13f, 0), 0.7f, 0.8f),
            new IdleChannel("TailPaddle", new Vector3(0, 12f, 0), 0.5f, 1.0f),

            // Arms / Wings
            new IdleChannel("Arm_L", new Vector3(3f, 0, 2f), 0.5f, 0.5f),
            new IdleChannel("Arm_R", new Vector3(3f, 0, 2f), 0.5f, 2.0f),

            // Legs
            new IdleChannel("Leg_L", new Vector3(1.5f, 0, 0), 0.35f, 0f),
            new IdleChannel("Leg_R", new Vector3(1.5f, 0, 0), 0.35f, Mathf.PI),
            new IdleChannel("Leg_FL", new Vector3(1f, 0, 0), 0.3f, 0f),
            new IdleChannel("Leg_FR", new Vector3(1f, 0, 0), 0.3f, Mathf.PI),
            new IdleChannel("Leg_RL", new Vector3(1f, 0, 0), 0.25f, 0.5f),
            new IdleChannel("Leg_RR", new Vector3(1f, 0, 0), 0.25f, Mathf.PI + 0.5f),

            // Flippers
            new IdleChannel("Flipper_FL", new Vector3(0, 0, 10f), 0.4f, 0f),
            new IdleChannel("Flipper_FR", new Vector3(0, 0, 10f), 0.4f, Mathf.PI),
            new IdleChannel("Flipper_RL", new Vector3(0, 0, 7f), 0.35f, 0.5f),
            new IdleChannel("Flipper_RR", new Vector3(0, 0, 7f), 0.35f, Mathf.PI + 0.5f),
        };
    }
}
