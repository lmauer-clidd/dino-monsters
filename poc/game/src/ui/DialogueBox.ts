import Phaser from 'phaser';
import {
  GAME_WIDTH,
  GAME_HEIGHT,
  COLORS,
  DIALOGUE_SPEED,
  DIALOGUE_BOX_HEIGHT,
  FONT_FAMILY,
  FONT_SMALL,
  FONT_TINY,
} from '../utils/constants';
import { AudioSystem } from '../systems/AudioSystem';

// --- Layout constants ---
const MARGIN = 16;
const BOX_X = MARGIN;
const BOX_Y = GAME_HEIGHT - DIALOGUE_BOX_HEIGHT - MARGIN;
const BOX_W = GAME_WIDTH - MARGIN * 2;  // 608px
const BOX_H = DIALOGUE_BOX_HEIGHT;       // 96px
const PADDING = 16;
const BORDER_OUTER = 3;
const BORDER_INNER = 2;
const CORNER_RADIUS = 8;
const CHARS_PER_LINE = 32;
const MAX_LINES = 3;

// --- Text color as CSS hex ---
const TEXT_COLOR = '#F0E8D0';
const TEXT_SHADOW_COLOR = '#181018';

export class DialogueBox {
  private scene: Phaser.Scene;
  private container!: Phaser.GameObjects.Container;
  private bgGraphics!: Phaser.GameObjects.Graphics;
  private nameTag!: Phaser.GameObjects.Container;
  private nameText!: Phaser.GameObjects.Text;
  private nameTagBg!: Phaser.GameObjects.Graphics;
  private bodyText!: Phaser.GameObjects.Text;
  private arrowIndicator!: Phaser.GameObjects.Text;
  private choiceTexts: Phaser.GameObjects.Text[] = [];
  private choiceContainer!: Phaser.GameObjects.Container;
  private choiceBgGraphics!: Phaser.GameObjects.Graphics;

  private active = false;
  private typewriting = false;
  private clickCooldown = 0;
  private fullText = '';
  private displayedText = '';
  private charIndex = 0;
  private typewriterTimer?: Phaser.Time.TimerEvent;
  private pages: string[] = [];
  private currentPage = 0;
  private onComplete?: () => void;

  // Choices
  private choiceMode = false;
  private choices: string[] = [];
  private selectedChoice = 0;
  private onChoiceSelect?: (index: number) => void;

  // Input keys
  private enterKey?: Phaser.Input.Keyboard.Key;
  private spaceKey?: Phaser.Input.Keyboard.Key;
  private upKey?: Phaser.Input.Keyboard.Key;
  private downKey?: Phaser.Input.Keyboard.Key;

  // Arrow bounce tween
  private arrowTween?: Phaser.Tweens.Tween;

  constructor(scene: Phaser.Scene) {
    this.scene = scene;
    this.create();
  }

