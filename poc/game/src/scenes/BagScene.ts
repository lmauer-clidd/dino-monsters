// ============================================================
// Jurassic Trainers -- Bag Scene (Inventory / Sac)
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
  StatusEffect,
  DinoType,
  DINO_TYPE_COLORS,
} from '../utils/constants';
import { GameState } from '../systems/GameState';
import { DialogueBox } from '../ui/DialogueBox';
import { ItemCategory, IItemData, getItem, hasItemData } from '../systems/InventorySystem';
import { Dino } from '../entities/Dino';

// ===================== Tab definition =====================

interface BagTab {
  label: string;
  category: ItemCategory;
}

const TABS: BagTab[] = [
  { label: 'SOINS', category: 'healing' },
  { label: 'BALLS', category: 'capture' },
  { label: 'COMBAT', category: 'battle' },
  { label: 'CLES', category: 'key' },
];

// ===================== Scene modes =====================

type BagMode = 'browse' | 'action' | 'target';

// ===================== Scene =====================

export class BagScene extends Phaser.Scene {
  private graphics!: Phaser.GameObjects.Graphics;
  private texts: Phaser.GameObjects.Text[] = [];
  private dialogueBox!: DialogueBox;

  private mode: BagMode = 'browse';
  private tabIndex = 0;
  private selectedIndex = 0;
  private scrollOffset = 0;
  private maxVisible = 10;
  private actionIndex = 0;
  private targetIndex = 0;

  // Cached current items
  private currentItems: { item: IItemData; quantity: number }[] = [];

  // Return info
  private returnScene = '';

  // Keys
  private cursors!: Phaser.Types.Input.Keyboard.CursorKeys;
  private enterKey!: Phaser.Input.Keyboard.Key;
  private escKey!: Phaser.Input.Keyboard.Key;

  constructor() {
    super({ key: SCENE_KEYS.BAG });
  }

  init(data: { returnScene?: string }): void {
    this.returnScene = data.returnScene ?? SCENE_KEYS.OVERWORLD;
    this.tabIndex = 0;
    this.selectedIndex = 0;
    this.scrollOffset = 0;
    this.mode = 'browse';
    this.actionIndex = 0;
    this.targetIndex = 0;
  }

