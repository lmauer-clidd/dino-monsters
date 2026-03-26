import Phaser from 'phaser';
import {
  SCENE_KEYS,
  GAME_WIDTH,
  GAME_HEIGHT,
  COLORS,
  FONT_FAMILY,
  DinoType,
  DINO_TYPE_COLORS,
  DINO_TYPE_NAMES,
  STAB_MULTIPLIER,
  CRITICAL_HIT_CHANCE,
  CRITICAL_HIT_MULTIPLIER,
  DAMAGE_RANDOM_MIN,
  DAMAGE_RANDOM_MAX,
  StatusEffect,
} from '../utils/constants';
import { BattleHUD } from '../ui/BattleHUD';
import { TransitionFX } from '../ui/TransitionFX';
import { GameState } from '../systems/GameState';
import { TrainerDino } from '../data/trainers';
import { Dino, getSpecies } from '../entities/Dino';
import { getMove, hasMove } from '../systems/BattleSystem';
import { getItem, hasItemData } from '../systems/InventorySystem';
import { AudioSystem } from '../systems/AudioSystem';
import { AnimationSequencer } from '../systems/AnimationSequencer';
import { ParticlePool, EmitConfig } from '../systems/ParticlePool';
import { CameraFX } from '../systems/CameraFX';

// ============================================================
// Map DinoType to ParticlePool preset for attack hit effects
// ============================================================
function getTypeParticleConfig(type: DinoType, x: number, y: number): EmitConfig {
  switch (type) {
    case DinoType.Fire:     return ParticlePool.fireHit(x, y);
    case DinoType.Water:    return ParticlePool.waterSplash(x, y);
    case DinoType.Electric: return ParticlePool.electricSpark(x, y);
    case DinoType.Flora:    return ParticlePool.floraLeaves(x, y);
    case DinoType.Ice:      return ParticlePool.iceShards(x, y);
    case DinoType.Shadow:   return ParticlePool.shadowWisp(x, y);
    case DinoType.Light:    return ParticlePool.lightBurst(x, y);
    case DinoType.Earth:    return ParticlePool.earthRumble(x, y);
    case DinoType.Fossil:   return ParticlePool.fossilCrumble(x, y);
    case DinoType.Air:      return ParticlePool.airGust(x, y);
    case DinoType.Venom:    return ParticlePool.venomSplatter(x, y);
    case DinoType.Metal:    return ParticlePool.metalClang(x, y);
    case DinoType.Primal:   return ParticlePool.primalSurge(x, y);
    case DinoType.Normal:
    default:                return ParticlePool.normalHit(x, y);
  }
}

// ============================================================
// Type effectiveness chart (simplified inline)
// ============================================================
const TYPE_CHART: Record<number, Record<number, number>> = {
  [DinoType.Fire]: {
    [DinoType.Flora]: 2, [DinoType.Ice]: 2, [DinoType.Metal]: 2,
    [DinoType.Water]: 0.5, [DinoType.Fire]: 0.5, [DinoType.Earth]: 0.5, [DinoType.Fossil]: 0.5,
  },
  [DinoType.Water]: {
    [DinoType.Fire]: 2, [DinoType.Earth]: 2, [DinoType.Fossil]: 2,
    [DinoType.Water]: 0.5, [DinoType.Flora]: 0.5, [DinoType.Electric]: 0.5,
  },
  [DinoType.Flora]: {
    [DinoType.Water]: 2, [DinoType.Earth]: 2, [DinoType.Fossil]: 2,
    [DinoType.Fire]: 0.5, [DinoType.Flora]: 0.5, [DinoType.Venom]: 0.5, [DinoType.Air]: 0.5,
  },
  [DinoType.Electric]: {
    [DinoType.Water]: 2, [DinoType.Air]: 2, [DinoType.Metal]: 2,
    [DinoType.Electric]: 0.5, [DinoType.Earth]: 0, [DinoType.Flora]: 0.5,
  },
  [DinoType.Earth]: {
    [DinoType.Fire]: 2, [DinoType.Electric]: 2, [DinoType.Venom]: 2, [DinoType.Fossil]: 2, [DinoType.Metal]: 2,
    [DinoType.Flora]: 0.5, [DinoType.Air]: 0,
  },
  [DinoType.Ice]: {
    [DinoType.Flora]: 2, [DinoType.Earth]: 2, [DinoType.Air]: 2, [DinoType.Primal]: 2,
    [DinoType.Fire]: 0.5, [DinoType.Water]: 0.5, [DinoType.Ice]: 0.5, [DinoType.Metal]: 0.5,
  },
  [DinoType.Air]: {
    [DinoType.Flora]: 2, [DinoType.Venom]: 2,
    [DinoType.Electric]: 0.5, [DinoType.Fossil]: 0.5, [DinoType.Ice]: 0.5,
  },
  [DinoType.Venom]: {
    [DinoType.Flora]: 2, [DinoType.Light]: 2,
    [DinoType.Earth]: 0.5, [DinoType.Fossil]: 0.5, [DinoType.Venom]: 0.5, [DinoType.Metal]: 0,
  },
  [DinoType.Shadow]: {
    [DinoType.Shadow]: 2, [DinoType.Light]: 2,
    [DinoType.Normal]: 0, [DinoType.Metal]: 0.5,
  },
  [DinoType.Light]: {
    [DinoType.Shadow]: 2, [DinoType.Venom]: 2,
    [DinoType.Light]: 0.5, [DinoType.Metal]: 0.5,
  },
  [DinoType.Fossil]: {
    [DinoType.Fire]: 2, [DinoType.Ice]: 2, [DinoType.Air]: 2,
    [DinoType.Earth]: 0.5, [DinoType.Metal]: 0.5,
  },
  [DinoType.Metal]: {
    [DinoType.Ice]: 2, [DinoType.Fossil]: 2, [DinoType.Light]: 2,
    [DinoType.Fire]: 0.5, [DinoType.Electric]: 0.5, [DinoType.Metal]: 0.5,
  },
  [DinoType.Primal]: {
    [DinoType.Primal]: 2, [DinoType.Shadow]: 2,
    [DinoType.Light]: 0.5, [DinoType.Metal]: 0.5,
  },
  [DinoType.Normal]: {},
};

function getTypeEffectiveness(atkType: DinoType, defTypes: DinoType[]): number {
  let mult = 1;
  for (const dt of defTypes) {
    const chart = TYPE_CHART[atkType];
    if (chart && chart[dt] !== undefined) {
      mult *= chart[dt];
    }
  }
  return mult;
}

// ============================================================
// Data interfaces
// ============================================================
interface MoveData {
  name: string;
  type: DinoType;
  category: 'physical' | 'special' | 'status';
  power: number;
  pp: number;
  maxPp: number;
}

interface DinoData {
  name: string;
  level: number;
  types: DinoType[];
  currentHp: number;
  maxHp: number;
  moves: MoveData[];
  attack?: number;
  defense?: number;
  spAttack?: number;
  spDefense?: number;
  speed?: number;
  status?: StatusEffect;
}

interface BattleInitData {
  isWild: boolean;
  wildDino?: DinoData;
  returnScene?: string;
  returnData?: any;
  // Trainer battle fields
  trainerId?: string;
  trainerName?: string;
  trainerClass?: string;
  trainerParty?: TrainerDino[];
  trainerDialogueAfter?: string;
  reward?: number;
  badge?: number;
  tmReward?: { moveId: number; name: string };
  isEliteFour?: boolean;
  isChampion?: boolean;
}

// ============================================================
// Floating particle for dino type effects
// ============================================================
interface TypeParticle {
  x: number;
  y: number;
  vx: number;
  vy: number;
  life: number;
  maxLife: number;
  color: number;
  size: number;
}

// ============================================================
// BattleScene
// ============================================================
export class BattleScene extends Phaser.Scene {
  private hud!: BattleHUD;
  private bgGraphics!: Phaser.GameObjects.Graphics;
  private enemyGfx!: Phaser.GameObjects.Graphics;
  private playerGfx!: Phaser.GameObjects.Graphics;
  private particleGfx!: Phaser.GameObjects.Graphics;

  // Positions (used for animations)
  private enemyBaseX = 0;
  private enemyBaseY = 0;
  private playerBaseX = 0;
  private playerBaseY = 0;
  private enemyOffsetX = 0;
  private enemyOffsetY = 0;
  private playerOffsetX = 0;
  private playerOffsetY = 0;

  // Battle data
  private isWild = true;
  private returnScene: string = SCENE_KEYS.OVERWORLD;
  private returnData: any = {};
  private playerDino!: DinoData;
  private enemyDino!: DinoData;

  // Trainer battle data
  private trainerId?: string;
  private trainerName?: string;
  private trainerClass?: string;
  private trainerParty: TrainerDino[] = [];
  private trainerPartyIndex = 0;
  private trainerDialogueAfter?: string;
  private reward = 0;
  private badge?: number;
  private tmReward?: { moveId: number; name: string };
  private isEliteFour = false;
  private isChampion = false;

