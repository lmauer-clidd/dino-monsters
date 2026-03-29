using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Game constants ported from the Phaser POC (utils/constants.ts).
/// </summary>
public static class Constants
{
    // -------------------------------------------------------------------------
    // Player
    // -------------------------------------------------------------------------
    public const int MaxPartySize  = 6;
    public const int MaxLevel      = 50;
    public const int MaxMoveSlots  = 4;

    // -------------------------------------------------------------------------
    // Battle
    // -------------------------------------------------------------------------
    public const float BaseEncounterRate      = 8f;      // % per step in tall grass
    public const float StabMultiplier         = 1.5f;
    public const float CriticalHitMultiplier  = 1.5f;
    public const float CriticalHitChance      = 6.25f;   // %
    public const float DamageRandomMin        = 0.85f;
    public const float DamageRandomMax        = 1.0f;

    // -------------------------------------------------------------------------
    // Capture
    // -------------------------------------------------------------------------
    public const int MaxCaptureRate = 255;

    // -------------------------------------------------------------------------
    // Save
    // -------------------------------------------------------------------------
    public const string SaveKeyPrefix = "jurassic-trainers-save-";
    public const int MaxSaveSlots     = 3;

    // -------------------------------------------------------------------------
    // UI — Dialogue
    // -------------------------------------------------------------------------
    public const float DialogueSpeed     = 0.03f;  // seconds per character (typewriter)
    public const float DialogueFastSpeed = 0.01f;
    public const float DialogueBoxHeight = 96f;

    // -------------------------------------------------------------------------
    // Type names (indexed by DinoType enum)
    // -------------------------------------------------------------------------
    public static readonly string[] DinoTypeNames = {
        "Normal",   // 0
        "Fire",     // 1
        "Water",    // 2
        "Earth",    // 3
        "Air",      // 4
        "Electric", // 5
        "Ice",      // 6
        "Venom",    // 7
        "Flora",    // 8
        "Fossil",   // 9
        "Shadow",   // 10
        "Light",    // 11
        "Metal",    // 12
        "Primal",   // 13
    };

    // -------------------------------------------------------------------------
    // Type colors (indexed by DinoType enum) — warm, amber-toned palette
    // -------------------------------------------------------------------------
    public static readonly Color[] DinoTypeColors = {
        new Color32(0xa8, 0xa8, 0x78, 0xFF), // Normal
        new Color32(0xf0, 0x80, 0x30, 0xFF), // Fire
        new Color32(0x68, 0x90, 0xf0, 0xFF), // Water
        new Color32(0xc8, 0xa0, 0x40, 0xFF), // Earth
        new Color32(0xa8, 0x90, 0xf0, 0xFF), // Air
        new Color32(0xf8, 0xd0, 0x30, 0xFF), // Electric
        new Color32(0x98, 0xd8, 0xd8, 0xFF), // Ice
        new Color32(0xa0, 0x40, 0xa0, 0xFF), // Venom
        new Color32(0x78, 0xc8, 0x50, 0xFF), // Flora
        new Color32(0xb8, 0xa0, 0x38, 0xFF), // Fossil
        new Color32(0x70, 0x58, 0x48, 0xFF), // Shadow
        new Color32(0xf8, 0xe8, 0x70, 0xFF), // Light
        new Color32(0xb8, 0xb8, 0xd0, 0xFF), // Metal
        new Color32(0x70, 0x38, 0xf8, 0xFF), // Primal
    };

    // -------------------------------------------------------------------------
    // UI Colors — Fossil/Amber Art Direction palette
    // Obsidienne → Bitume → Terre brûlée → Sédiment → Résine → Sable → Calcaire → Quartz
    // -------------------------------------------------------------------------

    // Master palette
    public static readonly Color ColorObsidienne   = HexToColor(0x181018); // deep black
    public static readonly Color ColorBitume        = HexToColor(0x282028); // warm black
    public static readonly Color ColorTerreBrulee   = HexToColor(0x503820); // warm brown
    public static readonly Color ColorSediment      = HexToColor(0x886830); // earth/wood
    public static readonly Color ColorResine        = HexToColor(0xC89840); // SIGNATURE amber
    public static readonly Color ColorSableSec      = HexToColor(0xE8C868); // warm sand
    public static readonly Color ColorCalcaire      = HexToColor(0xF0E8D0); // cream/bone
    public static readonly Color ColorQuartz        = HexToColor(0xF8F0E8); // pure white

    // Semantic aliases
    public static readonly Color ColorBlack         = HexToColor(0x181018);
    public static readonly Color ColorWhite          = HexToColor(0xF0E8D0);

    // UI Panels — warm earthy tones (no more dark purple!)
    public static readonly Color ColorBgDark         = HexToColor(0x282028); // Bitume
    public static readonly Color ColorUiBg           = HexToColor(0x382818); // dark warm brown
    public static readonly Color ColorUiBorder       = HexToColor(0xC89840); // Résine/amber
    public static readonly Color ColorUiBorderLight  = HexToColor(0xE8C868); // Sable sec
    public static readonly Color ColorUiBorderDark   = HexToColor(0x503820); // Terre brûlée

    // HP/XP bars — art direction spec colors
    public static readonly Color ColorHpGreen        = HexToColor(0x48B838);
    public static readonly Color ColorHpYellow       = HexToColor(0xE8C038);
    public static readonly Color ColorHpRed          = HexToColor(0xD83028);
    public static readonly Color ColorXpBlue         = HexToColor(0x3078B8);
    public static readonly Color ColorHpBarBg        = HexToColor(0x383028);

    // Text
    public static readonly Color ColorTextPrimary    = HexToColor(0xF0E8D0); // Calcaire (light on dark)
    public static readonly Color ColorTextDarkPrimary = HexToColor(0x282028); // Bitume (dark on light)
    public static readonly Color ColorTextSecondary  = HexToColor(0x685848); // warm gray
    public static readonly Color ColorTextShadow     = HexToColor(0x181018);
    public static readonly Color ColorTextDark       = HexToColor(0x383028);

    // Dialogue — warm parchment style
    public static readonly Color ColorDialogueBg     = HexToColor(0xF0E8D0); // Calcaire cream
    public static readonly Color ColorDialogueBorder = HexToColor(0x503820); // Terre brûlée
    public static readonly Color ColorDialogueAccent = HexToColor(0x886830); // Sédiment

    // Menu
    public static readonly Color ColorMenuBg         = HexToColor(0x382818); // warm dark brown
    public static readonly Color ColorMenuOverlay    = new Color(0, 0, 0, 0.55f);

    // Selection
    public static readonly Color ColorSelection      = HexToColor(0xC89840); // Résine amber
    public static readonly Color ColorCursor          = HexToColor(0xE85020); // red-orange

    // -------------------------------------------------------------------------
    // Status Effects — defined in DinoMonsters.Dinos.StatusEffect (Dino.cs)
    // -------------------------------------------------------------------------

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------
    public static Color HexToColor(int hex)
    {
        float r = ((hex >> 16) & 0xFF) / 255f;
        float g = ((hex >> 8)  & 0xFF) / 255f;
        float b = (hex         & 0xFF) / 255f;
        return new Color(r, g, b, 1f);
    }

    public static string GetTypeName(DinoType type)
    {
        int idx = (int)type;
        if (idx >= 0 && idx < DinoTypeNames.Length) return DinoTypeNames[idx];
        return "???";
    }

    public static Color GetTypeColor(DinoType type)
    {
        int idx = (int)type;
        if (idx >= 0 && idx < DinoTypeColors.Length) return DinoTypeColors[idx];
        return Color.white;
    }
}
