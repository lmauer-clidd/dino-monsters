# Dino Monsters — Current State (as of 2026-03-27)

> Exact snapshot of where the project stands right now.
> Read this FIRST in any new session to know what is working, what is broken, and what to do next.

---

## Git Status

- **Branch**: main
- **Remote**: origin/main at https://github.com/lmauer-clidd/dino-monsters.git
- **Last commits** (most recent first):
  1. `b18a828` Replace door Quads with Cubes — visible from all camera angles
  2. `b83fd0b` Align building doors with walkable door tiles
  3. `d7edd96` Flatten TILE_WALL rendering — no more tall brown blocks
  4. `8098db6` Camera: zoom out to distance 14, pitch 55 for better overview
  5. `72df39d` Remove large UnityHubSetup.exe from tracking
  6. `53534e5` Dino Monsters — Full game (POC 2D + Unity 3D)
- **Untracked files**: CLAUDE.SESSION.md, context/
- **Other branches**: none (feature/asset-integration was abandoned and deleted)

---

## Project Structure

```
E:\Claude workspace\JV - Dino monsters\
├── poc/                         ← POC 2D Phaser (COMPLETE)
│   ├── game/
│   │   ├── docs/                ← 8 design documents (GDD, Art, Narrative, etc.)
│   │   └── src/
│   │       ├── data/            ← JSON: 150 dinos, 131 moves, 80 items, type chart
│   │       ├── entities/        ← Dino.ts
│   │       ├── scenes/          ← 15 scenes (Boot, Title, Battle, Overworld, etc.)
│   │       ├── systems/         ← 10 systems (Battle, TypeChart, Inventory, etc.)
│   │       ├── ui/              ← 4 UI components (DialogueBox, BattleHUD, etc.)
│   │       ├── utils/           ← damageCalc.ts, balanceSim.ts, constants.ts
│   │       └── __tests__/       ← 8 test files, 134 tests
│   └── studio/
│       ├── agents/              ← 30 agent personality files across 10 departments
│       ├── skills/              ← 6 formalized skill methodologies
│       ├── memory/              ← Phase records + project feedback/conventions
│       ├── GOVERNANCE.md
│       ├── WORKFLOW.md
│       ├── SKILLS.md
│       ├── MEMORY.md
│       └── DISPATCH.md
│
├── DinoMonsters/                ← Unity 3D game (IN PROGRESS)
│   └── Assets/
│       ├── Scenes/              ← 4 scenes: Title, StarterSelect, Overworld, Battle
│       ├── Scripts/             ← 38 C# scripts
│       │   ├── Audio/           ← AudioManager.cs
│       │   ├── Battle/          ← BattleSceneManager, BattleSystem, DamageCalculator, BattleEffects3D
│       │   ├── Core/            ← GameBootstrap, GameState, GameManager, Constants, DinoType,
│       │   │                      InventorySystem, MaterialManager, MaterialFixer, InputHelper,
│       │   │                      InputHelperUpdater, AssetLibrary (DISABLED), PrefabLoader,
│       │   │                      StoryEventSystem, AudioManager
│       │   ├── Data/            ← DataLoader.cs
│       │   ├── Dinos/           ← Dino.cs, DinoModelGenerator.cs
│       │   ├── Overworld/       ← OverworldManager (~3789 lines), BuildingRenderer,
│       │   │                      PlayerController, OverworldCamera, NPCController,
│       │   │                      InteriorManager, WeatherSystem
│       │   ├── Save/            ← SaveSystem.cs
│       │   └── UI/              ← TitleScreenUI, StarterSelectUI, BattleUI, DialogueUI,
│       │                          OverworldHUD, PartyUI, PauseMenuUI, WorldMapUI, ScreenFade
│       ├── StreamingAssets/     ← JSON data (dinos, moves, items, type_chart)
│       └── TextMesh Pro/        ← Font assets
│
├── context/                     ← Session memory files (THIS DIRECTORY)
│   ├── HISTORY.md               ← Chronological journal
│   ├── DECISIONS.md             ← Technical decisions + rationale
│   ├── LESSONS.md               ← What worked / didn't work
│   └── CURRENT_STATE.md         ← THIS FILE
│
├── CLAUDE.md                    ← Project instructions (Nova Forge Studio)
├── CLAUDE.SESSION.md            ← Session bootstrap instructions
├── NOVA_FORGE_INIT_PROMPT.md    ← Reusable prompt for initializing Nova Forge on other projects
├── README.md                    ← Project readme
└── UnityHubSetup.exe            ← Unity installer (should probably be gitignored)
```

---

## What Is Working (Unity 3D)

### Complete game flow
- Title screen -> New Game / Continue
- Starter selection (3 starters with procedural 3D models)
- Overworld exploration (tile-based, 21 maps)
- Map transitions between all towns and routes
- Wild dino encounters in tall grass
- Turn-based battles with full damage calculation (STAB, types, crits, physical/special)
- Dino capture with Jurassic Balls
- Evolution system
- Trainer battles (route trainers, gym leaders)
- Story events (Rival Rex, Escadron Meteore, gym badges)
- Item usage (potions, status heals, balls)
- Dino Center (free healing)
- Shops (buy/sell per town)
- Save/Load system
- Pause menu
- World map overview

