// ============================================================
// Jurassic Trainers -- Battle System
// ============================================================

import Phaser from 'phaser';
import { Dino } from '../entities/Dino';
import { typeChart } from './TypeChart';
import {
  DinoType,
  StatusEffect,
  STAB_MULTIPLIER,
  CRITICAL_HIT_MULTIPLIER,
  CRITICAL_HIT_CHANCE,
  DAMAGE_RANDOM_MIN,
  DAMAGE_RANDOM_MAX,
  MAX_CAPTURE_RATE,
} from '../utils/constants';

// ===================== Interfaces =====================

export interface IMoveData {
  id: number;
  name: string;
  type: DinoType;
  category: MoveCategory;
  power: number;        // 0 for status moves
  accuracy: number;     // 0-100, 0 = always hits
  pp: number;
  priority: number;     // higher goes first (default 0)
  effect?: IMoveEffect;
  description: string;
}

export type MoveCategory = 'physical' | 'special' | 'status';

export interface IMoveEffect {
  type: MoveEffectType;
  chance: number;         // probability 0-100
  value?: number;         // magnitude (damage, stat stages, etc.)
  target?: 'self' | 'opponent';
  stat?: string;          // which stat to modify
}

export type MoveEffectType =
  | 'burn'
  | 'poison'
  | 'paralyze'
  | 'sleep'
  | 'freeze'
  | 'confuse'
  | 'stat_up'
  | 'stat_down'
  | 'heal'
  | 'recoil'
  | 'drain'
  | 'flinch';

export enum BattlePhase {
  INTRO = 'INTRO',
  PLAYER_TURN = 'PLAYER_TURN',
  ENEMY_TURN = 'ENEMY_TURN',
  EXECUTING = 'EXECUTING',
  RESOLUTION = 'RESOLUTION',
  SWITCH = 'SWITCH',
  FORCE_SWITCH = 'FORCE_SWITCH',
  CAPTURE = 'CAPTURE',
  END = 'END',
}

export enum BattleAction {
  FIGHT = 'FIGHT',
  BAG = 'BAG',
  SWITCH = 'SWITCH',
  RUN = 'RUN',
}

export interface IBattleResult {
  won: boolean;
  fled: boolean;
  captured: boolean;
  capturedDino?: Dino;
  xpGained: number;
  moneyGained: number;
}

interface ITurnAction {
  actor: 'player' | 'enemy';
  type: BattleAction;
  moveIndex?: number;
  itemId?: number;
  switchIndex?: number;
  ballId?: number;
  priority: number;
  speed: number;
}

export interface IBattleConfig {
  isWild: boolean;
  terrain?: string;
  trainerName?: string;
  trainerSprite?: string;
  canRun?: boolean;
  moneyReward?: number;
}

// ===================== Move Registry =====================

const moveRegistry: Map<number, IMoveData> = new Map();

export function registerMoves(data: IMoveData[]): void {
  for (const m of data) {
    moveRegistry.set(m.id, m);
  }
}

export function getMove(id: number): IMoveData {
  const m = moveRegistry.get(id);
  if (!m) throw new Error(`Unknown move id: ${id}`);
  return m;
}

export function hasMove(id: number): boolean {
  return moveRegistry.has(id);
}

// ===================== Battle System =====================

export class BattleSystem extends Phaser.Events.EventEmitter {
  // Party references
  playerParty: Dino[] = [];
  enemyParty: Dino[] = [];

  // Active dinos
  playerDino!: Dino;
  enemyDino!: Dino;
  playerDinoIndex: number = 0;
  enemyDinoIndex: number = 0;

  // Battle state
  phase: BattlePhase = BattlePhase.INTRO;
  config!: IBattleConfig;
  turnCount: number = 0;
  runAttempts: number = 0;

  // Stat stage modifiers (-6 to +6)
  playerStatStages: Record<string, number> = {};
  enemyStatStages: Record<string, number> = {};

  // Volatile status (confusion, flinch -- reset on switch)
  playerConfused: number = 0; // turns remaining
  enemyConfused: number = 0;
  playerFlinched: boolean = false;
  enemyFlinched: boolean = false;

  // Result
  result: IBattleResult | null = null;

  constructor() {
    super();
  }

  // ==================== Public API ====================

