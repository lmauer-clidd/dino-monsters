// ============================================================
// Jurassic Trainers -- Narrative Event System
// Manages story events: triggers, actions, flag tracking
// ============================================================

import Phaser from 'phaser';
import { GameState } from './GameState';
import { DialogueBox } from '../ui/DialogueBox';
import { TransitionFX } from '../ui/TransitionFX';
import { SCENE_KEYS } from '../utils/constants';
import {
  StoryEvent,
  EventAction,
  STORY_EVENTS,
  getMapEnterEvents,
  getTileStepEvents,
  getNpcInteractEvents,
  getBadgeCountEvents,
  getFlagEvents,
} from '../data/events';
import { getTrainer, createRivalEncounter, TrainerData } from '../data/trainers';
import { DinoType } from '../utils/constants';

// ===================== Event System =====================

export class EventSystem {
  private scene: Phaser.Scene;
  private dialogue: DialogueBox;
  private gameState: GameState;
  private executing = false;
  private eventQueue: StoryEvent[] = [];

  constructor(scene: Phaser.Scene, dialogue: DialogueBox) {
    this.scene = scene;
    this.dialogue = dialogue;
    this.gameState = GameState.getInstance();
  }

  // ===================== Public API =====================

  /** Check if an event is currently executing */
  isExecuting(): boolean {
    return this.executing;
  }

  /** Check for map_enter events when the player enters a new map */
  checkMapEnterEvents(mapId: string): StoryEvent | null {
    const events = getMapEnterEvents(mapId);
    return this.findEligibleEvent(events);
  }

  /** Check for tile_step events when the player steps on a tile */
  checkTileEvents(mapId: string, x: number, y: number): StoryEvent | null {
    const events = getTileStepEvents(mapId, x, y);
    return this.findEligibleEvent(events);
  }

  /** Check for npc_interact events when player talks to an NPC */
  checkNpcEvents(npcId: string): StoryEvent | null {
    const events = getNpcInteractEvents(npcId);
    return this.findEligibleEvent(events);
  }

  /** Check for badge_count events after obtaining a badge */
  checkBadgeEvents(badgeCount: number): StoryEvent | null {
    const events = getBadgeCountEvents(badgeCount);
    return this.findEligibleEvent(events);
  }

  /** Check for flag-triggered events when a flag changes */
  checkFlagEvents(flag: string): StoryEvent | null {
    const events = getFlagEvents(flag);
    return this.findEligibleEvent(events);
  }

  /**
   * Execute a story event by processing its actions sequentially.
   * Returns a promise that resolves when all actions are complete.
   */
  async executeEvent(event: StoryEvent): Promise<void> {
    if (this.executing) {
      // Queue the event if one is already running
      this.eventQueue.push(event);
      return;
    }

    this.executing = true;

    try {
      // Pre-scan: if the event contains a 'battle' action, pre-execute critical
      // post-battle actions (setFlag, giveItem) BEFORE starting. This prevents
      // infinite loops when scene.start() destroys the overworld and the event
      // system can never reach its post-battle actions.
      const hasBattle = event.actions.some(a => a.type === 'battle');
      if (hasBattle) {
        const lastBattleIdx = event.actions.reduce((last, a, i) => a.type === 'battle' ? i : last, -1);
        for (let i = lastBattleIdx + 1; i < event.actions.length; i++) {
          const a = event.actions[i];
          if (a.type === 'setFlag') {
            this.gameState.setFlag(a.flag, a.value);
          } else if (a.type === 'giveItem') {
            this.gameState.getInventorySystem().addItem(a.itemId, a.quantity);
          }
        }
      }

      for (const action of event.actions) {
        await this.executeAction(action);
      }
    } catch (err) {
      console.error(`[EventSystem] Error executing event ${event.id}:`, err);
    }

    this.executing = false;

    // Process queued events
    if (this.eventQueue.length > 0) {
      const next = this.eventQueue.shift()!;
      await this.executeEvent(next);
    }
  }

