#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public class ProjectSetup
{
    [MenuItem("Dino Monsters/Validate Project")]
    public static void ValidateProject()
    {
        // Check StreamingAssets data files exist
        string[] required = { "dinos.json", "moves.json", "type_chart.json", "items.json" };
        int found = 0;
        foreach (var file in required)
        {
            string path = Path.Combine(Application.streamingAssetsPath, file);
            if (File.Exists(path)) { found++; Debug.Log($"[OK] {file}"); }
            else Debug.LogError($"[MISSING] {file} — expected at {path}");
        }
        Debug.Log($"Project validation: {found}/{required.Length} data files found");
    }
}
#endif
