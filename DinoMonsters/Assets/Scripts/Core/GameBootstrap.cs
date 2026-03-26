// ============================================================
// Dino Monsters -- Game Bootstrap
// First thing that runs. Creates persistent singletons.
// ============================================================

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        // Create persistent Game root
        var go = new GameObject("[GameSystems]");
        DontDestroyOnLoad(go);

        // Add singletons — order matters: DataLoader first (others may depend on it)
        go.AddComponent<DataLoader>();
        // go.AddComponent<AssetLibrary>();  // DISABLED — using procedural graphics for now
        go.AddComponent<GameManager>();
        go.AddComponent<AudioManager>();
        go.AddComponent<ScreenFade>();
        go.AddComponent<StoryEventSystem>();
        go.AddComponent<DialogueUI>();
        go.AddComponent<InputHelperUpdater>();

        Debug.Log("[Bootstrap] Game systems initialized");
    }
}
