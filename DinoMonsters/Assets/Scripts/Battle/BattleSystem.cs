// ============================================================
// Dino Monsters -- Battle System (Pure Logic, NOT MonoBehaviour)
// ============================================================
//
// Uses existing DataLoader types (MoveData with int type, string category,
// string effect, int effectChance).
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;

// --------------- Result Structs ---------------

[System.Serializable]
public struct BattleResult
{
    public int damage;
    public float effectiveness;
    public bool isCritical;
    public bool targetFainted;
    public string message;
    public int moveType;  // DinoType int for attack animation
    public int movePower; // Move power for animation scaling
}

[System.Serializable]
public struct CaptureResult
{
    public bool success;
    public int shakes; // 0-4
    public string message;
}

public enum BattleState
{
    NotStarted,
    Active,
    Won,
    Lost,
    Fled,
    Captured
}

// --------------- Battle System ---------------

public class BattleSystem
{
    // --- Battle participants ---
    public Dino PlayerDino { get; private set; }
    public Dino EnemyDino { get; private set; }
    public bool IsWild { get; private set; }
    public string TrainerName { get; private set; }
    public BattleState State { get; private set; }

    // --- Status counters ---
    private int playerSleepCounter;
    private int enemySleepCounter;

    // --- Turn tracking ---
    public int TurnCount { get; private set; }

    // --------------- Start Battle ---------------

    public void StartBattle(Dino playerDino, Dino enemyDino, bool isWild, string trainerName = null)
    {
        PlayerDino = playerDino;
        EnemyDino = enemyDino;
        IsWild = isWild;
        TrainerName = trainerName;
        State = BattleState.Active;
        TurnCount = 0;
        playerSleepCounter = 0;
        enemySleepCounter = 0;
        fleeAttempts = 0;
    }

    // --------------- Execute Full Turn ---------------

    /// <summary>
    /// Execute a move chosen by the player. Resolves full turn (player + enemy).
    /// Returns results for both actions in order of speed.
    /// </summary>
    public List<BattleResult> ExecuteTurn(int playerMoveIndex)
    {
        if (State != BattleState.Active)
            return new List<BattleResult>();

        TurnCount++;
        var results = new List<BattleResult>();

        // Determine turn order by speed (paralysis halves speed)
        int playerSpeed = PlayerDino.stats.speed;
        int enemySpeed = EnemyDino.stats.speed;
        if (PlayerDino.status == StatusEffect.Paralysis) playerSpeed /= 2;
        if (EnemyDino.status == StatusEffect.Paralysis) enemySpeed /= 2;

        bool playerFirst = playerSpeed >= enemySpeed;

        if (playerFirst)
        {
            var playerResult = ExecuteMove(PlayerDino, EnemyDino, playerMoveIndex, ref playerSleepCounter);
            results.Add(playerResult);

            if (!EnemyDino.IsFainted())
            {
                var enemyResult = ExecuteEnemyMove();
                results.Add(enemyResult);
            }
        }
        else
        {
            var enemyResult = ExecuteEnemyMove();
            results.Add(enemyResult);

            if (!PlayerDino.IsFainted())
            {
                var playerResult = ExecuteMove(PlayerDino, EnemyDino, playerMoveIndex, ref playerSleepCounter);
                results.Add(playerResult);
            }
        }

        // Process end-of-turn status effects
        results.AddRange(ProcessEndOfTurnStatus(PlayerDino));
        results.AddRange(ProcessEndOfTurnStatus(EnemyDino));

        // Check battle end conditions
        CheckBattleEnd();

        return results;
    }

    /// <summary>
    /// Execute only the player's move (for single-action flow).
    /// </summary>
    public BattleResult ExecutePlayerMove(int moveIndex)
    {
        if (State != BattleState.Active) return default;
        return ExecuteMove(PlayerDino, EnemyDino, moveIndex, ref playerSleepCounter);
    }

