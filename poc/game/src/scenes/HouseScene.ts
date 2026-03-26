// ============================================================
// Jurassic Trainers -- House Scene (generic interior)
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

// ===================== Tile Palette =====================

enum HouseTile {
  Floor = 0,
  Wall = 1,
  Door = 2,
  Table = 3,
  Chair = 4,
  Bookshelf = 5,
  Bed = 6,
  Mat = 7,
}

const HOUSE_TILE_COLORS: Record<number, number> = {
  [HouseTile.Floor]: 0xC8A878,
  [HouseTile.Wall]: 0xD0B890,
  [HouseTile.Door]: 0x604020,
  [HouseTile.Table]: 0x886830,
  [HouseTile.Chair]: 0x785828,
  [HouseTile.Bookshelf]: 0x704828,
  [HouseTile.Bed]: 0x4868A8,
  [HouseTile.Mat]: 0xA04040,
};

const COLLISION_TILES = new Set([
  HouseTile.Wall, HouseTile.Table, HouseTile.Bookshelf, HouseTile.Bed,
]);

// ===================== Map Layout (8x6) =====================

const MAP_W = 8;
const MAP_H = 6;

// prettier-ignore
const HOUSE_MAP: number[][] = [
  [1, 1, 1, 1, 1, 1, 1, 1],
  [1, 5, 0, 0, 0, 3, 4, 1],
  [1, 0, 0, 0, 0, 0, 0, 1],
  [1, 6, 0, 0, 0, 0, 0, 1],
  [1, 0, 0, 7, 7, 0, 0, 1],
  [1, 1, 1, 2, 2, 1, 1, 1],
];

// ===================== NPC data =====================

interface HouseNPCData {
  name: string;
  dialogue: string[];
}

interface HouseNPCRuntime {
  tileX: number;
  tileY: number;
  name: string;
  dialogue: string[];
  bodyColor: number;
  hairColor: number;
}

// NPC positions inside the house (avoid furniture)
const NPC_POSITIONS: { x: number; y: number }[] = [
  { x: 2, y: 2 },
  { x: 5, y: 3 },
];

const NPC_BODY_COLORS = [0x60A040, 0xC0A030, 0x4080C0, 0xA06080];
const NPC_HAIR_COLORS = [0x604030, 0x302010, 0x403020, 0x503040];

// ===================== Scene =====================

export class HouseScene extends Phaser.Scene {
  private mapGraphics!: Phaser.GameObjects.Graphics;
  private npcGraphics!: Phaser.GameObjects.Graphics;
  private playerGraphics!: Phaser.GameObjects.Graphics;
  private dialogueBox!: DialogueBox;

  // Player position in tile coords
  private playerX = 3;
  private playerY = 4;
  private playerFacing = Direction.Up;
  private isMoving = false;
  private canInput = true;

  // Scene data
  private returnMapId = '';
  private returnX = 0;
  private returnY = 0;
  private houseId = '';
  private houseName = 'Maison';
  private houseNPCs: HouseNPCRuntime[] = [];

  // Offsets to center map in viewport
  private offsetX = 0;
  private offsetY = 0;

  // Keys
  private cursors!: Phaser.Types.Input.Keyboard.CursorKeys;
  private enterKey!: Phaser.Input.Keyboard.Key;
  private escKey!: Phaser.Input.Keyboard.Key;

  constructor() {
    super({ key: SCENE_KEYS.HOUSE });
  }

