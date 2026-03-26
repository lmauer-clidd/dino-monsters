using UnityEngine;
using System.IO;
using System.Collections.Generic;

// =============================================================================
// JSON data classes — field names must match the JSON keys exactly
// =============================================================================

[System.Serializable]
public class DinoSpeciesData
{
    public int id;
    public string name;
    public int[] types;
    public BaseStatsData baseStats;
    public int captureRate;
    public string xpGroup;   // "slow", "medium", "fast"
    public int xpYield;
    public EvolutionData evolution;
    public LearnsetEntry[] learnset;
    public string[] abilities;
    public float height;
    public float weight;
    public string description;
    public string habitat;
}

[System.Serializable]
public class BaseStatsData
{
    public int hp;
    public int atk;
    public int def;
    public int spatk;
    public int spdef;
    public int speed;
}

[System.Serializable]
public class LearnsetEntry
{
    public int level;
    public int moveId;
}

[System.Serializable]
public class EvolutionData
{
    public int to;
    public int level;
}

[System.Serializable]
public class MoveData
{
    public int id;
    public string name;
    public int type;
    public string category; // "physical", "special", "status"
    public int power;
    public int accuracy;
    public int pp;
    public int priority;
    public string effect;
    public int effectChance;
    public string description;
}

[System.Serializable]
public class ItemData
{
    public int id;
    public string name;
    public string category;
    public int price;
    public string effect;
    public float value;
    public string description;
    public bool usableInBattle;
    public bool usableInField;
}

[System.Serializable]
public class TypeChartRow
{
    public float[] values;
}

[System.Serializable]
public class TypeChartData
{
    public string[] types;
    public TypeChartRow[] chart;
}

// Wrapper classes for JSON array deserialization (Unity JsonUtility limitation)
[System.Serializable] public class DinoArray { public DinoSpeciesData[] items; }
[System.Serializable] public class MoveArray { public MoveData[] items; }
[System.Serializable] public class ItemArray { public ItemData[] items; }

// =============================================================================
// DataLoader — Singleton that loads all JSON data at game start
// =============================================================================

public class DataLoader : MonoBehaviour
{
    public static DataLoader Instance { get; private set; }

    public Dictionary<int, DinoSpeciesData> Dinos { get; private set; } = new Dictionary<int, DinoSpeciesData>();
    public Dictionary<int, MoveData> Moves { get; private set; } = new Dictionary<int, MoveData>();
    public Dictionary<int, ItemData> Items { get; private set; } = new Dictionary<int, ItemData>();
    public float[,] TypeChart { get; private set; }
    public string[] TypeNames { get; private set; }

