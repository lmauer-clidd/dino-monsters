# Dino Monsters — Unity 3D

Pokemon-like game with dinosaurs, built with Unity 2022.3 LTS.

## Setup
1. Open project in Unity 2022.3+
2. Menu: Dino Monsters > Validate Project
3. Open Scenes/Title scene
4. Press Play

## Architecture
- `Scripts/Core/` — GameManager, GameState, SaveSystem, Constants
- `Scripts/Data/` — DataLoader, JSON deserialization
- `Scripts/Dinos/` — Dino entity, stats, evolution
- `Scripts/Battle/` — BattleSystem, DamageCalculator
- `Scripts/Overworld/` — Player controller, NPC, map transitions
- `Scripts/UI/` — HUD, menus, dialogue system
- `StreamingAssets/` — Game data (150 dinos, 129 moves, items, type chart)

## Build Targets
- PC (Windows/Mac/Linux)
- Android
- iOS
- WebGL
- Console (Switch/PS via Unity)