  startBattle(
    playerParty: Dino[],
    enemyParty: Dino[],
    config: IBattleConfig,
  ): void {
    this.playerParty = playerParty;
    this.enemyParty = enemyParty;
    this.config = config;
    this.turnCount = 0;
    this.runAttempts = 0;
    this.result = null;

    // Select first healthy dinos
    this.playerDinoIndex = this.getFirstHealthyIndex(this.playerParty);
    this.enemyDinoIndex = this.getFirstHealthyIndex(this.enemyParty);
    this.playerDino = this.playerParty[this.playerDinoIndex];
    this.enemyDino = this.enemyParty[this.enemyDinoIndex];

    this.resetStatStages('player');
    this.resetStatStages('enemy');
    this.playerConfused = 0;
    this.enemyConfused = 0;

    this.phase = BattlePhase.INTRO;
    this.emit('battleStart', {
      playerDino: this.playerDino,
      enemyDino: this.enemyDino,
      isWild: config.isWild,
      trainerName: config.trainerName,
    });
  }

  /** Player selects a move. */
  selectMove(moveIndex: number): void {
    if (this.phase !== BattlePhase.PLAYER_TURN) return;
    const slot = this.playerDino.moves[moveIndex];
    if (!slot || slot.currentPP <= 0) {
      this.emit('noPP', { moveIndex });
      return;
    }
    const move = getMove(slot.moveId);
    const playerAction: ITurnAction = {
      actor: 'player',
      type: BattleAction.FIGHT,
      moveIndex,
      priority: move.priority,
      speed: this.getEffectiveSpeed(this.playerDino, 'player'),
    };

    this.executeTurn(playerAction);
  }

  /** Player uses an item from bag. */
  selectItem(itemId: number, targetPartyIndex?: number): void {
    if (this.phase !== BattlePhase.PLAYER_TURN) return;
    const playerAction: ITurnAction = {
      actor: 'player',
      type: BattleAction.BAG,
      itemId,
      switchIndex: targetPartyIndex,
      priority: 7, // Items go before moves
      speed: 0,
    };
    this.executeTurn(playerAction);
  }

  /** Player switches dinos. */
  selectSwitch(partyIndex: number): void {
    if (this.phase !== BattlePhase.PLAYER_TURN && this.phase !== BattlePhase.FORCE_SWITCH) return;
    if (partyIndex === this.playerDinoIndex) return;
    if (this.playerParty[partyIndex].isFainted()) return;

    if (this.phase === BattlePhase.FORCE_SWITCH) {
      this.performSwitch('player', partyIndex);
      this.phase = BattlePhase.PLAYER_TURN;
      this.emit('switchComplete', { actor: 'player', dino: this.playerDino });

      // Check if battle should continue
      if (!this.isBattleOver()) {
        this.startPlayerTurn();
      }
      return;
    }

    const playerAction: ITurnAction = {
      actor: 'player',
      type: BattleAction.SWITCH,
      switchIndex: partyIndex,
      priority: 6, // Switches go before moves
      speed: 0,
    };
    this.executeTurn(playerAction);
  }

  /** Attempt to run from a wild battle. */
  tryRun(): void {
    if (this.phase !== BattlePhase.PLAYER_TURN) return;
    if (!this.config.isWild) {
      this.emit('cantRun', { reason: 'trainer' });
      return;
    }

    const playerAction: ITurnAction = {
      actor: 'player',
      type: BattleAction.RUN,
      priority: 7,
      speed: this.getEffectiveSpeed(this.playerDino, 'player'),
    };
    this.executeTurn(playerAction);
  }

  /** Attempt to capture a wild dino. */
  tryCapture(ballId: number): void {
    if (this.phase !== BattlePhase.PLAYER_TURN) return;
    if (!this.config.isWild) {
      this.emit('cantCapture', { reason: 'trainer' });
      return;
    }

    const playerAction: ITurnAction = {
      actor: 'player',
      type: BattleAction.BAG,
      itemId: ballId,
      priority: 7,
      speed: 0,
    };

    // Override to treat as capture
    this.phase = BattlePhase.CAPTURE;
    this.performCapture(ballId);
  }

  /** Called by scene to advance to player turn after intro. */
  beginPlayerTurn(): void {
    this.startPlayerTurn();
  }

  // ==================== Turn Execution ====================

  private executeTurn(playerAction: ITurnAction): void {
    this.phase = BattlePhase.EXECUTING;
    this.turnCount += 1;
    this.playerFlinched = false;
    this.enemyFlinched = false;

    // Handle non-fight actions first
    if (playerAction.type === BattleAction.RUN) {
      this.executeRun();
      return;
    }

    // Get enemy action
    const enemyAction = this.getAIAction();

    // Determine order
    const actions = this.determineTurnOrder(playerAction, enemyAction);

    // Execute in order
    this.executeActions(actions);
  }