  // State
  private battleOver = false;
  private animating = false;

  // Type effect particles (ambient)
  private enemyParticles: TypeParticle[] = [];
  private playerParticles: TypeParticle[] = [];

  // Battle FX particle pool (hits, super-effective, etc.)
  private battleParticlePool!: ParticlePool;

  constructor() {
    super({ key: SCENE_KEYS.BATTLE });
  }

  init(data: BattleInitData): void {
    this.isWild = data.isWild ?? true;
    this.returnScene = data.returnScene ?? SCENE_KEYS.OVERWORLD;
    this.returnData = data.returnData ?? {};

    // Trainer battle data
    this.trainerId = data.trainerId;
    this.trainerName = data.trainerName;
    this.trainerClass = data.trainerClass;
    this.trainerDialogueAfter = data.trainerDialogueAfter;
    this.reward = data.reward ?? 0;
    this.badge = data.badge;
    this.tmReward = data.tmReward;
    this.isEliteFour = data.isEliteFour ?? false;
    this.isChampion = data.isChampion ?? false;

    // Build trainer party for sequential battles
    if (!this.isWild && data.trainerParty && data.trainerParty.length > 0) {
      this.trainerParty = data.trainerParty;
      this.trainerPartyIndex = 0;
      const firstDino = this.trainerParty[0];
      this.enemyDino = this.buildTrainerDino(firstDino);
    } else {
      this.trainerParty = [];
      this.trainerPartyIndex = 0;
      this.enemyDino = data.wildDino ?? {
        name: 'COMPSO',
        level: 3,
        types: [DinoType.Normal],
        currentHp: 20,
        maxHp: 20,
        moves: [
          { name: 'Charge', type: DinoType.Normal, category: 'physical', power: 40, pp: 35, maxPp: 35 },
        ],
      };
    }

    // Player dino from GameState if available, otherwise default
    const gs = GameState.getInstance();
    const party = gs.party;
    if (party.length > 0) {
      const lead = party[0];
      const types: DinoType[] = [lead.type1];
      if (lead.type2 !== undefined) types.push(lead.type2);

      // Resolve moves from move registry
      const moveList: MoveData[] = lead.moves.slice(0, 4).map(slot => {
        try {
          if (hasMove(slot.moveId)) {
            const md = getMove(slot.moveId);
            return { name: md.name, type: md.type, category: md.category, power: md.power, pp: slot.currentPP, maxPp: slot.maxPP };
          }
        } catch (_e) { /* ignore */ }
        return { name: 'Charge', type: DinoType.Normal, category: 'physical', power: 40, pp: slot.currentPP, maxPp: slot.maxPP };
      });

      this.playerDino = {
        name: lead.nickname,
        level: lead.level,
        types,
        currentHp: lead.currentHp,
        maxHp: lead.maxHp,
        attack: lead.stats.attack,
        defense: lead.stats.defense,
        spAttack: lead.stats.spAttack,
        spDefense: lead.stats.spDefense,
        speed: lead.stats.speed,
        status: lead.status ?? StatusEffect.None,
        moves: moveList.length > 0 ? moveList : [
          { name: 'Charge', type: DinoType.Normal, category: 'physical', power: 40, pp: 35, maxPp: 35 },
        ],
      };
    } else {
      // Fallback default starter — level 5, only Normal attack + stat move
      this.playerDino = {
        name: 'PYREX',
        level: 5,
        types: [DinoType.Fire],
        currentHp: 25,
        maxHp: 25,
        attack: 12,
        defense: 10,
        spAttack: 11,
        spDefense: 10,
        speed: 13,
        moves: [
          { name: 'Charge', type: DinoType.Normal, category: 'physical', power: 40, pp: 35, maxPp: 35 },
          { name: 'Grondement', type: DinoType.Normal, category: 'status', power: 0, pp: 40, maxPp: 40 },
        ],
      };
    }

    // Default stats if missing — use the same balanced formula for both sides
    // At level 5: ~10-12. At level 50: ~55-60. Mirrors the Pokémon stat formula output.
    const defaultStat = (base: number, level: number) => Math.floor(((2 * base + 20) * level) / 100 + 5);
    if (!this.enemyDino.attack) this.enemyDino.attack = defaultStat(50, this.enemyDino.level);
    if (!this.enemyDino.defense) this.enemyDino.defense = defaultStat(50, this.enemyDino.level);
    if (!this.enemyDino.spAttack) this.enemyDino.spAttack = defaultStat(50, this.enemyDino.level);
    if (!this.enemyDino.spDefense) this.enemyDino.spDefense = defaultStat(50, this.enemyDino.level);
    if (!this.enemyDino.speed) this.enemyDino.speed = defaultStat(50, this.enemyDino.level);
    if (!this.playerDino.attack) this.playerDino.attack = defaultStat(50, this.playerDino.level);
    if (!this.playerDino.defense) this.playerDino.defense = defaultStat(50, this.playerDino.level);
    if (!this.playerDino.spAttack) this.playerDino.spAttack = defaultStat(50, this.playerDino.level);
    if (!this.playerDino.spDefense) this.playerDino.spDefense = defaultStat(50, this.playerDino.level);
    if (!this.playerDino.speed) this.playerDino.speed = defaultStat(50, this.playerDino.level);
  }

  create(): void {
    this.battleOver = false;
    this.animating = false;
    this.enemyOffsetX = 0;
    this.enemyOffsetY = 0;
    this.playerOffsetX = 0;
    this.playerOffsetY = 0;

    // Base positions for dinos
    this.enemyBaseX = GAME_WIDTH * 0.72;
    this.enemyBaseY = GAME_HEIGHT * 0.32;
    this.playerBaseX = GAME_WIDTH * 0.22;
    this.playerBaseY = GAME_HEIGHT * 0.58;

    // Background
    this.bgGraphics = this.add.graphics();
    this.drawBackground();

    // Particle layer (ambient type particles)
    this.particleGfx = this.add.graphics().setDepth(15);
    this.initTypeParticles();

    // Battle FX particle pool (hit effects, rendered above dinos)
    this.battleParticlePool = new ParticlePool(this, 200, 50);

    // Dino graphics
    this.enemyGfx = this.add.graphics().setDepth(10);
    this.playerGfx = this.add.graphics().setDepth(10);

    // HUD
    this.hud = new BattleHUD();
    this.hud.create(this);

    this.hud.setEnemyDino(
      this.enemyDino.name,
      this.enemyDino.level,
      this.enemyDino.types,
      this.enemyDino.currentHp,
      this.enemyDino.maxHp
    );

    this.hud.setPlayerDino(
      this.playerDino.name,
      this.playerDino.level,
      this.playerDino.types,
      this.playerDino.currentHp,
      this.playerDino.maxHp
    );

    this.hud.updateXP(0, 100);

    // Show status icons if any
    this.hud.setEnemyStatus(this.enemyDino.status);
    this.hud.setPlayerStatus(this.playerDino.status);

    // Start battle music
    const audio = AudioSystem.getInstance();
    audio.init();
    audio.playBattleMusic();

    // Intro animation
    this.playIntro();
  }

  // ============================================================
  // Background
  // ============================================================
  private drawBackground(): void {
    const g = this.bgGraphics;

    // Sky gradient (top light blue to medium blue)
    const skyTop = GAME_HEIGHT * 0.5;
    const steps = 20;
    for (let i = 0; i < steps; i++) {
      const t = i / steps;
      const r = Phaser.Math.Linear(0x88, 0x58, t);
      const gr = Phaser.Math.Linear(0xD0, 0xA8, t);
      const b = Phaser.Math.Linear(0xF8, 0xE8, t);
      const color = (Math.floor(r) << 16) | (Math.floor(gr) << 8) | Math.floor(b);
      const yy = (skyTop / steps) * i;
      g.fillStyle(color, 1);
      g.fillRect(0, yy, GAME_WIDTH, skyTop / steps + 1);
    }

    // Ground — perspective grass field
    const groundTop = skyTop;
    const groundSteps = 15;
    for (let i = 0; i < groundSteps; i++) {
      const t = i / groundSteps;
      const r = Phaser.Math.Linear(0x78, 0x48, t);
      const gr2 = Phaser.Math.Linear(0xC0, 0x90, t);
      const b = Phaser.Math.Linear(0x58, 0x38, t);
      const color = (Math.floor(r) << 16) | (Math.floor(gr2) << 8) | Math.floor(b);
      const yy = groundTop + ((GAME_HEIGHT - groundTop) / groundSteps) * i;
      g.fillStyle(color, 1);
      g.fillRect(0, yy, GAME_WIDTH, (GAME_HEIGHT - groundTop) / groundSteps + 1);
    }

    // Horizontal grass row lines
    g.lineStyle(1, 0x58A050, 0.15);
    for (let ly = groundTop + 20; ly < GAME_HEIGHT; ly += 16) {
      g.lineBetween(0, ly, GAME_WIDTH, ly);
    }

    // Enemy platform (elevated ellipse, upper-right)
    g.fillStyle(0x90C070, 1);
    g.fillEllipse(this.enemyBaseX, this.enemyBaseY + 40, 140, 28);
    g.fillStyle(0xA0D080, 0.7);
    g.fillEllipse(this.enemyBaseX, this.enemyBaseY + 38, 120, 20);
    // Edge highlight
    g.lineStyle(2, 0x68A048, 0.4);
    g.strokeEllipse(this.enemyBaseX, this.enemyBaseY + 40, 140, 28);

    // Player platform (lower-left)
    g.fillStyle(0x78A058, 1);
    g.fillEllipse(this.playerBaseX, this.playerBaseY + 50, 160, 32);
    g.fillStyle(0x88B068, 0.7);
    g.fillEllipse(this.playerBaseX, this.playerBaseY + 48, 140, 24);
    g.lineStyle(2, 0x60903C, 0.4);
    g.strokeEllipse(this.playerBaseX, this.playerBaseY + 50, 160, 32);
  }

