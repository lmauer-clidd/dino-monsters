/**
 * Data Integrity Validation Script for Dino Monsters
 *
 * Run with:   npx ts-node game/src/data/validate-data.ts
 * Or:         npx tsx game/src/data/validate-data.ts
 *
 * Checks all JSON data files for consistency and correctness.
 * Uses plain Node.js (no Phaser dependency).
 */

import * as fs from 'fs';
import * as path from 'path';

// ── Load data files ──────────────────────────────────────────────────

const dataDir = path.resolve(__dirname);

function loadJSON(filename: string): any {
  const filepath = path.join(dataDir, filename);
  const raw = fs.readFileSync(filepath, 'utf-8');
  return JSON.parse(raw);
}

const dinos: any[] = loadJSON('dinos.json');
const moves: any[] = loadJSON('moves.json');
const typeChart: any = loadJSON('type_chart.json');

// ── Helpers ──────────────────────────────────────────────────────────

let totalPassed = 0;
let totalFailed = 0;
const errors: string[] = [];

function pass(label: string): void {
  console.log(`  \u2713 ${label}`);
  totalPassed++;
}

function fail(label: string, details: string[]): void {
  console.log(`  \u2717 ${label}`);
  details.forEach((d) => console.log(`      - ${d}`));
  errors.push(...details);
  totalFailed++;
}

// ── Validation checks ────────────────────────────────────────────────

console.log('\n=== Dino Monsters Data Validation ===\n');

// 1. No duplicate dino IDs
{
  const ids = dinos.map((d) => d.id);
  const seen = new Set<number>();
  const dupes: number[] = [];
  for (const id of ids) {
    if (seen.has(id)) dupes.push(id);
    seen.add(id);
  }
  if (dupes.length === 0) {
    pass(`No duplicate dino IDs (${ids.length} dinos)`);
  } else {
    fail('Duplicate dino IDs found', dupes.map((id) => `Dino ID ${id} appears more than once`));
  }
}

// 2. No duplicate move IDs
{
  const ids = moves.map((m) => m.id);
  const seen = new Set<number>();
  const dupes: number[] = [];
  for (const id of ids) {
    if (seen.has(id)) dupes.push(id);
    seen.add(id);
  }
  if (dupes.length === 0) {
    pass(`No duplicate move IDs (${ids.length} moves)`);
  } else {
    fail('Duplicate move IDs found', dupes.map((id) => `Move ID ${id} appears more than once`));
  }
}

// 3. All dinos have required fields
{
  const required = ['id', 'name', 'types', 'baseStats', 'learnset'];
  const statFields = ['hp', 'atk', 'def', 'spatk', 'spdef', 'speed'];
  const problems: string[] = [];

  for (const d of dinos) {
    for (const field of required) {
      if (d[field] === undefined || d[field] === null) {
        problems.push(`Dino #${d.id} (${d.name || '?'}): missing field "${field}"`);
      }
    }
    if (d.baseStats) {
      for (const sf of statFields) {
        if (d.baseStats[sf] === undefined || d.baseStats[sf] === null) {
          problems.push(`Dino #${d.id} (${d.name || '?'}): missing baseStats.${sf}`);
        }
      }
    }
  }

  if (problems.length === 0) {
    pass('All dinos have required fields (id, name, types, baseStats, learnset)');
  } else {
    fail('Dinos with missing required fields', problems);
  }
}

// 4. All moves have required fields
{
  const required = ['id', 'name', 'type', 'category', 'power', 'accuracy', 'pp'];
  const problems: string[] = [];

  for (const m of moves) {
    for (const field of required) {
      if (m[field] === undefined || m[field] === null) {
        problems.push(`Move #${m.id} (${m.name || '?'}): missing field "${field}"`);
      }
    }
  }

  if (problems.length === 0) {
    pass('All moves have required fields (id, name, type, category, power, accuracy, pp)');
  } else {
    fail('Moves with missing required fields', problems);
  }
}

// 5. Type range check -- all type values in dinos and moves are 0-13
{
  const problems: string[] = [];

  for (const d of dinos) {
    if (Array.isArray(d.types)) {
      for (const t of d.types) {
        if (typeof t !== 'number' || t < 0 || t > 13) {
          problems.push(`Dino #${d.id} (${d.name}): invalid type value ${t}`);
        }
      }
    }
  }

  for (const m of moves) {
    if (typeof m.type !== 'number' || m.type < 0 || m.type > 13) {
      problems.push(`Move #${m.id} (${m.name}): invalid type value ${m.type}`);
    }
  }

  if (problems.length === 0) {
    pass('All type values are in valid range 0-13');
  } else {
    fail('Invalid type values found', problems);
  }
}

// 6. All moveIds in dino learnsets exist in moves.json
{
  const moveIds = new Set(moves.map((m) => m.id));
  const problems: string[] = [];

  for (const d of dinos) {
    if (Array.isArray(d.learnset)) {
      for (const entry of d.learnset) {
        if (!moveIds.has(entry.moveId)) {
          problems.push(
            `Dino #${d.id} (${d.name}): learnset references move #${entry.moveId} which does not exist`,
          );
        }
      }
    }
  }

  if (problems.length === 0) {
    pass('All learnset moveIds reference existing moves');
  } else {
    fail('Learnset references to missing moves', problems);
  }
}

// 7. All evolution targets exist
{
  const dinoIds = new Set(dinos.map((d) => d.id));
  const problems: string[] = [];

  for (const d of dinos) {
    if (d.evolution && d.evolution.to !== undefined && d.evolution.to !== null) {
      if (!dinoIds.has(d.evolution.to)) {
        problems.push(
          `Dino #${d.id} (${d.name}): evolves to #${d.evolution.to} which does not exist`,
        );
      }
    }
  }

  if (problems.length === 0) {
    pass('All evolution targets point to existing dinos');
  } else {
    fail('Evolution targets pointing to missing dinos', problems);
  }
}

// 8. Type chart is 14x14
{
  const chart = typeChart.chart;
  const problems: string[] = [];

  if (!Array.isArray(chart)) {
    problems.push('type_chart.json .chart is not an array');
  } else {
    if (chart.length !== 14) {
      problems.push(`Type chart has ${chart.length} rows (expected 14)`);
    }
    chart.forEach((row: any, i: number) => {
      if (!Array.isArray(row)) {
        problems.push(`Row ${i} is not an array`);
      } else if (row.length !== 14) {
        problems.push(`Row ${i} has ${row.length} columns (expected 14)`);
      }
    });
  }

  if (problems.length === 0) {
    pass('Type chart is 14x14');
  } else {
    fail('Type chart dimension issues', problems);
  }
}

// ── Summary ──────────────────────────────────────────────────────────

console.log('\n--- Summary ---');
console.log(`  Passed: ${totalPassed}`);
console.log(`  Failed: ${totalFailed}`);

if (totalFailed > 0) {
  console.log('\nAll errors:');
  errors.forEach((e) => console.log(`  - ${e}`));
  process.exit(1);
} else {
  console.log('\nAll checks passed!');
  process.exit(0);
}