  private async executeActions(actions: ITurnAction[]): Promise<void> {
    for (const action of actions) {
      if (this.isBattleOver()) break;

      switch (action.type) {
        case BattleAction.FIGHT:
          this.executeFight(action);
          break;
        case BattleAction.SWITCH:
          if (action.actor === 'player' && action.switchIndex !== undefined) {
            this.performSwitch('player', action.switchIndex);
            this.emit('switchComplete', { actor: 'player', dino: this.playerDino });
          }
          break;
        case BattleAction.BAG:
          this.emit('itemUsed', { actor: action.actor, itemId: action.itemId });
          break;
      }

      // Check for faint after each action
      if (this.playerDino.isFainted()) {
        this.emit('faint', { actor: 'player', dino: this.playerDino });
        if (!this.hasHealthyDino(this.playerParty)) {
          this.endBattle(false);
          return;
        }
        // Force switch
        this.phase = BattlePhase.FORCE_SWITCH;
        this.emit('forceSwitch', { actor: 'player' });
        return;
      }

      if (this.enemyDino.isFainted()) {
        this.emit('faint', { actor: 'enemy', dino: this.enemyDino });
        this.awardXp();

        if (!this.hasHealthyDino(this.enemyParty)) {
          this.endBattle(true);
          return;
        }
        // AI switches to next dino
        this.performEnemySwitch();
      }
    }

    // End-of-turn resolution
    this.resolveEndOfTurn();
  }

  private executeFight(action: ITurnAction): void {
    const isPlayer = action.actor === 'player';
    const attacker = isPlayer ? this.playerDino : this.enemyDino;
    const defender = isPlayer ? this.enemyDino : this.playerDino;
    const attackerSide = isPlayer ? 'player' : 'enemy';
    const defenderSide = isPlayer ? 'enemy' : 'player';

    // Check if attacker is fainted
    if (attacker.isFainted()) return;

    // Check flinch
    if (isPlayer && this.playerFlinched || !isPlayer && this.enemyFlinched) {
      this.emit('flinch', { actor: attackerSide, dino: attacker });
      return;
    }

    // Check paralysis (25% chance to not move)
    if (attacker.status === StatusEffect.Paralysis && Math.random() < 0.25) {
      this.emit('paralyzed', { actor: attackerSide, dino: attacker });
      return;
    }

    // Check sleep
    if (attacker.status === StatusEffect.Sleep) {
      // 33% chance to wake up each turn
      if (Math.random() < 0.33) {
        attacker.cureStatus();
        this.emit('wakeUp', { actor: attackerSide, dino: attacker });
      } else {
        this.emit('asleep', { actor: attackerSide, dino: attacker });
        return;
      }
    }

    // Check freeze (20% chance to thaw)
    if (attacker.status === StatusEffect.Freeze) {
      if (Math.random() < 0.2) {
        attacker.cureStatus();
        this.emit('thaw', { actor: attackerSide, dino: attacker });
      } else {
        this.emit('frozen', { actor: attackerSide, dino: attacker });
        return;
      }
    }

    // Check confusion
    const confusionTurns = isPlayer ? this.playerConfused : this.enemyConfused;
    if (confusionTurns > 0) {
      if (isPlayer) this.playerConfused--;
      else this.enemyConfused--;

      if (Math.random() < 0.33) {
        // Hit self in confusion
        const selfDamage = this.calculateConfusionDamage(attacker, attackerSide);
        attacker.takeDamage(selfDamage);
        this.emit('confusionHit', { actor: attackerSide, dino: attacker, damage: selfDamage });
        return;
      }
    }

    // Get move data
    const moveSlot = attacker.moves[action.moveIndex ?? 0];
    if (!moveSlot || moveSlot.currentPP <= 0) return;

    const move = getMove(moveSlot.moveId);
    moveSlot.currentPP -= 1;

    this.emit('moveUsed', {
      actor: attackerSide,
      dino: attacker,
      move,
    });

    // Accuracy check
    if (move.accuracy > 0) {
      const accMod = this.getStatStageMultiplier(
        (isPlayer ? this.playerStatStages : this.enemyStatStages)['accuracy'] ?? 0,
      );
      const evaMod = this.getStatStageMultiplier(
        (isPlayer ? this.enemyStatStages : this.playerStatStages)['evasion'] ?? 0,
      );
      const hitChance = (move.accuracy / 100) * (accMod / evaMod);
      if (Math.random() > hitChance) {
        this.emit('miss', { actor: attackerSide, dino: attacker, move });
        return;
      }
    }

    // Status moves
    if (move.category === 'status') {
      this.applyMoveEffect(move, attacker, defender, attackerSide, defenderSide, true);
      return;
    }

    // Damage calculation
    const { damage, isCritical, effectiveness } = this.calculateDamage(
      attacker, defender, move, attackerSide,
    );

    // Apply damage
    defender.takeDamage(damage);

    this.emit('damage', {
      actor: attackerSide,
      target: defenderSide,
      dino: attacker,
      targetDino: defender,
      move,
      damage,
      isCritical,
      effectiveness,
    });

    // Effectiveness message
    const effLabel = typeChart.getEffectivenessLabel(effectiveness);
    if (effLabel) {
      this.emit('effectivenessMessage', { message: effLabel, effectiveness });
    }

    // Critical message
    if (isCritical) {
      this.emit('criticalHit', { actor: attackerSide });
    }

    // Move secondary effects
    if (move.effect) {
      this.applyMoveEffect(move, attacker, defender, attackerSide, defenderSide, false);
    }

    // Fire-type move thaws frozen attacker
    if (attacker.status === StatusEffect.Freeze && move.type === DinoType.Fire) {
      attacker.cureStatus();
      this.emit('thaw', { actor: attackerSide, dino: attacker });
    }
  }

