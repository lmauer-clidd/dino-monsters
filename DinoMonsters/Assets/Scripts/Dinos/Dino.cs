// ============================================================
// Dino Monsters -- Dino Entity
// Ported from TypeScript POC (Phaser) to C# (Unity)
// ============================================================
//
// Uses existing DataLoader types (DinoSpeciesData, BaseStatsData, etc.)
// from DataLoader.cs. Enums (DinoType, StatusEffect, XpGroup) and
// Dino-specific data structures (Stats, MoveSlot, Temperament) are
// defined here.
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;

// --------------- Enums ---------------
// DinoType is defined globally in Core/DinoType.cs (no namespace)

public enum StatusEffect
{
    None = 0,
    Poison = 1,
    Burn = 2,
    Paralysis = 3,
    Sleep = 4,
    Freeze = 5
}

public enum XpGroup
{
    Fast,
    Medium,
    Slow
}

// --------------- Data Structures ---------------

[System.Serializable]
public class Stats
{
    public int hp;
    public int attack;
    public int defense;
    public int spAttack;
    public int spDefense;
    public int speed;

    public Stats() { }

    public Stats(int hp, int attack, int defense, int spAttack, int spDefense, int speed)
    {
        this.hp = hp;
        this.attack = attack;
        this.defense = defense;
        this.spAttack = spAttack;
        this.spDefense = spDefense;
        this.speed = speed;
    }

    public Stats Clone()
    {
        return new Stats(hp, attack, defense, spAttack, spDefense, speed);
    }

    public int GetStat(string key)
    {
        switch (key)
        {
            case "hp": return hp;
            case "attack": return attack;
            case "defense": return defense;
            case "spAttack": return spAttack;
            case "spDefense": return spDefense;
            case "speed": return speed;
            default: return 0;
        }
    }

    public void SetStat(string key, int value)
    {
        switch (key)
        {
            case "hp": hp = value; break;
            case "attack": attack = value; break;
            case "defense": defense = value; break;
            case "spAttack": spAttack = value; break;
            case "spDefense": spDefense = value; break;
            case "speed": speed = value; break;
        }
    }

    public int Total => hp + attack + defense + spAttack + spDefense + speed;

    /// <summary>
    /// Create Stats from DataLoader's BaseStatsData (maps atk->attack, def->defense, etc.)
    /// </summary>
    public static Stats FromBaseStatsData(BaseStatsData bsd)
    {
        return new Stats(bsd.hp, bsd.atk, bsd.def, bsd.spatk, bsd.spdef, bsd.speed);
    }
}

[System.Serializable]
public class MoveSlot
{
    public int moveId;
    public int currentPP;
    public int maxPP;

    public MoveSlot() { }

    public MoveSlot(int moveId, int currentPP, int maxPP)
    {
        this.moveId = moveId;
        this.currentPP = currentPP;
        this.maxPP = maxPP;
    }

    public MoveSlot Clone()
    {
        return new MoveSlot(moveId, currentPP, maxPP);
    }
}

[System.Serializable]
public class Temperament
{
    public string name;
    public string plus;  // stat key or null/empty for neutral
    public string minus; // stat key or null/empty for neutral

    public Temperament() { }

    public Temperament(string name, string plus, string minus)
    {
        this.name = name;
        this.plus = plus;
        this.minus = minus;
    }

    public bool IsNeutral => string.IsNullOrEmpty(plus);
}

// --------------- Dino Class ---------------

[System.Serializable]
public class Dino
{
    // --------------- Constants ---------------

    public const int MAX_LEVEL = 50;
    public const int MAX_GV = 31;
    public const int MAX_TP_PER_STAT = 252;
    public const int MAX_TP_TOTAL = 510;
    public const int MAX_FRIENDSHIP = 255;
    public const int MAX_MOVE_SLOTS = 4;

    // --------------- Temperaments ---------------

