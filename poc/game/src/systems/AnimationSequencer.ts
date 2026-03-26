import Phaser from 'phaser';

// ============================================================
// AnimationSequencer — Chainable tween sequencer
// Wraps Phaser's tween manager for sequential/parallel animations
// ============================================================

type StepType = 'tween' | 'delay' | 'call' | 'parallel';

interface TweenStep {
  type: 'tween';
  config: Phaser.Types.Tweens.TweenBuilderConfig;
}

interface DelayStep {
  type: 'delay';
  ms: number;
}

interface CallStep {
  type: 'call';
  fn: () => void;
}

interface ParallelStep {
  type: 'parallel';
  configs: Phaser.Types.Tweens.TweenBuilderConfig[];
}

type Step = TweenStep | DelayStep | CallStep | ParallelStep;

export class AnimationSequencer {
  private scene: Phaser.Scene;
  private steps: Step[] = [];

  constructor(scene: Phaser.Scene) {
    this.scene = scene;
  }

  /**
   * Add a tween to the sequence (runs after the previous step completes).
   */
  tween(config: Phaser.Types.Tweens.TweenBuilderConfig): this {
    this.steps.push({ type: 'tween', config });
    return this;
  }

  /**
   * Add a pause (hitstop) between steps.
   */
  delay(ms: number): this {
    this.steps.push({ type: 'delay', ms });
    return this;
  }

  /**
   * Add a callback between steps.
   */
  call(fn: () => void): this {
    this.steps.push({ type: 'call', fn });
    return this;
  }

  /**
   * Run multiple tweens simultaneously; the step completes when all finish.
   */
  parallel(configs: Phaser.Types.Tweens.TweenBuilderConfig[]): this {
    this.steps.push({ type: 'parallel', configs });
    return this;
  }

  /**
   * Start the sequence. Returns a Promise that resolves when every step is done.
   */
  play(): Promise<void> {
    return new Promise<void>((resolve) => {
      this.runStep(0, resolve);
    });
  }

  // ----------------------------------------------------------
  // Internal runner
  // ----------------------------------------------------------

  private runStep(index: number, done: () => void): void {
    if (index >= this.steps.length) {
      done();
      return;
    }

    const step = this.steps[index];

    switch (step.type) {
      case 'tween':
        this.runTween(step.config, () => this.runStep(index + 1, done));
        break;

      case 'delay':
        this.scene.time.delayedCall(step.ms, () => this.runStep(index + 1, done));
        break;

      case 'call':
        step.fn();
        this.runStep(index + 1, done);
        break;

      case 'parallel':
        this.runParallel(step.configs, () => this.runStep(index + 1, done));
        break;
    }
  }

  private runTween(
    config: Phaser.Types.Tweens.TweenBuilderConfig,
    onComplete: () => void
  ): void {
    const original = config.onComplete;
    this.scene.tweens.add({
      ...config,
      onComplete: (tween, targets, param) => {
        if (original) {
          (original as Function)(tween, targets, param);
        }
        onComplete();
      },
    });
  }

  private runParallel(
    configs: Phaser.Types.Tweens.TweenBuilderConfig[],
    onComplete: () => void
  ): void {
    if (configs.length === 0) {
      onComplete();
      return;
    }

    let remaining = configs.length;
    const finish = () => {
      remaining--;
      if (remaining <= 0) onComplete();
    };

    for (const config of configs) {
      this.runTween(config, finish);
    }
  }

  // ----------------------------------------------------------
  // Static presets — reusable animation patterns
  // ----------------------------------------------------------

  /**
   * Attack lunge: quick forward movement then snap back.
   */
  static attackLunge(
    scene: Phaser.Scene,
    target: Phaser.GameObjects.GameObject,
    direction: number = 1,
    distance: number = 12
  ): AnimationSequencer {
    return new AnimationSequencer(scene)
      .tween({
        targets: target,
        x: `+=${direction * distance}`,
        duration: 100,
        ease: 'Power2',
      })
      .tween({
        targets: target,
        x: `-=${direction * distance}`,
        duration: 150,
        ease: 'Sine.easeOut',
      });
  }

  /**
   * Hit flash: rapid alpha blinking (3x).
   */
  static hitFlash(
    scene: Phaser.Scene,
    target: Phaser.GameObjects.GameObject
  ): AnimationSequencer {
    return new AnimationSequencer(scene)
      .tween({
        targets: target,
        alpha: 0.3,
        duration: 80,
        yoyo: true,
        repeat: 2,
      });
  }

  /**
   * Faint: sprite sinks downward and fades out.
   */
  static faint(
    scene: Phaser.Scene,
    target: Phaser.GameObjects.GameObject
  ): AnimationSequencer {
    return new AnimationSequencer(scene)
      .tween({
        targets: target,
        y: '+=30',
        alpha: 0,
        duration: 500,
        ease: 'Power2',
      });
  }

  /**
   * Entrance slide: target slides in from off-screen.
   */
  static slideIn(
    scene: Phaser.Scene,
    target: Phaser.GameObjects.GameObject & { x: number },
    fromX: number,
    toX: number,
    duration: number = 400
  ): AnimationSequencer {
    (target as any).x = fromX;
    return new AnimationSequencer(scene)
      .tween({
        targets: target,
        x: toX,
        duration,
        ease: 'Back.easeOut',
      });
  }

  /**
   * Bounce: quick scale pulse (for level-ups, captures, etc.).
   */
  static bounce(
    scene: Phaser.Scene,
    target: Phaser.GameObjects.GameObject
  ): AnimationSequencer {
    return new AnimationSequencer(scene)
      .tween({
        targets: target,
        scaleX: 1.2,
        scaleY: 1.2,
        duration: 120,
        ease: 'Quad.easeOut',
        yoyo: true,
      });
  }
}
