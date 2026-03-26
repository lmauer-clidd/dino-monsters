// ============================================================
// Jurassic Trainers -- Party Scene (team management)
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
  MAX_PARTY_SIZE,
  DinoType,
  DINO_TYPE_COLORS,
  DINO_TYPE_NAMES,
  StatusEffect,
} from '../utils/constants';
import { GameState } from '../systems/GameState';
import { Dino, getSpecies, hasSpecies } from '../entities/Dino';

// ===================== Scene =====================

type PartyMode = 'list' | 'submenu' | 'summary' | 'swap';

export class PartyScene extends Phaser.Scene {
  private graphics!: Phaser.GameObjects.Graphics;
  private texts: Phaser.GameObjects.Text[] = [];

  private mode: PartyMode = 'list';
  private selectedIndex = 0;
  private swapIndex = -1;
  private submenuIndex = 0;

  // Return info
  private returnScene = '';
  private fromBattle = false;

  // Keys
  private cursors!: Phaser.Types.Input.Keyboard.CursorKeys;
  private enterKey!: Phaser.Input.Keyboard.Key;
  private escKey!: Phaser.Input.Keyboard.Key;

  constructor() {
    super({ key: SCENE_KEYS.PARTY });
  }

  init(data: { returnScene?: string; fromBattle?: boolean }): void {
    this.returnScene = data.returnScene ?? SCENE_KEYS.OVERWORLD;
    this.fromBattle = data.fromBattle ?? false;
    this.selectedIndex = 0;
    this.mode = 'list';
    this.swapIndex = -1;
    this.submenuIndex = 0;
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
    switch (this.mode) {
      case 'list': this.handleListInput(); break;
      case 'submenu': this.handleSubmenuInput(); break;
      case 'summary': this.handleSummaryInput(); break;
      case 'swap': this.handleSwapInput(); break;
    }
  }

  // ===================== Rendering =====================

  private render(): void {
    this.graphics.clear();
    this.clearTexts();

    switch (this.mode) {
      case 'list':
      case 'swap':
        this.renderList();
        break;
      case 'submenu':
        this.renderList();
        this.renderSubmenu();
        break;
      case 'summary':
        this.renderSummary();
        break;
    }
  }

  private renderList(): void {
    // Full screen amber border
    this.drawPanel(0, 0, GAME_WIDTH, GAME_HEIGHT);

    const gs = GameState.getInstance();
    const party = gs.party;

    // Title
    const titleStr = this.mode === 'swap' ? 'CHANGER DE PLACE' : 'EQUIPE';
    this.addText(24, 16, titleStr, FONT_SMALL, '#E8C868');

    // Party slots
    const startY = 48;
    const slotH = 52;

    for (let i = 0; i < MAX_PARTY_SIZE; i++) {
      const y = startY + i * slotH;
      const isSelected = i === this.selectedIndex;
      const isSwapSource = this.mode === 'swap' && i === this.swapIndex;

      // Slot background
      if (isSelected) {
        this.graphics.fillStyle(COLORS.UI_BORDER, 0.3);
        this.graphics.fillRoundedRect(20, y, GAME_WIDTH - 40, slotH - 4, 6);
      }
      if (isSwapSource) {
        this.graphics.fillStyle(COLORS.XP_BLUE, 0.2);
        this.graphics.fillRoundedRect(20, y, GAME_WIDTH - 40, slotH - 4, 6);
      }

      if (i < party.length) {
        const dino = party[i];
        const prefix = isSelected ? '\u25B6 ' : '  ';

        // Dino colored shape (type-based)
        const shapeX = 50;
        const shapeY = y + 12;
        this.drawDinoShape(shapeX, shapeY, dino.type1, 24);

        // Name and level
        this.addText(80, y + 4, `${prefix}${dino.nickname}`, FONT_SMALL,
          isSelected ? '#E8C868' : '#F0E8D0');
        this.addText(80, y + 24, `Nv.${dino.level}`, FONT_TINY, '#F0E8D0');

        // HP bar
        const hpBarX = 320;
        const hpBarW = 180;
        const hpBarH = 8;
        const hpPercent = dino.getHpPercent();

        this.graphics.fillStyle(COLORS.BLACK, 1);
        this.graphics.fillRect(hpBarX, y + 10, hpBarW, hpBarH);

        const hpColor = hpPercent > 0.5 ? COLORS.HP_GREEN :
                        hpPercent > 0.25 ? COLORS.HP_YELLOW : COLORS.HP_RED;
        this.graphics.fillStyle(hpColor, 1);
        this.graphics.fillRect(hpBarX, y + 10, Math.floor(hpBarW * hpPercent), hpBarH);

        this.addText(hpBarX, y + 24, `${dino.currentHp}/${dino.maxHp}`, FONT_TINY, '#F0E8D0');

        // Type dots
        const dotX = 540;
        this.graphics.fillStyle(DINO_TYPE_COLORS[dino.type1], 1);
        this.graphics.fillCircle(dotX, y + 14, 6);
        if (dino.type2 !== undefined) {
          this.graphics.fillStyle(DINO_TYPE_COLORS[dino.type2], 1);
          this.graphics.fillCircle(dotX + 18, y + 14, 6);
        }

        // Status
        if (dino.status !== StatusEffect.None) {
          this.addText(dotX - 10, y + 28, dino.status.toUpperCase(), FONT_TINY, '#F85030');
        }
        if (dino.isFainted()) {
          this.addText(dotX - 10, y + 28, 'K.O.', FONT_TINY, '#F85030');
        }
      } else {
        // Empty slot
        const prefix = isSelected ? '\u25B6 ' : '  ';
        this.addText(80, y + 14, `${prefix}---`, FONT_SMALL, '#606060');
      }
    }
  }

