import { describe, it, expect } from 'vitest';
import { calcCaptureRate } from '../utils/damageCalc';

describe('Capture Mechanics', () => {
  it('full HP, common dino (rate 255): should still be capturable', () => {
    const rate = calcCaptureRate(100, 100, 255, 1.0, 1.0);
    // hpFactor at full HP = (300-200)/300 = 1/3
    // rate = floor(1/3 * 255 * 1 * 1) = floor(85) = 85
    expect(rate).toBe(85);
    expect(rate).toBeGreaterThan(0);
  });

  it('1 HP gives much higher rate than full HP', () => {
    const fullHp = calcCaptureRate(100, 100, 200, 1.0, 1.0);
    const oneHp = calcCaptureRate(100, 1, 200, 1.0, 1.0);
    expect(oneHp).toBeGreaterThan(fullHp);
  });

  it('1 HP maximizes hpFactor close to 1.0', () => {
    const rate = calcCaptureRate(100, 1, 255, 1.0, 1.0);
    // hpFactor = (300-2)/300 = 298/300 ~ 0.993
    // rate = floor(0.993 * 255) = floor(253.3) = 253
    expect(rate).toBeGreaterThanOrEqual(250);
  });

  it('sleep/freeze gives 2.5x modifier', () => {
    const noStatus = calcCaptureRate(100, 50, 100, 1.0, 1.0);
    const sleep = calcCaptureRate(100, 50, 100, 1.0, 2.5);
    expect(sleep).toBeGreaterThan(noStatus);
    // Sleep rate should be roughly 2.5x the no-status rate (rounding may differ by 1)
    expect(sleep).toBeGreaterThanOrEqual(Math.floor(noStatus * 2.5) - 1);
    expect(sleep).toBeLessThanOrEqual(Math.ceil(noStatus * 2.5) + 1);
  });

  it('poison/burn gives 1.5x modifier', () => {
    const noStatus = calcCaptureRate(100, 50, 100, 1.0, 1.0);
    const poison = calcCaptureRate(100, 50, 100, 1.0, 1.5);
    expect(poison).toBeGreaterThan(noStatus);
  });

  it('Ultra Ball gives 2.0x modifier — higher rate than Pokeball', () => {
    const pokeball = calcCaptureRate(100, 50, 100, 1.0, 1.0);
    const ultraBall = calcCaptureRate(100, 50, 100, 2.0, 1.0);
    expect(ultraBall).toBeGreaterThan(pokeball);
    expect(ultraBall).toBeLessThanOrEqual(255);
  });

  it('legendary (rate 3): nearly impossible at full HP', () => {
    const rate = calcCaptureRate(200, 200, 3, 1.0, 1.0);
    // hpFactor = 1/3, rate = floor(1/3 * 3) = floor(1) = 1
    expect(rate).toBeLessThanOrEqual(3);
    expect(rate).toBeGreaterThanOrEqual(1);
  });

  it('rate is capped at 255', () => {
    const rate = calcCaptureRate(100, 1, 255, 2.0, 2.5);
    expect(rate).toBeLessThanOrEqual(255);
  });

  it('rate is always non-negative', () => {
    const rate = calcCaptureRate(100, 100, 3, 1.0, 1.0);
    expect(rate).toBeGreaterThanOrEqual(0);
  });

  it('half HP with standard ball and no status', () => {
    const rate = calcCaptureRate(100, 50, 200, 1.0, 1.0);
    // hpFactor = (300-100)/300 = 200/300 = 2/3
    // rate = floor(2/3 * 200) = floor(133.3) = 133
    expect(rate).toBe(133);
  });
});
