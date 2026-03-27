# Dino Monsters — Lessons Learned

> What worked and what did not. Read this to avoid repeating mistakes
> and to replicate successful patterns.

---

## What Worked Well

### POC-first approach
Building the complete 2D game in Phaser before attempting the Unity port was invaluable. All game mechanics, data structures, balance, and story events were validated in a fast-iteration environment. The Unity port was mostly mechanical translation (TypeScript to C#) rather than design exploration.

### Automated testing for balance
The 134 tests (8 test files) caught regressions early and validated balance changes systematically. The balance simulation tests (balanceSim.ts) detected one-shot problems before any manual playtesting. Every balance change was immediately verified against the test suite.

### User feedback as permanent constraints
Saving every user feedback in `studio/memory/project/feedback.md` with interpretation, impact, action, and status prevented repeating mistakes. The "graphics are critical" feedback became a permanent project constraint. The "Pyrex is blue" bug led to the TYPE_MAPPING.md documentation that prevented future type mapping errors.

### Procedural everything (in Unity)
Procedural buildings, dinos, and terrain using Unity primitives + MaterialManager were reliable and consistent. No shader issues, no scale mismatches, no missing references. Every object renders correctly from every angle. Fast to iterate on (change a color constant, see it immediately).

### Parallel agent execution
Running 3-5 agents on different files simultaneously (during the studio framework phase) was efficient. The dispatch system assigned exclusive file ownership per lane, preventing merge conflicts.

### Formalized skills
The 6 skills (implement-feature, create-dino, balance-check, debug-and-fix, generate-tests, create-agent) improved agent output quality by encoding step-by-step methodologies. Agents stopped improvising and started following proven processes.

### Memory system
The convention file (`conventions.md`) captured hard-won knowledge about type mapping, tile ordering, learnset rules, combat balance, event flags, and scene architecture. New work could reference these conventions instead of rediscovering them.

### JSON data architecture
Storing all game data as JSON (150 dinos, 131 moves, 80 items, type chart) worked perfectly for both Phaser and Unity. The same JSON files are used by both projects. Easy to edit, easy to validate with tests, easy to load at runtime.

---

## What Did NOT Work

### EmaceArt prefab assembly (CRITICAL FAILURE)
**What**: Tried to programmatically assemble buildings from the Slavic Medieval Town Kit (walls, roofs, foundations, windows, doors).
**Why it failed**: The prefab parts are Z-wide panels designed for manual modular construction in the Unity editor. Programmatic assembly produced misaligned, overlapping, or backwards walls. Wall orientation assumes you're building from inside a room looking out. From an overworld camera, you see through the back of walls.
**Lesson**: Asset packs designed for manual editor use do NOT work for runtime/programmatic assembly. Either use the editor to manually place buildings, or use procedural geometry. Do not attempt to script modular construction.

### MaterialFixer for color-sheet textures
**What**: Tried to fix pink materials by converting URP materials to Standard shader at runtime.
**Why it failed**: EmaceArt uses a single color-sheet texture with UV mapping. Creating a new Standard material and assigning the texture doesn't preserve the UV coordinates correctly. The result is wrong colors or solid color instead of the intended look.
**Lesson**: Shader/material conversion between render pipelines is not trivial. If assets are made for URP, use URP. If you need Standard pipeline, use assets made for Standard. Runtime conversion is unreliable.

### Room interiors as exterior buildings
**What**: Tried using room prefabs (designed for interiors) as building exteriors on the overworld.
**Why it failed**: Room meshes face inward. From outside, you see through the back walls. The mesh has no exterior faces.
**Lesson**: Interior and exterior are different mesh requirements. Interior rooms cannot be repurposed as overworld buildings.

### DialogueBox nested callback chains (POC)
**What**: In the Phaser POC, dialogue used nested callbacks (onComplete calls next dialogue, which calls next...).
**Why it failed**: If the scene was destroyed mid-chain (e.g., battle starts), resume handlers never fire. The chain breaks, leaving the game in a locked input state.
**Lesson**: Use state machines for dialogue flow, not callback chains. Set all flags BEFORE scene transitions. Never rely on post-transition callbacks.

### scene.start() for event continuation (POC)
**What**: Used `scene.start('BattleScene')` and expected to resume the event after battle.
**Why it failed**: `scene.start()` destroys the old scene. All closures, callbacks, and resume handlers are garbage collected.
**Lesson**: Pre-scan events and apply all side effects (setFlag, giveItem) BEFORE starting a new scene. The battle result can be checked via GameState flags when the overworld scene is recreated.

### try/catch on Input.GetAxis (Unity)
**What**: Wrapped `Input.GetAxis("RightStickX")` in try/catch to handle missing input axes.
**Why it failed**: Unity throws an exception EVERY FRAME for missing axes. The console floods with thousands of exception messages, making debugging impossible and potentially impacting performance.
**Lesson**: Use a safe wrapper (InputHelper) that checks if the axis exists before querying. Or simply don't use axes that aren't defined in the Input Manager.

### Multiple agents editing the same file
**What**: Two agents tried to modify OverworldManager.cs simultaneously.
**Why it failed**: Merge conflicts, lost changes, inconsistent state.
**Lesson**: The dispatch system must assign exclusive file ownership per agent per lane. Never have two agents touch the same file in the same work batch.

---

## Key Debugging Patterns

These are recurring patterns encountered during development. When you see these symptoms, check these causes first:

| Symptom | Likely Cause | Fix |
|---------|-------------|-----|
| Pink/magenta materials | Missing shader or wrong render pipeline | Check if asset uses URP vs Standard. Use MaterialFixer or switch pipeline. |
| Buttons don't respond to clicks | Missing EventSystem in scene | Add EventSystem check in UI script Awake/Start |
| Game freezes after dialogue | LockInput called but UnlockInput never reached | Check if DialogueUI is null (not DontDestroyOnLoad) |
| Infinite loop after battle | Event flag not set because scene was destroyed before flag write | Use pre-scan pattern: set flags BEFORE scene.start() |
| Building too big/small | Prefab scale vs tile grid mismatch | Check localScale. One tile = 1 Unity unit. |
| Dino shows wrong type/color | JSON type numbering vs code enum mismatch | Check mapType() conversion. JSON 0=Fossil, Code 0=Normal. |
| Interior softlock | DialogueUI destroyed with scene, LockInput stuck | DialogueUI must be DontDestroyOnLoad singleton |
| Camera sees through buildings | Single-sided mesh (Quad) or interior-facing walls | Use Cubes instead of Quads. Don't use room interiors as exteriors. |
| Door invisible from some angles | Door rendered as Quad (single-sided) | Replace with thin Cube |
| Fade doesn't work during pause | Using Time.deltaTime with timeScale=0 | Use Time.unscaledDeltaTime in ScreenFade |

---

## Process Lessons

### What to do at the start of a new work session
1. Read CURRENT_STATE.md to know exactly where things stand
2. Read feedback.md to know what the user cares about
3. Read conventions.md before writing any game logic code
4. Run the POC tests if touching game data or balance

### What to do before a commit
1. Verify the change works (build in Unity or run tests in POC)
2. Check that no regressions were introduced
3. Update CURRENT_STATE.md if the project state changed meaningfully
4. Update conventions.md if a new convention was discovered

### What to do when something breaks
1. Check the debugging patterns table above first
2. Check conventions.md for known constraints
3. Check feedback.md for user-reported issues that match
4. Fix the root cause, not the symptom
5. Add the fix to conventions.md if it reveals a new convention
