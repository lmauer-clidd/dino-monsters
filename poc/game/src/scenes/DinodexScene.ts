// ============================================================
// Jurassic Trainers -- Dinodex Scene (Pokedex equivalent)
// ============================================================

import Phaser from 'phaser';
import {
  SCENE_KEYS,
  GAME_WIDTH,
  GAME_HEIGHT,
  COLORS,
  FONT_FAMILY,
  FONT_SMALL,
  FONT_TINY,
  FONT_BODY,
  DinoType,
  DINO_TYPE_COLORS,
  DINO_TYPE_NAMES,
} from '../utils/constants';
import { GameState } from '../systems/GameState';
import { getSpecies, hasSpecies, ISpeciesData } from '../entities/Dino';

// ===================== Constants =====================

const TOTAL_DINOS = 150;
const LIST_VISIBLE = 14;
const LIST_X = 16;
const LIST_W = 220;
const DETAIL_X = LIST_X + LIST_W + 16;
const DETAIL_W = GAME_WIDTH - DETAIL_X - 16;

// ===================== Scene =====================

export class DinodexScene extends Phaser.Scene {
  private graphics!: Phaser.GameObjects.Graphics;
  private texts: Phaser.GameObjects.Text[] = [];

  private selectedIndex = 0;
  private scrollOffset = 0;
  private showDetails = false;

  // Return info
  private returnScene = '';

  // Keys
  private cursors!: Phaser.Types.Input.Keyboard.CursorKeys;
  private enterKey!: Phaser.Input.Keyboard.Key;
  private escKey!: Phaser.Input.Keyboard.Key;

  constructor() {
    super({ key: SCENE_KEYS.DINODEX });
  }

  init(data: { returnScene?: string }): void {
    this.returnScene = data.returnScene ?? SCENE_KEYS.OVERWORLD;
    this.selectedIndex = 0;
    this.scrollOffset = 0;
    this.showDetails = false;
  }