  // ============================================================
  // Type effect particles
  // ============================================================
  private initTypeParticles(): void {
    this.enemyParticles = [];
    this.playerParticles = [];

    // Enemy dino particles
    this.spawnTypeParticles(
      this.enemyParticles,
      this.enemyDino.types[0],
      this.enemyBaseX,
      this.enemyBaseY,
      4
    );

    // Player dino particles
    this.spawnTypeParticles(
      this.playerParticles,
      this.playerDino.types[0],
      this.playerBaseX,
      this.playerBaseY,
      4
    );
  }

  private spawnTypeParticles(
    arr: TypeParticle[],
    type: DinoType,
    cx: number,
    cy: number,
    count: number
  ): void {
    const pColor = DINO_TYPE_COLORS[type] ?? 0xA8A878;

    for (let i = 0; i < count; i++) {
      arr.push({
        x: cx + Phaser.Math.FloatBetween(-20, 20),
        y: cy + Phaser.Math.FloatBetween(-10, 20),
        vx: Phaser.Math.FloatBetween(-0.2, 0.2),
        vy: this.getTypeParticleVY(type),
        life: Phaser.Math.Between(0, 80),
        maxLife: Phaser.Math.Between(80, 160),
        color: pColor,
        size: Phaser.Math.FloatBetween(1.5, 3),
      });
    }
  }

  private getTypeParticleVY(type: DinoType): number {
    switch (type) {
      case DinoType.Fire: return Phaser.Math.FloatBetween(-0.6, -0.3);
      case DinoType.Water: return Phaser.Math.FloatBetween(0.2, 0.5);
      case DinoType.Flora: return Phaser.Math.FloatBetween(-0.2, -0.05);
      case DinoType.Electric: return Phaser.Math.FloatBetween(-0.4, 0.4);
      case DinoType.Ice: return Phaser.Math.FloatBetween(0.1, 0.3);
      default: return Phaser.Math.FloatBetween(-0.3, -0.1);
    }
  }

  private updateTypeParticles(delta: number): void {
    this.particleGfx.clear();

    const update = (particles: TypeParticle[], cx: number, cy: number, type: DinoType) => {
      for (let i = 0; i < particles.length; i++) {
        const p = particles[i];
        p.x += p.vx * (delta / 16);
        p.y += p.vy * (delta / 16);
        p.life++;

        if (p.life >= p.maxLife) {
          // Reset
          p.x = cx + Phaser.Math.FloatBetween(-20, 20);
          p.y = cy + Phaser.Math.FloatBetween(-10, 20);
          p.vy = this.getTypeParticleVY(type);
          p.life = 0;
          continue;
        }

        const alpha = 1 - (p.life / p.maxLife);
        this.particleGfx.fillStyle(p.color, alpha * 0.7);
        this.particleGfx.fillCircle(p.x, p.y, p.size);
      }
    };

    if (!this.battleOver) {
      update(
        this.enemyParticles,
        this.enemyBaseX + this.enemyOffsetX,
        this.enemyBaseY + this.enemyOffsetY,
        this.enemyDino.types[0]
      );
      update(
        this.playerParticles,
        this.playerBaseX + this.playerOffsetX,
        this.playerBaseY + this.playerOffsetY,
        this.playerDino.types[0]
      );
    }
  }

  // ============================================================
  // Dino Drawing
  // ============================================================
  private getDinoSize(level: number): { w: number; h: number; stage: string } {
    if (level < 10) return { w: 48, h: 40, stage: 'baby' };
    if (level < 25) return { w: 64, h: 52, stage: 'young' };
    return { w: 80, h: 68, stage: 'adult' };
  }

  private isCarnivore(types: DinoType[]): boolean {
    return types.some(t => [DinoType.Fire, DinoType.Shadow, DinoType.Venom].includes(t));
  }

  private isQuadruped(types: DinoType[]): boolean {
    return types.some(t => [DinoType.Flora, DinoType.Fossil, DinoType.Earth].includes(t));
  }

