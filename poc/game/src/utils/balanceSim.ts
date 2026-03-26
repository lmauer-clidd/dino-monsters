/**
 * Balance Simulation Utility
 * Pure Node.js module (no Phaser) for automated battle simulation.
 * Used by balance tests to verify game balance across all 150 dinos.
 */

import { readFileSync } from 'fs';
import { join } from 'path';

// Load data
const dataDir = join(__dirname, '..', 'data');
const dinos = JSON.parse(readFileSync(join(dataDir, 'dinos.json'), 'utf-8'));
const moves = JSON.parse(readFileSync(join(dataDir, 'moves.json'), 'utf-8'));
const typeChartData = JSON.parse(readFileSync(join(dataDir, 'type_chart.json'), 'utf-8'));
const chart: number[][] = typeChartData.chart;

// Type chart JSON numbering:
// 0=Roche(Fossil), 1=Eau(Water), 2=Feu(Fire), 3=Plante(Flora), 4=Glace(Ice),
// 5=Vol(Air), 6=Terre(Earth), 7=Foudre(Electric), 8=Poison(Venom), 9=Acier(Metal),
// 10=Ombre(Shadow), 11=Lumiere(Light), 12=Sable(Normal), 13=Fossile(Primal)

export interface SimDino {
  id: number;
  name: string;
  types: number[];   // JSON type numbering
  level: number;
  hp: number;
  atk: number;
  def: number;
  spatk: number;
  spdef: number;
  speed: number;
  moves: SimMove[];
}

export interface SimMove {
  id: number;
  name: string;
  type: number;      // JSON type numbering
  category: string;  // 'physical' | 'special' | 'status'
  power: number;
  accuracy: number;
  pp: number;
}

export interface BattleResult {
  winner: 'dino1' | 'dino2' | 'draw';
  turns: number;
  dino1HpLeft: number;
  dino2HpLeft: number;
}

/**
 * Calculate stat at a given level (Pokemon formula, GV=15 average).
 */
export function calcStat(base: number, level: number, isHp: boolean): number {
  const gv = 15;
  const raw = Math.floor(((2 * base + gv) * level) / 100);
  return isHp ? raw + level + 10 : raw + 5;
}

/**
 * Build a SimDino from species data at a given level.
 * Picks the 4 strongest damaging moves available at that level.
 */
export function buildDino(speciesId: number, level: number): SimDino {
  const species = dinos.find((d: any) => d.id === speciesId);
  if (!species) throw new Error(`Species ${speciesId} not found`);

  const bs = species.baseStats;
  const hp = calcStat(bs.hp, level, true);

  // Get moves available at this level from learnset
  const availableMoves: SimMove[] = [];
  for (const entry of species.learnset) {
    if (entry.level <= level) {
      const moveData = moves.find((m: any) => m.id === entry.moveId);
      if (moveData && moveData.power > 0) {
        availableMoves.push({
          id: moveData.id,
          name: moveData.name,
          type: moveData.type,
          category: moveData.category,
          power: moveData.power,
          accuracy: moveData.accuracy,
          pp: moveData.pp,
        });
      }
    }
  }

  // Take the 4 strongest moves
  availableMoves.sort((a, b) => b.power - a.power);
  const topMoves = availableMoves.slice(0, 4);

  // If no damaging moves, add Charge (id 121, Normal/physical, power 40)
  if (topMoves.length === 0) {
    const charge = moves.find((m: any) => m.id === 121);
    if (charge) {
      topMoves.push({
        id: 121,
        name: charge.name,
        type: charge.type,
        category: charge.category,
        power: charge.power,
        accuracy: charge.accuracy,
        pp: charge.pp,
      });
    }
  }

  return {
    id: speciesId,
    name: species.name,
    types: species.types,
    level,
    hp,
    atk: calcStat(bs.atk, level, false),
    def: calcStat(bs.def, level, false),
    spatk: calcStat(bs.spatk, level, false),
    spdef: calcStat(bs.spdef, level, false),
    speed: calcStat(bs.speed, level, false),
    moves: topMoves,
  };
}

/**
 * Get type effectiveness multiplier (JSON numbering).
 */
export function getEffectiveness(atkType: number, defTypes: number[]): number {
  let mult = 1;
  for (const dt of defTypes) {
    if (chart[atkType] && chart[atkType][dt] !== undefined) {
      mult *= chart[atkType][dt];
    }
  }
  return mult;
}

/**
 * Calculate damage (deterministic, no random, no crit).
 */
export function calcDamage(attacker: SimDino, defender: SimDino, move: SimMove): number {
  if (move.power === 0) return 0;

  const isPhysical = move.category === 'physical';
  const atk = isPhysical ? attacker.atk : attacker.spatk;
  const def = isPhysical ? defender.def : defender.spdef;

  let dmg = ((2 * attacker.level / 5 + 2) * move.power * (atk / def)) / 50 + 2;

  // STAB
  if (attacker.types.includes(move.type)) {
    dmg *= 1.5;
  }

  // Type effectiveness
  dmg *= getEffectiveness(move.type, defender.types);

  // No random, no crit for deterministic sim
  return Math.max(1, Math.floor(dmg));
}