    /// <summary>
    /// Execute the enemy's move (for single-action flow).
    /// </summary>
    public BattleResult ExecuteEnemyMove()
    {
        // Simple AI: pick a random move with PP remaining
        var availableMoves = new List<int>();
        for (int i = 0; i < EnemyDino.moves.Count; i++)
        {
            if (EnemyDino.moves[i].currentPP > 0)
                availableMoves.Add(i);
        }

        if (availableMoves.Count == 0)
        {
            // Struggle equivalent — no moves left
            int struggleDmg = Mathf.Max(1, EnemyDino.maxHp / 4);
            PlayerDino.TakeDamage(struggleDmg);
            return new BattleResult
            {
                damage = struggleDmg,
                message = $"{EnemyDino.nickname} se debat!",
                targetFainted = PlayerDino.IsFainted()
            };
        }

        int moveIndex = availableMoves[UnityEngine.Random.Range(0, availableMoves.Count)];
        return ExecuteMove(EnemyDino, PlayerDino, moveIndex, ref enemySleepCounter);
    }

    // --------------- Execute a Single Move ---------------

    private BattleResult ExecuteMove(Dino attacker, Dino defender, int moveIndex, ref int sleepCounter)
    {
        var result = new BattleResult();

        // Check if attacker can act (status effects)
        if (!CanAct(attacker, ref sleepCounter, out string statusMsg))
        {
            result.message = statusMsg;
            return result;
        }

        // Validate move index
        if (moveIndex < 0 || moveIndex >= attacker.moves.Count)
        {
            result.message = "Attaque invalide!";
            return result;
        }

        var moveSlot = attacker.moves[moveIndex];

        // Check PP
        if (moveSlot.currentPP <= 0)
        {
            result.message = "Plus de PP pour cette attaque!";
            return result;
        }

        // Consume PP
        moveSlot.currentPP--;

        // Get move data from DataLoader
        var moveData = DataLoader.Instance.GetMove(moveSlot.moveId);
        if (moveData == null)
        {
            result.message = "Attaque inconnue!";
            return result;
        }

        // Accuracy check
        if (moveData.accuracy > 0 && moveData.accuracy < 100)
        {
            int roll = UnityEngine.Random.Range(1, 101);
            if (roll > moveData.accuracy)
            {
                result.message = $"{attacker.nickname} rate son attaque!";
                return result;
            }
        }

        // Damage moves (power > 0)
        if (moveData.power > 0)
        {
            // Determine physical vs special from DataLoader's category string
            bool isSpecial = moveData.category == "special";
            int atkStat = isSpecial ? attacker.stats.spAttack : attacker.stats.attack;
            int defStat = isSpecial ? defender.stats.spDefense : defender.stats.defense;

            // Burn halves physical attack
            if (attacker.status == StatusEffect.Burn && !isSpecial)
                atkStat /= 2;

            // Type effectiveness
            DinoType moveType = (DinoType)moveData.type;
            float effectiveness = DamageCalculator.GetEffectiveness(moveType, defender.Type1, defender.Type2);
            bool stab = DamageCalculator.IsSTAB(moveType, attacker.Type1, attacker.Type2);
            bool critical = DamageCalculator.RollCritical(0);

            int damage = DamageCalculator.CalculateDamage(
                attacker.level, moveData.power, atkStat, defStat,
                stab, effectiveness, critical
            );

            defender.TakeDamage(damage);

            result.damage = damage;
            result.effectiveness = effectiveness;
            result.isCritical = critical;
            result.targetFainted = defender.IsFainted();
            result.moveType = moveData.type;
            result.movePower = moveData.power;

            string msg = $"{attacker.nickname} utilise {moveData.name}!";
            string effLabel = DamageCalculator.GetEffectivenessLabel(effectiveness);
            if (!string.IsNullOrEmpty(effLabel)) msg += $" {effLabel}";
            if (critical) msg += " Coup critique!";
            if (defender.IsFainted()) msg += $" {defender.nickname} est K.O.!";
            result.message = msg;
        }
        else
        {
            result.message = $"{attacker.nickname} utilise {moveData.name}!";
            result.moveType = moveData.type;
            result.movePower = moveData.power;
        }

        // Status effect application (from move's effect string + effectChance)
        if (!defender.IsFainted() && moveData.effectChance > 0)
        {
            StatusEffect statusToApply = ParseStatusEffect(moveData.effect);
            if (statusToApply != StatusEffect.None)
            {
                int statusRoll = UnityEngine.Random.Range(1, 101);
                if (statusRoll <= moveData.effectChance)
                {
                    if (defender.SetStatus(statusToApply))
                    {
                        result.message += $" {defender.nickname} est {GetStatusName(statusToApply)}!";
                    }
                }
            }
        }

        return result;
    }

