// ============================================================
// Jurassic Trainers -- Type Effectiveness Chart
// ============================================================

import { DinoType } from '../utils/constants';

/**
 * Type chart data: typeChart[attackType][defenderType] = multiplier.
 * 0 = immune, 0.5 = not very effective, 1 = neutral, 2 = super effective.
 * Loaded at boot from type_chart.json, or populated with defaults.
 */

type TypeMatrix = number[][];

// Default built-in chart (14 types)
// Rows = attacking type, Cols = defending type
// Order: Normal Fire Water Earth Air Electric Ice Venom Flora Fossil Shadow Light Metal Primal
const DEFAULT_CHART: TypeMatrix = [
  //         Nor  Fir  Wat  Ear  Air  Ele  Ice  Ven  Flo  Fos  Sha  Lig  Met  Pri
  /* Nor */ [ 1,   1,   1,   1,   1,   1,   1,   1,   1,   0.5, 0,   1,   0.5, 1   ],
  /* Fir */ [ 1,   0.5, 0.5, 1,   1,   1,   2,   1,   2,   0.5, 1,   1,   2,   1   ],
  /* Wat */ [ 1,   2,   0.5, 2,   1,   1,   1,   1,   0.5, 2,   1,   1,   1,   1   ],
  /* Ear */ [ 1,   2,   1,   1,   0,   2,   1,   2,   0.5, 1,   1,   1,   2,   1   ],
  /* Air */ [ 1,   1,   1,   1,   1,   0.5, 1,   1,   2,   0.5, 1,   1,   0.5, 1   ],
  /* Ele */ [ 1,   1,   2,   0,   2,   0.5, 1,   1,   0.5, 1,   1,   1,   1,   1   ],
  /* Ice */ [ 1,   0.5, 0.5, 2,   2,   1,   0.5, 1,   2,   1,   1,   1,   0.5, 2   ],
  /* Ven */ [ 1,   1,   1,   0.5, 1,   1,   1,   0.5, 2,   0.5, 1,   1,   0,   1   ],
  /* Flo */ [ 1,   0.5, 2,   2,   0.5, 1,   1,   0.5, 0.5, 2,   1,   1,   0.5, 1   ],
  /* Fos */ [ 1,   2,   0.5, 1,   2,   1,   2,   1,   1,   1,   1,   1,   0.5, 1   ],
  /* Sha */ [ 0,   1,   1,   1,   1,   1,   1,   1,   1,   1,   2,   0.5, 1,   0.5 ],
  /* Lig */ [ 1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   2,   1,   0.5, 2   ],
  /* Met */ [ 1,   0.5, 0.5, 1,   1,   0.5, 2,   1,   1,   2,   1,   2,   0.5, 1   ],
  /* Pri */ [ 1,   1,   1,   1,   1,   1,   1,   1,   1,   1,   2,   0.5, 1,   0.5 ],
];

export class TypeChart {
  private chart: TypeMatrix;

  constructor() {
    // Start with default; can be overwritten by loadFromJSON
    this.chart = DEFAULT_CHART.map(row => [...row]);
  }

  /**
   * Load chart from parsed JSON data.
   * Expected format: 2D array [attackingType][defendingType] = multiplier
   */
  loadFromJSON(data: TypeMatrix): void {
    this.chart = data.map(row => [...row]);
  }

  /**
   * Get the effectiveness multiplier for an attack type vs one or two defender types.
   * Returns the combined multiplier (e.g. 0, 0.25, 0.5, 1, 2, 4).
   */
  getEffectiveness(attackType: DinoType, defenderType1: DinoType, defenderType2?: DinoType): number {
    let mult = this.getSingleEffectiveness(attackType, defenderType1);
    if (defenderType2 !== undefined && defenderType2 !== defenderType1) {
      mult *= this.getSingleEffectiveness(attackType, defenderType2);
    }
    return mult;
  }

  /**
   * Single type-vs-type lookup.
   */
  private getSingleEffectiveness(attackType: DinoType, defenderType: DinoType): number {
    const row = this.chart[attackType];
    if (!row) return 1;
    const val = row[defenderType];
    return val !== undefined ? val : 1;
  }

  /**
   * Get a display label for the multiplier.
   */
  getEffectivenessLabel(multiplier: number): string {
    if (multiplier === 0)  return 'Aucun effet!';
    if (multiplier < 1)    return 'Pas très efficace...';
    if (multiplier > 1)    return 'Super efficace!';
    return ''; // neutral, no message
  }
}

// Singleton instance
export const typeChart = new TypeChart();
