// ============================================================
// Jurassic Trainers -- Evolution Scene
// ============================================================

import Phaser from 'phaser';
import {
  SCENE_KEYS,
  GAME_WIDTH,
  GAME_HEIGHT,
  COLORS,
  FONT_FAMILY,
  FONT_SMALL,
  FONT_BODY,
  FONT_TINY,
  DinoType,
  DINO_TYPE_COLORS,
} from '../utils/constants';
import { GameState } from '../systems/GameState';
import { Dino, getSpecies, hasSpecies } from '../entities/Dino';

// ===================== Scene =====================

export class EvolutionScene extends Phaser.Scene {
  private graphics!: Phaser.GameObjects.Graphics;
  private texts: Phaser.GameObjects.Text[] = [];

  private dino!: Dino;
  private partyIndex = 0;
  private oldName = '';
  private newName = '';
  private oldType!: DinoType;
  private newType!: DinoType;
  private targetSpeciesId = 0;

  // Animation state
  private phase: 'intro' | 'animating' | 'done' | 'cancelled' = 'intro';
  private animTimer = 0;
  private animDuration = 3000; // ms
  private flashCount = 0;
  private showNewForm = false;
  private canCancel = true;

  // Return info
  private returnScene = '';
  private returnData: any = {};

  // Keys
  private escKey!: Phaser.Input.Keyboard.Key;
  private enterKey!: Phaser.Input.Keyboard.Key;
  private bKey!: Phaser.Input.Keyboard.Key;

  constructor() {
    super({ key: SCENE_KEYS.EVOLUTION });
  }

  init(data: {
    partyIndex?: number;
    returnScene?: string;
    returnData?: any;
  }): void {
    this.partyIndex = data.partyIndex ?? 0;
    this.returnScene = data.returnScene ?? SCENE_KEYS.OVERWORLD;
    this.returnData = data.returnData ?? {};
    this.phase = 'intro';
    this.animTimer = 0;
    this.flashCount = 0;
    this.showNewForm = false;
    this.canCancel = true;
  }

