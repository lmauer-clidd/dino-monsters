#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor utility that copies imported asset prefabs/FBX into Resources/ subfolders
/// so they can be loaded at runtime via Resources.Load.
/// Run via menu: Dino Monsters > Setup Asset References
/// </summary>
public class AssetCopier : MonoBehaviour
{
    // ================================================================
    //  Source paths (EmaceArt Slavic World Free)
    // ================================================================
    const string EA = "Assets/EmaceArt/Slavic World Free/Prefabs";
    const string WALLS = EA + "/Compound/Walls";
    const string ROOFS = EA + "/Compound/Roofs";
    const string FOUNDATION = EA + "/Compound/Foundation";
    const string PORCH = EA + "/Compound/Porch";
    const string ENV = EA + "/Environment";
    const string ROAD = ENV + "/Road";
    const string ITEMS_HOUSE = EA + "/Items/House";
    const string PROP = EA + "/Prop";

    // Source paths (Normalgon Characters)
    const string CHAR = "Assets/Normalgon/Characters";
    const string CHAR_CORE = CHAR + "/Character_Core";
    const string CHAR_PARTS = CHAR + "/Medieval_Characters/FBX/RiggedParts";

    // ================================================================
    //  Destination base
    // ================================================================
    const string RES = "Assets/Resources";
    const string TK = RES + "/TownKit";
    const string CH = RES + "/Characters";