    public static readonly Temperament[] TEMPERAMENTS = new Temperament[]
    {
        // Neutral
        new Temperament("Hardy",   null, null),
        new Temperament("Docile",  null, null),
        new Temperament("Serious", null, null),
        new Temperament("Bashful", null, null),
        new Temperament("Quirky",  null, null),
        // +Attack
        new Temperament("Lonely",  "attack",    "defense"),
        new Temperament("Brave",   "attack",    "speed"),
        new Temperament("Adamant", "attack",    "spAttack"),
        new Temperament("Naughty", "attack",    "spDefense"),
        // +Defense
        new Temperament("Bold",    "defense",   "attack"),
        new Temperament("Relaxed", "defense",   "speed"),
        new Temperament("Impish",  "defense",   "spAttack"),
        new Temperament("Lax",     "defense",   "spDefense"),
        // +SpAttack
        new Temperament("Modest",  "spAttack",  "attack"),
        new Temperament("Mild",    "spAttack",  "defense"),
        new Temperament("Quiet",   "spAttack",  "speed"),
        new Temperament("Rash",    "spAttack",  "spDefense"),
        // +SpDefense
        new Temperament("Calm",    "spDefense", "attack"),
        new Temperament("Gentle",  "spDefense", "defense"),
        new Temperament("Sassy",   "spDefense", "speed"),
        new Temperament("Careful", "spDefense", "spAttack"),
        // +Speed
        new Temperament("Timid",   "speed",     "attack"),
        new Temperament("Hasty",   "speed",     "defense"),
        new Temperament("Jolly",   "speed",     "spAttack"),
        new Temperament("Naive",   "speed",     "spDefense"),
    };

    // --------------- Serialized Fields ---------------

    public string uid;
    public int speciesId;
    public string nickname;
    public int level;
    public int xp;
    public int currentHp;
    public int maxHp;
    public Stats stats;
    public Stats gv;
    public Stats tp;
    public int temperamentIndex;
    public List<MoveSlot> moves;
    public StatusEffect status;
    public int friendship;

    // --------------- Non-Serialized ---------------

    private static int uidCounter = 0;

    // --------------- Accessors (bridge to DataLoader types) ---------------

    /// <summary>Get species data from DataLoader.</summary>
    public DinoSpeciesData SpeciesData => DataLoader.Instance.GetSpecies(speciesId);

    /// <summary>Get species name.</summary>
    public string SpeciesName => SpeciesData?.name ?? "???";

    /// <summary>Get XP group parsed from species data string.</summary>
    public XpGroup XpGroupValue
    {
        get
        {
            var sp = SpeciesData;
            if (sp == null) return XpGroup.Medium;
            switch (sp.xpGroup)
            {
                case "fast": return XpGroup.Fast;
                case "slow": return XpGroup.Slow;
                default: return XpGroup.Medium;
            }
        }
    }

    /// <summary>Get primary type (first entry in species types array).</summary>
    public DinoType Type1
    {
        get
        {
            var sp = SpeciesData;
            if (sp == null || sp.types == null || sp.types.Length == 0) return DinoType.Normal;
            return (DinoType)sp.types[0];
        }
    }

    /// <summary>Get secondary type if present.</summary>
    public DinoType? Type2
    {
        get
        {
            var sp = SpeciesData;
            if (sp == null || sp.types == null || sp.types.Length < 2) return null;
            if (sp.types[1] == sp.types[0]) return null; // same type = single type
            return (DinoType)sp.types[1];
        }
    }

    public Temperament TemperamentData => TEMPERAMENTS[temperamentIndex];

    // --------------- Factory Methods ---------------

    public static Dino CreateWild(int speciesId, int level)
    {
        var gv = new Stats(
            RandomGV(), RandomGV(), RandomGV(),
            RandomGV(), RandomGV(), RandomGV()
        );
        var tp = new Stats();
        int temperamentIndex = UnityEngine.Random.Range(0, TEMPERAMENTS.Length);
        return CreateInternal(speciesId, level, gv, tp, temperamentIndex, null);
    }

    public static Dino CreateStarter(int speciesId, int level)
    {
        var gv = new Stats(
            RandomGoodGV(), RandomGoodGV(), RandomGoodGV(),
            RandomGoodGV(), RandomGoodGV(), RandomGoodGV()
        );
        var tp = new Stats();

        // Starters get a beneficial temperament (non-neutral)
        var nonNeutral = new List<int>();
        for (int i = 0; i < TEMPERAMENTS.Length; i++)
        {
            if (!TEMPERAMENTS[i].IsNeutral) nonNeutral.Add(i);
        }
        int temperamentIndex = nonNeutral[UnityEngine.Random.Range(0, nonNeutral.Count)];
        return CreateInternal(speciesId, level, gv, tp, temperamentIndex, null);
    }

    /// <summary>
    /// Restore a dino from deserialized save data (recalculates stats).
    /// </summary>
    public static Dino CreateFromSerialized(Dino data)
    {
        data.CalculateStats();
        if (data.currentHp > data.maxHp) data.currentHp = data.maxHp;
        return data;
    }