  create(): void {
    this.graphics = this.add.graphics();

    const gs = GameState.getInstance();
    const party = gs.party;
    const returnScene = this.returnScene;

    // Crash guard: bail if dino or evolution data is missing
    if (!party[this.partyIndex]) {
      this.scene.start(returnScene, this.returnData);
      return;
    }

    this.dino = party[this.partyIndex] as Dino;

    if (!this.dino.species || !this.dino.species.evolution) {
      this.scene.start(returnScene, this.returnData);
      return;
    }

    this.oldName = this.dino.nickname;
    this.oldType = this.dino.type1;

    const evo = this.dino.species.evolution;
    if (evo && evo.targetId && hasSpecies(evo.targetId)) {
      this.targetSpeciesId = evo.targetId;
      this.newName = getSpecies(evo.targetId).name;
      this.newType = getSpecies(evo.targetId).type1;
    } else {
      // No valid evolution, bail
      this.phase = 'cancelled';
      this.newName = this.oldName;
      this.newType = this.oldType;
    }

    if (this.input.keyboard) {
      this.escKey = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.ESC);
      this.enterKey = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.ENTER);
      this.bKey = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.B);
    }

    // Dark background
    this.cameras.main.setBackgroundColor(0x101020);
    this.cameras.main.fadeIn(500, 0x10, 0x10, 0x20);

    // Start intro
    this.renderIntro();
  }

  update(_time: number, delta: number): void {
    switch (this.phase) {
      case 'intro':
        this.handleIntroInput();
        break;
      case 'animating':
        this.updateAnimation(delta);
        break;
      case 'done':
        this.handleDoneInput();
        break;
      case 'cancelled':
        this.handleCancelledInput();
        break;
    }
  }

  // ===================== Phases =====================

  private renderIntro(): void {
    this.graphics.clear();
    this.clearTexts();

    // Dark background
    this.graphics.fillStyle(0x101020, 1);
    this.graphics.fillRect(0, 0, GAME_WIDTH, GAME_HEIGHT);

    // Old form dino shape
    this.drawDinoShape(GAME_WIDTH / 2 - 40, GAME_HEIGHT / 2 - 60, this.oldType, 80);

    // Text
    this.addText(GAME_WIDTH / 2, GAME_HEIGHT / 2 + 60,
      `Quoi ? ${this.oldName} evolue !`,
      FONT_BODY, '#F0E8D0').setOrigin(0.5, 0.5);

    this.addText(GAME_WIDTH / 2, GAME_HEIGHT - 40,
      'B pour annuler',
      FONT_TINY, '#888888').setOrigin(0.5, 0.5);

    // Start animation after a short delay
    this.time.delayedCall(1500, () => {
      if (this.phase === 'intro') {
        this.phase = 'animating';
        this.animTimer = 0;
      }
    });
  }

  private updateAnimation(delta: number): void {
    this.animTimer += delta;
    const progress = Math.min(1, this.animTimer / this.animDuration);

    this.graphics.clear();
    this.clearTexts();

    // Dark background
    this.graphics.fillStyle(0x101020, 1);
    this.graphics.fillRect(0, 0, GAME_WIDTH, GAME_HEIGHT);

    // Pulsing flash effect
    const flashRate = 3 + progress * 12; // Speed up over time
    const flashPhase = Math.sin(this.animTimer / 1000 * flashRate * Math.PI);
    const isWhite = flashPhase > 0;

    // Scale grows slightly during animation
    const scale = 80 + progress * 20;
    const cx = GAME_WIDTH / 2 - scale / 2;
    const cy = GAME_HEIGHT / 2 - 60;

    if (isWhite) {
      // Flash white
      this.graphics.fillStyle(0xFFFFFF, 0.8);
      this.graphics.fillCircle(GAME_WIDTH / 2, GAME_HEIGHT / 2 - 20, scale / 1.5);

      // Alternating between old and new form
      if (progress > 0.3) {
        this.showNewForm = !this.showNewForm;
        if (this.showNewForm) {
          this.drawDinoShape(cx, cy, this.newType, scale, 0.5);
        } else {
          this.drawDinoShape(cx, cy, this.oldType, scale, 0.5);
        }
      } else {
        this.drawDinoShape(cx, cy, this.oldType, scale, 0.5);
      }
    } else {
      // Show form
      if (progress > 0.5) {
        this.drawDinoShape(cx, cy, this.newType, scale);
      } else {
        this.drawDinoShape(cx, cy, this.oldType, scale);
      }
    }

    // Text
    this.addText(GAME_WIDTH / 2, GAME_HEIGHT / 2 + 60,
      `${this.oldName} evolue...`,
      FONT_SMALL, '#F0E8D0').setOrigin(0.5, 0.5);

    // Can still cancel in first 60%
    if (progress < 0.6) {
      this.addText(GAME_WIDTH / 2, GAME_HEIGHT - 40,
        'B pour annuler',
        FONT_TINY, '#888888').setOrigin(0.5, 0.5);

      // Check cancel
      if (Phaser.Input.Keyboard.JustDown(this.bKey)) {
        this.cancelEvolution();
        return;
      }
    } else {
      this.canCancel = false;
    }

    // Animation complete
    if (progress >= 1) {
      this.completeEvolution();
    }
  }

  private completeEvolution(): void {
    // Perform the actual evolution
    const oldNickname = this.dino.nickname;
    const hadCustomName = oldNickname !== this.oldName;
    this.dino.evolve();

    // Update nickname if it wasn't custom
    if (!hadCustomName) {
      this.dino.nickname = this.newName;
    }

    // Update dinodex
    const gs = GameState.getInstance();
    gs.catchDino(this.targetSpeciesId);

    this.phase = 'done';

    // Final render
    this.graphics.clear();
    this.clearTexts();

    // Bright flash
    this.graphics.fillStyle(0xFFFFFF, 1);
    this.graphics.fillRect(0, 0, GAME_WIDTH, GAME_HEIGHT);

    this.time.delayedCall(300, () => {
      this.renderComplete();
    });
  }

  private renderComplete(): void {
    this.graphics.clear();
    this.clearTexts();

    this.graphics.fillStyle(0x101020, 1);
    this.graphics.fillRect(0, 0, GAME_WIDTH, GAME_HEIGHT);

    // New form (large)
    this.drawDinoShape(GAME_WIDTH / 2 - 48, GAME_HEIGHT / 2 - 70, this.newType, 96);

    // Sparkle effects (small diamond shapes)
    for (let i = 0; i < 8; i++) {
      const angle = (i / 8) * Math.PI * 2;
      const dist = 80;
      const sx = GAME_WIDTH / 2 + Math.cos(angle) * dist;
      const sy = GAME_HEIGHT / 2 - 20 + Math.sin(angle) * dist;
      this.graphics.fillStyle(0xF8E870, 0.8);
      this.graphics.fillTriangle(sx, sy - 6, sx - 4, sy, sx + 4, sy);
      this.graphics.fillTriangle(sx, sy + 6, sx - 4, sy, sx + 4, sy);
    }

    const displayOldName = this.oldName;
    const displayNewName = this.dino.nickname;

    this.addText(GAME_WIDTH / 2, GAME_HEIGHT / 2 + 60,
      `${displayOldName} a evolue en ${displayNewName} !`,
      FONT_SMALL, '#E8C868').setOrigin(0.5, 0.5);

    this.addText(GAME_WIDTH / 2, GAME_HEIGHT - 40,
      'Appuyez sur Entree',
      FONT_TINY, '#888888').setOrigin(0.5, 0.5);
  }

  private cancelEvolution(): void {
    this.phase = 'cancelled';

    this.graphics.clear();
    this.clearTexts();

    this.graphics.fillStyle(0x101020, 1);
    this.graphics.fillRect(0, 0, GAME_WIDTH, GAME_HEIGHT);

    // Show old form
    this.drawDinoShape(GAME_WIDTH / 2 - 40, GAME_HEIGHT / 2 - 60, this.oldType, 80);

    this.addText(GAME_WIDTH / 2, GAME_HEIGHT / 2 + 60,
      `${this.oldName} n'a pas evolue.`,
      FONT_SMALL, '#F0E8D0').setOrigin(0.5, 0.5);

    this.addText(GAME_WIDTH / 2, GAME_HEIGHT - 40,
      'Appuyez sur Entree',
      FONT_TINY, '#888888').setOrigin(0.5, 0.5);
  }

  // ===================== Input =====================

  private handleIntroInput(): void {
    if (Phaser.Input.Keyboard.JustDown(this.bKey)) {
      this.cancelEvolution();
    }
  }

  private handleDoneInput(): void {
    if (Phaser.Input.Keyboard.JustDown(this.enterKey) ||
        Phaser.Input.Keyboard.JustDown(this.escKey)) {
      this.exitScene();
    }
  }

  private handleCancelledInput(): void {
    if (Phaser.Input.Keyboard.JustDown(this.enterKey) ||
        Phaser.Input.Keyboard.JustDown(this.escKey)) {
      this.exitScene();
    }
  }

  // ===================== Exit =====================

  private exitScene(): void {
    this.cameras.main.fadeOut(400, 0x10, 0x10, 0x20);
    this.cameras.main.once('camerafadeoutcomplete', () => {
      this.scene.start(this.returnScene, this.returnData);
    });
  }

  // ===================== Drawing Helpers =====================

  private drawDinoShape(x: number, y: number, type: DinoType, size: number, alpha: number = 1): void {
    const color = DINO_TYPE_COLORS[type] ?? 0xa8a878;
    this.graphics.fillStyle(color, alpha);
    const half = size / 2;
    this.graphics.fillTriangle(x, y + half, x + half, y, x + size, y + half);
    this.graphics.fillTriangle(x, y + half, x + half, y + size, x + size, y + half);
    // Eye
    this.graphics.fillStyle(0x181018, alpha);
    this.graphics.fillCircle(x + half + half / 4, y + half - half / 6, Math.max(2, size / 12));
  }

  private addText(x: number, y: number, text: string, size: string, color: string): Phaser.GameObjects.Text {
    const t = this.add.text(x, y, text, {
      fontFamily: FONT_FAMILY,
      fontSize: size,
      color,
      shadow: { offsetX: 1, offsetY: 1, color: '#181018', blur: 0, fill: true },
    }).setDepth(100);
    this.texts.push(t);
    return t;
  }

  private clearTexts(): void {
    for (const t of this.texts) {
      t.destroy();
    }
    this.texts = [];
  }
}
