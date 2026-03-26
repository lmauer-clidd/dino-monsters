// ============================================================
// Jurassic Trainers — EliteFourScene
// Linear sequence: 4 Elite Four rooms + Champion room
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
  FONT_SMALL,
  FONT_TINY,
  Direction,
  DinoType,
  DINO_TYPE_COLORS,
} from '../utils/constants';
import { DialogueBox } from '../ui/DialogueBox';
import { TransitionFX } from '../ui/TransitionFX';
import { GameState } from '../systems/GameState';
import {
  getEliteFourMember,
  getChampion,
  TrainerData,
} from '../data/trainers';

// Room layout: 11 wide x 9 tall
const ROOM_W = 11;
const ROOM_H = 9;

// Elite 4 type colors for each room
const E4_ROOM_COLORS: number[] = [
  0xC8A040, // TERRA - Earth (amber/brown)
  0x504060, // SHADOW - Shadow (dark purple)
  0xF0E870, // LUMEN - Light (gold)
  0xB0B0C8, // CHROME - Metal (silver)
  0xD8A020, // GENESIS - Champion (gold)
];

const E4_MEMBER_NAMES = ['TERRA', 'SHADOW', 'LUMEN', 'CHROME', 'GENESIS'];

export class EliteFourScene extends Phaser.Scene {
  // Current room index (0-3 = Elite 4, 4 = Champion)
  private currentRoom = 0;

  // Graphics
  private tileGfx!: Phaser.GameObjects.Graphics;
  private npcGfx!: Phaser.GameObjects.Graphics;
  private playerGfx!: Phaser.GameObjects.Graphics;
  private uiContainer!: Phaser.GameObjects.Container;

  // Player
  private playerTileX = 5;
  private playerTileY = 7;
  private playerDirection = Direction.Up;
  private isMoving = false;
  private playerPixelX = 0;
  private playerPixelY = 0;

  // NPC (current E4 member or champion)
  private memberData?: TrainerData;
  private memberDefeated = false;

  // Nurse NPC for healing between rooms
  private hasNurse = false;
  private nurseX = 8;
  private nurseY = 6;

  // UI
  private dialogue!: DialogueBox;

  // Input
  private cursors!: Phaser.Types.Input.Keyboard.CursorKeys;
  private enterKey!: Phaser.Input.Keyboard.Key;

  // State
  private battlePending = false;
  private hallOfFameShown = false;

  constructor() {
    super({ key: SCENE_KEYS.ELITE_FOUR });
  }

  init(data?: { room?: number }): void {
    this.currentRoom = data?.room ?? 0;
  }

  create(): void {
    this.battlePending = false;
    this.isMoving = false;
    this.hallOfFameShown = false;

    // Get current member data
    if (this.currentRoom < 4) {
      this.memberData = getEliteFourMember(this.currentRoom);
    } else {
      this.memberData = getChampion();
    }

    const gs = GameState.getInstance();
    this.memberDefeated = this.memberData ? gs.isTrainerDefeated(this.memberData.id) : false;

    // Show nurse in rooms 1-4 (after first E4 member, before each battle)
    this.hasNurse = this.currentRoom > 0 && this.currentRoom <= 4;

    // Graphics
    this.tileGfx = this.add.graphics();
    this.npcGfx = this.add.graphics().setDepth(10);
    this.playerGfx = this.add.graphics().setDepth(20);
    this.uiContainer = this.add.container(0, 0).setDepth(100).setScrollFactor(0);

    this.drawRoom();
    this.drawNPCs();

    // Player position
    this.playerTileX = 5;
    this.playerTileY = 7;
    this.playerDirection = Direction.Up;
    this.playerPixelX = this.playerTileX * TILE_SIZE;
    this.playerPixelY = this.playerTileY * TILE_SIZE;

    // Camera
    this.cameras.main.setBounds(0, 0, ROOM_W * TILE_SIZE, ROOM_H * TILE_SIZE);
    this.cameras.main.setBackgroundColor(COLORS.BLACK);

    // UI
    this.dialogue = new DialogueBox(this);

    // Input
    if (this.input.keyboard) {
      this.cursors = this.input.keyboard.createCursorKeys();
      this.enterKey = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.ENTER);
    }

