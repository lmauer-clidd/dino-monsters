using UnityEngine;

/// <summary>
/// Stub — asset integration removed. Kept as empty singleton to avoid compile errors.
/// </summary>
public class AssetLibrary : MonoBehaviour
{
    public static AssetLibrary Instance { get; private set; }
    void Awake() { Instance = this; }
    public bool HasTownKit() { return false; }
    public bool HasCharacters() { return false; }
}
