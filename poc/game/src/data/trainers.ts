// ============================================================
// Jurassic Trainers — Complete Trainer Database
// All trainers: route, gym leaders, Elite 4, Champion, Rival, Escadron Meteore
// ============================================================

import { DinoType } from '../utils/constants';

// --- Interfaces ---

export interface TrainerMove {
  name: string;
  type: number;
  category: string;
  power: number;
  pp: number;
  maxPp: number;
}

export interface TrainerDino {
  speciesId: number;
  name: string;
  level: number;
  types: number[];
  moves: TrainerMove[];
}

export interface TrainerData {
  id: string;
  name: string;
  trainerClass: string;
  spriteType: string;
  dialogue: { before: string; after: string };
  party: TrainerDino[];
  reward: number;
  rematchable?: boolean;
  badge?: number;
  tmReward?: { moveId: number; name: string };
}

// --- Helper: quick move builder ---
function mv(name: string, type: number, category: string, power: number, pp: number): TrainerMove {
  return { name, type, category, power, pp, maxPp: pp };
}

// --- Helper: quick dino builder ---
function dino(speciesId: number, name: string, level: number, types: number[], moves: TrainerMove[]): TrainerDino {
  return { speciesId, name, level, types, moves };
}

// ============================================================
// Common move pool (reused across many trainers)
// ============================================================
const MOVE = {
  // Normal
  Charge: (pp = 35) => mv('Charge', DinoType.Normal, 'physical', 40, pp),
  Morsure: (pp = 30) => mv('Morsure', DinoType.Normal, 'physical', 35, pp),
  Grondement: (pp = 40) => mv('Grondement', DinoType.Normal, 'status', 0, pp),
  Rugissement: (pp = 30) => mv('Rugissement', DinoType.Normal, 'status', 0, pp),
  PlaquageSauvage: (pp = 20) => mv('Plaquage Sauvage', DinoType.Normal, 'physical', 60, pp),
  HyperVoix: (pp = 10) => mv('Hyper Voix', DinoType.Normal, 'special', 90, pp),
  // Fire
  Flammeche: (pp = 25) => mv('Flammeche', DinoType.Fire, 'special', 40, pp),
  LanceFlammes: (pp = 15) => mv('Lance-Flammes', DinoType.Fire, 'special', 90, pp),
  Deflagration: (pp = 5) => mv('Deflagration', DinoType.Fire, 'special', 110, pp),
  RoueDeFeu: (pp = 20) => mv('Roue de Feu', DinoType.Fire, 'physical', 60, pp),
  // Water
  PistoletAEau: (pp = 25) => mv('Pistolet a Eau', DinoType.Water, 'special', 40, pp),
  SurfGlacial: (pp = 15) => mv('Surf Glacial', DinoType.Water, 'special', 90, pp),
  Hydrocanon: (pp = 5) => mv('Hydrocanon', DinoType.Water, 'special', 110, pp),
  AquaJet: (pp = 20) => mv('Aqua Jet', DinoType.Water, 'physical', 40, pp),
  BullesDEau: (pp = 20) => mv("Bulles d'Eau", DinoType.Water, 'special', 65, pp),
  // Flora
  FouetLiane: (pp = 25) => mv('Fouet Liane', DinoType.Flora, 'physical', 45, pp),
  TranchHerbe: (pp = 25) => mv("Tranch'Herbe", DinoType.Flora, 'physical', 55, pp),
  LameVegetale: (pp = 15) => mv('Lame Vegetale', DinoType.Flora, 'physical', 90, pp),
  PoudreToxik: (pp = 20) => mv('Poudre Toxik', DinoType.Venom, 'status', 0, pp),
  CanonGraine: (pp = 15) => mv('Canon Graine', DinoType.Flora, 'special', 80, pp),
  // Earth
  JetDeSable: (pp = 25) => mv('Jet de Sable', DinoType.Earth, 'status', 0, pp),
  Seisme: (pp = 10) => mv('Seisme', DinoType.Earth, 'physical', 100, pp),
  Eboulement: (pp = 15) => mv('Eboulement', DinoType.Fossil, 'physical', 75, pp),
  CoupDeBoue: (pp = 20) => mv('Coup de Boue', DinoType.Earth, 'special', 55, pp),
  TirDePierre: (pp = 20) => mv('Tir de Pierre', DinoType.Fossil, 'physical', 50, pp),
  // Electric
  Etincelle: (pp = 25) => mv('Etincelle', DinoType.Electric, 'special', 40, pp),
  Tonnerre: (pp = 15) => mv('Tonnerre', DinoType.Electric, 'special', 90, pp),
  Fatal_Foudre: (pp = 5) => mv('Fatal-Foudre', DinoType.Electric, 'special', 110, pp),
  OndeDeChoc: (pp = 20) => mv('Onde de Choc', DinoType.Electric, 'special', 60, pp),
  // Ice
  PoudreigeNeige: (pp = 25) => mv('Poudreige', DinoType.Ice, 'special', 40, pp),
  RayonGlace: (pp = 15) => mv('Rayon Glace', DinoType.Ice, 'special', 90, pp),
  Blizzard: (pp = 5) => mv('Blizzard', DinoType.Ice, 'special', 110, pp),
  SouffleGlace: (pp = 20) => mv('Souffle Glace', DinoType.Ice, 'special', 55, pp),
  // Venom
  DardVenin: (pp = 25) => mv('Dard-Venin', DinoType.Venom, 'physical', 40, pp),
  BombeAcide: (pp = 15) => mv('Bombe Acide', DinoType.Venom, 'special', 90, pp),
  CrocPoison: (pp = 15) => mv('Croc Poison', DinoType.Venom, 'physical', 50, pp),
  PureDeToxin: (pp = 10) => mv('Puree de Toxin', DinoType.Venom, 'special', 80, pp),
  // Air
  Tornade: (pp = 25) => mv('Tornade', DinoType.Air, 'special', 40, pp),
  LameDAir: (pp = 15) => mv("Lame d'Air", DinoType.Air, 'special', 75, pp),
  Aeroblast: (pp = 5) => mv('Aeroblast', DinoType.Air, 'special', 100, pp),
  Cru_Aile: (pp = 20) => mv("Cru-Aile", DinoType.Air, 'physical', 60, pp),
  // Shadow
  BallOmbre: (pp = 15) => mv("Ball'Ombre", DinoType.Shadow, 'special', 80, pp),
  GriffeOmbre: (pp = 20) => mv('Griffe Ombre', DinoType.Shadow, 'physical', 70, pp),
  PulsationNuit: (pp = 10) => mv('Pulsation Nuit', DinoType.Shadow, 'special', 85, pp),
  // Light
  RayonLumiere: (pp = 15) => mv('Rayon Lumiere', DinoType.Light, 'special', 80, pp),
  EclatSolaire: (pp = 10) => mv('Eclat Solaire', DinoType.Light, 'special', 90, pp),
  FlashLumineux: (pp = 20) => mv('Flash Lumineux', DinoType.Light, 'special', 65, pp),
  // Metal
  GriffeAcier: (pp = 20) => mv("Griffe d'Acier", DinoType.Metal, 'physical', 50, pp),
  QueueDeFer: (pp = 15) => mv('Queue de Fer', DinoType.Metal, 'physical', 100, pp),
  CanonAcier: (pp = 10) => mv('Canon Acier', DinoType.Metal, 'special', 80, pp),
  // Fossil
  PouvoirAntique: (pp = 10) => mv('Pouvoir Antique', DinoType.Fossil, 'special', 60, pp),
  FrappePrehistorique: (pp = 5) => mv('Frappe Prehistorique', DinoType.Fossil, 'physical', 100, pp),
  // Primal
  ForceOriginelle: (pp = 5) => mv('Force Originelle', DinoType.Primal, 'special', 120, pp),
  RugissementPrimal: (pp = 10) => mv('Rugissement Primal', DinoType.Primal, 'special', 80, pp),
};

// ============================================================
// ROUTE TRAINERS
// ============================================================

// --- Route 1 (Lv 3-7) ---
const ROUTE_1_TRAINERS: TrainerData[] = [
  {
    id: 'route1_ranger_marc',
    name: 'Ranger Marc',
    trainerClass: 'ranger',
    spriteType: 'trainer',
    dialogue: {
      before: "He ! Toi la-bas ! Voyons si tes dinos sont a la hauteur !",
      after: "Pas mal du tout ! Continue comme ca, petit.",
    },
    party: [
      dino(4, 'Compso', 4, [DinoType.Normal], [MOVE.Charge(), MOVE.Grondement()]),
      dino(2, 'Herbex', 5, [DinoType.Flora], [MOVE.Charge(), MOVE.FouetLiane()]),
    ],
    reward: 200,
  },
  {
    id: 'route1_scout_julie',
    name: 'Scout Julie',
    trainerClass: 'scout',
    spriteType: 'trainer',
    dialogue: {
      before: "J'explore cette route depuis ce matin ! Un combat pour se rechauffer ?",
      after: "Ouah, t'es vraiment fort ! Je dois encore m'entrainer...",
    },
    party: [
      dino(5, 'Pterodon', 5, [DinoType.Air], [MOVE.Charge(), MOVE.Tornade()]),
    ],
    reward: 150,
  },
  {
    id: 'route1_scout_thomas',
    name: 'Scout Thomas',
    trainerClass: 'scout',
    spriteType: 'trainer',
    dialogue: {
      before: "Mon Compso est le plus rapide de la route !",
      after: "D'accord... peut-etre pas le plus fort.",
    },
    party: [
      dino(4, 'Compso', 3, [DinoType.Normal], [MOVE.Charge()]),
      dino(4, 'Compso', 4, [DinoType.Normal], [MOVE.Charge(), MOVE.Morsure()]),
    ],
    reward: 120,
  },
];

