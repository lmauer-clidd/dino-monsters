/**
 * Automated Balance Testing Suite
 * Simulates thousands of battles to verify game balance across all dinos.
 *
 * Test failures here ARE the balance report — they reveal issues
 * in the game data, not bugs in the test code.
 */

import { describe, it, expect } from 'vitest';
import {
  buildDino,
  simulateBattle,
  calcDamage,
  pickBestMove,
  calcStat,
  getEffectiveness,
  buildNeutralTarget,
  getDinoCount,
  getSuperEffectiveMatchups,
  findDinoByType,
  getTypeNames,
  getAllDinos,
  GYM_LEADERS,
  SimDino,
} from '../utils/balanceSim';

// ============================================================
// 2a. Starter triangle balance
// ============================================================
describe('Starter Triangle Balance', () => {
  // Pyrex (id 1, Fire/type 2) vs Aquadon (id 4, Water/type 1) vs Florasaur (id 7, Flora/type 3)
  // Fire > Flora > Water > Fire
  const LEVELS = [5, 15, 30, 50];

  for (const level of LEVELS) {
    describe(`at level ${level}`, () => {
      it(`Pyrex (Fire) should beat Florasaur (Flora) — type advantage`, () => {
        const pyrex = buildDino(1, level);
        const florasaur = buildDino(7, level);
        const result = simulateBattle(pyrex, florasaur);
        expect(result.winner,
          `Pyrex Lv${level} vs Florasaur Lv${level}: expected Pyrex to win (Fire > Flora), got ${result.winner} in ${result.turns} turns`
        ).toBe('dino1');
      });

      it(`Florasaur (Flora) should beat Aquadon (Water) — type advantage`, () => {
        const florasaur = buildDino(7, level);
        const aquadon = buildDino(4, level);
        const result = simulateBattle(florasaur, aquadon);
        expect(result.winner,
          `Florasaur Lv${level} vs Aquadon Lv${level}: expected Florasaur to win (Flora > Water), got ${result.winner} in ${result.turns} turns`
        ).toBe('dino1');
      });

      it(`Aquadon (Water) should beat Pyrex (Fire) — type advantage`, () => {
        const aquadon = buildDino(4, level);
        const pyrex = buildDino(1, level);
        const result = simulateBattle(aquadon, pyrex);
        expect(result.winner,
          `Aquadon Lv${level} vs Pyrex Lv${level}: expected Aquadon to win (Water > Fire), got ${result.winner} in ${result.turns} turns`
        ).toBe('dino1');
      });

      it(`no starter should one-shot another at same level`, () => {
        const pyrex = buildDino(1, level);
        const aquadon = buildDino(4, level);
        const florasaur = buildDino(7, level);

        const pairs: [SimDino, SimDino, string][] = [
          [pyrex, aquadon, 'Pyrex vs Aquadon'],
          [pyrex, florasaur, 'Pyrex vs Florasaur'],
          [aquadon, pyrex, 'Aquadon vs Pyrex'],
          [aquadon, florasaur, 'Aquadon vs Florasaur'],
          [florasaur, pyrex, 'Florasaur vs Pyrex'],
          [florasaur, aquadon, 'Florasaur vs Aquadon'],
        ];

        for (const [atk, def, label] of pairs) {
          const bestMove = pickBestMove(atk, def);
          const dmg = calcDamage(atk, def, bestMove);
          expect(dmg,
            `${label} Lv${level}: ${atk.name} ${bestMove.name} does ${dmg} damage vs ${def.name} HP=${def.hp} — one-shot!`
          ).toBeLessThan(def.hp);
        }
      });

      it(`battles should last 3-6 turns`, () => {
        const pyrex = buildDino(1, level);
        const aquadon = buildDino(4, level);
        const florasaur = buildDino(7, level);

        const matchups: [SimDino, SimDino, string][] = [
          [pyrex, aquadon, 'Pyrex vs Aquadon'],
          [pyrex, florasaur, 'Pyrex vs Florasaur'],
          [aquadon, florasaur, 'Aquadon vs Florasaur'],
        ];

        for (const [d1, d2, label] of matchups) {
          const result = simulateBattle(d1, d2);
          expect(result.turns,
            `${label} Lv${level}: battle lasted ${result.turns} turns — expected 3-6`
          ).toBeGreaterThanOrEqual(3);
          expect(result.turns,
            `${label} Lv${level}: battle lasted ${result.turns} turns — too long, expected 3-6`
          ).toBeLessThanOrEqual(6);
        }
      });
    });
  }
});

