# Dino Monsters — Complete Chronological History

> This file documents everything that was done on the project, in chronological order.
> A new session should read this to understand the full trajectory.

---

## Phase 0: Project Setup & Brainstorm

### What happened
- User described the game concept: a Pokemon clone with dinosaurs, GBA Fire Red style
- Core vision: 150 dinos, 8 gyms, Elite 4, rival, story about extinction, region called Pangaea
- Created the **Nova Forge Studio** framework — a virtual game dev studio with specialized AI agents
- Set up 25+ agents across 10 departments (design, tech, art, narrative, balance, QA, animation, dino-design, business, direction)
- Established governance rules (GOVERNANCE.md), production pipeline (WORKFLOW.md), memory system (MEMORY.md), dispatch system (DISPATCH.md)
- Ran a full brainstorm with all agents — each contributed from their specialty
- KAELEN (Creative Director) synthesized the brainstorm into a unified direction
- User validated the direction

### Key outputs
- Game concept: "Jurassic Trainers" (later renamed "Dino Monsters")
- 14 types instead of 18 (fused some for simplicity): Normal, Fire, Water, Earth, Air, Electric, Ice, Venom, Flora, Fossil, Shadow, Light, Metal, Primal
- 150 dinos in 50 evolution lines (3 stages: Baby/Young/Adult)
- 3 starters: PYREX (Fire), AQUADON (Water), FLORASAUR (Flora)
- 10 towns, 9 routes, 8 gyms, Elite 4 + Champion GENESIS
- Villain team: Escadron Meteore (Commandant Impact)
- Legendary: PANGAEON (Fossil/Light guardian)

---

## Phase 1: Pre-Production (Documents)

### What happened
- All design documents written by the agents in parallel
- Each document went through review and iteration

### Documents produced (in `poc/game/docs/`)
1. **GDD.md** — Full Game Design Document with damage formulas, capture mechanics, XP curves, 25 temperaments, GV/TP system (IV/EV equivalent), 14x14 type chart
2. **ART_DIRECTION.md** — Visual direction with hex color palettes per biome/type/UI, sprite specs, animation rules, starter designs
3. **NARRATIVE_BIBLE.md** — 3-act story, 8 gym leaders, 4 Elite, Champion, Rival REX (6 encounters), Escadron Meteore (4 lieutenants + Commandant Impact), side quests
4. **WORLD_BIBLE.md** — 65M-year timeline, geography, culture, ecology, mythology
5. **DINODEX.md** — All 150 dinos with stats, types, abilities, designs, habitats
6. **TYPE_CHART.md** — 14x14 type effectiveness matrix with 3 immunities
7. **MOVES_AND_ITEMS.md** — 120 attacks + 80 items (potions, balls, held items, keys, TMs)
8. **TECH_ARCHITECTURE.md** — Stack decision (Phaser 3 + TypeScript + Vite), system designs

---

## Phase 2: 2D Prototype (Phaser 3 + TypeScript)

### What happened
- Built a complete 2D game as a proof of concept
- Stack: Phaser 3.80 + TypeScript 5 + Vite
- All game data stored as JSON files: 150 dinos, 131 moves, 80 items, 14x14 type chart
- Implemented the full core loop: explore, battle, capture, evolve, progress

### Systems implemented
- **BattleSystem.ts** — Turn-based combat with damage formula, STAB, type effectiveness, criticals, status effects, capture, trainer AI
- **TypeChart.ts** — Type effectiveness lookup
- **InventorySystem.ts** — Bag management by category
- **PartySystem.ts** — Team of 6 + PC storage
- **SaveSystem.ts** — localStorage with 3 slots
- **GameState.ts** — Global singleton state
- **EventSystem.ts** — Story event triggers and flags
- **AudioSystem.ts** — Procedural music and SFX
- **AnimationSequencer.ts** — Battle animation sequencing
- **CameraFX.ts** — Camera effects (shake, zoom)
- **ParticlePool.ts** — Particle effects system

### Scenes (15 total)
BootScene, TitleScene, StarterSelectScene, OverworldScene, BattleScene, DinoCenterScene, ShopScene, PartyScene, BagScene, DinodexScene, EvolutionScene, GymScene, EliteFourScene, HouseScene

### Maps
- 21 connected maps: 10 towns, 9 routes, 2 dungeons (Grotte des Ancetres, Tour des Fossiles)
- All with warp transitions between them

