// ============================================================
// Dino Monsters -- Damage Calculator (Pure Static Utility)
// Ported from TypeScript POC to C# (Unity)
// ============================================================

using System;

/// <summary>
/// Pure damage formula and type effectiveness calculations.
/// No state — all methods are static.
/// </summary>
public static class DamageCalculator
{
    // --------------- Type Chart ---------------
    // 14 types: Normal(0) Fire(1) Water(2) Earth(3) Air(4) Electric(5) Ice(6)
    //           Venom(7) Flora(8) Fossil(9) Shadow(10) Light(11) Metal(12) Primal(13)

    private static readonly float[,] TYPE_CHART = new float[14, 14]
    {
        //              Nor   Fir   Wat   Ear   Air   Ele   Ice   Ven   Flo   Fos   Sha   Lig   Met   Pri
        /* Normal */ {  1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,  0.5f,  0f,   1f,  0.5f,  1f  },
        /* Fire   */ {  1f,  0.5f, 0.5f,  1f,   1f,   1f,   2f,   1f,   2f,  0.5f,  1f,   1f,   2f,   1f  },
        /* Water  */ {  1f,   2f,  0.5f,  2f,   1f,   1f,   1f,   1f,  0.5f,  2f,   1f,   1f,   1f,   1f  },
        /* Earth  */ {  1f,   2f,   1f,   1f,   0f,   2f,   1f,   2f,  0.5f,  1f,   1f,   1f,   2f,   1f  },
        /* Air    */ {  1f,   1f,   1f,   1f,   1f,  0.5f,  1f,   1f,   2f,  0.5f,  1f,   1f,  0.5f,  1f  },
        /* Elec   */ {  1f,   1f,   2f,   0f,   2f,  0.5f,  1f,   1f,  0.5f,  1f,   1f,   1f,   1f,   1f  },
        /* Ice    */ {  1f,  0.5f, 0.5f,  2f,   2f,   1f,  0.5f,  1f,   2f,   1f,   1f,   1f,  0.5f,  2f  },
        /* Venom  */ {  1f,   1f,   1f,  0.5f,  1f,   1f,   1f,  0.5f,  2f,  0.5f,  1f,   1f,   0f,   1f  },
        /* Flora  */ {  1f,  0.5f,  2f,   2f,  0.5f,  1f,   1f,  0.5f, 0.5f,  2f,   1f,   1f,  0.5f,  1f  },
        /* Fossil */ {  1f,   2f,  0.5f,  1f,   2f,   1f,   2f,   1f,   1f,   1f,   1f,   1f,  0.5f,  1f  },
        /* Shadow */ {  0f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   2f,  0.5f,  1f,  0.5f },
        /* Light  */ {  1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   2f,   1f,  0.5f,  2f  },
        /* Metal  */ {  1f,  0.5f, 0.5f,  1f,   1f,  0.5f,  2f,   1f,   1f,   2f,   1f,   2f,  0.5f,  1f  },
        /* Primal */ {  1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   2f,  0.5f,  1f,  0.5f },
    };

    // --------------- Damage Calculation ---------------

    /// <summary>
    /// Calculate damage using the standard Pokemon-style formula.
    /// </summary>
    /// <param name="level">Attacker's level</param>
    /// <param name="power">Move's base power</param>
    /// <param name="attack">Attacker's relevant attack stat</param>
    /// <param name="defense">Defender's relevant defense stat</param>
    /// <param name="stab">Same-Type Attack Bonus</param>
    /// <param name="effectiveness">Type effectiveness multiplier</param>
    /// <param name="critical">Whether this is a critical hit</param>
    /// <param name="randomFactor">Random factor 0.85-1.0 (default -1 for actual random)</param>
    /// <returns>Final damage (minimum 1 unless immune)</returns>
    public static int CalculateDamage(int level, int power, int attack, int defense,
        bool stab, float effectiveness, bool critical, float randomFactor = -1f)
    {
        // Immune check
        if (effectiveness == 0f) return 0;

        float dmg = ((2f * level / 5f + 2f) * power * ((float)attack / defense)) / 50f + 2f;

        if (stab) dmg *= 1.5f;
        dmg *= effectiveness;
        if (critical) dmg *= 1.5f;

        // Apply random factor
        if (randomFactor < 0f)
            randomFactor = UnityEngine.Random.Range(0.85f, 1.0f);
        dmg *= randomFactor;

        return Math.Max(1, (int)Math.Floor(dmg));
    }