  // ==================== Damage Calculation ====================

  calculateDamage(
    attacker: Dino,
    defender: Dino,
    move: IMoveData,
    attackerSide: 'player' | 'enemy',
  ): { damage: number; isCritical: boolean; effectiveness: number } {
    if (move.power === 0) {
      return { damage: 0, isCritical: false, effectiveness: 1 };
    }

    const level = attacker.level;

    // Physical vs special
    const isPhysical = move.category === 'physical';
    const atkStat = isPhysical ? 'attack' : 'spAttack';
    const defStat = isPhysical ? 'defense' : 'spDefense';

    const atkStages = attackerSide === 'player' ? this.playerStatStages : this.enemyStatStages;
    const defStages = attackerSide === 'player' ? this.enemyStatStages : this.playerStatStages;

    let atk = attacker.stats[atkStat] * this.getStatStageMultiplier(atkStages[atkStat] ?? 0);
    let def = defender.stats[defStat] * this.getStatStageMultiplier(defStages[defStat] ?? 0);

    // Burn halves physical attack
    if (isPhysical && attacker.status === StatusEffect.Burn) {
      atk *= 0.5;
    }

    // Critical hit
    const critChance = CRITICAL_HIT_CHANCE / 100;
    const isCritical = Math.random() < critChance;

    // Critical ignores negative atk stages and positive def stages
    if (isCritical) {
      if ((atkStages[atkStat] ?? 0) < 0) {
        atk = attacker.stats[atkStat];
      }
      if ((defStages[defStat] ?? 0) > 0) {
        def = defender.stats[defStat];
      }
    }

    // Base damage formula
    let damage = Math.floor(((2 * level / 5 + 2) * move.power * (atk / def)) / 50 + 2);

    // STAB
    const hasStab = move.type === attacker.type1 ||
      (attacker.type2 !== undefined && move.type === attacker.type2);
    if (hasStab) {
      damage = Math.floor(damage * STAB_MULTIPLIER);
    }

    // Type effectiveness
    const effectiveness = typeChart.getEffectiveness(
      move.type, defender.type1, defender.type2,
    );
    damage = Math.floor(damage * effectiveness);

    // Critical multiplier
    if (isCritical) {
      damage = Math.floor(damage * CRITICAL_HIT_MULTIPLIER);
    }

    // Random factor (0.85 to 1.0)
    const rand = DAMAGE_RANDOM_MIN + Math.random() * (DAMAGE_RANDOM_MAX - DAMAGE_RANDOM_MIN);
    damage = Math.floor(damage * rand);

    // Minimum 1 damage (if move has power and types allow it)
    if (effectiveness > 0 && damage < 1) damage = 1;

    return { damage, isCritical, effectiveness };
  }

  private calculateConfusionDamage(dino: Dino, side: 'player' | 'enemy'): number {
    const level = dino.level;
    const stages = side === 'player' ? this.playerStatStages : this.enemyStatStages;
    const atk = dino.stats.attack * this.getStatStageMultiplier(stages['attack'] ?? 0);
    const def = dino.stats.defense * this.getStatStageMultiplier(stages['defense'] ?? 0);
    // Confusion damage uses a fixed power of 40
    let damage = Math.floor(((2 * level / 5 + 2) * 40 * (atk / def)) / 50 + 2);
    return Math.max(1, damage);
  }

  // ==================== Move Effects ====================