  private renderSubmenu(): void {
    const menuItems = this.fromBattle
      ? ['CHOISIR', 'RESUME', 'ANNULER']
      : ['RESUME', 'CHANGER', 'ANNULER'];

    const menuW = 160;
    const itemH = 24;
    const menuH = menuItems.length * itemH + 16;
    const menuX = GAME_WIDTH - menuW - 32;
    const menuY = GAME_HEIGHT / 2 - menuH / 2;

    this.drawPanel(menuX, menuY, menuW, menuH);

    for (let i = 0; i < menuItems.length; i++) {
      const isSelected = i === this.submenuIndex;
      const prefix = isSelected ? '\u25B6 ' : '  ';
      this.addText(menuX + 14, menuY + 8 + i * itemH,
        `${prefix}${menuItems[i]}`, FONT_TINY,
        isSelected ? '#E8C868' : '#F0E8D0');
    }
  }

  private renderSummary(): void {
    this.drawPanel(0, 0, GAME_WIDTH, GAME_HEIGHT);

    const gs = GameState.getInstance();
    const party = gs.party;
    if (this.selectedIndex >= party.length) return;

    const dino = party[this.selectedIndex];
    const species = dino.species;

    // Title
    this.addText(24, 16, 'RESUME', FONT_SMALL, '#E8C868');

    // Dino shape (large)
    this.drawDinoShape(80, 60, dino.type1, 64);

    // Name & Level
    this.addText(180, 50, dino.nickname, FONT_BODY, '#F0E8D0');
    this.addText(180, 76, `Nv. ${dino.level}`, FONT_SMALL, '#F0E8D0');

    // Types
    const type1Name = DINO_TYPE_NAMES[dino.type1] ?? 'Normal';
    this.addText(180, 100, `Type: ${type1Name}`, FONT_TINY, '#F0E8D0');
    if (dino.type2 !== undefined) {
      const type2Name = DINO_TYPE_NAMES[dino.type2] ?? '';
      this.addText(350, 100, `/ ${type2Name}`, FONT_TINY, '#F0E8D0');
    }

    // HP Bar
    this.addText(180, 128, `PV: ${dino.currentHp} / ${dino.maxHp}`, FONT_TINY, '#F0E8D0');
    const hpBarX = 180;
    const hpBarW = 200;
    const hpPercent = dino.getHpPercent();
    this.graphics.fillStyle(COLORS.BLACK, 1);
    this.graphics.fillRect(hpBarX, 146, hpBarW, 10);
    const hpColor = hpPercent > 0.5 ? COLORS.HP_GREEN :
                    hpPercent > 0.25 ? COLORS.HP_YELLOW : COLORS.HP_RED;
    this.graphics.fillStyle(hpColor, 1);
    this.graphics.fillRect(hpBarX, 146, Math.floor(hpBarW * hpPercent), 10);

    // XP
    this.addText(180, 164, `EXP: ${dino.getXpProgress()} / ${dino.getXpToNextLevel()}`, FONT_TINY, '#58A8F8');

    // Stats
    const statsY = 200;
    const col1X = 40;
    const col2X = 320;

    this.addText(col1X, statsY, 'STATISTIQUES', FONT_TINY, '#E8C868');
    this.addText(col1X, statsY + 22, `Attaque:     ${dino.stats.attack}`, FONT_TINY, '#F0E8D0');
    this.addText(col1X, statsY + 42, `Defense:     ${dino.stats.defense}`, FONT_TINY, '#F0E8D0');
    this.addText(col1X, statsY + 62, `Att. Spe:    ${dino.stats.spAttack}`, FONT_TINY, '#F0E8D0');
    this.addText(col1X, statsY + 82, `Def. Spe:    ${dino.stats.spDefense}`, FONT_TINY, '#F0E8D0');
    this.addText(col1X, statsY + 102, `Vitesse:     ${dino.stats.speed}`, FONT_TINY, '#F0E8D0');

    // Moves
    this.addText(col2X, statsY, 'CAPACITES', FONT_TINY, '#E8C868');
    for (let i = 0; i < dino.moves.length; i++) {
      const move = dino.moves[i];
      this.addText(col2X, statsY + 22 + i * 20,
        `${i + 1}. Move#${move.moveId}  PP:${move.currentPP}/${move.maxPP}`,
        FONT_TINY, '#F0E8D0');
    }

    // Temperament
    this.addText(col1X, statsY + 132, `Temperament: ${dino.temperament.name}`, FONT_TINY, '#C89840');

    // Status
    if (dino.status !== StatusEffect.None) {
      this.addText(col2X, statsY + 132, `Statut: ${dino.status}`, FONT_TINY, '#F85030');
    }

    // Back instruction
    this.addText(GAME_WIDTH / 2 - 80, GAME_HEIGHT - 32, 'ESC pour retour', FONT_TINY, '#888888');
  }