  // ===================== Eligibility Check =====================

  /**
   * Find the first eligible event from a list.
   * An event is eligible if:
   *  - All requiredFlags are true in GameState
   *  - No blockedByFlags are true in GameState
   */
  private findEligibleEvent(events: StoryEvent[]): StoryEvent | null {
    for (const event of events) {
      if (this.isEventEligible(event)) {
        return event;
      }
    }
    return null;
  }

  private isEventEligible(event: StoryEvent): boolean {
    // Check required flags
    if (event.requiredFlags) {
      for (const flag of event.requiredFlags) {
        if (!this.gameState.hasFlag(flag)) {
          return false;
        }
      }
    }

    // Check blocked flags
    if (event.blockedByFlags) {
      for (const flag of event.blockedByFlags) {
        if (this.gameState.hasFlag(flag)) {
          return false;
        }
      }
    }

    return true;
  }

  // ===================== Action Execution =====================

  private async executeAction(action: EventAction): Promise<void> {
    switch (action.type) {
      case 'dialogue':
        return this.actionDialogue(action.speaker, action.text);

      case 'choice':
        return this.actionChoice(action.prompt, action.choices, action.resultFlags);

      case 'battle':
        return this.actionBattle(action.trainerId);

      case 'setFlag':
        return this.actionSetFlag(action.flag, action.value);

      case 'heal':
        return this.actionHeal();

      case 'giveItem':
        return this.actionGiveItem(action.itemId, action.quantity);

      case 'giveDino':
        return this.actionGiveDino(action.speciesId, action.level);

      case 'screenEffect':
        return this.actionScreenEffect(action.effect);

      case 'moveNpc':
        return this.actionMoveNpc(action.npcId, action.direction, action.tiles);

      case 'wait':
        return this.actionWait(action.ms);

      case 'teleport':
        return this.actionTeleport(action.mapId, action.x, action.y);

      default:
        console.warn(`[EventSystem] Unknown action type: ${(action as EventAction).type}`);
    }
  }

  // --- Dialogue ---
  private actionDialogue(speaker: string, text: string): Promise<void> {
    return new Promise<void>((resolve) => {
      // Replace dynamic placeholders
      const processedText = this.processText(text);
      const speakerName = speaker || undefined;
      this.dialogue.showText(processedText, () => resolve(), speakerName);
    });
  }

  // --- Choice ---
  private actionChoice(prompt: string, choices: string[], resultFlags: string[]): Promise<void> {
    return new Promise<void>((resolve) => {
      const processedPrompt = this.processText(prompt);
      this.dialogue.showChoices(processedPrompt, choices, (selectedIndex: number) => {
        // Set the corresponding flag for the selected choice
        if (resultFlags[selectedIndex]) {
          this.gameState.setFlag(resultFlags[selectedIndex], true);
        }
        resolve();
      });
    });
  }

  // --- Battle ---
  private actionBattle(trainerId: string): Promise<void> {
    return new Promise<void>((resolve) => {
      // Store resolve callback so the overworld scene can call it when battle ends
      (this.scene as any).__eventBattleResolve = resolve;
      (this.scene as any).__eventBattleTrainerId = trainerId;

      // Resolve trainer data so BattleScene gets the correct party
      const trainerData = this.resolveTrainer(trainerId);

      TransitionFX.battleTransition(this.scene, () => {
        this.scene.scene.start(SCENE_KEYS.BATTLE, {
          isWild: false,
          trainerId: trainerId,
          trainerName: trainerData?.name,
          trainerClass: trainerData?.trainerClass,
          trainerParty: trainerData?.party ?? [],
          trainerDialogueAfter: trainerData?.dialogue?.after,
          reward: trainerData?.reward ?? 0,
          badge: trainerData?.badge,
          tmReward: trainerData?.tmReward,
          isStoryEvent: true,
          returnScene: SCENE_KEYS.OVERWORLD,
          returnData: {
            mapId: this.gameState.currentMap,
            playerX: this.gameState.player.x,
            playerY: this.gameState.player.y,
            hasStarter: true,
            resumeEvent: true,
          },
        });
      });

      // The battle system will mark the trainer as defeated and return.
      // On battle return, OverworldScene should call __eventBattleResolve().
      // As a fallback, we listen for the scene resume event.
      const resumeHandler = () => {
        this.scene.events.off('resume', resumeHandler);
        // Small delay to let battle results propagate
        this.scene.time.delayedCall(100, () => {
          const resolveFn = (this.scene as any).__eventBattleResolve;
          if (resolveFn) {
            delete (this.scene as any).__eventBattleResolve;
            delete (this.scene as any).__eventBattleTrainerId;
            resolveFn();
          }
        });
      };

      this.scene.events.on('resume', resumeHandler);
    });
  }

