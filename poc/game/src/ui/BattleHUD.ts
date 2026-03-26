import Phaser from 'phaser';
import {
  GAME_WIDTH,
  GAME_HEIGHT,
  COLORS,
  FONT_FAMILY,
  FONT_SMALL,
  FONT_TINY,
  DinoType,
  DINO_TYPE_COLORS,
  DINO_TYPE_NAMES,
  StatusEffect,
} from '../utils/constants';

// --- Layout ---
const BORDER_OUTER = 3;
const BORDER_INNER = 2;
const CORNER_RADIUS = 8;
const TEXT_COLOR = '#F0E8D0';
const TEXT_COLOR_DIM = '#B8B0A0';
const TEXT_SHADOW_COLOR = '#181018';

// --- Panel dimensions ---
const ENEMY_PANEL_W = 230;
const ENEMY_PANEL_H = 60;
const PLAYER_PANEL_W = 250;
const PLAYER_PANEL_H = 76;

// --- Menu dimensions ---
const ACTION_MENU_W = 290;
const ACTION_MENU_H = 96;
const MOVE_MENU_W = 420;
const MOVE_MENU_H = 96;

// --- HP Bar ---
const HP_BAR_W = 120;
const HP_BAR_H = 8;
const XP_BAR_H = 4;

interface MoveInfo {
  name: string;
  type: DinoType;
  power: number;
  pp: number;
  maxPp: number;
  category?: 'physical' | 'special' | 'status';
}

/** Shared text style config */
function textStyle(fontSize: string, color = TEXT_COLOR): Phaser.Types.GameObjects.Text.TextStyle {
  return {
    fontFamily: FONT_FAMILY,
    fontSize,
    color,
    shadow: {
      offsetX: 1,
      offsetY: 1,
      color: TEXT_SHADOW_COLOR,
      blur: 0,
      fill: true,
    },
  };
}

/** Draw the signature 3-layer amber border */
function drawAmberPanel(
  gfx: Phaser.GameObjects.Graphics,
  x: number,
  y: number,
  w: number,
  h: number,
  radius = CORNER_RADIUS,
  bgColor = COLORS.UI_BG,
  bgAlpha = 0.95
): void {
  // Outer dark amber
  gfx.fillStyle(COLORS.UI_BORDER_DARK, 1);
  gfx.fillRoundedRect(x, y, w, h, radius);

  // Middle amber
  gfx.fillStyle(COLORS.UI_BORDER, 1);
  gfx.fillRoundedRect(x + BORDER_OUTER, y + BORDER_OUTER, w - BORDER_OUTER * 2, h - BORDER_OUTER * 2, radius - 1);

  // Inner light amber
  gfx.fillStyle(COLORS.UI_BORDER_LIGHT, 1);
  gfx.fillRoundedRect(
    x + BORDER_OUTER + BORDER_INNER,
    y + BORDER_OUTER + BORDER_INNER,
    w - (BORDER_OUTER + BORDER_INNER) * 2,
    h - (BORDER_OUTER + BORDER_INNER) * 2,
    radius - 2
  );

  // Inner background
  gfx.fillStyle(bgColor, bgAlpha);
  gfx.fillRoundedRect(
    x + BORDER_OUTER + BORDER_INNER + 1,
    y + BORDER_OUTER + BORDER_INNER + 1,
    w - (BORDER_OUTER + BORDER_INNER + 1) * 2,
    h - (BORDER_OUTER + BORDER_INNER + 1) * 2,
    radius - 3
  );
}

export class BattleHUD {
  private scene!: Phaser.Scene;
  private container!: Phaser.GameObjects.Container;

  // Enemy info panel (top-left)
  private enemyPanel!: Phaser.GameObjects.Container;
  private enemyPanelBg!: Phaser.GameObjects.Graphics;
  private enemyNameText!: Phaser.GameObjects.Text;
  private enemyLevelText!: Phaser.GameObjects.Text;
  private enemyHPBar!: Phaser.GameObjects.Graphics;
  private enemyHPCurrent = 1;
  private enemyHPMax = 1;
  private enemyTypeDots!: Phaser.GameObjects.Graphics;
  private enemyStatusText!: Phaser.GameObjects.Text;

