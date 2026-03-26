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
import { MenuUI } from '../ui/MenuUI';
import { TransitionFX } from '../ui/TransitionFX';
import { GameState } from '../systems/GameState';
import { EventSystem } from '../systems/EventSystem';
import { getMap, TileType, MapData, MapNPC, MapWarp, WildEncounterTable, BuildingData } from '../data/maps';
import { getTrainer, getRouteTrainers, TrainerData } from '../data/trainers';
import { Dino, getSpecies } from '../entities/Dino';
import { hasMove, getMove } from '../systems/BattleSystem';
import { AudioSystem } from '../systems/AudioSystem';

// ============================================================
// Tile palette — kept for rendering colors
// ============================================================
const TILE_BASE_COLORS: Record<number, number> = {
  [TileType.Grass]: 0x58A848,
  [TileType.Path]: 0xD8C888,
  [TileType.TallGrass]: 0x388828,
  [TileType.Water]: 0x4890D8,
  [TileType.Tree]: 0x286828,
  [TileType.Wall]: 0xD8C0A0,
  [TileType.Roof]: 0xB84030,
  [TileType.Door]: 0x604020,
  [TileType.Fence]: 0xC8A868,
  [TileType.Flower]: 0x58A848,
  [TileType.Sign]: 0x58A848,
  [TileType.Ledge]: 0x58A848,
  [TileType.Sand]: 0xE8D8A0,
  [TileType.Ice]: 0xC0E8F8,
  [TileType.Lava]: 0xD04020,
  [TileType.Bridge]: 0xA08050,
};

// ============================================================
// NPC runtime data
// ============================================================
interface NPCRuntime {
  data: MapNPC;
  bodyColor: number;
  hairColor: number;
  dialogueIndex: number;
}

// Sprite type -> body/hair color mapping
const SPRITE_COLORS: Record<string, { body: number; hair: number }> = {
  mom: { body: 0xE06090, hair: 0x704030 },
  professor: { body: 0xE0E0F0, hair: 0x606080 },
  assistant: { body: 0xE0E0F0, hair: 0x404060 },
  trainer: { body: 0xC0A030, hair: 0x302010 },
  villager: { body: 0x60A040, hair: 0x604030 },
  shopkeeper: { body: 0x4080C0, hair: 0x403020 },
  nurse: { body: 0xF0A0B0, hair: 0xE06090 },
  rival: { body: 0x303060, hair: 0x202020 },
  grunt: { body: 0x505050, hair: 0x303030 },
};

// ============================================================
// OverworldScene — Multi-Map System
// ============================================================
export class OverworldScene extends Phaser.Scene {
  // Current map data
  private currentMap!: MapData;
  private currentMapId = 'BOURG_NID';

  // Map layers (derived from map data)
  private collisionSet!: Set<number>;
  private encounterSet!: Set<number>;

  // Graphics
  private mapContainer!: Phaser.GameObjects.Container;
  private tileGfx!: Phaser.GameObjects.Graphics;
  private tileDetailGfx!: Phaser.GameObjects.Graphics;
  private playerContainer!: Phaser.GameObjects.Container;
  private playerGfx!: Phaser.GameObjects.Graphics;
  private npcContainer!: Phaser.GameObjects.Container;
  private hudContainer!: Phaser.GameObjects.Container;

  // Player
  private playerTileX = 14;
  private playerTileY = 18;
  private playerDirection = Direction.Down;
  private isMoving = false;
  private playerPixelX = 0;
  private playerPixelY = 0;

  // NPCs
  private npcs: NPCRuntime[] = [];

  // UI
  private dialogue!: DialogueBox;
  private menu!: MenuUI;

  // Location banner
  private locationBanner!: Phaser.GameObjects.Container;
  private locationShown = false;

  // Input
  private cursors!: Phaser.Types.Input.Keyboard.CursorKeys;
  private enterKey!: Phaser.Input.Keyboard.Key;
  private escKey!: Phaser.Input.Keyboard.Key;

  // State
  private hasStarter = false;
  private encounterBlocked = false;
  private isWarping = false;
  private battlePending = false;

  // Exclamation mark graphics
  private exclamationGfx!: Phaser.GameObjects.Graphics;

  // Event system
  private eventSystem!: EventSystem;

  // Animation timer
  private animTimer = 0;

  constructor() {
    super({ key: SCENE_KEYS.OVERWORLD });
  }

  private pendingResumeEvent = false;

  init(data?: {
    hasStarter?: boolean;
    starterName?: string;
    mapId?: string;
    playerX?: number;
    playerY?: number;
    resumeEvent?: boolean;
  }): void {
    this.hasStarter = data?.hasStarter ?? this.hasStarter;

    // Fall back to GameState position if no explicit data is provided
    const gs = GameState.getInstance();
    this.currentMapId = data?.mapId ?? gs.currentMap ?? 'BOURG_NID';
    if (data?.playerX !== undefined) {
      this.playerTileX = data.playerX;
    } else if (gs.player.x !== 0 || gs.player.y !== 0) {
      this.playerTileX = gs.player.x;
    }
    if (data?.playerY !== undefined) {
      this.playerTileY = data.playerY;
    } else if (gs.player.x !== 0 || gs.player.y !== 0) {
      this.playerTileY = gs.player.y;
    }

    // If returning from a sub-scene (party, bag, etc.), restore hasStarter from GameState
    if (!data?.hasStarter && gs.party.length > 0) {
      this.hasStarter = true;
    }

    this.pendingResumeEvent = data?.resumeEvent ?? false;
  }

  create(): void {
    try {
    this.animTimer = 0;
    this.isWarping = false;
    this.locationShown = false;
    this.battlePending = false;
    // Block encounters briefly when returning from battle to prevent instant re-encounter
    this.encounterBlocked = true;
    this.time.delayedCall(1000, () => { this.encounterBlocked = false; });

    // Load map data
    this.loadMap(this.currentMapId);

    // Map container holds all tiles
    this.mapContainer = this.add.container(0, 0);
    this.tileGfx = this.add.graphics();
    this.tileDetailGfx = this.add.graphics();
    this.mapContainer.add(this.tileGfx);
    this.mapContainer.add(this.tileDetailGfx);

    this.drawMap();

    // NPC container
    this.npcContainer = this.add.container(0, 0).setDepth(10);
    this.exclamationGfx = this.add.graphics().setDepth(15);
    this.spawnNPCs();

    // Player container
    this.playerContainer = this.add.container(0, 0).setDepth(20);
    this.playerGfx = this.add.graphics();
    this.playerContainer.add(this.playerGfx);

    // Player position in pixels
    this.playerPixelX = this.playerTileX * TILE_SIZE;
    this.playerPixelY = this.playerTileY * TILE_SIZE;

    // Camera
    this.cameras.main.setBounds(
      0, 0,
      this.currentMap.width * TILE_SIZE,
      this.currentMap.height * TILE_SIZE
    );
    this.cameras.main.setBackgroundColor(COLORS.BLACK);

    // HUD overlay (fixed to camera)
    this.hudContainer = this.add.container(0, 0).setDepth(100).setScrollFactor(0);
    this.createLocationBanner();
    this.createPartyIndicator();

    // UI
    this.dialogue = new DialogueBox(this);
    this.menu = new MenuUI();

    // Event system
    this.eventSystem = new EventSystem(this, this.dialogue);

    // Listen for NPC movement events from the event system
    this.events.on('event:moveNpc', (data: { npcId: string; direction: string; tiles: number }) => {
      this.moveNpcVisual(data.npcId, data.direction, data.tiles);
    });

    // Input
    if (this.input.keyboard) {
      this.cursors = this.input.keyboard.createCursorKeys();
      this.enterKey = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.ENTER);
      this.escKey = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.ESC);
    }

    // Start music based on map type
    const audio = AudioSystem.getInstance();
    audio.init();
    if (this.currentMapId.startsWith('ROUTE_') || this.currentMapId.startsWith('GROTTE') || this.currentMapId.startsWith('TOUR')) {
      audio.playRouteMusic();
    } else {
      audio.playTownMusic();
    }

    // Fade in
    TransitionFX.fadeIn(this, 400);

    // Show location name banner
    this.showLocationBanner(this.currentMap.name);

    // If returning from a story event battle, resolve the pending promise
    if (this.pendingResumeEvent && (this as any).__eventBattleResolve) {
      const resolve = (this as any).__eventBattleResolve;
      delete (this as any).__eventBattleResolve;
      delete (this as any).__eventBattleTrainerId;
      this.time.delayedCall(200, () => resolve());
    }

