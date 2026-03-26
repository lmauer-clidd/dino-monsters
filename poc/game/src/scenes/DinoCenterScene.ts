// ============================================================
// Jurassic Trainers -- Dino Center Scene (healing center)
// ============================================================

import Phaser from 'phaser';
import {
  SCENE_KEYS,
  GAME_WIDTH,
  GAME_HEIGHT,
  TILE_SIZE,
  COLORS,
  FONT_FAMILY,
  FONT_SMALL,
  FONT_TINY,
  Direction,
  PLAYER_MOVE_DURATION,
} from '../utils/constants';
import { DialogueBox } from '../ui/DialogueBox';
import { GameState } from '../systems/GameState';

// ===================== Tile Palette =====================

enum CenterTile {
  Floor = 0,
  Counter = 1,
  Wall = 2,
  Door = 3,
  Machine = 4,
  Mat = 5,
  Plant = 6,
}

const CENTER_TILE_COLORS: Record<number, number> = {
  [CenterTile.Floor]: 0xE8D8C0,
  [CenterTile.Counter]: 0x886830,
  [CenterTile.Wall]: 0xD0C0A8,
  [CenterTile.Door]: 0x604020,
  [CenterTile.Machine]: 0x58A8F8,
  [CenterTile.Mat]: 0xC84040,
  [CenterTile.Plant]: 0x48A040,
};

const COLLISION_TILES = new Set([
  CenterTile.Counter, CenterTile.Wall, CenterTile.Machine, CenterTile.Plant,
]);

// ===================== Map Layout (10x8) =====================

const MAP_W = 10;
const MAP_H = 8;

// prettier-ignore
const CENTER_MAP: number[][] = [
  [2, 2, 2, 2, 2, 2, 2, 2, 2, 2],
  [2, 0, 0, 6, 1, 1, 1, 6, 0, 2],
  [2, 0, 0, 0, 1, 4, 1, 0, 0, 2],
  [2, 0, 0, 0, 0, 0, 0, 0, 0, 2],
  [2, 0, 0, 0, 0, 0, 0, 0, 0, 2],
  [2, 0, 0, 0, 0, 0, 0, 0, 0, 2],
  [2, 0, 0, 0, 5, 5, 0, 0, 0, 2],
  [2, 2, 2, 2, 3, 3, 2, 2, 2, 2],
];

// ===================== NPC =====================

interface CenterNPC {
  x: number;
  y: number;
  name: string;
  color: number;
  hairColor: number;
}

const NURSE: CenterNPC = {
  x: 5,
  y: 1,
  name: 'Infirmiere Joy',
  color: 0xF08888,
  hairColor: 0xF8A8B8,
};

// ===================== Scene =====================

export class DinoCenterScene extends Phaser.Scene {
  private mapGraphics!: Phaser.GameObjects.Graphics;
  private nurseGraphics!: Phaser.GameObjects.Graphics;
  private playerGraphics!: Phaser.GameObjects.Graphics;
  private dialogueBox!: DialogueBox;

  // Player position in tile coords
  private playerX = 4;
  private playerY = 6;
  private playerFacing = Direction.Up;
  private isMoving = false;
  private canInput = true;

  // Scene data
  private returnMapId = '';
  private returnX = 0;
  private returnY = 0;

  // Offsets to center 10x8 map in 640x400
  private offsetX = 0;
  private offsetY = 0;

  // Keys
  private cursors!: Phaser.Types.Input.Keyboard.CursorKeys;
  private enterKey!: Phaser.Input.Keyboard.Key;
  private escKey!: Phaser.Input.Keyboard.Key;

  // Healing state
  private isHealing = false;

  constructor() {
    super({ key: SCENE_KEYS.DINO_CENTER });
  }

  init(data: { returnMapId?: string; returnX?: number; returnY?: number }): void {
    this.returnMapId = data.returnMapId ?? 'BOURG_NID';
    this.returnX = data.returnX ?? 7;
    this.returnY = data.returnY ?? 5;
    this.playerX = 4;
    this.playerY = 6;
    this.playerFacing = Direction.Up;
    this.isMoving = false;
    this.canInput = true;
    this.isHealing = false;
  }

