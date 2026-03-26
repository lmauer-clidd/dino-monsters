// ============================================================
// Dino Monsters -- Inventory System
// Ported from TypeScript POC to C# (Unity)
// ============================================================
//
// Uses existing DataLoader types (ItemData with string effect/category).
// ============================================================

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemUseResult
{
    public bool success;
    public string message;

    public ItemUseResult(bool success, string message)
    {
        this.success = success;
        this.message = message;
    }
}

[System.Serializable]
public class InventoryEntry
{
    public int itemId;
    public int count;
}

// --------------- Inventory System ---------------

public class InventorySystem
{
    private Dictionary<int, int> items;

    public InventorySystem()
    {
        items = new Dictionary<int, int>();
    }

    // --------------- Core Operations ---------------

    public void AddItem(int itemId, int quantity = 1)
    {
        if (items.ContainsKey(itemId))
            items[itemId] += quantity;
        else
            items[itemId] = quantity;
    }

    public bool RemoveItem(int itemId, int quantity = 1)
    {
        if (!items.ContainsKey(itemId)) return false;
        int current = items[itemId];
        if (current < quantity) return false;

        int remaining = current - quantity;
        if (remaining <= 0)
            items.Remove(itemId);
        else
            items[itemId] = remaining;

        return true;
    }

    public bool HasItem(int itemId)
    {
        return items.ContainsKey(itemId) && items[itemId] > 0;
    }

    public int GetCount(int itemId)
    {
        return items.ContainsKey(itemId) ? items[itemId] : 0;
    }

    // --------------- Item Usage ---------------

    /// <summary>
    /// Apply an item effect to a target dino.
    /// Returns success status and message. Consumes the item on success (except key items).
    /// Item effect types are determined by the "effect" string field in DataLoader's ItemData.
    /// </summary>
    public ItemUseResult UseItem(int itemId, Dino target = null)
    {
        if (!HasItem(itemId))
            return new ItemUseResult(false, "Vous n'avez pas cet objet!");

        var itemData = DataLoader.Instance.GetItem(itemId);
        if (itemData == null)
            return new ItemUseResult(false, "Objet inconnu!");

        ItemUseResult result;
        string effect = itemData.effect ?? "";

        switch (effect)
        {
            case "heal_hp":
                result = ApplyHealHP(itemData, target);
                break;
            case "heal_pp":
                result = ApplyHealPP(itemData, target);
                break;
            case "heal_pp_all":
                result = ApplyHealPPAll(itemData, target);
                break;
            case "heal_status":
                result = ApplyHealStatus(itemData, target);
                break;
            case "full_heal":
                result = ApplyFullHeal(itemData, target);
                break;
            case "revive":
                result = ApplyRevive(itemData, target);
                break;
            case "rare_candy":
                result = ApplyRareCandy(itemData, target);
                break;
            case "capture":
                // Capture balls handled by BattleSystem
                result = new ItemUseResult(true, "");
                break;
            case "x_stat":
                // X items handled by BattleSystem
                result = new ItemUseResult(true, $"{itemData.name} utilise!");
                break;
            default:
                result = new ItemUseResult(false, "Cet objet ne peut pas etre utilise ici.");
                break;
        }

        if (result.success && itemData.category != "key")
        {
            RemoveItem(itemId);
        }

        return result;
    }

    // --------------- Effect Implementations ---------------

    private ItemUseResult ApplyHealHP(ItemData itemData, Dino target)
    {
        if (target == null) return new ItemUseResult(false, "Pas de cible!");
        if (target.IsFainted()) return new ItemUseResult(false, $"{target.nickname} est K.O.!");
        if (target.currentHp >= target.maxHp) return new ItemUseResult(false, $"{target.nickname} a deja tous ses PV!");

        int healAmount = itemData.value > 0 ? (int)itemData.value : 20;
        int healed = target.Heal(healAmount);
        return new ItemUseResult(true, $"{target.nickname} recupere {healed} PV!");
    }

