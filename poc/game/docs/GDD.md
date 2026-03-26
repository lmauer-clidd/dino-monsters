# JURASSIC TRAINERS — Game Design Document

> **Authors:**
> - **YUKI** (Lead Game Designer) — Vision, core loop, player experience
> - **MARCUS** (System Designer) — Formulas, balance, data architecture
> - **SANA** (Level Designer) — World, routes, encounters, progression gates

> **Version:** 1.0
> **Date:** 2026-03-22
> **Status:** Production-Ready

---

## Table of Contents

1. [Game Overview](#1-game-overview)
2. [Core Loop](#2-core-loop)
3. [Combat System](#3-combat-system)
4. [Dinosaur System](#4-dinosaur-system)
5. [Capture System](#5-capture-system)
6. [Progression System](#6-progression-system)
7. [Economy & Items](#7-economy--items)
8. [World Design](#8-world-design)
9. [NPC System](#9-npc-system)
10. [Field Mechanics](#10-field-mechanics)
11. [Save System](#11-save-system)
12. [Dinodex](#12-dinodex)
13. [Post-Game Content](#13-post-game-content)

---

## 1. Game Overview

**Elevator Pitch:**
Jurassic Trainers is a turn-based monster-taming RPG where you capture, train, and battle 150 dinosaurs across a prehistoric-themed world. Inspired by the GBA Fire Red era, it combines deep strategic combat with the thrill of building your ultimate team of dinosaurs.

**Genre:** Turn-based RPG / Monster Taming
**Platform:** GBA-style (pixel art, 240x160 resolution target, tile-based)
**Target Audience:** Players aged 10+, fans of classic monster-taming RPGs
**Play Time:** 25-35 hours main story, 60+ hours for completion
**Tone:** Adventurous, warm, occasionally humorous. A world where humans and dinosaurs coexist.

**Tagline:** *"Discover. Tame. Evolve."*

> **YUKI's Note:** The magic is in the first 10 minutes. The player picks a starter, wins their first battle, and catches their first wild dino. That sequence must feel *incredible* every single time.

---

## 2. Core Loop

### 2.1 The Fundamental Loop

```
    ┌─────────────────────────────────────────────┐
    │                                             │
    ▼                                             │
 EXPLORE ──► ENCOUNTER ──► BATTLE ──► REWARD ─────┘
   │              │           │          │
   │              ▼           ▼          ▼
   │          CAPTURE      VICTORY    EXP + $
   │              │           │          │
   │              ▼           ▼          ▼
   │          NEW DINO    LEVEL UP   NEW ITEMS
   │              │           │          │
   │              └─────┬─────┘          │
   │                    ▼                │
   │              TEAM STRONGER ◄────────┘
   │                    │
   │                    ▼
   │              NEW AREA UNLOCKED
   │                    │
   └────────────────────┘
```

### 2.2 The 30-Second Rule

Every 30 seconds of gameplay, the player must experience at least one of:
- A wild encounter or trainer battle
- An item discovery
- A new NPC interaction or dialogue
- A visual change in environment (new tile set, weather)
- A decision point (which path to take, which move to learn)

**Route density targets:**
- Grass patches every 8-12 tiles of walkable route
- Trainers placed every 15-20 tiles of linear path
- Items visible or hidden every 20-30 tiles
- Environmental variety shift every 40-60 tiles

### 2.3 Session Loop (Macro)

```
Start Session
  └► Travel to next town/gym
       └► Train team on routes (5-15 min)
            └► Challenge Gym (10-20 min)
                 └► Unlock new area with Field Ability
                      └► Explore new routes, catch new dinos
                           └► Save & End Session (or continue)
```

> **YUKI's Note:** The player should always know what their next goal is. Every town has an NPC who says something like "The next gym is in [Town], you'll need [ability] to get there."

---

## 3. Combat System

### 3.1 Battle Flow

1. Player and opponent send out their active dinosaur.
2. Each turn, both sides select an action: **Fight**, **Bag**, **Dino** (switch), or **Run**.
3. Actions resolve in priority order:
   - Switching dinos: priority +6
   - Using items: priority +5
   - Running: priority +4
   - Moves: resolved by move priority tier, then Speed stat
4. Effects apply (damage, status, stat changes).
5. End-of-turn effects (weather, status damage, terrain).
6. Faint checks. If a dino faints, the trainer sends the next one.
7. Battle ends when one side has no dinos left, or the player runs/catches.

### 3.2 Turn Order

- Each move has a **Priority** value (default 0, range -7 to +5).
- Higher priority moves go first regardless of Speed.
- If priority is equal, the dino with the higher **Speed** stat acts first.
- Speed ties: resolved randomly (50/50).

**Priority examples:**
| Priority | Moves |
|----------|-------|
| +5 | Protective Shell (Protect equivalent) |
| +2 | Raptor Strike (Quick Attack equivalent) |
| +1 | Primal Instinct (priority status move) |
| 0 | Most moves |
| -1 | Fossil Slam (delayed power move) |
| -3 | Ancient Roar (always-last category) |
| -7 | Trick Tail (counter move, always last) |

### 3.3 Damage Formula

```
Damage = ((((2 * Level / 5 + 2) * Power * A / D) / 50) + 2)
         * STAB * TypeEffectiveness * Critical * Random * Other

Where:
  Level       = Attacker's level (1-50)
  Power       = Move's base power (10-150)
  A           = Attacker's Attack (Physical) or SpAtk (Special)
  D           = Defender's Defense (Physical) or SpDef (Special)
  STAB        = 1.5 if move type matches attacker's type, else 1.0
  TypeEffect  = 0, 0.5, 1.0, or 2.0 (see type chart; multiplicative for dual types)
  Critical    = 1.5 if critical hit, else 1.0
  Random      = Random float in [0.85, 1.00]
  Other       = Product of: weather (1.0 or 1.5 or 0.5), burn penalty (0.5 on physical if burned), held item, ability modifier
```

**Minimum damage:** 1 (a move that hits always deals at least 1 damage).

**Example calculation:**
Level 30 PYREX (Atk 85) uses Flame Fang (Power 65, Physical, Feu) vs AQUADON (Def 70):
```
Base = ((2*30/5 + 2) * 65 * 85 / 70) / 50 + 2
     = ((14) * 65 * 85 / 70) / 50 + 2
     = (14 * 65 * 1.214) / 50 + 2
     = (1105.71) / 50 + 2
     = 22.11 + 2 = 24.11
* STAB 1.5 = 36.17
* TypeEffect 0.5 (Feu vs Eau) = 18.08
* Random ~0.925 = 16.73
Final: 16 damage
```

### 3.4 Type Effectiveness Chart (14 Types)

Legend: `2` = Super Effective (x2), `H` = Half (x0.5), `0` = Immune (x0), blank = Neutral (x1)

| ATK ► / DEF ▼ | Roche | Eau | Feu | Plante | Glace | Vol | Terre | Foudre | Poison | Acier | Ombre | Lumiere | Sable | Fossile |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| **Roche** | | | 2 | | 2 | 2 | H | | | H | | | H | |
| **Eau** | | H | 2 | H | | | 2 | | | | | | 2 | |
| **Feu** | H | H | H | 2 | 2 | | | | | 2 | | | | H |
| **Plante** | | 2 | H | H | | | 2 | | H | | | | 2 | |
| **Glace** | | | H | 2 | H | 2 | 2 | | | H | | | | |
| **Vol** | H | | | 2 | | | | H | | | | | | 2 |
| **Terre** | 2 | | 2 | H | | 0 | | 2 | 2 | 2 | | | H | |
| **Foudre** | | 2 | | | | 2 | 0 | H | | | | | | H |
| **Poison** | | | | 2 | | | H | | H | 0 | | | | 2 |
| **Acier** | 2 | | H | | 2 | | | | | H | | 2 | 2 | |
| **Ombre** | | | | | | | | | | | 2 | H | | 2 |
| **Lumiere** | | | | | | | | | 2 | | 2 | H | | |
| **Sable** | 2 | H | | H | | | | | | H | | | | 2 |
| **Fossile** | | | 2 | | | H | | 2 | H | | H | | H | |

**Design rationale (MARCUS):**
- Every type has 2-3 weaknesses and 2-3 resistances.
- One immunity: Terre immune to Vol (grounded), Acier immune to Poison (inorganic), Foudre immune to Terre (insulated for game balance).
- Fossile is the pseudo-legendary type: strong offensively but also vulnerable.
- Ombre and Lumiere are mirrors: each super effective against the other.
- Sable synergizes with Roche but adds unique interactions with Eau and Fossile.

**Dual-type interactions:** Multiply both type matchups.
Example: Feu/Roche defender vs Eau attack = 2 (Eau vs Feu) * 1 (Eau vs Roche) = x2.

### 3.5 Status Effects

| Status | Effect | Duration | Cure |
|--------|--------|----------|------|
| **Brulure** (Burn) | Lose 1/16 max PV per turn. Physical Attack reduced by 50%. | Persistent until cured | Onguent (Antidote), Full Heal, switching does NOT cure |
| **Poison** | Lose 1/8 max PV per turn. | Persistent until cured | Antidote, Full Heal |
| **Poison Grave** (Toxic) | Lose 1/16 max PV turn 1, increasing by 1/16 each turn (1/16, 2/16, 3/16...) | Persistent until cured, counter resets on switch | Antidote, Full Heal |
| **Paralysie** | Speed reduced by 50%. 25% chance of being fully paralyzed each turn (cannot act). | Persistent until cured | Anti-Para, Full Heal |
| **Sommeil** (Sleep) | Cannot act. | 1-3 turns (random). Counter starts when inflicted. | Reveil (Awakening), Full Heal, waking naturally |
| **Gel** (Freeze) | Cannot act. | Each turn, 20% chance to thaw. Fire moves used against thaw instantly. | Thaws naturally, Fire move, Full Heal |
| **Confusion** | 33% chance of hitting self each turn. Self-hit uses 40-power Physical typeless attack against own Defense. | 1-4 turns | Wears off naturally, switching out |

**Rules:**
- A dino can only have ONE primary status (Burn/Poison/Para/Sleep/Freeze).
- Confusion and stat changes can stack ON TOP of a primary status.
- Status moves respect type immunities (e.g., Poison moves cannot poison Acier types).

### 3.6 Critical Hits

**Base critical rate:** 1/16 (6.25%)

**Critical hit stages:**
| Stage | Rate |
|-------|------|
| 0 | 1/16 (6.25%) |
| +1 | 1/8 (12.5%) |
| +2 | 1/4 (25%) |
| +3 | 1/3 (33.3%) |
| +4+ | 1/2 (50%) |

**Critical damage multiplier:** 1.5x

**Critical hits ignore:**
- Attacker's negative Attack/SpAtk stat changes
- Defender's positive Defense/SpDef stat changes

**Crit-boosting effects:**
- High-crit moves: +1 stage (e.g., Razor Claw, Primal Slash)
- Held item "Sharp Fang": +1 stage
- Ability "Predator Focus": +1 stage

### 3.7 Move Categories

| Category | Stat Used (Attack) | Stat Used (Defense) | Examples |
|----------|-------------------|--------------------|----|
| **Physical** | Attaque | Defense | Tail Slam, Bite, Rock Throw |
| **Special** | Atk Spe | Def Spe | Flame Breath, Tidal Wave, Psychic Roar |
| **Status** | N/A | N/A | Roar (forces switch), Harden (+Def), Toxic Spores |

Each move has: Name, Type, Category, Power (0 for Status), Accuracy (1-100%), PP, Priority, Effect description, Effect chance.

### 3.8 Terrain Bonuses

Battles can take place on different terrain types. Terrain is determined by the map tile where the encounter begins.

| Terrain | Bonus |
|---------|-------|
| **Volcan** (Volcanic) | Feu moves +20% damage, Glace moves -20% damage |
| **Marais** (Swamp) | Eau and Poison moves +20% damage, Speed stat halved for non-Eau/Poison types |
| **Foret** (Forest) | Plante moves +20% damage, accuracy of all moves reduced by 10% (dense foliage) |
| **Desert** (Desert) | Sable and Feu moves +20% damage, Eau moves -20% |
| **Caverne** (Cave) | Roche and Terre moves +20% damage, Vol moves -20% |
| **Rivage** (Shore) | Eau moves +20% damage |
| **Montagne** (Mountain) | Vol and Roche moves +20% damage, Terre moves cannot miss |
| **Ruines** (Ruins) | Fossile and Ombre moves +20% damage |
| **Plaines** (Plains) | No bonus (neutral) |
| **Toundra** (Tundra) | Glace moves +20% damage, Feu moves -20% |

Terrain bonus is applied as a multiplier in the `Other` section of the damage formula: 1.2 for boosted, 0.8 for reduced.

### 3.9 Accuracy & Evasion

```
HitChance = MoveAccuracy * (AccuracyStage / EvasionStage)

Stage multipliers:
  -6: 3/9    -5: 3/8    -4: 3/7    -3: 3/6
  -2: 3/5    -1: 3/4     0: 3/3 (1.0)
  +1: 4/3    +2: 5/3    +3: 6/3
  +4: 7/3    +5: 8/3    +6: 9/3
```

- A move with 100% accuracy at neutral stages always hits.
- Moves with "--" accuracy (e.g., Swift equivalent) bypass accuracy checks entirely.
- One-hit KO moves (if any) have fixed 30% accuracy, unaffected by evasion. Not recommended for main game.

---

## 4. Dinosaur System

### 4.1 Base Stats

Every dinosaur species has 6 base stats:

| Stat | Abbreviation | Role |
|------|-------------|------|
| Points de Vie | PV | Hit points |
| Attaque | ATK | Physical damage dealt |
| Defense | DEF | Physical damage received |
| Attaque Speciale | SPATK | Special damage dealt |
| Defense Speciale | SPDEF | Special damage received |
| Vitesse | VIT | Turn order |

**Base Stat Total (BST) guidelines:**
| Stage | BST Range | Example |
|-------|-----------|---------|
| Baby (Stage 1) | 280-320 | Starter base forms |
| Young (Stage 2) | 380-420 | Mid evolutions |
| Adult (Stage 3) | 480-535 | Final evolutions |
| Legendary | 560-600 | One-of-a-kind dinos |

### 4.2 Stat Calculation Formula

```
If stat is PV:
  PV = ((2 * Base + GV + (TP / 4)) * Level / 50) + Level + 10

If stat is any other:
  Stat = (((2 * Base + GV + (TP / 4)) * Level / 50) + 5) * TemperamentModifier

Where:
  Base  = Species base stat (0-255)
  GV    = Genetic Value for that stat (0-31), determined at encounter
  TP    = Training Points for that stat (0-252, max 510 total across all stats)
  Level = Current level (1-50)
  TemperamentModifier = 0.9, 1.0, or 1.1 depending on Temperament
```

### 4.3 Stat Growth Per Level

Stats are recalculated from scratch each level using the formula above. There is no incremental gain — the formula is absolute. This means stat changes are smooth and predictable.

**Approximate stat at key levels (for a base-80 stat, neutral temperament, 15 GV, 0 TP):**
| Level | Stat Value |
|-------|-----------|
| 5 | 18 |
| 10 | 24 |
| 20 | 41 |
| 30 | 57 |
| 40 | 74 |
| 50 | 90 |

### 4.4 Temperament System

Each dinosaur has one of 25 Temperaments, determined randomly at encounter. Temperaments modify two stats: one gets +10%, one gets -10%. Five temperaments are neutral (no effect).

| Temperament | +10% | -10% |
|-------------|------|------|
| **Feroce** | ATK | SPDEF |
| **Brave** | ATK | VIT |
| **Enrage** | ATK | SPATK |
| **Solitaire** | ATK | DEF |
| **Docile** | DEF | ATK |
| **Relaxe** | DEF | VIT |
| **Espiegle** | DEF | SPATK |
| **Assure** | DEF | SPDEF |
| **Timide** | VIT | ATK |
| **Presse** | VIT | DEF |
| **Joyeux** | VIT | SPATK |
| **Naif** | VIT | SPDEF |
| **Modeste** | SPATK | ATK |
| **Calme** | SPATK | DEF |
| **Discret** | SPATK | VIT |
| **Doux** | SPATK | SPDEF |
| **Prudent** | SPDEF | ATK |
| **Gentil** | SPDEF | DEF |
| **Attentif** | SPDEF | VIT |
| **Malin** | SPDEF | SPATK |
| **Neutre** | — | — |
| **Robuste** | — | — |
| **Placide** | — | — |
| **Serieux** | — | — |
| **Versatile** | — | — |

**Temperament also affects capture rate:**
- Docile, Gentil, Calme, Doux, Relaxe: capture rate x1.2 (easier)
- Feroce, Brave, Solitaire, Enrage: capture rate x0.8 (harder)
- All others: x1.0

**Temperament affects flavor preference** (for future berry/food system):
Each temperament likes one flavor and dislikes another, matching its stat preference.

### 4.5 Evolution System

**Level-based evolution (majority):**
- Baby to Young: levels 14-20 (varies per species)
- Young to Adult: levels 30-38 (varies per species)

Evolution triggers on level-up. The player can cancel evolution by pressing B, and the dino will attempt to evolve again on next level-up.

**Special evolution methods (10-15 species):**

| Method | Example |
|--------|---------|
| **Item evolution** | Use "Pierre Solaire" on HELIODON (Lumiere baby) to evolve |
| **Location evolution** | Level up GLACIREX near Frost Peak to evolve into Glace/Roche adult |
| **Trade evolution** | Trade COMPSOLINK holding "Cable Fossile" to evolve |
| **Happiness evolution** | PTERAPUP evolves when happiness ≥ 220 and levels up |
| **Move evolution** | SPINEX evolves when it knows "Ancient Power" and levels up |
| **Time of day** | NOCTURN evolves at night; SOLAUREX evolves during daytime |

### 4.6 Genetic Values (GV) — IV Equivalent

- 6 values, one per stat: random integer 0-31
- Determined at encounter (wild or egg) and never change
- Hidden from player initially; a late-game NPC (GV Judge) reveals ranges:
  - 0-5: "Mediocre"
  - 6-15: "Decent"
  - 16-25: "Great"
  - 26-30: "Excellent"
  - 31: "Perfect"

### 4.7 Training Points (TP) — EV Equivalent

- Gained from defeating specific dinosaurs (each species gives 1-3 TP in one stat)
- Max 252 TP in a single stat, max 510 TP total
- Every 4 TP = +1 stat point at level 50
- TP-reducing items available (TP Reset Berry equivalent)

**TP yield examples:**
| Defeated Dino | TP Gained |
|---|---|
| PYREX (Baby) | +1 ATK |
| AQUADON (Young) | +2 SPDEF |
| STEGOSHIELD (Adult) | +3 DEF |

### 4.8 Experience Groups

| Group | Total XP to Level 50 | Species Count |
|-------|----------------------|---------------|
| **Fast** | 100,000 | ~50 (common, early-game) |
| **Medium** | 150,000 | ~70 (most species) |
| **Slow** | 200,000 | ~30 (pseudo-legendaries, rare) |

**XP formula per group:**

```
Fast:   XP(L) = (4 * L^3) / 5
Medium: XP(L) = L^3
Slow:   XP(L) = (5 * L^3) / 4
```

Where `XP(L)` = total cumulative XP required to reach level L.

**XP gained from battle:**
```
XP = (BaseXP * EnemyLevel * TrainerBonus) / (5 * ParticipantCount)

Where:
  BaseXP        = Species-specific value (40-220)
  EnemyLevel    = Defeated dino's level
  TrainerBonus  = 1.5 if trainer battle, 1.0 if wild
  ParticipantCount = Number of player's dinos that participated in the fight
```

**EXP Share item** (obtained after Badge 3): Splits XP — 50% to active fighter, 50% split among party.

### 4.9 Starter Dinosaurs (Detailed)

#### PYREX (Feu)
- **Line:** PYREX (Baby) → PYROBLAZE (Young, lvl 16) → INFERNODON (Adult, lvl 36)
- **Adult Types:** Feu / Roche
- **BST:** 310 → 405 → 530
- **Adult Stats:** PV 78, ATK 104, DEF 78, SPATK 85, SPDEF 70, VIT 115
- **Signature Move:** "Eruption Charge" — Feu, Physical, 120 power, 85% acc, user takes 33% recoil
- **Design Note:** Velociraptor-inspired. Fast and fierce.

#### AQUADON (Eau)
- **Line:** AQUADON (Baby) → AQUAJAW (Young, lvl 16) → ABYSSODON (Adult, lvl 36)
- **Adult Types:** Eau / Glace
- **BST:** 314 → 405 → 530
- **Adult Stats:** PV 95, ATK 70, DEF 90, SPATK 105, SPDEF 95, VIT 75
- **Signature Move:** "Abyss Torrent" — Eau, Special, 110 power, 90% acc, 20% freeze chance
- **Design Note:** Mosasaur-inspired. Tanky special attacker.

#### FLORASAUR (Plante)
- **Line:** FLORASAUR (Baby) → FLORAVINE (Young, lvl 16) → TERRAFLORA (Adult, lvl 36)
- **Adult Types:** Plante / Terre
- **BST:** 318 → 405 → 530
- **Adult Stats:** PV 100, ATK 82, DEF 95, SPATK 90, SPDEF 98, VIT 65
- **Signature Move:** "Tectonic Bloom" — Plante, Special, 100 power, 100% acc, sets Terrain to Forest for 5 turns
- **Design Note:** Ankylosaurus/plant hybrid. The bulky balanced option.

---

## 5. Capture System

### 5.1 Capture Rate Formula

```
CatchValue = ((3 * MaxPV - 2 * CurrentPV) * SpeciesCatchRate * BallModifier * StatusModifier * TemperamentModifier) / (3 * MaxPV)

Where:
  MaxPV             = Target's max PV
  CurrentPV         = Target's current PV
  SpeciesCatchRate  = Species-specific value (3 for legendaries, 45 for starters, 255 for common)
  BallModifier      = Ball type multiplier (see below)
  StatusModifier    = 2.0 for Sleep/Freeze, 1.5 for Paralysis/Burn/Poison, 1.0 otherwise
  TemperamentModifier = 0.8, 1.0, or 1.2 (see Temperament section)
```

### 5.2 Shake Check

After calculating CatchValue, perform up to 3 shake checks:

```
ShakeThreshold = 1048560 / sqrt(sqrt(16711680 / CatchValue))

For each shake (3 total):
  Generate random number 0-65535
  If random < ShakeThreshold → shake succeeds
  If random >= ShakeThreshold → ball breaks open, capture fails

If all 3 shakes succeed → CAPTURE!
```

**Approximate capture probabilities at full HP, neutral temperament, standard ball:**
| Species Catch Rate | Probability |
|---|---|
| 255 (very common) | ~33% |
| 190 (common) | ~25% |
| 120 (uncommon) | ~16% |
| 60 (rare) | ~8% |
| 45 (starter-tier) | ~6% |
| 3 (legendary) | ~0.4% |

At 1 HP with Sleep: legendary becomes ~5%, common becomes ~95%.

### 5.3 Ball Types

| Ball | Modifier | Cost | Availability |
|------|----------|------|-------------|
| **Jurassic Ball** | x1.0 | 200 F | Town 1+ |
| **Super Ball** | x1.5 | 600 F | After Badge 3 |
| **Ultra Ball** | x2.0 | 1200 F | After Badge 6 |
| **Chrono Ball** | Turn count / 10, max x4.0 | 1000 F | After Badge 5 |
| **Fossil Ball** | x3.5 for Fossile-type | 1000 F | After Badge 7 |
| **Abyssal Ball** | x3.5 for Eau-type | 1000 F | Port town shop |
| **Dusk Ball** | x3.0 in caves or at night | 1000 F | After Badge 4 |
| **Rapide Ball** | x5.0 on first turn, x1.0 after | 1000 F | After Badge 5 |
| **Master Ball** | Guaranteed capture | Unique (1 only) | Story reward |

### 5.4 Special Capture Conditions

- **Legendaries:** Only one of each. Cannot flee from legendaries (guaranteed encounter until caught or KO'd, KO'd ones respawn after Elite 4 rematch).
- **Static encounters:** Some dinos appear as overworld sprites. Interacting starts battle. These have higher-than-average GVs (minimum 10 in each stat).
- **Fossil Revival:** Bring a fossil item to the lab in Ville Ambre to revive a Fossile-type dino at level 20.
- **Gift Dinos:** Some NPCs give dinos directly (they have set TemperamentModifier, fixed GV of 15 in all stats).

---

## 6. Progression System

### 6.1 XP Curve

See Section 4.8 for XP formulas. XP required per level (Medium group):

| Level | Cumulative XP | XP to Next Level |
|-------|--------------|-----------------|
| 1 | 0 | 8 |
| 5 | 125 | 91 |
| 10 | 1,000 | 331 |
| 15 | 3,375 | 721 |
| 20 | 8,000 | 1,261 |
| 25 | 15,625 | 1,951 |
| 30 | 27,000 | 2,791 |
| 35 | 42,875 | 3,781 |
| 40 | 64,000 | 4,921 |
| 45 | 91,125 | 6,211 |
| 50 | 125,000 | — |

### 6.2 Badge Progression

| Badge | Town | Gym Leader | Type | Level Range | Unlock |
|-------|------|-----------|------|-------------|--------|
| 1 | Ville Griffe | GRIFF | Roche | 10-14 | Coupe (Cut equivalent) |
| 2 | Ville Marais | ONDINE | Eau | 16-20 | Eclairage (Flash equivalent) |
| 3 | Ville Fournaise | VOLCA | Feu | 22-26 | Surf (same name) |
| 4 | Ville Sylve | SILVA | Plante | 26-30 | Force (Strength equivalent) |
| 5 | Ville Eclair | VOLTA | Foudre | 30-34 | Vol (Fly equivalent) |
| 6 | Ville Gelee | FRIMAS | Glace | 33-37 | Plongee (Dive equivalent) |
| 7 | Ville Ombre | NOXUS | Ombre | 36-40 | Escalade (Rock Climb equivalent) |
| 8 | Ville Acier | FERRUM | Acier | 40-45 | Cascade (Waterfall equivalent) |

**Badge stat boosts:** Each badge grants a permanent x1.125 boost to a specific stat in battle:
1. ATK, 2. DEF, 3. SPATK, 4. VIT, 5. PV (via damage reduction), 6. SPDEF, 7. Accuracy, 8. Evasion

### 6.3 Difficulty Scaling

**Trainer Scaling:**
All NPC trainers on routes have levels set relative to the player's average party level:
```
TrainerLevel = clamp(AreaBaseLevel + random(-1, +1), PlayerAvg - 2, PlayerAvg + 2)
```

**Gym Leaders:** Fixed levels, NOT scaled. They define the expected level for that point in the game.

**Wild Dinos:** Fixed level ranges per route (see World Design). NOT scaled.

> **SANA's Note:** Scaling trainers but not wilds creates a nice dynamic — routes feel progressively easier as you outlevel them, but trainers always keep you honest.

### 6.4 Key Items & Gates

| Key Item | Obtained | Gate Unlocked |
|----------|----------|--------------|
| Chaussures Rapides (Running Shoes) | Rival's mom, early game | Sprint on overworld |
| Canne Primitive (Old Rod) | Fisherman NPC, Town 2 | Catch Eau dinos in water tiles |
| Super Canne (Good Rod) | Fisherman NPC, Town 5 | Better Eau dinos |
| Mega Canne (Super Rod) | Fisherman NPC, Town 8 | Best Eau dinos |
| Velo-Raptor (Bicycle) | Bike shop voucher, Town 3 | Fast overworld movement, required for certain paths |
| Detecteur Fossile (Itemfinder) | Professor's aide, Route 6 | Locate hidden items |
| Dinodex Upgrade | After catching 50 dinos | Shows area encounters and catch status |
| Passe Bateau (Boat Pass) | Story event, Town 5 | Access island routes |
| Cle Ancienne (Ancient Key) | Dungeon reward, Cave of Echoes | Unlock Ruins of Pangaea |

---

## 7. Economy & Items

### 7.1 Currency

**Fossiles (F)** — The universal currency. Earned from trainer battles and found on the ground.

**Trainer payout formula:**
```
Payout = TrainerClass * HighestDinoLevel * 4

TrainerClass values:
  Youngster/Lass:     8
  Hiker/Swimmer:      12
  Ace Trainer:        16
  Gym Leader:         25
  Elite 4 Member:     30
  Champion:           50
```

### 7.2 Full Item List

#### Healing Items

| Item | Effect | Price | Availability |
|------|--------|-------|-------------|
| Potion | Restore 20 PV | 200 F | Town 1+ |
| Super Potion | Restore 50 PV | 700 F | After Badge 2 |
| Hyper Potion | Restore 120 PV | 1500 F | After Badge 5 |
| Potion Max | Restore all PV | 2500 F | After Badge 7 |
| Soin Total (Full Heal) | Cure any status | 500 F | After Badge 3 |
| Antidote | Cure Poison | 100 F | Town 1+ |
| Anti-Brulure (Burn Heal) | Cure Burn | 200 F | Town 1+ |
| Anti-Para (Parlyz Heal) | Cure Paralysis | 200 F | Town 1+ |
| Reveil (Awakening) | Cure Sleep | 200 F | Town 1+ |
| Anti-Gel (Ice Heal) | Cure Freeze | 200 F | Town 1+ |
| Rappel (Revive) | Revive fainted dino with 50% PV | 1500 F | After Badge 3 |
| Rappel Max (Max Revive) | Revive fainted dino with 100% PV | 4000 F | Not sold; found only |

#### Capture Items

| Item | Effect | Price | Availability |
|------|--------|-------|-------------|
| Jurassic Ball | Standard ball (x1.0) | 200 F | Town 1+ |
| Super Ball | Better ball (x1.5) | 600 F | After Badge 3 |
| Ultra Ball | Great ball (x2.0) | 1200 F | After Badge 6 |
| Chrono Ball | Better over time (x turn/10) | 1000 F | After Badge 5 |
| Fossil Ball | Bonus vs Fossile types (x3.5) | 1000 F | After Badge 7 |
| Abyssal Ball | Bonus vs Eau types (x3.5) | 1000 F | Town 5 (port) |
| Dusk Ball | Bonus in caves/night (x3.0) | 1000 F | After Badge 4 |
| Rapide Ball | x5 first turn, x1 after | 1000 F | After Badge 5 |
| Master Ball | Guaranteed catch | N/A | One-time story gift |

#### Battle Items

| Item | Effect | Price |
|------|--------|-------|
| Attaque+ (X Attack) | +1 ATK stage in battle | 500 F |
| Defense+ (X Defend) | +1 DEF stage in battle | 500 F |
| Vitesse+ (X Speed) | +1 VIT stage in battle | 500 F |
| Special+ (X Special) | +1 SPATK stage in battle | 500 F |
| Precision+ (X Accuracy) | +1 Accuracy stage in battle | 500 F |
| Repousse Dino (Repel) | No wild encounters for 100 steps | 350 F |
| Super Repousse | No wild encounters for 200 steps | 500 F |
| Max Repousse | No wild encounters for 250 steps | 700 F |
| Corde Sortie (Escape Rope) | Exit dungeon instantly | 550 F |

#### Held Items

| Item | Effect | How Obtained |
|------|--------|-------------|
| Griffe Rapide (Quick Claw) | 20% chance to move first | Found in Cave of Echoes |
| Reste (Leftovers) | Restore 1/16 max PV per turn | Found on Route 10 (hidden) |
| Bandeau Choix (Choice Band) | x1.5 ATK but locked to one move | Battle Tower prize |
| Lunettes Choix (Choice Specs) | x1.5 SPATK but locked to one move | Battle Tower prize |
| Echarpe Choix (Choice Scarf) | x1.5 VIT but locked to one move | Battle Tower prize |
| Ceinture Force (Focus Sash) | Survive one KO hit at 1 PV if at full PV | Battle Tower prize |
| Orbe Vie (Life Orb) | x1.3 damage but lose 10% max PV per attack | Found in Ruins of Pangaea |
| Baie Sitrus | Restore 25% PV when below 50% PV | Berry trees, various routes |
| Baie Lum | Cure any status condition once | Berry trees, Route 8 |
| Pierre Solaire (Sun Stone) | Evolution item | Found in Dusty Plateau |
| Pierre Lunaire (Moon Stone) | Evolution item | Found in Lunar Cave |
| Pierre Fossile (Fossil Stone) | Evolution item | Found in Fossil Quarry |
| Croc Acere (Sharp Fang) | +1 critical hit stage | Found in Predator's Den |

#### TP-modifying Items

| Item | Effect | Price |
|------|--------|-------|
| Proteine | +10 ATK TP | 9800 F |
| Fer (Iron) | +10 DEF TP | 9800 F |
| Calcium | +10 SPATK TP | 9800 F |
| Zinc | +10 SPDEF TP | 9800 F |
| Carbone (Carbos) | +10 VIT TP | 9800 F |
| PV Plus (HP Up) | +10 PV TP | 9800 F |
| Baie Reset | Reset all TP to 0 | Found only (rare) |

### 7.3 Shop Inventory Per Town

| Town | Items Available |
|------|----------------|
| **Village Depart** (Start) | Potion x5 (free from Mom) |
| **Ville Griffe** (Town 1) | Potion, Jurassic Ball, Antidote, Anti-Brulure, Anti-Para, Reveil, Anti-Gel, Repousse |
| **Ville Marais** (Town 2) | + Super Potion, Super Repousse |
| **Ville Fournaise** (Town 3) | + Super Ball, Soin Total, Rappel, Corde Sortie |
| **Ville Sylve** (Town 4) | + Max Repousse, all Battle Items (Attaque+, etc.) |
| **Ville Eclair** (Town 5) | + Chrono Ball, Rapide Ball, Abyssal Ball |
| **Ville Gelee** (Town 6) | + Hyper Potion |
| **Ville Ombre** (Town 7) | + Ultra Ball, Dusk Ball, Fossil Ball |
| **Ville Acier** (Town 8) | + Potion Max, all TP vitamins |
| **Ligue Plateau** (League) | Full inventory (all items) |

---

## 8. World Design

### 8.1 World Map Overview

> **SANA's Note:** The region is called **PANGAERA** — a supercontinent-inspired landmass with diverse biomes. The player starts in the southeast and travels counter-clockwise.

```
                    ┌─────────────┐
                    │ Ligue       │
                    │ Plateau     │
                    └──────┬──────┘
                           │ Victory Road
           ┌───────────────┤
           │         ┌─────┴──────┐
     ┌─────┴────┐    │ Ville      │
     │ Ville     │    │ Acier      │
     │ Ombre     │    │ (Gym 8)    │
     │ (Gym 7)   │    └─────┬──────┘
     └─────┬─────┘          │
           │ Route 12       │ Route 13
     ┌─────┴─────┐    ┌─────┴──────┐
     │ Ville      │    │ Ville      │
     │ Gelee      │    │ Eclair     │
     │ (Gym 6)    │    │ (Gym 5)    │
     └─────┬──────┘    └─────┬──────┘
           │ Route 10        │ Route 9
           └────────┬────────┘
                    │
              ┌─────┴──────┐
              │ Ville       │
              │ Sylve       │
              │ (Gym 4)     │
              └─────┬───────┘
                    │ Route 7
              ┌─────┴───────┐
              │ Ville        │
              │ Fournaise    │
              │ (Gym 3)      │
              └─────┬────────┘
                    │ Route 5
              ┌─────┴───────┐      ┌──────────┐
              │ Ville        │      │ Ile       │
              │ Marais       ├──────┤ Fossile   │
              │ (Gym 2)      │ Boat │ (Post-    │
              └─────┬────────┘      │  game)    │
                    │ Route 3       └──────────┘
              ┌─────┴───────┐
              │ Ville        │
              │ Griffe       │
              │ (Gym 1)      │
              └─────┬────────┘
                    │ Route 1
              ┌─────┴───────┐
              │ Village      │
              │ Depart       │
              └─────────────┘
```

### 8.2 Routes

#### Route 1: Village Depart → Ville Griffe
- **Terrain:** Plaines, light forest
- **Wild Dinos (lvl 2-5):** COMPYX (Plante, common), LITHOREX (Roche, common), PETRODAC (Vol, uncommon)
- **Trainers:** 2 Youngsters (lvl 3-5)
- **Items:** Potion x2 (visible), Antidote (hidden)
- **Features:** Tutorial route. Straight path with one grass patch. NPC teaches catching.

#### Route 2: Branch path from Route 1 (optional)
- **Terrain:** Plaines, rocky
- **Wild Dinos (lvl 3-6):** LITHOREX (Roche), SANDILLO (Sable), ZAPPLET (Foudre, rare)
- **Trainers:** 1 Lass, 1 Youngster (lvl 4-6)
- **Items:** Super Potion (hidden, requires Itemfinder), Jurassic Ball x3
- **Features:** Dead-end route. Optional area for extra training before Gym 1.

#### Route 3: Ville Griffe → Ville Marais
- **Terrain:** Forest transitioning to swamp
- **Wild Dinos (lvl 8-13):** BUGLOR (Plante/Poison), MARECAGE (Eau, swamp area only), FANGLET (Poison)
- **Trainers:** 4 trainers (Bug Catchers, Hikers, lvl 10-13)
- **Items:** Repousse, Potion x2, TM Toxic Spores (hidden)
- **Features:** First dungeon entrance — **Grotte Humide** (short cave, 2 floors). Requires navigating around water. Introduces ledges (one-way jumps).

#### Route 4: Branch from Ville Marais (waterside)
- **Terrain:** Shore, river
- **Wild Dinos (lvl 10-15):** AQUADON (Eau, if not chosen), SHELLTOPS (Eau/Roche), JANGLEX (Plante)
- **Trainers:** 3 Fishermen (lvl 12-15)
- **Items:** Old Rod given by NPC, TM Water Pulse
- **Features:** Fishing spots. First Surf-locked area visible but inaccessible.

#### Route 5: Ville Marais → Ville Fournaise
- **Terrain:** Swamp → volcanic foothills
- **Wild Dinos (lvl 14-19):** MAGMANDER (Feu), TOXIDON (Poison/Terre), GUSTWING (Vol)
- **Trainers:** 5 trainers (Hikers, Ace Trainer, lvl 16-19)
- **Items:** Super Potion, Burn Heal x2, TM Flame Fang
- **Features:** Terrain shifts from green to red/brown. Heat haze visual effect. Optional **Mont Braise** dungeon (volcanic cave, Feu dinos).

#### Route 6: Branch from Ville Fournaise (northern)
- **Terrain:** Rocky highlands
- **Wild Dinos (lvl 18-23):** ARMOREX (Acier/Roche), DRILLTOPS (Terre), PYREX (Feu, if not chosen)
- **Trainers:** 3 trainers (lvl 20-23)
- **Items:** Detecteur Fossile (from Professor's Aide), Super Ball x3
- **Features:** First hidden item area. Good training spot.

#### Route 7: Ville Fournaise → Ville Sylve
- **Terrain:** Dense forest (Foret terrain)
- **Wild Dinos (lvl 20-25):** FLORASAUR (Plante, if not chosen), RAPTOGREEN (Plante/Vol), VENOMTAIL (Poison)
- **Trainers:** 6 trainers (lvl 22-26), including first Ace Trainer duo battle
- **Items:** TM Giga Drain, Max Repousse, hidden Baie Sitrus
- **Features:** Maze-like forest with cuttable trees (Coupe required). Hidden grove with rare Plante dinos.

#### Route 8: Ville Sylve → connector hub
- **Terrain:** Open fields, crossroads
- **Wild Dinos (lvl 24-28):** CHARGEHORN (Foudre), GALEWING (Vol/Acier), SANDSTORM (Sable)
- **Trainers:** 4 trainers (lvl 26-29)
- **Items:** TM Thunderbolt, Baie Lum tree, hidden Rare Candy
- **Features:** Splits to Route 9 (east) and Route 10 (west). Bike path shortcut (requires Velo-Raptor).

#### Route 9: Hub → Ville Eclair
- **Terrain:** Mountain pass, stormy weather
- **Wild Dinos (lvl 28-33):** VOLTADON (Foudre/Roche), STORMWING (Vol/Foudre), DUSKFANG (Ombre)
- **Trainers:** 5 trainers (lvl 30-33)
- **Items:** TM Shadow Claw, Thunder Stone
- **Features:** Weather effects (rain increases Eau/Foudre damage). **Central Caverne** dungeon connects Routes 9 and 10 underground.

#### Route 10: Hub → Ville Gelee
- **Terrain:** Mountain → tundra transition
- **Wild Dinos (lvl 28-33):** GLACIREX (Glace/Roche), FROSTFANG (Glace), YETISAUR (Glace/Terre)
- **Trainers:** 5 trainers (lvl 30-34)
- **Items:** TM Ice Beam, Hyper Potion, hidden Reste (Leftovers)
- **Features:** Sliding ice puzzles in **Grotte Givre**. Snowfall reduces visibility.

#### Route 11: Sea route (Ville Eclair → Ile Fossile, post-game)
- **Terrain:** Open ocean (Surf required)
- **Wild Dinos (lvl 35-45, water):** DEEPDON (Eau/Ombre), LEVIATHAN (Eau, very rare), CORALDON (Eau/Roche)
- **Trainers:** 6 Swimmers (lvl 38-45)
- **Items:** TM Surf Power, hidden Deep Sea Scale
- **Features:** Underwater areas accessible with Plongee. Leads to Fossil Island.

#### Route 12: Ville Gelee → Ville Ombre
- **Terrain:** Haunted forest / mist
- **Wild Dinos (lvl 33-38):** PHANTODON (Ombre), GLOOMWING (Ombre/Vol), LUMINEX (Lumiere, rare)
- **Trainers:** 5 trainers (lvl 35-38)
- **Items:** TM Shadow Ball, Dusk Ball x3, hidden Pierre Lunaire
- **Features:** Mist reduces visibility. **Tour Spectrale** — 5-floor tower dungeon with trainer gauntlet and Ombre legendary at top.

#### Route 13: Ville Eclair → Ville Acier
- **Terrain:** Industrial / mountain
- **Wild Dinos (lvl 34-40):** FERRODON (Acier), GEAREX (Acier/Foudre), SANDILLO evolution
- **Trainers:** 6 trainers (lvl 36-40)
- **Items:** TM Steel Wing, Metal Coat, hidden Iron
- **Features:** Conveyor belt puzzle in **Usine Ancienne** (abandoned factory).

#### Route 14: Victory Road (Ville Acier → Ligue Plateau)
- **Terrain:** Mountain cave, multi-floor dungeon
- **Wild Dinos (lvl 38-45):** DRAKOREX (Fossile/Vol), TITANDON (Roche/Acier), PRISMEX (Lumiere)
- **Trainers:** 10 Ace Trainers (lvl 42-48)
- **Items:** TM Earthquake, Full Restore x2, hidden Rare Candy x2
- **Features:** Requires all 8 field abilities. Strength puzzles, waterfall climbing, rock smashing. Final gauntlet before the League.

### 8.3 Towns

#### Village Depart (Starting Town)
- **Buildings:** Player's house, Rival's house, Professor Pangaea's Lab
- **Key NPCs:** Mom (gives Running Shoes), Professor Pangaea (gives starter + Dinodex), Rival REXO
- **Events:** Starter selection, Rival battle 1 (lvl 5)
- **Shop:** None (Mom gives 5 Potions)

#### Ville Griffe (Gym 1 — Roche)
- **Buildings:** Gym, DinoCenter (heal), DinoMart, NPC houses
- **Key NPCs:** Gym Leader GRIFF (uses Roche team), Move Tutor (teaches Headbutt equivalent)
- **Events:** Gym challenge (rock puzzle — push boulders to create path)
- **Shop Tier:** Basic (Potions, Jurassic Balls, status heals)
- **Gym Team:** LITHOREX lvl 12, BOULDERDON lvl 14

#### Ville Marais (Gym 2 — Eau)
- **Buildings:** Gym, DinoCenter, DinoMart, Fisherman's Hut, Move Deleter house
- **Key NPCs:** Gym Leader ONDINE, Fisherman (Old Rod), Name Rater
- **Events:** Gym challenge (water current puzzle — ride currents to reach leader)
- **Shop Tier:** Basic + Super Potion
- **Gym Team:** MARECAGE lvl 18, SHELLTOPS lvl 19, AQUAJAW lvl 20 (if player didn't pick Eau starter)

#### Ville Fournaise (Gym 3 — Feu)
- **Buildings:** Gym, DinoCenter, DinoMart, Bike Shop, Fossil Lab
- **Key NPCs:** Gym Leader VOLCA, Bike Shop owner (gives voucher after side quest), Fossil Researcher (revives fossils)
- **Events:** Gym challenge (quiz gates — answer questions about types to proceed), Team Jurassique first major encounter
- **Shop Tier:** + Super Ball, Soin Total, Rappel
- **Gym Team:** MAGMANDER lvl 24, PYROBLAZE lvl 25, LAVAREX lvl 26

#### Ville Sylve (Gym 4 — Plante)
- **Buildings:** Gym (giant tree), DinoCenter, DinoMart, Greenhouse, Day Care
- **Key NPCs:** Gym Leader SILVA, Day Care couple (hold 2 dinos, level them up while walking), Berry Master
- **Events:** Gym challenge (hedge maze with trainers), EXP Share given by Professor's Aide (requires 30 dinos caught)
- **Shop Tier:** + Max Repousse, Battle Items
- **Gym Team:** RAPTOGREEN lvl 28, JANGLEX lvl 28, FLORAVINE lvl 29, TERRAFLORA lvl 30

#### Ville Eclair (Gym 5 — Foudre)
- **Buildings:** Gym (power plant), DinoCenter, DinoMart, Boat Dock, Game Corner
- **Key NPCs:** Gym Leader VOLTA, Boat Captain (gives Pass after story event), Move Reminder (re-learn forgotten moves for Heart Scale)
- **Events:** Gym challenge (electrical switch puzzle — activate correct switches to open doors), Team Jurassique submarine heist story event
- **Shop Tier:** + Chrono Ball, Rapide Ball, Abyssal Ball
- **Gym Team:** CHARGEHORN lvl 32, VOLTADON lvl 33, STORMWING lvl 34

#### Ville Gelee (Gym 6 — Glace)
- **Buildings:** Gym (ice cavern), DinoCenter, DinoMart, Hot Springs (fully heals party once per visit)
- **Key NPCs:** Gym Leader FRIMAS, GV Judge (rates Genetic Values)
- **Events:** Gym challenge (sliding ice floor puzzle), Rival battle 5 (lvl 33-36)
- **Shop Tier:** + Hyper Potion
- **Gym Team:** FROSTFANG lvl 35, GLACIREX lvl 36, YETISAUR lvl 36, AURORADON lvl 37

#### Ville Ombre (Gym 7 — Ombre)
- **Buildings:** Gym (haunted mansion), DinoCenter, DinoMart, Mystic's House (teaches Ombre moves)
- **Key NPCs:** Gym Leader NOXUS, Mystic (gives Shadow TM), Old woman (tells legendary lore)
- **Events:** Gym challenge (darkness puzzle — room is dark, must navigate by limited light), Team Jurassique HQ infiltration story arc
- **Shop Tier:** + Ultra Ball, Dusk Ball, Fossil Ball
- **Gym Team:** PHANTODON lvl 38, DUSKFANG lvl 39, GLOOMWING lvl 39, ABYSSODON lvl 40 (Ombre/Eau)

#### Ville Acier (Gym 8 — Acier)
- **Buildings:** Gym (fortress), DinoCenter, DinoMart, Move Tutor House (ultimate moves), Tm Mega Store
- **Key NPCs:** Gym Leader FERRUM, Ultimate Move Tutor (teaches signature moves to fully evolved starters), TP Trainer (shows TP values)
- **Events:** Gym challenge (gear/conveyor puzzle), Rival battle 6 (lvl 42-46)
- **Shop Tier:** Full inventory
- **Gym Team:** FERRODON lvl 42, GEAREX lvl 43, ARMOREX lvl 43, TITANDON lvl 44, CHROMADON lvl 45

#### Ligue Plateau (Dino League)
- **Buildings:** DinoCenter, Shop (full inventory), Hall of Fame
- **Key NPCs:** Elite 4, Champion
- **Shop:** Everything available. Last chance to stock up.

#### Ile Fossile (Post-game island)
- **Buildings:** Research Center, Battle Tower, Fossil Museum
- **Key NPCs:** Battle Tower Master, Fossil Collector, Legendary quest NPC
- **Features:** Post-game hub (see Section 13)

### 8.4 Dungeons

| Dungeon | Location | Floors | Puzzle | Key Dinos |
|---------|----------|--------|--------|-----------|
| **Grotte Humide** | Route 3 | 2 | Navigate around water, push rocks | MARECAGE, GLOWTAIL (Lumiere, rare) |
| **Mont Braise** | Route 5 (optional) | 3 | Lava flow timing, strength boulders | MAGMANDER, LAVALING (Feu/Roche) |
| **Central Caverne** | Routes 9/10 | 4 | Strength + Flash + Rock Smash combo | Mixed types, DRAKOREX egg (event) |
| **Grotte Givre** | Route 10 | 3 | Ice sliding puzzles | GLACIREX, CRYSTADON (Glace/Lumiere, rare) |
| **Tour Spectrale** | Route 12 | 5 | Trainer gauntlet, warp tiles | PHANTODON, SPECTREX (Ombre legendary at top) |
| **Usine Ancienne** | Route 13 | 3 | Conveyor belts, switch timing | GEAREX, FERRODON |
| **Victoire Cave** | Route 14 | 5 | All HM abilities required | TITANDON, DRAKOREX, rare spawns |
| **Ruines de Pangaea** | Post-game | 7 | Ancient puzzle (requires Cle Ancienne) | CHRONODON (Fossile legendary) |
| **Abyssal Trench** | Post-game (underwater) | 4 | Dive puzzles, current navigation | LEVIATHAN (Eau legendary) |

### 8.5 Hidden Areas & Secrets

- **Jardin Secret** (Secret Garden): Behind Ville Sylve, accessible with Coupe + Surf. Contains rare Plante and Lumiere dinos.
- **Ile Inconnue** (Unknown Island): Accessible after obtaining National Dinodex. Roaming legendary SOLAREX (Lumiere/Feu) appears here.
- **Souterrain Ancien** (Ancient Tunnel): Under Ville Griffe, accessible after Champion. Contains fossil items and ANCESTREX (Fossile/Roche, high-level static encounter).
- **Cratere Lunaire** (Lunar Crater): Top of mountain accessible with Escalade + Vol. LUNASAUR (Ombre/Glace) appears at night only.

---

## 9. NPC System

### 9.1 NPC Types

| Type | Function | Count |
|------|----------|-------|
| **Townsfolk** | Flavor dialogue, world-building, hints | ~80 |
| **Trainers** | Battle on sight (one-time per save) | ~120 |
| **Gym Leaders** | Boss battles, give badges + TMs | 8 |
| **Elite 4 + Champion** | Final boss gauntlet | 5 |
| **Rival (REXO)** | Recurring battles, story driver | 7 encounters |
| **Team Jurassique** | Villain team grunts + admins + boss | ~30 |
| **Service NPCs** | DinoCenter Nurse, Shop Clerk, Move Tutor, etc. | ~25 |
| **Professor Pangaea** | Story progression, Dinodex upgrades | 1 |
| **Quest NPCs** | Give items/rewards for tasks | ~15 |

### 9.2 Dialogue System

**Dialogue Structure:**
- Each NPC has 1-3 dialogue states based on story flags.
- Dialogue is context-sensitive: NPCs react to badges earned, team composition, story events.
- Max dialogue length: 3 text boxes per interaction (keep it snappy).

**Dialogue Box:** 2 lines, ~36 characters per line (GBA style).

**Special dialogue triggers:**
- Walking with a specific dino type near an NPC
- Visiting at different times (if day/night cycle exists)
- After defeating a gym leader
- After catching a certain number of dinos

### 9.3 Rival Encounter Schedule

| Encounter | Location | Trigger | Rival's Ace Level |
|-----------|----------|---------|-------------------|
| 1 | Professor's Lab | Story event | 5 |
| 2 | Route 3 entrance | Automatic | 13 |
| 3 | Ville Fournaise | Before Gym 3 | 23 |
| 4 | Route 8 crossroads | Automatic | 30 |
| 5 | Ville Gelee | After Gym 6 | 36 |
| 6 | Ville Acier | Before Gym 8 | 46 |
| 7 | Champion battle | Elite 4 gauntlet | 50 |

**Rival team:** Always has the starter strong against player's choice, plus a balanced team of 5 other dinos (varies by encounter). Rival's team is fixed, not scaled.

### 9.4 Team Jurassique

**Theme:** A group trying to resurrect ancient mega-dinosaurs to "restore the world to its natural order."

**Structure:**
- **Grunts:** Use Poison, Ombre, and Fossile types (lvl varies by story point)
- **Admin PETRA:** Specializes in Roche/Fossile (3 encounters)
- **Admin TOXIS:** Specializes in Poison/Ombre (3 encounters)
- **Boss REX:** Final encounter with full team of 6, diverse types, ace is a pseudo-legendary Fossile dino

**Story beats:**
1. First encounter: Route 5, grunts blocking path (1-2 battles)
2. Ville Eclair: Submarine heist (infiltrate base, 4-5 grunt battles, Admin PETRA boss)
3. Route 9: Ambush (Admin TOXIS boss)
4. Ville Ombre: HQ infiltration (6-8 grunt battles, both Admins, Boss REX first fight)
5. Ruines de Pangaea: Final confrontation (Boss REX final battle, ties into legendary quest)

---

## 10. Field Mechanics

### 10.1 Overworld Movement

- **Walking:** Default speed. 1 tile per 0.25 seconds.
- **Running (Chaussures Rapides):** Obtained early. 1 tile per 0.125 seconds. Hold B to run.
- **Velo-Raptor (Bicycle):** Obtained in Ville Fournaise. 1 tile per 0.0625 seconds. Some paths are bike-only (narrow ledges).

### 10.2 Field Abilities (HM Equivalents)

Field abilities are usable outside of battle by dinos in the party that know them. They are NOT moves used in battle (avoids the "HM slave" problem). Instead, they are permanent passive abilities unlocked by earning badges.

| Ability | Badge Required | Effect | Replaces |
|---------|---------------|--------|----------|
| **Coupe** (Cut) | Badge 1 | Cut down small trees blocking paths | HM01 Cut |
| **Eclairage** (Flash) | Badge 2 | Light up dark caves | HM05 Flash |
| **Surf** | Badge 3 | Travel across water on a dino | HM03 Surf |
| **Force** (Strength) | Badge 4 | Push large boulders | HM04 Strength |
| **Vol** (Fly) | Badge 5 | Fast travel to any visited town | HM02 Fly |
| **Plongee** (Dive) | Badge 6 | Dive underwater at dark water spots | Dive |
| **Escalade** (Rock Climb) | Badge 7 | Climb rough rock walls | Rock Climb |
| **Cascade** (Waterfall) | Badge 8 | Climb waterfalls while surfing | HM07 Waterfall |

**Implementation:** When approaching a field obstacle, a prompt appears: "This tree looks cuttable. Use Coupe? (A/B)." Any dino in the party with the ability can use it — no need to be in the active slot.

**Dinos that learn each ability:**
- Coupe: Most Plante and Acier types
- Eclairage: Most Foudre and Lumiere types
- Surf: Most Eau types
- Force: Most Roche and Terre types
- Vol: Most Vol types
- Plongee: Large Eau types only
- Escalade: Most Roche and Terre types
- Cascade: Large Eau types only

### 10.3 Day/Night Cycle

**Not implemented in base game.** The GBA Fire Red style uses a static daytime. However, certain events reference "night":
- NOCTURN evolves if leveled up between 6 PM and 6 AM (system clock).
- Lunar Crater legendary only appears between 8 PM and 4 AM.
- Dusk Balls get their bonus in caves (simulating darkness) instead of true night.

> **YUKI's Note:** We keep it simple for the GBA aesthetic. A full day/night cycle adds complexity that doesn't serve the core loop for this project.

### 10.4 Fishing

| Rod | Location | Dinos Available |
|-----|----------|----------------|
| **Canne Primitive** (Old Rod) | Town 2, from Fisherman | SHELLTOPS (Eau/Roche, lvl 5-10), FINFANG (Eau, lvl 5-10) |
| **Super Canne** (Good Rod) | Town 5, from Fisherman | AQUAJAW (Eau, lvl 15-25), CORALDON (Eau/Roche, lvl 15-25) |
| **Mega Canne** (Super Rod) | Town 8, from Fisherman | ABYSSODON (Eau/Glace, lvl 30-40), DEEPDON (Eau/Ombre, lvl 30-40), LEVIATHAN (Eau, lvl 40, very rare) |

**Fishing minigame:** Press A when "!" appears. Timing window: 0.5 seconds (Old Rod), 0.3 seconds (Good/Super).

### 10.5 Headbutt & Rock Smash (Hidden Dino Encounters)

- **Headbutt Trees:** Certain trees on routes can be headbutted (requires a dino with "Impact Crane" move). Spawns arboreal dinos not found in grass: COMPYX, NESTLING (Vol/Plante), BARKDON (Plante/Roche).
- **Brise Roche (Rock Smash):** Smashable rocks in caves spawn underground dinos: GEODON (Roche), GEMMEX (Roche/Lumiere), FOSSILING (Fossile, very rare).

---

## 11. Save System

### 11.1 Save Slots

**Single save slot** per cartridge/game file (classic GBA style).

### 11.2 What Is Saved

- Player position (map, coordinates, direction)
- Party dinos (all stats, moves, items, GV, TP, happiness, status)
- PC Box dinos (30 boxes x 30 slots = 900 storage)
- Bag inventory (items, key items, TMs, held items)
- Dinodex completion status
- Story flags and event progress
- Badge count and field ability unlocks
- Play time
- Money (Fossiles)
- Options (text speed, battle animations, sound)
- Trainer card (name, ID, badges, Hall of Fame entries)

### 11.3 Save Locations

Save anywhere via the menu (Start → Save). No restrictions.

**Autosave:** Not present (authentic GBA experience). A warning message plays if the player attempts to turn off during save.

---

## 12. Dinodex

### 12.1 Entry Format

Each Dinodex entry contains:

```
No. [001-150]
Name: [SPECIES NAME]
Types: [Type 1] / [Type 2 or —]
Classification: [Category, e.g., "Flame Predator Dino"]
Height: [X.X m]
Weight: [XX.X kg]
Description: [2-3 sentences of flavor text about the species]
Habitat: [Route/Area where found]
Rarity: [Common / Uncommon / Rare / Very Rare / Legendary]
```

### 12.2 Sample Entries

```
No. 001
Name: PYREX
Types: Feu
Classification: Flame Hatchling Dino
Height: 0.6 m
Weight: 8.5 kg
Description: A small raptor dino with ember-tipped feathers.
  Its body temperature runs so hot that it leaves scorch
  marks where it sleeps.
Habitat: Starter (Professor Pangaea's Lab)
Rarity: Starter
```

```
No. 004
Name: AQUADON
Types: Eau
Classification: Tide Pup Dino
Height: 0.5 m
Weight: 12.0 kg
Description: This aquatic dino is born knowing how to swim.
  It playfully squirts water at anyone who approaches,
  then hides behind its trainer's legs.
Habitat: Starter (Professor Pangaea's Lab)
Rarity: Starter
```

```
No. 007
Name: FLORASAUR
Types: Plante
Classification: Sprout Shell Dino
Height: 0.7 m
Weight: 15.0 kg
Description: A gentle herbivore whose back sprouts a small
  fern. The fern grows larger as FLORASAUR evolves,
  eventually becoming a full garden.
Habitat: Starter (Professor Pangaea's Lab)
Rarity: Starter
```

```
No. 150
Name: CHRONODON
Types: Fossile / Lumiere
Classification: Time Sovereign Dino
Height: 12.5 m
Weight: 850.0 kg
Description: Said to have existed before time itself was
  measured. Ancient texts describe it as the guardian of
  Pangaea's past, present, and future.
Habitat: Ruines de Pangaea (Post-game legendary)
Rarity: Legendary
```

### 12.3 Dinodex Distribution by Type

| Type | Primary Count | Secondary Appearances |
|------|--------------|----------------------|
| Roche | 12 | 8 |
| Eau | 14 | 6 |
| Feu | 11 | 5 |
| Plante | 13 | 6 |
| Glace | 8 | 5 |
| Vol | 10 | 8 |
| Terre | 10 | 7 |
| Foudre | 9 | 4 |
| Poison | 10 | 5 |
| Acier | 8 | 6 |
| Ombre | 9 | 4 |
| Lumiere | 7 | 5 |
| Sable | 8 | 4 |
| Fossile | 6 | 3 |

*Some dinos are dual-typed, so they appear in both columns.*

---

## 13. Post-Game Content

### 13.1 After Becoming Champion

Upon defeating the Champion (Rival REXO) and entering the Hall of Fame:

1. **Credits roll** with scenes of your journey.
2. Player restarts in Village Depart.
3. Professor Pangaea upgrades the Dinodex to **National mode** (shows all 150 + location data).
4. New areas unlock:
   - **Ile Fossile** (Battle Tower, Fossil Museum, post-game quests)
   - **Ruines de Pangaea** (legendary quest for CHRONODON)
   - **Abyssal Trench** (legendary quest for LEVIATHAN)
   - **Cratere Lunaire** (legendary LUNASAUR at night)
   - **Ile Inconnue** (roaming legendary SOLAREX)

### 13.2 Legendary Quests

**CHRONODON (Fossile/Lumiere) — BST 580:**
1. Collect 5 Ancient Tablet fragments (scattered in post-game dungeons).
2. Bring them to the Fossil Museum on Ile Fossile.
3. Researcher deciphers location of Ruines de Pangaea.
4. Navigate 7-floor dungeon with puzzles involving all field abilities.
5. CHRONODON awaits at the bottom. Level 50. Static encounter.

**LEVIATHAN (Eau/Fossile) — BST 580:**
1. Obtain the Deep Sea Compass from a fisherman on Ile Fossile (requires catching 100 dinos).
2. Use Plongee at a specific spot on Route 11.
3. Navigate Abyssal Trench (4 floors, underwater puzzles).
4. LEVIATHAN at the deepest point. Level 50. Static encounter.

**LUNASAUR (Ombre/Glace) — BST 570:**
1. Talk to the Old Woman in Ville Ombre after becoming Champion.
2. She gives you the Moon Fragment.
3. Travel to Cratere Lunaire (top of mountain, requires Escalade + Vol).
4. Use Moon Fragment at the altar between 8 PM and 4 AM.
5. LUNASAUR appears. Level 48. Static encounter.

**SOLAREX (Lumiere/Feu) — BST 570:**
1. After catching LUNASAUR, a new event triggers.
2. Professor Pangaea reports sightings of a blazing dino across Pangaera.
3. SOLAREX roams the overworld (changes route after each encounter, flees after 1 turn).
4. Must be tracked and weakened over multiple encounters. Damage persists between encounters.
5. Level 48. Roaming encounter.

### 13.3 Battle Tower (Tour de Combat)

- **Location:** Ile Fossile
- **Format:** Single battles, 3v3, set level 50
- **Rules:** No items, no duplicate species, no duplicate held items
- **Structure:** Win 7 battles in a row to earn Battle Points (BP)
- **Rewards:** Choice Band, Choice Specs, Choice Scarf, Focus Sash, and other competitive items
- **Boss trainers:** Every 7th battle is a Tower Trainer with a themed team and perfect GVs

**Battle Points (BP) shop:**

| Item | BP Cost |
|------|---------|
| Choice Band | 48 BP |
| Choice Specs | 48 BP |
| Choice Scarf | 48 BP |
| Focus Sash | 48 BP |
| Life Orb | 48 BP |
| Muscle Band (+10% physical) | 32 BP |
| Wise Glasses (+10% special) | 32 BP |
| TP Reset Berry | 16 BP |
| Rare Candy | 24 BP |
| All vitamins | 2 BP each |

### 13.4 Rematch System

After becoming Champion:
- Gym Leaders can be rematched once per day. Their teams are upgraded to lvl 45-50 with full teams of 6.
- Elite 4 can be re-challenged. Their teams scale to lvl 50 with improved coverage moves.
- Rival REXO appears at the top of Victoire Cave every weekend (in-game day counter) with his full lvl 50 team.

### 13.5 Completion Rewards

| Milestone | Reward |
|-----------|--------|
| Catch 50 dinos | Dinodex Upgrade (shows area encounters) |
| Catch 100 dinos | Deep Sea Compass (unlocks Leviathan quest) |
| Catch all 150 dinos | Diplome from Professor Pangaea + Shiny Charm (3x shiny rate) |
| Win 50 Battle Tower fights | Gold Trainer Card |
| Defeat all post-game legendaries | Ancient Crown (held item: +10% all stats, cosmetic) |

### 13.6 Shiny Dinosaurs

- **Base rate:** 1/4096
- **Shiny Charm:** 3/4096 (obtained for completing Dinodex)
- **Visual:** Alternate color palette + sparkle animation on send-out
- **Mechanical:** Purely cosmetic. No stat difference.

---

## Appendix A: Elite 4 + Champion Teams

### Elite 4 Member 1: TERRA (Terre/Sable specialist)
| Dino | Type | Level |
|------|------|-------|
| DRILLTOPS | Terre | 44 |
| SANDSTORM | Sable | 44 |
| TOXIDON | Poison/Terre | 45 |
| YETISAUR | Glace/Terre | 45 |
| TITANDON | Roche/Acier | 46 |

### Elite 4 Member 2: LUMINA (Lumiere/Feu specialist)
| Dino | Type | Level |
|------|------|-------|
| LUMINEX | Lumiere | 45 |
| PRISMEX | Lumiere | 45 |
| INFERNODON | Feu/Roche | 46 |
| SOLARWING | Vol/Lumiere | 46 |
| CRYSTADON | Glace/Lumiere | 47 |

### Elite 4 Member 3: ABYSSO (Eau/Ombre specialist)
| Dino | Type | Level |
|------|------|-------|
| DEEPDON | Eau/Ombre | 46 |
| PHANTODON | Ombre | 46 |
| ABYSSODON | Eau/Glace | 47 |
| GLOOMWING | Ombre/Vol | 47 |
| CORALDON | Eau/Roche | 48 |

### Elite 4 Member 4: FOSSIUS (Fossile/Acier specialist)
| Dino | Type | Level |
|------|------|-------|
| DRAKOREX | Fossile/Vol | 47 |
| FERRODON | Acier | 47 |
| ARMOREX | Acier/Roche | 48 |
| ANCESTREX | Fossile/Roche | 48 |
| GEAREX | Acier/Foudre | 49 |

### Champion REXO (Rival — Balanced Team)
| Dino | Type | Level |
|------|------|-------|
| DRAKOREX | Fossile/Vol | 48 |
| CHARGEHORN | Foudre | 48 |
| STEGOSHIELD | Roche/Plante | 49 |
| GLACIREX | Glace/Roche | 49 |
| PHANTODON | Ombre | 49 |
| [Starter strong vs. player] | [Varies] | 50 |

---

## Appendix B: TM List (Condensed)

| TM | Move | Type | Category | Power | Accuracy | Location |
|----|------|------|----------|-------|----------|----------|
| 01 | Primal Slash | Roche | Physical | 75 | 100% | Gym 1 reward |
| 02 | Tidal Wave | Eau | Special | 80 | 100% | Gym 2 reward |
| 03 | Flame Fang | Feu | Physical | 65 | 95% | Route 5 |
| 04 | Toxic Spores | Poison | Status | — | 90% | Route 3 (hidden) |
| 05 | Ancient Roar | Fossile | Special | 60 | 100% | Ville Fournaise |
| 06 | Protective Shell | Acier | Status | — | —% | Gym 8 reward |
| 07 | Giga Drain | Plante | Special | 75 | 100% | Route 7 |
| 08 | Thunderbolt | Foudre | Special | 90 | 100% | Route 8 |
| 09 | Ice Beam | Glace | Special | 90 | 100% | Route 10 |
| 10 | Shadow Ball | Ombre | Special | 80 | 100% | Route 12 |
| 11 | Steel Wing | Acier | Physical | 70 | 90% | Route 13 |
| 12 | Earthquake | Terre | Physical | 100 | 100% | Victory Road |
| 13 | Luminous Pulse | Lumiere | Special | 85 | 100% | Gym 7 reward |
| 14 | Sand Tomb | Sable | Physical | 70 | 85% | Desert area |
| 15 | Aerial Ace | Vol | Physical | 60 | —% | Gym 5 reward |

---

## Appendix C: Design Philosophy Notes

> **YUKI (Lead Game Designer):**
> "This game lives and dies by the first hour. Pick your starter, beat your rival, catch your first dino in the grass, and feel the world open up before you. Every system we build serves that feeling of discovery. If a mechanic doesn't make the player say 'I want to see what's next,' we cut it."

> **MARCUS (System Designer):**
> "The formulas are deliberately close to the classic Pokémon Gen III math because it's been battle-tested for 20+ years. Where we deviate — terrain bonuses, the temperament capture modifier, field abilities not being battle moves — it's to fix known pain points while keeping the depth that competitive players love. The 14-type chart gives us more design space without the bloat of 18 types."

> **SANA (Level Designer):**
> "The world is designed as a counter-clockwise spiral so backtracking feels intentional, not tedious. Every time you unlock a new field ability, there should be 2-3 previously visible but inaccessible areas that suddenly open up. That 'oh, NOW I can get there!' moment is pure dopamine. The route density targets ensure the player is never bored — every 30 seconds, something happens."

---

*End of Game Design Document — Jurassic Trainers v1.0*
*Document authored by YUKI, MARCUS, and SANA of Nova Forge Studio*