    // Banner
    this.showRoomBanner();

    TransitionFX.fadeIn(this, 400);
  }

  // ============================================================
  // Drawing
  // ============================================================
  private drawRoom(): void {
    this.tileGfx.clear();
    const roomColor = E4_ROOM_COLORS[this.currentRoom] ?? 0x808080;
    const isChampionRoom = this.currentRoom === 4;

    for (let y = 0; y < ROOM_H; y++) {
      for (let x = 0; x < ROOM_W; x++) {
        const px = x * TILE_SIZE;
        const py = y * TILE_SIZE;

        // Walls
        if (y === 0 || x === 0 || x === ROOM_W - 1) {
          this.tileGfx.fillStyle(Phaser.Display.Color.IntegerToColor(roomColor).darken(40).color, 1);
          this.tileGfx.fillRect(px, py, TILE_SIZE, TILE_SIZE);
          // Decorative pillar tops
          if ((x === 0 || x === ROOM_W - 1) && y % 2 === 0) {
            this.tileGfx.fillStyle(roomColor, 0.3);
            this.tileGfx.fillRect(px + 4, py + 4, TILE_SIZE - 8, TILE_SIZE - 8);
          }
          continue;
        }

        // Bottom wall with door
        if (y === ROOM_H - 1) {
          if (x === 5) {
            // Door
            this.tileGfx.fillStyle(0x604020, 1);
            this.tileGfx.fillRect(px, py, TILE_SIZE, TILE_SIZE);
            this.tileGfx.fillStyle(0x806030, 1);
            this.tileGfx.fillRect(px + 2, py, TILE_SIZE - 4, TILE_SIZE);
          } else {
            this.tileGfx.fillStyle(Phaser.Display.Color.IntegerToColor(roomColor).darken(40).color, 1);
            this.tileGfx.fillRect(px, py, TILE_SIZE, TILE_SIZE);
          }
          continue;
        }

        // Champion room: special golden floor
        if (isChampionRoom) {
          const isCenter = x >= 3 && x <= 7 && y >= 1 && y <= 5;
          this.tileGfx.fillStyle(isCenter ? 0x302818 : 0x201810, 1);
          this.tileGfx.fillRect(px, py, TILE_SIZE, TILE_SIZE);
          if (isCenter) {
            this.tileGfx.fillStyle(roomColor, 0.15);
            this.tileGfx.fillRect(px, py, TILE_SIZE, TILE_SIZE);
          }
          // Diamond pattern
          if ((x + y) % 2 === 0) {
            this.tileGfx.fillStyle(roomColor, 0.08);
            this.tileGfx.fillRect(px + 4, py + 4, TILE_SIZE - 8, TILE_SIZE - 8);
          }
          continue;
        }

        // Regular E4 room floor
        this.tileGfx.fillStyle(0x201820, 1);
        this.tileGfx.fillRect(px, py, TILE_SIZE, TILE_SIZE);
        // Subtle type-colored accent
        this.tileGfx.fillStyle(roomColor, 0.08);
        this.tileGfx.fillRect(px, py, TILE_SIZE, TILE_SIZE);
        // Tile grid
        this.tileGfx.lineStyle(1, roomColor, 0.1);
        this.tileGfx.strokeRect(px, py, TILE_SIZE, TILE_SIZE);
      }
    }

    // Member platform at top center
    const platX = 4 * TILE_SIZE;
    const platY = 2 * TILE_SIZE;
    this.tileGfx.fillStyle(roomColor, 0.3);
    this.tileGfx.fillRoundedRect(platX, platY, 3 * TILE_SIZE, 2 * TILE_SIZE, 4);
    this.tileGfx.lineStyle(2, roomColor, 0.5);
    this.tileGfx.strokeRoundedRect(platX, platY, 3 * TILE_SIZE, 2 * TILE_SIZE, 4);
  }

  private drawNPCs(): void {
    this.npcGfx.clear();

    // Draw E4 member / Champion at top center
    if (this.memberData) {
      const mx = 5 * TILE_SIZE;
      const my = 2 * TILE_SIZE;
      const roomColor = E4_ROOM_COLORS[this.currentRoom];
      const skinColor = 0xF0C890;

      // Shadow
      this.npcGfx.fillStyle(0x000000, 0.3);
      this.npcGfx.fillEllipse(mx + TILE_SIZE / 2, my + TILE_SIZE - 2, 20, 6);

      // Body
      const bodyColor = this.currentRoom === 4 ? 0xC89840 : roomColor;
      this.npcGfx.fillStyle(0x303050, 1);
      this.npcGfx.fillRect(mx + 10, my + 23, 5, 6);
      this.npcGfx.fillRect(mx + 17, my + 23, 5, 6);
      this.npcGfx.fillStyle(0x402020, 1);
      this.npcGfx.fillRect(mx + 9, my + 28, 6, 3);
      this.npcGfx.fillRect(mx + 17, my + 28, 6, 3);
      this.npcGfx.fillStyle(bodyColor, 1);
      this.npcGfx.fillRoundedRect(mx + 7, my + 12, 18, 12, 3);
      // Cape for champion
      if (this.currentRoom === 4) {
        this.npcGfx.fillStyle(0xA02020, 0.8);
        this.npcGfx.fillTriangle(mx + 5, my + 14, mx + 16, my + 14, mx + 3, my + 30);
        this.npcGfx.fillTriangle(mx + 27, my + 14, mx + 16, my + 14, mx + 29, my + 30);
      }
      this.npcGfx.fillStyle(skinColor, 1);
      this.npcGfx.fillCircle(mx + 16, my + 8, 7);
      // Hair
      this.npcGfx.fillStyle(bodyColor, 1);
      this.npcGfx.fillRect(mx + 8, my + 1, 16, 5);
      // Crown/headpiece
      this.npcGfx.fillStyle(0xD8A820, 1);
      this.npcGfx.fillTriangle(mx + 10, my, mx + 16, my - 6, mx + 22, my);
      if (this.currentRoom === 4) {
        // Extra crown details for champion
        this.npcGfx.fillStyle(0xF0D040, 1);
        this.npcGfx.fillCircle(mx + 16, my - 4, 3);
      }
      // Eyes
      this.npcGfx.fillStyle(0x202020, 1);
      this.npcGfx.fillRect(mx + 12, my + 7, 2, 2);
      this.npcGfx.fillRect(mx + 18, my + 7, 2, 2);
    }

    // Draw nurse if present
    if (this.hasNurse) {
      const nx = this.nurseX * TILE_SIZE;
      const ny = this.nurseY * TILE_SIZE;
      const skinColor = 0xF0C890;

      this.npcGfx.fillStyle(0x000000, 0.2);
      this.npcGfx.fillEllipse(nx + TILE_SIZE / 2, ny + TILE_SIZE - 2, 18, 5);

      this.npcGfx.fillStyle(0x303050, 1);
      this.npcGfx.fillRect(nx + 10, ny + 24, 5, 5);
      this.npcGfx.fillRect(nx + 17, ny + 24, 5, 5);
      this.npcGfx.fillStyle(0xF0F0F0, 1);
      this.npcGfx.fillRoundedRect(nx + 7, ny + 10, 18, 14, 3);
      // Red cross
      this.npcGfx.fillStyle(0xD03030, 1);
      this.npcGfx.fillRect(nx + 14, ny + 13, 4, 8);
      this.npcGfx.fillRect(nx + 12, ny + 15, 8, 4);
      this.npcGfx.fillStyle(skinColor, 1);
      this.npcGfx.fillCircle(nx + 16, ny + 8, 6);
      this.npcGfx.fillStyle(0xE06090, 1);
      this.npcGfx.fillRect(nx + 9, ny + 1, 14, 5);
      this.npcGfx.fillRect(nx + 8, ny + 4, 4, 8);
      this.npcGfx.fillRect(nx + 20, ny + 4, 4, 8);
      this.npcGfx.fillStyle(0x202020, 1);
      this.npcGfx.fillRect(nx + 13, ny + 7, 2, 2);
      this.npcGfx.fillRect(nx + 18, ny + 7, 2, 2);
    }
  }

  private drawPlayer(): void {
    this.playerGfx.clear();
    const px = this.playerPixelX;
    const py = this.playerPixelY;

    this.playerGfx.fillStyle(0x000000, 0.2);
    this.playerGfx.fillEllipse(px + TILE_SIZE / 2, py + TILE_SIZE - 2, 20, 6);

    const skinColor = 0xF0C890;
    const hatColor = 0xD03030;
    const shirtColor = 0x3060C0;
    const pantsColor = 0x303050;
    const shoeColor = 0x402020;
    const hairColor = 0x604020;

    this.playerGfx.fillStyle(pantsColor, 1);
    this.playerGfx.fillRect(px + 9, py + 22, 6, 7);
    this.playerGfx.fillRect(px + 17, py + 22, 6, 7);
    this.playerGfx.fillStyle(shoeColor, 1);
    this.playerGfx.fillRect(px + 8, py + 28, 7, 3);
    this.playerGfx.fillRect(px + 17, py + 28, 7, 3);
    this.playerGfx.fillStyle(shirtColor, 1);
    this.playerGfx.fillRoundedRect(px + 7, py + 12, 18, 12, 3);
    this.playerGfx.fillStyle(skinColor, 1);
    this.playerGfx.fillCircle(px + TILE_SIZE / 2, py + 8, 7);
    this.playerGfx.fillStyle(hairColor, 1);
    this.playerGfx.fillRect(px + 8, py + 1, 16, 5);
    this.playerGfx.fillStyle(hatColor, 1);
    this.playerGfx.fillRoundedRect(px + 6, py, 20, 5, 2);
    this.playerGfx.fillRect(px + 10, py - 2, 12, 4);

    if (this.playerDirection === Direction.Down || this.playerDirection === Direction.Up) {
      if (this.playerDirection === Direction.Down) {
        this.playerGfx.fillStyle(0x202020, 1);
        this.playerGfx.fillRect(px + 12, py + 7, 2, 2);
        this.playerGfx.fillRect(px + 18, py + 7, 2, 2);
      }
    }
  }

  private showRoomBanner(): void {
    const name = E4_MEMBER_NAMES[this.currentRoom] ?? '???';
    const label = this.currentRoom < 4 ? `Elite 4 - ${name}` : `Champion - ${name}`;

    const banner = this.add.container(GAME_WIDTH / 2, -40).setDepth(200).setScrollFactor(0);
    const bg = this.add.graphics();
    bg.fillStyle(COLORS.UI_BG, 0.9);
    bg.fillRoundedRect(-120, 0, 240, 32, 6);
    bg.lineStyle(2, COLORS.UI_BORDER, 1);
    bg.strokeRoundedRect(-120, 0, 240, 32, 6);
    banner.add(bg);

    const text = this.add.text(0, 8, label, {
      fontFamily: FONT_FAMILY,
      fontSize: '14px',
      color: '#F0E8D0',
    }).setOrigin(0.5, 0);
    banner.add(text);

    this.uiContainer.add(banner);

    this.tweens.add({ targets: banner, y: 10, duration: 400, ease: 'Back.easeOut' });
    this.time.delayedCall(2500, () => {
      this.tweens.add({ targets: banner, y: -40, duration: 300, ease: 'Power2' });
    });
  }

  // ============================================================
  // Collision
  // ============================================================
  private isCollision(x: number, y: number): boolean {
    if (x < 1 || x >= ROOM_W - 1 || y < 1 || y >= ROOM_H) return true;
    if (y === ROOM_H - 1 && x !== 5) return true;

    // Member NPC
    if (x === 5 && y === 2) return true;

    // Nurse
    if (this.hasNurse && x === this.nurseX && y === this.nurseY) return true;

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

    // Exit: go back (lose = sent to Dino Center, or after clearing go to next room)
    if (newY >= ROOM_H && newX === 5) {
      if (this.memberDefeated) {
        this.advanceToNextRoom();
      } else {
        // Can't go back once inside (thematic)
        return;
      }
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

    // Nurse interaction
    if (this.hasNurse && facingX === this.nurseX && facingY === this.nurseY) {
      this.dialogue.showText(
        "Bienvenue ! Vos dinos sont soignes ! ... Bonne chance pour la suite !",
        () => {
          // Heal party (simplified: just mark flag)
          const gs = GameState.getInstance();
          // In real implementation, heal all party dinos
          TransitionFX.flashScreen(this, 0x48D848, 200);
        },
        'Infirmiere'
      );
      return;
    }

    // E4 member / Champion interaction
    if (facingX === 5 && facingY === 2 && this.memberData) {
      if (this.memberDefeated) {
        this.dialogue.showText(this.memberData.dialogue.after, undefined, this.memberData.name);
        return;
      }

      // Start battle
      this.dialogue.showText(this.memberData.dialogue.before, () => {
        this.startE4Battle();
      }, this.memberData.name);
    }
  }

  private startE4Battle(): void {
    if (!this.memberData) return;
    this.battlePending = true;

    const trainer = this.memberData;

    TransitionFX.battleTransition(this, () => {
      this.scene.start(SCENE_KEYS.BATTLE, {
        isWild: false,
        trainerId: trainer.id,
        trainerName: trainer.name,
        trainerClass: trainer.trainerClass,
        trainerParty: trainer.party,
        trainerDialogueAfter: trainer.dialogue.after,
        reward: trainer.reward,
        isEliteFour: true,
        isChampion: this.currentRoom === 4,
        returnScene: SCENE_KEYS.ELITE_FOUR,
        returnData: {
          room: this.currentRoom,
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

  private advanceToNextRoom(): void {
    if (this.currentRoom < 4) {
      // Next E4 member or Champion
      TransitionFX.fadeOut(this, 400, () => {
        this.scene.restart({ room: this.currentRoom + 1 });
      });
    } else {
      // Champion defeated! Show Hall of Fame
      this.showHallOfFame();
    }
  }

  // ============================================================
  // Hall of Fame & Credits
  // ============================================================
  private showHallOfFame(): void {
    if (this.hallOfFameShown) return;
    this.hallOfFameShown = true;

    const gs = GameState.getInstance();
    gs.setFlag('champion_defeated', true);

    // Overlay
    const overlay = this.add.graphics().setDepth(500);
    overlay.fillStyle(0x000000, 0);
    overlay.fillRect(0, 0, GAME_WIDTH, GAME_HEIGHT);
    overlay.setScrollFactor(0);

    // Fade to black
    this.tweens.add({
      targets: overlay,
      alpha: 1,
      duration: 1000,
      onComplete: () => {
        this.showCelebration(overlay);
      },
    });
  }

  private showCelebration(overlay: Phaser.GameObjects.Graphics): void {
    const gs = GameState.getInstance();

    // Title
    const titleText = this.add.text(GAME_WIDTH / 2, 30, 'TEMPLE DE LA GLOIRE', {
      fontFamily: FONT_FAMILY,
      fontSize: '20px',
      color: '#F8D830',
      shadow: { offsetX: 2, offsetY: 2, color: '#000000', blur: 0, fill: true },
    }).setOrigin(0.5, 0).setDepth(501).setScrollFactor(0);

    // "You are the new champion" text
    const champText = this.add.text(GAME_WIDTH / 2, 60, `${gs.player.name} est le nouveau\nChampion de Pangaea !`, {
      fontFamily: FONT_FAMILY,
      fontSize: '14px',
      color: '#F0E8D0',
      align: 'center',
      shadow: { offsetX: 1, offsetY: 1, color: '#000000', blur: 0, fill: true },
    }).setOrigin(0.5, 0).setDepth(501).setScrollFactor(0);

    // Show party dinos (simplified: colored circles representing each party member)
    const partyGfx = this.add.graphics().setDepth(501).setScrollFactor(0);
    const party = gs.party;
    const startX = GAME_WIDTH / 2 - (party.length * 50) / 2;

    for (let i = 0; i < party.length; i++) {
      const d = party[i];
      const cx = startX + i * 50 + 25;
      const cy = 160;

      // Platform
      partyGfx.fillStyle(0x404040, 1);
      partyGfx.fillEllipse(cx, cy + 30, 40, 10);

      // Dino representation (colored circle with type)
      const typeColor = DINO_TYPE_COLORS[d.type1] ?? 0xA8A878;
      partyGfx.fillStyle(typeColor, 1);
      partyGfx.fillCircle(cx, cy, 18);
      partyGfx.fillStyle(Phaser.Display.Color.IntegerToColor(typeColor).brighten(30).color, 0.5);
      partyGfx.fillCircle(cx - 4, cy - 4, 8);

      // Name
      this.add.text(cx, cy + 38, d.nickname || d.species.name, {
        fontFamily: FONT_FAMILY,
        fontSize: '10px',
        color: '#F0E8D0',
      }).setOrigin(0.5, 0).setDepth(501).setScrollFactor(0);

      // Level
      this.add.text(cx, cy + 50, `Lv ${d.level}`, {
        fontFamily: FONT_FAMILY,
        fontSize: '10px',
        color: '#B8B0A0',
      }).setOrigin(0.5, 0).setDepth(501).setScrollFactor(0);
    }

    // Sparkle effects
    for (let s = 0; s < 20; s++) {
      const sx = Phaser.Math.Between(20, GAME_WIDTH - 20);
      const sy = Phaser.Math.Between(20, GAME_HEIGHT - 20);
      const star = this.add.text(sx, sy, '\u2605', {
        fontFamily: FONT_FAMILY,
        fontSize: `${Phaser.Math.Between(8, 16)}px`,
        color: '#F8D830',
      }).setDepth(501).setScrollFactor(0).setAlpha(0);

      this.tweens.add({
        targets: star,
        alpha: 1,
        duration: 500,
        delay: s * 100,
        yoyo: true,
        repeat: 2,
      });
    }

    // After celebration, show credits
    this.time.delayedCall(5000, () => {
      this.showCredits();
    });
  }

  private showCredits(): void {
    // Clear the screen
    const creditsBg = this.add.graphics().setDepth(600).setScrollFactor(0);
    creditsBg.fillStyle(0x000000, 1);
    creditsBg.fillRect(0, 0, GAME_WIDTH, GAME_HEIGHT);

    const credits = [
      'JURASSIC TRAINERS',
      '',
      'Developpe par Nova Forge Studio',
      '',
      '--- EQUIPE ---',
      '',
      'Directeur Creatif: KAELEN',
      'Narrative Designer: YUKI',
      'Game Designer: NOVA',
      'Lead Programmer: CIPHER',
      'UI/UX Designer: PIXEL',
      'Sound Designer: ECHO',
      'Artist: AURORA',
      'Level Designer: ATLAS',
      'QA Lead: GLITCH',
      'Producer: NEXUS',
      'Music: HARMONY',
      'Animation: MOTION',
      'AI Programmer: SAGE',
      'Community: PULSE',
      'Marketing: BLAZE',
      'Localization: LINGUA',
      '',
      'Merci d\'avoir joue !',
      '',
      'FIN',
    ];

    const creditsText = this.add.text(GAME_WIDTH / 2, GAME_HEIGHT + 20, credits.join('\n'), {
      fontFamily: FONT_FAMILY,
      fontSize: '14px',
      color: '#F0E8D0',
      align: 'center',
      lineSpacing: 8,
      shadow: { offsetX: 1, offsetY: 1, color: '#000000', blur: 0, fill: true },
    }).setOrigin(0.5, 0).setDepth(601).setScrollFactor(0);

    // Scroll credits up
    this.tweens.add({
      targets: creditsText,
      y: -creditsText.height - 50,
      duration: 15000,
      ease: 'Linear',
      onComplete: () => {
        // Return to Bourg-Nid
        TransitionFX.fadeOut(this, 1000, () => {
          this.scene.start(SCENE_KEYS.OVERWORLD, {
            hasStarter: true,
            mapId: 'BOURG_NID',
            playerX: 14,
            playerY: 18,
          });
        });
      },
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

    if (!this.isMoving && !this.dialogue.isActive() && !this.battlePending && !this.hallOfFameShown) {
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