  // Player info panel (bottom-right)
  private playerPanel!: Phaser.GameObjects.Container;
  private playerPanelBg!: Phaser.GameObjects.Graphics;
  private playerNameText!: Phaser.GameObjects.Text;
  private playerLevelText!: Phaser.GameObjects.Text;
  private playerHPBar!: Phaser.GameObjects.Graphics;
  private playerHPText!: Phaser.GameObjects.Text;
  private playerXPBar!: Phaser.GameObjects.Graphics;
  private playerHPCurrent = 1;
  private playerHPMax = 1;
  private playerXPCurrent = 0;
  private playerXPMax = 1;
  private playerStatusText!: Phaser.GameObjects.Text;

  // Action menu
  private actionMenu!: Phaser.GameObjects.Container;
  private actionMenuBg!: Phaser.GameObjects.Graphics;
  private actionTexts: Phaser.GameObjects.Text[] = [];
  private actionButtonBgs: Phaser.GameObjects.Graphics[] = [];
  private actionIndex = 0;
  private readonly actionLabels = ['COMBAT', 'SAC', 'DINOS', 'FUITE'];

  // Move menu
  private moveMenu!: Phaser.GameObjects.Container;
  private moveMenuBg!: Phaser.GameObjects.Graphics;
  private moveTexts: Phaser.GameObjects.Text[] = [];
  private movePPTexts: Phaser.GameObjects.Text[] = [];
  private moveTypeBarGfx: Phaser.GameObjects.Graphics[] = [];
  private moveButtonBgs: Phaser.GameObjects.Graphics[] = [];
  private moveIndex = 0;
  private currentMoves: MoveInfo[] = [];

  // Move info panel (right side of move menu)
  private moveInfoContainer!: Phaser.GameObjects.Container;
  private moveInfoBg!: Phaser.GameObjects.Graphics;
  private moveInfoTypeText!: Phaser.GameObjects.Text;
  private moveInfoCategoryText!: Phaser.GameObjects.Text;
  private moveInfoPowerText!: Phaser.GameObjects.Text;

  // Message area
  private messageContainer!: Phaser.GameObjects.Container;
  private messageBg!: Phaser.GameObjects.Graphics;
  private messageText!: Phaser.GameObjects.Text;

  // State
  private menuVisible: 'action' | 'move' | 'none' = 'none';
  private onActionSelect?: (index: number) => void;
  private onMoveSelect?: (index: number) => void;

  // Input
  private cursors?: Phaser.Types.Input.Keyboard.CursorKeys;
  private enterKey?: Phaser.Input.Keyboard.Key;
  private escKey?: Phaser.Input.Keyboard.Key;

  create(scene: Phaser.Scene): void {
    this.scene = scene;
    this.container = scene.add.container(0, 0).setDepth(900).setScrollFactor(0);

    this.createEnemyPanel();
    this.createPlayerPanel();
    this.createActionMenu();
    this.createMoveMenu();
    this.createMessageArea();

    const kb = scene.input.keyboard;
    if (kb) {
      this.cursors = kb.createCursorKeys();
      this.enterKey = kb.addKey(Phaser.Input.Keyboard.KeyCodes.ENTER);
      this.escKey = kb.addKey(Phaser.Input.Keyboard.KeyCodes.ESC);
    }

    this.hideMenus();
  }

  // ---- Panels ----

  private createEnemyPanel(): void {
    this.enemyPanel = this.scene.add.container(12, 12);

    this.enemyPanelBg = this.scene.add.graphics();
    drawAmberPanel(this.enemyPanelBg, 0, 0, ENEMY_PANEL_W, ENEMY_PANEL_H);
    this.enemyPanel.add(this.enemyPanelBg);

    this.enemyNameText = this.scene.add.text(14, 8, '', textStyle(FONT_SMALL));
    this.enemyPanel.add(this.enemyNameText);

    this.enemyLevelText = this.scene.add.text(ENEMY_PANEL_W - 14, 8, '', textStyle(FONT_TINY, TEXT_COLOR_DIM)).setOrigin(1, 0);
    this.enemyPanel.add(this.enemyLevelText);

    this.enemyTypeDots = this.scene.add.graphics();
    this.enemyPanel.add(this.enemyTypeDots);

    this.enemyHPBar = this.scene.add.graphics();
    this.enemyPanel.add(this.enemyHPBar);

    this.enemyStatusText = this.scene.add.text(14, 44, '', textStyle(FONT_TINY)).setOrigin(0, 0);
    this.enemyStatusText.setVisible(false);
    this.enemyPanel.add(this.enemyStatusText);

    this.container.add(this.enemyPanel);
  }

