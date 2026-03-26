import { describe, it, expect } from 'vitest';
import * as fs from 'fs';
import * as path from 'path';

const dataDir = path.resolve(__dirname, '..', 'data');
const dinos: any[] = JSON.parse(fs.readFileSync(path.join(dataDir, 'dinos.json'), 'utf-8'));

describe('Learnset Rules', () => {
  it('ALL 150 dinos have Charge (121) at level 1', () => {
    for (const dino of dinos) {
      const hasCharge = dino.learnset.some((m: any) => m.level === 1 && m.moveId === 121);
      expect(hasCharge, `${dino.name} (id: ${dino.id}) is missing Charge at level 1`).toBe(true);
    }
  });

  it('ALL 150 dinos have Grondement (122) at level 1', () => {
    for (const dino of dinos) {
      const hasGrondement = dino.learnset.some((m: any) => m.level === 1 && m.moveId === 122);
      expect(hasGrondement, `${dino.name} (id: ${dino.id}) is missing Grondement at level 1`).toBe(true);
    }
  });

  it('NO dino has a non-121/122 move before level 7', () => {
    for (const dino of dinos) {
      const earlyMoves = dino.learnset.filter(
        (m: any) => m.level < 7 && m.moveId !== 121 && m.moveId !== 122,
      );
      expect(
        earlyMoves.length,
        `${dino.name} (id: ${dino.id}) has early moves: ${JSON.stringify(earlyMoves)}`,
      ).toBe(0);
    }
  });

  it('each dino has at least 4 moves in its learnset', () => {
    for (const dino of dinos) {
      expect(
        dino.learnset.length,
        `${dino.name} (id: ${dino.id}) only has ${dino.learnset.length} moves`,
      ).toBeGreaterThanOrEqual(4);
    }
  });

  it('moves are ordered by level (ascending)', () => {
    for (const dino of dinos) {
      for (let i = 1; i < dino.learnset.length; i++) {
        expect(
          dino.learnset[i].level,
          `${dino.name} (id: ${dino.id}) has out-of-order learnset at index ${i}`,
        ).toBeGreaterThanOrEqual(dino.learnset[i - 1].level);
      }
    }
  });

  it('no duplicate moveIds in any single learnset', () => {
    for (const dino of dinos) {
      const moveIds = dino.learnset.map((m: any) => m.moveId);
      const uniqueIds = new Set(moveIds);
      expect(
        uniqueIds.size,
        `${dino.name} (id: ${dino.id}) has duplicate moves in learnset`,
      ).toBe(moveIds.length);
    }
  });

  it('all learnset entries have valid structure', () => {
    for (const dino of dinos) {
      for (const entry of dino.learnset) {
        expect(entry).toHaveProperty('level');
        expect(entry).toHaveProperty('moveId');
        expect(typeof entry.level).toBe('number');
        expect(typeof entry.moveId).toBe('number');
        expect(entry.level).toBeGreaterThanOrEqual(1);
      }
    }
  });
});
