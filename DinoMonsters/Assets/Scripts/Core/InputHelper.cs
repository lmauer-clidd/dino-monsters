// ============================================================
// Dino Monsters -- Input Helper (Static Utility)
// Abstracts input across keyboard, mouse and gamepad.
// Reads: Horizontal/Vertical (keyboard+stick), DPadH/DPadV (d-pad)
// ============================================================

using UnityEngine;

public static class InputHelper
{
    // Previous frame combined axis values for edge detection
    private static float prevH = 0f;
    private static float prevV = 0f;
    private static float currH = 0f;
    private static float currV = 0f;
    private static bool axesRead = false;

    private const float DEADZONE = 0.4f;

    // =================================================================
    // Frame update — called by InputHelperUpdater each LateUpdate
    // =================================================================

    public static void LateFrameUpdate()
    {
        prevH = currH;
        prevV = currV;
        axesRead = false;
    }

    private static float SafeAxis(string name)
    {
        try { return Input.GetAxisRaw(name); }
        catch { return 0f; }
    }

    private static void ReadAxes()
    {
        if (axesRead) return;

        // Combine stick (Horizontal/Vertical) and D-pad (DPadH/DPadV)
        // Take whichever has the larger absolute value
        float stickH = SafeAxis("Horizontal");
        float stickV = SafeAxis("Vertical");
        float dpadH = SafeAxis("DPadH");
        float dpadV = SafeAxis("DPadV");

        currH = Mathf.Abs(dpadH) > Mathf.Abs(stickH) ? dpadH : stickH;
        currV = Mathf.Abs(dpadV) > Mathf.Abs(stickV) ? dpadV : stickV;

        axesRead = true;
    }

    // =================================================================
    // Single-press directions (for menu navigation)
    // Detects transition from neutral to pressed
    // =================================================================

    public static bool Up
    {
        get
        {
            ReadAxes();
            return Input.GetKeyDown(KeyCode.UpArrow) ||
                   Input.GetKeyDown(KeyCode.W) ||
                   (currV > DEADZONE && prevV <= DEADZONE);
        }
    }

    public static bool Down
    {
        get
        {
            ReadAxes();
            return Input.GetKeyDown(KeyCode.DownArrow) ||
                   Input.GetKeyDown(KeyCode.S) ||
                   (currV < -DEADZONE && prevV >= -DEADZONE);
        }
    }

    public static bool Left
    {
        get
        {
            ReadAxes();
            return Input.GetKeyDown(KeyCode.LeftArrow) ||
                   Input.GetKeyDown(KeyCode.A) ||
                   (currH < -DEADZONE && prevH >= -DEADZONE);
        }
    }

    public static bool Right
    {
        get
        {
            ReadAxes();
            return Input.GetKeyDown(KeyCode.RightArrow) ||
                   Input.GetKeyDown(KeyCode.D) ||
                   (currH > DEADZONE && prevH <= DEADZONE);
        }
    }

    // =================================================================
    // Held directions (for overworld movement)
    // =================================================================

    public static bool UpHeld
    {
        get
        {
            ReadAxes();
            return Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) || currV > DEADZONE;
        }
    }

    public static bool DownHeld
    {
        get
        {
            ReadAxes();
            return Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S) || currV < -DEADZONE;
        }
    }

    public static bool LeftHeld
    {
        get
        {
            ReadAxes();
            return Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) || currH < -DEADZONE;
        }
    }

    public static bool RightHeld
    {
        get
        {
            ReadAxes();
            return Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) || currH > DEADZONE;
        }
    }

    // =================================================================
    // Actions
    // =================================================================

    public static bool Confirm =>
        Input.GetKeyDown(KeyCode.Return) ||
        Input.GetKeyDown(KeyCode.Space) ||
        Input.GetKeyDown(KeyCode.JoystickButton0); // A (Xbox) / Cross (PS)

    public static bool Cancel =>
        Input.GetKeyDown(KeyCode.Escape) ||
        Input.GetKeyDown(KeyCode.JoystickButton1); // B (Xbox) / Circle (PS)

    public static bool Pause =>
        Input.GetKeyDown(KeyCode.Escape) ||
        Input.GetKeyDown(KeyCode.JoystickButton7); // Start (Xbox) / Options (PS)

    public static bool Interact =>
        Input.GetKeyDown(KeyCode.Return) ||
        Input.GetKeyDown(KeyCode.E) ||
        Input.GetKeyDown(KeyCode.JoystickButton0);

    public static bool RunHeld =>
        Input.GetKey(KeyCode.LeftShift) ||
        Input.GetKey(KeyCode.RightShift);
}
