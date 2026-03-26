import { describe, it, expect } from 'vitest';
import * as fs from 'fs';
import * as path from 'path';

const dataDir = path.resolve(__dirname, '..', 'data');
const dinos: any[] = JSON.parse(fs.readFileSync(path.join(dataDir, 'dinos.json'), 'utf-8'));
const moves: any[] = JSON.parse(fs.readFileSync(path.join(dataDir, 'moves.json'), 'utf-8'));
const typeChart: any = JSON.parse(fs.readFileSync(path.join(dataDir, 'type_chart.json'), 'utf-8'));

const moveIdSet = new Set(moves.map((m: any) => m.id));
const dinoIdSet = new Set(dinos.map((d: any) => d.id));

describe('Data Integrity — Dinos', () => {
  it('should have exactly 150 dinos', () => {
    expect(dinos.length).toBe(150);
  });

  it('all dinos have required fields', () => {
    for (const dino of dinos) {
      expect(dino).toHaveProperty('id');
      expect(dino).toHaveProperty('name');
      expect(dino).toHaveProperty('types');
      expect(dino).toHaveProperty('baseStats');
      expect(dino).toHaveProperty('learnset');
      expect(typeof dino.id).toBe('number');
      expect(typeof dino.name).toBe('string');
      expect(Array.isArray(dino.types)).toBe(true);
      expect(dino.types.length).toBeGreaterThanOrEqual(1);
      expect(dino.baseStats).toHaveProperty('hp');
      expect(dino.baseStats).toHaveProperty('atk');
      expect(dino.baseStats).toHaveProperty('def');
      expect(dino.baseStats).toHaveProperty('spatk');
      expect(dino.baseStats).toHaveProperty('spdef');
      expect(dino.baseStats).toHaveProperty('speed');
    }
  });

  it('no duplicate dino IDs', () => {
    const ids = dinos.map((d: any) => d.id);
    expect(new Set(ids).size).toBe(ids.length);
  });

  it('all evolution targets reference existing dinos', () => {
    for (const dino of dinos) {
      if (dino.evolution && dino.evolution.to) {
        expect(dinoIdSet.has(dino.evolution.to)).toBe(true);
      }
    }
  });

  it('all dinos start with moveId 121 (Charge) and 122 (Grondement) at level 1', () => {
    for (const dino of dinos) {
      const level1Moves = dino.learnset.filter((m: any) => m.level === 1);
      const moveIds = level1Moves.map((m: any) => m.moveId);
      expect(moveIds).toContain(121);
      expect(moveIds).toContain(122);
    }
  });

  it('no typed moves before level 7 (only 121/122 allowed before 7)', () => {
    for (const dino of dinos) {
      const earlyMoves = dino.learnset.filter((m: any) => m.level < 7);
      for (const move of earlyMoves) {
        expect([121, 122]).toContain(move.moveId);
      }
    }
  });
});

describe('Data Integrity — Moves', () => {
  it('all moves have required fields', () => {
    for (const move of moves) {
      expect(move).toHaveProperty('id');
      expect(move).toHaveProperty('name');
      expect(move).toHaveProperty('type');
      expect(move).toHaveProperty('category');
      expect(move).toHaveProperty('power');
      expect(move).toHaveProperty('accuracy');
      expect(move).toHaveProperty('pp');
      expect(typeof move.id).toBe('number');
      expect(typeof move.name).toBe('string');
      expect(['physical', 'special', 'status']).toContain(move.category);
    }
  });

  it('no duplicate move IDs', () => {
    const ids = moves.map((m: any) => m.id);
    expect(new Set(ids).size).toBe(ids.length);
  });

  it('all moveIds in learnsets reference existing moves', () => {
    for (const dino of dinos) {
      for (const entry of dino.learnset) {
        expect(moveIdSet.has(entry.moveId)).toBe(true);
      }
    }
  });
});

describe('Data Integrity — Type Chart', () => {
  it('type chart is 14x14', () => {
    expect(typeChart.chart.length).toBe(14);
    for (const row of typeChart.chart) {
      expect(row.length).toBe(14);
    }
  });

  it('all type values are valid multipliers', () => {
    const validValues = [0, 0.5, 1, 2];
    for (const row of typeChart.chart) {
      for (const val of row) {
        expect(validValues).toContain(val);
      }
    }
  });

  it('all dino type indices are within 0-13 range', () => {
    for (const dino of dinos) {
      for (const typeIdx of dino.types) {
        expect(typeIdx).toBeGreaterThanOrEqual(0);
        expect(typeIdx).toBeLessThanOrEqual(13);
      }
    }
  });

  it('has exactly 14 type names', () => {
    expect(typeChart.types.length).toBe(14);
  });
});
