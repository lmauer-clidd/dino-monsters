// ============================================================
// Jurassic Trainers -- Shop Scene (Boutique)
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
  FONT_BODY,
  Direction,
  PLAYER_MOVE_DURATION,
} from '../utils/constants';
import { DialogueBox } from '../ui/DialogueBox';
import { GameState } from '../systems/GameState';
import { getItem, hasItemData, IItemData } from '../systems/InventorySystem';

// ===================== Tile Palette =====================

enum ShopTile {
  Floor = 0,
  Counter = 1,
  Wall = 2,
  Door = 3,
  Shelf = 4,
  Mat = 5,
}

const SHOP_TILE_COLORS: Record<number, number> = {
  [ShopTile.Floor]: 0xE0D0B8,
  [ShopTile.Counter]: 0x886830,
  [ShopTile.Wall]: 0xC8B898,
  [ShopTile.Door]: 0x604020,
  [ShopTile.Shelf]: 0xA08050,
  [ShopTile.Mat]: 0x4888C8,
};

const COLLISION_TILES = new Set([
  ShopTile.Counter, ShopTile.Wall, ShopTile.Shelf,
]);

// ===================== Map Layout (10x8) =====================

const MAP_W = 10;
const MAP_H = 8;

// prettier-ignore
const SHOP_MAP: number[][] = [
  [2, 2, 2, 2, 2, 2, 2, 2, 2, 2],
  [2, 4, 4, 0, 1, 1, 1, 4, 4, 2],
  [2, 4, 4, 0, 0, 0, 0, 4, 4, 2],
  [2, 0, 0, 0, 0, 0, 0, 0, 0, 2],
  [2, 0, 0, 0, 0, 0, 0, 0, 0, 2],
  [2, 0, 0, 0, 0, 0, 0, 0, 0, 2],
  [2, 0, 0, 0, 5, 5, 0, 0, 0, 2],
  [2, 2, 2, 2, 3, 3, 2, 2, 2, 2],
];

// ===================== Shop Inventory per Town =====================

interface ShopItem {
  itemId: number;
  name: string;
  price: number;
}

const SHOP_INVENTORIES: Record<number, ShopItem[]> = {
  1: [
    { itemId: 1, name: 'Potion', price: 200 },
    { itemId: 10, name: 'Jurassic Ball', price: 200 },
    { itemId: 20, name: 'Antidote', price: 100 },
    { itemId: 30, name: 'Repousse', price: 350 },
  ],
  2: [
    { itemId: 1, name: 'Potion', price: 200 },
    { itemId: 2, name: 'Super Potion', price: 700 },
    { itemId: 10, name: 'Jurassic Ball', price: 200 },
    { itemId: 11, name: 'Super Ball', price: 600 },
    { itemId: 20, name: 'Antidote', price: 100 },
    { itemId: 30, name: 'Repousse', price: 350 },
  ],
  3: [
    { itemId: 1, name: 'Potion', price: 200 },
    { itemId: 2, name: 'Super Potion', price: 700 },
    { itemId: 3, name: 'Hyper Potion', price: 1200 },
    { itemId: 10, name: 'Jurassic Ball', price: 200 },
    { itemId: 11, name: 'Super Ball', price: 600 },
    { itemId: 12, name: 'Ultra Ball', price: 1200 },
    { itemId: 20, name: 'Antidote', price: 100 },
    { itemId: 21, name: 'Total Soin', price: 600 },
    { itemId: 22, name: 'Rappel', price: 1500 },
    { itemId: 30, name: 'Repousse', price: 350 },
  ],
};

// ===================== NPC =====================

interface ShopNPC {
  x: number;
  y: number;
  name: string;
  color: number;
  hairColor: number;
}

const SHOPKEEPER: ShopNPC = {
  x: 5,
  y: 1,
  name: 'Vendeur',
  color: 0x48A868,
  hairColor: 0x604020,
};

// ===================== Scene =====================

export class ShopScene extends Phaser.Scene {
  private mapGraphics!: Phaser.GameObjects.Graphics;
  private npcGraphics!: Phaser.GameObjects.Graphics;
  private playerGraphics!: Phaser.GameObjects.Graphics;
  private shopUIGraphics!: Phaser.GameObjects.Graphics;
  private shopUIContainer!: Phaser.GameObjects.Container;
  private dialogueBox!: DialogueBox;