  /** Resolve a trainerId to full TrainerData, handling rival encounters dynamically */
  private resolveTrainer(trainerId: string): TrainerData | undefined {
    // Check if this is a rival encounter — needs dynamic party based on player's starter
    const rivalMatch = trainerId.match(/^RIVAL_REX_(\d+|FINAL|POSTGAME)$/i);
    if (rivalMatch) {
      const encounterMap: Record<string, number> = {
        '1': 1, '2': 2, '3': 3, '4': 4, '5': 5, 'FINAL': 6, 'POSTGAME': 6,
      };
      const encounter = encounterMap[rivalMatch[1].toUpperCase()] ?? 1;

      // Determine player's starter type from party
      const party = this.gameState.party;
      let starterType: 'fire' | 'water' | 'flora' = 'fire';
      if (party.length > 0) {
        const firstType = party[0].type1;
        if (firstType === DinoType.Water) starterType = 'water';
        else if (firstType === DinoType.Flora) starterType = 'flora';
        else starterType = 'fire';
      }

      return createRivalEncounter(encounter, starterType);
    }

    // Regular trainer lookup
    return getTrainer(trainerId);
  }

  // --- Set Flag ---
  private actionSetFlag(flag: string, value: boolean): Promise<void> {
    this.gameState.setFlag(flag, value);

    // After setting a flag, check for flag-triggered events
    if (value) {
      const flagEvent = this.checkFlagEvents(flag);
      if (flagEvent) {
        this.eventQueue.push(flagEvent);
      }
    }

    return Promise.resolve();
  }

  // --- Heal ---
  private actionHeal(): Promise<void> {
    // Heal all dinos in party
    const party = this.gameState.getPartySystem();
    for (const dino of party.getParty()) {
      dino.currentHp = dino.maxHp;
      // Reset status if the dino has one
      if ('status' in dino) {
        (dino as any).status = 'none';
      }
      // Restore PP for all moves
      if (dino.moves) {
        for (const move of dino.moves) {
          move.currentPP = move.maxPP;
        }
      }
    }
    return Promise.resolve();
  }

  // --- Give Item ---
  private actionGiveItem(itemId: number, quantity: number): Promise<void> {
    return new Promise<void>((resolve) => {
      this.gameState.getInventorySystem().addItem(itemId, quantity);
      // Show a brief notification via dialogue
      const itemName = this.getItemName(itemId);
      const msg = quantity > 1
        ? `Tu as recu ${quantity} ${itemName} !`
        : `Tu as recu un(e) ${itemName} !`;
      this.dialogue.showText(msg, () => resolve());
    });
  }

  // --- Give Dino ---
  private actionGiveDino(speciesId: number, level: number): Promise<void> {
    return new Promise<void>((resolve) => {
      // Import Dino class dynamically to avoid circular deps
      try {
        const { Dino } = require('../entities/Dino');
        const dino = Dino.createStarter(speciesId, level);
        this.gameState.getPartySystem().addToParty(dino);
        this.gameState.catchDino(speciesId);
        this.dialogue.showText(`Un nouveau dino a rejoint ton equipe !`, () => resolve());
      } catch (err) {
        console.error('[EventSystem] Failed to give dino:', err);
        resolve();
      }
    });
  }

