---
name: TECH_ARCHITECTURE
description: Technical architecture document for Jurassic Trainers
type: design-doc
author: CIPHER (Lead Architect / CTO)
version: 0.1.0
date: 2026-03-22
---

# Jurassic Trainers -- Technical Architecture

> Written by **CIPHER** -- Lead Architect / CTO, Nova Forge Studio

---

## 1. Technology Stack

| Layer          | Choice               | Rationale                                              |
| -------------- | -------------------- | ------------------------------------------------------ |
| Runtime        | Browser (HTML5 Canvas) | Zero install, cross-platform, easy sharing           |
| Engine         | Phaser 3.80+         | Battle-tested 2D engine, huge community, MIT license   |
| Language       | TypeScript 5.x       | Type safety catches bugs early, great DX               |
| Build          | Vite                 | Instant HMR, fast builds, native TS support            |
| Map Editor     | Tiled (TMX to JSON)  | Industry standard for 2D tile maps, free               |
| Package Mgr    | npm                  | Default Node ecosystem, lockfile for reproducibility   |
| Version Ctrl   | Git                  | Standard; game data is JSON so diffs are readable      |

### Why Phaser over raw Canvas?

Phaser gives us scene management, input handling, sprite animation, tilemap rendering, tweens, audio, and camera systems out of the box. Rolling our own would cost months with no benefit for a GBA-style RPG.

---

## 2. Project Structure

```
game/src/
├── index.html
├── main.ts              # Entry point, Phaser config
├── scenes/
│   ├── BootScene.ts     # Asset preloading
│   ├── TitleScene.ts    # Title screen
│   ├── OverworldScene.ts # Main exploration
│   ├── BattleScene.ts   # Combat system
│   ├── MenuScene.ts     # Pause menu
│   ├── DinodexScene.ts  # Dino encyclopedia
│   └── DialogueScene.ts # Dialogue overlay
├── entities/
│   ├── Player.ts        # Player character
│   ├── NPC.ts           # Non-player characters
│   ├── Dino.ts          # Dino instance (stats, moves, etc.)
│   └── WildEncounter.ts # Wild encounter trigger
├── systems/
│   ├── BattleSystem.ts  # Turn-based combat logic
│   ├── CaptureSystem.ts # Capture mechanics
│   ├── EvolutionSystem.ts # Evolution checks and animation
│   ├── InventorySystem.ts # Bag/items management
│   ├── SaveSystem.ts    # LocalStorage save/load
│   ├── TypeChart.ts     # Type effectiveness lookup
│   ├── XPSystem.ts      # Experience and leveling
│   └── PartySystem.ts   # Team management (6 dinos max)
├── ui/
│   ├── DialogueBox.ts   # Text box with typewriter effect
│   ├── BattleHUD.ts     # HP bars, menus during battle
│   ├── MenuUI.ts        # Pause menu UI
│   ├── DinodexUI.ts     # Dinodex interface
│   └── TransitionFX.ts  # Screen transitions (spiral, fade)
├── data/
│   ├── dinos.json       # All 150 dinos: stats, types, moves, evolution
│   ├── moves.json       # All moves: power, accuracy, type, effects
│   ├── items.json       # All items: name, effect, price
│   ├── type_chart.json  # 14x14 effectiveness matrix
│   ├── trainers.json    # NPC trainer data
│   ├── maps/            # Tiled JSON exports
│   └── dialogue/        # Dialogue scripts per NPC/event
├── utils/
│   ├── math.ts          # Damage calc, capture calc, random
│   ├── constants.ts     # Game constants
│   └── helpers.ts       # Generic utilities
└── assets/
    ├── sprites/         # Character & dino sprites
    ├── tilesets/        # Map tilesets
    ├── ui/              # UI elements
    ├── music/           # BGM (OGG/MP3)
    └── sfx/             # Sound effects
```

### Naming conventions

- Files: `PascalCase.ts` for classes/scenes, `camelCase.ts` for utilities, `snake_case.json` for data.
- Classes: `PascalCase`.
- Constants: `UPPER_SNAKE_CASE`.
- Interfaces: `I` prefix (e.g. `IDinoSpecies`).