    // --------------- Status Effect Processing ---------------

    private bool CanAct(Dino dino, ref int sleepCounter, out string message)
    {
        message = "";

        switch (dino.status)
        {
            case StatusEffect.Paralysis:
                if (UnityEngine.Random.value < 0.25f)
                {
                    message = $"{dino.nickname} est paralyse! Il ne peut pas attaquer!";
                    return false;
                }
                break;

            case StatusEffect.Sleep:
                sleepCounter++;
                if (sleepCounter >= UnityEngine.Random.Range(1, 4))
                {
                    dino.CureStatus();
                    sleepCounter = 0;
                    message = $"{dino.nickname} se reveille!";
                }
                else
                {
                    message = $"{dino.nickname} dort profondement...";
                    return false;
                }
                break;

            case StatusEffect.Freeze:
                if (UnityEngine.Random.value < 0.20f)
                {
                    dino.CureStatus();
                    message = $"{dino.nickname} degele!";
                }
                else
                {
                    message = $"{dino.nickname} est gele!";
                    return false;
                }
                break;
        }

        return true;
    }

    private List<BattleResult> ProcessEndOfTurnStatus(Dino dino)
    {
        var results = new List<BattleResult>();
        if (dino.IsFainted()) return results;

        switch (dino.status)
        {
            case StatusEffect.Poison:
            {
                int dmg = Mathf.Max(1, dino.maxHp / 8);
                dino.TakeDamage(dmg);
                results.Add(new BattleResult
                {
                    damage = dmg,
                    message = $"{dino.nickname} souffre du poison! (-{dmg} PV)",
                    targetFainted = dino.IsFainted()
                });
                break;
            }

            case StatusEffect.Burn:
            {
                int dmg = Mathf.Max(1, dino.maxHp / 16);
                dino.TakeDamage(dmg);
                results.Add(new BattleResult
                {
                    damage = dmg,
                    message = $"{dino.nickname} souffre de sa brulure! (-{dmg} PV)",
                    targetFainted = dino.IsFainted()
                });
                break;
            }
        }

        return results;
    }

    // --------------- Capture ---------------