// --- Route 2 (Lv 7-12) ---
const ROUTE_2_TRAINERS: TrainerData[] = [
  {
    id: 'route2_ranger_sophie',
    name: 'Ranger Sophie',
    trainerClass: 'ranger',
    spriteType: 'trainer',
    dialogue: {
      before: "Je protege cette foret et ses dinos. Prouve que tu les respectes !",
      after: "Je vois que tu prends soin de tes dinos. Bonne route !",
    },
    party: [
      dino(2, 'Herbex', 8, [DinoType.Flora], [MOVE.FouetLiane(), MOVE.Charge(), MOVE.PoudreToxik()]),
      dino(7, 'Terrapod', 9, [DinoType.Earth], [MOVE.Charge(), MOVE.JetDeSable(), MOVE.CoupDeBoue()]),
    ],
    reward: 350,
  },
  {
    id: 'route2_hiker_paul',
    name: 'Randonneur Paul',
    trainerClass: 'hiker',
    spriteType: 'trainer',
    dialogue: {
      before: "Rien de tel qu'un bon combat en pleine nature !",
      after: "Mes dinos sont peut-etre fatigues de la marche...",
    },
    party: [
      dino(7, 'Terrapod', 10, [DinoType.Earth], [MOVE.Charge(), MOVE.JetDeSable(), MOVE.TirDePierre()]),
    ],
    reward: 300,
  },
  {
    id: 'route2_scout_emma',
    name: 'Scout Emma',
    trainerClass: 'scout',
    spriteType: 'trainer',
    dialogue: {
      before: "J'ai attrape un nouveau dino ! Tu veux le voir en action ?",
      after: "Il a encore besoin d'entrainement, on dirait...",
    },
    party: [
      dino(5, 'Pterodon', 8, [DinoType.Air], [MOVE.Tornade(), MOVE.Charge()]),
      dino(6, 'Aquadon', 9, [DinoType.Water], [MOVE.PistoletAEau(), MOVE.Charge()]),
    ],
    reward: 280,
  },
];

// --- Route 3 (Lv 14-18) ---
const ROUTE_3_TRAINERS: TrainerData[] = [
  {
    id: 'route3_nageur_pierre',
    name: 'Nageur Pierre',
    trainerClass: 'swimmer',
    spriteType: 'trainer',
    dialogue: {
      before: "L'eau c'est la vie ! Mes dinos aquatiques vont te submerger !",
      after: "Gloub gloub... je coule...",
    },
    party: [
      dino(6, 'Aquadon', 14, [DinoType.Water], [MOVE.PistoletAEau(), MOVE.AquaJet(), MOVE.Morsure()]),
      dino(12, 'Nageoire', 15, [DinoType.Water], [MOVE.BullesDEau(), MOVE.AquaJet(), MOVE.Charge()]),
    ],
    reward: 450,
  },
  {
    id: 'route3_ranger_lucas',
    name: 'Ranger Lucas',
    trainerClass: 'ranger',
    spriteType: 'trainer',
    dialogue: {
      before: "Le passage entre les villes est dangereux. Montre que tu es pret !",
      after: "Tu es assez fort pour continuer. Bonne chance !",
    },
    party: [
      dino(2, 'Herbex', 14, [DinoType.Flora], [MOVE.TranchHerbe(), MOVE.FouetLiane(), MOVE.PoudreToxik()]),
      dino(8, 'Ignitos', 15, [DinoType.Fire], [MOVE.Flammeche(), MOVE.Charge(), MOVE.Morsure()]),
      dino(7, 'Terrapod', 14, [DinoType.Earth], [MOVE.CoupDeBoue(), MOVE.TirDePierre()]),
    ],
    reward: 500,
  },
  {
    id: 'route3_scout_claire',
    name: 'Scout Claire',
    trainerClass: 'scout',
    spriteType: 'trainer',
    dialogue: {
      before: "Je m'entraine tous les jours pour devenir championne !",
      after: "Tu es deja plus fort que moi... Impressionnant.",
    },
    party: [
      dino(4, 'Compso', 16, [DinoType.Normal], [MOVE.PlaquageSauvage(), MOVE.Morsure(), MOVE.Charge()]),
    ],
    reward: 350,
  },
];

// --- Route 4 (Lv 18-24) ---
const ROUTE_4_TRAINERS: TrainerData[] = [
  {
    id: 'route4_hiker_jean',
    name: 'Randonneur Jean',
    trainerClass: 'hiker',
    spriteType: 'trainer',
    dialogue: {
      before: "Les montagnes forgent les plus forts dresseurs !",
      after: "La montagne m'a appris la patience... mais pas assez apparemment.",
    },
    party: [
      dino(15, 'Caillex', 20, [DinoType.Fossil], [MOVE.Eboulement(), MOVE.TirDePierre(), MOVE.Charge()]),
      dino(7, 'Terrapod', 19, [DinoType.Earth], [MOVE.CoupDeBoue(), MOVE.Seisme(), MOVE.JetDeSable()]),
    ],
    reward: 600,
  },
  {
    id: 'route4_scientist_marie',
    name: 'Scientifique Marie',
    trainerClass: 'scientist',
    spriteType: 'trainer',
    dialogue: {
      before: "Mes recherches montrent que la strategie bat la force brute. Verifions !",
      after: "Hypothese invalidee... Je dois revoir mes donnees.",
    },
    party: [
      dino(10, 'Voltex', 20, [DinoType.Electric], [MOVE.Etincelle(), MOVE.OndeDeChoc(), MOVE.Charge()]),
      dino(11, 'Cryodon', 21, [DinoType.Ice], [MOVE.PoudreigeNeige(), MOVE.SouffleGlace(), MOVE.Morsure()]),
    ],
    reward: 700,
  },
  {
    id: 'route4_ranger_antoine',
    name: 'Ranger Antoine',
    trainerClass: 'ranger',
    spriteType: 'trainer',
    dialogue: {
      before: "Volcanville est tout pres. Prepare-toi a la chaleur !",
      after: "Tu vas avoir besoin de cette force la-bas.",
    },
    party: [
      dino(8, 'Ignitos', 20, [DinoType.Fire], [MOVE.Flammeche(), MOVE.RoueDeFeu(), MOVE.Morsure()]),
      dino(2, 'Florasaur', 21, [DinoType.Flora], [MOVE.TranchHerbe(), MOVE.CanonGraine(), MOVE.PoudreToxik()]),
      dino(4, 'Raptogriffe', 22, [DinoType.Normal], [MOVE.PlaquageSauvage(), MOVE.Morsure(), MOVE.Charge()]),
    ],
    reward: 650,
  },
];

// --- Route 5 (Lv 24-30) ---
const ROUTE_5_TRAINERS: TrainerData[] = [
  {
    id: 'route5_nageur_marine',
    name: 'Nageuse Marine',
    trainerClass: 'swimmer',
    spriteType: 'trainer',
    dialogue: {
      before: "Les courants ici sont forts, comme mes dinos !",
      after: "Le courant m'a emportee... Bien joue.",
    },
    party: [
      dino(12, 'Nageoire', 25, [DinoType.Water], [MOVE.BullesDEau(), MOVE.AquaJet(), MOVE.SurfGlacial()]),
      dino(6, 'Aquadon', 26, [DinoType.Water], [MOVE.SurfGlacial(), MOVE.AquaJet(), MOVE.Morsure()]),
    ],
    reward: 800,
  },
  {
    id: 'route5_scientist_rene',
    name: 'Scientifique Rene',
    trainerClass: 'scientist',
    spriteType: 'trainer',
    dialogue: {
      before: "J'etudie l'evolution des dinos. Tu es un sujet interessant !",
      after: "Fascinant... tes dinos sont remarquablement evolues.",
    },
    party: [
      dino(10, 'Voltex', 26, [DinoType.Electric], [MOVE.OndeDeChoc(), MOVE.Tonnerre(), MOVE.Charge()]),
      dino(20, 'Fossirex', 27, [DinoType.Fossil], [MOVE.PouvoirAntique(), MOVE.Eboulement(), MOVE.Morsure()]),
    ],
    reward: 850,
  },
  {
    id: 'route5_ranger_celine',
    name: 'Ranger Celine',
    trainerClass: 'ranger',
    spriteType: 'trainer',
    dialogue: {
      before: "Les glaciers approchent. Seuls les plus forts passent !",
      after: "Tu as le feu sacre. Continue.",
    },
    party: [
      dino(11, 'Cryodon', 26, [DinoType.Ice], [MOVE.SouffleGlace(), MOVE.RayonGlace(), MOVE.Morsure()]),
      dino(8, 'Ignitos', 27, [DinoType.Fire], [MOVE.RoueDeFeu(), MOVE.LanceFlammes(), MOVE.Charge()]),
      dino(15, 'Graviroc', 26, [DinoType.Fossil], [MOVE.Eboulement(), MOVE.TirDePierre(), MOVE.Charge()]),
    ],
    reward: 900,
  },
];

