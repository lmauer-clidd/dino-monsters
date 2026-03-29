// ============================================================
// Dino Monsters -- Procedural Animation System
// ============================================================
// MonoBehaviour attached to dino model root GameObjects.
// Drives articulated animations via named joint transforms:
//   - Continuous idle loops (sine-wave oscillations)
//   - Species-specific attack sequences (coroutines)
//   - Hit reactions and faint animations
// ============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DinoAnimator : MonoBehaviour
{
    // ===============================================================
    // State
    // ===============================================================

    private enum AnimState { Idle, Attacking, Hit, Faint, Stopped }
    private AnimState currentState = AnimState.Stopped;

    private Dictionary<string, Transform> joints = new Dictionary<string, Transform>();
    private Dictionary<string, Quaternion> restRotations = new Dictionary<string, Quaternion>();
    private Dictionary<string, Vector3> restPositions = new Dictionary<string, Vector3>();
    private Dictionary<string, Vector3> restScales = new Dictionary<string, Vector3>();

    private DinoAnimProfiles.IdleChannel[] idleProfile;
    private float idleTime;
    private int speciesId = -1;

    private Coroutine activeCoroutine;
    private Vector3 rootRestPosition;

    // ===============================================================
    // Initialization
    // ===============================================================

    void Awake()
    {
        DiscoverJoints(transform);

        // Parse species ID from name (pattern: "Dino_Name")
        speciesId = GuessSpeciesId();
        idleProfile = DinoAnimProfiles.GetIdleProfile(speciesId);
    }

    private void DiscoverJoints(Transform root)
    {
        foreach (Transform child in root)
        {
            // A joint is any GameObject without a Renderer (empty pivot)
            // OR any named transform we recognize
            bool isJoint = child.GetComponent<Renderer>() == null;
            bool isNamedPart = IsKnownJointName(child.name);

            if (isJoint || isNamedPart)
            {
                if (!joints.ContainsKey(child.name))
                {
                    joints[child.name] = child;
                    restRotations[child.name] = child.localRotation;
                    restPositions[child.name] = child.localPosition;
                    restScales[child.name] = child.localScale;
                }
            }

            // Recurse into children
            DiscoverJoints(child);
        }
    }

    private bool IsKnownJointName(string name)
    {
        return name == "Body" || name == "Head" || name == "Neck" || name == "Jaw" ||
               name == "Crest" || name == "Flame" ||
               name.StartsWith("Neck_") || name.StartsWith("Tail_") ||
               name.StartsWith("Arm_") || name.StartsWith("Leg_") ||
               name.StartsWith("Flipper_") || name.StartsWith("Flower_") ||
               name == "TailPaddle" || name == "TailLeaf" || name == "Butterfly";
    }

    private int GuessSpeciesId()
    {
        // Try to get species ID from DataLoader by matching name
        string goName = gameObject.name;
        if (goName.StartsWith("Dino_"))
        {
            string speciesName = goName.Substring(5);
            if (DataLoader.Instance != null)
            {
                for (int i = 1; i <= 150; i++)
                {
                    var sp = DataLoader.Instance.GetSpecies(i);
                    if (sp != null && sp.name == speciesName)
                        return sp.id;
                }
            }
        }
        return -1;
    }

    // ===============================================================
    // Public API
    // ===============================================================

    public void PlayIdle()
    {
        if (currentState == AnimState.Faint) return; // can't idle after faint
        currentState = AnimState.Idle;
        idleTime = 0f;
    }

    public void PlayAttack(Action onHit, Action onDone)
    {
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        currentState = AnimState.Attacking;
        activeCoroutine = StartCoroutine(AttackCoroutine(onHit, onDone));
    }

    public Coroutine PlayAttackCoroutine(Action onHit, Action onDone)
    {
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        currentState = AnimState.Attacking;
        activeCoroutine = StartCoroutine(AttackCoroutine(onHit, onDone));
        return activeCoroutine;
    }

    public void PlayHit(Action onDone)
    {
        if (currentState == AnimState.Faint) return;
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        currentState = AnimState.Hit;
        activeCoroutine = StartCoroutine(HitCoroutine(onDone));
    }

    public void PlayFaint(Action onDone)
    {
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        currentState = AnimState.Faint;
        activeCoroutine = StartCoroutine(FaintCoroutine(onDone));
    }

    public Coroutine PlayFaintCoroutine(Action onDone)
    {
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        currentState = AnimState.Faint;
        activeCoroutine = StartCoroutine(FaintCoroutine(onDone));
        return activeCoroutine;
    }

    public void StopAll()
    {
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = null;
        currentState = AnimState.Stopped;
        ResetJointsToRest();
    }

    // ===============================================================
    // Idle Animation (Update Loop)
    // ===============================================================

    void Update()
    {
        if (currentState != AnimState.Idle || idleProfile == null) return;

        // Use unscaledDeltaTime so idle works in pause menu (Dinodex)
        idleTime += Time.unscaledDeltaTime;

        foreach (var ch in idleProfile)
        {
            if (!joints.TryGetValue(ch.partName, out Transform joint)) continue;

            float t = idleTime;

            // Rotation oscillation
            if (ch.rotAmplitude.sqrMagnitude > 0.001f)
            {
                float sin = Mathf.Sin(t * ch.frequency * Mathf.PI * 2f + ch.phaseOffset);
                Vector3 rotDelta = ch.rotAmplitude * sin;
                joint.localRotation = restRotations[ch.partName] * Quaternion.Euler(rotDelta);
            }

            // Scale oscillation
            if (ch.scaleAmplitude.sqrMagnitude > 0.0001f)
            {
                float sinS = Mathf.Sin(t * ch.scaleFrequency * Mathf.PI * 2f + ch.phaseOffset);
                Vector3 scaleDelta = ch.scaleAmplitude * sinS;
                joint.localScale = restScales[ch.partName] + scaleDelta;
            }

            // Position oscillation
            if (ch.posAmplitude.sqrMagnitude > 0.0001f)
            {
                float sinP = Mathf.Sin(t * ch.posFrequency * Mathf.PI * 2f + ch.phaseOffset);
                Vector3 posDelta = ch.posAmplitude * sinP;
                joint.localPosition = restPositions[ch.partName] + posDelta;
            }
        }
    }

    // ===============================================================
    // Attack Animations (Species-Specific)
    // ===============================================================

    private IEnumerator AttackCoroutine(Action onHit, Action onDone)
    {
        switch (speciesId)
        {
            case 1: yield return PyrexAttack(onHit); break;
            case 4: yield return AquadonAttack(onHit); break;
            case 7: yield return FlorasaurAttack(onHit); break;
            default: yield return GenericAttack(onHit); break;
        }

        // Blend back to rest and resume idle
        yield return BlendToRest(0.3f);
        currentState = AnimState.Idle;
        idleTime = 0f;
        activeCoroutine = null;
        onDone?.Invoke();
    }

    // ---- PYREX: Crouch + Lunge + Jaw Snap ----
    private IEnumerator PyrexAttack(Action onHit)
    {
        // Phase 1: Crouch (0.3s) — body dips, legs bend, tail raises
        yield return TweenMultiple(0.3f, new TweenTarget[]
        {
            new TweenTarget("Body", rot: new Vector3(12, 0, 0), pos: new Vector3(0, -0.06f, 0)),
            new TweenTarget("Leg_L", rot: new Vector3(15, 0, 0)),
            new TweenTarget("Leg_R", rot: new Vector3(15, 0, 0)),
            new TweenTarget("Tail_1", rot: new Vector3(-15, 0, 0)),
            new TweenTarget("Crest", scale: new Vector3(0.15f, 0.2f, 0.15f)),
        });

        // Phase 2: Lunge forward (0.2s) — root moves, head thrusts, jaw opens
        yield return TweenMultiple(0.2f, new TweenTarget[]
        {
            new TweenTarget("Body", rot: new Vector3(-8, 0, 0), pos: new Vector3(0, 0.04f, 0.5f)),
            new TweenTarget("Head", rot: new Vector3(-15, 0, 0)),
            new TweenTarget("Jaw", rot: new Vector3(30, 0, 0)),
            new TweenTarget("Arm_L", rot: new Vector3(-20, 0, -15)),
            new TweenTarget("Arm_R", rot: new Vector3(-20, 0, 15)),
        });

        // Phase 3: Impact — jaw snaps shut, crest flares
        onHit?.Invoke();
        yield return TweenMultiple(0.08f, new TweenTarget[]
        {
            new TweenTarget("Jaw", rot: new Vector3(-5, 0, 0)),
            new TweenTarget("Crest", scale: new Vector3(0.3f, 0.4f, 0.3f)),
            new TweenTarget("Flame", scale: new Vector3(0.3f, 0.3f, 0.3f)),
        });

        // Phase 4: Hold at impact
        yield return new WaitForSeconds(0.15f);

        // Phase 5: Return is handled by BlendToRest in AttackCoroutine
    }

    // ---- AQUADON: Neck Rear + Whip Strike ----
    private IEnumerator AquadonAttack(Action onHit)
    {
        // Phase 1: Rear back (0.4s) — neck curls backward, flippers flare
        yield return TweenMultiple(0.4f, new TweenTarget[]
        {
            new TweenTarget("Neck_1", rot: new Vector3(-20, 0, 0)),
            new TweenTarget("Neck_2", rot: new Vector3(-25, 0, 0)),
            new TweenTarget("Neck_3", rot: new Vector3(-20, 0, 0)),
            new TweenTarget("Neck_4", rot: new Vector3(-15, 0, 0)),
            new TweenTarget("Head", rot: new Vector3(-10, 0, 0)),
            new TweenTarget("Flipper_FL", rot: new Vector3(0, 0, -30)),
            new TweenTarget("Flipper_FR", rot: new Vector3(0, 0, 30)),
        });

        // Phase 2: Whip forward — cascade through neck segments
        yield return TweenJoint("Neck_1", new Vector3(35, 0, 0), 0.07f);
        yield return TweenJoint("Neck_2", new Vector3(40, 0, 0), 0.07f);
        yield return TweenJoint("Neck_3", new Vector3(35, 0, 0), 0.06f);
        yield return TweenJoint("Neck_4", new Vector3(30, 0, 0), 0.05f);

        // Phase 3: Head strike + impact
        yield return TweenJoint("Head", new Vector3(20, 0, 0), 0.05f);
        onHit?.Invoke();

        // Phase 4: Recoil
        yield return new WaitForSeconds(0.12f);
    }

    // ---- FLORASAUR: Rear Up + Earthquake Stomp ----
    private IEnumerator FlorasaurAttack(Action onHit)
    {
        // Phase 1: Gather (0.3s) — body lowers, flowers tighten
        yield return TweenMultiple(0.3f, new TweenTarget[]
        {
            new TweenTarget("Body", pos: new Vector3(0, -0.05f, 0)),
            new TweenTarget("Flower_1", rot: new Vector3(0, 0, -5)),
            new TweenTarget("Flower_2", rot: new Vector3(0, 0, -5)),
            new TweenTarget("Flower_3", rot: new Vector3(0, 0, -5)),
        });

        // Phase 2: Rear up (0.4s) — tilt back, front legs lift
        yield return TweenMultiple(0.4f, new TweenTarget[]
        {
            new TweenTarget("Body", rot: new Vector3(-25, 0, 0), pos: new Vector3(0, 0.15f, -0.1f)),
            new TweenTarget("Leg_FL", rot: new Vector3(-35, 0, 0)),
            new TweenTarget("Leg_FR", rot: new Vector3(-35, 0, 0)),
            new TweenTarget("Head", rot: new Vector3(15, 0, 0)),
            new TweenTarget("Flower_1", rot: new Vector3(0, 0, 15)),
            new TweenTarget("Flower_2", rot: new Vector3(0, 0, 12)),
            new TweenTarget("Flower_3", rot: new Vector3(0, 0, 10)),
            new TweenTarget("Tail_1", rot: new Vector3(-10, 0, 0)),
        });

        // Phase 3: Apex hold
        yield return new WaitForSeconds(0.2f);

        // Phase 4: STOMP (0.12s) — slam down fast
        yield return TweenMultiple(0.12f, new TweenTarget[]
        {
            new TweenTarget("Body", rot: new Vector3(8, 0, 0), pos: new Vector3(0, -0.08f, 0.05f)),
            new TweenTarget("Leg_FL", rot: new Vector3(10, 0, 0)),
            new TweenTarget("Leg_FR", rot: new Vector3(10, 0, 0)),
            new TweenTarget("Head", rot: new Vector3(-10, 0, 0)),
            new TweenTarget("Tail_1", rot: new Vector3(20, 0, 0)),
            new TweenTarget("Tail_2", rot: new Vector3(25, 0, 0)),
        });

        onHit?.Invoke();

        // Phase 5: Bounce settle
        yield return TweenMultiple(0.15f, new TweenTarget[]
        {
            new TweenTarget("Body", rot: new Vector3(-3, 0, 0), pos: new Vector3(0, 0.03f, 0)),
        });
        yield return new WaitForSeconds(0.1f);
    }

    // ---- GENERIC: Simple lunge ----
    private IEnumerator GenericAttack(Action onHit)
    {
        // Lunge forward
        Vector3 startPos = transform.localPosition;
        Vector3 targetPos = startPos + transform.forward * 0.8f;

        yield return TweenRootPosition(targetPos, 0.2f);

        if (joints.ContainsKey("Head"))
            yield return TweenJoint("Head", new Vector3(-15, 0, 0), 0.08f);

        onHit?.Invoke();
        yield return new WaitForSeconds(0.1f);

        yield return TweenRootPosition(startPos, 0.25f);
    }

    // ===============================================================
    // Hit Reaction
    // ===============================================================

    private IEnumerator HitCoroutine(Action onDone)
    {
        // Phase 1: Recoil (0.15s)
        yield return TweenMultiple(0.15f, new TweenTarget[]
        {
            new TweenTarget("Body", rot: new Vector3(8, 0, 0), pos: new Vector3(0, 0, -0.08f)),
            new TweenTarget("Head", rot: new Vector3(12, 0, 0)),
        });

        // Phase 2: Compress (0.15s)
        if (joints.ContainsKey("Body"))
        {
            var body = joints["Body"];
            Vector3 origScale = restScales.ContainsKey("Body") ? restScales["Body"] : body.localScale;
            body.localScale = new Vector3(origScale.x * 1.05f, origScale.y * 0.88f, origScale.z * 1.05f);
        }
        yield return new WaitForSeconds(0.15f);

        // Species flair
        if (speciesId == 1 && joints.ContainsKey("Crest"))
        {
            // Pyrex: flame dims briefly
            joints["Crest"].localScale = restScales.ContainsKey("Crest")
                ? restScales["Crest"] * 0.5f : joints["Crest"].localScale * 0.5f;
        }
        else if (speciesId == 4)
        {
            // Aquadon: neck wobbles
            for (int i = 1; i <= 4; i++)
            {
                string nk = $"Neck_{i}";
                if (joints.ContainsKey(nk))
                    joints[nk].localRotation = restRotations[nk] * Quaternion.Euler(
                        UnityEngine.Random.Range(-8f, 8f), UnityEngine.Random.Range(-5f, 5f), 0);
            }
        }

        // Phase 3: Recovery — blend back to rest
        yield return BlendToRest(0.25f);

        currentState = AnimState.Idle;
        idleTime = 0f;
        activeCoroutine = null;
        onDone?.Invoke();
    }

    // ===============================================================
    // Faint Animation
    // ===============================================================

    private IEnumerator FaintCoroutine(Action onDone)
    {
        // Phase 1: Stagger (0.3s) — oscillating sway
        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            float sway = Mathf.Sin(elapsed * 25f) * (1f - elapsed / 0.3f) * 10f;
            if (joints.ContainsKey("Body"))
                joints["Body"].localRotation = restRotations.ContainsKey("Body")
                    ? restRotations["Body"] * Quaternion.Euler(0, 0, sway)
                    : Quaternion.Euler(0, 0, sway);
            yield return null;
        }

        // Phase 2: Collapse (0.5s) — species-specific
        elapsed = 0f;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.5f;
            float eased = t * t; // ease-in for dramatic fall

            if (speciesId == 1) // Pyrex tips forward
            {
                if (joints.ContainsKey("Body"))
                    joints["Body"].localRotation = Quaternion.Euler(Mathf.Lerp(0, 70, eased), 0, Mathf.Lerp(0, 8, eased));
                if (joints.ContainsKey("Head"))
                    joints["Head"].localRotation = Quaternion.Euler(Mathf.Lerp(0, -20, eased), 0, 0);
                if (joints.ContainsKey("Tail_1"))
                    joints["Tail_1"].localRotation = Quaternion.Euler(Mathf.Lerp(0, -30, eased), 0, 0);
            }
            else if (speciesId == 4) // Aquadon neck goes limp
            {
                for (int i = 1; i <= 4; i++)
                {
                    string nk = $"Neck_{i}";
                    if (joints.ContainsKey(nk))
                    {
                        float delay = (i - 1) * 0.15f;
                        float localT = Mathf.Clamp01((t - delay) / (1f - delay));
                        joints[nk].localRotation = Quaternion.Euler(Mathf.Lerp(0, 40 + i * 5, localT), 0, Mathf.Lerp(0, i * 3, localT));
                    }
                }
                if (joints.ContainsKey("Body"))
                    joints["Body"].localPosition = restPositions.ContainsKey("Body")
                        ? restPositions["Body"] + Vector3.down * eased * 0.15f
                        : Vector3.down * eased * 0.15f;
            }
            else if (speciesId == 7) // Florasaur legs buckle
            {
                foreach (string leg in new[] { "Leg_FL", "Leg_FR", "Leg_RL", "Leg_RR" })
                {
                    if (joints.ContainsKey(leg))
                        joints[leg].localRotation = Quaternion.Euler(Mathf.Lerp(0, 45, eased), 0, 0);
                }
                if (joints.ContainsKey("Body"))
                    joints["Body"].localPosition = restPositions.ContainsKey("Body")
                        ? restPositions["Body"] + Vector3.down * eased * 0.25f
                        : Vector3.down * eased * 0.25f;
                // Flowers droop
                for (int i = 1; i <= 5; i++)
                {
                    string fl = $"Flower_{i}";
                    if (joints.ContainsKey(fl))
                        joints[fl].localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(0, 40, eased));
                }
            }
            else // Generic: tip to side
            {
                if (joints.ContainsKey("Body"))
                    joints["Body"].localRotation = Quaternion.Euler(Mathf.Lerp(0, 60, eased), 0, Mathf.Lerp(0, 20, eased));
            }

            yield return null;
        }

        // Phase 3: Settle bounce (0.2s)
        yield return new WaitForSeconds(0.2f);

        // Phase 4: Dim all renderers (0.5s)
        var renderers = GetComponentsInChildren<Renderer>();
        elapsed = 0f;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.5f;
            foreach (var r in renderers)
            {
                if (r.material != null)
                {
                    Color c = r.material.color;
                    r.material.color = Color.Lerp(c, c * 0.4f, t * 0.1f);
                }
            }
            yield return null;
        }

        activeCoroutine = null;
        onDone?.Invoke();
    }

    // ===============================================================
    // Tween Helpers
    // ===============================================================

    private struct TweenTarget
    {
        public string partName;
        public Vector3 targetRotation; // euler angles delta from rest
        public Vector3 targetPosition; // position delta from rest
        public Vector3 targetScale;    // scale delta from rest
        public bool hasRot, hasPos, hasScale;

        public TweenTarget(string name, Vector3 rot = default, Vector3 pos = default, Vector3 scale = default)
        {
            partName = name;
            targetRotation = rot;
            targetPosition = pos;
            targetScale = scale;
            hasRot = rot.sqrMagnitude > 0.0001f;
            hasPos = pos.sqrMagnitude > 0.0001f;
            hasScale = scale.sqrMagnitude > 0.0001f;
        }
    }

    private IEnumerator TweenMultiple(float duration, TweenTarget[] targets)
    {
        // Capture start states
        var starts = new (Quaternion rot, Vector3 pos, Vector3 scale)[targets.Length];
        for (int i = 0; i < targets.Length; i++)
        {
            if (joints.TryGetValue(targets[i].partName, out Transform j))
                starts[i] = (j.localRotation, j.localPosition, j.localScale);
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);

            for (int i = 0; i < targets.Length; i++)
            {
                if (!joints.TryGetValue(targets[i].partName, out Transform joint)) continue;
                var tgt = targets[i];
                var rest = starts[i];

                if (tgt.hasRot)
                {
                    Quaternion restRot = restRotations.ContainsKey(tgt.partName)
                        ? restRotations[tgt.partName] : rest.rot;
                    Quaternion target = restRot * Quaternion.Euler(tgt.targetRotation);
                    joint.localRotation = Quaternion.Slerp(rest.rot, target, t);
                }

                if (tgt.hasPos)
                {
                    Vector3 restPos = restPositions.ContainsKey(tgt.partName)
                        ? restPositions[tgt.partName] : rest.pos;
                    Vector3 target = restPos + tgt.targetPosition;
                    joint.localPosition = Vector3.Lerp(rest.pos, target, t);
                }

                if (tgt.hasScale)
                {
                    Vector3 restScl = restScales.ContainsKey(tgt.partName)
                        ? restScales[tgt.partName] : rest.scale;
                    Vector3 target = restScl + tgt.targetScale;
                    joint.localScale = Vector3.Lerp(rest.scale, target, t);
                }
            }
            yield return null;
        }
    }

    private IEnumerator TweenJoint(string partName, Vector3 targetRotEuler, float duration)
    {
        if (!joints.TryGetValue(partName, out Transform joint)) yield break;

        Quaternion startRot = joint.localRotation;
        Quaternion restRot = restRotations.ContainsKey(partName) ? restRotations[partName] : startRot;
        Quaternion endRot = restRot * Quaternion.Euler(targetRotEuler);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            joint.localRotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }
        joint.localRotation = endRot;
    }

    private IEnumerator TweenRootPosition(Vector3 target, float duration)
    {
        Vector3 start = transform.localPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            transform.localPosition = Vector3.Lerp(start, target, t);
            yield return null;
        }
        transform.localPosition = target;
    }

    private IEnumerator BlendToRest(float duration)
    {
        // Capture current state of ALL joints (NOT the root transform)
        var currentStates = new Dictionary<string, (Quaternion rot, Vector3 pos, Vector3 scale)>();
        foreach (var kvp in joints)
        {
            currentStates[kvp.Key] = (kvp.Value.localRotation, kvp.Value.localPosition, kvp.Value.localScale);
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);

            foreach (var kvp in joints)
            {
                if (currentStates.ContainsKey(kvp.Key))
                {
                    var current = currentStates[kvp.Key];
                    if (restRotations.ContainsKey(kvp.Key))
                        kvp.Value.localRotation = Quaternion.Slerp(current.rot, restRotations[kvp.Key], t);
                    if (restPositions.ContainsKey(kvp.Key))
                        kvp.Value.localPosition = Vector3.Lerp(current.pos, restPositions[kvp.Key], t);
                    if (restScales.ContainsKey(kvp.Key))
                        kvp.Value.localScale = Vector3.Lerp(current.scale, restScales[kvp.Key], t);
                }
            }

            yield return null;
        }

        ResetJointsToRest();
    }

    /// <summary>
    /// Reset all joints to their rest pose. Does NOT move the root transform.
    /// </summary>
    private void ResetJointsToRest()
    {
        foreach (var kvp in joints)
        {
            if (restRotations.ContainsKey(kvp.Key))
                kvp.Value.localRotation = restRotations[kvp.Key];
            if (restPositions.ContainsKey(kvp.Key))
                kvp.Value.localPosition = restPositions[kvp.Key];
            if (restScales.ContainsKey(kvp.Key))
                kvp.Value.localScale = restScales[kvp.Key];
        }
    }
}
