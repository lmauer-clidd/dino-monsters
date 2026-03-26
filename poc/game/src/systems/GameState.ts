// ============================================================
// Jurassic Trainers -- Game State (Singleton)
// ============================================================

import { Dino } from '../entities/Dino';
import { PartySystem } from './PartySystem';
import { InventorySystem } from './InventorySystem';
import { saveSystem, IGameState, IDinodexEntry } from './SaveSystem';
import { Direction } from '../utils/constants';

// ===================== Interfaces =====================

export interface IPlayerState {
  name: string;
  x: number;
  y: number;
  mapId: string;
  facing: Direction;
  money: number;
  badges: number[];
  playtime: number;   // seconds
}

// ===================== Game State Singleton =====================

export class GameState {
  private static instance: GameState | null = null;

  // --- Player ---
  player: IPlayerState;

  // --- Systems ---
  private partySystem: PartySystem;
  private inventorySystem: InventorySystem;

  // --- Collections ---
  dinodex: Map<number, IDinodexEntry>;
  flags: Map<string, boolean>;
  defeatedTrainers: Set<string>;

  // --- Runtime ---
  currentMap: string;
  private sessionStartTime: number;
  private accumulatedPlaytime: number;

  // --- Last heal point (respawn on blackout) ---
  lastHealMap: string;
  lastHealX: number;
  lastHealY: number;

  private constructor() {
    this.player = {
      name: '',
      x: 0,
      y: 0,
      mapId: 'BOURG_NID',
      facing: Direction.Down,
      money: 3000,
      badges: [],
      playtime: 0,
    };

    this.partySystem = new PartySystem();
    this.inventorySystem = new InventorySystem();
    this.dinodex = new Map();
    this.flags = new Map();
    this.defeatedTrainers = new Set();
    this.currentMap = 'BOURG_NID';
    this.lastHealMap = 'BOURG_NID';
    this.lastHealX = 7;
    this.lastHealY = 5;
    this.sessionStartTime = Date.now();
    this.accumulatedPlaytime = 0;
  }

  // --------------- Singleton ---------------

  static getInstance(): GameState {
    if (!GameState.instance) {
      GameState.instance = new GameState();
    }
    return GameState.instance;
  }

  /** Reset the singleton (for new game). */
  static reset(): void {
    GameState.instance = null;
  }

  // --------------- Initialization ---------------

  /**
   * Initialize a new game with player name and starter dino.
   */
  init(playerName: string, starterId: number): void {
    this.player.name = playerName;
    this.player.x = 7;   // Starting position (center of map)
    this.player.y = 5;
    this.player.mapId = 'BOURG_NID';
    this.player.facing = Direction.Down;
    this.player.money = 3000;
    this.player.badges = [];
    this.player.playtime = 0;

    this.partySystem = new PartySystem();
    this.inventorySystem = new InventorySystem();
    this.dinodex = new Map();
    this.flags = new Map();
    this.defeatedTrainers = new Set();
    this.currentMap = 'BOURG_NID';
    this.sessionStartTime = Date.now();
    this.accumulatedPlaytime = 0;

    // Create starter dino
    const starter = Dino.createStarter(starterId, 5);
    this.partySystem.addToParty(starter);
    this.catchDino(starterId);

    // Give starting items (IDs match items.json)
    this.inventorySystem.addItem(1, 3);   // 3 Potions (id=1)
    this.inventorySystem.addItem(16, 5);  // 5 Jurassic Balls (id=16)

    // Default heal point: starting village
    this.lastHealMap = 'BOURG_NID';
    this.lastHealX = 7;
    this.lastHealY = 5;
  }

  // --------------- System Accessors ---------------

  getPartySystem(): PartySystem {
    return this.partySystem;
  }

  getInventorySystem(): InventorySystem {
    return this.inventorySystem;
  }

  /** Shortcut: get the party array. */
  get party(): readonly Dino[] {
    return this.partySystem.getParty();
  }

  /** Shortcut: get the storage array. */
  get storage(): readonly Dino[] {
    return this.partySystem.getStorage();
  }

  // --------------- Dinodex ---------------

  seeDino(speciesId: number): void {
    const entry = this.dinodex.get(speciesId);
    if (entry) {
      entry.seen = true;
    } else {
      this.dinodex.set(speciesId, { seen: true, caught: false });
    }
  }

  catchDino(speciesId: number): void {
    const entry = this.dinodex.get(speciesId);
    if (entry) {
      entry.seen = true;
      entry.caught = true;
    } else {
      this.dinodex.set(speciesId, { seen: true, caught: true });
    }
  }

  getDinodexEntry(speciesId: number): IDinodexEntry | undefined {
    return this.dinodex.get(speciesId);
  }

  getDinodexSeenCount(): number {
    let count = 0;
    for (const entry of this.dinodex.values()) {
      if (entry.seen) count++;
    }
    return count;
  }

  getDinodexCaughtCount(): number {
    let count = 0;
    for (const entry of this.dinodex.values()) {
      if (entry.caught) count++;
    }
    return count;
  }

  // --------------- Badges ---------------