  // --- Screen Effect ---
  private actionScreenEffect(effect: 'flash' | 'shake' | 'fadeOut' | 'fadeIn'): Promise<void> {
    return new Promise<void>((resolve) => {
      switch (effect) {
        case 'flash':
          this.scene.cameras.main.flash(300, 255, 255, 255, false, (_cam: any, progress: number) => {
            if (progress >= 1) resolve();
          });
          break;

        case 'shake':
          this.scene.cameras.main.shake(400, 0.01, false, (_cam: any, progress: number) => {
            if (progress >= 1) resolve();
          });
          break;

        case 'fadeOut':
          TransitionFX.fadeOut(this.scene, 500, () => resolve());
          break;

        case 'fadeIn':
          TransitionFX.fadeIn(this.scene, 500, () => resolve());
          break;

        default:
          resolve();
      }
    });
  }

  // --- Move NPC ---
  private actionMoveNpc(npcId: string, direction: string, tiles: number): Promise<void> {
    // NPC movement is visual-only; the OverworldScene handles actual sprite movement.
    // We emit an event that the OverworldScene can listen to.
    return new Promise<void>((resolve) => {
      this.scene.events.emit('event:moveNpc', { npcId, direction, tiles });
      // Estimate movement time: 150ms per tile
      const duration = tiles * 150;
      this.scene.time.delayedCall(duration, () => resolve());
    });
  }

  // --- Wait ---
  private actionWait(ms: number): Promise<void> {
    return new Promise<void>((resolve) => {
      this.scene.time.delayedCall(ms, () => resolve());
    });
  }

  // --- Teleport ---
  private actionTeleport(mapId: string, x: number, y: number): Promise<void> {
    return new Promise<void>((resolve) => {
      this.gameState.setPlayerPosition(x, y, mapId);
      TransitionFX.fadeOut(this.scene, 400, () => {
        this.scene.scene.restart({
          hasStarter: true,
          mapId: mapId,
          playerX: x,
          playerY: y,
        });
        resolve();
      });
    });
  }

  // ===================== Text Processing =====================

  /**
   * Replace placeholders in event text with runtime values.
   * Supported: [JOUEUR], [RIVAL_STARTER]
   */
  private processText(text: string): string {
    let result = text;
    result = result.replace(/\[JOUEUR\]/g, this.gameState.player.name || 'Dresseur');

    // Determine rival starter based on player's starter
    const rivalStarter = this.getRivalStarterName();
    result = result.replace(/\[RIVAL_STARTER\]/g, rivalStarter);

    return result;
  }

  /** Get the name of the rival's starter based on the player's choice */
  private getRivalStarterName(): string {
    // Player chose Pyrex (Fire) -> Rival has Aquadon (Water)
    // Player chose Aquadon (Water) -> Rival has Florasaur (Flora)
    // Player chose Florasaur (Flora) -> Rival has Pyrex (Fire)
    // This logic should match the starter selection
    const party = this.gameState.getPartySystem().getParty();
    if (party.length === 0) return 'son dino';

    const starterName = party[0]?.nickname?.toLowerCase() || '';
    if (starterName.includes('pyrex')) return 'Aquadon';
    if (starterName.includes('aquadon')) return 'Florasaur';
    if (starterName.includes('florasaur')) return 'Pyrex';
    return 'son dino';
  }

  // ===================== Item Names =====================

  private getItemName(itemId: number): string {
    const ITEM_NAMES: Record<number, string> = {
      1: 'Potion',
      2: 'Jurassic Ball',
      3: 'Super Potion',
      4: 'Super Ball',
      5: 'Hyper Potion',
      6: 'Hyper Ball',
      7: 'Master Ball',
      8: 'Rappel',
      9: 'Antidote',
      10: 'Fossile-Radar',
    };
    return ITEM_NAMES[itemId] || 'Objet';
  }

  // ===================== Cleanup =====================

  destroy(): void {
    this.eventQueue = [];
    this.executing = false;
  }
}
