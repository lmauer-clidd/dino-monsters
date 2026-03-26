// ============================================================
// Jurassic Trainers -- Inventory System
// ============================================================

import { Dino } from '../entities/Dino';
import { StatusEffect } from '../utils/constants';

// ===================== Interfaces =====================

export type ItemCategory = 'healing' | 'capture' | 'battle' | 'key' | 'held' | 'tm';

export interface IItemData {
  id: number;
  name: string;
  category: ItemCategory;
  description: string;
  price: number;         // 0 = not buyable
  usableInBattle: boolean;
  usableOutside: boolean;
  effect?: IItemEffect;
  sprite?: string;
}

export interface IItemEffect {
  type: ItemEffectType;
  value?: number;        // heal amount, PP restore, etc.
  moveId?: number;       // for TMs
  statBoost?: string;    // for battle items (X Attack, etc.)
}

export type ItemEffectType =
  | 'heal_hp'
  | 'heal_pp'
  | 'heal_pp_all'
  | 'heal_status'
  | 'full_heal'        // HP + status + PP
  | 'revive'
  | 'capture'
  | 'x_stat'           // temporary stat boost in battle
  | 'tm'
  | 'key_item'
  | 'held_item'
  | 'rare_candy';       // instant level up

// ===================== Item Registry =====================

const itemRegistry: Map<number, IItemData> = new Map();

export function registerItems(data: IItemData[]): void {
  for (const item of data) {
    itemRegistry.set(item.id, item);
  }
}

export function getItem(id: number): IItemData {
  const item = itemRegistry.get(id);
  if (!item) throw new Error(`Unknown item id: ${id}`);
  return item;
}

export function hasItemData(id: number): boolean {
  return itemRegistry.has(id);
}

// ===================== Inventory System =====================

export class InventorySystem {
  /** itemId -> quantity */
  private items: Map<number, number>;

  constructor() {
    this.items = new Map();
  }

  // --------------- Core Operations ---------------

  addItem(itemId: number, quantity: number = 1): void {
    const current = this.items.get(itemId) ?? 0;
    this.items.set(itemId, current + quantity);
  }

  removeItem(itemId: number, quantity: number = 1): boolean {
    const current = this.items.get(itemId) ?? 0;
    if (current < quantity) return false;
    const remaining = current - quantity;
    if (remaining <= 0) {
      this.items.delete(itemId);
    } else {
      this.items.set(itemId, remaining);
    }
    return true;
  }

  hasItem(itemId: number): boolean {
    return (this.items.get(itemId) ?? 0) > 0;
  }

  getItemCount(itemId: number): number {
    return this.items.get(itemId) ?? 0;
  }

  // --------------- Category Queries ---------------

  getItemsByCategory(category: ItemCategory): { item: IItemData; quantity: number }[] {
    const result: { item: IItemData; quantity: number }[] = [];
    for (const [itemId, quantity] of this.items) {
      if (!hasItemData(itemId)) continue;
      const item = getItem(itemId);
      if (item.category === category) {
        result.push({ item, quantity });
      }
    }
    // Sort by item ID for consistent ordering
    result.sort((a, b) => a.item.id - b.item.id);
    return result;
  }

  getAllItems(): { item: IItemData; quantity: number }[] {
    const result: { item: IItemData; quantity: number }[] = [];
    for (const [itemId, quantity] of this.items) {
      if (!hasItemData(itemId)) continue;
      result.push({ item: getItem(itemId), quantity });
    }
    result.sort((a, b) => a.item.id - b.item.id);
    return result;
  }

  canUseInBattle(itemId: number): boolean {
    if (!hasItemData(itemId)) return false;
    return getItem(itemId).usableInBattle;
  }

  canUseOutside(itemId: number): boolean {
    if (!hasItemData(itemId)) return false;
    return getItem(itemId).usableOutside;
  }

  // --------------- Item Usage ---------------

  /**
   * Apply an item's effect to a target dino.
   * Returns true if the item was successfully used (and consumed).
   */
  useItem(itemId: number, target?: Dino): { success: boolean; message: string } {
    if (!this.hasItem(itemId)) {
      return { success: false, message: "Vous n'avez pas cet objet!" };
    }

    if (!hasItemData(itemId)) {
      return { success: false, message: 'Objet inconnu!' };
    }

    const item = getItem(itemId);
    const effect = item.effect;

    if (!effect) {
      return { success: false, message: "Cet objet ne peut pas être utilisé ici." };
    }

    let result: { success: boolean; message: string };

    switch (effect.type) {
      case 'heal_hp':
        result = this.applyHealHp(item, target);
        break;

      case 'heal_pp':
        result = this.applyHealPP(item, target);
        break;

      case 'heal_pp_all':
        result = this.applyHealPPAll(item, target);
        break;

      case 'heal_status':
        result = this.applyHealStatus(item, target);
        break;

      case 'full_heal':
        result = this.applyFullHeal(item, target);
        break;

      case 'revive':
        result = this.applyRevive(item, target);
        break;

      case 'rare_candy':
        result = this.applyRareCandy(item, target);
        break;

      case 'capture':
        // Capture balls are handled by BattleSystem directly
        result = { success: true, message: '' };
        break;

      case 'x_stat':
        // X items are handled by BattleSystem directly
        result = { success: true, message: `${item.name} utilisé!` };
        break;

      default:
        result = { success: false, message: "Cet objet ne peut pas être utilisé ici." };
    }

    if (result.success) {
      // Key items are not consumed
      if (item.category !== 'key') {
        this.removeItem(itemId);
      }
    }

    return result;
  }

