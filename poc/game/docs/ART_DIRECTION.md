---
name: Art Direction
description: Document complet de direction artistique pour Jurassic Trainers — palettes, specs techniques, guidelines de design, UI/UX
type: art
status: production-ready
authors: ZEPHYR (Art Director), NOVA (Concept Artist), ATLAS (Environment Artist), PIXEL (UI/UX Designer)
version: 1.0
---

# JURASSIC TRAINERS — Art Direction Document

> *"Un seul screenshot doit suffire a identifier ce jeu. Pas un Pokemon clone, pas un jeu dino generique — JURASSIC TRAINERS."*
> — ZEPHYR, Art Director

> *"Chaque creature doit raconter 65 millions d'annees d'histoire dans sa silhouette."*
> — NOVA, Concept Artist

> *"Les environnements ne sont pas des decors. Ce sont les os fossiles d'un monde qui respire encore."*
> — ATLAS, Environment Artist

> *"L'interface doit etre aussi naturelle que de retourner une pierre pour trouver un fossile en dessous."*
> — PIXEL, UI/UX Designer

---

## Table des matieres

1. [Visual Identity & Philosophy](#1-visual-identity--philosophy)
2. [Technical Specifications](#2-technical-specifications)
3. [Color Palettes](#3-color-palettes)
4. [Character Design](#4-character-design)
5. [Dinosaur Design Guidelines](#5-dinosaur-design-guidelines)
6. [Environment Art](#6-environment-art)
7. [UI/UX Art Direction](#7-uiux-art-direction)
8. [Animation Guidelines](#8-animation-guidelines)
9. [Visual Effects](#9-visual-effects)
10. [Reference Board](#10-reference-board)

---

## 1. Visual Identity & Philosophy

### 1.1 Core Visual DNA — Ce qui nous differencie

**ZEPHYR :** Le piege mortel serait de faire "Pokemon mais avec des dinos". On doit creer une identite si forte qu'un joueur qui voit un screenshot pense JURASSIC TRAINERS, pas "encore un clone". Voici les 4 piliers visuels :

#### Pilier 1 : Terre & Pierre (Materialite)
Pokemon utilise des couleurs vives, presque plastiques. Nous utilisons des tons **terreux, mineraux, organiques**. Le monde de Pangaea est fait de roche sedimentaire, de lave refroidie, de fossiles apparents. Chaque surface a de la texture, de la matiere.

#### Pilier 2 : Le Temps Profond (Temporalite)
Tout dans le jeu rappelle que des millions d'annees ont passe. Les batiments sont a moitie enfouis, les chemins suivent d'anciennes traces de migration, les fossiles affleurent dans les murs des grottes. La patine du temps est partout.

#### Pilier 3 : Nature Sauvage (Credibilite)
Nos dinos ne sont pas des mascottes — ce sont des ANIMAUX. Stylises, oui. Mignons aux stades bebes, oui. Mais avec une anatomie credible, des proportions qui evoquent de vrais animaux, des comportements naturalistes. Un T-Rex adulte doit inspirer le respect, pas faire rire.

#### Pilier 4 : Chaleur Ambre (Emotion)
La teinte dominante du jeu est l'ambre — ce jaune-brun chaud qui evoque la resine fossile, le soleil prehistorique, la preservation. Meme les scenes froides (Cryo-Cite) ont une touche d'ambre dans les lumieres artificielles. C'est notre signature.

### 1.2 Approche "Realiste-Fantaisiste"

**NOVA :** Le design des creatures suit une regle simple : **commencer par la science, finir par la magie.**

- **Base scientifique** : Chaque dino commence avec l'anatomie reelle de l'espece dont il s'inspire. Un raptor a la bonne posture, le bon nombre de doigts, la bonne structure osseuse.
- **Stylisation selective** : On simplifie pour le pixel art, on exagere les traits les plus reconnaissables (la crete du parasaurolophus, les cornes du triceratops, les ailes du pteranodon).
- **Touche fantaisiste** : Le type ajoute l'element fantastique. Un raptor de type Feu a des ecailles qui rougeoient aux extremites — pas des flammes cartoonesques, mais une incandescence subtile, comme de la braise.

**Ce qu'on NE fait PAS :**
- Pas de dinos humanises (pas de dinos qui portent des vetements ou des objets)
- Pas de traits "kawaii" excessifs (pas d'yeux geants qui occupent la moitie du visage, sauf stade Bebe)
- Pas de designs "cool pour etre cool" (pas de dinos avec des lunettes de soleil ou des cicatrices edgy sans raison)
- Pas de couleurs neon non justifiees par le type

### 1.3 Le Theme de l'Extinction dans les Visuels

**ATLAS :** L'extinction est notre sous-texte permanent. Elle se manifeste visuellement par :

- **Strates geologiques visibles** : Les falaises et grottes montrent des couches de sediments colores, comme un calendrier du temps
- **Fossiles decoratifs** : Des empreintes de pas dans la roche, des os qui depassent du sol, des coquillages petrifies dans les murs
- **Vegetation persistante** : Des fougeres geantes, des coniferes primitifs, des ginkgos — la flore evoque le Mesozoique
- **Ruines d'une civilisation ancienne** : Des structures megalithiques mysterieuses, trop anciennes pour etre expliquees
- **Ciel ambigument menacant** : Un voile subtil dans le ciel, comme une poussiere en suspension — le souvenir de l'impact

---

## 2. Technical Specifications

### 2.1 Resolution & Rendu

| Parametre | Valeur |
|-----------|--------|
| Resolution native | 240 x 160 pixels |
| Ratio d'aspect | 3:2 |
| Upscale affichage | x3 (720x480) ou x4 (960x640) |
| Methode d'upscale | Nearest-neighbor (pas de filtrage bilineaire) |
| Frame rate cible | 60 FPS (logique a 60, rendu a 60) |
| Tick rate gameplay | 60 Hz |

### 2.2 Tailles de Sprites — Reference Complete

#### Personnages (Overworld)

| Element | Taille (px) | Notes |
|---------|-------------|-------|
| Joueur / PNJ overworld | 16 x 24 | 16 large, 24 haut (tete depasse la tile) |
| Joueur / PNJ tile footprint | 16 x 16 | La zone de collision correspond a 1 tile |
| Sprite sheet joueur | 64 x 96 | 4 colonnes (directions) x 4 lignes (idle + 3 walk frames) |
| PNJ statique | 16 x 24 | 1 direction, idle seulement |
| PNJ mobile | 64 x 96 | Meme format que joueur |
| PNJ important (Prof, Rival) | 32 x 32 | Overworld : taille normale. Portrait en dialogue : 32x32 |

#### Dinosaures (Combat — Vue de face, ennemi)

| Stade | Taille (px) | Notes |
|-------|-------------|-------|
| Bebe | 40 x 40 | Centre dans une zone de 48x48 |
| Jeune | 48 x 48 | Centre dans une zone de 56x56 |
| Adulte | 56 x 56 | Peut deborder jusqu'a 64x64 pour legendaires |
| Legendaire PANGAEON | 64 x 64 | Taille maximale absolue |

#### Dinosaures (Combat — Vue de dos, allie)

| Stade | Taille (px) | Notes |
|-------|-------------|-------|
| Bebe | 40 x 40 | Vu de dos, leger zoom par rapport a face |
| Jeune | 48 x 48 | Vu de dos |
| Adulte | 56 x 56 | Vu de dos |
| Legendaire | 64 x 64 | Vu de dos |

#### Dinosaures (Overworld — Follower)

| Element | Taille (px) | Notes |
|---------|-------------|-------|
| Dino follower (tous stades) | 16 x 16 | Version super-deformee, reconnaissable par silhouette + couleur |

#### Dinosaures (Dinodex — Illustration)

| Element | Taille (px) | Notes |
|---------|-------------|-------|
| Vignette Dinodex | 32 x 32 | Mini-portrait pour les listes |
| Portrait Dinodex full | 64 x 64 | Illustration detaillee dans la fiche |

#### Icones & Items

| Element | Taille (px) | Notes |
|---------|-------------|-------|
| Item inventaire | 16 x 16 | Icone standard |
| Item au sol (overworld) | 16 x 16 | Icone dans une Pokeball/sphere visible |
| Badge d'arene | 16 x 16 | 8 badges, chacun unique |
| Icone de type | 8 x 8 | 14 icones, une par type |
| Icone de type (grand) | 16 x 16 | Pour l'ecran de detail du Dinodex |
| Jurassic Ball | 16 x 16 | Animation de lancer : 8 x 8 en vol |

#### Portraits & Dialogues

| Element | Taille (px) | Notes |
|---------|-------------|-------|
| Portrait PNJ (dialogue) | 32 x 32 | Affiche a cote de la boite de dialogue |
| Portrait full (scenes cles) | 64 x 80 | Pour les cutscenes importantes |

### 2.3 Taille des Tiles

| Element | Taille (px) | Notes |
|---------|-------------|-------|
| Tile de base | 16 x 16 | Unite fondamentale de la map |
| Meta-tile (batiment) | 32 x 32 | Bloc de 2x2 tiles pour structures |
| Mega-tile (grand batiment) | 48 x 48 ou 64 x 64 | Pour les arenes, centres Paleo |
| Tile de collision | 16 x 16 | Grille de collision aligne sur les tiles |

### 2.4 Profondeur de Couleur & Contraintes Palette

**ZEPHYR :** On respecte les contraintes GBA pour l'authenticite, avec quelques libertes modernes.

| Parametre | Valeur GBA | Notre approche |
|-----------|-----------|----------------|
| Couleurs totales | 32 768 (15-bit RGB) | On utilise 15-bit : RGB555 (0-31 par canal) |
| Couleurs par palette | 16 (dont 1 transparente) | 15 couleurs utiles + 1 transparente |
| Palettes BG max | 16 palettes de 16 couleurs | On s'impose 12 palettes BG max par ecran |
| Palettes Sprite max | 16 palettes de 16 couleurs | On s'impose 10 palettes Sprite max par ecran |
| Couleur transparente | Index 0 de chaque palette | Toujours `#FF00FF` (magenta) dans les sources |

**Regle de conversion RGB555 :**
Les couleurs sont definies en RGB888 (hex standard) dans ce document pour la lisibilite, mais chaque valeur doit etre arrondie au multiple de 8 le plus proche pour rester GBA-accurate.
- Formule : `valeur_GBA = round(valeur_RGB888 / 8) * 8`
- Exemple : `#B47A3E` -> R=176(B0), G=120(78), B=64(40) -> `#B07840`

### 2.5 Nombre de Frames d'Animation

| Animation | Frames | Vitesse | Notes |
|-----------|--------|---------|-------|
| Walk cycle (overworld) | 4 (idle + 3 pas) | 8 FPS | Par direction = 16 frames total |
| Idle PNJ | 2 | 2 FPS | Leger balancement |
| Dino combat idle | 3 | 4 FPS | Respiration subtile |
| Attaque physique | 4-6 | 12 FPS | Mouvement vers l'avant + frappe |
| Attaque speciale | 3-5 | 10 FPS | Charge + release |
| Touche (hit) | 2 | 15 FPS | Flash blanc + recul |
| KO (faint) | 4 | 6 FPS | Chute + fondu |
| Capture ball vol | 6 | 12 FPS | Arc parabolique |
| Capture ball shake | 3 | 4 FPS | Gauche-centre-droite + click |
| Evolution | 8 | Variable | Flash progressif + transformation |
| Tile animee (eau) | 4 | 4 FPS | Loop infini |
| Tile animee (lave) | 4 | 3 FPS | Plus lent que l'eau |
| Tile animee (herbe) | 2 | 2 FPS | Balancement subtil |
| Curseur menu | 2 | 4 FPS | Pulse/bounce |

---

## 3. Color Palettes

### 3.1 Palette Globale — Signature Ambre

**ZEPHYR :** Notre identite tient en un mot : AMBRE. C'est la teinte qui unifie tout. Voici la palette maitresse :

#### Palette Maitresse Jurassic Trainers

| Role | Nom | Hex | RGB | Utilisation |
|------|-----|-----|-----|-------------|
| Noir profond | Obsidienne | `#181018` | 24, 16, 24 | Outlines, ombres maximales |
| Noir chaud | Bitume | `#282028` | 40, 32, 40 | Outlines doux, texte |
| Brun fonce | Terre brulee | `#503820` | 80, 56, 32 | Ombres chaudes, ecorce |
| Brun moyen | Sediment | `#886830` | 136, 104, 48 | Terre, rochers, bois |
| Ambre | Resine | `#C89840` | 200, 152, 64 | COULEUR SIGNATURE — accents chauds |
| Jaune sable | Sable sec | `#E8C868` | 232, 200, 104 | Highlights chauds, sable |
| Blanc creme | Calcaire | `#F0E8D0` | 240, 232, 208 | Highlights maximum, os |
| Blanc pur | Quartz | `#F8F0E8` | 248, 240, 232 | Eclats, reflets speculaires |

### 3.2 Palettes de Biomes / Villes

---

#### 1. Bourg-Nid (Village de depart — Plaines)

*ATLAS : Un village pastoral baigne dans la lumiere doree de l'apres-midi. Des clotures en bois, des toits de chaume, des oeufs fossiles comme decoration.*

| Role | Nom | Hex | Utilisation |
|------|-----|-----|-------------|
| Dominante | Herbe tendre | `#68A830` | Prairies, gazon |
| Secondaire | Terre battue | `#B08848` | Chemins, sol |
| Accent 1 | Chaume dore | `#D8B850` | Toits, clotures |
| Accent 2 | Bois brun | `#785030` | Structures, troncs |
| Ciel | Azur doux | `#88C0E0` | Ciel, arriere-plan |
| Highlight | Fleur blanche | `#F0E8D0` | Fleurs, details lumineux |
| Ombre | Sous-bois | `#386018` | Ombres vegetales |
| Detail | Oeuf creme | `#E8D8B0` | Oeufs decoratifs, pierres claires |

**Transition vers Ville-Fougere :** L'herbe devient progressivement plus haute et dense, les arbres se multiplient, le vert s'assombrit.

---

#### 2. Ville-Fougere (Foret dense — Arene Plante)

*ATLAS : Une ville construite dans et autour d'arbres geants. Les racines forment les fondations, les canopees forment les toits. Tout est organique, vivant, respire le vert.*

| Role | Nom | Hex | Utilisation |
|------|-----|-----|-------------|
| Dominante | Feuillage profond | `#306028` | Canopee, vegetation dense |
| Secondaire | Mousse | `#58A038` | Sol forestier, mousse |
| Accent 1 | Ecorce ancienne | `#604828` | Troncs, racines |
| Accent 2 | Spore doree | `#C8A838` | Lumieres filtrees, champignons |
| Ombre | Sous-canopee | `#183810` | Zones ombragees profondes |
| Highlight | Rayon solaire | `#D8E078` | Percees de lumiere |
| Eau | Ruisseau clair | `#68B8A0` | Ruisseaux forestiers |
| Detail | Fougere pale | `#90C858` | Fougeres decoratives |

**Transition vers Port-Coquille :** La foret s'eclaircit, les arbres cedent aux palmiers, le sol devient sableux.

---

#### 3. Port-Coquille (Cote maritime — Arene Eau)

*ATLAS : Un port de peche pittoresque, avec des maisons colorees sur pilotis, des filets qui sechent, des coquillages geants transformes en boutiques.*

| Role | Nom | Hex | Utilisation |
|------|-----|-----|-------------|
| Dominante | Ocean | `#3078B8` | Mer, eau profonde |
| Secondaire | Sable mouille | `#D0B878` | Plage, quais |
| Accent 1 | Corail rose | `#E07868` | Coquillages, details |
| Accent 2 | Ecume | `#E0F0F8` | Vagues, embruns |
| Ombre | Profondeur | `#183860` | Eau profonde, ombres |
| Highlight | Reflet solaire | `#F8F0D8` | Reflets sur l'eau |
| Structure | Bois flotte | `#A08058` | Pilotis, docks |
| Detail | Algue verte | `#48A068` | Algues, vegetation marine |

**Transition vers Roche-Haute :** Le sable laisse place a la roche, le terrain s'eleve, les falaises apparaissent.

---

#### 4. Roche-Haute (Montagnes — Arene Roche)

*ATLAS : Une ville taillee dans la montagne elle-meme. Les maisons sont des grottes amenagees, les escaliers sont sculptes dans la roche. Des strates geologiques multicolores sont visibles partout.*

| Role | Nom | Hex | Utilisation |
|------|-----|-----|-------------|
| Dominante | Granit | `#888078` | Roche, falaises |
| Secondaire | Strate rouge | `#A86048` | Couches sedimentaires |
| Accent 1 | Mineral bleu | `#5880A8` | Cristaux, mineraux |
| Accent 2 | Ambre fossile | `#C89840` | Fossiles, resine |
| Ombre | Caverne | `#383028` | Grottes, ombres profondes |
| Highlight | Quartz | `#E0D8D0` | Eclats de roche claire |
| Structure | Pierre taillee | `#B0A090` | Architecture |
| Detail | Lichen jaune | `#C8B868` | Vegetation de montagne |

**Transition vers Volcanville :** La roche devient plus sombre, des fissures orangees apparaissent, la temperature visuelle augmente.

---

#### 5. Volcanville (Zone volcanique — Arene Feu)

*ATLAS : La ville la plus dramatique. Construite autour d'un volcan semi-actif, avec des riviers de lave solidifiee qui servent de routes. L'air est orange, charge de cendres.*

| Role | Nom | Hex | Utilisation |
|------|-----|-----|-------------|
| Dominante | Basalte | `#383038` | Roche volcanique, sol |
| Secondaire | Lave refroidie | `#684038` | Formations de lave |
| Accent 1 | Lave active | `#E85020` | Riviers de lave, brasiers |
| Accent 2 | Braise | `#F0A030` | Lueurs, particules |
| Ombre | Obsidienne | `#181018` | Ombres maximales |
| Highlight | Cendre chaude | `#C8A080` | Cendres eclairees |
| Ciel | Ciel cendreux | `#806858` | Ciel pollue de cendres |
| Detail | Magma jaune | `#F8D038` | Points les plus chauds |

**Transition vers Cryo-Cite :** Le terrain s'eleve encore, la lave disparait, le froid s'installe, la neige commence.

---

#### 6. Cryo-Cite (Toundra glaciaire — Arene Glace)

*ATLAS : Une cite de glace et d'acier. Les batiments sont des domes transparents contre le froid. La lumiere est bleutee, cristalline. Des aurores boreales dansent dans le ciel nocturne.*

| Role | Nom | Hex | Utilisation |
|------|-----|-----|-------------|
| Dominante | Glace bleue | `#A0D0E8` | Surfaces glacees |
| Secondaire | Neige | `#E8F0F8` | Sol enneige, toits |
| Accent 1 | Aurore verte | `#58E0A0` | Aurores boreales |
| Accent 2 | Aurore mauve | `#A070D8` | Aurores boreales secondaires |
| Ombre | Glace profonde | `#385068` | Ombres, crevasses |
| Highlight | Cristal | `#F0F8F8` | Eclats de glace |
| Structure | Metal froid | `#8890A0` | Structures, domes |
| Detail | Ambre interieur | `#C89840` | Lumieres chaudes interieures |

**Transition vers Electropolis :** La neige fond, le terrain redevient temepre, des structures metalliques apparaissent.

---

#### 7. Electropolis (Plateau technologique — Arene Foudre)

*ATLAS : La ville la plus "moderne" de Pangaea. Des paratonnerres geants, des cables en cuivre, des arcs electriques decoratifs. L'architecture est angulaire, industrielle, mais avec des elements organiques fossiles.*

| Role | Nom | Hex | Utilisation |
|------|-----|-----|-------------|
| Dominante | Acier bleu | `#607088` | Structures metalliques |
| Secondaire | Cuivre | `#C88840` | Cables, conduites |
| Accent 1 | Eclair jaune | `#F8E038` | Arcs electriques, lumieres |
| Accent 2 | Neon cyan | `#38D8E8` | Indicateurs, ecrans |
| Ombre | Metal sombre | `#303840` | Ombres industrielles |
| Highlight | Flash blanc | `#F8F8E0` | Eclairs, eclats |
| Sol | Dalle grise | `#909898` | Sol industriel |
| Detail | Rouille | `#A06030` | Details uses, patine |

**Transition vers Marais-Noir :** Les structures se rouillent, le terrain descend, l'eau stagnante apparait.

---

#### 8. Marais-Noir (Marecages — Arene Poison)

*ATLAS : Un lieu oppressant et beau a la fois. Des cypres couverts de mousse espagnole, de l'eau sombre et huileuse, des lueurs phosphorescentes dans le brouillard. La decomposition est l'esthetique.*

| Role | Nom | Hex | Utilisation |
|------|-----|-----|-------------|
| Dominante | Eau sombre | `#283828` | Eau de marais, sol |
| Secondaire | Mousse morte | `#607848` | Vegetation pourrissante |
| Accent 1 | Phosphore violet | `#A058C0` | Lueurs toxiques |
| Accent 2 | Spore verte | `#78E050` | Champignons, bioluminescence |
| Ombre | Boue noire | `#181810` | Eau profonde, ombres |
| Highlight | Brume grise | `#A8A898` | Brouillard, vapeurs |
| Structure | Bois pourri | `#584830` | Cabanes, ponts |
| Detail | Bulle toxique | `#C0D038` | Bulles dans l'eau |

**Transition vers Ciel-Haut :** Le terrain remonte abruptement, l'air se purifie, les falaises se revelent.

---

#### 9. Ciel-Haut (Plateau aerien — Arene Vol)

*ATLAS : Une ville sur un plateau si haut qu'on est au-dessus des nuages. Les batiments sont legers, aeres, avec des ponts suspendus et des nids geants. Le ciel est d'un bleu impossible.*

| Role | Nom | Hex | Utilisation |
|------|-----|-----|-------------|
| Dominante | Ciel profond | `#3868C0` | Ciel, arriere-plan |
| Secondaire | Nuage blanc | `#E8E8F0` | Nuages, brume |
| Accent 1 | Soleil haut | `#F8D868` | Lumiere directe |
| Accent 2 | Plume doree | `#D0A840` | Nids, plumes, details |
| Ombre | Ciel nocturne | `#182848` | Ombres, profondeur |
| Highlight | Eclat celeste | `#F0F8F8` | Reflets, highlights |
| Structure | Pierre blanche | `#D0C8B8` | Architecture aeree |
| Detail | Mousse d'altitude | `#78A858` | Vegetation rare |

**Transition vers Paleo-Capital :** Le plateau descend vers une vallee ancienne, les ruines apparaissent.

---

#### 10. Paleo-Capital (Capitale fossile — Arene Fossile + Ligue des 4)

*ATLAS : La ville la plus majestueuse. Construite sur les ruines d'une civilisation ancienne, avec des colonnes de pierre geantes, des mosaiques fossiles au sol, un colisee central pour la Ligue. L'ambre domine tout.*

| Role | Nom | Hex | Utilisation |
|------|-----|-----|-------------|
| Dominante | Ambre royal | `#C89840` | Architecture principale |
| Secondaire | Marbre ancien | `#D8D0C0` | Colonnes, sol |
| Accent 1 | Or fossile | `#E8C040` | Details dores, ornements |
| Accent 2 | Lapis bleu | `#3858A8` | Mosaiques, eau |
| Ombre | Pierre ancienne | `#585040` | Ombres architecturales |
| Highlight | Lumiere sacree | `#F8F0D8` | Rayons dans le colisee |
| Structure | Gres rouge | `#B07848` | Murs, arches |
| Detail | Mousse ancienne | `#68884 0` | Vegetation sur les ruines |

### 3.3 Palette UI

**PIXEL :** L'UI utilise une palette dediee qui reste lisible sur tous les fonds.

| Element | Hex | RGB | Utilisation |
|---------|-----|-----|-------------|
| Fond de fenetre | `#F0E8D0` | 240, 232, 208 | Fond des menus, panneaux |
| Fond de fenetre sombre | `#D8C8A8` | 216, 200, 168 | Variante plus contrastee |
| Bordure de fenetre | `#503820` | 80, 56, 32 | Cadres des menus |
| Bordure accent | `#886830` | 136, 104, 48 | Bordure double |
| Texte principal | `#282028` | 40, 32, 40 | Texte noir chaud |
| Texte secondaire | `#685848` | 104, 88, 72 | Descriptions, sous-textes |
| Texte selection | `#F0E8D0` | 240, 232, 208 | Texte sur fond selectionne |
| Fond selection | `#C89840` | 200, 152, 64 | Highlight de selection ambre |
| Curseur | `#E85020` | 232, 80, 32 | Fleche de selection rouge-orange |
| Barre HP verte | `#48B838` | 72, 184, 56 | PV hauts (>50%) |
| Barre HP jaune | `#E8C038` | 232, 192, 56 | PV moyens (20-50%) |
| Barre HP rouge | `#D83028` | 216, 48, 40 | PV bas (<20%) |
| Barre EXP | `#3078B8` | 48, 120, 184 | Barre d'experience bleue |
| Fond barre HP | `#383028` | 56, 48, 40 | Arriere-plan des barres |
| Fond dialogue | `#F0E8D0` | 240, 232, 208 | Boite de dialogue |
| Bordure dialogue | `#503820` | 80, 56, 32 | Cadre de la boite |

### 3.4 Palette de Combat

| Element | Hex | Utilisation |
|---------|-----|-------------|
| Fond de combat (plaine) | `#C8D888` | Sol herbeux |
| Fond de combat (grotte) | `#706058` | Sol rocheux |
| Fond de combat (eau) | `#68A8D0` | Surface aquatique |
| Fond de combat (sable) | `#E0C878` | Sol desertique |
| Fond de combat (neige) | `#D8E8F0` | Sol enneige |
| Fond de combat (volcan) | `#583830` | Sol volcanique |
| Fond de combat (foret) | `#487838` | Sol forestier |
| Fond de combat (interieur) | `#B0A090` | Sol de batiment |
| Plateforme allie | `#A0C868` | Socle du dino allie |
| Plateforme ennemi | `#C0A870` | Socle du dino ennemi |
| Ombre dino | `#00000040` | Ombre circulaire (semi-transparente) |

### 3.5 Couleurs des 14 Types

**ZEPHYR :** Chaque type a une couleur primaire et une couleur secondaire. La primaire est utilisee pour les badges, la secondaire pour les fonds de type.

| Type | Primaire (Hex) | Secondaire (Hex) | Logique visuelle |
|------|---------------|-----------------|------------------|
| Roche | `#A89070` | `#D0C0A8` | Gris-brun, pierre naturelle |
| Eau | `#3880C0` | `#90C8E8` | Bleu ocean |
| Feu | `#E05028` | `#F0A060` | Rouge-orange, braise |
| Plante | `#48A030` | `#90D068` | Vert feuillage |
| Glace | `#68C8E0` | `#C0E8F0` | Cyan cristallin |
| Vol | `#7898D0` | `#B0C8E8` | Bleu ciel venteux |
| Terre | `#B08038` | `#D0B070` | Brun terre, argile |
| Foudre | `#E8C020` | `#F0E078` | Jaune electrique |
| Poison | `#9048A8` | `#C888D8` | Violet toxique |
| Acier | `#6880A0` | `#A8B8C8` | Gris metallique bleu |
| Ombre | `#484058` | `#786880` | Pourpre sombre |
| Lumiere | `#E8D848` | `#F0E8A0` | Or lumineux |
| Sable | `#C8A850` | `#E0D098` | Jaune-brun desert |
| Fossile | `#907050` | `#C0A888` | Brun-beige, os ancien |

### 3.6 Regles de Teinture Jour / Nuit

**ATLAS :** Le cycle jour/nuit utilise un overlay de couleur multiplie sur l'ecran.

| Moment | Teinte overlay | Opacite | Heures in-game |
|--------|---------------|---------|----------------|
| Aube | `#F8D0A0` | 15% | 05:00 - 07:00 |
| Matin | Aucune | 0% | 07:00 - 10:00 |
| Midi | `#F8F0D8` | 5% | 10:00 - 14:00 |
| Apres-midi | Aucune | 0% | 14:00 - 17:00 |
| Crepuscule | `#E87830` | 20% | 17:00 - 19:00 |
| Soir | `#3838A0` | 25% | 19:00 - 21:00 |
| Nuit | `#181838` | 40% | 21:00 - 04:00 |
| Nuit profonde | `#101030` | 50% | 00:00 - 03:00 |

**Regles :**
- Les interieurs ne sont PAS affectes par le tint (eclairage artificiel)
- Les combats ne sont PAS affectes (pour la lisibilite)
- Les zones de grotte/caverne ont leur propre eclairage fixe (sombre + torches)
- Les lumieres artificielles (fenetres, lampadaires, torches) deviennent plus visibles avec l'overlay de nuit
- Transition entre les etats : 30 secondes reelles, interpolation lineaire

---

## 4. Character Design

### 4.1 Joueur — Specs Overworld

**NOVA :** Le joueur est un pre-ado (12 ans environ). Son design doit etre neutre en genre — le joueur choisit Garcon ou Fille en debut de partie, mais les deux designs suivent les memes principes.

#### Design commun
- **Proportions overworld** : 1.5 tete de haut (16x24, tete = ~8px de haut)
- **Chapeau/casquette** : Element distinctif principal, toujours visible dans les 4 directions. Style "explorateur" avec un insigne fossile
- **Sac a dos** : Toujours visible de dos et de profil. Petit, pratique, avec un badge Dinodex
- **Vetements** : Short/pantalon d'exploration, bottes de marche, gilet a poches. Couleurs : brun terre `#886830`, beige `#D8C8A8`, accent rouge-orange `#E05028`
- **Palette personnage** :
  - Peau : `#F0C888` (clair) ou `#C08850` (fonce) — choix du joueur
  - Cheveux : `#503820` (brun) ou `#282028` (noir) ou `#C89840` (blond) — choix du joueur
  - Vetement principal : `#886830`
  - Vetement accent : `#E05028`
  - Bottes : `#503820`

#### Sprite Sheet Structure (64 x 96 px)
```
Colonne :   Bas     Gauche   Droite   Haut
Ligne 1 :   idle    idle     idle     idle      (16x24 chaque)
Ligne 2 :   walk1   walk1    walk1    walk1
Ligne 3 :   walk2   walk2    walk2    walk2
Ligne 4 :   walk3   walk3    walk3    walk3
```

Le cycle de marche est : idle -> walk1 -> walk2 -> walk3 -> walk2 -> walk1 -> idle (symetrique).

#### Animations speciales du joueur
- **Lancer de Jurassic Ball** : 3 frames, bras en mouvement (utilise uniquement en overworld pour les lancers scriptees)
- **Velo/Monture** : Le joueur peut monter un dino terrestre. Sprite assis 16x24, pattes du dino en dessous

### 4.2 Archetypes de PNJ

**NOVA :** Chaque PNJ doit etre identifiable par sa silhouette et sa palette, meme en 16x24.

#### Dresseurs (Types de combat)

| Archetype | Silhouette distinctive | Palette dominante | Notes |
|-----------|----------------------|-------------------|-------|
| Jeune Dresseur | Casquette, short | `#E05028` rouge | Le plus courant, enfant |
| Randonneur | Grand sac, chapeau large | `#886830` brun | Routes de montagne |
| Nageuse/Nageur | Maillot, cheveux mouilles | `#3880C0` bleu | Routes aquatiques |
| Scientifique | Blouse blanche, lunettes | `#D8D0C0` blanc | Labos, grottes |
| Eleveur | Tablier, bottes | `#48A030` vert | Zones rurales, fermes |
| Karateka | Kimono, ceinture | `#E8C020` jaune | Dojos, arenes |
| Gothique | Cape, capuche | `#484058` pourpre | Zones sombres, nuit |
| As Dresseur | Manteau long, insigne | `#C89840` ambre | Post-game, routes tardives |

#### Habitants

| Archetype | Silhouette distinctive | Notes |
|-----------|----------------------|-------|
| Villageois | Simple, pas de couvre-chef | Varie par biome |
| Vendeur | Tablier + chapeau | En boutique |
| Infirmiere Paleo | Blouse rose + croix | Centres de soin |
| Garde | Uniforme + casquette | Arene, batiments |
| Vieux sage | Barbe, canne | Donne des conseils |
| Enfant | Petit (12x20), casquette | Court, ne combat pas |

### 4.3 Personnages Cles — Design Notes

#### Professeur Paleo

**NOVA :** C'est le mentor bienveillant. Design inspire d'un paleontologue de terrain, pas d'un scientifique de labo.

- **Silhouette** : Grand, chapeau de brousse, barbe courte, veste pleine de poches avec des outils qui depassent
- **Palette** : Kaki `#A09058`, brun fonce `#503820`, blanc casse `#E8D8B0`
- **Accessoires** : Loupe, marteau de geologue a la ceinture, Dinodex prototype
- **Portrait 32x32** : Expression chaleureuse, sourire sous la barbe, yeux petillants derriere des lunettes rondes
- **Sprite overworld** : Reconnaissable par son chapeau large et sa silhouette haute

#### Rival REX

**NOVA :** Le petit-fils du Prof. Commence arrogant, finit humble. Son design evolue subtilement au cours du jeu.

- **Debut** : Cheveux en pics, T-shirt rouge `#D83028`, air hautain, posture droite
- **Milieu** : Meme tenue mais plus "usee", cheveux un peu plus calmes
- **Fin** : Ajoute une veste d'explorateur par-dessus, posture plus detendue
- **Palette** : Rouge vif `#D83028`, jean bleu fonce `#384068`, peau, cheveux noirs
- **Trait distinctif overworld** : Cheveux en pics, toujours reconnaissable meme en 16x24

#### Escadron Meteore — Grunts

**NOVA :** L'Escadron Meteore a une esthetique "apocalypse chic". Leurs uniformes evoquent un impact d'asteroide.

- **Uniforme** : Combinaison gris fonce `#484048` avec des motifs de "cratere" orange `#E87830`
- **Casque** : Forme arrondie evoquant un asteroide, visiere noire
- **Logo** : Une meteore tombante avec une trainee de feu (sur le torse)
- **Palette grunt** : Gris `#484048`, orange `#E87830`, noir `#282028`, rouge `#C03020`
- **Silhouette** : Casque arrondi = immediatement reconnaissable comme ennemi

#### Commandant Impact

**NOVA :** Le leader de l'Escadron. Ancien champion, marque par la perte de son dino. Son design montre la dualite grandeur passee / obsession presente.

- **Silhouette** : Manteau long qui evoque une cape, posture rigide, grand
- **Palette** : Noir `#181018`, ambre fonce `#886830`, rouge profond `#802020`, metal `#6880A0`
- **Trait distinctif** : Un pendentif fossile autour du cou (le fossile de son dino perdu)
- **Cicatrice** : Une marque sur la joue gauche, en forme de griffure — justifiee par l'histoire
- **Gants** : Gants en cuir noir, toujours visibles meme en sprite overworld
- **Portrait** : Regard intense mais triste, pas "mechant cartoon" — un antagoniste complexe

---

## 5. Dinosaur Design Guidelines

### 5.1 Philosophie de Design des Dinosaures

**NOVA :** C'est LA section critique. Nos 150 dinos doivent chacun etre uniques, reconnaissables, et donner envie d'etre captures. Voici les regles fondamentales :

#### La Regle des 3 Secondes
Un joueur doit pouvoir, en 3 secondes :
1. Identifier le dino (silhouette unique)
2. Deviner son type (indices visuels)
3. Ressentir une emotion (cute, impressionnant, effrayant, majestueux)

#### La Regle de la Silhouette
Chaque dino, meme en silhouette noire unie, doit etre distinguable de tous les autres. Cela signifie :
- Pas deux dinos avec la meme forme de tete
- Des proportions variees (trapu, elance, massif, aerien)
- Des elements distinctifs clairs (cretes, cornes, ailes, queues)

### 5.2 Stade Bebe — Langage Visuel

**NOVA :** Les bebes sont le stade "attachement". Le joueur doit vouloir les proteger.

#### Proportions
- **Ratio tete/corps** : 1:1.2 (tete presque aussi grande que le corps)
- **Yeux** : 30-35% de la surface du visage, ronds, brillants avec un highlight blanc
- **Pattes** : Courtes, un peu maladroites, largement ecartees pour un look instable
- **Queue** : Courte, souvent enroulee ou relevee

#### Traits universels des Bebes
- Peau/ecailles plus lisses, moins de details que les stades ulterieurs
- Couleurs plus pastel, plus claires que l'adulte (la couleur "mure" en evoluant)
- Petit museau arrondi (pas de dents visibles ou tres petites)
- Expression : curiosite, joie, maladresse

#### Taille sprite combat
- **40 x 40 px** — ils sont petits et on doit le sentir
- Idle animation : 3 frames, leger rebond (le bebe sautille sur place)

### 5.3 Stade Jeune — Langage Visuel

**NOVA :** L'adolescent dino. Plus grand, plus confiant, mais pas encore mature.

#### Proportions
- **Ratio tete/corps** : 1:1.8 (tete encore relativement grande mais le corps grandit)
- **Yeux** : 22-25% du visage, toujours ronds mais plus determines
- **Pattes** : Plus longues, proportionnees, posture plus assuree
- **Queue** : S'allonge, peut montrer des caracteristiques de type

#### Traits universels des Jeunes
- Les traits de type commencent a se manifester fortement (flammes naissantes, cristaux qui poussent, nageoires qui s'etendent)
- Couleurs a mi-chemin entre bebe et adulte — plus saturees que le bebe
- Debut de motifs distinctifs (rayures, taches, plaques)
- Les dents/griffes deviennent visibles
- Expression : determination, energie, parfois maladresse residuelle

#### Taille sprite combat
- **48 x 48 px**
- Idle animation : 3 frames, balancement + regard actif

### 5.4 Stade Adulte — Langage Visuel

**NOVA :** L'adulte est la forme finale. Majestueux, puissant, ou terrifiants — jamais ordinaire.

#### Proportions
- **Ratio tete/corps** : 1:2.5 a 1:3.5 (proportions realistes)
- **Yeux** : 12-18% du visage, formes variees (ronds amicaux, fentes predateurs, yeux expressifs)
- **Pattes** : Proportionnees a la morphologie reelle du dino inspire
- **Queue** : Complete, souvent un element de design majeur (arme, stabilisateur, element de type)

#### Traits universels des Adultes
- Expression complete du type : flammes, glace, plantes, eclairs — clairement visibles
- Palette de couleurs finale, la plus saturee et contrastee
- Motifs complexes et distinctifs (ecailles detaillees, marques uniques)
- Posture qui exprime le temperament (agressif = penche en avant, defensif = recroqueville, majestueux = tete haute)
- Accessoires naturels au maximum (cornes, cretes, plaques, voiles, griffes)

#### Taille sprite combat
- **56 x 56 px** (jusqu'a 64x64 pour legendaires)
- Idle animation : 3 frames, mouvement de respiration + trait de personnalite (griffe le sol, deploie les ailes partiellement, gronde)

### 5.5 Communication Visuelle des Types

**ZEPHYR :** Le type d'un dino doit etre lisible instantanement. Voici comment chaque type se manifeste visuellement :

| Type | Indices de couleur | Indices morphologiques | Effets visuels |
|------|-------------------|----------------------|----------------|
| Roche | Gris, brun, touches de mineral | Peau rocheuse, plaques, cristaux incrustes | Particules de poussiere autour des pieds |
| Eau | Bleu, cyan, blanc ecume | Nageoires, membranes, peau lisse et brillante | Gouttelettes, reflets |
| Feu | Rouge, orange, jaune | Ecailles rougeoyantes aux extremites, cretes enflammees | Braises, ondulation de chaleur |
| Plante | Vert, brun ecorce, fleurs | Feuilles, lianes, mousse, fleurs sur le corps | Spores, petales qui flottent |
| Glace | Bleu pale, cyan, blanc cristal | Cristaux de glace, peau translucide, souffle visible | Flocons, givre |
| Vol | Bleu clair, blanc, gris plume | Ailes (membranes ou plumes), corps aerodynamique | Courants d'air, plumes qui flottent |
| Terre | Brun, ocre, beige | Peau craquelée comme terre seche, griffes puissantes | Poussiere, seisme subtil |
| Foudre | Jaune, bleu electrique | Motifs en eclair sur le corps, poils/plumes herissees | Arcs electriques, etincelles |
| Poison | Violet, vert acide, rose toxique | Glandes, epines venimeuses, couleurs d'avertissement | Bulles toxiques, vapeurs |
| Acier | Gris metal, bleu acier | Plaques metalliques, armure naturelle, reflets | Eclats metalliques |
| Ombre | Pourpre, noir, gris sombre | Silhouette partielle, yeux brillants, forme fluide | Ombres qui bougent, fumee sombre |
| Lumiere | Or, blanc, jaune radiant | Aura lumineuse, cristaux transparents, motifs solaires | Rayons, eclats de lumiere |
| Sable | Jaune sable, beige, brun | Ecailles de desert, collerettes, camouflage | Grains de sable, vent |
| Fossile | Brun os, beige, ambre | Os apparents, ambre incrusté, elements squelettiques | Poussiere ancienne, lueur d'ambre |

### 5.6 Regles de Fusion Visuelle (Double Type)

**ZEPHYR :** Quand un dino a deux types, on applique la regle du 60/40 :

- **Type primaire (60%)** : Determine la forme generale, la posture et la majorite de la palette
- **Type secondaire (40%)** : Ajoute des elements morphologiques, des accents de couleur et des effets visuels

**Exemples concrets :**
- **Feu/Roche** (PYREX adulte) : Corps principalement rouge-orange avec des plaques de roche grise. Lave qui coule entre les interstices des plaques.
- **Eau/Glace** (AQUADON adulte) : Corps bleu ocean avec des cristaux de glace sur le dos et la queue. Nageoires aux bords givres.
- **Plante/Terre** (FLORASAUR adulte) : Corps vert avec des racines brunes qui s'etendent depuis les pattes. Sol craquele sous ses pas, fleurs au sommet.

**Cas speciaux :**
- Les dinos qui GAGNENT un type en evoluant montrent le nouveau type progressivement (d'abord subtil au stade Jeune, puis pleinement au stade Adulte)
- Le type Fossile est toujours visible par des os / ambre apparents, quel que soit l'autre type

### 5.7 Specs de Sprites de Combat

#### Vue de Face (Dino ennemi)

```
+------------------+
|                  |
|   Sprite dino    |  56x56 (adulte)
|   face avant     |  48x48 (jeune)
|                  |  40x40 (bebe)
+------------------+
|  Ombre au sol    |  Ellipse 60% de la largeur
+------------------+
```

- Le dino regarde vers la gauche (face au dino du joueur)
- Position : quadrant superieur droit de l'ecran de combat
- L'ombre est une ellipse semi-transparente noire `#000000` a 25% d'opacite

#### Vue de Dos (Dino allie)

```
+------------------+
|                  |
|   Sprite dino    |  Memes tailles que face
|   vue arriere    |
|                  |
+------------------+
|  Ombre au sol    |
+------------------+
```

- Le dino regarde vers la droite (face a l'ennemi)
- Position : quadrant inferieur gauche de l'ecran de combat
- Plus grand visuellement que l'ennemi car plus proche de la "camera"

#### Frames d'animation de combat

| Animation | Frames | Description |
|-----------|--------|-------------|
| Idle | 3 | Boucle : respiration (leger zoom in/out de 1-2px verticalement) |
| Attaque physique | 5 | Avance de 4-8px vers l'ennemi, frappe (flash), recule |
| Attaque speciale | 4 | Brille/pulse, release d'effet (l'effet est un sprite separe) |
| Touche (prend des degats) | 2 | Flash blanc complet (1 frame), recul de 2-4px (1 frame), retour |
| Critique | 3 | Flash blanc + secoue (2x gauche-droite, 1px chaque) |
| KO (faint) | 4 | Tombe vers le bas, fondu progressif (opacite 100->75->50->0) |
| Entree (envoi) | 3 | Materialisation depuis la Jurassic Ball (flash blanc -> silhouette -> sprite complet) |
| Rappel | 3 | Inverse de l'entree (sprite -> flash rouge -> disparait) |

### 5.8 Overworld Follower Sprites

**NOVA :** Le premier dino de l'equipe suit le joueur en overworld. Tous les dinos ont un sprite follower 16x16 super-deforme.

#### Regles du sprite follower
- Taille fixe : **16 x 16 px** quel que soit le stade (le T-Rex adulte et le bebe raptor font la meme taille en overworld — convention de gameplay)
- Style "chibi" : Tete = 50-60% du sprite, corps simplifie
- Reconnaissable par : couleur dominante + silhouette simplifiee + 1 trait distinctif (la crete, les cornes, la flamme)
- 4 directions, 4 frames chaque (meme structure que le joueur)
- Sprite sheet : 64 x 64 px (4 colonnes x 4 lignes)

### 5.9 Les 3 Starters — Design Detaille

---

#### PYREX (Feu -> Feu/Roche)

*Inspire du : Compsognathus (bebe/jeune) -> Carnotaurus (adulte)*

##### Stade 1 : PYREX Bebe (Feu)
- **Concept** : Un petit compy enjoue avec une flamme au bout de la queue
- **Silhouette** : Bipede, petites pattes arriere, bras minuscules, queue relevee avec flamme
- **Tete** : Ronde, grands yeux rouge-orange `#E05028`, museau court et arrondi
- **Corps** : Ecailles rouge-orange `#D85030`, ventre creme `#F0D8A8`
- **Trait distinctif** : Flamme au bout de la queue (2x3 px, jaune-orange, animee 2 frames)
- **Personality pose** : Penche en avant, curieux, queue relevee

##### Stade 2 : PYREX Jeune (Feu)
- **Concept** : Compy qui grandit, commence a montrer des cornes et des plaques de roche
- **Silhouette** : Plus grand, deux petites cornes naissent au-dessus des yeux, posture plus droite
- **Tete** : Allongee, cornes de 2-3px, yeux plus determines
- **Corps** : Rouge plus profond `#C04020`, plaques gris-roche `#887868` apparaissent sur les epaules
- **Trait distinctif** : Flamme de queue plus grande (3x4 px), des veines rougeoyantes sur les plaques
- **Personality pose** : Posture de defi, une patte en avant

##### Stade 3 : PYROTAURUS Adulte (Feu/Roche)
- **Concept** : Un carnotaurus dont le corps est un melange de lave et de roche — un tank de feu
- **Silhouette** : Massif, grandes cornes, posture legerement penchee en avant, queue puissante
- **Tete** : Carree, 2 grandes cornes au-dessus des yeux (`#A08868` roche), machoire puissante, yeux feroces `#E05028`
- **Corps** : Plaques de roche grise `#807068` avec des fissures de lave orange `#E87830` entre elles. Ventre plus clair `#C09868`
- **Flamme** : La queue se termine par une flamme permanente (4x5 px, animee 3 frames), plus des braises autour des cornes
- **Pieds** : Griffes de roche, laissent des traces de braise
- **Palette complete** : `#C04020`, `#E87830`, `#807068`, `#A08868`, `#F0A030`, `#F0D8A8`, `#503820`, `#282028`

---

#### AQUADON (Eau -> Eau/Glace)

*Inspire du : Bebe plesiosaure (bebe/jeune) -> Elasmosaurus (adulte)*

##### Stade 1 : AQUADON Bebe (Eau)
- **Concept** : Un adorable petit plesiosaure avec de grands yeux et des nageoires maladroites
- **Silhouette** : Quadrupede, long cou pour sa taille, petites nageoires, corps arrondi
- **Tete** : Ronde, grands yeux bleu doux `#68B8D8`, petit sourire, pas de dents visibles
- **Corps** : Bleu clair `#68A8D0`, ventre blanc-bleu `#D0E8F0`
- **Trait distinctif** : Petite collerette translucide sur la tete (comme une nageoire), peau luisante
- **Personality pose** : Cou tendu vers le haut, curieux, une nageoire levee

##### Stade 2 : AQUADON Jeune (Eau)
- **Concept** : Le cou s'allonge, les nageoires grandissent, debut de cristaux de glace
- **Silhouette** : Cou plus long et gracieux, nageoires plus amples, corps plus fusele
- **Tete** : Plus allongee, yeux toujours doux mais plus intelligents
- **Corps** : Bleu moyen `#4890C0`, marques plus claires `#90C8E0` sur les flancs, premiers cristaux de glace `#C0E8F0` sur le dos
- **Trait distinctif** : Souffle visible (petit nuage de froid a la bouche, 2 frames)
- **Personality pose** : Cou en S elegant, nageoires deployees

##### Stade 3 : GLACIODON Adulte (Eau/Glace)
- **Concept** : Un elasmosaurus majestueux avec une armure de glace cristalline sur le dos et le cou
- **Silhouette** : Tres long cou, corps massif, 4 grandes nageoires, cretes de glace le long de la colonne
- **Tete** : Elegante, yeux bleu profond `#3068A0`, machoire fine mais redoutable
- **Corps** : Bleu profond `#3878B0`, ventre argenté `#C8D8E0`
- **Glace** : Cristaux de glace `#A0D8E8` le long du cou et du dos, translucides avec des reflets `#E8F0F8`
- **Nageoires** : Bords givres, effet de givre qui s'etend en combat
- **Palette complete** : `#3878B0`, `#4890C0`, `#A0D8E8`, `#E8F0F8`, `#C8D8E0`, `#3068A0`, `#183858`, `#282028`

---

#### FLORASAUR (Plante -> Plante/Terre)

*Inspire du : Protoceratops bebe (bebe/jeune) -> Ankylosaurus (adulte)*

##### Stade 1 : FLORASAUR Bebe (Plante)
- **Concept** : Un petit protoceratops avec une collerette couverte de bourgeons de fleurs
- **Silhouette** : Quadrupede trapu, petite collerette, queue courte, pattes courtes
- **Tete** : Large, collerette arrondie avec 3-4 bourgeons verts, grands yeux verts `#58A038`, petit bec
- **Corps** : Vert tendre `#78B848`, ventre jaune-vert `#C8D868`
- **Trait distinctif** : Bourgeons sur la collerette (qui fleuriront en evoluant)
- **Personality pose** : Assis, tete inclinee, air innocent

##### Stade 2 : FLORASAUR Jeune (Plante)
- **Concept** : Le corps s'epaissit, la collerette fleurit, des racines apparaissent aux pattes
- **Silhouette** : Plus large, collerette plus grande avec des fleurs ouvertes, pattes plus fortes
- **Tete** : Collerette en feuilles, 2-3 fleurs oranges `#E8A038` ouvertes, yeux determines
- **Corps** : Vert plus profond `#489030`, debut de motifs "terre craquelée" brun `#886830` sur les flancs
- **Trait distinctif** : Racines visibles aux pieds (2-3 px de lignes brunes), fleurs en eclosion
- **Personality pose** : Posture solide, tete haute, collerette deployee

##### Stade 3 : TERRAFLORIS Adulte (Plante/Terre)
- **Concept** : Un ankylosaurus-jardin. Son dos est un veritable ecosysteme miniature avec des fleurs, de la mousse, de petits arbres. Ses pattes sont des racines.
- **Silhouette** : Tres large, bas sur pattes, dos bombe couvert de vegetation, queue en massue avec une fleur
- **Tete** : Petite par rapport au corps, yeux doux et sages `#388020`, bec solide
- **Corps** : Vert-brun `#587840` pour le corps, `#886830` pour les motifs de terre craquelée. Le dos est un jardin : mousse `#58A038`, fleurs `#E8A038` et `#D83868`, petites feuilles `#90D068`
- **Pattes** : Ressemblent a des racines d'arbre `#685030`, s'enfoncent dans le sol
- **Queue** : Massue couverte de mousse, une grande fleur au bout
- **Palette complete** : `#587840`, `#489030`, `#90D068`, `#886830`, `#685030`, `#E8A038`, `#D83868`, `#282028`

---

### 5.10 PANGAEON — Le Legendaire (Fossile/Lumiere)

**NOVA :** PANGAEON est le gardien de l'equilibre, l'ame de Pangaea. Son design doit inspirer le sacre.

*Inspire du : Titanosaure + elements mythologiques*

- **Taille sprite** : 64 x 64 px (taille maximale)
- **Silhouette** : Un titanosaure majestueux dont le corps est a moitie fossile, a moitie lumiere vivante
- **Cote gauche** : Squelette fossile visible — os blancs-beiges `#E8D8C0`, ambre `#C89840` qui remplit les jointures
- **Cote droit** : Chair vivante, lumineuse — ecailles dorees `#E8C040` avec des veines de lumiere blanche `#F8F0D8`
- **Tete** : Crane allonge, yeux qui brillent d'une lumiere ambre `#C89840`, couronne de cristaux fossiles
- **Cou** : Long, la transition fossile/vivant est visible — comme si la vie revenait dans les os
- **Dos** : Des cristaux d'ambre `#C89840` poussent comme des stalagmites, emettant une lumiere chaude
- **Queue** : Se termine en spirale fossile, avec un eclat de lumiere au bout
- **Aura** : Particules d'ambre et de lumiere qui flottent autour de lui (effet de particules, pas dans le sprite)
- **Palette** : `#E8D8C0`, `#C89840`, `#E8C040`, `#F8F0D8`, `#907050`, `#685030`, `#503820`, `#181018`
- **Idle animation** : 3 frames, respiration + les cristaux pulsent de lumiere (cycle lumineux)

---

## 6. Environment Art

### 6.1 Specifications des Tilesets

**ATLAS :** Chaque biome a son propre tileset. Un tileset = 256 x 256 px (16 x 16 tiles). Chaque biome necessite 2-3 tilesets (exterieur, interieur, special).

#### Structure commune de chaque tileset

Tous les tilesets contiennent ces elements de base :

| Categorie | Tiles | Notes |
|-----------|-------|-------|
| Sol (walkable) | 8-12 variantes | Herbe, terre, dalle, sable — avec variations pour eviter la repetition |
| Sol (decoratif) | 4-6 variantes | Fleurs, cailloux, fissures — superposables au sol de base |
| Mur / Obstacle | 8-10 variantes | Rochers, arbres, murs — tiles de collision |
| Eau | 4 tiles + 4 bords | Eau animee (4 frames) + transitions sol-eau |
| Escalier / Pente | 4 (haut, bas, gauche, droite) | Changement de niveau |
| Batiment (exterieur) | 16-24 tiles | Murs, toit, porte, fenetres — meta-tiles 32x32 |
| Batiment (porte) | 2 tiles | Entree dans un interieur |
| Herbes hautes | 4 tiles (animees) | Zones de rencontre sauvage |
| Decoration | 8-12 variantes | Panneaux, lampes, bancs, fossiles |
| Bord de map | 4-8 tiles | Arbres denses, falaises — bloquent le joueur |

### 6.2 Tilesets par Biome

---

#### Bourg-Nid (Plaines pastorales)

**Tiles specifiques :**
- Sol : Herbe verte douce, chemin de terre battue, pavés de pierre plate
- Decoration : Clotures en bois (horizontales et verticales, coins), mangeoires a dinos, nids en paille, oeufs fossiles decoratifs, boites aux lettres
- Batiments : Maisons en bois et chaume, 2 etages max. Toits en triangle, poutres visibles. Labo du Prof en pierre, plus grand (3x2 meta-tiles)
- Vegetation : Arbres fruitiers (ronds, canopee verte + fruits orange), buissons fleuris, parterre de fleurs
- Eau : Petit ruisseau traversant le village (3-4 tiles de large)
- Special : Panneau "Bourg-Nid" a l'entree, statue de dino fondateur sur la place

---

#### Ville-Fougere (Foret ancienne)

**Tiles specifiques :**
- Sol : Terre forestiere avec feuilles, racines apparentes, mousse
- Decoration : Champignons lumineux, toiles d'araignee, troncs creux, souches, pierres moussues
- Batiments : Maisons-arbres (construites dans/autour des troncs), ponts de bois entre les arbres, escaliers en spirale autour des troncs
- Vegetation : Arbres geants (troncs de 2 tiles de large, canopee de 4x4), fougeres geantes (2 tiles de haut), lianes pendantes
- Eau : Ruisseau forestier vert-bleu, mare stagnante avec nenuphar
- Special : Arene dans un arbre creux geant (interieur = racines + lumiere filtree)

---

#### Port-Coquille (Cote)

**Tiles specifiques :**
- Sol : Sable sec, sable mouille (plus fonce), planches de dock, dalle de port
- Decoration : Filets de peche, tonneaux, ancres, coquillages geants (decoratifs), phare (meta-tile 2x4), bateaux (meta-tile 3x2)
- Batiments : Maisons sur pilotis, toits en tuile bleue/blanche, balcons avec vue mer. Certaines maisons dans des coquillages geants
- Vegetation : Palmiers (tronc fin, feuilles en etoile), algues echouees
- Eau : Ocean (bleu profond, anime 4 frames), vagues sur la plage (ecume blanche, anime 3 frames), eau de port (plus calme)
- Special : Marche aux poissons/dinos marins, arena = bassin ouvert

---

#### Roche-Haute (Montagne)

**Tiles specifiques :**
- Sol : Roche grise, gravier, sentier de montagne, neige legere en altitude
- Decoration : Strates geologiques (bandes colorees dans les falaises), fossiles incrustes dans la roche, stalactites/stalagmites, torches dans les grottes
- Batiments : Maisons creusees dans la montagne (portes dans la roche), toits plats en pierre, cheminées
- Vegetation : Arbres de montagne (pins courts), mousse, lichen jaune
- Eau : Cascade (anime 4 frames, ecume en bas), lac de montagne
- Special : Escaliers sculptes dans la roche, pont naturel en pierre

---

#### Volcanville (Zone volcanique)

**Tiles specifiques :**
- Sol : Basalte noir, cendres grises, roche craquelée, obsidienne (reflets)
- Decoration : Fumerolles (anime 3 frames), cristaux de soufre jaune, os fossilises (calcines), geysers
- Batiments : Architecture en metal et roche ignee, murs epais, fenetres etroites (protection chaleur), forges
- Vegetation : Quasiment absente — quelques plantes resistantes rouges/oranges
- Eau : LAVE a la place de l'eau (rouge-orange, anime 4 frames, plus lent que l'eau). Riviers de lave, lac de lave
- Special : Volcan en arriere-plan (meta-tile 8x6, fumee animee), pont au-dessus de la lave

---

#### Cryo-Cite (Toundra)

**Tiles specifiques :**
- Sol : Neige compactee, glace (reflets), roche gelee, permafrost
- Decoration : Stalagmites de glace, congeres, empreintes geantes dans la neige, cristaux de glace
- Batiments : Domes geodesiques transparents (reflets), structures metalliques isolantes, sas d'entree doubles
- Vegetation : Sapins enneiges (rares), mousse arctique, pas de fleurs
- Eau : Lac gele (surface brillante, anime 2 frames — reflets qui bougent), eau sous la glace visible
- Special : Aurores boreales en arriere-plan (effet de parallaxe, anime 6 frames)

---

#### Electropolis (Plateau industriel)

**Tiles specifiques :**
- Sol : Dalles metalliques, grilles d'aeration, cables au sol, pavés industriels
- Decoration : Paratonnerres, transformateurs, cables en cuivre, ecrans/panneaux lumineux, generateurs
- Batiments : Architecture angulaire, metal + verre, toits plats avec antennes, usines reconverties en maisons
- Vegetation : Jardins interieurs visibles a travers les vitres, pots de plantes sur les balcons
- Eau : Pas d'eau naturelle — canaux de refroidissement, bassins industriels
- Special : Arcs electriques entre deux tours (anime 3 frames, flash), train electrique (meta-tile mobile)

---

#### Marais-Noir (Marecages)

**Tiles specifiques :**
- Sol : Boue (variantes claires/foncees), planches de bois pourri, racines aeriennes, tourbe
- Decoration : Champignons phosphorescents (anime 2 frames, glow), mousse espagnole pendante, cranes d'animaux, bulles dans l'eau
- Batiments : Cabanes sur pilotis (bois gris-vert), ponts branlants, huttes en roseau
- Vegetation : Cypres (troncs tordus, mousse), roseaux, nenuphas, plantes carnivores
- Eau : Eau de marais (vert-noir, opaque, anime 4 frames — bulles occasionnelles), marecage infranchissable
- Special : Brouillard de parallaxe (couche semi-transparente qui se deplace lentement)

---

#### Ciel-Haut (Plateau aerien)

**Tiles specifiques :**
- Sol : Pierre blanche, herbe d'altitude, mousse, bord de falaise (avec vide visible en dessous)
- Decoration : Nids geants (pteranodons), plumes, anemometres, cloches a vent, ponts suspendus
- Batiments : Architecture legere — structures en bambou et tissu, toits en voile, terrasses ouvertes
- Vegetation : Herbe d'altitude rase, fleurs alpines, pas d'arbres hauts (trop de vent)
- Eau : Sources d'altitude (eau cristalline), cascade qui tombe dans le vide (spectaculaire)
- Special : Nuages en dessous du joueur (parallaxe), pteranodons qui volent en arriere-plan

---

#### Paleo-Capital (Capitale)

**Tiles specifiques :**
- Sol : Marbre ancien, mosaiques fossiles (motifs de dinos dans le sol), paves en ambre
- Decoration : Colonnes (meta-tile 1x3), statues de dinos anciens, fontaines, drapeaux, torches ornementales, vitraux
- Batiments : Architecture megalithique — colonnes, arches, domes. Melange de pierre taillée et de metal ambre. Le Colisee de la Ligue (meta-tile 8x8 minimum)
- Vegetation : Jardins soignes, arbres centenaires, haies taillees
- Eau : Fontaines decoratives, canaux artificiels
- Special : Mosaique du sol qui represente Pangaeon (visible quand on recule la camera), bannières des 8 arenes

### 6.3 Interieurs — Regles de Tileset

**ATLAS :** Tous les interieurs partagent un systeme commun, modifie par le biome.

#### Elements communs a tous les interieurs
- Murs : 3 tiles de haut (sol, mur bas, mur haut). Couleur selon le biome
- Sol : Plancher bois / dalle pierre / carrelage selon le type de batiment
- Escalier : 2 tiles pour monter/descendre d'un etage
- Porte : 1 tile en bas du mur, transition vers l'exterieur
- Tapis : Decoratif, souvent devant les portes ou au centre
- Meuble : Tables (2x1), chaises (1x1), lits (1x2), etageres (1x2), posters (1x1 sur mur)

#### Types d'interieurs specifiques

| Interieur | Elements cles | Notes |
|-----------|--------------|-------|
| Maison PNJ | Lit, table, TV/radio, etagere, plante | Cozy, personnel |
| Centre Paleo (soin) | Comptoir, machine de soin, banquettes d'attente, PC Stockage | Rose-blanc, propre |
| Boutique | Comptoir, etageres avec items visibles, tapis d'entree | Eclairage chaud |
| Arene | Grande salle, terrain de combat au centre, gradins, statues | Unique par arene |
| Labo Paleo | Machines, ecrans, livres, fossiles en vitrine | Scientifique |
| Grotte | Pas de murs construits — roche naturelle, torches, stalactites | Sombre, torches |
| QG Escadron | Metal sombre, ecrans, carte de Pangaea, symbole meteore | Menaçant |

### 6.4 Eclairage et Atmosphere par Biome

**ATLAS :** L'eclairage est narratif. Chaque biome a une "heure du jour ideale" qui est sa lumiere par defaut.

| Biome | Heure ideale | Temperature couleur | Ambiance |
|-------|-------------|--------------------|---------|
| Bourg-Nid | 15h (apres-midi dore) | Chaude, doree | Securite, nostalgie |
| Ville-Fougere | 10h (matin brumeux) | Froide-verte, filtree | Mystere, decouverte |
| Port-Coquille | 14h (plein soleil) | Chaude, blanche | Aventure, liberte |
| Roche-Haute | 11h (lumiere crue) | Neutre, claire | Defi, grandeur |
| Volcanville | 19h (crepuscule) | Rouge-orange, dramatique | Danger, puissance |
| Cryo-Cite | 8h (aube froide) | Froide, bleutee | Isolation, beaute |
| Electropolis | 22h (nuit artificielle) | Neon, cyan-jaune | Modernite, energie |
| Marais-Noir | 5h (pre-aube) | Verte, brumeuse | Malaise, mystere |
| Ciel-Haut | 12h (zenith) | Blanche, eblouissante | Elevation, liberte |
| Paleo-Capital | 17h (golden hour) | Ambre, majestueuse | Accomplissement, heritage |

### 6.5 Effets Meteo — Specs

| Effet | Particules | Taille | Vitesse | Opacite | Biomes concernes |
|-------|-----------|--------|---------|---------|-----------------|
| Pluie | Lignes verticales diagonales | 1x4 px | 8 px/frame, angle 15 deg | 50% | Port-Coquille, Marais-Noir |
| Neige | Points blancs | 2x2 px | 1 px/frame, mouvement sinusoidal | 70% | Cryo-Cite, Roche-Haute (altitude) |
| Tempete de sable | Lignes horizontales | 2x1 px | 6 px/frame | 40% | Desert (route entre villes) |
| Cendres volcaniques | Points gris | 1x1 px | 0.5 px/frame, montee lente | 30% | Volcanville |
| Brouillard | Overlay en couches | Plein ecran | 0.2 px/frame horizontal | 20-35% | Marais-Noir |
| Eclairs | Flash plein ecran | Plein ecran | 1 frame flash, 3 frames fade | 80% flash | Electropolis (rare) |
| Spores | Points vert-jaune | 1x1 px | 0.3 px/frame, montee lente | 40% | Ville-Fougere |
| Petales | Pixels roses | 2x2 px | 1 px/frame, mouvement sinusoidal | 60% | Bourg-Nid (printemps) |

### 6.6 Tiles Animees — Specs Detaillees

| Tile | Frames | FPS | Description |
|------|--------|-----|-------------|
| Eau calme | 4 | 4 | Ondulation legere — les pixels de highlight se decalent de 1px par frame en vague horizontale |
| Eau de riviere | 4 | 6 | Plus rapide, decalage de 2px par frame, direction selon le courant |
| Lave | 4 | 3 | Bulles qui montent (1 pixel orange qui monte de 1px par frame), surface ondulante |
| Cascade | 4 | 8 | Lignes blanches verticales qui descendent rapidement |
| Herbes hautes | 2 | 2 | Balancement gauche-droite de 1px au sommet |
| Flamme (torche) | 3 | 6 | Le pixel superieur clignote jaune/orange/rouge |
| Fumerole | 3 | 3 | Pixels gris qui montent et s'estompent |
| Aurore boreale | 6 | 2 | Bandes de couleur qui ondulent lentement (vert -> cyan -> mauve) |
| Ecran/Neon | 2 | 4 | Clignotement subtil (luminosite +/-10%) |
| Cristal lumineux | 2 | 1 | Pulse lente de lumiere (reflet qui apparait/disparait) |

---

## 7. UI/UX Art Direction

### 7.1 Philosophie UI

**PIXEL :** L'UI de Jurassic Trainers suit la regle du fossile : **couche par couche, chaque information est revelee au bon moment**. Pas de surcharge. L'UI est thematiquement integree — les menus ressemblent a un carnet de paleontologue, pas a un menu informatique.

### 7.2 Style de Fenetre / Panneau

#### Cadre standard (Menu, Dialogue, Inventaire)

```
Exterieur a interieur :
1. Bordure externe : 1px, couleur #503820 (brun fonce)
2. Bordure interne : 1px, couleur #886830 (brun moyen)
3. Fond : fill, couleur #F0E8D0 (creme calcaire)
4. Padding interne : 2px avant le texte
```

**Coins :** Arrondis de 2px (les coins ont 1 pixel coupe en diagonale — effet GBA classique).

**Ombre portee :** 1px a droite et en bas, couleur `#50382080` (50% opacite). Donne un leger relief.

#### Cadre accent (Selection active, Info importante)

Meme structure mais la bordure interne est `#C89840` (ambre) au lieu de brun.

#### Cadre alerte (Danger, Avertissement)

Bordure interne `#D83028` (rouge), fond `#F0D8D0` (rose clair).

### 7.3 Menu Principal

**Layout (240 x 160 px) :**

```
+------------------------------------------+
|                                          |
|        JURASSIC TRAINERS                 |   Logo : 120x32 px, style pixel art
|        [logo fossile]                    |   Fond : scene de titre animee
|                                          |
|     > Nouvelle Partie                    |   Options de menu, centrees
|       Continuer                          |
|       Options                            |
|                                          |
|   v0.1         (C) Nova Forge Studio     |   Credits en bas
+------------------------------------------+
```

- **Fond** : Illustration pixel art panoramique de Pangaea avec les 3 starters en silhouette devant un coucher de soleil ambre. Legerement animee (nuages qui bougent, herbe qui ondule).
- **Logo** : "JURASSIC TRAINERS" en pixel font personnalisee. Les lettres ont une texture fossile/pierre. Le "J" et le "T" sont plus grands. Un petit crane de dino remplace le point du "i".
- **Curseur** : Fleche fossile `#E05028`, animee (pulse 2 frames)
- **Texte menu** : Blanc creme `#F0E8D0`, highlight quand selectionne : ambre `#C89840`

### 7.4 Menu en Jeu (Bouton Start)

**Layout :**

```
+-------------------+
| DINODEX           |
| EQUIPE            |
| SAC               |
| DRESSEUR          |
| SAUVEGARDE        |
| OPTIONS           |
| FERMER            |
+-------------------+
```

- Position : Droite de l'ecran, aligné a droite
- Largeur : 80 px
- Hauteur : Selon nombre d'options (variable, max ~110 px)
- Chaque option : 14px de haut (8px texte + 6px padding)
- Le reste de l'ecran est visible mais assombri (overlay `#00000040`)

### 7.5 Ecran de Combat — HUD Layout

**PIXEL :** Le combat est LE moment le plus critique pour l'UI. Chaque pixel compte.

```
+------------------------------------------+
|  [Nom Ennemi    Lv.XX]                   |   HUD ennemi : en haut a gauche
|  [====HP Bar========]                    |
|  [Type1][Type2]                          |
|                         +--------+       |
|                         | SPRITE |       |   Sprite ennemi : haut-droite
|                         | ENNEMI |       |
|                         +--------+       |
|                                          |
|    +--------+                            |   Sprite allie : bas-gauche
|    | SPRITE |                            |
|    | ALLIE  |                            |
|    +--------+                            |
|                   [Nom Allie     Lv.XX]  |   HUD allie : en bas a droite
|                   [====HP Bar========]   |
|                   [====EXP Bar=======]   |
+------------------------------------------+
|  [ATTAQUE]  [SAC]                        |   Menu d'action : bande du bas
|  [DINOS]    [FUITE]                      |   4 boutons, grille 2x2
+------------------------------------------+
```

#### Specs detaillees du HUD

| Element | Taille | Position | Notes |
|---------|--------|----------|-------|
| Boite info ennemi | 104 x 28 px | x:2, y:2 | Coin superieur gauche |
| Boite info allie | 112 x 34 px | x:126, y:96 | Inclut barre EXP en plus |
| Barre HP | 48 px de large | Dans la boite info | Largeur utile : 48px = 48 HP visibles |
| Barre EXP | 64 px de large | Sous la barre HP allie | Bleu `#3078B8` |
| Badge type | 8 x 8 px | A cote du nom | Couleur du type, icone dedans |
| Nom du dino | Max 10 caracteres | Dans la boite info | Font 8px |
| Niveau | "Lv.XX" | Apres le nom | Font 8px |
| Menu action | 240 x 32 px | Bas de l'ecran | Grille 2x2, 4 boutons |

#### Barre de PV — Etats visuels

| Etat | Seuil | Couleur | Comportement |
|------|-------|---------|-------------|
| Pleine sante | 100-51% | `#48B838` (vert) | Statique |
| Sante moyenne | 50-21% | `#E8C038` (jaune) | Statique |
| Sante critique | 20-1% | `#D83028` (rouge) | Pulse lent (1 Hz) |
| KO | 0% | Vide | Barre disparue |

La barre se vide de droite a gauche, de maniere fluide (pas instantanee — animation de ~0.5 seconde).

#### Menu d'attaque (apres selection "ATTAQUE")

```
+------------------------------------------+
| [Nom Attaque 1] PP XX/XX    [Type Badge] |   4 attaques, liste verticale
| [Nom Attaque 2] PP XX/XX    [Type Badge] |   Ou grille 2x2
| [Nom Attaque 3] PP XX/XX    [Type Badge] |
| [Nom Attaque 4] PP XX/XX    [Type Badge] |
+------------------------------------------+
```

- Chaque attaque montre : Nom (max 12 chars), PP restants/max, badge type
- L'attaque selectionnee est highlightee en ambre `#C89840`
- Les attaques sans PP restants sont grisees `#888078`

### 7.6 Dinodex — Interface

**PIXEL :** Le Dinodex est le carnet du paleontologue numerique. Son design evoque un journal de terrain.

#### Liste des dinos

```
+------------------------------------------+
| DINODEX                         XXX/150  |   Header
+------------------------------------------+
| #001 [icon] PYREX           [Feu]        |   Liste scrollable
| #002 [icon] ???             [???]        |   Dinos non vus = silhouette grise
| #003 [icon] AQUADON         [Eau]        |
| ...                                      |
+------------------------------------------+
```

- Icone : 16x16, mini-portrait du dino
- Dinos captures : Sprite couleur + nom
- Dinos vus mais pas captures : Silhouette noire + nom
- Dinos jamais vus : `#888078` "???" + silhouette grise

#### Fiche individuelle

```
+------------------------------------------+
|  #001 PYREX              [Feu]           |   Header
+------------------------------------------+
|  +--------+    Dino Etincelle            |   Portrait 64x64 + nom de categorie
|  |        |    Taille: 0.3m              |
|  | 64x64  |    Poids: 3.2kg             |
|  |portrait|    Zone: Bourg-Nid           |
|  |        |                              |
|  +--------+    Description du dino       |
|                sur 2-3 lignes max        |
|                                          |
|  Cri: [bouton play]                      |   Audio (si implemente)
+------------------------------------------+
```

### 7.7 Ecran d'Equipe

```
+------------------------------------------+
| EQUIPE                                   |
+------------------------------------------+
| [1] [sprite] PYREX  Lv.15  HP: 45/52    |   Slot 1 = premier, toujours visible
| [2] [sprite] ???    Lv.--  HP: --/--     |   Slots vides = cadre en pointilles
| [3] [sprite] ???    Lv.--  HP: --/--     |
| [4] [sprite] ???    Lv.--  HP: --/--     |
| [5] [sprite] ???    Lv.--  HP: --/--     |
| [6] [sprite] ???    Lv.--  HP: --/--     |
+------------------------------------------+
```

- Chaque slot : 230 x 20 px
- Sprite follower 16x16 du dino
- Nom, niveau, barre HP miniature
- Dino selectionne : fond `#C8984030` (ambre transparent)
- Slot vide : Cadre en pointilles `#88807860`

### 7.8 Boite de Dialogue

```
+------------------------------------------+
|                                          |
|  (scene visible au dessus)               |
|                                          |
+------------------------------------------+  y = 112
| [Portrait]  Texte du dialogue qui se     |  32px de haut
| [32x32  ]   revele caractere par car...▼ |  Portrait optionnel
+------------------------------------------+  y = 144
```

- Position : Bas de l'ecran, pleine largeur
- Hauteur : 32 px (2 lignes de texte max)
- Portrait : 32x32, optionnel, a gauche
- Zone texte : 198 x 24 px (sans portrait) ou 160 x 24 px (avec portrait)
- Indicateur "suite" : Petit triangle `#C89840` qui pulse en bas a droite quand le texte est complet

### 7.9 Font & Texte

**PIXEL :** On utilise une police pixel custom inspiree de Fire Red, avec quelques ajustements pour la lisibilite.

| Usage | Taille | Couleur | Notes |
|-------|--------|---------|-------|
| Texte de dialogue | 8px haut | `#282028` | Police proportionnelle, max ~28 chars/ligne |
| Noms de dinos | 8px haut | `#282028` | Meme police |
| Titres de menu | 8px haut | `#503820` | Meme police, couleur plus foncee |
| Nombres (HP, Niv) | 8px haut | `#282028` | Police monospace pour l'alignement |
| Texte petit (PP, description) | 8px haut | `#685848` | Couleur plus claire = secondaire |

- Vitesse de defilement du texte : 1 caractere toutes les 2 frames (30 chars/seconde a 60 FPS)
- Vitesse rapide (bouton maintenu) : 1 char par frame (60 chars/seconde)
- Vitesse instantanee : Bouton = tout le texte s'affiche immediatement

### 7.10 Transitions & Effets d'Ecran

#### Transition vers le combat

**PIXEL :** La transition doit etre excitante. On s'inspire de Fire Red mais avec notre identite.

1. **Flash blanc** : 2 frames, opacite 100% -> 0%
2. **Spirale fossile** : L'ecran se remplit en spirale depuis le centre avec des tiles de motif fossile (6 frames, ~0.3 secondes)
3. **Fondu au noir** : 3 frames
4. **Ecran de combat apparait** : Fondu depuis le noir, 4 frames. Les sprites glissent depuis les bords (ennemi depuis la droite, HUD depuis le haut/bas)

#### Autres transitions

| Transition | Effet | Duree |
|-----------|-------|-------|
| Entrer dans un batiment | Fondu au noir (4 frames) -> fondu depuis noir (4 frames) | 0.27 sec |
| Changer de zone | Fondu au noir -> chargement -> fondu depuis noir | 0.4 sec |
| Ouvrir un menu | Le menu glisse depuis la droite (4 frames) | 0.13 sec |
| Fermer un menu | Le menu glisse vers la droite (4 frames) | 0.13 sec |
| Evolution | Flash blanc pulse (accelere), 5 pulses, puis reveal | 3 sec |
| Capture reussie | Etoiles + flash + fanfare | 2 sec |
| Victoire combat | Fondu du sprite ennemi + texte victoire | 1.5 sec |

### 7.11 Indicateur d'Efficacite de Type

| Efficacite | Texte | Couleur du texte | Effet supplementaire |
|-----------|-------|-----------------|---------------------|
| Super efficace (x2) | "C'est super efficace !" | `#E05028` (rouge-orange) | Screen shake leger (1px, 2 frames) |
| Pas tres efficace (x0.5) | "Ce n'est pas tres efficace..." | `#685848` (gris) | Aucun |
| Aucun effet (x0) | "Ca n'affecte pas [nom]..." | `#484058` (sombre) | Aucun |
| Coup critique | "Coup critique !" | `#E8C020` (jaune) | Flash + shake |
| Neutre (x1) | (pas de message) | — | — |

---

## 8. Animation Guidelines

### 8.1 Overworld — Animations

#### Cycle de marche du joueur

Le cycle de marche est le mouvement le plus vu du jeu. Il doit etre fluide meme a 4 frames.

| Frame | Description | Pixel offset |
|-------|-------------|-------------|
| 0 (idle) | Position neutre, pieds joints | y: 0 |
| 1 (walk1) | Pied droit avance, corps penche legerement | y: -1 (monte) |
| 2 (walk2) | Mi-pas, les deux pieds au sol, corps au plus haut | y: -1 |
| 3 (walk3) | Pied gauche avance, symetrique de walk1 | y: -1 |

- Vitesse de deplacement : 1 tile (16px) en 16 frames = 1 px/frame
- Le cycle est : idle -> walk1 -> walk2 -> walk3 -> walk2 -> walk1 -> idle (6 transitions pour une boucle, mais seulement 4 sprites uniques)
- En courant (bottes de vitesse) : 1 tile en 8 frames = 2 px/frame, meme cycle mais accelere

#### Animations idle des PNJ

| Type PNJ | Animation | Frames | Notes |
|----------|-----------|--------|-------|
| PNJ statique | Regard gauche-droite | 2 (+ idle) | Tourne la tete toutes les 3-5 secondes |
| PNJ marcheur | Marche en boucle sur un chemin | 4 par direction | Suit un pattern predéfini |
| PNJ dormeur | Z qui flotte | 2 | "Z" pixel qui monte et disparait |
| PNJ energique | Sautillement | 2 | Monte de 1px et redescend, toutes les 2 secondes |
| Dresseur (pre-combat) | "!" au-dessus de la tete | 3 | Apparition du "!" + slide vers le joueur |

#### Animations environnementales

| Element | Animation | Frames | Notes |
|---------|-----------|--------|-------|
| Herbes hautes (entre par joueur) | Bruissement | 3 | Les pixels du haut de l'herbe bougent a gauche puis a droite |
| Porte qui s'ouvre | Ouverture | 3 | La porte disparait tile par tile |
| Objet au sol (ramasse) | Saut + disparition | 4 | L'item monte de 4px puis disparait |
| Panneau (lu) | "!" | 1 | Flash du panneau |

### 8.2 Combat — Animations d'Attaque par Type

**ZEPHYR :** Chaque type a un style d'animation d'attaque qui lui est propre. Ces descriptions s'appliquent aux effets visuels (VFX), pas au mouvement du dino lui-meme.

#### ROCHE
- **Physique** : Des blocs de pierre (4x4 px, brun `#A89070`) tombent du haut de l'ecran sur l'ennemi. 4-5 blocs, trajectoire en arc. Impact : ecran shake 1px.
- **Special** : Le sol se fissure sous l'ennemi (lignes brunes qui s'etendent depuis le centre), puis des pointes de roche jaillissent (3 frames montee).

#### EAU
- **Physique** : Un jet d'eau (ligne de pixels bleus `#3880C0` -> `#90C8E8`) part de l'allie vers l'ennemi. 4 frames, s'elargit en arrivant.
- **Special** : Une vague (arc de pixels bleus, 8px de haut) traverse l'ecran de gauche a droite. Ecume blanche au sommet `#E0F0F8`.

#### FEU
- **Physique** : Une boule de feu (4x4 px, centre jaune `#F8D038`, bord orange `#E87830`, contour rouge `#E05028`) part en arc vers l'ennemi. Trainee de 3 pixels en arriere.
- **Special** : Colonne de flammes montant du bas (pixels rouges/oranges/jaunes, 6 frames, monte puis s'estompe). L'ennemi est englouti.

#### PLANTE
- **Physique** : Des feuilles tranchantes (2x3 px, vertes `#48A030`) sont lancees en eventail (3-4 feuilles). Trajectoire courbe.
- **Special** : Des lianes (lignes vertes, 1px) jaillissent du bas de l'ecran et s'enroulent autour de l'ennemi (4 frames). Des fleurs `#E8A038` eclosent au contact.

#### GLACE
- **Physique** : Un rayon de glace (ligne de pixels cyan `#68C8E0` -> blanc `#E8F0F8`) part de l'allie. Au contact : cristaux de glace qui se forment sur l'ennemi (3 frames).
- **Special** : L'ecran entier se couvre de givre depuis les bords (8 frames). L'ennemi gele (overlay bleu pale `#C0E8F040`).

#### VOL
- **Physique** : Le dino monte hors ecran (3 frames) puis tombe sur l'ennemi (2 frames). Impact : des plumes blanches `#E8E8F0` volent.
- **Special** : Des lames d'air (arcs de pixels blanc/bleu pale, semi-transparents) traversent l'ecran en diagonale. 3 lames successives.

#### TERRE
- **Physique** : Le sol tremble (screen shake 2px horizontal, 4 frames). Des fissures brunes apparaissent sous l'ennemi.
- **Special** : Le dino frappe le sol (1 frame), onde sismique (ligne de poussiere brune qui s'etend du centre vers les bords, 4 frames).

#### FOUDRE
- **Physique** : Un eclair (ligne brisee jaune `#E8C020`, 2px d'epaisseur, avec halo blanc `#F8F8E0`) tombe du haut de l'ecran sur l'ennemi. Flash blanc 1 frame au contact.
- **Special** : Plusieurs eclairs plus petits (4-6) frappent en succession rapide (1 frame chaque). L'ennemi clignote jaune entre chaque.

#### POISON
- **Physique** : Un nuage toxique (amas de pixels violets `#9048A8` et verts `#78E050`, semi-transparents) se deplace vers l'ennemi. 4 frames, s'expanding.
- **Special** : Des bulles toxiques (`#78E050`, 3-4px de diametre) montent depuis le sol sous l'ennemi. L'ennemi prend une teinte violette temporaire (overlay `#9048A830`).

#### ACIER
- **Physique** : Des eclats metalliques (pixels gris `#6880A0` avec reflets blancs `#A8B8C8`) frappent l'ennemi. Effet de percussion metallique : flash blanc au contact + screen shake 1px.
- **Special** : Des lames d'acier (formes geometriques, aretes vives) traversent l'ecran horizontalement. Son metallique. Etincelles au contact.

#### OMBRE
- **Physique** : Une ombre noire (silhouette sombre, semi-transparente `#48405880`) glisse au sol jusqu'a l'ennemi, puis remonte pour le frapper par en dessous.
- **Special** : L'ecran s'assombrit (overlay `#10103060`), des yeux brillants `#D83028` apparaissent dans l'obscurite autour de l'ennemi, puis des griffes d'ombre frappent. L'ecran revient a la normale.

#### LUMIERE
- **Physique** : Un rayon de lumiere doree (ligne de pixels or `#E8D848` -> blanc `#F8F0D8`) frappe l'ennemi depuis un angle de 45 deg. Eclat radial au point d'impact.
- **Special** : Des prismes de lumiere (`#E8D848`, `#E05028`, `#3880C0`, `#48A030` — arc-en-ciel) convergent sur l'ennemi depuis les 4 coins de l'ecran. Flash blanc intense au centre.

#### SABLE
- **Physique** : Un tourbillon de sable (pixels jaune-brun `#C8A850`, en spirale) englobe l'ennemi. 5 frames, le tourbillon accelere.
- **Special** : Une tempete de sable couvre tout l'ecran (overlay `#C8A85050`, particules rapides). L'ennemi est a peine visible. 6 frames puis dissipation.

#### FOSSILE
- **Physique** : Un os geant (8x3 px, beige `#E8D8C0`) est lance en rotation vers l'ennemi. L'os tourne (4 frames de rotation).
- **Special** : Le sol sous l'ennemi se transforme en terrain fossile (pixels beige/ambre), des os anciens jaillissent du sol comme des stalagmites (4 frames montée, 2 frames statique, 3 frames disparition).

### 8.3 Animations de Combat — Autres

#### Sequence de Capture

| Phase | Frames | Description |
|-------|--------|-------------|
| 1. Lancer | 6 | La Jurassic Ball (8x8 px) part en arc parabolique de la position du joueur vers l'ennemi |
| 2. Contact | 2 | Flash blanc au contact, le dino se transforme en energie rouge |
| 3. Absorption | 4 | L'energie rouge est aspiree dans la Ball (lignes convergentes) |
| 4. Chute | 3 | La Ball tombe au sol avec un petit rebond |
| 5. Shake 1 | 3 | La Ball penche a gauche (2px), revient au centre |
| 6. Shake 2 | 3 | La Ball penche a droite (2px), revient au centre |
| 7. Shake 3 | 3 | La Ball penche a gauche (2px), revient au centre |
| 8a. Capture | 3 | Etoiles (4 etoiles 3x3 px, jaunes) + "Click" sonore. La Ball s'immobilise |
| 8b. Echec | 4 | La Ball s'ouvre, flash blanc, le dino se rematerialise, la Ball disparait |

- Les shakes 1-3 ont chacun une chance d'echec. Si echec, on passe directement a 8b.
- Le nombre de shakes avant capture/echec depend de la mecanique de jeu (pas de la direction artistique).

#### Sequence d'Evolution

| Phase | Frames | Duree | Description |
|-------|--------|-------|-------------|
| 1. Debut | 1 | 0.5s | Le dino commence a briller (overlay blanc `#F8F0D8`, 20%) |
| 2. Pulse 1 | 4 | 0.5s | Flash blanc 50% -> retour. Le sprite est a moitie visible |
| 3. Pulse 2 | 4 | 0.4s | Flash blanc 70% -> retour. Plus rapide |
| 4. Pulse 3 | 4 | 0.3s | Flash blanc 90% -> retour. Encore plus rapide |
| 5. Transformation | 2 | 0.3s | Blanc 100% pendant 0.3s. Le sprite change |
| 6. Reveal | 6 | 1.0s | Le blanc s'estompe pour reveler la nouvelle forme |
| 7. Cri | — | 0.5s | Le nouveau dino fait son animation idle + cri |

- Pendant les phases 2-4, le joueur peut annuler l'evolution en maintenant B (si implemente)
- Les particules de lumiere (pixels blancs/ambre) flottent vers le haut pendant toute la sequence

#### Level Up

- Texte "[Nom] monte au niveau [X] !" dans la boite de dialogue
- Barre d'EXP se remplit de maniere fluide
- Si le niveau est atteint : petit jingle + les stats qui augmentent s'affichent un par un
- Si le dino apprend une attaque : ecran special avec le nom de l'attaque + type

---

## 9. Visual Effects

### 9.1 Effets de Particules par Type (Ambient)

**ZEPHYR :** En combat, chaque dino emet de subtils effets de particules en idle. C'est ce qui rend le combat vivant.

| Type | Particule | Taille | Comportement | Frequence |
|------|-----------|--------|-------------|-----------|
| Roche | Grains de poussiere | 1x1 px `#A89070` | Tombent lentement autour des pieds | 1 toutes les 30 frames |
| Eau | Gouttelettes | 1x1 px `#90C8E8` | Tombent du corps, rebondissent | 1 toutes les 20 frames |
| Feu | Braises | 1x1 px `#F0A030` | Montent depuis le corps, s'estompent | 1 toutes les 10 frames |
| Plante | Spores / Petales | 1x2 px `#90D068` | Flottent en montant, mouvement sinusoidal | 1 toutes les 40 frames |
| Glace | Flocons | 1x1 px `#E8F0F8` | Tombent lentement en zigzag | 1 toutes les 25 frames |
| Vol | Plumes | 2x2 px `#E8E8F0` | Tombent en tournoyant | 1 toutes les 60 frames |
| Terre | Poussiere | 1x1 px `#D0B070` | S'elevent depuis les pieds | 1 toutes les 35 frames |
| Foudre | Etincelles | 1x1 px `#F8E038` | Apparaissent brievement (flash) sur le corps | 1 toutes les 15 frames |
| Poison | Bulles | 2x2 px `#C888D8` | Montent depuis le corps | 1 toutes les 20 frames |
| Acier | Eclats metalliques | 1x1 px `#A8B8C8` | Flash bref, reflet | 1 toutes les 45 frames |
| Ombre | Fumee sombre | 2x2 px `#48405840` | S'eleve, semi-transparente | 1 toutes les 15 frames |
| Lumiere | Etincelles dorees | 1x1 px `#E8D848` | Montent en spirale | 1 toutes les 12 frames |
| Sable | Grains de sable | 1x1 px `#C8A850` | Tourbillonnent autour des pieds | 1 toutes les 18 frames |
| Fossile | Poussiere d'ambre | 1x1 px `#C89840` | S'elevent lentement | 1 toutes les 30 frames |

**Regle :** Maximum 4 particules visibles simultanement par dino. Les particules vivent 30-60 frames puis disparaissent (fondu).

### 9.2 Effets d'Ecran (Screen Effects)

| Effet | Utilisation | Implementation |
|-------|-------------|---------------|
| Screen shake horizontal | Attaque Terre, coup critique | Decalage de l'ecran de 1-2px gauche/droite, 3-4 oscillations, 6-8 frames total |
| Screen shake vertical | Attaque Roche (chute de pierres) | Decalage de 1-2px haut/bas, 2-3 oscillations |
| Flash blanc | Touche, evolution, eclair | Overlay blanc `#F8F0D8` a 80-100% opacite, 1-2 frames |
| Flash couleur | Touche super efficace | Overlay couleur du type a 40%, 2 frames |
| Fondu au noir | Transitions | Overlay noir `#181018`, opacite 0->100% en 4-6 frames |
| Fondu depuis noir | Transitions | Inverse du fondu au noir |
| Assombrissement | Menu overlay | Overlay `#00000040` permanent tant que le menu est ouvert |
| Teinte rouge | HP bas (hors combat) | Bords de l'ecran en overlay rouge `#D8302820`, pulse lent |
| Vignette | Grottes, zones sombres | Assombrissement circulaire des bords de l'ecran |

### 9.3 Effets Meteo en Combat

Les combats en exterieur heritent de la meteo de la zone :

| Meteo | Effet en combat | Impact visuel |
|-------|----------------|---------------|
| Pluie | Lignes de pluie en arriere-plan (derriere les dinos) | Fond assombri de 10%, reflets sur le sol |
| Neige | Flocons en premier plan et arriere-plan | Fond eclairci de 5%, givre sur les bords du HUD |
| Tempete sable | Particules de sable devant tout | Visibility reduite (dinos legerement estompes), teinte jaune |
| Cendres | Particules grises lentes | Ciel assombri, teinte orange |
| Soleil intense | Pas de particules | Fond eclairci de 10%, ombres plus marquees |

### 9.4 Animation de la Jurassic Ball

**NOVA :** Les Jurassic Balls ont un design unique — elles ressemblent a des oeufs fossiles plutot qu'a des spheres lisses.

#### Design de la Ball standard
- **Forme** : Ovale vertical (pas parfaitement rond — forme d'oeuf), 8x8 px (en vol) et 16x16 px (au sol)
- **Moitie superieure** : Brun rouille `#A06030` avec texture de coquille craquelée
- **Moitie inferieure** : Beige fossile `#E8D8C0`
- **Ligne de separation** : Bande noire `#282028` de 1px avec un cercle central (le mecanisme d'ouverture, 2x2 px ambre `#C89840`)
- **Highlight** : 1 pixel blanc `#F8F0E8` en haut a gauche (reflet)

#### Variantes de Balls (palette uniquement — meme forme)
| Ball | Moitie haute | Moitie basse | Accent |
|------|-------------|-------------|--------|
| Jurassic Ball (standard) | `#A06030` | `#E8D8C0` | `#C89840` |
| Super Ball | `#3078B8` | `#E8D8C0` | `#E8C020` |
| Hyper Ball | `#282028` | `#E8D8C0` | `#E05028` |
| Master Ball | `#9048A8` | `#E8D8C0` | `#E8D848` |
| Fossil Ball (legendaires) | `#C89840` | `#F0E8D0` | `#F8F0D8` (lumineux) |

---

## 10. Reference Board

### 10.1 References Visuelles par Categorie

#### Style Pixel Art General
- **Pokemon Fire Red / Leaf Green (GBA)** : Notre reference technique principale. La resolution, les proportions de sprite, les tailles de tiles, les transitions de combat — tout est calque sur ce modele. A etudier : comment ils font tenir tant d'information dans 240x160.
- **Pokemon Emerald (GBA)** : Pour les animations de combat en particulier. Chaque dino a un idle anime — on reprend ce principe.
- **Golden Sun (GBA)** : Pour les effets visuels de combat. Golden Sun a les plus beaux VFX de la GBA — les attaques elementaires sont spectaculaires meme en pixel art. A etudier : leurs particules, leurs eclairages dynamiques, leurs screen effects.
- **Mother 3 (GBA)** : Pour l'emotion et l'atmosphere. Les palettes de couleurs racontent les emotions. A etudier : comment la couleur change entre les moments joyeux et tristes.

#### Design de Creatures
- **Pokemon (toutes generations)** : La reference absolue pour le design de creatures collectionnables. A etudier : la lisibilite des silhouettes, la communication visuelle des types, la progression d'evolution.
- **Monster Hunter** : Pour le realisme anatomique. Nos dinos adultes doivent avoir la credibilite des monstres de MH — on sent le poids, la masse, la dangerosité. A ne PAS copier : la complexite excessive des textures (on est en pixel art).
- **Fossil Fighters (DS)** : Jeu de dinos avec un style similaire a notre cible. A etudier : comment ils stylisent les dinos pour le gameplay. A ne PAS copier : le style trop cartoon.
- **References paleontologiques reelles** : National Geographic, paleoart de Julius Csotonyi, Mark Witton. Pour l'anatomie de base avant stylisation.

#### Environnements
- **Pokemon Fire Red** : Tilesets, structure des villes, interieurs. A reproduire fidelement en termes de structure.
- **Final Fantasy VI (SNES/GBA)** : Pour les ambiances. Les villes de FFVI ont chacune une personnalite forte malgré les contraintes techniques. A etudier : Narshe (ville de glace), Zozo (ville sombre), Figaro (desert).
- **Chrono Trigger (SNES)** : Pour les variations temporelles. Les changements d'epoque montrent comment un meme lieu peut avoir des atmospheres radicalement differentes. A etudier : les transitions entre epoques prehistorique et futuriste.

#### UI/UX
- **Pokemon Fire Red** : La base. Menus, dialogue, combat HUD — on part de la et on ameliore.
- **Persona 5** : Pour l'audace. Meme si le style est tres different, P5 prouve qu'un UI peut avoir du CHARACTER. Notre "character" c'est le theme fossile/paleontologue.
- **Undertale** : Pour la simplicite efficace. Pas un pixel de gaspille dans l'UI d'Undertale. Tout est lisible instantanement.

### 10.2 Ce qu'on Prend de Pokemon Fire Red Specifiquement

| Element | Ce qu'on reprend | Ce qu'on change |
|---------|-----------------|-----------------|
| Resolution | 240x160 exactement | Rien — identique |
| Taille de tile | 16x16 | Rien — identique |
| Taille sprite overworld | 16x24 (joueur) | Rien — identique |
| Taille sprite combat | 64x64 max | On garde mais on utilise des tailles variables par stade |
| Structure de la map | Grille de tiles, routes entre villes | On ajoute plus de verticalite et de secrets |
| Combat HUD | Layout general (dino haut-droite, bas-gauche) | On ajoute les badges de type, on modernise le style |
| Menus | Structure hierarchique simple | On ajoute le theme "carnet de paleontologue" |
| Transitions | Effet spiral/slide vers le combat | On change pour une spirale fossile |
| Cycle jour/nuit | Absent dans Fire Red | ON AJOUTE — c'est un plus majeur |

### 10.3 Ce qu'on Prend d'Autres Jeux GBA

| Jeu | Element emprunte |
|-----|-----------------|
| Golden Sun | Effets de particules avances, eclairage dynamique, Mode 7-like pour certains effets |
| Metroid Fusion | Ambiance oppressante pour les zones sombres (Marais-Noir, grottes) |
| Advance Wars | Lisibilite des sprites petits — chaque unite est identifiable |
| Fire Emblem | Portraits expressifs dans les dialogues |
| Castlevania: Aria of Sorrow | Palette sombre mais lisible, ennemis avec presence |

### 10.4 Ce qu'on EVITE — Red Flags Visuels

**ZEPHYR :** Liste non-negociable de ce qui rendrait notre jeu generique ou cheap.

| A EVITER | Pourquoi | Que faire a la place |
|----------|---------|---------------------|
| Palettes neon saturees | Ca crie "mobile game gratuit" | Palette terreuse, ambre, naturelle |
| Outlines trop epaisses (2px+) | Ca ecrase les details en petite resolution | Outlines de 1px, parfois 0 (selectively lineless) |
| Sprites generiques recolores | Ca hurle "manque de budget" | Chaque dino a une silhouette unique |
| Fonds de combat plats et vides | Ca fait "prototype" | Au moins 3 couches : sol + element + ciel |
| UI avec des coins 90 deg parfaits | Ca fait "placeholder" | Coins arrondis 2px, style GBA authentique |
| Texte en police systeme | Ca casse l'immersion | Police pixel custom integree |
| Animations manquantes (teleportation) | Ca fait amateur | Meme 2 frames valent mieux que 0 |
| Echelle inconsistante | Ca brise la credibilite | Tous les sprites respectent les specs de ce document |
| Couleurs de type qui ne matchent pas les dinos | Ca confond le joueur | Chaque dino a au moins 30% de sa palette en couleur de type |
| Plagiat direct de Pokemon | Ca fait "ripoff" | S'inspirer de la structure, pas du style visuel |

---

## Annexe A : Checklist de Validation d'Asset

**ZEPHYR :** Avant qu'un asset soit considere "fini", il doit passer cette checklist.

### Sprite de Dino
- [ ] Taille correcte selon le stade (40/48/56/64 px)
- [ ] Palette de 15 couleurs max + 1 transparent
- [ ] Silhouette unique et reconnaissable
- [ ] Type lisible en 3 secondes (couleur + morphologie)
- [ ] Idle animation (3 frames) implementee
- [ ] Vue face ET vue dos
- [ ] Version overworld follower 16x16
- [ ] Version Dinodex 32x32
- [ ] Coherence avec les autres stades de la meme lignee

### Tile de Biome
- [ ] Taille 16x16 px
- [ ] S'assemble correctement avec les tiles adjacentes (pas de coutures visibles)
- [ ] Palette du biome respectee
- [ ] Variantes anti-repetition (au moins 2 pour les sols)
- [ ] Tiles animees en boucle fluide

### Element UI
- [ ] Lisible a la resolution native 240x160
- [ ] Contraste suffisant (ratio 4.5:1 minimum texte/fond)
- [ ] Respecte le style de cadre defini (bordure double, coins arrondis)
- [ ] Fonctionne avec le overlay jour/nuit (si visible en overworld)
- [ ] Taille de texte minimum 8px

### Animation
- [ ] Nombre de frames conforme a ce document
- [ ] Boucle fluide (pas de saut entre la derniere et la premiere frame)
- [ ] Timing correct (FPS specifie)
- [ ] Pas de pixel "mort" qui clignote involontairement

---

## Annexe B : Glossaire Technique

| Terme | Definition |
|-------|-----------|
| Meta-tile | Bloc de 2x2 tiles (32x32 px) pour les structures |
| Palette swap | Recoloration d'un sprite en changeant uniquement sa palette — INTERDIT pour les dinos (chacun est unique) |
| Outline | Contour de 1px autour des sprites, generalement en noir chaud `#282028` |
| Idle animation | Animation en boucle quand le personnage/dino ne fait rien |
| Hit flash | Le sprite devient entierement blanc pendant 1 frame quand il est touche |
| Screen shake | Deplacement rapide de tout l'ecran de 1-2px pour simuler un impact |
| Overlay | Couche semi-transparente appliquee par-dessus l'ecran (pour la nuit, les menus, etc.) |
| Parallaxe | Plusieurs couches d'arriere-plan qui se deplacent a des vitesses differentes pour creer de la profondeur |
| Super-deforme (SD) | Style de sprite ou la tete est disproportionnellement grande (utilise pour les followers overworld) |
| RGB555 | Format de couleur GBA : 5 bits par canal (32 niveaux par canal, 32768 couleurs totales) |

---

*Document redige par l'equipe artistique de Nova Forge Studio.*
*ZEPHYR (direction), NOVA (creatures & personnages), ATLAS (environnements), PIXEL (UI/UX).*
*Version 1.0 — Phase de Pre-Production.*
