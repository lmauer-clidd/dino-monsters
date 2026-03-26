---
name: conventions
description: Conventions decouvertes au fil du developpement
type: project
---

# Conventions du Projet

## Data

### Types JSON vs Code
- Les fichiers JSON (dinos.json, moves.json, type_chart.json) utilisent leur PROPRE numerotation : 0=Fossil, 1=Water, 2=Fire, 3=Flora...
- Le code TypeScript utilise l'enum DinoType : 0=Normal, 1=Fire, 2=Water, 3=Earth...
- La conversion se fait dans BootScene.ts via mapType()
- **Ne JAMAIS utiliser les numeros JSON dans le code TypeScript**
- Documentation complete : `game/src/data/TYPE_MAPPING.md`

### Ordre de placement des tiles
- Les chemins (Path) PEUVENT ecraser des tiles importantes (Door, Sign)
- **TOUJOURS** appeler restoreDoors() apres avoir trace les chemins
- Le pattern est automatise : une boucle dans maps.ts appelle restoreDoors() sur ALL_MAPS au chargement

### Learnsets
- TOUS les dinos commencent par moveId 121 (Charge) + 122 (Grondement) a level 1
- Pas de move type avant level 7
- Au moins 4 moves par learnset
- Les moves sont ordonnes par level croissant

## Combat

### Physical / Special Split
- Les moves physiques utilisent attacker.attack vs defender.defense
- Les moves speciaux utilisent attacker.spAttack vs defender.spDefense
- La category du move ('physical' | 'special' | 'status') determine quel stat pair utiliser

### Stats des ennemis
- Ne JAMAIS utiliser des formules de fallback differentes pour le joueur et l'ennemi
- Utiliser buildTrainerDino() avec les vrais baseStats de l'espece
- Les dinos sauvages doivent etre crees via Dino.createWild() (pas de stats hardcodees)

### Equilibre
- Combats cibles : 3-6 tours de duree moyenne
- 0 one-shot au meme niveau (sauf combo SE + STAB + Crit extreme)
- Triangle starters equilibre a tous les niveaux
- Move power tiers : 20-35 (early) / 45-60 (mid) / 65-80 (late) / 85-100 (endgame)

## Events

### Flags et combats
- scene.start() DETRUIT la scene overworld → les actions post-combat ne s'executent jamais
- Le pre-scan dans EventSystem execute les setFlag et giveItem AVANT de lancer le combat
- Les dresseurs sont marques "vaincus" meme en cas de defaite du joueur (pour eviter les boucles)

## Architecture

### Scenes
- Chaque scene interieure recoit { returnMapId, returnX, returnY } pour revenir a l'overworld
- Toujours passer hasStarter: true dans les donnees de retour vers OverworldScene
- Transitions avec TransitionFX.fadeOut (300ms) pour entrer, fadeIn (400ms) pour sortir

### Tests
- Tous les tests dans game/src/__tests__/
- Pas de dependance Phaser dans les tests (pure Node.js)
- Logique pure extraite dans game/src/utils/ (damageCalc.ts, balanceSim.ts)
- 134 tests minimum, < 500ms execution
- Pas de merge sans tests