### Story content
- 30+ story events across 3 acts
- Rival REX (6 progressive encounters)
- Escadron Meteore (grunts, 4 lieutenants, Commandant Impact)
- 8 Gym Leaders with badges and TMs
- Elite 4 + Champion GENESIS
- Legendary PANGAEON quest (post-game)
- Credits with Nova Forge Studio team

### Visual upgrades
- Resolution bumped from 240x160 to 640x400 (modern quality, user feedback)
- Press Start 2P font for readability
- Amber signature palette with triple-layer borders
- Detailed procedural dino sprites (body, head, eyes, legs, typed tail, particles)
- Detailed player character (cap, hair, clothes)
- NPCs visually distinct by role
- Detailed tiles (grass with blades, water with waves, trees with canopy)

### Testing
- 8 test files with 134 tests total (balance, capture rate, damage calc, data integrity, dino stats, learnset rules, progression, type effectiveness)
- Tests run in pure Node.js (no Phaser dependency)
- Balance simulation (balanceSim.ts) to detect one-shot problems
- All 134 tests passing, execution < 500ms

### Bug fix cycles (user feedback driven)
1. **"Pyrex displays as blue"** — Type mapping bug. JSON numbering (0=Fossil) differs from code enum (0=Normal). Fixed with mapType() in BootScene.ts. Documented in TYPE_MAPPING.md.
2. **"My fire dino does almost no damage to water dino"** — Fallback stats were asymmetric (enemy had 2x defense). Fixed with buildTrainerDino() using real baseStats.
3. **"Battles are not balanced"** — Move power too high, HP too low, no physical/special split. Fix: -30% move power, +40% HP, +20% Def, added physical/special split. Target: 3-6 turn battles.
4. **"Can't enter houses, no doors"** — Paths were overwriting door tiles. Fixed with restoreDoors() called after path generation.
5. **"Style too pixelated, text barely readable"** — Resolution upgrade 240x160 to 640x400, font size increases.
6. **"Graphics play a major role in game quality"** — Integrated as permanent constraint: always prioritize art direction.
7. **Polish cycle** — PP sync bug, evolution guard, status icons, capture animation, HP bar colors, XP animation, item descriptions, battle fade, damage numbers, audio safety. 13 issues fixed in 3 waves (P0, P1, P2+P3). 0 regressions.

---

## Phase 3: Unity 3D Port

### What happened
- User wanted a 3D version of the game
- Moved the entire POC to `poc/` folder
- Created Unity 2022 LTS project in `DinoMonsters/`
- Ported all game systems from TypeScript to C#
- All graphics are procedural (primitive 3D shapes, no external art assets)

### Unity project structure
- 4 scenes: Title, StarterSelect, Overworld, Battle
- 38 C# scripts across 8 folders (Audio, Battle, Core, Data, Dinos, Overworld, Save, UI)
- JSON data copied to StreamingAssets (dinos.json, moves.json, items.json, type_chart.json)

### Core scripts ported
- **GameState.cs** — Persistent state (DontDestroyOnLoad singleton)
- **GameBootstrap.cs** — Scene initialization, singleton setup, AssetLibrary (currently DISABLED)
- **InventorySystem.cs** — Bag management
- **BattleSystem.cs** — Turn-based combat logic
- **DamageCalculator.cs** — Damage formula with physical/special split
- **SaveSystem.cs** — JSON-based save/load
- **DataLoader.cs** — Loads JSON data from StreamingAssets
- **Dino.cs** — Dino entity with stats, moves, evolution
- **StoryEventSystem.cs** — 14+ story events with flag system
- **AudioManager.cs** — Procedural music generation + SFX
- **WeatherSystem.cs** — Visual weather effects
- **MaterialManager.cs** — Consistent material creation for procedural objects

### Overworld system
- **OverworldManager.cs** (~3789 lines) — Main game file handling all 21 maps, tile rendering, NPC placement, encounters, trainers, transitions
- **BuildingRenderer.cs** — Procedural 3D buildings with typed colors and doors
- **PlayerController.cs** — Tile-based movement
- **OverworldCamera.cs** — Isometric-ish camera (distance 14, pitch 55)
- **NPCController.cs** — NPC behavior
- **InteriorManager.cs** — Building interiors
- **WeatherSystem.cs** — Rain, snow, sandstorm visuals

