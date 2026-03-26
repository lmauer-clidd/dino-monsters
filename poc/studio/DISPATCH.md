# Nova Forge Studio — Methode de Dispatch Parallele

## Principe
CIPHER (CTO) et les leads d'equipe (FLICKER, FOSSIL, SCALE) dispatchen le travail en **lanes paralleles** isolees. Chaque lane a un owner, des fichiers cibles exclusifs, et un contrat d'interface. Aucun agent ne touche aux fichiers d'un autre lane sans coordination explicite.

---

## 1. LANES DE TRAVAIL

Chaque lane est un flux de travail isole. Les agents d'un lane possedent des fichiers specifiques et ne touchent a rien d'autre.

### LANE A — Dino Data (Owner: FOSSIL)
| Agent | Fichiers EXCLUSIFS | Output |
|-------|-------------------|--------|
| FOSSIL | `dinos.json` (champs: id, name, types, baseStats, evolution, description, habitat) | Fiches especes |
| PRISM | `type_chart.json` | Table de types |
| CLAW | `moves.json` | Catalogue d'attaques |
| STRIPE | `dinos.json` (champ: learnset UNIQUEMENT) | Learnsets |

**Regle** : STRIPE ne modifie QUE le champ `learnset` de chaque dino. Il ne touche pas aux stats, types, ou descriptions (c'est FOSSIL).

### LANE B — Balance & Trainers (Owner: SCALE)
| Agent | Fichiers EXCLUSIFS | Output |
|-------|-------------------|--------|
| COMPASS | `maps.ts` (encounters, niveaux sauvages) | Courbe de progression |
| ARENA | `trainers.ts` (equipes, niveaux, rewards) | Equipes adverses |
| SCALE | `constants.ts` (multiplicateurs combat) | Parametres d'equilibre |

**Regle** : ARENA ne modifie QUE les equipes et dialogues des trainers. Il ne change pas la structure des maps (c'est COMPASS/SANA).

### LANE C — Animation & VFX (Owner: FLICKER)
| Agent | Fichiers EXCLUSIFS | Output |
|-------|-------------------|--------|
| FLICKER | `AnimationSystem.ts` (nouveau), sequences de combat | Choreographie |
| EMBER | `ParticleEngine.ts` (nouveau), effets par type | VFX |
| DRIFT | `TransitionFX.ts`, animations overworld/UI | Transitions |
| PULSE | `ParticleEngine.ts` (infra), `CameraSystem.ts` (nouveau) | Moteur |

**Regle** : FLICKER definit les specs, PULSE code l'infra, EMBER utilise l'infra pour les effets. Pas d'overlap.

### LANE D — Integration & QA (Owner: CIPHER)
| Agent | Fichiers EXCLUSIFS | Output |
|-------|-------------------|--------|
| GRID | `BootScene.ts` (chargement data), scripts de validation | Data integrity |
| CIPHER | Architecture globale, revue cross-lane | Validation |

---

## 2. CONTRATS D'INTERFACE

Les lanes communiquent par des **contrats** — des specs figees que les deux cotes respectent.

### Contrat A→B : "Dino Data → Balance"
```
FOSSIL fournit a SCALE :
- Liste des speciesId avec types et baseStats
- SCALE ne modifie PAS dinos.json, il ajuste constants.ts

CLAW fournit a ARENA :
- Liste des moveId avec type/power/pp/effect
- ARENA utilise ces moveId dans trainers.ts
```

### Contrat A→C : "Dino Data → Animation"
```
FOSSIL fournit a FLICKER :
- Liste des dinos avec types (pour couleurs d'animation)
- CLAW fournit les noms de moves (pour labels d'animation)
- FLICKER n'a pas besoin des stats
```

### Contrat B→C : "Balance → Animation"
```
SCALE fournit a FLICKER :
- Multiplicateurs (super-efficace, critique) pour intensite des effets
- EMBER adapte les VFX en fonction (super-efficace = effet plus gros)
```

### Contrat C→D : "Animation → Integration"
```
PULSE fournit a CIPHER :
- Nouveaux fichiers systemes (AnimationSystem, ParticleEngine, CameraSystem)
- CIPHER valide l'architecture et la performance avant merge
```

### Contrat A→D : "Data → Integration"
```
Tout changement dans dinos.json, moves.json, type_chart.json
  → GRID valide les schemas
  → GRID verifie les references croisees
  → GRID met a jour BootScene.ts si necessaire
```

---

## 3. REGLES ANTI-INTERFERENCE

### Regle du fichier unique
> Un fichier ne peut etre modifie que par UN agent a la fois. Si deux agents ont besoin du meme fichier, l'un attend que l'autre ait fini.

### Regle du champ exclusif
> Dans `dinos.json`, FOSSIL possede tout SAUF `learnset` (STRIPE) et `xpYield` (COMPASS). Ils peuvent travailler en parallele sur le meme fichier car ils ne touchent pas aux memes champs.

### Regle du merge sequentiel
> Quand un lane termine, ses changements sont valides par GRID (data) ou CIPHER (code) AVANT d'etre merges dans le projet. Pas de merge direct.

### Regle de la spec figee
> Un contrat d'interface ne change pas en cours de sprint. Si CLAW ajoute un nouveau champ a moves.json, il le communique a GRID qui propage le changement APRES que le sprint en cours est fini.

---

## 4. ORDRE D'EXECUTION

### Phase 1 — Fondations (parallelisable)
```
LANE A: PRISM (type_chart) + FOSSIL (dinos stats/types) + CLAW (moves)
LANE C: PULSE (AnimationSystem + ParticleEngine infra)
LANE D: GRID (scripts de validation)
```
→ Ces 3 lanes n'ont AUCUNE dependance entre eux.

### Phase 2 — Assemblage (parallelisable apres Phase 1)
```
LANE A: STRIPE (learnsets) — depend de FOSSIL + CLAW
LANE B: COMPASS (progression) + ARENA (trainers) — depend de FOSSIL + CLAW
LANE C: FLICKER (choreographie) + EMBER (VFX) + DRIFT (transitions) — depend de PULSE
```
→ Ces 3 lanes dependent de Phase 1 mais sont paralleles entre eux.

### Phase 3 — Integration (sequentiel)
```
LANE D: GRID valide toute la data
LANE D: CIPHER valide tout le code
LANE D: Test complet end-to-end
```

---

## 5. COMMUNICATION ENTRE LANES

### Channels
- **#data-changes** : FOSSIL/CLAW/PRISM annoncent tout changement de schema
- **#balance-numbers** : SCALE/COMPASS/ARENA partagent les chiffres d'equilibre
- **#animation-specs** : FLICKER/EMBER/DRIFT partagent les specs d'animation
- **#integration** : GRID/CIPHER annoncent les validations et les problemes

### Checkpoints
Apres chaque phase, un **checkpoint** reunit les owners de lane :
1. FOSSIL confirme "data dinos prete"
2. SCALE confirme "balance calibree"
3. FLICKER confirme "animations specifiees"
4. CIPHER confirme "integration valide"
5. → Phase suivante autorisee

---

## 6. RESOLUTION DE CONFLITS

| Situation | Resolution |
|-----------|-----------|
| Deux agents veulent modifier le meme fichier | L'owner du lane arbitre l'ordre |
| Un contrat d'interface doit changer | CIPHER approuve, changement applique au prochain sprint |
| Un lane est bloque en attendant un autre | L'agent bloque travaille sur des taches internes (docs, specs, tests) |
| Desaccord entre deux owners de lane | KAELEN arbitre |
