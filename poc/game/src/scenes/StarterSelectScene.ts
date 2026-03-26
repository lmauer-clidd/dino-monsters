import Phaser from 'phaser';
import {
  SCENE_KEYS,
  GAME_WIDTH,
  GAME_HEIGHT,
  COLORS,
  FONT_FAMILY,
  DinoType,
  DINO_TYPE_COLORS,
  DINO_TYPE_NAMES,
} from '../utils/constants';
import { GameState } from '../systems/GameState';
import { AudioSystem } from '../systems/AudioSystem';

// ============================================================
// Starter data
// ============================================================
const STARTERS = [
  { name: 'PYREX', type: DinoType.Fire, speciesId: 1 },
  { name: 'AQUADON', type: DinoType.Water, speciesId: 4 },
  { name: 'FLORASAUR', type: DinoType.Flora, speciesId: 7 },
];

// ============================================================
// Phases — simple linear flow, no nested callbacks
// ============================================================
type Phase =
  | 'intro1'      // "Bienvenue..." — click to advance
  | 'intro2'      // "Choisis ton compagnon" — click to advance
  | 'select'      // Player picks a starter
  | 'confirm'     // "Tu choisis X ?" OUI/NON
  | 'outro1'      // "Excellent choix !"
  | 'outro2'      // "Ton rival a choisi..."
  | 'transition'; // Going to overworld

export class StarterSelectScene extends Phaser.Scene {
  private selectedIndex = 0;
  private phase: Phase = 'intro1';
  private confirmChoice = 0; // 0=OUI, 1=NON
  private pulseTimer = 0;
  private inputReady = false; // blocks input briefly after phase change

  // Graphics
  private starterContainers: Phaser.GameObjects.Container[] = [];
  private selectorGfx!: Phaser.GameObjects.Graphics;
  private textBox!: Phaser.GameObjects.Graphics;
  private textContent!: Phaser.GameObjects.Text;
  private nameTag!: Phaser.GameObjects.Container;
  private nameTagBg!: Phaser.GameObjects.Graphics;
  private nameTagText!: Phaser.GameObjects.Text;
  private choiceTexts: Phaser.GameObjects.Text[] = [];
  private choiceBox!: Phaser.GameObjects.Graphics;
  private arrowText!: Phaser.GameObjects.Text;

  constructor() {
    super({ key: SCENE_KEYS.STARTER_SELECT });
  }