    [MenuItem("Dino Monsters/Setup Asset References")]
    static void CopyAssetsToResources()
    {
        // Create destination folders
        EnsureFolder(RES);
        EnsureFolder(TK);
        EnsureFolder(CH);

        int copied = 0;

        // ---- Buildings (Walls) ----
        copied += Copy(WALLS + "/EA03_Town_Hut_Wall_Front_04e_PRE.prefab",      TK + "/Wall_Front_Door.prefab");
        copied += Copy(WALLS + "/EA03_Town_Hut_Wall_Front_04b_PRE.prefab",      TK + "/Wall_Front.prefab");
        copied += Copy(WALLS + "/EA03_Town_Hut_Wall_Front_04h_PRE.prefab",      TK + "/Wall_Front_Window.prefab");
        copied += Copy(WALLS + "/EA03_Village_Hut_Wall_Front_02d_PRE.prefab",   TK + "/Wall_Village.prefab");
        copied += Copy(WALLS + "/EA03_Village_Hut_Wall_Front_02i_PRE.prefab",   TK + "/Wall_Village_B.prefab");
        copied += Copy(WALLS + "/EA03_Village_Hut_Wall_Front_03f_PRE.prefab",   TK + "/Wall_Village_C.prefab");

        // ---- Roofs ----
        copied += Copy(ROOFS + "/EA03_Village_Hut_Roof_01c_PRE.prefab",         TK + "/Roof.prefab");
        copied += Copy(ROOFS + "/EA03_Village_Hut_Roof_Cut_01a_PRE.prefab",     TK + "/Roof_Cut_A.prefab");
        copied += Copy(ROOFS + "/EA03_Village_Hut_Roof_Cut_01b_PRE.prefab",     TK + "/Roof_Cut_B.prefab");
        copied += Copy(ROOFS + "/EA03_Village_Hut_Roof_Half_01a_PRE.prefab",    TK + "/Roof_Half.prefab");

        // ---- Foundation ----
        copied += Copy(FOUNDATION + "/EA03_Town_Hut_Foundation_02b_PRE.prefab", TK + "/Foundation.prefab");

        // ---- Porch ----
        copied += Copy(PORCH + "/EA03_Village_HouseModule_Porch_R_03b_PRE.prefab", TK + "/Porch.prefab");

        // ---- Door ----
        copied += Copy(ENV + "/EA03_Prop_House_Door_01b_PRE.prefab",            TK + "/Door.prefab");

        // ---- Roads ----
        copied += Copy(ROAD + "/EA03_Environment_Road_Cobble_01a_PRE.prefab",   TK + "/Road_Cobble.prefab");
        copied += Copy(ROAD + "/EA03_Environment_Road_Cobble_01b_PRE.prefab",   TK + "/Road_Cobble_B.prefab");
        copied += Copy(ROAD + "/EA03_Environment_Road_Cobble_Corner_01a_PRE.prefab", TK + "/Road_Cobble_Corner.prefab");
        copied += Copy(ROAD + "/EA03_Env_Road_Wooden_01d_PRE.prefab",           TK + "/Road_Wooden.prefab");

        // ---- Interior Rooms (from Prefabs root) ----
        copied += Copy(EA + "/EA_Room_Comp_3X3_PRE.prefab",                     TK + "/Room_Small.prefab");
        copied += Copy(EA + "/EA_Room_Int_Kitchen_Comp_7x10_01a_PRE.prefab",    TK + "/Room_Kitchen.prefab");
        copied += Copy(EA + "/EA_Room_Int_LivingRoom_Comp_7x7_01a_PRE.prefab",  TK + "/Room_LivingA.prefab");
        copied += Copy(EA + "/EA_Room_Int_LivingRoom_Comp_7x7_01b_PRE.prefab",  TK + "/Room_LivingB.prefab");

        // ---- Props ----
        copied += Copy(ITEMS_HOUSE + "/EA03_Items_House_Firewood_01a_PRE.prefab", TK + "/Prop_Firewood.prefab");
        copied += Copy(ITEMS_HOUSE + "/EA03_Prop_Crate_Under_Ceiling_01b_PRE.prefab", TK + "/Prop_Crate.prefab");
        copied += Copy(ITEMS_HOUSE + "/EA03_Prop_House_Shelf_01g_PRE.prefab",    TK + "/Prop_Shelf.prefab");
        copied += Copy(ITEMS_HOUSE + "/EA03_Prop_Tool_Basket_01_PRE.prefab",     TK + "/Prop_Basket.prefab");
        copied += Copy(ITEMS_HOUSE + "/EA03_Prop_House_Hanger_01b_PRE.prefab",   TK + "/Prop_Hanger.prefab");

        // ---- Characters ----
        copied += Copy(CHAR_CORE + "/Character_Core.prefab",                     CH + "/CharacterBase.prefab");

        // Character parts (FBX)
        copied += Copy(CHAR_PARTS + "/Head/Head_Human.fbx",                      CH + "/Head_Human.fbx");
        copied += Copy(CHAR_PARTS + "/Head/Head_Orc.fbx",                        CH + "/Head_Orc.fbx");

        // Torso
        copied += Copy(CHAR_PARTS + "/Torso/Long_Top/Shirt_DoublePuffSleeves.fbx", CH + "/Shirt_DoublePuffSleeves.fbx");
        copied += Copy(CHAR_PARTS + "/Torso/Long_Top/Knight_PlatedArmor.fbx",      CH + "/Knight_PlatedArmor.fbx");
        copied += Copy(CHAR_PARTS + "/Torso/Long_Top/Robe_Long_OverPants.fbx",     CH + "/Robe_Long.fbx");
        copied += Copy(CHAR_PARTS + "/Torso/Long_Top/Apron_DoublePockets.fbx",     CH + "/Apron_DoublePockets.fbx");
        copied += Copy(CHAR_PARTS + "/Torso/Long_Top/OverCoat_PuffedSleeves_OverDress.fbx", CH + "/OverCoat_PuffedSleeves.fbx");
        copied += Copy(CHAR_PARTS + "/Torso/WaistLength_Top/WL_Bodice_LacedTop.fbx", CH + "/Bodice_LacedTop.fbx");

        // Legs
        copied += Copy(CHAR_PARTS + "/Leg/Pants_Fitted.fbx",                     CH + "/Pants_Fitted.fbx");
        copied += Copy(CHAR_PARTS + "/Leg/Pants_PuffedKnees.fbx",                CH + "/Pants_PuffedKnees.fbx");
        copied += Copy(CHAR_PARTS + "/Leg/Pants_WideCuffed.fbx",                 CH + "/Pants_WideCuffed.fbx");
        copied += Copy(CHAR_PARTS + "/Leg/Skirt_Long_DrapedCurtain.fbx",         CH + "/Skirt_Long.fbx");
        copied += Copy(CHAR_PARTS + "/Leg/Skirt_Long_Wrap.fbx",                  CH + "/Skirt_Wrap.fbx");

        // Feet (left + right pairs)
        copied += Copy(CHAR_PARTS + "/Feet/Boot_Plain_Left.fbx",                 CH + "/Boot_Plain_Left.fbx");
        copied += Copy(CHAR_PARTS + "/Feet/Boot_Plain_Right.fbx",                CH + "/Boot_Plain_Right.fbx");
        copied += Copy(CHAR_PARTS + "/Feet/Boot_Flare_Left.fbx",                 CH + "/Boot_Flare_Left.fbx");
        copied += Copy(CHAR_PARTS + "/Feet/Boot_Flare_Right.fbx",                CH + "/Boot_Flare_Right.fbx");
        copied += Copy(CHAR_PARTS + "/Feet/Boot_MetalPlain_Left.fbx",            CH + "/Boot_Metal_Left.fbx");
        copied += Copy(CHAR_PARTS + "/Feet/Boot_MetalPlain_Right.fbx",           CH + "/Boot_Metal_Right.fbx");

        // Hands
        copied += Copy(CHAR_PARTS + "/Hand/Hand_Human_Left.fbx",                 CH + "/Hand_Human_Left.fbx");
        copied += Copy(CHAR_PARTS + "/Hand/Hand_Human_Right.fbx",                CH + "/Hand_Human_Right.fbx");
        copied += Copy(CHAR_PARTS + "/Hand/Glove_MetalPlated_Left.fbx",          CH + "/Glove_Metal_Left.fbx");
        copied += Copy(CHAR_PARTS + "/Hand/Glove_MetalPlated_Right.fbx",         CH + "/Glove_Metal_Right.fbx");

        // Hair / Helmets
        copied += Copy(CHAR_PARTS + "/Helmet_Hair/Hair_Short_SideParted.fbx",    CH + "/Hair_Short_SideParted.fbx");
        copied += Copy(CHAR_PARTS + "/Helmet_Hair/Hair_Short_Shaggy.fbx",        CH + "/Hair_Short_Shaggy.fbx");
        copied += Copy(CHAR_PARTS + "/Helmet_Hair/Hair_Long_Straight.fbx",       CH + "/Hair_Long_Straight.fbx");
        copied += Copy(CHAR_PARTS + "/Helmet_Hair/Hair_Long_SideBangs.fbx",      CH + "/Hair_Long_SideBangs.fbx");
        copied += Copy(CHAR_PARTS + "/Helmet_Hair/Coif.fbx",                     CH + "/Coif.fbx");
        copied += Copy(CHAR_PARTS + "/Helmet_Hair/Helmet_Nasal.fbx",             CH + "/Helmet_Nasal.fbx");
        copied += Copy(CHAR_PARTS + "/Helmet_Hair/Helmet_Spangenhelm.fbx",       CH + "/Helmet_Spangenhelm.fbx");

        AssetDatabase.Refresh();
        Debug.Log($"[AssetCopier] Setup complete! {copied} assets copied to Resources/.");
    }