    private static Dino CreateInternal(int speciesId, int level, Stats gv, Stats tp, int temperamentIndex, string nickname)
    {
        var dino = new Dino();
        dino.uid = GenerateUid();
        dino.speciesId = speciesId;
        dino.level = Mathf.Min(level, MAX_LEVEL);

        var sp = DataLoader.Instance.GetSpecies(speciesId);
        XpGroup xpg = ParseXpGroup(sp?.xpGroup);
        dino.xp = GetTotalXpForLevel(dino.level, xpg);
        dino.gv = gv;
        dino.tp = tp;
        dino.temperamentIndex = temperamentIndex;
        dino.moves = new List<MoveSlot>();
        dino.status = StatusEffect.None;
        dino.friendship = 70;
        dino.nickname = nickname ?? sp?.name ?? "Dino";
        dino.stats = new Stats();
        dino.maxHp = 0;
        dino.currentHp = 0;

        dino.CalculateStats();
        dino.currentHp = dino.maxHp;
        dino.PopulateMovesForLevel();

        return dino;
    }

    // --------------- Stat Calculation ---------------

    /// <summary>
    /// Recalculate all stats from base stats, GV, TP, level and temperament.
    /// Uses Pokemon formula: ((2*base + gv + tp/4) * level) / 100 + 5  (HP gets + level + 10)
    /// </summary>
    public void CalculateStats()
    {
        var sp = SpeciesData;
        if (sp == null || sp.baseStats == null) return;

        // Bridge: convert DataLoader's BaseStatsData to our Stats
        var baseStats = Stats.FromBaseStatsData(sp.baseStats);
        int lvl = level;

        // HP = floor(((2 * base + gv + floor(tp/4)) * level / 100) + level + 10)
        stats.hp = Mathf.FloorToInt(
            ((2 * baseStats.hp + gv.hp + Mathf.FloorToInt(tp.hp / 4f)) * lvl) / 100f + lvl + 10
        );
        maxHp = stats.hp;

        // Other stats
        string[] statKeys = { "attack", "defense", "spAttack", "spDefense", "speed" };
        foreach (string key in statKeys)
        {
            int baseStat = baseStats.GetStat(key);
            int gvStat = gv.GetStat(key);
            int tpStat = tp.GetStat(key);

            int raw = Mathf.FloorToInt(
                ((2 * baseStat + gvStat + Mathf.FloorToInt(tpStat / 4f)) * lvl) / 100f + 5
            );
            float mod = GetTemperamentModifier(key);
            stats.SetStat(key, Mathf.Max(1, Mathf.FloorToInt(raw * mod)));
        }
    }

    private float GetTemperamentModifier(string stat)
    {
        var temp = TemperamentData;
        if (temp.plus == stat) return 1.1f;
        if (temp.minus == stat) return 0.9f;
        return 1.0f;
    }

    // --------------- XP / Leveling ---------------

    public static int GetXpForLevel(int level, XpGroup group)
    {
        if (level <= 1) return 0;
        int l3 = level * level * level;
        switch (group)
        {
            case XpGroup.Fast:   return Mathf.FloorToInt(0.8f * l3);
            case XpGroup.Medium: return l3;
            case XpGroup.Slow:   return Mathf.FloorToInt(1.25f * l3);
            default: return l3;
        }
    }

    public static int GetTotalXpForLevel(int level, XpGroup group)
    {
        return GetXpForLevel(level, group);
    }

    public int GetXpForNextLevel()
    {
        if (level >= MAX_LEVEL) return 0;
        return GetXpForLevel(level + 1, XpGroupValue);
    }

    public int GetXpProgress()
    {
        int currentLevelXp = GetXpForLevel(level, XpGroupValue);
        int nextLevelXp = GetXpForNextLevel();
        if (nextLevelXp <= currentLevelXp) return 0;
        return xp - currentLevelXp;
    }

    public int GetXpToNextLevel()
    {
        int currentLevelXp = GetXpForLevel(level, XpGroupValue);
        int nextLevelXp = GetXpForNextLevel();
        return nextLevelXp - currentLevelXp;
    }

    /// <summary>
    /// Result of a single level-up event.
    /// </summary>
    public class LevelUpResult
    {
        public int newLevel;
        public List<int> movesLearned = new List<int>();   // auto-learned (had free slots)
        public List<int> movesPending = new List<int>();   // could not auto-learn (slots full)
        public bool canEvolve;
    }