### UI system
- **TitleScreenUI.cs** — Title with New Game / Continue
- **StarterSelectUI.cs** — Starter selection with 3D models
- **BattleUI.cs** — Battle HUD with HP bars, menus
- **DialogueUI.cs** — Typewriter dialogue (DontDestroyOnLoad singleton)
- **OverworldHUD.cs** — Map name, party preview
- **PartyUI.cs** — Team management
- **PauseMenuUI.cs** — Pause with save option
- **WorldMapUI.cs** — Region overview
- **ScreenFade.cs** — Scene transition fades (uses unscaledDeltaTime)

### Battle system
- **BattleSceneManager.cs** — Battle flow controller
- **BattleEffects3D.cs** — Visual effects during battle
- **DinoModelGenerator.cs** — Procedural 3D dino models from primitives

### Fix cycles in Unity
1. **EventSystem missing** — Unity requires EventSystem for UI clicks. Each UI script now checks and creates one if missing.
2. **Battle HP applied to wrong dino** — Fixed target/source confusion in damage application.
3. **Respawn position lost** — GameState now persists respawn coordinates across scenes.
4. **Interior dialogue deadlock** — DialogueUI was null because it wasn't DontDestroyOnLoad. Fixed in GameBootstrap.
5. **Door visibility** — Doors rendered as Quads were invisible from some camera angles. Replaced with Cubes.
6. **Doors not aligned** — Building doors were not on walkable tiles. Fixed door positioning to match TILE_PATH.
7. **Tall brown wall blocks** — TILE_WALL was rendering as tall cubes blocking the view. Flattened to thin slabs.
8. **Camera too close** — Zoomed out to distance 14, pitch 55 for better overview.

---

## Phase 4: Asset Integration Attempt (ABANDONED)

### What happened
- Attempted to integrate external 3D asset packs:
  - **Slavic Medieval Town Kit** (EmaceArt) — Building parts (walls, roofs, foundations, windows, doors)
  - **Modular Characters** (Normalgon) — Character models
- Created BuildingWorkshop system and FORGE pipeline for assembling buildings from prefab parts
- Created OBJ export pipeline

### Problems encountered
- **Pink materials** — URP to Standard shader mismatch. MaterialFixer attempted runtime conversion but EmaceArt's color-sheet uses UV mapping incompatible with runtime Standard shader materials.
- **Wrong scales** — Prefab scale vs tile grid mismatch. Buildings too big or too small.
- **Wall orientation** — Z-wide panels designed for modular editor construction. Programmatic assembly produced misaligned/overlapping walls.
- **Buildings not covering tile footprint** — Walls face inward (designed for room interiors). From overworld camera, you see through back walls.

### Resolution
- **ABANDONED** the prefab assembly approach after 5+ iterations
- Kept procedural buildings (cubes with MaterialManager colors)
- Cherry-picked useful fixes from the feature branch back to main:
  - Door alignment with walkable tiles
  - Camera distance adjustment
  - Flat wall rendering
- The `feature/asset-integration` branch was abandoned (no longer exists)

---

## Phase 5: Studio Framework Formalization

### What happened
- Formalized the Nova Forge Studio with industrial-grade processes
- Created 6 reusable skills in `poc/studio/skills/`:
  1. **implement-feature** — Step-by-step feature implementation methodology
  2. **create-dino** — Dino creation pipeline (stats, moves, evolution, balance)
  3. **balance-check** — Balance verification methodology
  4. **debug-and-fix** — Systematic debugging process
  5. **generate-tests** — Test generation methodology
  6. **create-agent** — Agent creation process

### Protocols established
- **INVOKE** — Agent requests help from another agent
- **REVIEW** — Agent reviews another's work
- **ESCALATE** — Agent escalates a blocker to KAELEN

### Memory system
- `studio/memory/project/feedback.md` — All user feedback with interpretation, impact, action, status
- `studio/memory/project/conventions.md` — Technical conventions discovered during development
- `studio/memory/phases/` — Phase completion records
- `studio/memory/agents/` — Per-agent memory (empty, ready for use)

### Reusable init prompt
- Created `NOVA_FORGE_INIT_PROMPT.md` — A complete prompt that can initialize Nova Forge Studio on any other game project