    // --------------- Type Effectiveness ---------------

    /// <summary>
    /// Get the combined effectiveness multiplier for an attack type vs defender types.
    /// Returns the product of all type matchups (e.g. 0, 0.25, 0.5, 1, 2, 4).
    /// </summary>
    public static float GetEffectiveness(int attackType, int[] defenderTypes)
    {
        float mult = 1f;
        foreach (int defType in defenderTypes)
        {
            if (attackType >= 0 && attackType < 14 && defType >= 0 && defType < 14)
                mult *= TYPE_CHART[attackType, defType];
        }
        return mult;
    }

    /// <summary>
    /// Get effectiveness for DinoType enums with optional second type.
    /// </summary>
    public static float GetEffectiveness(DinoType attackType, DinoType defType1, DinoType? defType2 = null)
    {
        float mult = TYPE_CHART[(int)attackType, (int)defType1];
        if (defType2.HasValue && defType2.Value != defType1)
            mult *= TYPE_CHART[(int)attackType, (int)defType2.Value];
        return mult;
    }

    // --------------- STAB Check ---------------

    /// <summary>
    /// Check if the move type matches any of the attacker's types (Same-Type Attack Bonus).
    /// </summary>
    public static bool IsSTAB(int moveType, int[] attackerTypes)
    {
        foreach (int t in attackerTypes)
        {
            if (moveType == t) return true;
        }
        return false;
    }

    public static bool IsSTAB(DinoType moveType, DinoType type1, DinoType? type2 = null)
    {
        if (moveType == type1) return true;
        if (type2.HasValue && moveType == type2.Value) return true;
        return false;
    }

    // --------------- Critical Hit ---------------

    /// <summary>
    /// Roll for a critical hit. Base rate is 1/24, increases with crit stage.
    /// Stage 0: 1/24, Stage 1: 1/8, Stage 2: 1/2, Stage 3+: always crit.
    /// </summary>
    public static bool RollCritical(int critStage = 0)
    {
        float threshold;
        switch (critStage)
        {
            case 0:  threshold = 1f / 24f; break;
            case 1:  threshold = 1f / 8f;  break;
            case 2:  threshold = 1f / 2f;  break;
            default: return true; // Stage 3+ = guaranteed crit
        }
        return UnityEngine.Random.value < threshold;
    }

    // --------------- Capture Rate ---------------

    /// <summary>
    /// Calculate capture rate (0-255).
    /// </summary>
    public static int CalculateCaptureRate(int maxHp, int currentHp, int captureRate, float ballMod, float statusMod)
    {
        float hpFactor = (3f * maxHp - 2f * currentHp) / (3f * maxHp);
        return Math.Min(255, (int)Math.Floor(hpFactor * captureRate * ballMod * statusMod));
    }

    /// <summary>
    /// Calculate XP yield for defeating a dino.
    /// </summary>
    public static int CalculateXpYield(int baseXpYield, int defeatedLevel, int participantCount = 1)
    {
        return (int)Math.Floor((float)(baseXpYield * defeatedLevel) / (5 * participantCount));
    }

    // --------------- Effectiveness Label ---------------

    public static string GetEffectivenessLabel(float multiplier)
    {
        if (multiplier == 0f) return "Aucun effet!";
        if (multiplier < 1f) return "Pas tres efficace...";
        if (multiplier > 1f) return "Super efficace!";
        return ""; // neutral
    }
}
