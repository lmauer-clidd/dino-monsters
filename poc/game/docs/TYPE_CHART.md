# Jurassic Trainers -- Type Effectiveness Chart

## Overview

14 types: **Roche, Eau, Feu, Plante, Glace, Vol, Terre, Foudre, Poison, Acier, Ombre, Lumiere, Sable, Fossile**

Values:
- **2** = Super effective (double damage)
- **1** = Normal effectiveness
- **0.5** = Not very effective (half damage)
- **0** = Immune (no damage)

## 14x14 Type Chart

Rows = Attacker type, Columns = Defender type.

| ATK \ DEF | Roche | Eau | Feu | Plante | Glace | Vol | Terre | Foudre | Poison | Acier | Ombre | Lumiere | Sable | Fossile |
|-----------|-------|-----|-----|--------|-------|-----|-------|--------|--------|-------|-------|---------|-------|---------|
| **Roche** | 0.5 | 1 | 2 | 1 | 2 | 2 | 0.5 | 1 | 1 | 0.5 | 1 | 1 | 1 | 1 |
| **Eau** | 2 | 0.5 | 2 | 0.5 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 2 | 1 |
| **Feu** | 0.5 | 0.5 | 0.5 | 2 | 2 | 1 | 1 | 1 | 1 | 2 | 1 | 1 | 0.5 | 0.5 |
| **Plante** | 2 | 2 | 0.5 | 0.5 | 0.5 | 0.5 | 2 | 1 | 0.5 | 1 | 1 | 1 | 1 | 1 |
| **Glace** | 0.5 | 1 | 0.5 | 2 | 0.5 | 2 | 2 | 1 | 1 | 0.5 | 1 | 1 | 2 | 1 |
| **Vol** | 0.5 | 1 | 1 | 2 | 0.5 | 1 | 1 | 0.5 | 2 | 1 | 1 | 1 | 1 | 1 |
| **Terre** | 2 | 0.5 | 2 | 0.5 | 0.5 | 0 | 1 | 2 | 2 | 2 | 1 | 1 | 1 | 1 |
| **Foudre** | 1 | 2 | 1 | 1 | 1 | 2 | 0 | 0.5 | 1 | 1 | 1 | 1 | 0.5 | 1 |
| **Poison** | 1 | 1 | 1 | 2 | 1 | 1 | 0.5 | 1 | 0.5 | 0 | 1 | 2 | 1 | 1 |
| **Acier** | 2 | 1 | 0.5 | 1 | 2 | 1 | 0.5 | 1 | 1 | 0.5 | 1 | 2 | 1 | 1 |
| **Ombre** | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 2 | 2 | 1 | 1 |
| **Lumiere** | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 0.5 | 1 | 2 | 0.5 | 1 | 1 |
| **Sable** | 1 | 0.5 | 2 | 0.5 | 0.5 | 1 | 1 | 2 | 2 | 1 | 1 | 1 | 0.5 | 1 |
| **Fossile** | 1 | 0.5 | 2 | 0.5 | 2 | 2 | 0.5 | 1 | 1 | 0.5 | 1 | 1 | 1 | 1 |

## Immunities (5 total)

| Defender | Immune to |
|----------|-----------|
| Vol | Terre |
| Acier | Poison |
| Terre | Foudre |
| Ombre | *(none -- but resists Ombre at 0.5 via Ombre attacking itself would be 2, see note)* |

**Corrected immunity list:**
1. **Vol** is immune to **Terre** (flying dinosaurs avoid ground attacks)
2. **Acier** is immune to **Poison** (metal cannot be poisoned)
3. **Terre** is immune to **Foudre** (ground absorbs electricity)

*Note: 3 hard immunities keep the game strategic without being frustrating. The chart also features numerous 0.5x resistances that serve as soft counters.*

## Type-by-Type Summary

### Roche (Rock)
- **Super effective against (2x):** Feu, Glace, Vol
- **Not very effective against (0.5x):** Roche, Terre, Acier
- **Resists (takes 0.5x from):** Feu, Vol, Fossile
- **Weak to (takes 2x from):** Eau, Plante, Terre, Acier
- **Immune to:** --
- **Offensive score:** 3 super / 3 resisted = Balanced
- **Defensive score:** 4 weaknesses / 3 resistances = Slightly fragile