  private createPlayerPanel(): void {
    this.playerPanel = this.scene.add.container(GAME_WIDTH - PLAYER_PANEL_W - 12, GAME_HEIGHT - PLAYER_PANEL_H - ACTION_MENU_H - 16);

    this.playerPanelBg = this.scene.add.graphics();
    drawAmberPanel(this.playerPanelBg, 0, 0, PLAYER_PANEL_W, PLAYER_PANEL_H);
    this.playerPanel.add(this.playerPanelBg);

    this.playerNameText = this.scene.add.text(14, 8, '', textStyle(FONT_SMALL));
    this.playerPanel.add(this.playerNameText);

    this.playerLevelText = this.scene.add.text(PLAYER_PANEL_W - 14, 8, '', textStyle(FONT_TINY, TEXT_COLOR_DIM)).setOrigin(1, 0);
    this.playerPanel.add(this.playerLevelText);

    this.playerHPBar = this.scene.add.graphics();
    this.playerPanel.add(this.playerHPBar);

    this.playerHPText = this.scene.add.text(PLAYER_PANEL_W - 14, 40, '', textStyle(FONT_TINY, TEXT_COLOR_DIM)).setOrigin(1, 0);
    this.playerPanel.add(this.playerHPText);

    this.playerXPBar = this.scene.add.graphics();
    this.playerPanel.add(this.playerXPBar);

    this.playerStatusText = this.scene.add.text(14, 60, '', textStyle(FONT_TINY)).setOrigin(0, 0);
    this.playerStatusText.setVisible(false);
    this.playerPanel.add(this.playerStatusText);

    this.container.add(this.playerPanel);
  }

  private drawHPBar(
    gfx: Phaser.GameObjects.Graphics,
    x: number,
    y: number,
    w: number,
    current: number,
    max: number
  ): void {
    gfx.clear();
    const ratio = Math.max(0, Math.min(1, current / max));
    let color: number = COLORS.HP_GREEN;
    if (ratio <= 0.25) color = COLORS.HP_RED;
    else if (ratio <= 0.5) color = COLORS.HP_YELLOW;

    // "HP" label area
    const labelW = 28;
    gfx.fillStyle(0x505050, 1);
    gfx.fillRoundedRect(x, y, labelW, HP_BAR_H, 3);

    // HP text
    gfx.fillStyle(0xF8E870, 1);
    // H
    gfx.fillRect(x + 4, y + 1, 1, 6);
    gfx.fillRect(x + 8, y + 1, 1, 6);
    gfx.fillRect(x + 4, y + 3, 5, 1);
    // P
    gfx.fillRect(x + 12, y + 1, 1, 6);
    gfx.fillRect(x + 12, y + 1, 5, 1);
    gfx.fillRect(x + 12, y + 3, 5, 1);
    gfx.fillRect(x + 16, y + 1, 1, 3);

    // Bar background
    gfx.fillStyle(0x303030, 1);
    gfx.fillRoundedRect(x + labelW, y, w - labelW, HP_BAR_H, { tl: 0, tr: 3, bl: 0, br: 3 });

    // Bar fill
    const fillW = (w - labelW) * ratio;
    if (fillW > 0) {
      gfx.fillStyle(color, 1);
      gfx.fillRoundedRect(x + labelW, y + 1, fillW, HP_BAR_H - 2, { tl: 0, tr: 2, bl: 0, br: 2 });

      // Lighter highlight on top half for gradient effect
      gfx.fillStyle(0xFFFFFF, 0.15);
      gfx.fillRect(x + labelW, y + 1, fillW, (HP_BAR_H - 2) / 2);
    }
  }