// --- Route 6 (Lv 28-34) ---
const ROUTE_6_TRAINERS: TrainerData[] = [
  {
    id: 'route6_hiker_bernard',
    name: 'Randonneur Bernard',
    trainerClass: 'hiker',
    spriteType: 'trainer',
    dialogue: {
      before: "Les circuits electriques de la ville m'ont inspire !",
      after: "Court-circuit... Je suis grille.",
    },
    party: [
      dino(10, 'Fulgurex', 30, [DinoType.Electric], [MOVE.Tonnerre(), MOVE.OndeDeChoc(), MOVE.Charge()]),
      dino(15, 'Graviroc', 31, [DinoType.Fossil], [MOVE.Eboulement(), MOVE.TirDePierre(), MOVE.Seisme()]),
    ],
    reward: 950,
  },
  {
    id: 'route6_scout_kevin',
    name: 'Scout Kevin',
    trainerClass: 'scout',
    spriteType: 'trainer',
    dialogue: {
      before: "J'ai six badges ! Et toi ?",
      after: "Bon, les badges c'est pas tout dans la vie...",
    },
    party: [
      dino(8, 'Ignitos', 30, [DinoType.Fire], [MOVE.LanceFlammes(), MOVE.RoueDeFeu(), MOVE.Morsure()]),
      dino(6, 'Aquadon', 31, [DinoType.Water], [MOVE.SurfGlacial(), MOVE.AquaJet(), MOVE.BullesDEau()]),
      dino(5, 'Pterodon', 30, [DinoType.Air], [MOVE.LameDAir(), MOVE.Cru_Aile(), MOVE.Tornade()]),
    ],
    reward: 1000,
  },
];

// --- Route 7 (Lv 32-38) ---
const ROUTE_7_TRAINERS: TrainerData[] = [
  {
    id: 'route7_scientist_helene',
    name: 'Scientifique Helene',
    trainerClass: 'scientist',
    spriteType: 'trainer',
    dialogue: {
      before: "Le poison est une arme subtile. Tu vas comprendre pourquoi.",
      after: "Mon poison n'a pas fonctionne cette fois...",
    },
    party: [
      dino(17, 'Toxidon', 34, [DinoType.Venom], [MOVE.CrocPoison(), MOVE.BombeAcide(), MOVE.DardVenin()]),
      dino(18, 'Venomex', 35, [DinoType.Venom], [MOVE.PureDeToxin(), MOVE.CrocPoison(), MOVE.Morsure()]),
    ],
    reward: 1100,
  },
  {
    id: 'route7_ranger_hugo',
    name: 'Ranger Hugo',
    trainerClass: 'ranger',
    spriteType: 'trainer',
    dialogue: {
      before: "Les marais cachent des dinos redoutables. Comme les miens !",
      after: "Les marais t'ont endurci, on dirait.",
    },
    party: [
      dino(17, 'Toxidon', 34, [DinoType.Venom], [MOVE.DardVenin(), MOVE.CrocPoison(), MOVE.Morsure()]),
      dino(7, 'Terrapod', 35, [DinoType.Earth], [MOVE.Seisme(), MOVE.CoupDeBoue(), MOVE.JetDeSable()]),
      dino(2, 'Florasaur', 34, [DinoType.Flora], [MOVE.CanonGraine(), MOVE.TranchHerbe(), MOVE.PoudreToxik()]),
    ],
    reward: 1150,
  },
  {
    id: 'route7_scout_nadia',
    name: 'Scout Nadia',
    trainerClass: 'scout',
    spriteType: 'trainer',
    dialogue: {
      before: "Bientot la Ligue ! Mais d'abord, il faut me battre !",
      after: "Tu vas cartonner a la Ligue, j'en suis sure !",
    },
    party: [
      dino(4, 'Raptogriffe', 36, [DinoType.Normal], [MOVE.PlaquageSauvage(), MOVE.Morsure(), MOVE.HyperVoix()]),
      dino(11, 'Glaciosaure', 35, [DinoType.Ice], [MOVE.RayonGlace(), MOVE.SouffleGlace(), MOVE.Blizzard()]),
    ],
    reward: 1200,
  },
];

// --- Route 8 (Lv 36-40) ---
const ROUTE_8_TRAINERS: TrainerData[] = [
  {
    id: 'route8_hiker_victor',
    name: 'Randonneur Victor',
    trainerClass: 'hiker',
    spriteType: 'trainer',
    dialogue: {
      before: "Les hauteurs de Ciel-Haut ne sont pas pour les faibles !",
      after: "Tu as des ailes, ma parole !",
    },
    party: [
      dino(15, 'Monolisaure', 38, [DinoType.Fossil], [MOVE.Eboulement(), MOVE.FrappePrehistorique(), MOVE.TirDePierre()]),
      dino(5, 'Aeroraptor', 37, [DinoType.Air], [MOVE.LameDAir(), MOVE.Cru_Aile(), MOVE.Tornade()]),
    ],
    reward: 1300,
  },
  {
    id: 'route8_scientist_alain',
    name: 'Scientifique Alain',
    trainerClass: 'scientist',
    spriteType: 'trainer',
    dialogue: {
      before: "Mes dinos metalliques sont indestructibles !",
      after: "Apparemment pas si indestructibles...",
    },
    party: [
      dino(22, 'Mecanorex', 38, [DinoType.Metal], [MOVE.GriffeAcier(), MOVE.QueueDeFer(), MOVE.CanonAcier()]),
      dino(10, 'Tonneraptor', 39, [DinoType.Electric], [MOVE.Tonnerre(), MOVE.Fatal_Foudre(), MOVE.OndeDeChoc()]),
    ],
    reward: 1400,
  },
  {
    id: 'route8_ranger_diane',
    name: 'Ranger Diane',
    trainerClass: 'ranger',
    spriteType: 'trainer',
    dialogue: {
      before: "La derniere arene t'attend. Mais d'abord, montre-moi ta valeur !",
      after: "Tu es pret pour AETHER. Bonne chance, dresseur.",
    },
    party: [
      dino(5, 'Plumex', 38, [DinoType.Air], [MOVE.LameDAir(), MOVE.Aeroblast(), MOVE.Cru_Aile()]),
      dino(8, 'Magmatops', 39, [DinoType.Fire], [MOVE.LanceFlammes(), MOVE.Deflagration(), MOVE.RoueDeFeu()]),
      dino(6, 'Marexis', 38, [DinoType.Water], [MOVE.SurfGlacial(), MOVE.Hydrocanon(), MOVE.AquaJet()]),
    ],
    reward: 1500,
  },
];

// --- Route 9 (Lv 40-44, pre-League) ---
const ROUTE_9_TRAINERS: TrainerData[] = [
  {
    id: 'route9_ranger_maxime',
    name: 'Ranger Maxime',
    trainerClass: 'ranger',
    spriteType: 'trainer',
    dialogue: {
      before: "Seuls les meilleurs arrivent jusqu'ici. Es-tu l'un d'eux ?",
      after: "Tu es l'un des meilleurs. La Ligue t'attend.",
    },
    party: [
      dino(8, 'Magmatops', 42, [DinoType.Fire], [MOVE.Deflagration(), MOVE.LanceFlammes(), MOVE.RoueDeFeu()]),
      dino(6, 'Marexis', 41, [DinoType.Water], [MOVE.Hydrocanon(), MOVE.SurfGlacial(), MOVE.AquaJet()]),
      dino(2, 'Florasaur', 42, [DinoType.Flora], [MOVE.LameVegetale(), MOVE.CanonGraine(), MOVE.TranchHerbe()]),
    ],
    reward: 1600,
  },
  {
    id: 'route9_scientist_patricia',
    name: 'Scientifique Patricia',
    trainerClass: 'scientist',
    spriteType: 'trainer',
    dialogue: {
      before: "Je compile les donnees de tous les combats. Le tien sera le dernier !",
      after: "Donnees enregistrees. Resultat : defaite totale de ma part.",
    },
    party: [
      dino(20, 'Fossirex', 42, [DinoType.Fossil], [MOVE.FrappePrehistorique(), MOVE.PouvoirAntique(), MOVE.Eboulement()]),
      dino(22, 'Mecanorex', 43, [DinoType.Metal], [MOVE.QueueDeFer(), MOVE.CanonAcier(), MOVE.GriffeAcier()]),
      dino(10, 'Tonneraptor', 42, [DinoType.Electric], [MOVE.Fatal_Foudre(), MOVE.Tonnerre(), MOVE.OndeDeChoc()]),
    ],
    reward: 1700,
  },
];