  private applyMoveEffect(
    move: IMoveData,
    attacker: Dino,
    defender: Dino,
    attackerSide: 'player' | 'enemy',
    defenderSide: 'player' | 'enemy',
    isStatusMove: boolean,
  ): void {
    const effect = move.effect;
    if (!effect) return;

    // Check probability
    const chance = isStatusMove ? 100 : (effect.chance ?? 100);
    if (Math.random() * 100 > chance) return;

    const target = effect.target === 'self' ? attacker : defender;
    const targetSide = effect.target === 'self' ? attackerSide : defenderSide;

    switch (effect.type) {
      case 'burn':
        if (target.type1 !== DinoType.Fire && target.type2 !== DinoType.Fire) {
          if (target.setStatus(StatusEffect.Burn)) {
            this.emit('statusApplied', { actor: targetSide, dino: target, status: StatusEffect.Burn });
          }
        }
        break;

      case 'poison':
        if (target.type1 !== DinoType.Venom && target.type2 !== DinoType.Venom) {
          if (target.setStatus(StatusEffect.Poison)) {
            this.emit('statusApplied', { actor: targetSide, dino: target, status: StatusEffect.Poison });
          }
        }
        break;

      case 'paralyze':
        if (target.type1 !== DinoType.Electric && target.type2 !== DinoType.Electric) {
          if (target.setStatus(StatusEffect.Paralysis)) {
            this.emit('statusApplied', { actor: targetSide, dino: target, status: StatusEffect.Paralysis });
          }
        }
        break;

      case 'sleep':
        if (target.setStatus(StatusEffect.Sleep)) {
          this.emit('statusApplied', { actor: targetSide, dino: target, status: StatusEffect.Sleep });
        }
        break;

      case 'freeze':
        if (target.type1 !== DinoType.Ice && target.type2 !== DinoType.Ice) {
          if (target.setStatus(StatusEffect.Freeze)) {
            this.emit('statusApplied', { actor: targetSide, dino: target, status: StatusEffect.Freeze });
          }
        }
        break;

      case 'confuse':
        if (targetSide === 'player') {
          if (this.playerConfused === 0) {
            this.playerConfused = 2 + Math.floor(Math.random() * 4); // 2-5 turns
            this.emit('statusApplied', { actor: targetSide, dino: target, status: 'confuse' });
          }
        } else {
          if (this.enemyConfused === 0) {
            this.enemyConfused = 2 + Math.floor(Math.random() * 4);
            this.emit('statusApplied', { actor: targetSide, dino: target, status: 'confuse' });
          }
        }
        break;

      case 'stat_up': {
        const stages = targetSide === 'player' ? this.playerStatStages : this.enemyStatStages;
        const stat = effect.stat ?? 'attack';
        const val = effect.value ?? 1;
        const oldStage = stages[stat] ?? 0;
        stages[stat] = Math.min(6, oldStage + val);
        this.emit('statChange', { actor: targetSide, dino: target, stat, stages: stages[stat] - oldStage });
        break;
      }

      case 'stat_down': {
        const stages = targetSide === 'player' ? this.playerStatStages : this.enemyStatStages;
        const stat = effect.stat ?? 'attack';
        const val = effect.value ?? 1;
        const oldStage = stages[stat] ?? 0;
        stages[stat] = Math.max(-6, oldStage - val);
        this.emit('statChange', { actor: targetSide, dino: target, stat, stages: stages[stat] - oldStage });
        break;
      }

      case 'heal': {
        const amount = Math.floor(attacker.maxHp * (effect.value ?? 50) / 100);
        attacker.heal(amount);
        this.emit('heal', { actor: attackerSide, dino: attacker, amount });
        break;
      }

      case 'recoil': {
        // Recoil is a percentage of damage dealt -- we approximate with move power
        const recoil = Math.max(1, Math.floor(attacker.maxHp * (effect.value ?? 25) / 100));
        attacker.takeDamage(recoil);
        this.emit('recoil', { actor: attackerSide, dino: attacker, damage: recoil });
        break;
      }

      case 'drain': {
        // Drain heals attacker by a fraction of damage dealt
        // The actual damage was already applied; we estimate here
        const drainAmount = Math.max(1, Math.floor(attacker.maxHp * (effect.value ?? 50) / 100));
        attacker.heal(drainAmount);
        this.emit('drain', { actor: attackerSide, dino: attacker, amount: drainAmount });
        break;
      }

      case 'flinch':
        if (targetSide === 'player') this.playerFlinched = true;
        else this.enemyFlinched = true;
        break;
    }
  }

  // ==================== End-of-Turn ====================

