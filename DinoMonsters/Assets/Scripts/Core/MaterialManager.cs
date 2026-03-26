// ============================================================
// Dino Monsters -- Material Manager
// ============================================================
//
// Static utility that creates and caches materials at runtime.
// Avoids creating duplicate materials for the same color/effect.
// Uses Standard shader (compatible with both Built-in and URP
// via fallback).
// ============================================================

using UnityEngine;
using System.Collections.Generic;

public static class MaterialManager
{
    private static Dictionary<int, Material> solidCache = new Dictionary<int, Material>();
    private static Dictionary<int, Material> emissiveCache = new Dictionary<int, Material>();
    private static Dictionary<int, Material> transparentCache = new Dictionary<int, Material>();

    // ===============================================================
    // Solid Color Material
    // ===============================================================

    /// <summary>
    /// Get or create a solid opaque material with the given color.
    /// Cached by color hash — safe to share across renderers.
    /// </summary>
    public static Material GetSolidColor(Color color)
    {
        int key = ColorToHash(color);
        if (solidCache.TryGetValue(key, out Material cached) && cached != null)
            return cached;

        var mat = new Material(GetBaseShader());
        mat.color = color;
        mat.name = $"Solid_{ColorHex(color)}";
        solidCache[key] = mat;
        return mat;
    }

    // ===============================================================
    // Emissive Material
    // ===============================================================

    /// <summary>
    /// Get or create an emissive material (glowing effect).
    /// </summary>
    public static Material GetEmissive(Color color, float intensity = 1f)
    {
        int key = ColorToHash(color) ^ (intensity.GetHashCode() << 16);
        if (emissiveCache.TryGetValue(key, out Material cached) && cached != null)
            return cached;

        var mat = new Material(GetBaseShader());
        mat.color = color;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color * intensity);
        mat.name = $"Emissive_{ColorHex(color)}_{intensity:F1}";
        emissiveCache[key] = mat;
        return mat;
    }

    // ===============================================================
    // Transparent Material
    // ===============================================================

    /// <summary>
    /// Get or create a transparent material with the given color and alpha.
    /// </summary>
    public static Material GetTransparent(Color color, float alpha = 0.5f)
    {
        Color c = new Color(color.r, color.g, color.b, alpha);
        int key = ColorToHash(c) ^ 0x7F000000; // distinguish from solid
        if (transparentCache.TryGetValue(key, out Material cached) && cached != null)
            return cached;

        var mat = new Material(GetBaseShader());
        mat.color = c;
        mat.name = $"Transparent_{ColorHex(color)}_{alpha:F2}";

        // Standard shader transparent mode
        mat.SetFloat("_Mode", 3); // Transparent
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;

        transparentCache[key] = mat;
        return mat;
    }

    // ===============================================================
    // Helpers
    // ===============================================================

    private static Shader GetBaseShader()
    {
        // Try URP Lit first, fall back to Standard
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");
        return shader;
    }

    /// <summary>
    /// Deterministic hash from a Color (including alpha).
    /// </summary>
    private static int ColorToHash(Color c)
    {
        int r = Mathf.RoundToInt(c.r * 255);
        int g = Mathf.RoundToInt(c.g * 255);
        int b = Mathf.RoundToInt(c.b * 255);
        int a = Mathf.RoundToInt(c.a * 255);
        return (r << 24) | (g << 16) | (b << 8) | a;
    }

    private static string ColorHex(Color c)
    {
        return $"{Mathf.RoundToInt(c.r * 255):X2}{Mathf.RoundToInt(c.g * 255):X2}{Mathf.RoundToInt(c.b * 255):X2}";
    }

    /// <summary>
    /// Clear all caches. Call on scene unload if needed.
    /// </summary>
    public static void ClearCache()
    {
        solidCache.Clear();
        emissiveCache.Clear();
        transparentCache.Clear();
    }
}
