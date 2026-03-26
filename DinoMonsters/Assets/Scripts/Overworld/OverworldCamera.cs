using System.Collections;
using UnityEngine;

/// <summary>
/// Top-down follow camera with orbit control.
/// Follow the player, orbit with right-click + mouse drag or scroll to zoom.
/// </summary>
public class OverworldCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera Settings")]
    public float distance = 8f;
    public float pitch = 50f;        // vertical angle (degrees)
    public float yaw = 0f;           // horizontal orbit angle
    public float smoothSpeed = 10f;

    [Header("Orbit Control")]
    public float mouseSensitivity = 3f;
    public float scrollZoomSpeed = 3f;
    public float minPitch = 20f;
    public float maxPitch = 80f;
    public float minDistance = 3f;
    public float maxDistance = 25f;

    // Internal
    private Vector3 currentVelocity;
    private bool snapped = false;

    // Shake
    private float shakeTimer = 0f;
    private float shakeIntensity = 0f;

    void LateUpdate()
    {
        // Keep looking for player if no target
        if (target == null)
        {
            var pc = FindObjectOfType<PlayerController>();
            if (pc != null) target = pc.transform;
            if (target == null) return;
        }

        // --- Mouse orbit: right-click + drag ---
        if (Input.GetMouseButton(1))
        {
            yaw += GetAxisSafe("Mouse X") * mouseSensitivity;
            pitch -= GetAxisSafe("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        // --- Scroll zoom ---
        float scroll = GetAxisSafe("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            distance -= scroll * scrollZoomSpeed * 5f;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        // --- Calculate camera position from spherical coordinates ---
        float pitchRad = pitch * Mathf.Deg2Rad;
        float yawRad = yaw * Mathf.Deg2Rad;

        float y = distance * Mathf.Sin(pitchRad);
        float hDist = distance * Mathf.Cos(pitchRad);
        float x = -hDist * Mathf.Sin(yawRad);
        float z = -hDist * Mathf.Cos(yawRad);

        Vector3 offset = new Vector3(x, y, z);
        Vector3 desiredPos = target.position + offset;

        // Apply shake
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            float decay = Mathf.Clamp01(shakeTimer / 0.3f);
            desiredPos += new Vector3(
                Random.Range(-1f, 1f) * shakeIntensity * decay,
                Random.Range(-1f, 1f) * shakeIntensity * decay * 0.5f,
                0f);
        }

        // Snap immediately on first frame, then smooth follow
        if (!snapped)
        {
            transform.position = desiredPos;
            snapped = true;
        }
        else
        {
            transform.position = Vector3.SmoothDamp(
                transform.position, desiredPos, ref currentVelocity, 1f / smoothSpeed);
        }

        // Always look at the player
        transform.LookAt(target.position + Vector3.up * 0.5f);
    }

    // =================================================================
    // Public API
    // =================================================================

    public void SnapToTarget()
    {
        snapped = false; // Will snap on next LateUpdate
    }

    public void Shake(float intensity = 0.15f, float duration = 0.3f)
    {
        shakeIntensity = intensity;
        shakeTimer = duration;
    }

    public void BattleEntryShake()
    {
        Shake(0.15f, 0.3f);
    }

    public void ZoomIn(float factor = 0.7f)
    {
        distance *= factor;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
    }

    public void ZoomOut()
    {
        distance = 8f;
    }

    public void ResetOrbit()
    {
        yaw = 0f;
        pitch = 50f;
    }

    public void SetTarget(Transform newTarget, bool snap = false)
    {
        target = newTarget;
        if (snap) snapped = false;
    }

    /// <summary>
    /// Safe wrapper for Input.GetAxis — returns 0 if the axis doesn't exist.
    /// </summary>
    private static float GetAxisSafe(string axisName)
    {
        try { return Input.GetAxis(axisName); }
        catch { return 0f; }
    }
}