// ============================================================
// 2b. No move one-shots at same level
// ============================================================
describe('No One-Shot Moves (same level, neutral target)', () => {
  const LEVELS = [10, 20, 30, 40, 50];
  const dinoCount = getDinoCount();

  for (const level of LEVELS) {
    it(`at level ${level}, no dino's best hit exceeds 60% of neutral target HP`, () => {
      const target = buildNeutralTarget(level);
      const violations: string[] = [];

      for (let id = 1; id <= Math.min(dinoCount, 150); id++) {
        try {
          const dino = buildDino(id, level);
          if (dino.moves.length === 0) continue;

          const bestMove = pickBestMove(dino, target);
          const dmg = calcDamage(dino, target, bestMove);
          const pct = dmg / target.hp;

          // Exception: super-effective STAB at high level can do more
          const isSE = getEffectiveness(bestMove.type, target.types) >= 2;
          const isSTAB = dino.types.includes(bestMove.type);
          const isHighLevel = level >= 40;
          if (isSE && isSTAB && isHighLevel) continue;

          if (pct > 0.60) {
            violations.push(
              `${dino.name} (id ${id}) Lv${level}: ${bestMove.name} (pow ${bestMove.power}) does ${dmg} dmg = ${(pct * 100).toFixed(1)}% of target HP ${target.hp}`
            );
          }
        } catch {
          // Skip dinos that fail to build (missing data)
        }
      }

      expect(violations,
        `${violations.length} dinos exceed 60% damage at Lv${level}:\n${violations.join('\n')}`
      ).toHaveLength(0);
    });
  }
});

// ============================================================
// 2c. Damage output consistency across levels
// ============================================================
describe('Damage Output Consistency Across Levels', () => {
  // Sample 10 dinos: starters + some evos + some randoms
  const sampleIds = [1, 4, 7, 2, 5, 8, 10, 20, 50, 100];

  it('at level 5: moves should typically do 3-15 damage', () => {
    const violations: string[] = [];
    for (const id of sampleIds) {
      try {
        const dino = buildDino(id, 5);
        const target = buildNeutralTarget(5);
        if (dino.moves.length === 0) continue;
        for (const move of dino.moves) {
          const dmg = calcDamage(dino, target, move);
          if (dmg < 2 || dmg > 20) {
            violations.push(
              `${dino.name} Lv5 ${move.name} (pow ${move.power}) does ${dmg} dmg — expected 3-15 range`
            );
          }
        }
      } catch { /* skip missing */ }
    }
    expect(violations,
      `Lv5 damage out of range:\n${violations.join('\n')}`
    ).toHaveLength(0);
  });

  it('at level 25: moves should typically do 10-60 damage', () => {
    const violations: string[] = [];
    for (const id of sampleIds) {
      try {
        const dino = buildDino(id, 25);
        const target = buildNeutralTarget(25);
        if (dino.moves.length === 0) continue;
        for (const move of dino.moves) {
          const dmg = calcDamage(dino, target, move);
          if (dmg < 5 || dmg > 80) {
            violations.push(
              `${dino.name} Lv25 ${move.name} (pow ${move.power}) does ${dmg} dmg — expected 10-60 range`
            );
          }
        }
      } catch { /* skip missing */ }
    }
    expect(violations,
      `Lv25 damage out of range:\n${violations.join('\n')}`
    ).toHaveLength(0);
  });

  it('at level 50: moves should typically do 20-150 damage', () => {
    const violations: string[] = [];
    for (const id of sampleIds) {
      try {
        const dino = buildDino(id, 50);
        const target = buildNeutralTarget(50);
        if (dino.moves.length === 0) continue;
        for (const move of dino.moves) {
          const dmg = calcDamage(dino, target, move);
          if (dmg < 10 || dmg > 200) {
            violations.push(
              `${dino.name} Lv50 ${move.name} (pow ${move.power}) does ${dmg} dmg — expected 20-150 range`
            );
          }
        }
      } catch { /* skip missing */ }
    }
    expect(violations,
      `Lv50 damage out of range:\n${violations.join('\n')}`
    ).toHaveLength(0);
  });
});