  private resolveEndOfTurn(): void {
    this.phase = BattlePhase.RESOLUTION;

    // Burn damage (1/16 max HP)
    this.applyEndOfTurnStatus(this.playerDino, 'player');
    if (this.isBattleOver()) return;
    this.applyEndOfTurnStatus(this.enemyDino, 'enemy');
    if (this.isBattleOver()) return;

    // Check for faints from end-of-turn effects
    if (this.playerDino.isFainted()) {
      this.emit('faint', { actor: 'player', dino: this.playerDino });
      if (!this.hasHealthyDino(this.playerParty)) {
        this.endBattle(false);
        return;
      }
      this.phase = BattlePhase.FORCE_SWITCH;
      this.emit('forceSwitch', { actor: 'player' });
      return;
    }

    if (this.enemyDino.isFainted()) {
      this.emit('faint', { actor: 'enemy', dino: this.enemyDino });
      this.awardXp();
      if (!this.hasHealthyDino(this.enemyParty)) {
        this.endBattle(true);
        return;
      }
      this.performEnemySwitch();
    }

    // Next turn
    this.startPlayerTurn();
  }

  private applyEndOfTurnStatus(dino: Dino, side: 'player' | 'enemy'): void {
    if (dino.isFainted()) return;

    switch (dino.status) {
      case StatusEffect.Burn: {
        const damage = Math.max(1, Math.floor(dino.maxHp / 16));
        dino.takeDamage(damage);
        this.emit('statusDamage', { actor: side, dino, status: StatusEffect.Burn, damage });
        break;
      }
      case StatusEffect.Poison: {
        const damage = Math.max(1, Math.floor(dino.maxHp / 8));
        dino.takeDamage(damage);
        this.emit('statusDamage', { actor: side, dino, status: StatusEffect.Poison, damage });
        break;
      }
    }
  }

  // ==================== Capture ====================

  private performCapture(ballId: number): void {
    const target = this.enemyDino;
    const ballModifier = this.getBallModifier(ballId);
    const statusMod = this.getCaptureStatusModifier(target.status);
    const captureRate = target.species.captureRate;

    // rate = ((3*maxHp - 2*currentHp) / (3*maxHp)) * captureRate * ballModifier * statusModifier
    const hpFactor = (3 * target.maxHp - 2 * target.currentHp) / (3 * target.maxHp);
    const rate = Math.min(MAX_CAPTURE_RATE, hpFactor * captureRate * ballModifier * statusMod);

    // Shake probability: each of 4 shakes must pass
    // shakeChance = sqrt(sqrt(rate / 255))  (fourth root)
    const shakeChance = Math.pow(rate / MAX_CAPTURE_RATE, 0.25);

    let shakes = 0;
    for (let i = 0; i < 4; i++) {
      if (Math.random() < shakeChance) {
        shakes++;
      } else {
        break;
      }
    }

    this.emit('captureAttempt', { ballId, shakes });

    if (shakes === 4) {
      // Captured!
      this.emit('captureSuccess', { dino: target });
      this.result = {
        won: true,
        fled: false,
        captured: true,
        capturedDino: target,
        xpGained: 0,
        moneyGained: 0,
      };
      this.phase = BattlePhase.END;
      this.emit('battleEnd', this.result);
    } else {
      this.emit('captureFail', { shakes });
      // Enemy gets a turn after failed capture
      const enemyAction = this.getAIAction();
      this.executeFight(enemyAction);

      // Check for player faint
      if (this.playerDino.isFainted()) {
        this.emit('faint', { actor: 'player', dino: this.playerDino });
        if (!this.hasHealthyDino(this.playerParty)) {
          this.endBattle(false);
          return;
        }
        this.phase = BattlePhase.FORCE_SWITCH;
        this.emit('forceSwitch', { actor: 'player' });
        return;
      }

      this.resolveEndOfTurn();
    }
  }

  private getBallModifier(ballId: number): number {
    // Ball item IDs: 1=jurassic, 2=super, 3=ultra
    switch (ballId) {
      case 1: return 1.0;
      case 2: return 1.5;
      case 3: return 2.0;
      default: return 1.0;
    }
  }

  private getCaptureStatusModifier(status: StatusEffect): number {
    switch (status) {
      case StatusEffect.Sleep:
      case StatusEffect.Freeze:
        return 2.5;
      case StatusEffect.Paralysis:
      case StatusEffect.Poison:
      case StatusEffect.Burn:
        return 1.5;
      default:
        return 1.0;
    }
  }

  // ==================== Running ====================