  create(): void {
    this.graphics = this.add.graphics();

    if (this.input.keyboard) {
      this.cursors = this.input.keyboard.createCursorKeys();
      this.enterKey = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.ENTER);
      this.escKey = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.ESC);
    }

    this.cameras.main.fadeIn(200, 0x18, 0x10, 0x18);
    this.render();
  }

  update(): void {
    if (this.showDetails) {
      this.handleDetailInput();
    } else {
      this.handleListInput();
    }
  }

  // ===================== Rendering =====================

  private render(): void {
    this.graphics.clear();
    this.clearTexts();

    // Full screen panel
    this.drawPanel(0, 0, GAME_WIDTH, GAME_HEIGHT);

    // Counts
    const gs = GameState.getInstance();
    const seenCount = gs.getDinodexSeenCount();
    const caughtCount = gs.getDinodexCaughtCount();
    this.addText(GAME_WIDTH / 2, 12, `VU: ${seenCount} / CAPTURE: ${caughtCount}`, FONT_TINY, '#E8C868')
      .setOrigin(0.5, 0);

    // Left panel: list
    this.renderList();

    // Right panel: details
    if (this.showDetails) {
      this.renderDetails();
    } else {
      this.renderPreview();
    }
  }

  private renderList(): void {
    this.drawPanel(LIST_X, 32, LIST_W, GAME_HEIGHT - 48);

    const gs = GameState.getInstance();
    const startY = 44;
    const itemH = 22;

    for (let i = 0; i < LIST_VISIBLE; i++) {
      const idx = i + this.scrollOffset;
      if (idx >= TOTAL_DINOS) break;

      const speciesId = idx + 1;
      const entry = gs.getDinodexEntry(speciesId);
      const y = startY + i * itemH;
      const isSelected = idx === this.selectedIndex;

      if (isSelected) {
        this.graphics.fillStyle(COLORS.UI_BORDER, 0.25);
        this.graphics.fillRoundedRect(LIST_X + 6, y - 1, LIST_W - 12, itemH - 2, 3);
      }

      const numStr = `#${String(speciesId).padStart(3, '0')}`;

      if (entry?.caught) {
        // Caught: show number, name, and pokeball icon
        let name = '???';
        if (hasSpecies(speciesId)) {
          name = getSpecies(speciesId).name;
        }
        const icon = '\u25CF'; // filled circle = caught
        this.addText(LIST_X + 12, y + 2, `${numStr} ${name}`, FONT_TINY,
          isSelected ? '#E8C868' : '#F0E8D0');
        this.addText(LIST_X + LIST_W - 20, y + 2, icon, FONT_TINY, '#48D848');
      } else if (entry?.seen) {
        // Seen: show number and name but grayed
        let name = '???';
        if (hasSpecies(speciesId)) {
          name = getSpecies(speciesId).name;
        }
        const icon = '\u25CB'; // empty circle = seen
        this.addText(LIST_X + 12, y + 2, `${numStr} ${name}`, FONT_TINY,
          isSelected ? '#C89840' : '#888888');
        this.addText(LIST_X + LIST_W - 20, y + 2, icon, FONT_TINY, '#888888');
      } else {
        // Unknown
        this.addText(LIST_X + 12, y + 2, `${numStr} ???`, FONT_TINY,
          isSelected ? '#886830' : '#505050');
      }
    }

    // Scroll indicators
    if (this.scrollOffset > 0) {
      this.addText(LIST_X + LIST_W / 2, startY - 8, '\u25B2', FONT_TINY, '#888888').setOrigin(0.5, 0.5);
    }
    if (this.scrollOffset + LIST_VISIBLE < TOTAL_DINOS) {
      this.addText(LIST_X + LIST_W / 2, startY + LIST_VISIBLE * itemH + 4, '\u25BC', FONT_TINY, '#888888').setOrigin(0.5, 0.5);
    }
  }

  private renderPreview(): void {
    this.drawPanel(DETAIL_X, 32, DETAIL_W, GAME_HEIGHT - 48);

    const speciesId = this.selectedIndex + 1;
    const gs = GameState.getInstance();
    const entry = gs.getDinodexEntry(speciesId);

    if (!entry?.seen) {
      this.addText(DETAIL_X + DETAIL_W / 2, GAME_HEIGHT / 2, 'Pas de donnees', FONT_SMALL, '#505050')
        .setOrigin(0.5, 0.5);
      return;
    }

    const caught = entry.caught;

    if (!hasSpecies(speciesId)) {
      this.addText(DETAIL_X + DETAIL_W / 2, GAME_HEIGHT / 2, `#${speciesId}`, FONT_BODY, '#888888')
        .setOrigin(0.5, 0.5);
      return;
    }

    const species = getSpecies(speciesId);

    // Dino shape
    if (caught) {
      this.drawDinoShape(DETAIL_X + DETAIL_W / 2 - 32, 60, species.type1, 64);
    } else {
      // Silhouette
      this.graphics.fillStyle(0x303030, 1);
      const cx = DETAIL_X + DETAIL_W / 2;
      const cy = 92;
      this.graphics.fillTriangle(cx - 32, cy + 32, cx, cy - 32, cx + 32, cy + 32);
      this.graphics.fillTriangle(cx - 32, cy + 32, cx, cy + 96, cx + 32, cy + 32);
    }

    // Name
    this.addText(DETAIL_X + 16, 140, `#${String(speciesId).padStart(3, '0')} ${species.name}`, FONT_SMALL,
      caught ? '#F0E8D0' : '#888888');

    if (caught) {
      // Type labels
      const type1Name = DINO_TYPE_NAMES[species.type1] ?? 'Normal';
      const type1Color = DINO_TYPE_COLORS[species.type1] ?? 0xa8a878;

      this.graphics.fillStyle(type1Color, 1);
      this.graphics.fillRoundedRect(DETAIL_X + 16, 166, 80, 18, 4);
      this.addText(DETAIL_X + 56, 168, type1Name, FONT_TINY, '#181018').setOrigin(0.5, 0);

      if (species.type2 !== undefined) {
        const type2Name = DINO_TYPE_NAMES[species.type2] ?? '';
        const type2Color = DINO_TYPE_COLORS[species.type2] ?? 0xa8a878;
        this.graphics.fillStyle(type2Color, 1);
        this.graphics.fillRoundedRect(DETAIL_X + 104, 166, 80, 18, 4);
        this.addText(DETAIL_X + 144, 168, type2Name, FONT_TINY, '#181018').setOrigin(0.5, 0);
      }

      // Height / Weight
      this.addText(DETAIL_X + 16, 196, `Taille: ${species.height}m`, FONT_TINY, '#F0E8D0');
      this.addText(DETAIL_X + 16, 216, `Poids: ${species.weight}kg`, FONT_TINY, '#F0E8D0');

      // Description
      this.addText(DETAIL_X + 16, 248, species.description, FONT_TINY, '#C8B898')
        .setWordWrapWidth(DETAIL_W - 32);

      this.addText(DETAIL_X + DETAIL_W / 2, GAME_HEIGHT - 52, 'Entree pour details', FONT_TINY, '#888888')
        .setOrigin(0.5, 0);
    } else {
      this.addText(DETAIL_X + 16, 166, 'Non capture', FONT_TINY, '#888888');
    }
  }

  private renderDetails(): void {
    // Full detail view replaces the right panel
    this.drawPanel(DETAIL_X, 32, DETAIL_W, GAME_HEIGHT - 48);

    const speciesId = this.selectedIndex + 1;
    const gs = GameState.getInstance();
    const entry = gs.getDinodexEntry(speciesId);

    if (!entry?.caught || !hasSpecies(speciesId)) {
      this.addText(DETAIL_X + DETAIL_W / 2, GAME_HEIGHT / 2, 'Pas de donnees', FONT_SMALL, '#505050')
        .setOrigin(0.5, 0.5);
      return;
    }

    const species = getSpecies(speciesId);

    // Large dino shape
    this.drawDinoShape(DETAIL_X + DETAIL_W / 2 - 40, 48, species.type1, 80);

    // Name
    this.addText(DETAIL_X + 16, 140, `#${String(speciesId).padStart(3, '0')} ${species.name}`, FONT_BODY, '#E8C868');

    // Types
    const type1Name = DINO_TYPE_NAMES[species.type1] ?? 'Normal';
    this.addText(DETAIL_X + 16, 170, `Type: ${type1Name}`, FONT_TINY, '#F0E8D0');
    if (species.type2 !== undefined) {
      const type2Name = DINO_TYPE_NAMES[species.type2] ?? '';
      this.addText(DETAIL_X + 180, 170, `/ ${type2Name}`, FONT_TINY, '#F0E8D0');
    }

    // Stats
    this.addText(DETAIL_X + 16, 196, `Taille: ${species.height}m`, FONT_TINY, '#F0E8D0');
    this.addText(DETAIL_X + 160, 196, `Poids: ${species.weight}kg`, FONT_TINY, '#F0E8D0');

    // Base stats
    this.addText(DETAIL_X + 16, 224, 'Stats de base:', FONT_TINY, '#E8C868');
    const bs = species.baseStats;
    this.addText(DETAIL_X + 16, 244, `PV:${bs.hp}  ATK:${bs.attack}  DEF:${bs.defense}`, FONT_TINY, '#F0E8D0');
    this.addText(DETAIL_X + 16, 264, `ATS:${bs.spAttack}  DFS:${bs.spDefense}  VIT:${bs.speed}`, FONT_TINY, '#F0E8D0');

    // Description
    this.addText(DETAIL_X + 16, 296, species.description, FONT_TINY, '#C8B898')
      .setWordWrapWidth(DETAIL_W - 32);

    this.addText(DETAIL_X + DETAIL_W / 2, GAME_HEIGHT - 52, 'ESC pour retour', FONT_TINY, '#888888')
      .setOrigin(0.5, 0);
  }

  // ===================== Input =====================

  private handleListInput(): void {
    if (Phaser.Input.Keyboard.JustDown(this.cursors.up)) {
      this.selectedIndex = Math.max(0, this.selectedIndex - 1);
      this.adjustScroll();
      this.render();
    }
    if (Phaser.Input.Keyboard.JustDown(this.cursors.down)) {
      this.selectedIndex = Math.min(TOTAL_DINOS - 1, this.selectedIndex + 1);
      this.adjustScroll();
      this.render();
    }
    if (Phaser.Input.Keyboard.JustDown(this.enterKey)) {
      const speciesId = this.selectedIndex + 1;
      const gs = GameState.getInstance();
      const entry = gs.getDinodexEntry(speciesId);
      if (entry?.caught) {
        this.showDetails = true;
        this.render();
      }
    }
    if (Phaser.Input.Keyboard.JustDown(this.escKey)) {
      this.exitScene();
    }
  }

  private handleDetailInput(): void {
    if (Phaser.Input.Keyboard.JustDown(this.escKey) ||
        Phaser.Input.Keyboard.JustDown(this.enterKey)) {
      this.showDetails = false;
      this.render();
    }
  }

  private adjustScroll(): void {
    if (this.selectedIndex < this.scrollOffset) {
      this.scrollOffset = this.selectedIndex;
    }
    if (this.selectedIndex >= this.scrollOffset + LIST_VISIBLE) {
      this.scrollOffset = this.selectedIndex - LIST_VISIBLE + 1;
    }
  }

  // ===================== Exit =====================

  private exitScene(): void {
    this.cameras.main.fadeOut(200, 0x18, 0x10, 0x18);
    this.cameras.main.once('camerafadeoutcomplete', () => {
      this.scene.start(this.returnScene);
    });
  }

  // ===================== Drawing Helpers =====================

  private drawPanel(x: number, y: number, w: number, h: number): void {
    this.graphics.fillStyle(COLORS.UI_BORDER_DARK, 1);
    this.graphics.fillRoundedRect(x, y, w, h, 8);
    this.graphics.fillStyle(COLORS.UI_BORDER, 1);
    this.graphics.fillRoundedRect(x + 3, y + 3, w - 6, h - 6, 7);
    this.graphics.fillStyle(COLORS.UI_BORDER_LIGHT, 1);
    this.graphics.fillRoundedRect(x + 5, y + 5, w - 10, h - 10, 6);
    this.graphics.fillStyle(COLORS.DIALOGUE_BG, 0.97);
    this.graphics.fillRoundedRect(x + 6, y + 6, w - 12, h - 12, 5);
  }

  private drawDinoShape(x: number, y: number, type: DinoType, size: number): void {
    const color = DINO_TYPE_COLORS[type] ?? 0xa8a878;
    this.graphics.fillStyle(color, 1);
    const half = size / 2;
    this.graphics.fillTriangle(x, y + half, x + half, y, x + size, y + half);
    this.graphics.fillTriangle(x, y + half, x + half, y + size, x + size, y + half);
    // Eye
    this.graphics.fillStyle(0x181018, 1);
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