  // Player
  private playerX = 4;
  private playerY = 6;
  private playerFacing = Direction.Up;
  private isMoving = false;
  private canInput = true;

  // Scene data
  private returnMapId = '';
  private returnX = 0;
  private returnY = 0;
  private townId = 1;

  // Offsets to center map
  private offsetX = 0;
  private offsetY = 0;

  // Shop state
  private shopMode: 'none' | 'buy' | 'sell' | 'buy_list' | 'sell_list' | 'quantity' = 'none';
  private shopItems: ShopItem[] = [];
  private sellItems: { itemId: number; name: string; price: number; quantity: number }[] = [];
  private selectedIndex = 0;
  private scrollOffset = 0;
  private maxVisible = 8;
  private selectedQuantity = 1;
  private pendingItem: ShopItem | null = null;
  private pendingSellItem: { itemId: number; name: string; price: number; quantity: number } | null = null;

  // Keys
  private cursors!: Phaser.Types.Input.Keyboard.CursorKeys;
  private enterKey!: Phaser.Input.Keyboard.Key;
  private escKey!: Phaser.Input.Keyboard.Key;

  // UI texts
  private moneyText!: Phaser.GameObjects.Text;
  private shopTexts: Phaser.GameObjects.Text[] = [];

  constructor() {
    super({ key: SCENE_KEYS.SHOP });
  }

  init(data: { returnMapId?: string; returnX?: number; returnY?: number; townId?: number }): void {
    this.returnMapId = data.returnMapId ?? 'BOURG_NID';
    this.returnX = data.returnX ?? 7;
    this.returnY = data.returnY ?? 5;
    this.townId = data.townId ?? 1;
    this.playerX = 4;
    this.playerY = 6;
    this.playerFacing = Direction.Up;
    this.isMoving = false;
    this.canInput = true;
    this.shopMode = 'none';
    this.selectedIndex = 0;
    this.scrollOffset = 0;
    this.selectedQuantity = 1;
    this.pendingItem = null;
    this.pendingSellItem = null;
  }

