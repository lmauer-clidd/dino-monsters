import Phaser from 'phaser';
import {
  SCENE_KEYS,
  GAME_WIDTH,
  GAME_HEIGHT,
  COLORS,
  FONT_FAMILY,
  SAVE_KEY_PREFIX,
} from '../utils/constants';
import { GameState } from '../systems/GameState';
import { AudioSystem } from '../systems/AudioSystem';

// ============================================================
// Particle — tiny floating amber dot
// ============================================================
interface Particle {
  x: number;
  y: number;
  vx: number;
  vy: number;
  alpha: number;
  size: number;
  life: number;
  maxLife: number;
}

export class TitleScene extends Phaser.Scene {
  private blinkTimer = 0;
  private pressStartText!: Phaser.GameObjects.Text;
  private menuContainer!: Phaser.GameObjects.Container;
  private menuTexts: Phaser.GameObjects.Text[] = [];
  private selectedIndex = 0;
  private menuActive = false;
  private hasSave = false;

  private particles: Particle[] = [];
  private particleGfx!: Phaser.GameObjects.Graphics;

  private upKey?: Phaser.Input.Keyboard.Key;
  private downKey?: Phaser.Input.Keyboard.Key;
  private enterKey?: Phaser.Input.Keyboard.Key;

  constructor() {
    super({ key: SCENE_KEYS.TITLE });
  }

  create(): void {
    this.menuActive = false;
    this.selectedIndex = 0;
    this.hasSave = !!localStorage.getItem(SAVE_KEY_PREFIX + '0');

    // Initialize audio on first interaction and play title music
    const audioInit = () => {
      const audio = AudioSystem.getInstance();
      audio.init();
      audio.playTitleMusic();
      this.input.off('pointerdown', audioInit);
      this.input.keyboard?.off('keydown', audioInit);
    };
    this.input.on('pointerdown', audioInit);
    this.input.keyboard?.on('keydown', audioInit);

    // Dark background
    this.cameras.main.setBackgroundColor(COLORS.BG_DARK);

    // --- T-Rex silhouette (detailed multi-shape outline) ---
    const dinoGfx = this.add.graphics();
    const cx = GAME_WIDTH / 2;
    const cy = GAME_HEIGHT / 2 - 10;
    const silColor = 0x252540;

    dinoGfx.fillStyle(silColor, 1);

    // Body (large torso)
    dinoGfx.fillEllipse(cx, cy + 10, 100, 70);

    // Tail — curves left and tapers
    dinoGfx.fillTriangle(
      cx - 50, cy + 10,
      cx - 130, cy - 10,
      cx - 60, cy + 35
    );
    // Tail tip
    dinoGfx.fillTriangle(
      cx - 120, cy - 5,
      cx - 155, cy - 25,
      cx - 125, cy + 10
    );

    // Neck (upward)
    dinoGfx.fillEllipse(cx + 30, cy - 30, 36, 50);

    // Head
    dinoGfx.fillEllipse(cx + 50, cy - 55, 48, 32);

    // Jaw (lower)
    dinoGfx.fillEllipse(cx + 58, cy - 43, 40, 16);

    // Teeth (small triangles along jaw line)
    const teethColor = 0x303050;
    dinoGfx.fillStyle(teethColor, 1);
    for (let i = 0; i < 5; i++) {
      const tx = cx + 42 + i * 7;
      const ty = cy - 48;
      dinoGfx.fillTriangle(tx, ty, tx + 3, ty + 6, tx + 6, ty);
    }

    // Eye socket
    dinoGfx.fillStyle(0x1a1a2e, 1);
    dinoGfx.fillCircle(cx + 58, cy - 58, 6);
    // Glowing eye
    dinoGfx.fillStyle(0xf08030, 0.7);
    dinoGfx.fillCircle(cx + 59, cy - 58, 3);

    // Small arms
    dinoGfx.fillStyle(silColor, 1);
    dinoGfx.fillRect(cx + 10, cy + 5, 8, 18);
    dinoGfx.fillRect(cx + 18, cy + 18, 6, 4);

    // Legs (two thick columns)
    dinoGfx.fillRoundedRect(cx - 20, cy + 30, 22, 50, 4);
    dinoGfx.fillRoundedRect(cx + 8, cy + 30, 22, 50, 4);

    // Feet
    dinoGfx.fillEllipse(cx - 12, cy + 80, 28, 10);
    dinoGfx.fillEllipse(cx + 16, cy + 80, 28, 10);

    // Claws on feet
    dinoGfx.fillStyle(teethColor, 1);
    for (let f = 0; f < 2; f++) {
      const footX = f === 0 ? cx - 12 : cx + 16;
      for (let c = 0; c < 3; c++) {
        const clawX = footX - 8 + c * 8;
        dinoGfx.fillTriangle(clawX, cy + 82, clawX + 3, cy + 88, clawX + 6, cy + 82);
      }
    }

    // Back ridges
    dinoGfx.fillStyle(silColor, 1);
    for (let r = 0; r < 4; r++) {
      const rx = cx - 30 + r * 18;
      const ry = cy - 20 + r * 3;
      dinoGfx.fillTriangle(rx, ry, rx + 6, ry - 12, rx + 12, ry);
    }

    // --- Particles layer (behind text, in front of dino) ---
    this.particleGfx = this.add.graphics().setDepth(5);
    this.initParticles();

    // --- Title Text ---
    this.add.text(cx, 50, 'JURASSIC', {
      fontFamily: FONT_FAMILY,
      fontSize: '48px',
      color: '#F08030',
    }).setOrigin(0.5).setDepth(10);

    this.add.text(cx, 105, 'TRAINERS', {
      fontFamily: FONT_FAMILY,
      fontSize: '32px',
      color: '#F0E8D0',
    }).setOrigin(0.5).setDepth(10);

    // Version
    this.add.text(cx, 132, 'v0.2.0', {
      fontFamily: FONT_FAMILY,
      fontSize: '10px',
      color: '#666666',
    }).setOrigin(0.5).setDepth(10);

    // Press Start blinking text
    this.pressStartText = this.add.text(cx, GAME_HEIGHT - 50, 'APPUYEZ SUR ENTREE', {
      fontFamily: FONT_FAMILY,
      fontSize: '14px',
      color: '#F0E8D0',
    }).setOrigin(0.5).setDepth(10);

    // Menu container (hidden initially)
    this.menuContainer = this.add.container(0, 0).setDepth(20).setVisible(false);

    // Input
    const kb = this.input.keyboard;
    if (kb) {
      this.enterKey = kb.addKey(Phaser.Input.Keyboard.KeyCodes.ENTER);
      this.upKey = kb.addKey(Phaser.Input.Keyboard.KeyCodes.UP);
      this.downKey = kb.addKey(Phaser.Input.Keyboard.KeyCodes.DOWN);
    }
  }

