// ============================================================
// Jurassic Trainers — Complete Map Data
// All maps with full tile arrays, NPCs, warps, encounters
// ============================================================

// ============================================================
// LEVEL PROGRESSION GUIDE (validated by COMPASS — Phase 2)
//
// Route 1:  Wild Lv.2-5,   Trainers Lv.3-6,   Player arrives Lv.5
// Route 2:  Wild Lv.4-8,   Trainers Lv.6-9,   Player arrives Lv.7-9
// Gym 1 (Flora/Ville-Fougere):  Leader Lv.12-14,  Player should be Lv.10-13
// Route 3:  Wild Lv.7-11,  Trainers Lv.9-12,  Player arrives Lv.12-14
// Gym 2 (Marin/Port-Coquille):  Leader Lv.15-18,  Player should be Lv.14-17
// Route 4:  Wild Lv.10-15, Trainers Lv.13-17, Player arrives Lv.17-19
// Gym 3 (Roche-Haute/Fossil):   Leader Lv.19-22,  Player should be Lv.18-21
// Gym 4 (Volcanville/Fire):     Leader Lv.23-26,  Player should be Lv.22-25
// Route 5:  Wild Lv.14-19, Trainers Lv.18-22, Player arrives Lv.24-26
// Gym 5 (Cryo-Cite/Ice):        Leader Lv.27-30,  Player should be Lv.26-29
// Route 6:  Wild Lv.18-23, Trainers Lv.22-26, Player arrives Lv.28-30
// Gym 6 (Electropolis/Electric): Leader Lv.31-34,  Player should be Lv.30-33
// Route 7:  Wild Lv.22-27, Trainers Lv.26-30, Player arrives Lv.32-34
// Gym 7 (Marais-Noir/Venom):    Leader Lv.35-38,  Player should be Lv.34-37
// Route 8:  Wild Lv.26-32, Trainers Lv.30-35, Player arrives Lv.36-38
// Gym 8 (Ciel-Haut/Air):        Leader Lv.39-42,  Player should be Lv.38-41
// Route 9:  Wild Lv.30-38, Trainers Lv.35-40, Player arrives Lv.40-42
// Elite 4 + Champion:           Lv.42-50,         Player should be Lv.42-48
// ============================================================

// --- Tile Types ---
export enum TileType {
  Grass = 0,
  Path = 1,
  TallGrass = 2,
  Water = 3,
  Tree = 4,
  Wall = 5,
  Roof = 6,
  Door = 7,
  Fence = 8,
  Flower = 9,
  Sign = 10,
  Ledge = 11,
  Sand = 12,
  Ice = 13,
  Lava = 14,
  Bridge = 15,
}

// --- Interfaces ---
export interface MapNPC {
  id: string;
  name: string;
  x: number;
  y: number;
  spriteType: 'mom' | 'professor' | 'assistant' | 'trainer' | 'villager' | 'shopkeeper' | 'nurse' | 'rival' | 'grunt';
  dialogue: string[];
  facing: 'up' | 'down' | 'left' | 'right';
  movement?: 'static' | 'wander';
  /** If set, this NPC is hidden when the specified flag is true */
  hideIfFlag?: string;
}

export interface BuildingData {
  id: string;
  type: 'dino_center' | 'shop' | 'gym' | 'house';
  doorX: number;
  doorY: number;
  signX: number;
  signY: number;
  signText: string;
  sceneData?: Record<string, any>;
}

export interface MapWarp {
  x: number;
  y: number;
  targetMap: string;
  targetX: number;
  targetY: number;
}

export interface WildEncounterTable {
  dinoId: number;
  name: string;
  minLevel: number;
  maxLevel: number;
  weight: number;
}

export interface MapData {
  id: string;
  name: string;
  width: number;
  height: number;
  tiles: number[][];
  collisionTiles: number[];
  encounterTiles: number[];
  encounterRate: number;
  encounters: WildEncounterTable[];
  npcs: MapNPC[];
  warps: MapWarp[];
  buildings?: BuildingData[];
  music?: string;
  terrain?: string;
}

// ============================================================
// Helper: generate a 2D array filled with a value
// ============================================================
function fillGrid(w: number, h: number, val: number): number[][] {
  const grid: number[][] = [];
  for (let y = 0; y < h; y++) {
    grid[y] = new Array(w).fill(val);
  }
  return grid;
}

function setRect(tiles: number[][], x: number, y: number, w: number, h: number, val: number): void {
  for (let row = y; row < y + h; row++) {
    for (let col = x; col < x + w; col++) {
      if (row >= 0 && row < tiles.length && col >= 0 && col < tiles[0].length) {
        tiles[row][col] = val;
      }
    }
  }
}

function setTile(tiles: number[][], x: number, y: number, val: number): void {
  if (y >= 0 && y < tiles.length && x >= 0 && x < tiles[0].length) {
    tiles[y][x] = val;
  }
}

function buildBuilding(tiles: number[][], bx: number, by: number, w: number, h: number, doorX: number, doorY: number, roofColor?: number): void {
  // Roof (top 2 rows)
  for (let x = bx; x < bx + w; x++) {
    setTile(tiles, x, by, TileType.Roof);
    if (h > 2) setTile(tiles, x, by + 1, TileType.Roof);
  }
  // Walls
  const wallStart = h > 2 ? by + 2 : by + 1;
  for (let y = wallStart; y < by + h; y++) {
    for (let x = bx; x < bx + w; x++) {
      setTile(tiles, x, y, TileType.Wall);
    }
  }
  // Door
  setTile(tiles, doorX, doorY, TileType.Door);
}

/**
 * Ensure all building doors are placed AFTER paths have been drawn.
 * Call this at the end of each map creation function to fix doors
 * that may have been overwritten by path/terrain tiles.
 */
function restoreDoors(tiles: number[][], buildings?: BuildingData[]): void {
  if (!buildings) return;
  for (const b of buildings) {
    setTile(tiles, b.doorX, b.doorY, TileType.Door);
  }
}