---

## 3. Phaser Configuration

```typescript
const config: Phaser.Types.Core.GameConfig = {
  type: Phaser.AUTO,           // WebGL with Canvas fallback
  width: 240,                  // Native GBA width
  height: 160,                 // Native GBA height
  pixelArt: true,              // Disables anti-aliasing globally
  roundPixels: true,           // Snap sprites to integer coords
  scale: {
    mode: Phaser.Scale.FIT,    // Scale to fit viewport
    autoCenter: Phaser.Scale.CENTER_BOTH,
  },
  physics: {
    default: 'arcade',
    arcade: {
      gravity: { x: 0, y: 0 },  // Top-down RPG, no gravity
      debug: false,
    },
  },
  fps: {
    target: 60,
    forceSetTimeOut: false,
  },
  scene: [BootScene, TitleScene, OverworldScene, BattleScene, MenuScene, DinodexScene, DialogueScene],
};
```

### Resolution strategy

The native GBA resolution is 240x160. We render at this resolution and use Phaser's `FIT` scale mode so the canvas fills the browser window while maintaining the correct aspect ratio (3:2). Integer scaling ensures pixel-perfect rendering -- no sub-pixel blurring.

---

## 4. Core Systems Design

### 4.1 BattleSystem

**State machine:**

```
INTRO --> PLAYER_TURN --> ENEMY_TURN --> RESOLUTION --> END
  ^            |               |              |
  |            v               v              |
  +--- (flee/catch fail) <----+--- (loop) ----+
```

**States:**

| State         | Description                                                        |
| ------------- | ------------------------------------------------------------------ |
| `INTRO`       | Slide-in animations, "A wild X appeared!" text                     |
| `PLAYER_TURN` | Show action menu (Fight / Bag / Dinos / Run), wait for input       |
| `ENEMY_TURN`  | AI selects move, execute, play animation                           |
| `RESOLUTION`  | Check faint, XP gain, evolution trigger, capture result             |
| `END`         | Victory/defeat screen, return to overworld                         |

**Damage Calculation:**

```
damage = ((2 * level / 5 + 2) * power * (atk / def)) / 50 + 2
damage *= STAB       // 1.5 if move type matches dino type
damage *= typeEffect  // 0, 0.5, 1, or 2 from type_chart.json
damage *= random(0.85, 1.0)
```

**AI (Wild dinos):** Random move selection from available moves.
**AI (Trainer dinos):** Weighted selection favoring super-effective moves, with a chance to use items at low HP.

**Key interfaces:**

```typescript
interface IBattleState {
  phase: 'INTRO' | 'PLAYER_TURN' | 'ENEMY_TURN' | 'RESOLUTION' | 'END';
  playerDino: IDinoInstance;
  enemyDino: IDinoInstance;
  isWild: boolean;
  turnOrder: 'player' | 'enemy';  // Based on speed comparison
  currentWeather?: WeatherType;
}
```

**Communication:** BattleScene owns the BattleSystem. The system emits events (`'move-executed'`, `'dino-fainted'`, `'battle-end'`) that the scene listens to for triggering UI updates and animations.

---

### 4.2 OverworldScene

**Tile-based movement:**

- Player moves in 16x16 tile increments (standard GBA RPG tile size).
- Movement is grid-locked: press a direction, tween the player sprite 16px over ~200ms.
- No diagonal movement (classic GBA style).
- Collision checking happens BEFORE the tween starts by reading the collision layer.

**Layer system (Tiled):**

| Layer          | Purpose                                    |
| -------------- | ------------------------------------------ |
| `ground`       | Base terrain (grass, water, paths)         |
| `collision`    | Invisible tiles marking impassable areas   |
| `above-player` | Treetops, roofs -- rendered above player   |
| `events`       | Invisible tiles for triggers (doors, signs)|

**Encounter system:**

