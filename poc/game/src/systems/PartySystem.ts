// ============================================================
// Jurassic Trainers -- Party System
// ============================================================

import { Dino, IDinoSerialized } from '../entities/Dino';
import { MAX_PARTY_SIZE } from '../utils/constants';

// ===================== Party System =====================

export class PartySystem {
  private party: Dino[];
  private storage: Dino[];

  constructor() {
    this.party = [];
    this.storage = [];
  }

  // --------------- Party Operations ---------------

  /**
   * Add a dino to the party. If the party is full, send to storage.
   * Returns 'party' or 'storage' to indicate where it went.
   */
  addToParty(dino: Dino): 'party' | 'storage' {
    if (this.party.length < MAX_PARTY_SIZE) {
      this.party.push(dino);
      return 'party';
    }
    this.storage.push(dino);
    return 'storage';
  }

  /**
   * Remove a dino from the party by index.
   * Fails if it's the last dino or the last healthy dino.
   */
  removeFromParty(index: number): Dino | null {
    if (index < 0 || index >= this.party.length) return null;
    if (this.party.length <= 1) return null; // Must keep at least 1

    // Check if removing would leave no healthy dinos
    const remaining = this.party.filter((_, i) => i !== index);
    if (!remaining.some(d => !d.isFainted())) return null;

    const [removed] = this.party.splice(index, 1);
    return removed;
  }

  /** Swap two party slots. */
  swapPartyOrder(index1: number, index2: number): boolean {
    if (index1 < 0 || index1 >= this.party.length) return false;
    if (index2 < 0 || index2 >= this.party.length) return false;
    if (index1 === index2) return false;

    const temp = this.party[index1];
    this.party[index1] = this.party[index2];
    this.party[index2] = temp;
    return true;
  }

  /** Get the first non-fainted dino. Returns null if all fainted. */
  getFirstHealthy(): Dino | null {
    return this.party.find(d => !d.isFainted()) ?? null;
  }

  /** Get the index of the first non-fainted dino. */
  getFirstHealthyIndex(): number {
    return this.party.findIndex(d => !d.isFainted());
  }

  /** Full-heal all party dinos. */
  healAll(): void {
    for (const dino of this.party) {
      dino.fullHeal();
    }
  }

  /** Check if all party dinos are fainted. */
  isWhitedOut(): boolean {
    return this.party.length > 0 && this.party.every(d => d.isFainted());
  }

  /** Get the party array (read-only reference). */
  getParty(): readonly Dino[] {
    return this.party;
  }

  /** Get a party member by index. */
  getPartyMember(index: number): Dino | undefined {
    return this.party[index];
  }

  /** Get party size. */
  getPartySize(): number {
    return this.party.length;
  }

  /** Check if party is full. */
  isPartyFull(): boolean {
    return this.party.length >= MAX_PARTY_SIZE;
  }

  /** Get count of healthy dinos. */
  getHealthyCount(): number {
    return this.party.filter(d => !d.isFainted()).length;
  }

  // --------------- Storage Operations ---------------

  /** Add a dino directly to storage. */
  addToStorage(dino: Dino): void {
    this.storage.push(dino);
  }

  /**
   * Withdraw a dino from storage to party.
   * Returns false if party is full.
   */
  withdrawFromStorage(storageIndex: number): boolean {
    if (storageIndex < 0 || storageIndex >= this.storage.length) return false;
    if (this.party.length >= MAX_PARTY_SIZE) return false;

    const [dino] = this.storage.splice(storageIndex, 1);
    this.party.push(dino);
    return true;
  }

  /**
   * Deposit a party dino to storage.
   * Fails if only 1 dino in party or last healthy one.
   */
  depositToStorage(partyIndex: number): boolean {
    const removed = this.removeFromParty(partyIndex);
    if (!removed) return false;
    this.storage.push(removed);
    return true;
  }

  /** Swap a party dino with a storage dino. */
  swapPartyWithStorage(partyIndex: number, storageIndex: number): boolean {
    if (partyIndex < 0 || partyIndex >= this.party.length) return false;
    if (storageIndex < 0 || storageIndex >= this.storage.length) return false;

    const temp = this.party[partyIndex];
    this.party[partyIndex] = this.storage[storageIndex];
    this.storage[storageIndex] = temp;

    // Validate at least one healthy dino remains in party
    if (!this.party.some(d => !d.isFainted())) {
      // Revert
      this.storage[storageIndex] = this.party[partyIndex];
      this.party[partyIndex] = temp;
      return false;
    }

    return true;
  }

  /** Get the storage array (read-only reference). */
  getStorage(): readonly Dino[] {
    return this.storage;
  }

  /** Get storage size. */
  getStorageSize(): number {
    return this.storage.length;
  }

  /** Get a storage dino by index. */
  getStorageDino(index: number): Dino | undefined {
    return this.storage[index];
  }

  // --------------- Serialization ---------------

  serializeParty(): IDinoSerialized[] {
    return this.party.map(d => d.serialize());
  }

  serializeStorage(): IDinoSerialized[] {
    return this.storage.map(d => d.serialize());
  }

  loadParty(data: IDinoSerialized[]): void {
    this.party = data.map(d => Dino.fromSerialized(d));
  }

  loadStorage(data: IDinoSerialized[]): void {
    this.storage = data.map(d => Dino.fromSerialized(d));
  }

  /** Direct setter for party (used by GameState). */
  setParty(party: Dino[]): void {
    this.party = party;
  }

  /** Direct setter for storage (used by GameState). */
  setStorage(storage: Dino[]): void {
    this.storage = storage;
  }
}
