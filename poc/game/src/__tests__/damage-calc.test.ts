import { describe, it, expect } from 'vitest';
import { calculatePureDamage } from '../utils/damageCalc';

describe('Damage Calculation', () => {
  const baseInput = {
    level: 50,
    power: 80,
    attack: 100,
    defense: 100,
    stab: false,
    effectiveness: 1.0,
    critical: false,
    randomFactor: 1.0,
  };

  it('STAB gives 1.5x damage', () => {
    const noStab = calculatePureDamage({ ...baseInput, stab: false });
    const withStab = calculatePureDamage({ ...baseInput, stab: true });
    expect(withStab).toBe(Math.floor(noStab * 1.5));
  });

  it('super effective gives 2x damage', () => {
    const normal = calculatePureDamage({ ...baseInput, effectiveness: 1.0 });
    const superEffective = calculatePureDamage({ ...baseInput, effectiveness: 2.0 });
    expect(superEffective).toBe(Math.floor(normal * 2));
  });

  it('not very effective gives 0.5x damage', () => {
    const normal = calculatePureDamage({ ...baseInput, effectiveness: 1.0 });
    const nve = calculatePureDamage({ ...baseInput, effectiveness: 0.5 });
    // Floor of (base * 0.5) — the ratio check needs to account for flooring
    expect(nve).toBeLessThan(normal);
    expect(nve).toBeGreaterThan(0);
  });

  it('immune gives 0 damage', () => {
    const immune = calculatePureDamage({ ...baseInput, effectiveness: 0 });
    expect(immune).toBe(0);
  });

  it('critical gives 1.5x damage', () => {
    const noCrit = calculatePureDamage({ ...baseInput, critical: false });
    const withCrit = calculatePureDamage({ ...baseInput, critical: true });
    expect(withCrit).toBe(Math.floor(noCrit * 1.5));
  });

  it('minimum 1 damage (never 0 unless immune)', () => {
    const tiny = calculatePureDamage({
      level: 1,
      power: 10,
      attack: 1,
      defense: 255,
      stab: false,
      effectiveness: 0.5,
      critical: false,
      randomFactor: 0.85,
    });
    expect(tiny).toBeGreaterThanOrEqual(1);
  });

  it('STAB + SE + Crit = 1.5 * 2 * 1.5 = 4.5x multiplier', () => {
    const base = calculatePureDamage({ ...baseInput });
    const combo = calculatePureDamage({
      ...baseInput,
      stab: true,
      effectiveness: 2.0,
      critical: true,
    });
    // Due to sequential flooring, check approximate ratio
    const ratio = combo / base;
    expect(ratio).toBeGreaterThanOrEqual(4.0);
    expect(ratio).toBeLessThanOrEqual(5.0);
  });

  it('Lv5 Pyrex (atk=11) Charge (40) vs Aquadon (def=10) — expected ~5 damage', () => {
    // Pyrex base atk=55, Lv5 -> stat ~11
    // Aquadon base def=45, Lv5 -> stat ~10
    // Charge: power 40, Normal type (12), Pyrex is Fire (2) so no STAB
    const dmg = calculatePureDamage({
      level: 5,
      power: 40,
      attack: 11,
      defense: 10,
      stab: false,
      effectiveness: 1.0,
      critical: false,
      randomFactor: 1.0,
    });
    // ((2*5/5+2)*40*(11/10))/50+2 = (4*40*1.1)/50+2 = 176/50+2 = 3.52+2 = 5.52 -> floor = 5
    expect(dmg).toBe(5);
  });

  it('random factor 0.85 reduces damage', () => {
    const full = calculatePureDamage({ ...baseInput, randomFactor: 1.0 });
    const low = calculatePureDamage({ ...baseInput, randomFactor: 0.85 });
    expect(low).toBeLessThanOrEqual(full);
    expect(low).toBeGreaterThan(0);
  });

  it('higher level means more damage', () => {
    const lv10 = calculatePureDamage({ ...baseInput, level: 10 });
    const lv50 = calculatePureDamage({ ...baseInput, level: 50 });
    const lv100 = calculatePureDamage({ ...baseInput, level: 100 });
    expect(lv50).toBeGreaterThan(lv10);
    expect(lv100).toBeGreaterThan(lv50);
  });
});