  init(data: {
    returnMapId?: string;
    returnX?: number;
    returnY?: number;
    houseId?: string;
    houseName?: string;
    npcs?: Array<{ name: string; dialogue: string[] }>;
  }): void {
    this.returnMapId = data.returnMapId ?? 'BOURG_NID';
    this.returnX = data.returnX ?? 7;
    this.returnY = data.returnY ?? 5;
    this.houseId = data.houseId ?? 'unknown';
    this.houseName = data.houseName ?? 'Maison';
    this.playerX = 3;
    this.playerY = 4;
    this.playerFacing = Direction.Up;
    this.isMoving = false;
    this.canInput = true;

    // Build NPC runtime data (up to 2)
    this.houseNPCs = [];
    const npcInput = data.npcs ?? [];
    const count = Math.min(npcInput.length, 2);
    for (let i = 0; i < count; i++) {
      const pos = NPC_POSITIONS[i];
      this.houseNPCs.push({
        tileX: pos.x,
        tileY: pos.y,
        name: npcInput[i].name,
        dialogue: npcInput[i].dialogue,
        bodyColor: NPC_BODY_COLORS[i % NPC_BODY_COLORS.length],
        hairColor: NPC_HAIR_COLORS[i % NPC_HAIR_COLORS.length],
      });
    }
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

    // Draw NPCs
    this.npcGraphics = this.add.graphics();
    this.drawNPCs();

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

    if (!this.canInput || this.isMoving) return;

    // Check for exit (door tiles at bottom center)
    if (this.playerX >= 3 && this.playerX <= 4 && this.playerY === 5) {
      this.exitHouse();
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
      if (this.playerY >= 4) {
        this.exitHouse();
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
    const tile = HOUSE_MAP[ny][nx];
    if (COLLISION_TILES.has(tile)) return;

    // Collision with NPCs
    for (const npc of this.houseNPCs) {
      if (nx === npc.tileX && ny === npc.tileY) return;
    }

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

    // Check if facing an NPC
    for (const npc of this.houseNPCs) {
      if (targetX === npc.tileX && targetY === npc.tileY) {
        this.talkToNPC(npc);
        return;
      }
    }

    // Check if facing furniture for flavor text
    if (targetY >= 0 && targetY < MAP_H && targetX >= 0 && targetX < MAP_W) {
      const tile = HOUSE_MAP[targetY][targetX];
      if (tile === HouseTile.Bookshelf) {
        this.dialogueBox.showText('Des livres sur les dinos... Passionnant !');
      } else if (tile === HouseTile.Table) {
        this.dialogueBox.showText('Une table en bois solide.');
      } else if (tile === HouseTile.Bed) {
        this.dialogueBox.showText('Un lit confortable.');
      }
    }
  }

  // ===================== NPC Dialogue =====================

  private talkToNPC(npc: HouseNPCRuntime): void {
    if (npc.dialogue.length === 0) return;

    this.canInput = false;
    let lineIndex = 0;

    const showNext = (): void => {
      if (lineIndex < npc.dialogue.length) {
        const line = npc.dialogue[lineIndex];
        lineIndex++;
        this.dialogueBox.showText(line, () => {
          if (lineIndex < npc.dialogue.length) {
            showNext();
          } else {
            this.canInput = true;
          }
        }, npc.name);
      }
    };

    showNext();
  }

  // ===================== Exit =====================

  private exitHouse(): void {
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
        const tile = HOUSE_MAP[y][x];
        const color = HOUSE_TILE_COLORS[tile] ?? 0xC8A878;
        const px = this.offsetX + x * TILE_SIZE;
        const py = this.offsetY + y * TILE_SIZE;

        this.mapGraphics.fillStyle(color, 1);
        this.mapGraphics.fillRect(px, py, TILE_SIZE, TILE_SIZE);

        // Tile details
        if (tile === HouseTile.Table) {
          // Table top highlight
          this.mapGraphics.fillStyle(0xA08040, 1);
          this.mapGraphics.fillRect(px + 2, py + 2, TILE_SIZE - 4, TILE_SIZE - 4);
          this.mapGraphics.fillStyle(0xB89850, 1);
          this.mapGraphics.fillRect(px + 4, py + 4, TILE_SIZE - 8, TILE_SIZE - 8);
        } else if (tile === HouseTile.Chair) {
          // Chair seat
          this.mapGraphics.fillStyle(0x906838, 1);
          this.mapGraphics.fillRect(px + 6, py + 8, TILE_SIZE - 12, TILE_SIZE - 12);
          // Chair back
          this.mapGraphics.fillStyle(0x704828, 1);
          this.mapGraphics.fillRect(px + 6, py + 2, TILE_SIZE - 12, 8);
        } else if (tile === HouseTile.Bookshelf) {
          // Shelf body
          this.mapGraphics.fillStyle(0x805830, 1);
          this.mapGraphics.fillRect(px + 2, py + 2, TILE_SIZE - 4, TILE_SIZE - 4);
          // Book spines (colored rectangles)
          const bookColors = [0xC04040, 0x4080C0, 0x40A040, 0xC0A030];
          for (let i = 0; i < 4; i++) {
            this.mapGraphics.fillStyle(bookColors[i], 1);
            this.mapGraphics.fillRect(px + 5 + i * 6, py + 4, 5, 12);
          }
          // Lower shelf books
          for (let i = 0; i < 4; i++) {
            this.mapGraphics.fillStyle(bookColors[(i + 2) % 4], 1);
            this.mapGraphics.fillRect(px + 5 + i * 6, py + 18, 5, 10);
          }
        } else if (tile === HouseTile.Bed) {
          // Bed frame
          this.mapGraphics.fillStyle(0x604020, 1);
          this.mapGraphics.fillRect(px + 2, py + 2, TILE_SIZE - 4, TILE_SIZE - 4);
          // Sheets
          this.mapGraphics.fillStyle(0x88A8D0, 1);
          this.mapGraphics.fillRect(px + 4, py + 4, TILE_SIZE - 8, TILE_SIZE - 12);
          // Pillow
          this.mapGraphics.fillStyle(0xE8E0D0, 1);
          this.mapGraphics.fillRect(px + 6, py + 4, TILE_SIZE - 12, 8);
        } else if (tile === HouseTile.Door) {
          // Door handle
          this.mapGraphics.fillStyle(0xC8A040, 1);
          this.mapGraphics.fillRect(px + TILE_SIZE - 8, py + 14, 4, 4);
        } else if (tile === HouseTile.Mat) {
          // Welcome mat pattern
          this.mapGraphics.fillStyle(0x883030, 1);
          this.mapGraphics.fillRect(px + 2, py + 2, TILE_SIZE - 4, TILE_SIZE - 4);
        }

        // Grid lines
        this.mapGraphics.lineStyle(1, 0x000000, 0.05);
        this.mapGraphics.strokeRect(px, py, TILE_SIZE, TILE_SIZE);
      }
    }
  }

  private drawNPCs(): void {
    this.npcGraphics.clear();

    for (const npc of this.houseNPCs) {
      const px = this.offsetX + npc.tileX * TILE_SIZE;
      const py = this.offsetY + npc.tileY * TILE_SIZE;

      // Body
      this.npcGraphics.fillStyle(npc.bodyColor, 1);
      this.npcGraphics.fillRect(px + 8, py + 12, 16, 18);

      // Head
      this.npcGraphics.fillStyle(0xF8D0B0, 1);
      this.npcGraphics.fillCircle(px + 16, py + 10, 8);

      // Hair
      this.npcGraphics.fillStyle(npc.hairColor, 1);
      this.npcGraphics.fillRect(px + 8, py + 2, 16, 6);

      // Eyes
      this.npcGraphics.fillStyle(0x181018, 1);
      this.npcGraphics.fillCircle(px + 13, py + 10, 2);
      this.npcGraphics.fillCircle(px + 19, py + 10, 2);
    }
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