    /// <summary>
    /// Attempt to capture the wild dino with a ball item.
    /// </summary>
    public CaptureResult AttemptCapture(int ballItemId)
    {
        if (!IsWild)
        {
            return new CaptureResult
            {
                success = false,
                shakes = 0,
                message = "On ne peut pas capturer le dino d'un dresseur!"
            };
        }

        // Get ball modifier from item data value
        var itemData = DataLoader.Instance.GetItem(ballItemId);
        float ballMod = itemData != null ? itemData.value : 1f;

        // Status modifier
        float statusMod = GetStatusCaptureMod(EnemyDino.status);

        // Get species capture rate
        var species = EnemyDino.SpeciesData;
        int baseCaptureRate = species != null ? species.captureRate : 45;

        // Calculate capture rate
        int captureRate = DamageCalculator.CalculateCaptureRate(
            EnemyDino.maxHp, EnemyDino.currentHp,
            baseCaptureRate, ballMod, statusMod
        );

        // Shake check (4 checks, need all 4 to succeed)
        int shakes = 0;
        float shakeThreshold = Mathf.Pow(captureRate / 255f, 0.25f);

        for (int i = 0; i < 4; i++)
        {
            if (UnityEngine.Random.value < shakeThreshold)
                shakes++;
            else
                break;
        }

        bool success = shakes >= 4;

        if (success)
        {
            State = BattleState.Captured;
            return new CaptureResult
            {
                success = true,
                shakes = 4,
                message = $"{EnemyDino.nickname} est capture!"
            };
        }

        string shakeMsg;
        switch (shakes)
        {
            case 0: shakeMsg = "La balle n'a meme pas bouge..."; break;
            case 1: shakeMsg = "La balle a bouge une fois..."; break;
            case 2: shakeMsg = "La balle a bouge deux fois..."; break;
            case 3: shakeMsg = "Presque! La balle a bouge trois fois!"; break;
            default: shakeMsg = "Capture echouee!"; break;
        }

        return new CaptureResult
        {
            success = false,
            shakes = shakes,
            message = shakeMsg
        };
    }

    // --------------- Flee ---------------

    private int fleeAttempts = 0;

    /// <summary>
    /// Attempt to flee from a wild battle.
    /// Formula: (playerSpeed * 128 / enemySpeed + 30 * attempts) mod 256
    /// A random roll 0-255 must be less than the computed value to flee.
    /// </summary>
    public bool AttemptFlee()
    {
        if (!IsWild) return false;

        fleeAttempts++;

        int playerSpeed = PlayerDino.stats.speed;
        int enemySpeed = Mathf.Max(1, EnemyDino.stats.speed);

        int fleeValue = (playerSpeed * 128 / enemySpeed + 30 * fleeAttempts) % 256;

        // If player is faster, always flee
        if (playerSpeed >= enemySpeed || UnityEngine.Random.Range(0, 256) < fleeValue)
        {
            State = BattleState.Fled;
            return true;
        }

        return false;
    }

    // --------------- Battle End ---------------

    private void CheckBattleEnd()
    {
        if (State != BattleState.Active) return;

        if (EnemyDino.IsFainted())
            State = BattleState.Won;
        else if (PlayerDino.IsFainted())
            State = BattleState.Lost;
    }

    // --------------- Helpers ---------------

    private float GetStatusCaptureMod(StatusEffect status)
    {
        switch (status)
        {
            case StatusEffect.Sleep:     return 2.0f;
            case StatusEffect.Freeze:    return 2.0f;
            case StatusEffect.Paralysis: return 1.5f;
            case StatusEffect.Poison:    return 1.5f;
            case StatusEffect.Burn:      return 1.5f;
            default: return 1.0f;
        }
    }

    private string GetStatusName(StatusEffect status)
    {
        switch (status)
        {
            case StatusEffect.Poison:    return "empoisonne";
            case StatusEffect.Burn:      return "brule";
            case StatusEffect.Paralysis: return "paralyse";
            case StatusEffect.Sleep:     return "endormi";
            case StatusEffect.Freeze:    return "gele";
            default: return "";
        }
    }

    /// <summary>
    /// Parse a status effect from the move's effect string (e.g. "poison", "burn", "paralyze").
    /// </summary>
    private StatusEffect ParseStatusEffect(string effect)
    {
        if (string.IsNullOrEmpty(effect)) return StatusEffect.None;
        string lower = effect.ToLower();
        if (lower.Contains("poison")) return StatusEffect.Poison;
        if (lower.Contains("burn")) return StatusEffect.Burn;
        if (lower.Contains("paralyze") || lower.Contains("paralysis")) return StatusEffect.Paralysis;
        if (lower.Contains("sleep")) return StatusEffect.Sleep;
        if (lower.Contains("freeze")) return StatusEffect.Freeze;
        return StatusEffect.None;
    }
}
