// ============================================================
// Jurassic Trainers -- Dino Entity
// ============================================================

import {
  DinoType,
  StatusEffect,
  MAX_MOVE_SLOTS,
} from '../utils/constants';
import { hasMove as hasMoveData, getMove as getMoveData } from '../systems/BattleSystem';

// --------------- Interfaces ---------------

/** Raw species data loaded from dinos.json */
export interface ISpeciesData {
  id: number;
  name: string;
  type1: DinoType;
  type2?: DinoType;
  baseStats: IBaseStats;
  xpGroup: XpGroup;
  captureRate: number;
  baseXpYield: number;
  learnset: ILearnsetEntry[];
  evolution?: IEvolutionData;
  description: string;
  height: number;   // metres
  weight: number;   // kg
  sprite: string;   // asset key
}

export interface IBaseStats {
  hp: number;
  attack: number;
  defense: number;
  spAttack: number;
  spDefense: number;
  speed: number;
}

export interface IStats {
  hp: number;
  attack: number;
  defense: number;
  spAttack: number;
  spDefense: number;
  speed: number;
}

export interface ILearnsetEntry {
  level: number;
  moveId: number;
}

export interface IEvolutionData {
  targetId: number;
  level: number;       // minimum level to evolve
  condition?: string;  // optional extra condition (item, friendship, etc.)
}

export interface IMoveSlot {
  moveId: number;
  currentPP: number;
  maxPP: number;
}

export type XpGroup = 'fast' | 'medium' | 'slow';

/** Temperament modifiers: one stat gets +10%, another -10%. */
export interface ITemperament {
  name: string;
  plus: keyof Omit<IStats, 'hp'> | null;   // null = neutral
  minus: keyof Omit<IStats, 'hp'> | null;
}

// 25 temperaments (5 neutral on the diagonal)
export const TEMPERAMENTS: ITemperament[] = [
  // Neutral (no effect)
  { name: 'Hardy',   plus: null,        minus: null },
  { name: 'Docile',  plus: null,        minus: null },
  { name: 'Serious', plus: null,        minus: null },
  { name: 'Bashful', plus: null,        minus: null },
  { name: 'Quirky',  plus: null,        minus: null },
  // +Atk
  { name: 'Lonely',  plus: 'attack',    minus: 'defense' },
  { name: 'Brave',   plus: 'attack',    minus: 'speed' },
  { name: 'Adamant', plus: 'attack',    minus: 'spAttack' },
  { name: 'Naughty', plus: 'attack',    minus: 'spDefense' },
  // +Def
  { name: 'Bold',    plus: 'defense',   minus: 'attack' },
  { name: 'Relaxed', plus: 'defense',   minus: 'speed' },
  { name: 'Impish',  plus: 'defense',   minus: 'spAttack' },
  { name: 'Lax',     plus: 'defense',   minus: 'spDefense' },
  // +SpAtk
  { name: 'Modest',  plus: 'spAttack',  minus: 'attack' },
  { name: 'Mild',    plus: 'spAttack',  minus: 'defense' },
  { name: 'Quiet',   plus: 'spAttack',  minus: 'speed' },
  { name: 'Rash',    plus: 'spAttack',  minus: 'spDefense' },
  // +SpDef
  { name: 'Calm',    plus: 'spDefense', minus: 'attack' },
  { name: 'Gentle',  plus: 'spDefense', minus: 'defense' },
  { name: 'Sassy',   plus: 'spDefense', minus: 'speed' },
  { name: 'Careful', plus: 'spDefense', minus: 'spAttack' },
  // +Speed
  { name: 'Timid',   plus: 'speed',     minus: 'attack' },
  { name: 'Hasty',   plus: 'speed',     minus: 'defense' },
  { name: 'Jolly',   plus: 'speed',     minus: 'spAttack' },
  { name: 'Naive',   plus: 'speed',     minus: 'spDefense' },
];

export interface IDinoSerialized {
  uid: string;
  speciesId: number;
  nickname: string;
  level: number;
  xp: number;
  currentHp: number;
  gv: IStats;
  tp: IStats;
  temperamentIndex: number;
  moves: IMoveSlot[];
  status: StatusEffect;
  friendship: number;
}

// --------------- Constants ---------------

const MAX_LEVEL = 50;
const MAX_GV = 31;
const MAX_TP_PER_STAT = 252;
const MAX_TP_TOTAL = 510;
const MAX_FRIENDSHIP = 255;

// --------------- Helpers ---------------

let _uidCounter = 0;

function generateUid(): string {
  _uidCounter += 1;
  return `dino_${Date.now()}_${_uidCounter}`;
}

function randomInt(min: number, max: number): number {
  return Math.floor(Math.random() * (max - min + 1)) + min;
}