- Tiles marked as "tall grass" in the event layer trigger an encounter check on each step.
- Base encounter rate: 8% per step in grass.
- Rate modified by items (e.g. Repel sets rate to 0).
- When triggered, transition to BattleScene with a spiral/fade effect.

**NPC interaction:**

- NPCs have a facing direction and a trigger zone (the tile in front of them).
- Press action button facing an NPC to trigger their dialogue script.
- Trainer NPCs have a "line of sight" (3-5 tiles ahead); stepping into it triggers a forced battle.

**Communication:** OverworldScene reads map data from Tiled JSON. It creates Player and NPC entities. Player input is handled by Phaser's cursor keys. Scene transitions use `this.scene.start()` or `this.scene.launch()` for overlays (dialogue).

---

### 4.3 SaveSystem

**What gets serialized:**

```typescript
interface ISaveData {
  version: string;           // Schema version for migration
  timestamp: number;         // Date.now() at save time
  player: {
    name: string;
    position: { map: string; x: number; y: number };
    facing: Direction;
    badges: boolean[];       // 8 gym badges
    money: number;
    playtime: number;        // Seconds
  };
  party: IDinoInstance[];    // Up to 6
  storage: IDinoInstance[];  // PC box
  inventory: IInventorySlot[];
  dinodex: IDinodexEntry[];  // Seen/caught flags per species
  flags: Record<string, boolean>;  // Story progress flags
  defeatedTrainers: string[];      // Trainer IDs already beaten
}
```

**LocalStorage schema:**

- Key: `jurassic-trainers-save-{slot}` where slot is 1, 2, or 3.
- Value: JSON string of `ISaveData`, compressed via LZ-string if size becomes an issue.
- Max 3 save slots (classic RPG convention).

**Save/Load flow:**

1. **Save:** Player opens menu, selects "Save". System serializes current GameState into ISaveData, writes to LocalStorage. Confirmation message displayed.
2. **Load:** Title screen shows save slots with preview (player name, playtime, badge count). Selecting a slot deserializes ISaveData and hydrates GameState. OverworldScene loads the saved map and positions the player.
3. **Auto-save:** Not implemented initially. Manual save only (authentic GBA experience).

---

### 4.4 DialogueSystem

**Architecture:**

The DialogueSystem is a queue-based text renderer that overlays on top of any scene.

**Components:**

- `DialogueBox` (UI): Renders the text box at the bottom of the screen. Supports typewriter effect (character-by-character reveal at configurable speed).
- `DialogueScene` (Scene): Launched as an overlay scene. Pauses the underlying scene's input.

**Text queue:**

```typescript
interface IDialogueLine {
  speaker?: string;          // Name shown above text box
  portrait?: string;         // Sprite key for portrait (optional)
  text: string;              // The dialogue text
  choices?: IDialogueChoice[];  // If present, show choice menu after text
  onComplete?: () => void;   // Callback when this line is dismissed
}

interface IDialogueChoice {
  label: string;
  onSelect: () => void;
}
```

**Flow:**

1. A scene calls `DialogueSystem.show(lines: IDialogueLine[])`.
2. DialogueScene is launched as overlay, first line begins typewriting.
3. Player presses action button: if text is still typing, complete it instantly; if complete, advance to next line.
4. If a line has `choices`, render choice buttons and wait for selection.
5. After all lines are processed, DialogueScene shuts down and returns control to the parent scene.

**Branching:** Handled by the caller. Choice callbacks can push new lines to the queue or set game flags that alter future dialogue.

---

## 5. Data Schemas

### DinoSpecies (base data -- dinos.json)

```typescript
interface IDinoSpecies {
  id: number;                // 1-150
  name: string;              // e.g. "Raptorling"
  types: DinoType[];         // 1-2 types
  baseStats: {
    hp: number;
    attack: number;
    defense: number;
    spAttack: number;
    spDefense: number;
    speed: number;
  };
  learnset: ILearnsetEntry[];  // Moves learned by level
  evolution?: {
    into: number;            // Species ID to evolve into
    level: number;           // Level required
    condition?: string;      // Special condition (item, trade, etc.)
  };
  captureRate: number;       // 1-255, higher = easier to catch
  expYield: number;          // Base XP given when defeated
  description: string;       // Dinodex entry text
  spriteKey: string;         // Asset key for sprite sheet
  cry?: string;              // Audio key for cry SFX
}

interface ILearnsetEntry {
  level: number;
  moveId: number;
}
```

