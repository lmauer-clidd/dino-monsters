// ============================================================
// Jurassic Trainers -- Save System
// ============================================================

import { SAVE_KEY_PREFIX, MAX_SAVE_SLOTS } from '../utils/constants';
import { IDinoSerialized } from '../entities/Dino';

// ===================== Interfaces =====================

export interface IGameState {
  version: number;
  player: IPlayerData;
  party: IDinoSerialized[];
  storage: IDinoSerialized[];
  inventory: Record<number, number>;     // itemId -> quantity
  dinodex: Record<number, IDinodexEntry>;
  badges: number[];                       // badge indices collected
  flags: Record<string, boolean>;         // story progress flags
  defeatedTrainers: string[];             // trainer IDs
  playtime: number;                       // seconds
  money: number;
  savedAt: string;                        // ISO timestamp
}

export interface IPlayerData {
  name: string;
  x: number;
  y: number;
  mapId: string;
  facing: string;
}

export interface IDinodexEntry {
  seen: boolean;
  caught: boolean;
}

export interface ISaveSlotInfo {
  exists: boolean;
  name: string;
  badges: number;
  playtime: number;
  partyLevel: number;   // highest level in party
  partySize: number;
  savedAt: string;
}

// ===================== Constants =====================

const SAVE_VERSION = 1;

// ===================== Save System =====================

export class SaveSystem {

  /**
   * Save the game state to a slot (0-indexed, 0 to MAX_SAVE_SLOTS-1).
   */
  save(slot: number, gameState: IGameState): boolean {
    if (!this.isValidSlot(slot)) return false;

    try {
      gameState.version = SAVE_VERSION;
      gameState.savedAt = new Date().toISOString();
      const key = this.getKey(slot);
      const json = JSON.stringify(gameState);
      localStorage.setItem(key, json);
      return true;
    } catch (e) {
      console.error('Save failed:', e);
      return false;
    }
  }

  /**
   * Load a game state from a slot. Returns null if no save exists.
   */
  load(slot: number): IGameState | null {
    if (!this.isValidSlot(slot)) return null;

    try {
      const key = this.getKey(slot);
      const json = localStorage.getItem(key);
      if (!json) return null;

      const data = JSON.parse(json) as IGameState;

      // Version migration could happen here
      if (data.version !== SAVE_VERSION) {
        return this.migrateVersion(data);
      }

      return data;
    } catch (e) {
      console.error('Load failed:', e);
      return null;
    }
  }

  /**
   * Check if a save exists in the given slot.
   */
  hasSave(slot: number): boolean {
    if (!this.isValidSlot(slot)) return false;
    const key = this.getKey(slot);
    return localStorage.getItem(key) !== null;
  }

  /**
   * Delete a save from a slot.
   */
  deleteSave(slot: number): boolean {
    if (!this.isValidSlot(slot)) return false;
    try {
      const key = this.getKey(slot);
      localStorage.removeItem(key);
      return true;
    } catch (e) {
      console.error('Delete failed:', e);
      return false;
    }
  }

  /**
   * Get summary info for a save slot (for the slot selection screen).
   */
  getSaveInfo(slot: number): ISaveSlotInfo {
    const empty: ISaveSlotInfo = {
      exists: false,
      name: '',
      badges: 0,
      playtime: 0,
      partyLevel: 0,
      partySize: 0,
      savedAt: '',
    };

    if (!this.isValidSlot(slot)) return empty;

    const data = this.load(slot);
    if (!data) return empty;

    // Find highest level in party
    let maxLevel = 0;
    for (const dino of data.party) {
      if (dino.level > maxLevel) maxLevel = dino.level;
    }

    return {
      exists: true,
      name: data.player.name,
      badges: data.badges.length,
      playtime: data.playtime,
      partyLevel: maxLevel,
      partySize: data.party.length,
      savedAt: data.savedAt,
    };
  }

  /**
   * Get info for all save slots.
   */
  getAllSaveInfo(): ISaveSlotInfo[] {
    const infos: ISaveSlotInfo[] = [];
    for (let i = 0; i < MAX_SAVE_SLOTS; i++) {
      infos.push(this.getSaveInfo(i));
    }
    return infos;
  }

  /**
   * Format playtime as "HH:MM:SS".
   */
  static formatPlaytime(seconds: number): string {
    const h = Math.floor(seconds / 3600);
    const m = Math.floor((seconds % 3600) / 60);
    const s = Math.floor(seconds % 60);
    return `${h.toString().padStart(2, '0')}:${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`;
  }

  // --------------- Internal ---------------

  private getKey(slot: number): string {
    return `${SAVE_KEY_PREFIX}${slot}`;
  }

  private isValidSlot(slot: number): boolean {
    return Number.isInteger(slot) && slot >= 0 && slot < MAX_SAVE_SLOTS;
  }

  /**
   * Migrate save data from an older version to current.
   * For now, just returns data as-is since we're on version 1.
   */
  private migrateVersion(data: IGameState): IGameState {
    // Future: handle migrations between versions
    data.version = SAVE_VERSION;
    return data;
  }
}

// Singleton instance
export const saveSystem = new SaveSystem();