  // --------------- Effect Implementations ---------------

  private applyHealHp(item: IItemData, target?: Dino): { success: boolean; message: string } {
    if (!target) return { success: false, message: 'Pas de cible!' };
    if (target.isFainted()) return { success: false, message: `${target.nickname} est K.O.!` };
    if (target.currentHp >= target.maxHp) return { success: false, message: `${target.nickname} a déjà tous ses PV!` };

    const healed = target.heal(item.effect!.value ?? 20);
    return { success: true, message: `${target.nickname} récupère ${healed} PV!` };
  }

  private applyHealPP(item: IItemData, target?: Dino): { success: boolean; message: string } {
    if (!target) return { success: false, message: 'Pas de cible!' };

    // Restore PP to the first move that needs it
    let restored = false;
    for (const move of target.moves) {
      if (move.currentPP < move.maxPP) {
        const amount = item.effect!.value ?? 10;
        move.currentPP = Math.min(move.maxPP, move.currentPP + amount);
        restored = true;
        break;
      }
    }

    if (!restored) return { success: false, message: 'PP déjà au maximum!' };
    return { success: true, message: `PP restaurés!` };
  }

  private applyHealPPAll(item: IItemData, target?: Dino): { success: boolean; message: string } {
    if (!target) return { success: false, message: 'Pas de cible!' };

    let restored = false;
    for (const move of target.moves) {
      if (move.currentPP < move.maxPP) {
        move.currentPP = move.maxPP;
        restored = true;
      }
    }

    if (!restored) return { success: false, message: 'PP déjà au maximum!' };
    return { success: true, message: `Tous les PP restaurés!` };
  }

  private applyHealStatus(item: IItemData, target?: Dino): { success: boolean; message: string } {
    if (!target) return { success: false, message: 'Pas de cible!' };
    if (target.status === StatusEffect.None) {
      return { success: false, message: `${target.nickname} n'a pas de problème de statut!` };
    }

    target.cureStatus();
    return { success: true, message: `${target.nickname} est guéri!` };
  }

  private applyFullHeal(item: IItemData, target?: Dino): { success: boolean; message: string } {
    if (!target) return { success: false, message: 'Pas de cible!' };
    if (target.isFainted()) return { success: false, message: `${target.nickname} est K.O.!` };

    target.fullHeal();
    return { success: true, message: `${target.nickname} est complètement guéri!` };
  }

  private applyRevive(item: IItemData, target?: Dino): { success: boolean; message: string } {
    if (!target) return { success: false, message: 'Pas de cible!' };
    if (!target.isFainted()) return { success: false, message: `${target.nickname} n'est pas K.O.!` };

    const healPercent = (item.effect!.value ?? 50) / 100;
    target.heal(Math.floor(target.maxHp * healPercent));
    target.cureStatus();
    return { success: true, message: `${target.nickname} reprend connaissance!` };
  }

  private applyRareCandy(item: IItemData, target?: Dino): { success: boolean; message: string } {
    if (!target) return { success: false, message: 'Pas de cible!' };
    if (target.level >= 50) return { success: false, message: `${target.nickname} est déjà au niveau maximum!` };

    // Give enough XP to level up exactly once
    const needed = target.getXpForNextLevel() - target.xp;
    target.gainXp(Math.max(1, needed));
    return { success: true, message: `${target.nickname} monte au niveau ${target.level}!` };
  }

  // --------------- Serialization ---------------

  serialize(): Record<number, number> {
    const data: Record<number, number> = {};
    for (const [id, qty] of this.items) {
      data[id] = qty;
    }
    return data;
  }

  static deserialize(data: Record<number, number>): InventorySystem {
    const inv = new InventorySystem();
    for (const [idStr, qty] of Object.entries(data)) {
      inv.items.set(Number(idStr), qty);
    }
    return inv;
  }

  /** Get raw map for GameState access */
  getRawMap(): Map<number, number> {
    return this.items;
  }

  /** Load from raw map */
  loadFromMap(map: Map<number, number>): void {
    this.items = new Map(map);
  }
}
