import { describe, it, expect } from 'vitest';
import { calcXpRequired, calcXpYield } from '../utils/damageCalc';

describe('XP and Progression', () => {
  it('medium group: XP required follows cubic curve (n^3)', () => {
    expect(calcXpRequired(10, 'medium')).toBe(1000);
    expect(calcXpRequired(50, 'medium')).toBe(125000);
    expect(calcXpRequired(100, 'medium')).toBe(1000000);
  });

  it('fast group requires less XP than medium at same level', () => {
    for (const level of [10, 25, 50, 75, 100]) {
      expect(calcXpRequired(level, 'fast')).toBeLessThan(calcXpRequired(level, 'medium'));
    }
  });

  it('slow group requires more XP than medium at same level', () => {
    for (const level of [10, 25, 50, 75, 100]) {
      expect(calcXpRequired(level, 'slow')).toBeGreaterThan(calcXpRequired(level, 'medium'));
    }
  });

  it('medium group: level 10 requires less XP than level 50', () => {
    expect(calcXpRequired(10, 'medium')).toBeLessThan(calcXpRequired(50, 'medium'));
  });

  it('XP required increases monotonically with level', () => {
    for (const group of ['fast', 'medium', 'slow'] as const) {
      let prev = 0;
      for (let level = 2; level <= 100; level++) {
        const xp = calcXpRequired(level, group);
        expect(xp, `${group} group: level ${level} XP not greater than level ${level - 1}`).toBeGreaterThan(prev);
        prev = xp;
      }
    }
  });

  it('defeating a Lv5 dino gives reasonable XP for a Lv5 player dino', () => {
    // Pyrex xpYield = 62, Lv5
    // XP = floor(62 * 5 / (5 * 1)) = floor(62) = 62
    const xp = calcXpYield(62, 5, 1);
    expect(xp).toBe(62);
    expect(xp).toBeGreaterThan(0);
    expect(xp).toBeLessThan(500);
  });

  it('higher level defeated dino gives more XP', () => {
    const xpLow = calcXpYield(62, 5, 1);
    const xpHigh = calcXpYield(62, 50, 1);
    expect(xpHigh).toBeGreaterThan(xpLow);
  });

  it('splitting XP among participants reduces each share', () => {
    const solo = calcXpYield(100, 30, 1);
    const duo = calcXpYield(100, 30, 2);
    expect(duo).toBeLessThan(solo);
  });

  it('level 1 requires minimal XP', () => {
    expect(calcXpRequired(1, 'medium')).toBe(1);
    expect(calcXpRequired(1, 'fast')).toBe(0); // floor(4/5) = 0
  });

  it('fast group formula: 4n^3/5', () => {
    expect(calcXpRequired(5, 'fast')).toBe(Math.floor((4 * 125) / 5));
    expect(calcXpRequired(20, 'fast')).toBe(Math.floor((4 * 8000) / 5));
  });

  it('slow group formula: 5n^3/4', () => {
    expect(calcXpRequired(4, 'slow')).toBe(Math.floor((5 * 64) / 4));
    expect(calcXpRequired(20, 'slow')).toBe(Math.floor((5 * 8000) / 4));
  });
});