  private drawXPBar(
    gfx: Phaser.GameObjects.Graphics,
    x: number,
    y: number,
    w: number,
    current: number,
    max: number
  ): void {
    gfx.clear();
    const ratio = max > 0 ? Math.min(1, current / max) : 0;

    // Background
    gfx.fillStyle(0x303030, 1);
    gfx.fillRoundedRect(x, y, w, XP_BAR_H, 2);

    // Fill
    if (ratio > 0) {
      gfx.fillStyle(COLORS.XP_BLUE, 1);
      gfx.fillRoundedRect(x, y, w * ratio, XP_BAR_H, 2);

      // Highlight
      gfx.fillStyle(0xFFFFFF, 0.15);
      gfx.fillRect(x, y, w * ratio, XP_BAR_H / 2);
    }
  }

  // ---- Action Menu ----

  private createActionMenu(): void {
    this.actionMenu = this.scene.add.container(0, 0);
    const menuX = GAME_WIDTH - ACTION_MENU_W - 16;
    const menuY = GAME_HEIGHT - ACTION_MENU_H - 12;

    // Background
    this.actionMenuBg = this.scene.add.graphics();
    drawAmberPanel(this.actionMenuBg, menuX, menuY, ACTION_MENU_W, ACTION_MENU_H);
    this.actionMenu.add(this.actionMenuBg);

    // 2x2 grid of buttons
    const cellW = (ACTION_MENU_W - 30) / 2;
    const cellH = (ACTION_MENU_H - 26) / 2;
    const positions = [
      { x: menuX + 10, y: menuY + 10 },
      { x: menuX + 10 + cellW + 8, y: menuY + 10 },
      { x: menuX + 10, y: menuY + 10 + cellH + 6 },
      { x: menuX + 10 + cellW + 8, y: menuY + 10 + cellH + 6 },
    ];

    this.actionTexts = [];
    this.actionButtonBgs = [];

    for (let i = 0; i < 4; i++) {
      const bg = this.scene.add.graphics();
      this.actionMenu.add(bg);
      this.actionButtonBgs.push(bg);

      const t = this.scene.add.text(
        positions[i].x + cellW / 2,
        positions[i].y + cellH / 2,
        this.actionLabels[i],
        textStyle(FONT_TINY)
      ).setOrigin(0.5, 0.5);
      this.actionMenu.add(t);
      this.actionTexts.push(t);
    }

    this.container.add(this.actionMenu);
    this.actionMenu.setVisible(false);
  }

  private drawActionButtons(): void {
    const menuX = GAME_WIDTH - ACTION_MENU_W - 16;
    const menuY = GAME_HEIGHT - ACTION_MENU_H - 12;
    const cellW = (ACTION_MENU_W - 30) / 2;
    const cellH = (ACTION_MENU_H - 26) / 2;

    const positions = [
      { x: menuX + 10, y: menuY + 10 },
      { x: menuX + 10 + cellW + 8, y: menuY + 10 },
      { x: menuX + 10, y: menuY + 10 + cellH + 6 },
      { x: menuX + 10 + cellW + 8, y: menuY + 10 + cellH + 6 },
    ];

    for (let i = 0; i < 4; i++) {
      const bg = this.actionButtonBgs[i];
      bg.clear();
      const isSelected = i === this.actionIndex;

      // Button background
      bg.fillStyle(isSelected ? COLORS.UI_BORDER : 0x2a2a48, isSelected ? 0.35 : 0.8);
      bg.fillRoundedRect(positions[i].x, positions[i].y, cellW, cellH, 4);

      // Button border
      bg.lineStyle(1, isSelected ? COLORS.UI_BORDER_LIGHT : COLORS.UI_BORDER_DARK, 1);
      bg.strokeRoundedRect(positions[i].x, positions[i].y, cellW, cellH, 4);

      // Update text
      const prefix = isSelected ? '\u25B6 ' : '';
      this.actionTexts[i].setText(prefix + this.actionLabels[i]);
      this.actionTexts[i].setColor(isSelected ? '#E8C868' : TEXT_COLOR);
    }
  }