  // ===================== Input Handlers =====================

  private handleListInput(): void {
    const gs = GameState.getInstance();
    const partySize = gs.party.length;

    if (Phaser.Input.Keyboard.JustDown(this.cursors.up)) {
      this.selectedIndex = (this.selectedIndex - 1 + partySize) % partySize;
      this.render();
    }
    if (Phaser.Input.Keyboard.JustDown(this.cursors.down)) {
      this.selectedIndex = (this.selectedIndex + 1) % partySize;
      this.render();
    }
    if (Phaser.Input.Keyboard.JustDown(this.enterKey)) {
      if (this.selectedIndex < partySize) {
        this.mode = 'submenu';
        this.submenuIndex = 0;
        this.render();
      }
    }
    if (Phaser.Input.Keyboard.JustDown(this.escKey)) {
      this.exitScene();
    }
  }

  private handleSubmenuInput(): void {
    const menuItems = this.fromBattle
      ? ['CHOISIR', 'RESUME', 'ANNULER']
      : ['RESUME', 'CHANGER', 'ANNULER'];

    if (Phaser.Input.Keyboard.JustDown(this.cursors.up)) {
      this.submenuIndex = (this.submenuIndex - 1 + menuItems.length) % menuItems.length;
      this.render();
    }
    if (Phaser.Input.Keyboard.JustDown(this.cursors.down)) {
      this.submenuIndex = (this.submenuIndex + 1) % menuItems.length;
      this.render();
    }
    if (Phaser.Input.Keyboard.JustDown(this.enterKey)) {
      const action = menuItems[this.submenuIndex];
      switch (action) {
        case 'RESUME':
          this.mode = 'summary';
          this.render();
          break;
        case 'CHANGER':
          this.swapIndex = this.selectedIndex;
          this.mode = 'swap';
          this.render();
          break;
        case 'CHOISIR':
          // Battle switch: return selected dino index
          this.exitScene(this.selectedIndex);
          break;
        case 'ANNULER':
          this.mode = 'list';
          this.render();
          break;
      }
    }
    if (Phaser.Input.Keyboard.JustDown(this.escKey)) {
      this.mode = 'list';
      this.render();
    }
  }

  private handleSummaryInput(): void {
    if (Phaser.Input.Keyboard.JustDown(this.escKey) ||
        Phaser.Input.Keyboard.JustDown(this.enterKey)) {
      this.mode = 'list';
      this.render();
    }
  }

  private handleSwapInput(): void {
    const gs = GameState.getInstance();
    const partySize = gs.party.length;

    if (Phaser.Input.Keyboard.JustDown(this.cursors.up)) {
      this.selectedIndex = (this.selectedIndex - 1 + partySize) % partySize;
      this.render();
    }
    if (Phaser.Input.Keyboard.JustDown(this.cursors.down)) {
      this.selectedIndex = (this.selectedIndex + 1) % partySize;
      this.render();
    }
    if (Phaser.Input.Keyboard.JustDown(this.enterKey)) {
      if (this.selectedIndex !== this.swapIndex) {
        gs.getPartySystem().swapPartyOrder(this.swapIndex, this.selectedIndex);
      }
      this.swapIndex = -1;
      this.mode = 'list';
      this.render();
    }
    if (Phaser.Input.Keyboard.JustDown(this.escKey)) {
      this.swapIndex = -1;
      this.mode = 'list';
      this.render();
    }
  }

  // ===================== Exit =====================

  private exitScene(selectedDinoIndex?: number): void {
    this.cameras.main.fadeOut(200, 0x18, 0x10, 0x18);
    this.cameras.main.once('camerafadeoutcomplete', () => {
      if (this.fromBattle && selectedDinoIndex !== undefined) {
        this.scene.start(SCENE_KEYS.BATTLE, { switchTo: selectedDinoIndex });
      } else {
        this.scene.start(this.returnScene);
      }
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
    // Draw a hexagon-like shape
    const half = size / 2;
    const qtr = size / 4;
    this.graphics.fillTriangle(
      x, y + half,
      x + half, y,
      x + size, y + half,
    );
    this.graphics.fillTriangle(
      x, y + half,
      x + half, y + size,
      x + size, y + half,
    );
    // Eye
    this.graphics.fillStyle(0x181018, 1);
    this.graphics.fillCircle(x + half + qtr / 2, y + half - qtr / 3, Math.max(2, size / 12));
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