function randomGV(): number {
  return randomInt(0, MAX_GV);
}

function randomGoodGV(): number {
  // Starter-quality: 20-31 per stat
  return randomInt(20, MAX_GV);
}

// --------------- Species Registry ---------------

/** Static registry of species data, loaded once from dinos.json */
const speciesRegistry: Map<number, ISpeciesData> = new Map();

export function registerSpecies(data: ISpeciesData[]): void {
  for (const sp of data) {
    speciesRegistry.set(sp.id, sp);
  }
}

export function getSpecies(id: number): ISpeciesData {
  const sp = speciesRegistry.get(id);
  if (!sp) throw new Error(`Unknown species id: ${id}`);
  return sp;
}

export function hasSpecies(id: number): boolean {
  return speciesRegistry.has(id);
}

// --------------- Dino Class ---------------

export class Dino {
  uid: string;
  speciesId: number;
  nickname: string;
  level: number;
  xp: number;
  currentHp: number;
  maxHp: number;
  stats: IStats;
  gv: IStats;
  tp: IStats;
  temperament: ITemperament;
  temperamentIndex: number;
  moves: IMoveSlot[];
  status: StatusEffect;
  friendship: number;

  private constructor(
    speciesId: number,
    level: number,
    gv: IStats,
    tp: IStats,
    temperamentIndex: number,
    nickname?: string,
  ) {
    this.uid = generateUid();
    this.speciesId = speciesId;
    this.level = Math.min(level, MAX_LEVEL);
    this.xp = Dino.getTotalXpForLevel(this.level, this.xpGroup);
    this.gv = gv;
    this.tp = tp;
    this.temperamentIndex = temperamentIndex;
    this.temperament = TEMPERAMENTS[temperamentIndex];
    this.moves = [];
    this.status = StatusEffect.None;
    this.friendship = 70;
    this.nickname = nickname ?? this.species.name;

    // Calculate stats and set HP to max
    this.stats = { hp: 0, attack: 0, defense: 0, spAttack: 0, spDefense: 0, speed: 0 };
    this.maxHp = 0;
    this.currentHp = 0;
    this.calculateStats();
    this.currentHp = this.maxHp;

    // Populate learnset up to current level
    this.populateMovesForLevel();
  }

  // --------------- Accessors ---------------

  get species(): ISpeciesData {
    return getSpecies(this.speciesId);
  }

  get xpGroup(): XpGroup {
    return this.species.xpGroup;
  }

  get type1(): DinoType {
    return this.species.type1;
  }

  get type2(): DinoType | undefined {
    return this.species.type2;
  }

  // --------------- Factory Methods ---------------

  static createWild(speciesId: number, level: number): Dino {
    const gv: IStats = {
      hp: randomGV(),
      attack: randomGV(),
      defense: randomGV(),
      spAttack: randomGV(),
      spDefense: randomGV(),
      speed: randomGV(),
    };
    const tp: IStats = { hp: 0, attack: 0, defense: 0, spAttack: 0, spDefense: 0, speed: 0 };
    const temperamentIndex = randomInt(0, TEMPERAMENTS.length - 1);
    return new Dino(speciesId, level, gv, tp, temperamentIndex);
  }

  static createStarter(speciesId: number, level: number): Dino {
    const gv: IStats = {
      hp: randomGoodGV(),
      attack: randomGoodGV(),
      defense: randomGoodGV(),
      spAttack: randomGoodGV(),
      spDefense: randomGoodGV(),
      speed: randomGoodGV(),
    };
    const tp: IStats = { hp: 0, attack: 0, defense: 0, spAttack: 0, spDefense: 0, speed: 0 };
    // Starters get a beneficial temperament (non-neutral)
    const nonNeutral = TEMPERAMENTS.map((t, i) => ({ t, i })).filter(x => x.t.plus !== null);
    const pick = nonNeutral[randomInt(0, nonNeutral.length - 1)];
    return new Dino(speciesId, level, gv, tp, pick.i);
  }

  static fromSerialized(data: IDinoSerialized): Dino {
    const dino = new Dino(
      data.speciesId,
      data.level,
      { ...data.gv },
      { ...data.tp },
      data.temperamentIndex,
      data.nickname,
    );
    dino.uid = data.uid;
    dino.xp = data.xp;
    dino.currentHp = data.currentHp;
    dino.moves = data.moves.map(m => ({ ...m }));
    dino.status = data.status;
    dino.friendship = data.friendship;
    dino.calculateStats();
    // Clamp HP to max (species might have changed via data update)
    if (dino.currentHp > dino.maxHp) dino.currentHp = dino.maxHp;
    return dino;
  }

  // --------------- Serialization ---------------