  private create(): void {
    this.container = this.scene.add.container(0, 0).setDepth(1000);
    this.container.setScrollFactor(0);

    // --- Background with 3-layer amber border ---
    this.bgGraphics = this.scene.add.graphics();
    this.drawBox();
    this.container.add(this.bgGraphics);

    // --- Speaker name tag (above box) ---
    this.nameTag = this.scene.add.container(BOX_X + 16, BOX_Y - 24);
    this.nameTagBg = this.scene.add.graphics();
    this.nameTag.add(this.nameTagBg);

    this.nameText = this.scene.add.text(12, 5, '', {
      fontFamily: FONT_FAMILY,
      fontSize: FONT_TINY,
      color: TEXT_COLOR,
      shadow: {
        offsetX: 1,
        offsetY: 1,
        color: TEXT_SHADOW_COLOR,
        blur: 0,
        fill: true,
      },
    });
    this.nameTag.add(this.nameText);
    this.nameTag.setVisible(false);
    this.container.add(this.nameTag);

    // --- Body text ---
    this.bodyText = this.scene.add.text(
      BOX_X + PADDING + BORDER_OUTER + BORDER_INNER,
      BOX_Y + PADDING + BORDER_OUTER,
      '',
      {
        fontFamily: FONT_FAMILY,
        fontSize: FONT_SMALL,
        color: TEXT_COLOR,
        wordWrap: { width: BOX_W - PADDING * 2 - (BORDER_OUTER + BORDER_INNER) * 2 },
        lineSpacing: 6,
        shadow: {
          offsetX: 1,
          offsetY: 1,
          color: TEXT_SHADOW_COLOR,
          blur: 0,
          fill: true,
        },
      }
    );
    this.container.add(this.bodyText);

    // --- Advance indicator (bouncing triangle) ---
    this.arrowIndicator = this.scene.add.text(
      BOX_X + BOX_W - PADDING - 12,
      BOX_Y + BOX_H - PADDING - 8,
      '\u25BC',
      {
        fontFamily: FONT_FAMILY,
        fontSize: FONT_TINY,
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
    this.arrowIndicator.setVisible(false);
    this.container.add(this.arrowIndicator);

    // Bouncing animation for arrow
    this.arrowTween = this.scene.tweens.add({
      targets: this.arrowIndicator,
      y: BOX_Y + BOX_H - PADDING - 4,
      duration: 400,
      yoyo: true,
      repeat: -1,
      ease: 'Sine.easeInOut',
    });

    // --- Choice container ---
    this.choiceContainer = this.scene.add.container(0, 0);
    this.choiceBgGraphics = this.scene.add.graphics();
    this.choiceContainer.add(this.choiceBgGraphics);
    this.container.add(this.choiceContainer);

    // --- Input setup ---
    const kb = this.scene.input.keyboard;
    if (kb) {
      this.enterKey = kb.addKey(Phaser.Input.Keyboard.KeyCodes.ENTER);
      this.spaceKey = kb.addKey(Phaser.Input.Keyboard.KeyCodes.SPACE);
      this.upKey = kb.addKey(Phaser.Input.Keyboard.KeyCodes.UP);
      this.downKey = kb.addKey(Phaser.Input.Keyboard.KeyCodes.DOWN);
    }

    // --- Mouse/touch click to advance dialogue ---
    this.scene.input.on('pointerdown', () => {
      if (!this.active) return;
      if (this.clickCooldown > 0) return;
      this.clickCooldown = 150; // 150ms cooldown between clicks
      if (this.choiceMode) {
        // Click selects the current choice
        const idx = this.selectedChoice;
        this.close();
        this.onChoiceSelect?.(idx);
      } else if (this.typewriting) {
        this.skipTypewriter();
      } else {
        this.currentPage++;
        if (this.currentPage < this.pages.length) {
          this.startTypewriter(this.pages[this.currentPage]);
        } else {
          const cb = this.onComplete;
          this.close();
          cb?.();
        }
      }
    });

    this.container.setVisible(false);
  }

  /** Draw the 3-layer amber border dialogue box */
  private drawBox(): void {
    this.bgGraphics.clear();

    // Layer 1: Outer dark amber border
    this.bgGraphics.fillStyle(COLORS.UI_BORDER_DARK, 1);
    this.bgGraphics.fillRoundedRect(BOX_X, BOX_Y, BOX_W, BOX_H, CORNER_RADIUS);

    // Layer 2: Middle amber border
    this.bgGraphics.fillStyle(COLORS.UI_BORDER, 1);
    this.bgGraphics.fillRoundedRect(
      BOX_X + BORDER_OUTER,
      BOX_Y + BORDER_OUTER,
      BOX_W - BORDER_OUTER * 2,
      BOX_H - BORDER_OUTER * 2,
      CORNER_RADIUS - 1
    );

    // Layer 3: Inner light amber border
    this.bgGraphics.fillStyle(COLORS.UI_BORDER_LIGHT, 1);
    this.bgGraphics.fillRoundedRect(
      BOX_X + BORDER_OUTER + BORDER_INNER,
      BOX_Y + BORDER_OUTER + BORDER_INNER,
      BOX_W - (BORDER_OUTER + BORDER_INNER) * 2,
      BOX_H - (BORDER_OUTER + BORDER_INNER) * 2,
      CORNER_RADIUS - 2
    );

    // Layer 4: Inner dark background
    this.bgGraphics.fillStyle(COLORS.DIALOGUE_BG, 0.97);
    this.bgGraphics.fillRoundedRect(
      BOX_X + BORDER_OUTER + BORDER_INNER + 1,
      BOX_Y + BORDER_OUTER + BORDER_INNER + 1,
      BOX_W - (BORDER_OUTER + BORDER_INNER + 1) * 2,
      BOX_H - (BORDER_OUTER + BORDER_INNER + 1) * 2,
      CORNER_RADIUS - 3
    );
  }

  /** Draw the speaker name tag background */
  private drawNameTag(name: string): void {
    this.nameTagBg.clear();
    const textWidth = name.length * 11 + 28; // wider for Press Start 2P font
    const tagH = 26;

    // Same 3-layer border as dialogue box
    this.nameTagBg.fillStyle(COLORS.UI_BORDER_DARK, 1);
    this.nameTagBg.fillRoundedRect(0, 0, textWidth, tagH, 6);

    this.nameTagBg.fillStyle(COLORS.UI_BORDER, 1);
    this.nameTagBg.fillRoundedRect(2, 2, textWidth - 4, tagH - 4, 5);

    this.nameTagBg.fillStyle(COLORS.UI_BORDER_LIGHT, 1);
    this.nameTagBg.fillRoundedRect(3, 3, textWidth - 6, tagH - 6, 4);

    this.nameTagBg.fillStyle(COLORS.DIALOGUE_BG, 0.97);
    this.nameTagBg.fillRoundedRect(4, 4, textWidth - 8, tagH - 8, 3);
  }

  /** Split long text into pages that fit in the box */
  private paginate(text: string): string[] {
    const words = text.split(' ');
    const pages: string[] = [];
    let currentLine = '';
    let lineCount = 0;
    let currentPage = '';

    for (const word of words) {
      const testLine = currentLine ? currentLine + ' ' + word : word;
      if (testLine.length > CHARS_PER_LINE) {
        if (currentPage) currentPage += '\n';
        currentPage += currentLine;
        lineCount++;

        if (lineCount >= MAX_LINES) {
          pages.push(currentPage);
          currentPage = '';
          lineCount = 0;
        }
        currentLine = word;
      } else {
        currentLine = testLine;
      }
    }

    // Flush remaining
    if (currentLine) {
      if (currentPage) currentPage += '\n';
      currentPage += currentLine;
    }
    if (currentPage) pages.push(currentPage);

    return pages.length > 0 ? pages : [''];
  }

  /** Display text with typewriter effect */
  showText(text: string, callback?: () => void, speakerName?: string): void {
    this.active = true;
    this.choiceMode = false;
    this.clickCooldown = 300; // prevent instant-skip on new dialogue
    this.container.setVisible(true);
    this.choiceContainer.setVisible(false);

    // Speaker name
    if (speakerName) {
      this.nameText.setText(speakerName);
      this.drawNameTag(speakerName);
      this.nameTag.setVisible(true);
    } else {
      this.nameTag.setVisible(false);
    }

    this.onComplete = callback;
    this.pages = this.paginate(text);
    this.currentPage = 0;
    this.startTypewriter(this.pages[0]);
  }

  private startTypewriter(text: string): void {
    this.fullText = text;
    this.displayedText = '';
    this.charIndex = 0;
    this.typewriting = true;
    this.bodyText.setText('');
    this.arrowIndicator.setVisible(false);

    if (this.typewriterTimer) {
      this.typewriterTimer.destroy();
    }

    this.typewriterTimer = this.scene.time.addEvent({
      delay: DIALOGUE_SPEED,
      repeat: this.fullText.length - 1,
      callback: () => {
        this.displayedText += this.fullText[this.charIndex];
        this.charIndex++;
        this.bodyText.setText(this.displayedText);

        if (this.charIndex >= this.fullText.length) {
          this.typewriting = false;
          this.arrowIndicator.setVisible(true);
        }
      },
    });
  }

  /** Instantly show all text of current page */
  skipTypewriter(): void {
    if (!this.typewriting) return;
    if (this.typewriterTimer) {
      this.typewriterTimer.destroy();
      this.typewriterTimer = undefined;
    }
    this.displayedText = this.fullText;
    this.bodyText.setText(this.displayedText);
    this.typewriting = false;
    this.arrowIndicator.setVisible(true);
  }

  /** Show choice menu */
  showChoices(
    prompt: string,
    choices: string[],
    callback: (index: number) => void
  ): void {
    this.active = true;
    this.choiceMode = false;
    this.container.setVisible(true);

    // Show prompt text first, then display choices after typewriter completes
    this.showText(prompt, () => {
      this.choiceMode = true;
      this.choices = choices;
      this.selectedChoice = 0;
      this.onChoiceSelect = callback;
      this.arrowIndicator.setVisible(false);
      this.renderChoices();
    });
  }

  private renderChoices(): void {
    // Remove old choice texts (keep the bg graphics)
    this.choiceContainer.getAll().forEach((child) => {
      if (child !== this.choiceBgGraphics) {
        child.destroy();
      }
    });
    this.choiceTexts = [];

    // Choice panel to the right of the dialogue box
    const choiceW = 160;
    const itemH = 24;
    const choiceH = this.choices.length * itemH + 16;
    const choiceX = GAME_WIDTH - MARGIN - choiceW;
    const choiceY = BOX_Y - choiceH - 8;

    // Draw 3-layer border
    this.choiceBgGraphics.clear();
    this.choiceBgGraphics.fillStyle(COLORS.UI_BORDER_DARK, 1);
    this.choiceBgGraphics.fillRoundedRect(choiceX, choiceY, choiceW, choiceH, 6);

    this.choiceBgGraphics.fillStyle(COLORS.UI_BORDER, 1);
    this.choiceBgGraphics.fillRoundedRect(choiceX + 2, choiceY + 2, choiceW - 4, choiceH - 4, 5);

    this.choiceBgGraphics.fillStyle(COLORS.UI_BORDER_LIGHT, 1);
    this.choiceBgGraphics.fillRoundedRect(choiceX + 4, choiceY + 4, choiceW - 8, choiceH - 8, 4);

    this.choiceBgGraphics.fillStyle(COLORS.DIALOGUE_BG, 0.97);
    this.choiceBgGraphics.fillRoundedRect(choiceX + 5, choiceY + 5, choiceW - 10, choiceH - 10, 3);

    this.choices.forEach((choice, i) => {
      const isSelected = i === this.selectedChoice;
      const prefix = isSelected ? '\u25B6 ' : '  ';
      const t = this.scene.add.text(
        choiceX + 14,
        choiceY + 8 + i * itemH,
        prefix + choice,
        {
          fontFamily: FONT_FAMILY,
          fontSize: FONT_SMALL,
          color: isSelected ? '#E8C868' : TEXT_COLOR,
          shadow: {
            offsetX: 1,
            offsetY: 1,
            color: TEXT_SHADOW_COLOR,
            blur: 0,
            fill: true,
          },
        }
      );
      this.choiceContainer.add(t);
      this.choiceTexts.push(t);
    });

    this.choiceContainer.setVisible(true);
  }

  private updateChoiceHighlight(): void {
    this.choiceTexts.forEach((t, i) => {
      const isSelected = i === this.selectedChoice;
      const prefix = isSelected ? '\u25B6 ' : '  ';
      t.setText(prefix + this.choices[i]);
      t.setColor(isSelected ? '#E8C868' : TEXT_COLOR);
    });
  }

  /** Call this in scene update loop */
  update(): void {
    if (this.clickCooldown > 0) {
      this.clickCooldown -= this.scene.game.loop.delta;
    }
    if (!this.active) return;

    const enterJustDown = this.enterKey && Phaser.Input.Keyboard.JustDown(this.enterKey);
    const spaceJustDown = this.spaceKey && Phaser.Input.Keyboard.JustDown(this.spaceKey);
    const upJustDown = this.upKey && Phaser.Input.Keyboard.JustDown(this.upKey);
    const downJustDown = this.downKey && Phaser.Input.Keyboard.JustDown(this.downKey);

    if (this.choiceMode) {
      if (upJustDown) {
        this.selectedChoice = (this.selectedChoice - 1 + this.choices.length) % this.choices.length;
        this.updateChoiceHighlight();
        AudioSystem.getInstance().playMenuMove();
      }
      if (downJustDown) {
        this.selectedChoice = (this.selectedChoice + 1) % this.choices.length;
        this.updateChoiceHighlight();
        AudioSystem.getInstance().playMenuMove();
      }
      if (enterJustDown || spaceJustDown) {
        AudioSystem.getInstance().playMenuSelect();
        const idx = this.selectedChoice;
        this.close();
        this.onChoiceSelect?.(idx);
      }
      return;
    }

    if (enterJustDown || spaceJustDown) {
      AudioSystem.getInstance().playMenuSelect();
      if (this.typewriting) {
        this.skipTypewriter();
      } else {
        // Advance to next page or close
        this.currentPage++;
        if (this.currentPage < this.pages.length) {
          this.startTypewriter(this.pages[this.currentPage]);
        } else {
          const cb = this.onComplete;
          this.close();
          cb?.();
        }
      }
    }
  }

  isActive(): boolean {
    return this.active;
  }

  close(): void {
    this.active = false;
    this.typewriting = false;
    this.choiceMode = false;
    if (this.typewriterTimer) {
      this.typewriterTimer.destroy();
      this.typewriterTimer = undefined;
    }
    this.container.setVisible(false);
    this.choiceContainer.setVisible(false);
    this.choiceContainer.getAll().forEach((child) => {
      if (child !== this.choiceBgGraphics) {
        child.destroy();
      }
    });
    this.choiceTexts = [];
  }

  destroy(): void {
    this.close();
    if (this.arrowTween) {
      this.arrowTween.destroy();
    }
    this.container.destroy();
  }
}