    // Check for story events on map enter
    const mapEvent = this.eventSystem.checkMapEnterEvents(this.currentMapId);
    if (mapEvent) {
      this.eventSystem.executeEvent(mapEvent);
    }
    } catch (e: any) {
      console.error('OverworldScene.create() ERROR:', e.message, e.stack);
    }
  }

  // ============================================================
  // Map Loading
  // ============================================================
  private loadMap(mapId: string): void {
    this.currentMap = getMap(mapId);
    this.currentMapId = mapId;

    // Build collision and encounter sets from map data
    this.collisionSet = new Set(this.currentMap.collisionTiles);
    this.encounterSet = new Set(this.currentMap.encounterTiles);
  }

  // ============================================================
  // Warp System
  // ============================================================
  private checkWarp(x: number, y: number): MapWarp | undefined {
    return this.currentMap.warps.find(w => w.x === x && w.y === y);
  }

  private executeWarp(warp: MapWarp): void {
    if (this.isWarping) return;
    this.isWarping = true;

    // Fade out, switch map, fade in
    TransitionFX.fadeOut(this, 300, () => {
      this.scene.restart({
        hasStarter: this.hasStarter,
        mapId: warp.targetMap,
        playerX: warp.targetX,
        playerY: warp.targetY,
      });
    });
  }

  // ============================================================
  // Location Banner
  // ============================================================
  private createLocationBanner(): void {
    this.locationBanner = this.add.container(GAME_WIDTH / 2, -40).setDepth(200);
    this.locationBanner.setScrollFactor(0);

    const bannerGfx = this.add.graphics();
    bannerGfx.fillStyle(COLORS.UI_BG, 0.9);
    bannerGfx.fillRoundedRect(-120, 0, 240, 32, 6);
    bannerGfx.lineStyle(2, COLORS.UI_BORDER, 1);
    bannerGfx.strokeRoundedRect(-120, 0, 240, 32, 6);
    this.locationBanner.add(bannerGfx);

    const bannerText = this.add.text(0, 8, '', {
      fontFamily: FONT_FAMILY,
      fontSize: '14px',
      color: '#F0E8D0',
    }).setOrigin(0.5, 0);
    this.locationBanner.add(bannerText);

    this.hudContainer.add(this.locationBanner);
  }

  private showLocationBanner(name: string): void {
    if (this.locationShown) return;
    this.locationShown = true;

    const textObj = this.locationBanner.getAt(1) as Phaser.GameObjects.Text;
    textObj.setText(name);

    // Slide in
    this.tweens.add({
      targets: this.locationBanner,
      y: 10,
      duration: 400,
      ease: 'Back.easeOut',
    });

    // Slide out after 2s
    this.time.delayedCall(2500, () => {
      this.tweens.add({
        targets: this.locationBanner,
        y: -40,
        duration: 300,
        ease: 'Power2',
      });
    });
  }

  // ============================================================
  // Party indicator (top-right)
  // ============================================================
  private createPartyIndicator(): void {
    if (!this.hasStarter) return;

    const indicatorGfx = this.add.graphics();
    indicatorGfx.fillStyle(COLORS.HP_GREEN, 0.8);
    indicatorGfx.fillCircle(GAME_WIDTH - 16, 16, 5);
    indicatorGfx.lineStyle(1, 0xFFFFFF, 0.4);
    indicatorGfx.strokeCircle(GAME_WIDTH - 16, 16, 5);
    this.hudContainer.add(indicatorGfx);
  }

  // ============================================================
  // Detailed Tile Drawing
  // ============================================================
  private drawMap(): void {
    this.tileGfx.clear();
    this.tileDetailGfx.clear();

    const mapW = this.currentMap.width;
    const mapH = this.currentMap.height;

    // Use seeded random for consistent tile details
    const seedRand = (x: number, y: number, salt: number) => {
      const h = ((x * 374761393 + y * 668265263 + salt) & 0xFFFFFFFF) >>> 0;
      return (h % 1000) / 1000;
    };

    for (let y = 0; y < mapH; y++) {
      for (let x = 0; x < mapW; x++) {
        const tile = this.currentMap.tiles[y][x];
        const px = x * TILE_SIZE;
        const py = y * TILE_SIZE;

        switch (tile) {
          case TileType.Grass:
            this.drawGrassTile(px, py, x, y, seedRand);
            break;
          case TileType.Path:
            this.drawPathTile(px, py, x, y, seedRand);
            break;
          case TileType.TallGrass:
            this.drawTallGrassTile(px, py, x, y, seedRand);
            break;
          case TileType.Water:
            this.drawWaterTile(px, py, x, y);
            break;
          case TileType.Tree:
            this.drawTreeTile(px, py, x, y, seedRand);
            break;
          case TileType.Wall:
            this.drawWallTile(px, py, x, y);
            break;
          case TileType.Roof:
            this.drawRoofTile(px, py, x, y);
            break;
          case TileType.Door:
            this.drawDoorTile(px, py);
            break;
          case TileType.Fence:
            this.drawFenceTile(px, py);
            break;
          case TileType.Flower:
            this.drawFlowerTile(px, py, x, y, seedRand);
            break;
          case TileType.Sign:
            this.drawSignTile(px, py);
            break;
          case TileType.Ledge:
            this.drawLedgeTile(px, py);
            break;
          case TileType.Sand:
            this.drawSandTile(px, py, x, y, seedRand);
            break;
          case TileType.Ice:
            this.drawIceTile(px, py, x, y, seedRand);
            break;
          case TileType.Lava:
            this.drawLavaTile(px, py, x, y);
            break;
          case TileType.Bridge:
            this.drawBridgeTile(px, py, x, y);
            break;
        }
      }
    }
  }

  // --- Individual tile drawing methods ---

  private drawGrassTile(px: number, py: number, x: number, y: number, seed: (x: number, y: number, s: number) => number): void {
    this.tileGfx.fillStyle(0x58A848, 1);
    this.tileGfx.fillRect(px, py, TILE_SIZE, TILE_SIZE);
    this.tileDetailGfx.fillStyle(0x68B858, 0.5);
    if (seed(x, y, 1) > 0.5) {
      this.tileDetailGfx.fillRect(px + 4, py + 6, 3, 2);
    }
    if (seed(x, y, 2) > 0.6) {
      this.tileDetailGfx.fillRect(px + 18, py + 14, 2, 2);
    }
    this.tileDetailGfx.fillStyle(0x489838, 0.4);
    if (seed(x, y, 3) > 0.7) {
      this.tileDetailGfx.fillRect(px + 10, py + 20, 4, 3);
    }
    this.tileDetailGfx.lineStyle(1, 0x68C058, 0.4);
    if (seed(x, y, 4) > 0.4) {
      this.tileDetailGfx.lineBetween(px + 8, py + 28, px + 10, py + 24);
      this.tileDetailGfx.lineBetween(px + 22, py + 10, px + 24, py + 6);
    }
  }

  private drawPathTile(px: number, py: number, x: number, y: number, seed: (x: number, y: number, s: number) => number): void {
    this.tileGfx.fillStyle(0xD8C888, 1);
    this.tileGfx.fillRect(px, py, TILE_SIZE, TILE_SIZE);
    this.tileDetailGfx.fillStyle(0xC8B070, 0.5);
    for (let s = 0; s < 4; s++) {
      const sx = px + Math.floor(seed(x, y, 10 + s) * 28) + 2;
      const sy = py + Math.floor(seed(x, y, 20 + s) * 28) + 2;
      this.tileDetailGfx.fillRect(sx, sy, 2, 2);
    }
    this.tileDetailGfx.fillStyle(0xC0A868, 0.3);
    this.tileDetailGfx.fillRect(px, py + TILE_SIZE - 1, TILE_SIZE, 1);
  }

  private drawTallGrassTile(px: number, py: number, x: number, y: number, seed: (x: number, y: number, s: number) => number): void {
    this.tileGfx.fillStyle(0x388828, 1);
    this.tileGfx.fillRect(px, py, TILE_SIZE, TILE_SIZE);
    this.tileDetailGfx.fillStyle(0x58A848, 0.8);
    for (let gx = 3; gx < TILE_SIZE - 4; gx += 8) {
      for (let gy = 2; gy < TILE_SIZE - 6; gy += 10) {
        const offsetX = seed(x, y, gx + gy * 10) * 4 - 2;
        this.tileDetailGfx.fillRect(px + gx + offsetX, py + gy + 4, 1, 5);
        this.tileDetailGfx.fillRect(px + gx + offsetX + 3, py + gy + 4, 1, 5);
        this.tileDetailGfx.fillRect(px + gx + offsetX + 1, py + gy + 8, 2, 2);
      }
    }
    this.tileDetailGfx.fillStyle(0x68C048, 0.6);
    this.tileDetailGfx.fillRect(px + 6, py + 2, 1, 3);
    this.tileDetailGfx.fillRect(px + 20, py + 4, 1, 3);
    this.tileDetailGfx.fillRect(px + 14, py + 1, 1, 3);
  }

  private drawWaterTile(px: number, py: number, x: number, y: number): void {
    this.tileGfx.fillStyle(0x4890D8, 1);
    this.tileGfx.fillRect(px, py, TILE_SIZE, TILE_SIZE);
    this.tileDetailGfx.lineStyle(1, 0x68B0F0, 0.5);
    this.tileDetailGfx.lineBetween(px + 4, py + 10, px + 14, py + 8);
    this.tileDetailGfx.lineBetween(px + 18, py + 20, px + 28, py + 18);

    const mapW = this.currentMap.width;
    const mapH = this.currentMap.height;
    const isWater = (tx: number, ty: number) =>
      tx >= 0 && tx < mapW && ty >= 0 && ty < mapH && this.currentMap.tiles[ty][tx] === TileType.Water;

    if (!isWater(x, y - 1)) {
      this.tileDetailGfx.fillStyle(0x90C8A0, 0.5);
      this.tileDetailGfx.fillRect(px, py, TILE_SIZE, 3);
    }
    if (!isWater(x, y + 1)) {
      this.tileDetailGfx.fillStyle(0x90C8A0, 0.5);
      this.tileDetailGfx.fillRect(px, py + TILE_SIZE - 3, TILE_SIZE, 3);
    }
    if (!isWater(x - 1, y)) {
      this.tileDetailGfx.fillStyle(0x90C8A0, 0.5);
      this.tileDetailGfx.fillRect(px, py, 3, TILE_SIZE);
    }
    if (!isWater(x + 1, y)) {
      this.tileDetailGfx.fillStyle(0x90C8A0, 0.5);
      this.tileDetailGfx.fillRect(px + TILE_SIZE - 3, py, 3, TILE_SIZE);
    }
  }

  private drawTreeTile(px: number, py: number, x: number, y: number, seed: (x: number, y: number, s: number) => number): void {
    this.tileGfx.fillStyle(0x58A848, 1);
    this.tileGfx.fillRect(px, py, TILE_SIZE, TILE_SIZE);
    this.tileDetailGfx.fillStyle(0x685030, 1);
    this.tileDetailGfx.fillRect(px + 12, py + 18, 8, 14);
    this.tileDetailGfx.fillStyle(0x806840, 0.5);
    this.tileDetailGfx.fillRect(px + 14, py + 20, 3, 10);
    this.tileDetailGfx.fillStyle(0x286828, 1);
    this.tileDetailGfx.fillCircle(px + 16, py + 12, 14);
    this.tileDetailGfx.fillStyle(0x388838, 0.7);
    this.tileDetailGfx.fillCircle(px + 12, py + 8, 6);
    this.tileDetailGfx.fillStyle(0x388838, 0.5);
    this.tileDetailGfx.fillCircle(px + 20, py + 14, 5);
  }

  private drawWallTile(px: number, py: number, x: number, y: number): void {
    this.tileGfx.fillStyle(0xD8C0A0, 1);
    this.tileGfx.fillRect(px, py, TILE_SIZE, TILE_SIZE);
    this.tileDetailGfx.lineStyle(1, 0xC0A880, 0.6);
    for (let ly = 6; ly < TILE_SIZE; ly += 8) {
      this.tileDetailGfx.lineBetween(px, py + ly, px + TILE_SIZE, py + ly);
    }
    this.tileDetailGfx.fillStyle(0xB0A080, 0.4);
    this.tileDetailGfx.fillRect(px, py, 2, TILE_SIZE);
    this.tileDetailGfx.fillRect(px, py + TILE_SIZE - 2, TILE_SIZE, 2);
  }

  private drawRoofTile(px: number, py: number, x: number, y: number): void {
    // Determine roof color based on current map (gym towns get colored roofs)
    let roofColor = 0xB84030; // default red-brown
    // Check if this roof is part of a gym based on map context
    if (this.currentMapId === 'VILLE_FOUGERE' && px >= 14 * TILE_SIZE && px < 20 * TILE_SIZE && py >= 20 * TILE_SIZE) {
      roofColor = 0x388838; // green for plant gym
    } else if (this.currentMapId === 'PORT_COQUILLE' && px >= 25 * TILE_SIZE && px < 31 * TILE_SIZE && py >= 5 * TILE_SIZE && py < 9 * TILE_SIZE) {
      roofColor = 0x4080C0; // blue for water gym
    } else if (this.currentMapId === 'ROCHE_HAUTE' && px >= 22 * TILE_SIZE && px < 28 * TILE_SIZE && py >= 5 * TILE_SIZE && py < 9 * TILE_SIZE) {
      roofColor = 0x8B6914; // brown for rock gym
    }

    this.tileGfx.fillStyle(roofColor, 1);
    this.tileGfx.fillRect(px, py, TILE_SIZE, TILE_SIZE);
    this.tileDetailGfx.lineStyle(1, this.darkenColor(roofColor, 0.7), 0.7);
    this.tileDetailGfx.lineBetween(px, py + 8, px + TILE_SIZE, py + 8);
    this.tileDetailGfx.lineBetween(px, py + 16, px + TILE_SIZE, py + 16);
    this.tileDetailGfx.lineBetween(px, py + 24, px + TILE_SIZE, py + 24);
    this.tileDetailGfx.fillStyle(this.darkenColor(roofColor, 0.8), 0.5);
    this.tileDetailGfx.fillRect(px, py + TILE_SIZE - 4, TILE_SIZE, 4);
  }

  private darkenColor(color: number, factor: number): number {
    const r = Math.floor(((color >> 16) & 0xFF) * factor);
    const g = Math.floor(((color >> 8) & 0xFF) * factor);
    const b = Math.floor((color & 0xFF) * factor);
    return (r << 16) | (g << 8) | b;
  }

  private drawDoorTile(px: number, py: number): void {
    // Wall background
    this.tileGfx.fillStyle(0xD8C0A0, 1);
    this.tileGfx.fillRect(px, py, TILE_SIZE, TILE_SIZE);

    // Welcome mat (red carpet before door)
    this.tileDetailGfx.fillStyle(0xC04040, 0.8);
    this.tileDetailGfx.fillRect(px + 2, py + TILE_SIZE - 6, TILE_SIZE - 4, 6);
    this.tileDetailGfx.fillStyle(0xA03030, 0.6);
    this.tileDetailGfx.fillRect(px + 4, py + TILE_SIZE - 4, TILE_SIZE - 8, 3);

    // Door frame (darker border)
    this.tileDetailGfx.fillStyle(0x503018, 1);
    this.tileDetailGfx.fillRect(px + 2, py, 2, TILE_SIZE - 4);
    this.tileDetailGfx.fillRect(px + TILE_SIZE - 4, py, 2, TILE_SIZE - 4);
    this.tileDetailGfx.fillRect(px + 2, py, TILE_SIZE - 4, 2);

    // Door panel (wood)
    this.tileDetailGfx.fillStyle(0x704828, 1);
    this.tileDetailGfx.fillRoundedRect(px + 4, py + 2, TILE_SIZE - 8, TILE_SIZE - 6, 2);

    // Door detail — two panels
    this.tileDetailGfx.fillStyle(0x604020, 1);
    this.tileDetailGfx.fillRect(px + 6, py + 4, 8, 10);
    this.tileDetailGfx.fillRect(px + 18, py + 4, 8, 10);
    this.tileDetailGfx.fillRect(px + 6, py + 16, 8, 8);
    this.tileDetailGfx.fillRect(px + 18, py + 16, 8, 8);

    // Door handle (golden knob)
    this.tileDetailGfx.fillStyle(0xE8C050, 1);
    this.tileDetailGfx.fillCircle(px + 22, py + 16, 2.5);
    this.tileDetailGfx.lineStyle(1, 0xC89840, 1);
    this.tileDetailGfx.strokeCircle(px + 22, py + 16, 2.5);

    // Center line
    this.tileDetailGfx.lineStyle(1, 0x503018, 0.7);
    this.tileDetailGfx.lineBetween(px + 16, py + 2, px + 16, py + TILE_SIZE - 6);
  }

  private drawFenceTile(px: number, py: number): void {
    this.tileGfx.fillStyle(0x58A848, 1);
    this.tileGfx.fillRect(px, py, TILE_SIZE, TILE_SIZE);
    this.tileDetailGfx.fillStyle(0xC8A868, 1);
    this.tileDetailGfx.fillRect(px + 4, py + 6, 4, 22);
    this.tileDetailGfx.fillRect(px + 24, py + 6, 4, 22);
    this.tileDetailGfx.fillRect(px, py + 10, TILE_SIZE, 3);
    this.tileDetailGfx.fillRect(px, py + 20, TILE_SIZE, 3);
    this.tileDetailGfx.fillStyle(0xD8B878, 1);
    this.tileDetailGfx.fillRect(px + 3, py + 4, 6, 3);
    this.tileDetailGfx.fillRect(px + 23, py + 4, 6, 3);
  }

  private drawFlowerTile(px: number, py: number, x: number, y: number, seed: (x: number, y: number, s: number) => number): void {
    this.drawGrassTile(px, py, x, y, seed);
    const flowerColors = [0xFF5050, 0xF8D830, 0x5080FF, 0xFF80C0];
    for (let i = 0; i < 4; i++) {
      const fx = px + 4 + Math.floor(seed(x, y, 30 + i) * 22);
      const fy = py + 4 + Math.floor(seed(x, y, 40 + i) * 22);
      const fc = flowerColors[(x + y + i) % flowerColors.length];
      this.tileDetailGfx.fillStyle(fc, 0.9);
      this.tileDetailGfx.fillCircle(fx, fy, 2.5);
      this.tileDetailGfx.lineStyle(1, 0x48A038, 0.6);
      this.tileDetailGfx.lineBetween(fx, fy + 2, fx, fy + 6);
    }
  }

  private drawSignTile(px: number, py: number): void {
    this.tileGfx.fillStyle(0x58A848, 1);
    this.tileGfx.fillRect(px, py, TILE_SIZE, TILE_SIZE);
    this.tileDetailGfx.fillStyle(0x806040, 1);
    this.tileDetailGfx.fillRect(px + 13, py + 14, 6, 18);
    this.tileDetailGfx.fillStyle(0xD8C090, 1);
    this.tileDetailGfx.fillRoundedRect(px + 4, py + 4, 24, 14, 2);
    this.tileDetailGfx.lineStyle(1, 0xA09060, 1);
    this.tileDetailGfx.strokeRoundedRect(px + 4, py + 4, 24, 14, 2);
    this.tileDetailGfx.fillStyle(0x605040, 0.5);
    this.tileDetailGfx.fillRect(px + 8, py + 8, 16, 1);
    this.tileDetailGfx.fillRect(px + 8, py + 12, 12, 1);
  }

  private drawLedgeTile(px: number, py: number): void {
    this.tileGfx.fillStyle(0x58A848, 1);
    this.tileGfx.fillRect(px, py, TILE_SIZE, TILE_SIZE);
    this.tileDetailGfx.fillStyle(0x306020, 0.7);
    this.tileDetailGfx.fillRect(px, py + TILE_SIZE - 6, TILE_SIZE, 6);
    this.tileDetailGfx.fillStyle(0x205010, 0.8);
    this.tileDetailGfx.fillRect(px, py + TILE_SIZE - 2, TILE_SIZE, 2);
  }

  private drawSandTile(px: number, py: number, x: number, y: number, seed: (x: number, y: number, s: number) => number): void {
    this.tileGfx.fillStyle(0xE8D8A0, 1);
    this.tileGfx.fillRect(px, py, TILE_SIZE, TILE_SIZE);
    // Sand texture dots
    this.tileDetailGfx.fillStyle(0xD8C890, 0.5);
    for (let s = 0; s < 5; s++) {
      const sx = px + Math.floor(seed(x, y, 50 + s) * 28) + 2;
      const sy = py + Math.floor(seed(x, y, 60 + s) * 28) + 2;
      this.tileDetailGfx.fillRect(sx, sy, 2, 1);
    }
    // Subtle wave lines
    this.tileDetailGfx.lineStyle(1, 0xD0C080, 0.3);
    this.tileDetailGfx.lineBetween(px + 2, py + 10, px + 20, py + 12);
    this.tileDetailGfx.lineBetween(px + 10, py + 22, px + 28, py + 20);
  }

  private drawIceTile(px: number, py: number, x: number, y: number, seed: (x: number, y: number, s: number) => number): void {
    this.tileGfx.fillStyle(0xC0E8F8, 1);
    this.tileGfx.fillRect(px, py, TILE_SIZE, TILE_SIZE);
    // Shine highlights
    this.tileDetailGfx.fillStyle(0xE0F0FF, 0.6);
    this.tileDetailGfx.fillRect(px + 4, py + 4, 6, 2);
    this.tileDetailGfx.fillRect(px + 18, py + 16, 4, 2);
    // Cracks
    this.tileDetailGfx.lineStyle(1, 0xA0D0E8, 0.4);
    this.tileDetailGfx.lineBetween(px + 8, py + 12, px + 20, py + 18);
  }

  private drawLavaTile(px: number, py: number, x: number, y: number): void {
    this.tileGfx.fillStyle(0xD04020, 1);
    this.tileGfx.fillRect(px, py, TILE_SIZE, TILE_SIZE);
    // Bright spots
    this.tileDetailGfx.fillStyle(0xF08030, 0.7);
    this.tileDetailGfx.fillCircle(px + 10, py + 10, 4);
    this.tileDetailGfx.fillCircle(px + 22, py + 20, 3);
    // Dark crust
    this.tileDetailGfx.fillStyle(0x802010, 0.5);
    this.tileDetailGfx.fillRect(px + 14, py + 6, 8, 3);
    this.tileDetailGfx.fillRect(px + 4, py + 22, 6, 3);
  }

  private drawBridgeTile(px: number, py: number, x: number, y: number): void {
    // Water or sand underneath
    this.tileGfx.fillStyle(0x4890D8, 1);
    this.tileGfx.fillRect(px, py, TILE_SIZE, TILE_SIZE);
    // Wooden planks
    this.tileDetailGfx.fillStyle(0xA08050, 1);
    this.tileDetailGfx.fillRect(px + 2, py, TILE_SIZE - 4, TILE_SIZE);
    // Plank lines
    this.tileDetailGfx.lineStyle(1, 0x806030, 0.6);
    for (let ly = 4; ly < TILE_SIZE; ly += 8) {
      this.tileDetailGfx.lineBetween(px + 2, py + ly, px + TILE_SIZE - 2, py + ly);
    }
    // Rails
    this.tileDetailGfx.fillStyle(0x685030, 1);
    this.tileDetailGfx.fillRect(px, py, 3, TILE_SIZE);
    this.tileDetailGfx.fillRect(px + TILE_SIZE - 3, py, 3, TILE_SIZE);
  }

  // ============================================================
  // Player Drawing (32x32, detailed)
  // ============================================================
  private drawPlayer(): void {
    this.playerGfx.clear();
    const px = 0;
    const py = 0;
    const TS = TILE_SIZE;

    // Shadow underneath
    this.playerGfx.fillStyle(0x000000, 0.2);
    this.playerGfx.fillEllipse(px + TS / 2, py + TS - 2, 20, 6);

    const skinColor = 0xF0C890;
    const hairColor = 0x604020;
    const hatColor = 0xD03030;
    const shirtColor = 0x3060C0;
    const pantsColor = 0x303050;
    const shoeColor = 0x402020;

    if (this.playerDirection === Direction.Down) {
      this.playerGfx.fillStyle(pantsColor, 1);
      this.playerGfx.fillRect(px + 9, py + 22, 6, 7);
      this.playerGfx.fillRect(px + 17, py + 22, 6, 7);
      this.playerGfx.fillStyle(shoeColor, 1);
      this.playerGfx.fillRect(px + 8, py + 28, 7, 3);
      this.playerGfx.fillRect(px + 17, py + 28, 7, 3);
      this.playerGfx.fillStyle(shirtColor, 1);
      this.playerGfx.fillRoundedRect(px + 7, py + 12, 18, 12, 3);
      this.playerGfx.fillStyle(skinColor, 1);
      this.playerGfx.fillCircle(px + TS / 2, py + 8, 7);
      this.playerGfx.fillStyle(hairColor, 1);
      this.playerGfx.fillRect(px + 8, py + 1, 16, 5);
      this.playerGfx.fillStyle(hatColor, 1);
      this.playerGfx.fillRoundedRect(px + 6, py, 20, 5, 2);
      this.playerGfx.fillRect(px + 10, py - 2, 12, 4);
      this.playerGfx.fillStyle(0x202020, 1);
      this.playerGfx.fillRect(px + 12, py + 7, 2, 2);
      this.playerGfx.fillRect(px + 18, py + 7, 2, 2);
    } else if (this.playerDirection === Direction.Up) {
      this.playerGfx.fillStyle(pantsColor, 1);
      this.playerGfx.fillRect(px + 9, py + 22, 6, 7);
      this.playerGfx.fillRect(px + 17, py + 22, 6, 7);
      this.playerGfx.fillStyle(shoeColor, 1);
      this.playerGfx.fillRect(px + 8, py + 28, 7, 3);
      this.playerGfx.fillRect(px + 17, py + 28, 7, 3);
      this.playerGfx.fillStyle(shirtColor, 1);
      this.playerGfx.fillRoundedRect(px + 7, py + 12, 18, 12, 3);
      this.playerGfx.fillStyle(hairColor, 1);
      this.playerGfx.fillCircle(px + TS / 2, py + 8, 7);
      this.playerGfx.fillRect(px + 8, py + 1, 16, 8);
      this.playerGfx.fillStyle(hatColor, 1);
      this.playerGfx.fillRoundedRect(px + 6, py, 20, 5, 2);
      this.playerGfx.fillRect(px + 10, py - 2, 12, 4);
    } else if (this.playerDirection === Direction.Left) {
      this.playerGfx.fillStyle(pantsColor, 1);
      this.playerGfx.fillRect(px + 10, py + 22, 6, 7);
      this.playerGfx.fillRect(px + 16, py + 22, 6, 7);
      this.playerGfx.fillStyle(shoeColor, 1);
      this.playerGfx.fillRect(px + 8, py + 28, 7, 3);
      this.playerGfx.fillRect(px + 15, py + 28, 7, 3);
      this.playerGfx.fillStyle(shirtColor, 1);
      this.playerGfx.fillRoundedRect(px + 8, py + 12, 16, 12, 3);
      this.playerGfx.fillStyle(skinColor, 1);
      this.playerGfx.fillCircle(px + 13, py + 8, 7);
      this.playerGfx.fillStyle(hairColor, 1);
      this.playerGfx.fillRect(px + 10, py + 1, 12, 6);
      this.playerGfx.fillStyle(hatColor, 1);
      this.playerGfx.fillRoundedRect(px + 4, py, 18, 5, 2);
      this.playerGfx.fillRect(px + 8, py - 2, 10, 4);
      this.playerGfx.fillStyle(0x202020, 1);
      this.playerGfx.fillRect(px + 9, py + 7, 2, 2);
    } else {
      this.playerGfx.fillStyle(pantsColor, 1);
      this.playerGfx.fillRect(px + 10, py + 22, 6, 7);
      this.playerGfx.fillRect(px + 16, py + 22, 6, 7);
      this.playerGfx.fillStyle(shoeColor, 1);
      this.playerGfx.fillRect(px + 10, py + 28, 7, 3);
      this.playerGfx.fillRect(px + 17, py + 28, 7, 3);
      this.playerGfx.fillStyle(shirtColor, 1);
      this.playerGfx.fillRoundedRect(px + 8, py + 12, 16, 12, 3);
      this.playerGfx.fillStyle(skinColor, 1);
      this.playerGfx.fillCircle(px + 19, py + 8, 7);
      this.playerGfx.fillStyle(hairColor, 1);
      this.playerGfx.fillRect(px + 10, py + 1, 12, 6);
      this.playerGfx.fillStyle(hatColor, 1);
      this.playerGfx.fillRoundedRect(px + 10, py, 18, 5, 2);
      this.playerGfx.fillRect(px + 14, py - 2, 10, 4);
      this.playerGfx.fillStyle(0x202020, 1);
      this.playerGfx.fillRect(px + 21, py + 7, 2, 2);
    }

    this.playerContainer.setPosition(this.playerPixelX, this.playerPixelY);
  }

  // ============================================================
  // NPC Drawing
  // ============================================================
  private drawNPCs(): void {
    this.npcContainer.removeAll(true);

    for (const npc of this.npcs) {
      const px = npc.data.x * TILE_SIZE;
      const py = npc.data.y * TILE_SIZE;

      const gfx = this.add.graphics();

      // Shadow
      gfx.fillStyle(0x000000, 0.2);
      gfx.fillEllipse(px + TILE_SIZE / 2, py + TILE_SIZE - 2, 18, 5);

      const skinColor = 0xF0C890;
      const role = npc.data.spriteType;

      if (role === 'mom') {
        gfx.fillStyle(skinColor, 1);
        gfx.fillRect(px + 11, py + 24, 4, 5);
        gfx.fillRect(px + 17, py + 24, 4, 5);
        gfx.fillStyle(npc.bodyColor, 1);
        gfx.fillRoundedRect(px + 6, py + 10, 20, 16, 4);
        gfx.fillStyle(skinColor, 1);
        gfx.fillCircle(px + 16, py + 8, 7);
        gfx.fillStyle(npc.hairColor, 1);
        gfx.fillRect(px + 8, py + 0, 16, 6);
        gfx.fillRect(px + 8, py + 4, 4, 10);
        gfx.fillRect(px + 20, py + 4, 4, 10);
        gfx.fillStyle(0x202020, 1);
        gfx.fillRect(px + 13, py + 7, 2, 2);
        gfx.fillRect(px + 18, py + 7, 2, 2);
      } else if (role === 'assistant' || role === 'professor') {
        gfx.fillStyle(0x303050, 1);
        gfx.fillRect(px + 10, py + 24, 5, 5);
        gfx.fillRect(px + 17, py + 24, 5, 5);
        gfx.fillStyle(npc.bodyColor, 1);
        gfx.fillRoundedRect(px + 7, py + 10, 18, 14, 3);
        gfx.fillStyle(skinColor, 1);
        gfx.fillCircle(px + 16, py + 8, 6);
        gfx.fillStyle(npc.hairColor, 1);
        gfx.fillRect(px + 9, py + 1, 14, 5);
        gfx.fillStyle(0x202020, 1);
        gfx.fillRect(px + 13, py + 7, 2, 2);
        gfx.fillRect(px + 18, py + 7, 2, 2);
      } else if (role === 'nurse') {
        gfx.fillStyle(0x303050, 1);
        gfx.fillRect(px + 10, py + 24, 5, 5);
        gfx.fillRect(px + 17, py + 24, 5, 5);
        gfx.fillStyle(0xF0F0F0, 1); // white uniform
        gfx.fillRoundedRect(px + 7, py + 10, 18, 14, 3);
        // Red cross
        gfx.fillStyle(0xD03030, 1);
        gfx.fillRect(px + 14, py + 13, 4, 8);
        gfx.fillRect(px + 12, py + 15, 8, 4);
        gfx.fillStyle(skinColor, 1);
        gfx.fillCircle(px + 16, py + 8, 6);
        gfx.fillStyle(npc.hairColor, 1);
        gfx.fillRect(px + 9, py + 1, 14, 5);
        gfx.fillRect(px + 8, py + 4, 4, 8);
        gfx.fillRect(px + 20, py + 4, 4, 8);
        gfx.fillStyle(0x202020, 1);
        gfx.fillRect(px + 13, py + 7, 2, 2);
        gfx.fillRect(px + 18, py + 7, 2, 2);
      } else {
        // Generic: trainer, villager, shopkeeper, rival, grunt
        gfx.fillStyle(0x303050, 1);
        gfx.fillRect(px + 10, py + 23, 5, 6);
        gfx.fillRect(px + 17, py + 23, 5, 6);
        gfx.fillStyle(0x402020, 1);
        gfx.fillRect(px + 9, py + 28, 6, 3);
        gfx.fillRect(px + 17, py + 28, 6, 3);
        gfx.fillStyle(npc.bodyColor, 1);
        gfx.fillRoundedRect(px + 8, py + 12, 16, 12, 3);
        gfx.fillStyle(skinColor, 1);
        gfx.fillCircle(px + 16, py + 8, 6);
        gfx.fillStyle(npc.hairColor, 1);
        gfx.fillRect(px + 9, py + 1, 14, 5);
        gfx.fillStyle(0x202020, 1);
        gfx.fillRect(px + 13, py + 7, 2, 2);
        gfx.fillRect(px + 18, py + 7, 2, 2);
      }

      this.npcContainer.add(gfx);
    }
  }

  // ============================================================
  // NPCs — spawn from map data
  // ============================================================
  private spawnNPCs(): void {
    const gs = GameState.getInstance();
    this.npcs = this.currentMap.npcs
      .filter(npcData => {
        // Hide NPC if its hideIfFlag is already set
        if (npcData.hideIfFlag && gs.hasFlag(npcData.hideIfFlag)) return false;
        return true;
      })
      .map(npcData => {
        const colors = SPRITE_COLORS[npcData.spriteType] || SPRITE_COLORS.villager;
        return {
          data: npcData,
          bodyColor: colors.body,
          hairColor: colors.hair,
          dialogueIndex: 0,
        };
      });

    this.drawNPCs();
  }

  // ============================================================
  // Collision — uses map data
  // ============================================================
  private isCollision(x: number, y: number): boolean {
    const mapW = this.currentMap.width;
    const mapH = this.currentMap.height;
    if (x < 0 || x >= mapW || y < 0 || y >= mapH) return true; // out of bounds = collision unless warp

    const tileType = this.currentMap.tiles[y][x];
    if (this.collisionSet.has(tileType)) return true;

    // NPC collision
    if (this.npcs.some(n => n.data.x === x && n.data.y === y)) return true;

    return false;
  }

  private canMove(x: number, y: number): boolean {
    // Allow moving into warp tiles even if out of bounds
    const warp = this.checkWarp(x, y);
    if (warp) return true;

    return !this.isCollision(x, y);
  }

  // ============================================================
  // Movement
  // ============================================================
  private movePlayer(dir: Direction): void {
    if (this.isMoving || this.isWarping || this.dialogue.isActive() || this.menu.isOpen() || this.eventSystem.isExecuting()) return;

    this.playerDirection = dir;

    let dx = 0;
    let dy = 0;
    switch (dir) {
      case Direction.Up: dy = -1; break;
      case Direction.Down: dy = 1; break;
      case Direction.Left: dx = -1; break;
      case Direction.Right: dx = 1; break;
    }

    const newX = this.playerTileX + dx;
    const newY = this.playerTileY + dy;

    if (!this.canMove(newX, newY)) {
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
        this.onStepComplete(newX, newY);
      },
    });
  }

  private onStepComplete(x: number, y: number): void {
    // Check for story tile events
    if (!this.eventSystem.isExecuting()) {
      const tileEvent = this.eventSystem.checkTileEvents(this.currentMapId, x, y);
      if (tileEvent) {
        this.eventSystem.executeEvent(tileEvent);
        return; // Don't process warp or encounter while event runs
      }
    }

    // Walking onto a Door tile = enter building automatically
    const tile = this.currentMap.tiles[y]?.[x];
    if (tile === TileType.Door) {
      this.handleDoorInteraction(x, y);
      return;
    }

    // Check for warp
    const warp = this.checkWarp(x, y);
    if (warp) {
      this.executeWarp(warp);
      return;
    }

    // Check for wild encounter on encounter tiles
    if (this.hasStarter && !this.encounterBlocked && this.currentMap.encounterRate > 0) {
      const mapW = this.currentMap.width;
      const mapH = this.currentMap.height;
      if (x >= 0 && x < mapW && y >= 0 && y < mapH) {
        const tileType = this.currentMap.tiles[y][x];
        if (this.encounterSet.has(tileType)) {
          const roll = Phaser.Math.Between(1, 100);
          if (roll <= this.currentMap.encounterRate) {
            this.triggerWildEncounter();
            return;
          }
        }
      }
    }

    // Check if any trainer spots the player
    if (this.hasStarter && !this.battlePending) {
      this.checkTrainerSightLines();
    }
  }

  // ============================================================
  // Trainer line-of-sight detection
  // ============================================================
  private checkTrainerSightLines(): void {
    const gs = GameState.getInstance();

    for (const npc of this.npcs) {
      // Only check trainer-type NPCs
      const trainerData = this.getTrainerDataForNPC(npc);
      if (!trainerData) continue;

      // Skip already defeated trainers
      if (gs.isTrainerDefeated(trainerData.id)) continue;

      // Check if player is in this trainer's line of sight
      const dx = this.playerTileX - npc.data.x;
      const dy = this.playerTileY - npc.data.y;

      let inSight = false;
      let distance = 0;

      switch (npc.data.facing) {
        case 'up':
          if (dx === 0 && dy < 0) { distance = -dy; inSight = true; }
          break;
        case 'down':
          if (dx === 0 && dy > 0) { distance = dy; inSight = true; }
          break;
        case 'left':
          if (dy === 0 && dx < 0) { distance = -dx; inSight = true; }
          break;
        case 'right':
          if (dy === 0 && dx > 0) { distance = dx; inSight = true; }
          break;
      }

      if (!inSight || distance > 4 || distance <= 0) continue;

      // Check for collision tiles blocking the line of sight
      let blocked = false;
      const stepX = dx === 0 ? 0 : (dx > 0 ? 1 : -1);
      const stepY = dy === 0 ? 0 : (dy > 0 ? 1 : -1);
      for (let i = 1; i < distance; i++) {
        const checkX = npc.data.x + stepX * i;
        const checkY = npc.data.y + stepY * i;
        if (checkX >= 0 && checkX < this.currentMap.width && checkY >= 0 && checkY < this.currentMap.height) {
          const tile = this.currentMap.tiles[checkY][checkX];
          if (this.collisionSet.has(tile)) {
            blocked = true;
            break;
          }
        }
      }

      if (blocked) continue;

      // Trainer spots the player!
      this.battlePending = true;

      // Show "!" exclamation above trainer
      this.showExclamation(npc.data.x, npc.data.y, () => {
        // Walk trainer toward the player (simple: teleport to adjacent tile)
        const targetX = this.playerTileX - (dx === 0 ? 0 : (dx > 0 ? 1 : -1));
        const targetY = this.playerTileY - (dy === 0 ? 0 : (dy > 0 ? 1 : -1));
        npc.data.x = targetX;
        npc.data.y = targetY;
        this.drawNPCs();

        // Show before dialogue then start battle
        this.dialogue.showText(trainerData.dialogue.before, () => {
          this.startTrainerBattle(trainerData);
        }, trainerData.name);
      });

      // Only trigger one trainer at a time
      return;
    }
  }

  // ============================================================
  // Wild Encounter — uses map encounter table
  // ============================================================
  private triggerWildEncounter(): void {
    if (this.currentMap.encounters.length === 0) return;

    this.encounterBlocked = true;

    // Weighted random selection
    const totalWeight = this.currentMap.encounters.reduce((sum, e) => sum + e.weight, 0);
    let roll = Phaser.Math.Between(1, totalWeight);
    let selected: WildEncounterTable = this.currentMap.encounters[0];
    for (const enc of this.currentMap.encounters) {
      roll -= enc.weight;
      if (roll <= 0) {
        selected = enc;
        break;
      }
    }

    const level = Phaser.Math.Between(selected.minLevel, selected.maxLevel);
    const hp = 12 + level * 3;

    // Register the wild dino as seen in dinodex
    GameState.getInstance().seeDino(selected.dinoId);

    // Create a proper wild Dino with real stats from species data
    const wildDino = Dino.createWild(selected.dinoId, level);
    const wildTypes: DinoType[] = [wildDino.type1];
    if (wildDino.type2 !== undefined) wildTypes.push(wildDino.type2);

    // Resolve moves from registry
    const wildMoves = wildDino.moves.slice(0, 4).map(slot => {
      try {
        if (hasMove(slot.moveId)) {
          const md = getMove(slot.moveId);
          return { name: md.name, type: md.type, category: md.category, power: md.power, pp: slot.currentPP, maxPp: slot.maxPP };
        }
      } catch (_e) { /* ignore */ }
      return { name: 'Charge', type: DinoType.Normal, category: 'physical' as const, power: 40, pp: 35, maxPp: 35 };
    });
    if (wildMoves.length === 0) {
      wildMoves.push({ name: 'Charge', type: DinoType.Normal, category: 'physical' as const, power: 40, pp: 35, maxPp: 35 });
    }

    AudioSystem.getInstance().stopMusic();
    TransitionFX.battleTransition(this, () => {
      this.scene.start(SCENE_KEYS.BATTLE, {
        isWild: true,
        wildDino: {
          name: wildDino.nickname,
          level: wildDino.level,
          types: wildTypes,
          currentHp: wildDino.currentHp,
          maxHp: wildDino.maxHp,
          attack: wildDino.stats.attack,
          defense: wildDino.stats.defense,
          spAttack: wildDino.stats.spAttack,
          spDefense: wildDino.stats.spDefense,
          speed: wildDino.stats.speed,
          moves: wildMoves,
        },
        returnScene: SCENE_KEYS.OVERWORLD,
        returnData: {
          mapId: this.currentMapId,
          playerX: this.playerTileX,
          playerY: this.playerTileY,
          hasStarter: true,
          wildSpeciesId: selected.dinoId,
        },
      });
    });
  }

  // ============================================================
  // Interaction
  // ============================================================
  private interact(): void {
    if (this.dialogue.isActive() || this.menu.isOpen() || this.battlePending) return;

    let facingX = this.playerTileX;
    let facingY = this.playerTileY;
    switch (this.playerDirection) {
      case Direction.Up: facingY--; break;
      case Direction.Down: facingY++; break;
      case Direction.Left: facingX--; break;
      case Direction.Right: facingX++; break;
    }

    // Check for NPC
    const npc = this.npcs.find(n => n.data.x === facingX && n.data.y === facingY);
    if (npc) {
      // Check for story events on this NPC first
      const npcEvent = this.eventSystem.checkNpcEvents(npc.data.id || npc.data.name);
      if (npcEvent) {
        this.eventSystem.executeEvent(npcEvent);
        return;
      }

      // Check if this NPC is a trainer that can be battled
      const trainerData = this.getTrainerDataForNPC(npc);
      const gs = GameState.getInstance();

      if (trainerData && !gs.isTrainerDefeated(trainerData.id)) {
        // Undefeated trainer — show "!" then battle dialogue
        this.showExclamation(npc.data.x, npc.data.y, () => {
          this.dialogue.showText(trainerData.dialogue.before, () => {
            this.startTrainerBattle(trainerData);
          }, trainerData.name);
        });
        return;
      } else if (trainerData && gs.isTrainerDefeated(trainerData.id)) {
        // Defeated trainer — show after dialogue
        this.dialogue.showText(trainerData.dialogue.after, undefined, trainerData.name);
        return;
      }

      // Nurse NPC — heal party + set respawn point
      if (npc.data.spriteType === 'nurse') {
        const gs = GameState.getInstance();
        const party = gs.getPartySystem().getParty();
        for (const d of party) { d.fullHeal(); }
        gs.setHealPoint(this.currentMapId, this.playerTileX, this.playerTileY);
        AudioSystem.getInstance().playMenuSelect();
        this.dialogue.showText('Vos dinos sont en pleine forme ! Bonne chance, dresseur !', undefined, npc.data.name);
        return;
      }

      // Regular NPC dialogue
      const text = npc.data.dialogue[npc.dialogueIndex % npc.data.dialogue.length];
      npc.dialogueIndex++;
      this.dialogue.showText(text, undefined, npc.data.name);
      return;
    }

    // Check tile interactions
    const mapW = this.currentMap.width;
    const mapH = this.currentMap.height;
    if (facingX >= 0 && facingX < mapW && facingY >= 0 && facingY < mapH) {
      const tile = this.currentMap.tiles[facingY][facingX];

      if (tile === TileType.Door) {
        this.handleDoorInteraction(facingX, facingY);
      }

      if (tile === TileType.Sign) {
        this.handleSignInteraction(facingX, facingY);
      }
    }
  }

  /** Look up trainer data for an NPC based on their id or spriteType */
  private getTrainerDataForNPC(npc: NPCRuntime): TrainerData | undefined {
    // Try direct lookup by NPC id
    const trainer = getTrainer(npc.data.id);
    if (trainer) return trainer;

    // Try matching by NPC spriteType being a trainer type
    if (npc.data.spriteType === 'trainer' || npc.data.spriteType === 'rival' || npc.data.spriteType === 'grunt') {
      // Check route trainers for current map
      const routeNum = this.getRouteNumberFromMapId(this.currentMapId);
      if (routeNum > 0) {
        const routeTrainers = getRouteTrainers(routeNum);
        // Match by trainer name or id prefix
        const match = routeTrainers.find(t => t.id === npc.data.id || t.name === npc.data.name);
        if (match) return match;
      }
    }

    return undefined;
  }

  /** Extract route number from map ID */
  private getRouteNumberFromMapId(mapId: string): number {
    const match = mapId.match(/ROUTE_(\d+)/);
    return match ? parseInt(match[1], 10) : 0;
  }

  /** Show exclamation mark above an NPC */
  private showExclamation(tileX: number, tileY: number, callback: () => void): void {
    this.exclamationGfx.clear();
    const px = tileX * TILE_SIZE + TILE_SIZE / 2;
    const py = tileY * TILE_SIZE - 8;

    // Exclamation bubble
    this.exclamationGfx.fillStyle(0xFFFFFF, 1);
    this.exclamationGfx.fillRoundedRect(px - 8, py - 16, 16, 20, 4);
    this.exclamationGfx.fillTriangle(px - 3, py + 4, px + 3, py + 4, px, py + 8);

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

  /** Start a trainer battle from the overworld */
  private startTrainerBattle(trainer: TrainerData): void {
    this.battlePending = true;

    AudioSystem.getInstance().stopMusic();
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
        returnScene: SCENE_KEYS.OVERWORLD,
        returnData: {
          mapId: this.currentMapId,
          playerX: this.playerTileX,
          playerY: this.playerTileY,
          hasStarter: true,
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
            category: (m.category as 'physical' | 'special' | 'status') ?? 'physical',
            power: m.power,
            pp: m.pp,
            maxPp: m.maxPp,
          })),
        },
      });
    });
  }

  private handleDoorInteraction(x: number, y: number): void {
    // --- Special case: Bourg-Nid lab door (starter select, not a building scene) ---
    if (this.currentMapId === 'BOURG_NID' && x === 19 && y === 8) {
      if (!this.hasStarter) {
        this.dialogue.showText('Le labo du Professeur Paleo ! Allons choisir notre premier dino !', () => {
          TransitionFX.fadeOut(this, 400, () => {
            this.scene.start(SCENE_KEYS.STARTER_SELECT);
          });
        });
      } else {
        this.dialogue.showText('Le labo du Professeur Paleo. Tes dinos ont l\'air en forme !');
      }
      return;
    }

    // --- Unified building lookup ---
    const building = this.currentMap.buildings?.find(b => b.doorX === x && b.doorY === y);
    if (building) {
      const returnMapId = this.currentMapId;
      const returnX = this.playerTileX;
      const returnY = this.playerTileY;

      switch (building.type) {
        case 'dino_center':
          TransitionFX.fadeOut(this, 400, () => {
            this.scene.start(SCENE_KEYS.DINO_CENTER, { returnMapId, returnX, returnY });
          });
          break;
        case 'shop':
          TransitionFX.fadeOut(this, 400, () => {
            this.scene.start(SCENE_KEYS.SHOP, { returnMapId, returnX, returnY, townId: building.id });
          });
          break;
        case 'gym':
          TransitionFX.fadeOut(this, 400, () => {
            this.scene.start(SCENE_KEYS.GYM, { returnMapId, returnX, returnY, ...building.sceneData });
          });
          break;
        case 'house':
          TransitionFX.fadeOut(this, 300, () => {
            this.scene.start(SCENE_KEYS.HOUSE, {
              returnMapId, returnX, returnY,
              houseId: building.id,
              houseName: building.sceneData?.houseName,
              npcs: building.sceneData?.npcs,
            });
          });
          break;
      }
      return;
    }

    // --- Legacy per-city handlers (cities without buildings[] data yet) ---
    if (this.currentMapId === 'PORT_COQUILLE') {
      if (x === 6 && y === 7) {
        this.enterDinoCenter(x, y);
      } else if (x === 14 && y === 7) {
        this.enterShop(x, y, 2);
      } else if (x === 28 && y === 8) {
        this.enterGym(1, x, y);
      } else {
        this.dialogue.showText('La porte est fermee.');
      }
    } else if (this.currentMapId === 'ROCHE_HAUTE') {
      if (x === 6 && y === 7) {
        this.enterDinoCenter(x, y);
      } else if (x === 14 && y === 7) {
        this.enterShop(x, y, 3);
      } else if (x === 25 && y === 8) {
        this.enterGym(2, x, y);
      } else if (x === 17 && y === 4) {
        this.dialogue.showText('Entree de la Mine Abandonnee. L\'acces est interdit pour le moment...');
      } else {
        this.dialogue.showText('La porte est fermee.');
      }
    } else if (this.currentMapId === 'VOLCANVILLE') {
      this.handleVolcanvilleDoor(x, y);
    } else if (this.currentMapId === 'CRYO_CITE') {
      this.handleCryoCiteDoor(x, y);
    } else if (this.currentMapId === 'ELECTROPOLIS') {
      this.handleElectropolitDoor(x, y);
    } else if (this.currentMapId === 'MARAIS_NOIR') {
      this.handleMaraisNoirDoor(x, y);
    } else if (this.currentMapId === 'CIEL_HAUT') {
      this.handleCielHautDoor(x, y);
    } else if (this.currentMapId === 'PALEO_CAPITAL') {
      this.handlePaleoCapitalDoor(x, y);
    } else {
      this.dialogue.showText('La porte est fermee.');
    }
  }

  private handleSignInteraction(x: number, y: number): void {
    // --- Unified building sign lookup ---
    const building = this.currentMap.buildings?.find(b => b.signX === x && b.signY === y);
    if (building && building.signText) {
      this.dialogue.showText(building.signText);
      return;
    }

    // --- Legacy per-map signs (routes and cities without buildings[] data yet) ---
    if (this.currentMapId === 'ROUTE_1') {
      if (x === 4 && y === 3) {
        this.dialogue.showText('ROUTE 1 — Bourg-Nid <-> Ville-Fougere');
      } else if (x === 45 && y === 8) {
        this.dialogue.showText('Ville-Fougere — Tout droit !');
      } else {
        this.dialogue.showText('Un panneau, mais le texte est illisible.');
      }
    } else if (this.currentMapId === 'ROUTE_2') {
      if (x === 19 && y === 3) {
        this.dialogue.showText('Grotte du Passage — Attention, dinos sauvages a l\'interieur !');
      } else {
        this.dialogue.showText('Un panneau, mais le texte est illisible.');
      }
    } else if (this.currentMapId === 'PORT_COQUILLE') {
      if (x === 3 && y === 7) {
        this.dialogue.showText('Centre Dino — Soins gratuits.');
      } else if (x === 11 && y === 7) {
        this.dialogue.showText('Boutique de Port-Coquille.');
      } else if (x === 24 && y === 8) {
        this.dialogue.showText('Arene de Port-Coquille — Champion : MARIN');
      } else {
        this.dialogue.showText('Un panneau, mais le texte est illisible.');
      }
    } else if (this.currentMapId === 'ROUTE_3') {
      if (x === 13 && y === 2) {
        this.dialogue.showText('Roche-Haute — Vers le nord.');
      } else if (x === 13 && y === 37) {
        this.dialogue.showText('Port-Coquille — Vers le sud.');
      } else {
        this.dialogue.showText('Un panneau, mais le texte est illisible.');
      }
    } else if (this.currentMapId === 'ROCHE_HAUTE') {
      if (x === 3 && y === 7) {
        this.dialogue.showText('Centre Dino — Soins gratuits.');
      } else if (x === 11 && y === 7) {
        this.dialogue.showText('Boutique de Roche-Haute.');
      } else if (x === 21 && y === 8) {
        this.dialogue.showText('Arene de Roche-Haute — Championne : PETRA');
      } else if (x === 14 && y === 4) {
        this.dialogue.showText('Mine Abandonnee — DANGER : Acces interdit.');
      } else {
        this.dialogue.showText('Un panneau, mais le texte est illisible.');
      }
    } else {
      this.dialogue.showText('Un panneau, mais le texte est illisible.');
    }
  }

  // ============================================================
  // Gym & League Entry
  // ============================================================
  private enterGym(gymId: number, doorX: number, doorY: number): void {
    TransitionFX.fadeOut(this, 400, () => {
      this.scene.start(SCENE_KEYS.GYM, {
        gymId,
        returnMapId: this.currentMapId,
        returnX: doorX,
        returnY: doorY + 1,
      });
    });
  }

  private enterLeague(): void {
    const gs = GameState.getInstance();
    if (gs.getBadgeCount() < 8) {
      this.dialogue.showText('Il vous faut 8 Badges pour entrer dans la Ligue ! Vous en avez ' + gs.getBadgeCount() + '.');
      return;
    }
    TransitionFX.fadeOut(this, 400, () => {
      this.scene.start(SCENE_KEYS.ELITE_FOUR, { room: 0 });
    });
  }

  private enterDinoCenter(doorX: number, doorY: number): void {
    TransitionFX.fadeOut(this, 300, () => {
      this.scene.start(SCENE_KEYS.DINO_CENTER, {
        returnMapId: this.currentMapId,
        returnX: doorX,
        returnY: doorY + 1,
      });
    });
  }

  private enterShop(doorX: number, doorY: number, townId: number): void {
    TransitionFX.fadeOut(this, 300, () => {
      this.scene.start(SCENE_KEYS.SHOP, {
        returnMapId: this.currentMapId,
        returnX: doorX,
        returnY: doorY + 1,
        townId,
      });
    });
  }

  private enterHouse(doorX: number, doorY: number, houseId: string, houseName?: string, npcs?: Array<{ name: string; dialogue: string[] }>): void {
    TransitionFX.fadeOut(this, 300, () => {
      this.scene.start(SCENE_KEYS.HOUSE, {
        returnMapId: this.currentMapId,
        returnX: doorX,
        returnY: doorY + 1,
        houseId,
        houseName,
        npcs,
      });
    });
  }

  // --- City door handlers for gyms 4-8 ---
  private handleVolcanvilleDoor(x: number, y: number): void {
    // Assume gym door position; adjust as maps are created
    if (this.isGymDoor(x, y)) {
      this.enterGym(3, x, y);
    } else if (this.isDinoCenterDoor(x, y)) {
      this.enterDinoCenter(x, y);
    } else if (this.isShopDoor(x, y)) {
      this.enterShop(x, y, 3);
    } else {
      this.dialogue.showText('La porte est fermee.');
    }
  }

  private handleCryoCiteDoor(x: number, y: number): void {
    if (this.isGymDoor(x, y)) {
      this.enterGym(4, x, y);
    } else if (this.isDinoCenterDoor(x, y)) {
      this.enterDinoCenter(x, y);
    } else {
      this.dialogue.showText('La porte est fermee.');
    }
  }

  private handleElectropolitDoor(x: number, y: number): void {
    if (this.isGymDoor(x, y)) {
      this.enterGym(5, x, y);
    } else if (this.isDinoCenterDoor(x, y)) {
      this.enterDinoCenter(x, y);
    } else {
      this.dialogue.showText('La porte est fermee.');
    }
  }

  private handleMaraisNoirDoor(x: number, y: number): void {
    if (this.isGymDoor(x, y)) {
      this.enterGym(6, x, y);
    } else if (this.isDinoCenterDoor(x, y)) {
      this.enterDinoCenter(x, y);
    } else {
      this.dialogue.showText('La porte est fermee.');
    }
  }

  private handleCielHautDoor(x: number, y: number): void {
    if (this.isGymDoor(x, y)) {
      this.enterGym(7, x, y);
    } else if (this.isDinoCenterDoor(x, y)) {
      this.enterDinoCenter(x, y);
    } else {
      this.dialogue.showText('La porte est fermee.');
    }
  }

  private handlePaleoCapitalDoor(x: number, y: number): void {
    if (this.isLeagueDoor(x, y)) {
      this.enterLeague();
    } else if (this.isDinoCenterDoor(x, y)) {
      this.enterDinoCenter(x, y);
    } else {
      this.dialogue.showText('La porte est fermee.');
    }
  }

  // Generic door type checks (positions will vary per map, using common patterns)
  private isGymDoor(x: number, _y: number): boolean {
    // Each map should define its gym door position; for now use a heuristic
    // Gym doors are typically on the right side of the map
    const npcGym = this.npcs.find(n => n.data.name.includes('Arene') || n.data.name.includes('arene'));
    // Check warps for gym-type warps
    return this.currentMap.warps.some(w => w.x === x && w.targetMap.includes('GYM'));
  }

  private isDinoCenterDoor(x: number, y: number): boolean {
    return x === 6 && y === 7; // Common Dino Center position
  }

  private isShopDoor(x: number, y: number): boolean {
    return (x === 14 || x === 16) && y === 7;
  }

  private isLeagueDoor(x: number, y: number): boolean {
    // League entrance - large building at center of Paleo Capital
    return x >= 12 && x <= 16 && y <= 5;
  }

  // ============================================================
  // Update
  // ============================================================
  update(_time: number, delta: number): void {
    this.animTimer += delta;

    // UI updates
    this.dialogue.update();
    this.menu.update();

    // Menu toggle
    if (this.escKey && Phaser.Input.Keyboard.JustDown(this.escKey)) {
      if (this.dialogue.isActive()) return;
      if (this.menu.isOpen()) {
        this.menu.close();
      } else {
        this.menu.open(this, (option: string) => {
          this.handleMenuOption(option);
        });
      }
    }

    // Interaction
    if (this.enterKey && Phaser.Input.Keyboard.JustDown(this.enterKey)) {
      if (!this.dialogue.isActive() && !this.menu.isOpen()) {
        this.interact();
      }
    }

    // Movement
    if (!this.isMoving && !this.isWarping && !this.dialogue.isActive() && !this.menu.isOpen() && !this.battlePending && !this.eventSystem.isExecuting()) {
      if (this.cursors.up.isDown) this.movePlayer(Direction.Up);
      else if (this.cursors.down.isDown) this.movePlayer(Direction.Down);
      else if (this.cursors.left.isDown) this.movePlayer(Direction.Left);
      else if (this.cursors.right.isDown) this.movePlayer(Direction.Right);
    }

    // Draw player
    this.drawPlayer();

    // Camera follow (smooth)
    this.cameras.main.centerOn(
      this.playerPixelX + TILE_SIZE / 2,
      this.playerPixelY + TILE_SIZE / 2
    );
  }

  // ============================================================
  // Menu Option Handler
  // ============================================================
  private handleMenuOption(option: string): void {
    switch (option) {
      case 'SAUVEGARDER': {
        const gs = GameState.getInstance();
        gs.setPlayerPosition(this.playerTileX, this.playerTileY, this.currentMapId);
        const success = gs.saveGame(0);
        this.menu.close();
        if (success) {
          this.dialogue.showText('Partie sauvegardee !');
        } else {
          this.dialogue.showText('Erreur lors de la sauvegarde...');
        }
        break;
      }
      case 'EQUIPE': {
        this.menu.close();
        // Save position in GameState before leaving
        GameState.getInstance().setPlayerPosition(this.playerTileX, this.playerTileY, this.currentMapId);
        this.scene.start(SCENE_KEYS.PARTY, { returnScene: SCENE_KEYS.OVERWORLD });
        break;
      }
      case 'SAC': {
        this.menu.close();
        GameState.getInstance().setPlayerPosition(this.playerTileX, this.playerTileY, this.currentMapId);
        this.scene.start(SCENE_KEYS.BAG, { returnScene: SCENE_KEYS.OVERWORLD });
        break;
      }
      case 'DINODEX': {
        this.menu.close();
        GameState.getInstance().setPlayerPosition(this.playerTileX, this.playerTileY, this.currentMapId);
        this.scene.start(SCENE_KEYS.DINODEX, { returnScene: SCENE_KEYS.OVERWORLD });
        break;
      }
      case 'CARTE':
        this.menu.close();
        this.dialogue.showText('La carte n\'est pas encore disponible.');
        break;
      case 'OPTIONS':
        this.menu.close();
        this.dialogue.showText('Les options ne sont pas encore disponibles.');
        break;
      default:
        this.menu.close();
        break;
    }
  }

  // ============================================================
  // NPC Visual Movement (triggered by EventSystem)
  // ============================================================
  private moveNpcVisual(npcId: string, direction: string, tiles: number): void {
    const npc = this.npcs.find(n => n.data.id === npcId || n.data.name === npcId);
    if (!npc) return;

    let dx = 0, dy = 0;
    switch (direction) {
      case 'up': dy = -1; break;
      case 'down': dy = 1; break;
      case 'left': dx = -1; break;
      case 'right': dx = 1; break;
    }

    // Animate NPC movement tile by tile
    let moved = 0;
    const moveNext = () => {
      if (moved >= tiles) return;
      npc.data.x += dx;
      npc.data.y += dy;
      moved++;
      this.drawNPCs();
      if (moved < tiles) {
        this.time.delayedCall(150, moveNext);
      }
    };
    moveNext();
  }
}