  private drawDino(
    gfx: Phaser.GameObjects.Graphics,
    dino: DinoData,
    cx: number,
    cy: number,
    isBackView: boolean
  ): void {
    gfx.clear();
    const size = this.getDinoSize(dino.level);
    const color = DINO_TYPE_COLORS[dino.types[0]] ?? 0xA8A878;
    const bright = Phaser.Display.Color.IntegerToColor(color).brighten(25).color;
    const dark = Phaser.Display.Color.IntegerToColor(color).darken(20).color;
    const w = size.w;
    const h = size.h;
    const carnivore = this.isCarnivore(dino.types);
    const quadruped = this.isQuadruped(dino.types);

    // Shadow under dino
    gfx.fillStyle(0x000000, 0.15);
    gfx.fillEllipse(cx, cy + h * 0.45, w * 0.8, h * 0.2);

    if (isBackView) {
      // ---- BACK VIEW (player's dino, facing right, seen from behind) ----

      // Body
      gfx.fillStyle(color, 1);
      gfx.fillRoundedRect(cx - w * 0.4, cy - h * 0.5, w * 0.8, h * 0.7, w * 0.12);

      // Darker back texture
      gfx.fillStyle(dark, 0.3);
      gfx.fillRoundedRect(cx - w * 0.35, cy - h * 0.45, w * 0.7, h * 0.3, w * 0.1);

      // Tail (prominent in back view)
      this.drawTail(gfx, dino, cx - w * 0.3, cy - h * 0.1, w * 0.5, true, color, dark);

      // Legs
      if (quadruped) {
        // 4 legs from behind
        gfx.fillStyle(dark, 1);
        gfx.fillRoundedRect(cx - w * 0.32, cy + h * 0.1, w * 0.18, h * 0.35, 4);
        gfx.fillRoundedRect(cx - w * 0.1, cy + h * 0.1, w * 0.18, h * 0.35, 4);
        gfx.fillRoundedRect(cx + w * 0.05, cy + h * 0.12, w * 0.16, h * 0.32, 4);
        gfx.fillRoundedRect(cx + w * 0.2, cy + h * 0.12, w * 0.16, h * 0.32, 4);
      } else {
        // 2 legs from behind
        gfx.fillStyle(dark, 1);
        gfx.fillRoundedRect(cx - w * 0.22, cy + h * 0.1, w * 0.2, h * 0.35, 5);
        gfx.fillRoundedRect(cx + w * 0.05, cy + h * 0.1, w * 0.2, h * 0.35, 5);
        // Feet
        gfx.fillStyle(color, 1);
        gfx.fillEllipse(cx - w * 0.12, cy + h * 0.45, w * 0.22, h * 0.08);
        gfx.fillEllipse(cx + w * 0.15, cy + h * 0.45, w * 0.22, h * 0.08);
      }

      // Back of head
      gfx.fillStyle(color, 1);
      gfx.fillCircle(cx + w * 0.1, cy - h * 0.5, w * 0.2);
      gfx.fillStyle(dark, 0.3);
      gfx.fillCircle(cx + w * 0.1, cy - h * 0.5, w * 0.15);

      // Spines/ridges on back
      gfx.fillStyle(dark, 0.6);
      for (let r = 0; r < 3; r++) {
        const rx = cx - w * 0.15 + r * w * 0.12;
        const ry = cy - h * 0.5 + r * h * 0.05;
        gfx.fillTriangle(rx, ry, rx + 4, ry - 8, rx + 8, ry);
      }

    } else {
      // ---- FRONT VIEW (enemy dino, facing left toward player) ----

      // Body
      gfx.fillStyle(color, 1);
      gfx.fillRoundedRect(cx - w * 0.4, cy - h * 0.4, w * 0.8, h * 0.65, w * 0.12);

      // Lighter belly
      gfx.fillStyle(bright, 1);
      gfx.fillEllipse(cx, cy + h * 0.05, w * 0.5, h * 0.35);

      // Head
      const headX = cx - w * 0.15;
      const headY = cy - h * 0.55;
      const headR = w * 0.22;
      gfx.fillStyle(color, 1);

      if (carnivore) {
        // Angular head for carnivores
        gfx.fillRoundedRect(headX - headR, headY - headR * 0.8, headR * 2.2, headR * 1.8, headR * 0.3);
        // Teeth
        gfx.fillStyle(0xF0F0F0, 0.9);
        const jawY = headY + headR * 0.5;
        for (let t = 0; t < 4; t++) {
          const tx = headX - headR * 0.6 + t * headR * 0.4;
          gfx.fillTriangle(tx, jawY, tx + 3, jawY + 5, tx + 6, jawY);
        }
      } else {
        // Rounder head for herbivores
        gfx.fillCircle(headX, headY, headR);
      }

      // Eyes
      const eyeX = headX - headR * 0.3;
      const eyeY = headY - headR * 0.1;
      const eyeR = Math.max(3, headR * 0.25);

      // Eye sclera
      gfx.fillStyle(0xFFFFFF, 1);
      gfx.fillCircle(eyeX, eyeY, eyeR);
      // Iris (type-colored)
      gfx.fillStyle(DINO_TYPE_COLORS[dino.types[0]], 1);
      gfx.fillCircle(eyeX - eyeR * 0.15, eyeY, eyeR * 0.6);
      // Pupil
      gfx.fillStyle(0x101010, 1);
      gfx.fillCircle(eyeX - eyeR * 0.2, eyeY, eyeR * 0.35);
      // Eye highlight
      gfx.fillStyle(0xFFFFFF, 0.8);
      gfx.fillCircle(eyeX - eyeR * 0.35, eyeY - eyeR * 0.3, eyeR * 0.2);

      // Legs
      if (quadruped) {
        // 4 legs
        gfx.fillStyle(dark, 1);
        gfx.fillRoundedRect(cx - w * 0.35, cy + h * 0.15, w * 0.16, h * 0.3, 4);
        gfx.fillRoundedRect(cx - w * 0.15, cy + h * 0.15, w * 0.16, h * 0.3, 4);
        gfx.fillRoundedRect(cx + w * 0.05, cy + h * 0.17, w * 0.14, h * 0.28, 4);
        gfx.fillRoundedRect(cx + w * 0.2, cy + h * 0.17, w * 0.14, h * 0.28, 4);
        // Feet/claws
        gfx.fillStyle(color, 1);
        for (let f = 0; f < 4; f++) {
          const fx = cx - w * 0.32 + f * w * 0.2;
          gfx.fillEllipse(fx + w * 0.07, cy + h * 0.45, w * 0.12, h * 0.06);
        }
      } else {
        // 2 bipedal legs
        gfx.fillStyle(dark, 1);
        gfx.fillRoundedRect(cx - w * 0.25, cy + h * 0.15, w * 0.2, h * 0.3, 5);
        gfx.fillRoundedRect(cx + w * 0.08, cy + h * 0.15, w * 0.2, h * 0.3, 5);
        // Feet with claws
        gfx.fillStyle(color, 1);
        gfx.fillEllipse(cx - w * 0.18, cy + h * 0.45, w * 0.2, h * 0.07);
        gfx.fillEllipse(cx + w * 0.15, cy + h * 0.45, w * 0.2, h * 0.07);
        // Claw marks
        gfx.fillStyle(dark, 0.7);
        for (let c = 0; c < 2; c++) {
          const footCx = c === 0 ? cx - w * 0.18 : cx + w * 0.15;
          for (let cl = 0; cl < 3; cl++) {
            const clx = footCx - 5 + cl * 5;
            gfx.fillTriangle(clx, cy + h * 0.47, clx + 2, cy + h * 0.52, clx + 4, cy + h * 0.47);
          }
        }
      }

      // Small arms (bipedal only)
      if (!quadruped) {
        gfx.fillStyle(color, 1);
        gfx.fillRect(cx - w * 0.42, cy - h * 0.08, w * 0.1, h * 0.15);
        gfx.fillRect(cx + w * 0.32, cy - h * 0.08, w * 0.1, h * 0.15);
        // Claws
        gfx.fillStyle(dark, 0.8);
        gfx.fillRect(cx - w * 0.44, cy + h * 0.05, 3, 4);
        gfx.fillRect(cx + w * 0.4, cy + h * 0.05, 3, 4);
      }

      // Tail
      this.drawTail(gfx, dino, cx + w * 0.35, cy - h * 0.05, w * 0.45, false, color, dark);

      // Type-specific features on body
      this.drawTypeFeatures(gfx, dino, cx, cy, w, h);
    }
  }

  private drawTail(
    gfx: Phaser.GameObjects.Graphics,
    dino: DinoData,
    x: number,
    y: number,
    length: number,
    isBack: boolean,
    color: number,
    dark: number
  ): void {
    const type = dino.types[0];
    const dir = isBack ? -1 : 1;

    // Base tail shape
    gfx.fillStyle(color, 1);
    gfx.fillTriangle(
      x, y - length * 0.2,
      x + dir * length, y - length * 0.3,
      x, y + length * 0.2
    );

    // Tail tip varies by type
    const tipX = x + dir * length;
    const tipY = y - length * 0.3;

    switch (type) {
      case DinoType.Fire:
        // Flame tip
        gfx.fillStyle(0xF8D830, 0.9);
        gfx.fillCircle(tipX, tipY, 6);
        gfx.fillStyle(0xF08030, 0.8);
        gfx.fillCircle(tipX + dir * 3, tipY - 4, 4);
        gfx.fillStyle(0xF8D830, 0.6);
        gfx.fillCircle(tipX - dir * 2, tipY - 2, 3);
        break;
      case DinoType.Water:
        // Fin
        gfx.fillStyle(dark, 0.8);
        gfx.fillTriangle(tipX, tipY - 8, tipX + dir * 10, tipY, tipX, tipY + 8);
        break;
      case DinoType.Flora:
        // Leaf
        gfx.fillStyle(0x48A030, 1);
        gfx.fillEllipse(tipX + dir * 4, tipY, 10, 6);
        gfx.lineStyle(1, 0x306820, 0.7);
        gfx.lineBetween(tipX, tipY, tipX + dir * 8, tipY);
        break;
      case DinoType.Fossil:
      case DinoType.Earth:
        // Club
        gfx.fillStyle(dark, 1);
        gfx.fillCircle(tipX, tipY, 7);
        break;
      case DinoType.Electric:
        // Zigzag
        gfx.fillStyle(0xF8D030, 1);
        gfx.fillTriangle(tipX, tipY - 4, tipX + dir * 6, tipY, tipX, tipY + 4);
        gfx.fillTriangle(tipX + dir * 4, tipY - 6, tipX + dir * 10, tipY - 2, tipX + dir * 4, tipY + 2);
        break;
      default:
        // Simple taper (already done by base triangle)
        break;
    }
  }

  private drawTypeFeatures(
    gfx: Phaser.GameObjects.Graphics,
    dino: DinoData,
    cx: number,
    cy: number,
    w: number,
    h: number
  ): void {
    const type = dino.types[0];

    switch (type) {
      case DinoType.Flora:
        // Small leaves on body
        gfx.fillStyle(0x48A030, 0.8);
        gfx.fillTriangle(cx - 6, cy - h * 0.35, cx - 2, cy - h * 0.5, cx + 4, cy - h * 0.35);
        gfx.fillTriangle(cx + 6, cy - h * 0.32, cx + 10, cy - h * 0.48, cx + 14, cy - h * 0.32);
        // Flower
        gfx.fillStyle(0xE85080, 0.8);
        gfx.fillCircle(cx + 2, cy - h * 0.52, 4);
        break;
      case DinoType.Fossil:
      case DinoType.Earth:
        // Rocky patches
        gfx.fillStyle(0x000000, 0.15);
        gfx.fillCircle(cx - w * 0.15, cy - h * 0.1, 5);
        gfx.fillCircle(cx + w * 0.1, cy + h * 0.05, 4);
        gfx.fillCircle(cx + w * 0.2, cy - h * 0.2, 3);
        break;
      case DinoType.Ice:
        // Frost crystals
        gfx.fillStyle(0xC0E8F8, 0.7);
        for (let i = 0; i < 3; i++) {
          const fx = cx - w * 0.2 + i * w * 0.15;
          const fy = cy - h * 0.3 - i * 4;
          gfx.fillTriangle(fx, fy, fx + 3, fy - 8, fx + 6, fy);
          gfx.fillTriangle(fx + 1, fy - 3, fx + 3, fy - 10, fx + 5, fy - 3);
        }
        break;
      case DinoType.Metal:
        // Metallic sheen
        gfx.fillStyle(0xFFFFFF, 0.15);
        gfx.fillRect(cx - w * 0.1, cy - h * 0.3, w * 0.05, h * 0.4);
        gfx.fillRect(cx + w * 0.05, cy - h * 0.25, w * 0.04, h * 0.3);
        break;
    }
  }

