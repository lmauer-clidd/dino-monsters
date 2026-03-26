export interface DamageInput {
  level: number;
  power: number;
  attack: number;
  defense: number;
  stab: boolean;
  effectiveness: number;
  critical: boolean;
  randomFactor?: number; // 0.85-1.0, default 1.0 for deterministic tests
}

export function calculatePureDamage(input: DamageInput): number {
  // Immune check
  if (input.effectiveness === 0) return 0;

  let dmg = ((2 * input.level / 5 + 2) * input.power * (input.attack / input.defense)) / 50 + 2;
  if (input.stab) dmg *= 1.5;
  dmg *= input.effectiveness;
  if (input.critical) dmg *= 1.5;
  dmg *= input.randomFactor ?? 1.0;
  return Math.max(1, Math.floor(dmg));
}

export function calcStat(base: number, level: number, gv: number, tp: number, isHp: boolean): number {
  const raw = Math.floor(((2 * base + gv + Math.floor(tp / 4)) * level) / 100);
  return isHp ? raw + level + 10 : raw + 5;
}

export function calcCaptureRate(
  maxHp: number,
  currentHp: number,
  captureRate: number,
  ballMod: number,
  statusMod: number,
): number {
  const hpFactor = (3 * maxHp - 2 * currentHp) / (3 * maxHp);
  return Math.min(255, Math.floor(hpFactor * captureRate * ballMod * statusMod));
}

export function calcXpRequired(level: number, group: 'fast' | 'medium' | 'slow'): number {
  switch (group) {
    case 'fast':
      return Math.floor((4 * Math.pow(level, 3)) / 5);
    case 'medium':
      return Math.pow(level, 3);
    case 'slow':
      return Math.floor((5 * Math.pow(level, 3)) / 4);
  }
}

export function calcXpYield(
  baseXpYield: number,
  defeatedLevel: number,
  participantCount: number = 1,
): number {
  return Math.floor((baseXpYield * defeatedLevel) / (5 * participantCount));
}
