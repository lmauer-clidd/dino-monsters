# Dino Monsters — Technical Decisions & Rationale

> Every significant technical decision made on this project, with the reasoning behind it.
> Read this to understand WHY things are the way they are.

---

## Game Design Decisions

### 1. 14 types instead of 18 (like Pokemon)
**Decision**: Normal, Fire, Water, Earth, Air, Electric, Ice, Venom, Flora, Fossil, Shadow, Light, Metal, Primal
**Rationale**: Simpler chart but still deep strategy. Fused overlapping types (Wind into Air, Swamp into Venom/Water, Magma into Fire/Earth, Crystal into Metal/Light). 14x14 is manageable to balance manually. 3 immunities for strategic depth.

### 2. Physical/Special split
**Decision**: Moves have a `category` field: physical, special, or status. Physical uses Atk/Def, special uses SpAtk/SpDef.
**Rationale**: Added after balance testing. Originally all attacks used the same stat pair, which made some types overpowered (high Atk types with "special" moves). Split gave each dino a meaningful stat profile.

### 3. Charge + Grondement at Level 1 for all dinos
**Decision**: Every dino in the game starts with moveId 121 (Charge, Normal/Physical/35) and moveId 122 (Grondement, Normal/Status/lowers Atk).
**Rationale**: No typed attacks before Level 7. This prevents early one-shots from type advantage. All starters are equal at the beginning. Forces players to survive the early game on neutral damage.

### 4. Move power tiers
**Decision**: Early 20-35 / Mid 45-60 / Late 65-80 / Endgame 85-100
**Rationale**: Original powers were too high, causing 1-2 turn battles. Reduced all move powers by 30% and boosted HP by 40%. Target combat length: 3-6 turns. Validated with balance simulation tests.

### 5. 150 dinos in 50 evolution lines of 3 stages
**Decision**: 50 Baby -> 50 Young -> 50 Adult. Evolution at levels 14-20 (stage 1) and 30-38 (stage 2).
**Rationale**: Clean 3-stage system mirrors Pokemon's approach. 50 lines is enough for variety across 14 types. Level thresholds pace evolution with gym progression.

---

## Technical Stack Decisions

### 6. Phaser 3 + TypeScript + Vite for the POC
**Decision**: Browser-based 2D game engine with TypeScript for type safety and Vite for fast builds.
**Rationale**: Fast iteration, no compilation wait, instant hot-reload in browser. TypeScript catches bugs at compile time. Vite is the fastest bundler. Perfect for proving out all mechanics before committing to a bigger engine.

### 7. Unity 2022 LTS for 3D version
**Decision**: Unity (not Unreal, not Godot) for the 3D port.
**Rationale**: User wanted 3D. Unity has the most accessible C# scripting, huge community, and 2022 LTS is stable. The procedural approach (no pre-made assets) works well with Unity's primitive creation API.

### 8. Procedural 3D over asset packs
**Decision**: Keep buildings, dinos, and terrain as procedural primitives (cubes, spheres, capsules) with MaterialManager colors.
**Rationale**: Tried EmaceArt Slavic Medieval Town Kit but wall/roof assembly was unreliable programmatically. Prefab parts are designed for manual editor placement, not runtime assembly. Pink material issues (URP vs Standard shader), scale mismatches, and wall orientation problems made it impractical. Procedural cubes are consistent, predictable, and always render correctly.

### 9. No namespaces in Unity scripts
**Decision**: All 38 C# scripts in global namespace.
**Rationale**: Simplicity. With 38 scripts there's no naming collision risk. Avoids `using` boilerplate. Consistent across the entire codebase.

---

## Architecture Decisions

### 10. JSON type numbering differs from code enum
**Decision**: JSON files use their own numbering (0=Fossil, 1=Water, 2=Fire...) while C# code uses DinoType enum (0=Normal, 1=Fire, 2=Water...). A `mapType()` function converts at load time.
**Rationale**: Historical accident. JSON data was generated first by design agents using Dinodex ordering. Code enum was designed later with logical grouping (Normal first). Too risky to renumber 150 dinos and 131 moves. Conversion is done once at load. **This caused the "Pyrex displays as blue" bug** — always verify type mapping when adding new data.