  addBadge(index: number): void {
    if (!this.player.badges.includes(index)) {
      this.player.badges.push(index);
      this.player.badges.sort((a, b) => a - b);
    }
  }

  hasBadge(index: number): boolean {
    return this.player.badges.includes(index);
  }

  getBadgeCount(): number {
    return this.player.badges.length;
  }

  // --------------- Heal Point (respawn) ---------------

  /** Save the current position as the respawn point (call when healing at a center) */
  setHealPoint(mapId: string, x: number, y: number): void {
    this.lastHealMap = mapId;
    this.lastHealX = x;
    this.lastHealY = y;
  }

  /** Get the last heal point for respawn on blackout */
  getHealPoint(): { mapId: string; x: number; y: number } {
    return { mapId: this.lastHealMap, x: this.lastHealX, y: this.lastHealY };
  }

  // --------------- Flags ---------------

  setFlag(key: string, value: boolean = true): void {
    this.flags.set(key, value);
  }

  hasFlag(key: string): boolean {
    return this.flags.get(key) === true;
  }

  getFlag(key: string): boolean {
    return this.flags.get(key) ?? false;
  }

  // --------------- Trainers ---------------

  defeatTrainer(id: string): void {
    this.defeatedTrainers.add(id);
  }

  isTrainerDefeated(id: string): boolean {
    return this.defeatedTrainers.has(id);
  }

  // --------------- Money ---------------

  addMoney(amount: number): void {
    this.player.money += Math.max(0, Math.floor(amount));
    // Cap at 999,999
    this.player.money = Math.min(this.player.money, 999999);
  }

  removeMoney(amount: number): boolean {
    const cost = Math.max(0, Math.floor(amount));
    if (this.player.money < cost) return false;
    this.player.money -= cost;
    return true;
  }

  getMoney(): number {
    return this.player.money;
  }

  // --------------- Playtime ---------------

  /** Get total playtime in seconds (accumulated + current session). */
  getPlaytime(): number {
    const sessionSeconds = Math.floor((Date.now() - this.sessionStartTime) / 1000);
    return this.accumulatedPlaytime + sessionSeconds;
  }

  // --------------- Position ---------------

  setPlayerPosition(x: number, y: number, mapId?: string): void {
    this.player.x = x;
    this.player.y = y;
    if (mapId) {
      this.player.mapId = mapId;
      this.currentMap = mapId;
    }
  }

  setPlayerFacing(direction: Direction): void {
    this.player.facing = direction;
  }

  // --------------- Save / Load ---------------

  /**
   * Save the current game state to a slot.
   */
  saveGame(slot: number): boolean {
    const state = this.serializeState();
    return saveSystem.save(slot, state);
  }

  /**
   * Load a game state from a slot and apply it.
   */
  loadGame(slot: number): boolean {
    const state = saveSystem.load(slot);
    if (!state) return false;
    this.deserializeState(state);
    return true;
  }

  private serializeState(): IGameState {
    // Convert dinodex Map to Record
    const dinodexRecord: Record<number, IDinodexEntry> = {};
    for (const [id, entry] of this.dinodex) {
      dinodexRecord[id] = { ...entry };
    }

    // Convert flags Map to Record
    const flagsRecord: Record<string, boolean> = {};
    for (const [key, value] of this.flags) {
      flagsRecord[key] = value;
    }

    return {
      version: 1,
      player: {
        name: this.player.name,
        x: this.player.x,
        y: this.player.y,
        mapId: this.player.mapId,
        facing: this.player.facing,
      },
      party: this.partySystem.serializeParty(),
      storage: this.partySystem.serializeStorage(),
      inventory: this.inventorySystem.serialize(),
      dinodex: dinodexRecord,
      badges: [...this.player.badges],
      flags: flagsRecord,
      defeatedTrainers: Array.from(this.defeatedTrainers),
      playtime: this.getPlaytime(),
      money: this.player.money,
      savedAt: new Date().toISOString(),
    };
  }

  private deserializeState(state: IGameState): void {
    // Player
    this.player.name = state.player.name;
    this.player.x = state.player.x;
    this.player.y = state.player.y;
    this.player.mapId = state.player.mapId;
    this.player.facing = state.player.facing as Direction;
    this.player.money = state.money;
    this.player.badges = [...state.badges];
    this.currentMap = state.player.mapId;

    // Playtime
    this.accumulatedPlaytime = state.playtime;
    this.sessionStartTime = Date.now();

    // Party & Storage
    this.partySystem = new PartySystem();
    this.partySystem.loadParty(state.party);
    this.partySystem.loadStorage(state.storage);

    // Inventory
    this.inventorySystem = InventorySystem.deserialize(state.inventory);

    // Dinodex
    this.dinodex = new Map();
    for (const [idStr, entry] of Object.entries(state.dinodex)) {
      this.dinodex.set(Number(idStr), { ...entry });
    }

    // Flags
    this.flags = new Map();
    for (const [key, value] of Object.entries(state.flags)) {
      this.flags.set(key, value);
    }

    // Defeated trainers
    this.defeatedTrainers = new Set(state.defeatedTrainers);
  }
}