    // ================================================================
    //  Helpers
    // ================================================================

    /// <summary>
    /// Copy an asset from src to dst. Returns 1 on success, 0 on failure or if already exists.
    /// </summary>
    static int Copy(string src, string dst)
    {
        if (!AssetExists(src))
        {
            Debug.LogWarning($"[AssetCopier] Source not found: {src}");
            return 0;
        }

        if (AssetExists(dst))
        {
            // Already copied, skip
            return 0;
        }

        bool ok = AssetDatabase.CopyAsset(src, dst);
        if (!ok)
        {
            Debug.LogError($"[AssetCopier] Failed to copy: {src} -> {dst}");
            return 0;
        }

        return 1;
    }

    static bool AssetExists(string path)
    {
        return !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path, AssetPathToGUIDOptions.OnlyExistingAssets));
    }

    /// <summary>
    /// Ensure a folder exists, creating intermediate folders as needed.
    /// </summary>
    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        // Split path and create each level
        string[] parts = path.Replace("\\", "/").Split('/');
        string current = parts[0]; // "Assets"
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }
            current = next;
        }
    }

    // ================================================================
    //  Cleanup
    // ================================================================

    [MenuItem("Dino Monsters/Clear Asset References")]
    static void ClearResourceCopies()
    {
        if (AssetDatabase.IsValidFolder(TK))
        {
            AssetDatabase.DeleteAsset(TK);
        }
        if (AssetDatabase.IsValidFolder(CH))
        {
            AssetDatabase.DeleteAsset(CH);
        }
        AssetDatabase.Refresh();
        Debug.Log("[AssetCopier] Cleared Resources/TownKit and Resources/Characters.");
    }
}
#endif