  private drawEnemyDino(): void {
    const cx = this.enemyBaseX + this.enemyOffsetX;
    const cy = this.enemyBaseY + this.enemyOffsetY;
    this.drawDino(this.enemyGfx, this.enemyDino, cx, cy, false);
  }

  private drawPlayerDino(): void {
    const cx = this.playerBaseX + this.playerOffsetX;
    const cy = this.playerBaseY + this.playerOffsetY;
    this.drawDino(this.playerGfx, this.playerDino, cx, cy, true);
  }

  // ============================================================
  // Intro Animation
  // ============================================================
  private playIntro(): void {
    this.animating = true;
    this.cameras.main.fadeIn(400);

    // Start dinos off-screen
    this.enemyOffsetX = GAME_WIDTH;
    this.playerOffsetX = -GAME_WIDTH;
    this.drawEnemyDino();
    this.drawPlayerDino();

    // Cinematic letterbox bars during intro
    CameraFX.letterbox(this, true, 250, 30);

    // Slide enemy in from right (bouncy ease)
    this.tweens.add({
      targets: this,
      enemyOffsetX: 0,
      duration: 600,
      ease: 'Back.easeOut',
      onUpdate: () => this.drawEnemyDino(),
    });

    // Slide player in from left (bouncy ease)
    this.tweens.add({
      targets: this,
      playerOffsetX: 0,
      duration: 600,
      delay: 200,
      ease: 'Back.easeOut',
      onUpdate: () => this.drawPlayerDino(),
      onComplete: () => {
        let introText: string;
        if (this.isWild) {
          introText = `Un ${this.enemyDino.name} sauvage apparait !`;
        } else if (this.trainerName) {
          introText = `${this.trainerName} veut se battre !`;
        } else {
          introText = 'Le combat commence !';
        }

        this.hud.showMessage(introText);
        // Remove letterbox bars as intro text appears
        CameraFX.letterbox(this, false, 400, 30);
        this.time.delayedCall(1500, () => {
          if (!this.isWild && this.trainerName) {
            // Show trainer's first dino
            this.hud.showMessage(`${this.trainerName} envoie ${this.enemyDino.name} !`);
            this.time.delayedCall(1200, () => {
              this.hud.hideMessage();
              this.animating = false;
              this.startPlayerTurn();
            });
          } else {
            this.hud.hideMessage();
            this.animating = false;
            this.startPlayerTurn();
          }
        });
      },
    });
  }

  // ============================================================
  // Turn flow
  // ============================================================
  private startPlayerTurn(): void {
    if (this.battleOver) return;
    this.hud.showActionMenu((actionIndex) => {
      this.handleAction(actionIndex);
    });
  }

  private handleAction(index: number): void {
    this.hud.hideMenus();

    switch (index) {
      case 0: // COMBAT
        this.hud.showMoveMenu(
          this.playerDino.moves.map((m) => ({
            name: m.name,
            type: m.type,
            category: m.category,
            power: m.power,
            pp: m.pp,
            maxPp: m.maxPp,
          })),
          (moveIndex) => {
            this.executePlayerMove(moveIndex);
          }
        );
        break;
      case 1: // SAC
        this.handleBag();
        break;
      case 2: // DINOS
        this.hud.showMessage('Vous n\'avez qu\'un seul dino !');
        this.time.delayedCall(1200, () => {
          this.hud.hideMessage();
          this.startPlayerTurn();
        });
        break;
      case 3: // FUITE
        this.attemptFlee();
        break;
    }
  }

  // ============================================================
  // Bag handling
  // ============================================================
  private handleBag(): void {
    const gs = GameState.getInstance();
    const inv = gs.getInventorySystem();

    // Gather usable battle items from inventory
    const battleItems: { itemId: number; name: string; quantity: number }[] = [];
    const rawMap = inv.getRawMap();
    for (const [itemId, qty] of rawMap) {
      if (qty <= 0) continue;
      if (hasItemData(itemId)) {
        const item = getItem(itemId);
        if (item.usableInBattle) {
          battleItems.push({ itemId, name: item.name, quantity: qty });
        }
      } else {
        // Fallback for items without registry data — show with generic name
        const fallbackNames: Record<number, string> = { 1: 'Potion', 2: 'Super Potion', 16: 'Jurassic Ball' };
        const name = fallbackNames[itemId] ?? `Objet #${itemId}`;
        battleItems.push({ itemId, name, quantity: qty });
      }
    }

    if (battleItems.length === 0) {
      this.hud.showMessage('Le sac est vide !');
      this.time.delayedCall(1200, () => {
        this.hud.hideMessage();
        this.startPlayerTurn();
      });
      return;
    }

    // Show items as move-like buttons (reuse move menu)
    const moveInfos = battleItems.slice(0, 4).map(bi => ({
      name: `${bi.name} x${bi.quantity}`,
      type: DinoType.Normal as DinoType,
      power: 0,
      pp: bi.quantity,
      maxPp: bi.quantity,
      category: 'status' as const,
    }));

    this.hud.showMoveMenu(moveInfos, (itemIndex) => {
      this.hud.hideMenus();
      const selected = battleItems[itemIndex];
      if (!selected) {
        this.startPlayerTurn();
        return;
      }

      // Try to use the item
      const party = gs.getPartySystem().getParty();
      const lead = party.length > 0 ? party[0] : null;

      // For capture balls, handle capture flow
      if (hasItemData(selected.itemId)) {
        const itemData = getItem(selected.itemId);
        if (itemData.effect?.type === 'capture') {
          this.attemptCapture(selected.itemId, itemData.effect.value ?? 1.0);
          return;
        }

        const result = inv.useItem(selected.itemId, lead ?? undefined);
        if (result.success) {
          this.hud.showMessage(result.message);
          // Sync HP to battle state
          if (lead) {
            this.playerDino.currentHp = lead.currentHp;
            this.playerDino.maxHp = lead.maxHp;
            this.hud.updatePlayerHP(lead.currentHp, lead.maxHp, true);
          }
          this.time.delayedCall(1500, () => {
            this.hud.hideMessage();
            // Enemy gets a turn after using an item
            this.doEnemyTurn(() => {
              if (this.checkFaint(this.playerDino, 'player')) return;
              this.animating = false;
              this.startPlayerTurn();
            });
          });
        } else {
          this.hud.showMessage(result.message);
          this.time.delayedCall(1200, () => {
            this.hud.hideMessage();
            this.startPlayerTurn();
          });
        }
      } else {
        // Items without registry — apply basic healing for potions
        if (selected.itemId === 1 && lead) {
          // Potion: heal 20 HP
          const healed = lead.heal(20);
          inv.removeItem(selected.itemId);
          this.playerDino.currentHp = lead.currentHp;
          this.hud.updatePlayerHP(lead.currentHp, lead.maxHp, true);
          this.hud.showMessage(`${lead.nickname} recupere ${healed} PV !`);
          this.time.delayedCall(1500, () => {
            this.hud.hideMessage();
            this.doEnemyTurn(() => {
              if (this.checkFaint(this.playerDino, 'player')) return;
              this.animating = false;
              this.startPlayerTurn();
            });
          });
        } else {
          this.hud.showMessage('Impossible d\'utiliser cet objet ici.');
          this.time.delayedCall(1200, () => {
            this.hud.hideMessage();
            this.startPlayerTurn();
          });
        }
      }
    });
  }

  // ============================================================
  // Combat execution
  // ============================================================
  private executePlayerMove(moveIndex: number): void {
    this.hud.hideMenus();
    this.animating = true;

    const move = this.playerDino.moves[moveIndex];
    if (move.pp <= 0) {
      this.hud.showMessage('Plus de PP pour cette attaque !');
      this.time.delayedCall(1000, () => {
        this.hud.hideMessage();
        this.animating = false;
        this.startPlayerTurn();
      });
      return;
    }

    move.pp--;

    // Sync PP back to GameState
    const gs = GameState.getInstance();
    const party = gs.getPartySystem().getParty();
    if (party.length > 0) {
      const lead = party[0];
      const moveIdx = this.playerDino.moves.indexOf(move);
      if (moveIdx >= 0 && lead.moves[moveIdx]) {
        lead.moves[moveIdx].currentPP = move.pp;
      }
    }

    // Determine turn order by speed
    const playerFirst = (this.playerDino.speed ?? 10) >= (this.enemyDino.speed ?? 8);

    if (playerFirst) {
      this.doAttack(this.playerDino, this.enemyDino, move, 'player', () => {
        if (this.checkFaint(this.enemyDino, 'enemy')) return;
        this.doEnemyTurn(() => {
          if (this.checkFaint(this.playerDino, 'player')) return;
          this.animating = false;
          this.startPlayerTurn();
        });
      });
    } else {
      this.doEnemyTurn(() => {
        if (this.checkFaint(this.playerDino, 'player')) return;
        this.doAttack(this.playerDino, this.enemyDino, move, 'player', () => {
          if (this.checkFaint(this.enemyDino, 'enemy')) return;
          this.animating = false;
          this.startPlayerTurn();
        });
      });
    }
  }