// ============================================================
// MAP 1: BOURG_NID — Starting Village (40x30)
// ============================================================
function createBourgNid(): MapData {
  const W = 40, H = 30;
  const tiles = fillGrid(W, H, TileType.Grass);

  // --- Top boundary: dense trees (3 rows) ---
  setRect(tiles, 0, 0, W, 3, TileType.Tree);

  // --- Left boundary: trees ---
  for (let y = 0; y < H; y++) {
    setTile(tiles, 0, y, TileType.Tree);
    setTile(tiles, 1, y, TileType.Tree);
  }

  // --- Right boundary: trees ---
  for (let y = 0; y < H; y++) {
    setTile(tiles, W - 1, y, TileType.Tree);
    setTile(tiles, W - 2, y, TileType.Tree);
  }

  // --- Bottom boundary: trees except south exit (center) ---
  for (let x = 0; x < W; x++) {
    if (x < 18 || x > 21) {
      setTile(tiles, x, H - 1, TileType.Tree);
      setTile(tiles, x, H - 2, TileType.Tree);
    }
  }

  // --- Pond area (left side) with bridge ---
  for (let y = 8; y <= 14; y++) {
    for (let x = 2; x <= 7; x++) {
      setTile(tiles, x, y, TileType.Water);
    }
  }
  // Bridge across pond
  setTile(tiles, 5, 10, TileType.Bridge);
  setTile(tiles, 5, 11, TileType.Bridge);

  // --- Player's House (4x3) at position (9, 6) ---
  buildBuilding(tiles, 9, 6, 4, 3, 11, 8);
  // Sign next to house
  setTile(tiles, 8, 8, TileType.Sign);

  // --- Prof Paleo's Lab (5x3) at position (17, 5) --- bigger
  buildBuilding(tiles, 17, 5, 5, 4, 19, 8);
  // Sign next to lab
  setTile(tiles, 16, 8, TileType.Sign);

  // --- Small house (3x3) right side ---
  buildBuilding(tiles, 27, 7, 4, 3, 29, 9);

  // --- Main horizontal path ---
  for (let x = 5; x <= 35; x++) {
    setTile(tiles, x, 12, TileType.Path);
    setTile(tiles, x, 13, TileType.Path);
  }

  // Vertical path from player house to main path (start BELOW door at y=9)
  for (let y = 9; y <= 13; y++) {
    setTile(tiles, 11, y, TileType.Path);
    setTile(tiles, 12, y, TileType.Path);
  }

  // Vertical path from lab to main path (start BELOW door at y=9)
  for (let y = 9; y <= 13; y++) {
    setTile(tiles, 19, y, TileType.Path);
    setTile(tiles, 20, y, TileType.Path);
  }

  // Vertical path from right house to main path (start BELOW door at y=10)
  for (let y = 10; y <= 13; y++) {
    setTile(tiles, 29, y, TileType.Path);
  }

  // Path to bridge
  for (let x = 5; x <= 9; x++) {
    setTile(tiles, x, 11, TileType.Path);
    setTile(tiles, x, 10, TileType.Path);
  }

  // South exit path
  for (let y = 13; y < H; y++) {
    setTile(tiles, 19, y, TileType.Path);
    setTile(tiles, 20, y, TileType.Path);
  }

  // --- Fences above houses ---
  for (let x = 8; x <= 25; x++) {
    if (tiles[4][x] === TileType.Grass) {
      setTile(tiles, x, 4, TileType.Fence);
    }
  }

  // Fences along path in village
  for (let x = 24; x <= 35; x++) {
    if (tiles[11][x] === TileType.Grass) {
      setTile(tiles, x, 11, TileType.Fence);
    }
  }

  // --- Decorative trees ---
  const treePosVillage = [
    [8, 6], [8, 7], [15, 6], [15, 7], [15, 8],
    [25, 6], [25, 7], [26, 6], [26, 7],
    [8, 15], [9, 15],
    [3, 5], [4, 5], [3, 6], [4, 6],
    [3, 16], [4, 16], [3, 17], [4, 17],
    [14, 16], [14, 17], [23, 16], [23, 17],
    [33, 5], [34, 5], [33, 6], [34, 6],
    [33, 15], [34, 15], [33, 16], [34, 16],
  ];
  for (const [tx, ty] of treePosVillage) {
    if (tiles[ty]?.[tx] === TileType.Grass) {
      setTile(tiles, tx, ty, TileType.Tree);
    }
  }

  // --- Flowers ---
  const flowerSpots = [
    [10, 15], [11, 15], [12, 15], [13, 15],
    [22, 11], [23, 11],
    [6, 6], [6, 7], [7, 6],
    [14, 14], [15, 14],
    [28, 14], [30, 14], [31, 14],
    [6, 17], [7, 17], [8, 17],
    [16, 16], [17, 16], [18, 16],
  ];
  for (const [fx, fy] of flowerSpots) {
    if (tiles[fy]?.[fx] === TileType.Grass) {
      setTile(tiles, fx, fy, TileType.Flower);
    }
  }

  return {
    id: 'BOURG_NID',
    name: 'BOURG-NID',
    width: W,
    height: H,
    tiles,
    collisionTiles: [TileType.Water, TileType.Tree, TileType.Wall, TileType.Roof, TileType.Fence, TileType.Sign],
    encounterTiles: [],
    encounterRate: 0,
    encounters: [],
    npcs: [
      {
        id: 'bourg_mom',
        name: 'MAMAN',
        x: 10, y: 12,
        spriteType: 'mom',
        dialogue: [
          'Bonjour mon chou ! Le Professeur Paleo te cherchait au labo.',
          'Fais attention dans les hautes herbes !',
          'Reviens quand tu voudras, mais pars quand tu dois.',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'bourg_assistant',
        name: 'ASSISTANT',
        x: 18, y: 12,
        spriteType: 'assistant',
        dialogue: [
          'Le Professeur est a l\'interieur du labo.',
          'Il a de nouveaux dinos a te montrer !',
          'Le Dinodex enregistre chaque espece que tu rencontres.',
        ],
        facing: 'left',
        movement: 'static',
      },
      {
        id: 'bourg_villager1',
        name: 'VILLAGEOIS',
        x: 16, y: 14,
        spriteType: 'villager',
        dialogue: [
          'Bienvenue a Bourg-Nid ! C\'est un village paisible.',
          'La Route 1 au sud mene a Ville-Fougere.',
          'On dit que des dinos rares vivent dans les hautes herbes.',
        ],
        facing: 'down',
        movement: 'wander',
      },
      {
        id: 'bourg_villager2',
        name: 'VILLAGEOISE',
        x: 28, y: 12,
        spriteType: 'villager',
        dialogue: [
          'Mon mari peche souvent pres de l\'etang.',
          'Les dinos d\'eau sont rares par ici, mais on en trouve a Port-Coquille.',
        ],
        facing: 'left',
        movement: 'static',
      },
    ],
    warps: [
      // South exit -> Route 1
      { x: 19, y: H - 1, targetMap: 'ROUTE_1', targetX: 2, targetY: 1 },
      { x: 20, y: H - 1, targetMap: 'ROUTE_1', targetX: 3, targetY: 1 },
    ],
    buildings: [
      // Player's house
      { id: 'bourg_player_house', type: 'house', doorX: 11, doorY: 8, signX: 8, signY: 8, signText: 'Maison de la famille du joueur.' },
      // Professor Paleo's lab — interior handled by starter select event, not a building scene
      { id: 'bourg_lab', type: 'house', doorX: 19, doorY: 8, signX: 16, signY: 8, signText: 'Laboratoire du Professeur Paleo - Recherche en Paleontologie.' },
      // Small house (right side) — no sign tile in the map
      { id: 'bourg_house2', type: 'house', doorX: 29, doorY: 9, signX: -1, signY: -1, signText: '' },
    ],
    music: 'town_theme',
    terrain: 'grass',
  };
}

// ============================================================
// MAP 2: ROUTE_1 — First Route (50x20)
// ============================================================
function createRoute1(): MapData {
  const W = 50, H = 20;
  const tiles = fillGrid(W, H, TileType.Grass);

  // --- Top boundary: trees ---
  setRect(tiles, 0, 0, W, 2, TileType.Tree);

  // --- Bottom boundary: trees ---
  setRect(tiles, 0, H - 2, W, 2, TileType.Tree);

  // --- Left boundary (entry from Bourg-Nid) ---
  for (let y = 0; y < H; y++) {
    if (y < 1 || y > 2) {
      setTile(tiles, 0, y, TileType.Tree);
    }
  }

  // --- Right boundary (exit to Ville-Fougere) ---
  for (let y = 0; y < H; y++) {
    if (y < 9 || y > 10) {
      setTile(tiles, W - 1, y, TileType.Tree);
    }
  }

  // --- Main horizontal path through center ---
  for (let x = 0; x < W; x++) {
    setTile(tiles, x, 9, TileType.Path);
    setTile(tiles, x, 10, TileType.Path);
  }

  // --- Entry path from west (connects to Bourg-Nid south exit) ---
  for (let y = 1; y <= 9; y++) {
    setTile(tiles, 2, y, TileType.Path);
    setTile(tiles, 3, y, TileType.Path);
  }

  // --- Tall grass patch 1 (left area) ---
  for (let y = 4; y <= 7; y++) {
    for (let x = 6; x <= 10; x++) {
      if (tiles[y][x] === TileType.Grass) {
        setTile(tiles, x, y, TileType.TallGrass);
      }
    }
  }

  // --- Tall grass patch 2 (center) ---
  for (let y = 5; y <= 8; y++) {
    for (let x = 20; x <= 24; x++) {
      if (tiles[y][x] === TileType.Grass) {
        setTile(tiles, x, y, TileType.TallGrass);
      }
    }
  }

  // --- Tall grass patch 3 (right area) ---
  for (let y = 12; y <= 15; y++) {
    for (let x = 35; x <= 39; x++) {
      if (tiles[y][x] === TileType.Grass) {
        setTile(tiles, x, y, TileType.TallGrass);
      }
    }
  }

  // --- Small pond at north ---
  for (let y = 3; y <= 5; y++) {
    for (let x = 14; x <= 17; x++) {
      if (tiles[y][x] !== TileType.Tree) {
        setTile(tiles, x, y, TileType.Water);
      }
    }
  }

  // --- Scattered trees ---
  const routeTrees = [
    [5, 4], [5, 5], [12, 3], [12, 4],
    [18, 5], [18, 6], [19, 5],
    [26, 4], [27, 4], [26, 5],
    [30, 12], [31, 12], [30, 13],
    [34, 5], [34, 6], [42, 4], [42, 5],
    [43, 12], [43, 13], [44, 12],
  ];
  for (const [tx, ty] of routeTrees) {
    if (tiles[ty]?.[tx] === TileType.Grass || tiles[ty]?.[tx] === TileType.TallGrass) {
      setTile(tiles, tx, ty, TileType.Tree);
    }
  }

  // --- Flowers ---
  const flowers = [
    [8, 11], [9, 11], [15, 12], [16, 12],
    [25, 11], [33, 8], [34, 8], [40, 11],
  ];
  for (const [fx, fy] of flowers) {
    if (tiles[fy]?.[fx] === TileType.Grass) {
      setTile(tiles, fx, fy, TileType.Flower);
    }
  }

  // --- Signs ---
  setTile(tiles, 4, 3, TileType.Sign); // Near entrance
  setTile(tiles, 45, 8, TileType.Sign); // Near exit

  return {
    id: 'ROUTE_1',
    name: 'ROUTE 1',
    width: W,
    height: H,
    tiles,
    collisionTiles: [TileType.Water, TileType.Tree, TileType.Wall, TileType.Roof, TileType.Fence, TileType.Sign],
    encounterTiles: [TileType.TallGrass],
    encounterRate: 8,
    encounters: [
      { dinoId: 10, name: 'Caillex', minLevel: 2, maxLevel: 4, weight: 30 },
      { dinoId: 13, name: 'Plumex', minLevel: 2, maxLevel: 4, weight: 25 },
      { dinoId: 16, name: 'Toxidon', minLevel: 3, maxLevel: 5, weight: 20 },
      { dinoId: 19, name: 'Fernex', minLevel: 2, maxLevel: 4, weight: 20 },
      { dinoId: 25, name: 'Zephyros', minLevel: 3, maxLevel: 5, weight: 5 },
    ],
    npcs: [
      {
        id: 'rival_rex',
        name: 'REX',
        x: 3, y: 3,
        spriteType: 'rival',
        dialogue: [
          'Ah te voila ! Je t\'attendais !',
        ],
        facing: 'up',
        movement: 'static',
        hideIfFlag: 'rival_1_defeated',
      },
      {
        id: 'route1_trainer1',
        name: 'DRESSEUR MARC',
        x: 15, y: 9,
        spriteType: 'trainer',
        dialogue: [
          'He toi ! Tu es un nouveau dresseur ?',
          'Mon Caillex est imbattable ! Enfin... presque.',
          'Astuce : les dinos de type Terre sont faibles contre l\'Eau.',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'route1_trainer2',
        name: 'DRESSEUSE LINA',
        x: 38, y: 10,
        spriteType: 'trainer',
        dialogue: [
          'Ville-Fougere est juste a l\'est !',
          'N\'oublie pas de soigner tes dinos au Centre Dino.',
          'Le premier gym utilise des dinos de type Plante.',
        ],
        facing: 'left',
        movement: 'static',
      },
    ],
    warps: [
      // West -> Bourg-Nid
      { x: 2, y: 0, targetMap: 'BOURG_NID', targetX: 19, targetY: 28 },
      { x: 3, y: 0, targetMap: 'BOURG_NID', targetX: 20, targetY: 28 },
      // East -> Ville-Fougere
      { x: W - 1, y: 9, targetMap: 'VILLE_FOUGERE', targetX: 1, targetY: 15 },
      { x: W - 1, y: 10, targetMap: 'VILLE_FOUGERE', targetX: 1, targetY: 16 },
    ],
    music: 'route_theme',
    terrain: 'grass',
  };
}

// ============================================================
// MAP 3: VILLE_FOUGERE — First Gym Town (35x30)
// ============================================================
function createVilleFougere(): MapData {
  const W = 35, H = 30;
  const tiles = fillGrid(W, H, TileType.Grass);

  // --- Top boundary: trees (forest theme = extra trees) ---
  setRect(tiles, 0, 0, W, 3, TileType.Tree);

  // --- Bottom boundary: trees ---
  setRect(tiles, 0, H - 2, W, 2, TileType.Tree);

  // --- Left boundary (entry from Route 1) ---
  for (let y = 0; y < H; y++) {
    if (y < 15 || y > 16) {
      setTile(tiles, 0, y, TileType.Tree);
    }
  }

  // --- Right boundary (exit to Route 2) ---
  for (let y = 0; y < H; y++) {
    if (y < 14 || y > 15) {
      setTile(tiles, W - 1, y, TileType.Tree);
      setTile(tiles, W - 2, y, TileType.Tree);
    }
  }

  // --- Main paths ---
  // Horizontal main street
  for (let x = 0; x < W; x++) {
    setTile(tiles, x, 15, TileType.Path);
    setTile(tiles, x, 16, TileType.Path);
  }

  // Vertical path connecting buildings
  for (let y = 6; y <= 16; y++) {
    setTile(tiles, 10, y, TileType.Path);
    setTile(tiles, 11, y, TileType.Path);
  }

  // Path to gym
  for (let y = 16; y <= 23; y++) {
    setTile(tiles, 17, y, TileType.Path);
    setTile(tiles, 18, y, TileType.Path);
  }

  // --- Dino Center (healing) at (4, 5) ---
  buildBuilding(tiles, 4, 5, 5, 3, 6, 7);
  setTile(tiles, 3, 7, TileType.Sign);
  // Path from center to main street
  for (let y = 7; y <= 15; y++) {
    setTile(tiles, 6, y, TileType.Path);
  }

  // --- Boutique (shop) at (14, 5) ---
  buildBuilding(tiles, 14, 5, 4, 3, 16, 7);
  setTile(tiles, 13, 7, TileType.Sign);
  // Path from shop to main street
  for (let y = 7; y <= 15; y++) {
    setTile(tiles, 16, y, TileType.Path);
  }

  // --- Gym (larger, green roof) at (14, 20) ---
  buildBuilding(tiles, 14, 20, 6, 4, 17, 23);
  setTile(tiles, 13, 23, TileType.Sign);

  // --- Houses ---
  // House 1
  buildBuilding(tiles, 24, 5, 4, 3, 26, 7);
  for (let y = 7; y <= 15; y++) {
    setTile(tiles, 26, y, TileType.Path);
  }

  // House 2
  buildBuilding(tiles, 24, 19, 4, 3, 26, 21);
  for (let y = 16; y <= 21; y++) {
    setTile(tiles, 26, y, TileType.Path);
  }

  // --- Forest town theme: extra trees scattered ---
  const extraTrees = [
    [2, 4], [3, 4], [2, 10], [3, 10], [2, 11], [3, 11],
    [2, 19], [3, 19], [2, 20], [3, 20],
    [12, 4], [13, 4], [12, 10], [13, 10],
    [22, 4], [23, 4], [22, 10], [23, 10],
    [22, 19], [23, 19],
    [29, 5], [30, 5], [29, 6], [30, 6],
    [29, 10], [30, 10], [29, 11], [30, 11],
    [29, 19], [30, 19], [29, 20], [30, 20],
    [29, 24], [30, 24], [29, 25], [30, 25],
    [5, 24], [6, 24], [5, 25], [6, 25],
    [10, 24], [11, 24], [10, 25], [11, 25],
  ];
  for (const [tx, ty] of extraTrees) {
    if (tiles[ty]?.[tx] === TileType.Grass) {
      setTile(tiles, tx, ty, TileType.Tree);
    }
  }

  // --- Flowers (forest town = lots of green) ---
  const flowers = [
    [5, 10], [6, 10], [7, 10], [8, 10],
    [15, 10], [16, 10], [17, 10],
    [5, 19], [6, 19], [7, 19],
    [15, 18], [16, 18],
    [25, 11], [26, 11], [27, 11],
    [8, 24], [9, 24],
  ];
  for (const [fx, fy] of flowers) {
    if (tiles[fy]?.[fx] === TileType.Grass) {
      setTile(tiles, fx, fy, TileType.Flower);
    }
  }

  return {
    id: 'VILLE_FOUGERE',
    name: 'VILLE-FOUGERE',
    width: W,
    height: H,
    tiles,
    collisionTiles: [TileType.Water, TileType.Tree, TileType.Wall, TileType.Roof, TileType.Fence, TileType.Sign],
    encounterTiles: [],
    encounterRate: 0,
    encounters: [],
    npcs: [
      {
        id: 'fougere_nurse',
        name: 'INFIRMIERE JOY',
        x: 5, y: 8,
        spriteType: 'nurse',
        dialogue: [
          'Bienvenue au Centre Dino ! Je peux soigner tes dinos.',
          'N\'hesite pas a revenir apres chaque combat difficile.',
          'Tes dinos ont l\'air en pleine forme !',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'fougere_shopkeeper',
        name: 'VENDEUR',
        x: 15, y: 8,
        spriteType: 'shopkeeper',
        dialogue: [
          'Bienvenue a la Boutique ! On a des Jurassic Balls et des Potions.',
          'Les Jurassic Balls permettent de capturer des dinos sauvages.',
          'Economise pour les Super Balls plus tard !',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'fougere_gymguide',
        name: 'GUIDE ARENA',
        x: 16, y: 24,
        spriteType: 'villager',
        dialogue: [
          'L\'arene de Ville-Fougere est de type Plante.',
          'Flora, la championne, utilise Fernling et Thornback.',
          'Les dinos de type Feu ont un avantage ici !',
        ],
        facing: 'up',
        movement: 'static',
      },
      {
        id: 'fougere_villager1',
        name: 'JARDINIER',
        x: 8, y: 15,
        spriteType: 'villager',
        dialogue: [
          'Ville-Fougere tire son nom des fougeres anciennes qui poussent partout.',
          'L\'Escadron Meteore a essaye de voler mes graines rares !',
          'Heureusement, un jeune dresseur courageux les a chasses.',
        ],
        facing: 'right',
        movement: 'wander',
      },
      {
        id: 'fougere_villager2',
        name: 'BOTANISTE',
        x: 20, y: 16,
        spriteType: 'villager',
        dialogue: [
          'Les fougeres de cette ville ont des milliers d\'annees.',
          'Elles coexistent avec les dinos depuis toujours.',
        ],
        facing: 'left',
        movement: 'static',
      },
      {
        id: 'fougere_villager3',
        name: 'VILLAGEOIS',
        x: 25, y: 15,
        spriteType: 'villager',
        dialogue: [
          'La Route 2 a l\'est traverse une foret dense.',
          'On dit qu\'il y a une grotte cachee quelque part.',
        ],
        facing: 'down',
        movement: 'static',
      },
    ],
    warps: [
      // West -> Route 1
      { x: 0, y: 15, targetMap: 'ROUTE_1', targetX: 48, targetY: 9 },
      { x: 0, y: 16, targetMap: 'ROUTE_1', targetX: 48, targetY: 10 },
      // East -> Route 2
      { x: W - 1, y: 14, targetMap: 'ROUTE_2', targetX: 1, targetY: 12 },
      { x: W - 1, y: 15, targetMap: 'ROUTE_2', targetX: 1, targetY: 13 },
    ],
    buildings: [
      { id: 'fougere_dino_center', type: 'dino_center', doorX: 6, doorY: 7, signX: 3, signY: 7, signText: 'Centre Dino — Soins gratuits pour vos dinos.' },
      { id: 'fougere_shop', type: 'shop', doorX: 16, doorY: 7, signX: 13, signY: 7, signText: 'Boutique — Jurassic Balls, Potions, et plus !' },
      { id: 'fougere_gym', type: 'gym', doorX: 17, doorY: 23, signX: 13, signY: 23, signText: 'Arene de Ville-Fougere — Championne : FLORA', sceneData: { gymId: 0 } },
      { id: 'fougere_house1', type: 'house', doorX: 26, doorY: 7, signX: -1, signY: -1, signText: '' },
      { id: 'fougere_house2', type: 'house', doorX: 26, doorY: 21, signX: -1, signY: -1, signText: '' },
    ],
    music: 'town_theme',
    terrain: 'grass',
  };
}

// ============================================================
// MAP 4: ROUTE_2 — Dense Forest Route (40x25)
// ============================================================
function createRoute2(): MapData {
  const W = 40, H = 25;
  const tiles = fillGrid(W, H, TileType.Grass);

  // --- Top boundary ---
  setRect(tiles, 0, 0, W, 2, TileType.Tree);

  // --- Bottom boundary ---
  setRect(tiles, 0, H - 2, W, 2, TileType.Tree);

  // --- Left boundary ---
  for (let y = 0; y < H; y++) {
    if (y < 12 || y > 13) {
      setTile(tiles, 0, y, TileType.Tree);
    }
  }

  // --- Right boundary ---
  for (let y = 0; y < H; y++) {
    if (y < 12 || y > 13) {
      setTile(tiles, W - 1, y, TileType.Tree);
    }
  }

  // --- Main horizontal path ---
  for (let x = 0; x < W; x++) {
    setTile(tiles, x, 12, TileType.Path);
    setTile(tiles, x, 13, TileType.Path);
  }

  // --- Dense forest: more trees scattered ---
  const forestTrees = [
    // Left section
    [2, 4], [3, 4], [2, 5], [3, 5], [4, 3], [5, 3],
    [2, 8], [3, 8], [4, 8], [2, 9], [3, 9],
    [6, 5], [7, 5], [6, 6], [7, 6],
    [2, 16], [3, 16], [2, 17], [3, 17],
    [5, 18], [6, 18], [5, 19], [6, 19],
    // Center
    [12, 4], [13, 4], [12, 5], [13, 5],
    [17, 3], [18, 3], [17, 4],
    [15, 8], [16, 8], [15, 9],
    [12, 16], [13, 16], [12, 17],
    [17, 17], [18, 17], [17, 18],
    // Right section
    [24, 3], [25, 3], [24, 4], [25, 4],
    [28, 5], [29, 5], [28, 6],
    [32, 3], [33, 3], [32, 4], [33, 4],
    [35, 7], [36, 7], [35, 8],
    [24, 17], [25, 17], [24, 18],
    [28, 16], [29, 16], [28, 17],
    [32, 18], [33, 18], [32, 19],
    [35, 16], [36, 16], [35, 17], [36, 17],
  ];
  for (const [tx, ty] of forestTrees) {
    if (tiles[ty]?.[tx] === TileType.Grass) {
      setTile(tiles, tx, ty, TileType.Tree);
    }
  }

  // --- Tall grass patches (more and denser) ---
  // Patch 1
  for (let y = 5; y <= 8; y++) {
    for (let x = 8; x <= 12; x++) {
      if (tiles[y][x] === TileType.Grass) {
        setTile(tiles, x, y, TileType.TallGrass);
      }
    }
  }
  // Patch 2
  for (let y = 6; y <= 10; y++) {
    for (let x = 19; x <= 23; x++) {
      if (tiles[y][x] === TileType.Grass) {
        setTile(tiles, x, y, TileType.TallGrass);
      }
    }
  }
  // Patch 3
  for (let y = 15; y <= 19; y++) {
    for (let x = 8; x <= 11; x++) {
      if (tiles[y][x] === TileType.Grass) {
        setTile(tiles, x, y, TileType.TallGrass);
      }
    }
  }
  // Patch 4
  for (let y = 15; y <= 18; y++) {
    for (let x = 20; x <= 24; x++) {
      if (tiles[y][x] === TileType.Grass) {
        setTile(tiles, x, y, TileType.TallGrass);
      }
    }
  }
  // Patch 5
  for (let y = 6; y <= 9; y++) {
    for (let x = 30; x <= 34; x++) {
      if (tiles[y][x] === TileType.Grass) {
        setTile(tiles, x, y, TileType.TallGrass);
      }
    }
  }

  // --- Cave entrance on north side ---
  // Small building representing cave entrance
  setTile(tiles, 20, 2, TileType.Wall);
  setTile(tiles, 21, 2, TileType.Wall);
  setTile(tiles, 22, 2, TileType.Wall);
  setTile(tiles, 21, 3, TileType.Door);
  // Path to cave
  for (let y = 3; y <= 12; y++) {
    setTile(tiles, 21, y, TileType.Path);
  }
  setTile(tiles, 19, 3, TileType.Sign);

  // --- Flowers ---
  const flowers = [
    [10, 11], [11, 11], [14, 7], [15, 7],
    [26, 11], [27, 11], [30, 15], [31, 15],
  ];
  for (const [fx, fy] of flowers) {
    if (tiles[fy]?.[fx] === TileType.Grass) {
      setTile(tiles, fx, fy, TileType.Flower);
    }
  }

  return {
    id: 'ROUTE_2',
    name: 'ROUTE 2',
    width: W,
    height: H,
    tiles,
    collisionTiles: [TileType.Water, TileType.Tree, TileType.Wall, TileType.Roof, TileType.Fence, TileType.Sign],
    encounterTiles: [TileType.TallGrass],
    encounterRate: 10,
    encounters: [
      { dinoId: 13, name: 'Plumex', minLevel: 4, maxLevel: 6, weight: 20 },
      { dinoId: 16, name: 'Toxidon', minLevel: 4, maxLevel: 7, weight: 20 },
      { dinoId: 19, name: 'Fernex', minLevel: 5, maxLevel: 7, weight: 20 },
      { dinoId: 20, name: 'Fougeraptor', minLevel: 5, maxLevel: 8, weight: 15 },
      { dinoId: 22, name: 'Terravore', minLevel: 5, maxLevel: 7, weight: 15 },
      { dinoId: 25, name: 'Zephyros', minLevel: 4, maxLevel: 6, weight: 10 },
    ],
    npcs: [
      {
        id: 'route2_trainer1',
        name: 'DRESSEUR AXEL',
        x: 10, y: 12,
        spriteType: 'trainer',
        dialogue: [
          'Cette foret est pleine de dinos sauvages !',
          'Mon Toxidon empoisonne tous ses adversaires.',
          'Attention au poison, emporte des Antidotes !',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'route2_trainer2',
        name: 'DRESSEUSE SOPHIE',
        x: 25, y: 13,
        spriteType: 'trainer',
        dialogue: [
          'Tu as vu la grotte au nord ? Elle est mysterieuse.',
          'On dit que des peintures anciennes s\'y trouvent.',
          'Elles representent un dino gigantesque entoure de lumiere...',
        ],
        facing: 'up',
        movement: 'static',
      },
      {
        id: 'route2_trainer3',
        name: 'RANDONNEUR LEO',
        x: 35, y: 12,
        spriteType: 'trainer',
        dialogue: [
          'Port-Coquille est tout pres ! Tu sens l\'air marin ?',
          'Le gym de Port-Coquille utilise des dinos de type Eau.',
          'Prepare-toi bien, Marin est un adversaire redoutable !',
        ],
        facing: 'left',
        movement: 'static',
      },
    ],
    warps: [
      // West -> Ville-Fougere
      { x: 0, y: 12, targetMap: 'VILLE_FOUGERE', targetX: 33, targetY: 14 },
      { x: 0, y: 13, targetMap: 'VILLE_FOUGERE', targetX: 33, targetY: 15 },
      // East -> Port-Coquille
      { x: W - 1, y: 12, targetMap: 'PORT_COQUILLE', targetX: 1, targetY: 14 },
      { x: W - 1, y: 13, targetMap: 'PORT_COQUILLE', targetX: 1, targetY: 15 },
    ],
    music: 'route_theme',
    terrain: 'forest',
  };
}

// ============================================================
// MAP 5: PORT_COQUILLE — Coastal Town (40x30)
// ============================================================
function createPortCoquille(): MapData {
  const W = 40, H = 30;
  const tiles = fillGrid(W, H, TileType.Grass);

  // --- Top boundary: trees ---
  setRect(tiles, 0, 0, W, 2, TileType.Tree);
  // North exit for Route 3
  setTile(tiles, 19, 0, TileType.Path);
  setTile(tiles, 20, 0, TileType.Path);
  setTile(tiles, 19, 1, TileType.Path);
  setTile(tiles, 20, 1, TileType.Path);

  // --- Left boundary ---
  for (let y = 0; y < H; y++) {
    if (y < 14 || y > 15) {
      setTile(tiles, 0, y, TileType.Tree);
    }
  }

  // --- Right boundary: water (coastal) ---
  for (let y = 0; y < H; y++) {
    setTile(tiles, W - 1, y, TileType.Water);
    setTile(tiles, W - 2, y, TileType.Water);
    setTile(tiles, W - 3, y, TileType.Water);
  }

  // --- Bottom: water (ocean) + sand beach ---
  for (let x = 0; x < W; x++) {
    setTile(tiles, x, H - 1, TileType.Water);
    setTile(tiles, x, H - 2, TileType.Water);
    setTile(tiles, x, H - 3, TileType.Water);
    if (tiles[H - 4][x] !== TileType.Water) {
      setTile(tiles, x, H - 4, TileType.Sand);
    }
    if (tiles[H - 5][x] !== TileType.Water) {
      setTile(tiles, x, H - 5, TileType.Sand);
    }
  }

  // --- Main paths ---
  // Horizontal main street
  for (let x = 0; x < W - 3; x++) {
    setTile(tiles, x, 14, TileType.Path);
    setTile(tiles, x, 15, TileType.Path);
  }

  // North exit path
  for (let y = 1; y <= 14; y++) {
    setTile(tiles, 19, y, TileType.Path);
    setTile(tiles, 20, y, TileType.Path);
  }

  // Path to beach
  for (let y = 15; y <= H - 5; y++) {
    setTile(tiles, 10, y, TileType.Path);
    setTile(tiles, 11, y, TileType.Path);
  }

  // --- Dino Center at (4, 5) ---
  buildBuilding(tiles, 4, 5, 5, 3, 6, 7);
  setTile(tiles, 3, 7, TileType.Sign);
  for (let y = 7; y <= 14; y++) {
    setTile(tiles, 6, y, TileType.Path);
  }

  // --- Boutique at (12, 5) ---
  buildBuilding(tiles, 12, 5, 4, 3, 14, 7);
  setTile(tiles, 11, 7, TileType.Sign);
  for (let y = 7; y <= 14; y++) {
    setTile(tiles, 14, y, TileType.Path);
  }

  // --- Gym (blue-themed, near water) at (25, 5) ---
  buildBuilding(tiles, 25, 5, 6, 4, 28, 8);
  setTile(tiles, 24, 8, TileType.Sign);
  for (let y = 8; y <= 14; y++) {
    setTile(tiles, 28, y, TileType.Path);
  }

  // --- Harbor/docks (bridge tiles over water) ---
  // Dock extending into water from sand
  for (let y = H - 5; y < H; y++) {
    setTile(tiles, 30, y, TileType.Bridge);
    setTile(tiles, 31, y, TileType.Bridge);
  }
  // Horizontal dock
  for (let x = 28; x <= 33; x++) {
    setTile(tiles, x, H - 4, TileType.Bridge);
  }

  // --- Houses ---
  buildBuilding(tiles, 4, 17, 4, 3, 6, 19);
  for (let y = 15; y <= 19; y++) {
    setTile(tiles, 6, y, TileType.Path);
  }

  buildBuilding(tiles, 25, 17, 4, 3, 27, 19);
  for (let y = 15; y <= 19; y++) {
    setTile(tiles, 27, y, TileType.Path);
  }

  // --- Trees ---
  const townTrees = [
    [2, 4], [3, 4], [2, 10], [3, 10], [2, 11], [3, 11],
    [10, 4], [11, 4], [10, 10], [11, 10],
    [22, 4], [23, 4], [22, 10], [23, 10],
    [33, 4], [34, 4], [33, 10], [34, 10],
    [2, 22], [3, 22], [15, 22], [16, 22],
    [22, 22], [23, 22],
  ];
  for (const [tx, ty] of townTrees) {
    if (tiles[ty]?.[tx] === TileType.Grass) {
      setTile(tiles, tx, ty, TileType.Tree);
    }
  }

  // --- Flowers ---
  const flowers = [
    [8, 10], [9, 10], [15, 10], [16, 10],
    [8, 22], [9, 22], [13, 17], [14, 17],
  ];
  for (const [fx, fy] of flowers) {
    if (tiles[fy]?.[fx] === TileType.Grass) {
      setTile(tiles, fx, fy, TileType.Flower);
    }
  }

  return {
    id: 'PORT_COQUILLE',
    name: 'PORT-COQUILLE',
    width: W,
    height: H,
    tiles,
    collisionTiles: [TileType.Water, TileType.Tree, TileType.Wall, TileType.Roof, TileType.Fence, TileType.Sign],
    encounterTiles: [],
    encounterRate: 0,
    encounters: [],
    npcs: [
      {
        id: 'port_nurse',
        name: 'INFIRMIERE',
        x: 5, y: 8,
        spriteType: 'nurse',
        dialogue: [
          'Bienvenue au Centre Dino de Port-Coquille !',
          'L\'air marin est bon pour les dinos. Les tiens ont l\'air en forme.',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'port_shopkeeper',
        name: 'VENDEUR',
        x: 13, y: 8,
        spriteType: 'shopkeeper',
        dialogue: [
          'Port-Coquille est celebre pour ses perles de dino !',
          'J\'ai des Super Balls en stock. Elles sont plus efficaces que les Jurassic Balls.',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'port_fisher',
        name: 'PECHEUR REMY',
        x: 29, y: H - 5,
        spriteType: 'villager',
        dialogue: [
          'La peche est bonne aujourd\'hui... enfin, presque.',
          'L\'Escadron Meteore a bloque l\'acces a la Grotte Sous-Marine.',
          'Ils cherchent quelque chose dans les profondeurs...',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'port_gymguide',
        name: 'GUIDE ARENA',
        x: 27, y: 9,
        spriteType: 'villager',
        dialogue: [
          'Le gym de Port-Coquille est de type Eau.',
          'Marin est un ancien capitaine. Il ne fait pas de cadeaux !',
          'Les dinos Plante ou Electrique sont tes meilleurs allies ici.',
        ],
        facing: 'left',
        movement: 'static',
      },
      {
        id: 'port_sailor',
        name: 'MARIN JEAN',
        x: 32, y: H - 5,
        spriteType: 'villager',
        dialogue: [
          'Les bateaux ne partent plus vers le sud depuis les tremblements.',
          'La mer est agitee ces derniers temps. Les dinos marins sont nerveux.',
        ],
        facing: 'left',
        movement: 'static',
      },
      {
        id: 'port_villager1',
        name: 'VILLAGEOISE',
        x: 15, y: 15,
        spriteType: 'villager',
        dialogue: [
          'Port-Coquille etait un petit village de pecheurs autrefois.',
          'Maintenant c\'est une ville animee grace au commerce de dinos.',
        ],
        facing: 'right',
        movement: 'wander',
      },
      {
        id: 'port_villager2',
        name: 'VILLAGEOIS',
        x: 8, y: 14,
        spriteType: 'villager',
        dialogue: [
          'La Route 3 au nord mene a Roche-Haute a travers les collines.',
          'Prepare-toi, le chemin est escarpe et les dinos sauvages sont costauds.',
        ],
        facing: 'down',
        movement: 'static',
      },
    ],
    warps: [
      // West -> Route 2
      { x: 0, y: 14, targetMap: 'ROUTE_2', targetX: 38, targetY: 12 },
      { x: 0, y: 15, targetMap: 'ROUTE_2', targetX: 38, targetY: 13 },
      // North -> Route 3
      { x: 19, y: 0, targetMap: 'ROUTE_3', targetX: 14, targetY: 38 },
      { x: 20, y: 0, targetMap: 'ROUTE_3', targetX: 15, targetY: 38 },
    ],
    buildings: [
      { id: 'port_dino_center', type: 'dino_center', doorX: 6, doorY: 7, signX: 3, signY: 7, signText: 'CENTRE DINO de Port-Coquille — Soins gratuits pour vos dinos' },
      { id: 'port_shop', type: 'shop', doorX: 14, doorY: 7, signX: 11, signY: 7, signText: 'BOUTIQUE Port-Coquille — Potions, Balls et plus !' },
      { id: 'port_gym', type: 'gym', doorX: 28, doorY: 8, signX: 24, signY: 8, signText: 'ARENE de Port-Coquille — Champion : MARIN — Type : Eau', sceneData: { gymId: 1 } },
      { id: 'port_house1', type: 'house', doorX: 6, doorY: 19, signX: -1, signY: -1, signText: '' },
      { id: 'port_house2', type: 'house', doorX: 27, doorY: 19, signX: -1, signY: -1, signText: '' },
    ],
    music: 'port_theme',
    terrain: 'sand',
  };
}

// ============================================================
// MAP 6: ROUTE_3 — Vertical Route (30x40)
// ============================================================
function createRoute3(): MapData {
  const W = 30, H = 40;
  const tiles = fillGrid(W, H, TileType.Grass);

  // --- Left boundary: trees ---
  for (let y = 0; y < H; y++) {
    setTile(tiles, 0, y, TileType.Tree);
    setTile(tiles, 1, y, TileType.Tree);
  }

  // --- Right boundary: trees ---
  for (let y = 0; y < H; y++) {
    setTile(tiles, W - 1, y, TileType.Tree);
    setTile(tiles, W - 2, y, TileType.Tree);
  }

  // --- Top boundary (exit to Roche-Haute) ---
  for (let x = 0; x < W; x++) {
    if (x < 14 || x > 15) {
      setTile(tiles, x, 0, TileType.Tree);
      setTile(tiles, x, 1, TileType.Tree);
    }
  }

  // --- Bottom boundary (entry from Port-Coquille) ---
  for (let x = 0; x < W; x++) {
    if (x < 14 || x > 15) {
      setTile(tiles, x, H - 1, TileType.Tree);
    }
  }

  // --- Main vertical path ---
  for (let y = 0; y < H; y++) {
    setTile(tiles, 14, y, TileType.Path);
    setTile(tiles, 15, y, TileType.Path);
  }

  // --- Rocky terrain: scattered trees and formations ---
  const rockyTrees = [
    // Left side clusters
    [3, 3], [4, 3], [3, 4], [4, 4],
    [3, 10], [4, 10], [3, 11], [4, 11],
    [5, 15], [6, 15], [5, 16],
    [3, 20], [4, 20], [3, 21], [4, 21],
    [5, 28], [6, 28], [5, 29], [6, 29],
    [3, 33], [4, 33], [3, 34], [4, 34],
    // Right side clusters
    [24, 5], [25, 5], [24, 6], [25, 6],
    [23, 12], [24, 12], [23, 13], [24, 13],
    [22, 18], [23, 18], [22, 19],
    [24, 25], [25, 25], [24, 26], [25, 26],
    [23, 32], [24, 32], [23, 33], [24, 33],
    [22, 37], [23, 37],
  ];
  for (const [tx, ty] of rockyTrees) {
    if (tiles[ty]?.[tx] === TileType.Grass) {
      setTile(tiles, tx, ty, TileType.Tree);
    }
  }

  // --- Tall grass patches ---
  // Patch 1 (top-left)
  for (let y = 5; y <= 8; y++) {
    for (let x = 5; x <= 9; x++) {
      if (tiles[y][x] === TileType.Grass) {
        setTile(tiles, x, y, TileType.TallGrass);
      }
    }
  }
  // Patch 2 (right side)
  for (let y = 14; y <= 17; y++) {
    for (let x = 18; x <= 22; x++) {
      if (tiles[y][x] === TileType.Grass) {
        setTile(tiles, x, y, TileType.TallGrass);
      }
    }
  }
  // Patch 3 (left-center)
  for (let y = 23; y <= 27; y++) {
    for (let x = 6; x <= 10; x++) {
      if (tiles[y][x] === TileType.Grass) {
        setTile(tiles, x, y, TileType.TallGrass);
      }
    }
  }
  // Patch 4 (right-bottom)
  for (let y = 30; y <= 34; y++) {
    for (let x = 18; x <= 22; x++) {
      if (tiles[y][x] === TileType.Grass) {
        setTile(tiles, x, y, TileType.TallGrass);
      }
    }
  }

  // --- Ledges (one-way jumps going south) ---
  for (let x = 5; x <= 12; x++) {
    setTile(tiles, x, 20, TileType.Ledge);
  }
  for (let x = 17; x <= 24; x++) {
    setTile(tiles, x, 28, TileType.Ledge);
  }

  // --- Flowers ---
  const flowers = [
    [10, 12], [11, 12], [8, 22], [9, 22],
    [18, 8], [19, 8], [20, 22], [21, 22],
  ];
  for (const [fx, fy] of flowers) {
    if (tiles[fy]?.[fx] === TileType.Grass) {
      setTile(tiles, fx, fy, TileType.Flower);
    }
  }

  // Signs
  setTile(tiles, 13, 2, TileType.Sign);
  setTile(tiles, 13, 37, TileType.Sign);

  // --- Relay Center (rest stop) ---
  buildBuilding(tiles, 19, 21, 3, 2, 20, 22);
  setTile(tiles, 21, 22, TileType.Sign);
  // Path from relay to main path
  for (let x = 15; x <= 20; x++) {
    setTile(tiles, x, 22, TileType.Path);
  }

  return {
    id: 'ROUTE_3',
    name: 'ROUTE 3',
    width: W,
    height: H,
    tiles,
    collisionTiles: [TileType.Water, TileType.Tree, TileType.Wall, TileType.Roof, TileType.Fence, TileType.Sign],
    encounterTiles: [TileType.TallGrass],
    encounterRate: 10,
    encounters: [
      { dinoId: 19, name: 'Fernex', minLevel: 7, maxLevel: 9, weight: 15 },
      { dinoId: 20, name: 'Fougeraptor', minLevel: 8, maxLevel: 10, weight: 15 },
      { dinoId: 22, name: 'Terravore', minLevel: 7, maxLevel: 10, weight: 20 },
      { dinoId: 23, name: 'Seismops', minLevel: 8, maxLevel: 11, weight: 15 },
      { dinoId: 25, name: 'Zephyros', minLevel: 7, maxLevel: 9, weight: 15 },
      { dinoId: 28, name: 'Ignitops', minLevel: 8, maxLevel: 11, weight: 10 },
      { dinoId: 10, name: 'Caillex', minLevel: 7, maxLevel: 9, weight: 10 },
    ],
    npcs: [
      {
        id: 'route3_trainer1',
        name: 'RANDONNEUR PAUL',
        x: 14, y: 10,
        spriteType: 'trainer',
        dialogue: [
          'Ce sentier est escarpe, mais la vue en vaut la peine !',
          'Mon Terravore adore les combats en altitude.',
          'Attention aux rebords, tu ne peux sauter que vers le bas !',
        ],
        facing: 'right',
        movement: 'static',
      },
      {
        id: 'route3_trainer2',
        name: 'DRESSEUSE CLAIRE',
        x: 15, y: 30,
        spriteType: 'trainer',
        dialogue: [
          'Roche-Haute est la-haut ! Encore un effort !',
          'La championne Petra utilise des dinos de type Roche.',
          'Les dinos Eau et Plante sont efficaces contre la Roche.',
        ],
        facing: 'left',
        movement: 'static',
      },
    ],
    warps: [
      // South -> Port-Coquille
      { x: 14, y: H - 1, targetMap: 'PORT_COQUILLE', targetX: 19, targetY: 1 },
      { x: 15, y: H - 1, targetMap: 'PORT_COQUILLE', targetX: 20, targetY: 1 },
      // North -> Roche-Haute
      { x: 14, y: 0, targetMap: 'ROCHE_HAUTE', targetX: 17, targetY: 28 },
      { x: 15, y: 0, targetMap: 'ROCHE_HAUTE', targetX: 18, targetY: 28 },
    ],
    buildings: [
      { id: 'route3_relay', type: 'dino_center', doorX: 20, doorY: 22, signX: 21, signY: 22, signText: 'RELAIS ROUTE 3 — Centre Dino de passage' },
    ],
    music: 'route_theme',
    terrain: 'rock',
  };
}

// ============================================================
// MAP 7: ROCHE_HAUTE — Mountain/Mining Town (35x30)
// ============================================================
function createRocheHaute(): MapData {
  const W = 35, H = 30;
  const tiles = fillGrid(W, H, TileType.Grass);

  // --- Top boundary: dense trees/mountain wall ---
  setRect(tiles, 0, 0, W, 3, TileType.Tree);

  // --- Left boundary ---
  for (let y = 0; y < H; y++) {
    setTile(tiles, 0, y, TileType.Tree);
    setTile(tiles, 1, y, TileType.Tree);
  }

  // --- Right boundary (exit to Route 4) ---
  for (let y = 0; y < H; y++) {
    if (y < 14 || y > 15) {
      setTile(tiles, W - 1, y, TileType.Tree);
      setTile(tiles, W - 2, y, TileType.Tree);
    }
  }

  // --- Bottom boundary (entry from Route 3) ---
  for (let x = 0; x < W; x++) {
    if (x < 17 || x > 18) {
      setTile(tiles, x, H - 1, TileType.Tree);
    }
  }
  setRect(tiles, 0, H - 1, 2, 1, TileType.Tree);

  // --- Main paths ---
  // Horizontal main street
  for (let x = 2; x < W - 2; x++) {
    setTile(tiles, x, 14, TileType.Path);
    setTile(tiles, x, 15, TileType.Path);
  }

  // South entry path
  for (let y = 15; y < H; y++) {
    setTile(tiles, 17, y, TileType.Path);
    setTile(tiles, 18, y, TileType.Path);
  }

  // East exit path
  for (let x = W - 4; x < W; x++) {
    setTile(tiles, x, 14, TileType.Path);
    setTile(tiles, x, 15, TileType.Path);
  }

  // --- Dino Center at (4, 5) ---
  buildBuilding(tiles, 4, 5, 5, 3, 6, 7);
  setTile(tiles, 3, 7, TileType.Sign);
  for (let y = 7; y <= 14; y++) {
    setTile(tiles, 6, y, TileType.Path);
  }

  // --- Boutique at (12, 5) ---
  buildBuilding(tiles, 12, 5, 4, 3, 14, 7);
  setTile(tiles, 11, 7, TileType.Sign);
  for (let y = 7; y <= 14; y++) {
    setTile(tiles, 14, y, TileType.Path);
  }

  // --- Gym (brown/rocky roof) at (22, 5) ---
  buildBuilding(tiles, 22, 5, 6, 4, 25, 8);
  setTile(tiles, 21, 8, TileType.Sign);
  for (let y = 8; y <= 14; y++) {
    setTile(tiles, 25, y, TileType.Path);
  }

  // --- Mine entrance building (north side) ---
  buildBuilding(tiles, 15, 3, 4, 2, 17, 4);
  setTile(tiles, 14, 4, TileType.Sign);
  for (let y = 4; y <= 14; y++) {
    setTile(tiles, 17, y, TileType.Path);
  }

  // --- Houses ---
  buildBuilding(tiles, 4, 18, 4, 3, 6, 20);
  for (let y = 15; y <= 20; y++) {
    setTile(tiles, 6, y, TileType.Path);
  }

  buildBuilding(tiles, 12, 18, 4, 3, 14, 20);
  for (let y = 15; y <= 20; y++) {
    setTile(tiles, 14, y, TileType.Path);
  }

  buildBuilding(tiles, 24, 18, 4, 3, 26, 20);
  for (let y = 15; y <= 20; y++) {
    setTile(tiles, 26, y, TileType.Path);
  }

  // --- Rocky terrain: less vegetation, more stone-like ---
  // Fewer trees, use fences to represent rocky outcrops
  const rockyFeatures = [
    [2, 10], [3, 10], [2, 11], [3, 11],
    [10, 10], [11, 10],
    [20, 10], [21, 10],
    [30, 4], [31, 4], [30, 5], [31, 5],
    [30, 10], [31, 10], [30, 11], [31, 11],
    [2, 22], [3, 22], [2, 23], [3, 23],
    [10, 24], [11, 24],
    [20, 24], [21, 24],
    [30, 22], [31, 22], [30, 23], [31, 23],
  ];
  for (const [tx, ty] of rockyFeatures) {
    if (tiles[ty]?.[tx] === TileType.Grass) {
      setTile(tiles, tx, ty, TileType.Tree);
    }
  }

  // --- Fences around mining areas ---
  for (let x = 4; x <= 10; x++) {
    setTile(tiles, x, 4, TileType.Fence);
  }

  // --- Flowers (sparse in mountain town) ---
  const flowers = [
    [8, 12], [9, 12], [16, 12],
    [8, 22], [9, 22],
  ];
  for (const [fx, fy] of flowers) {
    if (tiles[fy]?.[fx] === TileType.Grass) {
      setTile(tiles, fx, fy, TileType.Flower);
    }
  }

  return {
    id: 'ROCHE_HAUTE',
    name: 'ROCHE-HAUTE',
    width: W,
    height: H,
    tiles,
    collisionTiles: [TileType.Water, TileType.Tree, TileType.Wall, TileType.Roof, TileType.Fence, TileType.Sign],
    encounterTiles: [],
    encounterRate: 0,
    encounters: [],
    npcs: [
      {
        id: 'roche_nurse',
        name: 'INFIRMIERE',
        x: 5, y: 8,
        spriteType: 'nurse',
        dialogue: [
          'Bienvenue au Centre Dino de Roche-Haute !',
          'Les mineurs amenent souvent des dinos blesses. Je suis habituee.',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'roche_shopkeeper',
        name: 'VENDEUR',
        x: 13, y: 8,
        spriteType: 'shopkeeper',
        dialogue: [
          'On vend des Hyper Potions ici. Tu en auras besoin dans la mine.',
          'Les dinos de type Roche sont resistants. Prepare-toi bien !',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'roche_miner',
        name: 'MINEUR GASTON',
        x: 16, y: 5,
        spriteType: 'villager',
        dialogue: [
          'La mine au nord est interdite d\'acces depuis que l\'Escadron Meteore rode.',
          'Avant, c\'etait calme ici. Depuis qu\'ils sont la, la montagne gronde.',
          'Ils cherchent quelque chose... la Pierre d\'Extinction, qu\'ils disent.',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'roche_gymguide',
        name: 'GUIDE ARENA',
        x: 24, y: 9,
        spriteType: 'villager',
        dialogue: [
          'L\'arene de Roche-Haute est de type Roche.',
          'Petra, la championne, est aussi patiente que la pierre.',
          'Les dinos Eau, Plante ou Combat sont recommandes !',
        ],
        facing: 'left',
        movement: 'static',
      },
      {
        id: 'roche_villager1',
        name: 'VILLAGEOIS',
        x: 10, y: 15,
        spriteType: 'villager',
        dialogue: [
          'Roche-Haute est construite a flanc de montagne.',
          'On extrait des fossiles rares dans les mines.',
        ],
        facing: 'right',
        movement: 'wander',
      },
      {
        id: 'roche_villager2',
        name: 'GEOLOGUE',
        x: 20, y: 14,
        spriteType: 'villager',
        dialogue: [
          'Les tremblements de terre sont de plus en plus frequents.',
          'Je crains que l\'Escadron Meteore ne reveille quelque chose sous la montagne.',
        ],
        facing: 'left',
        movement: 'static',
      },
      {
        id: 'roche_villager3',
        name: 'VILLAGEOISE',
        x: 7, y: 20,
        spriteType: 'villager',
        dialogue: [
          'La Route 4 a l\'est mene vers le Desert de Cendres.',
          'C\'est un endroit dangereux. Assure-toi d\'etre bien prepare.',
        ],
        facing: 'right',
        movement: 'static',
      },
    ],
    warps: [
      // South -> Route 3
      { x: 17, y: H - 1, targetMap: 'ROUTE_3', targetX: 14, targetY: 1 },
      { x: 18, y: H - 1, targetMap: 'ROUTE_3', targetX: 15, targetY: 1 },
      // East -> Route 4
      { x: W - 1, y: 14, targetMap: 'ROUTE_4', targetX: 1, targetY: 9 },
      { x: W - 1, y: 15, targetMap: 'ROUTE_4', targetX: 1, targetY: 10 },
    ],
    buildings: [
      { id: 'roche_dino_center', type: 'dino_center', doorX: 6, doorY: 7, signX: 3, signY: 7, signText: 'CENTRE DINO de Roche-Haute — Soins gratuits pour vos dinos' },
      { id: 'roche_shop', type: 'shop', doorX: 14, doorY: 7, signX: 11, signY: 7, signText: 'BOUTIQUE Roche-Haute — Potions, Balls et plus !' },
      { id: 'roche_gym', type: 'gym', doorX: 25, doorY: 8, signX: 21, signY: 8, signText: 'ARENE de Roche-Haute — Championne : PETRA — Type : Roche', sceneData: { gymId: 2 } },
      { id: 'roche_mine', type: 'house', doorX: 17, doorY: 4, signX: 14, signY: 4, signText: 'MINE DE ROCHE-HAUTE — Acces interdit sans autorisation' },
      { id: 'roche_house1', type: 'house', doorX: 6, doorY: 20, signX: -1, signY: -1, signText: '' },
      { id: 'roche_house2', type: 'house', doorX: 14, doorY: 20, signX: -1, signY: -1, signText: '' },
      { id: 'roche_house3', type: 'house', doorX: 26, doorY: 20, signX: -1, signY: -1, signText: '' },
    ],
    music: 'mountain_theme',
    terrain: 'rock',
  };
}

// ============================================================
// MAP 8: ROUTE_4 — Rocky Desert Route (40x20)
// ============================================================
function createRoute4(): MapData {
  const W = 40, H = 20;
  const tiles = fillGrid(W, H, TileType.Sand);

  // --- Top boundary ---
  setRect(tiles, 0, 0, W, 2, TileType.Tree);

  // --- Bottom boundary ---
  setRect(tiles, 0, H - 2, W, 2, TileType.Tree);

  // --- Left boundary (entry from Roche-Haute) ---
  for (let y = 0; y < H; y++) {
    if (y < 9 || y > 10) {
      setTile(tiles, 0, y, TileType.Tree);
    }
  }

  // --- Right boundary (exit to Volcanville) ---
  for (let y = 0; y < H; y++) {
    if (y < 9 || y > 10) {
      setTile(tiles, W - 1, y, TileType.Tree);
    }
  }

  // --- Main horizontal path ---
  for (let x = 0; x < W; x++) {
    setTile(tiles, x, 9, TileType.Path);
    setTile(tiles, x, 10, TileType.Path);
  }

  // --- Lava pools ---
  setRect(tiles, 7, 4, 3, 2, TileType.Lava);
  setRect(tiles, 25, 13, 2, 2, TileType.Lava);
  setRect(tiles, 33, 5, 2, 3, TileType.Lava);

  // --- Tall grass patches (surviving vegetation) ---
  for (let y = 4; y <= 7; y++) {
    for (let x = 12; x <= 16; x++) {
      if (tiles[y][x] === TileType.Sand) setTile(tiles, x, y, TileType.TallGrass);
    }
  }
  for (let y = 12; y <= 15; y++) {
    for (let x = 18; x <= 22; x++) {
      if (tiles[y][x] === TileType.Sand) setTile(tiles, x, y, TileType.TallGrass);
    }
  }
  for (let y = 5; y <= 8; y++) {
    for (let x = 28; x <= 31; x++) {
      if (tiles[y][x] === TileType.Sand) setTile(tiles, x, y, TileType.TallGrass);
    }
  }

  // --- Rocky outcrops (trees representing boulders) ---
  const rocks = [
    [3, 4], [4, 4], [3, 5],
    [11, 3], [11, 4],
    [20, 3], [21, 3],
    [24, 5], [24, 6],
    [36, 4], [37, 4], [36, 5],
    [5, 14], [6, 14],
    [15, 15], [16, 15],
    [30, 14], [31, 14],
  ];
  for (const [tx, ty] of rocks) {
    if (tiles[ty]?.[tx] !== TileType.Path && tiles[ty]?.[tx] !== TileType.Lava) {
      setTile(tiles, tx, ty, TileType.Tree);
    }
  }

  // --- Signs ---
  setTile(tiles, 2, 8, TileType.Sign);
  setTile(tiles, 37, 8, TileType.Sign);

  return {
    id: 'ROUTE_4',
    name: 'ROUTE 4',
    width: W,
    height: H,
    tiles,
    collisionTiles: [TileType.Water, TileType.Tree, TileType.Wall, TileType.Roof, TileType.Fence, TileType.Sign, TileType.Lava],
    encounterTiles: [TileType.TallGrass],
    encounterRate: 10,
    encounters: [
      { dinoId: 28, name: 'Ignitops', minLevel: 10, maxLevel: 13, weight: 25 },
      { dinoId: 29, name: 'Pyrovore', minLevel: 11, maxLevel: 14, weight: 20 },
      { dinoId: 30, name: 'Magmaclaw', minLevel: 12, maxLevel: 15, weight: 15 },
      { dinoId: 32, name: 'Rockjaw', minLevel: 10, maxLevel: 13, weight: 20 },
      { dinoId: 35, name: 'Ashwing', minLevel: 11, maxLevel: 14, weight: 10 },
      { dinoId: 38, name: 'Embersaur', minLevel: 12, maxLevel: 15, weight: 10 },
    ],
    npcs: [
      {
        id: 'route4_trainer1',
        name: 'DRESSEUR HUGO',
        x: 10, y: 9,
        spriteType: 'trainer',
        dialogue: [
          'Ce desert brule sous le soleil et la lave !',
          'Mon Ignitops adore la chaleur.',
          'Les dinos de type Eau sont rares ici, mais devastateurs.',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'route4_trainer2',
        name: 'DRESSEUSE NADIA',
        x: 22, y: 10,
        spriteType: 'trainer',
        dialogue: [
          'Volcanville est juste a l\'est. Tu sentiras la chaleur du volcan.',
          'Le gym de Volcanville est de type Feu. Prepare-toi !',
        ],
        facing: 'left',
        movement: 'static',
      },
      {
        id: 'route4_trainer3',
        name: 'RANDONNEUR FELIX',
        x: 35, y: 10,
        spriteType: 'trainer',
        dialogue: [
          'J\'ai failli marcher dans la lave ! Fais attention.',
          'Les dinos Feu sont dans leur element ici.',
          'Tu devrais capturer un Magmaclaw, ils sont puissants.',
        ],
        facing: 'left',
        movement: 'static',
      },
    ],
    warps: [
      // West -> Roche-Haute
      { x: 0, y: 9, targetMap: 'ROCHE_HAUTE', targetX: 33, targetY: 14 },
      { x: 0, y: 10, targetMap: 'ROCHE_HAUTE', targetX: 33, targetY: 15 },
      // East -> Volcanville
      { x: W - 1, y: 9, targetMap: 'VOLCANVILLE', targetX: 1, targetY: 12 },
      { x: W - 1, y: 10, targetMap: 'VOLCANVILLE', targetX: 1, targetY: 13 },
    ],
    music: 'route_theme',
    terrain: 'rock',
  };
}

// ============================================================
// MAP 9: VOLCANVILLE — Gym 4 Fire Town (35x25)
// ============================================================
function createVolcanville(): MapData {
  const W = 35, H = 25;
  const tiles = fillGrid(W, H, TileType.Sand);

  // --- Top boundary (exit to Route 5) ---
  for (let x = 0; x < W; x++) {
    if (x < 16 || x > 17) {
      setTile(tiles, x, 0, TileType.Tree);
      setTile(tiles, x, 1, TileType.Tree);
    }
  }

  // --- Left boundary (entry from Route 4) ---
  for (let y = 0; y < H; y++) {
    if (y < 12 || y > 13) {
      setTile(tiles, 0, y, TileType.Tree);
    }
  }

  // --- Right boundary ---
  for (let y = 0; y < H; y++) {
    setTile(tiles, W - 1, y, TileType.Tree);
    setTile(tiles, W - 2, y, TileType.Tree);
  }

  // --- Bottom boundary ---
  setRect(tiles, 0, H - 2, W, 2, TileType.Tree);

  // --- Main paths ---
  for (let x = 0; x < W - 2; x++) {
    setTile(tiles, x, 12, TileType.Path);
    setTile(tiles, x, 13, TileType.Path);
  }
  // North exit path
  for (let y = 0; y <= 12; y++) {
    setTile(tiles, 16, y, TileType.Path);
    setTile(tiles, 17, y, TileType.Path);
  }

  // --- Dino Center at (3, 4) ---
  buildBuilding(tiles, 3, 4, 5, 3, 5, 6);
  setTile(tiles, 2, 6, TileType.Sign);
  for (let y = 6; y <= 12; y++) setTile(tiles, 5, y, TileType.Path);

  // --- Boutique at (10, 4) ---
  buildBuilding(tiles, 10, 4, 4, 3, 12, 6);
  setTile(tiles, 9, 6, TileType.Sign);
  for (let y = 6; y <= 12; y++) setTile(tiles, 12, y, TileType.Path);

  // --- Gym (dark red/orange) at (22, 4) ---
  buildBuilding(tiles, 22, 4, 6, 4, 25, 7);
  setTile(tiles, 21, 7, TileType.Sign);
  for (let y = 7; y <= 12; y++) setTile(tiles, 25, y, TileType.Path);

  // --- Forge/Blacksmith building at (10, 16) ---
  buildBuilding(tiles, 10, 16, 5, 3, 12, 18);
  setTile(tiles, 9, 18, TileType.Sign);
  for (let y = 13; y <= 18; y++) setTile(tiles, 12, y, TileType.Path);

  // --- Lava streams decoration ---
  for (let y = 3; y <= 8; y++) setTile(tiles, 20, y, TileType.Lava);
  setRect(tiles, 28, 8, 2, 3, TileType.Lava);
  setRect(tiles, 4, 15, 2, 2, TileType.Lava);

  // --- Rocky decorations ---
  const rocks = [
    [2, 10], [3, 10], [8, 10], [9, 10],
    [2, 16], [3, 16],
    [28, 4], [29, 4], [30, 4],
    [22, 16], [23, 16], [24, 16],
    [28, 16], [29, 16],
  ];
  for (const [tx, ty] of rocks) {
    if (tiles[ty]?.[tx] === TileType.Sand) setTile(tiles, tx, ty, TileType.Tree);
  }

  return {
    id: 'VOLCANVILLE',
    name: 'VOLCANVILLE',
    width: W,
    height: H,
    tiles,
    collisionTiles: [TileType.Water, TileType.Tree, TileType.Wall, TileType.Roof, TileType.Fence, TileType.Sign, TileType.Lava],
    encounterTiles: [],
    encounterRate: 0,
    encounters: [],
    npcs: [
      {
        id: 'volcan_nurse',
        name: 'INFIRMIERE',
        x: 4, y: 7,
        spriteType: 'nurse',
        dialogue: [
          'Bienvenue au Centre Dino de Volcanville !',
          'La chaleur du volcan rend les dinos Feu plus agressifs.',
          'Tes dinos sont soignes et prets a combattre !',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'volcan_shopkeeper',
        name: 'VENDEUR',
        x: 11, y: 7,
        spriteType: 'shopkeeper',
        dialogue: [
          'On a des Anti-Brulures en stock. Tres utile dans le coin !',
          'Les Super Balls sont en promotion cette semaine.',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'volcan_gymguide',
        name: 'GUIDE ARENA',
        x: 24, y: 8,
        spriteType: 'villager',
        dialogue: [
          'L\'arene de Volcanville est de type Feu.',
          'Brazier, le champion, ne connait que les flammes.',
          'Les dinos de type Eau ou Roche sont essentiels ici !',
        ],
        facing: 'left',
        movement: 'static',
      },
      {
        id: 'volcan_blacksmith',
        name: 'FORGERON VULCAIN',
        x: 11, y: 19,
        spriteType: 'villager',
        dialogue: [
          'Ma forge est alimentee par la lave du volcan.',
          'Je fabrique les meilleures armures pour dinos du continent.',
          'L\'Escadron Meteore voulait que je forge une clef... j\'ai refuse.',
        ],
        facing: 'up',
        movement: 'static',
      },
      {
        id: 'volcan_villager1',
        name: 'VILLAGEOIS',
        x: 8, y: 12,
        spriteType: 'villager',
        dialogue: [
          'Le volcan est actif mais les anciens disent qu\'il ne menace personne.',
          'Tant que le gardien de lave dort, tout ira bien.',
        ],
        facing: 'right',
        movement: 'wander',
      },
      {
        id: 'volcan_villager2',
        name: 'VILLAGEOISE',
        x: 20, y: 13,
        spriteType: 'villager',
        dialogue: [
          'La Route 5 au nord mene vers des contrees glaciales.',
          'Quel contraste avec notre fournaise !',
        ],
        facing: 'up',
        movement: 'static',
      },
    ],
    warps: [
      // West -> Route 4
      { x: 0, y: 12, targetMap: 'ROUTE_4', targetX: 38, targetY: 9 },
      { x: 0, y: 13, targetMap: 'ROUTE_4', targetX: 38, targetY: 10 },
      // North -> Route 5
      { x: 16, y: 0, targetMap: 'ROUTE_5', targetX: 17, targetY: 33 },
      { x: 17, y: 0, targetMap: 'ROUTE_5', targetX: 18, targetY: 33 },
    ],
    buildings: [
      { id: 'volcan_dino_center', type: 'dino_center', doorX: 5, doorY: 6, signX: 2, signY: 6, signText: 'CENTRE DINO de Volcanville — Soins gratuits pour vos dinos' },
      { id: 'volcan_shop', type: 'shop', doorX: 12, doorY: 6, signX: 9, signY: 6, signText: 'BOUTIQUE Volcanville — Potions, Balls et plus !' },
      { id: 'volcan_gym', type: 'gym', doorX: 25, doorY: 7, signX: 21, signY: 7, signText: 'ARENE de Volcanville — Champion : BRAZIER — Type : Feu', sceneData: { gymId: 3 } },
      { id: 'volcan_forge', type: 'house', doorX: 12, doorY: 18, signX: 9, signY: 18, signText: 'FORGE DE VULCAIN — Armures et objets pour dinos' },
    ],
    music: 'volcano_theme',
    terrain: 'rock',
  };
}

// ============================================================
// MAP 10: ROUTE_5 — Volcanville to Cryo-Cite (35x35)
// ============================================================
function createRoute5(): MapData {
  const W = 35, H = 35;
  const tiles = fillGrid(W, H, TileType.Grass);

  // --- Left boundary ---
  for (let y = 0; y < H; y++) {
    setTile(tiles, 0, y, TileType.Tree);
    setTile(tiles, 1, y, TileType.Tree);
  }

  // --- Right boundary ---
  for (let y = 0; y < H; y++) {
    setTile(tiles, W - 1, y, TileType.Tree);
    setTile(tiles, W - 2, y, TileType.Tree);
  }

  // --- Top boundary (exit to Cryo-Cite) ---
  for (let x = 0; x < W; x++) {
    if (x < 16 || x > 17) {
      setTile(tiles, x, 0, TileType.Tree);
      setTile(tiles, x, 1, TileType.Tree);
    }
  }

  // --- Bottom boundary (entry from Volcanville) ---
  for (let x = 0; x < W; x++) {
    if (x < 17 || x > 18) {
      setTile(tiles, x, H - 1, TileType.Tree);
    }
  }

  // --- Terrain transition: sand (bottom) -> grass (middle) -> ice (top) ---
  // Bottom third: sand
  for (let y = 24; y < H; y++) {
    for (let x = 2; x < W - 2; x++) {
      if (tiles[y][x] === TileType.Grass) setTile(tiles, x, y, TileType.Sand);
    }
  }
  // Top third: ice
  for (let y = 2; y <= 10; y++) {
    for (let x = 2; x < W - 2; x++) {
      if (tiles[y][x] === TileType.Grass) setTile(tiles, x, y, TileType.Ice);
    }
  }

  // --- Main vertical path ---
  for (let y = 0; y < H; y++) {
    setTile(tiles, 16, y, TileType.Path);
    setTile(tiles, 17, y, TileType.Path);
  }

  // --- Tall grass patches ---
  // Patch 1 (sand zone)
  for (let y = 26; y <= 30; y++) {
    for (let x = 5; x <= 9; x++) {
      if (tiles[y][x] === TileType.Sand) setTile(tiles, x, y, TileType.TallGrass);
    }
  }
  // Patch 2 (grass zone)
  for (let y = 15; y <= 19; y++) {
    for (let x = 5; x <= 10; x++) {
      if (tiles[y][x] === TileType.Grass) setTile(tiles, x, y, TileType.TallGrass);
    }
  }
  // Patch 3 (grass zone right)
  for (let y = 18; y <= 22; y++) {
    for (let x = 22; x <= 27; x++) {
      if (tiles[y][x] === TileType.Grass) setTile(tiles, x, y, TileType.TallGrass);
    }
  }
  // Patch 4 (transition zone)
  for (let y = 11; y <= 14; y++) {
    for (let x = 22; x <= 26; x++) {
      if (tiles[y][x] === TileType.Grass) setTile(tiles, x, y, TileType.TallGrass);
    }
  }

  // --- Trees and rocks ---
  const features = [
    [4, 4], [5, 4], [4, 5], [5, 5],
    [10, 3], [11, 3], [10, 4],
    [25, 4], [26, 4], [25, 5],
    [4, 13], [5, 13], [4, 14],
    [12, 17], [13, 17], [12, 18],
    [25, 14], [26, 14],
    [4, 24], [5, 24], [4, 25],
    [25, 26], [26, 26], [25, 27],
    [10, 30], [11, 30],
    [28, 30], [29, 30],
  ];
  for (const [tx, ty] of features) {
    if (tiles[ty]?.[tx] !== TileType.Path) setTile(tiles, tx, ty, TileType.Tree);
  }

  // --- Signs ---
  setTile(tiles, 15, 2, TileType.Sign);
  setTile(tiles, 15, 33, TileType.Sign);

  // --- Ranger Cabin (rest stop) ---
  buildBuilding(tiles, 8, 20, 3, 2, 9, 21);
  setTile(tiles, 10, 21, TileType.Sign);
  // Path from cabin to main path
  for (let x = 9; x <= 16; x++) {
    setTile(tiles, x, 21, TileType.Path);
  }

  return {
    id: 'ROUTE_5',
    name: 'ROUTE 5',
    width: W,
    height: H,
    tiles,
    collisionTiles: [TileType.Water, TileType.Tree, TileType.Wall, TileType.Roof, TileType.Fence, TileType.Sign, TileType.Lava],
    encounterTiles: [TileType.TallGrass],
    encounterRate: 10,
    encounters: [
      { dinoId: 30, name: 'Magmaclaw', minLevel: 14, maxLevel: 17, weight: 15 },
      { dinoId: 33, name: 'Glacidon', minLevel: 15, maxLevel: 18, weight: 15 },
      { dinoId: 35, name: 'Ashwing', minLevel: 14, maxLevel: 17, weight: 15 },
      { dinoId: 38, name: 'Embersaur', minLevel: 15, maxLevel: 18, weight: 10 },
      { dinoId: 40, name: 'Frostbite', minLevel: 16, maxLevel: 19, weight: 15 },
      { dinoId: 42, name: 'Snowpounce', minLevel: 14, maxLevel: 17, weight: 15 },
      { dinoId: 45, name: 'Tundraclaw', minLevel: 15, maxLevel: 19, weight: 10 },
      { dinoId: 48, name: 'Blizzarex', minLevel: 16, maxLevel: 19, weight: 5 },
    ],
    npcs: [
      {
        id: 'route5_trainer1',
        name: 'DRESSEUR NATHAN',
        x: 16, y: 28,
        spriteType: 'trainer',
        dialogue: [
          'Le terrain change vite ici. Du sable a la neige en une journee.',
          'Mes dinos Feu n\'aiment pas trop le froid la-haut.',
        ],
        facing: 'up',
        movement: 'static',
      },
      {
        id: 'route5_trainer2',
        name: 'DRESSEUSE ELSA',
        x: 17, y: 16,
        spriteType: 'trainer',
        dialogue: [
          'Je m\'entraine dans les deux extremes : feu et glace.',
          'Un bon dresseur s\'adapte a tout !',
          'Cryo-Cite est une merveille de glace au nord.',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'route5_trainer3',
        name: 'RANDONNEUR BORIS',
        x: 16, y: 6,
        spriteType: 'trainer',
        dialogue: [
          'Brrr ! Il fait un froid glacial par ici.',
          'Mes dinos Glace adorent, mais moi je grelotte.',
          'Le gym de Cryo-Cite est de type Glace. Prevois du Feu !',
        ],
        facing: 'down',
        movement: 'static',
      },
    ],
    warps: [
      // South -> Volcanville
      { x: 17, y: H - 1, targetMap: 'VOLCANVILLE', targetX: 16, targetY: 1 },
      { x: 18, y: H - 1, targetMap: 'VOLCANVILLE', targetX: 17, targetY: 1 },
      // North -> Cryo-Cite
      { x: 16, y: 0, targetMap: 'CRYO_CITE', targetX: 17, targetY: 23 },
      { x: 17, y: 0, targetMap: 'CRYO_CITE', targetX: 18, targetY: 23 },
    ],
    buildings: [
      { id: 'route5_ranger', type: 'dino_center', doorX: 9, doorY: 21, signX: 10, signY: 21, signText: 'CABANE DU RANGER — Soins d\'urgence' },
    ],
    music: 'route_theme',
    terrain: 'grass',
  };
}

// ============================================================
// MAP 11: CRYO_CITE — Gym 5 Ice Town (35x25)
// ============================================================
function createCryoCite(): MapData {
  const W = 35, H = 25;
  const tiles = fillGrid(W, H, TileType.Ice);

  // --- Top boundary ---
  setRect(tiles, 0, 0, W, 2, TileType.Tree);

  // --- Bottom boundary (entry from Route 5) ---
  for (let x = 0; x < W; x++) {
    if (x < 17 || x > 18) {
      setTile(tiles, x, H - 1, TileType.Tree);
    }
  }

  // --- Left boundary ---
  for (let y = 0; y < H; y++) {
    setTile(tiles, 0, y, TileType.Tree);
    setTile(tiles, 1, y, TileType.Tree);
  }

  // --- Right boundary (exit to Route 6) ---
  for (let y = 0; y < H; y++) {
    if (y < 12 || y > 13) {
      setTile(tiles, W - 1, y, TileType.Tree);
      setTile(tiles, W - 2, y, TileType.Tree);
    }
  }

  // --- Main paths ---
  for (let x = 2; x < W; x++) {
    setTile(tiles, x, 12, TileType.Path);
    setTile(tiles, x, 13, TileType.Path);
  }
  // South entry path
  for (let y = 13; y < H; y++) {
    setTile(tiles, 17, y, TileType.Path);
    setTile(tiles, 18, y, TileType.Path);
  }

  // --- Dino Center at (3, 4) ---
  buildBuilding(tiles, 3, 4, 5, 3, 5, 6);
  setTile(tiles, 2, 6, TileType.Sign);
  for (let y = 6; y <= 12; y++) setTile(tiles, 5, y, TileType.Path);

  // --- Boutique at (10, 4) ---
  buildBuilding(tiles, 10, 4, 4, 3, 12, 6);
  setTile(tiles, 9, 6, TileType.Sign);
  for (let y = 6; y <= 12; y++) setTile(tiles, 12, y, TileType.Path);

  // --- Gym (light blue/white) at (22, 4) ---
  buildBuilding(tiles, 22, 4, 6, 4, 25, 7);
  setTile(tiles, 21, 7, TileType.Sign);
  for (let y = 7; y <= 12; y++) setTile(tiles, 25, y, TileType.Path);

  // --- Research Lab at (22, 16) ---
  buildBuilding(tiles, 22, 16, 5, 3, 24, 18);
  setTile(tiles, 21, 18, TileType.Sign);
  for (let y = 13; y <= 18; y++) setTile(tiles, 24, y, TileType.Path);

  // --- Snowy trees (scattered) ---
  const snowTrees = [
    [2, 8], [3, 8], [8, 8], [9, 8],
    [15, 4], [16, 4], [15, 5],
    [2, 16], [3, 16], [2, 17], [3, 17],
    [8, 16], [9, 16],
    [15, 17], [16, 17],
    [28, 8], [29, 8], [30, 8],
    [28, 17], [29, 17],
  ];
  for (const [tx, ty] of snowTrees) {
    if (tiles[ty]?.[tx] === TileType.Ice) setTile(tiles, tx, ty, TileType.Tree);
  }

  // --- Grass patches in ice ---
  setRect(tiles, 5, 16, 3, 3, TileType.Grass);
  setRect(tiles, 14, 15, 3, 3, TileType.Grass);

  return {
    id: 'CRYO_CITE',
    name: 'CRYO-CITE',
    width: W,
    height: H,
    tiles,
    collisionTiles: [TileType.Water, TileType.Tree, TileType.Wall, TileType.Roof, TileType.Fence, TileType.Sign, TileType.Lava],
    encounterTiles: [],
    encounterRate: 0,
    encounters: [],
    npcs: [
      {
        id: 'cryo_nurse',
        name: 'INFIRMIERE',
        x: 4, y: 7,
        spriteType: 'nurse',
        dialogue: [
          'Bienvenue au Centre Dino de Cryo-Cite !',
          'Attention aux gelures, les dinos aussi peuvent en souffrir.',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'cryo_shopkeeper',
        name: 'VENDEUR',
        x: 11, y: 7,
        spriteType: 'shopkeeper',
        dialogue: [
          'On a des Anti-Gel et des Hyper Potions en stock.',
          'Les Ultra Balls sont arrivees, tres efficaces !',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'cryo_gymguide',
        name: 'GUIDE ARENA',
        x: 24, y: 8,
        spriteType: 'villager',
        dialogue: [
          'L\'arene de Cryo-Cite est de type Glace.',
          'Givralia, la championne, gele tout sur son passage.',
          'Les dinos Feu, Combat ou Roche sont tes meilleurs allies !',
        ],
        facing: 'left',
        movement: 'static',
      },
      {
        id: 'cryo_researcher',
        name: 'CHERCHEUSE AURORA',
        x: 23, y: 19,
        spriteType: 'villager',
        dialogue: [
          'Je suis la Chercheuse Aurora. J\'etudie les dinos prehistoriques congeles.',
          'Nos recherches montrent que certains dinos peuvent survivre des millenaires dans la glace.',
          'L\'Escadron Meteore s\'interesse aussi a nos decouvertes... inquietant.',
        ],
        facing: 'up',
        movement: 'static',
      },
      {
        id: 'cryo_villager1',
        name: 'VILLAGEOIS',
        x: 10, y: 12,
        spriteType: 'villager',
        dialogue: [
          'Cryo-Cite est la ville la plus froide du continent.',
          'Mais on s\'y habitue ! Nos dinos nous tiennent chaud.',
        ],
        facing: 'right',
        movement: 'wander',
      },
      {
        id: 'cryo_villager2',
        name: 'VILLAGEOISE',
        x: 18, y: 13,
        spriteType: 'villager',
        dialogue: [
          'La Route 6 a l\'est mene vers Electropolis.',
          'C\'est une grande ville moderne. Quel changement apres notre petite cite !',
        ],
        facing: 'left',
        movement: 'static',
      },
    ],
    warps: [
      // South -> Route 5
      { x: 17, y: H - 1, targetMap: 'ROUTE_5', targetX: 16, targetY: 1 },
      { x: 18, y: H - 1, targetMap: 'ROUTE_5', targetX: 17, targetY: 1 },
      // East -> Route 6
      { x: W - 1, y: 12, targetMap: 'ROUTE_6', targetX: 1, targetY: 9 },
      { x: W - 1, y: 13, targetMap: 'ROUTE_6', targetX: 1, targetY: 10 },
    ],
    buildings: [
      { id: 'cryo_dino_center', type: 'dino_center', doorX: 5, doorY: 6, signX: 2, signY: 6, signText: 'CENTRE DINO de Cryo-Cite — Soins gratuits pour vos dinos' },
      { id: 'cryo_shop', type: 'shop', doorX: 12, doorY: 6, signX: 9, signY: 6, signText: 'BOUTIQUE Cryo-Cite — Potions, Balls et plus !' },
      { id: 'cryo_gym', type: 'gym', doorX: 25, doorY: 7, signX: 21, signY: 7, signText: 'ARENE de Cryo-Cite — Championne : GIVRALIA — Type : Glace', sceneData: { gymId: 4 } },
      { id: 'cryo_research_lab', type: 'house', doorX: 24, doorY: 18, signX: 21, signY: 18, signText: 'LABORATOIRE DE RECHERCHE — Etude des dinos prehistoriques' },
    ],
    music: 'ice_theme',
    terrain: 'ice',
  };
}

// ============================================================
// MAP 12: ROUTE_6 — Cryo-Cite to Electropolis (45x20)
// ============================================================
function createRoute6(): MapData {
  const W = 45, H = 20;
  const tiles = fillGrid(W, H, TileType.Grass);

  // --- Top boundary ---
  setRect(tiles, 0, 0, W, 2, TileType.Tree);

  // --- Bottom boundary ---
  setRect(tiles, 0, H - 2, W, 2, TileType.Tree);

  // --- Left boundary (entry from Cryo-Cite) ---
  for (let y = 0; y < H; y++) {
    if (y < 9 || y > 10) setTile(tiles, 0, y, TileType.Tree);
  }

  // --- Right boundary (exit to Electropolis) ---
  for (let y = 0; y < H; y++) {
    if (y < 9 || y > 10) setTile(tiles, W - 1, y, TileType.Tree);
  }

  // --- Terrain transition: ice (left) -> grass (center) -> path-heavy (right) ---
  for (let y = 2; y < H - 2; y++) {
    for (let x = 1; x <= 8; x++) {
      if (tiles[y][x] === TileType.Grass) setTile(tiles, x, y, TileType.Ice);
    }
  }

  // --- Main horizontal path ---
  for (let x = 0; x < W; x++) {
    setTile(tiles, x, 9, TileType.Path);
    setTile(tiles, x, 10, TileType.Path);
  }

  // --- Tall grass patches ---
  for (let y = 4; y <= 7; y++) {
    for (let x = 12; x <= 17; x++) {
      if (tiles[y][x] === TileType.Grass) setTile(tiles, x, y, TileType.TallGrass);
    }
  }
  for (let y = 12; y <= 15; y++) {
    for (let x = 22; x <= 27; x++) {
      if (tiles[y][x] === TileType.Grass) setTile(tiles, x, y, TileType.TallGrass);
    }
  }
  for (let y = 4; y <= 8; y++) {
    for (let x = 33; x <= 37; x++) {
      if (tiles[y][x] === TileType.Grass) setTile(tiles, x, y, TileType.TallGrass);
    }
  }

  // --- Trees ---
  const treesR6 = [
    [5, 4], [6, 4], [5, 5],
    [10, 3], [10, 4],
    [19, 4], [20, 4], [19, 5],
    [20, 14], [21, 14],
    [30, 4], [31, 4], [30, 5],
    [30, 13], [31, 13],
    [39, 4], [40, 4],
    [39, 14], [40, 14],
  ];
  for (const [tx, ty] of treesR6) {
    if (tiles[ty]?.[tx] !== TileType.Path) setTile(tiles, tx, ty, TileType.Tree);
  }

  // --- Signs ---
  setTile(tiles, 3, 8, TileType.Sign);
  setTile(tiles, 42, 8, TileType.Sign);

  return {
    id: 'ROUTE_6',
    name: 'ROUTE 6',
    width: W,
    height: H,
    tiles,
    collisionTiles: [TileType.Water, TileType.Tree, TileType.Wall, TileType.Roof, TileType.Fence, TileType.Sign, TileType.Lava],
    encounterTiles: [TileType.TallGrass],
    encounterRate: 10,
    encounters: [
      { dinoId: 40, name: 'Frostbite', minLevel: 18, maxLevel: 21, weight: 15 },
      { dinoId: 42, name: 'Snowpounce', minLevel: 18, maxLevel: 21, weight: 15 },
      { dinoId: 45, name: 'Tundraclaw', minLevel: 19, maxLevel: 22, weight: 10 },
      { dinoId: 50, name: 'Voltarex', minLevel: 20, maxLevel: 23, weight: 15 },
      { dinoId: 52, name: 'Sparkjaw', minLevel: 19, maxLevel: 22, weight: 15 },
      { dinoId: 55, name: 'Thundertail', minLevel: 20, maxLevel: 23, weight: 15 },
      { dinoId: 58, name: 'Stormraptor', minLevel: 21, maxLevel: 23, weight: 10 },
      { dinoId: 60, name: 'Boltwing', minLevel: 20, maxLevel: 23, weight: 5 },
    ],
    npcs: [
      {
        id: 'route6_trainer1',
        name: 'DRESSEUSE MARINA',
        x: 15, y: 9,
        spriteType: 'trainer',
        dialogue: [
          'La neige fond deja ici. Le printemps revient vite.',
          'Mes dinos Glace commencent a avoir chaud !',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'route6_trainer2',
        name: 'DRESSEUR LUCAS',
        x: 28, y: 10,
        spriteType: 'trainer',
        dialogue: [
          'Electropolis est une ville incroyable ! Des lumieres partout.',
          'Le gym utilise des dinos Foudre. Gare aux paralysies !',
          'Les dinos de type Terre sont immunises contre l\'electricite.',
        ],
        facing: 'left',
        movement: 'static',
      },
      {
        id: 'route6_trainer3',
        name: 'RANDONNEUSE DIANA',
        x: 38, y: 10,
        spriteType: 'trainer',
        dialogue: [
          'On voit les lumieres d\'Electropolis d\'ici !',
          'C\'est la plus grande ville du continent.',
        ],
        facing: 'left',
        movement: 'static',
      },
    ],
    warps: [
      // West -> Cryo-Cite
      { x: 0, y: 9, targetMap: 'CRYO_CITE', targetX: 33, targetY: 12 },
      { x: 0, y: 10, targetMap: 'CRYO_CITE', targetX: 33, targetY: 13 },
      // East -> Electropolis
      { x: W - 1, y: 9, targetMap: 'ELECTROPOLIS', targetX: 1, targetY: 14 },
      { x: W - 1, y: 10, targetMap: 'ELECTROPOLIS', targetX: 1, targetY: 15 },
    ],
    music: 'route_theme',
    terrain: 'grass',
  };
}

// ============================================================
// MAP 13: ELECTROPOLIS — Gym 6 Lightning City (40x30)
// ============================================================
function createElectropolis(): MapData {
  const W = 40, H = 30;
  const tiles = fillGrid(W, H, TileType.Path);

  // --- Top boundary ---
  setRect(tiles, 0, 0, W, 2, TileType.Tree);

  // --- Bottom boundary (exit to Route 7) ---
  for (let x = 0; x < W; x++) {
    if (x < 19 || x > 20) {
      setTile(tiles, x, H - 1, TileType.Tree);
      setTile(tiles, x, H - 2, TileType.Tree);
    }
  }

  // --- Left boundary (entry from Route 6) ---
  for (let y = 0; y < H; y++) {
    if (y < 14 || y > 15) {
      setTile(tiles, 0, y, TileType.Tree);
    }
  }

  // --- Right boundary ---
  for (let y = 0; y < H; y++) {
    setTile(tiles, W - 1, y, TileType.Tree);
    setTile(tiles, W - 2, y, TileType.Tree);
  }

  // --- Main paths (wider streets for big city feel) ---
  for (let x = 0; x < W; x++) {
    setTile(tiles, x, 14, TileType.Path);
    setTile(tiles, x, 15, TileType.Path);
  }
  for (let y = 0; y < H; y++) {
    setTile(tiles, 19, y, TileType.Path);
    setTile(tiles, 20, y, TileType.Path);
  }

  // --- Some grass patches in the city (parks) ---
  setRect(tiles, 3, 8, 4, 4, TileType.Grass);
  setRect(tiles, 30, 20, 4, 4, TileType.Grass);

  // --- Dino Center at (3, 3) ---
  buildBuilding(tiles, 3, 3, 5, 3, 5, 5);
  setTile(tiles, 2, 5, TileType.Sign);

  // --- Boutique (larger) at (10, 3) ---
  buildBuilding(tiles, 10, 3, 5, 3, 12, 5);
  setTile(tiles, 9, 5, TileType.Sign);

  // --- Gym (yellow roof) at (25, 3) ---
  buildBuilding(tiles, 25, 3, 7, 4, 28, 6);
  setTile(tiles, 24, 6, TileType.Sign);

  // --- Game Corner at (3, 18) ---
  buildBuilding(tiles, 3, 18, 5, 3, 5, 20);
  setTile(tiles, 2, 20, TileType.Sign);

  // --- Department Store at (10, 18) ---
  buildBuilding(tiles, 10, 18, 6, 4, 13, 21);
  setTile(tiles, 9, 21, TileType.Sign);

  // --- Houses ---
  buildBuilding(tiles, 25, 18, 4, 3, 27, 20);
  buildBuilding(tiles, 33, 3, 4, 3, 35, 5);
  buildBuilding(tiles, 33, 10, 4, 3, 35, 12);

  // --- Trees (sparse, it's a city) ---
  const cityTrees = [
    [4, 9], [5, 9], [4, 10], [5, 10],
    [31, 21], [32, 21], [31, 22], [32, 22],
    [17, 5], [18, 5],
    [17, 10], [18, 10],
    [22, 10], [23, 10],
  ];
  for (const [tx, ty] of cityTrees) {
    if (tiles[ty]?.[tx] === TileType.Grass || tiles[ty]?.[tx] === TileType.Path) {
      setTile(tiles, tx, ty, TileType.Tree);
    }
  }

  // --- Flowers in parks ---
  const flowers = [
    [3, 8], [4, 8], [6, 10], [6, 11],
    [30, 20], [33, 20], [30, 23], [33, 23],
  ];
  for (const [fx, fy] of flowers) {
    if (tiles[fy]?.[fx] === TileType.Grass) setTile(tiles, fx, fy, TileType.Flower);
  }

  return {
    id: 'ELECTROPOLIS',
    name: 'ELECTROPOLIS',
    width: W,
    height: H,
    tiles,
    collisionTiles: [TileType.Water, TileType.Tree, TileType.Wall, TileType.Roof, TileType.Fence, TileType.Sign, TileType.Lava],
    encounterTiles: [],
    encounterRate: 0,
    encounters: [],
    npcs: [
      {
        id: 'electro_nurse',
        name: 'INFIRMIERE',
        x: 4, y: 6,
        spriteType: 'nurse',
        dialogue: [
          'Bienvenue au Centre Dino d\'Electropolis !',
          'La ville ne dort jamais, et moi non plus.',
          'Tes dinos sont soignes et prets pour le gym !',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'electro_shopkeeper',
        name: 'VENDEUR',
        x: 11, y: 6,
        spriteType: 'shopkeeper',
        dialogue: [
          'Le plus grand magasin du continent ! On a tout ici.',
          'Ultra Balls, Hyper Potions, Rappels... a toi de choisir !',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'electro_gymguide',
        name: 'GUIDE ARENA',
        x: 27, y: 7,
        spriteType: 'villager',
        dialogue: [
          'L\'arene d\'Electropolis est de type Foudre.',
          'Voltaire, le champion, est un genie de l\'electricite.',
          'Les dinos de type Terre sont completement immunises !',
        ],
        facing: 'left',
        movement: 'static',
      },
      {
        id: 'electro_scientist',
        name: 'SCIENTIFIQUE TESLA',
        x: 22, y: 14,
        spriteType: 'villager',
        dialogue: [
          'Je travaille sur une source d\'energie a base de dinos Foudre.',
          'Electropolis est alimentee par les orages que provoquent les Voltarex sauvages.',
          'L\'Escadron Meteore veut exploiter cette technologie a des fins destructrices.',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'electro_gamer',
        name: 'JOUEUR KEVIN',
        x: 4, y: 21,
        spriteType: 'villager',
        dialogue: [
          'Le Coin des Jeux est genial ! On peut gagner des prix rares.',
          'J\'ai gagne un CT pour mon dino. C\'etait la chance de ma vie !',
        ],
        facing: 'right',
        movement: 'static',
      },
      {
        id: 'electro_villager1',
        name: 'CITADIN',
        x: 15, y: 15,
        spriteType: 'villager',
        dialogue: [
          'Electropolis est la ville la plus avancee du continent.',
          'Tout fonctionne a l\'energie electrique des dinos.',
        ],
        facing: 'down',
        movement: 'wander',
      },
      {
        id: 'electro_villager2',
        name: 'CITADINE',
        x: 25, y: 15,
        spriteType: 'villager',
        dialogue: [
          'La Route 7 au sud traverse les marais. C\'est lugubre.',
          'Fais attention aux dinos Poison la-bas.',
        ],
        facing: 'left',
        movement: 'static',
      },
      {
        id: 'electro_villager3',
        name: 'EMPLOYE',
        x: 12, y: 22,
        spriteType: 'villager',
        dialogue: [
          'Le Grand Magasin a tout ce dont un dresseur a besoin.',
          'N\'oublie pas d\'acheter des Rappels avant de partir.',
        ],
        facing: 'up',
        movement: 'static',
      },
    ],
    warps: [
      // West -> Route 6
      { x: 0, y: 14, targetMap: 'ROUTE_6', targetX: 43, targetY: 9 },
      { x: 0, y: 15, targetMap: 'ROUTE_6', targetX: 43, targetY: 10 },
      // South -> Route 7
      { x: 19, y: H - 1, targetMap: 'ROUTE_7', targetX: 14, targetY: 1 },
      { x: 20, y: H - 1, targetMap: 'ROUTE_7', targetX: 15, targetY: 1 },
    ],
    buildings: [
      { id: 'electro_dino_center', type: 'dino_center', doorX: 5, doorY: 5, signX: 2, signY: 5, signText: 'CENTRE DINO d\'Electropolis — Soins gratuits pour vos dinos' },
      { id: 'electro_shop', type: 'shop', doorX: 12, doorY: 5, signX: 9, signY: 5, signText: 'BOUTIQUE Electropolis — Potions, Balls et plus !' },
      { id: 'electro_gym', type: 'gym', doorX: 28, doorY: 6, signX: 24, signY: 6, signText: 'ARENE d\'Electropolis — Champion : VOLTAIRE — Type : Foudre', sceneData: { gymId: 5 } },
      { id: 'electro_game_corner', type: 'house', doorX: 5, doorY: 20, signX: 2, signY: 20, signText: 'COIN DES JEUX — Divertissement et prix rares' },
      { id: 'electro_dept_store', type: 'shop', doorX: 13, doorY: 21, signX: 9, signY: 21, signText: 'GRAND MAGASIN — Le plus grand magasin du continent' },
      { id: 'electro_house1', type: 'house', doorX: 27, doorY: 20, signX: -1, signY: -1, signText: '' },
      { id: 'electro_house2', type: 'house', doorX: 35, doorY: 5, signX: -1, signY: -1, signText: '' },
      { id: 'electro_house3', type: 'house', doorX: 35, doorY: 12, signX: -1, signY: -1, signText: '' },
    ],
    music: 'city_theme',
    terrain: 'grass',
  };
}

// ============================================================
// MAP 14: ROUTE_7 — Marshland Route (30x40)
// ============================================================
function createRoute7(): MapData {
  const W = 30, H = 40;
  const tiles = fillGrid(W, H, TileType.Grass);

  // --- Left boundary ---
  for (let y = 0; y < H; y++) {
    setTile(tiles, 0, y, TileType.Tree);
    setTile(tiles, 1, y, TileType.Tree);
  }

  // --- Right boundary ---
  for (let y = 0; y < H; y++) {
    setTile(tiles, W - 1, y, TileType.Tree);
    setTile(tiles, W - 2, y, TileType.Tree);
  }

  // --- Top boundary (entry from Electropolis) ---
  for (let x = 0; x < W; x++) {
    if (x < 14 || x > 15) {
      setTile(tiles, x, 0, TileType.Tree);
    }
  }

  // --- Bottom boundary (exit to Marais-Noir) ---
  for (let x = 0; x < W; x++) {
    if (x < 14 || x > 15) {
      setTile(tiles, x, H - 1, TileType.Tree);
    }
  }

  // --- Main vertical path ---
  for (let y = 0; y < H; y++) {
    setTile(tiles, 14, y, TileType.Path);
    setTile(tiles, 15, y, TileType.Path);
  }

  // --- Water sections (marsh) ---
  setRect(tiles, 3, 6, 6, 4, TileType.Water);
  setRect(tiles, 20, 12, 5, 5, TileType.Water);
  setRect(tiles, 4, 22, 5, 4, TileType.Water);
  setRect(tiles, 18, 28, 6, 4, TileType.Water);
  setRect(tiles, 5, 33, 4, 3, TileType.Water);

  // --- Bridges over water ---
  setTile(tiles, 6, 6, TileType.Bridge);
  setTile(tiles, 6, 7, TileType.Bridge);
  setTile(tiles, 22, 12, TileType.Bridge);
  setTile(tiles, 22, 13, TileType.Bridge);
  setTile(tiles, 6, 22, TileType.Bridge);
  setTile(tiles, 6, 23, TileType.Bridge);
  setTile(tiles, 20, 28, TileType.Bridge);
  setTile(tiles, 20, 29, TileType.Bridge);

  // --- Tall grass (dark marsh vegetation) ---
  for (let y = 4; y <= 8; y++) {
    for (let x = 10; x <= 13; x++) {
      if (tiles[y][x] === TileType.Grass) setTile(tiles, x, y, TileType.TallGrass);
    }
  }
  for (let y = 14; y <= 18; y++) {
    for (let x = 3; x <= 7; x++) {
      if (tiles[y][x] === TileType.Grass) setTile(tiles, x, y, TileType.TallGrass);
    }
  }
  for (let y = 20; y <= 24; y++) {
    for (let x = 16; x <= 20; x++) {
      if (tiles[y][x] === TileType.Grass) setTile(tiles, x, y, TileType.TallGrass);
    }
  }
  for (let y = 30; y <= 34; y++) {
    for (let x = 10; x <= 13; x++) {
      if (tiles[y][x] === TileType.Grass) setTile(tiles, x, y, TileType.TallGrass);
    }
  }

  // --- Dark trees ---
  const marshTrees = [
    [3, 3], [4, 3], [3, 4],
    [22, 4], [23, 4], [22, 5],
    [3, 12], [4, 12],
    [3, 18], [4, 18],
    [22, 18], [23, 18],
    [10, 26], [11, 26],
    [22, 24], [23, 24],
    [3, 35], [4, 35], [3, 36],
    [22, 34], [23, 34],
  ];
  for (const [tx, ty] of marshTrees) {
    if (tiles[ty]?.[tx] === TileType.Grass) setTile(tiles, tx, ty, TileType.Tree);
  }

  // --- Signs ---
  setTile(tiles, 13, 2, TileType.Sign);
  setTile(tiles, 13, 37, TileType.Sign);

  // --- Mountain Refuge (rest stop) ---
  buildBuilding(tiles, 20, 7, 3, 2, 21, 8);
  setTile(tiles, 19, 8, TileType.Sign);
  // Path from refuge to main path
  for (let x = 15; x <= 21; x++) {
    setTile(tiles, x, 8, TileType.Path);
  }

  return {
    id: 'ROUTE_7',
    name: 'ROUTE 7',
    width: W,
    height: H,
    tiles,
    collisionTiles: [TileType.Water, TileType.Tree, TileType.Wall, TileType.Roof, TileType.Fence, TileType.Sign, TileType.Lava],
    encounterTiles: [TileType.TallGrass],
    encounterRate: 12,
    encounters: [
      { dinoId: 55, name: 'Thundertail', minLevel: 22, maxLevel: 25, weight: 10 },
      { dinoId: 58, name: 'Stormraptor', minLevel: 23, maxLevel: 26, weight: 10 },
      { dinoId: 60, name: 'Boltwing', minLevel: 22, maxLevel: 25, weight: 10 },
      { dinoId: 62, name: 'Venomjaw', minLevel: 23, maxLevel: 26, weight: 20 },
      { dinoId: 65, name: 'Toxifang', minLevel: 24, maxLevel: 27, weight: 15 },
      { dinoId: 68, name: 'Swampfin', minLevel: 23, maxLevel: 26, weight: 15 },
      { dinoId: 70, name: 'Murkscale', minLevel: 24, maxLevel: 27, weight: 10 },
      { dinoId: 75, name: 'Shadowmaw', minLevel: 25, maxLevel: 27, weight: 10 },
    ],
    npcs: [
      {
        id: 'route7_trainer1',
        name: 'DRESSEUR DAMIEN',
        x: 14, y: 10,
        spriteType: 'trainer',
        dialogue: [
          'Les marais sont sinistres, mais mes dinos Poison adorent.',
          'Attention ou tu mets les pieds, la boue est profonde.',
        ],
        facing: 'right',
        movement: 'static',
      },
      {
        id: 'route7_trainer2',
        name: 'DRESSEUSE MORGANE',
        x: 15, y: 22,
        spriteType: 'trainer',
        dialogue: [
          'L\'Escadron Meteore a une tour cachee pres d\'ici.',
          'J\'ai vu des grunts transporter des machines etranges.',
          'Sois prudent si tu t\'en approches.',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'route7_trainer3',
        name: 'RANDONNEUR SERGE',
        x: 14, y: 34,
        spriteType: 'trainer',
        dialogue: [
          'Marais-Noir est juste au sud. Le gym est de type Poison.',
          'Les dinos Psy et Sol sont tres efficaces contre le Poison.',
          'N\'oublie pas tes Antidotes, tu en auras besoin !',
        ],
        facing: 'down',
        movement: 'static',
      },
    ],
    warps: [
      // North -> Electropolis
      { x: 14, y: 0, targetMap: 'ELECTROPOLIS', targetX: 19, targetY: 28 },
      { x: 15, y: 0, targetMap: 'ELECTROPOLIS', targetX: 20, targetY: 28 },
      // South -> Marais-Noir
      { x: 14, y: H - 1, targetMap: 'MARAIS_NOIR', targetX: 17, targetY: 1 },
      { x: 15, y: H - 1, targetMap: 'MARAIS_NOIR', targetX: 18, targetY: 1 },
    ],
    buildings: [
      { id: 'route7_refuge', type: 'dino_center', doorX: 21, doorY: 8, signX: 19, signY: 8, signText: 'REFUGE DE MONTAGNE — Centre Dino & Mini-Boutique' },
    ],
    music: 'marsh_theme',
    terrain: 'grass',
  };
}

// ============================================================
// MAP 15: MARAIS_NOIR — Gym 7 Poison Town (35x30)
// ============================================================
function createMaraisNoir(): MapData {
  const W = 35, H = 30;
  const tiles = fillGrid(W, H, TileType.Grass);

  // --- Top boundary (entry from Route 7) ---
  for (let x = 0; x < W; x++) {
    if (x < 17 || x > 18) {
      setTile(tiles, x, 0, TileType.Tree);
    }
  }

  // --- Bottom boundary ---
  setRect(tiles, 0, H - 2, W, 2, TileType.Tree);

  // --- Left boundary ---
  for (let y = 0; y < H; y++) {
    setTile(tiles, 0, y, TileType.Tree);
    setTile(tiles, 1, y, TileType.Tree);
  }

  // --- Right boundary (exit to Route 8) ---
  for (let y = 0; y < H; y++) {
    if (y < 14 || y > 15) {
      setTile(tiles, W - 1, y, TileType.Tree);
      setTile(tiles, W - 2, y, TileType.Tree);
    }
  }

  // --- Water sections (swamp) ---
  setRect(tiles, 3, 5, 4, 3, TileType.Water);
  setRect(tiles, 26, 4, 4, 4, TileType.Water);
  setRect(tiles, 3, 20, 5, 3, TileType.Water);
  setRect(tiles, 22, 20, 5, 4, TileType.Water);

  // --- Bridges ---
  setTile(tiles, 5, 5, TileType.Bridge);
  setTile(tiles, 5, 6, TileType.Bridge);
  setTile(tiles, 28, 4, TileType.Bridge);
  setTile(tiles, 28, 5, TileType.Bridge);
  setTile(tiles, 5, 20, TileType.Bridge);
  setTile(tiles, 5, 21, TileType.Bridge);
  setTile(tiles, 24, 20, TileType.Bridge);
  setTile(tiles, 24, 21, TileType.Bridge);

  // --- Main paths ---
  for (let x = 2; x < W; x++) {
    setTile(tiles, x, 14, TileType.Path);
    setTile(tiles, x, 15, TileType.Path);
  }
  // North entry path
  for (let y = 0; y <= 14; y++) {
    setTile(tiles, 17, y, TileType.Path);
    setTile(tiles, 18, y, TileType.Path);
  }

  // --- Dino Center at (4, 9) ---
  buildBuilding(tiles, 4, 9, 5, 3, 6, 11);
  setTile(tiles, 3, 11, TileType.Sign);
  for (let y = 11; y <= 14; y++) setTile(tiles, 6, y, TileType.Path);

  // --- Boutique at (12, 9) ---
  buildBuilding(tiles, 12, 9, 4, 3, 14, 11);
  setTile(tiles, 11, 11, TileType.Sign);
  for (let y = 11; y <= 14; y++) setTile(tiles, 14, y, TileType.Path);

  // --- Gym (purple roof) at (22, 9) ---
  buildBuilding(tiles, 22, 9, 6, 4, 25, 12);
  setTile(tiles, 21, 12, TileType.Sign);
  for (let y = 12; y <= 14; y++) setTile(tiles, 25, y, TileType.Path);

  // --- Herbalist Hut at (10, 18) ---
  buildBuilding(tiles, 10, 18, 4, 3, 12, 20);
  setTile(tiles, 9, 20, TileType.Sign);
  for (let y = 15; y <= 20; y++) setTile(tiles, 12, y, TileType.Path);

  // --- Dark trees ---
  const swampTrees = [
    [8, 4], [9, 4], [8, 5],
    [14, 3], [15, 3], [14, 4],
    [20, 4], [21, 4],
    [2, 15], [3, 15],
    [8, 17], [9, 17],
    [16, 18], [17, 18],
    [2, 24], [3, 24], [2, 25], [3, 25],
    [10, 25], [11, 25],
    [17, 24], [18, 24],
    [28, 17], [29, 17],
    [28, 25], [29, 25],
  ];
  for (const [tx, ty] of swampTrees) {
    if (tiles[ty]?.[tx] === TileType.Grass) setTile(tiles, tx, ty, TileType.Tree);
  }

  return {
    id: 'MARAIS_NOIR',
    name: 'MARAIS-NOIR',
    width: W,
    height: H,
    tiles,
    collisionTiles: [TileType.Water, TileType.Tree, TileType.Wall, TileType.Roof, TileType.Fence, TileType.Sign, TileType.Lava],
    encounterTiles: [],
    encounterRate: 0,
    encounters: [],
    npcs: [
      {
        id: 'marais_nurse',
        name: 'INFIRMIERE',
        x: 5, y: 12,
        spriteType: 'nurse',
        dialogue: [
          'Bienvenue au Centre Dino de Marais-Noir !',
          'Les empoisonnements sont frequents ici. Je suis preparee.',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'marais_shopkeeper',
        name: 'VENDEUR',
        x: 13, y: 12,
        spriteType: 'shopkeeper',
        dialogue: [
          'Antidotes, Total Soins, Rappels... tout pour survivre aux marais.',
          'Les Hyper Balls sont notre specialite !',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'marais_gymguide',
        name: 'GUIDE ARENA',
        x: 24, y: 13,
        spriteType: 'villager',
        dialogue: [
          'L\'arene de Marais-Noir est de type Poison.',
          'Toxica, la championne, maitrise l\'art de l\'empoisonnement.',
          'Les dinos Psy, Sol ou Acier resistant au Poison !',
        ],
        facing: 'left',
        movement: 'static',
      },
      {
        id: 'marais_herbalist',
        name: 'HERBORISTE VENIN',
        x: 11, y: 21,
        spriteType: 'villager',
        dialogue: [
          'Je suis l\'Herboriste Venin. Je transforme les poisons en remedes.',
          'Les plantes toxiques des marais guerissent autant qu\'elles tuent.',
          'L\'equilibre de la nature est fragile. L\'Escadron Meteore le menace.',
        ],
        facing: 'up',
        movement: 'static',
      },
      {
        id: 'marais_villager1',
        name: 'VILLAGEOIS',
        x: 10, y: 14,
        spriteType: 'villager',
        dialogue: [
          'Marais-Noir a mauvaise reputation, mais ses habitants sont accueillants.',
          'On vit en harmonie avec les dinos Poison depuis des generations.',
        ],
        facing: 'right',
        movement: 'wander',
      },
      {
        id: 'marais_villager2',
        name: 'VILLAGEOISE',
        x: 20, y: 15,
        spriteType: 'villager',
        dialogue: [
          'La Route 8 a l\'est est longue et difficile.',
          'Elle mene a Ciel-Haut, le dernier gym avant la Ligue.',
        ],
        facing: 'left',
        movement: 'static',
      },
    ],
    warps: [
      // North -> Route 7
      { x: 17, y: 0, targetMap: 'ROUTE_7', targetX: 14, targetY: 38 },
      { x: 18, y: 0, targetMap: 'ROUTE_7', targetX: 15, targetY: 38 },
      // East -> Route 8
      { x: W - 1, y: 14, targetMap: 'ROUTE_8', targetX: 1, targetY: 9 },
      { x: W - 1, y: 15, targetMap: 'ROUTE_8', targetX: 1, targetY: 10 },
    ],
    buildings: [
      { id: 'marais_dino_center', type: 'dino_center', doorX: 6, doorY: 11, signX: 3, signY: 11, signText: 'CENTRE DINO de Marais-Noir — Soins gratuits pour vos dinos' },
      { id: 'marais_shop', type: 'shop', doorX: 14, doorY: 11, signX: 11, signY: 11, signText: 'BOUTIQUE Marais-Noir — Potions, Balls et plus !' },
      { id: 'marais_gym', type: 'gym', doorX: 25, doorY: 12, signX: 21, signY: 12, signText: 'ARENE de Marais-Noir — Championne : TOXICA — Type : Poison', sceneData: { gymId: 6 } },
      { id: 'marais_herbalist', type: 'house', doorX: 12, doorY: 20, signX: 9, signY: 20, signText: 'HERBORISTERIE DE VENIN — Remedes et antidotes naturels' },
    ],
    music: 'marsh_theme',
    terrain: 'grass',
  };
}

// ============================================================
// MAP 16: ROUTE_8 — Long Mountain Route (50x20)
// ============================================================
function createRoute8(): MapData {
  const W = 50, H = 20;
  const tiles = fillGrid(W, H, TileType.Grass);

  // --- Top boundary ---
  setRect(tiles, 0, 0, W, 2, TileType.Tree);

  // --- Bottom boundary ---
  setRect(tiles, 0, H - 2, W, 2, TileType.Tree);

  // --- Left boundary (entry from Marais-Noir) ---
  for (let y = 0; y < H; y++) {
    if (y < 9 || y > 10) setTile(tiles, 0, y, TileType.Tree);
  }

  // --- Right boundary (exit to Ciel-Haut) ---
  for (let y = 0; y < H; y++) {
    if (y < 9 || y > 10) setTile(tiles, W - 1, y, TileType.Tree);
  }

  // --- Main horizontal path ---
  for (let x = 0; x < W; x++) {
    setTile(tiles, x, 9, TileType.Path);
    setTile(tiles, x, 10, TileType.Path);
  }

  // --- Rocky sections ---
  setRect(tiles, 8, 3, 3, 2, TileType.Sand);
  setRect(tiles, 20, 3, 4, 3, TileType.Sand);
  setRect(tiles, 35, 4, 3, 2, TileType.Sand);
  setRect(tiles, 12, 14, 3, 2, TileType.Sand);
  setRect(tiles, 28, 13, 4, 3, TileType.Sand);
  setRect(tiles, 42, 3, 3, 3, TileType.Sand);

  // --- Ledges ---
  for (let x = 5; x <= 12; x++) setTile(tiles, x, 7, TileType.Ledge);
  for (let x = 25; x <= 32; x++) setTile(tiles, x, 12, TileType.Ledge);
  for (let x = 38; x <= 45; x++) setTile(tiles, x, 7, TileType.Ledge);

  // --- Tall grass (dense) ---
  for (let y = 4; y <= 7; y++) {
    for (let x = 3; x <= 7; x++) {
      if (tiles[y][x] === TileType.Grass) setTile(tiles, x, y, TileType.TallGrass);
    }
  }
  for (let y = 12; y <= 16; y++) {
    for (let x = 15; x <= 20; x++) {
      if (tiles[y][x] === TileType.Grass) setTile(tiles, x, y, TileType.TallGrass);
    }
  }
  for (let y = 4; y <= 8; y++) {
    for (let x = 25; x <= 29; x++) {
      if (tiles[y][x] === TileType.Grass) setTile(tiles, x, y, TileType.TallGrass);
    }
  }
  for (let y = 12; y <= 16; y++) {
    for (let x = 38; x <= 43; x++) {
      if (tiles[y][x] === TileType.Grass) setTile(tiles, x, y, TileType.TallGrass);
    }
  }

  // --- Trees/boulders ---
  const r8Trees = [
    [2, 4], [2, 5],
    [13, 4], [14, 4], [13, 5],
    [18, 3], [19, 3],
    [24, 14], [25, 14],
    [33, 3], [34, 3], [33, 4],
    [37, 14], [37, 15],
    [46, 4], [47, 4], [46, 5],
  ];
  for (const [tx, ty] of r8Trees) {
    if (tiles[ty]?.[tx] === TileType.Grass || tiles[ty]?.[tx] === TileType.Sand) {
      setTile(tiles, tx, ty, TileType.Tree);
    }
  }

  // --- Signs ---
  setTile(tiles, 2, 8, TileType.Sign);
  setTile(tiles, 47, 8, TileType.Sign);

  return {
    id: 'ROUTE_8',
    name: 'ROUTE 8',
    width: W,
    height: H,
    tiles,
    collisionTiles: [TileType.Water, TileType.Tree, TileType.Wall, TileType.Roof, TileType.Fence, TileType.Sign, TileType.Lava],
    encounterTiles: [TileType.TallGrass],
    encounterRate: 12,
    encounters: [
      { dinoId: 70, name: 'Murkscale', minLevel: 26, maxLevel: 29, weight: 15 },
      { dinoId: 72, name: 'Boulderfist', minLevel: 27, maxLevel: 30, weight: 15 },
      { dinoId: 75, name: 'Shadowmaw', minLevel: 28, maxLevel: 31, weight: 10 },
      { dinoId: 78, name: 'Aerodactus', minLevel: 27, maxLevel: 30, weight: 15 },
      { dinoId: 80, name: 'Skycrusher', minLevel: 28, maxLevel: 31, weight: 10 },
      { dinoId: 85, name: 'Titanowl', minLevel: 29, maxLevel: 32, weight: 10 },
      { dinoId: 88, name: 'Peakfang', minLevel: 28, maxLevel: 31, weight: 10 },
      { dinoId: 90, name: 'Windtalon', minLevel: 29, maxLevel: 32, weight: 10 },
      { dinoId: 95, name: 'Fossilwyrm', minLevel: 30, maxLevel: 32, weight: 5 },
    ],
    npcs: [
      {
        id: 'route8_trainer1',
        name: 'DRESSEUR MAXIME',
        x: 10, y: 9,
        spriteType: 'trainer',
        dialogue: [
          'La montee vers Ciel-Haut est rude, mais la vue est magnifique.',
          'Mes dinos sont plus forts que jamais. Pret a te battre !',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'route8_trainer2',
        name: 'DRESSEUSE CHARLOTTE',
        x: 22, y: 10,
        spriteType: 'trainer',
        dialogue: [
          'Les dinos de haute montagne sont redoutables.',
          'Ne sous-estime jamais un adversaire a ce stade du voyage !',
          'Ciel-Haut est le dernier gym. Tu y es presque !',
        ],
        facing: 'left',
        movement: 'static',
      },
      {
        id: 'route8_trainer3',
        name: 'ALPINISTE YVES',
        x: 35, y: 9,
        spriteType: 'trainer',
        dialogue: [
          'J\'escalade cette montagne depuis des annees.',
          'Les dinos Vol sont dans leur element ici.',
        ],
        facing: 'right',
        movement: 'static',
      },
      {
        id: 'route8_trainer4',
        name: 'DRESSEUSE VALERIE',
        x: 44, y: 10,
        spriteType: 'trainer',
        dialogue: [
          'Ciel-Haut est juste a l\'est ! Le gym de type Vol t\'attend.',
          'Les dinos Electrique, Glace ou Roche sont efficaces contre le Vol.',
        ],
        facing: 'left',
        movement: 'static',
      },
    ],
    warps: [
      // West -> Marais-Noir
      { x: 0, y: 9, targetMap: 'MARAIS_NOIR', targetX: 33, targetY: 14 },
      { x: 0, y: 10, targetMap: 'MARAIS_NOIR', targetX: 33, targetY: 15 },
      // East -> Ciel-Haut
      { x: W - 1, y: 9, targetMap: 'CIEL_HAUT', targetX: 1, targetY: 12 },
      { x: W - 1, y: 10, targetMap: 'CIEL_HAUT', targetX: 1, targetY: 13 },
    ],
    music: 'route_theme',
    terrain: 'rock',
  };
}

// ============================================================
// MAP 17: CIEL_HAUT — Gym 8 Flying Town (35x25)
// ============================================================
function createCielHaut(): MapData {
  const W = 35, H = 25;
  const tiles = fillGrid(W, H, TileType.Grass);

  // --- Top boundary ---
  setRect(tiles, 0, 0, W, 2, TileType.Tree);

  // --- Bottom boundary ---
  setRect(tiles, 0, H - 2, W, 2, TileType.Tree);

  // --- Left boundary (entry from Route 8) ---
  for (let y = 0; y < H; y++) {
    if (y < 12 || y > 13) {
      setTile(tiles, 0, y, TileType.Tree);
    }
  }

  // --- Right boundary (exit to Route 9) ---
  for (let y = 0; y < H; y++) {
    if (y < 12 || y > 13) {
      setTile(tiles, W - 1, y, TileType.Tree);
      setTile(tiles, W - 2, y, TileType.Tree);
    }
  }

  // --- Stone paths (elevated plateau feel) ---
  for (let x = 0; x < W; x++) {
    setTile(tiles, x, 12, TileType.Path);
    setTile(tiles, x, 13, TileType.Path);
  }
  // Vertical connector paths
  for (let y = 2; y <= 12; y++) {
    setTile(tiles, 10, y, TileType.Path);
    setTile(tiles, 17, y, TileType.Path);
    setTile(tiles, 25, y, TileType.Path);
  }

  // --- Dino Center at (3, 4) ---
  buildBuilding(tiles, 3, 4, 5, 3, 5, 6);
  setTile(tiles, 2, 6, TileType.Sign);
  for (let y = 6; y <= 12; y++) setTile(tiles, 5, y, TileType.Path);

  // --- Boutique at (12, 4) ---
  buildBuilding(tiles, 12, 4, 4, 3, 14, 6);
  setTile(tiles, 11, 6, TileType.Sign);
  for (let y = 6; y <= 12; y++) setTile(tiles, 14, y, TileType.Path);

  // --- Gym (sky blue roof) at (22, 4) ---
  buildBuilding(tiles, 22, 4, 6, 4, 25, 7);
  setTile(tiles, 21, 7, TileType.Sign);

  // --- Observatory/Temple at (12, 16) ---
  buildBuilding(tiles, 12, 16, 6, 4, 15, 19);
  setTile(tiles, 11, 19, TileType.Sign);
  for (let y = 13; y <= 19; y++) setTile(tiles, 15, y, TileType.Path);

  // --- Houses ---
  buildBuilding(tiles, 3, 16, 4, 3, 5, 18);
  for (let y = 13; y <= 18; y++) setTile(tiles, 5, y, TileType.Path);

  buildBuilding(tiles, 25, 16, 4, 3, 27, 18);
  for (let y = 13; y <= 18; y++) setTile(tiles, 27, y, TileType.Path);

  // --- Open grassy terrain (few trees for plateau feel) ---
  const plateauTrees = [
    [2, 8], [3, 8],
    [8, 3], [9, 3],
    [19, 3], [20, 3],
    [30, 3], [31, 3],
    [8, 17], [9, 17],
    [20, 17], [21, 17],
    [30, 17], [31, 17],
  ];
  for (const [tx, ty] of plateauTrees) {
    if (tiles[ty]?.[tx] === TileType.Grass) setTile(tiles, tx, ty, TileType.Tree);
  }

  // --- Flowers ---
  const flowers = [
    [6, 8], [7, 8], [6, 9], [7, 9],
    [20, 8], [21, 8], [20, 9], [21, 9],
    [6, 20], [7, 20],
    [28, 20], [29, 20],
  ];
  for (const [fx, fy] of flowers) {
    if (tiles[fy]?.[fx] === TileType.Grass) setTile(tiles, fx, fy, TileType.Flower);
  }

  return {
    id: 'CIEL_HAUT',
    name: 'CIEL-HAUT',
    width: W,
    height: H,
    tiles,
    collisionTiles: [TileType.Water, TileType.Tree, TileType.Wall, TileType.Roof, TileType.Fence, TileType.Sign, TileType.Lava],
    encounterTiles: [],
    encounterRate: 0,
    encounters: [],
    npcs: [
      {
        id: 'ciel_nurse',
        name: 'INFIRMIERE',
        x: 4, y: 7,
        spriteType: 'nurse',
        dialogue: [
          'Bienvenue au Centre Dino de Ciel-Haut !',
          'L\'air pur du plateau fait des merveilles pour tes dinos.',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'ciel_shopkeeper',
        name: 'VENDEUR',
        x: 13, y: 7,
        spriteType: 'shopkeeper',
        dialogue: [
          'C\'est le dernier magasin avant la Route de la Victoire.',
          'Fais le plein de tout ! Ultra Balls, Rappels Max, Guerisons...',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'ciel_gymguide',
        name: 'GUIDE ARENA',
        x: 24, y: 8,
        spriteType: 'villager',
        dialogue: [
          'L\'arene de Ciel-Haut est de type Vol.',
          'Celesta, la championne, danse avec le vent.',
          'Les dinos Electrique, Glace ou Roche dominent le Vol !',
        ],
        facing: 'left',
        movement: 'static',
      },
      {
        id: 'ciel_monk',
        name: 'MOINE AETHER',
        x: 14, y: 20,
        spriteType: 'villager',
        dialogue: [
          'Je suis le Moine Aether, gardien du Temple du Ciel.',
          'Les anciens croyaient qu\'un dino divin planait au-dessus des nuages.',
          'L\'equilibre entre ciel et terre maintient le monde en paix.',
        ],
        facing: 'up',
        movement: 'static',
      },
      {
        id: 'ciel_elder',
        name: 'ANCIEN',
        x: 16, y: 12,
        spriteType: 'villager',
        dialogue: [
          'Jeune dresseur, si tu as les huit badges, la Ligue t\'attend.',
          'La Route 9 est la Route de la Victoire. Seuls les meilleurs la traversent.',
          'Paleo-Capital est au bout du chemin. Bonne chance.',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'ciel_villager1',
        name: 'VILLAGEOIS',
        x: 8, y: 13,
        spriteType: 'villager',
        dialogue: [
          'Ciel-Haut est si haute qu\'on voit tout le continent depuis le temple.',
          'Par temps clair, on apercoit meme Paleo-Capital au loin.',
        ],
        facing: 'right',
        movement: 'wander',
      },
      {
        id: 'ciel_villager2',
        name: 'VILLAGEOISE',
        x: 28, y: 13,
        spriteType: 'villager',
        dialogue: [
          'Les huit badges brillent comme des etoiles dans le ciel.',
          'Tu les as tous ? Alors fonce vers la Ligue !',
        ],
        facing: 'left',
        movement: 'static',
      },
    ],
    warps: [
      // West -> Route 8
      { x: 0, y: 12, targetMap: 'ROUTE_8', targetX: 48, targetY: 9 },
      { x: 0, y: 13, targetMap: 'ROUTE_8', targetX: 48, targetY: 10 },
      // East -> Route 9
      { x: W - 1, y: 12, targetMap: 'ROUTE_9', targetX: 1, targetY: 14 },
      { x: W - 1, y: 13, targetMap: 'ROUTE_9', targetX: 1, targetY: 15 },
    ],
    buildings: [
      { id: 'ciel_dino_center', type: 'dino_center', doorX: 5, doorY: 6, signX: 2, signY: 6, signText: 'CENTRE DINO de Ciel-Haut — Soins gratuits pour vos dinos' },
      { id: 'ciel_shop', type: 'shop', doorX: 14, doorY: 6, signX: 11, signY: 6, signText: 'BOUTIQUE Ciel-Haut — Potions, Balls et plus !' },
      { id: 'ciel_gym', type: 'gym', doorX: 25, doorY: 7, signX: 21, signY: 7, signText: 'ARENE de Ciel-Haut — Championne : CELESTA — Type : Vol', sceneData: { gymId: 7 } },
      { id: 'ciel_temple', type: 'house', doorX: 15, doorY: 19, signX: 11, signY: 19, signText: 'TEMPLE DU CIEL — Observatoire et lieu de meditation' },
      { id: 'ciel_house1', type: 'house', doorX: 5, doorY: 18, signX: -1, signY: -1, signText: '' },
      { id: 'ciel_house2', type: 'house', doorX: 27, doorY: 18, signX: -1, signY: -1, signText: '' },
    ],
    music: 'sky_theme',
    terrain: 'grass',
  };
}

// ============================================================
// MAP 18: ROUTE_9 — Victory Road (40x30)
// ============================================================
function createRoute9(): MapData {
  const W = 40, H = 30;
  const tiles = fillGrid(W, H, TileType.Grass);

  // --- Top boundary ---
  setRect(tiles, 0, 0, W, 2, TileType.Tree);

  // --- Bottom boundary ---
  setRect(tiles, 0, H - 2, W, 2, TileType.Tree);

  // --- Left boundary (entry from Ciel-Haut) ---
  for (let y = 0; y < H; y++) {
    if (y < 14 || y > 15) setTile(tiles, 0, y, TileType.Tree);
  }

  // --- Right boundary (exit to Paleo-Capital) ---
  for (let y = 0; y < H; y++) {
    if (y < 14 || y > 15) {
      setTile(tiles, W - 1, y, TileType.Tree);
    }
  }

  // --- Main horizontal path (winding) ---
  for (let x = 0; x < 10; x++) {
    setTile(tiles, x, 14, TileType.Path);
    setTile(tiles, x, 15, TileType.Path);
  }
  for (let y = 6; y <= 14; y++) {
    setTile(tiles, 10, y, TileType.Path);
    setTile(tiles, 11, y, TileType.Path);
  }
  for (let x = 10; x <= 22; x++) {
    setTile(tiles, x, 6, TileType.Path);
    setTile(tiles, x, 7, TileType.Path);
  }
  for (let y = 6; y <= 22; y++) {
    setTile(tiles, 22, y, TileType.Path);
    setTile(tiles, 23, y, TileType.Path);
  }
  for (let x = 22; x < W; x++) {
    setTile(tiles, x, 22, TileType.Path);
    setTile(tiles, x, 23, TileType.Path);
  }
  for (let y = 14; y <= 22; y++) {
    setTile(tiles, 35, y, TileType.Path);
    setTile(tiles, 36, y, TileType.Path);
  }
  for (let x = 35; x < W; x++) {
    setTile(tiles, x, 14, TileType.Path);
    setTile(tiles, x, 15, TileType.Path);
  }

  // --- Rocky sections ---
  setRect(tiles, 5, 4, 3, 3, TileType.Sand);
  setRect(tiles, 15, 10, 4, 3, TileType.Sand);
  setRect(tiles, 28, 4, 4, 4, TileType.Sand);
  setRect(tiles, 28, 17, 3, 3, TileType.Sand);

  // --- Water crossing ---
  setRect(tiles, 14, 18, 5, 3, TileType.Water);
  setTile(tiles, 16, 18, TileType.Bridge);
  setTile(tiles, 16, 19, TileType.Bridge);
  setTile(tiles, 16, 20, TileType.Bridge);

  // --- Ledges ---
  for (let x = 3; x <= 8; x++) setTile(tiles, x, 10, TileType.Ledge);
  for (let x = 25; x <= 30; x++) setTile(tiles, x, 10, TileType.Ledge);

  // --- Dense tall grass ---
  for (let y = 3; y <= 5; y++) {
    for (let x = 2; x <= 5; x++) {
      if (tiles[y][x] === TileType.Grass) setTile(tiles, x, y, TileType.TallGrass);
    }
  }
  for (let y = 9; y <= 13; y++) {
    for (let x = 13; x <= 17; x++) {
      if (tiles[y][x] === TileType.Grass) setTile(tiles, x, y, TileType.TallGrass);
    }
  }
  for (let y = 16; y <= 20; y++) {
    for (let x = 3; x <= 8; x++) {
      if (tiles[y][x] === TileType.Grass) setTile(tiles, x, y, TileType.TallGrass);
    }
  }
  for (let y = 8; y <= 12; y++) {
    for (let x = 25; x <= 29; x++) {
      if (tiles[y][x] === TileType.Grass) setTile(tiles, x, y, TileType.TallGrass);
    }
  }
  for (let y = 24; y <= 26; y++) {
    for (let x = 26; x <= 32; x++) {
      if (tiles[y][x] === TileType.Grass) setTile(tiles, x, y, TileType.TallGrass);
    }
  }

  // --- Trees/boulders ---
  const r9Trees = [
    [2, 8], [3, 8],
    [8, 3], [9, 3],
    [13, 3], [14, 3],
    [18, 12], [19, 12],
    [25, 3], [26, 3], [27, 3],
    [33, 8], [34, 8], [33, 9],
    [3, 22], [4, 22], [3, 23],
    [10, 22], [11, 22],
    [33, 25], [34, 25],
    [36, 3], [37, 3],
  ];
  for (const [tx, ty] of r9Trees) {
    if (tiles[ty]?.[tx] === TileType.Grass || tiles[ty]?.[tx] === TileType.Sand) {
      setTile(tiles, tx, ty, TileType.Tree);
    }
  }

  // --- Signs ---
  setTile(tiles, 2, 13, TileType.Sign);
  setTile(tiles, 37, 13, TileType.Sign);

  return {
    id: 'ROUTE_9',
    name: 'ROUTE 9',
    width: W,
    height: H,
    tiles,
    collisionTiles: [TileType.Water, TileType.Tree, TileType.Wall, TileType.Roof, TileType.Fence, TileType.Sign, TileType.Lava],
    encounterTiles: [TileType.TallGrass],
    encounterRate: 15,
    encounters: [
      { dinoId: 85, name: 'Titanowl', minLevel: 30, maxLevel: 34, weight: 10 },
      { dinoId: 88, name: 'Peakfang', minLevel: 31, maxLevel: 35, weight: 10 },
      { dinoId: 90, name: 'Windtalon', minLevel: 32, maxLevel: 36, weight: 10 },
      { dinoId: 92, name: 'Ironhide', minLevel: 31, maxLevel: 35, weight: 10 },
      { dinoId: 95, name: 'Fossilwyrm', minLevel: 32, maxLevel: 36, weight: 10 },
      { dinoId: 98, name: 'Obsidianex', minLevel: 33, maxLevel: 37, weight: 10 },
      { dinoId: 100, name: 'Primalord', minLevel: 34, maxLevel: 38, weight: 10 },
      { dinoId: 102, name: 'Archosaur', minLevel: 33, maxLevel: 37, weight: 10 },
      { dinoId: 105, name: 'Dracovex', minLevel: 34, maxLevel: 38, weight: 10 },
      { dinoId: 110, name: 'Extinctus', minLevel: 35, maxLevel: 38, weight: 10 },
    ],
    npcs: [
      {
        id: 'route9_trainer1',
        name: 'DRESSEUR ARTHUR',
        x: 5, y: 14,
        spriteType: 'trainer',
        dialogue: [
          'La Route de la Victoire ! Seuls les meilleurs arrivent ici.',
          'Mes dinos sont au sommet de leur puissance. Bats-les si tu peux !',
        ],
        facing: 'right',
        movement: 'static',
      },
      {
        id: 'route9_trainer2',
        name: 'DRESSEUSE HELENE',
        x: 11, y: 6,
        spriteType: 'trainer',
        dialogue: [
          'Ce chemin est un labyrinthe. Beaucoup se perdent.',
          'Mais toi, tu as l\'air determine. Montre-moi ta force !',
        ],
        facing: 'right',
        movement: 'static',
      },
      {
        id: 'route9_trainer3',
        name: 'VETERANE JEANNE',
        x: 22, y: 14,
        spriteType: 'trainer',
        dialogue: [
          'J\'ai battu les huit gyms il y a vingt ans.',
          'La Ligue m\'attend encore. Mais d\'abord, combattons !',
          'Montre-moi que la nouvelle generation est digne.',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'route9_trainer4',
        name: 'DRESSEUR RAPHAEL',
        x: 30, y: 22,
        spriteType: 'trainer',
        dialogue: [
          'Paleo-Capital est si pres ! Je peux presque la voir.',
          'Un dernier combat avant la gloire !',
        ],
        facing: 'left',
        movement: 'static',
      },
      {
        id: 'route9_trainer5',
        name: 'CHAMPIONNE EMERITE LUNA',
        x: 36, y: 14,
        spriteType: 'trainer',
        dialogue: [
          'Je suis Luna, ancienne championne de la Ligue.',
          'Si tu me bats, tu es pret pour le Conseil des 4.',
          'Ne recule devant rien. Tes dinos croient en toi.',
        ],
        facing: 'left',
        movement: 'static',
      },
    ],
    warps: [
      // West -> Ciel-Haut
      { x: 0, y: 14, targetMap: 'CIEL_HAUT', targetX: 33, targetY: 12 },
      { x: 0, y: 15, targetMap: 'CIEL_HAUT', targetX: 33, targetY: 13 },
      // East -> Paleo-Capital
      { x: W - 1, y: 14, targetMap: 'PALEO_CAPITAL', targetX: 1, targetY: 17 },
      { x: W - 1, y: 15, targetMap: 'PALEO_CAPITAL', targetX: 1, targetY: 18 },
    ],
    music: 'victory_road_theme',
    terrain: 'rock',
  };
}

// ============================================================
// MAP 19: PALEO_CAPITAL — Capital City + League (45x35)
// ============================================================
function createPaleoCapital(): MapData {
  const W = 45, H = 35;
  const tiles = fillGrid(W, H, TileType.Path);

  // --- Top boundary ---
  setRect(tiles, 0, 0, W, 2, TileType.Tree);

  // --- Bottom boundary ---
  setRect(tiles, 0, H - 2, W, 2, TileType.Tree);

  // --- Left boundary (entry from Route 9) ---
  for (let y = 0; y < H; y++) {
    if (y < 17 || y > 18) setTile(tiles, 0, y, TileType.Tree);
  }

  // --- Right boundary ---
  for (let y = 0; y < H; y++) {
    setTile(tiles, W - 1, y, TileType.Tree);
    setTile(tiles, W - 2, y, TileType.Tree);
  }

  // --- Wide boulevards ---
  for (let x = 0; x < W; x++) {
    setTile(tiles, x, 17, TileType.Path);
    setTile(tiles, x, 18, TileType.Path);
  }
  for (let y = 2; y < H - 2; y++) {
    setTile(tiles, 22, y, TileType.Path);
    setTile(tiles, 23, y, TileType.Path);
  }

  // --- Grass areas (parks and gardens) ---
  setRect(tiles, 3, 8, 6, 5, TileType.Grass);
  setRect(tiles, 35, 8, 5, 5, TileType.Grass);
  setRect(tiles, 3, 24, 6, 5, TileType.Grass);
  setRect(tiles, 35, 24, 5, 5, TileType.Grass);

  // --- Dino Center at (3, 3) ---
  buildBuilding(tiles, 3, 3, 5, 3, 5, 5);
  setTile(tiles, 2, 5, TileType.Sign);

  // --- Boutique (best items) at (10, 3) ---
  buildBuilding(tiles, 10, 3, 5, 3, 12, 5);
  setTile(tiles, 9, 5, TileType.Sign);

  // --- Pokemon League Building (grand, large) at (17, 3) ---
  buildBuilding(tiles, 17, 3, 10, 5, 22, 7);
  setTile(tiles, 16, 7, TileType.Sign);

  // --- Museum at (3, 20) ---
  buildBuilding(tiles, 3, 20, 6, 3, 6, 22);
  setTile(tiles, 2, 22, TileType.Sign);

  // --- Trainer Hall of Fame at (33, 3) ---
  buildBuilding(tiles, 33, 3, 6, 4, 36, 6);
  setTile(tiles, 32, 6, TileType.Sign);

  // --- Houses ---
  buildBuilding(tiles, 10, 20, 4, 3, 12, 22);
  buildBuilding(tiles, 33, 20, 4, 3, 35, 22);
  buildBuilding(tiles, 10, 27, 4, 3, 12, 29);
  buildBuilding(tiles, 33, 27, 4, 3, 35, 29);

  // --- Decorative trees and flowers ---
  const parkTrees = [
    [4, 9], [5, 9], [7, 9], [8, 9],
    [4, 11], [5, 11], [7, 11], [8, 11],
    [36, 9], [37, 9], [38, 9],
    [36, 11], [37, 11], [38, 11],
    [4, 25], [5, 25], [7, 25], [8, 25],
    [4, 27], [5, 27], [7, 27], [8, 27],
    [36, 25], [37, 25], [38, 25],
    [36, 27], [37, 27], [38, 27],
  ];
  for (const [tx, ty] of parkTrees) {
    if (tiles[ty]?.[tx] === TileType.Grass) setTile(tiles, tx, ty, TileType.Tree);
  }

  const parkFlowers = [
    [4, 10], [5, 10], [7, 10], [8, 10],
    [36, 10], [37, 10], [38, 10],
    [4, 26], [5, 26], [7, 26], [8, 26],
    [36, 26], [37, 26], [38, 26],
  ];
  for (const [fx, fy] of parkFlowers) {
    if (tiles[fy]?.[fx] === TileType.Grass) setTile(tiles, fx, fy, TileType.Flower);
  }

  // --- Fences around parks ---
  for (let x = 2; x <= 9; x++) {
    setTile(tiles, x, 7, TileType.Fence);
    setTile(tiles, x, 13, TileType.Fence);
  }
  for (let x = 34; x <= 40; x++) {
    setTile(tiles, x, 7, TileType.Fence);
    setTile(tiles, x, 13, TileType.Fence);
  }

  return {
    id: 'PALEO_CAPITAL',
    name: 'PALEO-CAPITAL',
    width: W,
    height: H,
    tiles,
    collisionTiles: [TileType.Water, TileType.Tree, TileType.Wall, TileType.Roof, TileType.Fence, TileType.Sign, TileType.Lava],
    encounterTiles: [],
    encounterRate: 0,
    encounters: [],
    npcs: [
      {
        id: 'paleo_nurse',
        name: 'INFIRMIERE',
        x: 4, y: 6,
        spriteType: 'nurse',
        dialogue: [
          'Bienvenue au Centre Dino de Paleo-Capital !',
          'Soigne tes dinos avant d\'affronter le Conseil des 4.',
          'C\'est le dernier soin gratuit avant la Ligue !',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'paleo_shopkeeper',
        name: 'VENDEUR',
        x: 11, y: 6,
        spriteType: 'shopkeeper',
        dialogue: [
          'Les meilleurs objets du continent sont ici.',
          'Rappels Max, Guerisons, Master Balls en quantite limitee...',
          'Fais le plein, le Conseil des 4 ne pardonne pas.',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'paleo_leagueguard',
        name: 'GARDIEN DE LA LIGUE',
        x: 22, y: 8,
        spriteType: 'trainer',
        dialogue: [
          'Halte ! Seuls ceux qui possedent les huit badges peuvent entrer.',
          'Le Conseil des 4 et le Champion attendent a l\'interieur.',
          'Es-tu pret a devenir le Maitre des Dinos ?',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'paleo_museum',
        name: 'GUIDE DU MUSEE',
        x: 5, y: 23,
        spriteType: 'villager',
        dialogue: [
          'Le Musee de Paleo-Capital retrace l\'histoire des dinos.',
          'Des fossiles millenaires aux dinos modernes, tout est la.',
          'La Pierre d\'Extinction est notre piece maitresse... ou elle l\'etait.',
        ],
        facing: 'right',
        movement: 'static',
      },
      {
        id: 'paleo_villager1',
        name: 'CITADIN',
        x: 15, y: 17,
        spriteType: 'villager',
        dialogue: [
          'Paleo-Capital est le coeur du monde des dresseurs.',
          'Les meilleurs viennent ici pour defier la Ligue.',
        ],
        facing: 'right',
        movement: 'wander',
      },
      {
        id: 'paleo_villager2',
        name: 'CITADINE',
        x: 30, y: 18,
        spriteType: 'villager',
        dialogue: [
          'Le Temple de la Renommee garde les noms de tous les Champions.',
          'Un jour, ton nom y sera peut-etre inscrit !',
        ],
        facing: 'left',
        movement: 'static',
      },
      {
        id: 'paleo_villager3',
        name: 'HISTORIEN',
        x: 18, y: 17,
        spriteType: 'villager',
        dialogue: [
          'Les anciens disaient que les dinos et les humains partageaient le meme ancetre.',
          'La science moderne a confirme ce lien unique.',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'paleo_villager4',
        name: 'VETERAN',
        x: 35, y: 7,
        spriteType: 'trainer',
        dialogue: [
          'J\'ai battu la Ligue il y a dix ans. Le sentiment est indescriptible.',
          'Donne tout ce que tu as. Tes dinos comptent sur toi.',
        ],
        facing: 'down',
        movement: 'static',
      },
    ],
    warps: [
      // West -> Route 9
      { x: 0, y: 17, targetMap: 'ROUTE_9', targetX: 38, targetY: 14 },
      { x: 0, y: 18, targetMap: 'ROUTE_9', targetX: 38, targetY: 15 },
    ],
    buildings: [
      { id: 'paleo_dino_center', type: 'dino_center', doorX: 5, doorY: 5, signX: 2, signY: 5, signText: 'CENTRE DINO de Paleo-Capital — Soins gratuits pour vos dinos' },
      { id: 'paleo_shop', type: 'shop', doorX: 12, doorY: 5, signX: 9, signY: 5, signText: 'BOUTIQUE Paleo-Capital — Potions, Balls et plus !' },
      { id: 'paleo_league', type: 'house', doorX: 22, doorY: 7, signX: 16, signY: 7, signText: 'LIGUE DINO — Conseil des 4 et Champion — Huit badges requis' },
      { id: 'paleo_museum', type: 'house', doorX: 6, doorY: 22, signX: 2, signY: 22, signText: 'MUSEE DE PALEO-CAPITAL — Histoire des dinos a travers les ages' },
      { id: 'paleo_hall_of_fame', type: 'house', doorX: 36, doorY: 6, signX: 32, signY: 6, signText: 'TEMPLE DE LA RENOMMEE — Les plus grands Champions' },
      { id: 'paleo_house1', type: 'house', doorX: 12, doorY: 22, signX: -1, signY: -1, signText: '' },
      { id: 'paleo_house2', type: 'house', doorX: 35, doorY: 22, signX: -1, signY: -1, signText: '' },
      { id: 'paleo_house3', type: 'house', doorX: 12, doorY: 29, signX: -1, signY: -1, signText: '' },
      { id: 'paleo_house4', type: 'house', doorX: 35, doorY: 29, signX: -1, signY: -1, signText: '' },
    ],
    music: 'capital_theme',
    terrain: 'grass',
  };
}

// ============================================================
// MAP 20: GROTTE_ANCETRES — Dungeon Cave (25x30)
// ============================================================
function createGrotteAncetres(): MapData {
  const W = 25, H = 30;
  const tiles = fillGrid(W, H, TileType.Wall);

  // --- Carve out winding cave path ---
  // Main passage
  for (let y = 1; y <= 5; y++) {
    for (let x = 10; x <= 14; x++) setTile(tiles, x, y, TileType.Path);
  }
  for (let x = 5; x <= 14; x++) {
    setTile(tiles, x, 5, TileType.Path);
    setTile(tiles, x, 6, TileType.Path);
  }
  for (let y = 6; y <= 12; y++) {
    setTile(tiles, 5, y, TileType.Path);
    setTile(tiles, 6, y, TileType.Path);
  }
  for (let x = 5; x <= 18; x++) {
    setTile(tiles, x, 12, TileType.Path);
    setTile(tiles, x, 13, TileType.Path);
  }
  for (let y = 13; y <= 19; y++) {
    setTile(tiles, 17, y, TileType.Path);
    setTile(tiles, 18, y, TileType.Path);
  }
  for (let x = 8; x <= 18; x++) {
    setTile(tiles, x, 19, TileType.Path);
    setTile(tiles, x, 20, TileType.Path);
  }
  for (let y = 20; y <= 26; y++) {
    setTile(tiles, 8, y, TileType.Path);
    setTile(tiles, 9, y, TileType.Path);
  }
  for (let x = 8; x <= 16; x++) {
    setTile(tiles, x, 26, TileType.Path);
    setTile(tiles, x, 27, TileType.Path);
  }

  // --- Wider chambers ---
  // Chamber 1 (near entrance)
  setRect(tiles, 9, 3, 4, 3, TileType.Path);
  // Chamber 2 (middle)
  setRect(tiles, 3, 9, 5, 4, TileType.Path);
  // Chamber 3 (right)
  setRect(tiles, 15, 10, 5, 5, TileType.Path);
  // Chamber 4 (bottom)
  setRect(tiles, 11, 23, 5, 5, TileType.Path);

  // --- Water pools ---
  setRect(tiles, 4, 10, 2, 2, TileType.Water);
  setRect(tiles, 16, 11, 2, 2, TileType.Water);
  setRect(tiles, 12, 24, 2, 2, TileType.Water);

  // --- Encounter tiles (rocky ground in chambers) ---
  setRect(tiles, 10, 4, 2, 1, TileType.TallGrass);
  setRect(tiles, 5, 10, 2, 2, TileType.TallGrass);
  setRect(tiles, 18, 11, 1, 2, TileType.TallGrass);
  setRect(tiles, 14, 24, 1, 2, TileType.TallGrass);
  setRect(tiles, 9, 25, 2, 2, TileType.TallGrass);

  // --- Exit door (top, connects to Route 2) ---
  setTile(tiles, 12, 0, TileType.Door);

  return {
    id: 'GROTTE_ANCETRES',
    name: 'GROTTE DES ANCETRES',
    width: W,
    height: H,
    tiles,
    collisionTiles: [TileType.Water, TileType.Tree, TileType.Wall, TileType.Roof, TileType.Fence, TileType.Sign, TileType.Lava],
    encounterTiles: [TileType.TallGrass],
    encounterRate: 15,
    encounters: [
      { dinoId: 95, name: 'Fossilwyrm', minLevel: 15, maxLevel: 20, weight: 15 },
      { dinoId: 98, name: 'Obsidianex', minLevel: 16, maxLevel: 21, weight: 15 },
      { dinoId: 100, name: 'Primalord', minLevel: 17, maxLevel: 22, weight: 10 },
      { dinoId: 102, name: 'Archosaur', minLevel: 18, maxLevel: 23, weight: 10 },
      { dinoId: 105, name: 'Dracovex', minLevel: 19, maxLevel: 24, weight: 10 },
      { dinoId: 108, name: 'Bonecrusher', minLevel: 16, maxLevel: 21, weight: 15 },
      { dinoId: 110, name: 'Extinctus', minLevel: 20, maxLevel: 25, weight: 5 },
      { dinoId: 115, name: 'Crystadon', minLevel: 17, maxLevel: 22, weight: 10 },
      { dinoId: 120, name: 'Ancienthra', minLevel: 20, maxLevel: 25, weight: 10 },
    ],
    npcs: [
      {
        id: 'grotte_grunt1',
        name: 'GRUNT METEORE',
        x: 6, y: 12,
        spriteType: 'grunt',
        dialogue: [
          'Comment tu es entre ici ?! Cette grotte appartient a l\'Escadron Meteore !',
          'On cherche les peintures anciennes. Elles revelent un secret...',
          'Tu ne sortiras pas d\'ici aussi facilement !',
        ],
        facing: 'right',
        movement: 'static',
      },
      {
        id: 'grotte_grunt2',
        name: 'GRUNT METEORE',
        x: 17, y: 19,
        spriteType: 'grunt',
        dialogue: [
          'Le Lieutenant m\'a ordonne de garder ce passage.',
          'Les peintures montrent un dino capable de detruire le monde...',
          'L\'Escadron Meteore le reveillera, et rien ne pourra l\'arreter !',
        ],
        facing: 'up',
        movement: 'static',
      },
    ],
    warps: [
      // Exit -> Route 2 (cave entrance)
      { x: 12, y: 0, targetMap: 'ROUTE_2', targetX: 21, targetY: 4 },
    ],
    music: 'cave_theme',
    terrain: 'cave',
  };
}

// ============================================================
// MAP 21: TOUR_FOSSILES — Escadron Meteore HQ Tower (20x40)
// ============================================================
function createTourFossiles(): MapData {
  const W = 20, H = 40;
  const tiles = fillGrid(W, H, TileType.Wall);

  // --- Floor 1 (bottom, entrance) y=35..39 ---
  setRect(tiles, 2, 36, 16, 3, TileType.Path);
  setTile(tiles, 10, 39, TileType.Door);
  // Stairs up (right side)
  setTile(tiles, 16, 36, TileType.Door);

  // --- Floor 2 y=28..34 ---
  setRect(tiles, 2, 29, 16, 5, TileType.Path);
  // Stairs from floor 1
  setTile(tiles, 16, 33, TileType.Door);
  // Stairs up
  setTile(tiles, 3, 29, TileType.Door);

  // --- Floor 3 y=21..27 ---
  setRect(tiles, 2, 22, 16, 5, TileType.Path);
  // Stairs from floor 2
  setTile(tiles, 3, 26, TileType.Door);
  // Stairs up
  setTile(tiles, 16, 22, TileType.Door);

  // --- Floor 4 y=14..20 ---
  setRect(tiles, 2, 15, 16, 5, TileType.Path);
  // Stairs from floor 3
  setTile(tiles, 16, 19, TileType.Door);
  // Stairs up
  setTile(tiles, 3, 15, TileType.Door);

  // --- Floor 5 (top, lieutenant) y=7..13 ---
  setRect(tiles, 2, 8, 16, 5, TileType.Path);
  // Stairs from floor 4
  setTile(tiles, 3, 12, TileType.Door);
  // Boss room
  setRect(tiles, 6, 3, 8, 4, TileType.Path);
  for (let y = 3; y <= 8; y++) {
    setTile(tiles, 10, y, TileType.Path);
  }

  // --- Decorative walls between floors ---
  for (let x = 0; x < W; x++) {
    setTile(tiles, x, 35, TileType.Fence);
    setTile(tiles, x, 28, TileType.Fence);
    setTile(tiles, x, 21, TileType.Fence);
    setTile(tiles, x, 14, TileType.Fence);
    setTile(tiles, x, 7, TileType.Fence);
  }
  // Keep stair doors open through fences
  setTile(tiles, 16, 35, TileType.Path);
  setTile(tiles, 16, 28, TileType.Path);
  setTile(tiles, 3, 28, TileType.Path);
  setTile(tiles, 3, 21, TileType.Path);
  setTile(tiles, 16, 21, TileType.Path);
  setTile(tiles, 16, 14, TileType.Path);
  setTile(tiles, 3, 14, TileType.Path);
  setTile(tiles, 3, 7, TileType.Path);

  return {
    id: 'TOUR_FOSSILES',
    name: 'TOUR DES FOSSILES',
    width: W,
    height: H,
    tiles,
    collisionTiles: [TileType.Water, TileType.Tree, TileType.Wall, TileType.Roof, TileType.Fence, TileType.Sign, TileType.Lava],
    encounterTiles: [],
    encounterRate: 0,
    encounters: [],
    npcs: [
      {
        id: 'tour_grunt1',
        name: 'GRUNT METEORE',
        x: 8, y: 37,
        spriteType: 'grunt',
        dialogue: [
          'Qui ose penetrer dans notre QG ?!',
          'L\'Escadron Meteore te fera regretter d\'etre venu !',
        ],
        facing: 'left',
        movement: 'static',
      },
      {
        id: 'tour_grunt2',
        name: 'GRUNT METEORE',
        x: 10, y: 31,
        spriteType: 'grunt',
        dialogue: [
          'Tu es monte jusqu\'ici ? Pas mal pour un gamin.',
          'Mais les vrais combats commencent maintenant !',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'tour_grunt3',
        name: 'GRUNT METEORE',
        x: 8, y: 24,
        spriteType: 'grunt',
        dialogue: [
          'Le Lieutenant est au sommet. Tu ne l\'atteindras jamais.',
          'Nos machines vont bientot reveiller le dino ancien !',
        ],
        facing: 'right',
        movement: 'static',
      },
      {
        id: 'tour_grunt4',
        name: 'GRUNT METEORE',
        x: 10, y: 17,
        spriteType: 'grunt',
        dialogue: [
          'Dernier etage avant le sommet. Arrete-toi ici !',
          'Le plan du Lieutenant est presque acheve. Tu arrives trop tard !',
        ],
        facing: 'down',
        movement: 'static',
      },
      {
        id: 'tour_lieutenant',
        name: 'LIEUTENANT STYX',
        x: 10, y: 4,
        spriteType: 'grunt',
        dialogue: [
          'Je suis le Lieutenant Styx de l\'Escadron Meteore.',
          'Tu as fait tout ce chemin pour rien, jeune dresseur.',
          'La Pierre d\'Extinction est en notre possession. Le dino ancien va se reveiller !',
        ],
        facing: 'down',
        movement: 'static',
      },
    ],
    warps: [
      // Exit -> Route 7 area
      { x: 10, y: 39, targetMap: 'ROUTE_7', targetX: 14, targetY: 20 },
    ],
    music: 'evil_theme',
    terrain: 'cave',
  };
}

// ============================================================
// ALL MAPS registry
// ============================================================
const BOURG_NID = createBourgNid();
const ROUTE_1 = createRoute1();
const VILLE_FOUGERE = createVilleFougere();
const ROUTE_2 = createRoute2();
const PORT_COQUILLE = createPortCoquille();
const ROUTE_3 = createRoute3();
const ROCHE_HAUTE = createRocheHaute();
const ROUTE_4 = createRoute4();
const VOLCANVILLE = createVolcanville();
const ROUTE_5 = createRoute5();
const CRYO_CITE = createCryoCite();
const ROUTE_6 = createRoute6();
const ELECTROPOLIS = createElectropolis();
const ROUTE_7 = createRoute7();
const MARAIS_NOIR = createMaraisNoir();
const ROUTE_8 = createRoute8();
const CIEL_HAUT = createCielHaut();
const ROUTE_9 = createRoute9();
const PALEO_CAPITAL = createPaleoCapital();
const GROTTE_ANCETRES = createGrotteAncetres();
const TOUR_FOSSILES = createTourFossiles();

export const ALL_MAPS: Record<string, MapData> = {
  BOURG_NID,
  ROUTE_1,
  VILLE_FOUGERE,
  ROUTE_2,
  PORT_COQUILLE,
  ROUTE_3,
  ROCHE_HAUTE,
  ROUTE_4,
  VOLCANVILLE,
  ROUTE_5,
  CRYO_CITE,
  ROUTE_6,
  ELECTROPOLIS,
  ROUTE_7,
  MARAIS_NOIR,
  ROUTE_8,
  CIEL_HAUT,
  ROUTE_9,
  PALEO_CAPITAL,
  GROTTE_ANCETRES,
  TOUR_FOSSILES,
};

// Restore all doors that may have been overwritten by paths
for (const map of Object.values(ALL_MAPS)) {
  restoreDoors(map.tiles, map.buildings);
}

export function getMap(id: string): MapData {
  const map = ALL_MAPS[id];
  if (!map) {
    throw new Error(`Map not found: ${id}`);
  }
  return map;
}

export function getMapList(): { id: string; name: string }[] {
  return Object.values(ALL_MAPS).map(m => ({ id: m.id, name: m.name }));
}