    /// <summary>
    /// Gain XP and handle level-ups.
    /// Returns levels gained, list of move IDs learned, and list of move IDs pending (slots full).
    /// </summary>
    public (int levelsGained, List<int> movesLearned, List<int> movesPending) GainXp(int amount)
    {
        if (level >= MAX_LEVEL) return (0, new List<int>(), new List<int>());

        xp += amount;
        int levelsGained = 0;
        var movesLearned = new List<int>();
        var movesPending = new List<int>();

        while (level < MAX_LEVEL)
        {
            int needed = GetXpForNextLevel();
            if (xp < needed) break;

            level += 1;
            levelsGained += 1;

            int oldMaxHp = maxHp;
            CalculateStats();

            // Scale current HP proportionally
            currentHp = Mathf.Min(maxHp, currentHp + (maxHp - oldMaxHp));

            // Check for new moves at this level
            var sp = SpeciesData;
            if (sp?.learnset != null)
            {
                foreach (var entry in sp.learnset)
                {
                    if (entry.level == level)
                    {
                        if (TryLearnMove(entry.moveId))
                            movesLearned.Add(entry.moveId);
                        else if (!HasMove(entry.moveId))
                            movesPending.Add(entry.moveId);
                    }
                }
            }
        }

        // Clamp XP at max level
        if (level >= MAX_LEVEL)
        {
            xp = GetXpForLevel(MAX_LEVEL, XpGroupValue);
        }

        return (levelsGained, movesLearned, movesPending);
    }

    /// <summary>
    /// Add XP without processing level-ups. Returns true if ready to level up.
    /// Used for animated XP gain where level-ups are handled one at a time.
    /// </summary>
    public bool AddXpRaw(int amount)
    {
        if (level >= MAX_LEVEL) return false;
        xp += amount;
        return xp >= GetXpForNextLevel();
    }

    /// <summary>
    /// Process a single level-up. Call only when XP >= next level threshold.
    /// Returns details about the level-up event.
    /// </summary>
    public LevelUpResult ProcessSingleLevelUp()
    {
        if (level >= MAX_LEVEL) return null;
        int needed = GetXpForNextLevel();
        if (xp < needed) return null;

        level += 1;
        int oldMaxHp = maxHp;
        CalculateStats();
        currentHp = Mathf.Min(maxHp, currentHp + (maxHp - oldMaxHp));

        var result = new LevelUpResult { newLevel = level };

        // Check for new moves at this level
        var sp = SpeciesData;
        if (sp?.learnset != null)
        {
            foreach (var entry in sp.learnset)
            {
                if (entry.level == level)
                {
                    if (TryLearnMove(entry.moveId))
                        result.movesLearned.Add(entry.moveId);
                    else if (!HasMove(entry.moveId))
                        result.movesPending.Add(entry.moveId);
                }
            }
        }

        // Check evolution eligibility
        result.canEvolve = CanEvolve();

        // Clamp XP at max level
        if (level >= MAX_LEVEL)
            xp = GetXpForLevel(MAX_LEVEL, XpGroupValue);

        return result;
    }

    /// <summary>Check if the dino already knows a move.</summary>
    public bool HasMove(int moveId)
    {
        foreach (var m in moves)
        {
            if (m.moveId == moveId) return true;
        }
        return false;
    }

    /// <summary>
    /// Try to learn a move. Returns true if auto-learned, false if slots full.
    /// </summary>
    public bool TryLearnMove(int moveId)
    {
        // Don't learn duplicates
        foreach (var m in moves)
        {
            if (m.moveId == moveId) return false;
        }

        if (moves.Count < MAX_MOVE_SLOTS)
        {
            // Get real PP from DataLoader if available
            var moveData = DataLoader.Instance.GetMove(moveId);
            int pp = moveData != null ? moveData.pp : 20;
            moves.Add(new MoveSlot(moveId, pp, pp));
            return true;
        }
        return false; // Full — UI must ask player which move to replace
    }

    /// <summary>
    /// Replace a move at a given slot index.
    /// </summary>
    public void ReplaceMove(int slotIndex, int moveId, int maxPP = -1)
    {
        if (slotIndex < 0 || slotIndex >= moves.Count) return;
        if (maxPP < 0)
        {
            var moveData = DataLoader.Instance.GetMove(moveId);
            maxPP = moveData != null ? moveData.pp : 20;
        }
        moves[slotIndex] = new MoveSlot(moveId, maxPP, maxPP);
    }

