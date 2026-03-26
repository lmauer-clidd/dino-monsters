// ============================================================
// Dino Monsters -- 3D Battle Visual Effects
// ============================================================
//
// Particle-like effects using small procedural GameObjects.
// All effects are coroutine-driven, no external assets needed.
// ============================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEffects3D : MonoBehaviour
{
    // ===============================================================
    // Attack Effect — Type-colored particles burst at position
    // ===============================================================

    public void AttackEffect(DinoType type, Vector3 position)
    {
        StartCoroutine(AttackEffectCoroutine(type, position));
    }

    private IEnumerator AttackEffectCoroutine(DinoType type, Vector3 position)
    {
        Color effectColor = Constants.GetTypeColor(type);
        int particleCount = 20;
        var particles = new List<GameObject>();
        var initialScales = new float[particleCount];

        for (int i = 0; i < particleCount; i++)
        {
            var particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            particle.name = "AttackParticle";
            particle.transform.position = position;
            float baseSize = Random.Range(0.04f, 0.18f);
            initialScales[i] = baseSize;
            particle.transform.localScale = Vector3.one * baseSize;

            // Remove collider
            var col = particle.GetComponent<Collider>();
            if (col != null) Destroy(col);

            // Emissive + transparent material
            var renderer = particle.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Standard"));
            mat.color = effectColor;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", effectColor * 2f);
            SetMaterialTransparent(mat);
            renderer.material = mat;

            particles.Add(particle);
        }

        // Animate particles outward
        float duration = 0.6f;
        float elapsed = 0f;

        // Generate random directions for each particle
        var directions = new Vector3[particleCount];
        var speeds = new float[particleCount];
        for (int i = 0; i < particleCount; i++)
        {
            directions[i] = Random.onUnitSphere;
            speeds[i] = Random.Range(1.0f, 2.0f);
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            for (int i = 0; i < particles.Count; i++)
            {
                if (particles[i] == null) continue;

                // Move outward with individual speeds
                particles[i].transform.position = position + directions[i] * t * 1.5f * speeds[i];

                // Shrink from initial size to zero with size variation
                float scale = Mathf.Lerp(initialScales[i], 0f, t);
                particles[i].transform.localScale = Vector3.one * Mathf.Max(0f, scale);

                // Fade out alpha over lifetime
                var r = particles[i].GetComponent<Renderer>();
                if (r != null)
                {
                    Color c = r.material.color;
                    c.a = Mathf.Lerp(1f, 0f, t * t); // quadratic fade for smoother look
                    r.material.color = c;
                }
            }

            yield return null;
        }

        // Cleanup
        foreach (var p in particles)
        {
            if (p != null) Destroy(p);
        }
    }

    // ===============================================================
    // Hit Flash — Flash target white briefly
    // ===============================================================

    public void HitFlash(GameObject target)
    {
        if (target != null)
            StartCoroutine(HitFlashCoroutine(target));
    }

    private IEnumerator HitFlashCoroutine(GameObject target)
    {
        if (target == null) yield break;

        // Collect all renderers
        var renderers = target.GetComponentsInChildren<Renderer>();
        var originalColors = new Dictionary<Renderer, Color>();

        foreach (var r in renderers)
        {
            if (r != null && r.material != null)
                originalColors[r] = r.material.color;
        }

        // Flash white 3 times
        int flashes = 3;
        float flashDuration = 0.08f;

        for (int i = 0; i < flashes; i++)
        {
            // Set white
            foreach (var r in renderers)
            {
                if (r != null && r.material != null)
                    r.material.color = Color.white;
            }
            yield return new WaitForSeconds(flashDuration);

            // Restore
            foreach (var kvp in originalColors)
            {
                if (kvp.Key != null && kvp.Key.material != null)
                    kvp.Key.material.color = kvp.Value;
            }
            yield return new WaitForSeconds(flashDuration);
        }

        // Ensure colors are restored
        foreach (var kvp in originalColors)
        {
            if (kvp.Key != null && kvp.Key.material != null)
                kvp.Key.material.color = kvp.Value;
        }
    }

    // ===============================================================
    // Faint Animation — Sink into ground + fade out
    // ===============================================================

    public void FaintAnimation(GameObject target)
    {
        if (target != null)
            StartCoroutine(FaintCoroutine(target));
    }

    private IEnumerator FaintCoroutine(GameObject target)
    {
        if (target == null) yield break;

        Vector3 startPos = target.transform.position;
        Vector3 endPos = startPos + Vector3.down * 1.5f;
        Vector3 startScale = target.transform.localScale;

        var renderers = target.GetComponentsInChildren<Renderer>();

        float duration = 1.0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            if (target == null) yield break;

            // Sink downward
            target.transform.position = Vector3.Lerp(startPos, endPos, t);

            // Shrink vertically (squish effect)
            float scaleY = Mathf.Lerp(startScale.y, 0f, t);
            target.transform.localScale = new Vector3(startScale.x, scaleY, startScale.z);

            // Fade out all renderers
            float alpha = 1f - t;
            foreach (var r in renderers)
            {
                if (r == null || r.material == null) continue;
                Color c = r.material.color;
                c.a = alpha;
                r.material.color = c;

                // Switch to transparent if needed
                if (t > 0.1f)
                    SetMaterialTransparent(r.material);
            }

            yield return null;
        }

        // Deactivate after faint
        if (target != null)
            target.SetActive(false);
    }

    // ===============================================================
    // Critical Hit Effect — Extra particles + camera shake
    // ===============================================================

    public void CriticalEffect(Vector3 position)
    {
        StartCoroutine(CriticalEffectCoroutine(position));
    }

    private IEnumerator CriticalEffectCoroutine(Vector3 position)
    {
        // Bright yellow burst
        int particleCount = 16;
        var particles = new List<GameObject>();
        Color critColor = new Color(1f, 0.9f, 0.2f);

        for (int i = 0; i < particleCount; i++)
        {
            var particle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            particle.name = "CritParticle";
            particle.transform.position = position;
            float size = Random.Range(0.05f, 0.12f);
            particle.transform.localScale = Vector3.one * size;
            particle.transform.rotation = Random.rotation;

            var col = particle.GetComponent<Collider>();
            if (col != null) Destroy(col);

            var renderer = particle.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Standard"));
            mat.color = critColor;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", critColor * 3f);
            renderer.material = mat;

            particles.Add(particle);
        }

        // Star flash at center
        var starFlash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        starFlash.name = "CritFlash";
        starFlash.transform.position = position;
        starFlash.transform.localScale = Vector3.one * 0.05f;

        var starCol = starFlash.GetComponent<Collider>();
        if (starCol != null) Destroy(starCol);

        var starRenderer = starFlash.GetComponent<Renderer>();
        var starMat = new Material(Shader.Find("Standard"));
        starMat.color = Color.white;
        starMat.EnableKeyword("_EMISSION");
        starMat.SetColor("_EmissionColor", Color.white * 5f);
        starRenderer.material = starMat;

        // Animate
        float duration = 0.6f;
        float elapsed = 0f;

        var directions = new Vector3[particleCount];
        for (int i = 0; i < particleCount; i++)
            directions[i] = Random.onUnitSphere * Random.Range(1f, 2.5f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Expand star then shrink
            float starScale = t < 0.3f
                ? Mathf.Lerp(0.05f, 0.8f, t / 0.3f)
                : Mathf.Lerp(0.8f, 0f, (t - 0.3f) / 0.7f);
            if (starFlash != null)
                starFlash.transform.localScale = Vector3.one * starScale;

            // Particles fly outward
            for (int i = 0; i < particles.Count; i++)
            {
                if (particles[i] == null) continue;
                particles[i].transform.position = position + directions[i] * t;
                float scale = Mathf.Lerp(0.1f, 0f, t);
                particles[i].transform.localScale = Vector3.one * Mathf.Max(0, scale);
                particles[i].transform.Rotate(Vector3.one * 360f * Time.deltaTime);
            }

            yield return null;
        }

        // Cleanup
        if (starFlash != null) Destroy(starFlash);
        foreach (var p in particles)
        {
            if (p != null) Destroy(p);
        }
    }

    // ===============================================================
    // Super Effective Effect — Big flash ring
    // ===============================================================

    public void SuperEffectiveEffect(Vector3 position)
    {
        StartCoroutine(SuperEffectiveCoroutine(position));
    }

    private IEnumerator SuperEffectiveCoroutine(Vector3 position)
    {
        // Create an expanding ring of cubes
        int ringCount = 8;
        var ringParts = new List<GameObject>();
        Color seColor = new Color(1f, 0.4f, 0.1f);

        for (int i = 0; i < ringCount; i++)
        {
            var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = "SEParticle";
            part.transform.position = position;
            part.transform.localScale = Vector3.one * 0.1f;

            var col = part.GetComponent<Collider>();
            if (col != null) Destroy(col);

            var renderer = part.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Standard"));
            mat.color = seColor;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", seColor * 2f);
            renderer.material = mat;

            ringParts.Add(part);
        }

        // Central flash
        var flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flash.name = "SEFlash";
        flash.transform.position = position;
        flash.transform.localScale = Vector3.zero;

        var flashCol = flash.GetComponent<Collider>();
        if (flashCol != null) Destroy(flashCol);

        var flashRenderer = flash.GetComponent<Renderer>();
        var flashMat = new Material(Shader.Find("Standard"));
        flashMat.color = new Color(1f, 1f, 0.8f, 0.8f);
        flashMat.EnableKeyword("_EMISSION");
        flashMat.SetColor("_EmissionColor", Color.white * 3f);
        SetMaterialTransparent(flashMat);
        flashRenderer.material = flashMat;

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Expand flash sphere then fade
            float flashScale = t < 0.4f
                ? Mathf.Lerp(0f, 1.5f, t / 0.4f)
                : Mathf.Lerp(1.5f, 2f, (t - 0.4f) / 0.6f);
            if (flash != null)
            {
                flash.transform.localScale = Vector3.one * flashScale;
                Color fc = flashMat.color;
                fc.a = 1f - t;
                flashMat.color = fc;
            }

            // Ring expands outward
            for (int i = 0; i < ringParts.Count; i++)
            {
                if (ringParts[i] == null) continue;
                float angle = (i / (float)ringCount) * Mathf.PI * 2f;
                float radius = t * 2f;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
                ringParts[i].transform.position = position + offset;

                float scale = Mathf.Lerp(0.1f, 0f, t);
                ringParts[i].transform.localScale = Vector3.one * Mathf.Max(0, scale);
            }

            yield return null;
        }

        // Cleanup
        if (flash != null) Destroy(flash);
        foreach (var p in ringParts)
        {
            if (p != null) Destroy(p);
        }
    }

    // ===============================================================
    // Status Effect Visuals (optional — called from BattleSceneManager)
    // ===============================================================

    /// <summary>
    /// Show a persistent status effect indicator on a dino model.
    /// </summary>
    public void ShowStatusEffect(GameObject dinoModel, StatusEffect status)
    {
        if (dinoModel == null || status == StatusEffect.None) return;
        StartCoroutine(StatusEffectCoroutine(dinoModel, status));
    }

    private IEnumerator StatusEffectCoroutine(GameObject dinoModel, StatusEffect status)
    {
        Color statusColor;
        switch (status)
        {
            case StatusEffect.Poison: statusColor = new Color(0.63f, 0.25f, 0.63f); break;
            case StatusEffect.Burn: statusColor = new Color(0.94f, 0.50f, 0.19f); break;
            case StatusEffect.Paralysis: statusColor = new Color(0.97f, 0.82f, 0.17f); break;
            case StatusEffect.Sleep: statusColor = new Color(0.4f, 0.4f, 0.6f); break;
            case StatusEffect.Freeze: statusColor = new Color(0.59f, 0.85f, 0.85f); break;
            default: yield break;
        }

        // Brief flash of status color
        var renderers = dinoModel.GetComponentsInChildren<Renderer>();
        var originalColors = new Dictionary<Renderer, Color>();

        foreach (var r in renderers)
        {
            if (r != null && r.material != null)
                originalColors[r] = r.material.color;
        }

        // Tint with status color
        foreach (var r in renderers)
        {
            if (r != null && r.material != null)
                r.material.color = Color.Lerp(r.material.color, statusColor, 0.5f);
        }

        yield return new WaitForSeconds(0.3f);

        // Restore
        foreach (var kvp in originalColors)
        {
            if (kvp.Key != null && kvp.Key.material != null)
                kvp.Key.material.color = kvp.Value;
        }
    }

    // ===============================================================
    // Idle Breathing Animation (subtle scale oscillation)
    // ===============================================================

    /// <summary>
    /// Start a subtle breathing animation on a dino model.
    /// Call this after spawning to make the model feel alive.
    /// </summary>
    public Coroutine StartBreathingAnimation(GameObject dinoModel)
    {
        if (dinoModel == null) return null;
        return StartCoroutine(BreathingCoroutine(dinoModel));
    }

    private IEnumerator BreathingCoroutine(GameObject dinoModel)
    {
        Vector3 baseScale = dinoModel.transform.localScale;
        float breathSpeed = 1.5f;
        float breathAmount = 0.02f;

        while (dinoModel != null && dinoModel.activeInHierarchy)
        {
            float t = Mathf.Sin(Time.time * breathSpeed) * breathAmount;
            dinoModel.transform.localScale = baseScale + new Vector3(0, t, 0);
            yield return null;
        }
    }

    // ===============================================================
    // Level Up Effect — Golden sparkles rising upward
    // ===============================================================

    public void LevelUpEffect(Transform target)
    {
        if (target != null)
            StartCoroutine(LevelUpEffectCoroutine(target));
    }

    private IEnumerator LevelUpEffectCoroutine(Transform target)
    {
        Color gold = new Color(1f, 0.85f, 0.2f);
        int particleCount = 24;
        var particles = new List<GameObject>();

        Vector3 basePos = target != null ? target.position : Vector3.zero;

        for (int i = 0; i < particleCount; i++)
        {
            var particle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            particle.name = "LevelUpSparkle";
            // Spawn in a ring around the target
            float angle = (i / (float)particleCount) * Mathf.PI * 2f;
            float radius = Random.Range(0.3f, 0.8f);
            Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, Random.Range(-0.2f, 0.2f), Mathf.Sin(angle) * radius);
            particle.transform.position = basePos + offset;
            float size = Random.Range(0.04f, 0.10f);
            particle.transform.localScale = Vector3.one * size;
            particle.transform.rotation = Random.rotation;

            var col = particle.GetComponent<Collider>();
            if (col != null) Destroy(col);

            var renderer = particle.GetComponent<Renderer>();
            var mat = new Material(Shader.Find("Standard"));
            Color sparkleColor = Color.Lerp(gold, Color.white, Random.Range(0f, 0.3f));
            mat.color = sparkleColor;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", sparkleColor * 3f);
            SetMaterialTransparent(mat);
            renderer.material = mat;

            particles.Add(particle);
        }

        float duration = 1.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            for (int i = 0; i < particles.Count; i++)
            {
                if (particles[i] == null) continue;

                // Rise upward
                particles[i].transform.position += Vector3.up * 2.5f * Time.deltaTime;
                // Gentle sway
                float sway = Mathf.Sin(Time.time * 4f + i) * 0.3f * Time.deltaTime;
                particles[i].transform.position += new Vector3(sway, 0, sway);
                // Spin
                particles[i].transform.Rotate(Vector3.one * 180f * Time.deltaTime);

                // Fade out in the last 40%
                float alpha = t > 0.6f ? Mathf.Lerp(1f, 0f, (t - 0.6f) / 0.4f) : 1f;
                float scale = Mathf.Lerp(0.08f, 0.02f, t);
                particles[i].transform.localScale = Vector3.one * scale;

                var r = particles[i].GetComponent<Renderer>();
                if (r != null)
                {
                    Color c = r.material.color;
                    c.a = alpha;
                    r.material.color = c;
                }
            }

            yield return null;
        }

        foreach (var p in particles)
        {
            if (p != null) Destroy(p);
        }
    }

    // ===============================================================
    // Heal Effect — Green crosses floating up
    // ===============================================================

    public void HealEffect(Transform target)
    {
        if (target != null)
            StartCoroutine(HealEffectCoroutine(target));
    }

    private IEnumerator HealEffectCoroutine(Transform target)
    {
        Color healGreen = new Color(0.2f, 0.9f, 0.3f);
        int crossCount = 8;
        var crosses = new List<GameObject>();

        Vector3 basePos = target != null ? target.position : Vector3.zero;

        for (int i = 0; i < crossCount; i++)
        {
            // Create a plus/cross shape from two cubes
            var crossRoot = new GameObject("HealCross");
            float angle = (i / (float)crossCount) * Mathf.PI * 2f;
            float radius = Random.Range(0.2f, 0.6f);
            Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, Random.Range(0f, 0.5f), Mathf.Sin(angle) * radius);
            crossRoot.transform.position = basePos + offset;

            // Horizontal bar
            var hBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hBar.transform.SetParent(crossRoot.transform);
            hBar.transform.localPosition = Vector3.zero;
            hBar.transform.localScale = new Vector3(0.12f, 0.04f, 0.04f);
            var hCol = hBar.GetComponent<Collider>();
            if (hCol != null) Destroy(hCol);

            var hRenderer = hBar.GetComponent<Renderer>();
            var hMat = new Material(Shader.Find("Standard"));
            hMat.color = healGreen;
            hMat.EnableKeyword("_EMISSION");
            hMat.SetColor("_EmissionColor", healGreen * 2f);
            SetMaterialTransparent(hMat);
            hRenderer.material = hMat;

            // Vertical bar
            var vBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            vBar.transform.SetParent(crossRoot.transform);
            vBar.transform.localPosition = Vector3.zero;
            vBar.transform.localScale = new Vector3(0.04f, 0.12f, 0.04f);
            var vCol = vBar.GetComponent<Collider>();
            if (vCol != null) Destroy(vCol);

            var vRenderer = vBar.GetComponent<Renderer>();
            var vMat = new Material(Shader.Find("Standard"));
            vMat.color = healGreen;
            vMat.EnableKeyword("_EMISSION");
            vMat.SetColor("_EmissionColor", healGreen * 2f);
            SetMaterialTransparent(vMat);
            vRenderer.material = vMat;

            crosses.Add(crossRoot);
        }

        float duration = 1.0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            for (int i = 0; i < crosses.Count; i++)
            {
                if (crosses[i] == null) continue;

                // Float upward
                crosses[i].transform.position += Vector3.up * 1.8f * Time.deltaTime;
                // Gentle spin
                crosses[i].transform.Rotate(0f, 90f * Time.deltaTime, 0f);

                // Fade out
                float alpha = t > 0.5f ? Mathf.Lerp(1f, 0f, (t - 0.5f) / 0.5f) : 1f;
                float scale = Mathf.Lerp(1f, 0.5f, t);
                crosses[i].transform.localScale = Vector3.one * scale;

                var renderers = crosses[i].GetComponentsInChildren<Renderer>();
                foreach (var r in renderers)
                {
                    if (r != null && r.material != null)
                    {
                        Color c = r.material.color;
                        c.a = alpha;
                        r.material.color = c;
                    }
                }
            }

            yield return null;
        }

        foreach (var cross in crosses)
        {
            if (cross != null) Destroy(cross);
        }
    }

    // ===============================================================
    // Capture Effect — Red/white sphere that shrinks to a point
    // ===============================================================

    public void CaptureEffect(Vector3 position)
    {
        StartCoroutine(CaptureEffectCoroutine(position));
    }

    private IEnumerator CaptureEffectCoroutine(Vector3 position)
    {
        // Create a sphere representing the pokeball
        var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ball.name = "CaptureBall";
        ball.transform.position = position + Vector3.up * 0.5f;
        ball.transform.localScale = Vector3.one * 0.8f;

        var ballCol = ball.GetComponent<Collider>();
        if (ballCol != null) Destroy(ballCol);

        // Red top half (we'll just use red color)
        var ballRenderer = ball.GetComponent<Renderer>();
        var ballMat = new Material(Shader.Find("Standard"));
        ballMat.color = new Color(0.9f, 0.2f, 0.2f);
        ballMat.EnableKeyword("_EMISSION");
        ballMat.SetColor("_EmissionColor", new Color(0.9f, 0.2f, 0.2f) * 1.5f);
        ballRenderer.material = ballMat;

        // White center line (small cylinder)
        var line = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        line.name = "CaptureLine";
        line.transform.SetParent(ball.transform);
        line.transform.localPosition = Vector3.zero;
        line.transform.localScale = new Vector3(1.05f, 0.05f, 1.05f);

        var lineCol = line.GetComponent<Collider>();
        if (lineCol != null) Destroy(lineCol);

        var lineRenderer = line.GetComponent<Renderer>();
        var lineMat = new Material(Shader.Find("Standard"));
        lineMat.color = Color.white;
        lineMat.EnableKeyword("_EMISSION");
        lineMat.SetColor("_EmissionColor", Color.white * 2f);
        lineRenderer.material = lineMat;

        // Animate: wobble 3 times then shrink to a point
        float wobbleTime = 1.5f;
        float shrinkTime = 0.5f;
        float elapsed = 0f;

        // Wobble phase
        Vector3 ballPos = ball.transform.position;
        while (elapsed < wobbleTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / wobbleTime;
            // 3 wobbles
            float wobble = Mathf.Sin(t * Mathf.PI * 6f) * 15f * (1f - t);
            ball.transform.rotation = Quaternion.Euler(0f, 0f, wobble);
            yield return null;
        }

        // Shrink phase
        elapsed = 0f;
        Vector3 startScale = ball.transform.localScale;
        while (elapsed < shrinkTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shrinkTime;
            float scale = Mathf.Lerp(1f, 0f, t * t);
            ball.transform.localScale = startScale * scale;

            // Flash white at the end
            if (t > 0.7f)
            {
                float flashT = (t - 0.7f) / 0.3f;
                ballMat.color = Color.Lerp(new Color(0.9f, 0.2f, 0.2f), Color.white, flashT);
                ballMat.SetColor("_EmissionColor", Color.white * flashT * 5f);
            }

            yield return null;
        }

        // Spawn success sparkles
        for (int i = 0; i < 12; i++)
        {
            var sparkle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sparkle.name = "CaptureSparkle";
            sparkle.transform.position = ballPos;
            sparkle.transform.localScale = Vector3.one * Random.Range(0.03f, 0.08f);

            var sCol = sparkle.GetComponent<Collider>();
            if (sCol != null) Destroy(sCol);

            var sRenderer = sparkle.GetComponent<Renderer>();
            var sMat = new Material(Shader.Find("Standard"));
            Color sparkleColor = Color.Lerp(Color.white, Color.yellow, Random.Range(0f, 0.5f));
            sMat.color = sparkleColor;
            sMat.EnableKeyword("_EMISSION");
            sMat.SetColor("_EmissionColor", sparkleColor * 3f);
            sRenderer.material = sMat;

            // Animate sparkle outward
            StartCoroutine(AnimateSparkle(sparkle, ballPos, Random.onUnitSphere * 1.5f, 0.4f));
        }

        // Destroy ball
        if (ball != null) Destroy(ball);
    }

    private IEnumerator AnimateSparkle(GameObject sparkle, Vector3 origin, Vector3 direction, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            if (sparkle == null) yield break;
            sparkle.transform.position = origin + direction * t;
            float scale = Mathf.Lerp(0.06f, 0f, t);
            sparkle.transform.localScale = Vector3.one * Mathf.Max(0, scale);
            yield return null;
        }
        if (sparkle != null) Destroy(sparkle);
    }

    // ===============================================================
    // Type-Specific Attack Animations (Static — called from BattleSceneManager)
    // ===============================================================

    /// <summary>
    /// Dispatch to the correct type-specific attack animation.
    /// Falls back to generic burst for unhandled types.
    /// </summary>
    public static IEnumerator TypedAttack(MonoBehaviour owner, int type, Vector3 attacker, Vector3 target, float power)
    {
        DinoType dinoType = (DinoType)type;
        switch (dinoType)
        {
            case DinoType.Fire:     yield return FireAttack(owner, target, power); break;
            case DinoType.Water:    yield return WaterAttack(owner, target, power); break;
            case DinoType.Flora:    yield return FloraAttack(owner, target, power); break;
            case DinoType.Electric: yield return ElectricAttack(owner, target, power); break;
            case DinoType.Ice:      yield return IceAttack(owner, target, power); break;
            case DinoType.Earth:    yield return EarthAttack(owner, target, power); break;
            case DinoType.Shadow:   yield return ShadowAttack(owner, attacker, target, power); break;
            case DinoType.Normal:   yield return NormalAttack(owner, target, power); break;
            default:
                // Fallback: generic colored burst at target
                yield return GenericAttack(owner, dinoType, target, power);
                break;
        }
    }

    // ---------------------------------------------------------------
    // Fire Attack — Fireball + explosion + smoke
    // ---------------------------------------------------------------

    public static IEnumerator FireAttack(MonoBehaviour owner, Vector3 target, float power)
    {
        float scale = GetPowerScale(power);
        int particleCount = (int)(12 * scale);
        Color fireOrange = new Color(1f, 0.5f, 0.05f);
        Color fireRed = new Color(1f, 0.15f, 0.0f);

        // Fireball impact at target
        var fireball = CreateEffectPrimitive(PrimitiveType.Sphere, target + Vector3.up * 0.5f,
            0.2f * scale, fireOrange, 3f);

        // Expand fireball
        float duration = 0.6f;
        float elapsed = 0f;
        var particles = new List<GameObject>();

        // Spawn explosion particles
        for (int i = 0; i < particleCount; i++)
        {
            Color c = Color.Lerp(fireOrange, fireRed, Random.Range(0f, 1f));
            float size = Random.Range(0.05f, 0.15f) * scale;
            var p = CreateEffectPrimitive(PrimitiveType.Sphere, target + Vector3.up * 0.5f, size, c, 2f);
            particles.Add(p);
        }

        // Smoke puffs (dark grey spheres)
        int smokeCount = (int)(6 * scale);
        for (int i = 0; i < smokeCount; i++)
        {
            Color smoke = new Color(0.3f, 0.3f, 0.3f, 0.6f);
            float size = Random.Range(0.1f, 0.2f) * scale;
            var s = CreateEffectPrimitive(PrimitiveType.Sphere, target + Vector3.up * 0.5f, size, smoke, 0.5f);
            SetTransparent(s);
            particles.Add(s);
        }

        var directions = new Vector3[particles.Count];
        var speeds = new float[particles.Count];
        for (int i = 0; i < particles.Count; i++)
        {
            directions[i] = Random.onUnitSphere;
            directions[i].y = Mathf.Abs(directions[i].y); // Mostly upward
            speeds[i] = Random.Range(1f, 3f) * scale;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Expand then shrink fireball
            if (fireball != null)
            {
                float fbScale = t < 0.3f
                    ? Mathf.Lerp(0.2f, 0.6f, t / 0.3f) * scale
                    : Mathf.Lerp(0.6f, 0f, (t - 0.3f) / 0.7f) * scale;
                fireball.transform.localScale = Vector3.one * fbScale;
            }

            for (int i = 0; i < particles.Count; i++)
            {
                if (particles[i] == null) continue;
                particles[i].transform.position = target + Vector3.up * 0.5f + directions[i] * t * speeds[i];
                float s = Mathf.Lerp(1f, 0f, t);
                particles[i].transform.localScale *= (1f - Time.deltaTime * 3f);
            }

            yield return null;
        }

        // Cleanup
        if (fireball != null) Object.Destroy(fireball);
        foreach (var p in particles) if (p != null) Object.Destroy(p);
    }

    // ---------------------------------------------------------------
    // Water Attack — Water stream + splash + droplets
    // ---------------------------------------------------------------

    public static IEnumerator WaterAttack(MonoBehaviour owner, Vector3 target, float power)
    {
        float scale = GetPowerScale(power);
        Color waterBlue = new Color(0.2f, 0.5f, 1f);
        Color waterLight = new Color(0.5f, 0.75f, 1f);

        int dropCount = (int)(16 * scale);
        var particles = new List<GameObject>();

        // Create water splash at target
        for (int i = 0; i < dropCount; i++)
        {
            Color c = Color.Lerp(waterBlue, waterLight, Random.Range(0f, 0.5f));
            float size = Random.Range(0.04f, 0.12f) * scale;
            var p = CreateEffectPrimitive(PrimitiveType.Sphere, target + Vector3.up * 0.3f, size, c, 2f);
            SetTransparent(p);
            particles.Add(p);
        }

        // Central wave (flattened sphere)
        var wave = CreateEffectPrimitive(PrimitiveType.Sphere, target, 0.1f, waterBlue, 2f);
        SetTransparent(wave);
        wave.transform.localScale = new Vector3(0.3f * scale, 0.05f, 0.3f * scale);

        var directions = new Vector3[dropCount];
        for (int i = 0; i < dropCount; i++)
        {
            directions[i] = Random.onUnitSphere;
            directions[i].y = Mathf.Abs(directions[i].y) * 2f; // Arc upward
        }

        float duration = 0.7f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Wave expands outward
            if (wave != null)
            {
                float waveScale = Mathf.Lerp(0.3f, 2f, t) * scale;
                float waveAlpha = Mathf.Lerp(0.8f, 0f, t);
                wave.transform.localScale = new Vector3(waveScale, 0.05f, waveScale);
                SetAlpha(wave, waveAlpha);
            }

            // Droplets arc upward then fall
            for (int i = 0; i < particles.Count; i++)
            {
                if (particles[i] == null) continue;
                Vector3 pos = target + Vector3.up * 0.3f + directions[i] * t * 2f * scale;
                pos.y += Mathf.Sin(t * Mathf.PI) * 1.5f * scale - t * t * 2f; // Parabolic arc
                particles[i].transform.position = pos;

                float alpha = t > 0.6f ? Mathf.Lerp(1f, 0f, (t - 0.6f) / 0.4f) : 1f;
                SetAlpha(particles[i], alpha);
            }

            yield return null;
        }

        if (wave != null) Object.Destroy(wave);
        foreach (var p in particles) if (p != null) Object.Destroy(p);
    }

    // ---------------------------------------------------------------
    // Flora Attack — Vine whip / leaf storm
    // ---------------------------------------------------------------

    public static IEnumerator FloraAttack(MonoBehaviour owner, Vector3 target, float power)
    {
        float scale = GetPowerScale(power);
        Color leafGreen = new Color(0.2f, 0.8f, 0.1f);
        Color leafDark = new Color(0.1f, 0.55f, 0.05f);

        int leafCount = (int)(14 * scale);
        var leaves = new List<GameObject>();

        // Leaves spiral toward target
        for (int i = 0; i < leafCount; i++)
        {
            Color c = Color.Lerp(leafGreen, leafDark, Random.Range(0f, 0.5f));
            float size = Random.Range(0.06f, 0.14f) * scale;
            // Start from a ring around the target
            float angle = (i / (float)leafCount) * Mathf.PI * 2f;
            float radius = 2f * scale;
            Vector3 startPos = target + new Vector3(Mathf.Cos(angle) * radius, Random.Range(0.5f, 2f), Mathf.Sin(angle) * radius);
            var leaf = CreateEffectPrimitive(PrimitiveType.Cube, startPos, size, c, 2f);
            // Flatten to look like a leaf
            leaf.transform.localScale = new Vector3(size * 1.5f, size * 0.3f, size);
            leaf.transform.rotation = Random.rotation;
            leaves.Add(leaf);
        }

        float duration = 0.7f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            for (int i = 0; i < leaves.Count; i++)
            {
                if (leaves[i] == null) continue;

                float angle = (i / (float)leafCount) * Mathf.PI * 2f + t * Mathf.PI * 3f;
                float radius = Mathf.Lerp(2f, 0f, t) * scale;
                float y = Mathf.Lerp(1.5f, 0.5f, t);
                Vector3 pos = target + new Vector3(Mathf.Cos(angle) * radius, y, Mathf.Sin(angle) * radius);
                leaves[i].transform.position = pos;

                // Spin leaves
                leaves[i].transform.Rotate(Vector3.up * 360f * Time.deltaTime, Space.World);
                leaves[i].transform.Rotate(Vector3.right * 180f * Time.deltaTime, Space.World);

                // Fade in last portion
                float alpha = t > 0.7f ? Mathf.Lerp(1f, 0f, (t - 0.7f) / 0.3f) : 1f;
                SetAlpha(leaves[i], alpha);
            }

            yield return null;
        }

        foreach (var l in leaves) if (l != null) Object.Destroy(l);
    }

    // ---------------------------------------------------------------
    // Electric Attack — Lightning bolt zigzag + spark burst
    // ---------------------------------------------------------------

    public static IEnumerator ElectricAttack(MonoBehaviour owner, Vector3 target, float power)
    {
        float scale = GetPowerScale(power);
        Color yellow = new Color(1f, 0.95f, 0.2f);
        Color white = new Color(1f, 1f, 0.8f);

        // Build zigzag bolt from above the target downward
        int segments = (int)(6 + 4 * scale);
        var boltParts = new List<GameObject>();
        Vector3 boltStart = target + Vector3.up * 3f;
        Vector3 boltEnd = target + Vector3.up * 0.3f;

        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)segments;
            Vector3 pos = Vector3.Lerp(boltStart, boltEnd, t);
            // Zigzag offset
            pos.x += Mathf.Sin(t * Mathf.PI * 4f) * 0.3f * scale;
            pos.z += Mathf.Cos(t * Mathf.PI * 3f) * 0.15f * scale;

            Color c = Color.Lerp(yellow, white, Random.Range(0f, 0.3f));
            float size = Random.Range(0.06f, 0.12f) * scale;
            var cube = CreateEffectPrimitive(PrimitiveType.Cube, pos, size, c, 4f);
            cube.transform.rotation = Random.rotation;
            boltParts.Add(cube);
        }

        // Spark burst at impact point
        int sparkCount = (int)(10 * scale);
        var sparks = new List<GameObject>();
        var sparkDirs = new Vector3[sparkCount];
        for (int i = 0; i < sparkCount; i++)
        {
            float size = Random.Range(0.03f, 0.08f) * scale;
            var spark = CreateEffectPrimitive(PrimitiveType.Cube, target + Vector3.up * 0.3f, size, yellow, 3f);
            spark.transform.rotation = Random.rotation;
            sparks.Add(spark);
            sparkDirs[i] = Random.onUnitSphere * Random.Range(1f, 2.5f) * scale;
        }

        float duration = 0.6f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Bolt flickers and fades
            for (int i = 0; i < boltParts.Count; i++)
            {
                if (boltParts[i] == null) continue;
                // Flicker by randomly scaling
                float flicker = Random.Range(0.5f, 1.5f);
                float fadeScale = Mathf.Lerp(1f, 0f, t) * flicker;
                boltParts[i].transform.localScale = Vector3.one * 0.08f * scale * fadeScale;
                boltParts[i].transform.Rotate(Random.onUnitSphere * 500f * Time.deltaTime);
            }

            // Sparks fly outward
            for (int i = 0; i < sparks.Count; i++)
            {
                if (sparks[i] == null) continue;
                sparks[i].transform.position = target + Vector3.up * 0.3f + sparkDirs[i] * t;
                float sScale = Mathf.Lerp(0.06f, 0f, t) * scale;
                sparks[i].transform.localScale = Vector3.one * Mathf.Max(0, sScale);
                sparks[i].transform.Rotate(Random.onUnitSphere * 720f * Time.deltaTime);
            }

            yield return null;
        }

        foreach (var b in boltParts) if (b != null) Object.Destroy(b);
        foreach (var s in sparks) if (s != null) Object.Destroy(s);
    }

    // ---------------------------------------------------------------
    // Ice Attack — Ice shards + freeze flash
    // ---------------------------------------------------------------

    public static IEnumerator IceAttack(MonoBehaviour owner, Vector3 target, float power)
    {
        float scale = GetPowerScale(power);
        Color iceBlue = new Color(0.6f, 0.85f, 1f);
        Color iceWhite = new Color(0.9f, 0.95f, 1f);

        int shardCount = (int)(10 * scale);
        var shards = new List<GameObject>();
        var shardStartPos = new Vector3[shardCount];
        var shardEndPos = new Vector3[shardCount];

        // Ice shards fly toward target from various angles
        for (int i = 0; i < shardCount; i++)
        {
            float angle = (i / (float)shardCount) * Mathf.PI * 2f;
            float dist = Random.Range(2f, 3f) * scale;
            Vector3 start = target + new Vector3(Mathf.Cos(angle) * dist, Random.Range(0.5f, 2f), Mathf.Sin(angle) * dist);
            Vector3 end = target + Vector3.up * Random.Range(0.2f, 0.8f);

            shardStartPos[i] = start;
            shardEndPos[i] = end;

            Color c = Color.Lerp(iceBlue, iceWhite, Random.Range(0f, 0.5f));
            float size = Random.Range(0.05f, 0.12f) * scale;
            var shard = CreateEffectPrimitive(PrimitiveType.Cube, start, size, c, 2f);
            // Elongate to look like an ice shard
            shard.transform.localScale = new Vector3(size * 0.4f, size * 2f, size * 0.4f);
            shard.transform.LookAt(end);
            shards.Add(shard);
        }

        // Freeze flash sphere
        var flash = CreateEffectPrimitive(PrimitiveType.Sphere, target + Vector3.up * 0.5f, 0.05f, iceWhite, 3f);
        SetTransparent(flash);

        float duration = 0.7f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Shards converge on target in first half
            float shardT = Mathf.Clamp01(t * 2f);
            for (int i = 0; i < shards.Count; i++)
            {
                if (shards[i] == null) continue;
                shards[i].transform.position = Vector3.Lerp(shardStartPos[i], shardEndPos[i], shardT);

                // Fade out in second half
                if (t > 0.5f)
                {
                    float fadeT = (t - 0.5f) / 0.5f;
                    float s = Mathf.Lerp(1f, 0f, fadeT);
                    shards[i].transform.localScale *= (1f - Time.deltaTime * 4f);
                }
            }

            // Flash expands on impact
            if (flash != null && t > 0.4f)
            {
                float flashT = (t - 0.4f) / 0.6f;
                float flashScale = Mathf.Lerp(0.1f, 2f, flashT) * scale;
                float flashAlpha = Mathf.Lerp(0.8f, 0f, flashT);
                flash.transform.localScale = Vector3.one * flashScale;
                SetAlpha(flash, flashAlpha);
            }

            yield return null;
        }

        if (flash != null) Object.Destroy(flash);
        foreach (var s in shards) if (s != null) Object.Destroy(s);
    }

    // ---------------------------------------------------------------
    // Earth Attack — Ground shake + rising rocks + dust
    // ---------------------------------------------------------------

    public static IEnumerator EarthAttack(MonoBehaviour owner, Vector3 target, float power)
    {
        float scale = GetPowerScale(power);
        Color brown = new Color(0.55f, 0.35f, 0.15f);
        Color rock = new Color(0.45f, 0.4f, 0.35f);
        Color dust = new Color(0.7f, 0.6f, 0.4f, 0.5f);

        int rockCount = (int)(8 * scale);
        var rocks = new List<GameObject>();
        var rockTargetY = new float[rockCount];

        // Rocks rise from below target
        for (int i = 0; i < rockCount; i++)
        {
            float offsetX = Random.Range(-0.8f, 0.8f) * scale;
            float offsetZ = Random.Range(-0.8f, 0.8f) * scale;
            Vector3 pos = target + new Vector3(offsetX, -0.5f, offsetZ);
            float size = Random.Range(0.1f, 0.25f) * scale;
            Color c = Color.Lerp(brown, rock, Random.Range(0f, 1f));
            var r = CreateEffectPrimitive(PrimitiveType.Cube, pos, size, c, 1f);
            r.transform.rotation = Random.rotation;
            rocks.Add(r);
            rockTargetY[i] = Random.Range(0.5f, 1.8f) * scale;
        }

        // Dust cloud particles
        int dustCount = (int)(10 * scale);
        var dusts = new List<GameObject>();
        for (int i = 0; i < dustCount; i++)
        {
            float size = Random.Range(0.08f, 0.2f) * scale;
            Vector3 pos = target + new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f));
            var d = CreateEffectPrimitive(PrimitiveType.Sphere, pos, size, dust, 0.5f);
            SetTransparent(d);
            dusts.Add(d);
        }

        float duration = 0.8f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Rocks rise up then fall back
            for (int i = 0; i < rocks.Count; i++)
            {
                if (rocks[i] == null) continue;
                Vector3 pos = rocks[i].transform.position;
                if (t < 0.4f)
                    pos.y = Mathf.Lerp(-0.5f, rockTargetY[i], t / 0.4f);
                else
                    pos.y = Mathf.Lerp(rockTargetY[i], -0.3f, (t - 0.4f) / 0.6f);
                rocks[i].transform.position = pos;
                rocks[i].transform.Rotate(Random.onUnitSphere * 120f * Time.deltaTime);

                // Fade in last portion
                if (t > 0.7f)
                {
                    float fadeT = (t - 0.7f) / 0.3f;
                    float s = Mathf.Lerp(1f, 0f, fadeT);
                    rocks[i].transform.localScale = Vector3.one * rocks[i].transform.localScale.x * (1f - Time.deltaTime * 5f);
                }
            }

            // Dust expands outward and fades
            for (int i = 0; i < dusts.Count; i++)
            {
                if (dusts[i] == null) continue;
                Vector3 dir = (dusts[i].transform.position - target).normalized;
                if (dir.magnitude < 0.01f) dir = Random.onUnitSphere;
                dusts[i].transform.position += dir * 1.5f * Time.deltaTime * scale;
                dusts[i].transform.position += Vector3.up * 0.5f * Time.deltaTime;

                float alpha = Mathf.Lerp(0.6f, 0f, t);
                SetAlpha(dusts[i], alpha);
            }

            yield return null;
        }

        foreach (var r in rocks) if (r != null) Object.Destroy(r);
        foreach (var d in dusts) if (d != null) Object.Destroy(d);
    }

    // ---------------------------------------------------------------
    // Shadow Attack — Dark pulse + shadow tendrils
    // ---------------------------------------------------------------

    public static IEnumerator ShadowAttack(MonoBehaviour owner, Vector3 attacker, Vector3 target, float power)
    {
        float scale = GetPowerScale(power);
        Color dark = new Color(0.15f, 0.0f, 0.2f);
        Color purple = new Color(0.4f, 0.0f, 0.5f);

        // Dark pulse ring expanding from attacker
        int ringCount = (int)(8 * scale);
        var ringParts = new List<GameObject>();
        for (int i = 0; i < ringCount; i++)
        {
            float size = 0.08f * scale;
            var part = CreateEffectPrimitive(PrimitiveType.Sphere, attacker + Vector3.up * 0.5f, size, dark, 2f);
            SetTransparent(part);
            ringParts.Add(part);
        }

        // Shadow tendrils reaching from attacker to target
        int tendrilSegments = (int)(8 * scale);
        var tendrils = new List<GameObject>();
        for (int i = 0; i < tendrilSegments; i++)
        {
            float size = Random.Range(0.04f, 0.1f) * scale;
            Color c = Color.Lerp(dark, purple, Random.Range(0f, 0.5f));
            var t2 = CreateEffectPrimitive(PrimitiveType.Sphere, attacker + Vector3.up * 0.3f, size, c, 1.5f);
            SetTransparent(t2);
            tendrils.Add(t2);
        }

        float duration = 0.8f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Dark pulse ring expands
            for (int i = 0; i < ringParts.Count; i++)
            {
                if (ringParts[i] == null) continue;
                float angle = (i / (float)ringCount) * Mathf.PI * 2f + t * Mathf.PI;
                float radius = t * 2f * scale;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
                ringParts[i].transform.position = attacker + Vector3.up * 0.5f + offset;

                float alpha = Mathf.Lerp(0.8f, 0f, t);
                SetAlpha(ringParts[i], alpha);
                float pScale = Mathf.Lerp(0.08f, 0.02f, t) * scale;
                ringParts[i].transform.localScale = Vector3.one * pScale;
            }

            // Tendrils travel from attacker to target
            for (int i = 0; i < tendrils.Count; i++)
            {
                if (tendrils[i] == null) continue;
                float progress = Mathf.Clamp01(t * 1.5f - (i / (float)tendrilSegments) * 0.3f);
                Vector3 pos = Vector3.Lerp(attacker, target, progress);
                // Wavy offset
                float wave = Mathf.Sin(progress * Mathf.PI * 3f + i) * 0.3f * scale;
                pos.y += 0.3f + wave;
                tendrils[i].transform.position = pos;

                float alpha = t > 0.6f ? Mathf.Lerp(0.8f, 0f, (t - 0.6f) / 0.4f) : 0.8f;
                SetAlpha(tendrils[i], alpha);
            }

            yield return null;
        }

        foreach (var r in ringParts) if (r != null) Object.Destroy(r);
        foreach (var t2 in tendrils) if (t2 != null) Object.Destroy(t2);
    }

    // ---------------------------------------------------------------
    // Normal Attack — Simple impact burst
    // ---------------------------------------------------------------

    public static IEnumerator NormalAttack(MonoBehaviour owner, Vector3 target, float power)
    {
        float scale = GetPowerScale(power);
        Color white = new Color(0.9f, 0.9f, 0.9f);
        Color grey = new Color(0.6f, 0.6f, 0.6f);

        int particleCount = (int)(10 * scale);
        var particles = new List<GameObject>();
        var directions = new Vector3[particleCount];

        for (int i = 0; i < particleCount; i++)
        {
            Color c = Color.Lerp(white, grey, Random.Range(0f, 0.5f));
            float size = Random.Range(0.04f, 0.12f) * scale;
            var p = CreateEffectPrimitive(PrimitiveType.Sphere, target + Vector3.up * 0.5f, size, c, 2f);
            particles.Add(p);
            directions[i] = Random.onUnitSphere * Random.Range(1f, 2f) * scale;
        }

        // Central impact flash
        var flash = CreateEffectPrimitive(PrimitiveType.Sphere, target + Vector3.up * 0.5f, 0.3f * scale, Color.white, 3f);
        SetTransparent(flash);

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Flash expands and fades
            if (flash != null)
            {
                float flashScale = Mathf.Lerp(0.3f, 1f, t) * scale;
                flash.transform.localScale = Vector3.one * flashScale;
                SetAlpha(flash, Mathf.Lerp(0.8f, 0f, t));
            }

            // Particles fly outward
            for (int i = 0; i < particles.Count; i++)
            {
                if (particles[i] == null) continue;
                particles[i].transform.position = target + Vector3.up * 0.5f + directions[i] * t;
                float pScale = Mathf.Lerp(0.08f, 0f, t) * scale;
                particles[i].transform.localScale = Vector3.one * Mathf.Max(0, pScale);
            }

            yield return null;
        }

        if (flash != null) Object.Destroy(flash);
        foreach (var p in particles) if (p != null) Object.Destroy(p);
    }

    // ---------------------------------------------------------------
    // Generic Fallback — Type-colored burst (for types without custom anim)
    // ---------------------------------------------------------------

    private static IEnumerator GenericAttack(MonoBehaviour owner, DinoType type, Vector3 target, float power)
    {
        float scale = GetPowerScale(power);
        Color effectColor = Constants.GetTypeColor(type);

        int particleCount = (int)(15 * scale);
        var particles = new List<GameObject>();
        var directions = new Vector3[particleCount];

        for (int i = 0; i < particleCount; i++)
        {
            float size = Random.Range(0.04f, 0.14f) * scale;
            var p = CreateEffectPrimitive(PrimitiveType.Sphere, target + Vector3.up * 0.5f, size, effectColor, 2f);
            particles.Add(p);
            directions[i] = Random.onUnitSphere * Random.Range(1f, 2f) * scale;
        }

        float duration = 0.6f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            for (int i = 0; i < particles.Count; i++)
            {
                if (particles[i] == null) continue;
                particles[i].transform.position = target + Vector3.up * 0.5f + directions[i] * t;
                float pScale = Mathf.Lerp(0.1f, 0f, t) * scale;
                particles[i].transform.localScale = Vector3.one * Mathf.Max(0, pScale);
            }

            yield return null;
        }

        foreach (var p in particles) if (p != null) Object.Destroy(p);
    }

    // ---------------------------------------------------------------
    // Static Helpers for typed attacks
    // ---------------------------------------------------------------

    /// <summary>
    /// Maps move power to a visual scale factor.
    /// power &lt; 40 = small (0.6), 40-70 = medium (1.0), &gt; 70 = large (1.5)
    /// </summary>
    private static float GetPowerScale(float power)
    {
        if (power < 40f) return 0.6f;
        if (power <= 70f) return 1.0f;
        return 1.5f;
    }

    /// <summary>
    /// Create a primitive with emissive colored material, no collider.
    /// </summary>
    private static GameObject CreateEffectPrimitive(PrimitiveType type, Vector3 position, float size, Color color, float emissionMultiplier)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = "TypedAttackParticle";
        go.transform.position = position;
        go.transform.localScale = Vector3.one * size;

        var col = go.GetComponent<Collider>();
        if (col != null) Object.Destroy(col);

        var renderer = go.GetComponent<Renderer>();
        var mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color * emissionMultiplier);
        renderer.material = mat;

        return go;
    }

    /// <summary>
    /// Switch a GameObject's material to transparent rendering mode.
    /// </summary>
    private static void SetTransparent(GameObject go)
    {
        var renderer = go.GetComponent<Renderer>();
        if (renderer == null) return;
        var mat = renderer.material;
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }

    /// <summary>
    /// Set the alpha of a GameObject's material.
    /// </summary>
    private static void SetAlpha(GameObject go, float alpha)
    {
        var renderer = go.GetComponent<Renderer>();
        if (renderer == null || renderer.material == null) return;
        Color c = renderer.material.color;
        c.a = alpha;
        renderer.material.color = c;
    }

    // ===============================================================
    // Material Helper
    // ===============================================================

    private void SetMaterialTransparent(Material mat)
    {
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }
}