  // ---- Move Menu ----

  private createMoveMenu(): void {
    this.moveMenu = this.scene.add.container(0, 0);
    const menuX = 16;
    const menuY = GAME_HEIGHT - MOVE_MENU_H - 12;

    // Background
    this.moveMenuBg = this.scene.add.graphics();
    drawAmberPanel(this.moveMenuBg, menuX, menuY, MOVE_MENU_W, MOVE_MENU_H);
    this.moveMenu.add(this.moveMenuBg);

    this.moveTexts = [];
    this.movePPTexts = [];
    this.moveTypeBarGfx = [];
    this.moveButtonBgs = [];

    const cellW = (MOVE_MENU_W - 30) / 2;
    const cellH = (MOVE_MENU_H - 26) / 2;

    for (let i = 0; i < 4; i++) {
      const col = i % 2;
      const row = Math.floor(i / 2);
      const cx = menuX + 10 + col * (cellW + 8);
      const cy = menuY + 10 + row * (cellH + 6);

      // Button bg
      const bg = this.scene.add.graphics();
      this.moveMenu.add(bg);
      this.moveButtonBgs.push(bg);

      // Type color bar
      const typeBar = this.scene.add.graphics();
      this.moveMenu.add(typeBar);
      this.moveTypeBarGfx.push(typeBar);

      // Move name
      const nameT = this.scene.add.text(cx + 8, cy + 3, '', textStyle(FONT_TINY));
      this.moveMenu.add(nameT);
      this.moveTexts.push(nameT);

      // PP text
      const ppT = this.scene.add.text(cx + cellW - 6, cy + cellH - 14, '', textStyle(FONT_TINY, TEXT_COLOR_DIM)).setOrigin(1, 0);
      this.moveMenu.add(ppT);
      this.movePPTexts.push(ppT);
    }

    // Move info panel (right of move menu)
    this.moveInfoContainer = this.scene.add.container(0, 0);
    this.moveInfoBg = this.scene.add.graphics();
    this.moveInfoContainer.add(this.moveInfoBg);

    const infoX = menuX + MOVE_MENU_W + 8;
    const infoY = menuY;
    const infoW = GAME_WIDTH - infoX - 16;
    const infoH = MOVE_MENU_H;
    drawAmberPanel(this.moveInfoBg, infoX, infoY, infoW, infoH, 6);

    this.moveInfoTypeText = this.scene.add.text(infoX + 14, infoY + 12, '', textStyle(FONT_TINY));
    this.moveInfoContainer.add(this.moveInfoTypeText);

    this.moveInfoCategoryText = this.scene.add.text(infoX + 14, infoY + 32, '', textStyle(FONT_TINY, TEXT_COLOR_DIM));
    this.moveInfoContainer.add(this.moveInfoCategoryText);

    // Power label (third line)
    this.moveInfoPowerText = this.scene.add.text(infoX + 14, infoY + 52, '', textStyle(FONT_TINY, '#F0E8D0'));
    this.moveInfoContainer.add(this.moveInfoPowerText);

    this.moveMenu.add(this.moveInfoContainer);

    this.container.add(this.moveMenu);
    this.moveMenu.setVisible(false);
  }

