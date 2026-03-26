---
name: debug-and-fix
description: Methodologie pour diagnostiquer et corriger un bug (reproduire -> isoler -> root cause -> fix -> test regression)
owner: CIPHER
domain: tech
triggers: Quand un bug est signale ou detecte dans le jeu
---

# Debug and Fix

## Objectif
Corriger les bugs en traitant la cause racine, pas le symptome, et empecher toute regression future.

## Quand l'utiliser
- Bug signale par un testeur ou l'utilisateur
- Comportement inattendu detecte en preview
- Test qui echoue de maniere intermittente

## Methodologie

### Etape 1 : Reproduire
- Obtenir les etapes exactes pour declencher le bug
- Reproduire le bug de maniere consistante
- Noter l'etat attendu vs l'etat observe
- Si non reproductible : chercher les conditions de race ou les etats specifiques

### Etape 2 : Isoler
- Ajouter des console.log strategiques pour tracer le flux de donnees
- Utiliser le preview pour observer le comportement en temps reel
- Restreindre progressivement : fichier -> fonction -> ligne
- Verifier les donnees en entree de chaque etape du flux

### Etape 3 : Root Cause
- Comprendre POURQUOI le bug arrive, pas seulement OU
- Tracer le flux complet de donnees du point d'entree au point de sortie
- Exemples de root causes vs symptomes :
  - Symptome : "les portes disparaissent" -> Root cause : "les chemins ecrasent les tiles de porte"
  - Symptome : "Pyrex est bleu" -> Root cause : "numerotation des types incompatible JSON/enum"
- Verifier si d'autres endroits du code ont le meme pattern defectueux

### Etape 4 : Fix
- Appliquer le fix minimal qui corrige la cause racine
- Ne PAS patcher le symptome (ex: "si porte manquante, re-dessiner" = mauvais)
- Verifier que le fix ne casse rien d'autre (effets de bord)
- Si le meme pattern existe ailleurs, corriger toutes les occurrences

### Etape 5 : Deploiement
- INVOKE SHERLOCK : creer un test de regression qui reproduit le bug et verifie le fix
- Compilation TypeScript sans erreur
- Tous les tests passent (npx vitest run)
- Verifier visuellement dans le preview que le bug est corrige
- Retirer les console.log de debug

## Contraintes
- **Tech** : Fix minimal, pas de refactoring opportuniste pendant un bugfix
- **Business** : Prioriser les bugs qui impactent l'experience joueur
- **Produit** : Le fix doit preserver le comportement attendu de la feature
- **Design** : Si le bug revele un probleme de design, creer une issue separee

## Anti-patterns
- Corriger le symptome sans comprendre la cause (le bug reviendra)
- Ne pas ecrire de test de regression (le bug reviendra)
- Fix trop large qui refactore en meme temps (risque de nouvelles regressions)
- Laisser les console.log de debug dans le code
- Corriger une seule occurrence quand le pattern est repete ailleurs

## Lecons apprises
- Le bug Pyrex-bleu etait cause par une incompatibilite de numerotation des types entre JSON et enum. Toujours tracer le flux complet des donnees.
- Le bug des portes manquantes etait cause par l'ordre d'ecriture des tiles (chemins ecrasant les portes). Toujours verifier l'ordre des operations sur les structures partagees.
