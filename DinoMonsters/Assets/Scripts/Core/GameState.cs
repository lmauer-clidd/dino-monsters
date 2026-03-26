// ============================================================
// Dino Monsters -- Game State (Plain C# Singleton)
// Ported from TypeScript POC to C# (Unity)
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;

// --------------- Serializable Sub-Structures ---------------

[System.Serializable]
public class HealPoint
{
    public string mapId;
    public int x;
    public int y;

    public HealPoint()
    {
        mapId = "BOURG_NID";
        x = 7;
        y = 5;
    }

    public HealPoint(string mapId, int x, int y)
    {
        this.mapId = mapId;
        this.x = x;
        this.y = y;
    }
}

[System.Serializable]
public class DinodexEntry
{
    public int speciesId;
    public bool seen;
    public bool caught;
}

[System.Serializable]
public class FlagEntry
{
    public string key;
    public bool value;
}

// InventoryEntry is defined in InventorySystem.cs

// --------------- Serializable Save Data ---------------

[System.Serializable]
public class GameSaveData
{
    public int version = 1;
    public string playerName;
    public int money;
    public bool[] badges = new bool[8];
    public List<Dino> party = new List<Dino>();
    public List<Dino> pcStorage = new List<Dino>();
    public List<InventoryEntry> inventory = new List<InventoryEntry>();
    public List<DinodexEntry> dinodex = new List<DinodexEntry>();
    public List<FlagEntry> flags = new List<FlagEntry>();
    public List<string> defeatedTrainers = new List<string>();
    public HealPoint healPoint = new HealPoint();
    public string currentMapId;
    public int playerX;
    public int playerY;
    public float playtime;
    public string savedAt;
}

// --------------- Game State Singleton ---------------

public class GameState
{
    private static GameState instance;

    // --- Player ---
    public string PlayerName { get; set; }
    public int Money { get; private set; }
    public bool[] Badges { get; private set; }

    // --- Party & Storage ---
    public List<Dino> Party { get; private set; }
    public List<Dino> PCStorage { get; private set; }
    public const int MAX_PARTY_SIZE = 6;

    // --- Inventory ---
    public InventorySystem Inventory { get; private set; }

    // --- Story Progression ---
    private Dictionary<string, bool> flags;
    private HashSet<string> defeatedTrainers;

    // --- Dinodex ---
    private Dictionary<int, DinodexEntry> dinodex;

    // --- Location ---
    public string CurrentMapId { get; set; }
    public int PlayerX { get; set; }
    public int PlayerY { get; set; }
    public HealPoint LastHealPoint { get; private set; }

    // --- Playtime ---
    private float sessionStartTime;
    private float accumulatedPlaytime;

    // --------------- Singleton ---------------

    public static GameState Instance
    {
        get
        {
            if (instance == null)
                instance = new GameState();
            return instance;
        }
    }

    public static void Reset()
    {
        instance = null;
    }

    // --------------- Constructor ---------------

    private GameState()
    {
        PlayerName = "";
        Money = 3000;
        Badges = new bool[8];
        Party = new List<Dino>();
        PCStorage = new List<Dino>();
        Inventory = new InventorySystem();
        flags = new Dictionary<string, bool>();
        defeatedTrainers = new HashSet<string>();
        dinodex = new Dictionary<int, DinodexEntry>();
        CurrentMapId = "BOURG_NID";
        PlayerX = 7;
        PlayerY = 5;
        LastHealPoint = new HealPoint();
        sessionStartTime = Time.realtimeSinceStartup;
        accumulatedPlaytime = 0f;
    }

    // --------------- Initialization ---------------

    public void Init(string playerName, int starterId)
    {
        PlayerName = playerName;
        Money = 3000;
        Badges = new bool[8];
        Party = new List<Dino>();
        PCStorage = new List<Dino>();
        Inventory = new InventorySystem();
        flags = new Dictionary<string, bool>();
        defeatedTrainers = new HashSet<string>();
        dinodex = new Dictionary<int, DinodexEntry>();
        CurrentMapId = "BOURG_NID";
        PlayerX = 7;
        PlayerY = 5;
        LastHealPoint = new HealPoint();
        sessionStartTime = Time.realtimeSinceStartup;
        accumulatedPlaytime = 0f;

        // Create starter
        var starter = Dino.CreateStarter(starterId, 5);
        AddToParty(starter);
        RegisterCaught(starterId);

        // Starting items
        Inventory.AddItem(1, 3);   // 3 Potions
        Inventory.AddItem(16, 5);  // 5 Jurassic Balls
    }

    // --------------- Party Management ---------------

    public bool AddToParty(Dino dino)
    {
        if (Party.Count >= MAX_PARTY_SIZE) return false;
        Party.Add(dino);
        return true;
    }

    public void AddToPC(Dino dino)
    {
        PCStorage.Add(dino);
    }

    public Dino GetLeadDino()
    {
        return Party.Count > 0 ? Party[0] : null;
    }

    public bool HasAliveDino()
    {
        foreach (var dino in Party)
        {
            if (!dino.IsFainted()) return true;
        }
        return false;
    }

    // --------------- Money ---------------

    public void AddMoney(int amount)
    {
        Money = Mathf.Min(Money + Mathf.Max(0, amount), 999999);
    }

    public bool RemoveMoney(int amount)
    {
        int cost = Mathf.Max(0, amount);
        if (Money < cost) return false;
        Money -= cost;
        return true;
    }