  private drawMoveButtons(): void {
    const menuX = 16;
    const menuY = GAME_HEIGHT - MOVE_MENU_H - 12;
    const cellW = (MOVE_MENU_W - 30) / 2;
    const cellH = (MOVE_MENU_H - 26) / 2;

    for (let i = 0; i < 4; i++) {
      const col = i % 2;
      const row = Math.floor(i / 2);
      const cx = menuX + 10 + col * (cellW + 8);
      const cy = menuY + 10 + row * (cellH + 6);
      const bg = this.moveButtonBgs[i];
      const typeBar = this.moveTypeBarGfx[i];
      bg.clear();
      typeBar.clear();

      if (i >= this.currentMoves.length) {
        this.moveTexts[i].setVisible(false);
        this.movePPTexts[i].setVisible(false);
        continue;
      }

      const isSelected = i === this.moveIndex;
      const move = this.currentMoves[i];

      // Button background
      bg.fillStyle(isSelected ? COLORS.UI_BORDER : 0x2a2a48, isSelected ? 0.35 : 0.8);
      bg.fillRoundedRect(cx, cy, cellW, cellH, 4);

      bg.lineStyle(1, isSelected ? COLORS.UI_BORDER_LIGHT : COLORS.UI_BORDER_DARK, 1);
      bg.strokeRoundedRect(cx, cy, cellW, cellH, 4);

      // Type color bar at left edge
      typeBar.fillStyle(DINO_TYPE_COLORS[move.type], 1);
      typeBar.fillRoundedRect(cx, cy, 4, cellH, { tl: 4, bl: 4, tr: 0, br: 0 });

      // Move name
      const prefix = isSelected ? '\u25B6 ' : '';
      this.moveTexts[i].setText(prefix + move.name);
      this.moveTexts[i].setColor(isSelected ? '#E8C868' : TEXT_COLOR);
      this.moveTexts[i].setVisible(true);

      // PP + Power
      const powerStr = move.power > 0 ? `PWR ${move.power}  ` : '';
      this.movePPTexts[i].setText(`${powerStr}${move.pp}/${move.maxPp}`);
      this.movePPTexts[i].setVisible(true);
    }

    // Update move info panel
    if (this.currentMoves.length > 0 && this.moveIndex < this.currentMoves.length) {
      const move = this.currentMoves[this.moveIndex];
      const typeColorNum = DINO_TYPE_COLORS[move.type] ?? 0xA8A878;
      const typeColor = '#' + typeColorNum.toString(16).padStart(6, '0');
      this.moveInfoTypeText.setText(DINO_TYPE_NAMES[move.type] ?? 'Normal');
      this.moveInfoTypeText.setColor(typeColor);
      const categoryLabel = move.category
        ? move.category === 'physical' ? 'PHYSIQUE' : move.category === 'special' ? 'SPECIAL' : 'STATUT'
        : '---';
      this.moveInfoCategoryText.setText(categoryLabel);
      this.moveInfoPowerText.setText(move.power > 0 ? `PUISSANCE: ${move.power}` : '---');
    }
  }

  // ---- Message Area ----

  private createMessageArea(): void {
    this.messageContainer = this.scene.add.container(0, 0);

    this.messageBg = this.scene.add.graphics();
    this.messageBg.fillStyle(COLORS.UI_BG, 0.9);
    this.messageBg.fillRoundedRect(GAME_WIDTH / 2 - 240, 6, 480, 44, 8);
    this.messageContainer.add(this.messageBg);

    this.messageText = this.scene.add.text(GAME_WIDTH / 2, 14, '', {
      ...textStyle(FONT_SMALL),
      wordWrap: { width: 450 },
      maxLines: 2,
    }).setOrigin(0.5, 0);
    this.messageContainer.add(this.messageText);

    this.container.add(this.messageContainer);
    this.messageContainer.setVisible(false);
  }

  // ---- Public API ----

  setEnemyDino(name: string, level: number, types: DinoType[], currentHp: number, maxHp: number): void {
    const truncName = name.length > 12 ? name.substring(0, 11) + '.' : name;
    this.enemyNameText.setText(truncName);
    this.enemyLevelText.setText('Lv ' + level);
    this.enemyHPCurrent = currentHp;
    this.enemyHPMax = maxHp;
    this.drawHPBar(this.enemyHPBar, 14, 28, HP_BAR_W + 60, currentHp, maxHp);

    // Type dots
    this.enemyTypeDots.clear();
    types.forEach((t, i) => {
      this.enemyTypeDots.fillStyle(DINO_TYPE_COLORS[t], 1);
      this.enemyTypeDots.fillCircle(ENEMY_PANEL_W - 20 - i * 14, 14, 4);
    });
  }

