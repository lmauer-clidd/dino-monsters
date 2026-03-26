import Phaser from 'phaser';

// ============================================================
// ParticlePool — Object-pooled procedural particle system
// Zero GC during combat: pre-allocated pool, no textures
// ============================================================

export type ParticleShape = 'circle' | 'square' | 'triangle' | 'line';

export interface Particle {
  x: number;
  y: number;
  vx: number;
  vy: number;
  life: number;
  maxLife: number;
  size: number;
  color: number;
  alpha: number;
  alphaStart: number;
  alphaEnd: number;
  gravity: number;
  shape: ParticleShape;
  active: boolean;
}

export interface EmitConfig {
  count: number;
  x: number;
  y: number;
  speed: { min: number; max: number };
  angle: { min: number; max: number };     // degrees
  life: { min: number; max: number };      // ms
  size: { min: number; max: number };
  color: number | readonly number[];        // single or array to pick from
  alpha?: { start: number; end: number };
  gravity?: number;
  shape?: ParticleShape;
}

// -- Type color palettes --
const TYPE_COLORS = {
  fire:     [0xF08030, 0xF8D830],
  water:    [0x6890F0, 0x88C0F8],
  flora:    [0x48A030, 0x78D860],
  electric: [0xF8D030, 0xFFFFFF],
  ice:      [0x98D8D8, 0xD0F0FF],
  shadow:   [0x705898, 0x483868],
  light:    [0xF8F878, 0xFFF8E0],
  normal:   [0xC0C0C0, 0xF0F0F0],
  earth:    [0xC0A060, 0x8B7040],
  fossil:   [0xB8A060, 0x887860],
  air:      [0xA0C0E8, 0xC8E0F8],
  venom:    [0xA040A0, 0xD080D0],
  metal:    [0xB8B8D0, 0xD0D0E0],
  primal:   [0xE8C050, 0xF8E888],
} as const;

const DEFAULT_POOL_SIZE = 200;

export class ParticlePool {
  private pool: Particle[];
  private graphics: Phaser.GameObjects.Graphics;

  constructor(scene: Phaser.Scene, poolSize: number = DEFAULT_POOL_SIZE, depth: number = 100) {
    this.graphics = scene.add.graphics().setDepth(depth);
    this.pool = [];
    for (let i = 0; i < poolSize; i++) {
      this.pool.push(this.createDeadParticle());
    }
  }

  // ----------------------------------------------------------
  // Core API
  // ----------------------------------------------------------

  /**
   * Spawn particles from the pool according to config.
   */
  emit(config: EmitConfig): void {
    const colors = Array.isArray(config.color) ? config.color : [config.color];
    const alphaStart = config.alpha?.start ?? 1;
    const alphaEnd = config.alpha?.end ?? 0;
    const gravity = config.gravity ?? 0;
    const shape = config.shape ?? 'circle';

    let spawned = 0;
    for (let i = 0; i < this.pool.length && spawned < config.count; i++) {
      const p = this.pool[i];
      if (p.active) continue;

      const angleDeg = Phaser.Math.FloatBetween(config.angle.min, config.angle.max);
      const angleRad = Phaser.Math.DegToRad(angleDeg);
      const speed = Phaser.Math.FloatBetween(config.speed.min, config.speed.max);

      p.x = config.x;
      p.y = config.y;
      p.vx = Math.cos(angleRad) * speed;
      p.vy = Math.sin(angleRad) * speed;
      p.life = 0;
      p.maxLife = Phaser.Math.FloatBetween(config.life.min, config.life.max);
      p.size = Phaser.Math.FloatBetween(config.size.min, config.size.max);
      p.color = Phaser.Utils.Array.GetRandom(colors);
      p.alphaStart = alphaStart;
      p.alphaEnd = alphaEnd;
      p.alpha = alphaStart;
      p.gravity = gravity;
      p.shape = shape;
      p.active = true;

      spawned++;
    }
  }

  /**
   * Advance all active particles. Call every frame with scene delta (ms).
   */
  update(delta: number): void {
    const dt = delta / 1000; // convert to seconds

    for (let i = 0; i < this.pool.length; i++) {
      const p = this.pool[i];
      if (!p.active) continue;

      p.life += delta;
      if (p.life >= p.maxLife) {
        p.active = false;
        continue;
      }

      const t = p.life / p.maxLife; // 0..1

      p.vy += p.gravity * dt;
      p.x += p.vx * dt;
      p.y += p.vy * dt;

      // Lerp alpha
      p.alpha = p.alphaStart + (p.alphaEnd - p.alphaStart) * t;
    }
  }

