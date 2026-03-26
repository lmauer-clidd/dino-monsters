import Phaser from 'phaser';
import {
  GAME_WIDTH,
  GAME_HEIGHT,
  COLORS,
  FONT_FAMILY,
  FONT_SMALL,
  FONT_TINY,
} from '../utils/constants';

const MENU_OPTIONS = [
  'EQUIPE',
  'SAC',
  'DINODEX',
  'CARTE',
  'SAUVEGARDER',
  'OPTIONS',
  'QUITTER',
];

// --- Menu option icon colors (placeholder squares) ---
const MENU_ICON_COLORS = [
  0xF08030, // EQUIPE - orange
  0x78C850, // SAC - green
  0x6890F0, // DINODEX - blue
  0xF8D030, // CARTE - yellow
  0xA040A0, // SAUVEGARDER - purple
  0xB8B8D0, // OPTIONS - gray
  0xF85030, // QUITTER - red
];

const MENU_W = 200;
const MENU_ITEM_H = 32;
const MENU_PADDING = 16;
const BORDER_OUTER = 3;
const BORDER_INNER = 2;
const CORNER_RADIUS = 8;
const TEXT_COLOR = '#F0E8D0';
const TEXT_SHADOW_COLOR = '#181018';

export class MenuUI {
  private scene!: Phaser.Scene;
  private container!: Phaser.GameObjects.Container;
  private optionTexts: Phaser.GameObjects.Text[] = [];
  private iconGraphics: Phaser.GameObjects.Graphics[] = [];
  private selectedIndex = 0;
  private _isOpen = false;

  // Input
  private upKey?: Phaser.Input.Keyboard.Key;
  private downKey?: Phaser.Input.Keyboard.Key;
  private enterKey?: Phaser.Input.Keyboard.Key;
  private escKey?: Phaser.Input.Keyboard.Key;

  private onSelect?: (option: string, index: number) => void;

  open(scene: Phaser.Scene, onSelect?: (option: string, index: number) => void): void {
    if (this._isOpen) return;
    this.scene = scene;
    this._isOpen = true;
    this.selectedIndex = 0;
    this.onSelect = onSelect;

    this.container = scene.add.container(0, 0).setDepth(1100).setScrollFactor(0);

    // --- Dim overlay ---
    const dim = scene.add.graphics();
    dim.fillStyle(COLORS.BLACK, 0.5);
    dim.fillRect(0, 0, GAME_WIDTH, GAME_HEIGHT);
    this.container.add(dim);

    // --- Menu panel (right side) ---
    const menuH = MENU_OPTIONS.length * MENU_ITEM_H + MENU_PADDING * 2;
    const menuX = GAME_WIDTH - MENU_W - 16;
    const menuY = 16;

    const gfx = scene.add.graphics();

    // 3-layer amber border
    gfx.fillStyle(COLORS.UI_BORDER_DARK, 1);
    gfx.fillRoundedRect(menuX, menuY, MENU_W, menuH, CORNER_RADIUS);

    gfx.fillStyle(COLORS.UI_BORDER, 1);
    gfx.fillRoundedRect(
      menuX + BORDER_OUTER,
      menuY + BORDER_OUTER,
      MENU_W - BORDER_OUTER * 2,
      menuH - BORDER_OUTER * 2,
      CORNER_RADIUS - 1
    );

    gfx.fillStyle(COLORS.UI_BORDER_LIGHT, 1);
    gfx.fillRoundedRect(
      menuX + BORDER_OUTER + BORDER_INNER,
      menuY + BORDER_OUTER + BORDER_INNER,
      MENU_W - (BORDER_OUTER + BORDER_INNER) * 2,
      menuH - (BORDER_OUTER + BORDER_INNER) * 2,
      CORNER_RADIUS - 2
    );

    gfx.fillStyle(COLORS.MENU_BG, 0.97);
    gfx.fillRoundedRect(
      menuX + BORDER_OUTER + BORDER_INNER + 1,
      menuY + BORDER_OUTER + BORDER_INNER + 1,
      MENU_W - (BORDER_OUTER + BORDER_INNER + 1) * 2,
      menuH - (BORDER_OUTER + BORDER_INNER + 1) * 2,
      CORNER_RADIUS - 3
    );

    this.container.add(gfx);

    // --- Options with icons ---
    this.optionTexts = [];
    this.iconGraphics = [];

    MENU_OPTIONS.forEach((label, i) => {
      const itemY = menuY + MENU_PADDING + i * MENU_ITEM_H;

      // Icon placeholder (small colored square)
      const iconGfx = scene.add.graphics();
      iconGfx.fillStyle(MENU_ICON_COLORS[i], 1);
      iconGfx.fillRoundedRect(menuX + MENU_PADDING + 4, itemY + 6, 12, 12, 2);
      this.container.add(iconGfx);
      this.iconGraphics.push(iconGfx);

      // Option text
      const t = scene.add.text(
        menuX + MENU_PADDING + 24,
        itemY + 4,
        label,
        {
          fontFamily: FONT_FAMILY,
          fontSize: FONT_SMALL,
          color: TEXT_COLOR,
          shadow: {
            offsetX: 1,
            offsetY: 1,
            color: TEXT_SHADOW_COLOR,
            blur: 0,
            fill: true,
          },
        }
      );
      this.container.add(t);
      this.optionTexts.push(t);
    });

    this.updateHighlight();

    // --- Input ---
    const kb = scene.input.keyboard;
    if (kb) {
      this.upKey = kb.addKey(Phaser.Input.Keyboard.KeyCodes.UP);
      this.downKey = kb.addKey(Phaser.Input.Keyboard.KeyCodes.DOWN);
      this.enterKey = kb.addKey(Phaser.Input.Keyboard.KeyCodes.ENTER);
      this.escKey = kb.addKey(Phaser.Input.Keyboard.KeyCodes.ESC);
    }
  }

  close(): void {
    if (!this._isOpen) return;
    this._isOpen = false;
    this.container?.destroy();
    this.optionTexts = [];
    this.iconGraphics = [];
  }

  isOpen(): boolean {
    return this._isOpen;
  }

  private updateHighlight(): void {
    this.optionTexts.forEach((t, i) => {
      const isSelected = i === this.selectedIndex;
      const prefix = isSelected ? '\u25B6 ' : '   ';
      t.setText(prefix + MENU_OPTIONS[i]);
      t.setColor(isSelected ? '#E8C868' : TEXT_COLOR);
    });
  }

  /** Call in scene update */
  update(): void {
    if (!this._isOpen) return;

    if (this.upKey && Phaser.Input.Keyboard.JustDown(this.upKey)) {
      this.selectedIndex = (this.selectedIndex - 1 + MENU_OPTIONS.length) % MENU_OPTIONS.length;
      this.updateHighlight();
    }
    if (this.downKey && Phaser.Input.Keyboard.JustDown(this.downKey)) {
      this.selectedIndex = (this.selectedIndex + 1) % MENU_OPTIONS.length;
      this.updateHighlight();
    }
    if (this.enterKey && Phaser.Input.Keyboard.JustDown(this.enterKey)) {
      const option = MENU_OPTIONS[this.selectedIndex];
      if (option === 'QUITTER') {
        this.close();
      } else {
        this.onSelect?.(option, this.selectedIndex);
      }
    }
    if (this.escKey && Phaser.Input.Keyboard.JustDown(this.escKey)) {
      this.close();
    }
  }
}
