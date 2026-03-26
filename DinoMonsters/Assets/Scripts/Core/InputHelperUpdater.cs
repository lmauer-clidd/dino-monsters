// ============================================================
// Dino Monsters -- InputHelper Frame Updater
// Attach to a persistent GameObject (e.g. GameBootstrap).
// Calls InputHelper.LateFrameUpdate() each frame to track
// previous axis state for D-pad/stick "just pressed" detection.
// ============================================================

using UnityEngine;

public class InputHelperUpdater : MonoBehaviour
{
    private static InputHelperUpdater instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void LateUpdate()
    {
        InputHelper.LateFrameUpdate();
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
