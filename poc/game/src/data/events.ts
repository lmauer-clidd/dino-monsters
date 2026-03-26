// ============================================================
// Jurassic Trainers -- Narrative Event Data
// All story beats, rival encounters, and Escadron Meteore events
// ============================================================

// ===================== Interfaces =====================

export interface StoryEvent {
  id: string;
  trigger: 'map_enter' | 'tile_step' | 'npc_interact' | 'badge_count' | 'flag';
  triggerMap?: string;
  triggerTile?: { x: number; y: number };
  triggerNpc?: string;
  triggerBadgeCount?: number;
  triggerFlag?: string;
  requiredFlags?: string[];
  blockedByFlags?: string[];
  actions: EventAction[];
}

export type EventAction =
  | { type: 'dialogue'; speaker: string; text: string }
  | { type: 'choice'; prompt: string; choices: string[]; resultFlags: string[] }
  | { type: 'battle'; trainerId: string }
  | { type: 'setFlag'; flag: string; value: boolean }
  | { type: 'heal' }
  | { type: 'giveItem'; itemId: number; quantity: number }
  | { type: 'giveDino'; speciesId: number; level: number }
  | { type: 'screenEffect'; effect: 'flash' | 'shake' | 'fadeOut' | 'fadeIn' }
  | { type: 'moveNpc'; npcId: string; direction: string; tiles: number }
  | { type: 'wait'; ms: number }
  | { type: 'teleport'; mapId: string; x: number; y: number };

// ===================== Item IDs =====================
// Referenced from InventorySystem
// Item IDs matching items.json
const ITEM_POTION = 1;
const ITEM_JURASSIC_BALL = 16;
const ITEM_SUPER_POTION = 2;
const ITEM_SUPER_BALL = 17;
const ITEM_HYPER_POTION = 3;
const ITEM_HYPER_BALL = 18;
const ITEM_MASTER_BALL = 19;
const ITEM_REVIVE = 12;
const ITEM_ANTIDOTE = 5;
const ITEM_FOSSILE_RADAR = 36;

// ===================== All Story Events =====================