  create(): void {
    // Calculate offsets to center the map
    const mapPixelW = MAP_W * TILE_SIZE;
    const mapPixelH = MAP_H * TILE_SIZE;
    this.offsetX = Math.floor((GAME_WIDTH - mapPixelW) / 2);
    this.offsetY = Math.floor((GAME_HEIGHT - mapPixelH) / 2);

    // Draw map
    this.mapGraphics = this.add.graphics();
    this.drawMap();

    // Draw nurse NPC
    this.nurseGraphics = this.add.graphics();
    this.drawNurse();

    // Draw player
    this.playerGraphics = this.add.graphics();
    this.drawPlayer();

    // Dialogue box
    this.dialogueBox = new DialogueBox(this);

    // Input
    if (this.input.keyboard) {
      this.cursors = this.input.keyboard.createCursorKeys();
      this.enterKey = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.ENTER);
      this.escKey = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.ESC);
    }

    // Fade in
    this.cameras.main.fadeIn(300, 0x18, 0x10, 0x18);
  }

  update(): void {
    // Dialogue takes priority
    if (this.dialogueBox.isActive()) {
      this.dialogueBox.update();
      return;
    }

    if (!this.canInput || this.isMoving || this.isHealing) return;

    // Check for exit
    if (this.playerX >= 4 && this.playerX <= 5 && this.playerY === 7) {
      this.exitCenter();
      return;
    }

    // Movement
    if (this.cursors.up.isDown) {
      this.tryMove(Direction.Up);
    } else if (this.cursors.down.isDown) {
      this.tryMove(Direction.Down);
    } else if (this.cursors.left.isDown) {
      this.tryMove(Direction.Left);
    } else if (this.cursors.right.isDown) {
      this.tryMove(Direction.Right);
    }

    // Interact
    if (Phaser.Input.Keyboard.JustDown(this.enterKey)) {
      this.tryInteract();
    }

    // Escape to leave
    if (Phaser.Input.Keyboard.JustDown(this.escKey)) {
      if (this.playerY >= 6) {
        this.exitCenter();
      }
    }
  }

  // ===================== Movement =====================

  private tryMove(dir: Direction): void {
    this.playerFacing = dir;
    let nx = this.playerX;
    let ny = this.playerY;

    switch (dir) {
      case Direction.Up: ny--; break;
      case Direction.Down: ny++; break;
      case Direction.Left: nx--; break;
      case Direction.Right: nx++; break;
    }

    // Bounds check
    if (nx < 0 || nx >= MAP_W || ny < 0 || ny >= MAP_H) return;

    // Collision with tiles
    const tile = CENTER_MAP[ny][nx];
    if (COLLISION_TILES.has(tile)) return;

    // Collision with NPC
    if (nx === NURSE.x && ny === NURSE.y) return;

    // Move
    this.isMoving = true;
    this.playerX = nx;
    this.playerY = ny;
    this.drawPlayer();

    this.time.delayedCall(PLAYER_MOVE_DURATION, () => {
      this.isMoving = false;
    });
  }

  private tryInteract(): void {
    let targetX = this.playerX;
    let targetY = this.playerY;

    switch (this.playerFacing) {
      case Direction.Up: targetY--; break;
      case Direction.Down: targetY++; break;
      case Direction.Left: targetX--; break;
      case Direction.Right: targetX++; break;
    }

    // Check if facing the nurse or counter
    if (targetX >= 4 && targetX <= 6 && targetY <= 2) {
      this.startHealing();
    }
  }

  // ===================== Healing =====================

  private startHealing(): void {
    this.canInput = false;
    this.isHealing = true;

    this.dialogueBox.showText(
      'Bienvenue au Centre Dino ! Je vais soigner vos dinos.',
      () => {
        // Healing animation: flash effect
        this.doHealAnimation();
      },
      NURSE.name,
    );
  }

  private doHealAnimation(): void {
    // Flash white a few times
    const flash = this.add.rectangle(
      GAME_WIDTH / 2, GAME_HEIGHT / 2,
      GAME_WIDTH, GAME_HEIGHT,
      0xffffff, 0,
    ).setDepth(500);

    let flashCount = 0;
    const maxFlashes = 3;

    const flashTimer = this.time.addEvent({
      delay: 300,
      repeat: maxFlashes * 2 - 1,
      callback: () => {
        flashCount++;
        if (flashCount % 2 === 1) {
          flash.setAlpha(0.6);
        } else {
          flash.setAlpha(0);
        }

        if (flashCount >= maxFlashes * 2) {
          flash.destroy();
          this.completeHealing();
        }
      },
    });
  }

  private completeHealing(): void {
    // Heal all party dinos and set heal point
    const gs = GameState.getInstance();
    gs.getPartySystem().healAll();
    gs.setHealPoint(this.returnMapId, this.returnX, this.returnY);

    this.dialogueBox.showText(
      'Vos dinos sont en pleine forme !',
      () => {
        this.isHealing = false;
        this.canInput = true;
      },
      NURSE.name,
    );
  }

  // ===================== Exit =====================

  private exitCenter(): void {
    this.canInput = false;
    this.cameras.main.fadeOut(300, 0x18, 0x10, 0x18);
    this.cameras.main.once('camerafadeoutcomplete', () => {
      this.dialogueBox.destroy();
      this.scene.start(SCENE_KEYS.OVERWORLD, {
        hasStarter: true,
        mapId: this.returnMapId,
        playerX: this.returnX,
        playerY: this.returnY,
      });
    });
  }

  // ===================== Drawing =====================

  private drawMap(): void {
    this.mapGraphics.clear();

    for (let y = 0; y < MAP_H; y++) {
      for (let x = 0; x < MAP_W; x++) {
        const tile = CENTER_MAP[y][x];
        const color = CENTER_TILE_COLORS[tile] ?? 0xE8D8C0;
        const px = this.offsetX + x * TILE_SIZE;
        const py = this.offsetY + y * TILE_SIZE;

        this.mapGraphics.fillStyle(color, 1);
        this.mapGraphics.fillRect(px, py, TILE_SIZE, TILE_SIZE);

        // Tile details
        if (tile === CenterTile.Counter) {
          // Draw counter top highlight
          this.mapGraphics.fillStyle(0xA08040, 1);
          this.mapGraphics.fillRect(px, py, TILE_SIZE, 4);
        } else if (tile === CenterTile.Machine) {
          // Draw healing machine
          this.mapGraphics.fillStyle(0x3888D8, 1);
          this.mapGraphics.fillRect(px + 4, py + 4, TILE_SIZE - 8, TILE_SIZE - 8);
          this.mapGraphics.fillStyle(0x88D8F8, 1);
          this.mapGraphics.fillRect(px + 8, py + 8, TILE_SIZE - 16, TILE_SIZE - 16);
          // Red cross
          this.mapGraphics.fillStyle(0xF04040, 1);
          this.mapGraphics.fillRect(px + 14, py + 10, 4, 12);
          this.mapGraphics.fillRect(px + 10, py + 14, 12, 4);
        } else if (tile === CenterTile.Door) {
          // Door handle
          this.mapGraphics.fillStyle(0xC8A040, 1);
          this.mapGraphics.fillRect(px + TILE_SIZE - 8, py + 14, 4, 4);
        } else if (tile === CenterTile.Plant) {
          // Potted plant
          this.mapGraphics.fillStyle(0x804020, 1);
          this.mapGraphics.fillRect(px + 8, py + 18, 16, 12);
          this.mapGraphics.fillStyle(0x58B848, 1);
          this.mapGraphics.fillRect(px + 4, py + 4, 24, 16);
        } else if (tile === CenterTile.Mat) {
          // Welcome mat pattern
          this.mapGraphics.fillStyle(0xA83030, 1);
          this.mapGraphics.fillRect(px + 2, py + 2, TILE_SIZE - 4, TILE_SIZE - 4);
        }

        // Grid lines
        this.mapGraphics.lineStyle(1, 0x000000, 0.05);
        this.mapGraphics.strokeRect(px, py, TILE_SIZE, TILE_SIZE);
      }
    }
  }

  private drawNurse(): void {
    this.nurseGraphics.clear();
    const px = this.offsetX + NURSE.x * TILE_SIZE;
    const py = this.offsetY + NURSE.y * TILE_SIZE;

    // Body
    this.nurseGraphics.fillStyle(NURSE.color, 1);
    this.nurseGraphics.fillRect(px + 8, py + 12, 16, 18);

    // Head
    this.nurseGraphics.fillStyle(0xF8D0B0, 1);
    this.nurseGraphics.fillCircle(px + 16, py + 10, 8);

    // Hair
    this.nurseGraphics.fillStyle(NURSE.hairColor, 1);
    this.nurseGraphics.fillRect(px + 8, py + 2, 16, 6);

    // Nurse cap (cross)
    this.nurseGraphics.fillStyle(0xFFFFFF, 1);
    this.nurseGraphics.fillRect(px + 12, py + 1, 8, 6);
    this.nurseGraphics.fillStyle(0xF04040, 1);
    this.nurseGraphics.fillRect(px + 15, py + 2, 2, 4);
    this.nurseGraphics.fillRect(px + 14, py + 3, 4, 2);
  }

  private drawPlayer(): void {
    this.playerGraphics.clear();
    const px = this.offsetX + this.playerX * TILE_SIZE;
    const py = this.offsetY + this.playerY * TILE_SIZE;

    // Body
    this.playerGraphics.fillStyle(0x3868B8, 1);
    this.playerGraphics.fillRect(px + 8, py + 12, 16, 18);

    // Head
    this.playerGraphics.fillStyle(0xF8D0B0, 1);
    this.playerGraphics.fillCircle(px + 16, py + 10, 8);

    // Hat
    this.playerGraphics.fillStyle(0xC84040, 1);
    this.playerGraphics.fillRect(px + 6, py + 2, 20, 6);

    // Direction indicator
    this.playerGraphics.fillStyle(0x181018, 1);
    let ex = px + 16, ey = py + 10;
    switch (this.playerFacing) {
      case Direction.Up: ey -= 3; break;
      case Direction.Down: ey += 3; break;
      case Direction.Left: ex -= 3; break;
      case Direction.Right: ex += 3; break;
    }
    this.playerGraphics.fillCircle(ex, ey, 2);
  }
}