  // ---- Particles ----

  private initParticles(): void {
    this.particles = [];
    for (let i = 0; i < 30; i++) {
      this.particles.push(this.makeParticle());
    }
  }

  private makeParticle(): Particle {
    return {
      x: Phaser.Math.Between(40, GAME_WIDTH - 40),
      y: Phaser.Math.Between(GAME_HEIGHT + 5, GAME_HEIGHT + 60),
      vx: Phaser.Math.FloatBetween(-0.15, 0.15),
      vy: Phaser.Math.FloatBetween(-0.3, -0.8),
      alpha: Phaser.Math.FloatBetween(0.2, 0.6),
      size: Phaser.Math.FloatBetween(1, 3),
      life: 0,
      maxLife: Phaser.Math.Between(300, 700),
    };
  }

  private updateParticles(delta: number): void {
    this.particleGfx.clear();

    for (let i = 0; i < this.particles.length; i++) {
      const p = this.particles[i];
      p.x += p.vx * (delta / 16);
      p.y += p.vy * (delta / 16);
      p.life += delta / 16;

      const lifeRatio = p.life / p.maxLife;
      let a = p.alpha;
      if (lifeRatio > 0.7) {
        a *= 1 - (lifeRatio - 0.7) / 0.3;
      }

      if (p.life >= p.maxLife || p.y < -10) {
        this.particles[i] = this.makeParticle();
        continue;
      }

      // Warm amber/orange colors
      const colors = [0xC89840, 0xE8C868, 0xF08030, 0xF0E8D0];
      const col = colors[i % colors.length];

      this.particleGfx.fillStyle(col, a);
      this.particleGfx.fillCircle(p.x, p.y, p.size);
    }
  }

  // ---- Menu ----

