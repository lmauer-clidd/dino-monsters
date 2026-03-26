# Type Numbering Systems -- Dino Monsters

## Why Two Different Systems?

The JSON data files (`dinos.json`, `moves.json`, `type_chart.json`) were generated first during
the data design phase, using their own numbering order (Rock/Fossil first, then Water, Fire, etc.).

The TypeScript `DinoType` enum in `constants.ts` was designed later during the code phase, using
a more conventional order (Normal first, then Fire, Water, etc.).

**These two systems are NOT compatible.** Using a raw JSON type number directly as a DinoType
value will produce the wrong type (e.g., JSON `2` = Fire, but DinoType `2` = Water).

---

## Side-by-Side Mapping Table

| Index | JSON meaning (data files) | DinoType enum (TypeScript) |
|-------|--------------------------|---------------------------|
| 0     | Fossil / Roche           | Normal                    |
| 1     | Water / Eau              | Fire                      |
| 2     | Fire / Feu               | Water                     |
| 3     | Flora / Plante           | Earth                     |
| 4     | Ice / Glace              | Air                       |
| 5     | Air / Vol                | Electric                  |
| 6     | Earth / Terre            | Ice                       |
| 7     | Electric / Foudre        | Venom                     |
| 8     | Venom / Poison           | Flora                     |
| 9     | Metal / Acier            | Fossil                    |
| 10    | Shadow / Ombre           | Shadow                    |
| 11    | Light / Lumiere          | Light                     |
| 12    | Normal / Sable           | Metal                     |
| 13    | Primal / Fossile         | Primal                    |

Note: Shadow (10), Light (11), and Primal (13) happen to have the same index in both systems.
All others differ.

---

## Conversion at Load Time

The conversion is handled in `BootScene.ts` via the `JSON_TO_DINOTYPE` mapping table and the
`mapType()` helper function. Every dino type and move type read from JSON passes through this
function before being stored in the game's runtime data structures.

```
JSON_TO_DINOTYPE: Record<number, DinoType> = {
  0:  DinoType.Fossil,   // JSON 0 (Roche)     -> DinoType.Fossil (9)
  1:  DinoType.Water,    // JSON 1 (Eau)        -> DinoType.Water  (2)
  2:  DinoType.Fire,     // JSON 2 (Feu)        -> DinoType.Fire   (1)
  3:  DinoType.Flora,    // JSON 3 (Plante)     -> DinoType.Flora  (8)
  ...
};

const mapType = (jsonType: number): DinoType =>
  JSON_TO_DINOTYPE[jsonType] ?? DinoType.Normal;
```

---

## WARNING

**Never use raw JSON type numbers in TypeScript code -- always use the `DinoType` enum after
loading.** The JSON numbers are only valid within the JSON files themselves (and in
`type_chart.json` for cross-referencing). Once data passes through `BootScene.mapType()`,
all type references use DinoType enum values.

If you add a new type or modify the JSON data files, you MUST update the `JSON_TO_DINOTYPE`
table in `BootScene.ts` to keep the mapping correct.