    // -------------------------------------------------------------------------
    // JSON type index -> Game DinoType enum mapping
    // The POC JSON uses a different type ordering than the game enum.
    //   JSON index:  0=Fossil, 1=Water, 2=Fire, 3=Flora, 4=Ice, 5=Air,
    //                6=Earth, 7=Electric, 8=Venom, 9=Metal, 10=Shadow,
    //                11=Light, 12=Normal, 13=Primal
    //   Game enum:   0=Normal, 1=Fire, 2=Water, 3=Earth, 4=Air, 5=Electric,
    //                6=Ice, 7=Venom, 8=Flora, 9=Fossil, 10=Shadow,
    //                11=Light, 12=Metal, 13=Primal
    // -------------------------------------------------------------------------
    private static readonly int[] JSON_TO_GAME_TYPE = {
        9,  // 0: Fossil  -> 9
        2,  // 1: Water   -> 2
        1,  // 2: Fire    -> 1
        8,  // 3: Flora   -> 8
        6,  // 4: Ice     -> 6
        4,  // 5: Air     -> 4
        3,  // 6: Earth   -> 3
        5,  // 7: Electric-> 5
        7,  // 8: Venom   -> 7
        12, // 9: Metal   -> 12
        10, // 10: Shadow -> 10
        11, // 11: Light  -> 11
        0,  // 12: Normal -> 0
        13  // 13: Primal -> 13
    };

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadAll();
    }

    // -------------------------------------------------------------------------
    // Load all data files from StreamingAssets
    // -------------------------------------------------------------------------
    private void LoadAll()
    {
        LoadDinos();
        LoadMoves();
        LoadItems();
        LoadTypeChart();
        Debug.Log($"[DataLoader] Loaded {Dinos.Count} dinos, {Moves.Count} moves, {Items.Count} items, {TypeNames.Length} types");
    }

    private string LoadJson(string filename)
    {
        string path = Path.Combine(Application.streamingAssetsPath, filename);
        return File.ReadAllText(path);
    }

    private void LoadDinos()
    {
        string json = LoadJson("dinos.json");
        // Unity JsonUtility cannot deserialize top-level arrays, so wrap it
        var array = JsonUtility.FromJson<DinoArray>("{\"items\":" + json + "}");
        foreach (var d in array.items)
        {
            // Map JSON type indices to game DinoType enum values
            for (int i = 0; i < d.types.Length; i++)
                d.types[i] = MapType(d.types[i]);
            Dinos[d.id] = d;
        }
    }

    private void LoadMoves()
    {
        string json = LoadJson("moves.json");
        var array = JsonUtility.FromJson<MoveArray>("{\"items\":" + json + "}");
        foreach (var m in array.items)
        {
            m.type = MapType(m.type);
            Moves[m.id] = m;
        }
    }

    private void LoadItems()
    {
        string json = LoadJson("items.json");
        var array = JsonUtility.FromJson<ItemArray>("{\"items\":" + json + "}");
        foreach (var item in array.items)
            Items[item.id] = item;
    }

    private void LoadTypeChart()
    {
        string json = LoadJson("type_chart.json");
        // JsonUtility cannot deserialize jagged arrays (float[][]) directly.
        // The JSON has { "types": [...], "chart": [[...], ...] }.
        // We pre-process the JSON to wrap each row: "chart":[{"values":[...]}, ...]
        json = PreProcessTypeChartJson(json);
        var data = JsonUtility.FromJson<TypeChartData>(json);
        TypeNames = data.types;
        int size = data.chart.Length;
        TypeChart = new float[size, size];
        for (int i = 0; i < size; i++)
            for (int j = 0; j < data.chart[i].values.Length; j++)
                TypeChart[i, j] = data.chart[i].values[j];
    }

    /// <summary>
    /// Pre-process the type chart JSON to wrap each row array into {"values": [...]}.
    /// Converts "chart":[[1,2],[3,4]] to "chart":[{"values":[1,2]},{"values":[3,4]}]
    /// </summary>
    private string PreProcessTypeChartJson(string json)
    {
        // Find the "chart" array and wrap each sub-array
        int chartIdx = json.IndexOf("\"chart\"");
        if (chartIdx < 0) return json;

        int colonIdx = json.IndexOf(':', chartIdx);
        if (colonIdx < 0) return json;

        // Find the opening '[' of the chart array
        int outerStart = json.IndexOf('[', colonIdx);
        if (outerStart < 0) return json;

        // Parse and rebuild: find each inner array and wrap it
        var sb = new System.Text.StringBuilder();
        sb.Append(json, 0, outerStart + 1); // everything up to and including outer '['

        int pos = outerStart + 1;
        bool first = true;
        while (pos < json.Length)
        {
            // Skip whitespace
            while (pos < json.Length && (json[pos] == ' ' || json[pos] == '\n' || json[pos] == '\r' || json[pos] == '\t' || json[pos] == ','))
                pos++;

            if (pos >= json.Length || json[pos] == ']') break;

            if (json[pos] == '[')
            {
                // Find matching ']'
                int innerStart = pos;
                int depth = 1;
                pos++;
                while (pos < json.Length && depth > 0)
                {
                    if (json[pos] == '[') depth++;
                    else if (json[pos] == ']') depth--;
                    pos++;
                }
                string innerArray = json.Substring(innerStart, pos - innerStart);

                if (!first) sb.Append(',');
                sb.Append("{\"values\":");
                sb.Append(innerArray);
                sb.Append('}');
                first = false;
            }
            else
            {
                pos++;
            }
        }

        // Find the closing ']' of the outer chart array
        while (pos < json.Length && json[pos] != ']') pos++;
        if (pos < json.Length)
        {
            sb.Append(']');
            pos++; // skip the ']'
        }

        // Append the rest of the JSON
        sb.Append(json, pos, json.Length - pos);
        return sb.ToString();
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>Map a JSON type index to the game's DinoType enum value.</summary>
    public static int MapType(int jsonType)
    {
        if (jsonType < 0 || jsonType >= JSON_TO_GAME_TYPE.Length) return 0;
        return JSON_TO_GAME_TYPE[jsonType];
    }

    /// <summary>Get type effectiveness multiplier (attacker type vs defender type).</summary>
    public float GetEffectiveness(int attackType, int defenseType)
    {
        if (TypeChart == null) return 1f;
        if (attackType < 0 || attackType >= TypeChart.GetLength(0)) return 1f;
        if (defenseType < 0 || defenseType >= TypeChart.GetLength(1)) return 1f;
        return TypeChart[attackType, defenseType];
    }

    public DinoSpeciesData GetSpecies(int id) => Dinos.TryGetValue(id, out var d) ? d : null;
    public MoveData GetMove(int id) => Moves.TryGetValue(id, out var m) ? m : null;
    public ItemData GetItem(int id) => Items.TryGetValue(id, out var item) ? item : null;
}