// ============================================================
// GYM LEADERS
// ============================================================
const GYM_LEADERS: TrainerData[] = [
  // Gym 1: FLORA — Plant (Ville-Fougere)
  {
    id: 'gym_leader_flora',
    name: 'FLORA',
    trainerClass: 'gym_leader',
    spriteType: 'trainer',
    dialogue: {
      before: "Bienvenue dans mon arene ! Les plantes sont la base de toute vie... et de toute defaite ! Prepare-toi !",
      after: "Magnifique ! Tes dinos ont eclore comme des fleurs au soleil. Tu merites le Badge Feuille !",
    },
    party: [
      dino(19, 'Fernex', 12, [DinoType.Flora], [MOVE.FouetLiane(), MOVE.Charge(), MOVE.PoudreToxik()]),
      dino(20, 'Fougeraptor', 14, [DinoType.Flora], [MOVE.TranchHerbe(), MOVE.FouetLiane(), MOVE.PoudreToxik(), MOVE.Charge()]),
    ],
    reward: 1500,
    badge: 0,
    tmReward: { moveId: 100, name: 'Fouet Liane' },
  },
  // Gym 2: MARIN — Water (Port-Coquille)
  {
    id: 'gym_leader_marin',
    name: 'MARIN',
    trainerClass: 'gym_leader',
    spriteType: 'trainer',
    dialogue: {
      before: "Ahoy, dresseur ! L'ocean est ma force ! Mes dinos vont te noyer sous une vague de puissance !",
      after: "Tu as surfe sur mes attaques comme un pro ! Le Badge Vague est a toi, moussaillon !",
    },
    party: [
      dino(6, 'Aquadon', 16, [DinoType.Water], [MOVE.PistoletAEau(), MOVE.AquaJet(), MOVE.Morsure()]),
      dino(12, 'Nageoire', 15, [DinoType.Water], [MOVE.BullesDEau(), MOVE.AquaJet(), MOVE.Charge()]),
      dino(13, 'Marexis', 18, [DinoType.Water], [MOVE.SurfGlacial(), MOVE.BullesDEau(), MOVE.AquaJet(), MOVE.Morsure()]),
    ],
    reward: 2000,
    badge: 1,
    tmReward: { moveId: 101, name: 'Surf Glacial' },
  },
  // Gym 3: PETRA — Rock/Fossil (Roche-Haute)
  {
    id: 'gym_leader_petra',
    name: 'PETRA',
    trainerClass: 'gym_leader',
    spriteType: 'trainer',
    dialogue: {
      before: "Les roches resistent a l'epreuve du temps. Tout comme moi ! Voyons si tu peux les briser !",
      after: "Incredible... Tu as fissure ma defense. Le Badge Pierre t'appartient. Porte-le avec fierte !",
    },
    party: [
      dino(15, 'Caillex', 20, [DinoType.Fossil], [MOVE.TirDePierre(), MOVE.Eboulement(), MOVE.Charge()]),
      dino(16, 'Graviroc', 19, [DinoType.Fossil, DinoType.Earth], [MOVE.Eboulement(), MOVE.CoupDeBoue(), MOVE.TirDePierre()]),
      dino(17, 'Monolisaure', 22, [DinoType.Fossil], [MOVE.Eboulement(), MOVE.FrappePrehistorique(), MOVE.TirDePierre(), MOVE.Charge()]),
    ],
    reward: 2500,
    badge: 2,
    tmReward: { moveId: 102, name: 'Eboulement' },
  },
  // Gym 4: VULKAN — Fire (Volcanville)
  {
    id: 'gym_leader_vulkan',
    name: 'VULKAN',
    trainerClass: 'gym_leader',
    spriteType: 'trainer',
    dialogue: {
      before: "HAHAHAHA ! Bienvenue dans la fournaise ! Mes dinos brulent d'envie de combattre ! Sens-tu la chaleur ?!",
      after: "BRAVO ! Tes flammes brulent plus fort que les miennes ! Prends le Badge Flamme... et ne te brule pas !",
    },
    party: [
      dino(8, 'Ignitops', 24, [DinoType.Fire], [MOVE.Flammeche(), MOVE.RoueDeFeu(), MOVE.Morsure()]),
      dino(9, 'Lavacorne', 23, [DinoType.Fire, DinoType.Earth], [MOVE.RoueDeFeu(), MOVE.CoupDeBoue(), MOVE.Flammeche()]),
      dino(14, 'Magmatops', 26, [DinoType.Fire], [MOVE.LanceFlammes(), MOVE.RoueDeFeu(), MOVE.Flammeche(), MOVE.Morsure()]),
    ],
    reward: 3000,
    badge: 3,
    tmReward: { moveId: 103, name: 'Lance-Flammes' },
  },
  // Gym 5: AURORA — Ice (Cryo-Cite)
  {
    id: 'gym_leader_aurora',
    name: 'AURORA',
    trainerClass: 'gym_leader',
    spriteType: 'trainer',
    dialogue: {
      before: "Le froid preserve tout... sauf les dresseurs imprudents. L'hiver arrive pour toi.",
      after: "Tu as fait fondre ma defense glaciale. Impressionnant. Voici le Badge Givre.",
    },
    party: [
      dino(11, 'Cryodon', 28, [DinoType.Ice], [MOVE.SouffleGlace(), MOVE.RayonGlace(), MOVE.Morsure()]),
      dino(19, 'Givrex', 27, [DinoType.Ice], [MOVE.PoudreigeNeige(), MOVE.RayonGlace(), MOVE.SouffleGlace()]),
      dino(21, 'Glaciosaure', 30, [DinoType.Ice], [MOVE.Blizzard(), MOVE.RayonGlace(), MOVE.SouffleGlace(), MOVE.Morsure()]),
    ],
    reward: 3500,
    badge: 4,
    tmReward: { moveId: 104, name: 'Rayon Glace' },
  },
  // Gym 6: TESLA — Electric (Electropolis)
  {
    id: 'gym_leader_tesla',
    name: 'TESLA',
    trainerClass: 'gym_leader',
    spriteType: 'trainer',
    dialogue: {
      before: "L'electricite est la force de l'avenir ! Mes dinos sont charges a bloc ! Tu vas prendre le courant !",
      after: "ELECTRISANT ! Tu as court-circuite toute ma strategie ! Le Badge Eclair brille pour toi !",
    },
    party: [
      dino(10, 'Voltex', 32, [DinoType.Electric], [MOVE.OndeDeChoc(), MOVE.Tonnerre(), MOVE.Etincelle()]),
      dino(23, 'Fulgurex', 31, [DinoType.Electric], [MOVE.Tonnerre(), MOVE.OndeDeChoc(), MOVE.Charge()]),
      dino(24, 'Tonneraptor', 34, [DinoType.Electric], [MOVE.Fatal_Foudre(), MOVE.Tonnerre(), MOVE.OndeDeChoc(), MOVE.Charge()]),
    ],
    reward: 4000,
    badge: 5,
    tmReward: { moveId: 105, name: 'Tonnerre' },
  },
  // Gym 7: VENOM — Poison (Marais-Noir)
  {
    id: 'gym_leader_venom',
    name: 'VENOM',
    trainerClass: 'gym_leader',
    spriteType: 'trainer',
    dialogue: {
      before: "Ksssss... Le poison s'insinue partout. Tu ne verras pas venir ta defaite...",
      after: "Tu as resiste au poison... Tu es plus coriace que je ne le pensais. Le Badge Toxine est a toi.",
    },
    party: [
      dino(17, 'Toxidon', 36, [DinoType.Venom], [MOVE.CrocPoison(), MOVE.BombeAcide(), MOVE.DardVenin()]),
      dino(18, 'Venomex', 35, [DinoType.Venom], [MOVE.PureDeToxin(), MOVE.CrocPoison(), MOVE.DardVenin()]),
      dino(25, 'Pandemonium', 38, [DinoType.Venom, DinoType.Shadow], [MOVE.BombeAcide(), MOVE.PureDeToxin(), MOVE.GriffeOmbre(), MOVE.CrocPoison()]),
    ],
    reward: 4500,
    badge: 6,
    tmReward: { moveId: 106, name: 'Bombe Acide' },
  },
  // Gym 8: AETHER — Air (Ciel-Haut)
  {
    id: 'gym_leader_aether',
    name: 'AETHER',
    trainerClass: 'gym_leader',
    spriteType: 'trainer',
    dialogue: {
      before: "Le ciel est mon domaine ! Ici, seuls ceux qui savent voler peuvent survivre ! Montre-moi tes ailes !",
      after: "Tu as atteint les sommets ! Le Badge Aile est a toi. Avec huit badges... la Ligue t'attend !",
    },
    party: [
      dino(5, 'Plumex', 40, [DinoType.Air], [MOVE.LameDAir(), MOVE.Aeroblast(), MOVE.Cru_Aile()]),
      dino(26, 'Aeroraptor', 39, [DinoType.Air], [MOVE.LameDAir(), MOVE.Cru_Aile(), MOVE.Tornade()]),
      dino(27, 'Cieloptere', 42, [DinoType.Air, DinoType.Fossil], [MOVE.Aeroblast(), MOVE.LameDAir(), MOVE.PouvoirAntique(), MOVE.Cru_Aile()]),
    ],
    reward: 5000,
    badge: 7,
    tmReward: { moveId: 107, name: "Lame d'Air" },
  },
];