  setPlayerDino(name: string, level: number, types: DinoType[], currentHp: number, maxHp: number): void {
    const truncName = name.length > 12 ? name.substring(0, 11) + '.' : name;
    this.playerNameText.setText(truncName);
    this.playerLevelText.setText('Lv ' + level);
    this.playerHPCurrent = currentHp;
    this.playerHPMax = maxHp;
    this.drawHPBar(this.playerHPBar, 14, 28, HP_BAR_W + 80, currentHp, maxHp);
    this.playerHPText.setText(currentHp + ' / ' + maxHp);
  }

  updatePlayerHP(current: number, max: number, animate = false): void {
    if (animate) {
      this.animateHPDrain('player', this.playerHPCurrent, current, 600);
    } else {
      this.playerHPCurrent = current;
      this.playerHPMax = max;
      this.drawHPBar(this.playerHPBar, 14, 28, HP_BAR_W + 80, current, max);
      this.playerHPText.setText(current + ' / ' + max);
    }
  }

  updateEnemyHP(current: number, max: number, animate = false): void {
    if (animate) {
      this.animateHPDrain('enemy', this.enemyHPCurrent, current, 600);
    } else {
      this.enemyHPCurrent = current;
      this.enemyHPMax = max;
      this.drawHPBar(this.enemyHPBar, 14, 28, HP_BAR_W + 60, current, max);
    }
  }

  private applyStatusLabel(textObj: Phaser.GameObjects.Text, status?: StatusEffect): void {
    if (!status || status === StatusEffect.None) {
      textObj.setVisible(false);
      return;
    }
    const STATUS_LABELS: Record<string, { label: string; color: string }> = {
      [StatusEffect.Poison]: { label: 'PSN', color: '#A040A0' },
      [StatusEffect.Burn]: { label: 'BRN', color: '#F08030' },
      [StatusEffect.Paralysis]: { label: 'PAR', color: '#F8D030' },
      [StatusEffect.Sleep]: { label: 'SLP', color: '#A0A0A0' },
      [StatusEffect.Freeze]: { label: 'GEL', color: '#60D0D0' },
    };
    const info = STATUS_LABELS[status];
    if (info) {
      textObj.setText(info.label);
      textObj.setColor(info.color);
      textObj.setVisible(true);
    } else {
      textObj.setVisible(false);
    }
  }

  setEnemyStatus(status?: StatusEffect): void {
    this.applyStatusLabel(this.enemyStatusText, status);
  }

  setPlayerStatus(status?: StatusEffect): void {
    this.applyStatusLabel(this.playerStatusText, status);
  }

  updateXP(current: number, max: number, animate = false): void {
    if (animate) {
      const fromXP = this.playerXPCurrent;
      this.playerXPMax = max;
      const tween = { val: fromXP };
      this.scene.tweens.add({
        targets: tween,
        val: current,
        duration: 800,
        ease: 'Linear',
        onUpdate: () => {
          this.drawXPBar(this.playerXPBar, 14, 56, PLAYER_PANEL_W - 28, Math.round(tween.val), max);
        },
        onComplete: () => {
          this.playerXPCurrent = current;
          this.drawXPBar(this.playerXPBar, 14, 56, PLAYER_PANEL_W - 28, current, max);
        },
      });
    } else {
      this.playerXPCurrent = current;
      this.playerXPMax = max;
      this.drawXPBar(this.playerXPBar, 14, 56, PLAYER_PANEL_W - 28, current, max);
    }
  }

  animateHPDrain(target: 'player' | 'enemy', from: number, to: number, duration: number): Promise<void> {
    return new Promise((resolve) => {
      const max = target === 'player' ? this.playerHPMax : this.enemyHPMax;
      const bar = target === 'player' ? this.playerHPBar : this.enemyHPBar;
      const barW = target === 'player' ? HP_BAR_W + 80 : HP_BAR_W + 60;

      const tween = { val: from };
      this.scene.tweens.add({
        targets: tween,
        val: to,
        duration,
        ease: 'Linear',
        onUpdate: () => {
          const hp = Math.round(tween.val);
          this.drawHPBar(bar, 14, 28, barW, hp, max);
          if (target === 'player') {
            this.playerHPText.setText(hp + ' / ' + max);
          }
        },
        onComplete: () => {
          if (target === 'player') {
            this.playerHPCurrent = to;
          } else {
            this.enemyHPCurrent = to;
          }
          resolve();
        },
      });
    });
  }