  serialize(): IDinoSerialized {
    return {
      uid: this.uid,
      speciesId: this.speciesId,
      nickname: this.nickname,
      level: this.level,
      xp: this.xp,
      currentHp: this.currentHp,
      gv: { ...this.gv },
      tp: { ...this.tp },
      temperamentIndex: this.temperamentIndex,
      moves: this.moves.map(m => ({ ...m })),
      status: this.status,
      friendship: this.friendship,
    };
  }

  // --------------- Stat Calculation ---------------

  calculateStats(): void {
    const base = this.species.baseStats;
    const lvl = this.level;

    // HP = floor(((2 * base + gv + floor(tp/4)) * level / 100) + level + 10)
    this.stats.hp = Math.floor(
      ((2 * base.hp + this.gv.hp + Math.floor(this.tp.hp / 4)) * lvl) / 100 + lvl + 10,
    );
    this.maxHp = this.stats.hp;

    // Other stats
    const statKeys: (keyof Omit<IStats, 'hp'>)[] = ['attack', 'defense', 'spAttack', 'spDefense', 'speed'];
    for (const key of statKeys) {
      const raw = Math.floor(
        ((2 * base[key] + this.gv[key] + Math.floor(this.tp[key] / 4)) * lvl) / 100 + 5,
      );
      const mod = this.getTemperamentModifier(key);
      this.stats[key] = Math.max(1, Math.floor(raw * mod));
    }
  }

  private getTemperamentModifier(stat: keyof Omit<IStats, 'hp'>): number {
    if (this.temperament.plus === stat) return 1.1;
    if (this.temperament.minus === stat) return 0.9;
    return 1.0;
  }

  // --------------- XP / Leveling ---------------

  static getXpForLevel(level: number, group: XpGroup): number {
    if (level <= 1) return 0;
    const l3 = level * level * level;
    switch (group) {
      case 'fast':   return Math.floor(0.8 * l3);
      case 'medium': return l3;
      case 'slow':   return Math.floor(1.25 * l3);
    }
  }

  static getTotalXpForLevel(level: number, group: XpGroup): number {
    return Dino.getXpForLevel(level, group);
  }

  getXpForNextLevel(): number {
    if (this.level >= MAX_LEVEL) return 0;
    return Dino.getXpForLevel(this.level + 1, this.xpGroup);
  }

  getXpProgress(): number {
    const currentLevelXp = Dino.getXpForLevel(this.level, this.xpGroup);
    const nextLevelXp = this.getXpForNextLevel();
    if (nextLevelXp <= currentLevelXp) return 0;
    return this.xp - currentLevelXp;
  }

  getXpToNextLevel(): number {
    const currentLevelXp = Dino.getXpForLevel(this.level, this.xpGroup);
    const nextLevelXp = this.getXpForNextLevel();
    return nextLevelXp - currentLevelXp;
  }

  /**
   * Gain XP and handle level-ups.
   * Returns an array of events: level-ups and new moves learned.
   */
  gainXp(amount: number): { levelsGained: number; movesLearned: number[] } {
    if (this.level >= MAX_LEVEL) return { levelsGained: 0, movesLearned: [] };

    this.xp += amount;
    let levelsGained = 0;
    const movesLearned: number[] = [];

    while (this.level < MAX_LEVEL) {
      const needed = this.getXpForNextLevel();
      if (this.xp < needed) break;

      this.level += 1;
      levelsGained += 1;

      const oldMaxHp = this.maxHp;
      this.calculateStats();

      // Scale current HP proportionally
      this.currentHp = Math.min(
        this.maxHp,
        this.currentHp + (this.maxHp - oldMaxHp),
      );

      // Check for new moves at this level
      const newMoves = this.species.learnset.filter(e => e.level === this.level);
      for (const entry of newMoves) {
        const learned = this.tryLearnMove(entry.moveId);
        if (learned) movesLearned.push(entry.moveId);
      }
    }

    // Clamp XP at max level
    if (this.level >= MAX_LEVEL) {
      this.xp = Dino.getXpForLevel(MAX_LEVEL, this.xpGroup);
    }

    return { levelsGained, movesLearned };
  }

  /**
   * Try to learn a move. If fewer than 4 moves, add it.
   * Returns true if auto-learned, false if slot is full (caller must handle replacement).
   */
  tryLearnMove(moveId: number): boolean {
    // Don't learn duplicates
    if (this.moves.some(m => m.moveId === moveId)) return false;

    if (this.moves.length < MAX_MOVE_SLOTS) {
      // For now, default PP to 20; real PP comes from moves.json
      this.moves.push({ moveId, currentPP: 20, maxPP: 20 });
      return true;
    }
    return false; // Full -- UI must ask player which move to replace
  }