### Eau (Water)
- **Super effective against (2x):** Roche, Feu, Sable
- **Not very effective against (0.5x):** Eau, Plante, Fossile
- **Resists (takes 0.5x from):** Feu, Eau, Glace, Acier
- **Weak to (takes 2x from):** Plante, Foudre
- **Immune to:** --
- **Offensive score:** 3 super / 3 resisted = Balanced
- **Defensive score:** 2 weaknesses / 4 resistances = Solid tank

### Feu (Fire)
- **Super effective against (2x):** Plante, Glace, Acier
- **Not very effective against (0.5x):** Roche, Eau, Feu, Sable, Fossile
- **Resists (takes 0.5x from):** Feu, Plante, Glace, Acier, Fossile, Sable
- **Weak to (takes 2x from):** Eau, Roche, Terre, Sable
- **Immune to:** --
- **Offensive score:** 3 super / 5 resisted = Walled often but powerful hits
- **Defensive score:** 4 weaknesses / 6 resistances = Great resistances offset weaknesses

### Plante (Grass)
- **Super effective against (2x):** Eau, Roche, Terre
- **Not very effective against (0.5x):** Feu, Plante, Glace, Vol, Poison
- **Resists (takes 0.5x from):** Eau, Plante, Foudre, Terre
- **Weak to (takes 2x from):** Feu, Glace, Vol, Poison
- **Immune to:** --
- **Offensive score:** 3 super / 5 resisted = Many walls
- **Defensive score:** 4 weaknesses / 4 resistances = Even

### Glace (Ice)
- **Super effective against (2x):** Plante, Vol, Terre, Sable
- **Not very effective against (0.5x):** Roche, Feu, Glace, Acier
- **Resists (takes 0.5x from):** Glace
- **Weak to (takes 2x from):** Feu, Roche, Acier, Fossile
- **Immune to:** --
- **Offensive score:** 4 super / 4 resisted = Great coverage
- **Defensive score:** 4 weaknesses / 1 resistance = Glass cannon

### Vol (Flying)
- **Super effective against (2x):** Plante, Poison
- **Not very effective against (0.5x):** Roche, Glace, Foudre
- **Resists (takes 0.5x from):** Plante, Vol, Terre (immune)
- **Weak to (takes 2x from):** Roche, Glace, Foudre, Fossile
- **Immune to:** Terre
- **Offensive score:** 2 super / 3 resisted = Narrow but useful
- **Defensive score:** 4 weaknesses / 2 resistances + 1 immunity = Immunity is key asset

### Terre (Ground)
- **Super effective against (2x):** Feu, Roche, Foudre, Poison, Acier
- **Not very effective against (0.5x):** Eau, Plante, Glace, Fossile
- **Cannot hit:** Vol (immune)
- **Resists (takes 0.5x from):** Roche, Poison, Fossile
- **Weak to (takes 2x from):** Eau, Plante, Glace
- **Immune to:** Foudre
- **Offensive score:** 5 super / 4 resisted + 1 immune = Best offensive type
- **Defensive score:** 3 weaknesses / 3 resistances + 1 immunity = Solid

### Foudre (Lightning)
- **Super effective against (2x):** Eau, Vol
- **Not very effective against (0.5x):** Foudre, Sable
- **Cannot hit:** Terre (immune)
- **Resists (takes 0.5x from):** Vol, Foudre, Acier
- **Weak to (takes 2x from):** Terre, Sable
- **Immune to:** --
- **Offensive score:** 2 super / 2 resisted + 1 immune = Narrow
- **Defensive score:** 2 weaknesses / 3 resistances = Decent

### Poison
- **Super effective against (2x):** Plante, Lumiere
- **Not very effective against (0.5x):** Terre, Poison
- **Cannot hit:** Acier (immune)
- **Resists (takes 0.5x from):** Plante, Poison, Lumiere
- **Weak to (takes 2x from):** Terre, Acier, Sable
- **Immune to:** --
- **Offensive score:** 2 super / 2 resisted + 1 immune = Narrow but Lumiere counter
- **Defensive score:** 3 weaknesses / 3 resistances = Balanced