  create(): void {
    const mapPixelW = MAP_W * TILE_SIZE;
    const mapPixelH = MAP_H * TILE_SIZE;
    this.offsetX = Math.floor((GAME_WIDTH - mapPixelW) / 2);
    this.offsetY = Math.floor((GAME_HEIGHT - mapPixelH) / 2);

    this.shopItems = SHOP_INVENTORIES[this.townId] ?? SHOP_INVENTORIES[1];

    // Draw map
    this.mapGraphics = this.add.graphics();
    this.drawMap();

    // NPC
    this.npcGraphics = this.add.graphics();
    this.drawNPC();

    // Player
    this.playerGraphics = this.add.graphics();
    this.drawPlayer();

    // Shop UI container (hidden until shopping)
    this.shopUIContainer = this.add.container(0, 0).setDepth(800).setVisible(false);
    this.shopUIGraphics = this.add.graphics();
    this.shopUIContainer.add(this.shopUIGraphics);

    // Money display
    this.moneyText = this.add.text(GAME_WIDTH - 16, 16, '', {
      fontFamily: FONT_FAMILY,
      fontSize: FONT_TINY,
      color: '#F0E8D0',
      shadow: { offsetX: 1, offsetY: 1, color: '#181018', blur: 0, fill: true },
    }).setOrigin(1, 0).setDepth(900).setScrollFactor(0);
    this.updateMoneyDisplay();

    // Dialogue box
    this.dialogueBox = new DialogueBox(this);

    // Input
    if (this.input.keyboard) {
      this.cursors = this.input.keyboard.createCursorKeys();
      this.enterKey = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.ENTER);
      this.escKey = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.ESC);
    }

    this.cameras.main.fadeIn(300, 0x18, 0x10, 0x18);
  }

  update(): void {
    this.updateMoneyDisplay();

    // Dialogue takes priority
    if (this.dialogueBox.isActive()) {
      this.dialogueBox.update();
      return;
    }

    // Shop UI modes
    if (this.shopMode === 'buy_list') {
      this.handleBuyListInput();
      return;
    }
    if (this.shopMode === 'sell_list') {
      this.handleSellListInput();
      return;
    }
    if (this.shopMode === 'quantity') {
      this.handleQuantityInput();
      return;
    }

    if (!this.canInput || this.isMoving) return;

    // Check for exit
    if (this.playerX >= 4 && this.playerX <= 5 && this.playerY === 7) {
      this.exitShop();
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

    if (Phaser.Input.Keyboard.JustDown(this.enterKey)) {
      this.tryInteract();
    }

    if (Phaser.Input.Keyboard.JustDown(this.escKey)) {
      if (this.playerY >= 6) {
        this.exitShop();
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

    if (nx < 0 || nx >= MAP_W || ny < 0 || ny >= MAP_H) return;
    const tile = SHOP_MAP[ny][nx];
    if (COLLISION_TILES.has(tile)) return;
    if (nx === SHOPKEEPER.x && ny === SHOPKEEPER.y) return;

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

    if (targetX >= 4 && targetX <= 6 && targetY <= 2) {
      this.startShopping();
    }
  }

  // ===================== Shopping =====================

  private startShopping(): void {
    this.canInput = false;
    this.dialogueBox.showChoices(
      'Bienvenue ! Que puis-je faire pour vous ?',
      ['ACHETER', 'VENDRE', 'QUITTER'],
      (index: number) => {
        switch (index) {
          case 0: this.openBuyMenu(); break;
          case 1: this.openSellMenu(); break;
          case 2:
            this.dialogueBox.showText('Merci et a bientot !', () => {
              this.canInput = true;
            }, SHOPKEEPER.name);
            break;
        }
      },
    );
  }

  // ===================== Buy Menu =====================

  private openBuyMenu(): void {
    this.shopMode = 'buy_list';
    this.selectedIndex = 0;
    this.scrollOffset = 0;
    this.renderBuyList();
  }

  private renderBuyList(): void {
    this.clearShopTexts();
    this.shopUIContainer.setVisible(true);
    this.shopUIGraphics.clear();

    const panelX = 32;
    const panelY = 16;
    const panelW = GAME_WIDTH - 64;
    const panelH = GAME_HEIGHT - 140;

    this.drawPanel(this.shopUIGraphics, panelX, panelY, panelW, panelH);

    // Title
    const title = this.add.text(panelX + 16, panelY + 12, 'ACHETER', {
      fontFamily: FONT_FAMILY,
      fontSize: FONT_SMALL,
      color: '#E8C868',
      shadow: { offsetX: 1, offsetY: 1, color: '#181018', blur: 0, fill: true },
    }).setDepth(850);
    this.shopUIContainer.add(title);
    this.shopTexts.push(title);

    // Items
    const items = this.shopItems;
    const startY = panelY + 36;
    const itemH = 22;

    for (let i = 0; i < Math.min(this.maxVisible, items.length); i++) {
      const idx = i + this.scrollOffset;
      if (idx >= items.length) break;
      const item = items[idx];
      const isSelected = idx === this.selectedIndex;
      const prefix = isSelected ? '\u25B6 ' : '  ';

      const t = this.add.text(panelX + 16, startY + i * itemH, `${prefix}${item.name}`, {
        fontFamily: FONT_FAMILY,
        fontSize: FONT_TINY,
        color: isSelected ? '#E8C868' : '#F0E8D0',
        shadow: { offsetX: 1, offsetY: 1, color: '#181018', blur: 0, fill: true },
      }).setDepth(850);
      this.shopUIContainer.add(t);
      this.shopTexts.push(t);

      const p = this.add.text(panelX + panelW - 32, startY + i * itemH, `${item.price}F`, {
        fontFamily: FONT_FAMILY,
        fontSize: FONT_TINY,
        color: isSelected ? '#E8C868' : '#F0E8D0',
        shadow: { offsetX: 1, offsetY: 1, color: '#181018', blur: 0, fill: true },
      }).setOrigin(1, 0).setDepth(850);
      this.shopUIContainer.add(p);
      this.shopTexts.push(p);

      // Item description
      if (isSelected && hasItemData(item.itemId)) {
        const itemData = getItem(item.itemId);
        if (itemData.description) {
          const desc = this.add.text(panelX + 16, panelH - 8, itemData.description, {
            fontFamily: FONT_FAMILY,
            fontSize: FONT_TINY,
            color: '#B8B0A0',
            shadow: { offsetX: 1, offsetY: 1, color: '#181018', blur: 0, fill: true },
            wordWrap: { width: panelW - 48 },
          }).setDepth(850);
          this.shopUIContainer.add(desc);
          this.shopTexts.push(desc);
        }
      }
    }

    // Cancel option at end
    const cancelIdx = items.length;
    const cancelVisIdx = cancelIdx - this.scrollOffset;
    if (cancelVisIdx >= 0 && cancelVisIdx < this.maxVisible) {
      const isSelected = this.selectedIndex === cancelIdx;
      const prefix = isSelected ? '\u25B6 ' : '  ';
      const ct = this.add.text(panelX + 16, startY + cancelVisIdx * itemH, `${prefix}ANNULER`, {
        fontFamily: FONT_FAMILY,
        fontSize: FONT_TINY,
        color: isSelected ? '#E8C868' : '#F0E8D0',
        shadow: { offsetX: 1, offsetY: 1, color: '#181018', blur: 0, fill: true },
      }).setDepth(850);
      this.shopUIContainer.add(ct);
      this.shopTexts.push(ct);
    }
  }

  private handleBuyListInput(): void {
    const totalItems = this.shopItems.length + 1; // +1 for cancel

    if (Phaser.Input.Keyboard.JustDown(this.cursors.up)) {
      this.selectedIndex = (this.selectedIndex - 1 + totalItems) % totalItems;
      this.adjustScroll(totalItems);
      this.renderBuyList();
    }
    if (Phaser.Input.Keyboard.JustDown(this.cursors.down)) {
      this.selectedIndex = (this.selectedIndex + 1) % totalItems;
      this.adjustScroll(totalItems);
      this.renderBuyList();
    }
    if (Phaser.Input.Keyboard.JustDown(this.enterKey)) {
      if (this.selectedIndex >= this.shopItems.length) {
        // Cancel
        this.closeShopUI();
        this.startShopping();
      } else {
        this.pendingItem = this.shopItems[this.selectedIndex];
        this.selectedQuantity = 1;
        this.shopMode = 'quantity';
        this.renderQuantityUI();
      }
    }
    if (Phaser.Input.Keyboard.JustDown(this.escKey)) {
      this.closeShopUI();
      this.startShopping();
    }
  }

  // ===================== Sell Menu =====================

  private openSellMenu(): void {
    const gs = GameState.getInstance();
    const inv = gs.getInventorySystem();
    const allItems = inv.getAllItems();

    if (allItems.length === 0) {
      this.dialogueBox.showText("Vous n'avez rien a vendre.", () => {
        this.startShopping();
      }, SHOPKEEPER.name);
      return;
    }

    this.sellItems = allItems.map(entry => ({
      itemId: entry.item.id,
      name: entry.item.name,
      price: Math.floor(entry.item.price / 2),
      quantity: entry.quantity,
    }));

    this.shopMode = 'sell_list';
    this.selectedIndex = 0;
    this.scrollOffset = 0;
    this.renderSellList();
  }

  private renderSellList(): void {
    this.clearShopTexts();
    this.shopUIContainer.setVisible(true);
    this.shopUIGraphics.clear();

    const panelX = 32;
    const panelY = 16;
    const panelW = GAME_WIDTH - 64;
    const panelH = GAME_HEIGHT - 140;

    this.drawPanel(this.shopUIGraphics, panelX, panelY, panelW, panelH);

    const title = this.add.text(panelX + 16, panelY + 12, 'VENDRE', {
      fontFamily: FONT_FAMILY,
      fontSize: FONT_SMALL,
      color: '#E8C868',
      shadow: { offsetX: 1, offsetY: 1, color: '#181018', blur: 0, fill: true },
    }).setDepth(850);
    this.shopUIContainer.add(title);
    this.shopTexts.push(title);

    const startY = panelY + 36;
    const itemH = 22;

    for (let i = 0; i < Math.min(this.maxVisible, this.sellItems.length); i++) {
      const idx = i + this.scrollOffset;
      if (idx >= this.sellItems.length) break;
      const item = this.sellItems[idx];
      const isSelected = idx === this.selectedIndex;
      const prefix = isSelected ? '\u25B6 ' : '  ';

      const t = this.add.text(panelX + 16, startY + i * itemH,
        `${prefix}${item.name} x${item.quantity}`, {
        fontFamily: FONT_FAMILY,
        fontSize: FONT_TINY,
        color: isSelected ? '#E8C868' : '#F0E8D0',
        shadow: { offsetX: 1, offsetY: 1, color: '#181018', blur: 0, fill: true },
      }).setDepth(850);
      this.shopUIContainer.add(t);
      this.shopTexts.push(t);

      const p = this.add.text(panelX + panelW - 32, startY + i * itemH, `${item.price}F`, {
        fontFamily: FONT_FAMILY,
        fontSize: FONT_TINY,
        color: isSelected ? '#E8C868' : '#F0E8D0',
        shadow: { offsetX: 1, offsetY: 1, color: '#181018', blur: 0, fill: true },
      }).setOrigin(1, 0).setDepth(850);
      this.shopUIContainer.add(p);
      this.shopTexts.push(p);
    }

    // Cancel
    const cancelIdx = this.sellItems.length;
    const cancelVisIdx = cancelIdx - this.scrollOffset;
    if (cancelVisIdx >= 0 && cancelVisIdx < this.maxVisible) {
      const isSelected = this.selectedIndex === cancelIdx;
      const prefix = isSelected ? '\u25B6 ' : '  ';
      const ct = this.add.text(panelX + 16, startY + cancelVisIdx * itemH, `${prefix}ANNULER`, {
        fontFamily: FONT_FAMILY,
        fontSize: FONT_TINY,
        color: isSelected ? '#E8C868' : '#F0E8D0',
        shadow: { offsetX: 1, offsetY: 1, color: '#181018', blur: 0, fill: true },
      }).setDepth(850);
      this.shopUIContainer.add(ct);
      this.shopTexts.push(ct);
    }
  }

  private handleSellListInput(): void {
    const totalItems = this.sellItems.length + 1;

    if (Phaser.Input.Keyboard.JustDown(this.cursors.up)) {
      this.selectedIndex = (this.selectedIndex - 1 + totalItems) % totalItems;
      this.adjustScroll(totalItems);
      this.renderSellList();
    }
    if (Phaser.Input.Keyboard.JustDown(this.cursors.down)) {
      this.selectedIndex = (this.selectedIndex + 1) % totalItems;
      this.adjustScroll(totalItems);
      this.renderSellList();
    }
    if (Phaser.Input.Keyboard.JustDown(this.enterKey)) {
      if (this.selectedIndex >= this.sellItems.length) {
        this.closeShopUI();
        this.startShopping();
      } else {
        const item = this.sellItems[this.selectedIndex];
        this.pendingSellItem = item;
        this.selectedQuantity = 1;
        this.shopMode = 'quantity';
        this.renderQuantityUI();
      }
    }
    if (Phaser.Input.Keyboard.JustDown(this.escKey)) {
      this.closeShopUI();
      this.startShopping();
    }
  }

  // ===================== Quantity Selection =====================

  private renderQuantityUI(): void {
    this.clearShopTexts();
    this.shopUIGraphics.clear();

    const isBuying = this.pendingItem !== null;
    const itemName = isBuying ? this.pendingItem!.name : this.pendingSellItem!.name;
    const unitPrice = isBuying ? this.pendingItem!.price : this.pendingSellItem!.price;
    const maxQty = isBuying ? 99 : this.pendingSellItem!.quantity;
    const total = this.selectedQuantity * unitPrice;

    const panelX = GAME_WIDTH / 2 - 120;
    const panelY = GAME_HEIGHT / 2 - 60;
    const panelW = 240;
    const panelH = 120;

    this.drawPanel(this.shopUIGraphics, panelX, panelY, panelW, panelH);

    const titleStr = isBuying ? `Acheter: ${itemName}` : `Vendre: ${itemName}`;
    const t1 = this.add.text(panelX + 16, panelY + 16, titleStr, {
      fontFamily: FONT_FAMILY,
      fontSize: FONT_TINY,
      color: '#E8C868',
      shadow: { offsetX: 1, offsetY: 1, color: '#181018', blur: 0, fill: true },
    }).setDepth(850);
    this.shopUIContainer.add(t1);
    this.shopTexts.push(t1);

    const t2 = this.add.text(panelX + 16, panelY + 44,
      `Quantite: \u25C4 ${this.selectedQuantity} \u25BA`, {
      fontFamily: FONT_FAMILY,
      fontSize: FONT_SMALL,
      color: '#F0E8D0',
      shadow: { offsetX: 1, offsetY: 1, color: '#181018', blur: 0, fill: true },
    }).setDepth(850);
    this.shopUIContainer.add(t2);
    this.shopTexts.push(t2);

    const t3 = this.add.text(panelX + 16, panelY + 72, `Total: ${total}F`, {
      fontFamily: FONT_FAMILY,
      fontSize: FONT_TINY,
      color: '#F0E8D0',
      shadow: { offsetX: 1, offsetY: 1, color: '#181018', blur: 0, fill: true },
    }).setDepth(850);
    this.shopUIContainer.add(t3);
    this.shopTexts.push(t3);
  }

  private handleQuantityInput(): void {
    const isBuying = this.pendingItem !== null;
    const maxQty = isBuying ? 99 : (this.pendingSellItem?.quantity ?? 1);

    if (Phaser.Input.Keyboard.JustDown(this.cursors.up) || Phaser.Input.Keyboard.JustDown(this.cursors.right)) {
      this.selectedQuantity = Math.min(maxQty, this.selectedQuantity + 1);
      this.renderQuantityUI();
    }
    if (Phaser.Input.Keyboard.JustDown(this.cursors.down) || Phaser.Input.Keyboard.JustDown(this.cursors.left)) {
      this.selectedQuantity = Math.max(1, this.selectedQuantity - 1);
      this.renderQuantityUI();
    }
    if (Phaser.Input.Keyboard.JustDown(this.enterKey)) {
      if (isBuying) {
        this.confirmBuy();
      } else {
        this.confirmSell();
      }
    }
    if (Phaser.Input.Keyboard.JustDown(this.escKey)) {
      this.pendingItem = null;
      this.pendingSellItem = null;
      if (isBuying) {
        this.shopMode = 'buy_list';
        this.renderBuyList();
      } else {
        this.shopMode = 'sell_list';
        this.openSellMenu();
      }
    }
  }

  private confirmBuy(): void {
    const item = this.pendingItem!;
    const total = item.price * this.selectedQuantity;
    const gs = GameState.getInstance();

    if (gs.getMoney() < total) {
      this.closeShopUI();
      this.dialogueBox.showText("Vous n'avez pas assez d'argent !", () => {
        this.pendingItem = null;
        this.shopMode = 'buy_list';
        this.renderBuyList();
      }, SHOPKEEPER.name);
      return;
    }

    gs.removeMoney(total);
    gs.getInventorySystem().addItem(item.itemId, this.selectedQuantity);

    this.closeShopUI();
    this.dialogueBox.showText(
      `Vous avez achete ${this.selectedQuantity}x ${item.name} !`,
      () => {
        this.pendingItem = null;
        this.shopMode = 'buy_list';
        this.renderBuyList();
      },
      SHOPKEEPER.name,
    );
  }

  private confirmSell(): void {
    const item = this.pendingSellItem!;
    const total = item.price * this.selectedQuantity;
    const gs = GameState.getInstance();

    gs.getInventorySystem().removeItem(item.itemId, this.selectedQuantity);
    gs.addMoney(total);

    this.closeShopUI();
    this.dialogueBox.showText(
      `Vendu ${this.selectedQuantity}x ${item.name} pour ${total}F !`,
      () => {
        this.pendingSellItem = null;
        this.openSellMenu();
      },
      SHOPKEEPER.name,
    );
  }

  // ===================== UI Helpers =====================

  private closeShopUI(): void {
    this.shopUIContainer.setVisible(false);
    this.shopUIGraphics.clear();
    this.clearShopTexts();
    this.shopMode = 'none';
  }

  private clearShopTexts(): void {
    for (const t of this.shopTexts) {
      t.destroy();
    }
    this.shopTexts = [];
  }

  private adjustScroll(totalItems: number): void {
    if (this.selectedIndex < this.scrollOffset) {
      this.scrollOffset = this.selectedIndex;
    }
    if (this.selectedIndex >= this.scrollOffset + this.maxVisible) {
      this.scrollOffset = this.selectedIndex - this.maxVisible + 1;
    }
  }

  private updateMoneyDisplay(): void {
    const gs = GameState.getInstance();
    this.moneyText?.setText(`${gs.getMoney()}F`);
  }

  private drawPanel(g: Phaser.GameObjects.Graphics, x: number, y: number, w: number, h: number): void {
    g.fillStyle(COLORS.UI_BORDER_DARK, 1);
    g.fillRoundedRect(x, y, w, h, 8);
    g.fillStyle(COLORS.UI_BORDER, 1);
    g.fillRoundedRect(x + 3, y + 3, w - 6, h - 6, 7);
    g.fillStyle(COLORS.UI_BORDER_LIGHT, 1);
    g.fillRoundedRect(x + 5, y + 5, w - 10, h - 10, 6);
    g.fillStyle(COLORS.DIALOGUE_BG, 0.97);
    g.fillRoundedRect(x + 6, y + 6, w - 12, h - 12, 5);
  }

  // ===================== Exit =====================

  private exitShop(): void {
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
        const tile = SHOP_MAP[y][x];
        const color = SHOP_TILE_COLORS[tile] ?? 0xE0D0B8;
        const px = this.offsetX + x * TILE_SIZE;
        const py = this.offsetY + y * TILE_SIZE;

        this.mapGraphics.fillStyle(color, 1);
        this.mapGraphics.fillRect(px, py, TILE_SIZE, TILE_SIZE);

        if (tile === ShopTile.Counter) {
          this.mapGraphics.fillStyle(0xA08040, 1);
          this.mapGraphics.fillRect(px, py, TILE_SIZE, 4);
        } else if (tile === ShopTile.Shelf) {
          // Shelf lines
          this.mapGraphics.fillStyle(0x806030, 1);
          this.mapGraphics.fillRect(px + 2, py + 8, TILE_SIZE - 4, 3);
          this.mapGraphics.fillRect(px + 2, py + 20, TILE_SIZE - 4, 3);
          // Items on shelf
          this.mapGraphics.fillStyle(0xE8A838, 1);
          this.mapGraphics.fillRect(px + 6, py + 3, 8, 5);
          this.mapGraphics.fillStyle(0x58A8F8, 1);
          this.mapGraphics.fillRect(px + 18, py + 3, 8, 5);
          this.mapGraphics.fillStyle(0xF86848, 1);
          this.mapGraphics.fillRect(px + 6, py + 13, 8, 7);
          this.mapGraphics.fillStyle(0x88D848, 1);
          this.mapGraphics.fillRect(px + 18, py + 13, 8, 7);
        } else if (tile === ShopTile.Door) {
          this.mapGraphics.fillStyle(0xC8A040, 1);
          this.mapGraphics.fillRect(px + TILE_SIZE - 8, py + 14, 4, 4);
        } else if (tile === ShopTile.Mat) {
          this.mapGraphics.fillStyle(0x3878B8, 1);
          this.mapGraphics.fillRect(px + 2, py + 2, TILE_SIZE - 4, TILE_SIZE - 4);
        }

        this.mapGraphics.lineStyle(1, 0x000000, 0.05);
        this.mapGraphics.strokeRect(px, py, TILE_SIZE, TILE_SIZE);
      }
    }
  }

  private drawNPC(): void {
    this.npcGraphics.clear();
    const px = this.offsetX + SHOPKEEPER.x * TILE_SIZE;
    const py = this.offsetY + SHOPKEEPER.y * TILE_SIZE;

    // Body
    this.npcGraphics.fillStyle(SHOPKEEPER.color, 1);
    this.npcGraphics.fillRect(px + 8, py + 12, 16, 18);

    // Head
    this.npcGraphics.fillStyle(0xF8D0B0, 1);
    this.npcGraphics.fillCircle(px + 16, py + 10, 8);

    // Hair
    this.npcGraphics.fillStyle(SHOPKEEPER.hairColor, 1);
    this.npcGraphics.fillRect(px + 8, py + 2, 16, 6);

    // Apron
    this.npcGraphics.fillStyle(0xE8E8E8, 1);
    this.npcGraphics.fillRect(px + 10, py + 14, 12, 14);
  }

  private drawPlayer(): void {
    this.playerGraphics.clear();
    const px = this.offsetX + this.playerX * TILE_SIZE;
    const py = this.offsetY + this.playerY * TILE_SIZE;

    this.playerGraphics.fillStyle(0x3868B8, 1);
    this.playerGraphics.fillRect(px + 8, py + 12, 16, 18);

    this.playerGraphics.fillStyle(0xF8D0B0, 1);
    this.playerGraphics.fillCircle(px + 16, py + 10, 8);

    this.playerGraphics.fillStyle(0xC84040, 1);
    this.playerGraphics.fillRect(px + 6, py + 2, 20, 6);

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