### DinoInstance (a specific dino)

```typescript
interface IDinoInstance {
  uid: string;               // Unique ID (UUID)
  speciesId: number;         // Reference to IDinoSpecies
  nickname?: string;         // Player-given name
  level: number;             // 1-100
  xp: number;               // Current XP
  currentHP: number;
  stats: {                   // Calculated from base + IVs + level
    hp: number;
    attack: number;
    defense: number;
    spAttack: number;
    spDefense: number;
    speed: number;
  };
  ivs: {                    // Individual values (0-31), set at creation
    hp: number;
    attack: number;
    defense: number;
    spAttack: number;
    spDefense: number;
    speed: number;
  };
  moves: (number | null)[];  // Up to 4 move IDs
  movePP: number[];          // Current PP for each move
  status?: StatusEffect;     // Poison, Burn, etc.
  ownerId: string;           // 'player' or trainer ID
}
```

### Move

```typescript
interface IMove {
  id: number;
  name: string;
  type: DinoType;
  category: 'physical' | 'special' | 'status';
  power: number;             // 0 for status moves
  accuracy: number;          // 1-100
  pp: number;                // Max power points
  effect?: IMoveEffect;
  description: string;
  animation?: string;        // Animation key
}

interface IMoveEffect {
  type: 'status' | 'stat-change' | 'heal' | 'recoil' | 'multi-hit';
  target: 'self' | 'enemy';
  value: string | number;    // e.g. 'poison', -1 (stat stage), 50 (% heal)
  chance: number;            // Probability 0-100
}
```

### Item

```typescript
interface IItem {
  id: number;
  name: string;
  category: 'capture' | 'healing' | 'battle' | 'key' | 'evolution';
  effect: string;            // Effect identifier
  value: number;             // Heal amount, capture rate modifier, etc.
  price: number;             // Shop price (0 = not buyable)
  description: string;
  spriteKey: string;
}

interface IInventorySlot {
  itemId: number;
  quantity: number;
}
```

### TrainerData

```typescript
interface ITrainerData {
  id: string;
  name: string;
  class: string;             // "Youngster", "Gym Leader", etc.
  spriteKey: string;
  party: ITrainerDino[];
  reward: number;            // Money given on defeat
  dialogue: {
    before: string;          // Dialogue key for pre-battle
    after: string;           // Dialogue key for post-battle
  };
  ai: 'random' | 'smart' | 'gym';  // AI difficulty
}

interface ITrainerDino {
  speciesId: number;
  level: number;
  moves: number[];
}
```

### MapData (Tiled export supplement)

```typescript
interface IMapData {
  id: string;                // e.g. "route_01"
  name: string;              // Display name
  tiledFile: string;         // Path to Tiled JSON
  connections: IMapConnection[];
  encounters?: IEncounterTable;
  music: string;             // Audio key for BGM
}

interface IMapConnection {
  direction: 'north' | 'south' | 'east' | 'west';
  targetMap: string;
  targetX: number;
  targetY: number;
}

interface IEncounterTable {
  rate: number;              // Base encounter rate (0-100)
  entries: IEncounterEntry[];
}

interface IEncounterEntry {
  speciesId: number;
  levelRange: [number, number];
  weight: number;            // Relative probability
}
```

### GameState (runtime)

```typescript
interface IGameState {
  player: {
    name: string;
    position: { map: string; x: number; y: number };
    facing: Direction;
    badges: boolean[];
    money: number;
    playtime: number;
  };
  party: IDinoInstance[];
  storage: IDinoInstance[];
  inventory: IInventorySlot[];
  dinodex: Map<number, { seen: boolean; caught: boolean }>;
  flags: Map<string, boolean>;
  defeatedTrainers: Set<string>;
}
```