  private executeRun(): void {
    this.runAttempts += 1;
    const playerSpeed = this.getEffectiveSpeed(this.playerDino, 'player');
    const enemySpeed = this.getEffectiveSpeed(this.enemyDino, 'enemy');

    // Escape formula: (playerSpeed * 128 / enemySpeed + 30 * runAttempts) mod 256
    // If >= 128 * random, escape succeeds
    let escapeChance: number;
    if (enemySpeed === 0) {
      escapeChance = 256;
    } else {
      escapeChance = Math.floor((playerSpeed * 128) / enemySpeed + 30 * this.runAttempts) % 256;
    }

    if (escapeChance >= Math.floor(Math.random() * 256)) {
      this.emit('runSuccess', {});
      this.result = {
        won: false,
        fled: true,
        captured: false,
        xpGained: 0,
        moneyGained: 0,
      };
      this.phase = BattlePhase.END;
      this.emit('battleEnd', this.result);
    } else {
      this.emit('runFail', {});
      // Enemy gets a free turn
      const enemyAction = this.getAIAction();
      this.executeFight(enemyAction);

      if (this.playerDino.isFainted()) {
        this.emit('faint', { actor: 'player', dino: this.playerDino });
        if (!this.hasHealthyDino(this.playerParty)) {
          this.endBattle(false);
          return;
        }
        this.phase = BattlePhase.FORCE_SWITCH;
        this.emit('forceSwitch', { actor: 'player' });
        return;
      }

      this.resolveEndOfTurn();
    }
  }

  // ==================== AI ====================

  getAIAction(): ITurnAction {
    const dino = this.enemyDino;
    const availableMoves = dino.moves
      .map((slot, index) => ({ slot, index }))
      .filter(m => m.slot.currentPP > 0);

    if (availableMoves.length === 0) {
      // Struggle equivalent: use first move with 0 PP -- will deal recoil
      return {
        actor: 'enemy',
        type: BattleAction.FIGHT,
        moveIndex: 0,
        priority: 0,
        speed: this.getEffectiveSpeed(dino, 'enemy'),
      };
    }

    if (this.config.isWild) {
      // Wild dinos pick a random move
      const pick = availableMoves[Math.floor(Math.random() * availableMoves.length)];
      const move = getMove(pick.slot.moveId);
      return {
        actor: 'enemy',
        type: BattleAction.FIGHT,
        moveIndex: pick.index,
        priority: move.priority,
        speed: this.getEffectiveSpeed(dino, 'enemy'),
      };
    }

    // Trainer AI: pick the move with best estimated damage
    let bestIndex = availableMoves[0].index;
    let bestScore = -1;

    for (const { slot, index } of availableMoves) {
      const move = getMove(slot.moveId);
      let score = 0;

      if (move.category === 'status') {
        // Status moves get a base score of 30
        score = 30;
        // Prefer status if opponent has no status
        if (this.playerDino.status === StatusEffect.None && move.effect) {
          score += 20;
        }
      } else {
        // Estimate damage
        const effectiveness = typeChart.getEffectiveness(
          move.type, this.playerDino.type1, this.playerDino.type2,
        );
        const stab = (move.type === dino.type1 || move.type === dino.type2) ? STAB_MULTIPLIER : 1;
        score = move.power * effectiveness * stab;
      }

      if (score > bestScore) {
        bestScore = score;
        bestIndex = index;
      }
    }

    const bestMove = getMove(dino.moves[bestIndex].moveId);
    return {
      actor: 'enemy',
      type: BattleAction.FIGHT,
      moveIndex: bestIndex,
      priority: bestMove.priority,
      speed: this.getEffectiveSpeed(dino, 'enemy'),
    };
  }

  // ==================== Turn Order ====================

  private determineTurnOrder(a: ITurnAction, b: ITurnAction): ITurnAction[] {
    // Higher priority goes first
    if (a.priority !== b.priority) {
      return a.priority > b.priority ? [a, b] : [b, a];
    }
    // Same priority: faster goes first
    if (a.speed !== b.speed) {
      return a.speed > b.speed ? [a, b] : [b, a];
    }
    // Speed tie: random
    return Math.random() < 0.5 ? [a, b] : [b, a];
  }

  // ==================== Switching ====================

  private performSwitch(side: 'player' | 'enemy', partyIndex: number): void {
    if (side === 'player') {
      this.playerDinoIndex = partyIndex;
      this.playerDino = this.playerParty[partyIndex];
      this.resetStatStages('player');
      this.playerConfused = 0;
      this.playerFlinched = false;
    } else {
      this.enemyDinoIndex = partyIndex;
      this.enemyDino = this.enemyParty[partyIndex];
      this.resetStatStages('enemy');
      this.enemyConfused = 0;
      this.enemyFlinched = false;
    }
  }

