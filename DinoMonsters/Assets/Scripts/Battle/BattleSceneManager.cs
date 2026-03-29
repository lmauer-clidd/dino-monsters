// ============================================================
// Dino Monsters -- Battle Scene Manager (3D MonoBehaviour Bridge)
// ============================================================
//
// Main MonoBehaviour that bridges the pure BattleSystem logic
// with the 3D battle scene. Handles arena creation, dino spawning,
// turn flow, animations, and scene transitions.
// ============================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleSceneManager : MonoBehaviour
{
    // --------------- Scene References ---------------

    private BattleSystem battleSystem;
    private BattleUI battleUI;
    private BattleEffects3D effects;

    // --- Arena objects ---
    private GameObject arenaRoot;
    private GameObject groundPlane;
    private Light mainLight;
    private Camera battleCamera;

    // --- Dino objects ---
    private GameObject playerDinoModel;
    private GameObject enemyDinoModel;

    // --- Battle data ---
    private BattleSetupData setupData;
    private Dino playerDino;
    private Dino enemyDino;

    // --- Positions ---
    private static readonly Vector3 PLAYER_DINO_POS = new Vector3(-2.2f, 0.75f, 0f);
    private static readonly Vector3 ENEMY_DINO_POS = new Vector3(2.2f, 0.75f, 0f);
    private static readonly Vector3 CAMERA_POS = new Vector3(-1f, 4f, -5.5f);
    private static readonly Vector3 CAMERA_LOOK_AT = new Vector3(0.5f, 0.5f, 0f);

    // --- State ---
    private bool isAnimating;
    private bool battleEnded;

    // --- Trainer party tracking (for gym leaders / multi-dino trainers) ---
    private int trainerPartyIndex = 0;
    private List<Dino> trainerPartyDinos;

    // ===============================================================
    // Unity Lifecycle
    // ===============================================================

    void Start()
    {
        SetupBattle();
    }

    // ===============================================================
    // SetupBattle — Read setup data, create arena, spawn dinos
    // ===============================================================

    private void SetupBattle()
    {
        // Read setup data from scene transition
        setupData = SceneTransitionData.Instance as BattleSetupData;
        if (setupData == null)
        {
            Debug.LogError("[BattleSceneManager] No BattleSetupData found! Creating debug battle.");
            setupData = CreateDebugSetup();
        }

        // Get player dino from party
        playerDino = GameState.Instance.GetLeadDino();
        if (playerDino == null)
        {
            Debug.LogError("[BattleSceneManager] No lead dino in party!");
            return;
        }

        // Create enemy dino(s)
        if (setupData.trainerParty != null && setupData.trainerParty.Count > 0)
        {
            // Multi-dino trainer battle (gym leaders, etc.)
            trainerPartyDinos = new List<Dino>();
            foreach (var entry in setupData.trainerParty)
            {
                trainerPartyDinos.Add(Dino.CreateWild(entry.speciesId, entry.level));
            }
            trainerPartyIndex = 0;
            enemyDino = trainerPartyDinos[0];
        }
        else
        {
            enemyDino = Dino.CreateWild(setupData.enemySpeciesId, setupData.enemyLevel);
        }

        // Initialize pure battle system
        battleSystem = new BattleSystem();
        battleSystem.StartBattle(playerDino, enemyDino, setupData.isWild, setupData.trainerName);

        // Create the 3D arena
        CreateArena();

        // Setup camera
        SetupCamera();

        // Spawn dino models
        SpawnDinos();

        // Create effects system
        var effectsGO = new GameObject("BattleEffects");
        effects = effectsGO.AddComponent<BattleEffects3D>();

        // Create UI overlay
        CreateBattleUI();

        // Register dinodex entry
        GameState.Instance.RegisterSeen(setupData.enemySpeciesId);

        // Play battle music
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBattleMusic();

        // Start the intro sequence
        StartCoroutine(BattleIntroSequence());
    }

    private BattleSetupData CreateDebugSetup()
    {
        return new BattleSetupData
        {
            isWild = true,
            enemySpeciesId = 1,
            enemyLevel = 5,
            trainerName = null,
            returnScene = "Overworld",
            returnPosition = Vector2.zero
        };
    }

    // ===============================================================
    // Arena Creation
    // ===============================================================

    private void CreateArena()
    {
        arenaRoot = new GameObject("BattleArena");

        // --- Ground with checkerboard pattern ---
        CreateCheckerboardGround();

        // --- Raised platform pedestals under each dino ---
        CreatePlatform("PlayerPlatform", PLAYER_DINO_POS, new Color(0.40f, 0.52f, 0.30f));
        CreatePlatform("EnemyPlatform", ENEMY_DINO_POS, new Color(0.52f, 0.40f, 0.30f));

        // --- Arena border decorations (rocks at edges) ---
        CreateArenaBorder();

        // --- Gradient sky ---
        CreateSkyGradient();

        // --- Main directional light (warm yellow sunlight) ---
        var lightGO = new GameObject("BattleLight");
        lightGO.transform.SetParent(arenaRoot.transform);
        mainLight = lightGO.AddComponent<Light>();
        mainLight.type = LightType.Directional;
        mainLight.color = new Color(1f, 0.95f, 0.82f);
        mainLight.intensity = 1.3f;
        mainLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        mainLight.shadows = LightShadows.Soft;

        // --- Fill light (cool blue, opposite side) ---
        var fillLightGO = new GameObject("FillLight");
        fillLightGO.transform.SetParent(arenaRoot.transform);
        var fillLight = fillLightGO.AddComponent<Light>();
        fillLight.type = LightType.Directional;
        fillLight.color = new Color(0.55f, 0.65f, 0.92f);
        fillLight.intensity = 0.45f;
        fillLight.transform.rotation = Quaternion.Euler(30f, 150f, 0f);
        fillLight.shadows = LightShadows.None;

        // --- Rim light (from behind, highlights dino edges) ---
        var rimLightGO = new GameObject("RimLight");
        rimLightGO.transform.SetParent(arenaRoot.transform);
        var rimLight = rimLightGO.AddComponent<Light>();
        rimLight.type = LightType.Directional;
        rimLight.color = new Color(1f, 0.92f, 0.75f);
        rimLight.intensity = 0.35f;
        rimLight.transform.rotation = Quaternion.Euler(-10f, 180f, 0f);
        rimLight.shadows = LightShadows.None;

        // --- Ambient ---
        RenderSettings.ambientLight = new Color(0.45f, 0.45f, 0.55f);
    }

    /// <summary>Checkerboard ground with two slightly different green shades.</summary>
    private void CreateCheckerboardGround()
    {
        Color colorA = new Color(0.42f, 0.54f, 0.28f);
        Color colorB = new Color(0.48f, 0.58f, 0.32f);
        int tilesX = 12;
        int tilesZ = 8;
        float tileSize = 2.5f;
        float startX = -tilesX * tileSize * 0.5f;
        float startZ = -tilesZ * tileSize * 0.5f;

        for (int tx = 0; tx < tilesX; tx++)
        {
            for (int tz = 0; tz < tilesZ; tz++)
            {
                var tile = GameObject.CreatePrimitive(PrimitiveType.Plane);
                tile.name = $"GroundTile_{tx}_{tz}";
                tile.transform.SetParent(arenaRoot.transform);
                tile.transform.position = new Vector3(
                    startX + tx * tileSize + tileSize * 0.5f,
                    0f,
                    startZ + tz * tileSize + tileSize * 0.5f);
                tile.transform.localScale = new Vector3(tileSize * 0.1f, 1f, tileSize * 0.1f);

                bool isEven = (tx + tz) % 2 == 0;
                var r = tile.GetComponent<Renderer>();
                r.sharedMaterial = MaterialManager.GetSolidColor(isEven ? colorA : colorB);

                var col = tile.GetComponent<Collider>();
                if (col != null) Destroy(col);
            }
        }

        // Keep a reference for cleanup
        groundPlane = new GameObject("GroundRef");
        groundPlane.transform.SetParent(arenaRoot.transform);
    }

    /// <summary>Gradient sky: camera blue background + horizon glow quads.</summary>
    private void CreateSkyGradient()
    {
        if (Camera.main != null)
        {
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = new Color(0.45f, 0.65f, 0.92f);
        }

        // Horizon glow (large translucent quad behind arena)
        var horizon = GameObject.CreatePrimitive(PrimitiveType.Quad);
        horizon.name = "HorizonGlow";
        horizon.transform.SetParent(arenaRoot.transform);
        horizon.transform.position = new Vector3(0, 2f, 20f);
        horizon.transform.localScale = new Vector3(60f, 12f, 1f);
        var hr = horizon.GetComponent<Renderer>();
        hr.sharedMaterial = MaterialManager.GetTransparent(new Color(0.85f, 0.9f, 1f), 0.4f);
        Destroy(horizon.GetComponent<Collider>());
    }

    /// <summary>Decorative rocks/pillars around the arena perimeter.</summary>
    private void CreateArenaBorder()
    {
        float arenaRadius = 12f;
        int count = 10;
        for (int i = 0; i < count; i++)
        {
            float angle = (i / (float)count) * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * arenaRadius;
            float z = Mathf.Sin(angle) * arenaRadius;

            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = $"ArenaPillar_{i}";
            pillar.transform.SetParent(arenaRoot.transform);

            float height = 0.3f + Mathf.Abs(Mathf.Sin(angle * 3)) * 0.5f;
            float radius = 0.15f + Mathf.Abs(Mathf.Cos(angle * 2)) * 0.15f;
            pillar.transform.position = new Vector3(x, height * 0.5f, z);
            pillar.transform.localScale = new Vector3(radius * 2, height, radius * 2);

            var pr = pillar.GetComponent<Renderer>();
            Color rockCol = new Color(
                0.50f + Mathf.Sin(i * 1.5f) * 0.08f,
                0.47f + Mathf.Cos(i * 1.2f) * 0.06f,
                0.40f + Mathf.Sin(i * 0.8f) * 0.05f);
            pr.sharedMaterial = MaterialManager.GetSolidColor(rockCol);
            Destroy(pillar.GetComponent<Collider>());
        }
    }

    private void CreatePlatform(string name, Vector3 position, Color color)
    {
        // Raised pedestal disc
        var platform = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        platform.name = name;
        platform.transform.SetParent(arenaRoot.transform);
        platform.transform.position = new Vector3(position.x, 0.06f, position.z);
        platform.transform.localScale = new Vector3(2.2f, 0.06f, 2.2f);

        var renderer = platform.GetComponent<Renderer>();
        renderer.sharedMaterial = MaterialManager.GetSolidColor(color);

        var collider = platform.GetComponent<Collider>();
        if (collider != null) Destroy(collider);

        // Inner lighter ring
        var innerRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        innerRing.name = $"{name}_Inner";
        innerRing.transform.SetParent(arenaRoot.transform);
        innerRing.transform.position = new Vector3(position.x, 0.07f, position.z);
        innerRing.transform.localScale = new Vector3(1.6f, 0.02f, 1.6f);

        var innerR = innerRing.GetComponent<Renderer>();
        Color innerColor = Color.Lerp(color, Color.white, 0.25f);
        innerR.sharedMaterial = MaterialManager.GetSolidColor(innerColor);
        var innerCol = innerRing.GetComponent<Collider>();
        if (innerCol != null) Destroy(innerCol);

        // Shadow under platform
        var shadow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shadow.name = $"{name}_Shadow";
        shadow.transform.SetParent(arenaRoot.transform);
        shadow.transform.position = new Vector3(position.x, 0.005f, position.z);
        shadow.transform.localScale = new Vector3(2.5f, 0.005f, 2.5f);

        var sr = shadow.GetComponent<Renderer>();
        sr.sharedMaterial = MaterialManager.GetTransparent(Color.black, 0.15f);
        var shadowCol = shadow.GetComponent<Collider>();
        if (shadowCol != null) Destroy(shadowCol);
    }

    // ===============================================================
    // Camera Setup
    // ===============================================================

    private void SetupCamera()
    {
        battleCamera = Camera.main;
        if (battleCamera == null)
        {
            var camGO = new GameObject("BattleCamera");
            battleCamera = camGO.AddComponent<Camera>();
        }

        battleCamera.transform.position = CAMERA_POS;
        battleCamera.transform.LookAt(CAMERA_LOOK_AT);
        battleCamera.fieldOfView = 45f;
        battleCamera.clearFlags = CameraClearFlags.SolidColor;
        battleCamera.backgroundColor = new Color(0.45f, 0.65f, 0.92f);
    }

    // ===============================================================
    // Spawn Dinos
    // ===============================================================

    private void SpawnDinos()
    {
        // Player dino (left side, facing right toward enemy) — start off-screen for intro slide
        playerDinoModel = DinoModelGenerator.CreateDinoModel(playerDino.speciesId, false);
        playerDinoModel.transform.position = PLAYER_DINO_POS + Vector3.left * 8f;
        playerDinoModel.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

        // Attach procedural animator and start idle
        var playerAnim = playerDinoModel.AddComponent<DinoAnimator>();
        playerAnim.PlayIdle();

        // Enemy dino (right side, facing left toward player) — start off-screen for intro slide
        enemyDinoModel = DinoModelGenerator.CreateDinoModel(enemyDino.speciesId, true);
        enemyDinoModel.transform.position = ENEMY_DINO_POS + Vector3.right * 8f;
        enemyDinoModel.transform.rotation = Quaternion.Euler(0f, -90f, 0f);

        // Attach procedural animator and start idle
        var enemyAnim = enemyDinoModel.AddComponent<DinoAnimator>();
        enemyAnim.PlayIdle();
    }

    // ===============================================================
    // Create Battle UI
    // ===============================================================

    private void CreateBattleUI()
    {
        var uiGO = new GameObject("BattleUI");
        battleUI = uiGO.AddComponent<BattleUI>();

        // Set callbacks
        battleUI.OnAttackSelected = OnPlayerAttack;
        battleUI.OnBagSelected = OnPlayerBag;
        battleUI.OnDinoSelected = OnPlayerDino;
        battleUI.OnRunSelected = OnPlayerRun;

        // Initialize dino info panels
        battleUI.SetPlayerInfo(playerDino.nickname, playerDino.level, playerDino.currentHp, playerDino.maxHp);
        battleUI.SetEnemyInfo(enemyDino.nickname, enemyDino.level, enemyDino.currentHp, enemyDino.maxHp);
        battleUI.UpdateXP(playerDino.GetXpPercent());

        if (playerDino.status != StatusEffect.None)
            battleUI.SetStatus(true, playerDino.status);
        if (enemyDino.status != StatusEffect.None)
            battleUI.SetStatus(false, enemyDino.status);
    }

    // ===============================================================
    // Battle Intro Sequence
    // ===============================================================

    private IEnumerator BattleIntroSequence()
    {
        isAnimating = true;

        // Fade in from black (scene was loaded with screen faded out)
        if (ScreenFade.Instance != null)
            yield return ScreenFade.Instance.FadeInCoroutine(0.3f);

        // Camera shake on battle entry
        yield return StartCoroutine(CameraShake(0.3f, 0.1f));

        // Slide dinos in from off-screen
        Vector3 playerStart = PLAYER_DINO_POS + Vector3.left * 8f;
        Vector3 enemyStart = ENEMY_DINO_POS + Vector3.right * 8f;

        playerDinoModel.transform.position = playerStart;
        enemyDinoModel.transform.position = enemyStart;

        float duration = 0.8f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            playerDinoModel.transform.position = Vector3.Lerp(playerStart, PLAYER_DINO_POS, t);
            enemyDinoModel.transform.position = Vector3.Lerp(enemyStart, ENEMY_DINO_POS, t);
            yield return null;
        }

        playerDinoModel.transform.position = PLAYER_DINO_POS;
        enemyDinoModel.transform.position = ENEMY_DINO_POS;

        // Show encounter message
        string introMsg;
        if (setupData.isWild)
            introMsg = $"Un {enemyDino.nickname} sauvage apparait!";
        else
            introMsg = $"{setupData.trainerName} envoie {enemyDino.nickname}!";

        bool msgDone = false;
        battleUI.ShowMessage(introMsg, () => msgDone = true);
        yield return new WaitUntil(() => msgDone);

        yield return new WaitForSeconds(0.3f);

        msgDone = false;
        battleUI.ShowMessage($"A toi, {playerDino.nickname}!", () => msgDone = true);
        yield return new WaitUntil(() => msgDone);

        yield return new WaitForSeconds(0.3f);

        isAnimating = false;

        // Start the first player turn
        StartPlayerTurn();
    }

    // ===============================================================
    // Turn Flow
    // ===============================================================

    public void StartPlayerTurn()
    {
        if (battleEnded) return;

        // Update UI with current HP values
        battleUI.SetPlayerInfo(playerDino.nickname, playerDino.level, playerDino.currentHp, playerDino.maxHp);
        battleUI.SetEnemyInfo(enemyDino.nickname, enemyDino.level, enemyDino.currentHp, enemyDino.maxHp);

        battleUI.ShowActionMenu();
    }

    // ===============================================================
    // Player Actions
    // ===============================================================

    public void OnPlayerAttack(int moveIndex)
    {
        if (isAnimating || battleEnded) return;
        battleUI.HideActionMenu();
        battleUI.HideMoveMenu();
        StartCoroutine(ExecuteTurnCoroutine(moveIndex));
    }

    public void OnPlayerBag()
    {
        if (isAnimating || battleEnded) return;
        // For now show a simple message — bag system integration
        StartCoroutine(ShowBagMenu());
    }

    public void OnPlayerDino()
    {
        if (isAnimating || battleEnded) return;
        StartCoroutine(ShowDinoSwitchMenu());
    }

    public void OnPlayerRun()
    {
        if (isAnimating || battleEnded) return;
        battleUI.HideActionMenu();
        StartCoroutine(AttemptFleeCoroutine());
    }

    // ===============================================================
    // Execute Turn (Player attacks)
    // ===============================================================

    private IEnumerator ExecuteTurnCoroutine(int moveIndex)
    {
        isAnimating = true;

        // Execute full turn through BattleSystem
        List<BattleResult> results = battleSystem.ExecuteTurn(moveIndex);

        // Determine who went first
        int playerSpeed = playerDino.stats.speed;
        int enemySpeed = enemyDino.stats.speed;
        if (playerDino.status == StatusEffect.Paralysis) playerSpeed /= 2;
        if (enemyDino.status == StatusEffect.Paralysis) enemySpeed /= 2;
        bool playerFirst = playerSpeed >= enemySpeed;

        // Play out each result sequentially
        for (int i = 0; i < results.Count; i++)
        {
            var result = results[i];
            bool isPlayerAction;

            // Determine which side performed this action
            if (playerFirst)
                isPlayerAction = (i == 0);
            else
                isPlayerAction = (i == 1);

            // For end-of-turn status results (index 2+), determine from message
            if (i >= 2)
            {
                isPlayerAction = result.message.Contains(playerDino.nickname);
            }

            yield return StartCoroutine(PlayResultCoroutine(isPlayerAction, result));

            // Check for fainting after each result
            if (result.targetFainted)
            {
                bool targetIsPlayer = !isPlayerAction;
                // For status damage, the target is the one mentioned in the message
                if (i >= 2)
                    targetIsPlayer = result.message.Contains(playerDino.nickname);

                yield return StartCoroutine(AnimateFaint(targetIsPlayer, null));
                yield return new WaitForSeconds(0.5f);
            }
        }

        // Update status icons
        battleUI.SetStatus(true, playerDino.status);
        battleUI.SetStatus(false, enemyDino.status);

        // Check battle end
        if (battleSystem.State == BattleState.Won)
        {
            yield return StartCoroutine(HandleVictory());
        }
        else if (battleSystem.State == BattleState.Lost)
        {
            yield return StartCoroutine(HandleDefeat());
        }
        else
        {
            isAnimating = false;
            StartPlayerTurn();
        }
    }

    private IEnumerator PlayResultCoroutine(bool isPlayerAction, BattleResult result)
    {
        // Show message with typewriter effect
        bool msgDone = false;
        battleUI.ShowMessage(result.message, () => msgDone = true);
        yield return new WaitUntil(() => msgDone);

        // If there was damage, animate attack
        if (result.damage > 0)
        {
            yield return StartCoroutine(AnimateAttack(isPlayerAction, result, null));

            // Update only the target's HP bar (the one that took damage)
            if (isPlayerAction)
                battleUI.UpdateHP(false, enemyDino.currentHp, enemyDino.maxHp, true);
            else
                battleUI.UpdateHP(true, playerDino.currentHp, playerDino.maxHp, true);

            // Wait for HP bar animation
            yield return new WaitForSeconds(0.6f);

            // Show damage number on the target
            Vector3 dmgPos = isPlayerAction ? ENEMY_DINO_POS : PLAYER_DINO_POS;
            dmgPos.y += 1.5f;
            battleUI.ShowDamageNumber(dmgPos, result.damage, result.effectiveness);
        }

        yield return new WaitForSeconds(0.3f);
    }

    // ===============================================================
    // Animations
    // ===============================================================

    public IEnumerator AnimateAttack(bool isPlayer, BattleResult result, Action onDone)
    {
        GameObject attacker = isPlayer ? playerDinoModel : enemyDinoModel;
        GameObject target = isPlayer ? enemyDinoModel : playerDinoModel;

        if (attacker == null || target == null)
        {
            onDone?.Invoke();
            yield break;
        }

        Vector3 attackerOrigin = attacker.transform.position;
        Vector3 targetPos = target.transform.position;

        // --- Try articulated attack via DinoAnimator ---
        var attackerAnim = attacker.GetComponent<DinoAnimator>();
        var targetAnim = target.GetComponent<DinoAnimator>();

        if (attackerAnim != null)
        {
            // Articulated attack — the animator handles wind-up, strike, return
            bool hitFired = false;
            bool done = false;

            attackerAnim.PlayAttack(
                onHit: () =>
                {
                    hitFired = true;

                    // Type-specific VFX particles
                    int moveType = result.moveType;
                    int movePower = result.movePower > 0 ? result.movePower : 50;
                    StartCoroutine(BattleEffects3D.TypedAttack(this, moveType, attackerOrigin, targetPos, movePower));

                    // Hit flash + SFX
                    effects.HitFlash(target);
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayHit();

                    // Target hit reaction (articulated flinch)
                    if (targetAnim != null) targetAnim.PlayHit(null);

                    // Critical / super effective extras
                    if (result.isCritical)
                    {
                        effects.CriticalEffect(targetPos + Vector3.up * 0.5f);
                        if (AudioManager.Instance != null) AudioManager.Instance.PlayCritical();
                    }
                    if (result.effectiveness > 1f)
                    {
                        effects.SuperEffectiveEffect(targetPos + Vector3.up * 0.5f);
                        if (AudioManager.Instance != null) AudioManager.Instance.PlaySuperEffective();
                    }

                    // Camera shake
                    StartCoroutine(CameraShake(result.isCritical ? 0.3f : 0.15f, result.isCritical ? 0.15f : 0.08f));
                },
                onDone: () => { done = true; }
            );

            // Wait for attack animation to complete
            while (!done) yield return null;

            // Safety: ensure hit always fires
            if (!hitFired)
            {
                effects.HitFlash(target);
                if (AudioManager.Instance != null) AudioManager.Instance.PlayHit();
            }
        }
        else
        {
            // --- Legacy lunge animation (for non-articulated dinos) ---
            Vector3 lungeTarget = Vector3.Lerp(attackerOrigin, targetPos, 0.5f);
            float lungeTime = 0.15f;
            float elapsed = 0f;

            while (elapsed < lungeTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / lungeTime;
                attacker.transform.position = Vector3.Lerp(attackerOrigin, lungeTarget, t);
                yield return null;
            }

            // Impact
            int moveType = result.moveType;
            int movePower = result.movePower > 0 ? result.movePower : 50;
            yield return StartCoroutine(BattleEffects3D.TypedAttack(this, moveType, attackerOrigin, targetPos, movePower));
            effects.HitFlash(target);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayHit();

            if (result.isCritical)
            {
                effects.CriticalEffect(targetPos + Vector3.up * 0.5f);
                if (AudioManager.Instance != null) AudioManager.Instance.PlayCritical();
            }
            if (result.effectiveness > 1f)
            {
                effects.SuperEffectiveEffect(targetPos + Vector3.up * 0.5f);
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySuperEffective();
            }

            yield return StartCoroutine(CameraShake(result.isCritical ? 0.3f : 0.15f, result.isCritical ? 0.15f : 0.08f));

            // Return to position
            elapsed = 0f;
            float returnTime = 0.2f;
            Vector3 currentPos = attacker.transform.position;
            while (elapsed < returnTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / returnTime);
                attacker.transform.position = Vector3.Lerp(currentPos, attackerOrigin, t);
                yield return null;
            }
            attacker.transform.position = attackerOrigin;
        }

        onDone?.Invoke();
    }

    public IEnumerator AnimateFaint(bool isPlayer, Action onDone)
    {
        GameObject dinoModel = isPlayer ? playerDinoModel : enemyDinoModel;
        if (dinoModel == null)
        {
            onDone?.Invoke();
            yield break;
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayFaint();

        // Try articulated faint, fallback to legacy
        var animator = dinoModel.GetComponent<DinoAnimator>();
        if (animator != null)
        {
            bool done = false;
            animator.PlayFaint(() => { done = true; });
            while (!done) yield return null;
        }
        else
        {
            effects.FaintAnimation(dinoModel);
            yield return new WaitForSeconds(1.2f);
        }

        onDone?.Invoke();
    }

    private IEnumerator CameraShake(float duration, float magnitude)
    {
        if (battleCamera == null) yield break;

        Vector3 originalPos = battleCamera.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float x = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            float y = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            battleCamera.transform.position = originalPos + new Vector3(x, y, 0f);
            yield return null;
        }

        battleCamera.transform.position = originalPos;
    }

    /// <summary>
    /// Animate the ball shaking during a capture attempt.
    /// Each shake tilts the enemy dino model left/right before it pops back.
    /// </summary>
    private IEnumerator AnimateBallShake(int shakes)
    {
        if (enemyDinoModel == null) yield break;

        // ---- Ball throw animation ----
        // Create ball at player dino position
        var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(ball.GetComponent<Collider>());
        ball.transform.localScale = Vector3.one * 0.25f;
        ball.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetSolidColor(Color.white);
        ball.transform.position = PLAYER_DINO_POS + Vector3.up * 0.5f;

        // Red bottom half (pokeball style)
        var ballBottom = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(ballBottom.GetComponent<Collider>());
        ballBottom.transform.SetParent(ball.transform);
        ballBottom.transform.localPosition = new Vector3(0, -0.15f, 0);
        ballBottom.transform.localScale = new Vector3(0.95f, 0.5f, 0.95f);
        ballBottom.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetSolidColor(new Color(0.85f, 0.20f, 0.20f));
        // Center button
        var ballBtn = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(ballBtn.GetComponent<Collider>());
        ballBtn.transform.SetParent(ball.transform);
        ballBtn.transform.localPosition = new Vector3(0, 0, 0.45f);
        ballBtn.transform.localScale = Vector3.one * 0.25f;
        ballBtn.GetComponent<Renderer>().sharedMaterial = MaterialManager.GetSolidColor(new Color(0.2f, 0.2f, 0.2f));

        // Animate ball arc from player to enemy
        Vector3 ballStart = PLAYER_DINO_POS + Vector3.up * 0.5f;
        Vector3 ballEnd = ENEMY_DINO_POS + Vector3.up * 0.5f;
        float throwDuration = 0.6f;
        float elapsed = 0f;

        while (elapsed < throwDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / throwDuration;
            // Parabolic arc
            Vector3 pos = Vector3.Lerp(ballStart, ballEnd, t);
            pos.y += Mathf.Sin(t * Mathf.PI) * 2.0f; // arc height
            ball.transform.position = pos;
            // Spin the ball
            ball.transform.Rotate(Vector3.forward, 600f * Time.deltaTime);
            yield return null;
        }

        // Ball hits — flash effect
        if (effects != null)
            effects.CaptureEffect(ENEMY_DINO_POS + Vector3.up * 0.5f);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayHit();

        yield return new WaitForSeconds(0.2f);

        // Hide enemy (sucked into ball)
        enemyDinoModel.SetActive(false);

        // Ball drops to ground
        ball.transform.position = ENEMY_DINO_POS + Vector3.up * 0.3f;
        ball.transform.rotation = Quaternion.identity;

        // ---- Ball shake animation ----
        Vector3 ballRestPos = ball.transform.position;
        for (int i = 0; i < shakes; i++)
        {
            yield return new WaitForSeconds(0.5f);

            // Wobble left
            elapsed = 0f;
            while (elapsed < 0.12f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.12f;
                ball.transform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(t * Mathf.PI) * 25f);
                yield return null;
            }
            // Wobble right
            elapsed = 0f;
            while (elapsed < 0.12f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.12f;
                ball.transform.rotation = Quaternion.Euler(0, 0, -Mathf.Sin(t * Mathf.PI) * 25f);
                yield return null;
            }
            ball.transform.rotation = Quaternion.identity;

            yield return StartCoroutine(CameraShake(0.1f, 0.03f));
        }

        // If not all 4 shakes, the dino breaks free
        if (shakes < 4)
        {
            yield return new WaitForSeconds(0.2f);
            // Ball breaks — destroy it
            Destroy(ball);
            enemyDinoModel.SetActive(true);
            if (effects != null)
                effects.HitFlash(enemyDinoModel);
        }
        else
        {
            // Capture success — ball clicks shut, sparkle
            yield return new WaitForSeconds(0.3f);
            // Shrink ball slightly (click)
            ball.transform.localScale = Vector3.one * 0.20f;
            yield return new WaitForSeconds(0.1f);
            ball.transform.localScale = Vector3.one * 0.25f;

            if (effects != null)
                effects.LevelUpEffect(ball.transform); // sparkle effect

            yield return new WaitForSeconds(0.5f);
            Destroy(ball);
        }
    }

    // ===============================================================
    // Bag Menu
    // ===============================================================

    private IEnumerator ShowBagMenu()
    {
        isAnimating = true;
        battleUI.HideActionMenu();

        // Get usable items grouped by category
        var inventory = GameState.Instance.Inventory;
        var allItems = inventory.GetAllItems();

        // Group items into categories
        var healingItems = new List<KeyValuePair<int, int>>();
        var captureItems = new List<KeyValuePair<int, int>>();
        var statusItems = new List<KeyValuePair<int, int>>();

        foreach (var kvp in allItems)
        {
            var itemData = DataLoader.Instance.GetItem(kvp.Key);
            if (itemData == null || !itemData.usableInBattle) continue;

            string effect = itemData.effect ?? "";
            if (effect == "capture")
                captureItems.Add(kvp);
            else if (effect == "heal_status" || effect == "full_heal")
                statusItems.Add(kvp);
            else if (effect == "heal_hp" || effect == "heal_pp" || effect == "heal_pp_all" || effect == "revive")
                healingItems.Add(kvp);
            else
                healingItems.Add(kvp); // default to healing category
        }

        bool hasAnyItems = healingItems.Count > 0 || captureItems.Count > 0 || statusItems.Count > 0;

        if (!hasAnyItems)
        {
            bool msgDone = false;
            battleUI.ShowMessage("Aucun objet utilisable!", () => msgDone = true);
            yield return new WaitUntil(() => msgDone);
            isAnimating = false;
            StartPlayerTurn();
            yield break;
        }

        // Show category menu first
        if (DialogueUI.Instance == null)
        {
            var dgo = new GameObject("DialogueUI");
            dgo.AddComponent<DialogueUI>();
        }

        // Build category choices (only show categories that have items)
        var categoryNames = new List<string>();
        var categoryLists = new List<List<KeyValuePair<int, int>>>();

        if (healingItems.Count > 0)
        {
            categoryNames.Add($"Soins ({healingItems.Count})");
            categoryLists.Add(healingItems);
        }
        if (captureItems.Count > 0)
        {
            categoryNames.Add($"Capture ({captureItems.Count})");
            categoryLists.Add(captureItems);
        }
        if (statusItems.Count > 0)
        {
            categoryNames.Add($"Statut ({statusItems.Count})");
            categoryLists.Add(statusItems);
        }
        categoryNames.Add("RETOUR");

        int categoryIndex = -1;
        DialogueUI.Instance.ShowChoices("Quelle poche?", categoryNames.ToArray(), (idx) => {
            categoryIndex = idx;
        });
        yield return new WaitUntil(() => categoryIndex >= 0);

        // RETOUR from category menu
        if (categoryIndex >= categoryLists.Count)
        {
            isAnimating = false;
            StartPlayerTurn();
            yield break;
        }

        // Show items in chosen category
        var selectedCategory = categoryLists[categoryIndex];
        int maxShow = Mathf.Min(selectedCategory.Count, 4);
        var choiceNames = new List<string>();
        for (int i = 0; i < maxShow; i++)
        {
            var itemData = DataLoader.Instance.GetItem(selectedCategory[i].Key);
            string itemName = itemData != null ? itemData.name : $"Objet #{selectedCategory[i].Key}";
            choiceNames.Add($"{itemName} x{selectedCategory[i].Value}");
        }
        choiceNames.Add("RETOUR");

        int chosenIndex = -1;
        DialogueUI.Instance.ShowChoices("Quel objet utiliser?", choiceNames.ToArray(), (idx) => {
            chosenIndex = idx;
        });
        yield return new WaitUntil(() => chosenIndex >= 0);

        // RETOUR selected or invalid
        if (chosenIndex >= maxShow || chosenIndex < 0)
        {
            isAnimating = false;
            StartPlayerTurn();
            yield break;
        }

        var firstItem = selectedCategory[chosenIndex];
        var data = DataLoader.Instance.GetItem(firstItem.Key);

        if (data.effect == "capture")
        {
            // Block capture in trainer/gym battles before consuming the item
            if (!setupData.isWild)
            {
                bool msgDone2 = false;
                battleUI.ShowMessage("On ne peut pas capturer le dino d'un dresseur!", () => msgDone2 = true);
                yield return new WaitUntil(() => msgDone2);
                isAnimating = false;
                StartPlayerTurn();
                yield break;
            }

            // Capture attempt (wild battle only)
            battleUI.HideActionMenu();
            var captureResult = battleSystem.AttemptCapture(firstItem.Key);
            inventory.RemoveItem(firstItem.Key);

            // Ball shake animation
            yield return StartCoroutine(AnimateBallShake(captureResult.shakes));

            bool msgDone = false;
            battleUI.ShowMessage(captureResult.message, () => msgDone = true);
            yield return new WaitUntil(() => msgDone);

            if (captureResult.success)
            {
                yield return StartCoroutine(HandleCapture());
            }
            else
            {
                // Enemy gets a turn after failed capture
                var enemyResult = battleSystem.ExecuteEnemyMove();
                yield return StartCoroutine(PlayResultCoroutine(false, enemyResult));

                battleUI.UpdateHP(true, playerDino.currentHp, playerDino.maxHp, true);
                battleUI.SetStatus(true, playerDino.status);
                battleUI.SetStatus(false, enemyDino.status);

                if (playerDino.IsFainted())
                {
                    yield return StartCoroutine(HandleDefeat());
                }
                else
                {
                    isAnimating = false;
                    StartPlayerTurn();
                }
            }
        }
        else
        {
            // Use item on player dino (healing, status cure, PP restore, etc.)
            var useResult = inventory.UseItem(firstItem.Key, playerDino);

            bool msgDone = false;
            battleUI.ShowMessage(useResult.message, () => msgDone = true);
            yield return new WaitUntil(() => msgDone);

            if (useResult.success)
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayHeal();

                battleUI.UpdateHP(true, playerDino.currentHp, playerDino.maxHp, true);
                battleUI.SetStatus(true, playerDino.status);

                yield return new WaitForSeconds(0.5f);

                // Enemy turn after item use
                var enemyResult = battleSystem.ExecuteEnemyMove();
                yield return StartCoroutine(PlayResultCoroutine(false, enemyResult));

                battleUI.UpdateHP(true, playerDino.currentHp, playerDino.maxHp, true);

                if (playerDino.IsFainted())
                {
                    yield return StartCoroutine(HandleDefeat());
                }
                else
                {
                    isAnimating = false;
                    StartPlayerTurn();
                }
            }
            else
            {
                isAnimating = false;
                StartPlayerTurn();
            }
        }
    }

    // ===============================================================
    // Dino Switch
    // ===============================================================

    private IEnumerator ShowDinoSwitchMenu()
    {
        isAnimating = true;
        battleUI.HideActionMenu();

        var party = GameState.Instance.Party;

        // Build choice list: party dinos + RETOUR
        var choiceNames = new List<string>();
        var validIndices = new List<int>();

        for (int i = 0; i < party.Count; i++)
        {
            var dino = party[i];
            string status = dino.IsFainted() ? " [K.O.]" : "";
            string current = (dino == playerDino) ? " [Actif]" : "";
            choiceNames.Add($"{dino.nickname} Nv.{dino.level} PV:{dino.currentHp}/{dino.maxHp}{status}{current}");
            validIndices.Add(i);
        }
        choiceNames.Add("RETOUR");

        // Show choice menu via DialogueUI
        if (DialogueUI.Instance == null)
        {
            var dgo = new GameObject("DialogueUI");
            dgo.AddComponent<DialogueUI>();
        }

        int chosenIndex = -1;
        DialogueUI.Instance.ShowChoices("Quel dino envoyer?", choiceNames.ToArray(), (idx) => {
            chosenIndex = idx;
        });
        yield return new WaitUntil(() => chosenIndex >= 0);

        // RETOUR selected
        if (chosenIndex >= party.Count)
        {
            isAnimating = false;
            StartPlayerTurn();
            yield break;
        }

        var chosenDino = party[chosenIndex];

        // Can't pick fainted dino
        if (chosenDino.IsFainted())
        {
            bool msgDone = false;
            battleUI.ShowMessage($"{chosenDino.nickname} est K.O. et ne peut pas combattre!", () => msgDone = true);
            yield return new WaitUntil(() => msgDone);
            isAnimating = false;
            StartPlayerTurn();
            yield break;
        }

        // Can't pick current dino
        if (chosenDino == playerDino)
        {
            bool msgDone = false;
            battleUI.ShowMessage($"{chosenDino.nickname} est deja au combat!", () => msgDone = true);
            yield return new WaitUntil(() => msgDone);
            isAnimating = false;
            StartPlayerTurn();
            yield break;
        }

        // --- Perform the switch ---
        yield return StartCoroutine(PerformDinoSwitch(chosenDino));
    }

    private IEnumerator PerformDinoSwitch(Dino newDino)
    {
        // "Reviens [current]!"
        bool msgDone = false;
        battleUI.ShowMessage($"Reviens {playerDino.nickname}!", () => msgDone = true);
        yield return new WaitUntil(() => msgDone);

        // Slide out current dino
        if (playerDinoModel != null)
        {
            Vector3 startPos = playerDinoModel.transform.position;
            Vector3 endPos = PLAYER_DINO_POS + Vector3.left * 8f;
            float duration = 0.4f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                playerDinoModel.transform.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }
            Destroy(playerDinoModel);
        }

        // Swap active dino reference
        playerDino = newDino;

        // Restart battle system with new player dino
        battleSystem.StartBattle(playerDino, enemyDino, setupData.isWild, setupData.trainerName);

        // "A toi [new]!"
        msgDone = false;
        battleUI.ShowMessage($"A toi {playerDino.nickname}!", () => msgDone = true);
        yield return new WaitUntil(() => msgDone);

        // Spawn new player dino model with slide-in animation
        playerDinoModel = DinoModelGenerator.CreateDinoModel(playerDino.speciesId, false);
        Vector3 offscreenPos = PLAYER_DINO_POS + Vector3.left * 8f;
        playerDinoModel.transform.position = offscreenPos;
        playerDinoModel.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        playerDinoModel.AddComponent<DinoAnimator>().PlayIdle();

        {
            float duration = 0.5f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                playerDinoModel.transform.position = Vector3.Lerp(offscreenPos, PLAYER_DINO_POS, t);
                yield return null;
            }
            playerDinoModel.transform.position = PLAYER_DINO_POS;
        }

        // Update HUD
        battleUI.SetPlayerInfo(playerDino.nickname, playerDino.level, playerDino.currentHp, playerDino.maxHp);
        battleUI.SetStatus(true, playerDino.status);
        battleUI.UpdateXP(playerDino.GetXpPercent());

        yield return new WaitForSeconds(0.3f);

        // Enemy gets a free turn after switching
        var enemyResult = battleSystem.ExecuteEnemyMove();
        yield return StartCoroutine(PlayResultCoroutine(false, enemyResult));

        battleUI.UpdateHP(true, playerDino.currentHp, playerDino.maxHp, true);
        battleUI.SetStatus(true, playerDino.status);
        battleUI.SetStatus(false, enemyDino.status);

        if (playerDino.IsFainted())
        {
            yield return StartCoroutine(HandleDefeat());
        }
        else
        {
            isAnimating = false;
            StartPlayerTurn();
        }
    }

    // ===============================================================
    // Flee Attempt
    // ===============================================================

    private IEnumerator AttemptFleeCoroutine()
    {
        isAnimating = true;

        if (!setupData.isWild)
        {
            bool msgDone = false;
            battleUI.ShowMessage("Impossible de fuir un combat de dresseur !", () => msgDone = true);
            yield return new WaitUntil(() => msgDone);
            isAnimating = false;
            StartPlayerTurn();
            yield break;
        }

        bool fled = battleSystem.AttemptFlee();

        if (fled)
        {
            bool msgDone = false;
            battleUI.ShowMessage("Vous prenez la fuite !", () => msgDone = true);
            yield return new WaitUntil(() => msgDone);

            // Fade out effect: slide player dino off-screen
            if (playerDinoModel != null)
            {
                Vector3 startPos = playerDinoModel.transform.position;
                Vector3 endPos = PLAYER_DINO_POS + Vector3.left * 8f;
                float duration = 0.4f;
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                    playerDinoModel.transform.position = Vector3.Lerp(startPos, endPos, t);
                    yield return null;
                }
            }

            yield return new WaitForSeconds(0.3f);
            EndBattle(false);
        }
        else
        {
            bool msgDone = false;
            battleUI.ShowMessage("La fuite a echoue !", () => msgDone = true);
            yield return new WaitUntil(() => msgDone);

            // Enemy gets a turn
            var enemyResult = battleSystem.ExecuteEnemyMove();
            yield return StartCoroutine(PlayResultCoroutine(false, enemyResult));

            battleUI.UpdateHP(true, playerDino.currentHp, playerDino.maxHp, true);
            battleUI.SetStatus(true, playerDino.status);
            battleUI.SetStatus(false, enemyDino.status);

            if (playerDino.IsFainted())
            {
                yield return StartCoroutine(HandleDefeat());
            }
            else
            {
                isAnimating = false;
                StartPlayerTurn();
            }
        }
    }

    // ===============================================================
    // Victory / Defeat / Capture
    // ===============================================================

    private IEnumerator HandleVictory()
    {
        bool msgDone = false;
        battleUI.ShowMessage($"{enemyDino.nickname} est K.O.!", () => msgDone = true);
        yield return new WaitUntil(() => msgDone);

        // Calculate XP: enemyLevel * 8 + 20
        int xpGained = enemyDino.level * 8 + 20;

        msgDone = false;
        battleUI.ShowMessage($"{playerDino.nickname} gagne {xpGained} points d'experience!", () => msgDone = true);
        yield return new WaitUntil(() => msgDone);

        // --- Animated XP gain with per-level handling ---
        yield return StartCoroutine(ApplyXpWithAnimation(xpGained));

        // Check if trainer has more dinos to send out
        if (trainerPartyDinos != null && trainerPartyIndex + 1 < trainerPartyDinos.Count)
        {
            trainerPartyIndex++;
            yield return StartCoroutine(SendNextTrainerDino());
            yield break; // Continue battle, don't end
        }

        // --- Final victory: all trainer dinos defeated ---
        battleEnded = true;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayVictoryJingle();

        // Money from trainer battles
        if (!setupData.isWild && !string.IsNullOrEmpty(setupData.trainerId))
        {
            GameState.Instance.DefeatTrainer(setupData.trainerId);

            if (setupData.isGymLeader && setupData.gymId >= 0)
            {
                // Gym leader victory: award badge + prize money
                int prize = 1500 + setupData.gymId * 500;
                GameState.Instance.AddMoney(prize);
                GameState.Instance.AddBadge(setupData.gymId);

                string badgeLabel = !string.IsNullOrEmpty(setupData.badgeName)
                    ? setupData.badgeName
                    : $"Badge #{setupData.gymId + 1}";

                msgDone = false;
                battleUI.ShowMessage($"Vous avez obtenu le {badgeLabel} !", () => msgDone = true);
                yield return new WaitUntil(() => msgDone);

                msgDone = false;
                battleUI.ShowMessage($"Vous gagnez {prize} $ !", () => msgDone = true);
                yield return new WaitUntil(() => msgDone);
            }
            else
            {
                // Regular trainer victory
                int prize = enemyDino.level * 50;
                GameState.Instance.AddMoney(prize);

                msgDone = false;
                battleUI.ShowMessage($"Vous gagnez {prize} $!", () => msgDone = true);
                yield return new WaitUntil(() => msgDone);
            }
        }

        yield return new WaitForSeconds(1f);
        EndBattle(true);
    }

    /// <summary>
    /// Send the next trainer dino after the current one fainted.
    /// </summary>
    private IEnumerator SendNextTrainerDino()
    {
        enemyDino = trainerPartyDinos[trainerPartyIndex];

        // Remove old enemy model
        if (enemyDinoModel != null) Destroy(enemyDinoModel);

        bool msgDone = false;
        battleUI.ShowMessage($"{setupData.trainerName} envoie {enemyDino.nickname}!", () => msgDone = true);
        yield return new WaitUntil(() => msgDone);

        // Register in dinodex
        GameState.Instance.RegisterSeen(enemyDino.speciesId);

        // Spawn new enemy model with slide-in animation
        enemyDinoModel = DinoModelGenerator.CreateDinoModel(enemyDino.speciesId, true);
        Vector3 startPos = ENEMY_DINO_POS + Vector3.right * 8f;
        enemyDinoModel.transform.position = startPos;
        enemyDinoModel.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
        enemyDinoModel.AddComponent<DinoAnimator>().PlayIdle();

        float duration = 0.6f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            enemyDinoModel.transform.position = Vector3.Lerp(startPos, ENEMY_DINO_POS, t);
            yield return null;
        }
        enemyDinoModel.transform.position = ENEMY_DINO_POS;

        // Restart the battle system with the new enemy dino
        battleSystem.StartBattle(playerDino, enemyDino, setupData.isWild, setupData.trainerName);

        // Update UI
        battleUI.SetPlayerInfo(playerDino.nickname, playerDino.level, playerDino.currentHp, playerDino.maxHp);
        battleUI.SetEnemyInfo(enemyDino.nickname, enemyDino.level, enemyDino.currentHp, enemyDino.maxHp);
        battleUI.SetStatus(false, enemyDino.status);

        yield return new WaitForSeconds(0.3f);

        isAnimating = false;
        StartPlayerTurn();
    }

    // ===============================================================
    // XP Gain Animation (per-level processing)
    // ===============================================================

    private IEnumerator ApplyXpWithAnimation(int xpGained)
    {
        float xpPercentBefore = playerDino.GetXpPercent();
        playerDino.AddXpRaw(xpGained);

        // Process level-ups one at a time for animation
        bool keepLeveling = true;
        while (keepLeveling)
        {
            int needed = playerDino.GetXpForNextLevel();
            bool willLevelUp = needed > 0 && playerDino.xp >= needed;

            if (willLevelUp)
            {
                // Animate XP bar filling to 100%
                bool xpAnimDone = false;
                battleUI.AnimateXP(xpPercentBefore, 1f, false, () => xpAnimDone = true);
                yield return new WaitUntil(() => xpAnimDone);

                // Process the level-up
                var lvResult = playerDino.ProcessSingleLevelUp();
                if (lvResult == null) break;

                // Level-up fanfare
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayLevelUp();

                // Level-up visual effect
                if (effects != null && playerDinoModel != null)
                    effects.LevelUpEffect(playerDinoModel.transform);

                // Update HUD
                battleUI.UpdatePlayerLevel(lvResult.newLevel);
                battleUI.UpdateHP(true, playerDino.currentHp, playerDino.maxHp, false);

                // Reset XP bar for new level
                float newXpPercent = playerDino.GetXpPercent();
                battleUI.UpdateXP(0f);
                yield return new WaitForSeconds(0.15f);

                // Animate from 0 to new percent
                xpAnimDone = false;
                battleUI.AnimateXP(0f, newXpPercent, false, () => xpAnimDone = true);
                yield return new WaitUntil(() => xpAnimDone);

                // Show level-up message
                bool msgDone = false;
                battleUI.ShowMessage($"{playerDino.nickname} monte au Nv. {lvResult.newLevel} !", () => msgDone = true);
                yield return new WaitUntil(() => msgDone);

                // --- Handle moves learned (auto-learned, had free slots) ---
                foreach (int moveId in lvResult.movesLearned)
                {
                    var moveData = DataLoader.Instance.GetMove(moveId);
                    if (moveData != null)
                    {
                        msgDone = false;
                        battleUI.ShowMessage($"{playerDino.nickname} apprend {moveData.name} !", () => msgDone = true);
                        yield return new WaitUntil(() => msgDone);
                    }
                }

                // --- Handle pending moves (slots full — player must choose) ---
                foreach (int moveId in lvResult.movesPending)
                {
                    yield return StartCoroutine(HandleMoveLearnChoice(moveId));
                }

                // --- Handle evolution ---
                if (lvResult.canEvolve)
                {
                    yield return StartCoroutine(HandleEvolution(playerDino));
                }

                // Prepare for potential next level-up
                xpPercentBefore = playerDino.GetXpPercent();
            }
            else
            {
                // No more level-ups — just animate XP bar to final percent
                float finalPercent = playerDino.GetXpPercent();
                bool xpAnimDone = false;
                battleUI.AnimateXP(xpPercentBefore, finalPercent, false, () => xpAnimDone = true);
                yield return new WaitUntil(() => xpAnimDone);
                keepLeveling = false;
            }
        }
    }

    // ===============================================================
    // Move Learning Choice (slots full)
    // ===============================================================

    private IEnumerator HandleMoveLearnChoice(int newMoveId)
    {
        var newMoveData = DataLoader.Instance.GetMove(newMoveId);
        if (newMoveData == null) yield break;

        // "PYREX veut apprendre [Move]."
        bool msgDone = false;
        battleUI.ShowMessage($"{playerDino.nickname} veut apprendre {newMoveData.name}.", () => msgDone = true);
        yield return new WaitUntil(() => msgDone);

        msgDone = false;
        battleUI.ShowMessage($"Mais {playerDino.nickname} connait deja 4 attaques.", () => msgDone = true);
        yield return new WaitUntil(() => msgDone);

        msgDone = false;
        battleUI.ShowMessage($"Oublier une attaque pour apprendre {newMoveData.name} ?", () => msgDone = true);
        yield return new WaitUntil(() => msgDone);

        // Build choice list: 4 current moves + "Ne pas apprendre"
        var choiceList = new List<string>();
        for (int i = 0; i < playerDino.moves.Count && i < 4; i++)
        {
            var md = DataLoader.Instance.GetMove(playerDino.moves[i].moveId);
            choiceList.Add(md != null ? md.name : "???");
        }
        choiceList.Add("Ne pas apprendre");
        string[] choices = choiceList.ToArray();

        // Show choices via DialogueUI
        int selectedIndex = -1;
        var dialogueUI = DialogueUI.Instance;
        if (dialogueUI != null)
        {
            dialogueUI.ShowChoices(
                $"Quelle attaque oublier ?",
                choices,
                (idx) => selectedIndex = idx
            );

            yield return new WaitUntil(() => selectedIndex >= 0);
        }
        else
        {
            // Fallback: auto-decline if no DialogueUI available
            selectedIndex = choices.Length - 1;
        }

        if (selectedIndex < playerDino.moves.Count)
        {
            // Replace the chosen move
            var forgottenData = DataLoader.Instance.GetMove(playerDino.moves[selectedIndex].moveId);
            string forgottenName = forgottenData != null ? forgottenData.name : "???";

            playerDino.ReplaceMove(selectedIndex, newMoveId);

            msgDone = false;
            battleUI.ShowMessage($"1, 2 et... Poof!", () => msgDone = true);
            yield return new WaitUntil(() => msgDone);

            msgDone = false;
            battleUI.ShowMessage($"{playerDino.nickname} oublie {forgottenName}...", () => msgDone = true);
            yield return new WaitUntil(() => msgDone);

            msgDone = false;
            battleUI.ShowMessage($"Et {playerDino.nickname} apprend {newMoveData.name} !", () => msgDone = true);
            yield return new WaitUntil(() => msgDone);
        }
        else
        {
            // Player chose not to learn
            msgDone = false;
            battleUI.ShowMessage($"{playerDino.nickname} n'apprend pas {newMoveData.name}.", () => msgDone = true);
            yield return new WaitUntil(() => msgDone);
        }
    }

    // ===============================================================
    // Evolution
    // ===============================================================

    private IEnumerator HandleEvolution(Dino dino)
    {
        var oldSpecies = dino.SpeciesData;
        var evoData = oldSpecies?.evolution;
        if (evoData == null) yield break;

        var newSpeciesData = DataLoader.Instance.GetSpecies(evoData.to);
        string oldName = dino.nickname;
        string newName = newSpeciesData != null ? newSpeciesData.name : "???";

        // "Quoi ? PYREX evolue !"
        bool msgDone = false;
        battleUI.ShowMessage($"Quoi ? {oldName} evolue !", () => msgDone = true);
        yield return new WaitUntil(() => msgDone);

        // --- White flash + scale pulse animation ---
        if (playerDinoModel != null)
        {
            // White flash on current model
            if (effects != null)
                effects.HitFlash(playerDinoModel);

            Vector3 originalScale = playerDinoModel.transform.localScale;
            float elapsed = 0f;
            float duration = 1.2f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                // Pulse: grow-shrink-grow pattern with increasing intensity
                float pulse = 1f + Mathf.Sin(t * Mathf.PI * 4f) * 0.3f * (1f - t * 0.5f) + t * 0.15f;
                playerDinoModel.transform.localScale = originalScale * pulse;
                yield return null;
            }

            playerDinoModel.transform.localScale = originalScale;
        }

        // Actually evolve the dino
        bool evolved = dino.Evolve();
        if (!evolved) yield break;

        // Update nickname if it still matches old species name
        if (oldName == (oldSpecies?.name ?? ""))
        {
            dino.nickname = newName;
        }

        // Regenerate the 3D model
        if (playerDinoModel != null)
        {
            Vector3 pos = playerDinoModel.transform.position;
            Quaternion rot = playerDinoModel.transform.rotation;
            Destroy(playerDinoModel);

            playerDinoModel = DinoModelGenerator.CreateDinoModel(dino.speciesId, false);
            playerDinoModel.transform.position = pos;
            playerDinoModel.transform.rotation = rot;
            playerDinoModel.AddComponent<DinoAnimator>().PlayIdle();
        }

        // Flash effect on new model
        if (effects != null && playerDinoModel != null)
            effects.HitFlash(playerDinoModel);

        // Update HUD
        battleUI.UpdatePlayerName(dino.nickname);
        battleUI.UpdatePlayerLevel(dino.level);
        battleUI.UpdateHP(true, dino.currentHp, dino.maxHp, false);
        battleUI.UpdateXP(dino.GetXpPercent());

        // Play fanfare
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayLevelUp();

        yield return new WaitForSeconds(0.5f);

        // "Felicitations ! PYREX a evolue en PYROVORE !"
        msgDone = false;
        battleUI.ShowMessage($"Felicitations ! {oldName} a evolue en {dino.nickname} !", () => msgDone = true);
        yield return new WaitUntil(() => msgDone);
    }

    // ===============================================================
    // Defeat
    // ===============================================================

    private IEnumerator HandleDefeat()
    {
        bool msgDone = false;
        battleUI.ShowMessage($"{playerDino.nickname} est K.O.!", () => msgDone = true);
        yield return new WaitUntil(() => msgDone);

        // Faint animation for player dino
        yield return StartCoroutine(AnimateFaint(true, null));

        // Check if player has other alive dinos
        if (GameState.Instance.HasAliveDino())
        {
            // Force switch to another dino
            yield return StartCoroutine(ForcedDinoSwitch());
            yield break;
        }

        // All dinos fainted — true defeat
        battleEnded = true;

        // Deduct half money
        int moneyLost = GameState.Instance.Money / 2;
        GameState.Instance.RemoveMoney(moneyLost);

        msgDone = false;
        battleUI.ShowMessage("Tous vos dinos sont K.O.! Vous etes ramene au centre de soin...", () => msgDone = true);
        yield return new WaitUntil(() => msgDone);

        if (moneyLost > 0)
        {
            msgDone = false;
            battleUI.ShowMessage($"Vous perdez {moneyLost} $ dans la panique...", () => msgDone = true);
            yield return new WaitUntil(() => msgDone);
        }

        // Heal all party dinos
        foreach (var dino in GameState.Instance.Party)
            dino.FullHeal();

        // Override return data to go to last heal point instead of battle location
        var healPoint = GameState.Instance.LastHealPoint;
        setupData.returnScene = healPoint.mapId;
        setupData.returnPosition = new Vector2(healPoint.x, healPoint.y);

        GameState.Instance.CurrentMapId = healPoint.mapId;
        GameState.Instance.PlayerX = healPoint.x;
        GameState.Instance.PlayerY = healPoint.y;

        yield return new WaitForSeconds(1f);
        EndBattle(false);
    }

    /// <summary>
    /// Force the player to switch to another alive dino after their active dino fainted.
    /// Unlike voluntary switch, enemy does NOT get a free turn.
    /// </summary>
    private IEnumerator ForcedDinoSwitch()
    {
        var party = GameState.Instance.Party;

        // Build choice list: only alive, non-current dinos
        var choiceNames = new List<string>();
        var validIndices = new List<int>();

        for (int i = 0; i < party.Count; i++)
        {
            var dino = party[i];
            if (dino.IsFainted() || dino == playerDino) continue;
            choiceNames.Add($"{dino.nickname} Nv.{dino.level} PV:{dino.currentHp}/{dino.maxHp}");
            validIndices.Add(i);
        }

        if (choiceNames.Count == 0)
        {
            // Should not happen since HasAliveDino was true, but safety fallback
            battleEnded = true;
            yield return new WaitForSeconds(1f);
            EndBattle(false);
            yield break;
        }

        if (DialogueUI.Instance == null)
        {
            var dgo = new GameObject("DialogueUI");
            dgo.AddComponent<DialogueUI>();
        }

        int chosenIndex = -1;
        DialogueUI.Instance.ShowChoices("Quel dino envoyer?", choiceNames.ToArray(), (idx) => {
            chosenIndex = idx;
        });
        yield return new WaitUntil(() => chosenIndex >= 0);

        // Map choice back to party index
        int partyIndex = validIndices[chosenIndex];
        var newDino = party[partyIndex];

        // Swap active dino reference
        playerDino = newDino;

        // Restart battle system with new player dino
        battleSystem.StartBattle(playerDino, enemyDino, setupData.isWild, setupData.trainerName);

        // "A toi [new]!"
        bool msgDone = false;
        battleUI.ShowMessage($"A toi {playerDino.nickname}!", () => msgDone = true);
        yield return new WaitUntil(() => msgDone);

        // Spawn new player dino model with slide-in animation
        if (playerDinoModel != null) Destroy(playerDinoModel);

        playerDinoModel = DinoModelGenerator.CreateDinoModel(playerDino.speciesId, false);
        Vector3 offscreenPos2 = PLAYER_DINO_POS + Vector3.left * 8f;
        playerDinoModel.transform.position = offscreenPos2;
        playerDinoModel.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        playerDinoModel.AddComponent<DinoAnimator>().PlayIdle();

        {
            float duration = 0.5f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                playerDinoModel.transform.position = Vector3.Lerp(offscreenPos2, PLAYER_DINO_POS, t);
                yield return null;
            }
            playerDinoModel.transform.position = PLAYER_DINO_POS;
        }

        // Update HUD
        battleUI.SetPlayerInfo(playerDino.nickname, playerDino.level, playerDino.currentHp, playerDino.maxHp);
        battleUI.SetStatus(true, playerDino.status);
        battleUI.UpdateXP(playerDino.GetXpPercent());

        yield return new WaitForSeconds(0.3f);

        // No enemy free turn on forced switch (dino fainted)
        isAnimating = false;
        StartPlayerTurn();
    }

    private IEnumerator HandleCapture()
    {
        battleEnded = true;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayCapture();

        yield return new WaitForSeconds(0.5f);

        // Register caught in dinodex
        GameState.Instance.RegisterCaught(enemyDino.speciesId);

        // ---- XP reward for capture ----
        int xpGained = enemyDino.level * 6 + 15; // slightly less than KO but still meaningful
        bool xpMsgDone = false;
        battleUI.ShowMessage($"{playerDino.nickname} gagne {xpGained} points d'experience!", () => xpMsgDone = true);
        yield return new WaitUntil(() => xpMsgDone);

        yield return StartCoroutine(ApplyXpWithAnimation(xpGained));

        // ---- Add captured dino to party or PC ----
        if (GameState.Instance.Party.Count < GameState.MAX_PARTY_SIZE)
        {
            GameState.Instance.AddToParty(enemyDino);
            bool msgDone = false;
            battleUI.ShowMessage($"{enemyDino.nickname} rejoint votre equipe!", () => msgDone = true);
            yield return new WaitUntil(() => msgDone);
        }
        else
        {
            GameState.Instance.AddToPC(enemyDino);
            bool msgDone = false;
            battleUI.ShowMessage($"{enemyDino.nickname} a ete transfere au PC!", () => msgDone = true);
            yield return new WaitUntil(() => msgDone);
        }

        yield return new WaitForSeconds(0.5f);
        EndBattle(true);
    }

    // ===============================================================
    // End Battle — Cleanup and return to overworld
    // ===============================================================

    public void EndBattle(bool victory)
    {
        StartCoroutine(EndBattleWithFade(victory));
    }

    private IEnumerator EndBattleWithFade(bool victory)
    {
        battleEnded = true;

        // Stop battle music
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopMusic();

        // Fade to black before leaving
        if (ScreenFade.Instance != null)
            yield return ScreenFade.Instance.FadeOutCoroutine(0.5f);

        // Cleanup
        if (arenaRoot != null) Destroy(arenaRoot);
        if (playerDinoModel != null) Destroy(playerDinoModel);
        if (enemyDinoModel != null) Destroy(enemyDinoModel);

        // Return to overworld — pass setupData back so OverworldManager can restore player position
        string returnScene = !string.IsNullOrEmpty(setupData.returnScene) ? setupData.returnScene : "Overworld";

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadScene(GameManager.GameScene.Overworld, setupData);
        }
        else
        {
            SceneTransitionData.Instance = setupData;
            SceneManager.LoadScene(returnScene);
        }
    }
}
