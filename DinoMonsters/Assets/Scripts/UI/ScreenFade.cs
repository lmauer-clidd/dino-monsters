// ============================================================
// Dino Monsters -- Screen Fade Singleton
// ============================================================
//
// Full-screen overlay for scene transitions. Creates its own
// Canvas + Image at runtime. Lives on [GameSystems] via
// DontDestroyOnLoad.
// ============================================================

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFade : MonoBehaviour
{
    public static ScreenFade Instance { get; private set; }

    private Canvas canvas;
    private Image fadeImage;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        if (Instance != null) { Destroy(this); return; }
        Instance = this;

        CreateOverlay();
    }

    private void CreateOverlay()
    {
        // Create canvas GameObject as child
        var canvasGO = new GameObject("ScreenFadeCanvas");
        canvasGO.transform.SetParent(transform);

        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // Above everything

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGO.AddComponent<GraphicRaycaster>();

        // Create full-screen black image
        var imageGO = new GameObject("FadeImage");
        imageGO.transform.SetParent(canvasGO.transform, false);

        fadeImage = imageGO.AddComponent<Image>();
        fadeImage.color = new Color(0f, 0f, 0f, 0f); // Start transparent
        fadeImage.raycastTarget = false;

        // Stretch to fill entire screen
        var rect = fadeImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Start with canvas disabled so it doesn't block anything
        canvas.enabled = false;
    }

    // ===============================================================
    // Public API — Callback style
    // ===============================================================

    /// <summary>
    /// Fade to black (alpha 0 -> 1) over duration seconds.
    /// </summary>
    public void FadeOut(float duration, Action onComplete)
    {
        StartCoroutine(FadeRoutine(0f, 1f, duration, onComplete));
    }

    /// <summary>
    /// Fade from black to clear (alpha 1 -> 0) over duration seconds.
    /// </summary>
    public void FadeIn(float duration, Action onComplete = null)
    {
        StartCoroutine(FadeRoutine(1f, 0f, duration, onComplete));
    }

    // ===============================================================
    // Public API — Coroutine style
    // ===============================================================

    /// <summary>
    /// Coroutine: Fade to black (alpha 0 -> 1).
    /// </summary>
    public IEnumerator FadeOutCoroutine(float duration)
    {
        yield return FadeRoutine(0f, 1f, duration, null);
    }

    /// <summary>
    /// Coroutine: Fade from black to clear (alpha 1 -> 0).
    /// </summary>
    public IEnumerator FadeInCoroutine(float duration)
    {
        yield return FadeRoutine(1f, 0f, duration, null);
    }

    // ===============================================================
    // Wild Encounter Flash — rapid white flashes then fade to black
    // ===============================================================

    /// <summary>
    /// Flash white 3 times rapidly, then fade to black.
    /// Used for wild encounter transitions.
    /// </summary>
    public IEnumerator WildEncounterFlash()
    {
        canvas.enabled = true;
        fadeImage.raycastTarget = true;

        // Flash white 3 times
        for (int i = 0; i < 3; i++)
        {
            // Flash ON — white
            fadeImage.color = new Color(1f, 1f, 1f, 0.9f);
            yield return new WaitForSeconds(0.08f);

            // Flash OFF — transparent
            fadeImage.color = new Color(1f, 1f, 1f, 0f);
            yield return new WaitForSeconds(0.06f);
        }

        // Now fade to black
        fadeImage.color = new Color(0f, 0f, 0f, 0f);
        yield return FadeRoutine(0f, 1f, 0.3f, null);
    }

    // ===============================================================
    // Internal
    // ===============================================================

    private IEnumerator FadeRoutine(float fromAlpha, float toAlpha, float duration, Action onComplete)
    {
        canvas.enabled = true;
        fadeImage.raycastTarget = true; // Block input during fade

        // Always use black for standard fades (reset any white from encounter flash)
        Color color = new Color(0f, 0f, 0f, fromAlpha);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled so it works even if timeScale is 0
            float t = Mathf.Clamp01(elapsed / duration);
            color.a = Mathf.Lerp(fromAlpha, toAlpha, t);
            fadeImage.color = color;
            yield return null;
        }

        color.a = toAlpha;
        fadeImage.color = color;

        // If fully transparent, disable canvas so it doesn't block raycasts
        if (toAlpha <= 0f)
        {
            fadeImage.raycastTarget = false;
            canvas.enabled = false;
        }

        onComplete?.Invoke();
    }

    /// <summary>
    /// Immediately set the screen to full black (no animation).
    /// Useful before a scene loads so it starts black.
    /// </summary>
    public void SetBlack()
    {
        canvas.enabled = true;
        fadeImage.color = new Color(0f, 0f, 0f, 1f);
        fadeImage.raycastTarget = true;
    }

    /// <summary>
    /// Immediately set the screen to fully transparent (no animation).
    /// </summary>
    public void SetClear()
    {
        fadeImage.color = new Color(0f, 0f, 0f, 0f);
        fadeImage.raycastTarget = false;
        canvas.enabled = false;
    }
}
