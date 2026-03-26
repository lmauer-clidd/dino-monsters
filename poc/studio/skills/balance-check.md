---
name: balance-check
description: Methodologie pour verifier l'equilibre du jeu apres un changement (simulation -> metriques -> ajustement)
owner: SCALE
domain: balance
triggers: Quand un changement affecte les stats, moves, type chart, ou niveaux des trainers
---

# Balance Check

## Objectif
Garantir que chaque changement numerique preserve l'equilibre du jeu grace a des donnees, pas de l'intuition.

## Quand l'utiliser
- Modification de la puissance d'un move
- Changement de stats d'un dino
- Mise a jour de la type chart
- Modification des niveaux/equipes des trainers

## Methodologie

### Etape 1 : Analyse
- Identifier precisement ce qui a change (avant/apres)
- Lister les mecaniques impactees en cascade (un move buff impacte tous les dinos qui l'apprennent)
- Definir les scenarios de test pertinents (quels matchups verifier)

### Etape 2 : POC Simulation
- Lancer balanceSim.ts avec les parametres concernes
- Construire des dinos aux niveaux pertinents (early, mid, late game)
- Simuler les combats critiques : starters entre eux, boss fights, matchups extremes
- Collecter les metriques brutes

### Etape 3 : Validation Metriques
- **Duree moyenne de combat** : cible 3-6 tours (trop court = one-shot, trop long = ennuyeux)
- **Taux de one-shot** : cible 0% (aucun dino ne doit etre elimine en un coup a niveau egal)
- **Triangle des starters** : win rates equilibres (proche de 50/50 avec avantage type)
- **Formules** : Physical = Atk/Def, Special = SpAtk/SpDef, STAB = 1.5x, SE = 2.0x max

### Etape 4 : Deploiement
- Executer `npx vitest run balance` — les 60 tests d'equilibre doivent passer
- Si un test echoue, ajuster les valeurs et re-simuler (boucle etape 2-3)
- Documenter les changements et le raisonnement dans la memoire

## Contraintes
- **Tech** : Physical moves utilisent Atk/Def, special moves utilisent SpAtk/SpDef
- **Business** : Le joueur ne doit jamais se sentir impuissant (pas de one-shot)
- **Produit** : STAB 1.5x, Super Effective 2.0x maximum
- **Design** : Combats de 3-6 tours, chaque tour doit avoir un choix significatif

## Anti-patterns
- Changer un nombre sans simuler l'impact en cascade
- Se fier a l'intuition ("ca a l'air bien") sans donnees
- Ajuster un seul dino sans verifier les matchups impactes
- Ignorer les edge cases (double resistance, double faiblesse)

## Lecons apprises
- Les puissances de moves ont du etre reduites de 30% et les HP augmentes de 40% pour atteindre la cible de 3-6 tours. Toujours faire un POC avant de valider des chiffres.
- Un changement mineur (ex: +5 puissance sur un move) peut avoir un impact majeur sur plusieurs matchups. Simuler systematiquement.