  /**
   * Enemy AI with 3 difficulty tiers based on enemy level:
   * - Lv 1-10  (Beginner): 60% random, 40% best move. Often wastes turns on status.
   * - Lv 11-30 (Competent): 30% random, 70% best move. Uses status smartly.
   * - Lv 31+   (Expert): 10% random, 90% best move. Exploits type advantages.
   */
  private doEnemyTurn(callback: () => void): void {
    const moves = this.enemyDino.moves.filter((m) => m.pp > 0);
    if (moves.length === 0) {
      this.hud.showMessage(`${this.enemyDino.name} n'a plus d'attaques !`);
      this.time.delayedCall(1000, () => {
        this.hud.hideMessage();
        callback();
      });
      return;
    }

    const level = this.enemyDino.level;
    let move: MoveData;

    // Determine AI skill: chance to pick the optimal move vs random
    const smartChance = level <= 10 ? 0.35 : level <= 30 ? 0.65 : 0.90;

    if (Math.random() < smartChance) {
      // Smart pick: score each move and pick the best
      move = this.pickBestEnemyMove(moves);
    } else {
      // Dumb pick: fully random
      move = Phaser.Utils.Array.GetRandom(moves);

      // Beginner AI: actively prefer weaker/status moves (plays badly)
      if (level <= 10) {
        const weakMoves = moves.filter(m => m.power <= 0 || m.power < 50);
        if (weakMoves.length > 0 && Math.random() < 0.5) {
          move = Phaser.Utils.Array.GetRandom(weakMoves);
        }
      }
    }

    move.pp--;
    this.doAttack(this.enemyDino, this.playerDino, move, 'enemy', callback);
  }

  /** Pick the best move based on type effectiveness and power */
  private pickBestEnemyMove(moves: MoveData[]): MoveData {
    let bestMove = moves[0];
    let bestScore = -1;

    for (const m of moves) {
      if (m.power === 0) {
        // Status moves: low score, but small bonus if player has high attack
        const score = (this.playerDino.attack ?? 10) > 15 ? 15 : 5;
        if (score > bestScore) { bestScore = score; bestMove = m; }
      } else {
        const eff = getTypeEffectiveness(m.type, this.playerDino.types);
        const stab = this.enemyDino.types.includes(m.type) ? STAB_MULTIPLIER : 1;
        const score = m.power * eff * stab;
        if (score > bestScore) { bestScore = score; bestMove = m; }
      }
    }

    return bestMove;
  }

  private doAttack(
    attacker: DinoData,
    defender: DinoData,
    move: MoveData,
    attackerSide: 'player' | 'enemy',
    callback: () => void
  ): void {
    this.hud.showMessage(`${attacker.name} utilise ${move.name} !`);

    this.time.delayedCall(800, () => {
      if (move.power === 0) {
        // Status move — apply stat effect based on move name
        const target = attackerSide === 'player' ? defender : attacker;
        const targetName = target.name;
        let statusMsg = `${attacker.name} utilise ${move.name} !`;

        // Grondement / moves that lower enemy attack
        if (move.name === 'Grondement' || move.name === 'Rugissement') {
          const defSide = attackerSide === 'player' ? defender : attacker;
          const newAtk = Math.max(1, Math.floor((defSide.attack ?? 10) * 0.8));
          defSide.attack = newAtk;
          statusMsg = `L'attaque de ${targetName} baisse !`;
        }
        // Endurance / moves that raise own defense
        else if (move.name === 'Endurance') {
          const newDef = Math.floor((attacker.defense ?? 10) * 1.25);
          attacker.defense = newDef;
          statusMsg = `La defense de ${attacker.name} monte !`;
        }
        // Generic fallback
        else {
          statusMsg = `${attacker.name} utilise ${move.name} !`;
        }

        this.hud.showMessage(statusMsg);
        this.time.delayedCall(1000, () => {
          this.hud.hideMessage();
          callback();
        });
        return;
      }

      // Calculate damage
      const result = this.calculateDamage(attacker, defender, move);

      // Attack lunge animation
      this.animateAttackLunge(attackerSide, () => {
        // Hit animation
        this.animateHit(attackerSide === 'player' ? 'enemy' : 'player', () => {
          defender.currentHp = Math.max(0, defender.currentHp - result.damage);

          // Play hit SFX
          const hitAudio = AudioSystem.getInstance();
          if (result.isCritical) {
            hitAudio.playCritical();
          } else {
            hitAudio.playHit();
          }

          // --- Battle FX: type-specific particles at defender position ---
          const defSide = attackerSide === 'player' ? 'enemy' : 'player';
          const defX = defSide === 'enemy'
            ? this.enemyBaseX + this.enemyOffsetX
            : this.playerBaseX + this.playerOffsetX;
          const defY = defSide === 'enemy'
            ? this.enemyBaseY + this.enemyOffsetY
            : this.playerBaseY + this.playerOffsetY;

          // Emit type-matched particles
          const particleConfig = getTypeParticleConfig(move.type, defX, defY);
          if (result.effectiveness >= 2) {
            // Super effective: double the particle count + smooth shake
            particleConfig.count = Math.floor(particleConfig.count * 2);
            this.battleParticlePool.emit(particleConfig);
            CameraFX.smoothShake(this, 6, 350);
          } else if (result.isCritical) {
            // Critical: normal particles + zoom punch
            this.battleParticlePool.emit(particleConfig);
            CameraFX.zoomPunch(this, 1.08, 250);
          } else {
            // Normal hit
            this.battleParticlePool.emit(particleConfig);
          }

          // Show floating damage number
          if (result.damage > 0) {
            const dmgText = this.add.text(defX, defY - 20, `-${result.damage}`, {
              fontFamily: FONT_FAMILY,
              fontSize: result.effectiveness >= 2 ? '18px' : result.isCritical ? '16px' : '14px',
              color: result.effectiveness >= 2 ? '#F8D830' : result.isCritical ? '#F8A030' : '#FFFFFF',
              stroke: '#000000',
              strokeThickness: 3,
            }).setOrigin(0.5).setDepth(200);

            this.tweens.add({
              targets: dmgText,
              y: dmgText.y - 30,
              alpha: 0,
              duration: 1200,
              ease: 'Power2',
              onComplete: () => dmgText.destroy(),
            });
          }

          // Animate HP drain
          const target = attackerSide === 'player' ? 'enemy' : 'player';
          this.hud.animateHPDrain(
            target,
            defender.currentHp + result.damage,
            defender.currentHp,
            500
          ).then(() => {
            // Show effectiveness messages
            this.showEffectivenessMessages(result, () => {
              this.hud.hideMessage();
              callback();
            });
          });
        });
      });
    });
  }

  private calculateDamage(
    attacker: DinoData,
    defender: DinoData,
    move: MoveData
  ): { damage: number; effectiveness: number; isCritical: boolean } {
    if (move.power === 0) return { damage: 0, effectiveness: 1, isCritical: false };

    const isPhysical = move.category === 'physical';
    const atkStat = isPhysical ? (attacker.attack ?? 10) : (attacker.spAttack ?? 10);
    const defStat = isPhysical ? (defender.defense ?? 10) : (defender.spDefense ?? 10);
    const level = attacker.level;

    // Base formula
    let baseDmg = ((2 * level / 5 + 2) * move.power * (atkStat / defStat)) / 50 + 2;

    // STAB
    const hasStab = attacker.types.includes(move.type);
    if (hasStab) {
      baseDmg *= STAB_MULTIPLIER;
    }

    // Type effectiveness — move type vs defender types ONLY
    const effectiveness = getTypeEffectiveness(move.type, defender.types);
    baseDmg *= effectiveness;

    // Critical hit
    const isCritical = Phaser.Math.Between(1, 100) <= CRITICAL_HIT_CHANCE;
    if (isCritical) {
      baseDmg *= CRITICAL_HIT_MULTIPLIER;
    }

    // Random factor
    const rand = Phaser.Math.FloatBetween(DAMAGE_RANDOM_MIN, DAMAGE_RANDOM_MAX);
    baseDmg *= rand;

    return {
      damage: Math.max(1, Math.floor(baseDmg)),
      effectiveness,
      isCritical,
    };
  }

  // ============================================================
  // Attack animations
  // ============================================================
  private animateAttackLunge(side: 'player' | 'enemy', callback: () => void): void {
    const prop = side === 'player' ? 'playerOffsetX' : 'enemyOffsetX';
    const dir = side === 'player' ? 1 : -1;
    const drawFn = side === 'player'
      ? () => this.drawPlayerDino()
      : () => this.drawEnemyDino();

    this.tweens.add({
      targets: this,
      [prop]: dir * 12,
      duration: 100,
      ease: 'Power2',
      yoyo: true,
      onUpdate: drawFn,
      onComplete: () => {
        (this as any)[prop] = 0;
        drawFn();
        callback();
      },
    });
  }