export const STORY_EVENTS: StoryEvent[] = [

  // ================================================================
  //  ACT 1 — L'EVEIL (Gyms 1-3)
  // ================================================================

  // --- EVENT_INTRO ---
  // After choosing starter, re-entering Bourg-Nid overworld
  {
    id: 'EVENT_INTRO',
    trigger: 'map_enter',
    triggerMap: 'BOURG_NID',
    requiredFlags: ['has_starter'],
    blockedByFlags: ['intro_complete'],
    actions: [
      { type: 'dialogue', speaker: 'Prof. Paleo', text: 'Ton aventure commence ici ! Explore le monde, remplis le Dinodex, et deviens le meilleur dresseur de Pangaea !' },
      { type: 'dialogue', speaker: 'Prof. Paleo', text: 'Le Dinodex est plus qu\'un catalogue. C\'est un acte de memoire. Chaque dino que tu enregistres est une preuve que cette espece existe encore.' },
      { type: 'dialogue', speaker: 'Prof. Paleo', text: 'Prends ces Jurassic Balls pour tes premiers pas. Tu en auras besoin pour capturer des dinos sauvages.' },
      { type: 'giveItem', itemId: ITEM_JURASSIC_BALL, quantity: 5 },
      { type: 'dialogue', speaker: 'Prof. Paleo', text: 'Et ces Potions, au cas ou tes dinos seraient blesses en route.' },
      { type: 'giveItem', itemId: ITEM_POTION, quantity: 3 },
      { type: 'dialogue', speaker: 'Maman', text: 'Mon enfant... Reviens quand tu voudras, mais pars quand tu dois.' },
      { type: 'dialogue', speaker: 'Maman', text: 'Tu ressembles a ton pere plus que tu ne le crois. Lui aussi avait cette lueur dans les yeux.' },
      { type: 'setFlag', flag: 'intro_complete', value: true },
    ],
  },

  // --- EVENT_RIVAL_1 ---
  // First encounter with Rex on Route 1 — he's waiting on the path
  {
    id: 'EVENT_RIVAL_1',
    trigger: 'map_enter',
    triggerMap: 'ROUTE_1',
    requiredFlags: ['intro_complete'],
    blockedByFlags: ['rival_1_defeated'],
    actions: [
      { type: 'dialogue', speaker: 'Rex', text: 'Ah te voila ! Je t\'attendais.' },
      { type: 'dialogue', speaker: 'Rex', text: 'Tu as choisi ton dino ? Moi j\'ai eu le meilleur, evidemment. Grand-pere sait reconnaitre le talent.' },
      { type: 'dialogue', speaker: 'Rex', text: 'Allez, on fait un match ! Je vais te montrer ce que c\'est, un vrai dresseur !' },
      { type: 'battle', trainerId: 'RIVAL_REX_1' },
      // Post-battle: Rex gives a potion, shows the direction, and leaves
      { type: 'dialogue', speaker: 'Rex', text: '... Pas mal. T\'as eu de la chance, c\'est tout.' },
      { type: 'dialogue', speaker: 'Rex', text: 'Tiens, prends ca. Tu en auras besoin plus que moi.' },
      { type: 'giveItem', itemId: 1, quantity: 2 },
      { type: 'dialogue', speaker: '', text: 'Rex vous donne 2 Potions !' },
      { type: 'dialogue', speaker: 'Rex', text: 'Ville-Fougere est a l\'est. Flora, la championne, utilise des dinos Plante. Prepare-toi bien !' },
      { type: 'dialogue', speaker: 'Rex', text: 'On se revoit la-bas. Et la prochaine fois, c\'est moi qui gagne !' },
      { type: 'moveNpc', npcId: 'rival_rex', direction: 'right', tiles: 15 },
      { type: 'setFlag', flag: 'rival_1_defeated', value: true },
    ],
  },

  // --- EVENT_VILLE_FOUGERE_ARRIVE ---
  // Arriving in Ville-Fougere for the first time
  {
    id: 'EVENT_VILLE_FOUGERE_ARRIVE',
    trigger: 'map_enter',
    triggerMap: 'VILLE_FOUGERE',
    requiredFlags: ['rival_1_defeated'],
    blockedByFlags: ['ville_fougere_visited'],
    actions: [
      { type: 'dialogue', speaker: 'Rex', text: 'Ville-Fougere ! La ville des plantes. Tu sens cette odeur ? C\'est la chlorophylle. Degoutant.' },
      { type: 'dialogue', speaker: 'Rex', text: 'L\'arene est specialisee en type Plante. Flora est la championne. Un peu trop zen a mon gout.' },
      { type: 'dialogue', speaker: 'Rex', text: 'Moi j\'ai deja mon badge. Facile. Bonne chance, t\'en auras besoin !' },
      { type: 'moveNpc', npcId: 'rival_rex', direction: 'right', tiles: 8 },
      { type: 'setFlag', flag: 'ville_fougere_visited', value: true },
    ],
  },

  // --- EVENT_VILLE_FOUGERE_GRUNT ---
  // Escadron grunt harassing a gardener at Ville-Fougere entrance
  {
    id: 'EVENT_VILLE_FOUGERE_GRUNT',
    trigger: 'map_enter',
    triggerMap: 'VILLE_FOUGERE',
    requiredFlags: ['ville_fougere_visited'],
    blockedByFlags: ['ville_fougere_grunt_defeated'],
    actions: [
      { type: 'dialogue', speaker: 'Grunt Meteore', text: 'He, le vieux ! File-nous ces graines de fougeres anciennes ! L\'Escadron Meteore en a besoin !' },
      { type: 'dialogue', speaker: 'Jardinier', text: 'Jamais ! Ces graines ont des milliers d\'annees d\'heritage ! Vous ne les aurez pas !' },
      { type: 'dialogue', speaker: 'Grunt Meteore', text: 'Tiens, un gamin. Tu veux jouer les heros ? Viens te battre alors !' },
      { type: 'battle', trainerId: 'GRUNT_FOUGERE_1' },
      { type: 'dialogue', speaker: 'Grunt Meteore', text: 'Tch ! Tu crois que ca change quelque chose, gamin ? La meteore arrive. On ne peut pas l\'arreter.' },
      { type: 'moveNpc', npcId: 'grunt_fougere', direction: 'left', tiles: 10 },
      { type: 'dialogue', speaker: 'Jardinier', text: 'Merci, jeune dresseur. Ces voyous de l\'Escadron Meteore deviennent de plus en plus audacieux...' },
      { type: 'dialogue', speaker: 'Jardinier', text: 'L\'arene est au sud de la ville. Flora saura reconnaitre ton courage.' },
      { type: 'setFlag', flag: 'ville_fougere_grunt_defeated', value: true },
    ],
  },

  // --- EVENT_BADGE_1_OBTAINED ---
  // After defeating Flora (Gym 1)
  {
    id: 'EVENT_BADGE_1_OBTAINED',
    trigger: 'badge_count',
    triggerBadgeCount: 1,
    blockedByFlags: ['badge_1_event_done'],
    actions: [
      { type: 'dialogue', speaker: 'Flora', text: 'La patience. C\'est ce que les plantes enseignent. Elles ne se pressent jamais, et pourtant elles conquierent tout.' },
      { type: 'dialogue', speaker: 'Flora', text: 'Prends ce Badge Racine — et la CT Fouet-Liane. Tu en auras besoin sur la route.' },
      { type: 'screenEffect', effect: 'flash' },
      { type: 'dialogue', speaker: '', text: 'Badge Racine obtenu !' },
      { type: 'setFlag', flag: 'badge_1_event_done', value: true },
    ],
  },

  // --- EVENT_GROTTE_PASSAGE ---
  // Discovery of PANGAEON mural in the cave between Ville-Fougere and Port-Coquille
  {
    id: 'EVENT_GROTTE_PASSAGE',
    trigger: 'tile_step',
    triggerMap: 'GROTTE_PASSAGE',
    triggerTile: { x: 8, y: 12 },
    blockedByFlags: ['grotte_passage_mural_seen'],
    actions: [
      { type: 'dialogue', speaker: '', text: 'Une fresque murale ancienne... Elle represente un dino gigantesque entoure de lumiere.' },
      { type: 'dialogue', speaker: 'Archeologue', text: 'Ah, vous regardez la fresque ? C\'est PANGAEON. Une legende.' },
      { type: 'dialogue', speaker: 'Archeologue', text: 'Les anciens croyaient qu\'il maintenait l\'equilibre du monde. Un gardien cosmique, ne de l\'asteroide qui a frole notre planete il y a 65 millions d\'annees.' },
      { type: 'dialogue', speaker: 'Archeologue', text: 'Personne ne sait si c\'est reel. Mais ces peintures ont dix mille ans. Quelqu\'un y croyait suffisamment pour les graver dans la pierre.' },
      { type: 'setFlag', flag: 'grotte_passage_mural_seen', value: true },
    ],
  },

  // --- EVENT_PORT_COQUILLE_ARRIVE ---
  // Arriving at Port-Coquille
  {
    id: 'EVENT_PORT_COQUILLE_ARRIVE',
    trigger: 'map_enter',
    triggerMap: 'PORT_COQUILLE',
    requiredFlags: ['badge_1_event_done'],
    blockedByFlags: ['port_coquille_visited'],
    actions: [
      { type: 'dialogue', speaker: '', text: 'Port-Coquille — la ville portuaire. L\'air sale emplit tes poumons.' },
      { type: 'dialogue', speaker: 'Pecheur', text: 'Bienvenue a Port-Coquille, jeune dresseur ! Mais attention... L\'Escadron Meteore rode pres de la Grotte Sous-Marine au nord.' },
      { type: 'dialogue', speaker: 'Pecheur', text: 'Ils cherchent quelque chose dans les profondeurs. Ca fait fuir les poissons...' },
      { type: 'setFlag', flag: 'port_coquille_visited', value: true },
    ],
  },

  // --- EVENT_GROTTE_SOUSMARINE_COMET ---
  // First encounter with Lieutenant COMET in the underwater cave
  {
    id: 'EVENT_GROTTE_SOUSMARINE_COMET',
    trigger: 'tile_step',
    triggerMap: 'GROTTE_SOUSMARINE',
    triggerTile: { x: 15, y: 8 },
    requiredFlags: ['port_coquille_visited'],
    blockedByFlags: ['comet_grotte_defeated'],
    actions: [
      { type: 'dialogue', speaker: 'Lt. Comet', text: '... Encore un enfant.' },
      { type: 'dialogue', speaker: 'Lt. Comet', text: 'Tu n\'es qu\'un enfant. Retourne jouer. Ce qu\'on fait ici depasse ta comprehension.' },
      { type: 'dialogue', speaker: 'Lt. Comet', text: 'Mais puisque tu insistes...' },
      { type: 'battle', trainerId: 'LT_COMET_1' },
      { type: 'dialogue', speaker: 'Lt. Comet', text: '... Impressionnant. Pour un enfant.' },
      { type: 'dialogue', speaker: '', text: 'La lieutenante laisse tomber un fragment de carte etrange en fuyant...' },
      { type: 'giveItem', itemId: ITEM_FOSSILE_RADAR, quantity: 1 },
      { type: 'setFlag', flag: 'comet_grotte_defeated', value: true },
    ],
  },

  // --- EVENT_ESCADRON_1 ---
  // After Badge 2 (Port-Coquille), two grunts block the exit
  {
    id: 'EVENT_ESCADRON_1',
    trigger: 'badge_count',
    triggerBadgeCount: 2,
    blockedByFlags: ['escadron_first_encounter'],
    actions: [
      { type: 'dialogue', speaker: 'Grunt Meteore A', text: 'Halte ! L\'Escadron Meteore controle cette zone !' },
      { type: 'dialogue', speaker: 'Grunt Meteore B', text: 'Les dinosaures sont une menace pour l\'humanite. Un jour, nous les eliminerons tous !' },
      { type: 'dialogue', speaker: 'Grunt Meteore A', text: 'Tu veux passer ? Alors bats-nous d\'abord !' },
      { type: 'battle', trainerId: 'GRUNT_PORT_1' },
      { type: 'dialogue', speaker: 'Grunt Meteore B', text: 'Mon tour ! Tu ne passeras pas !' },
      { type: 'battle', trainerId: 'GRUNT_PORT_2' },
      { type: 'dialogue', speaker: 'Grunt Meteore A', text: 'Grr... Vous n\'avez pas vu la derniere de nous ! Le Commandant ne laissera pas faire !' },
      { type: 'moveNpc', npcId: 'grunt_port_a', direction: 'right', tiles: 8 },
      { type: 'moveNpc', npcId: 'grunt_port_b', direction: 'right', tiles: 8 },
      { type: 'dialogue', speaker: 'Villageois', text: 'L\'Escadron Meteore... Ils disent vouloir provoquer une seconde extinction. Personne ne sait d\'ou ils viennent.' },
      { type: 'dialogue', speaker: 'Villageois', text: 'Fais attention sur la route, jeune dresseur.' },
      { type: 'setFlag', flag: 'escadron_first_encounter', value: true },
    ],
  },

  // --- EVENT_BADGE_2_OBTAINED ---
  {
    id: 'EVENT_BADGE_2_OBTAINED',
    trigger: 'badge_count',
    triggerBadgeCount: 2,
    blockedByFlags: ['badge_2_event_done'],
    actions: [
      { type: 'dialogue', speaker: 'Marin', text: 'Hmph. T\'as du cran, gamin. La mer aussi avait du cran le jour ou elle a presque englouti Port-Coquille.' },
      { type: 'dialogue', speaker: 'Marin', text: 'Prends ce Badge Maree — et la CT Aqua-Jet. Elle frappe vite. Comme toi, apparemment.' },
      { type: 'screenEffect', effect: 'flash' },
      { type: 'dialogue', speaker: '', text: 'Badge Maree obtenu !' },
      { type: 'setFlag', flag: 'badge_2_event_done', value: true },
    ],
  },

  // --- EVENT_RIVAL_ROUTE3 ---
  // Rex intercepts on Route 3, before Roche-Haute
  {
    id: 'EVENT_RIVAL_ROUTE3',
    trigger: 'map_enter',
    triggerMap: 'ROUTE_3',
    requiredFlags: ['escadron_first_encounter'],
    blockedByFlags: ['rival_route3_defeated'],
    actions: [
      { type: 'dialogue', speaker: 'Rex', text: 'Te voila ! J\'en ai marre de t\'attendre.' },
      { type: 'dialogue', speaker: 'Rex', text: 'Pourquoi tu progresses aussi vite ? T\'as un secret ? Un entraineur prive ?' },
      { type: 'dialogue', speaker: 'Rex', text: '... Non ? Alors comment ? Bref, on se bat. Maintenant.' },
      { type: 'battle', trainerId: 'RIVAL_REX_2' },
      { type: 'dialogue', speaker: 'Rex', text: 'ENCORE ?! C\'est... c\'est pas possible.' },
      { type: 'dialogue', speaker: 'Rex', text: 'La prochaine fois, je serai pret. Compte la-dessus.' },
      { type: 'moveNpc', npcId: 'rival_rex', direction: 'up', tiles: 8 },
      { type: 'setFlag', flag: 'rival_route3_defeated', value: true },
    ],
  },

  // --- EVENT_MINE_CRATER ---
  // Lieutenant CRATER in the abandoned mine at Roche-Haute
  {
    id: 'EVENT_MINE_CRATER',
    trigger: 'tile_step',
    triggerMap: 'MINE_ABANDONNEE',
    triggerTile: { x: 12, y: 20 },
    requiredFlags: ['rival_route3_defeated'],
    blockedByFlags: ['crater_mine_defeated'],
    actions: [
      { type: 'dialogue', speaker: '', text: 'Des machines de forage de l\'Escadron Meteore emplissent la mine. Ils creusent profondement...' },
      { type: 'dialogue', speaker: 'Lt. Crater', text: 'Encore un fouineur ? Tu ne sais pas ce que tu fais ici, gamin.' },
      { type: 'dialogue', speaker: 'Lt. Crater', text: 'La Pierre d\'Extinction est quelque part sous nos pieds. Quand on la trouvera, ce monde sera purifie.' },
      { type: 'dialogue', speaker: '', text: 'La Pierre d\'Extinction... ? Ce mot resonne etrangement.' },
      { type: 'battle', trainerId: 'LT_CRATER_1' },
      { type: 'dialogue', speaker: 'Lt. Crater', text: 'Tch ! Pas grave. La Pierre n\'est pas ici de toute facon. Mais on la trouvera.' },
      { type: 'screenEffect', effect: 'shake' },
      { type: 'dialogue', speaker: '', text: 'Crater s\'enfuit en faisant effondrer une section de la mine !' },
      { type: 'dialogue', speaker: '', text: 'Tu t\'echappes de justesse par une sortie secondaire...' },
      { type: 'wait', ms: 500 },
      { type: 'dialogue', speaker: 'Prof. Paleo (Dinocom)', text: 'Allo ?! Tu vas bien ?! J\'ai detecte une activite sismique pres de Roche-Haute !' },
      { type: 'dialogue', speaker: 'Prof. Paleo (Dinocom)', text: 'La Pierre d\'Extinction ? Non... c\'est impossible. Viens me voir a Paleo-Capital quand tu le pourras. C\'est urgent.' },
      { type: 'setFlag', flag: 'crater_mine_defeated', value: true },
      { type: 'setFlag', flag: 'pierre_extinction_mentioned', value: true },
    ],
  },

  // --- EVENT_BADGE_3_OBTAINED ---
  {
    id: 'EVENT_BADGE_3_OBTAINED',
    trigger: 'badge_count',
    triggerBadgeCount: 3,
    blockedByFlags: ['badge_3_event_done'],
    actions: [
      { type: 'dialogue', speaker: 'Petra', text: 'Tu frappes comme le vent dans la montagne. Pas fort. Mais tu ne t\'arretes jamais.' },
      { type: 'dialogue', speaker: 'Petra', text: 'Badge Silex — et CT Eboulement. Utilise-la avec precaution. La montagne ne pardonne pas.' },
      { type: 'screenEffect', effect: 'flash' },
      { type: 'dialogue', speaker: '', text: 'Badge Silex obtenu ! Trois badges... l\'aventure ne fait que commencer.' },
      { type: 'setFlag', flag: 'badge_3_event_done', value: true },
    ],
  },

  // ================================================================
  //  ACT 2 — L'ESCALADE (Gyms 4-6)
  // ================================================================

  // --- EVENT_RIVAL_2 ---
  // Rival encounter in Volcanville
  {
    id: 'EVENT_RIVAL_2',
    trigger: 'map_enter',
    triggerMap: 'VOLCANVILLE',
    requiredFlags: ['badge_3_event_done'],
    blockedByFlags: ['rival_2_defeated'],
    actions: [
      { type: 'dialogue', speaker: 'Rex', text: 'Volcanville ! La forge du monde. Tu sens la chaleur ? Mon sang bout !' },
      { type: 'dialogue', speaker: 'Rex', text: 'Vulkan, le champion, est un vrai dur. Un forgeron qui dresse des dinos Feu.' },
      { type: 'dialogue', speaker: 'Rex', text: 'Tu crois que tu es un heros ? Grand-pere m\'a tout raconte sur la Pierre. MOI AUSSI je peux sauver le monde.' },
      { type: 'dialogue', speaker: 'Rex', text: 'Mais d\'abord... prouve que tu le merites !' },
      { type: 'battle', trainerId: 'RIVAL_REX_3' },
      { type: 'dialogue', speaker: 'Rex', text: 'Tu progresses... mais moi aussi. La prochaine fois sera differente.' },
      { type: 'dialogue', speaker: 'Rex', text: 'L\'Escadron Meteore rode dans le coin. Vulkan les retient, mais ils sont nombreux.' },
      { type: 'setFlag', flag: 'rival_2_defeated', value: true },
    ],
  },

  // --- EVENT_VOLCAN_AIDE_VULKAN ---
  // Help Vulkan repel the Escadron at Volcanville
  {
    id: 'EVENT_VOLCAN_AIDE_VULKAN',
    trigger: 'map_enter',
    triggerMap: 'VOLCAN_INTERIEUR',
    requiredFlags: ['rival_2_defeated'],
    blockedByFlags: ['volcan_escadron_repousse'],
    actions: [
      { type: 'dialogue', speaker: 'Vulkan', text: 'Toi ! Le dresseur dont Rex parle ! Aide-moi a repousser ces types !' },
      { type: 'dialogue', speaker: 'Lt. Meteor', text: 'Le Commandant a un plan pour le Volcan Interieur. Vous ne nous arreterez pas !' },
      { type: 'battle', trainerId: 'GRUNT_VOLCAN_1' },
      { type: 'battle', trainerId: 'LT_METEOR_1' },
      { type: 'dialogue', speaker: 'Lt. Meteor', text: 'Retraite ! Mais ce n\'est que partie remise...' },
      { type: 'moveNpc', npcId: 'lt_meteor', direction: 'down', tiles: 10 },
      { type: 'dialogue', speaker: 'Vulkan', text: 'Bien joue ! Tu as du feu en toi. Viens me defier a l\'arene. Je veux voir si c\'est un feu de paille ou un brasier.' },
      { type: 'setFlag', flag: 'volcan_escadron_repousse', value: true },
    ],
  },

  // --- EVENT_VOLCAN_INSCRIPTIONS ---
  // Discovering ancient inscriptions about the Extinction Stone in Volcan Interieur
  {
    id: 'EVENT_VOLCAN_INSCRIPTIONS',
    trigger: 'tile_step',
    triggerMap: 'VOLCAN_INTERIEUR',
    triggerTile: { x: 10, y: 5 },
    requiredFlags: ['volcan_escadron_repousse'],
    blockedByFlags: ['volcan_inscriptions_read'],
    actions: [
      { type: 'dialogue', speaker: '', text: 'Des inscriptions anciennes sur le mur de la salle scellee...' },
      { type: 'dialogue', speaker: '', text: '"La Pierre nee de l\'etoile dormira en cinq morceaux, dispersee pour que nul ne reveille le Gardien."' },
      { type: 'dialogue', speaker: '', text: '"Quand les cinq fragments seront reunis, PANGAEON ouvrira les yeux et jugera les vivants."' },
      { type: 'dialogue', speaker: '', text: 'Il manque des sections. Le reste est illisible, ronge par le temps...' },
      { type: 'dialogue', speaker: '', text: 'Tu trouves un mot griffonne dans la poussiere, recent : "Luna aimait les volcans. Elle disait que le feu etait vivant. Elle avait raison."' },
      { type: 'setFlag', flag: 'volcan_inscriptions_read', value: true },
    ],
  },

  // --- EVENT_BADGE_4_OBTAINED ---
  {
    id: 'EVENT_BADGE_4_OBTAINED',
    trigger: 'badge_count',
    triggerBadgeCount: 4,
    blockedByFlags: ['badge_4_event_done'],
    actions: [
      { type: 'dialogue', speaker: 'Vulkan', text: 'Un brasier. Oui. Un brasier.' },
      { type: 'dialogue', speaker: 'Vulkan', text: 'Prends le Badge Braise et la CT Lance-Flammes. Le feu ne juge pas — il brule tout pareil. C\'est ce qui le rend honnete.' },
      { type: 'screenEffect', effect: 'flash' },
      { type: 'dialogue', speaker: '', text: 'Badge Braise obtenu !' },
      { type: 'setFlag', flag: 'badge_4_event_done', value: true },
    ],
  },

  // --- EVENT_CRYO_CITE_REVELATION ---
  // Major revelation at Cryo-Cite — Prof Paleo explains the Extinction Stone
  {
    id: 'EVENT_CRYO_CITE_REVELATION',
    trigger: 'map_enter',
    triggerMap: 'CRYO_CITE',
    requiredFlags: ['badge_4_event_done'],
    blockedByFlags: ['cryo_revelation_done'],
    actions: [
      { type: 'dialogue', speaker: 'Prof. Paleo', text: 'Te voila enfin ! J\'ai des nouvelles terribles.' },
      { type: 'dialogue', speaker: 'Prof. Paleo', text: 'La Pierre d\'Extinction... j\'esperais ne jamais entendre ce nom de mon vivant.' },
      { type: 'dialogue', speaker: 'Prof. Paleo', text: 'Il y a 65 millions d\'annees, un asteroide a frole notre monde sans le toucher. Mais il a laisse un fragment.' },
      { type: 'dialogue', speaker: 'Prof. Paleo', text: 'Cette pierre concentre une energie capable de provoquer un cataclysme comparable a l\'impact original.' },
      { type: 'dialogue', speaker: 'Prof. Paleo', text: 'Les anciennes civilisations l\'ont scellee, fragmentee en cinq morceaux, et dispersee a travers Pangaea.' },
      { type: 'dialogue', speaker: 'Prof. Paleo', text: 'L\'Escadron Meteore cherche a les reunir. Les fragments sont caches dans la Grotte des Ancetres, le Volcan Interieur, le Temple Sous-Marin, la Foret Petrifiee, et la Tour des Fossiles.' },
      { type: 'dialogue', speaker: 'Prof. Paleo', text: 'Prends ce Fossile-Radar. Il detecte les fragments a proximite. Tu en auras besoin.' },
      { type: 'giveItem', itemId: ITEM_FOSSILE_RADAR, quantity: 1 },
      { type: 'wait', ms: 300 },
      { type: 'screenEffect', effect: 'shake' },
      { type: 'dialogue', speaker: '???', text: 'Professeur. Ca fait longtemps.' },
      { type: 'dialogue', speaker: '', text: 'Un homme imposant apparait. Calme, sombre, triste. L\'air se glace.' },
      { type: 'dialogue', speaker: 'Commandant Impact', text: 'Je vois que vous avez trouve un nouveau pion a sacrifier.' },
      { type: 'dialogue', speaker: 'Prof. Paleo', text: 'Impact... tu etais mon meilleur eleve. Ne fais pas ca.' },
      { type: 'dialogue', speaker: 'Commandant Impact', text: 'Votre monde est une illusion, Professeur. Les dinos et les humains ne peuvent pas coexister.' },
      { type: 'dialogue', speaker: 'Commandant Impact', text: 'Ma fille en est la preuve.' },
      { type: 'dialogue', speaker: '', text: 'Il tourne les talons et disparait dans la neige. Le Professeur tremble.' },
      { type: 'dialogue', speaker: 'Prof. Paleo', text: '... Impact. L\'ancien Champion de Pangaea. Le plus grand dresseur que j\'aie jamais forme.' },
      { type: 'dialogue', speaker: 'Prof. Paleo', text: 'Sa fille Luna a ete tuee par des dinos sauvages. Depuis, il veut prouver que la coexistence est impossible.' },
      { type: 'dialogue', speaker: 'Prof. Paleo', text: 'Tu dois l\'arreter. Avant qu\'il ne soit trop tard.' },
      { type: 'setFlag', flag: 'cryo_revelation_done', value: true },
      { type: 'setFlag', flag: 'impact_revealed', value: true },
    ],
  },

  // --- EVENT_BADGE_5_OBTAINED ---
  {
    id: 'EVENT_BADGE_5_OBTAINED',
    trigger: 'badge_count',
    triggerBadgeCount: 5,
    blockedByFlags: ['badge_5_event_done'],
    actions: [
      { type: 'dialogue', speaker: 'Aurora', text: 'Tu ne caches rien. C\'est rare — et rafraichissant.' },
      { type: 'dialogue', speaker: 'Aurora', text: 'Badge Givre, et CT Souffle Glacial. La glace est fragile en apparence, mais elle a faconne des continents. Comme toi, peut-etre.' },
      { type: 'screenEffect', effect: 'flash' },
      { type: 'dialogue', speaker: '', text: 'Badge Givre obtenu !' },
      { type: 'setFlag', flag: 'badge_5_event_done', value: true },
    ],
  },

  // --- EVENT_ESCADRON_GROTTE ---
  // Escadron in the Grotte des Ancetres looking for the stone
  {
    id: 'EVENT_ESCADRON_GROTTE',
    trigger: 'map_enter',
    triggerMap: 'GROTTE_ANCETRES',
    requiredFlags: ['cryo_revelation_done'],
    blockedByFlags: ['escadron_grotte'],
    actions: [
      { type: 'dialogue', speaker: '', text: 'Des membres de l\'Escadron Meteore fouillent la grotte avec des machines...' },
      { type: 'dialogue', speaker: 'Lt. Meteor', text: 'La Pierre d\'Extinction... elle est quelque part dans ces grottes. Le Commandant sera satisfait.' },
      { type: 'dialogue', speaker: 'Lt. Meteor', text: 'Vous ! Ne vous melez pas de nos affaires !' },
      { type: 'battle', trainerId: 'GRUNT_GROTTE_1' },
      { type: 'battle', trainerId: 'GRUNT_GROTTE_2' },
      { type: 'dialogue', speaker: 'Lt. Meteor', text: 'Assez ! Je m\'en occupe personnellement !' },
      { type: 'battle', trainerId: 'LT_METEOR_2' },
      { type: 'dialogue', speaker: 'Lt. Meteor', text: 'La Pierre n\'est pas ici... Mais nous la trouverons. Le Commandant ne renonce jamais.' },
      { type: 'moveNpc', npcId: 'lt_meteor', direction: 'down', tiles: 10 },
      { type: 'setFlag', flag: 'escadron_grotte', value: true },
    ],
  },

  // --- EVENT_RIVAL_3 ---
  // Rival encounter before Electropolis
  {
    id: 'EVENT_RIVAL_3',
    trigger: 'map_enter',
    triggerMap: 'ELECTROPOLIS',
    requiredFlags: ['badge_5_event_done'],
    blockedByFlags: ['rival_3_defeated'],
    actions: [
      { type: 'dialogue', speaker: 'Rex', text: 'Electropolis, la cite de l\'innovation ! Tesla est un genie un peu fou, mais redoutable.' },
      { type: 'dialogue', speaker: 'Rex', text: 'J\'ai entendu parler de l\'Escadron Meteore... Ils cherchent une sorte de pierre ancienne. Grand-pere m\'a tout explique.' },
      { type: 'dialogue', speaker: 'Rex', text: 'Impact... etait un champion. Le meilleur. Et il a tout perdu a cause d\'un dino sauvage.' },
      { type: 'dialogue', speaker: 'Rex', text: 'Tu comprends pourquoi je m\'entraine si dur ? Je ne veux pas finir comme lui. Allez, bats-toi !' },
      { type: 'battle', trainerId: 'RIVAL_REX_4' },
      { type: 'dialogue', speaker: 'Rex', text: '... D\'accord. Tu es plus fort. Pour l\'instant.' },
      { type: 'dialogue', speaker: 'Rex', text: 'Mais je ne suis plus juste ton rival. Je suis ton allie. Qu\'on le veuille ou non.' },
      { type: 'setFlag', flag: 'rival_3_defeated', value: true },
    ],
  },

  // --- EVENT_TOUR_FOSSILES_DEFENSE ---
  // Defending the Tower of Fossils from the Escadron assault
  {
    id: 'EVENT_TOUR_FOSSILES_DEFENSE',
    trigger: 'map_enter',
    triggerMap: 'TOUR_FOSSILES',
    requiredFlags: ['rival_3_defeated'],
    blockedByFlags: ['tour_fossiles_defended'],
    actions: [
      { type: 'screenEffect', effect: 'shake' },
      { type: 'dialogue', speaker: '', text: 'La Tour des Fossiles est attaquee ! Des Grunts Meteore defoncent l\'entree !' },
      { type: 'dialogue', speaker: 'Tesla', text: 'Toi ! Le dresseur aux six badges ! Aide-moi a defendre la Tour !' },
      { type: 'dialogue', speaker: 'Tesla', text: 'Le cinquieme fragment de la Pierre d\'Extinction est expose au sommet. On ne peut pas les laisser le prendre !' },
      { type: 'battle', trainerId: 'GRUNT_TOUR_1' },
      { type: 'battle', trainerId: 'GRUNT_TOUR_2' },
      { type: 'dialogue', speaker: 'Lt. Asteroid', text: 'Vous ne comprenez pas. Le Commandant ne veut pas detruire le monde. Il veut le purifier.' },
      { type: 'dialogue', speaker: 'Lt. Asteroid', text: 'Les dinos etaient la avant nous. Ils seront la apres. L\'humanite est la veritable anomalie.' },
      { type: 'battle', trainerId: 'LT_ASTEROID_1' },
      { type: 'dialogue', speaker: 'Lt. Asteroid', text: 'Tch ! Le fragment est protege... pour l\'instant.' },
      { type: 'dialogue', speaker: 'Lt. Asteroid', text: 'Mais trois sur cinq nous suffisent pour le moment. La Foret Petrifiee sera la prochaine.' },
      { type: 'moveNpc', npcId: 'lt_asteroid', direction: 'down', tiles: 10 },
      { type: 'dialogue', speaker: 'Tesla', text: 'FANTASTIQUE ! Ton energie est exponentielle ! Viens a mon arene — je veux voir ta voltage !' },
      { type: 'setFlag', flag: 'tour_fossiles_defended', value: true },
    ],
  },

  // --- EVENT_BADGE_6_OBTAINED ---
  {
    id: 'EVENT_BADGE_6_OBTAINED',
    trigger: 'badge_count',
    triggerBadgeCount: 6,
    blockedByFlags: ['badge_6_event_done'],
    actions: [
      { type: 'dialogue', speaker: 'Tesla', text: 'FANTASTIQUE ! Ton energie est exponentielle !' },
      { type: 'dialogue', speaker: 'Tesla', text: 'Badge Volt — et CT Tonnerre. L\'electricite ne connait qu\'une direction : en avant. Comme toi, j\'espere.' },
      { type: 'screenEffect', effect: 'flash' },
      { type: 'dialogue', speaker: '', text: 'Badge Volt obtenu ! Six badges... La route est longue, mais tu avances.' },
      { type: 'dialogue', speaker: 'Prof. Paleo (Dinocom)', text: 'L\'Escadron Meteore possede deja trois fragments. Le temps presse. Dirige-toi vers la Foret Petrifiee !' },
      { type: 'setFlag', flag: 'badge_6_event_done', value: true },
    ],
  },

  // ================================================================
  //  ACT 3 — LE JUGEMENT (Gyms 7-8 + League)
  // ================================================================

  // --- EVENT_FORET_PETRIFIEE ---
  // Rex joins to fight Escadron in the Petrified Forest
  {
    id: 'EVENT_FORET_PETRIFIEE',
    trigger: 'map_enter',
    triggerMap: 'FORET_PETRIFIEE',
    requiredFlags: ['badge_6_event_done'],
    blockedByFlags: ['foret_petrifiee_complete'],
    actions: [
      { type: 'dialogue', speaker: 'Rex', text: 'Je t\'attendais. L\'Escadron est deja la — je les ai reperes.' },
      { type: 'dialogue', speaker: 'Rex', text: 'Le quatrieme fragment est dans cette foret. On doit le recuperer avant eux.' },
      { type: 'dialogue', speaker: 'Rex', text: 'Je gere les grunts. Toi, occupe-toi de leur lieutenant.' },
      { type: 'battle', trainerId: 'GRUNT_FORET_1' },
      { type: 'battle', trainerId: 'GRUNT_FORET_2' },
      { type: 'dialogue', speaker: 'Lt. Comet', text: 'Encore toi. Tu ne laches jamais, hein ?' },
      { type: 'dialogue', speaker: 'Lt. Comet', text: 'Je respecte ca. Mais le Commandant a besoin de ce fragment.' },
      { type: 'battle', trainerId: 'LT_COMET_2' },
      { type: 'dialogue', speaker: 'Lt. Comet', text: '... Vaincue. Encore une fois.' },
      { type: 'dialogue', speaker: 'Lt. Comet', text: 'Le Commandant... ne veut pas juste reunir la Pierre. Il veut l\'utiliser pour eveiller PANGAEON.' },
      { type: 'dialogue', speaker: 'Lt. Comet', text: 'Il croit que le Gardien jugera l\'humanite et la trouvera indigne. Peut-etre qu\'il a raison...' },
      { type: 'dialogue', speaker: '', text: 'Tu recuperes le quatrieme fragment ! Il en reste un a proteger a la Tour des Fossiles.' },
      { type: 'screenEffect', effect: 'flash' },
      { type: 'setFlag', flag: 'foret_petrifiee_complete', value: true },
      { type: 'setFlag', flag: 'fragment_4_recovered', value: true },
    ],
  },

  // --- EVENT_MARAIS_NOIR_VENOM ---
  // Venom reveals the full truth about PANGAEON
  {
    id: 'EVENT_MARAIS_NOIR_VENOM',
    trigger: 'map_enter',
    triggerMap: 'MARAIS_NOIR',
    requiredFlags: ['foret_petrifiee_complete'],
    blockedByFlags: ['venom_revelation_done'],
    actions: [
      { type: 'dialogue', speaker: '', text: 'Marais-Noir... Un brouillard permanent etouffe la lumiere. L\'air est lourd de secrets.' },
      { type: 'dialogue', speaker: 'Venom', text: 'Je t\'attendais, jeune dresseur. Les marais m\'ont murmure ton arrivee.' },
      { type: 'dialogue', speaker: 'Venom', text: 'Tu cherches la verite sur PANGAEON ? Assieds-toi. Ecoute.' },
      { type: 'dialogue', speaker: 'Venom', text: 'PANGAEON n\'est pas un gardien bienveillant. C\'est un juge.' },
      { type: 'dialogue', speaker: 'Venom', text: 'Il a ete cree par l\'energie de l\'asteroide qui a frole le monde. Si on le reveille avec la Pierre reconstituee...' },
      { type: 'dialogue', speaker: 'Venom', text: '... il jugera si les especes dominantes meritent de continuer a exister.' },
      { type: 'dialogue', speaker: 'Venom', text: 'La derniere fois qu\'il s\'est eveille, il y a dix mille ans, il a decide de laisser le monde en paix.' },
      { type: 'dialogue', speaker: 'Venom', text: 'Mais rien ne garantit qu\'il refasse le meme choix.' },
      { type: 'dialogue', speaker: '', text: 'Cette revelation change tout. Impact ne veut pas detruire le monde — il veut que PANGAEON juge l\'humanite coupable.' },
      { type: 'setFlag', flag: 'venom_revelation_done', value: true },
      { type: 'setFlag', flag: 'pangaeon_truth_known', value: true },
    ],
  },

  // --- EVENT_BADGE_7_OBTAINED ---
  {
    id: 'EVENT_BADGE_7_OBTAINED',
    trigger: 'badge_count',
    triggerBadgeCount: 7,
    blockedByFlags: ['badge_7_event_done'],
    actions: [
      { type: 'dialogue', speaker: 'Venom', text: 'Tu doses bien. Le poison est patient. Il n\'a pas besoin de frapper fort. Il a juste besoin de temps.' },
      { type: 'dialogue', speaker: 'Venom', text: 'Badge Miasme — et CT Toxik. Utilise-les avec sagesse.' },
      { type: 'screenEffect', effect: 'flash' },
      { type: 'dialogue', speaker: '', text: 'Badge Miasme obtenu ! Sept badges. La fin approche.' },
      { type: 'setFlag', flag: 'badge_7_event_done', value: true },
    ],
  },

  // --- EVENT_ESCADRON_TOUR ---
  // Full Escadron base infiltration at Tour des Fossiles (mandatory before Gym 8)
  {
    id: 'EVENT_ESCADRON_TOUR',
    trigger: 'map_enter',
    triggerMap: 'TOUR_FOSSILES_INVASION',
    requiredFlags: ['badge_7_event_done'],
    blockedByFlags: ['escadron_tour_complete'],
    actions: [
      { type: 'screenEffect', effect: 'shake' },
      { type: 'dialogue', speaker: '', text: 'La Tour des Fossiles est envahie par l\'Escadron Meteore ! Le cinquieme fragment est en danger !' },
      { type: 'dialogue', speaker: 'Prof. Paleo (Dinocom)', text: 'Ils ont lance un assaut massif ! Tu dois monter au sommet et proteger le fragment !' },
      // Floor 1
      { type: 'battle', trainerId: 'GRUNT_TOUR_FINAL_1' },
      { type: 'battle', trainerId: 'GRUNT_TOUR_FINAL_2' },
      // Floor 2
      { type: 'dialogue', speaker: 'Lt. Crater', text: 'Tu te souviens de moi ? La mine ? Cette fois, je ne fuirai pas !' },
      { type: 'battle', trainerId: 'LT_CRATER_2' },
      { type: 'dialogue', speaker: 'Lt. Crater', text: 'Ugh... Le Commandant est au sommet. Bonne chance, gamin. T\'en auras besoin.' },
      // Floor 3
      { type: 'battle', trainerId: 'GRUNT_TOUR_FINAL_3' },
      { type: 'battle', trainerId: 'GRUNT_TOUR_FINAL_4' },
      // Summit
      { type: 'dialogue', speaker: '', text: 'Au sommet de la Tour, le Commandant Impact se tient devant le fragment, immobile.' },
      { type: 'dialogue', speaker: 'Commandant Impact', text: 'Tu es arrive jusqu\'ici. Impressionnant. Tu me rappelles... quelqu\'un que j\'etais.' },
      { type: 'dialogue', speaker: 'Commandant Impact', text: 'Les dinosaures ont failli nous detruire une fois. Je ne laisserai pas ca se reproduire.' },
      { type: 'dialogue', speaker: 'Commandant Impact', text: 'Ma fille... Luna... elle avait sept ans. Elle aimait les dinos plus que tout au monde.' },
      { type: 'dialogue', speaker: 'Commandant Impact', text: 'Et c\'est un dino sauvage qui me l\'a prise.' },
      { type: 'dialogue', speaker: 'Commandant Impact', text: 'La Pierre d\'Extinction mettra fin a tout ca. PANGAEON jugera. Et l\'humanite sera enfin en securite.' },
      { type: 'battle', trainerId: 'COMMANDANT_IMPACT_1' },
      { type: 'dialogue', speaker: 'Commandant Impact', text: 'Vous ne comprenez pas... Un jour, vous regretterez de ne pas m\'avoir laisse finir.' },
      { type: 'dialogue', speaker: '', text: 'Impact s\'empare du fragment et s\'enfuit par le toit avant que tu ne puisses reagir !' },
      { type: 'screenEffect', effect: 'fadeOut' },
      { type: 'wait', ms: 1000 },
      { type: 'screenEffect', effect: 'fadeIn' },
      { type: 'dialogue', speaker: 'Prof. Paleo (Dinocom)', text: 'Il a le cinquieme fragment ?! Non... Il a maintenant les cinq morceaux !' },
      { type: 'dialogue', speaker: 'Prof. Paleo (Dinocom)', text: 'Il va se diriger vers la Grotte des Ancetres. C\'est la que PANGAEON dort.' },
      { type: 'dialogue', speaker: 'Prof. Paleo (Dinocom)', text: 'Obtiens ton huitieme badge et retrouve-moi la-bas. Le monde compte sur toi.' },
      { type: 'setFlag', flag: 'escadron_tour_complete', value: true },
      { type: 'setFlag', flag: 'impact_has_all_fragments', value: true },
    ],
  },

  // --- EVENT_RIVAL_4 ---
  // Rival at Ciel-Haut
  {
    id: 'EVENT_RIVAL_4',
    trigger: 'map_enter',
    triggerMap: 'CIEL_HAUT',
    requiredFlags: ['escadron_tour_complete'],
    blockedByFlags: ['rival_4_defeated'],
    actions: [
      { type: 'dialogue', speaker: 'Rex', text: 'Ciel-Haut... le plateau des anciens. Le vent ici est different. Plus pur.' },
      { type: 'dialogue', speaker: 'Rex', text: 'L\'Escadron Meteore a ete repousse de la Tour des Fossiles... mais Impact a le dernier fragment.' },
      { type: 'dialogue', speaker: 'Rex', text: 'Tu sais... Je voulais juste que grand-pere soit fier de moi.' },
      { type: 'dialogue', speaker: 'Rex', text: 'Mais je crois que ce qu\'il veut vraiment, c\'est que quelqu\'un sauve le monde qu\'il a passe sa vie a etudier.' },
      { type: 'dialogue', speaker: 'Rex', text: 'Et ce quelqu\'un... c\'est probablement toi.' },
      { type: 'dialogue', speaker: 'Rex', text: 'Mais d\'abord, prouve-le. Un dernier combat avant la tempete.' },
      { type: 'battle', trainerId: 'RIVAL_REX_5' },
      { type: 'dialogue', speaker: 'Rex', text: '... Je savais que tu gagnerais. Je le savais.' },
      { type: 'dialogue', speaker: 'Rex', text: 'Quand tu affronteras Impact... frappe de toutes tes forces. Pas pour toi. Pour tout le monde.' },
      { type: 'dialogue', speaker: 'Rex', text: 'Je te rejoindrai quand tu auras besoin de moi. C\'est une promesse.' },
      { type: 'setFlag', flag: 'rival_4_defeated', value: true },
    ],
  },

  // --- EVENT_BADGE_8_OBTAINED ---
  {
    id: 'EVENT_BADGE_8_OBTAINED',
    trigger: 'badge_count',
    triggerBadgeCount: 8,
    blockedByFlags: ['badge_8_event_done'],
    actions: [
      { type: 'dialogue', speaker: 'Aether', text: 'Tu es le vent. Tu ne possedes rien, tu ne retiens rien, et pourtant tu es partout.' },
      { type: 'dialogue', speaker: 'Aether', text: 'Badge Zephyr — et CT Aero-Lame. Le vent tranche sans haine. Souviens-toi de ca quand tu affronteras Impact.' },
      { type: 'screenEffect', effect: 'flash' },
      { type: 'dialogue', speaker: '', text: 'Badge Zephyr obtenu ! Huit badges ! La Ligue t\'attend !' },
      { type: 'screenEffect', effect: 'shake' },
      { type: 'dialogue', speaker: '', text: 'Un tremblement de terre secoue tout Pangaea !' },
      { type: 'dialogue', speaker: 'Prof. Paleo (Dinocom)', text: 'L\'Escadron a commence le rituel ! Ils se dirigent vers la Grotte des Ancetres !' },
      { type: 'dialogue', speaker: 'Prof. Paleo (Dinocom)', text: 'C\'est la que tout a commence — et c\'est la que tout peut finir. Depeche-toi !' },
      { type: 'setFlag', flag: 'badge_8_event_done', value: true },
      { type: 'setFlag', flag: 'grotte_ancetres_unlocked', value: true },
    ],
  },

  // --- EVENT_GROTTE_ANCETRES_FINALE ---
  // Final confrontation in the Grotte des Ancetres
  {
    id: 'EVENT_GROTTE_ANCETRES_FINALE',
    trigger: 'map_enter',
    triggerMap: 'GROTTE_ANCETRES_FINALE',
    requiredFlags: ['grotte_ancetres_unlocked'],
    blockedByFlags: ['grotte_ancetres_complete'],
    actions: [
      { type: 'dialogue', speaker: '', text: 'La Grotte des Ancetres. Six niveaux de profondeur. Les fresques anciennes pulsent d\'une lumiere etrange...' },
      // Level 2 — Comet
      { type: 'dialogue', speaker: 'Lt. Comet', text: 'Tu es arrive jusqu\'ici. Je savais que tu viendrais.' },
      { type: 'dialogue', speaker: 'Lt. Comet', text: 'Je ne crois plus en la mission du Commandant. Mais je dois me battre. C\'est tout ce que je sais faire.' },
      { type: 'battle', trainerId: 'LT_COMET_FINAL' },
      { type: 'dialogue', speaker: 'Lt. Comet', text: 'Va. Arrete-le. Peut-etre que tu pourras faire ce que je n\'ai pas eu le courage de faire.' },
      // Level 3 — Crater
      { type: 'dialogue', speaker: 'Lt. Crater', text: 'ENCORE TOI ?! Je vais t\'ecraser cette fois !' },
      { type: 'battle', trainerId: 'LT_CRATER_FINAL' },
      { type: 'dialogue', speaker: 'Lt. Crater', text: 'Comment... comment es-tu devenu si fort ?!' },
      // Level 4 — Meteor
      { type: 'dialogue', speaker: 'Lt. Meteor', text: 'Tu es tenace. Le Commandant avait raison de s\'inquieter.' },
      { type: 'dialogue', speaker: 'Lt. Meteor', text: 'Finissons-en. Calmement.' },
      { type: 'battle', trainerId: 'LT_METEOR_FINAL' },
      { type: 'dialogue', speaker: 'Lt. Meteor', text: '... Le Commandant est au niveau le plus profond. Bonne chance. Sincerement.' },
      // Level 5 — Asteroid
      { type: 'dialogue', speaker: 'Lt. Asteroid', text: 'Le Commandant a raison ! PANGAEON nous jugera ! Et il trouvera l\'humanite COUPABLE !' },
      { type: 'dialogue', speaker: 'Lt. Asteroid', text: 'Vous n\'avez aucun droit de l\'arreter !' },
      { type: 'battle', trainerId: 'LT_ASTEROID_FINAL' },
      { type: 'dialogue', speaker: 'Lt. Asteroid', text: 'Non... non... Le jugement doit avoir lieu !' },
      // Level 6 — Impact
      { type: 'dialogue', speaker: '', text: 'Au plus profond de la grotte, devant l\'autel, les cinq fragments brillent d\'une lumiere menacante.' },
      { type: 'dialogue', speaker: 'Commandant Impact', text: 'Tu es venu. Bien sur que tu es venu.' },
      { type: 'dialogue', speaker: 'Commandant Impact', text: 'Tu me rappelles quelqu\'un. Quelqu\'un qui croyait que le monde etait bon.' },
      { type: 'dialogue', speaker: 'Commandant Impact', text: 'J\'etais cette personne, avant. Avant que le monde ne me prenne tout.' },
      { type: 'dialogue', speaker: 'Commandant Impact', text: 'Luna avait sept ans. Elle voulait devenir dresseuse. Comme son papa.' },
      { type: 'dialogue', speaker: 'Commandant Impact', text: 'J\'etais le Champion de Pangaea. Tout le pouvoir du monde. Et je n\'ai pas pu la sauver.' },
      { type: 'dialogue', speaker: 'Commandant Impact', text: 'Depuis ce jour, j\'ai compris. Les humains et les dinos ne peuvent pas vivre ensemble.' },
      { type: 'dialogue', speaker: 'Commandant Impact', text: 'L\'un devra disparaitre. PANGAEON decidera lequel.' },
      { type: 'battle', trainerId: 'COMMANDANT_IMPACT_FINAL' },
      { type: 'dialogue', speaker: 'Commandant Impact', text: 'Meme ca... meme ca ne suffit pas. Rien ne la ramenera. Rien.' },
      { type: 'screenEffect', effect: 'shake' },
      { type: 'dialogue', speaker: '', text: 'La Pierre d\'Extinction s\'active d\'elle-meme ! Une lumiere aveuglante emplit la grotte !' },
      { type: 'screenEffect', effect: 'flash' },
      { type: 'wait', ms: 1000 },
      { type: 'setFlag', flag: 'grotte_ancetres_complete', value: true },
      { type: 'setFlag', flag: 'pangaeon_awakened', value: true },
    ],
  },

  // --- EVENT_PANGAEON_JUDGMENT ---
  // PANGAEON appears and must be fought (not captured)
  {
    id: 'EVENT_PANGAEON_JUDGMENT',
    trigger: 'flag',
    triggerFlag: 'pangaeon_awakened',
    blockedByFlags: ['pangaeon_judged'],
    actions: [
      { type: 'screenEffect', effect: 'flash' },
      { type: 'dialogue', speaker: '', text: 'PANGAEON apparait — un colosse de lumiere et de fossile, a la fois terrible et majestueux.' },
      { type: 'dialogue', speaker: '', text: 'Il ne parle pas avec des mots. Il emet des images, des emotions. L\'histoire du monde defile...' },
      { type: 'dialogue', speaker: '', text: 'Les eres. Les extinctions evitees. La montee de l\'humanite. La coexistence fragile. Tout.' },
      { type: 'screenEffect', effect: 'shake' },
      { type: 'dialogue', speaker: '', text: 'PANGAEON te regarde. Il attend. C\'est un defi. Prouve que l\'humanite merite de continuer.' },
      { type: 'battle', trainerId: 'PANGAEON_JUDGMENT' },
      { type: 'screenEffect', effect: 'flash' },
      { type: 'dialogue', speaker: '', text: 'PANGAEON se calme. La lumiere s\'adoucit.' },
      { type: 'dialogue', speaker: '', text: 'Il regarde le joueur, regarde Impact, et rend un jugement silencieux : le monde merite de continuer.' },
      { type: 'dialogue', speaker: '', text: 'La Pierre d\'Extinction se fragmente a nouveau. PANGAEON retourne a son sommeil.' },
      { type: 'screenEffect', effect: 'fadeOut' },
      { type: 'wait', ms: 1500 },
      { type: 'screenEffect', effect: 'fadeIn' },
      { type: 'dialogue', speaker: 'Commandant Impact', text: 'Luna... pardonne-moi. Je n\'ai pas pu te sauver. Et j\'ai failli detruire le monde pour ca.' },
      { type: 'dialogue', speaker: '', text: 'Rex arrive avec le Professeur Paleo et les Champions d\'Arene.' },
      { type: 'dialogue', speaker: 'Rex', text: 'C\'est fini. Tu l\'as fait.' },
      { type: 'dialogue', speaker: 'Prof. Paleo', text: 'Tu as fait ce que je n\'ai jamais pu faire. Tu as montre au monde — et a PANGAEON — que nous meritons d\'etre ici.' },
      { type: 'dialogue', speaker: 'Prof. Paleo', text: 'Merci.' },
      { type: 'dialogue', speaker: '', text: 'Le Professeur s\'agenouille aupres d\'Impact.' },
      { type: 'dialogue', speaker: 'Prof. Paleo', text: 'Rentre a la maison, mon ami. Il est temps.' },
      { type: 'dialogue', speaker: '', text: 'Impact est arrete par la Ligue. Son sort sera decide par la justice... mais Paleo promet de temoigner pour lui.' },
      { type: 'heal' },
      { type: 'setFlag', flag: 'pangaeon_judged', value: true },
      { type: 'setFlag', flag: 'main_story_complete', value: true },
    ],
  },

  // --- EVENT_RIVAL_ROUTE_VICTOIRE ---
  // Final rival battle before the League
  {
    id: 'EVENT_RIVAL_ROUTE_VICTOIRE',
    trigger: 'map_enter',
    triggerMap: 'ROUTE_VICTOIRE',
    requiredFlags: ['main_story_complete'],
    blockedByFlags: ['rival_final_defeated'],
    actions: [
      { type: 'dialogue', speaker: 'Rex', text: 'La Route Victoire. Le dernier obstacle avant la Ligue.' },
      { type: 'dialogue', speaker: 'Rex', text: 'La derniere fois qu\'on se bat avant que tu deviennes champion. Pas de regrets. Pas d\'excuses.' },
      { type: 'dialogue', speaker: 'Rex', text: 'Juste toi et moi. Le meilleur gagne.' },
      { type: 'dialogue', speaker: 'Rex', text: 'Et cette fois... ce sera peut-etre moi.' },
      { type: 'battle', trainerId: 'RIVAL_REX_FINAL' },
      { type: 'dialogue', speaker: 'Rex', text: '... Non. C\'est toi. C\'est toujours toi.' },
      { type: 'dialogue', speaker: 'Rex', text: 'Va devenir champion. Je m\'entrainerai jusqu\'a ce que tu aies un rival digne de ce nom.' },
      { type: 'dialogue', speaker: 'Rex', text: 'Enfin... un rival encore plus digne, je veux dire.' },
      { type: 'dialogue', speaker: '', text: 'Rex sourit. Pour la premiere fois, c\'est un sourire sans amertume.' },
      { type: 'setFlag', flag: 'rival_final_defeated', value: true },
    ],
  },

  // --- EVENT_CHAMPION_VICTORY ---
  // After becoming Champion
  {
    id: 'EVENT_CHAMPION_VICTORY',
    trigger: 'flag',
    triggerFlag: 'champion_defeated',
    blockedByFlags: ['champion_victory_event_done'],
    actions: [
      { type: 'dialogue', speaker: 'Genesis', text: 'Le titre est a toi.' },
      { type: 'dialogue', speaker: 'Genesis', text: 'Mais souviens-toi : etre champion, ce n\'est pas etre le plus fort.' },
      { type: 'dialogue', speaker: 'Genesis', text: 'C\'est etre celui qui protege les plus faibles. Impact l\'a oublie. Ne fais pas la meme erreur.' },
      { type: 'screenEffect', effect: 'flash' },
      { type: 'dialogue', speaker: '', text: 'Tu es introrise Champion de Pangaea ! Felicitations !' },
      { type: 'heal' },
      { type: 'setFlag', flag: 'champion_victory_event_done', value: true },
      { type: 'setFlag', flag: 'is_champion', value: true },
    ],
  },

  // ================================================================
  //  POST-GAME EVENTS
  // ================================================================

  // --- EVENT_LEGENDARY ---
  // PANGAEON can be found and captured post-game
  {
    id: 'EVENT_LEGENDARY',
    trigger: 'map_enter',
    triggerMap: 'GROTTE_ANCETRES_PROFONDEUR',
    requiredFlags: ['is_champion'],
    blockedByFlags: ['pangaeon_captured'],
    actions: [
      { type: 'dialogue', speaker: 'Prof. Paleo (Dinocom)', text: 'Champion ! J\'ai localise PANGAEON ! Il est au coeur de la Grotte des Ancetres !' },
      { type: 'dialogue', speaker: 'Prof. Paleo (Dinocom)', text: 'La Pierre d\'Extinction pulse d\'energie residuelle... PANGAEON se manifeste a nouveau !' },
      { type: 'dialogue', speaker: 'Prof. Paleo (Dinocom)', text: 'Cette fois, il ne juge pas. Il... t\'attend. Comme s\'il voulait te rejoindre.' },
      { type: 'screenEffect', effect: 'flash' },
      { type: 'dialogue', speaker: '', text: 'PANGAEON apparait ! Ses yeux brillent d\'une lumiere douce. Il est pret.' },
      { type: 'battle', trainerId: 'PANGAEON_CAPTURE' },
      { type: 'dialogue', speaker: 'Prof. Paleo (Dinocom)', text: 'Incroyable ! Tu as capture le gardien de Pangaea ! L\'equilibre du monde repose entre tes mains.' },
      { type: 'setFlag', flag: 'pangaeon_captured', value: true },
    ],
  },

  // --- EVENT_IMPACT_REDEMPTION ---
  // Commandant Impact's redemption arc
  {
    id: 'EVENT_IMPACT_REDEMPTION',
    trigger: 'npc_interact',
    triggerNpc: 'impact_prison',
    requiredFlags: ['is_champion'],
    blockedByFlags: ['impact_redeemed'],
    actions: [
      { type: 'dialogue', speaker: 'Commandant Impact', text: '... C\'est toi.' },
      { type: 'dialogue', speaker: 'Commandant Impact', text: 'J\'ai eu tort. En voyant PANGAEON... en voyant la lumiere dans ses yeux... j\'ai compris.' },
      { type: 'dialogue', speaker: 'Commandant Impact', text: 'Les dinos ne sont pas l\'ennemi. Ils ne l\'ont jamais ete.' },
      { type: 'dialogue', speaker: 'Commandant Impact', text: 'Ma fille aimait les dinosaures. Plus que tout. C\'est pour ca que sa perte m\'a rendu fou.' },
      { type: 'dialogue', speaker: 'Commandant Impact', text: 'J\'ai voulu detruire ce qu\'elle aimait le plus au monde. Quel pere fait ca ?' },
      { type: 'dialogue', speaker: 'Commandant Impact', text: 'Merci de m\'avoir arrete. Prends ceci. Luna l\'aurait voulu.' },
      { type: 'giveItem', itemId: ITEM_MASTER_BALL, quantity: 1 },
      { type: 'dialogue', speaker: '', text: 'Tu as recu une Master Ball !' },
      { type: 'dialogue', speaker: 'Commandant Impact', text: 'Utilise-la pour proteger un dino qui en a besoin. Pas pour le controler. Pour le sauver.' },
      { type: 'setFlag', flag: 'impact_redeemed', value: true },
    ],
  },

  // --- EVENT_REX_POSTGAME ---
  // Rex can be re-battled daily post-game
  {
    id: 'EVENT_REX_POSTGAME',
    trigger: 'npc_interact',
    triggerNpc: 'rival_rex_postgame',
    requiredFlags: ['is_champion'],
    blockedByFlags: [],
    actions: [
      { type: 'dialogue', speaker: 'Rex', text: 'Le Champion en personne ! Je me suis entraine comme un fou depuis ta victoire.' },
      { type: 'dialogue', speaker: 'Rex', text: 'Un match ? Pour le plaisir, cette fois. Pas de rancune. Juste deux dresseurs au sommet.' },
      { type: 'battle', trainerId: 'RIVAL_REX_POSTGAME' },
      { type: 'dialogue', speaker: 'Rex', text: 'Un jour, je te battrai. Et ce jour-la, on fera la fete ensemble.' },
    ],
  },

  // --- EVENT_PALEO_POSTGAME ---
  // Prof Paleo has new dialogue post-game
  {
    id: 'EVENT_PALEO_POSTGAME',
    trigger: 'npc_interact',
    triggerNpc: 'prof_paleo_lab',
    requiredFlags: ['is_champion'],
    blockedByFlags: ['paleo_postgame_done'],
    actions: [
      { type: 'dialogue', speaker: 'Prof. Paleo', text: 'Champion du monde ! Je suis si fier de toi.' },
      { type: 'dialogue', speaker: 'Prof. Paleo', text: 'Tu sais, ton pere... il m\'a ecrit recemment. Des Routes du Nord.' },
      { type: 'dialogue', speaker: 'Prof. Paleo', text: 'Il a entendu parler de tes exploits. Il dit qu\'il reviendra bientot.' },
      { type: 'dialogue', speaker: 'Prof. Paleo', text: 'En attendant, il y a des zones inexplorees qui attendent un dresseur de ton calibre.' },
      { type: 'dialogue', speaker: 'Prof. Paleo', text: 'L\'Ile des Geants, l\'Abime Sous-Marin, le Pic des Etoiles... Le monde est vaste.' },
      { type: 'dialogue', speaker: 'Prof. Paleo', text: 'Continue a remplir le Dinodex. Chaque espece enregistree est une victoire contre l\'oubli.' },
      { type: 'setFlag', flag: 'paleo_postgame_done', value: true },
    ],
  },

  // --- EVENT_MOM_POSTGAME ---
  // Mom's reaction to you becoming champion
  {
    id: 'EVENT_MOM_POSTGAME',
    trigger: 'npc_interact',
    triggerNpc: 'mom_home',
    requiredFlags: ['is_champion'],
    blockedByFlags: ['mom_postgame_done'],
    actions: [
      { type: 'dialogue', speaker: 'Maman', text: 'Mon enfant... Champion de Pangaea. Je n\'arrive pas a y croire.' },
      { type: 'dialogue', speaker: 'Maman', text: 'Ton pere serait tellement fier. Il est fier, j\'en suis sure, ou qu\'il soit.' },
      { type: 'dialogue', speaker: 'Maman', text: 'Cette maison sera toujours la pour toi. Peu importe ou tes aventures te menent.' },
      { type: 'heal' },
      { type: 'dialogue', speaker: 'Maman', text: 'Tes dinos ont l\'air fatigues. Repose-toi un peu avant de repartir.' },
      { type: 'setFlag', flag: 'mom_postgame_done', value: true },
    ],
  },
];

// ===================== Helper: Get events by trigger =====================

export function getMapEnterEvents(mapId: string): StoryEvent[] {
  return STORY_EVENTS.filter(e => e.trigger === 'map_enter' && e.triggerMap === mapId);
}

export function getTileStepEvents(mapId: string, x: number, y: number): StoryEvent[] {
  return STORY_EVENTS.filter(
    e => e.trigger === 'tile_step' && e.triggerMap === mapId &&
         e.triggerTile?.x === x && e.triggerTile?.y === y
  );
}

export function getNpcInteractEvents(npcId: string): StoryEvent[] {
  return STORY_EVENTS.filter(e => e.trigger === 'npc_interact' && e.triggerNpc === npcId);
}

export function getBadgeCountEvents(badgeCount: number): StoryEvent[] {
  return STORY_EVENTS.filter(e => e.trigger === 'badge_count' && e.triggerBadgeCount === badgeCount);
}

export function getFlagEvents(flag: string): StoryEvent[] {
  return STORY_EVENTS.filter(e => e.trigger === 'flag' && e.triggerFlag === flag);
}