    /// <summary>
    /// Populate initial moves from learnset up to current level.
    /// Before level 7: max 2 moves (Normal-type only).
    /// At level 7+: up to 4 moves, typed attacks unlocked.
    /// </summary>
    private void PopulateMovesForLevel()
    {
        var sp = SpeciesData;
        if (sp?.learnset == null) return;

        int maxSlots = level < 7 ? 2 : MAX_MOVE_SLOTS;

        // Get all moves learnable at or below current level, sorted by level desc
        var available = new List<LearnsetEntry>();
        foreach (var e in sp.learnset)
        {
            if (e.level <= level) available.Add(e);
        }
        available.Sort((a, b) => b.level.CompareTo(a.level));

        foreach (var entry in available)
        {
            if (moves.Count >= maxSlots) break;

            // Check duplicate
            bool hasDupe = false;
            foreach (var m in moves)
            {
                if (m.moveId == entry.moveId) { hasDupe = true; break; }
            }
            if (hasDupe) continue;

            // Before level 7, skip non-Normal typed attacks
            if (level < 7)
            {
                var moveData = DataLoader.Instance.GetMove(entry.moveId);
                if (moveData != null && moveData.type != (int)DinoType.Normal) continue;
            }

            var md = DataLoader.Instance.GetMove(entry.moveId);
            int pp = md != null ? md.pp : 20;
            moves.Add(new MoveSlot(entry.moveId, pp, pp));
        }
    }

    // --------------- Evolution ---------------

    public bool CanEvolve()
    {
        var sp = SpeciesData;
        if (sp?.evolution == null) return false;
        if (sp.evolution.level <= 0) return false; // no evolution data
        if (level < sp.evolution.level) return false;
        // Additional conditions could be checked here
        return true;
    }

    public bool Evolve()
    {
        if (!CanEvolve()) return false;
        var evo = SpeciesData.evolution;
        speciesId = evo.to; // DataLoader uses "to" field

        int oldMaxHp = maxHp;
        CalculateStats();
        currentHp += maxHp - oldMaxHp;
        if (currentHp > maxHp) currentHp = maxHp;

        return true;
    }

    // --------------- HP / Status ---------------

    public int TakeDamage(int amount)
    {
        int actual = Mathf.Min(currentHp, Mathf.Max(1, Mathf.FloorToInt(amount)));
        currentHp = Mathf.Max(0, currentHp - actual);
        return actual;
    }

    public int Heal(int amount)
    {
        int missing = maxHp - currentHp;
        int actual = Mathf.Min(missing, Mathf.Max(0, Mathf.FloorToInt(amount)));
        currentHp += actual;
        return actual;
    }

    public void FullHeal()
    {
        currentHp = maxHp;
        status = StatusEffect.None;
        foreach (var move in moves)
        {
            move.currentPP = move.maxPP;
        }
    }

    public bool IsFainted()
    {
        return currentHp <= 0;
    }

    public bool SetStatus(StatusEffect effect)
    {
        if (status != StatusEffect.None && effect != StatusEffect.None) return false;
        status = effect;
        return true;
    }

    public void CureStatus()
    {
        status = StatusEffect.None;
    }

    // --------------- Friendship ---------------

    public void AddFriendship(int amount)
    {
        friendship = Mathf.Clamp(friendship + amount, 0, MAX_FRIENDSHIP);
    }

    // --------------- Training Points ---------------

    public int AddTP(string stat, int amount)
    {
        int currentTotal = tp.Total;
        int roomTotal = MAX_TP_TOTAL - currentTotal;
        int roomStat = MAX_TP_PER_STAT - tp.GetStat(stat);
        int actual = Mathf.Min(amount, Mathf.Min(roomTotal, roomStat));
        if (actual > 0)
        {
            tp.SetStat(stat, tp.GetStat(stat) + actual);
            CalculateStats();
        }
        return actual;
    }

    // --------------- Display Helpers ---------------

    public float GetHpPercent()
    {
        if (maxHp == 0) return 0f;
        return (float)currentHp / maxHp;
    }

    public float GetXpPercent()
    {
        int toNext = GetXpToNextLevel();
        if (toNext <= 0) return 1f;
        return (float)GetXpProgress() / toNext;
    }

    public override string ToString()
    {
        return $"{nickname} (Lv.{level} {SpeciesName}) HP:{currentHp}/{maxHp}";
    }

    // --------------- Helpers ---------------

    private static string GenerateUid()
    {
        uidCounter++;
        return $"dino_{DateTime.Now.Ticks}_{uidCounter}";
    }

    private static int RandomGV()
    {
        return UnityEngine.Random.Range(0, MAX_GV + 1);
    }

    private static int RandomGoodGV()
    {
        return UnityEngine.Random.Range(20, MAX_GV + 1);
    }

    public static XpGroup ParseXpGroup(string group)
    {
        switch (group)
        {
            case "fast": return XpGroup.Fast;
            case "slow": return XpGroup.Slow;
            default: return XpGroup.Medium;
        }
    }
}
