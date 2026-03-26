---
name: implement-feature
description: Pipeline complet pour implementer une feature de jeu (analyse -> code -> test -> review -> deploy)
owner: CIPHER
domain: tech
triggers: Quand une nouvelle feature doit etre codee ou une feature existante etendue
---

# Implement Feature

## Objectif
Garantir que chaque feature est implementee proprement, testee, reviewee et deployee sans regression.

## Quand l'utiliser
- Nouvelle mecanique de jeu a coder
- Extension d'une feature existante
- Refactoring significatif d'un module

## Methodologie

### Etape 1 : Analyse
- Lire le code existant lie a la feature (grep les fichiers impactes)
- Consulter la checklist OAK pour la mecanique concernee
- Identifier TOUS les fichiers impactes (modele, logique, rendu, tests, data)
- Verifier si une feature similaire existe deja — ne pas reinventer la roue
- Lister les dependances : quels modules appellent / sont appeles par le code impacte

### Etape 2 : Production
- Coder en suivant les patterns existants du projet (conventions de nommage, structure)
- Separer la logique pure du rendering — aucune dependance Phaser dans la logique metier
- Utiliser des constantes (pas de valeurs hardcodees)
- Ajouter des fallbacks et du error handling sur chaque chemin critique
- Documenter les fonctions publiques avec JSDoc

### Etape 3 : Validation
- INVOKE SHERLOCK : generer les tests (unit + integration) pour la feature
- Minimum 3 tests : happy path, edge case, error case
- TOUS les tests doivent passer avant de continuer
- Verifier visuellement dans le preview si la feature a un rendu

### Etape 4 : Review
- CIPHER review l'architecture (separation des concerns, pas d'imports circulaires)
- Expert du domaine review la logique metier :
  - OAK pour les mecaniques de jeu
  - SCALE pour l'equilibre
  - FOSSIL pour les donnees dinos
- Corriger les retours avant deploiement

### Etape 5 : Deploiement
- GRID valide l'integrite des donnees JSON
- Compilation TypeScript sans erreur
- Tous les 134+ tests passent (npx vitest run)
- Verifier les performances (pas de regression fps)

## Contraintes
- **Tech** : 60fps target, pas d'imports circulaires, logique pure separee du rendu
- **Business** : Feature doit etre fun et respecter le joueur
- **Produit** : Coherence avec la vision Pokemon-like
- **Design** : Accessibilite, feedback visuel clair pour le joueur

## Anti-patterns
- Valeurs hardcodees au lieu de constantes
- Fallbacks manquants sur les cas d'erreur
- Pas de error handling (crash silencieux)
- Creer une feature from scratch quand une similaire existe deja
- Logique metier couplee au framework de rendu (Phaser)

## Lecons apprises
- Toujours verifier si une feature similaire existe avant d'en creer une nouvelle
- Les imports circulaires sont le signe d'un probleme d'architecture — refactorer avant d'ajouter
