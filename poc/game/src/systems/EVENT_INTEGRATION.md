---
name: event_system_integration
description: Integration guide for EventSystem into OverworldScene
type: integration_patch
---

# EventSystem Integration into OverworldScene

## Overview

The `EventSystem` needs to be wired into three places in `OverworldScene.ts`:
1. Map enter (when loading a new map)
2. Tile step (after each player movement)
3. NPC interaction (when talking to an NPC)

## Required Changes

### 1. Add imports

At the top of `OverworldScene.ts`, add:

```typescript
import { EventSystem } from '../systems/EventSystem';
```

### 2. Add property

In the class properties section (around line 93-98):

```typescript
private eventSystem!: EventSystem;
```

### 3. Initialize in create()

After the `DialogueBox` is created (around line 178), add:

```typescript
this.eventSystem = new EventSystem(this, this.dialogue);

// Listen for NPC movement events from the event system
this.events.on('event:moveNpc', (data: { npcId: string; direction: string; tiles: number }) => {
  this.moveNpcVisual(data.npcId, data.direction, data.tiles);
});
```

### 4. Hook into map loading

At the end of the `create()` method (around line 192, after `showLocationBanner`), add:

```typescript
// Check for story events on map enter
const mapEvent = this.eventSystem.checkMapEnterEvents(this.currentMapId);
if (mapEvent) {
  this.eventSystem.executeEvent(mapEvent);
}
```

### 5. Hook into movement completion

In `onStepComplete()` (around line 909), add BEFORE the warp check:

```typescript
// Check for story tile events
if (!this.eventSystem.isExecuting()) {
  const tileEvent = this.eventSystem.checkTileEvents(this.currentMapId, x, y);
  if (tileEvent) {
    this.eventSystem.executeEvent(tileEvent);
    return; // Don't process warp or encounter while event runs
  }
}
```

### 6. Hook into NPC interaction

In `interact()` (around line 997), modify the NPC interaction to check events first:

```typescript
// Check for NPC
const npc = this.npcs.find(n => n.data.x === facingX && n.data.y === facingY);
if (npc) {
  // Check for story events on this NPC first
  const npcEvent = this.eventSystem.checkNpcEvents(npc.data.id || npc.data.name);
  if (npcEvent) {
    this.eventSystem.executeEvent(npcEvent);
    return;
  }
  // Normal dialogue fallback
  const text = npc.data.dialogue[npc.dialogueIndex % npc.data.dialogue.length];
  npc.dialogueIndex++;
  this.dialogue.showText(text, undefined, npc.data.name);
  return;
}
```

### 7. Block movement during events

In `movePlayer()` (around line 861), add `this.eventSystem.isExecuting()` to the guard:

```typescript
if (this.isMoving || this.isWarping || this.dialogue.isActive() || this.menu.isOpen() || this.eventSystem.isExecuting()) return;
```

### 8. Add NPC visual movement helper

Add a new method to handle NPC movement triggered by events:

```typescript
private moveNpcVisual(npcId: string, direction: string, tiles: number): void {
  const npc = this.npcs.find(n => (n.data as any).id === npcId || n.data.name === npcId);
  if (!npc) return;

  let dx = 0, dy = 0;
  switch (direction) {
    case 'up': dy = -1; break;
    case 'down': dy = 1; break;
    case 'left': dx = -1; break;
    case 'right': dx = 1; break;
  }

  // Animate NPC movement tile by tile
  let moved = 0;
  const moveNext = () => {
    if (moved >= tiles) return;
    npc.data.x += dx;
    npc.data.y += dy;
    moved++;
    this.drawNPCs();
    if (moved < tiles) {
      this.time.delayedCall(150, moveNext);
    }
  };
  moveNext();
}
```

### 9. Handle battle return for story events

When returning from a battle scene (in `create()` or wherever return data is processed), check for `resumeEvent`:

```typescript
// If returning from a story event battle, resolve the pending promise
if (data?.resumeEvent && (this as any).__eventBattleResolve) {
  const resolve = (this as any).__eventBattleResolve;
  delete (this as any).__eventBattleResolve;
  delete (this as any).__eventBattleTrainerId;
  this.time.delayedCall(200, () => resolve());
}
```

### 10. Badge event check

After awarding a badge (wherever `GameState.addBadge()` is called), add:

```typescript
const badgeCount = GameState.getInstance().getBadgeCount();
const badgeEvent = this.eventSystem.checkBadgeEvents(badgeCount);
if (badgeEvent) {
  this.eventSystem.executeEvent(badgeEvent);
}
```

### 11. Cleanup

In the scene's `shutdown()` or cleanup method:

```typescript
this.eventSystem.destroy();
```

## Notes

- The EventSystem uses async/await internally but is compatible with Phaser's event loop
- Battles pause event execution until the player returns from BattleScene
- NPC movement is visual only; the NPC data coordinates are updated directly
- The `processText()` method in EventSystem handles `[JOUEUR]` and `[RIVAL_STARTER]` placeholders
- Flag-triggered events are automatically checked when `setFlag` actions fire