// ============================================================
// GYM TRAINERS (2 per gym, guarding the leader)
// ============================================================
const GYM_TRAINERS: TrainerData[] = [
  // Gym 1 trainers
  {
    id: 'gym1_trainer_1',
    name: 'Herboriste Lise',
    trainerClass: 'ranger',
    spriteType: 'trainer',
    dialogue: {
      before: "Les plantes de FLORA sont les plus belles ! Tu ne la battras pas !",
      after: "Bien joue... Mais FLORA est bien plus forte que moi !",
    },
    party: [
      dino(2, 'Herbex', 10, [DinoType.Flora], [MOVE.FouetLiane(), MOVE.Charge()]),
    ],
    reward: 400,
  },
  {
    id: 'gym1_trainer_2',
    name: 'Jardinier Gael',
    trainerClass: 'ranger',
    spriteType: 'trainer',
    dialogue: {
      before: "Mon jardin est plein de dinos plante ! En voici un !",
      after: "Mon dino a besoin d'un peu plus d'engrais...",
    },
    party: [
      dino(2, 'Herbex', 11, [DinoType.Flora], [MOVE.FouetLiane(), MOVE.Charge(), MOVE.PoudreToxik()]),
      dino(3, 'Fernex', 10, [DinoType.Flora], [MOVE.FouetLiane(), MOVE.Charge()]),
    ],
    reward: 450,
  },
  // Gym 2 trainers
  {
    id: 'gym2_trainer_1',
    name: 'Matelot Jacques',
    trainerClass: 'swimmer',
    spriteType: 'trainer',
    dialogue: {
      before: "Ohé ! MARIN m'a tout appris sur les dinos eau !",
      after: "Tu nages mieux que moi, on dirait...",
    },
    party: [
      dino(6, 'Aquadon', 13, [DinoType.Water], [MOVE.PistoletAEau(), MOVE.AquaJet()]),
      dino(12, 'Nageoire', 14, [DinoType.Water], [MOVE.BullesDEau(), MOVE.Charge()]),
    ],
    reward: 550,
  },
  {
    id: 'gym2_trainer_2',
    name: 'Nageuse Lea',
    trainerClass: 'swimmer',
    spriteType: 'trainer',
    dialogue: {
      before: "La mer est notre terrain de jeu !",
      after: "Tu as fait des vagues dans mon equipe...",
    },
    party: [
      dino(6, 'Aquadon', 15, [DinoType.Water], [MOVE.PistoletAEau(), MOVE.AquaJet(), MOVE.Morsure()]),
    ],
    reward: 500,
  },
  // Gym 3 trainers
  {
    id: 'gym3_trainer_1',
    name: 'Mineur Roc',
    trainerClass: 'hiker',
    spriteType: 'trainer',
    dialogue: {
      before: "Dans la mine, c'est la roche qui commande !",
      after: "Tu as brise mes defenses comme de la craie...",
    },
    party: [
      dino(15, 'Caillex', 17, [DinoType.Fossil], [MOVE.TirDePierre(), MOVE.Charge()]),
      dino(7, 'Terrapod', 18, [DinoType.Earth], [MOVE.CoupDeBoue(), MOVE.JetDeSable()]),
    ],
    reward: 650,
  },
  {
    id: 'gym3_trainer_2',
    name: 'Speleologue Nina',
    trainerClass: 'hiker',
    spriteType: 'trainer',
    dialogue: {
      before: "J'ai explore toutes les grottes de la region !",
      after: "Meme dans l'obscurite, tu brilles...",
    },
    party: [
      dino(16, 'Graviroc', 19, [DinoType.Fossil, DinoType.Earth], [MOVE.Eboulement(), MOVE.CoupDeBoue(), MOVE.TirDePierre()]),
    ],
    reward: 700,
  },
  // Gym 4 trainers
  {
    id: 'gym4_trainer_1',
    name: 'Pyroman Blaise',
    trainerClass: 'ranger',
    spriteType: 'trainer',
    dialogue: {
      before: "La chaleur ici est insupportable ! Mes dinos adorent ca !",
      after: "Mon feu s'est eteint...",
    },
    party: [
      dino(8, 'Ignitos', 21, [DinoType.Fire], [MOVE.Flammeche(), MOVE.RoueDeFeu()]),
      dino(8, 'Ignitos', 22, [DinoType.Fire], [MOVE.Flammeche(), MOVE.RoueDeFeu(), MOVE.Morsure()]),
    ],
    reward: 800,
  },
  {
    id: 'gym4_trainer_2',
    name: 'Volcanaute Iris',
    trainerClass: 'scientist',
    spriteType: 'trainer',
    dialogue: {
      before: "J'etudie les volcans depuis des annees. Mes dinos aussi !",
      after: "Mon eruption a fait long feu...",
    },
    party: [
      dino(9, 'Lavacorne', 23, [DinoType.Fire, DinoType.Earth], [MOVE.RoueDeFeu(), MOVE.CoupDeBoue(), MOVE.Flammeche()]),
    ],
    reward: 850,
  },
  // Gym 5 trainers
  {
    id: 'gym5_trainer_1',
    name: 'Skieur Florent',
    trainerClass: 'ranger',
    spriteType: 'trainer',
    dialogue: {
      before: "Le froid ne me fait pas peur ! Et toi ?",
      after: "Tu m'as donne des frissons... d'admiration.",
    },
    party: [
      dino(11, 'Cryodon', 25, [DinoType.Ice], [MOVE.PoudreigeNeige(), MOVE.SouffleGlace()]),
      dino(19, 'Givrex', 26, [DinoType.Ice], [MOVE.SouffleGlace(), MOVE.RayonGlace()]),
    ],
    reward: 950,
  },
  {
    id: 'gym5_trainer_2',
    name: 'Patineuse Agathe',
    trainerClass: 'ranger',
    spriteType: 'trainer',
    dialogue: {
      before: "La glace est mon element ! Glisse avec moi !",
      after: "J'ai glisse vers la defaite...",
    },
    party: [
      dino(11, 'Cryodon', 27, [DinoType.Ice], [MOVE.SouffleGlace(), MOVE.RayonGlace(), MOVE.Morsure()]),
    ],
    reward: 900,
  },
  // Gym 6 trainers
  {
    id: 'gym6_trainer_1',
    name: 'Electricien Samir',
    trainerClass: 'scientist',
    spriteType: 'trainer',
    dialogue: {
      before: "Attention au courant ! Mes dinos sont survoltés !",
      after: "Court-circuit... Je dois reparer mes fusibles.",
    },
    party: [
      dino(10, 'Voltex', 29, [DinoType.Electric], [MOVE.Etincelle(), MOVE.OndeDeChoc()]),
      dino(23, 'Fulgurex', 30, [DinoType.Electric], [MOVE.Tonnerre(), MOVE.OndeDeChoc()]),
    ],
    reward: 1050,
  },
  {
    id: 'gym6_trainer_2',
    name: 'Ingenieuse Zoe',
    trainerClass: 'scientist',
    spriteType: 'trainer',
    dialogue: {
      before: "J'ai concu un programme d'entrainement parfait !",
      after: "Mon programme avait un bug...",
    },
    party: [
      dino(10, 'Voltex', 31, [DinoType.Electric], [MOVE.Tonnerre(), MOVE.OndeDeChoc(), MOVE.Etincelle()]),
    ],
    reward: 1000,
  },
  // Gym 7 trainers
  {
    id: 'gym7_trainer_1',
    name: 'Alchimiste Felix',
    trainerClass: 'scientist',
    spriteType: 'trainer',
    dialogue: {
      before: "Mes potions... euh, mes dinos sont toxiques !",
      after: "Ma formule n'etait pas assez concentree...",
    },
    party: [
      dino(17, 'Toxidon', 33, [DinoType.Venom], [MOVE.DardVenin(), MOVE.CrocPoison()]),
      dino(18, 'Venomex', 34, [DinoType.Venom], [MOVE.CrocPoison(), MOVE.PureDeToxin()]),
    ],
    reward: 1200,
  },
  {
    id: 'gym7_trainer_2',
    name: 'Empoisonneuse Jade',
    trainerClass: 'scientist',
    spriteType: 'trainer',
    dialogue: {
      before: "Le poison est un art subtil. Tu vas l'apprecier...",
      after: "Mon art a ete surpasse...",
    },
    party: [
      dino(17, 'Toxidon', 35, [DinoType.Venom], [MOVE.BombeAcide(), MOVE.CrocPoison(), MOVE.DardVenin()]),
    ],
    reward: 1150,
  },
  // Gym 8 trainers
  {
    id: 'gym8_trainer_1',
    name: 'Pilote Fabrice',
    trainerClass: 'ranger',
    spriteType: 'trainer',
    dialogue: {
      before: "Le ciel est mon terrain de jeu ! En piste !",
      after: "Atterrissage force... Tu es trop fort.",
    },
    party: [
      dino(5, 'Pterodon', 37, [DinoType.Air], [MOVE.Cru_Aile(), MOVE.LameDAir(), MOVE.Tornade()]),
      dino(26, 'Aeroraptor', 38, [DinoType.Air], [MOVE.LameDAir(), MOVE.Cru_Aile()]),
    ],
    reward: 1350,
  },
  {
    id: 'gym8_trainer_2',
    name: 'Acrobate Mei',
    trainerClass: 'ranger',
    spriteType: 'trainer',
    dialogue: {
      before: "Mes dinos volent plus haut que les nuages !",
      after: "Je suis retombee sur terre...",
    },
    party: [
      dino(5, 'Plumex', 39, [DinoType.Air], [MOVE.Aeroblast(), MOVE.LameDAir(), MOVE.Cru_Aile()]),
    ],
    reward: 1300,
  },
];