    private ItemUseResult ApplyHealPP(ItemData itemData, Dino target)
    {
        if (target == null) return new ItemUseResult(false, "Pas de cible!");

        bool restored = false;
        foreach (var move in target.moves)
        {
            if (move.currentPP < move.maxPP)
            {
                int amount = itemData.value > 0 ? (int)itemData.value : 10;
                move.currentPP = Mathf.Min(move.maxPP, move.currentPP + amount);
                restored = true;
                break;
            }
        }

        if (!restored) return new ItemUseResult(false, "PP deja au maximum!");
        return new ItemUseResult(true, "PP restaures!");
    }

    private ItemUseResult ApplyHealPPAll(ItemData itemData, Dino target)
    {
        if (target == null) return new ItemUseResult(false, "Pas de cible!");

        bool restored = false;
        foreach (var move in target.moves)
        {
            if (move.currentPP < move.maxPP)
            {
                move.currentPP = move.maxPP;
                restored = true;
            }
        }

        if (!restored) return new ItemUseResult(false, "PP deja au maximum!");
        return new ItemUseResult(true, "Tous les PP restaures!");
    }

    private ItemUseResult ApplyHealStatus(ItemData itemData, Dino target)
    {
        if (target == null) return new ItemUseResult(false, "Pas de cible!");
        if (target.status == StatusEffect.None)
            return new ItemUseResult(false, $"{target.nickname} n'a pas de probleme de statut!");

        target.CureStatus();
        return new ItemUseResult(true, $"{target.nickname} est gueri!");
    }

    private ItemUseResult ApplyFullHeal(ItemData itemData, Dino target)
    {
        if (target == null) return new ItemUseResult(false, "Pas de cible!");
        if (target.IsFainted()) return new ItemUseResult(false, $"{target.nickname} est K.O.!");

        target.FullHeal();
        return new ItemUseResult(true, $"{target.nickname} est completement gueri!");
    }

    private ItemUseResult ApplyRevive(ItemData itemData, Dino target)
    {
        if (target == null) return new ItemUseResult(false, "Pas de cible!");
        if (!target.IsFainted()) return new ItemUseResult(false, $"{target.nickname} n'est pas K.O.!");

        float healPercent = (itemData.value > 0 ? itemData.value : 50f) / 100f;
        target.Heal(Mathf.FloorToInt(target.maxHp * healPercent));
        target.CureStatus();
        return new ItemUseResult(true, $"{target.nickname} reprend connaissance!");
    }

    private ItemUseResult ApplyRareCandy(ItemData itemData, Dino target)
    {
        if (target == null) return new ItemUseResult(false, "Pas de cible!");
        if (target.level >= Dino.MAX_LEVEL) return new ItemUseResult(false, $"{target.nickname} est deja au niveau maximum!");

        int needed = target.GetXpForNextLevel() - target.xp;
        target.GainXp(Mathf.Max(1, needed));
        return new ItemUseResult(true, $"{target.nickname} monte au niveau {target.level}!");
    }

    // --------------- Queries ---------------

    public List<KeyValuePair<int, int>> GetAllItems()
    {
        var result = new List<KeyValuePair<int, int>>();
        foreach (var kvp in items)
        {
            result.Add(kvp);
        }
        result.Sort((a, b) => a.Key.CompareTo(b.Key));
        return result;
    }

    // --------------- Serialization ---------------

    public List<InventoryEntry> SerializeToList()
    {
        var list = new List<InventoryEntry>();
        foreach (var kvp in items)
        {
            list.Add(new InventoryEntry { itemId = kvp.Key, count = kvp.Value });
        }
        return list;
    }

    public void DeserializeFromList(List<InventoryEntry> list)
    {
        items = new Dictionary<int, int>();
        if (list == null) return;
        foreach (var entry in list)
        {
            items[entry.itemId] = entry.count;
        }
    }
}