  private animateHit(targetSide: 'player' | 'enemy', callback: () => void): void {
    const targetGfx = targetSide === 'player' ? this.playerGfx : this.enemyGfx;

    // Screen flash
    TransitionFX.flashScreen(this, COLORS.WHITE, 80);

    // Flash target 3 times
    this.tweens.add({
      targets: targetGfx,
      alpha: 0,
      duration: 80,
      yoyo: true,
      repeat: 2,
      onComplete: () => {
        targetGfx.setAlpha(1);
        // Screen shake
        this.cameras.main.shake(200, 0.008);
        callback();
      },
    });
  }

  private showEffectivenessMessages(
    result: { damage: number; effectiveness: number; isCritical: boolean },
    callback: () => void
  ): void {
    const messages: string[] = [];

    if (result.isCritical) {
      messages.push('Coup critique !');
    }
    if (result.effectiveness >= 2) {
      messages.push('C\'est super efficace !');
    } else if (result.effectiveness > 0 && result.effectiveness < 1) {
      messages.push('Ce n\'est pas tres efficace...');
    } else if (result.effectiveness === 0) {
      messages.push('Aucun effet...');
    }

    if (messages.length === 0) {
      callback();
      return;
    }

    // Show messages sequentially
    let idx = 0;
    const showNext = () => {
      if (idx >= messages.length) {
        callback();
        return;
      }
      this.hud.showMessage(messages[idx]);
      if (messages[idx].includes('super efficace')) {
        TransitionFX.flashScreen(this, 0xF8D830, 100);
        AudioSystem.getInstance().playSuperEffective();
      } else if (messages[idx].includes('pas tres efficace')) {
        AudioSystem.getInstance().playNotEffective();
      }
      idx++;
      this.time.delayedCall(1000, showNext);
    };
    showNext();
  }

  // ============================================================
  // Faint / End
  // ============================================================
  private checkFaint(dino: DinoData, side: 'player' | 'enemy'): boolean {
    if (dino.currentHp > 0) return false;

    this.hud.hideMenus();

    // Faint animation: slide down and fade
    const gfx = side === 'enemy' ? this.enemyGfx : this.playerGfx;
    const offsetProp = side === 'enemy' ? 'enemyOffsetY' : 'playerOffsetY';
    const drawFn = side === 'enemy'
      ? () => this.drawEnemyDino()
      : () => this.drawPlayerDino();

    this.tweens.add({
      targets: this,
      [offsetProp]: 60,
      duration: 500,
      ease: 'Power2',
      onUpdate: drawFn,
    });
    this.tweens.add({
      targets: gfx,
      alpha: 0,
      duration: 500,
      ease: 'Power2',
    });

    // Battle FX: flash + clear particles on faint
    CameraFX.flashColor(this, 0xFFFFFF, 200);
    this.battleParticlePool.clear();

    this.hud.showMessage(`${dino.name} est K.O. !`);
    AudioSystem.getInstance().playFaint();

    if (side === 'enemy') {
      // Check if trainer has more dinos
      if (!this.isWild && this.trainerParty.length > 0) {
        this.trainerPartyIndex++;
        if (this.trainerPartyIndex < this.trainerParty.length) {
          // Trainer sends next dino
          this.time.delayedCall(1500, () => {
            this.sendTrainerNextDino();
          });
          return true;
        }
      }

      // All enemy dinos defeated — victory
      this.battleOver = true;

      this.time.delayedCall(1500, () => {
        AudioSystem.getInstance().playVictoryMusic();
        if (!this.isWild && this.trainerName) {
          this.hud.showMessage(`Vous avez battu ${this.trainerName} !`);
        } else {
          this.hud.showMessage('Vous avez gagne !');
        }
        this.time.delayedCall(1000, () => {
          // XP gain
          const xpGain = dino.level * 8 + 20;
          this.hud.showMessage(`Vous gagnez ${xpGain} points d'exp !`);
          this.hud.updateXP(xpGain, 100, true);

          this.time.delayedCall(1500, () => {
            // Trainer rewards
            if (!this.isWild) {
              this.handleTrainerVictory();
            } else {
              // Wild: simplified level-up check
              if (xpGain >= 50) {
                const newLevel = this.playerDino.level + 1;
                this.hud.showMessage(`${this.playerDino.name} monte au Nv. ${newLevel} !`);
                TransitionFX.flashScreen(this, 0xF8D830, 200);
                this.time.delayedCall(2000, () => {
                  this.endBattle(true);
                });
              } else {
                this.time.delayedCall(1000, () => {
                  this.endBattle(true);
                });
              }
            }
          });
        });
      });
    } else {
      this.battleOver = true;
      this.time.delayedCall(1500, () => {
        // Heal all party dinos on loss
        const gs = GameState.getInstance();
        const party = gs.getPartySystem().getParty();
        for (const d of party) {
          d.fullHeal();
        }

        // For story/trainer battles: mark trainer as defeated so the event won't loop
        if (!this.isWild && this.trainerId) {
          gs.defeatTrainer(this.trainerId);
        }

        // Lose half your money on blackout (Pokemon standard)
        const moneyLost = Math.floor(gs.player.money / 2);
        if (moneyLost > 0) {
          gs.player.money -= moneyLost;
        }

        this.hud.showMessage('Votre dino est K.O. !');
        this.time.delayedCall(1200, () => {
          if (moneyLost > 0) {
            this.hud.showMessage(`Vous avez perdu ${moneyLost} Poke-Dollars dans la panique !`);
          } else {
            this.hud.showMessage('Un PNJ soigne votre equipe. L\'aventure continue !');
          }
          this.time.delayedCall(2000, () => {
            this.hud.showMessage('Un PNJ soigne votre equipe. L\'aventure continue !');
            this.time.delayedCall(1500, () => {
              this.endBattle(false);
            });
          });
        });
      });
    }

    return true;
  }

  /**
   * Build a DinoData from a TrainerDino using real species stats when possible.
   */
  private buildTrainerDino(td: TrainerDino): DinoData {
    let hp: number, attack: number, defense: number, spAttack: number, spDefense: number, speed: number;
    try {
      const species = getSpecies(td.speciesId);
      const base = species.baseStats;
      const gv = 15; // average GV for trainers
      const lv = td.level;
      hp = Math.floor(((2 * base.hp + gv) * lv) / 100 + lv + 10);
      attack = Math.floor(((2 * base.attack + gv) * lv) / 100 + 5);
      defense = Math.floor(((2 * base.defense + gv) * lv) / 100 + 5);
      spAttack = Math.floor(((2 * base.spAttack + gv) * lv) / 100 + 5);
      spDefense = Math.floor(((2 * base.spDefense + gv) * lv) / 100 + 5);
      speed = Math.floor(((2 * base.speed + gv) * lv) / 100 + 5);
    } catch (_e) {
      // Fallback if species not found
      const lv = td.level;
      hp = Math.floor(((2 * 50 + 15) * lv) / 100 + lv + 10);
      attack = Math.floor(((2 * 50 + 15) * lv) / 100 + 5);
      defense = Math.floor(((2 * 50 + 15) * lv) / 100 + 5);
      spAttack = Math.floor(((2 * 50 + 15) * lv) / 100 + 5);
      spDefense = Math.floor(((2 * 50 + 15) * lv) / 100 + 5);
      speed = Math.floor(((2 * 50 + 15) * lv) / 100 + 5);
    }
    return {
      name: td.name,
      level: td.level,
      types: td.types as DinoType[],
      currentHp: hp,
      maxHp: hp,
      attack,
      defense,
      spAttack,
      spDefense,
      speed,
      moves: td.moves.map(m => ({
        name: m.name,
        type: m.type as DinoType,
        category: (m.category as 'physical' | 'special' | 'status') ?? 'physical',
        power: m.power,
        pp: m.pp,
        maxPp: m.maxPp,
      })),
    };
  }

  /** Send trainer's next dino after one faints */
  private sendTrainerNextDino(): void {
    const nextDino = this.trainerParty[this.trainerPartyIndex];
    this.enemyDino = this.buildTrainerDino(nextDino);

    this.hud.showMessage(`${this.trainerName} envoie ${this.enemyDino.name} !`);

    // Reset enemy graphics
    this.enemyOffsetX = GAME_WIDTH;
    this.enemyOffsetY = 0;
    this.enemyGfx.setAlpha(1);

    // Update HUD
    this.hud.setEnemyDino(
      this.enemyDino.name,
      this.enemyDino.level,
      this.enemyDino.types,
      this.enemyDino.currentHp,
      this.enemyDino.maxHp
    );

    // Reinit particles
    this.enemyParticles = [];
    this.spawnTypeParticles(
      this.enemyParticles,
      this.enemyDino.types[0],
      this.enemyBaseX,
      this.enemyBaseY,
      4
    );

    // Slide in
    this.tweens.add({
      targets: this,
      enemyOffsetX: 0,
      duration: 500,
      ease: 'Power2',
      onUpdate: () => this.drawEnemyDino(),
      onComplete: () => {
        this.time.delayedCall(800, () => {
          this.hud.hideMessage();
          this.animating = false;
          this.startPlayerTurn();
        });
      },
    });
  }