### Acier (Steel)
- **Super effective against (2x):** Roche, Glace, Lumiere
- **Not very effective against (0.5x):** Feu, Terre, Acier
- **Resists (takes 0.5x from):** Roche, Plante, Glace, Vol, Acier, Lumiere, Fossile
- **Weak to (takes 2x from):** Feu, Terre
- **Immune to:** Poison
- **Offensive score:** 3 super / 3 resisted = Balanced
- **Defensive score:** 2 weaknesses / 7 resistances + 1 immunity = Best defensive type

### Ombre (Shadow)
- **Super effective against (2x):** Ombre, Lumiere
- **Not very effective against (0.5x):** --
- **Resists (takes 0.5x from):** Poison
- **Weak to (takes 2x from):** Ombre, Lumiere
- **Immune to:** --
- **Offensive score:** 2 super / 0 resisted = Unresisted coverage (very strong)
- **Defensive score:** 2 weaknesses / 1 resistance = Fragile to its own kind

### Lumiere (Light)
- **Super effective against (2x):** Ombre
- **Not very effective against (0.5x):** Poison, Lumiere
- **Resists (takes 0.5x from):** Lumiere
- **Weak to (takes 2x from):** Poison, Ombre
- **Immune to:** --
- **Offensive score:** 1 super / 2 resisted = Narrow, support type
- **Defensive score:** 2 weaknesses / 1 resistance = Fragile

### Sable (Sand)
- **Super effective against (2x):** Feu, Foudre, Poison
- **Not very effective against (0.5x):** Eau, Plante, Glace, Sable
- **Resists (takes 0.5x from):** Foudre, Feu, Poison, Sable
- **Weak to (takes 2x from):** Eau, Plante, Glace
- **Immune to:** --
- **Offensive score:** 3 super / 4 resisted = Useful niches
- **Defensive score:** 3 weaknesses / 4 resistances = Balanced

### Fossile (Fossil)
- **Super effective against (2x):** Feu, Glace, Vol
- **Not very effective against (0.5x):** Eau, Plante, Terre, Acier
- **Resists (takes 0.5x from):** Feu, Vol, Foudre
- **Weak to (takes 2x from):** Eau, Plante, Terre, Acier
- **Immune to:** --
- **Offensive score:** 3 super / 4 resisted = Similar to Roche but different targets
- **Defensive score:** 4 weaknesses / 3 resistances = Slightly fragile

## Balance Notes

### Offensive Tier List
1. **S-Tier:** Terre (5 super, 1 immunity bypass needed), Glace (4 super)
2. **A-Tier:** Roche, Eau, Feu, Acier, Sable, Fossile (3 super each)
3. **B-Tier:** Plante (3 super but many walls), Ombre (2 super, 0 resisted), Vol, Foudre, Poison (2 super)
4. **C-Tier:** Lumiere (1 super -- relies on support utility)

### Defensive Tier List
1. **S-Tier:** Acier (2 weak, 7 resist + immunity)
2. **A-Tier:** Eau (2 weak, 4 resist), Feu (4 weak, 6 resist), Sable (3 weak, 4 resist), Terre (3 weak, 3 resist + immunity)
3. **B-Tier:** Roche, Plante, Foudre, Poison (balanced)
4. **C-Tier:** Glace (4 weak, 1 resist -- glass cannon), Vol (4 weak but immunity), Ombre, Lumiere (fragile)
5. **D-Tier:** Fossile (4 weak, 3 resist -- slightly worse Roche defensively)

### Key Strategic Triangles
- **Classic:** Feu > Plante > Eau > Feu
- **Prehistoric:** Roche > Fossile (resisted) vs Fossile > Feu > Roche (loop via Feu)
- **Mystic:** Ombre <> Lumiere (mutual super effectiveness)
- **Desert:** Sable > Foudre > Eau > Sable
- **Industrial:** Acier > Glace > Terre > Acier