// ============================================================
// ELITE 4
// ============================================================
const ELITE_FOUR: TrainerData[] = [
  // Elite 1: TERRA — Earth
  {
    id: 'elite4_terra',
    name: 'TERRA',
    trainerClass: 'elite4',
    spriteType: 'trainer',
    dialogue: {
      before: "Je suis TERRA, gardien de la terre ! Les fondations de ton equipe vont trembler !",
      after: "La terre a tremble sous tes pas... Tu es digne de continuer. Mais la route est encore longue.",
    },
    party: [
      dino(7, 'Terrapod', 44, [DinoType.Earth], [MOVE.Seisme(), MOVE.CoupDeBoue(), MOVE.JetDeSable(), MOVE.PlaquageSauvage()]),
      dino(15, 'Monolisaure', 45, [DinoType.Fossil], [MOVE.FrappePrehistorique(), MOVE.Eboulement(), MOVE.TirDePierre(), MOVE.Charge()]),
      dino(9, 'Lavacorne', 46, [DinoType.Fire, DinoType.Earth], [MOVE.Seisme(), MOVE.LanceFlammes(), MOVE.CoupDeBoue(), MOVE.RoueDeFeu()]),
      dino(16, 'Graviroc', 48, [DinoType.Fossil, DinoType.Earth], [MOVE.Seisme(), MOVE.FrappePrehistorique(), MOVE.Eboulement(), MOVE.TirDePierre()]),
    ],
    reward: 6000,
  },
  // Elite 2: SHADOW — Shadow
  {
    id: 'elite4_shadow',
    name: 'SHADOW',
    trainerClass: 'elite4',
    spriteType: 'trainer',
    dialogue: {
      before: "Les ombres devorent tout... Meme la lumiere de tes dinos s'eteindra face a moi.",
      after: "Tu as traverse les tenebres sans faillir. Ton courage est... lumineux.",
    },
    party: [
      dino(28, 'Ombrex', 45, [DinoType.Shadow], [MOVE.GriffeOmbre(), MOVE.BallOmbre(), MOVE.Morsure(), MOVE.PlaquageSauvage()]),
      dino(25, 'Pandemonium', 46, [DinoType.Venom, DinoType.Shadow], [MOVE.BombeAcide(), MOVE.GriffeOmbre(), MOVE.PulsationNuit(), MOVE.CrocPoison()]),
      dino(29, 'Nocturex', 47, [DinoType.Shadow], [MOVE.PulsationNuit(), MOVE.GriffeOmbre(), MOVE.BallOmbre(), MOVE.Morsure()]),
      dino(30, 'Spectrorex', 49, [DinoType.Shadow], [MOVE.PulsationNuit(), MOVE.BallOmbre(), MOVE.GriffeOmbre(), MOVE.HyperVoix()]),
    ],
    reward: 6500,
  },
  // Elite 3: LUMEN — Light
  {
    id: 'elite4_lumen',
    name: 'LUMEN',
    trainerClass: 'elite4',
    spriteType: 'trainer',
    dialogue: {
      before: "La lumiere revele toute faiblesse ! Tes dinos ne pourront pas se cacher !",
      after: "Ta lumiere interieure brille plus fort que la mienne. Continue a illuminer le monde.",
    },
    party: [
      dino(31, 'Luminex', 46, [DinoType.Light], [MOVE.FlashLumineux(), MOVE.RayonLumiere(), MOVE.Charge(), MOVE.PlaquageSauvage()]),
      dino(32, 'Aurore', 47, [DinoType.Light, DinoType.Ice], [MOVE.EclatSolaire(), MOVE.RayonGlace(), MOVE.FlashLumineux(), MOVE.SouffleGlace()]),
      dino(33, 'Solairex', 48, [DinoType.Light, DinoType.Fire], [MOVE.EclatSolaire(), MOVE.LanceFlammes(), MOVE.RayonLumiere(), MOVE.FlashLumineux()]),
      dino(34, 'Divinarex', 50, [DinoType.Light], [MOVE.EclatSolaire(), MOVE.RayonLumiere(), MOVE.FlashLumineux(), MOVE.HyperVoix()]),
    ],
    reward: 7000,
  },
  // Elite 4: CHROME — Metal
  {
    id: 'elite4_chrome',
    name: 'CHROME',
    trainerClass: 'elite4',
    spriteType: 'trainer',
    dialogue: {
      before: "L'acier ne plie pas, ne rompt pas. Comme ma volonte ! Essaie de me briser si tu l'oses !",
      after: "Tu as forge ta victoire dans le feu du combat. L'acier reconnait ta force.",
    },
    party: [
      dino(22, 'Mecanorex', 47, [DinoType.Metal], [MOVE.QueueDeFer(), MOVE.CanonAcier(), MOVE.GriffeAcier(), MOVE.PlaquageSauvage()]),
      dino(35, 'Blindorex', 48, [DinoType.Metal, DinoType.Fossil], [MOVE.QueueDeFer(), MOVE.FrappePrehistorique(), MOVE.CanonAcier(), MOVE.GriffeAcier()]),
      dino(36, 'Titanorex', 49, [DinoType.Metal], [MOVE.QueueDeFer(), MOVE.CanonAcier(), MOVE.GriffeAcier(), MOVE.Seisme()]),
      dino(37, 'Chromasaure', 50, [DinoType.Metal], [MOVE.QueueDeFer(), MOVE.CanonAcier(), MOVE.GriffeAcier(), MOVE.HyperVoix()]),
    ],
    reward: 7500,
  },
];

// ============================================================
// CHAMPION GENESIS
// ============================================================
const CHAMPION: TrainerData = {
  id: 'champion_genesis',
  name: 'GENESIS',
  trainerClass: 'champion',
  spriteType: 'trainer',
  dialogue: {
    before: "Tu as vaincu l'Elite 4... Impressionnant. Je suis GENESIS, le Champion de Pangaea. Mon equipe represente l'equilibre de toute la creation. Montre-moi que tu es digne du titre !",
    after: "C'est... incroyable. Tu as prouve que le lien entre un dresseur et ses dinos peut tout surmonter. Tu es le nouveau Champion de Pangaea !",
  },
  party: [
    dino(14, 'Magmatops', 48, [DinoType.Fire], [MOVE.Deflagration(), MOVE.LanceFlammes(), MOVE.RoueDeFeu(), MOVE.Morsure()]),
    dino(13, 'Marexis', 49, [DinoType.Water], [MOVE.Hydrocanon(), MOVE.SurfGlacial(), MOVE.AquaJet(), MOVE.BullesDEau()]),
    dino(2, 'Florasaur', 49, [DinoType.Flora], [MOVE.LameVegetale(), MOVE.CanonGraine(), MOVE.TranchHerbe(), MOVE.PoudreToxik()]),
    dino(24, 'Tonneraptor', 50, [DinoType.Electric], [MOVE.Fatal_Foudre(), MOVE.Tonnerre(), MOVE.OndeDeChoc(), MOVE.Charge()]),
    dino(30, 'Spectrorex', 51, [DinoType.Shadow], [MOVE.PulsationNuit(), MOVE.BallOmbre(), MOVE.GriffeOmbre(), MOVE.Morsure()]),
    dino(38, 'Primordius', 52, [DinoType.Primal], [MOVE.ForceOriginelle(), MOVE.RugissementPrimal(), MOVE.Seisme(), MOVE.HyperVoix()]),
  ],
  reward: 15000,
  rematchable: true,
};

// ============================================================
// RIVAL REX (6 encounters)
// Rival's starter is type-advantage over the player's choice
// We store 3 variants; the game picks based on player's starter
// ============================================================