  create(): void {
    this.cameras.main.setBackgroundColor(COLORS.BG_DARK);
    this.phase = 'intro1';
    this.selectedIndex = 0;
    this.pulseTimer = 0;
    this.inputReady = false;

    // Unblock input after 500ms
    this.time.delayedCall(500, () => { this.inputReady = true; });

    // Title
    this.add.text(GAME_WIDTH / 2, 28, 'LABORATOIRE DU PROF. PALEO', {
      fontFamily: FONT_FAMILY, fontSize: '16px', color: '#F0E8D0',
    }).setOrigin(0.5);

    const decoGfx = this.add.graphics();
    decoGfx.lineStyle(1, COLORS.UI_BORDER, 0.5);
    decoGfx.lineBetween(GAME_WIDTH / 2 - 180, 48, GAME_WIDTH / 2 + 180, 48);

    // Draw starters
    const spacing = 190;
    const startX = GAME_WIDTH / 2 - spacing;
    this.starterContainers = [];

    for (let i = 0; i < 3; i++) {
      const cx = startX + i * spacing;
      const cy = 155;
      const starter = STARTERS[i];
      const color = DINO_TYPE_COLORS[starter.type];

      const container = this.add.container(cx, cy);

      const platGfx = this.add.graphics();
      platGfx.fillStyle(0x303048, 1);
      platGfx.fillEllipse(0, 40, 100, 24);
      platGfx.fillStyle(0x3a3a52, 1);
      platGfx.fillEllipse(0, 38, 90, 18);
      container.add(platGfx);

      const dinoGfx = this.add.graphics();
      this.drawStarterDino(dinoGfx, 0, 0, i, color);
      container.add(dinoGfx);

      container.add(this.add.text(0, 60, starter.name, {
        fontFamily: FONT_FAMILY, fontSize: '14px', color: '#F0E8D0',
      }).setOrigin(0.5));

      const typeColor = '#' + DINO_TYPE_COLORS[starter.type].toString(16).padStart(6, '0');
      container.add(this.add.text(0, 78, DINO_TYPE_NAMES[starter.type], {
        fontFamily: FONT_FAMILY, fontSize: '12px', color: typeColor,
      }).setOrigin(0.5));

      this.starterContainers.push(container);
    }

    // Selector ring
    this.selectorGfx = this.add.graphics().setDepth(5);
    this.drawSelector();

    // ---- Text Box (bottom of screen, like DialogueBox but self-managed) ----
    const boxX = 16, boxY = GAME_HEIGHT - 112, boxW = GAME_WIDTH - 32, boxH = 96;

    this.textBox = this.add.graphics().setDepth(100).setScrollFactor(0);
    // 3-layer amber border
    this.textBox.fillStyle(0x886830, 1);
    this.textBox.fillRoundedRect(boxX, boxY, boxW, boxH, 8);
    this.textBox.fillStyle(0xC89840, 1);
    this.textBox.fillRoundedRect(boxX + 3, boxY + 3, boxW - 6, boxH - 6, 7);
    this.textBox.fillStyle(0xE8C868, 1);
    this.textBox.fillRoundedRect(boxX + 5, boxY + 5, boxW - 10, boxH - 10, 6);
    this.textBox.fillStyle(0x181028, 0.97);
    this.textBox.fillRoundedRect(boxX + 6, boxY + 6, boxW - 12, boxH - 12, 5);

    this.textContent = this.add.text(boxX + 20, boxY + 16, '', {
      fontFamily: FONT_FAMILY, fontSize: '14px', color: '#F0E8D0',
      wordWrap: { width: boxW - 44 }, lineSpacing: 6,
    }).setDepth(101).setScrollFactor(0);

    // Name tag
    this.nameTag = this.add.container(boxX + 16, boxY - 22).setDepth(102).setScrollFactor(0);
    this.nameTagBg = this.add.graphics();
    this.nameTag.add(this.nameTagBg);
    this.nameTagText = this.add.text(12, 5, '', {
      fontFamily: FONT_FAMILY, fontSize: '12px', color: '#F0E8D0',
    });
    this.nameTag.add(this.nameTagText);

    // Arrow indicator
    this.arrowText = this.add.text(boxX + boxW - 30, boxY + boxH - 22, '\u25BC', {
      fontFamily: FONT_FAMILY, fontSize: '12px', color: '#F0E8D0',
    }).setDepth(101).setScrollFactor(0);
    this.tweens.add({
      targets: this.arrowText, y: boxY + boxH - 18,
      duration: 400, yoyo: true, repeat: -1, ease: 'Sine.easeInOut',
    });

    // Choice box (for confirm phase)
    this.choiceBox = this.add.graphics().setDepth(102).setScrollFactor(0).setVisible(false);
    this.choiceTexts = [];

    // Set initial text
    this.showPhaseText();

    // ---- INPUT: One unified handler for EVERYTHING ----
    // Keyboard
    if (this.input.keyboard) {
      const enter = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.ENTER);
      const space = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.SPACE);
      const left = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.LEFT);
      const right = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.RIGHT);
      const up = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.UP);
      const down = this.input.keyboard.addKey(Phaser.Input.Keyboard.KeyCodes.DOWN);

      this.input.keyboard.on('keydown', (event: KeyboardEvent) => {
        if (!this.inputReady) return;
        const key = event.code;
        if (key === 'Enter' || key === 'Space') this.onConfirm();
        else if (key === 'ArrowLeft') this.onLeft();
        else if (key === 'ArrowRight') this.onRight();
        else if (key === 'ArrowUp') this.onUp();
        else if (key === 'ArrowDown') this.onDown();
      });
    }

    // Mouse / Touch
    this.input.on('pointerdown', () => {
      if (!this.inputReady) return;
      this.onConfirm();
    });
  }

  // ============================================================
  // Input handlers — simple state machine
  // ============================================================

  private onConfirm(): void {
    AudioSystem.getInstance().init();
    AudioSystem.getInstance().playMenuSelect();

    switch (this.phase) {
      case 'intro1':
        this.setPhase('intro2');
        break;
      case 'intro2':
        this.setPhase('select');
        break;
      case 'select':
        this.setPhase('confirm');
        break;
      case 'confirm':
        if (this.confirmChoice === 0) { // OUI
          this.doConfirmStarter();
        } else { // NON
          this.setPhase('select');
        }
        break;
      case 'outro1':
        this.setPhase('outro2');
        break;
      case 'outro2':
        this.setPhase('transition');
        break;
    }
  }

  private onLeft(): void {
    if (this.phase === 'select') {
      this.selectedIndex = (this.selectedIndex - 1 + 3) % 3;
      this.drawSelector();
    }
  }

  private onRight(): void {
    if (this.phase === 'select') {
      this.selectedIndex = (this.selectedIndex + 1) % 3;
      this.drawSelector();
    }
  }

  private onUp(): void {
    if (this.phase === 'confirm') {
      this.confirmChoice = 0;
      this.updateChoiceHighlight();
    }
  }

  private onDown(): void {
    if (this.phase === 'confirm') {
      this.confirmChoice = 1;
      this.updateChoiceHighlight();
    }
  }

  // ============================================================
  // Phase management
  // ============================================================

  private setPhase(p: Phase): void {
    this.phase = p;
    this.inputReady = false;
    // Brief input cooldown to prevent double-advancing
    this.time.delayedCall(250, () => { this.inputReady = true; });
    this.showPhaseText();

    if (p === 'transition') {
      this.doTransition();
    }
  }

  private showPhaseText(): void {
    this.choiceBox.setVisible(false);
    this.choiceTexts.forEach(t => t.destroy());
    this.choiceTexts = [];

    switch (this.phase) {
      case 'intro1':
        this.showText('Bienvenue dans mon laboratoire ! Je suis le Professeur Paleo.', 'PROF. PALEO');
        break;
      case 'intro2':
        this.showText('Le monde des dinos est vaste et plein de mysteres. Choisis ton premier compagnon !', 'PROF. PALEO');
        break;
      case 'select':
        this.showText('Clique sur un dino ou utilise les fleches + Entree pour choisir.', '');
        break;
      case 'confirm':
        this.confirmChoice = 0;
        this.showText(`Tu choisis ${STARTERS[this.selectedIndex].name} ?`, 'PROF. PALEO');
        this.showChoices();
        break;
      case 'outro1':
        this.showText(`Excellent choix ! ${STARTERS[this.selectedIndex].name} est un compagnon formidable.`, 'PROF. PALEO');
        break;
      case 'outro2': {
        const rivalIdx = (this.selectedIndex + 1) % 3;
        this.showText(`Ton rival a choisi ${STARTERS[rivalIdx].name}. Bonne chance, dresseur !`, 'PROF. PALEO');
        break;
      }
    }
  }

  private showText(text: string, speaker: string): void {
    this.textContent.setText(text);

    if (speaker) {
      this.nameTagText.setText(speaker);
      const w = speaker.length * 11 + 28;
      this.nameTagBg.clear();
      this.nameTagBg.fillStyle(0x886830, 1);
      this.nameTagBg.fillRoundedRect(0, 0, w, 26, 6);
      this.nameTagBg.fillStyle(0xC89840, 1);
      this.nameTagBg.fillRoundedRect(2, 2, w - 4, 22, 5);
      this.nameTagBg.fillStyle(0xE8C868, 1);
      this.nameTagBg.fillRoundedRect(3, 3, w - 6, 20, 4);
      this.nameTagBg.fillStyle(0x181028, 0.97);
      this.nameTagBg.fillRoundedRect(4, 4, w - 8, 18, 3);
      this.nameTag.setVisible(true);
    } else {
      this.nameTag.setVisible(false);
    }
  }

  private showChoices(): void {
    const cx = GAME_WIDTH - 200, cy = GAME_HEIGHT - 180, cw = 160, ch = 64;
    this.choiceBox.clear();
    this.choiceBox.fillStyle(0x886830, 1);
    this.choiceBox.fillRoundedRect(cx, cy, cw, ch, 6);
    this.choiceBox.fillStyle(0xC89840, 1);
    this.choiceBox.fillRoundedRect(cx + 2, cy + 2, cw - 4, ch - 4, 5);
    this.choiceBox.fillStyle(0x181028, 0.97);
    this.choiceBox.fillRoundedRect(cx + 4, cy + 4, cw - 8, ch - 8, 4);
    this.choiceBox.setVisible(true);

    const options = ['OUI', 'NON'];
    this.choiceTexts = options.map((opt, i) => {
      const prefix = i === this.confirmChoice ? '\u25B6 ' : '  ';
      return this.add.text(cx + 18, cy + 12 + i * 24, prefix + opt, {
        fontFamily: FONT_FAMILY, fontSize: '14px',
        color: i === this.confirmChoice ? '#E8C868' : '#F0E8D0',
      }).setDepth(103).setScrollFactor(0);
    });
  }

  private updateChoiceHighlight(): void {
    const options = ['OUI', 'NON'];
    this.choiceTexts.forEach((t, i) => {
      const prefix = i === this.confirmChoice ? '\u25B6 ' : '  ';
      t.setText(prefix + options[i]);
      t.setColor(i === this.confirmChoice ? '#E8C868' : '#F0E8D0');
    });
    AudioSystem.getInstance().playMenuMove();
  }

  // ============================================================
  // Confirm starter & transition
  // ============================================================

  private doConfirmStarter(): void {
    const starter = STARTERS[this.selectedIndex];
    GameState.reset();
    const gs = GameState.getInstance();
    gs.init('Dresseur', starter.speciesId);
    gs.setFlag('has_starter', true);
    this.setPhase('outro1');
  }

  private doTransition(): void {
    this.cameras.main.fadeOut(400, 24, 16, 24);
    // Use a simple Phaser timer — when it fires, the scene is still active
    this.time.delayedCall(450, () => {
      this.scene.start(SCENE_KEYS.OVERWORLD, {
        hasStarter: true,
        starterName: STARTERS[this.selectedIndex].name,
      });
    });
  }

  // ============================================================
  // Update — just pulse animation
  // ============================================================
  update(_time: number, delta: number): void {
    if (this.phase === 'select') {
      this.pulseTimer += delta;
      const scale = 1 + Math.sin(this.pulseTimer / 300) * 0.03;
      this.starterContainers.forEach((c, i) => {
        c.setScale(i === this.selectedIndex ? scale : 1);
      });
    }
  }

  // ============================================================
  // Selector ring
  // ============================================================
  private drawSelector(): void {
    this.selectorGfx.clear();
    const c = this.starterContainers[this.selectedIndex];
    if (!c) return;
    this.selectorGfx.lineStyle(3, COLORS.UI_BORDER, 0.9);
    this.selectorGfx.strokeCircle(c.x, c.y + 8, 50);
    this.selectorGfx.lineStyle(1, COLORS.UI_BORDER_LIGHT, 0.4);
    this.selectorGfx.strokeCircle(c.x, c.y + 8, 54);
  }

  // ============================================================
  // Dino Drawing (unchanged)
  // ============================================================
  private drawStarterDino(gfx: Phaser.GameObjects.Graphics, ox: number, oy: number, index: number, baseColor: number): void {
    const bright = Phaser.Display.Color.IntegerToColor(baseColor).brighten(25).color;
    const dark = Phaser.Display.Color.IntegerToColor(baseColor).darken(20).color;

    if (index === 0) {
      gfx.fillStyle(baseColor, 1);
      gfx.fillRoundedRect(ox - 14, oy - 12, 28, 24, 6);
      gfx.fillRoundedRect(ox + 4, oy - 28, 18, 16, 5);
      gfx.fillStyle(dark, 1);
      gfx.fillTriangle(ox + 16, oy - 28, ox + 20, oy - 40, ox + 22, oy - 26);
      gfx.fillStyle(0xFFFFFF, 1);
      gfx.fillCircle(ox + 16, oy - 22, 4);
      gfx.fillStyle(0x201010, 1);
      gfx.fillCircle(ox + 17, oy - 22, 2);
      gfx.lineStyle(1, dark, 1);
      gfx.lineBetween(ox + 20, oy - 18, ox + 24, oy - 17);
      gfx.fillStyle(bright, 1);
      gfx.fillEllipse(ox - 2, oy, 18, 14);
      gfx.fillStyle(baseColor, 1);
      gfx.fillRect(ox - 16, oy - 6, 6, 3);
      gfx.fillRect(ox + 12, oy - 6, 6, 3);
      gfx.fillStyle(dark, 1);
      gfx.fillRect(ox - 18, oy - 6, 2, 2);
      gfx.fillRect(ox + 18, oy - 6, 2, 2);
      gfx.fillStyle(baseColor, 1);
      gfx.fillRoundedRect(ox - 10, oy + 10, 8, 16, 3);
      gfx.fillRoundedRect(ox + 4, oy + 10, 8, 16, 3);
      gfx.fillStyle(dark, 1);
      gfx.fillEllipse(ox - 6, oy + 28, 12, 5);
      gfx.fillEllipse(ox + 8, oy + 28, 12, 5);
      gfx.fillStyle(baseColor, 1);
      gfx.fillTriangle(ox - 14, oy - 4, ox - 34, oy - 12, ox - 16, oy + 8);
      gfx.fillStyle(0xF8D830, 0.9);
      gfx.fillCircle(ox - 34, oy - 14, 5);
      gfx.fillStyle(0xF08030, 0.8);
      gfx.fillCircle(ox - 32, oy - 18, 4);
      gfx.fillStyle(0xF8D830, 0.6);
      gfx.fillCircle(ox - 36, oy - 10, 3);
    } else if (index === 1) {
      gfx.fillStyle(baseColor, 1);
      gfx.fillEllipse(ox, oy + 4, 34, 22);
      gfx.fillStyle(bright, 1);
      gfx.fillEllipse(ox, oy + 8, 24, 12);
      gfx.fillStyle(baseColor, 1);
      gfx.fillEllipse(ox + 12, oy - 14, 14, 30);
      gfx.fillEllipse(ox + 16, oy - 28, 16, 12);
      gfx.fillStyle(0xFFFFFF, 1);
      gfx.fillCircle(ox + 20, oy - 30, 3.5);
      gfx.fillStyle(0x102030, 1);
      gfx.fillCircle(ox + 21, oy - 30, 2);
      gfx.lineStyle(1, dark, 0.8);
      gfx.beginPath();
      gfx.arc(ox + 22, oy - 25, 5, 0, Math.PI * 0.6, false);
      gfx.strokePath();
      gfx.fillStyle(baseColor, 1);
      gfx.fillTriangle(ox - 16, oy, ox - 28, oy + 10, ox - 14, oy + 12);
      gfx.fillTriangle(ox + 16, oy, ox + 28, oy + 10, ox + 14, oy + 12);
      gfx.fillTriangle(ox - 16, oy + 4, ox - 34, oy - 4, ox - 18, oy + 14);
      gfx.fillStyle(dark, 1);
      gfx.fillTriangle(ox - 32, oy - 6, ox - 42, oy - 14, ox - 38, oy + 4);
      gfx.fillStyle(0x88C0F8, 0.7);
      gfx.fillCircle(ox - 24, oy - 20, 2.5);
      gfx.fillCircle(ox + 30, oy - 14, 2);
      gfx.fillCircle(ox - 10, oy - 34, 2);
      gfx.fillCircle(ox + 26, oy + 2, 1.5);
    } else {
      gfx.fillStyle(baseColor, 1);
      gfx.fillRoundedRect(ox - 16, oy - 10, 32, 24, 8);
      gfx.fillStyle(bright, 1);
      gfx.fillEllipse(ox, oy + 4, 24, 12);
      gfx.fillStyle(baseColor, 1);
      gfx.fillEllipse(ox + 20, oy - 8, 20, 16);
      gfx.fillStyle(0xFFFFFF, 1);
      gfx.fillCircle(ox + 24, oy - 12, 4);
      gfx.fillStyle(0x102010, 1);
      gfx.fillCircle(ox + 25, oy - 12, 2.5);
      gfx.fillStyle(dark, 1);
      gfx.fillCircle(ox + 30, oy - 6, 1.5);
      gfx.lineStyle(1, dark, 0.7);
      gfx.beginPath();
      gfx.arc(ox + 26, oy - 4, 4, 0, Math.PI * 0.5, false);
      gfx.strokePath();
      gfx.fillStyle(baseColor, 1);
      gfx.fillRoundedRect(ox - 14, oy + 12, 8, 14, 3);
      gfx.fillRoundedRect(ox - 2, oy + 12, 8, 14, 3);
      gfx.fillRoundedRect(ox + 8, oy + 12, 8, 14, 3);
      gfx.fillRoundedRect(ox - 8, oy + 14, 8, 12, 3);
      const leafColor = 0x48A030;
      gfx.fillStyle(leafColor, 1);
      gfx.fillTriangle(ox - 8, oy - 10, ox - 14, oy - 26, ox - 2, oy - 10);
      gfx.fillTriangle(ox + 2, oy - 10, ox - 2, oy - 24, ox + 8, oy - 10);
      gfx.fillTriangle(ox - 4, oy - 10, ox - 8, oy - 22, ox + 2, oy - 10);
      gfx.fillStyle(0xE85080, 1);
      gfx.fillCircle(ox - 2, oy - 26, 5);
      gfx.fillStyle(0xF8D830, 1);
      gfx.fillCircle(ox - 2, oy - 26, 2.5);
      gfx.fillStyle(baseColor, 1);
      gfx.fillTriangle(ox - 16, oy - 2, ox - 28, oy - 4, ox - 16, oy + 8);
      gfx.fillStyle(leafColor, 1);
      gfx.fillTriangle(ox - 26, oy - 8, ox - 36, oy - 4, ox - 28, oy + 2);
    }
  }
}