/**
 * Pick the move that deals the most damage to the defender.
 */
export function pickBestMove(attacker: SimDino, defender: SimDino): SimMove {
  let best = attacker.moves[0];
  let bestDmg = 0;
  for (const m of attacker.moves) {
    const dmg = calcDamage(attacker, defender, m);
    if (dmg > bestDmg) {
      bestDmg = dmg;
      best = m;
    }
  }
  return best;
}

/**
 * Simulate a battle (both dinos use best move each turn, deterministic).
 */
export function simulateBattle(d1: SimDino, d2: SimDino, maxTurns = 50): BattleResult {
  let hp1 = d1.hp;
  let hp2 = d2.hp;
  let turns = 0;

  while (hp1 > 0 && hp2 > 0 && turns < maxTurns) {
    turns++;

    // Each dino picks their best move against the other
    const bestMove1 = pickBestMove(d1, d2);
    const bestMove2 = pickBestMove(d2, d1);

    // Speed determines who goes first
    if (d1.speed >= d2.speed) {
      hp2 -= calcDamage(d1, d2, bestMove1);
      if (hp2 <= 0) break;
      hp1 -= calcDamage(d2, d1, bestMove2);
    } else {
      hp1 -= calcDamage(d2, d1, bestMove2);
      if (hp1 <= 0) break;
      hp2 -= calcDamage(d1, d2, bestMove1);
    }
  }

  return {
    winner: hp1 <= 0 ? 'dino2' : hp2 <= 0 ? 'dino1' : 'draw',
    turns,
    dino1HpLeft: Math.max(0, hp1),
    dino2HpLeft: Math.max(0, hp2),
  };
}

/**
 * Get total number of dino species.
 */
export function getDinoCount(): number {
  return dinos.length;
}

/**
 * Build a "neutral" target dino with average stats at a given level.
 * Uses base stats of 60 across the board, Normal type (12 in JSON).
 */
export function buildNeutralTarget(level: number): SimDino {
  const base = 60;
  return {
    id: 0,
    name: 'NeutralTarget',
    types: [12], // Sable/Normal in JSON numbering
    level,
    hp: calcStat(base, level, true),
    atk: calcStat(base, level, false),
    def: calcStat(base, level, false),
    spatk: calcStat(base, level, false),
    spdef: calcStat(base, level, false),
    speed: calcStat(base, level, false),
    moves: [],
  };
}

/**
 * Get all super-effective matchups from the type chart.
 * Returns array of { atkType, defType, mult } where mult >= 2.
 */
export function getSuperEffectiveMatchups(): Array<{ atkType: number; defType: number; mult: number }> {
  const results: Array<{ atkType: number; defType: number; mult: number }> = [];
  for (let atk = 0; atk < chart.length; atk++) {
    for (let def = 0; def < chart[atk].length; def++) {
      if (chart[atk][def] >= 2) {
        results.push({ atkType: atk, defType: def, mult: chart[atk][def] });
      }
    }
  }
  return results;
}

/**
 * Get dino species data by ID.
 */
export function getSpecies(id: number): any {
  return dinos.find((d: any) => d.id === id);
}

/**
 * Get move data by ID.
 */
export function getMove(id: number): any {
  return moves.find((m: any) => m.id === id);
}

/**
 * Get the type chart types array.
 */
export function getTypeNames(): string[] {
  return typeChartData.types;
}

/**
 * Find a dino of a specific type (first one found).
 */
export function findDinoByType(typeId: number): any | undefined {
  return dinos.find((d: any) => d.types.includes(typeId));
}

/**
 * Get all dino species data.
 */
export function getAllDinos(): any[] {
  return dinos;
}

/**
 * Gym leader data for balance testing.
 * ace = the last/strongest pokemon in the gym leader's party.
 */
export const GYM_LEADERS = [
  { name: 'FLORA',   badge: 0, aceId: 20, aceLevel: 14, expectedPlayerLevel: 13 },
  { name: 'MARIN',   badge: 1, aceId: 13, aceLevel: 18, expectedPlayerLevel: 17 },
  { name: 'PETRA',   badge: 2, aceId: 17, aceLevel: 22, expectedPlayerLevel: 21 },
  { name: 'VULKAN',  badge: 3, aceId: 14, aceLevel: 26, expectedPlayerLevel: 25 },
  { name: 'AURORA',  badge: 4, aceId: 21, aceLevel: 30, expectedPlayerLevel: 29 },
  { name: 'TESLA',   badge: 5, aceId: 24, aceLevel: 34, expectedPlayerLevel: 33 },
  { name: 'VENOM',   badge: 6, aceId: 25, aceLevel: 38, expectedPlayerLevel: 37 },
  { name: 'AETHER',  badge: 7, aceId: 27, aceLevel: 42, expectedPlayerLevel: 41 },
];
