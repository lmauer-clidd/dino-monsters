import Phaser from 'phaser';
import { GAME_WIDTH, GAME_HEIGHT } from '../utils/constants';

// ============================================================
// CameraFX — Camera effects utility
// Smooth shake, zoom punch, letterbox, color flash
// All methods return Promises for easy chaining
// ============================================================

export class CameraFX {
  // ----------------------------------------------------------
  // Perlin-noise-based smooth shake
  // ----------------------------------------------------------

  /**
   * Smooth camera shake using layered sine waves (approximates Perlin noise).
   * Much smoother than Phaser's built-in random jitter shake.
   *
   * @param scene     The active scene
   * @param intensity Max pixel offset (e.g. 4 for light, 8 for heavy)
   * @param duration  Total shake duration in ms
   */
  static smoothShake(
    scene: Phaser.Scene,
    intensity: number = 4,
    duration: number = 300
  ): Promise<void> {
    return new Promise<void>((resolve) => {
      const camera = scene.cameras.main;
      const startX = camera.scrollX;
      const startY = camera.scrollY;
      let elapsed = 0;

      // Use multiple sine frequencies for organic motion
      const freqX1 = 0.015 + Math.random() * 0.005;
      const freqX2 = 0.033 + Math.random() * 0.007;
      const freqY1 = 0.017 + Math.random() * 0.005;
      const freqY2 = 0.029 + Math.random() * 0.007;
      const phaseX = Math.random() * Math.PI * 2;
      const phaseY = Math.random() * Math.PI * 2;

      const event = scene.time.addEvent({
        delay: 16, // ~60fps
        loop: true,
        callback: () => {
          elapsed += 16;
          const t = elapsed / duration; // 0..1+

          if (t >= 1) {
            camera.scrollX = startX;
            camera.scrollY = startY;
            event.remove();
            resolve();
            return;
          }

          // Envelope: ramp up quickly then decay
          const envelope = Math.sin(t * Math.PI); // 0->1->0

          // Layered sine noise
          const offsetX = (
            Math.sin(elapsed * freqX1 + phaseX) * 0.6 +
            Math.sin(elapsed * freqX2 + phaseX * 1.7) * 0.4
          ) * intensity * envelope;

          const offsetY = (
            Math.sin(elapsed * freqY1 + phaseY) * 0.6 +
            Math.sin(elapsed * freqY2 + phaseY * 1.3) * 0.4
          ) * intensity * envelope;

          camera.scrollX = startX + offsetX;
          camera.scrollY = startY + offsetY;
        },
      });
    });
  }

  // ----------------------------------------------------------
  // Zoom punch (for criticals / super effective)
  // ----------------------------------------------------------

  /**
   * Quick zoom in then back to normal. Great for impact emphasis.
   *
   * @param scene    The active scene
   * @param scale    Peak zoom level (e.g. 1.08 for subtle, 1.2 for dramatic)
   * @param duration Total duration in ms (zoom in takes 40%, hold 10%, zoom out 50%)
   */
  static zoomPunch(
    scene: Phaser.Scene,
    scale: number = 1.1,
    duration: number = 250
  ): Promise<void> {
    return new Promise<void>((resolve) => {
      const camera = scene.cameras.main;
      const zoomInDur = duration * 0.4;
      const holdDur = duration * 0.1;
      const zoomOutDur = duration * 0.5;

      scene.tweens.add({
        targets: camera,
        zoom: scale,
        duration: zoomInDur,
        ease: 'Quad.easeOut',
        onComplete: () => {
          scene.time.delayedCall(holdDur, () => {
            scene.tweens.add({
              targets: camera,
              zoom: 1,
              duration: zoomOutDur,
              ease: 'Sine.easeInOut',
              onComplete: () => resolve(),
            });
          });
        },
      });
    });
  }

  // ----------------------------------------------------------
  // Letterbox (cinematic bars)
  // ----------------------------------------------------------

  /**
   * Show or hide cinematic letterbox bars (top and bottom).
   *
   * @param scene    The active scene
   * @param show     true to show bars, false to hide
   * @param duration Transition duration in ms
   * @param barSize  Bar height in pixels (default 40)
   * @returns Promise that resolves when transition completes.
   *          When showing, also returns a cleanup function to call later.
   */
  static letterbox(
    scene: Phaser.Scene,
    show: boolean,
    duration: number = 300,
    barSize: number = 40
  ): Promise<void> {
    return new Promise<void>((resolve) => {
      // Reuse existing bars or create new ones
      const key = '__cameraFX_letterbox';
      let data = (scene as any)[key] as {
        top: Phaser.GameObjects.Graphics;
        bottom: Phaser.GameObjects.Graphics;
      } | undefined;

      if (!data) {
        const top = scene.add.graphics().setDepth(1999).setScrollFactor(0);
        const bottom = scene.add.graphics().setDepth(1999).setScrollFactor(0);

        top.fillStyle(0x000000, 1);
        top.fillRect(0, 0, GAME_WIDTH, barSize);
        top.setY(-barSize);

        bottom.fillStyle(0x000000, 1);
        bottom.fillRect(0, 0, GAME_WIDTH, barSize);
        bottom.setY(GAME_HEIGHT);

        data = { top, bottom };
        (scene as any)[key] = data;
      }

      if (show) {
        // Slide bars in
        scene.tweens.add({
          targets: data.top,
          y: 0,
          duration,
          ease: 'Quad.easeInOut',
        });
        scene.tweens.add({
          targets: data.bottom,
          y: GAME_HEIGHT - barSize,
          duration,
          ease: 'Quad.easeInOut',
          onComplete: () => resolve(),
        });
      } else {
        // Slide bars out
        scene.tweens.add({
          targets: data.top,
          y: -barSize,
          duration,
          ease: 'Quad.easeInOut',
        });
        scene.tweens.add({
          targets: data.bottom,
          y: GAME_HEIGHT,
          duration,
          ease: 'Quad.easeInOut',
          onComplete: () => {
            data!.top.destroy();
            data!.bottom.destroy();
            delete (scene as any)[key];
            resolve();
          },
        });
      }
    });
  }

  // ----------------------------------------------------------
  // Flash color
  // ----------------------------------------------------------

  /**
   * Full-screen color flash that fades out.
   *
   * @param scene    The active scene
   * @param color    Flash color (hex, e.g. 0xFFFFFF)
   * @param duration Fade-out duration in ms
   */
  static flashColor(
    scene: Phaser.Scene,
    color: number = 0xFFFFFF,
    duration: number = 150
  ): Promise<void> {
    return new Promise<void>((resolve) => {
      const r = (color >> 16) & 0xff;
      const g = (color >> 8) & 0xff;
      const b = color & 0xff;

      scene.cameras.main.flash(duration, r, g, b, false, (_cam: Phaser.Cameras.Scene2D.Camera, progress: number) => {
        if (progress >= 1) {
          resolve();
        }
      });
    });
  }
}