// ============================================================
// 2d. Physical vs Special balance
// ============================================================
describe('Physical vs Special Balance', () => {
  it('Pyrex (Atk 55 > SpAtk 50) should do more with physical than special moves', () => {
    const pyrex = buildDino(1, 30);
    const target = buildNeutralTarget(30);

    // Find physical and special moves of same type if possible
    const physicalMoves = pyrex.moves.filter(m => m.category === 'physical');
    const specialMoves = pyrex.moves.filter(m => m.category === 'special');

    if (physicalMoves.length > 0 && specialMoves.length > 0) {
      // Compare strongest physical vs strongest special of similar power
      const bestPhys = physicalMoves.reduce((a, b) =>
        calcDamage(pyrex, target, a) > calcDamage(pyrex, target, b) ? a : b
      );
      const bestSpec = specialMoves.reduce((a, b) =>
        calcDamage(pyrex, target, a) > calcDamage(pyrex, target, b) ? a : b
      );

      // If moves have similar base power, physical should edge out
      if (Math.abs(bestPhys.power - bestSpec.power) <= 10) {
        const physDmg = calcDamage(pyrex, target, bestPhys);
        const specDmg = calcDamage(pyrex, target, bestSpec);
        expect(physDmg,
          `Pyrex physical ${bestPhys.name} (pow ${bestPhys.power}) does ${physDmg} vs special ${bestSpec.name} (pow ${bestSpec.power}) does ${specDmg} — physical attacker should do more with physical moves`
        ).toBeGreaterThanOrEqual(specDmg);
      }
    }
  });

  it('Aquadon (SpAtk 55 > Atk 40) should do more with special than physical moves', () => {
    const aquadon = buildDino(4, 30);
    const target = buildNeutralTarget(30);

    const physicalMoves = aquadon.moves.filter(m => m.category === 'physical');
    const specialMoves = aquadon.moves.filter(m => m.category === 'special');

    if (physicalMoves.length > 0 && specialMoves.length > 0) {
      const bestPhys = physicalMoves.reduce((a, b) =>
        calcDamage(aquadon, target, a) > calcDamage(aquadon, target, b) ? a : b
      );
      const bestSpec = specialMoves.reduce((a, b) =>
        calcDamage(aquadon, target, a) > calcDamage(aquadon, target, b) ? a : b
      );

      // If moves have similar base power, special should edge out
      if (Math.abs(bestPhys.power - bestSpec.power) <= 10) {
        const physDmg = calcDamage(aquadon, target, bestPhys);
        const specDmg = calcDamage(aquadon, target, bestSpec);
        expect(specDmg,
          `Aquadon special ${bestSpec.name} (pow ${bestSpec.power}) does ${specDmg} vs physical ${bestPhys.name} (pow ${bestPhys.power}) does ${physDmg} — special attacker should do more with special moves`
        ).toBeGreaterThanOrEqual(physDmg);
      }
    }
  });
});

