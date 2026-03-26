// ============================================================
// Jurassic Trainers -- Game Constants
// Visual overhaul: 640×400 native resolution, 32px tiles
// ============================================================

// --- Display ---
export const GAME_WIDTH = 640;
export const GAME_HEIGHT = 400;
export const TILE_SIZE = 32;

// --- Grid ---
export const GRID_COLS = 20;   // 640 / 32
export const GRID_ROWS = 12;   // visible rows (last row partially visible at 400px)

// --- Player ---
export const PLAYER_MOVE_DURATION = 150;  // ms per tile movement (faster for bigger tiles)
export const MAX_PARTY_SIZE = 6;
export const MAX_LEVEL = 50;
export const MAX_MOVE_SLOTS = 4;

// --- Battle ---
// BALANCE DESIGN NOTE (SCALE agent, Phase 2):
// Early typed attacks are 30-40 power, rising to 60-90 mid-game.
// With STAB (1.5x) + super-effective (2.0x) = 3x multiplier:
//   - Lv 5-10: a 35-power move does ~20 damage on 30 HP = survivable (2-3 hit KO)
//   - Lv 15-25: a 60-power move does ~35 damage = strong but fair
//   - Lv 30+: a 90-power move does ~50+ damage = one-shot possible, intended for late game
// The multipliers below are intentionally kept constant across all levels.
// Early balance is controlled by move power in learnsets (typed attacks start at Lv 7).
export const BASE_ENCOUNTER_RATE = 8;     // % per step in tall grass
export const STAB_MULTIPLIER = 1.5;
export const CRITICAL_HIT_MULTIPLIER = 1.5;
export const CRITICAL_HIT_CHANCE = 6.25;  // %
export const DAMAGE_RANDOM_MIN = 0.85;
export const DAMAGE_RANDOM_MAX = 1.0;

// --- Capture ---
export const MAX_CAPTURE_RATE = 255;

// --- Save ---
export const SAVE_KEY_PREFIX = 'jurassic-trainers-save-';
export const MAX_SAVE_SLOTS = 3;

// --- UI ---
export const DIALOGUE_SPEED = 30;         // ms per character (typewriter)
export const DIALOGUE_FAST_SPEED = 10;
export const DIALOGUE_BOX_HEIGHT = 96;

// --- Typography ---
export const FONT_FAMILY = "'Press Start 2P', 'Courier New', monospace";
export const FONT_TITLE = '48px';
export const FONT_HEADING = '24px';
export const FONT_BODY = '16px';
export const FONT_SMALL = '14px';
export const FONT_TINY = '12px';

// --- Colors — warm, amber-toned palette ---
export const COLORS = {
  BLACK: 0x181018,
  WHITE: 0xF0E8D0,          // warm cream, not pure white
  BG_DARK: 0x1a1a2e,
  UI_BG: 0x202038,
  UI_BORDER: 0xC89840,      // amber border
  UI_BORDER_LIGHT: 0xE8C868,
  UI_BORDER_DARK: 0x886830,
  HP_GREEN: 0x48D848,
  HP_YELLOW: 0xF8D830,
  HP_RED: 0xF85030,
  XP_BLUE: 0x58A8F8,
  TEXT_PRIMARY: 0xF0E8D0,
  TEXT_SHADOW: 0x181018,
  TEXT_DARK: 0x383028,
  DIALOGUE_BG: 0x181028,
  MENU_BG: 0x202038,
  BATTLE_GRASS: 0x58A848,
  BATTLE_SKY: 0x78C8F0,
} as const;

// --- Dino Types ---
export enum DinoType {
  Normal = 0,
  Fire = 1,
  Water = 2,
  Earth = 3,
  Air = 4,
  Electric = 5,
  Ice = 6,
  Venom = 7,
  Flora = 8,
  Fossil = 9,
  Shadow = 10,
  Light = 11,
  Metal = 12,
  Primal = 13,
}

export const DINO_TYPE_NAMES: Record<DinoType, string> = {
  [DinoType.Normal]: 'Normal',
  [DinoType.Fire]: 'Fire',
  [DinoType.Water]: 'Water',
  [DinoType.Earth]: 'Earth',
  [DinoType.Air]: 'Air',
  [DinoType.Electric]: 'Electric',
  [DinoType.Ice]: 'Ice',
  [DinoType.Venom]: 'Venom',
  [DinoType.Flora]: 'Flora',
  [DinoType.Fossil]: 'Fossil',
  [DinoType.Shadow]: 'Shadow',
  [DinoType.Light]: 'Light',
  [DinoType.Metal]: 'Metal',
  [DinoType.Primal]: 'Primal',
};

export const DINO_TYPE_COLORS: Record<DinoType, number> = {
  [DinoType.Normal]: 0xa8a878,
  [DinoType.Fire]: 0xf08030,
  [DinoType.Water]: 0x6890f0,
  [DinoType.Earth]: 0xc8a040,
  [DinoType.Air]: 0xa890f0,
  [DinoType.Electric]: 0xf8d030,
  [DinoType.Ice]: 0x98d8d8,
  [DinoType.Venom]: 0xa040a0,
  [DinoType.Flora]: 0x78c850,
  [DinoType.Fossil]: 0xb8a038,
  [DinoType.Shadow]: 0x705848,
  [DinoType.Light]: 0xf8e870,
  [DinoType.Metal]: 0xb8b8d0,
  [DinoType.Primal]: 0x7038f8,
};

// --- Directions ---
export enum Direction {
  Up = 'up',
  Down = 'down',
  Left = 'left',
  Right = 'right',
}

// --- Status Effects ---
export enum StatusEffect {
  None = 'none',
  Poison = 'poison',
  Burn = 'burn',
  Paralysis = 'paralysis',
  Sleep = 'sleep',
  Freeze = 'freeze',
}

// --- Battle Phases ---
export enum BattlePhase {
  Intro = 'INTRO',
  PlayerTurn = 'PLAYER_TURN',
  EnemyTurn = 'ENEMY_TURN',
  Resolution = 'RESOLUTION',
  End = 'END',
}

// --- Scene Keys ---
export const SCENE_KEYS = {
  BOOT: 'BootScene',
  TITLE: 'TitleScene',
  STARTER_SELECT: 'StarterSelectScene',
  OVERWORLD: 'OverworldScene',
  BATTLE: 'BattleScene',
  MENU: 'MenuScene',
  DINODEX: 'DinodexScene',
  DIALOGUE: 'DialogueScene',
  DINO_CENTER: 'DinoCenterScene',
  SHOP: 'ShopScene',
  PARTY: 'PartyScene',
  BAG: 'BagScene',
  EVOLUTION: 'EvolutionScene',
  GYM: 'GymScene',
  ELITE_FOUR: 'EliteFourScene',
  HOUSE: 'HouseScene',
} as const;