  showActionMenu(onSelect?: (index: number) => void): void {
    this.menuVisible = 'action';
    this.actionIndex = 0;
    this.onActionSelect = onSelect;
    this.actionMenu.setVisible(true);
    this.moveMenu.setVisible(false);
    this.drawActionButtons();
  }

  showMoveMenu(moves: MoveInfo[], onSelect?: (index: number) => void): void {
    this.menuVisible = 'move';
    this.moveIndex = 0;
    this.currentMoves = moves;
    this.onMoveSelect = onSelect;
    this.moveMenu.setVisible(true);
    this.actionMenu.setVisible(false);
    this.drawMoveButtons();
  }

  hideMenus(): void {
    this.menuVisible = 'none';
    this.actionMenu.setVisible(false);
    this.moveMenu.setVisible(false);
  }

  showMessage(text: string): void {
    this.messageText.setText(text);
    // Resize background to fit text
    const textW = Math.max(480, this.messageText.width + 48);
    const textH = Math.max(44, this.messageText.height + 16);
    this.messageBg.clear();
    this.messageBg.fillStyle(COLORS.UI_BG, 0.9);
    this.messageBg.fillRoundedRect(GAME_WIDTH / 2 - textW / 2, 6, textW, textH, 8);
    this.messageContainer.setVisible(true);
  }

  hideMessage(): void {
    this.messageContainer.setVisible(false);
  }

  /** Call in scene update */
  update(): void {
    if (!this.cursors) return;

    if (this.menuVisible === 'action') {
      this.handleActionInput();
    } else if (this.menuVisible === 'move') {
      this.handleMoveInput();
    }
  }

  private handleActionInput(): void {
    if (!this.cursors) return;
    const left = Phaser.Input.Keyboard.JustDown(this.cursors.left);
    const right = Phaser.Input.Keyboard.JustDown(this.cursors.right);
    const up = Phaser.Input.Keyboard.JustDown(this.cursors.up);
    const down = Phaser.Input.Keyboard.JustDown(this.cursors.down);
    const enter = this.enterKey && Phaser.Input.Keyboard.JustDown(this.enterKey);

    let changed = false;
    if (left && this.actionIndex % 2 === 1) { this.actionIndex--; changed = true; }
    if (right && this.actionIndex % 2 === 0) { this.actionIndex++; changed = true; }
    if (up && this.actionIndex >= 2) { this.actionIndex -= 2; changed = true; }
    if (down && this.actionIndex < 2) { this.actionIndex += 2; changed = true; }

    if (changed) this.drawActionButtons();

    if (enter) {
      this.onActionSelect?.(this.actionIndex);
    }
  }

  private handleMoveInput(): void {
    if (!this.cursors) return;
    const left = Phaser.Input.Keyboard.JustDown(this.cursors.left);
    const right = Phaser.Input.Keyboard.JustDown(this.cursors.right);
    const up = Phaser.Input.Keyboard.JustDown(this.cursors.up);
    const down = Phaser.Input.Keyboard.JustDown(this.cursors.down);
    const enter = this.enterKey && Phaser.Input.Keyboard.JustDown(this.enterKey);
    const esc = this.escKey && Phaser.Input.Keyboard.JustDown(this.escKey);

    const count = this.currentMoves.length;
    let changed = false;
    if (left && this.moveIndex % 2 === 1) { this.moveIndex--; changed = true; }
    if (right && this.moveIndex % 2 === 0 && this.moveIndex + 1 < count) { this.moveIndex++; changed = true; }
    if (up && this.moveIndex >= 2) { this.moveIndex -= 2; changed = true; }
    if (down && this.moveIndex + 2 < count) { this.moveIndex += 2; changed = true; }

    if (changed) this.drawMoveButtons();

    if (enter) {
      this.onMoveSelect?.(this.moveIndex);
    }
    if (esc) {
      this.showActionMenu(this.onActionSelect);
    }
  }

  getContainer(): Phaser.GameObjects.Container {
    return this.container;
  }

  destroy(): void {
    this.container.destroy();
  }
}
