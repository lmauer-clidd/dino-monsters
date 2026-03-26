// ============================================================
// Dino Monsters -- Save System (Static Utility)
// ============================================================

using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private const string SAVE_FILENAME = "save.json";

    public static void Save(GameState state)
    {
        GameSaveData data = state.Serialize();
        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine(Application.persistentDataPath, SAVE_FILENAME);
        File.WriteAllText(path, json);
        Debug.Log($"[SaveSystem] Game saved to {path}");
    }

    public static GameState Load()
    {
        string path = Path.Combine(Application.persistentDataPath, SAVE_FILENAME);
        if (!File.Exists(path)) return null;

        string json = File.ReadAllText(path);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

        if (data == null) return null;

        GameState.Reset();
        var state = GameState.Instance;
        state.Deserialize(data);

        Debug.Log($"[SaveSystem] Game loaded from {path}");
        return state;
    }

    public static bool HasSave()
    {
        return File.Exists(Path.Combine(Application.persistentDataPath, SAVE_FILENAME));
    }

    public static void DeleteSave()
    {
        string path = Path.Combine(Application.persistentDataPath, SAVE_FILENAME);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[SaveSystem] Save deleted at {path}");
        }
    }
}