  /**
   * Draw all active particles. Call every frame after update().
   */
  render(): void {
    this.graphics.clear();

    for (let i = 0; i < this.pool.length; i++) {
      const p = this.pool[i];
      if (!p.active) continue;

      this.graphics.fillStyle(p.color, p.alpha);

      switch (p.shape) {
        case 'circle':
          this.graphics.fillCircle(p.x, p.y, p.size);
          break;

        case 'square':
          this.graphics.fillRect(
            p.x - p.size / 2,
            p.y - p.size / 2,
            p.size,
            p.size
          );
          break;

        case 'triangle': {
          const s = p.size;
          this.graphics.fillTriangle(
            p.x, p.y - s,
            p.x - s * 0.866, p.y + s * 0.5,
            p.x + s * 0.866, p.y + s * 0.5
          );
          break;
        }

        case 'line': {
          this.graphics.lineStyle(1, p.color, p.alpha);
          const len = p.size * 2;
          const angle = Math.atan2(p.vy, p.vx);
          this.graphics.lineBetween(
            p.x, p.y,
            p.x - Math.cos(angle) * len,
            p.y - Math.sin(angle) * len
          );
          break;
        }
      }
    }
  }

  /**
   * Returns the underlying Graphics object (for depth/visibility control).
   */
  getGraphics(): Phaser.GameObjects.Graphics {
    return this.graphics;
  }

  /**
   * Kill all active particles instantly.
   */
  clear(): void {
    for (let i = 0; i < this.pool.length; i++) {
      this.pool[i].active = false;
    }
    this.graphics.clear();
  }

  /**
   * Destroy the pool and its graphics object.
   */
  destroy(): void {
    this.clear();
    this.graphics.destroy();
  }

  /**
   * Count of currently active particles.
   */
  get activeCount(): number {
    let count = 0;
    for (let i = 0; i < this.pool.length; i++) {
      if (this.pool[i].active) count++;
    }
    return count;
  }

  // ----------------------------------------------------------
  // Internal
  // ----------------------------------------------------------

  private createDeadParticle(): Particle {
    return {
      x: 0, y: 0, vx: 0, vy: 0,
      life: 0, maxLife: 0,
      size: 0, color: 0, alpha: 0,
      alphaStart: 1, alphaEnd: 0,
      gravity: 0,
      shape: 'circle',
      active: false,
    };
  }

  // ----------------------------------------------------------
  // Static presets — emit configs for each dino type
  // ----------------------------------------------------------

  static fireHit(x: number, y: number): EmitConfig {
    return {
      count: 20,
      x, y,
      speed: { min: 60, max: 180 },
      angle: { min: 200, max: 340 },
      life: { min: 300, max: 600 },
      size: { min: 2, max: 5 },
      color: TYPE_COLORS.fire,
      alpha: { start: 1, end: 0 },
      gravity: -40,
      shape: 'circle',
    };
  }

  static waterSplash(x: number, y: number): EmitConfig {
    return {
      count: 24,
      x, y,
      speed: { min: 40, max: 150 },
      angle: { min: 210, max: 330 },
      life: { min: 300, max: 700 },
      size: { min: 2, max: 4 },
      color: TYPE_COLORS.water,
      alpha: { start: 0.9, end: 0 },
      gravity: 120,
      shape: 'circle',
    };
  }

  static electricSpark(x: number, y: number): EmitConfig {
    return {
      count: 16,
      x, y,
      speed: { min: 100, max: 280 },
      angle: { min: 0, max: 360 },
      life: { min: 100, max: 350 },
      size: { min: 2, max: 5 },
      color: TYPE_COLORS.electric,
      alpha: { start: 1, end: 0.2 },
      gravity: 0,
      shape: 'line',
    };
  }

  static floraLeaves(x: number, y: number): EmitConfig {
    return {
      count: 14,
      x, y,
      speed: { min: 30, max: 100 },
      angle: { min: 200, max: 340 },
      life: { min: 400, max: 800 },
      size: { min: 2, max: 4 },
      color: TYPE_COLORS.flora,
      alpha: { start: 0.9, end: 0 },
      gravity: 30,
      shape: 'triangle',
    };
  }