  create(): void {
    this.graphics = this.add.graphics();
    this.dialogueBox = new DialogueBox(this);

    if (this.input.keyboard) {
      this.cursors = this.input.keyboard.createCursorKeys();
      this.enterKey = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.ENTER);
      this.escKey = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.ESC);
    }

    this.cameras.main.fadeIn(200, 0x18, 0x10, 0x18);
    this.refreshItems();
    this.render();
  }

  update(): void {
    if (this.dialogueBox.isActive()) {
      this.dialogueBox.update();
      return;
    }

    switch (this.mode) {
      case 'browse': this.handleBrowseInput(); break;
      case 'action': this.handleActionInput(); break;
      case 'target': this.handleTargetInput(); break;
    }
  }

  // ===================== Data =====================

  private refreshItems(): void {
    const gs = GameState.getInstance();
    const inv = gs.getInventorySystem();
    const category = TABS[this.tabIndex].category;
    this.currentItems = inv.getItemsByCategory(category);
  }

  // ===================== Rendering =====================

  private render(): void {
    this.graphics.clear();
    this.clearTexts();

    // Full screen panel
    this.drawPanel(0, 0, GAME_WIDTH, GAME_HEIGHT);

    // Tabs
    this.renderTabs();

    switch (this.mode) {
      case 'browse':
        this.renderItemList();
        break;
      case 'action':
        this.renderItemList();
        this.renderActionMenu();
        break;
      case 'target':
        this.renderTargetList();
        break;
    }
  }

  private renderTabs(): void {
    const tabW = Math.floor((GAME_WIDTH - 48) / TABS.length);
    const tabY = 12;

    for (let i = 0; i < TABS.length; i++) {
      const tabX = 24 + i * tabW;
      const isActive = i === this.tabIndex;

      if (isActive) {
        this.graphics.fillStyle(COLORS.UI_BORDER, 0.8);
        this.graphics.fillRoundedRect(tabX, tabY, tabW - 4, 26, 4);
      } else {
        this.graphics.fillStyle(COLORS.UI_BORDER_DARK, 0.5);
        this.graphics.fillRoundedRect(tabX, tabY, tabW - 4, 26, 4);
      }

      this.addText(
        tabX + (tabW - 4) / 2,
        tabY + 6,
        TABS[i].label,
        FONT_TINY,
        isActive ? '#E8C868' : '#888888',
      ).setOrigin(0.5, 0);
    }
  }

  private renderItemList(): void {
    const startY = 50;
    const itemH = 28;

    if (this.currentItems.length === 0) {
      this.addText(GAME_WIDTH / 2, GAME_HEIGHT / 2, 'Aucun objet', FONT_SMALL, '#888888').setOrigin(0.5, 0.5);
      return;
    }

    for (let i = 0; i < Math.min(this.maxVisible, this.currentItems.length); i++) {
      const idx = i + this.scrollOffset;
      if (idx >= this.currentItems.length) break;

      const entry = this.currentItems[idx];
      const isSelected = idx === this.selectedIndex;
      const y = startY + i * itemH;

      if (isSelected) {
        this.graphics.fillStyle(COLORS.UI_BORDER, 0.2);
        this.graphics.fillRoundedRect(24, y - 2, GAME_WIDTH - 48, itemH - 2, 4);
      }

      const prefix = isSelected ? '\u25B6 ' : '  ';
      this.addText(32, y + 2, `${prefix}${entry.item.name}`, FONT_TINY,
        isSelected ? '#E8C868' : '#F0E8D0');

      this.addText(350, y + 2, `x${entry.quantity}`, FONT_TINY, '#F0E8D0');

      // Description for selected item
      if (isSelected) {
        // Show description at bottom
        this.drawPanel(16, GAME_HEIGHT - 72, GAME_WIDTH - 32, 56);
        this.addText(32, GAME_HEIGHT - 62, entry.item.description, FONT_TINY, '#F0E8D0');
      }
    }
  }

  private renderActionMenu(): void {
    const actions = this.getAvailableActions();
    const menuW = 140;
    const itemH = 24;
    const menuH = actions.length * itemH + 16;
    const menuX = GAME_WIDTH - menuW - 40;
    const menuY = 60;

    this.drawPanel(menuX, menuY, menuW, menuH);

    for (let i = 0; i < actions.length; i++) {
      const isSelected = i === this.actionIndex;
      const prefix = isSelected ? '\u25B6 ' : '  ';
      this.addText(menuX + 12, menuY + 8 + i * itemH,
        `${prefix}${actions[i]}`, FONT_TINY,
        isSelected ? '#E8C868' : '#F0E8D0');
    }
  }

  private renderTargetList(): void {
    // Show party members to select a target
    this.drawPanel(0, 0, GAME_WIDTH, GAME_HEIGHT);
    this.addText(24, 16, 'CHOISIR UN DINO', FONT_SMALL, '#E8C868');

    const gs = GameState.getInstance();
    const party = gs.party;
    const startY = 50;
    const slotH = 48;

    for (let i = 0; i < party.length; i++) {
      const dino = party[i];
      const y = startY + i * slotH;
      const isSelected = i === this.targetIndex;

      if (isSelected) {
        this.graphics.fillStyle(COLORS.UI_BORDER, 0.3);
        this.graphics.fillRoundedRect(20, y, GAME_WIDTH - 40, slotH - 4, 6);
      }

      const prefix = isSelected ? '\u25B6 ' : '  ';

      // Dino shape
      this.drawDinoShape(50, y + 8, dino.type1, 24);

      this.addText(80, y + 4, `${prefix}${dino.nickname}`, FONT_SMALL,
        isSelected ? '#E8C868' : '#F0E8D0');
      this.addText(80, y + 24, `Nv.${dino.level}  PV:${dino.currentHp}/${dino.maxHp}`, FONT_TINY, '#F0E8D0');

      // HP bar
      const hpBarX = 380;
      const hpBarW = 150;
      const hpPercent = dino.getHpPercent();
      this.graphics.fillStyle(COLORS.BLACK, 1);
      this.graphics.fillRect(hpBarX, y + 10, hpBarW, 8);
      const hpColor = hpPercent > 0.5 ? COLORS.HP_GREEN :
                      hpPercent > 0.25 ? COLORS.HP_YELLOW : COLORS.HP_RED;
      this.graphics.fillStyle(hpColor, 1);
      this.graphics.fillRect(hpBarX, y + 10, Math.floor(hpBarW * hpPercent), 8);

      if (dino.isFainted()) {
        this.addText(hpBarX, y + 24, 'K.O.', FONT_TINY, '#F85030');
      }
    }

    this.addText(GAME_WIDTH / 2 - 80, GAME_HEIGHT - 32, 'ESC pour annuler', FONT_TINY, '#888888');
  }

  // ===================== Input =====================

  private handleBrowseInput(): void {
    if (Phaser.Input.Keyboard.JustDown(this.cursors.left)) {
      this.tabIndex = (this.tabIndex - 1 + TABS.length) % TABS.length;
      this.selectedIndex = 0;
      this.scrollOffset = 0;
      this.refreshItems();
      this.render();
    }
    if (Phaser.Input.Keyboard.JustDown(this.cursors.right)) {
      this.tabIndex = (this.tabIndex + 1) % TABS.length;
      this.selectedIndex = 0;
      this.scrollOffset = 0;
      this.refreshItems();
      this.render();
    }
    if (Phaser.Input.Keyboard.JustDown(this.cursors.up)) {
      if (this.currentItems.length > 0) {
        this.selectedIndex = (this.selectedIndex - 1 + this.currentItems.length) % this.currentItems.length;
        this.adjustScroll();
        this.render();
      }
    }
    if (Phaser.Input.Keyboard.JustDown(this.cursors.down)) {
      if (this.currentItems.length > 0) {
        this.selectedIndex = (this.selectedIndex + 1) % this.currentItems.length;
        this.adjustScroll();
        this.render();
      }
    }
    if (Phaser.Input.Keyboard.JustDown(this.enterKey)) {
      if (this.currentItems.length > 0) {
        this.mode = 'action';
        this.actionIndex = 0;
        this.render();
      }
    }
    if (Phaser.Input.Keyboard.JustDown(this.escKey)) {
      this.exitScene();
    }
  }

  private handleActionInput(): void {
    const actions = this.getAvailableActions();

    if (Phaser.Input.Keyboard.JustDown(this.cursors.up)) {
      this.actionIndex = (this.actionIndex - 1 + actions.length) % actions.length;
      this.render();
    }
    if (Phaser.Input.Keyboard.JustDown(this.cursors.down)) {
      this.actionIndex = (this.actionIndex + 1) % actions.length;
      this.render();
    }
    if (Phaser.Input.Keyboard.JustDown(this.enterKey)) {
      const action = actions[this.actionIndex];
      this.executeAction(action);
    }
    if (Phaser.Input.Keyboard.JustDown(this.escKey)) {
      this.mode = 'browse';
      this.render();
    }
  }

  private handleTargetInput(): void {
    const gs = GameState.getInstance();
    const partySize = gs.party.length;

    if (Phaser.Input.Keyboard.JustDown(this.cursors.up)) {
      this.targetIndex = (this.targetIndex - 1 + partySize) % partySize;
      this.render();
    }
    if (Phaser.Input.Keyboard.JustDown(this.cursors.down)) {
      this.targetIndex = (this.targetIndex + 1) % partySize;
      this.render();
    }
    if (Phaser.Input.Keyboard.JustDown(this.enterKey)) {
      this.useItemOnTarget();
    }
    if (Phaser.Input.Keyboard.JustDown(this.escKey)) {
      this.mode = 'browse';
      this.render();
    }
  }

  // ===================== Actions =====================

  private getAvailableActions(): string[] {
    if (this.selectedIndex >= this.currentItems.length) return ['ANNULER'];
    const entry = this.currentItems[this.selectedIndex];
    const actions: string[] = [];

    if (entry.item.usableOutside) {
      actions.push('UTILISER');
    }
    if (entry.item.category !== 'key') {
      actions.push('JETER');
    }
    actions.push('ANNULER');
    return actions;
  }

  private executeAction(action: string): void {
    switch (action) {
      case 'UTILISER':
        this.mode = 'target';
        this.targetIndex = 0;
        this.render();
        break;
      case 'JETER':
        this.tossItem();
        break;
      case 'ANNULER':
        this.mode = 'browse';
        this.render();
        break;
    }
  }

  private useItemOnTarget(): void {
    if (this.selectedIndex >= this.currentItems.length) return;

    const entry = this.currentItems[this.selectedIndex];
    const gs = GameState.getInstance();
    const party = gs.party;
    const target = party[this.targetIndex] as Dino;

    const result = gs.getInventorySystem().useItem(entry.item.id, target);

    this.mode = 'browse';
    this.dialogueBox.showText(result.message, () => {
      this.refreshItems();
      // If items ran out, adjust selectedIndex
      if (this.selectedIndex >= this.currentItems.length) {
        this.selectedIndex = Math.max(0, this.currentItems.length - 1);
      }
      this.render();
    });
  }

  private tossItem(): void {
    if (this.selectedIndex >= this.currentItems.length) return;

    const entry = this.currentItems[this.selectedIndex];
    const gs = GameState.getInstance();
    gs.getInventorySystem().removeItem(entry.item.id, 1);

    this.mode = 'browse';
    this.dialogueBox.showText(`Vous avez jete 1x ${entry.item.name}.`, () => {
      this.refreshItems();
      if (this.selectedIndex >= this.currentItems.length) {
        this.selectedIndex = Math.max(0, this.currentItems.length - 1);
      }
      this.render();
    });
  }

  // ===================== Helpers =====================

  private adjustScroll(): void {
    if (this.selectedIndex < this.scrollOffset) {
      this.scrollOffset = this.selectedIndex;
    }
    if (this.selectedIndex >= this.scrollOffset + this.maxVisible) {
      this.scrollOffset = this.selectedIndex - this.maxVisible + 1;
    }
  }

  private exitScene(): void {
    this.cameras.main.fadeOut(200, 0x18, 0x10, 0x18);
    this.cameras.main.once('camerafadeoutcomplete', () => {
      this.dialogueBox.destroy();
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
