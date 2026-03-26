// ============================================================
// Jurassic Trainers — GymScene
// Interior gym scene with trainers and gym leader
// ============================================================

import Phaser from 'phaser';
import {
  SCENE_KEYS,
  GAME_WIDTH,
  GAME_HEIGHT,
  TILE_SIZE,
  PLAYER_MOVE_DURATION,
  COLORS,
  FONT_FAMILY,
  Direction,
  DinoType,
  DINO_TYPE_COLORS,
} from '../utils/constants';
import { DialogueBox } from '../ui/DialogueBox';
import { TransitionFX } from '../ui/TransitionFX';
import { GameState } from '../systems/GameState';
import {
  getGymLeader,
  getGymTrainers,
  BADGE_NAMES,
  TrainerData,
} from '../data/trainers';

// --- Gym layout: 15 wide x 12 tall ---
const GYM_W = 15;
const GYM_H = 12;

// Tile types for gym interior
const GT = {
  FLOOR: 0,
  WALL: 1,
  STATUE: 2,
  CARPET: 3,
  DOOR: 4,
  LEADER_TILE: 5,
};

// Gym type colors (floor accent by gym index)
const GYM_FLOOR_COLORS: number[] = [
  0x48A038, // 0: Flora (green)
  0x4080C0, // 1: Water (blue)
  0x8B7355, // 2: Rock (brown)
  0xC04020, // 3: Fire (red)
  0x90D0E0, // 4: Ice (cyan)
  0xD8C020, // 5: Electric (yellow)
  0x8040A0, // 6: Venom (purple)
  0xA0B8D0, // 7: Air (light blue)
];

const GYM_WALL_COLORS: number[] = [
  0x305828, 0x305880, 0x5A4A30, 0x802818,
  0x608898, 0x887810, 0x502870, 0x607890,
];

// NPC positions in the gym
interface GymNPCSlot {
  x: number;
  y: number;
  isLeader: boolean;
  trainerId: string;
  trainerData?: TrainerData;
}

export class GymScene extends Phaser.Scene {
  private gymId = 0;
  private returnMapId = 'BOURG_NID';
  private returnX = 0;
  private returnY = 0;

  // Layout
  private tileGfx!: Phaser.GameObjects.Graphics;
  private npcGfx!: Phaser.GameObjects.Graphics;
  private playerGfx!: Phaser.GameObjects.Graphics;
  private uiContainer!: Phaser.GameObjects.Container;
  private exclamationGfx!: Phaser.GameObjects.Graphics;

  // Player
  private playerTileX = 7;
  private playerTileY = 10;
  private playerDirection = Direction.Up;
  private isMoving = false;
  private playerPixelX = 0;
  private playerPixelY = 0;

  // NPCs
  private gymNPCs: GymNPCSlot[] = [];

  // UI
  private dialogue!: DialogueBox;

  // Input
  private cursors!: Phaser.Types.Input.Keyboard.CursorKeys;
  private enterKey!: Phaser.Input.Keyboard.Key;
  private escKey!: Phaser.Input.Keyboard.Key;

  // State
  private battlePending = false;

  constructor() {
    super({ key: SCENE_KEYS.GYM });
  }

  init(data: {
    gymId: number;
    returnMapId?: string;
    returnX?: number;
    returnY?: number;
  }): void {
    this.gymId = data.gymId ?? 0;
    this.returnMapId = data.returnMapId ?? 'BOURG_NID';
    this.returnX = data.returnX ?? 7;
    this.returnY = data.returnY ?? 10;
  }