### 11. DialogueUI as DontDestroyOnLoad singleton in GameBootstrap
**Decision**: DialogueUI is created once by GameBootstrap and persists across all scenes.
**Rationale**: DialogueUI must be available in ALL scenes (overworld, interiors, battles). If it's scene-local, entering a building interior destroys it, causing null reference deadlocks (LockInput called, UnlockInput never reached because DialogueUI is null). GameBootstrap creates it once with DontDestroyOnLoad.

### 12. EventSystem created per-scene by UI scripts
**Decision**: Each UI script that needs clicks checks for EventSystem and creates one if missing.
**Rationale**: Unity requires exactly one EventSystem in the scene for UI interactions (Button.onClick, etc.). When transitioning between scenes, the EventSystem from the previous scene may be destroyed. Rather than relying on a specific scene to have it, each UI script self-heals by creating one if needed.

### 13. ScreenFade uses unscaledDeltaTime
**Decision**: Fade transitions use `Time.unscaledDeltaTime` instead of `Time.deltaTime`.
**Rationale**: Fades must work even when `Time.timeScale = 0` (during pause menu). Using unscaledDeltaTime ensures the fade animation plays regardless of game pause state.

### 14. Scene transitions: set flags BEFORE starting battle
**Decision**: In the event system, flags (like "trainer defeated") and items are applied BEFORE calling scene.start() for battle.
**Rationale**: `scene.start()` in Phaser DESTROYS the current scene. Any resume handlers or post-battle callbacks will never fire because the scene object is gone. The pre-scan pattern in EventSystem processes setFlag and giveItem actions before launching the battle scene.

### 15. Trainer defeated flag set on LOSS too
**Decision**: When the player loses to a trainer, the trainer is still marked as defeated.
**Rationale**: Prevents infinite event loops. If the trainer isn't marked defeated, returning to the map re-triggers the trainer battle event endlessly. Player gets healed and can continue. Better UX than a softlock.

### 16. Building doors use TILE_PATH
**Decision**: Door tiles on buildings are set to TILE_PATH (walkable) after the TILE_WALL rendering loop.
**Rationale**: Doors must be both walkable (player can step on them to enter) AND visually distinct. The wall rendering loop sets the entire building footprint to TILE_WALL. Doors are then overridden to TILE_PATH. The `restoreDoors()` function handles this in the POC.

### 17. Camera: distance 14, pitch 55 degrees
**Decision**: Isometric-style camera pulled back to distance 14 with 55-degree pitch.
**Rationale**: Original camera was too close, making it hard to see the surroundings. The zoomed-out view gives better spatial awareness while still showing building details. 55-degree pitch is a good compromise between top-down (navigation) and side-view (depth perception).

### 18. Doors as Cubes instead of Quads
**Decision**: Building doors rendered as thin Cubes instead of Quads.
**Rationale**: Quads are single-sided meshes — invisible from the back. With the isometric camera, some doors were invisible depending on building orientation. Cubes are visible from all angles. A thin cube (0.1 depth) looks the same as a quad but works from every camera position.

### 19. OverworldManager as monolithic file (~3789 lines)
**Decision**: All overworld logic in one massive file.
**Rationale**: Organic growth during rapid development. Contains map generation, NPC placement, encounter logic, trainer battles, transitions, and more. Ideally should be split, but works as-is. Splitting would be a refactoring task.

### 20. AssetLibrary DISABLED in GameBootstrap
**Decision**: The AssetLibrary component (for loading external prefabs) is disabled/commented out.
**Rationale**: After the asset integration attempt was abandoned, AssetLibrary has no prefabs to load. Keeping it disabled avoids null reference errors from missing assets. Can be re-enabled when proper assets are integrated.

### 21. POC tests run without Phaser dependency
**Decision**: All 134 tests use pure Node.js with extracted utility functions (damageCalc.ts, balanceSim.ts).
**Rationale**: Phaser requires a browser/canvas context that's hard to mock in Node. By extracting pure logic into utils/, tests run fast (< 500ms) without any game engine dependency. This also validates that the game logic is correctly separated from rendering.