function makeRivalParty(starterType: 'fire' | 'water' | 'flora', encounter: number): TrainerDino[] {
  // Rival gets the type advantage: if player picks fire, rival picks water, etc.
  const starters: Record<string, { id: number; name: string; type: number; evo1: string; evo2: string }> = {
    fire: { id: 8, name: 'Ignitos', type: DinoType.Fire, evo1: 'Lavacorne', evo2: 'Magmatops' },
    water: { id: 6, name: 'Aquadon', type: DinoType.Water, evo1: 'Nageoire', evo2: 'Marexis' },
    flora: { id: 2, name: 'Herbex', type: DinoType.Flora, evo1: 'Fernex', evo2: 'Florasaur' },
  };
  const rivalStarterKey = starterType === 'fire' ? 'water' : starterType === 'water' ? 'flora' : 'fire';
  const rs = starters[rivalStarterKey];

  switch (encounter) {
    case 1:
      // First rival fight: level 5, NO typed attacks — just Charge + a stat move
      return [
        dino(rs.id, rs.name, 5, [rs.type], [MOVE.Charge(), MOVE.Grondement()]),
      ];
    case 2:
      return [
        dino(rs.id, rs.name, 14, [rs.type], [MOVE.Charge(), mv(rs.type === DinoType.Fire ? 'Flammeche' : rs.type === DinoType.Water ? 'Pistolet a Eau' : 'Fouet Liane', rs.type, 'special', 40, 25), MOVE.Morsure()]),
        dino(4, 'Compso', 12, [DinoType.Normal], [MOVE.Charge(), MOVE.Morsure(), MOVE.PlaquageSauvage()]),
      ];
    case 3:
      return [
        dino(rs.id + 1, rs.evo1, 25, [rs.type], [mv(rs.type === DinoType.Fire ? 'Roue de Feu' : rs.type === DinoType.Water ? "Bulles d'Eau" : "Tranch'Herbe", rs.type, rs.type === DinoType.Fire ? 'physical' : 'special', rs.type === DinoType.Fire ? 60 : 65, 20), MOVE.Charge(), MOVE.Morsure()]),
        dino(4, 'Raptogriffe', 22, [DinoType.Normal], [MOVE.PlaquageSauvage(), MOVE.Morsure(), MOVE.Charge()]),
        dino(10, 'Voltex', 23, [DinoType.Electric], [MOVE.OndeDeChoc(), MOVE.Etincelle(), MOVE.Charge()]),
      ];
    case 4:
      return [
        dino(rs.id + 1, rs.evo1, 33, [rs.type], [mv(rs.type === DinoType.Fire ? 'Lance-Flammes' : rs.type === DinoType.Water ? 'Surf Glacial' : 'Canon Graine', rs.type, 'special', rs.type === DinoType.Flora ? 80 : 90, 15), MOVE.Morsure(), MOVE.PlaquageSauvage()]),
        dino(4, 'Raptogriffe', 30, [DinoType.Normal], [MOVE.PlaquageSauvage(), MOVE.HyperVoix(), MOVE.Morsure()]),
        dino(10, 'Fulgurex', 31, [DinoType.Electric], [MOVE.Tonnerre(), MOVE.OndeDeChoc(), MOVE.Etincelle()]),
        dino(11, 'Glaciosaure', 32, [DinoType.Ice], [MOVE.RayonGlace(), MOVE.SouffleGlace(), MOVE.Morsure()]),
      ];
    case 5:
      return [
        dino(rs.id + 2, rs.evo2, 42, [rs.type], [mv(rs.type === DinoType.Fire ? 'Deflagration' : rs.type === DinoType.Water ? 'Hydrocanon' : 'Lame Vegetale', rs.type, rs.type === DinoType.Flora ? 'physical' : 'special', rs.type === DinoType.Flora ? 90 : 110, rs.type === DinoType.Flora ? 15 : 5), mv(rs.type === DinoType.Fire ? 'Lance-Flammes' : rs.type === DinoType.Water ? 'Surf Glacial' : 'Canon Graine', rs.type, 'special', rs.type === DinoType.Flora ? 80 : 90, 15), MOVE.Morsure(), MOVE.PlaquageSauvage()]),
        dino(4, 'Raptogriffe', 38, [DinoType.Normal], [MOVE.HyperVoix(), MOVE.PlaquageSauvage(), MOVE.Morsure()]),
        dino(24, 'Tonneraptor', 39, [DinoType.Electric], [MOVE.Fatal_Foudre(), MOVE.Tonnerre(), MOVE.OndeDeChoc()]),
        dino(21, 'Glaciosaure', 40, [DinoType.Ice], [MOVE.Blizzard(), MOVE.RayonGlace(), MOVE.SouffleGlace()]),
        dino(20, 'Fossirex', 41, [DinoType.Fossil], [MOVE.FrappePrehistorique(), MOVE.PouvoirAntique(), MOVE.Eboulement()]),
      ];
    case 6:
    default:
      return [
        dino(rs.id + 2, rs.evo2, 52, [rs.type], [mv(rs.type === DinoType.Fire ? 'Deflagration' : rs.type === DinoType.Water ? 'Hydrocanon' : 'Lame Vegetale', rs.type, rs.type === DinoType.Flora ? 'physical' : 'special', rs.type === DinoType.Flora ? 90 : 110, rs.type === DinoType.Flora ? 15 : 5), mv(rs.type === DinoType.Fire ? 'Lance-Flammes' : rs.type === DinoType.Water ? 'Surf Glacial' : 'Canon Graine', rs.type, 'special', rs.type === DinoType.Flora ? 80 : 90, 15), MOVE.Morsure(), MOVE.PlaquageSauvage()]),
        dino(4, 'Raptogriffe', 48, [DinoType.Normal], [MOVE.HyperVoix(), MOVE.PlaquageSauvage(), MOVE.Morsure(), MOVE.Charge()]),
        dino(24, 'Tonneraptor', 49, [DinoType.Electric], [MOVE.Fatal_Foudre(), MOVE.Tonnerre(), MOVE.OndeDeChoc(), MOVE.Charge()]),
        dino(21, 'Glaciosaure', 50, [DinoType.Ice], [MOVE.Blizzard(), MOVE.RayonGlace(), MOVE.SouffleGlace(), MOVE.Morsure()]),
        dino(20, 'Fossirex', 50, [DinoType.Fossil], [MOVE.FrappePrehistorique(), MOVE.PouvoirAntique(), MOVE.Eboulement(), MOVE.TirDePierre()]),
        dino(38, 'Primordius', 51, [DinoType.Primal], [MOVE.ForceOriginelle(), MOVE.RugissementPrimal(), MOVE.Seisme(), MOVE.HyperVoix()]),
      ];
  }
}

// Template rival data — party gets filled based on player's starter choice
function createRivalEncounter(encounter: number, starterType: 'fire' | 'water' | 'flora'): TrainerData {
  const dialogues: { before: string; after: string }[] = [
    { before: "He, attends ! Je viens d'avoir mon dino aussi ! Voyons qui est le plus fort !", after: "Grrr... La prochaine fois, je te battrai !" },
    { before: "Tiens, tiens ! On se retrouve ! Mon equipe a grandi, pas comme toi !", after: "Tch... Tu as eu de la chance. Je serai Champion avant toi, tu verras !" },
    { before: "Encore toi ? Parfait, j'avais besoin de me defouler ! Mon equipe est imbattable !", after: "Comment tu fais... ? Non, je ne vais pas abandonner !" },
    { before: "REX : Tu es arrive jusqu'ici ? Impressionnant... mais c'est ta limite !", after: "Je... je n'y crois pas. Tu es vraiment devenu fort. Mais la prochaine fois..." },
    { before: "C'est ici que tout se decide ! Si je te bats, c'est moi le Champion ! Pret ?!", after: "Incroyable... Tu me surpasses encore. Va la-bas et deviens Champion. Pour nous deux." },
    { before: "Champion... Non, ANCIEN Champion ! Je me suis entraine comme jamais ! Revanche !", after: "OK, tu es vraiment le meilleur. Je l'admets. Mais je continuerai a progresser !" },
  ];

  const rewards = [300, 800, 1500, 2500, 4000, 8000];

  return {
    id: `rival_rex_encounter_${encounter}`,
    name: 'REX',
    trainerClass: 'rival',
    spriteType: 'rival',
    dialogue: dialogues[encounter - 1],
    party: makeRivalParty(starterType, encounter),
    reward: rewards[encounter - 1],
    rematchable: encounter === 6,
  };
}

// ============================================================
// ESCADRON METEORE
// ============================================================

// Grunts (generic pool, varying levels)
const METEORE_GRUNTS: TrainerData[] = [
  {
    id: 'meteore_grunt_1',
    name: 'Sbire Meteore',
    trainerClass: 'grunt',
    spriteType: 'grunt',
    dialogue: {
      before: "L'Escadron Meteore va remodeler ce monde ! Degage de notre chemin !",
      after: "Ce... ce n'est pas possible ! Le Commandant va etre furieux...",
    },
    party: [
      dino(17, 'Toxidon', 12, [DinoType.Venom], [MOVE.DardVenin(), MOVE.Charge()]),
    ],
    reward: 400,
  },
  {
    id: 'meteore_grunt_2',
    name: 'Sbire Meteore',
    trainerClass: 'grunt',
    spriteType: 'grunt',
    dialogue: {
      before: "Tu oses defier l'Escadron Meteore ? Tu vas le regretter !",
      after: "Ugh... les sbires sont remplaceables de toute facon...",
    },
    party: [
      dino(17, 'Toxidon', 14, [DinoType.Venom], [MOVE.DardVenin(), MOVE.CrocPoison()]),
      dino(28, 'Ombrex', 13, [DinoType.Shadow], [MOVE.GriffeOmbre(), MOVE.Morsure()]),
    ],
    reward: 500,
  },
  {
    id: 'meteore_grunt_3',
    name: 'Sbire Meteore',
    trainerClass: 'grunt',
    spriteType: 'grunt',
    dialogue: {
      before: "Le plan du Commandant est parfait ! Tu ne peux rien y changer !",
      after: "Non... Le Commandant avait tout prevu, sauf toi...",
    },
    party: [
      dino(17, 'Toxidon', 20, [DinoType.Venom], [MOVE.DardVenin(), MOVE.CrocPoison(), MOVE.BombeAcide()]),
    ],
    reward: 600,
  },
  {
    id: 'meteore_grunt_4',
    name: 'Sbire Meteore',
    trainerClass: 'grunt',
    spriteType: 'grunt',
    dialogue: {
      before: "Pour la gloire de l'Escadron ! A l'attaque !",
      after: "Retraite... retraite...",
    },
    party: [
      dino(28, 'Ombrex', 22, [DinoType.Shadow], [MOVE.GriffeOmbre(), MOVE.BallOmbre(), MOVE.Morsure()]),
      dino(17, 'Toxidon', 21, [DinoType.Venom], [MOVE.CrocPoison(), MOVE.DardVenin()]),
    ],
    reward: 650,
  },
  {
    id: 'meteore_grunt_5',
    name: 'Sbire Meteore',
    trainerClass: 'grunt',
    spriteType: 'grunt',
    dialogue: {
      before: "L'Escadron Meteore est invincible ! Tu vas voir !",
      after: "OK peut-etre pas si invincible...",
    },
    party: [
      dino(28, 'Ombrex', 28, [DinoType.Shadow], [MOVE.BallOmbre(), MOVE.GriffeOmbre(), MOVE.PulsationNuit()]),
      dino(18, 'Venomex', 27, [DinoType.Venom], [MOVE.BombeAcide(), MOVE.CrocPoison(), MOVE.DardVenin()]),
    ],
    reward: 800,
  },
  {
    id: 'meteore_grunt_6',
    name: 'Sbire Meteore',
    trainerClass: 'grunt',
    spriteType: 'grunt',
    dialogue: {
      before: "Le meteorite va tout changer ! Tu ne comprends pas la vision !",
      after: "Peut-etre que la vision est un peu floue...",
    },
    party: [
      dino(17, 'Toxidon', 30, [DinoType.Venom], [MOVE.BombeAcide(), MOVE.CrocPoison(), MOVE.PureDeToxin()]),
    ],
    reward: 750,
  },
];