// ============================================================
// 2e. Type advantage matters
// ============================================================
describe('Type Advantage Matters', () => {
  const typeNames = getTypeNames();
  const allDinos = getAllDinos();

  // For each type that has at least one SE matchup, test with real dinos
  const seMatchups = getSuperEffectiveMatchups();

  // Group by atkType to avoid thousands of individual tests
  const byAtkType = new Map<number, number[]>();
  for (const m of seMatchups) {
    if (!byAtkType.has(m.atkType)) byAtkType.set(m.atkType, []);
    byAtkType.get(m.atkType)!.push(m.defType);
  }

  for (const [atkType, defTypes] of byAtkType) {
    it(`${typeNames[atkType]} attackers should beat type-disadvantaged defenders`, () => {
      const attacker = allDinos.find((d: any) =>
        d.types.includes(atkType) && d.learnset.length > 0
      );
      if (!attacker) return; // no dino of this type

      const violations: string[] = [];

      for (const defType of defTypes) {
        const defender = allDinos.find((d: any) =>
          d.types.length === 1 && d.types[0] === defType && d.learnset.length > 0
        );
        if (!defender) continue;

        const level = 30;
        const atkDino = buildDino(attacker.id, level);
        const defDino = buildDino(defender.id, level);

        const result = simulateBattle(atkDino, defDino);

        if (result.winner !== 'dino1') {
          violations.push(
            `${atkDino.name} (${typeNames[atkType]}) Lv${level} LOST to ${defDino.name} (${typeNames[defType]}) — SE matchup should favor attacker. Battle: ${result.turns} turns, winner=${result.winner}`
          );
        }
      }

      // Allow some exceptions (dual types, stat differences, etc.)
      expect(violations.length,
        `${typeNames[atkType]} type advantage failures:\n${violations.join('\n')}`
      ).toBeLessThanOrEqual(Math.ceil(defTypes.length * 0.3)); // max 30% failure rate
    });
  }

  it('super-effective hits should NOT one-turn KO at same level (Lv 25)', () => {
    const violations: string[] = [];

    for (const { atkType, defType } of seMatchups.slice(0, 20)) { // test first 20
      const attacker = allDinos.find((d: any) =>
        d.types.includes(atkType) && d.learnset.length > 0
      );
      const defender = allDinos.find((d: any) =>
        d.types.length === 1 && d.types[0] === defType && d.learnset.length > 0
      );
      if (!attacker || !defender) continue;

      const atkDino = buildDino(attacker.id, 25);
      const defDino = buildDino(defender.id, 25);

      const result = simulateBattle(atkDino, defDino);
      if (result.turns <= 1) {
        violations.push(
          `${atkDino.name} (${typeNames[atkType]}) 1-turn KOs ${defDino.name} (${typeNames[defType]}) at Lv25`
        );
      }
    }

    expect(violations,
      `SE matchups with 1-turn KOs:\n${violations.join('\n')}`
    ).toHaveLength(0);
  });
});

// ============================================================
// 2f. Gym leader difficulty progression
// ============================================================
describe('Gym Leader Difficulty Progression', () => {
  // Player uses Pyrex (id 1) at expected player level vs each gym leader's ace
  for (const gym of GYM_LEADERS) {
    it(`Gym ${gym.badge + 1} (${gym.name}): player Pyrex Lv${gym.expectedPlayerLevel} should beat ace (id ${gym.aceId} Lv${gym.aceLevel}) in 4+ turns`, () => {
      const player = buildDino(1, gym.expectedPlayerLevel);
      const ace = buildDino(gym.aceId, gym.aceLevel);
      const result = simulateBattle(player, ace);

      // Player should be able to win (or at least survive a while)
      // Note: Pyrex has type disadvantage vs Water gym, so some losses expected
      if (result.winner === 'dino1') {
        expect(result.turns,
          `Gym ${gym.badge + 1} ${gym.name}: Pyrex Lv${gym.expectedPlayerLevel} beat ace in only ${result.turns} turns — too easy! Expected 4+`
        ).toBeGreaterThanOrEqual(4);
      }
      // If Pyrex loses, that's ok for type-disadvantaged gyms, but battle should last 3+ turns
      expect(result.turns,
        `Gym ${gym.badge + 1} ${gym.name}: battle lasted only ${result.turns} turn(s) — ace is too overpowered`
      ).toBeGreaterThanOrEqual(3);
    });
  }

  it('gym leaders should get progressively harder (ace HP increases)', () => {
    const aceHps: number[] = [];
    for (const gym of GYM_LEADERS) {
      const ace = buildDino(gym.aceId, gym.aceLevel);
      aceHps.push(ace.hp);
    }
    for (let i = 1; i < aceHps.length; i++) {
      expect(aceHps[i],
        `Gym ${i + 1} ace HP (${aceHps[i]}) should be >= Gym ${i} ace HP (${aceHps[i - 1]}) — difficulty should progress`
      ).toBeGreaterThanOrEqual(aceHps[i - 1]);
    }
  });
});