---

## 6. Performance Considerations

### Asset Loading Strategy

- **Per-scene preloading:** BootScene loads global assets (UI elements, common sprites, music). Each subsequent scene loads only what it needs.
- **Lazy load maps:** Map tilesets and tilemap JSON are loaded when the player enters a new area, during the transition screen.
- **Dino sprites:** Loaded on-demand when entering battle. Only the two active dino sprite sheets need to be in memory at once.

### Sprite Sheet Optimization

- All dino sprites should be consolidated into sprite sheets (e.g., 10 dinos per 640x640 atlas).
- Use TexturePacker or a similar tool with `--trim` and `--max-size 1024` for optimal GPU texture usage.
- Target sprite size: 64x64 per dino frame (front/back, idle/attack/faint = ~6 frames each).
- Player/NPC overworld sprites: 16x24 per frame, 4 directions x 3 frames = 12 frames per character.

### Map Chunking

- Not needed initially. GBA maps are small (typically 30x30 to 60x60 tiles).
- If we exceed ~100x100 tiles per map, implement chunked rendering (only render tiles within camera viewport + 2-tile margin).

### Memory Management

- Phaser's texture manager handles most of this.
- After leaving a battle, unload the opponent dino's sprite sheet if it is not in the player's party.
- Keep a running count of loaded textures; if it exceeds a threshold (~50 MB), trigger a cleanup of unused textures.
- Audio: Use Web Audio API (Phaser default). Stream long BGM tracks, preload short SFX.

### General Guidelines

- Avoid creating objects in the game loop. Pre-allocate and reuse.
- Use object pools for particle effects and floating damage numbers.
- Profile with Chrome DevTools Performance tab regularly.
- Keep JSON data files under 500 KB each; compress with gzip on the server.

---

## 7. Development Phases

### Phase 1: Overworld Movement + First Map (Weeks 1-2)

- Set up project (Vite + Phaser + TypeScript). **[This document covers this.]**
- Create BootScene and TitleScene.
- Build first map in Tiled (starter town, ~30x30 tiles).
- Implement grid-based player movement with collision.
- Add camera follow and layer rendering.
- Basic NPC placement (no dialogue yet).

### Phase 2: Battle System Core (Weeks 3-5)

- Implement BattleScene with state machine.
- Damage calculation and type chart.
- Move selection UI (BattleHUD).
- Wild encounter triggering from overworld.
- Basic battle animations (slide-in, flash on hit, faint).
- HP bar and XP bar UI.

### Phase 3: Capture + Party Management (Weeks 6-7)

- Capture mechanics and capture ball animation.
- Party system (swap dinos, view stats).
- Inventory system (use items in/out of battle).
- XP and leveling.
- Evolution checks and animation.
- Dinodex (mark seen/caught).

### Phase 4: Full Content Integration (Weeks 8-12)

- All 150 dino species data.
- All moves, items, and trainer data.
- All maps and routes.
- NPC dialogue and story events.
- Gym battles and badge progression.
- Shops and healing centers.

### Phase 5: Polish, Save System, Menus (Weeks 13-15)

- Save/Load system with 3 slots.
- Pause menu (Party, Bag, Dinodex, Save, Options).
- Screen transitions (battle intro spiral, map fade).
- Sound effects and music integration.
- Options menu (text speed, sound volume).
- Bug fixes and performance optimization.
- Playtesting and balance tuning.

---

## Appendix: Type Chart

14 types for Jurassic Trainers:

| #  | Type      |
| -- | --------- |
| 0  | Normal    |
| 1  | Fire      |
| 2  | Water     |
| 3  | Earth     |
| 4  | Air       |
| 5  | Electric  |
| 6  | Ice       |
| 7  | Venom     |
| 8  | Flora     |
| 9  | Fossil    |
| 10 | Shadow    |
| 11 | Light     |
| 12 | Metal     |
| 13 | Primal    |

The full 14x14 effectiveness matrix will be defined in `data/type_chart.json`.
