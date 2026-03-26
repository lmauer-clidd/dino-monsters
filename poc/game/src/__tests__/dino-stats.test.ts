import { describe, it, expect } from 'vitest';
import { calcStat } from '../utils/damageCalc';

describe('Stat Calculation', () => {
  it('Pyrex Lv5 (base atk=55, gv=20, tp=0): atk should be 11', () => {
    // raw = floor(((2*55 + 20 + 0) * 5) / 100) = floor((130 * 5) / 100) = floor(6.5) = 6
    // stat = 6 + 5 = 11
    expect(calcStat(55, 5, 20, 0, false)).toBe(11);
  });

  it('Pyrex Lv50 (base atk=55, gv=20, tp=0): atk should be ~70', () => {
    const stat = calcStat(55, 50, 20, 0, false);
    expect(stat).toBeGreaterThanOrEqual(65);
    expect(stat).toBeLessThanOrEqual(75);
  });

  it('HP formula is different from other stats (+ level + 10 instead of + 5)', () => {
    const hp = calcStat(50, 50, 15, 0, true);
    const atk = calcStat(50, 50, 15, 0, false);
    // HP = raw + level + 10 = raw + 60
    // ATK = raw + 5
    // Difference should be level + 5 = 55
    expect(hp - atk).toBe(55);
  });

  it('GV 0 vs GV 31: difference proportional to level', () => {
    const level = 100;
    const base = 80;
    const statGv0 = calcStat(base, level, 0, 0, false);
    const statGv31 = calcStat(base, level, 31, 0, false);
    const diff = statGv31 - statGv0;
    // diff = floor(31 * level / 100) = floor(31) = 31
    expect(diff).toBe(Math.floor((31 * level) / 100));
  });

  it('GV 0 vs GV 31 at level 50: difference ~15', () => {
    const stat0 = calcStat(80, 50, 0, 0, false);
    const stat31 = calcStat(80, 50, 31, 0, false);
    const diff = stat31 - stat0;
    expect(diff).toBe(Math.floor((31 * 50) / 100));
  });

  it('TP contribution: 252 TP adds floor(63 * level / 100)', () => {
    const level = 100;
    const base = 80;
    const statNoTp = calcStat(base, level, 15, 0, false);
    const statMaxTp = calcStat(base, level, 15, 252, false);
    const diff = statMaxTp - statNoTp;
    expect(diff).toBe(Math.floor((Math.floor(252 / 4) * level) / 100));
  });

  it('level 1 stats are low', () => {
    const hp = calcStat(45, 1, 15, 0, true);
    const atk = calcStat(55, 1, 15, 0, false);
    expect(hp).toBeLessThanOrEqual(15);
    expect(atk).toBeLessThanOrEqual(10);
  });

  it('level 100 stats are high', () => {
    const hp = calcStat(100, 100, 31, 252, true);
    const atk = calcStat(150, 100, 31, 252, false);
    expect(hp).toBeGreaterThanOrEqual(300);
    expect(atk).toBeGreaterThanOrEqual(350);
  });

  it('higher base stat means higher final stat', () => {
    const low = calcStat(30, 50, 15, 0, false);
    const high = calcStat(120, 50, 15, 0, false);
    expect(high).toBeGreaterThan(low);
  });
});