// ============================================================
// 2g. Level curve — battle duration
// ============================================================
describe('Battle Duration (same-level matchups)', () => {
  // Use a seeded pseudo-random selection of matchups for reproducibility
  function getMatchupPairs(level: number, count: number): [number, number][] {
    const pairs: [number, number][] = [];
    const maxId = Math.min(getDinoCount(), 150);
    // Deterministic pairs: pick evenly spaced IDs
    for (let i = 0; i < count; i++) {
      const id1 = (i * 3 % maxId) + 1;
      const id2 = ((i * 7 + 13) % maxId) + 1;
      if (id1 !== id2) pairs.push([id1, id2]);
    }
    return pairs;
  }

  for (const level of [10, 25, 50]) {
    it(`at level ${level}, random same-level matchups should average 3-8 turns (100 battles)`, () => {
      const pairs = getMatchupPairs(level, 100);
      const turnCounts: number[] = [];
      let completed = 0;

      for (const [id1, id2] of pairs) {
        try {
          const d1 = buildDino(id1, level);
          const d2 = buildDino(id2, level);
          if (d1.moves.length === 0 || d2.moves.length === 0) continue;
          const result = simulateBattle(d1, d2);
          turnCounts.push(result.turns);
          completed++;
        } catch { /* skip invalid */ }
      }

      if (completed < 10) return; // not enough data

      const avg = turnCounts.reduce((a, b) => a + b, 0) / turnCounts.length;
      const draws = turnCounts.filter(t => t >= 50).length;

      expect(avg,
        `Lv${level} avg battle duration: ${avg.toFixed(1)} turns (${completed} battles, ${draws} draws) — expected 3-8`
      ).toBeGreaterThanOrEqual(3);
      expect(avg,
        `Lv${level} avg battle duration: ${avg.toFixed(1)} turns — battles dragging too long, expected 3-8`
      ).toBeLessThanOrEqual(8);

      // No more than 10% draws
      expect(draws,
        `Lv${level}: ${draws}/${completed} battles (${(draws / completed * 100).toFixed(0)}%) ended in draws — too many stalls`
      ).toBeLessThan(completed * 0.10);
    });
  }
});

// ============================================================
// 2h. Move power distribution check
// ============================================================
describe('Move Power Distribution', () => {
  const LEVELS = [10, 25, 50];
  const sampleIds = [1, 4, 7, 10, 20, 30, 40, 50, 75, 100, 120, 140];

  for (const level of LEVELS) {
    it(`at level ${level}, strongest move should not be >2x stronger than second strongest`, () => {
      const violations: string[] = [];

      for (const id of sampleIds) {
        try {
          const dino = buildDino(id, level);
          if (dino.moves.length < 2) continue;

          const target = buildNeutralTarget(level);
          const damages = dino.moves
            .map(m => ({ move: m, dmg: calcDamage(dino, target, m) }))
            .sort((a, b) => b.dmg - a.dmg);

          const best = damages[0];
          const second = damages[1];
          if (second.dmg === 0) continue; // avoid div by zero

          const ratio = best.dmg / second.dmg;
          if (ratio > 2.0) {
            violations.push(
              `${dino.name} Lv${level}: ${best.move.name} does ${best.dmg} vs ${second.move.name} does ${second.dmg} — ratio ${ratio.toFixed(2)}x (max 2.0x)`
            );
          }
        } catch { /* skip missing */ }
      }

      expect(violations,
        `Move power ratio violations at Lv${level}:\n${violations.join('\n')}`
      ).toHaveLength(0);
    });
  }
});
