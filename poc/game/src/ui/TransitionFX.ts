import Phaser from 'phaser';
import { GAME_WIDTH, GAME_HEIGHT, COLORS } from '../utils/constants';

export class TransitionFX {
  /**
   * Battle transition:
   * 1. Screen flashes white 3 times (100ms each)
   * 2. Closing iris/circle wipe to black
   * 3. Brief black pause (200ms)
   * 4. Scene switch via callback
   */
  static battleTransition(scene: Phaser.Scene, callback: () => void): void {
    const overlay = scene.add.graphics().setDepth(2000).setScrollFactor(0);
    let flashCount = 0;
    const totalFlashes = 3;

    const doFlash = () => {
      // Flash white
      overlay.clear();
      overlay.fillStyle(COLORS.WHITE, 0.9);
      overlay.fillRect(0, 0, GAME_WIDTH, GAME_HEIGHT);

      scene.time.delayedCall(100, () => {
        overlay.clear();
        flashCount++;
        if (flashCount < totalFlashes) {
          scene.time.delayedCall(100, doFlash);
        } else {
          // After flashes, do iris wipe
          doIrisWipe(scene, overlay, callback);
        }
      });
    };

    doFlash();
  }

  /**
   * Fade the screen from black to transparent.
   */
  static fadeIn(scene: Phaser.Scene, duration = 400, callback?: () => void): void {
    const r = (COLORS.BLACK >> 16) & 0xff;
    const g = (COLORS.BLACK >> 8) & 0xff;
    const b = COLORS.BLACK & 0xff;
    scene.cameras.main.fadeIn(duration, r, g, b);
    if (callback) {
      scene.cameras.main.once('camerafadeincomplete', callback);
    }
  }

  /**
   * Fade the screen from transparent to black.
   */
  static fadeOut(scene: Phaser.Scene, duration = 400, callback?: () => void): void {
    const r = (COLORS.BLACK >> 16) & 0xff;
    const g = (COLORS.BLACK >> 8) & 0xff;
    const b = COLORS.BLACK & 0xff;
    scene.cameras.main.fadeOut(duration, r, g, b);
    if (callback) {
      scene.cameras.main.once('camerafadeoutcomplete', callback);
    }
  }

  /**
   * Quick full-screen flash with configurable color and alpha tween.
   */
  static flashScreen(
    scene: Phaser.Scene,
    color: number = COLORS.WHITE,
    duration = 150,
    callback?: () => void
  ): void {
    const overlay = scene.add.graphics().setDepth(2000).setScrollFactor(0);
    overlay.fillStyle(color, 1);
    overlay.fillRect(0, 0, GAME_WIDTH, GAME_HEIGHT);
    overlay.setAlpha(1);

    scene.tweens.add({
      targets: overlay,
      alpha: 0,
      duration,
      ease: 'Power2',
      onComplete: () => {
        overlay.destroy();
        callback?.();
      },
    });
  }
}

/**
 * Closing iris wipe: a shrinking circle reveals black underneath.
 * The circle starts large (covering the whole screen) and shrinks to 0.
 */
function doIrisWipe(
  scene: Phaser.Scene,
  overlay: Phaser.GameObjects.Graphics,
  callback: () => void
): void {
  const cx = GAME_WIDTH / 2;
  const cy = GAME_HEIGHT / 2;
  // Max radius: from center to corner
  const maxRadius = Math.sqrt(cx * cx + cy * cy);
  const steps = 20;
  let step = 0;

  // We use a mask approach: draw black everywhere except a shrinking circle.
  // Since Phaser Graphics doesn't have easy inverse circles, we draw the black
  // border by covering the screen with black and cutting a circle with blending,
  // or more practically, draw black rectangles + approximate the circle with a ring.

  scene.time.addEvent({
    delay: 25,
    repeat: steps - 1,
    callback: () => {
      step++;
      const progress = step / steps;
      const radius = maxRadius * (1 - progress);

      overlay.clear();

      // Fill entire screen black
      overlay.fillStyle(COLORS.BLACK, 1);
      overlay.fillRect(0, 0, GAME_WIDTH, GAME_HEIGHT);

      if (radius > 1) {
        // "Cut out" a circle by drawing a filled circle with the scene background color
        // This simulates the iris effect. We draw a circle that reveals through.
        // Use a RenderTexture approach or approximate with arc segments.

        // Create a mask: draw concentric ring to approximate hole
        // For simplicity, use the stencil approach with a geometry mask
        const maskShape = scene.add.graphics().setVisible(false);
        maskShape.fillStyle(0xffffff, 1);
        maskShape.fillCircle(cx, cy, radius);

        const mask = maskShape.createGeometryMask();
        mask.invertAlpha = true;

        overlay.setMask(mask);

        // Clean up previous frame mask on next tick
        scene.time.delayedCall(20, () => {
          overlay.clearMask(true);
          maskShape.destroy();
        });
      }

      if (step >= steps) {
        // Fully black
        overlay.clearMask(true);
        overlay.clear();
        overlay.fillStyle(COLORS.BLACK, 1);
        overlay.fillRect(0, 0, GAME_WIDTH, GAME_HEIGHT);

        scene.time.delayedCall(200, () => {
          overlay.destroy();
          callback();
        });
      }
    },
  });
}