    // --------------- Badges ---------------

    public void AddBadge(int index)
    {
        if (index >= 0 && index < 8)
            Badges[index] = true;
    }

    public bool HasBadge(int index)
    {
        if (index < 0 || index >= 8) return false;
        return Badges[index];
    }

    public int GetBadgeCount()
    {
        int count = 0;
        foreach (bool b in Badges)
        {
            if (b) count++;
        }
        return count;
    }

    // --------------- Flags ---------------

    public void SetFlag(string key, bool value = true)
    {
        flags[key] = value;
    }

    public bool HasFlag(string key)
    {
        return flags.ContainsKey(key) && flags[key];
    }

    public bool GetFlag(string key)
    {
        return flags.ContainsKey(key) && flags[key];
    }

    // --------------- Trainers ---------------

    public void DefeatTrainer(string id)
    {
        defeatedTrainers.Add(id);
    }

    public bool IsTrainerDefeated(string id)
    {
        return defeatedTrainers.Contains(id);
    }

    // --------------- Heal Point ---------------

    public void SetHealPoint(string mapId, int x, int y)
    {
        LastHealPoint = new HealPoint(mapId, x, y);
    }

    // --------------- Dinodex ---------------

    public void RegisterSeen(int speciesId)
    {
        if (!dinodex.ContainsKey(speciesId))
        {
            dinodex[speciesId] = new DinodexEntry { speciesId = speciesId, seen = true, caught = false };
        }
        else
        {
            dinodex[speciesId].seen = true;
        }
    }

    public void RegisterCaught(int speciesId)
    {
        if (!dinodex.ContainsKey(speciesId))
        {
            dinodex[speciesId] = new DinodexEntry { speciesId = speciesId, seen = true, caught = true };
        }
        else
        {
            dinodex[speciesId].seen = true;
            dinodex[speciesId].caught = true;
        }
    }

    public DinodexEntry GetDinodexEntry(int speciesId)
    {
        return dinodex.ContainsKey(speciesId) ? dinodex[speciesId] : null;
    }

    public int GetDinodexSeenCount()
    {
        int count = 0;
        foreach (var entry in dinodex.Values)
        {
            if (entry.seen) count++;
        }
        return count;
    }

    public int GetDinodexCaughtCount()
    {
        int count = 0;
        foreach (var entry in dinodex.Values)
        {
            if (entry.caught) count++;
        }
        return count;
    }

    // --------------- Playtime ---------------

    public float GetPlaytime()
    {
        float sessionSeconds = Time.realtimeSinceStartup - sessionStartTime;
        return accumulatedPlaytime + sessionSeconds;
    }

    // --------------- Save / Load ---------------

    public GameSaveData Serialize()
    {
        var data = new GameSaveData();
        data.version = 1;
        data.playerName = PlayerName;
        data.money = Money;
        data.badges = (bool[])Badges.Clone();
        data.party = new List<Dino>(Party);
        data.pcStorage = new List<Dino>(PCStorage);
        data.currentMapId = CurrentMapId;
        data.playerX = PlayerX;
        data.playerY = PlayerY;
        data.healPoint = new HealPoint(LastHealPoint.mapId, LastHealPoint.x, LastHealPoint.y);
        data.playtime = GetPlaytime();
        data.savedAt = DateTime.Now.ToString("o");

        // Inventory
        data.inventory = Inventory.SerializeToList();

        // Dinodex
        data.dinodex = new List<DinodexEntry>();
        foreach (var entry in dinodex.Values)
        {
            data.dinodex.Add(entry);
        }

        // Flags
        data.flags = new List<FlagEntry>();
        foreach (var kvp in flags)
        {
            data.flags.Add(new FlagEntry { key = kvp.Key, value = kvp.Value });
        }

        // Defeated trainers
        data.defeatedTrainers = new List<string>(defeatedTrainers);

        return data;
    }

    public void Deserialize(GameSaveData data)
    {
        PlayerName = data.playerName;
        Money = data.money;
        Badges = (bool[])data.badges.Clone();
        CurrentMapId = data.currentMapId;
        PlayerX = data.playerX;
        PlayerY = data.playerY;
        LastHealPoint = new HealPoint(data.healPoint.mapId, data.healPoint.x, data.healPoint.y);
        accumulatedPlaytime = data.playtime;
        sessionStartTime = Time.realtimeSinceStartup;

        // Party — recalculate stats from save
        Party = new List<Dino>();
        foreach (var dino in data.party)
        {
            Party.Add(Dino.CreateFromSerialized(dino));
        }

        // PC Storage
        PCStorage = new List<Dino>();
        foreach (var dino in data.pcStorage)
        {
            PCStorage.Add(Dino.CreateFromSerialized(dino));
        }

        // Inventory
        Inventory = new InventorySystem();
        Inventory.DeserializeFromList(data.inventory);

        // Dinodex
        dinodex = new Dictionary<int, DinodexEntry>();
        foreach (var entry in data.dinodex)
        {
            dinodex[entry.speciesId] = entry;
        }

        // Flags
        flags = new Dictionary<string, bool>();
        foreach (var entry in data.flags)
        {
            flags[entry.key] = entry.value;
        }

        // Defeated trainers
        defeatedTrainers = new HashSet<string>(data.defeatedTrainers);
    }
}