  create(): void {
    this.battlePending = false;
    this.isMoving = false;

    // Gym music
    import('../systems/AudioSystem').then(m => m.AudioSystem.getInstance().playGymMusic());

    // Setup NPCs for this gym
    this.setupGymNPCs();

    // Graphics
    this.tileGfx = this.add.graphics();
    this.npcGfx = this.add.graphics().setDepth(10);
    this.exclamationGfx = this.add.graphics().setDepth(15);
    this.playerGfx = this.add.graphics().setDepth(20);
    this.uiContainer = this.add.container(0, 0).setDepth(100).setScrollFactor(0);

    // Draw the gym
    this.drawGym();
    this.drawNPCs();

    // Player position
    this.playerTileX = 7;
    this.playerTileY = 10;
    this.playerDirection = Direction.Up;
    this.playerPixelX = this.playerTileX * TILE_SIZE;
    this.playerPixelY = this.playerTileY * TILE_SIZE;

    // Camera
    this.cameras.main.setBounds(0, 0, GYM_W * TILE_SIZE, GYM_H * TILE_SIZE);
    this.cameras.main.setBackgroundColor(COLORS.BLACK);

    // UI
    this.dialogue = new DialogueBox(this);

    // Input
    if (this.input.keyboard) {
      this.cursors = this.input.keyboard.createCursorKeys();
      this.enterKey = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.ENTER);
      this.escKey = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.ESC);
    }

    // Location banner
    this.showGymBanner();

    TransitionFX.fadeIn(this, 400);
  }

  // ============================================================
  // Setup
  // ============================================================
  private setupGymNPCs(): void {
    this.gymNPCs = [];
    const gs = GameState.getInstance();

    // Gym trainers (2 per gym)
    const trainers = getGymTrainers(this.gymId);
    // Trainer 1: left side, row 7
    if (trainers[0]) {
      this.gymNPCs.push({
        x: 4,
        y: 7,
        isLeader: false,
        trainerId: trainers[0].id,
        trainerData: trainers[0],
      });
    }
    // Trainer 2: right side, row 5
    if (trainers[1]) {
      this.gymNPCs.push({
        x: 10,
        y: 5,
        isLeader: false,
        trainerId: trainers[1].id,
        trainerData: trainers[1],
      });
    }

    // Gym leader: center, row 2
    const leader = getGymLeader(this.gymId);
    if (leader) {
      this.gymNPCs.push({
        x: 7,
        y: 2,
        isLeader: true,
        trainerId: leader.id,
        trainerData: leader,
      });
    }
  }

  // ============================================================
  // Drawing
  // ============================================================
  private drawGym(): void {
    this.tileGfx.clear();
    const floorColor = GYM_FLOOR_COLORS[this.gymId] ?? 0x808080;
    const wallColor = GYM_WALL_COLORS[this.gymId] ?? 0x404040;
    const floorDark = Phaser.Display.Color.IntegerToColor(floorColor).darken(20).color;
    const carpetColor = Phaser.Display.Color.IntegerToColor(floorColor).brighten(30).color;

    for (let y = 0; y < GYM_H; y++) {
      for (let x = 0; x < GYM_W; x++) {
        const px = x * TILE_SIZE;
        const py = y * TILE_SIZE;

        // Walls on top row and sides
        if (y === 0 || x === 0 || x === GYM_W - 1) {
          // Wall
          this.tileGfx.fillStyle(wallColor, 1);
          this.tileGfx.fillRect(px, py, TILE_SIZE, TILE_SIZE);
          // Brick lines
          this.tileGfx.lineStyle(1, Phaser.Display.Color.IntegerToColor(wallColor).darken(30).color, 0.5);
          for (let ly = 6; ly < TILE_SIZE; ly += 8) {
            this.tileGfx.lineBetween(px, py + ly, px + TILE_SIZE, py + ly);
          }
          continue;
        }

        // Door at bottom center
        if (y === GYM_H - 1 && x === 7) {
          this.tileGfx.fillStyle(0x604020, 1);
          this.tileGfx.fillRect(px, py, TILE_SIZE, TILE_SIZE);
          // Door frame
          this.tileGfx.fillStyle(0x806030, 1);
          this.tileGfx.fillRect(px + 2, py, TILE_SIZE - 4, TILE_SIZE);
          this.tileGfx.fillStyle(0xC89840, 1);
          this.tileGfx.fillCircle(px + 22, py + 16, 2);
          continue;
        }

        // Bottom wall (except door)
        if (y === GYM_H - 1) {
          this.tileGfx.fillStyle(wallColor, 1);
          this.tileGfx.fillRect(px, py, TILE_SIZE, TILE_SIZE);
          this.tileGfx.lineStyle(1, Phaser.Display.Color.IntegerToColor(wallColor).darken(30).color, 0.5);
          for (let ly = 6; ly < TILE_SIZE; ly += 8) {
            this.tileGfx.lineBetween(px, py + ly, px + TILE_SIZE, py + ly);
          }
          continue;
        }

        // Carpet (center column, rows 3-10)
        if (x >= 6 && x <= 8 && y >= 3 && y <= 10) {
          this.tileGfx.fillStyle(carpetColor, 0.4);
          this.tileGfx.fillRect(px, py, TILE_SIZE, TILE_SIZE);
          // Border
          if (x === 6) {
            this.tileGfx.fillStyle(carpetColor, 0.7);
            this.tileGfx.fillRect(px, py, 2, TILE_SIZE);
          }
          if (x === 8) {
            this.tileGfx.fillStyle(carpetColor, 0.7);
            this.tileGfx.fillRect(px + TILE_SIZE - 2, py, 2, TILE_SIZE);
          }
          continue;
        }

        // Leader platform area (row 1-2, center)
        if (y <= 2 && x >= 4 && x <= 10) {
          this.tileGfx.fillStyle(floorDark, 1);
          this.tileGfx.fillRect(px, py, TILE_SIZE, TILE_SIZE);
          // Decorative border
          if (y === 2) {
            this.tileGfx.fillStyle(floorColor, 0.6);
            this.tileGfx.fillRect(px, py + TILE_SIZE - 2, TILE_SIZE, 2);
          }
          continue;
        }

        // Statue positions (decorative columns)
        if ((x === 2 && y === 3) || (x === 12 && y === 3) ||
            (x === 2 && y === 8) || (x === 12 && y === 8)) {
          this.tileGfx.fillStyle(floorColor, 0.3);
          this.tileGfx.fillRect(px, py, TILE_SIZE, TILE_SIZE);
          // Draw statue
          this.tileGfx.fillStyle(0x888888, 1);
          this.tileGfx.fillRect(px + 8, py + 12, 16, 20);
          this.tileGfx.fillStyle(0xA0A0A0, 1);
          this.tileGfx.fillRect(px + 10, py + 4, 12, 10);
          this.tileGfx.fillStyle(0xC0C0C0, 0.5);
          this.tileGfx.fillRect(px + 12, py + 6, 4, 4);
          continue;
        }

        // Regular floor
        this.tileGfx.fillStyle(floorColor, 0.2);
        this.tileGfx.fillRect(px, py, TILE_SIZE, TILE_SIZE);
        // Tile grid
        this.tileGfx.lineStyle(1, floorColor, 0.15);
        this.tileGfx.strokeRect(px, py, TILE_SIZE, TILE_SIZE);
      }
    }

    // Gym emblem at leader area
    const emblemX = 7 * TILE_SIZE + TILE_SIZE / 2;
    const emblemY = 1 * TILE_SIZE + TILE_SIZE / 2;
    const typeColor = GYM_FLOOR_COLORS[this.gymId];
    this.tileGfx.fillStyle(typeColor, 0.5);
    this.tileGfx.fillCircle(emblemX, emblemY, 20);
    this.tileGfx.lineStyle(2, typeColor, 0.8);
    this.tileGfx.strokeCircle(emblemX, emblemY, 20);
    this.tileGfx.fillStyle(typeColor, 0.8);
    this.tileGfx.fillCircle(emblemX, emblemY, 8);
  }

  private drawNPCs(): void {
    this.npcGfx.clear();
    const gs = GameState.getInstance();

    for (const npc of this.gymNPCs) {
      // Skip defeated trainers (but not the leader once badge is earned — they stay but won't battle)
      const defeated = gs.isTrainerDefeated(npc.trainerId);

      const px = npc.x * TILE_SIZE;
      const py = npc.y * TILE_SIZE;

      // Shadow
      this.npcGfx.fillStyle(0x000000, 0.2);
      this.npcGfx.fillEllipse(px + TILE_SIZE / 2, py + TILE_SIZE - 2, 18, 5);

      // Body color based on gym type or leader
      const gymColor = GYM_FLOOR_COLORS[this.gymId];
      const bodyColor = npc.isLeader
        ? Phaser.Display.Color.IntegerToColor(gymColor).brighten(20).color
        : Phaser.Display.Color.IntegerToColor(gymColor).darken(10).color;

      const skinColor = 0xF0C890;

      // Legs
      this.npcGfx.fillStyle(0x303050, 1);
      this.npcGfx.fillRect(px + 10, py + 23, 5, 6);
      this.npcGfx.fillRect(px + 17, py + 23, 5, 6);
      // Shoes
      this.npcGfx.fillStyle(0x402020, 1);
      this.npcGfx.fillRect(px + 9, py + 28, 6, 3);
      this.npcGfx.fillRect(px + 17, py + 28, 6, 3);
      // Body
      this.npcGfx.fillStyle(bodyColor, 1);
      this.npcGfx.fillRoundedRect(px + 8, py + 12, 16, 12, 3);
      // Head
      this.npcGfx.fillStyle(skinColor, 1);
      this.npcGfx.fillCircle(px + 16, py + 8, 6);
      // Hair (leaders get distinct style)
      if (npc.isLeader) {
        this.npcGfx.fillStyle(gymColor, 1);
        this.npcGfx.fillRect(px + 9, py + 1, 14, 5);
        // Crown/headpiece
        this.npcGfx.fillStyle(0xC89840, 1);
        this.npcGfx.fillTriangle(px + 12, py, px + 16, py - 4, px + 20, py);
      } else {
        this.npcGfx.fillStyle(0x404060, 1);
        this.npcGfx.fillRect(px + 9, py + 1, 14, 5);
      }
      // Eyes
      this.npcGfx.fillStyle(0x202020, 1);
      this.npcGfx.fillRect(px + 13, py + 7, 2, 2);
      this.npcGfx.fillRect(px + 18, py + 7, 2, 2);

      // If defeated, gray out slightly
      if (defeated && !npc.isLeader) {
        this.npcGfx.fillStyle(0x000000, 0.3);
        this.npcGfx.fillRect(px, py, TILE_SIZE, TILE_SIZE);
      }
    }
  }

  private drawPlayer(): void {
    this.playerGfx.clear();
    const px = this.playerPixelX;
    const py = this.playerPixelY;

    // Shadow
    this.playerGfx.fillStyle(0x000000, 0.2);
    this.playerGfx.fillEllipse(px + TILE_SIZE / 2, py + TILE_SIZE - 2, 20, 6);

    const skinColor = 0xF0C890;
    const hatColor = 0xD03030;
    const shirtColor = 0x3060C0;
    const pantsColor = 0x303050;
    const shoeColor = 0x402020;
    const hairColor = 0x604020;

    // Legs
    this.playerGfx.fillStyle(pantsColor, 1);
    this.playerGfx.fillRect(px + 9, py + 22, 6, 7);
    this.playerGfx.fillRect(px + 17, py + 22, 6, 7);
    // Shoes
    this.playerGfx.fillStyle(shoeColor, 1);
    this.playerGfx.fillRect(px + 8, py + 28, 7, 3);
    this.playerGfx.fillRect(px + 17, py + 28, 7, 3);
    // Body
    this.playerGfx.fillStyle(shirtColor, 1);
    this.playerGfx.fillRoundedRect(px + 7, py + 12, 18, 12, 3);
    // Head
    this.playerGfx.fillStyle(skinColor, 1);
    this.playerGfx.fillCircle(px + TILE_SIZE / 2, py + 8, 7);
    // Hair
    this.playerGfx.fillStyle(hairColor, 1);
    this.playerGfx.fillRect(px + 8, py + 1, 16, 5);
    // Hat
    this.playerGfx.fillStyle(hatColor, 1);
    this.playerGfx.fillRoundedRect(px + 6, py, 20, 5, 2);
    this.playerGfx.fillRect(px + 10, py - 2, 12, 4);

    if (this.playerDirection === Direction.Down) {
      this.playerGfx.fillStyle(0x202020, 1);
      this.playerGfx.fillRect(px + 12, py + 7, 2, 2);
      this.playerGfx.fillRect(px + 18, py + 7, 2, 2);
    }
  }

  private showGymBanner(): void {
    const leaderName = getGymLeader(this.gymId)?.name ?? '???';
    const banner = this.add.container(GAME_WIDTH / 2, -40).setDepth(200).setScrollFactor(0);

    const bg = this.add.graphics();
    bg.fillStyle(COLORS.UI_BG, 0.9);
    bg.fillRoundedRect(-140, 0, 280, 32, 6);
    bg.lineStyle(2, COLORS.UI_BORDER, 1);
    bg.strokeRoundedRect(-140, 0, 280, 32, 6);
    banner.add(bg);

    const text = this.add.text(0, 8, `Arene - Champion: ${leaderName}`, {
      fontFamily: FONT_FAMILY,
      fontSize: '12px',
      color: '#F0E8D0',
    }).setOrigin(0.5, 0);
    banner.add(text);

    this.uiContainer.add(banner);

    this.tweens.add({
      targets: banner,
      y: 10,
      duration: 400,
      ease: 'Back.easeOut',
    });
    this.time.delayedCall(2500, () => {
      this.tweens.add({
        targets: banner,
        y: -40,
        duration: 300,
        ease: 'Power2',
      });
    });
  }

  // ============================================================
  // Collision
  // ============================================================
  private isCollision(x: number, y: number): boolean {
    if (x < 1 || x >= GYM_W - 1 || y < 1 || y >= GYM_H) return true;

    // Bottom wall (except door)
    if (y === GYM_H - 1 && x !== 7) return true;

    // Statues
    if ((x === 2 && y === 3) || (x === 12 && y === 3) ||
        (x === 2 && y === 8) || (x === 12 && y === 8)) return true;

    // NPCs
    if (this.gymNPCs.some(n => n.x === x && n.y === y)) return true;

    return false;
  }

  // ============================================================
  // Movement
  // ============================================================
  private movePlayer(dir: Direction): void {
    if (this.isMoving || this.dialogue.isActive() || this.battlePending) return;

    this.playerDirection = dir;

    let dx = 0, dy = 0;
    switch (dir) {
      case Direction.Up: dy = -1; break;
      case Direction.Down: dy = 1; break;
      case Direction.Left: dx = -1; break;
      case Direction.Right: dx = 1; break;
    }

    const newX = this.playerTileX + dx;
    const newY = this.playerTileY + dy;

    // Exit through door
    if (newY >= GYM_H && newX === 7) {
      this.exitGym();
      return;
    }

    if (this.isCollision(newX, newY)) {
      this.drawPlayer();
      return;
    }

    this.isMoving = true;
    const endX = newX * TILE_SIZE;
    const endY = newY * TILE_SIZE;
    this.playerTileX = newX;
    this.playerTileY = newY;

    const tweenTarget = { px: this.playerPixelX, py: this.playerPixelY };
    this.tweens.add({
      targets: tweenTarget,
      px: endX,
      py: endY,
      duration: PLAYER_MOVE_DURATION,
      ease: 'Linear',
      onUpdate: () => {
        this.playerPixelX = tweenTarget.px;
        this.playerPixelY = tweenTarget.py;
      },
      onComplete: () => {
        this.playerPixelX = endX;
        this.playerPixelY = endY;
        this.isMoving = false;
      },
    });
  }

  // ============================================================
  // Interaction
  // ============================================================
  private interact(): void {
    if (this.dialogue.isActive() || this.battlePending) return;

    let facingX = this.playerTileX;
    let facingY = this.playerTileY;
    switch (this.playerDirection) {
      case Direction.Up: facingY--; break;
      case Direction.Down: facingY++; break;
      case Direction.Left: facingX--; break;
      case Direction.Right: facingX++; break;
    }

    const npc = this.gymNPCs.find(n => n.x === facingX && n.y === facingY);
    if (!npc || !npc.trainerData) return;

    const gs = GameState.getInstance();
    const defeated = gs.isTrainerDefeated(npc.trainerId);

    if (defeated) {
      // Show after-battle dialogue
      this.dialogue.showText(npc.trainerData.dialogue.after, undefined, npc.trainerData.name);
      return;
    }

    // Show exclamation mark
    this.showExclamation(npc.x, npc.y, () => {
      // Show before-battle dialogue, then start battle
      this.dialogue.showText(npc.trainerData!.dialogue.before, () => {
        this.startTrainerBattle(npc);
      }, npc.trainerData!.name);
    });
  }

  private showExclamation(tileX: number, tileY: number, callback: () => void): void {
    this.exclamationGfx.clear();
    const px = tileX * TILE_SIZE + TILE_SIZE / 2;
    const py = tileY * TILE_SIZE - 8;

    // Draw "!" bubble
    this.exclamationGfx.fillStyle(0xFFFFFF, 1);
    this.exclamationGfx.fillRoundedRect(px - 8, py - 16, 16, 20, 4);
    this.exclamationGfx.fillTriangle(px - 3, py + 4, px + 3, py + 4, px, py + 8);

    // "!" text
    const excText = this.add.text(px, py - 8, '!', {
      fontFamily: FONT_FAMILY,
      fontSize: '14px',
      color: '#D03030',
    }).setOrigin(0.5, 0.5).setDepth(16);

    this.time.delayedCall(600, () => {
      this.exclamationGfx.clear();
      excText.destroy();
      callback();
    });
  }

  private startTrainerBattle(npc: GymNPCSlot): void {
    if (!npc.trainerData) return;
    this.battlePending = true;

    const trainer = npc.trainerData;
    const gs = GameState.getInstance();
    const playerParty = gs.party;

    // Get player's lead dino
    const leadDino = playerParty[0];
    if (!leadDino) return;

    TransitionFX.battleTransition(this, () => {
      this.scene.start(SCENE_KEYS.BATTLE, {
        isWild: false,
        trainerId: trainer.id,
        trainerName: trainer.name,
        trainerClass: trainer.trainerClass,
        trainerParty: trainer.party,
        trainerDialogueAfter: trainer.dialogue.after,
        reward: trainer.reward,
        badge: trainer.badge,
        tmReward: trainer.tmReward,
        returnScene: SCENE_KEYS.GYM,
        returnData: {
          gymId: this.gymId,
          returnMapId: this.returnMapId,
          returnX: this.returnX,
          returnY: this.returnY,
        },
        wildDino: {
          name: trainer.party[0].name,
          level: trainer.party[0].level,
          types: trainer.party[0].types,
          currentHp: 12 + trainer.party[0].level * 3,
          maxHp: 12 + trainer.party[0].level * 3,
          moves: trainer.party[0].moves.map(m => ({
            name: m.name,
            type: m.type,
            power: m.power,
            pp: m.pp,
            maxPp: m.maxPp,
          })),
        },
      });
    });
  }

  private exitGym(): void {
    TransitionFX.fadeOut(this, 300, () => {
      this.scene.start(SCENE_KEYS.OVERWORLD, {
        hasStarter: true,
        mapId: this.returnMapId,
        playerX: this.returnX,
        playerY: this.returnY,
      });
    });
  }

  // ============================================================
  // Update
  // ============================================================
  update(): void {
    this.dialogue.update();

    if (this.enterKey && Phaser.Input.Keyboard.JustDown(this.enterKey)) {
      if (!this.dialogue.isActive()) {
        this.interact();
      }
    }

    if (!this.isMoving && !this.dialogue.isActive() && !this.battlePending) {
      if (this.cursors.up.isDown) this.movePlayer(Direction.Up);
      else if (this.cursors.down.isDown) this.movePlayer(Direction.Down);
      else if (this.cursors.left.isDown) this.movePlayer(Direction.Left);
      else if (this.cursors.right.isDown) this.movePlayer(Direction.Right);
    }

    this.drawPlayer();

    this.cameras.main.centerOn(
      this.playerPixelX + TILE_SIZE / 2,
      this.playerPixelY + TILE_SIZE / 2
    );
  }
}