  private performEnemySwitch(): void {
    const nextIndex = this.getFirstHealthyIndex(this.enemyParty);
    if (nextIndex >= 0) {
      this.performSwitch('enemy', nextIndex);
      this.emit('switchComplete', { actor: 'enemy', dino: this.enemyDino });
    }
  }

  // ==================== XP ====================

  private awardXp(): void {
    const defeated = this.enemyDino;
    const baseXp = defeated.species.baseXpYield;
    const level = defeated.level;
    const isWild = this.config.isWild;

    // XP formula: ((baseXp * level) / 5) * (isTrainer ? 1.5 : 1)
    const trainerBonus = isWild ? 1.0 : 1.5;
    const xpAmount = Math.floor((baseXp * level / 5) * trainerBonus);

    this.emit('xpGain', {
      dino: this.playerDino,
      amount: xpAmount,
    });

    const result = this.playerDino.gainXp(xpAmount);
    if (result.levelsGained > 0) {
      this.emit('levelUp', {
        dino: this.playerDino,
        newLevel: this.playerDino.level,
        levelsGained: result.levelsGained,
      });
    }

    for (const moveId of result.movesLearned) {
      this.emit('moveLearned', {
        dino: this.playerDino,
        moveId,
      });
    }
  }

  // ==================== Battle End ====================

  private endBattle(playerWon: boolean): void {
    this.phase = BattlePhase.END;
    const moneyGained = playerWon ? (this.config.moneyReward ?? 0) : 0;

    this.result = {
      won: playerWon,
      fled: false,
      captured: false,
      xpGained: 0,
      moneyGained,
    };

    this.emit('battleEnd', this.result);
  }

  private startPlayerTurn(): void {
    this.phase = BattlePhase.PLAYER_TURN;
    this.emit('playerTurn', {
      playerDino: this.playerDino,
      enemyDino: this.enemyDino,
    });
  }

  // ==================== Utility ====================

  private isBattleOver(): boolean {
    return this.phase === BattlePhase.END;
  }

  private hasHealthyDino(party: Dino[]): boolean {
    return party.some(d => !d.isFainted());
  }

  private getFirstHealthyIndex(party: Dino[]): number {
    return party.findIndex(d => !d.isFainted());
  }

  private resetStatStages(side: 'player' | 'enemy'): void {
    const stages: Record<string, number> = {
      attack: 0,
      defense: 0,
      spAttack: 0,
      spDefense: 0,
      speed: 0,
      accuracy: 0,
      evasion: 0,
    };
    if (side === 'player') this.playerStatStages = stages;
    else this.enemyStatStages = stages;
  }

  private getEffectiveSpeed(dino: Dino, side: 'player' | 'enemy'): number {
    const stages = side === 'player' ? this.playerStatStages : this.enemyStatStages;
    let speed = dino.stats.speed * this.getStatStageMultiplier(stages['speed'] ?? 0);
    // Paralysis halves speed
    if (dino.status === StatusEffect.Paralysis) {
      speed *= 0.5;
    }
    return Math.floor(speed);
  }

  /**
   * Stat stage multiplier:
   * +1 = 1.5, +2 = 2.0, +3 = 2.5, +4 = 3.0, +5 = 3.5, +6 = 4.0
   * -1 = 0.67, -2 = 0.5, -3 = 0.4, -4 = 0.33, -5 = 0.29, -6 = 0.25
   */
  private getStatStageMultiplier(stage: number): number {
    const clamped = Math.max(-6, Math.min(6, stage));
    if (clamped >= 0) {
      return (2 + clamped) / 2;
    }
    return 2 / (2 - clamped);
  }

  // ==================== Public State Queries ====================

  getPlayerDino(): Dino { return this.playerDino; }
  getEnemyDino(): Dino { return this.enemyDino; }
  getPhase(): BattlePhase { return this.phase; }
  isWildBattle(): boolean { return this.config.isWild; }
  getTurnCount(): number { return this.turnCount; }
  getResult(): IBattleResult | null { return this.result; }

  canPlayerRun(): boolean {
    return this.config.isWild && (this.config.canRun !== false);
  }

  getPlayerMoves(): { move: IMoveData; currentPP: number; maxPP: number }[] {
    return this.playerDino.moves.map(slot => ({
      move: getMove(slot.moveId),
      currentPP: slot.currentPP,
      maxPP: slot.maxPP,
    }));
  }
}