  static iceShards(x: number, y: number): EmitConfig {
    return {
      count: 18,
      x, y,
      speed: { min: 50, max: 160 },
      angle: { min: 210, max: 330 },
      life: { min: 300, max: 600 },
      size: { min: 2, max: 5 },
      color: TYPE_COLORS.ice,
      alpha: { start: 1, end: 0.1 },
      gravity: 20,
      shape: 'square',
    };
  }

  static shadowWisp(x: number, y: number): EmitConfig {
    return {
      count: 12,
      x, y,
      speed: { min: 20, max: 80 },
      angle: { min: 0, max: 360 },
      life: { min: 500, max: 1000 },
      size: { min: 3, max: 6 },
      color: TYPE_COLORS.shadow,
      alpha: { start: 0.7, end: 0 },
      gravity: -20,
      shape: 'circle',
    };
  }

  static lightBurst(x: number, y: number): EmitConfig {
    return {
      count: 22,
      x, y,
      speed: { min: 80, max: 220 },
      angle: { min: 0, max: 360 },
      life: { min: 200, max: 500 },
      size: { min: 2, max: 4 },
      color: TYPE_COLORS.light,
      alpha: { start: 1, end: 0 },
      gravity: 0,
      shape: 'circle',
    };
  }

  static normalHit(x: number, y: number): EmitConfig {
    return {
      count: 10,
      x, y,
      speed: { min: 40, max: 120 },
      angle: { min: 0, max: 360 },
      life: { min: 200, max: 400 },
      size: { min: 2, max: 4 },
      color: TYPE_COLORS.normal,
      alpha: { start: 0.8, end: 0 },
      gravity: 60,
      shape: 'square',
    };
  }

  static earthRumble(x: number, y: number): EmitConfig {
    return {
      count: 16,
      x, y,
      speed: { min: 30, max: 100 },
      angle: { min: 230, max: 310 },
      life: { min: 300, max: 600 },
      size: { min: 3, max: 6 },
      color: TYPE_COLORS.earth,
      alpha: { start: 1, end: 0 },
      gravity: 150,
      shape: 'square',
    };
  }

  static fossilCrumble(x: number, y: number): EmitConfig {
    return {
      count: 14,
      x, y,
      speed: { min: 30, max: 90 },
      angle: { min: 220, max: 320 },
      life: { min: 400, max: 700 },
      size: { min: 2, max: 5 },
      color: TYPE_COLORS.fossil,
      alpha: { start: 0.9, end: 0 },
      gravity: 100,
      shape: 'square',
    };
  }

  static airGust(x: number, y: number): EmitConfig {
    return {
      count: 18,
      x, y,
      speed: { min: 60, max: 180 },
      angle: { min: 160, max: 200 },
      life: { min: 300, max: 600 },
      size: { min: 2, max: 4 },
      color: TYPE_COLORS.air,
      alpha: { start: 0.6, end: 0 },
      gravity: -30,
      shape: 'circle',
    };
  }

  static venomSplatter(x: number, y: number): EmitConfig {
    return {
      count: 16,
      x, y,
      speed: { min: 40, max: 130 },
      angle: { min: 0, max: 360 },
      life: { min: 400, max: 800 },
      size: { min: 2, max: 5 },
      color: TYPE_COLORS.venom,
      alpha: { start: 0.9, end: 0.1 },
      gravity: 80,
      shape: 'circle',
    };
  }

  static metalClang(x: number, y: number): EmitConfig {
    return {
      count: 12,
      x, y,
      speed: { min: 80, max: 200 },
      angle: { min: 0, max: 360 },
      life: { min: 150, max: 350 },
      size: { min: 1, max: 3 },
      color: TYPE_COLORS.metal,
      alpha: { start: 1, end: 0.2 },
      gravity: 0,
      shape: 'line',
    };
  }

  static primalSurge(x: number, y: number): EmitConfig {
    return {
      count: 24,
      x, y,
      speed: { min: 50, max: 200 },
      angle: { min: 0, max: 360 },
      life: { min: 300, max: 700 },
      size: { min: 3, max: 6 },
      color: TYPE_COLORS.primal,
      alpha: { start: 1, end: 0 },
      gravity: -20,
      shape: 'triangle',
    };
  }
}