  /** Replace a move at a given slot index. */
  replaceMove(slotIndex: number, moveId: number, maxPP: number = 20): void {
    if (slotIndex < 0 || slotIndex >= this.moves.length) return;
    this.moves[slotIndex] = { moveId, currentPP: maxPP, maxPP };
  }

  /**
   * Populate initial moves from learnset up to current level.
   * Before level 7: max 2 moves — 1 Normal attack + 1 stat move. NO typed attacks.
   * At level 7+: up to MAX_MOVE_SLOTS (4) moves, typed attacks unlocked.
   */
  private populateMovesForLevel(): void {
    const learnset = this.species.learnset;
    // Max moves depends on level: 2 before level 7, 4 at level 7+
    const maxSlots = this.level < 7 ? 2 : MAX_MOVE_SLOTS;

    // Get all moves learnable at or below current level, sorted by level desc (most recent first)
    const available = learnset
      .filter(e => e.level <= this.level)
      .sort((a, b) => b.level - a.level);

    // Before level 7: only allow Normal-type moves (no elemental attacks)
    const isNormalType = (moveId: number): boolean => {
      try {
        if (hasMoveData(moveId)) {
          return getMoveData(moveId).type === DinoType.Normal;
        }
      } catch (_e) { /* registry not loaded yet */ }
      return true; // assume normal if can't check
    };

    for (const entry of available) {
      if (this.moves.length >= maxSlots) break;
      if (this.moves.some(m => m.moveId === entry.moveId)) continue;

      // Before level 7, skip non-Normal typed attacks
      if (this.level < 7 && !isNormalType(entry.moveId)) continue;

      this.moves.push({ moveId: entry.moveId, currentPP: 20, maxPP: 20 });
    }
  }

  // --------------- Evolution ---------------

  canEvolve(): boolean {
    const evo = this.species.evolution;
    if (!evo) return false;
    if (this.level < evo.level) return false;
    // Additional conditions could be checked here (items, friendship, etc.)
    if (evo.condition === 'friendship' && this.friendship < 220) return false;
    return true;
  }

  evolve(): boolean {
    if (!this.canEvolve()) return false;
    const evo = this.species.evolution!;
    this.speciesId = evo.targetId;

    // Recalculate stats with new base stats
    const oldMaxHp = this.maxHp;
    this.calculateStats();
    // Heal the HP difference from evolution
    this.currentHp += this.maxHp - oldMaxHp;
    if (this.currentHp > this.maxHp) this.currentHp = this.maxHp;

    return true;
  }

  // --------------- HP / Status ---------------

  takeDamage(amount: number): number {
    const actual = Math.min(this.currentHp, Math.max(1, Math.floor(amount)));
    this.currentHp = Math.max(0, this.currentHp - actual);
    return actual;
  }

  heal(amount: number): number {
    const missing = this.maxHp - this.currentHp;
    const actual = Math.min(missing, Math.max(0, Math.floor(amount)));
    this.currentHp += actual;
    return actual;
  }

  fullHeal(): void {
    this.currentHp = this.maxHp;
    this.status = StatusEffect.None;
    // Restore all PP
    for (const move of this.moves) {
      move.currentPP = move.maxPP;
    }
  }

  isFainted(): boolean {
    return this.currentHp <= 0;
  }

  setStatus(effect: StatusEffect): boolean {
    // Can't override an existing non-None status
    if (this.status !== StatusEffect.None && effect !== StatusEffect.None) return false;
    this.status = effect;
    return true;
  }

  cureStatus(): void {
    this.status = StatusEffect.None;
  }

  // --------------- Friendship ---------------

  addFriendship(amount: number): void {
    this.friendship = Math.min(MAX_FRIENDSHIP, Math.max(0, this.friendship + amount));
  }

  // --------------- Training Points ---------------

  addTP(stat: keyof IStats, amount: number): number {
    const currentTotal = this.tp.hp + this.tp.attack + this.tp.defense +
      this.tp.spAttack + this.tp.spDefense + this.tp.speed;
    const roomTotal = MAX_TP_TOTAL - currentTotal;
    const roomStat = MAX_TP_PER_STAT - this.tp[stat];
    const actual = Math.min(amount, roomTotal, roomStat);
    if (actual > 0) {
      this.tp[stat] += actual;
      this.calculateStats();
    }
    return actual;
  }

  // --------------- Display Helpers ---------------

  getHpPercent(): number {
    if (this.maxHp === 0) return 0;
    return this.currentHp / this.maxHp;
  }

  getXpPercent(): number {
    const toNext = this.getXpToNextLevel();
    if (toNext <= 0) return 1;
    return this.getXpProgress() / toNext;
  }

  toString(): string {
    return `${this.nickname} (Lv.${this.level} ${this.species.name}) HP:${this.currentHp}/${this.maxHp}`;
  }
}