  /** Handle trainer victory: rewards, badges, TMs */
  private handleTrainerVictory(): void {
    const gs = GameState.getInstance();

    // Mark trainer as defeated
    if (this.trainerId) {
      gs.defeatTrainer(this.trainerId);
    }

    // Money reward
    if (this.reward > 0) {
      gs.addMoney(this.reward);
      this.hud.showMessage(`Vous recevez ${this.reward} Poke-Dollars !`);

      this.time.delayedCall(1500, () => {
        // Badge reward (gym leaders)
        if (this.badge !== undefined && this.badge !== null) {
          gs.addBadge(this.badge);
          const badgeNames = [
            'Badge Feuille', 'Badge Vague', 'Badge Pierre', 'Badge Flamme',
            'Badge Givre', 'Badge Eclair', 'Badge Toxine', 'Badge Aile',
          ];
          const badgeName = badgeNames[this.badge] ?? `Badge ${this.badge + 1}`;
          this.hud.showMessage(`Vous avez gagne le ${badgeName} !`);
          TransitionFX.flashScreen(this, 0xF8D830, 300);

          this.time.delayedCall(2000, () => {
            // TM reward
            if (this.tmReward) {
              this.hud.showMessage(`Vous recevez la CT ${this.tmReward.name} !`);
              this.time.delayedCall(1500, () => {
                this.endBattle(true);
              });
            } else {
              this.endBattle(true);
            }
          });
        } else {
          // No badge, just end
          this.time.delayedCall(1000, () => {
            this.endBattle(true);
          });
        }
      });
    } else {
      this.time.delayedCall(1000, () => {
        this.endBattle(true);
      });
    }
  }

  // ============================================================
  // Capture flow
  // ============================================================
  private attemptCapture(itemId: number, ballRate: number): void {
    if (!this.isWild) {
      this.hud.showMessage('On ne peut pas capturer le dino d\'un dresseur !');
      this.time.delayedCall(1200, () => {
        this.hud.hideMessage();
        this.startPlayerTurn();
      });
      return;
    }

    this.hud.hideMenus();
    this.animating = true;

    const gs = GameState.getInstance();
    const inv = gs.getInventorySystem();
    inv.removeItem(itemId);

    const enemy = this.enemyDino;
    const hpRatio = enemy.currentHp / enemy.maxHp;

    // Capture formula: lower HP = higher chance; ballRate multiplier
    // Master Ball (value=255) always captures
    let catchRate: number;
    if (ballRate >= 255) {
      catchRate = 1.0;
    } else {
      // Base catch rate influenced by HP remaining and ball multiplier
      const baseRate = 0.3 * ballRate;
      const hpBonus = (1 - hpRatio) * 0.5; // up to +0.5 at 0 HP
      catchRate = Math.min(0.95, baseRate * (0.5 + hpBonus));
    }

    this.hud.showMessage(`Vous lancez une ball !`);

    // Determine number of shakes (1-4)
    const shakes = ballRate >= 255 ? 4 : this.calculateShakes(catchRate);
    const captured = shakes >= 4;

    // Animate shakes one by one
    let delay = 1000;
    for (let i = 0; i < shakes && i < 4; i++) {
      this.time.delayedCall(delay, () => {
        // Shake effect: rock enemy sprite left-right
        this.tweens.add({
          targets: this,
          enemyOffsetX: -8,
          duration: 150,
          yoyo: true,
          repeat: 1,
          ease: 'Sine.easeInOut',
          onUpdate: () => this.drawEnemyDino(),
          onComplete: () => {
            this.enemyOffsetX = 0;
            this.drawEnemyDino();
          },
        });
      });
      delay += 800;
    }

    // After all shakes, show result
    this.time.delayedCall(delay, () => {
      if (captured) {
        // Success!
        this.battleOver = true;
        AudioSystem.getInstance().playVictoryMusic();

        // Fade out enemy
        this.tweens.add({
          targets: this.enemyGfx,
          alpha: 0,
          duration: 400,
          ease: 'Power2',
        });

        this.hud.showMessage(`GOTCHA ! ${enemy.name} a ete capture !`);

        // Add to party
        const speciesId = this.getEnemySpeciesId();
        if (speciesId > 0) {
          const capturedDino = Dino.createWild(speciesId, enemy.level);
          capturedDino.currentHp = enemy.currentHp;
          const destination = gs.getPartySystem().addToParty(capturedDino);
          gs.catchDino(speciesId);

          this.time.delayedCall(2000, () => {
            if (destination === 'storage') {
              this.hud.showMessage(`${enemy.name} a ete envoye au stockage !`);
            } else {
              this.hud.showMessage(`${enemy.name} a rejoint votre equipe !`);
            }
            this.time.delayedCall(1500, () => {
              this.endBattle(true);
            });
          });
        } else {
          this.time.delayedCall(2000, () => {
            this.endBattle(true);
          });
        }
      } else {
        // Failure
        this.hud.showMessage(`Oh non ! ${enemy.name} s'est echappe !`);
        this.time.delayedCall(1500, () => {
          this.hud.hideMessage();
          // Enemy gets a turn after failed capture
          this.doEnemyTurn(() => {
            if (this.checkFaint(this.playerDino, 'player')) return;
            this.animating = false;
            this.startPlayerTurn();
          });
        });
      }
    });
  }

  /**
   * Calculate number of shakes for a capture attempt (0-4).
   * 4 shakes = captured. Fewer = escaped.
   */
  private calculateShakes(catchRate: number): number {
    let shakes = 0;
    for (let i = 0; i < 4; i++) {
      if (Math.random() < catchRate) {
        shakes++;
      } else {
        break;
      }
    }
    return shakes;
  }

  private attemptFlee(): void {
    this.hud.hideMenus();
    this.animating = true;

    if (!this.isWild) {
      this.hud.showMessage('Impossible de fuir un combat de dresseur !');
      this.time.delayedCall(1200, () => {
        this.hud.hideMessage();
        this.animating = false;
        this.startPlayerTurn();
      });
      return;
    }

    // For prototype: always succeeds
    this.hud.showMessage('Vous prenez la fuite !');
    this.time.delayedCall(1000, () => {
      this.endBattle(false);
    });
  }

  private endBattle(victory: boolean): void {
    // Stop battle music
    AudioSystem.getInstance().stopMusic();

    const gs = GameState.getInstance();
    const party = gs.getPartySystem().getParty();

    if (victory) {
      // Sync player dino HP + apply XP
      if (party.length > 0) {
        const lead = party[0];
        lead.currentHp = Math.max(0, this.playerDino.currentHp);
        const xpGain = this.enemyDino.level * 8 + 20;
        lead.gainXp(xpGain);
      }
    } else {
      // DEFEAT — heal all dinos to full and respawn at last heal point
      for (const d of party) {
        d.fullHeal();
      }
    }

    // Mark dino as seen in dinodex
    if (this.isWild && this.enemyDino) {
      gs.seeDino(this.getEnemySpeciesId());
    }

    TransitionFX.fadeOut(this, 600, () => {
      if (!victory) {
        // BLACKOUT: return to last heal point with full HP
        const heal = gs.getHealPoint();
        this.scene.start(SCENE_KEYS.OVERWORLD, {
          hasStarter: true,
          mapId: heal.mapId,
          playerX: heal.x,
          playerY: heal.y,
          blackout: true,
        });
        return;
      }

      // Victory: return to where the battle started
      const returnPayload = {
        hasStarter: true,
        ...this.returnData,
        battleResult: 'win',
      };
      this.scene.start(this.returnScene, returnPayload);
    });
  }

  /** Get the species ID for the enemy dino (best effort) */
  private getEnemySpeciesId(): number {
    // For trainer battles, use the trainer party data
    if (!this.isWild && this.trainerParty.length > 0) {
      const idx = Math.min(this.trainerPartyIndex, this.trainerParty.length - 1);
      return this.trainerParty[idx].speciesId;
    }
    // For wild battles, try to match by name — fallback to 0
    return (this.returnData as any)?.wildSpeciesId ?? 0;
  }

  // ============================================================
  // Update
  // ============================================================
  update(_time: number, delta: number): void {
    this.hud.update();
    this.updateTypeParticles(delta);

    // Battle FX particle pool (hit particles, super-effective bursts, etc.)
    this.battleParticlePool.update(delta);
    this.battleParticlePool.render();
  }
}
