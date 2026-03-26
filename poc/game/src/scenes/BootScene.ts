import Phaser from 'phaser';
import {
  SCENE_KEYS,
  GAME_WIDTH,
  GAME_HEIGHT,
  COLORS,
  FONT_FAMILY,
  DinoType,
} from '../utils/constants';
import { registerSpecies } from '../entities/Dino';
import dinosData from '../data/dinos.json';
import movesData from '../data/moves.json';
import itemsData from '../data/items.json';
import { registerMoves, IMoveData, IMoveEffect, MoveEffectType } from '../systems/BattleSystem';
import { registerItems, IItemData, IItemEffect, ItemEffectType } from '../systems/InventorySystem';

export class BootScene extends Phaser.Scene {
  constructor() {
    super({ key: SCENE_KEYS.BOOT });
  }

  create(): void {
    // ─────────────────────────────────────────────────────────────────
    // TYPE CONVERSION: JSON data files ➜ DinoType enum
    //
    // JSON files (dinos.json, moves.json, type_chart.json) use a DIFFERENT
    // numbering than the DinoType enum in constants.ts.
    //   Example: JSON type 2 = Fire, but DinoType enum 2 = Water.
    //
    // The table below converts JSON type IDs to correct DinoType values.
    // All dino types and move types MUST pass through mapType() on load.
    //
    // Full documentation: game/src/data/TYPE_MAPPING.md
    //
    // WARNING: Never use raw JSON type numbers as DinoType values directly.
    // ─────────────────────────────────────────────────────────────────
    const JSON_TO_DINOTYPE: Record<number, DinoType> = {
      0:  DinoType.Fossil,    // Roche/Fossile (Caillex, Graviroc, ...)
      1:  DinoType.Water,     // Eau (Aquadon, Marexis, ...)
      2:  DinoType.Fire,      // Feu (Pyrex, Pyrovore, ...)
      3:  DinoType.Flora,     // Plante (Florasaur, Sylvacolle, ...)
      4:  DinoType.Ice,       // Glace (Givrex, Cryodonte, ...)
      5:  DinoType.Air,       // Air (Plumex, Aeroraptor, ...)
      6:  DinoType.Earth,     // Terre (Terravore, Seismops, ...)
      7:  DinoType.Electric,  // Electrique (Fulgure, Voltacroc, ...)
      8:  DinoType.Venom,     // Poison (Toxidon, Venomex, ...)
      9:  DinoType.Metal,     // Metal (Forgeron, Blindorex, ...)
      10: DinoType.Shadow,    // Ombre (Spectrovore, Abyssinthe, ...)
      11: DinoType.Light,     // Lumiere (Solaurex, Heliodonte, ...)
      12: DinoType.Normal,    // Normal (Dunex, Sirocco, ...)
      13: DinoType.Primal,    // Primordial (Relicor, Archeon, ...)
    };
    const mapType = (jsonType: number): DinoType => JSON_TO_DINOTYPE[jsonType] ?? DinoType.Normal;

    // Register all 150 dino species — adapt JSON format to ISpeciesData
    const species = (dinosData as any[]).map((d: any) => ({
      id: d.id,
      name: d.name,
      type1: mapType(d.types[0] ?? 12),
      type2: d.types.length > 1 ? mapType(d.types[1]) : undefined,
      baseStats: {
        hp: d.baseStats.hp,
        attack: d.baseStats.atk,
        defense: d.baseStats.def,
        spAttack: d.baseStats.spatk,
        spDefense: d.baseStats.spdef,
        speed: d.baseStats.speed,
      },
      xpGroup: d.xpGroup || 'medium',
      captureRate: d.captureRate || 150,
      baseXpYield: d.xpYield || 60,
      learnset: d.learnset || [],
      evolution: d.evolution ? { targetId: d.evolution.to, level: d.evolution.level } : undefined,
      description: d.description || '',
      height: d.height || 1,
      weight: d.weight || 10,
      sprite: d.name.toLowerCase(),
    }));
    registerSpecies(species);

    // Register all moves — adapt JSON format to IMoveData
    const moves: IMoveData[] = (movesData as any[]).map((m: any) => {
      let effect: IMoveEffect | undefined;
      if (m.effect && m.effect !== 'none') {
        const effectMap: Record<string, MoveEffectType> = {
          'burn': 'burn', 'poison': 'poison', 'paralyze': 'paralyze',
          'sleep': 'sleep', 'freeze': 'freeze', 'confuse': 'confuse',
          'flinch': 'flinch', 'recoil': 'recoil', 'drain': 'drain', 'heal': 'heal',
          'statUp_atk': 'stat_up', 'statUp_def': 'stat_up', 'statUp_spatk': 'stat_up',
          'statUp_spdef': 'stat_up', 'statUp_speed': 'stat_up',
          'statDown_atk': 'stat_down', 'statDown_def': 'stat_down', 'statDown_spatk': 'stat_down',
          'statDown_spdef': 'stat_down', 'statDown_speed': 'stat_down', 'statDown_accuracy': 'stat_down',
        };
        const effType = effectMap[m.effect];
        if (effType) {
          const statFromEffect = m.effect.includes('_') ? m.effect.split('_').pop() : undefined;
          effect = {
            type: effType,
            chance: m.effectChance ?? 0,
            target: m.effect.startsWith('statUp') ? 'self' : 'opponent',
            stat: statFromEffect,
          };
        }
      }
      return {
        id: m.id,
        name: m.name,
        type: mapType(m.type),
        category: m.category,
        power: m.power,
        accuracy: m.accuracy,
        pp: m.pp,
        priority: m.priority ?? 0,
        effect,
        description: m.description || '',
      };
    });
    registerMoves(moves);

    // Register all items — adapt JSON format to IItemData
    const items: IItemData[] = (itemsData as any[]).map((it: any) => {
      let effect: IItemEffect | undefined;
      if (it.effect) {
        const effectMap: Record<string, ItemEffectType> = {
          'heal_hp': 'heal_hp', 'heal_full': 'full_heal', 'heal_pp': 'heal_pp',
          'heal_pp_all': 'heal_pp_all', 'heal_status': 'heal_status',
          'cure_poison': 'heal_status', 'cure_burn': 'heal_status', 'cure_paralyze': 'heal_status',
          'cure_sleep': 'heal_status', 'cure_all': 'heal_status',
          'revive': 'revive', 'capture': 'capture',
          'rare_candy': 'rare_candy', 'x_stat': 'x_stat',
          'boost_atk': 'x_stat', 'boost_def': 'x_stat', 'boost_spatk': 'x_stat',
          'boost_spdef': 'x_stat', 'boost_speed': 'x_stat',
        };
        const effType = effectMap[it.effect];
        if (effType) {
          effect = { type: effType, value: it.value ?? 0 };
        }
      }
      return {
        id: it.id,
        name: it.name,
        category: it.category === 'status_cure' ? 'healing' as const
          : it.category === 'battle' ? 'battle' as const
          : it.category as any,
        description: it.description || '',
        price: it.price ?? 0,
        usableInBattle: it.usableInBattle ?? false,
        usableOutside: it.usableInField ?? false,
        effect,
      };
    });
    registerItems(items);

    this.cameras.main.setBackgroundColor(0x0a0a12);

    // Studio intro text
    const studioText = this.add.text(GAME_WIDTH / 2, GAME_HEIGHT / 2 - 8, 'NOVA FORGE STUDIO', {
      fontFamily: FONT_FAMILY,
      fontSize: '20px',
      color: '#C89840',
    }).setOrigin(0.5).setAlpha(0);

    const subtitleText = this.add.text(GAME_WIDTH / 2, GAME_HEIGHT / 2 + 20, 'presents', {
      fontFamily: FONT_FAMILY,
      fontSize: '12px',
      color: '#887050',
    }).setOrigin(0.5).setAlpha(0);

    // Small decorative line above and below
    const decoGfx = this.add.graphics().setAlpha(0);
    decoGfx.lineStyle(1, 0xC89840, 0.6);
    decoGfx.lineBetween(GAME_WIDTH / 2 - 120, GAME_HEIGHT / 2 - 30, GAME_WIDTH / 2 + 120, GAME_HEIGHT / 2 - 30);
    decoGfx.lineBetween(GAME_WIDTH / 2 - 80, GAME_HEIGHT / 2 + 40, GAME_WIDTH / 2 + 80, GAME_HEIGHT / 2 + 40);

    // Fade in studio name
    this.tweens.add({
      targets: [studioText, subtitleText, decoGfx],
      alpha: 1,
      duration: 600,
      ease: 'Power2',
    });

    // Hold, then fade out
    this.time.delayedCall(1500, () => {
      this.tweens.add({
        targets: [studioText, subtitleText, decoGfx],
        alpha: 0,
        duration: 500,
        ease: 'Power2',
        onComplete: () => {
          this.scene.start(SCENE_KEYS.TITLE);
        },
      });
    });
  }
}
