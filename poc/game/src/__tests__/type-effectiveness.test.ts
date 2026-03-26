import { describe, it, expect } from 'vitest';
import * as fs from 'fs';
import * as path from 'path';

const dataDir = path.resolve(__dirname, '..', 'data');
const typeChart: any = JSON.parse(fs.readFileSync(path.join(dataDir, 'type_chart.json'), 'utf-8'));
const chart: number[][] = typeChart.chart;

// Type indices: 0=Roche, 1=Eau, 2=Feu, 3=Plante, 4=Glace, 5=Vol, 6=Terre,
// 7=Foudre, 8=Poison, 9=Acier, 10=Ombre, 11=Lumiere, 12=Sable, 13=Fossile

function getEffectiveness(attackType: number, defenseType: number): number {
  return chart[attackType][defenseType];
}

describe('Type Effectiveness — Specific Matchups', () => {
  it('Fire (2) vs Flora (3) = 2.0 (super effective)', () => {
    expect(getEffectiveness(2, 3)).toBe(2);
  });

  it('Fire (2) vs Water (1) = 0.5 (not very effective)', () => {
    expect(getEffectiveness(2, 1)).toBe(0.5);
  });

  it('Water (1) vs Fire (2) = 2.0 (super effective)', () => {
    expect(getEffectiveness(1, 2)).toBe(2);
  });

  it('Terre (6) vs Vol (5) = 0 (immune)', () => {
    expect(getEffectiveness(6, 5)).toBe(0);
  });

  it('Foudre (7) vs Terre (6) = 0 (immune)', () => {
    expect(getEffectiveness(7, 6)).toBe(0);
  });

  it('Poison (8) vs Acier (9) = 0 (immune)', () => {
    expect(getEffectiveness(8, 9)).toBe(0);
  });

  it('Fossil (13) vs Ice (4) = 2.0 (fixed matchup)', () => {
    expect(getEffectiveness(13, 4)).toBe(2);
  });

  it('Light (11) vs Venom (8) = 2.0 (fixed matchup)', () => {
    expect(getEffectiveness(11, 8)).toBe(2);
  });

  it('Ombre (10) vs Ombre (10) = 2.0', () => {
    expect(getEffectiveness(10, 10)).toBe(2);
  });

  it('Lumiere (11) vs Ombre (10) = 2.0', () => {
    expect(getEffectiveness(11, 10)).toBe(2);
  });
});

describe('Type Effectiveness — Structural Checks', () => {
  it('each type has at least 1 super-effective matchup (attacking)', () => {
    for (let atk = 0; atk < 14; atk++) {
      const hasSuperEffective = chart[atk].some((val: number) => val === 2);
      expect(hasSuperEffective, `Type ${atk} (${typeChart.types[atk]}) has no super-effective matchup`).toBe(true);
    }
  });

  it('most types have at least 1 not-very-effective or immune matchup (attacking)', () => {
    // Ombre (10) is deliberately a "glass cannon" type with no resistances in attack — design choice
    const exemptTypes = new Set([10]); // Ombre
    for (let atk = 0; atk < 14; atk++) {
      if (exemptTypes.has(atk)) continue;
      const hasResisted = chart[atk].some((val: number) => val < 1);
      expect(hasResisted, `Type ${atk} (${typeChart.types[atk]}) has no resisted matchup`).toBe(true);
    }
  });

  it('chart has no values outside {0, 0.5, 1, 2}', () => {
    const validValues = new Set([0, 0.5, 1, 2]);
    for (let i = 0; i < 14; i++) {
      for (let j = 0; j < 14; j++) {
        expect(validValues.has(chart[i][j]), `Invalid value ${chart[i][j]} at [${i}][${j}]`).toBe(true);
      }
    }
  });

  it('diagonal (self-matchup) is never super effective except Ombre', () => {
    for (let i = 0; i < 14; i++) {
      if (i === 10) {
        // Ombre vs Ombre = 2 is expected
        expect(chart[i][i]).toBe(2);
      } else {
        expect(chart[i][i], `Type ${i} (${typeChart.types[i]}) is SE against itself`).toBeLessThanOrEqual(1);
      }
    }
  });
});