  private showMenu(): void {
    this.menuActive = true;
    this.pressStartText.setVisible(false);

    const options = ['NOUVELLE PARTIE'];
    if (this.hasSave) options.push('CONTINUER');

    // Build amber-bordered panel
    this.menuContainer.removeAll(true);

    const panelW = 320;
    const panelH = options.length * 36 + 28;
    const panelX = GAME_WIDTH / 2 - panelW / 2;
    const panelY = GAME_HEIGHT - panelH - 30;

    const gfx = this.add.graphics();
    // Outer border
    gfx.fillStyle(COLORS.UI_BORDER, 1);
    gfx.fillRoundedRect(panelX, panelY, panelW, panelH, 8);
    // Inner dark
    gfx.fillStyle(COLORS.UI_BG, 0.95);
    gfx.fillRoundedRect(panelX + 3, panelY + 3, panelW - 6, panelH - 6, 6);
    // Inner highlight border
    gfx.lineStyle(1, COLORS.UI_BORDER_LIGHT, 0.3);
    gfx.strokeRoundedRect(panelX + 5, panelY + 5, panelW - 10, panelH - 10, 5);
    this.menuContainer.add(gfx);

    this.menuTexts = [];
    for (let i = 0; i < options.length; i++) {
      const t = this.add.text(
        GAME_WIDTH / 2,
        panelY + 16 + i * 36,
        '',
        {
          fontFamily: FONT_FAMILY,
          fontSize: '14px',
          color: '#F0E8D0',
        }
      ).setOrigin(0.5, 0);
      this.menuContainer.add(t);
      this.menuTexts.push(t);
    }

    this.selectedIndex = 0;
    this.updateMenuHighlight();
    this.menuContainer.setVisible(true);

    // Slide-in from bottom
    this.menuContainer.setY(40);
    this.tweens.add({
      targets: this.menuContainer,
      y: 0,
      duration: 300,
      ease: 'Back.easeOut',
    });
  }

  private updateMenuHighlight(): void {
    const options = ['NOUVELLE PARTIE'];
    if (this.hasSave) options.push('CONTINUER');

    this.menuTexts.forEach((t, i) => {
      const prefix = i === this.selectedIndex ? '\u25b6 ' : '  ';
      t.setText(prefix + options[i]);
      t.setColor(i === this.selectedIndex ? '#F08030' : '#F0E8D0');
    });
  }

  private selectOption(): void {
    const options = ['NOUVELLE PARTIE'];
    if (this.hasSave) options.push('CONTINUER');
    const chosen = options[this.selectedIndex];

    if (chosen === 'NOUVELLE PARTIE') {
      AudioSystem.getInstance().stopMusic();
      this.cameras.main.fadeOut(500, 0, 0, 0);
      this.cameras.main.once('camerafadeoutcomplete', () => {
        this.scene.start(SCENE_KEYS.STARTER_SELECT);
      });
    } else if (chosen === 'CONTINUER') {
      AudioSystem.getInstance().stopMusic();
      // Load saved game from slot 0
      const gs = GameState.getInstance();
      const loaded = gs.loadGame(0);
      if (loaded) {
        this.cameras.main.fadeOut(500, 0, 0, 0);
        this.cameras.main.once('camerafadeoutcomplete', () => {
          this.scene.start(SCENE_KEYS.OVERWORLD, {
            hasStarter: true,
            mapId: gs.player.mapId,
            playerX: gs.player.x,
            playerY: gs.player.y,
          });
        });
      }
    }
  }

  // ---- Update ----

  update(_time: number, delta: number): void {
    this.updateParticles(delta);

    if (!this.menuActive) {
      // Blink
      this.blinkTimer += delta;
      if (this.blinkTimer >= 500) {
        this.blinkTimer = 0;
        this.pressStartText.setVisible(!this.pressStartText.visible);
      }

      if (this.enterKey && Phaser.Input.Keyboard.JustDown(this.enterKey)) {
        this.showMenu();
      }
    } else {
      if (this.upKey && Phaser.Input.Keyboard.JustDown(this.upKey)) {
        this.selectedIndex = (this.selectedIndex - 1 + this.menuTexts.length) % this.menuTexts.length;
        this.updateMenuHighlight();
      }
      if (this.downKey && Phaser.Input.Keyboard.JustDown(this.downKey)) {
        this.selectedIndex = (this.selectedIndex + 1) % this.menuTexts.length;
        this.updateMenuHighlight();
      }
      if (this.enterKey && Phaser.Input.Keyboard.JustDown(this.enterKey)) {
        this.selectOption();
      }
    }
  }
}