// Lieutenants
const METEORE_LIEUTENANTS: TrainerData[] = [
  {
    id: 'meteore_lt_meteor',
    name: 'Lieutenant METEOR',
    trainerClass: 'lieutenant',
    spriteType: 'grunt',
    dialogue: {
      before: "Je suis le Lieutenant METEOR ! Mon equipe frappe comme une pluie de meteorites !",
      after: "Pff... tu n'as fait que retarder l'inevitable. Le Commandant accomplira sa mission !",
    },
    party: [
      dino(28, 'Ombrex', 28, [DinoType.Shadow], [MOVE.BallOmbre(), MOVE.GriffeOmbre(), MOVE.PulsationNuit()]),
      dino(17, 'Toxidon', 27, [DinoType.Venom], [MOVE.BombeAcide(), MOVE.CrocPoison(), MOVE.DardVenin()]),
      dino(8, 'Ignitos', 29, [DinoType.Fire], [MOVE.LanceFlammes(), MOVE.RoueDeFeu(), MOVE.Morsure()]),
    ],
    reward: 2000,
  },
  {
    id: 'meteore_lt_crater',
    name: 'Lieutenant CRATER',
    trainerClass: 'lieutenant',
    spriteType: 'grunt',
    dialogue: {
      before: "CRATER, c'est moi ! Je creuse des crateres dans les equipes adverses !",
      after: "Mon cratere s'est transforme en trou dans lequel je suis tombe...",
    },
    party: [
      dino(7, 'Terrapod', 30, [DinoType.Earth], [MOVE.Seisme(), MOVE.CoupDeBoue(), MOVE.JetDeSable()]),
      dino(18, 'Venomex', 31, [DinoType.Venom], [MOVE.PureDeToxin(), MOVE.BombeAcide(), MOVE.CrocPoison()]),
      dino(29, 'Nocturex', 32, [DinoType.Shadow], [MOVE.PulsationNuit(), MOVE.GriffeOmbre(), MOVE.BallOmbre()]),
    ],
    reward: 2200,
  },
  {
    id: 'meteore_lt_comet',
    name: 'Lieutenant COMET',
    trainerClass: 'lieutenant',
    spriteType: 'grunt',
    dialogue: {
      before: "Rapide comme une comete ! Mes dinos sont les plus veloces de l'Escadron !",
      after: "Tu m'as rattrape... Personne n'avait jamais fait ca.",
    },
    party: [
      dino(5, 'Aeroraptor', 33, [DinoType.Air], [MOVE.LameDAir(), MOVE.Cru_Aile(), MOVE.Tornade()]),
      dino(10, 'Fulgurex', 34, [DinoType.Electric], [MOVE.Tonnerre(), MOVE.OndeDeChoc(), MOVE.Etincelle()]),
      dino(28, 'Ombrex', 33, [DinoType.Shadow], [MOVE.PulsationNuit(), MOVE.GriffeOmbre(), MOVE.BallOmbre()]),
    ],
    reward: 2400,
  },
  {
    id: 'meteore_lt_asteroid',
    name: 'Lieutenant ASTEROID',
    trainerClass: 'lieutenant',
    spriteType: 'grunt',
    dialogue: {
      before: "Je suis le bras droit du Commandant IMPACT ! Tu ne passeras pas !",
      after: "Le Commandant... pardonnez-moi. Ce gamin est trop fort.",
    },
    party: [
      dino(25, 'Pandemonium', 36, [DinoType.Venom, DinoType.Shadow], [MOVE.BombeAcide(), MOVE.PulsationNuit(), MOVE.GriffeOmbre(), MOVE.CrocPoison()]),
      dino(15, 'Monolisaure', 35, [DinoType.Fossil], [MOVE.FrappePrehistorique(), MOVE.Eboulement(), MOVE.TirDePierre()]),
      dino(30, 'Spectrorex', 37, [DinoType.Shadow], [MOVE.PulsationNuit(), MOVE.BallOmbre(), MOVE.GriffeOmbre(), MOVE.HyperVoix()]),
    ],
    reward: 2800,
  },
];

// Commander IMPACT
const METEORE_COMMANDER: TrainerData = {
  id: 'meteore_commander_impact',
  name: 'Commandant IMPACT',
  trainerClass: 'lieutenant',
  spriteType: 'grunt',
  dialogue: {
    before: "Je suis IMPACT, Commandant de l'Escadron Meteore ! Le Meteorite Primal va reveiller les dinos anciens et remodeler ce monde ! Tu ne peux pas arreter le progres !",
    after: "Non... C'est impossible ! Le Meteorite... Ma vision d'un monde domine par les dinos primordiaux... Tu l'as detruite. Mais souviens-toi : l'evolution ne peut pas etre arretee.",
  },
  party: [
    dino(25, 'Pandemonium', 42, [DinoType.Venom, DinoType.Shadow], [MOVE.BombeAcide(), MOVE.PulsationNuit(), MOVE.GriffeOmbre(), MOVE.CrocPoison()]),
    dino(30, 'Spectrorex', 43, [DinoType.Shadow], [MOVE.PulsationNuit(), MOVE.BallOmbre(), MOVE.GriffeOmbre(), MOVE.HyperVoix()]),
    dino(22, 'Mecanorex', 44, [DinoType.Metal], [MOVE.QueueDeFer(), MOVE.CanonAcier(), MOVE.GriffeAcier(), MOVE.Charge()]),
    dino(14, 'Magmatops', 45, [DinoType.Fire], [MOVE.Deflagration(), MOVE.LanceFlammes(), MOVE.RoueDeFeu(), MOVE.Morsure()]),
    dino(38, 'Primordius', 46, [DinoType.Primal], [MOVE.ForceOriginelle(), MOVE.RugissementPrimal(), MOVE.Seisme(), MOVE.HyperVoix()]),
  ],
  reward: 5000,
};

// ============================================================
// BADGE NAMES AND INFO (for GymScene use)
// ============================================================
export const BADGE_NAMES = [
  'Badge Feuille',
  'Badge Vague',
  'Badge Pierre',
  'Badge Flamme',
  'Badge Givre',
  'Badge Eclair',
  'Badge Toxine',
  'Badge Aile',
];

export const GYM_CITIES = [
  'Ville-Fougere',
  'Port-Coquille',
  'Roche-Haute',
  'Volcanville',
  'Cryo-Cite',
  'Electropolis',
  'Marais-Noir',
  'Ciel-Haut',
];

// ============================================================
// DATA EXPORT & LOOKUP
// ============================================================

/** All trainers in a flat array */
export const ALL_TRAINERS: TrainerData[] = [
  ...ROUTE_1_TRAINERS,
  ...ROUTE_2_TRAINERS,
  ...ROUTE_3_TRAINERS,
  ...ROUTE_4_TRAINERS,
  ...ROUTE_5_TRAINERS,
  ...ROUTE_6_TRAINERS,
  ...ROUTE_7_TRAINERS,
  ...ROUTE_8_TRAINERS,
  ...ROUTE_9_TRAINERS,
  ...GYM_TRAINERS,
  ...GYM_LEADERS,
  ...ELITE_FOUR,
  CHAMPION,
  ...METEORE_GRUNTS,
  ...METEORE_LIEUTENANTS,
  METEORE_COMMANDER,
];

/** Lookup trainer by ID */
export function getTrainer(id: string): TrainerData | undefined {
  return ALL_TRAINERS.find(t => t.id === id);
}

/** Get gym leader by badge index (0-7) */
export function getGymLeader(badgeIndex: number): TrainerData | undefined {
  return GYM_LEADERS[badgeIndex];
}

/** Get gym trainers for a specific gym (badge index 0-7) */
export function getGymTrainers(badgeIndex: number): TrainerData[] {
  const prefix = `gym${badgeIndex + 1}_trainer_`;
  return GYM_TRAINERS.filter(t => t.id.startsWith(prefix));
}

/** Get Elite Four member by index (0-3) */
export function getEliteFourMember(index: number): TrainerData | undefined {
  return ELITE_FOUR[index];
}

/** Get the Champion */
export function getChampion(): TrainerData {
  return CHAMPION;
}

/** Create a rival encounter with the correct party based on player's starter */
export { createRivalEncounter };

/** Get route trainers by route number (1-9) */
export function getRouteTrainers(routeNum: number): TrainerData[] {
  const routeArrays: Record<number, TrainerData[]> = {
    1: ROUTE_1_TRAINERS,
    2: ROUTE_2_TRAINERS,
    3: ROUTE_3_TRAINERS,
    4: ROUTE_4_TRAINERS,
    5: ROUTE_5_TRAINERS,
    6: ROUTE_6_TRAINERS,
    7: ROUTE_7_TRAINERS,
    8: ROUTE_8_TRAINERS,
    9: ROUTE_9_TRAINERS,
  };
  return routeArrays[routeNum] ?? [];
}

/** Get Escadron Meteore members */
export function getMeteoreGrunts(): TrainerData[] {
  return METEORE_GRUNTS;
}

export function getMeteoreLieutenants(): TrainerData[] {
  return METEORE_LIEUTENANTS;
}

export function getMeteoreCommander(): TrainerData {
  return METEORE_COMMANDER;
}