### Procedural 3D graphics
- Buildings colored by type (gym = type color, shop = green, center = pink, house = brown)
- Building doors as Cubes (visible from all angles), aligned with walkable tiles
- Flat wall tiles (thin slabs, not tall blocks)
- Terrain tiles with type-appropriate colors
- Procedural dino models from primitives (capsule body, sphere head, cylinder legs)
- Camera at distance 14, pitch 55 for good overview

### Audio & Effects
- AudioManager with procedural music generation
- Sound effects for battles, menus, transitions
- WeatherSystem (rain, snow, sandstorm visuals)
- ScreenFade for scene transitions (works during pause)

---

## What Needs Work (Unity 3D)

### Visual quality (PRIORITY — user cares deeply about art)
- Buildings are functional but basic colored cubes — need better proportions, details (windows, chimneys, signs)
- Character models are still capsule+sphere primitives — need proper character models
- Dino models are primitive assemblies — need more detailed/recognizable shapes
- Interior scenes are functional but visually basic
- No particle effects for environment (leaves, dust, etc.)
- Terrain could benefit from height variation

### Asset integration
- Normalgon character models are imported but not properly integrated
- Need to find/create dino models (or improve procedural generation significantly)
- Need to integrate character models for player, NPCs, trainers

### Gameplay polish
- End-to-end gameplay test (Title -> Champion GENESIS) not yet performed
- Gamepad D-pad support is incomplete
- Some buildings may be slightly offset from the tile grid
- Battle animations could be more dynamic

### Technical debt
- OverworldManager.cs is ~3789 lines — should be split into smaller files
- AssetLibrary is disabled — clean up or properly integrate
- MaterialFixer may have leftover code from the abandoned asset integration

---

## Key Files to Know

| File | Purpose | Notes |
|------|---------|-------|
| `DinoMonsters/Assets/Scripts/Core/GameBootstrap.cs` | Singleton initialization | Creates DialogueUI, GameState, AudioManager. AssetLibrary is DISABLED. |
| `DinoMonsters/Assets/Scripts/Overworld/OverworldManager.cs` | Main game logic | ~3789 lines. All 21 maps, NPCs, encounters, transitions. |
| `DinoMonsters/Assets/Scripts/Overworld/BuildingRenderer.cs` | Procedural buildings | Cubes with MaterialManager colors, doors aligned with tiles. |
| `DinoMonsters/Assets/Scripts/Battle/BattleSceneManager.cs` | Battle flow | Turn order, move execution, battle end conditions. |
| `DinoMonsters/Assets/Scripts/Battle/DamageCalculator.cs` | Damage formula | Physical/special split, STAB, type effectiveness, crits. |
| `DinoMonsters/Assets/Scripts/Core/GameState.cs` | Persistent state | Party, inventory, flags, badges. DontDestroyOnLoad. |
| `DinoMonsters/Assets/Scripts/Dinos/Dino.cs` | Dino entity | Stats, moves, evolution, type. |
| `DinoMonsters/Assets/Scripts/Core/StoryEventSystem.cs` | Story events | 14+ events with flag-based triggering. |
| `DinoMonsters/Assets/Scripts/UI/DialogueUI.cs` | Dialogue system | DontDestroyOnLoad singleton. MUST exist in all scenes. |
| `DinoMonsters/Assets/Scripts/UI/ScreenFade.cs` | Scene transitions | Uses unscaledDeltaTime for pause compatibility. |
| `DinoMonsters/Assets/Scripts/Core/MaterialManager.cs` | Material creation | Consistent procedural materials. The reliable approach. |
| `poc/game/src/data/dinos.json` | All 150 dinos | Shared between POC and Unity (via StreamingAssets). |
| `poc/game/src/data/moves.json` | All 131 moves | Shared between POC and Unity. |
| `poc/studio/memory/project/feedback.md` | User feedback log | Check before making visual/balance decisions. |
| `poc/studio/memory/project/conventions.md` | Technical conventions | Check before writing game logic code. |

---

## Next Priority Tasks (Suggested Order)

1. **End-to-end gameplay test** — Play from Title screen to Champion GENESIS. Document any blockers, bugs, or missing content.
2. **Polish procedural buildings** — Add windows, chimneys, signs, better proportions. This addresses the user's primary concern (visual quality).
3. **Improve procedural dino models** — More recognizable silhouettes, type-specific features.
4. **Character model integration** — Properly integrate Normalgon models for player and NPCs.
5. **Gamepad D-pad support** — Complete gamepad input for menus and overworld.
6. **OverworldManager refactoring** — Split the 3789-line file into map generation, NPC management, encounter logic, and transition management.
7. **Build for PC** — Create a standalone build and test outside the Unity editor.

---

## Important Reminders

- **Art quality is the #1 user priority** — Every visual decision must target modern Pokemon quality, not placeholder quality. Read `feedback.md`.
- **Always check conventions.md** before writing combat/data/event code.
- **JSON type mapping is a trap** — JSON uses different type numbering than the code enum. Always use mapType() or the equivalent conversion.
- **DialogueUI must be DontDestroyOnLoad** — If it gets destroyed, the game deadlocks.
- **EventSystem must exist for UI clicks** — Self-heal pattern: check and create if missing.
- **Set event flags BEFORE scene transitions** — scene.start() destroys the current scene and all callbacks.
